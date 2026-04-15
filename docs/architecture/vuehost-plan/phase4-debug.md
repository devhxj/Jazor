# Phase 4: 调试支持 (DAP + CDP) — 详细实施计划

## 目标

实现 `.jazor` 源码级调试：在 IDE 中设置断点，按 F5 启动调试后，断点命中并显示正确的 `.jazor` 源码行和变量值。VueHost 同时作为 LSP server 和 DAP server 运行。

**验收标准**: VS Code 中在 `.jazor` 文件设置断点，启动调试后断点命中，调用栈显示 `.jazor` 文件名和行号，变量可查看。

---

## 一、调试架构

### 1.1 三协议共存

```
┌─────────────┐      stdio LSP       ┌──────────────────┐
│     IDE     │─────────────────────▶│                  │
│  (VS Code)  │                      │                  │
│             │      stdio/TCP DAP   │   VueHost        │
│             │─────────────────────▶│   (.NET 进程)    │
└─────────────┘                      │                  │
                                     │   LSP 服务       │
┌─────────────┐      CDP/WS         │   DAP 服务       │
│   浏览器     │◀────────────────────│   Dev Server     │
│  DevTools   │                      │   SourceMap 服务 │
└─────────────┘                      └──────────────────┘
```

### 1.2 调试数据流

```
1. 用户在 .jazor:15 设置断点
       │
       ▼ DAP: setBreakpoints
┌──────────────────────────────┐
│  DAP Server                  │
│                              │
│  SourceMap 正向映射:          │
│  .jazor:15 → bundle.js:1042  │
│                              │
│  CDP: Debugger.setBreakpoint │
│  → 发送给浏览器               │
└──────────┬───────────────────┘
           │
           ▼ 浏览器暂停
┌──────────────────────────────┐
│  浏览器: Debugger.paused     │
│  → 调用栈:                   │
│    bundle.js:1042             │
│    bundle.js:1089             │
└──────────┬───────────────────┘
           │
           ▼ CDP 事件
┌──────────────────────────────┐
│  DAP Server                  │
│                              │
│  SourceMap 逆向映射:          │
│  bundle.js:1042 → .jazor:15  │
│  bundle.js:1089 → .jazor:23  │
│                              │
│  DAP: StackTrace response    │
│  → 返回 .jazor 调用栈给 IDE  │
└──────────────────────────────┘
```

### 1.3 DAP 与 Source Map 的关系

Phase 2 的 `ISourceMapService` 是 DAP 调试器的核心依赖：

| DAP 请求 | Source Map 操作 |
|---------|----------------|
| `setBreakpoints` | 正向映射: `.jazor` 位置 → JS 位置 |
| `stackTrace` | 逆向映射: JS 位置 → `.jazor` 位置 |
| `scopes`/`variables` | 变量名保持 (开发模式不压缩) |
| `evaluate` | 可选: 表达式转译 |

---

## 二、新增文件清单

```
src/Jazor.VueHost/
├── Debug/                             # [新建目录]
│   ├── DapServer.cs                   # DAP 协议服务端
│   ├── DapProtocol.cs                 # DAP 消息类型
│   ├── DapSession.cs                  # DAP 会话状态管理
│   ├── DapRequestHandler.cs           # DAP 请求分发
│   ├── BreakpointManager.cs           # 断点管理 + Source Map 映射
│   ├── CallStackMapper.cs             # 调用栈逆向映射
│   ├── VariableMapper.cs              # 变量名映射
│   ├── CdpClient.cs                   # Chrome DevTools Protocol 客户端
│   ├── CdpConnection.cs               # CDP WebSocket 连接管理
│   └── LaunchConfiguration.cs         # 启动配置 (.vscode/launch.json)
```

### 修改文件清单

| 文件 | 修改内容 |
|------|---------|
| `Program.cs` | 添加 `--dap` / `--debug` 入口 |
| `SourceMap/ISourceMapService.cs` | 确认接口满足 DAP 需求 |
| `DevServer/DevHttpServer.cs` | 调试模式下启动 CDP 连接 |

---

## 三、接口与类型定义

