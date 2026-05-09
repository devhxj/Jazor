# 扩展 Worker 进程

进程隔离扩展的 JSON-RPC 宿主服务器：worker 进程启动、stdio JSON-RPC 协议、Bootstrap/Invoke/Shutdown 消息处理、沙箱执行检查和超时控制。核心实现在 `src/Jolt/Extensions/ExtensionWorkerServer.cs`（约 1166 行）和 `src/Jolt/Extensions/OutOfProcessExtensionProxy.cs`（约 592 行）。

## 核心类型

### ExtensionWorkerServer Worker 宿主

**文件位置**: `src/Jolt/Extensions/ExtensionWorkerServer.cs`

**职责**:
- 启动独立进程（通过 `dotnet exec` 或直接执行）
- 监听 stdin/stdout JSON-RPC 消息
- 加载扩展程序集（`CollectibleExtensionLoadContext`）
- 路由 provider 调用请求
- 应用运行时沙箱策略（IO/网络验证）

### ExtensionWorkerClient Worker 客户端

**文件位置**: `src/Jolt/Extensions/ExtensionWorkerClient.cs`

**职责**:
- 启动 worker 子进程
- 通过 stdio 发送 JSON-RPC 请求
- 接收并解析 JSON-RPC 响应
- 处理连接失败和超时

### OutOfProcessExtensionProxy 进程隔离代理

**文件位置**: `src/Jolt/Extensions/OutOfProcessExtensionProxy.cs`

**职责**:
- 实现 `IExtension` 和所有 11 个 provider 接口
- 将 provider 调用转发到 worker 进程
- 处理 worker 崩溃和自动重启
- 断路器模式（Circuit Breaker）防止无限重启

## 核心算法

### Worker 启动流程

**方法**: `OutOfProcessExtensionProxy.CreateAsync`

**流程**:
1. **构建 Bootstrap 请求**:
   ```csharp
   var bootstrapRequest = new ExtensionWorkerBootstrapRequest(
       RootDirectory: rootDirectory,
       ExtensionDirectory: extensionDirectory,
       AssemblyPath: assemblyPath,
       ExtensionTypeName: extensionTypeName,
       Settings: settings,
       SandboxProfile: sandboxProfile);
   ```

2. **启动 Worker 进程** (`ExtensionWorkerClient.StartAsync`):
   ```csharp
   var processStartInfo = new ProcessStartInfo
   {
       FileName = "dotnet",
       Arguments = $"exec \"{workerAssemblyPath}\" worker",
       RedirectStandardInput = true,
       RedirectStandardOutput = true,
       RedirectStandardError = false,
       UseShellExecute = false,
       CreateNoWindow = true
   };

   var workerProcess = Process.Start(processStartInfo);
   var workerClient = new ExtensionWorkerClient(workerProcess);
   ```

3. **发送 Bootstrap 请求**:
   ```csharp
   var timeout = ResolveBootstrapTimeout(
       BootstrapTimeoutEnvironmentVariable,
       DefaultBootstrapTimeout);  // 默认 30 秒

   var bootstrap = await workerClient.BootstrapAsync(
       bootstrapRequest,
       timeout);
   ```

4. **验证 Bootstrap 响应**:
   ```csharp
   if (!string.Equals(bootstrap.Metadata.Id, extensionId))
       throw new InvalidOperationException("metadata id mismatch");

   return new OutOfProcessExtensionProxy(
       workerClient,
       bootstrap.Metadata,
       bootstrapRequest,
       bootstrap.Providers);
   ```

**环境变量控制**:
```bash
export JOLT_EXTENSION_BOOTSTRAP_TIMEOUT_MS=60000  # 60 秒
export JOLT_EXTENSION_INVOKE_TIMEOUT_MS=45000      # 45 秒
```

### JSON-RPC 协议

**请求信封** (`ExtensionWorkerRequestEnvelope`):
```json
{
  "id": 1,
  "method": "bootstrap",
  "params": { ... }
}
```

**响应信封** (`ExtensionWorkerResponseEnvelope`):
```json
{
  "id": 1,
  "result": { ... },
  "error": null
}
```

**错误信封**:
```json
{
  "id": 1,
  "result": null,
  "error": {
    "code": "SandboxViolation",
    "message": "sandbox io read denied for path '/etc/passwd'"
  }
}
```

### Bootstrap 消息处理

**方法**: `ExtensionWorkerServer.HandleBootstrapAsync`

