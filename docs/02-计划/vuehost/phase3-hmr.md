# Phase 3: HMR 热更新 + 开发体验闭环 — 详细实施计划

## 目标

实现完整的热模块替换 (HMR) 系统：文件变更后浏览器 < 500ms 内看到更新，无需手动刷新。包括文件监听、增量编译、依赖图驱动的变更传播、Vue 组件热替换、CSS 热注入、API 代理。

**验收标准**: 编辑 `.jazor` / `.vue` / `.ts` / `.css` 文件保存后，浏览器自动热更新，无需手动刷新，页面状态（如 Vue 组件 ref 状态）不丢失。

## 当前实现状态（2026-04-16）

### 已完成

- Dev Server 已落地 WebSocket 热更新通道、文件监听/去抖、变更分类、增量重编译和 API 代理。
- HTTPS/WSS 自签证书回归与 `Secure=false` 代理路径已覆盖，WebSocket subprotocol 转发已覆盖。
- `--dev` 与 `--lsp` 可组合运行，LSP `didChange` 已接入 Dev Server 的 workspace 变更通道。
- 未保存工作区变更的 HMR/full-reload/去重回归已覆盖 `.jazor`、`.jazor.cs`、`.vue`、`.ts`。
- 未保存 `.vue` style-only 变更的 `style-update` 与相同内容落盘去重已覆盖。
- 独立 `.css` 文档已补齐 `DocumentKind.Css`、combined `--dev --lsp` workspace 回源，以及未保存 `style-update`/落盘去重回归。

### 仍需明确或继续推进

- 本文下方很多 `HmrServer` / `WorkspaceFileWatcher` / `_lspHandledPaths` 段落属于原始设计蓝图；当前实际实现以 `DevServerReloadHub`、`DevHttpServer`、`ChangeProcessor` 和 workspace hash 去重为准。

---

## 一、HMR 整体架构

### 1.1 数据流

```
文件系统变更 (或 LSP didChange)
        │
        ▼
┌──────────────────────────────┐
│  FileWatcher                 │
│  - FileSystemWatcher 监听    │
│  - 去抖动 (100ms)            │
│  - 识别变更文件               │
└──────────┬───────────────────┘
           │ 变更文件路径
           ▼
┌──────────────────────────────┐
│  ChangeProcessor             │
│  - 查询 DependencyGraph      │
│  - 计算影响范围               │
│  - 增量编译变更文件           │
│  - 判定更新类型               │
└──────────┬───────────────────┘
           │ HMR updates
           ▼
┌──────────────────────────────┐
│  HmrServer (WebSocket)       │
│  - 维护客户端连接             │
│  - 推送更新 payload           │
│  - 处理客户端 ACK             │
└──────────┬───────────────────┘
           │ WebSocket
           ▼
┌──────────────────────────────┐
│  浏览器 HmrRuntime (jazor-hmr.js)│
│  - 接收更新                   │
│  - Vue 组件热替换             │
│  - JS 模块重新导入            │
│  - CSS 热注入                 │
│  - 必要时 full reload         │
└──────────────────────────────┘
```

### 1.2 更新类型决策

| 变更文件 | 更新类型 | 说明 |
|---------|---------|------|
| `.jazor` | `js-update` | 重新编译 → Vue 组件热替换 |
| `.vue` | `js-update` | 重新编译 → Vue 组件热替换 |
| `.ts` / `.js` | `js-update` | 重新编译 → 模块级热替换 |
| `.css` / `.vue` 中的 `<style>` | `style-update` | CSS 热注入，无刷新 |
| `index.html` | `full-reload` | 需要刷新页面 |
| `jazor.config.json` | `full-reload` | 配置变更，重启 Dev Server |
| `.jazor` 中的 `@code` 签名变更 | `full-reload` | Props/状态签名变更不可热替换 |

---

## 二、新增文件清单

```
src/Jazor.VueHost/
├── DevServer/
│   ├── Hmr/
│   │   ├── HmrServer.cs               # WebSocket HMR 服务端
│   │   ├── HmrClientConnection.cs      # 单个客户端连接
│   │   ├── HmrProtocol.cs              # HMR 消息类型定义
│   │   ├── HmrUpdate.cs                # 更新 payload 类型
│   │   └── IHotModuleHandler.cs        # 模块热更新处理接口
│   │
│   ├── FileWatching/
│   │   ├── WorkspaceFileWatcher.cs     # 文件系统监听器
│   │   ├── FileChangeEventArgs.cs      # 文件变更事件参数
│   │   └── FileChangeDebouncer.cs      # 去抖动处理器
│   │
│   ├── ChangeProcessor.cs              # [新建] 变更处理核心
│   ├── DependencyGraph.cs              # [修改] 补充反向依赖查询
│   ├── DevHttpServer.cs                # [修改] 集成 HMR + 文件监听
│   └── Client/
│       └── jazor-hmr.js                # [重写] 完整 HMR 客户端 runtime
│
│   ├── Proxy/
│   │   └── DevServerProxy.cs           # API 代理中间件
│   │
│   └── JazorConfig.cs                  # [修改] 添加 proxy/server 配置
```