### 3.1 DapProtocol — DAP 消息类型

```csharp
// Debug/DapProtocol.cs
namespace Jazor.VueHost.Debug;

/// <summary>DAP 消息基类</summary>
public sealed class DapRequest
{
    public required string Seq { get; init; }
    public required string Command { get; init; }
    public Dictionary<string, object?>? Arguments { get; init; }
}

public sealed class DapResponse
{
    public required string Seq { get; init; }
    public required string RequestSeq { get; init; }
    public required bool Success { get; init; }
    public string? Command { get; init; }
    public string? ErrorMessage { get; init; }
    public object? Body { get; init; }
}

public sealed class DapEvent
{
    public required string Seq { get; init; }
    public required string EventType { get; init; }
    public object? Body { get; init; }
}
```

### 3.2 DapServer

```csharp
// Debug/DapServer.cs
namespace Jazor.VueHost.Debug;

/// <summary>
/// Debug Adapter Protocol 服务端。
/// 通过 stdio 或 TCP 与 IDE 通信，通过 CDP 与浏览器通信。
/// </summary>
public sealed class DapServer : IAsyncDisposable
{
    public DapServer(
        ISourceMapService sourceMapService,
        CdpClient cdpClient);

    /// <summary>通过 stdio 运行 (IDE 直连模式)。</summary>
    public Task RunStdioAsync(CancellationToken cancellationToken);

    /// <summary>通过 TCP 运行 (远程调试模式)。</summary>
    public Task RunTcpAsync(string host, int port, CancellationToken cancellationToken);

    public ValueTask DisposeAsync();
}
```

### 3.3 DapSession

```csharp
// Debug/DapSession.cs
namespace Jazor.VueHost.Debug;

/// <summary>
/// DAP 调试会话状态。跟踪断点、线程状态、调用栈等。
/// </summary>
public sealed class DapSession
{
    /// <summary>会话是否已初始化。</summary>
    public bool IsInitialized { get; set; }

    /// <summary>调试是否已启动 (configurationDone 之后)。</summary>
    public bool IsStarted { get; set; }

    /// <summary>当前线程是否暂停。</summary>
    public bool IsPaused { get; set; }

    /// <summary>当前暂停的原因。</summary>
    public string? PauseReason { get; set; }

    /// <summary>当前调用栈帧列表 (DAP 格式，已映射)。</summary>
    public List<DapStackFrame>? CurrentFrames { get; set; }

    /// <summary>当前断点 ID 到 CDP breakpoint ID 的映射。</summary>
    public Dictionary<string, string> BreakpointIdMap { get; } = new();
}
```

### 3.4 BreakpointManager

```csharp
// Debug/BreakpointManager.cs
namespace Jazor.VueHost.Debug;

/// <summary>
/// 断点管理器。负责 .jazor 断点 → JS 断点的映射。
/// </summary>
public sealed class BreakpointManager
{
    public BreakpointManager(ISourceMapService sourceMapService);

    /// <summary>
    /// 将 .jazor 源码断点映射为 JS 断点位置。
    /// </summary>
    /// <param name="sourcePath">.jazor 文件路径</param>
    /// <param name="sourceLine">.jazor 中的行号 (0-based)</param>
    /// <returns>映射后的 JS 文件路径和行号，或 null 表示无法映射</returns>
    public MappedBreakpoint? MapBreakpoint(string sourcePath, int sourceLine);

    /// <summary>
    /// 注册一个 CDP 断点 ID 到 DAP 断点的映射。
    /// </summary>
    public void RegisterBreakpoint(string dapBreakpointId, string cdpBreakpointId);
}

public sealed class MappedBreakpoint
{
    public required string GeneratedPath { get; init; }   // JS 文件路径或 URL
    public required int GeneratedLine { get; init; }      // JS 行号 (0-based)
    public int GeneratedColumn { get; init; }              // JS 列号 (0-based, 可选)
}
```

### 3.5 CallStackMapper