**流程**:
1. **验证请求参数**:
   ```csharp
   if (string.IsNullOrWhiteSpace(request.RootDirectory))
       throw new ExtensionWorkerProtocolException(
           ExtensionWorkerErrorCodes.InvalidParams,
           "bootstrap rootDirectory is required.");

   if (string.IsNullOrWhiteSpace(request.AssemblyPath))
       throw new ExtensionWorkerProtocolException(
           ExtensionWorkerErrorCodes.InvalidParams,
           "bootstrap assemblyPath is required.");

   if (string.IsNullOrWhiteSpace(request.ExtensionTypeName))
       throw new ExtensionWorkerProtocolException(
           ExtensionWorkerErrorCodes.InvalidParams,
           "bootstrap extensionTypeName is required.");
   ```

2. **加载扩展程序集** (`CreateExtension`):
   ```csharp
   var loadContext = new CollectibleExtensionLoadContext(assemblyPath);
   var assembly = loadContext.LoadMainAssembly(assemblyPath);
   var extensionType = assembly.GetType(extensionTypeName);

   if (!typeof(IExtension).IsAssignableFrom(extensionType))
       throw new ExtensionWorkerProtocolException(
           ExtensionWorkerErrorCodes.InvalidParams,
           $"extension type '{extensionTypeName}' does not implement IExtension.");

   var extension = Activator.CreateInstance(extensionType) as IExtension;
   ```

3. **初始化和激活扩展**:
   ```csharp
   var context = new ExtensionContext(
       rootDirectory: normalizedRoot,
       extensionDirectory: normalizedExtensionDirectory,
       registry: NullExtensionRegistry.Instance,  // Worker 不注册到主注册表
       settings: request.Settings,
       sandboxProfile: request.SandboxProfile);

   await extension.InitializeAsync(context, cancellationToken);
   await extension.ActivateAsync(cancellationToken);
   ```

4. **发现 Provider 能力** (`DescribeProviders`):
   ```csharp
   var providers = new List<ExtensionWorkerProviderDescriptor>();

   if (extension is ILspDiagnosticProvider diagnosticProvider)
   {
       providers.Add(new ExtensionWorkerProviderDescriptor(
           Capability: ExtensionCapabilityNames.Diagnostic,
           Name: diagnosticProvider.Name,
           Priority: diagnosticProvider.Priority));
   }
   // ... 其他 10 个 provider 接口
   ```

5. **返回 Bootstrap 响应**:
   ```json
   {
     "metadata": {
       "id": "my-extension",
       "name": "My Extension",
       "version": "1.0.0"
     },
     "providers": [
       {
         "capability": "completion",
         "name": "MyCompletionProvider",
         "priority": 100
       }
     ]
   }
   ```

### Invoke 消息处理

**方法**: `ExtensionWorkerServer.HandleInvokeAsync`

**流程**:
1. **验证 Bootstrap 状态**:
   ```csharp
   if (!_bootstrapped || _extension is null)
       throw new ExtensionWorkerProtocolException(
           ExtensionWorkerErrorCodes.NotBootstrapped,
           "worker extension is not bootstrapped.");
   ```

2. **路由到能力处理器**:
   ```csharp
   return request.Capability.Trim() switch
   {
       ExtensionCapabilityNames.Diagnostic => await InvokeDiagnosticAsync(...),
       ExtensionCapabilityNames.CodeAction => await InvokeCodeActionAsync(...),
       ExtensionCapabilityNames.Hover => await InvokeHoverAsync(...),
       ExtensionCapabilityNames.Completion => await InvokeCompletionAsync(...),
       ExtensionCapabilityNames.DocumentSymbol => await InvokeDocumentSymbolAsync(...),
       ExtensionCapabilityNames.SignatureHelp => await InvokeSignatureHelpAsync(...),
       ExtensionCapabilityNames.InlayHint => await InvokeInlayHintAsync(...),
       ExtensionCapabilityNames.WorkspaceSymbol => await InvokeWorkspaceSymbolAsync(...),
       ExtensionCapabilityNames.FoldingRange => await InvokeFoldingRangeAsync(...),
       ExtensionCapabilityNames.References => await InvokeReferencesAsync(...),
       ExtensionCapabilityNames.Rename => await InvokeRenameAsync(...),
       _ => throw new ExtensionWorkerProtocolException(
           ExtensionWorkerErrorCodes.UnsupportedCapability,
           $"unsupported capability '{request.Capability}'.")
   };
   ```

