# Jazor RPC 分析客户端（RPC Analysis Client）

通过进程间通信（IPC）与外部分析服务交互，负责序列化/反序列化 RPC 请求响应、处理传输层错误和超时、记录失败日志。

## 核心类型

### `RpcVueAnalysisClient`

**文件路径**：`src/Jolt/Analysis/RpcVueAnalysisClient.cs`

实现 `IVueAnalysisClient` 接口，通过 RPC 调用远程分析服务。

```csharp
public async ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
    AnalyzeJazorRequest request,
    CancellationToken cancellationToken)
```

依赖 `IAnalysisRpcTransport` 作为 RPC 传输层抽象。错误处理策略：传输失败记录日志后重新抛出；RPC 错误提取错误码和消息，记录日志后抛出 `InvalidOperationException`；空载荷和反序列化失败同理。

### `IAnalysisRpcTransport`

**文件路径**：`src/Jolt/Analysis/IAnalysisRpcTransport.cs`

```csharp
public interface IAnalysisRpcTransport
{
    ValueTask<RpcResponseEnvelope> SendAsync(
        RpcRequestEnvelope request,
        CancellationToken cancellationToken);
}
```

传输层抽象，实现类为 `ProcessAnalysisRpcTransport`（子进程传输）。

### `ProcessAnalysisRpcTransport`

**文件路径**：`src/Jolt/Analysis/ProcessAnalysisRpcTransport.cs`

通过子进程与外部分析服务通信。配置：命令路径、命令行参数、30 秒超时（`DefaultRpcTimeout`）。

```csharp
StartInfo = new ProcessStartInfo
{
    FileName = _command,
    Arguments = _arguments ?? string.Empty,
    UseShellExecute = false,
    RedirectStandardInput = true,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    CreateNoWindow = true
}
```

### `VueAnalysisClientFactory`

**文件路径**：`src/Jolt/Analysis/VueAnalysisClientFactory.cs`

根据 CLI 参数创建分析客户端实例：

```csharp
public static IVueAnalysisClient Create(string[] args)
public static IVueAnalysisClient CreateDefault()
public static IVueAnalysisClient CreateFromTransport(IAnalysisRpcTransport transport)
public static IVueAnalysisClient Create(IAnalysisRpcTransport? transport = null)
```

## 核心算法

### RPC 调用流程

**实现**：`RpcVueAnalysisClient.AnalyzeJazorAsync()`

1. **参数验证**：
   ```csharp
   ArgumentNullException.ThrowIfNull(request);
   cancellationToken.ThrowIfCancellationRequested();
   ```

2. **构建 RPC 请求**：
   ```csharp
   var rpcRequest = new RpcRequestEnvelope(
       id: Guid.NewGuid().ToString("N"),
       method: SharedVueAnalysisRpcMethodNames.AnalyzeJazor,
       payloadJson: JoltRpcSerializer.Serialize(request));
   ```

3. **发送请求**：
   ```csharp
   rpcResponse = await _transport.SendAsync(rpcRequest, cancellationToken);
   ```

4. **处理传输失败**：
   ```csharp
   catch (Exception exception)
   {
       WriteFailureLog(rpcRequest.Id, rpcRequest.Method, "transport_failure", exception.Message, exception.GetType().FullName);
       throw;
   }
   ```

5. **检查 RPC 错误**：
   ```csharp
   if (!rpcResponse.Success)
   {
       var errorCode = rpcResponse.Error?.Code;
       var errorMessage = rpcResponse.Error?.Message ?? "VueAnalysis RPC call failed without an error payload.";
       WriteFailureLog(...);
       throw new InvalidOperationException(...);
   }
   ```

6. **验证载荷**：
   ```csharp
   if (string.IsNullOrWhiteSpace(rpcResponse.PayloadJson))
   {
       WriteFailureLog(...);
       throw new InvalidOperationException("VueAnalysis RPC call returned an empty payload.");
   }
   ```

7. **反序列化响应**：
   ```csharp
   response = JoltRpcSerializer.Deserialize<AnalyzeJazorResponse>(rpcResponse.PayloadJson);
   ```

8. **验证响应**：
   ```csharp
   if (response is null)
   {
       WriteFailureLog(...);
       throw new InvalidOperationException("VueAnalysis RPC response could not be deserialized.");
   }
   ```

### 子进程通信

**实现**：`ProcessAnalysisRpcTransport.SendAsync()`

1. **启动进程**：
   ```csharp
   using var process = new Process { StartInfo = ... };
   processStarted = process.Start();
   if (!processStarted)
   {
       throw new InvalidOperationException($"Failed to start analysis process '{_command}'.");
   }
   ```