```csharp
// Debug/CallStackMapper.cs
namespace Jazor.VueHost.Debug;

/// <summary>
/// 调用栈映射器。将 CDP 调用栈帧逆向映射为 .jazor 源码位置。
/// </summary>
public sealed class CallStackMapper
{
    public CallStackMapper(ISourceMapService sourceMapService);

    /// <summary>
    /// 将 CDP 调用栈映射为 DAP 调用栈。
    /// </summary>
    public IReadOnlyList<DapStackFrame> MapCallStack(IReadOnlyList<CdpCallFrame> cdpFrames);
}

public sealed class DapStackFrame
{
    public required int Id { get; init; }
    public required string Name { get; init; }            // 函数名
    public required DapSource Source { get; init; }       // 源文件信息
    public required int Line { get; init; }               // .jazor 行号 (0-based)
    public required int Column { get; init; }              // .jazor 列号 (0-based)
}

public sealed class DapSource
{
    public required string Name { get; init; }            // 文件名
    public required string Path { get; init; }            // 完整路径
    public int? SourceReference { get; init; }             // DAP source reference (可选)
}
```

### 3.6 VariableMapper

```csharp
// Debug/VariableMapper.cs
namespace Jazor.VueHost.Debug;

/// <summary>
/// 变量名映射器。将 JS 运行时变量名映射回 C# 源码变量名。
/// 开发模式下变量名通常不变（JazorVueCompiler 保持原始名称），
/// 但 camelCase 转换需要逆向映射: runtimeName → sourceName。
/// </summary>
public sealed class VariableMapper
{
    /// <summary>
    /// 将 JS 变量名映射回 C# 变量名。
    /// 例: "userName" → "UserName" (如果 PascalCase 原始名)
    /// </summary>
    public string? MapVariableName(string jsName, string sourcePath);

    /// <summary>
    /// 从 CDP scope 中提取 DAP 变量列表。
    /// </summary>
    public IReadOnlyList<DapVariable> ExtractVariables(CdpScope cdpScope);
}

public sealed class DapVariable
{
    public required string Name { get; init; }
    public required string Value { get; init; }
    public required string Type { get; init; }
    public int? VariablesReference { get; init; }  // 子变量引用 (用于展开对象)
}
```

### 3.7 CdpClient

```csharp
// Debug/CdpClient.cs
namespace Jazor.VueHost.Debug;

/// <summary>
/// Chrome DevTools Protocol 客户端。
/// 通过 WebSocket 连接浏览器的调试端口，控制调试会话。
/// </summary>
public sealed class CdpClient : IAsyncDisposable
{
    /// <summary>连接到浏览器的 CDP 端口。</summary>
    public Task ConnectAsync(string browserUrl, CancellationToken ct);

    /// <summary>启用 Debugger 域。</summary>
    public Task EnableDebuggerAsync(CancellationToken ct);

    /// <summary>设置断点 (按 URL + 行号)。</summary>
    public Task<CdpSetBreakpointResult> SetBreakpointByUrlAsync(
        string url, int lineNumber, int? columnNumber = null,
        CancellationToken ct = default);

    /// <summary>移除断点。</summary>
    public Task RemoveBreakpointAsync(string breakpointId, CancellationToken ct = default);

    /// <summary>继续执行。</summary>
    public Task ResumeAsync(CancellationToken ct = default);

    /// <summary>单步跳过。</summary>
    public Task StepOverAsync(CancellationToken ct = default);

    /// <summary>单步进入。</summary>
    public Task StepIntoAsync(CancellationToken ct = default);

    /// <summary>单步跳出。</summary>
    public Task StepOutAsync(CancellationToken ct = default);

    /// <summary>求值表达式。</summary>
    public Task<CdpEvaluateResult> EvaluateAsync(
        string expression, string? callFrameId = null,
        CancellationToken ct = default);

    /// <summary>获取指定调用帧的 scope 链。</summary>
    public Task<IReadOnlyList<CdpScope>> GetScopesAsync(
        string callFrameId, CancellationToken ct = default);

    /// <summary>获取指定 scope 的变量列表。</summary>
    public Task<IReadOnlyList<CdpProperty>> GetPropertiesAsync(
        string objectId, CancellationToken ct = default);

    // 事件
    public event EventHandler<CdpPausedEventArgs>? Paused;
    public event EventHandler? Resumed;

    public ValueTask DisposeAsync();
}
```