### 修改文件清单

| 文件 | 修改内容 |
|------|---------|
| `DevServer/DevHttpServer.cs` | 集成 HMR WebSocket + FileWatcher + Proxy |
| `DevServer/OnDemandCompiler.cs` | 支持增量编译 (传入旧结果做 diff) |
| `DevServer/DependencyGraph.cs` | 补充反向依赖 (dependents) 查询 |
| `DevServer/ModuleResolver.cs` | 解析代理路径 |
| `DevServer/JazorConfig.cs` | proxy 配置 |
| `Program.cs` | 传递更多配置到 Dev Server |

---

## 三、接口与类型定义

### 3.1 HmrProtocol — 消息类型

```csharp
// DevServer/Hmr/HmrProtocol.cs
namespace Jazor.VueHost.DevServer.Hmr;

/// <summary>HMR 消息方向</summary>
public enum HmrMessageDirection
{
    ServerToClient,
    ClientToServer
}

/// <summary>HMR 消息类型</summary>
public static class HmrMessageTypes
{
    // Server → Client
    public const string Connected     = "connected";
    public const string Update        = "update";
    public const string FullReload    = "full-reload";
    public const string Error         = "error";
    public const string Custom        = "custom";

    // Client → Server
    public const string Ready         = "ready";
    public const string Heartbeat     = "heartbeat";
}
```

### 3.2 HmrUpdate — 更新 Payload

```csharp
// DevServer/Hmr/HmrUpdate.cs
namespace Jazor.VueHost.DevServer.Hmr;

/// <summary>
/// 单个模块的热更新描述。
/// </summary>
public sealed class HmrUpdate
{
    /// <summary>更新类型。</summary>
    public required HmrUpdateType Type { get; init; }

    /// <summary>变更的模块路径 (URL 形式, 如 /App.jazor)。</summary>
    public required string Path { get; init; }

    /// <summary>变更时间戳 (ms)。</summary>
    public required long Timestamp { get; init; }

    /// <summary>编译后的模块内容 (仅在 inline 模式时携带)。</summary>
    public string? Content { get; init; }

    /// <summary>是否被客户端接受 (由 runtime 运行时决定)。</summary>
    public bool Accepted { get; set; }
}

public enum HmrUpdateType
{
    /// <summary>JS 模块更新 — 通过 import() 热替换。</summary>
    JsUpdate,

    /// <summary>CSS 更新 — 通过 style 标签注入。</summary>
    StyleUpdate,

    /// <summary>需要完全刷新页面。</summary>
    FullReload
}
```

### 3.3 HmrServer

```csharp
// DevServer/Hmr/HmrServer.cs
namespace Jazor.VueHost.DevServer.Hmr;

/// <summary>
/// HMR WebSocket 服务端。管理客户端连接，推送文件变更更新。
/// </summary>
public sealed class HmrServer : IAsyncDisposable
{
    public HmrServer(DevServerOptions options);

    /// <summary>绑定到 Kestrel WebSocket 管道。</summary>
    public void MapWebSocket(WebApplication app);

    /// <summary>向所有连接的客户端推送更新。</summary>
    public Task BroadcastUpdateAsync(IReadOnlyList<HmrUpdate> updates);

    /// <summary>向所有客户端发送 full-reload。</summary>
    public Task BroadcastFullReloadAsync(string reason);

    /// <summary>向所有客户端发送错误。</summary>
    public Task BroadcastErrorAsync(string message);

    /// <summary>当前连接的客户端数量。</summary>
    public int ConnectedClientCount { get; }

    public ValueTask DisposeAsync();
}
```

### 3.4 WorkspaceFileWatcher

```csharp
// DevServer/FileWatching/WorkspaceFileWatcher.cs
namespace Jazor.VueHost.DevServer.FileWatching;

/// <summary>
/// 工作区文件监听器。基于 FileSystemWatcher，监听 workspace 目录内的文件变更。
/// </summary>
public sealed class WorkspaceFileWatcher : IAsyncDisposable
{
    public WorkspaceFileWatcher(string rootDirectory, FileChangeDebouncer debouncer);

    /// <summary>文件变更事件。去抖动后触发。</summary>
    public event EventHandler<IReadOnlyList<FileChangeEventArgs>>? FileChanged;

    /// <summary>启动监听。</summary>
    public Task StartAsync(CancellationToken cancellationToken);

    /// <summary>停止监听。</summary>
    public ValueTask DisposeAsync();
}
```