3. **能力处理器示例** (`InvokeDiagnosticAsync`):
   ```csharp
   private static async ValueTask<IReadOnlyList<LspDiagnostic>> InvokeDiagnosticAsync(
       IExtension extension,
       object? context,
       ExtensionSandboxProfile sandboxProfile,
       CancellationToken cancellationToken)
   {
       if (extension is not ILspDiagnosticProvider provider)
           throw new ExtensionWorkerProtocolException(
               ExtensionWorkerErrorCodes.ProviderNotImplemented,
               $"extension does not implement provider capability '{ExtensionCapabilityNames.Diagnostic}'.");

       var typedContext = DeserializeRequired<LspDiagnosticProviderContext>(context, "diagnostic context");

       // 沙箱检查：验证读取权限
       EnsureReadPathAllowed(sandboxProfile, ExtensionCapabilityNames.Diagnostic, typedContext.Document);

       return await provider.ProvideDiagnosticsAsync(typedContext, cancellationToken);
   }
   ```

### 沙箱执行检查

**检查时机**:
- Provider 调用前：验证输入路径/URI
- Provider 返回后：验证输出路径/URI

**IO 读取检查** (`EnsureReadPathAllowed`):
```csharp
private static void EnsureReadPathAllowed(
    ExtensionSandboxProfile sandboxProfile,
    string capability,
    DocumentSnapshot document)
{
    var documentPath = NormalizeDocumentPathForSandbox(document, capability);
    if (sandboxProfile.IsReadPathAllowed(documentPath))
        return;

    throw new ExtensionWorkerProtocolException(
        ExtensionWorkerErrorCodes.SandboxViolation,
        $"sandbox io read denied for capability '{capability}' path '{documentPath}'.");
}
```

**IO 写入检查** (`EnsureWritePathsAllowedForCodeActions`):
```csharp
private static void EnsureWritePathsAllowedForCodeActions(
    ExtensionSandboxProfile sandboxProfile,
    string capability,
    IReadOnlyList<LspCodeAction>? actions)
{
    if (actions is null)
        return;

    foreach (var action in actions)
    {
        if (action?.Edit is null)
            continue;

        foreach (var change in action.Edit.Changes)
        {
            var writePath = ResolveWorkspaceEditWritePath(change.Key, capability);
            if (sandboxProfile.IsWritePathAllowed(writePath))
                continue;

            throw new ExtensionWorkerProtocolException(
                ExtensionWorkerErrorCodes.SandboxViolation,
                $"sandbox io write denied for capability '{capability}' path '{writePath}'.");
        }
    }
}
```

**网络 URI 检查** (`EnsureNetworkUrisAllowedForCodeActions`):
```csharp
private static void EnsureNetworkUrisAllowedForCodeActions(
    ExtensionSandboxProfile sandboxProfile,
    string capability,
    IReadOnlyList<LspCodeAction>? actions,
    string payloadKind)
{
    if (actions is null)
        return;

    foreach (var action in actions)
    {
        if (action?.Edit is null)
            continue;

        EnsureNetworkUrisAllowedForWorkspaceEdit(
            sandboxProfile,
            capability,
            action.Edit,
            payloadKind);
    }
}

private static void EnsureNetworkHostAllowedForUriValue(
    ExtensionSandboxProfile sandboxProfile,
    string capability,
    string? uriValue,
    string payloadKind)
{
    if (!TryParseNetworkUri(uriValue, out var uri))
        return;

    if (sandboxProfile.IsNetworkHostAllowed(uri.Host))
        return;

    throw new ExtensionWorkerProtocolException(
        ExtensionWorkerErrorCodes.SandboxViolation,
        $"sandbox network denied for capability '{capability}' {payloadKind} uri '{uri.AbsoluteUri}'.");
}
```

**网络 URI 解析** (`TryParseNetworkUri`):
```csharp
private static bool TryParseNetworkUri(string? value, out Uri? uri)
{
    uri = null;
    if (string.IsNullOrWhiteSpace(value))
        return false;

    if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var parsed))
        return false;

    if (!NetworkUriSchemes.Contains(parsed.Scheme, StringComparer.OrdinalIgnoreCase))
        return false;  // 仅支持 http, https, ws, wss

    if (string.IsNullOrWhiteSpace(parsed.Host))
        return false;

    uri = parsed;
    return true;
}
```

### 超时控制

