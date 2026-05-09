# Deno Worker 进程管理

Deno 子进程的低级管理器：`IDenoWorkerProcess` 接口、`DenoWorkerProcess` 实现类。负责启动和管理 Deno 子进程，通过 stdin/stdout 实现 JSON-RPC 通信，捕获 stderr 输出用于诊断，提供工作区隔离机制。

相关源文件：
- `src/Jolt/Frontend/Deno/Hosting/IDenoWorkerProcess.cs` - 接口定义
- `src/Jolt/Frontend/Deno/Hosting/DenoWorkerProcess.cs` - 主实现类
- `src/Jolt/Frontend/Deno/Hosting/DenoVolarHost.cs` - 上层管理器（独立文档）
- `src/Jolt/Frontend/Deno/Protocol/DenoFrontendProtocol.cs` - 请求/响应协议（独立文档）

## 核心类型

### `IDenoWorkerProcess` 接口

Deno worker 进程的抽象接口，提供基本的进程管理和 RPC 通信功能。

**状态属性**：
- `bool IsRunning { get; }` - 进程是否正在运行（检查 `Process.HasExited`）

**生命周期方法**：
- `ValueTask StartAsync(CancellationToken cancellationToken)` - 启动 Deno 子进程
- `ValueTask StopAsync(CancellationToken cancellationToken)` - 停止 Deno 子进程

**通信方法**：
- `ValueTask<TResult?> SendRequestAsync<TResult>(string method, object payload, CancellationToken cancellationToken)`
  - 发送 JSON-RPC 请求到 Deno worker
  - 等待响应并反序列化为指定类型
  - 支持任意请求/响应类型（通过泛型参数）

### `DenoWorkerProcess` 实现

核心实现类，直接管理 `System.Diagnostics.Process` 实例。

**核心字段**：
```csharp
private readonly DenoVolarHostOptions _options;
private readonly SemaphoreSlim _lifecycleGate = new(1, 1);  // 生命周期门控
private readonly SemaphoreSlim _requestGate = new(1, 1);     // 请求序列化门控
private readonly JsonSerializerOptions _jsonOptions;         // JSON 序列化选项
private readonly Lock _standardErrorGate = new();            // stderr 缓冲区锁
private readonly Queue<string> _standardErrorLines = [];      // stderr 行缓冲队列（最多 32 行）
private Process? _process;                                    // Deno 子进程
private StreamWriter? _writer;                                // stdin 写入器
private StreamReader? _reader;                                // stdout 读取器
private Task? _standardErrorPumpTask;                         // stderr pump 任务
private CancellationTokenSource? _standardErrorPumpCancellationSource;
private string? _launchWorkingDirectory;                      // 启动工作目录（隔离工作区）
private int _droppedStandardErrorLineCount;                   // 丢弃的 stderr 行数
```

**常量**：
```csharp
private const int MaxCapturedStandardErrorLines = 32;  // 最多保留 32 行 stderr
```

**静态字段**（工作区隔离）：
```csharp
private static int _launchWorkspaceSequence;                      // 工作区序列号
private static readonly Lock LaunchWorkspaceCleanupGate = new();  // 工作区清理门控
private static readonly HashSet<string> LaunchWorkspaces = new(StringComparer.OrdinalIgnoreCase);
private static bool _launchWorkspaceCleanupHookRegistered;        // 进程退出清理钩子注册标志
```

## 核心算法

### 进程启动流程（StartAsync）