### 3.5 FileChangeEventArgs

```csharp
// DevServer/FileWatching/FileChangeEventArgs.cs
namespace Jazor.VueHost.DevServer.FileWatching;

public sealed class FileChangeEventArgs
{
    public required string AbsolutePath { get; init; }
    public required FileChangeKind ChangeKind { get; init; }
    public required DocumentKind DocumentKind { get; init; }
}

public enum FileChangeKind
{
    Created,
    Modified,
    Deleted
}
```

### 3.6 FileChangeDebouncer

```csharp
// DevServer/FileWatching/FileChangeDebouncer.cs
namespace Jazor.VueHost.DevServer.FileWatching;

/// <summary>
/// 文件变更去抖动器。短时间内同一文件的多次变更合并为一次。
/// </summary>
public sealed class FileChangeDebouncer
{
    public FileChangeDebouncer(TimeSpan debounceInterval);

    /// <summary>记录一次文件变更。</summary>
    public void Record(FileChangeEventArgs change);

    /// <summary>去抖动后的变更事件。</summary>
    public event EventHandler<IReadOnlyList<FileChangeEventArgs>>? DebouncedChange;
}
```

### 3.7 ChangeProcessor

```csharp
// DevServer/ChangeProcessor.cs
namespace Jazor.VueHost.DevServer;

/// <summary>
/// 文件变更处理器。接收文件变更事件，执行增量编译，计算 HMR 更新。
/// </summary>
public sealed class ChangeProcessor
{
    public ChangeProcessor(
        OnDemandCompiler compiler,
        ModuleResolver moduleResolver,
        DependencyGraph dependencyGraph,
        HmrServer hmrServer);

    /// <summary>处理一组文件变更。</summary>
    public async Task ProcessChangesAsync(
        IReadOnlyList<FileChangeEventArgs> changes,
        CancellationToken cancellationToken);
}
```

### 3.8 DependencyGraph 扩展

```csharp
// DevServer/DependencyGraph.cs — Phase 1 基础 + Phase 3 扩展

public sealed class DependencyGraph
{
    // Phase 1 已有:
    public void Record(string modulePath, IReadOnlyList<string> dependencies);
    public IReadOnlyList<string> GetDependents(string modulePath);

    // Phase 3 新增:

    /// <summary>
    /// 获取所有受影响的模块（传递闭包）。
    /// A 导入 B，B 导入 C。C 变更 → 返回 [B, A]。
    /// </summary>
    public IReadOnlyList<string> GetAllAffectedModules(string changedModule);

    /// <summary>移除指定模块的所有依赖记录。</summary>
    public void Remove(string modulePath);

    /// <summary>获取整个图的统计信息（用于调试）。</summary>
    public DependencyGraphStats GetStats();
}

public sealed class DependencyGraphStats
{
    public required int TotalModules { get; init; }
    public required int TotalEdges { get; init; }
}
```

### 3.9 DevServerProxy

```csharp
// DevServer/Proxy/DevServerProxy.cs
namespace Jazor.VueHost.DevServer.Proxy;

/// <summary>
/// API 开发代理中间件。将匹配的请求转发到配置的后端服务。
/// </summary>
public sealed class DevServerProxy
{
    public DevServerProxy(IReadOnlyDictionary<string, ProxyTarget> proxyRules);

    /// <summary>
    /// 尝试代理请求。如果路径匹配某个代理规则，转发请求并返回 true。
    /// </summary>
    public async Task<bool> TryProxyAsync(HttpContext context);
}

public sealed class ProxyTarget
{
    public required string Target { get; init; }          // "http://localhost:5000"
    public bool Secure { get; init; } = false;
    public bool WebSocket { get; init; } = true;          // 代理 WebSocket
    public string? RewritePath { get; init; }             // 可选路径重写
}
```

---

## 四、核心实现细节

### 4.1 HmrServer — WebSocket 管理