**Invoke 超时** (`JOLT_EXTENSION_INVOKE_TIMEOUT_MS`):
```csharp
private static readonly TimeSpan DefaultInvokeTimeout = TimeSpan.FromSeconds(30);

var invokeTimeout = ResolveOperationTimeout(
    InvokeTimeoutEnvironmentVariable,
    DefaultInvokeTimeout);

using var invokeTimeoutSource = CreateOperationTimeoutTokenSource(
    cancellationToken,
    invokeTimeout);

try
{
    invokeResult = await HandleInvokeAsync(invokeRequest, invokeTimeoutSource.Token);
}
catch (OperationCanceledException exception)
    when (!cancellationToken.IsCancellationRequested && invokeTimeoutSource.IsCancellationRequested)
{
    throw new ExtensionWorkerProtocolException(
        ExtensionWorkerErrorCodes.InternalError,
        $"extension capability '{invokeRequest.Capability}' timed out after {invokeTimeout.TotalSeconds:0.###} seconds.",
        exception);
}
```

**超时令牌源** (`CreateOperationTimeoutTokenSource`):
```csharp
private static CancellationTokenSource CreateOperationTimeoutTokenSource(
    CancellationToken cancellationToken,
    TimeSpan timeout)
{
    var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    linkedSource.CancelAfter(timeout);
    return linkedSource;
}
```

### Shutdown 消息处理

**方法**: `ExtensionWorkerServer.ShutdownCoreAsync`

**流程**:
1. **清理状态**:
   ```csharp
   _extension = null;
   _loadContext = null;
   _providerDescriptors = Array.Empty<ExtensionWorkerProviderDescriptor>();
   _sandboxProfile = ExtensionSandboxProfile.Unrestricted;
   _bootstrapped = false;
   ```

2. **停用扩展**:
   ```csharp
   if (extension is not null)
   {
       try
       {
           await extension.DeactivateAsync(cancellationToken);
       }
       catch (Exception)
       {
           // Best-effort shutdown
       }
   }
   ```

3. **卸载加载上下文**:
   ```csharp
   if (loadContext is not null)
   {
       loadContext.Unload();
   }
   ```

4. **退出进程**:
   - Worker 进程退出（stdio 关闭）
   - 主进程检测到进程终止

## 线程安全模型

### ExtensionWorkerServer

**状态保护**:
```csharp
private IExtension? _extension;
private CollectibleExtensionLoadContext? _loadContext;
private IReadOnlyList<ExtensionWorkerProviderDescriptor> _providerDescriptors = Array.Empty<ExtensionWorkerProviderDescriptor>();
private ExtensionSandboxProfile _sandboxProfile = ExtensionSandboxProfile.Unrestricted;
private bool _bootstrapped;
```

**无锁设计**:
- Worker 进程为单线程事件循环
- 无并发请求（主进程串行调用）
- 无锁需求

**假设**: 主进程不会并发调用同一个 `OutOfProcessExtensionProxy`

### OutOfProcessExtensionProxy

**锁策略**:
```csharp
private readonly Lock _workerGate = new();
private readonly Lock _workerRestartHistoryGate = new();
private ExtensionWorkerClient? _workerClient;
```

**保护范围**:
- `_workerClient` 引用（worker 实例）
- `_workerRestartFailures` 队列（重启失败历史）

**原子操作**:
```csharp
private int _deactivateInvoked;  // 原子 int

// 原子交换
if (Interlocked.Exchange(ref _deactivateInvoked, 1) != 0)
    return;  // 已停用
```

## 错误处理

### 协议错误

**错误码** (`ExtensionWorkerErrorCodes`):
```csharp
public const string InvalidRequest = "InvalidRequest";
public const string InvalidParams = "InvalidParams";
public const string UnsupportedMethod = "UnsupportedMethod";
public const string UnsupportedCapability = "UnsupportedCapability";
public const string ProviderNotImplemented = "ProviderNotImplemented";
public const string NotBootstrapped = "NotBootstrapped";
public const string SandboxViolation = "SandboxViolation";
public const string InternalError = "InternalError";
```

**异常类型**: `ExtensionWorkerProtocolException`

**响应格式**:
```json
{
  "id": 1,
  "result": null,
  "error": {
    "code": "SandboxViolation",
    "message": "sandbox io read denied for capability 'diagnostic' path '/etc/passwd'"
  }
}
```

### Worker 崩溃恢复

**崩溃检测**: `ExtensionWorkerConnectionException`