```
1. 等待 _lifecycleGate（确保线程安全）
2. 检查 IsRunning，如果已运行则直接返回
3. 验证配置：
   a. 检查 ExecutablePath 非空
   b. 如果没有显式覆盖且是绝对路径，检查文件是否存在
   c. 如果不存在，抛出异常（包含 DenoRuntimeAssetResolver 的错误消息）
4. 创建 ProcessStartInfo：
   a. FileName = ExecutablePath
   b. UseShellExecute = false
   c. RedirectStandardInput/Output/Error = true
   d. CreateNoWindow = true
   e. StandardErrorEncoding/StandardOutputEncoding = UTF8
   f. 添加 Arguments 到 ArgumentList
5. 解析工作目录（ResolveLaunchWorkingDirectory）：
   a. 如果配置了 WorkingDirectory，使用配置的目录
   b. 如果工作目录 == worker 脚本所在目录，且有 deno.json：
      - 创建临时隔离工作区（格式：{TempPath}/Jolt/Deno/Workspaces/{ProcessId}-{Sequence}）
      - 复制配置文件（deno.json, deno.lock, package.json, package-lock.json, npm-shrinkwrap.json）
      - 注册清理钩子（ProcessExit 事件）
      - 返回临时目录路径
   c. 否则返回配置的目录
6. 创建并启动 Process：
   a. new Process { StartInfo = startInfo }
   b. ResetStandardErrorBuffer() - 清空 stderr 缓冲区
   c. 调用 _process.Start()
   d. 捕获 Win32Exception 并转换为友好的错误消息
7. 创建 stdin/stdout 管道：
   a. _writer = new StreamWriter(_process.StandardInput.BaseStream, UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" }
   b. _reader = new StreamReader(_process.StandardOutput.BaseStream, Encoding.UTF8)
8. 启动 stderr pump：
   a. _standardErrorPumpCancellationSource = new CancellationTokenSource()
   b. _standardErrorPumpTask = PumpStandardErrorAsync(_process.StandardError, ...)
9. 释放 _lifecycleGate
```

**ResolveLaunchWorkingDirectory 详细逻辑**：

```csharp
private string? ResolveLaunchWorkingDirectory()
{
    // 1. 如果没有配置工作目录，返回 null
    if (string.IsNullOrWhiteSpace(_options.WorkingDirectory))
        return null;

    // 2. 获取绝对路径
    var configuredWorkingDirectory = Path.GetFullPath(_options.WorkingDirectory);
    var workerDirectory = Path.GetDirectoryName(_options.WorkerScriptPath);

    // 3. 如果 worker 目录不存在，使用配置目录
    if (string.IsNullOrWhiteSpace(workerDirectory))
        return configuredWorkingDirectory;

    // 4. 如果配置目录 != worker 目录，使用配置目录
    var normalizedWorkerDirectory = Path.GetFullPath(workerDirectory);
    if (!string.Equals(configuredWorkingDirectory, normalizedWorkerDirectory, StringComparison.OrdinalIgnoreCase))
        return configuredWorkingDirectory;

    // 5. 如果 worker 目录没有 deno.json，使用配置目录
    if (!File.Exists(Path.Combine(normalizedWorkerDirectory, "deno.json")))
        return configuredWorkingDirectory;

    // 6. 如果已经创建了临时工作区，重用它
    if (!string.IsNullOrWhiteSpace(_launchWorkingDirectory))
        return _launchWorkingDirectory;

    // 7. 创建临时隔离工作区
    var launchWorkspaceRoot = string.IsNullOrWhiteSpace(_options.CacheDirectory)
        ? Path.Combine(Path.GetTempPath(), "Jolt", "Deno", "Workspaces")
        : Path.Combine(_options.CacheDirectory, "workspaces");
    var launchWorkspaceDirectory = Path.Combine(
        launchWorkspaceRoot,
        $"{Environment.ProcessId:D6}-{Interlocked.Increment(ref _launchWorkspaceSequence):D4}");

    // 8. 创建目录并复制配置文件
    Directory.CreateDirectory(launchWorkspaceDirectory);
    CopyWorkerConfigurationFiles(normalizedWorkerDirectory, launchWorkspaceDirectory);

    // 9. 注册清理钩子
    RegisterLaunchWorkspaceForCleanup(launchWorkspaceDirectory);
    _launchWorkingDirectory = launchWorkspaceDirectory;

    return launchWorkspaceDirectory;
}
```

**CopyWorkerConfigurationFiles 实现**：
```csharp
private static void CopyWorkerConfigurationFiles(
    string sourceDirectory,
    string destinationDirectory)
{
    foreach (var fileName in new[] {
        "deno.json",       // Deno 配置文件
        "deno.lock",       // Deno 锁文件
        "package.json",    // npm 包配置
        "package-lock.json", // npm 锁文件
        "npm-shrinkwrap.json" // npm shrinkwrap 文件
    })
    {
        var sourcePath = Path.Combine(sourceDirectory, fileName);
        if (!File.Exists(sourcePath))
            continue;

        File.Copy(sourcePath, Path.Combine(destinationDirectory, fileName), overwrite: true);
    }
}
```