```csharp
// DevServer/Hmr/HmrServer.cs
public sealed class HmrServer : IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, HmrClientConnection> _clients = new();
    private readonly DevServerOptions _options;

    public void MapWebSocket(WebApplication app)
    {
        app.MapWebSocketManager("/__hmr__", async (ws, ct) =>
        {
            var clientId = Guid.NewGuid().ToString("N")[..8];
            var connection = new HmrClientConnection(clientId, ws);
            _clients[clientId] = connection;

            // 发送 connected 消息
            await connection.SendAsync(new
            {
                type = "connected",
                clientId
            }, ct);

            // 接收循环 (处理 heartbeat / ready)
            await connection.ReceiveLoopAsync(ct);

            // 断开连接
            _clients.TryRemove(clientId, out _);
        });
    }

    public async Task BroadcastUpdateAsync(IReadOnlyList<HmrUpdate> updates)
    {
        var payload = new
        {
            type = "update",
            updates
        };
        var tasks = _clients.Values.Select(c => c.SendAsync(payload, CancellationToken.None));
        await Task.WhenAll(tasks);
    }

    public async Task BroadcastFullReloadAsync(string reason)
    {
        var payload = new { type = "full-reload", reason };
        var tasks = _clients.Values.Select(c => c.SendAsync(payload, CancellationToken.None));
        await Task.WhenAll(tasks);
    }
}
```

### 4.2 ChangeProcessor — 变更处理核心

```csharp
// DevServer/ChangeProcessor.cs
public async Task ProcessChangesAsync(
    IReadOnlyList<FileChangeEventArgs> changes, CancellationToken ct)
{
    var updates = new List<HmrUpdate>();

    foreach (var change in changes)
    {
        // 1. 查找受影响模块
        var affectedModules = _dependencyGraph.GetAllAffectedModules(change.AbsolutePath);
        var allChanged = new[] { change.AbsolutePath }
            .Concat(affectedModules)
            .Distinct()
            .ToList();

        // 2. 判定更新类型
        if (NeedsFullReload(change))
        {
            await _hmrServer.BroadcastFullReloadAsync(
                $"File {Path.GetFileName(change.AbsolutePath)} changed");
            return;
        }

        // 3. 增量编译
        foreach (var modulePath in allChanged)
        {
            var result = await _compiler.CompileAsync(modulePath, ct);

            if (result.IsError)
            {
                await _hmrServer.BroadcastErrorAsync(result.ErrorMessage ?? "Compilation error");
                continue;
            }

            var resolveResult = _moduleResolver.Resolve(modulePath);
            var url = resolveResult.ResolvedUrl;
            var kind = VueHostWorkspaceResolver.MapDocumentKind(modulePath);

            updates.Add(new HmrUpdate
            {
                Type = kind == DocumentKind.Unknown && url.EndsWith(".css")
                    ? HmrUpdateType.StyleUpdate
                    : HmrUpdateType.JsUpdate,
                Path = url,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Content = result.Content  // inline 模式携带编译结果
            });
        }
    }

    // 4. 推送更新
    if (updates.Count > 0)
    {
        await _hmrServer.BroadcastUpdateAsync(updates);
    }
}

private static bool NeedsFullReload(FileChangeEventArgs change)
{
    // index.html 变更 → full reload
    if (Path.GetFileName(change.AbsolutePath)
        .Equals("index.html", StringComparison.OrdinalIgnoreCase))
        return true;

    // 配置文件变更 → full reload
    if (Path.GetFileName(change.AbsolutePath)
        .Equals("jazor.config.json", StringComparison.OrdinalIgnoreCase))
        return true;

    // 文件删除 → full reload
    if (change.ChangeKind == FileChangeKind.Deleted)
        return true;

    return false;
}
```

### 4.3 WorkspaceFileWatcher — 文件监听

```csharp
// DevServer/FileWatching/WorkspaceFileWatcher.cs
public sealed class WorkspaceFileWatcher : IAsyncDisposable
{
    private readonly string _rootDirectory;
    private readonly FileChangeDebouncer _debouncer;
    private readonly List<FileSystemWatcher> _watchers = [];

    // 监听的扩展名
    private static readonly HashSet<string> WatchedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jazor", ".vue", ".ts", ".js", ".css", ".html", ".json"
    };

    // 跳过的目录
    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "node_modules", ".git", "bin", "obj", ".vs", ".deno"
    };

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // 监听根目录及一级子目录
        var watcher = new FileSystemWatcher(_rootDirectory)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
                        | NotifyFilters.LastWrite
                        | NotifyFilters.Size,
        };

        watcher.Changed += OnFileChanged;
        watcher.Created += OnFileCreated;
        watcher.Deleted += OnFileDeleted;
        watcher.Renamed += OnFileRenamed;

        watcher.EnableRaisingEvents = true;
        _watchers.Add(watcher);

        return Task.CompletedTask;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (!ShouldWatch(e.FullPath)) return;
        _debouncer.Record(new FileChangeEventArgs
        {
            AbsolutePath = e.FullPath,
            ChangeKind = FileChangeKind.Modified,
            DocumentKind = VueHostWorkspaceResolver.MapDocumentKind(e.FullPath)
        });
    }

    private static bool ShouldWatch(string path)
    {
        var ext = Path.GetExtension(path);
        if (!WatchedExtensions.Contains(ext)) return false;

        // 检查路径中是否包含跳过的目录
        var dir = Path.GetDirectoryName(path);
        while (dir is not null)
        {
            var name = Path.GetFileName(dir);
            if (IgnoredDirectories.Contains(name)) return false;
            dir = Path.GetDirectoryName(dir);
        }

        return true;
    }
}
```