### 3.8 CDP 事件类型

```csharp
// Debug/CdpTypes.cs (内联或独立文件)
namespace Jazor.VueHost.Debug;

/// <summary>CDP Debugger.paused 事件参数</summary>
public sealed class CdpPausedEventArgs
{
    public required string Reason { get; init; }             // "breakpoint", "step", "exception"
    public required IReadOnlyList<CdpCallFrame> CallFrames { get; init; }
    public string? HitBreakpoints { get; init; }             // 命中的断点 ID
}

/// <summary>CDP 调用帧</summary>
public sealed class CdpCallFrame
{
    public required string CallFrameId { get; init; }
    public required string FunctionName { get; init; }
    public required CdpLocation Location { get; init; }
}

/// <summary>CDP 位置</summary>
public sealed class CdpLocation
{
    public required string Url { get; init; }               // JS 文件 URL
    public required int LineNumber { get; init; }            // 0-based
    public int ColumnNumber { get; init; }                    // 0-based
}

/// <summary>CDP Scope</summary>
public sealed class CdpScope
{
    public required string Type { get; init; }               // "local", "global", "closure"
    public required CdpRemoteObject Object { get; init; }
}

/// <summary>CDP RemoteObject</summary>
public sealed class CdpRemoteObject
{
    public required string Type { get; init; }               // "object", "function", "string", etc.
    public required string ObjectId { get; init; }
    public string? Value { get; init; }
}

/// <summary>CDP setBreakpointByUrl 结果</summary>
public sealed class CdpSetBreakpointResult
{
    public required string BreakpointId { get; init; }
    public required CdpLocation ActualLocation { get; init; }
}
```

---

## 四、核心实现细节

### 4.1 DapRequestHandler — DAP 请求分发

```csharp
// Debug/DapRequestHandler.cs
public sealed class DapRequestHandler
{
    private readonly DapSession _session;
    private readonly BreakpointManager _breakpointManager;
    private readonly CallStackMapper _callStackMapper;
    private readonly VariableMapper _variableMapper;
    private readonly CdpClient _cdpClient;
    private readonly ISourceMapService _sourceMapService;

    public async Task<DapResponse> HandleRequestAsync(
        DapRequest request, CancellationToken ct)
    {
        return request.Command switch
        {
            "initialize"     => HandleInitialize(request),
            "configurationDone" => await HandleConfigurationDoneAsync(request, ct),
            "launch"         => await HandleLaunchAsync(request, ct),
            "attach"         => await HandleAttachAsync(request, ct),
            "setBreakpoints" => await HandleSetBreakpointsAsync(request, ct),
            "setExceptionBreakpoints" => HandleSetExceptionBreakpoints(request),
            "threads"        => HandleThreads(request),
            "stackTrace"     => HandleStackTrace(request),
            "scopes"         => await HandleScopesAsync(request, ct),
            "variables"      => await HandleVariablesAsync(request, ct),
            "continue"       => await HandleContinueAsync(request, ct),
            "next"           => await HandleNextAsync(request, ct),
            "stepIn"         => await HandleStepInAsync(request, ct),
            "stepOut"        => await HandleStepOutAsync(request, ct),
            "evaluate"       => await HandleEvaluateAsync(request, ct),
            "disconnect"     => await HandleDisconnectAsync(request, ct),
            _ => new DapResponse
            {
                Seq = NextSeq(),
                RequestSeq = request.Seq,
                Success = false,
                Command = request.Command,
                ErrorMessage = $"Unknown command: {request.Command}"
            }
        };
    }
}
```

### 4.2 HandleInitialize — 初始化