**设计意图**：
- **工作区隔离**：避免多个 worker 进程共享同一个 deno.json 和 lock 文件，防止并发冲突
- **临时工作区**：使用进程 ID + 序列号创建唯一工作区，确保每个 worker 实例独立
- **配置文件复制**：保留原始配置（依赖、导入映射、权限等）
- **自动清理**：注册进程退出钩子，确保临时工作区被删除

### 请求发送流程（SendRequestAsync）

```
1. 参数验证
2. ThrowIfWorkerUnavailable() - 检查 worker 是否可用
3. 等待 _requestGate（确保串行发送）
4. 再次 ThrowIfWorkerUnavailable() - 检查 worker 是否仍然可用
5. 构造请求信封：
   a. Id = Guid.NewGuid().ToString("N")（无连字符的 GUID）
   b. Method = method
   c. Payload = payload
6. 序列化请求（JSON）并写入 stdin：
   a. await _writer.WriteLineAsync(JsonSerializer.Serialize(request, _jsonOptions))
7. 从 stdout 读取响应行：
   a. var responseLine = await _reader.ReadLineAsync(cancellationToken)
   b. 如果为空，抛出异常（附加 stderr 摘要）
8. 反序列化响应信封：
   a. var response = JsonSerializer.Deserialize<DenoFrontendResponseEnvelope>(responseLine, _jsonOptions)
   b. 如果为 null，抛出异常
9. 检查响应状态：
   a. if (!response.Success) - 抛出异常（包含 response.Error 和 stderr 摘要）
10. 反序列化结果：
    a. if (response.Result is null || ValueKind == Null) - 返回 default
    b. return response.Result.Value.Deserialize<TResult>(_jsonOptions)
11. 释放 _requestGate
```

**ThrowIfWorkerUnavailable 实现**：
```csharp
[MemberNotNull(nameof(_process), nameof(_writer), nameof(_reader))]
private void ThrowIfWorkerUnavailable()
{
    // 1. 检查进程正在运行且管道已初始化
    if (_process is { HasExited: false }
        && _writer is not null
        && _reader is not null)
    {
        return;
    }

    // 2. 检查进程是否已退出
    if (_process is { HasExited: true } exitedProcess)
    {
        throw new InvalidOperationException(
            $"Deno frontend worker exited unexpectedly with code {exitedProcess.ExitCode}.{CreateStandardErrorSummarySuffix()}");
    }

    // 3. 进程未启动
    throw new InvalidOperationException($"Deno frontend worker is not running.{CreateStandardErrorSummarySuffix()}");
}
```

**JSON 序列化选项**：
```csharp
private readonly JsonSerializerOptions _jsonOptions = new()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // camelCase 命名
    PropertyNameCaseInsensitive = true                   // 不区分大小写
};
```

**设计意图**：
- **请求序列化**：使用 GUID 作为请求 ID，确保唯一性（N 格式去掉连字符，节省空间）
- **错误诊断**：所有异常都附加 stderr 摘要，帮助诊断问题
- **线程安全**：使用 `_requestGate` 确保请求串行发送（JSON-RPC over stdin/stdout 本质上是单线程的）

### 进程停止流程（StopAsync）

```
1. 等待 _lifecycleGate
2. 检查 _process 是否为 null，如果是则直接返回
3. 尝试终止进程：
   a. if (!_process.HasExited) - _process.Kill(entireProcessTree: true)
   b. await _process.WaitForExitAsync(cancellationToken)
4. 清理资源（在 finally 块中）：
   a. await StopStandardErrorPumpAsync() - 停止 stderr pump
   b. _writer?.Dispose() - 释放 stdin
   c. _reader?.Dispose() - 释放 stdout
   d. _process.Dispose() - 释放进程
   e. CleanupLaunchWorkingDirectory() - 清理临时工作区
5. 释放 _lifecycleGate
```

**StopStandardErrorPumpAsync 实现**：
```csharp
private async ValueTask StopStandardErrorPumpAsync()
{
    var pumpCancellationSource = _standardErrorPumpCancellationSource;
    var pumpTask = _standardErrorPumpTask;
    _standardErrorPumpCancellationSource = null;
    _standardErrorPumpTask = null;

    // 1. 取消 pump 任务
    if (pumpCancellationSource is not null)
    {
        try
        {
            pumpCancellationSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    // 2. 等待 pump 任务完成
    if (pumpTask is not null)
    {
        try
        {
            await pumpTask;
        }
        catch (OperationCanceledException)
        {
            // 预期的取消异常，忽略
        }
    }

    // 3. 释放 cancellation source
    pumpCancellationSource?.Dispose();
}
```