### 4.4 FileChangeDebouncer — 去抖动

```csharp
// DevServer/FileWatching/FileChangeDebouncer.cs
public sealed class FileChangeDebouncer
{
    private readonly TimeSpan _interval;
    private readonly ConcurrentDictionary<string, FileChangeEventArgs> _pending = new();
    private Timer? _timer;

    public FileChangeDebouncer(TimeSpan debounceInterval)
    {
        _interval = debounceInterval;
    }

    public void Record(FileChangeEventArgs change)
    {
        // 同一文件只保留最新的变更
        _pending.AddOrUpdate(
            change.AbsolutePath,
            change,
            (_, _) => change);

        // 重置定时器
        _timer ??= new Timer(Flush, null, _interval, Timeout.InfiniteTimeSpan);
        _timer.Change(_interval, Timeout.InfiniteTimeSpan);
    }

    private void Flush(object? state)
    {
        var changes = _pending.Values.ToList();
        _pending.Clear();

        if (changes.Count > 0)
        {
            DebouncedChange?.Invoke(this, changes);
        }
    }

    public event EventHandler<IReadOnlyList<FileChangeEventArgs>>? DebouncedChange;
}
```

### 4.5 DependencyGraph — 反向依赖传播

```csharp
// DevServer/DependencyGraph.cs — 新增方法

/// <summary>
/// 获取所有受影响的模块（传递闭包）。
/// 例: A imports B, B imports C。C 变更 → 返回 [B, A]。
/// </summary>
public IReadOnlyList<string> GetAllAffectedModules(string changedModule)
{
    var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var queue = new Queue<string>();
    queue.Enqueue(changedModule);

    var result = new List<string>();

    while (queue.Count > 0)
    {
        var current = queue.Dequeue();
        if (!visited.Add(current)) continue;

        var dependents = GetDependents(current);
        foreach (var dep in dependents)
        {
            result.Add(dep);
            queue.Enqueue(dep);
        }
    }

    return result;
}
```

### 4.6 DevServerProxy — API 代理

```csharp
// DevServer/Proxy/DevServerProxy.cs
public async Task<bool> TryProxyAsync(HttpContext context)
{
    var path = context.Request.Path.Value;
    if (path is null) return false;

    // 查找匹配的代理规则
    foreach (var (prefix, target) in _proxyRules)
    {
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

        var targetUri = new Uri(target.Target);
        var pathWithoutPrefix = path[prefix.Length..];
        var proxyPath = target.RewritePath ?? pathWithoutPrefix;

        // 使用 HttpClient 转发
        using var requestMessage = new HttpRequestMessage(
            new HttpMethod(context.Request.Method),
            new Uri(targetUri, proxyPath));

        // 复制请求头
        foreach (var header in context.Request.Headers)
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }

        // 复制请求体
        if (context.Request.Body is { CanRead: true })
        {
            requestMessage.Content = new StreamContent(context.Request.Body);
        }

        using var responseMessage = await _httpClient.SendAsync(
            requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

        // 复制响应
        context.Response.StatusCode = (int)responseMessage.StatusCode;
        foreach (var header in responseMessage.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        await responseMessage.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
        return true;
    }

    return false;
}
```

---

## 五、客户端 HMR Runtime (完整版)

### 5.1 jazor-hmr.js