2. **异步捕获 stderr**：
   ```csharp
   errorDrainTask = DrainErrorOutputAsync(process.StandardError, CancellationToken.None);
   ```

3. **发送请求**：
   ```csharp
   var requestJson = JoltRpcSerializer.Serialize(request);
   await process.StandardInput.WriteLineAsync(requestJson.AsMemory(), effectiveCancellationToken);
   await process.StandardInput.FlushAsync(effectiveCancellationToken);
   process.StandardInput.Close();
   ```

4. **读取响应**：
   ```csharp
   var responseJson = await ReadResponseJsonAsync(process.StandardOutput, effectiveCancellationToken);
   ```

5. **等待进程退出**：
   ```csharp
   await ChildProcessUtilities.WaitForExitOrTerminateOnCancellationAsync(process, effectiveCancellationToken);
   ```

6. **获取错误输出**：
   ```csharp
   var errorOutput = await AwaitCapturedOutputAsync(errorDrainTask);
   ```

7. **验证响应**：
   ```csharp
   if (string.IsNullOrWhiteSpace(responseJson))
   {
       throw CreateProcessFailure("Analysis process did not return a response.", errorOutput);
   }
   ```

8. **反序列化响应**：
   ```csharp
   return JoltRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
   ```

### 响应 JSON 读取

**实现**：`ReadResponseJsonAsync()`

读取策略：跳过非 JSON 行（最多 `MaxResponseProbeLines` 行），查找以 `{` 开头的行（JSON 对象），捕获跳过的行用于错误报告。

```
stderr: Some debug message
stdout: Another debug message
stdout: {"id":"...","success":true,"payloadJson":"{...}"}
```

限制常量：
```csharp
private const int MaxResponseProbeLines = 1000;
private const int MaxCapturedOutputLines = 200;
```

### 错误输出捕获

**实现**：`DrainErrorOutputAsync()`

逐行读取 stderr，限制捕获字符数（`MaxCapturedErrorChars = 16 * 1024`），保留最近的错误信息。

### 超时处理

使用 `CancellationTokenSource` 链接：

```csharp
using var timeoutSource = new CancellationTokenSource(DefaultRpcTimeout);
using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
    cancellationToken,
    timeoutSource.Token);
var effectiveCancellationToken = linkedSource.Token;
```

默认 30 秒超时。超时时终止子进程并抛出 `TimeoutException`：

```csharp
catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
{
    if (processStarted)
    {
        await ChildProcessUtilities.TerminateProcessAsync(process);
    }

    var errorOutput = await AwaitCapturedOutputAsync(errorDrainTask);
    throw new TimeoutException(
        BuildProcessFailureMessage(
            $"Analysis process '{_command}' timed out after {DefaultRpcTimeout.TotalSeconds:F0}s.",
            errorOutput));
}
```

### 失败日志记录

**实现**：`WriteFailureLog()`

```json
{
  "eventType": "analysisRpcFailure",
  "timestamp": "2026-04-21T12:34:56.789Z",
  "requestId": "guid",
  "method": "AnalyzeJazor",
  "errorCode": "transport_failure",
  "errorMessage": "Connection refused",
  "details": "System.IO.IOException: Connection refused"
}
```

输出到 `Console.Error`（stderr）。错误码类型：`transport_failure`（传输层异常）、`rpc_failure`（RPC 服务返回错误）、`empty_payload`（空载荷）、`invalid_payload`（反序列化失败）。

## 线程安全模型

`RpcVueAnalysisClient` 是 sealed class，每个实例持有独立的 `_transport` 实例，方法调用不共享可变状态。

`ProcessAnalysisRpcTransport` 每次调用创建新的子进程，无共享进程状态，超时和取消令牌独立。

多个线程可以同时调用 `AnalyzeJazorAsync()`，每次调用独立执行，进程隔离确保资源隔离。

## 错误处理

参数验证：
```csharp
ArgumentNullException.ThrowIfNull(request);
cancellationToken.ThrowIfCancellationRequested();
```

传输失败：记录日志后重新抛出原始异常。

RPC 错误：
```csharp
if (!rpcResponse.Success)
{
    var errorCode = rpcResponse.Error?.Code;
    var errorMessage = rpcResponse.Error?.Message ?? "VueAnalysis RPC call failed without an error payload.";
    WriteFailureLog(...);
    throw new InvalidOperationException(
        string.IsNullOrWhiteSpace(errorCode)
            ? errorMessage
            : errorCode + ": " + errorMessage);
}
```

超时处理：终止子进程后抛出 `TimeoutException`。

进程启动失败：`throw new InvalidOperationException($"Failed to start analysis process '{_command}'.");`