**CleanupLaunchWorkingDirectory 实现**：
```csharp
private void CleanupLaunchWorkingDirectory()
{
    // 1. 从静态集合中移除（避免进程退出钩子重复删除）
    string? launchWorkingDirectory;
    lock (LaunchWorkspaceCleanupGate)
    {
        launchWorkingDirectory = _launchWorkingDirectory;
        _launchWorkingDirectory = null;
        if (string.IsNullOrWhiteSpace(launchWorkingDirectory))
        {
            return;
        }

        LaunchWorkspaces.Remove(launchWorkingDirectory);
    }

    // 2. 尝试删除目录
    TryDeleteLaunchWorkspace(launchWorkingDirectory);
}

private static void TryDeleteLaunchWorkspace(string launchWorkspaceDirectory)
{
    try
    {
        if (Directory.Exists(launchWorkspaceDirectory))
        {
            Directory.Delete(launchWorkspaceDirectory, recursive: true);
        }
    }
    catch (IOException)
    {
        // 忽略 IO 错误（文件可能被占用）
    }
    catch (UnauthorizedAccessException)
    {
        // 忽略权限错误
    }
}
```

**设计意图**：
- **强制终止**：使用 `Kill(entireProcessTree: true)` 确保整个进程树被终止（防止子进程孤儿）
- **优雅清理**：先停止 stderr pump，再释放资源，避免竞态条件
- **工作区清理**：立即删除临时工作区，避免磁盘空间浪费
- **容错设计**：清理操作忽略所有异常，确保即使清理失败也不会影响系统状态

### Stderr Pump 流程（PumpStandardErrorAsync）

```
1. 循环读取 stderr 行：
   a. line = await standardErrorReader.ReadLineAsync(cancellationToken)
   b. 如果捕获到 OperationCanceledException（cancellationToken.IsCancellationRequested），返回
   c. 如果 line 为 null（EOF），返回
   d. 调用 CaptureStandardErrorLine(line) 捕获行
2. 捕获异常并忽略：
   a. IOException - 管道关闭
   b. ObjectDisposedException - 对象已释放
```

**CaptureStandardErrorLine 实现**：
```csharp
private void CaptureStandardErrorLine(string line)
{
    lock (_standardErrorGate)
    {
        // 1. 如果缓冲区已满，移除最旧的行
        while (_standardErrorLines.Count >= MaxCapturedStandardErrorLines)
        {
            _standardErrorLines.Dequeue();
            _droppedStandardErrorLineCount++;
        }

        // 2. 添加新行
        _standardErrorLines.Enqueue(line);
    }
}
```

**CreateStandardErrorSummarySuffix 实现**：
```csharp
private string CreateStandardErrorSummarySuffix()
{
    lock (_standardErrorGate)
    {
        if (_standardErrorLines.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(" stderr: ");

        // 1. 如果有丢弃的行，添加计数
        if (_droppedStandardErrorLineCount > 0)
        {
            builder.Append('(');
            builder.Append(_droppedStandardErrorLineCount);
            builder.Append(" earlier stderr lines omitted)");
            if (_standardErrorLines.Count > 0)
            {
                builder.AppendLine();
            }
        }

        // 2. 添加所有缓冲的行
        for (var index = 0; index < _standardErrorLines.Count; index++)
        {
            if (index > 0)
            {
                builder.AppendLine();
            }

            builder.Append(_standardErrorLines[index]);
        }

        return builder.ToString();
    }
}
```

**设计意图**：
- **环形缓冲**：保留最近的 32 行 stderr，丢弃旧行（避免内存泄漏）
- **线程安全**：使用 `Lock` 保护缓冲区访问
- **诊断友好**：所有异常都附加 stderr 摘要，帮助诊断问题
- **异步读取**：使用独立任务 pump stderr，避免阻塞 stdout 读取

### 进程退出清理钩子

**注册清理钩子**（`RegisterLaunchWorkspaceForCleanup`）：
```csharp
private static void RegisterLaunchWorkspaceForCleanup(string launchWorkspaceDirectory)
{
    lock (LaunchWorkspaceCleanupGate)
    {
        LaunchWorkspaces.Add(launchWorkspaceDirectory);

        // 只注册一次 ProcessExit 钩子
        if (_launchWorkspaceCleanupHookRegistered)
        {
            return;
        }

        AppDomain.CurrentDomain.ProcessExit += static (_, _) => CleanupLaunchWorkspaces();
        _launchWorkspaceCleanupHookRegistered = true;
    }
}
```