```javascript
// DevServer/Client/jazor-hmr.js — Phase 3 完整版本
(function () {
    "use strict";

    const HMR_PORT = location.port || (location.protocol === "https:" ? 443 : 80);
    const HMR_URL = `ws://${location.hostname}:${HMR_PORT}/__hmr__`;

    // 模块注册表: path → { deps, accept }
    const moduleRegistry = new Map();

    // CSS 样式标签追踪: path → <style> element
    const styleMap = new Map();

    // 全局 HMR API (暴露给编译后的模块)
    window.__JAZOR_HMR__ = {
        register(id, deps, accept) {
            moduleRegistry.set(id, { deps: deps || [], accept });
        },
        accept(cb) {
            // 当前模块接受自身热更新
        }
    };

    // 连接 WebSocket
    let ws;
    let reconnectTimer;

    function connect() {
        ws = new WebSocket(HMR_URL);

        ws.onopen = () => {
            console.log("[jazor-hmr] connected");
            if (reconnectTimer) {
                clearTimeout(reconnectTimer);
                reconnectTimer = null;
            }
        };

        ws.onmessage = (event) => {
            const data = JSON.parse(event.data);
            handleMessage(data);
        };

        ws.onclose = () => {
            console.log("[jazor-hmr] disconnected, reconnecting...");
            reconnectTimer = setTimeout(connect, 2000);
        };

        ws.onerror = () => {
            ws.close();
        };
    }

    async function handleMessage(data) {
        switch (data.type) {
            case "connected":
                console.log(`[jazor-hmr] client ${data.clientId} connected`);
                break;

            case "update":
                await handleUpdate(data.updates);
                break;

            case "full-reload":
                console.log(`[jazor-hmr] full reload: ${data.reason}`);
                location.reload();
                break;

            case "error":
                console.error(`[jazor-hmr] error: ${data.err || data.message}`);
                showErrorOverlay(data.err || data.message);
                break;
        }
    }

    async function handleUpdate(updates) {
        for (const update of updates) {
            switch (update.type) {
                case "js-update":
                    await handleJsUpdate(update);
                    break;
                case "style-update":
                    handleStyleUpdate(update);
                    break;
                case "full-reload":
                    location.reload();
                    return;
            }
        }
    }

    // === JS 模块热更新 ===
    async function handleJsUpdate(update) {
        const path = update.path;
        const registered = moduleRegistry.get(path);

        // 1. 检查模块是否注册了自定义热更新处理
        if (registered && registered.accept) {
            try {
                // 动态重新导入获取新模块
                const newModule = await import(`${path}?t=${update.timestamp}`);
                // 调用模块的 accept 回调
                await registered.accept(newModule);
                console.log(`[jazor-hmr] hot updated: ${path}`);
                return;
            } catch (e) {
                console.error(`[jazor-hmr] accept failed: ${path}`, e);
            }
        }

        // 2. Vue 组件热替换
        if (window.__VUE_HMR_RUNTIME__) {
            try {
                const newModule = await import(`${path}?t=${update.timestamp}`);
                if (newModule.default) {
                    // 使用 Vue 的 HMR runtime API
                    window.__VUE_HMR_RUNTIME__.reload(path, newModule.default);
                    console.log(`[jazor-hmr] Vue component updated: ${path}`);
                    return;
                }
            } catch (e) {
                console.error(`[jazor-hmr] Vue HMR failed: ${path}`, e);
            }
        }

        // 3. 无法热更新 → full reload
        console.log(`[jazor-hmr] cannot hot update: ${path}, full reload`);
        location.reload();
    }

    // === CSS 热更新 ===
    function handleStyleUpdate(update) {
        const path = update.path;
        const existing = styleMap.get(path);

        if (existing) {
            // 更新已有样式标签的内容
            existing.textContent = update.content;
            console.log(`[jazor-hmr] CSS updated: ${path}`);
        } else {
            // 创建新的样式标签
            const style = document.createElement("style");
            style.setAttribute("data-jazor-path", path);
            style.textContent = update.content;
            document.head.appendChild(style);
            styleMap.set(path, style);
            console.log(`[jazor-hmr] CSS injected: ${path}`);
        }
    }

    // === 错误覆盖层 ===
    function showErrorOverlay(message) {
        let overlay = document.getElementById("__jazor-error-overlay");
        if (!overlay) {
            overlay = document.createElement("div");
            overlay.id = "__jazor-error-overlay";
            overlay.style.cssText = `
                position: fixed; top: 0; left: 0; right: 0;
                z-index: 99999; padding: 12px 16px;
                background: #ff4444; color: white;
                font: 14px/1.5 monospace; white-space: pre-wrap;
                box-shadow: 0 2px 8px rgba(0,0,0,0.3);
            `;
            document.body.appendChild(overlay);
        }
        overlay.textContent = `[jazor] ${message}`;
    }

    // === 启动 ===
    connect();
})();
```

---

## 六、DevHttpServer 集成

### 6.1 中间件管道变更

```csharp
// DevServer/DevHttpServer.cs — Phase 3 修改