```csharp
private DapResponse HandleInitialize(DapRequest request)
{
    return new DapResponse
    {
        Seq = NextSeq(),
        RequestSeq = request.Seq,
        Success = true,
        Command = "initialize",
        Body = new
        {
            supportsConfigurationDoneRequest = true,
            supportsEvaluateForHovers = true,
            supportsStepBack = false,
            supportsSetVariable = false,
            supportsConditionalBreakpoints = true,
            supportsHitConditionalBreakpoints = false,
            supportsLogPoints = false,
            supportsLoadedSourcesRequest = false,
            exceptionBreakpointFilters = new[]
            {
                new { filter = "all", label = "All Exceptions", default_ = false },
                new { filter = "uncaught", label = "Uncaught Exceptions", default_ = true }
            }
        }
    };
}
```

### 4.3 HandleSetBreakpoints — 断点映射

```csharp
private async Task<DapResponse> HandleSetBreakpointsAsync(
    DapRequest request, CancellationToken ct)
{
    var args = request.Arguments!;
    var sourcePath = (string)args["source"]["path"];
    var sourceBreakpoints = (object[])args["breakpoints"];

    var dapBreakpoints = new List<object>();
    var cdpBreakpointsToSet = new List<(int line, MappedBreakpoint? mapped)>();

    // 1. 将 .jazor 断点映射到 JS 位置
    foreach (var bp in sourceBreakpoints)
    {
        var sourceLine = (int)bp["line"];

        var mapped = _breakpointManager.MapBreakpoint(sourcePath, sourceLine);

        if (mapped is not null)
        {
            cdpBreakpointsToSet.Add((sourceLine, mapped));
        }
        else
        {
            // 无法映射的断点 → 标记为 unverified
            dapBreakpoints.Add(new
            {
                id = NextBreakpointId(),
                verified = false,
                line = sourceLine,
                message = "Source map unavailable"
            });
        }
    }

    // 2. 清除旧断点
    foreach (var oldId in _session.BreakpointIdMap.Values)
    {
        await _cdpClient.RemoveBreakpointAsync(oldId, ct);
    }
    _session.BreakpointIdMap.Clear();

    // 3. 在浏览器中设置新断点 (通过 CDP)
    foreach (var (sourceLine, mapped) in cdpBreakpointsToSet)
    {
        if (mapped is null) continue;

        var cdpResult = await _cdpClient.SetBreakpointByUrlAsync(
            mapped.GeneratedPath,
            mapped.GeneratedLine,
            mapped.GeneratedColumn > 0 ? mapped.GeneratedColumn : null,
            ct);

        var dapBpId = NextBreakpointId().ToString();
        _session.BreakpointIdMap[dapBpId] = cdpResult.BreakpointId;

        dapBreakpoints.Add(new
        {
            id = dapBpId,
            verified = true,
            line = sourceLine,
            // 使用 CDP 返回的实际位置更新 IDE 中的断点位置
        });
    }

    return new DapResponse
    {
        Seq = NextSeq(),
        RequestSeq = request.Seq,
        Success = true,
        Command = "setBreakpoints",
        Body = new { breakpoints = dapBreakpoints }
    };
}
```

### 4.4 HandleStackTrace — 调用栈逆向映射

```csharp
private DapResponse HandleStackTrace(DapRequest request)
{
    if (_session.CurrentFrames is null)
        return Error(request, "No call stack available");

    // 调用栈已在 CDP Paused 事件中通过 CallStackMapper 映射
    return new DapResponse
    {
        Seq = NextSeq(),
        RequestSeq = request.Seq,
        Success = true,
        Command = "stackTrace",
        Body = new
        {
            stackFrames = _session.CurrentFrames.Select((f, i) => new
            {
                id = f.Id,
                name = f.Name,
                source = new
                {
                    name = f.Source.Name,
                    path = f.Source.Path
                },
                line = f.Line,
                column = f.Column
            }),
            totalFrames = _session.CurrentFrames.Count
        }
    };
}
```

### 4.5 CDP Paused 事件处理