**清理所有工作区**（`CleanupLaunchWorkspaces`）：
```csharp
private static void CleanupLaunchWorkspaces()
{
    // 1. 复制集合（避免在锁内删除）
    string[] launchWorkspaceDirectories;
    lock (LaunchWorkspaceCleanupGate)
    {
        launchWorkspaceDirectories = LaunchWorkspaces.ToArray();
        LaunchWorkspaces.Clear();
    }

    // 2. 尝试删除每个工作区
    foreach (var launchWorkspaceDirectory in launchWorkspaceDirectories)
    {
        try
        {
            if (Directory.Exists(launchWorkspaceDirectory))
            {
                Directory.Delete(launchWorkspaceDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
```

**设计意图**：
- **全局清理**：进程退出时清理所有临时工作区（即使 StopAsync 未被调用）
- **单次注册**：使用 `_launchWorkspaceCleanupHookRegistered` 标志避免重复注册
- **容错设计**：忽略所有删除失败异常（文件可能被占用）

## 线程安全模型

### 双锁设计

**生命周期锁（`_lifecycleGate`）**：
- **类型**：`SemaphoreSlim(1, 1)` - 互斥锁
- **保护的操作**：`StartAsync`, `StopAsync`
- **设计意图**：防止并发启动或停止，确保进程状态变更的原子性

**请求锁（`_requestGate`）**：
- **类型**：`SemaphoreSlim(1, 1)` - 互斥锁
- **保护的操作**：`SendRequestAsync`
- **设计意图**：确保请求串行发送（JSON-RPC over stdin/stdout 本质上是单线程的）

**Stderr 锁（`_standardErrorGate`）**：
- **类型**：`Lock` - 互斥锁（.NET 9+）
- **保护的操作**：`_standardErrorLines`, `_droppedStandardErrorLineCount`
- **设计意图**：保护 stderr 缓冲区访问，防止数据竞争

**工作区清理锁（`LaunchWorkspaceCleanupGate`）**：
- **类型**：`Lock` - 静态互斥锁
- **保护的操作**：`LaunchWorkspaces`, `_launchWorkspaceCleanupHookRegistered`, `_launchWorkspaceSequence`
- **设计意图**：保护全局静态状态，防止并发修改

### 锁分离设计

**为什么需要两个独立的锁？**

- **生命周期锁**：保护进程状态变更（启动、停止），操作时间长（涉及进程创建、销毁）
- **请求锁**：保护请求发送，操作时间短（序列化、IO）

**分离的好处**：
- 启动/停止操作不会阻塞正在进行的请求
- 请求发送不会阻塞启动/停止操作
- 提高并发性能

**潜在竞态条件**：
- `SendRequestAsync` 中调用 `ThrowIfWorkerUnavailable()` 时不持有 `_lifecycleGate`
- 可能出现：检查时进程正在运行，发送时进程已停止

**缓解措施**：
- `SendRequestAsync` 在发送前和发送后都检查 `ThrowIfWorkerUnavailable()`
- 如果进程意外退出，抛出 `InvalidOperationException`（包含 stderr 摘要）
- 上层 `DenoVolarHost` 捕获异常并重试（最多 3 次）

### 无锁读取

**只读属性**（无需锁保护）：
- `IsRunning` - 直接检查 `_process.HasExited`（`Process.HasExited` 是线程安全的）

### 异步取消安全

**stderr pump 取消**：
```csharp
try
{
    line = await standardErrorReader.ReadLineAsync(cancellationToken);
}
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    return;  // 预期的取消，正常返回
}
```

**设计意图**：使用 `when` 子句确保只捕获预期的取消异常，避免掩盖其他问题

## 错误处理

### 启动失败处理

**场景**：
- Deno 可执行文件不存在
- 权限不足
- 路径错误

**处理策略**：
- 如果没有显式覆盖路径（`!HasExplicitExecutableOverride`）且文件不存在：
  - 抛出 `InvalidOperationException`（包含 `DenoRuntimeAssetResolver.CreateMissingRuntimeMessage()` 的友好错误消息）