public async Task StartAsync(CancellationToken cancellationToken)
{
    var builder = WebApplication.CreateSlimBuilder();
    builder.WebHost.UseUrls($"http://{_options.Host}:{_options.Port}");

    var app = builder.Build();

    // === 中间件管道 (按顺序) ===

    // 1. API 代理 (优先级最高)
    app.Use(async (context, next) =>
    {
        if (_proxy is not null && await _proxy.TryProxyAsync(context))
            return;
        await next();
    });

    // 2. HMR WebSocket
    _hmrServer.MapWebSocket(app);

    // 3. 核心模块服务中间件
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value;
        if (path is null) { await next(); return; }

        // .map 文件
        if (path.EndsWith(".map")) { await ServeSourceMapAsync(context); return; }

        // index.html
        if (path == "/" || path.Equals("/index.html", OrdinalIgnoreCase))
            { await ServeIndexHtmlAsync(context); return; }

        // 可编译模块
        var resolve = _moduleResolver.Resolve(path);
        if (resolve.Found && resolve.IsVirtual)
            { await ServeCompiledModuleAsync(context, resolve, cancellationToken); return; }

        // HMR client script
        if (path.Equals("/@jazor/hmr", OrdinalIgnoreCase))
            { await ServeEmbeddedResourceAsync(context, "jazor-hmr.js"); return; }

        await next();
    });

    // 4. 静态文件
    app.UseStaticFiles();

    // 5. SPA fallback
    app.Use(async (context, next) =>
    {
        if (!Path.HasExtension(context.Request.Path.Value ?? ""))
            context.Request.Path = "/index.html";
        await next();
    });

    await app.StartAsync(cancellationToken);

    // === 启动文件监听 ===
    if (_options.HmrEnabled)
    {
        var debouncer = new FileChangeDebouncer(TimeSpan.FromMilliseconds(100));
        var watcher = new WorkspaceFileWatcher(_options.RootDirectory, debouncer);
        var changeProcessor = new ChangeProcessor(
            _compiler, _moduleResolver, _dependencyGraph, _hmrServer);

        debouncer.DebouncedChange += async (_, changes) =>
        {
            await changeProcessor.ProcessChangesAsync(changes, CancellationToken.None);
        };

        await watcher.StartAsync(cancellationToken);
    }

    Console.WriteLine($"  Dev Server: http://{_options.Host}:{_options.Port}");
    Console.WriteLine($"  HMR: {(_options.HmrEnabled ? "enabled" : "disabled")}");
}
```

### 6.2 LSP didChange 与文件监听协调

```csharp
// 当 LSP 和 Dev Server 同时运行时，需要避免重复编译。
// 策略: LSP 连接时，禁用文件监听中对应文件的编译处理。

// ChangeProcessor 中添加:
public void OnLspDocumentChanged(string documentPath)
{
    // 标记该文件已由 LSP 处理，跳过文件监听的编译
    _lspHandledPaths.Add(documentPath);
}

// ProcessChangesAsync 中:
foreach (var change in changes)
{
    if (_lspHandledPaths.Contains(change.AbsolutePath))
    {
        _lspHandledPaths.Remove(change.AbsolutePath);
        continue;  // LSP 已处理，跳过
    }
    // ... 正常处理 ...
}
```

---

## 七、jazor.config.json 扩展

```jsonc
{
    "server": {
        "port": 5173,
        "host": "localhost",
        "open": false,
        "hmr": true
    },
    "proxy": {
        "/api": {
            "target": "http://localhost:5000",
            "secure": false,
            "websocket": true
        },
        "/ws": {
            "target": "ws://localhost:5001",
            "websocket": true
        }
    },
    "resolve": {
        "alias": {
            "@": "./src"
        }
    }
}
```

```csharp
// DevServer/JazorConfig.cs — 扩展
public sealed class JazorConfig
{
    public ServerConfig? Server { get; init; }
    public ResolveConfig? Resolve { get; init; }
    public Dictionary<string, ProxyConfig>? Proxy { get; init; }  // 新增
}