响应验证失败：`throw CreateProcessFailure("Analysis process did not return a response.", errorOutput);`

## 配置选项

CLI 参数：
```
--analysis-command=/path/to/analysis-service
--analysis-args=--port=1234 --verbose
```

超时配置：默认 30 秒，硬编码在 `ProcessAnalysisRpcTransport` 中。

输出捕获限制：
```csharp
private const int MaxResponseProbeLines = 1000;
private const int MaxCapturedErrorChars = 16 * 1024;
private const int MaxCapturedOutputLines = 200;
```

工厂方法配置：

```csharp
// 默认客户端（进程内）
VueAnalysisClientFactory.CreateDefault()
// 返回：new JazorVueAnalysisService()

// RPC 客户端
VueAnalysisClientFactory.Create(new ProcessAnalysisRpcTransport("node", "server.js"))
// 返回：new RpcVueAnalysisClient(new ProcessAnalysisRpcTransport("node", "server.js"))

// 自动选择
VueAnalysisClientFactory.Create(args)
// 如果提供了 --analysis-command，返回 RPC 客户端；否则返回默认客户端
```

## 与其他子系统的交互

### 与分析服务交互

数据流：
```
AnalyzeJazorRequest
    ↓
RpcVueAnalysisClient.AnalyzeJazorAsync()
    ↓
RpcRequestEnvelope (序列化)
    ↓
ProcessAnalysisRpcTransport.SendAsync()
    ↓
子进程 stdin/stdout
    ↓
外部分析服务（IVueAnalysisRpcService）
    ↓
RpcResponseEnvelope
    ↓
AnalyzeJazorResponse
```

### 与 RPC 序列化器交互

依赖 `JoltRpcSerializer` 进行 JSON 序列化/反序列化。

### 与子进程工具交互

依赖 `ChildProcessUtilities`：
- `WaitForExitOrTerminateOnCancellationAsync()`：等待进程退出或取消时终止
- `TerminateProcessAsync()`：强制终止进程

确保子进程不会成为僵尸进程，取消时快速清理资源。

### 与遥测系统交互

失败日志通过 `WriteFailureLog()` 输出到 `Console.Error`，用于调试 RPC 通信问题、监控分析服务健康状态、识别配置错误。

### 与 LSP 服务交互

消费者：`LspSession`、`JoltWorkspaceResolver`，通过 RPC 分析服务获取诊断、导入符号、编译产物。

选择逻辑：
```csharp
// 在 LSP 会话初始化时
var analysisClient = VueAnalysisClientFactory.Create(args);

// 在处理 LSP 请求时
var response = await analysisClient.AnalyzeJazorAsync(request, cancellationToken);
```

## 设计权衡

### 进程隔离 vs 进程内共享

默认进程内（`JazorVueAnalysisService`），支持进程外（`RpcVueAnalysisClient`）。进程内快速、低开销、调试简单，但共享内存、崩溃影响主进程。进程外隔离、独立资源管理、支持多语言，但有 IPC 开销和启动延迟。开发环境用进程内快速迭代，生产环境用进程外保证稳定性。

### 同步 IPC vs 异步 IPC

使用异步 IPC（`async/await`）。LSP 和 DevServer 需要高并发，取消支持是关键需求，.NET 异步生态系统成熟。

### 固定超时 vs 可配置超时

使用固定的 30 秒超时。对大多数场景足够，避免配置过度设计，后期可扩展为可配置。

### 失败快速 vs 失败静默

所有失败都抛出异常（失败快速）。分析是关键功能，失败应可见，日志记录提供调试信息，调用方可以选择处理异常。

### stderr 捕获 vs stderr 忽略

捕获并限制 stderr 输出。子进程错误是常见问题，限制缓冲区大小控制开销，错误信息对调试至关重要。

## 附录：RPC 协议

请求信封：
```json
{
  "id": "guid",
  "method": "AnalyzeJazor",
  "payloadJson": "{... serialized AnalyzeJazorRequest ...}"
}
```

响应信封（成功）：
```json
{
  "id": "guid",
  "success": true,
  "payloadJson": "{... serialized AnalyzeJazorResponse ...}"
}
```

响应信封（失败）：
```json
{
  "id": "guid",
  "success": false,
  "error": {
    "code": "ErrorCode",
    "message": "Error message",
    "details": "Additional details"
  }
}
```

方法名称：
```csharp
public static class VueAnalysisRpcMethodNames
{
    public const string AnalyzeJazor = "AnalyzeJazor";
}
```

通信模式：JSON over stdin/stdout，单请求单响应（无持久连接），每次调用创建新进程（无进程池）。