- 捕获 `Win32Exception` 并转换为友好的错误消息（同样使用 `DenoRuntimeAssetResolver.CreateMissingRuntimeMessage()`）
- 其他异常直接抛出

**友好错误消息示例**：
```
Failed to locate the packaged Deno runtime for Jolt at 'runtimes/win-x64/native/deno.exe'.
Ensure DenoHost runtime assets are available for the current RID and restore/build Jolt before starting the Volar worker.
```

### 请求失败处理

**场景**：
- Worker 进程意外退出
- IO 通道故障
- JSON 反序列化失败
- Worker 返回错误响应

**处理策略**：
- 所有异常都附加 stderr 摘要（`CreateStandardErrorSummarySuffix()`）
- 抛出 `InvalidOperationException`，包含：
  - 失败的方法名
  - Worker 返回的错误消息（如果有）
  - 最近 32 行 stderr
  - 丢弃的 stderr 行数

**异常消息示例**：
```
Deno frontend worker request 'template/completion' failed: TypeError: Cannot read property 'forEach' of undefined. stderr: (5 earlier stderr lines omitted)
error: Uncaught (in promise) TypeError: Cannot read property 'forEach' of undefined
    at file:///path/to/frontend-worker.ts:42:15
    at ...
```

### Worker 意外退出处理

**场景**：Worker 进程在运行期间崩溃

**检测方法**：
- `ThrowIfWorkerUnavailable()` 检查 `_process.HasExited`
- 如果为 true，抛出异常（包含退出码和 stderr 摘要）

**异常消息示例**：
```
Deno frontend worker exited unexpectedly with code 1. stderr: error: Uncaught TypeError: ...
```

### Stderr 捕获失败处理

**场景**：Stderr pump 任务异常（管道关闭、对象释放）

**处理策略**：
- 捕获 `IOException` 和 `ObjectDisposedException` 并忽略
- 设计意图：stderr pump 是辅助功能，不应该影响主流程

## 配置选项

### 必需配置

| 配置项 | 说明 | 示例 |
|--------|------|------|
| `ExecutablePath` | Deno 可执行文件路径 | `runtimes/win-x64/native/deno.exe` |
| `WorkerScriptPath` | Worker 脚本路径（用于解析工作目录） | `Frontend/Deno/Worker/frontend-worker.ts` |

### 可选配置

| 配置项 | 说明 | 默认值 | 影响 |
|--------|------|--------|------|
| `CacheDirectory` | Deno 缓存目录（DENO_DIR） | `""` | 如果非空，工作区创建在 `{CacheDirectory}/workspaces/` 下 |
| `WorkingDirectory` | Deno 工作目录 | `null` | 如果与 worker 目录不同，不创建隔离工作区 |
| `Arguments` | Deno 命令行参数 | `[]` | 传递给 Deno 进程 |
| `HasExplicitExecutableOverride` | 是否显式覆盖可执行文件路径 | `false` | 影响启动失败错误消息 |

### 工作区隔离配置

**隔离条件**（同时满足）：
1. `WorkingDirectory` 配置为 worker 脚本所在目录
2. worker 目录存在 `deno.json` 文件

**隔离效果**：
- 创建临时工作区：`{TempPath}/Jolt/Deno/Workspaces/{ProcessId}-{Sequence}` 或 `{CacheDirectory}/workspaces/{ProcessId}-{Sequence}`
- 复制配置文件：`deno.json`, `deno.lock`, `package.json`, `package-lock.json`, `npm-shrinkwrap.json`
- 注册进程退出清理钩子

**不隔离的情况**：
- `WorkingDirectory` 未配置
- `WorkingDirectory` 不等于 worker 目录
- worker 目录没有 `deno.json`

## 与其他子系统的交互

### 与 DenoVolarHost 的交互

**关系**：`DenoVolarHost` 是 `DenoWorkerProcess` 的上层管理器

**交互模式**：
- 生命周期管理：调用 `StartAsync` / `StopAsync`
- 请求发送：调用 `SendRequestAsync<TResult>(method, payload, cancellationToken)`
- 状态查询：访问 `IsRunning` 属性

**关注点分离**：
- `DenoVolarHost`：高级 API、重试逻辑、故障恢复
- `DenoWorkerProcess`：低级进程管理、stdin/stdout 通信、线程安全

### 与 Deno Worker TypeScript 脚本的交互