```csharp
// DapServer 或 DapRequestHandler 中订阅 CDP 事件:

_cdpClient.Paused += async (sender, args) =>
{
    _session.IsPaused = true;
    _session.PauseReason = args.Reason;

    // 逆向映射调用栈
    _session.CurrentFrames = _callStackMapper
        .MapCallStack(args.CallFrames)
        .ToList();

    // 发送 DAP stopped 事件给 IDE
    await SendEventAsync(new DapEvent
    {
        Seq = NextSeq(),
        EventType = "stopped",
        Body = new
        {
            reason = MapPauseReason(args.Reason),
            threadId = 1,
            hitBreakpointIds = args.HitBreakpoints is not null
                ? MapCdpBreakpointIds(args.HitBreakpoints)
                : null
        }
    });
};

_cdpClient.Resumed += (sender, _) =>
{
    _session.IsPaused = false;
    _session.CurrentFrames = null;
    // 发送 DAP continued 事件 (可选，IDE 通常不需要)
};
```

### 4.6 CallStackMapper — 调用栈映射实现

```csharp
// Debug/CallStackMapper.cs
public IReadOnlyList<DapStackFrame> MapCallStack(IReadOnlyList<CdpCallFrame> cdpFrames)
{
    var frames = new List<DapStackFrame>();
    var frameId = 0;

    foreach (var cdpFrame in cdpFrames)
    {
        // 1. 查找该 JS URL 对应的 Source Map
        var originalPos = _sourceMapService.OriginalPositionFor(
            cdpFrame.Location.Url,
            cdpFrame.Location.LineNumber,
            cdpFrame.Location.ColumnNumber);

        if (originalPos is not null)
        {
            // 2. 映射成功 → 显示 .jazor 源码位置
            frames.Add(new DapStackFrame
            {
                Id = frameId++,
                Name = cdpFrame.FunctionName,
                Source = new DapSource
                {
                    Name = Path.GetFileName(originalPos.Source),
                    Path = originalPos.Source
                },
                Line = originalPos.Line,
                Column = originalPos.Column
            });
        }
        else
        {
            // 3. 映射失败 → 显示原始 JS 位置，标注 source map 不可用
            frames.Add(new DapStackFrame
            {
                Id = frameId++,
                Name = cdpFrame.FunctionName,
                Source = new DapSource
                {
                    Name = Path.GetFileName(cdpFrame.Location.Url),
                    Path = cdpFrame.Location.Url
                },
                Line = cdpFrame.Location.LineNumber,
                Column = cdpFrame.Location.ColumnNumber
            });
        }
    }

    return frames;
}
```

---

## 五、Program.cs 修改

### 5.1 新增 --dap 入口

```csharp
var useDap = args.Any(static arg =>
    string.Equals(arg, "--dap", StringComparison.OrdinalIgnoreCase));

// ... 现有服务创建 ...

if (useDap)
{
    var sourceMapService = new SourceMapService(); // ISourceMapService 实现
    var cdpClient = new CdpClient();
    var dapServer = new DapServer(sourceMapService, cdpClient);

    // DAP 服务端通过 stdio 通信 (VS Code 默认模式)
    await dapServer.RunStdioAsync(cancellationToken);
    return;
}
```

### 5.2 VS Code launch.json 配置