**恢复策略**: 自动重启 + 断路器

**重启策略** (`WorkerRestartPolicy`):
```csharp
private readonly record struct WorkerRestartPolicy(
    TimeSpan Window,       // 默认 1 分钟
    int MaxRestarts,       // 默认 3 次
    TimeSpan BaseDelay);   // 默认 250 毫秒
```

**环境变量控制**:
```bash
export JOLT_EXTENSION_WORKER_RESTART_WINDOW_MS=60000      # 1 分钟
export JOLT_EXTENSION_WORKER_MAX_RESTARTS=5               # 最多 5 次
export JOLT_EXTENSION_WORKER_RESTART_BASE_DELAY_MS=500    # 初始延迟 500 毫秒
```

**重启决策** (`RegisterWorkerRestartFailure`):
```csharp
var now = DateTimeOffset.UtcNow;
lock (_workerRestartHistoryGate)
{
    // 清理过期失败记录
    while (_workerRestartFailures.Count > 0
        && now - _workerRestartFailures.Peek() > _workerRestartPolicy.Window)
    {
        _workerRestartFailures.Dequeue();
    }

    // 记录本次失败
    _workerRestartFailures.Enqueue(now);
    var failureCount = _workerRestartFailures.Count;

    // 超过最大重启次数
    if (failureCount > _workerRestartPolicy.MaxRestarts)
    {
        return new WorkerRestartDecision(
            AllowRestart: false,
            Delay: TimeSpan.Zero,
            FailureCount: failureCount);
    }

    // 指数退避延迟
    if (failureCount <= 1 || _workerRestartPolicy.BaseDelay <= TimeSpan.Zero)
    {
        return new WorkerRestartDecision(AllowRestart: true, Delay: TimeSpan.Zero);
    }

    var exponent = Math.Min(failureCount - 2, 8);
    var delayMilliseconds = _workerRestartPolicy.BaseDelay.TotalMilliseconds * Math.Pow(2, exponent);
    var delay = TimeSpan.FromMilliseconds(
        Math.Min(delayMilliseconds, MaxWorkerRestartBackoff.TotalMilliseconds));

    return new WorkerRestartDecision(
        AllowRestart: true,
        Delay: delay,
        FailureCount: failureCount);
}
```

**重启流程** (`RestartWorkerAsync`):
```csharp
await _workerRestartGate.WaitAsync(cancellationToken);
try
{
    var currentWorker = GetWorkerClient();
    if (!ReferenceEquals(currentWorker, failedWorker))
        return currentWorker;  // 已被其他线程替换

    var restartDecision = RegisterWorkerRestartFailure();
    if (!restartDecision.AllowRestart)
        throw new ExtensionWorkerConnectionException(
            $"extension worker restart circuit opened after {restartDecision.FailureCount} failures.");

    if (restartDecision.Delay > TimeSpan.Zero)
        await Task.Delay(restartDecision.Delay, cancellationToken);

    var (replacementWorker, _) = await CreateBootstrappedWorkerAsync(
        _bootstrapRequest,
        cancellationToken);

    lock (_workerGate)
    {
        if (_workerClient is not null && ReferenceEquals(_workerClient, failedWorker))
        {
            _workerClient = replacementWorker;
            await DisposeWorkerSilentlyAsync(failedWorker);
            return replacementWorker;
        }
    }

    // 并发替换，丢弃新 worker
    await DisposeWorkerSilentlyAsync(replacementWorker);
    return GetWorkerClient();
}
finally
{
    _workerRestartGate.Release();
}
```

### 超时错误

**Bootstrap 超时**:
```csharp
throw new TimeoutException(
    $"extension worker bootstrap timed out after {timeout.TotalSeconds:0.###} seconds.");
```

**Invoke 超时**:
```csharp
throw new ExtensionWorkerProtocolException(
    ExtensionWorkerErrorCodes.InternalError,
    $"extension capability '{capability}' timed out after {timeout.TotalSeconds:0.###} seconds.");
```

## 配置选项

### 超时配置

| 环境变量 | 默认值 | 说明 |
|---------|--------|------|
| `JOLT_EXTENSION_BOOTSTRAP_TIMEOUT_MS` | 30000 (30秒) | Worker 启动超时 |
| `JOLT_EXTENSION_INVOKE_TIMEOUT_MS` | 30000 (30秒) | Provider 调用超时 |

### 重启配置