**通信协议**：JSON-RPC over stdin/stdout

**请求格式**：
```json
{
  "id": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
  "method": "template/completion",
  "payload": {
    "documentPath": "/path/to/document.vue",
    "text": "<template>...</template>",
    "position": { "line": 0, "character": 0 },
    "frontendContext": { ... },
    "frontendArtifacts": [ ... ]
  }
}
```

**响应格式**：
```json
{
  "id": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
  "success": true,
  "result": [ ... ],  // 或 null
  "error": null       // 或错误消息
}
```

**消息边界**：每行一个 JSON 对象（使用 `WriteLineAsync` / `ReadLineAsync`）

**设计意图**：
- 简单、跨语言、易于调试
- 与 Volar/LSP 协议一致（JSON-RPC）
- 每行一个消息，避免帧边界问题

### 7.3 与操作系统进程管理的交互

**进程创建**：使用 `System.Diagnostics.Process`

**进程终止**：`_process.Kill(entireProcessTree: true)` - .NET 6+ 特性

**进程等待**：`await _process.WaitForExitAsync(cancellationToken)` - .NET 6+ 异步 API

**环境变量**：`startInfo.Environment["DENO_DIR"] = _options.CacheDirectory`

**设计意图**：
- 使用现代 .NET 异步 API
- 确保整个进程树被终止（防止子进程孤儿）
- 支持取消令牌

## 8. 设计权衡

### 8.1 工作区隔离 vs 共享工作区

**权衡**：
- **隔离工作区**：每个 worker 实例独立的工作区，避免并发冲突，但增加磁盘使用
- **共享工作区**：多个 worker 共享同一工作区，节省磁盘空间，但可能导致锁冲突

**选择**：条件隔离
- 如果 `WorkingDirectory == worker 目录` 且有 `deno.json`，创建隔离工作区
- 否则使用配置的目录

**设计依据**：
- 大多数情况下，用户配置的工作目录与 worker 目录不同（如项目根目录），不需要隔离
- 只有在默认配置（worker 目录）时才隔离，避免影响用户的工作区

### 8.2 Stderr 缓冲限制（32 行）

**权衡**：
- **限制缓冲**：避免内存泄漏，但可能丢失早期诊断信息
- **无限缓冲**：保留所有 stderr，但可能导致内存溢出

**选择**：限制为 32 行，记录丢弃的行数

**设计依据**：
- 大多数错误相关的 stderr 出现在进程崩溃前后
- 32 行足够覆盖最近的错误上下文
- 记录丢弃行数，用户可以知道有更多信息被省略

### 8.3 双锁 vs 单锁

**权衡**：
- **双锁**：生命周期锁和请求锁分离，提高并发性能，但增加复杂度
- **单锁**：只有一个全局锁，简单但性能较差

**选择**：双锁

**设计依据**：
- 启动/停止操作时间长（进程创建、销毁），不应该阻塞请求
- 请求发送频繁，不应该被启动/停止阻塞
- 通过仔细设计避免竞态条件（双重检查、异常捕获）

### 8.4 进程退出清理钩子

**权衡**：
- **清理钩子**：确保临时工作区被清理，但 `AppDomain.CurrentDomain.ProcessExit` 事件在所有平台上可靠性不同
- **不清理钩子**：依赖 `StopAsync` 清理，但进程崩溃时可能未调用

**选择**：两者结合
- `StopAsync` 中立即清理（正常情况）
- 进程退出钩子兜底（异常情况）

**限制**：
- Windows: 可靠
- Linux: 可靠（SIGTERM）
- macOS: 可能不可靠（SIGKILL 无法捕获）

**缓解措施**：
- 使用临时目录（`Path.GetTempPath()`），操作系统定期清理
- 用户手动清理无副作用（只是临时文件）

### 8.5 JSON-RPC vs 二进制协议

**权衡**：
- **JSON-RPC**：简单、跨语言、易于调试，但性能较低
- **二进制协议**（如 gRPC、MessagePack）：性能高，但复杂、调试困难

**选择**：JSON-RPC

**设计依据**：
- LSP 智能感知请求频率相对较低（用户键入触发），性能损失可接受（< 1ms）
- 与 Volar/LSP 协议一致（JSON-RPC）
- TypeScript/JavaScript 互操作简单（原生 JSON 支持）
- 易于调试（可读的 JSON 文本）