VueHost 生成默认的 `.vscode/launch.json`:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Debug Jazor App",
            "type": "jazor",
            "request": "launch",
            "devServer": "http://localhost:5173",
            "webRoot": "${workspaceFolder}",
            "sourceMaps": true,
            "runtimeArgs": ["--dap"]
        }
    ]
}
```

### 5.3 VS Code 扩展 (远期)

Phase 4 MVP 阶段不开发 VS Code 扩展，而是复用现有的 "node" debug type：
- VueHost 的 DAP server 兼容 Node.js DAP 协议
- VS Code 通过 `debugServer` 字段直接连接

---

## 六、映射失败处理

### 6.1 降级策略

| 场景 | DAP 行为 |
|------|---------|
| `.jazor` 位置无法映射到 JS | 断点标记为 `unverified`，不设为验证态 |
| JS 位置无法映射回 `.jazor` | 调用栈显示 JS 原始位置 + `"(source map unavailable)"` |
| Source Map 链式合并失败 | 使用最近一级可用 Source Map |
| CDP 连接断开 | DAP 发送 `terminated` 事件给 IDE |
| 浏览器未启用远程调试 | DAP launch 报错，提示启动参数 |

### 6.2 浏览器启动

DAP `launch` 请求需要以调试模式启动浏览器：

```csharp
private async Task<DapResponse> HandleLaunchAsync(DapRequest request, CancellationToken ct)
{
    var args = request.Arguments!;
    var devServerUrl = (string)args.GetValueOrDefault("devServer", "http://localhost:5173");

    // 以远程调试模式启动 Chrome
    var debugPort = 9222;
    var chromePath = FindChrome(); // 查找系统 Chrome
    var browserProcess = Process.Start(new ProcessStartInfo
    {
        FileName = chromePath,
        Arguments = $"--remote-debugging-port={debugPort} \"{devServerUrl}\"",
        UseShellExecute = true
    });

    // 等待 CDP 端口可用
    await WaitForCdpPortAsync(debugPort, ct);

    // 连接 CDP
    await _cdpClient.ConnectAsync($"http://localhost:{debugPort}", ct);
    await _cdpClient.EnableDebuggerAsync(ct);

    _session.IsStarted = true;

    return new DapResponse
    {
        Seq = NextSeq(),
        RequestSeq = request.Seq,
        Success = true,
        Command = "launch"
    };
}
```

---

## 七、实施步骤（严格顺序）

### Step 1: CDP 客户端

**产出文件**:
- 新增 `Debug/CdpClient.cs`
- 新增 `Debug/CdpConnection.cs`
- 新增 CDP 类型 (`CdpCallFrame`, `CdpLocation`, etc.)

**测试**:
- 单元测试: CDP 消息序列化/反序列化
- 集成测试: 连接真实 Chrome CDP 端口 → 启用 Debugger → 设置断点

**退出标准**: CDP 客户端可以连接浏览器并控制调试。

### Step 2: BreakpointManager + CallStackMapper

**产出文件**:
- 新增 `Debug/BreakpointManager.cs`
- 新增 `Debug/CallStackMapper.cs`
- 新增 `Debug/VariableMapper.cs`

**测试**:
- 单元测试: 断点映射 (使用 Phase 2 的 Source Map)
- 单元测试: 调用栈逆向映射
- 单元测试: 映射失败 → 降级处理

**退出标准**: 断点和调用栈映射逻辑正确。

### Step 3: DAP 协议层

**产出文件**:
- 新增 `Debug/DapProtocol.cs`
- 新增 `Debug/DapSession.cs`
- 新增 `Debug/DapRequestHandler.cs`

**测试**:
- 单元测试: DAP 消息解析/序列化
- 单元测试: 各请求处理器返回正确格式

**退出标准**: DAP 协议消息处理正确。

### Step 4: DapServer 集成

**产出文件**:
- 新增 `Debug/DapServer.cs`
- 修改 `Program.cs` — `--dap` 入口

**测试**:
- 集成测试: DAP server stdio 通信
- 端到端测试: VS Code 设置断点 → 启动调试 → 断点命中

**退出标准**: VS Code 中可调试 `.jazor` 文件。

---

## 八、关键依赖关系

```
Step 1 (CDP Client)           ← 依赖 Phase 2 (Source Map)
    ↓
Step 2 (Breakpoint/Stack)     ← 依赖 Step 1 + Phase 2
    ↓
Step 3 (DAP Protocol)         ← 依赖 Step 2
    ↓
Step 4 (DapServer 集成)       ← 依赖 Step 1+2+3
```

Step 1 和 Step 3 的协议类型定义可以并行开发。

---

## 九、风险与降级

| 风险 | 影响 | 降级方案 |
|------|------|---------|
| Chrome CDP 端口被防火墙阻止 | 无法连接浏览器 | 使用 `--remote-debugging-pipe` 替代 |
| Source Map 精度不足 | 断点偏移 | Phase 2 行级精度通常足以命中断点 |
| 浏览器压缩变量名 | 变量显示压缩名 | 开发模式不压缩（Phase 5 约束） |
| DAP 协议版本不兼容 | VS Code 无法连接 | 严格遵循 DAP 1.51+ 规范 |
| 多线程调试 | 复杂度爆炸 | Phase 4 只支持单线程 (main thread) |