public sealed class ProxyConfig
{
    public required string Target { get; init; }
    public bool Secure { get; init; }
    public bool WebSocket { get; init; } = true;
    public string? RewritePath { get; init; }
}
```

---

## 八、实施步骤（严格顺序）

### Step 1: HMR Protocol + HmrServer

**产出文件**:
- 新增 `DevServer/Hmr/HmrProtocol.cs`
- 新增 `DevServer/Hmr/HmrUpdate.cs`
- 新增 `DevServer/Hmr/HmrServer.cs`
- 新增 `DevServer/Hmr/HmrClientConnection.cs`

**测试**:
- 单元测试: HmrServer 消息序列化/反序列化
- 集成测试: 启动 WebSocket 服务 → 客户端连接 → 收到 connected 消息

**退出标准**: WebSocket 服务端可接受连接并广播消息。

### Step 2: 客户端 HMR Runtime

**产出文件**:
- 重写 `DevServer/Client/jazor-hmr.js`

**测试**:
- 手动测试: 连接 WebSocket → 发送 update → 验证模块重载
- 手动测试: Vue 组件热替换
- 手动测试: CSS 热注入

**退出标准**: 浏览器收到更新消息后正确处理，不报错。

### Step 3: DependencyGraph 扩展

**产出文件**:
- 修改 `DevServer/DependencyGraph.cs`

**测试**:
- 单元测试: 直接依赖查询
- 单元测试: 传递闭包 (A→B→C, C 变更 → [B, A])
- 单元测试: 循环依赖不无限递归
- 单元测试: 移除模块后依赖清理

**退出标准**: 依赖传播计算正确。

### Step 4: FileWatcher + Debouncer

**产出文件**:
- 新增 `DevServer/FileWatching/WorkspaceFileWatcher.cs`
- 新增 `DevServer/FileWatching/FileChangeEventArgs.cs`
- 新增 `DevServer/FileWatching/FileChangeDebouncer.cs`

**测试**:
- 单元测试: Debouncer 100ms 内多次变更合并
- 集成测试: 创建/修改/删除文件 → 触发事件
- 集成测试: node_modules 内变更被忽略

**退出标准**: 文件变更被正确检测并去抖动。

### Step 5: ChangeProcessor

**产出文件**:
- 新增 `DevServer/ChangeProcessor.cs`

**测试**:
- 单元测试: .jazor 变更 → js-update
- 单元测试: .css 变更 → style-update
- 单元测试: index.html 变更 → full-reload
- 单元测试: 依赖传播: A imports B, B 变更 → A 也被更新

**退出标准**: 变更正确分类并编译。

### Step 6: DevHttpServer 集成

**产出文件**:
- 修改 `DevServer/DevHttpServer.cs`
- 修改 `DevServer/OnDemandCompiler.cs` — 增量编译优化

**测试**:
- 端到端测试: 编辑 .jazor → 浏览器自动更新 (< 500ms)
- 端到端测试: 编辑 .css → 样式更新无需刷新
- 端到端测试: 编辑 index.html → 自动刷新

**退出标准**: 文件变更后浏览器自动热更新。

### Step 7: API 代理

**产出文件**:
- 新增 `DevServer/Proxy/DevServerProxy.cs`
- 修改 `DevServer/JazorConfig.cs`
- 修改 `DevServer/DevHttpServer.cs`

**测试**:
- 集成测试: /api/* 请求转发到后端
- 集成测试: 404 路径不匹配时不代理

**退出标准**: API 请求正确转发。

---

## 九、性能目标

| 指标 | 目标 | 说明 |
|------|------|------|
| 文件变更到浏览器更新 | < 500ms | 包含去抖动 + 编译 + WebSocket 推送 + 客户端应用 |
| 去抖动窗口 | 100ms | 短时间多次保存合并为一次更新 |
| 增量编译 | 仅编译变更文件 | 利用 DependencyGraph 精确计算影响范围 |
| HMR WebSocket 消息大小 | < 100KB | 大文件考虑 external 模式 |

---

## 十、风险与降级

| 风险 | 影响 | 降级方案 |
|------|------|---------|
| FileSystemWatcher 在某些 OS 上不触发 | 文件变更不被检测 | 添加轮询 fallback (1s 间隔) |
| Vue `__VUE_HMR_RUNTIME__` 不可用 | Vue 组件无法热替换 | 退化为 full reload |
| 编译耗时 > 300ms | 总延迟超过 500ms | 增量编译 + 缓存优化 |
| WebSocket 连接断开 | 更新无法推送 | 客户端自动重连 (2s 间隔) |
| 循环依赖导致无限传播 | ChangeProcessor 死循环 | 传递闭包使用 visited 集合 |
| .jazor @code 签名变更 (如新增 Prop) | 热替换后组件状态不一致 | 检测签名变更 → full reload |

---

## 十一、关键依赖关系

```
Step 1 (HmrServer)     ← 独立
Step 2 (HMR Runtime)   ← 独立
Step 3 (DependencyGraph) ← 独立
Step 4 (FileWatcher)   ← 独立
    ↓↓↓↓ (以上可并行)
Step 5 (ChangeProcessor) ← 依赖 1+3+4
    ↓
Step 6 (DevHttpServer 集成) ← 依赖 1+2+5
    ↓
Step 7 (API Proxy)      ← 独立，可并行
```

Step 1-4 完全独立，可以并行开发。

---

## 十二、不做的事情 (Phase 3 明确排除)

| 排除项 | 原因 |
|--------|------|
| HMR 状态持久化 (跨刷新保留) | 远期 |
| 多浏览器同步 HMR | 远期 |
| 自定义 HMR 插件 API | 远期 |
| 轮询文件监听 (除非 OS 不支持 FSW) | 作为 fallback |
| HMR overlay 样式自定义 | 远期 |
| Partial HMR (部分组件更新) | Vue 已原生支持 |