| 环境变量 | 默认值 | 说明 |
|---------|--------|------|
| `JOLT_EXTENSION_WORKER_RESTART_WINDOW_MS` | 60000 (1分钟) | 重启失败时间窗口 |
| `JOLT_EXTENSION_WORKER_MAX_RESTARTS` | 3 | 窗口内最大重启次数 |
| `JOLT_EXTENSION_WORKER_RESTART_BASE_DELAY_MS` | 250 | 指数退避基础延迟 |

### 清单配置

**进程隔离声明**:
```json
{
  "permissions": {
    "processIsolation": true
  }
}
```

**主机策略强制**:
```json
{
  "extensions": {
    "requireProcessIsolation": true
  }
}
```

## 与其他子系统的交互

### 与 ExtensionLoader 的交互

**进程隔离扩展加载**:
1. `ExtensionLoader` 检测 `processIsolation: true`
2. 调用 `OutOfProcessExtensionProxy.CreateAsync`
3. 启动 worker 进程并 bootstrap
4. 验证 provider 能力
5. 注册 proxy 到 `ExtensionRegistry`

### 与 ExtensionSecurityPolicy 的交互

**沙箱配置传递**:
```csharp
var sandboxProfile = ExtensionSecurityPolicy.CreateRuntimeSandboxProfile(
    manifest,
    options.RootDirectory,
    extensionDirectory);

var proxy = await OutOfProcessExtensionProxy.CreateAsync(
    rootDirectory,
    extensionDirectory,
    assemblyPath,
    extensionTypeName,
    sandboxProfile,
    settings,
    cancellationToken);
```

**运行时验证**:
- Worker 服务器使用 `sandboxProfile` 验证所有 IO/网络操作
- 违规时抛出 `ExtensionWorkerProtocolException`

### 与 LSP 系统的交互

**Provider 调用转发**:
```csharp
async ValueTask<IReadOnlyList<LspCompletionItem>> ILspCompletionProvider.ProvideCompletionItemsAsync(
    LspCompletionProviderContext context,
    CancellationToken cancellationToken)
{
    return await InvokeOrDefaultAsync(
        ExtensionCapabilityNames.Completion,
        context,
        defaultValue: Array.Empty<LspCompletionItem>(),
        cancellationToken);
}
```

**上下文序列化**:
- Provider 上下文序列化为 JSON
- 通过 stdio 发送到 worker
- Worker 反序列化并调用扩展

## 设计权衡

### Stdio JSON-RPC vs Named Pipes / TCP

**Stdio JSON-RPC 优势**:
- 跨平台兼容（无需特定 IPC 机制）
- 简单调试（可手动输入/输出 JSON）
- 防火墙友好（本地进程）

**Stdio JSON-RPC 劣势**:
- 序列化开销（JSON 文本）
- 单工通信（单向流）
- 无法双向流式传输

**当前选择**: Stdio JSON-RPC
- 满足扩展场景需求（低频调用）
- 简化实现（无需复杂 IPC 库）
- 未来可升级到 gRPC/Named Pipes

### 进程级隔离 vs AppDomain 隔离

**进程隔离优势**:
- 完全内存隔离（崩溃不影响主进程）
- 可卸载（进程终止保证资源释放）
- 跨平台（.NET 5+ 不支持 AppDomain）

**AppDomain 隔离劣势**:
- .NET Core 不支持（仅 .NET Framework）
- 内存泄漏风险（共享 GC 堆）
- 复杂卸载（需要域卸载）

**当前选择**: 进程隔离
- 现代跨平台方案
- 完全隔离保证
- 符合云原生安全模型

### 自动重启 vs 快速失败

**自动重启优势**:
- 容错性（临时崩溃自动恢复）
- 用户体验（无需手动重启）
- 可观测性（记录重启历史）

**自动重启劣势**:
- 掩盖问题（崩溃原因未修复）
- 资源浪费（频繁重启消耗 CPU）
- 级联故障（重启风暴）

**当前策略**: 自动重启 + 断路器
- 窗口内最多重启 N 次（默认 3 次）
- 超过限制后快速失败
- 指数退避延迟（250ms → 5s）

### 同步 JSON-RPC vs 异步流式

**同步请求-响应**:
- 一次请求 → 一次响应
- 简单可靠
- 适合低频调用

**异步流式**:
- 持续推送事件
- 支持增量结果
- 复杂度高

**当前选择**: 同步请求-响应
- LSP provider 调用为同步语义
- 简化错误处理
- 未来可支持流式（如进度通知）
