# Jazor RPC 分析客户端（RPC Analysis Client）

> 状态：已实现
> 定位：Jolt 分析层的 RPC 客户端，通过子进程与外部分析服务通信

## 1. 文档定位

本文档描述 Jazor RPC 分析客户端，该客户端负责：
1. 通过进程间通信（IPC）与外部分析服务交互
2. 序列化和反序列化 RPC 请求/响应
3. 处理传输层错误和超时
4. 记录失败日志用于调试

## 2. 核心类型

### 2.1 `RpcVueAnalysisClient`

**文件路径**：`src/Jolt/Analysis/RpcVueAnalysisClient.cs`

**职责**：实现 `IVueAnalysisClient` 接口，通过 RPC 调用远程分析服务

**核心方法**：
```csharp
public async ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
    AnalyzeJazorRequest request,
    CancellationToken cancellationToken)
```

**依赖**：
- `IAnalysisRpcTransport`：RPC 传输层抽象

**错误处理**：
- 传输失败：记录失败日志并重新抛出异常
- RPC 错误：提取错误码和消息，记录日志并抛出 `InvalidOperationException`
- 空载荷：记录日志并抛出 `InvalidOperationException`
- 反序列化失败：记录日志并重新抛出异常

### 2.2 `IAnalysisRpcTransport`

**文件路径**：`src/Jolt/Analysis/IAnalysisRpcTransport.cs`

**接口定义**：
```csharp
public interface IAnalysisRpcTransport
{
    ValueTask<RpcResponseEnvelope> SendAsync(
        RpcRequestEnvelope request,
        CancellationToken cancellationToken);
}
```

**职责**：定义 RPC 传输层抽象

**实现类**：
- `ProcessAnalysisRpcTransport`：子进程传输实现

### 2.3 `ProcessAnalysisRpcTransport`

**文件路径**：`src/Jolt/Analysis/ProcessAnalysisRpcTransport.cs`

**职责**：通过子进程与外部分析服务通信

**核心方法**：
```csharp
public async ValueTask<RpcResponseEnvelope> SendAsync(
    RpcRequestEnvelope request,
    CancellationToken cancellationToken)
```

**配置**：
- **命令**：分析服务的可执行文件路径
- **参数**：传递给分析服务的命令行参数
- **超时**：30 秒（`DefaultRpcTimeout`）

**进程配置**：
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

### 2.4 `VueAnalysisClientFactory`

**文件路径**：`src/Jolt/Analysis/VueAnalysisClientFactory.cs`

**职责**：根据 CLI 参数创建分析客户端实例

**核心方法**：
```csharp
public static IVueAnalysisClient Create(string[] args)
public static IVueAnalysisClient CreateDefault()
public static IVueAnalysisClient CreateFromTransport(IAnalysisRpcTransport transport)
public static IVueAnalysisClient Create(IAnalysisRpcTransport? transport = null)
```

## 3. 核心算法

### 3.1 RPC 调用流程

**实现**：`RpcVueAnalysisClient.AnalyzeJazorAsync()`

**调用步骤**：

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

9. **返回响应**：
   ```csharp
   return response;
   ```

### 3.2 子进程通信

**实现**：`ProcessAnalysisRpcTransport.SendAsync()`

**通信步骤**：

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

### 3.3 响应 JSON 读取

**实现**：`ReadResponseJsonAsync()`

**读取策略**：
- 跳过非 JSON 行（最多 `MaxResponseProbeLines` 行）
- 查找以 `{` 开头的行（JSON 对象）
- 捕获跳过的行用于错误报告

**示例**：
```
stderr: Some debug message
stdout: Another debug message
stdout: {"id":"...","success":true,"payloadJson":"{...}"}
```

**限制**：
```csharp
private const int MaxResponseProbeLines = 1000;
private const int MaxCapturedOutputLines = 200;
```

### 3.4 错误输出捕获

**实现**：`DrainErrorOutputAsync()`

**捕获策略**：
- 逐行读取 `stderr`
- 限制捕获的字符数（`MaxCapturedErrorChars`）
- 保留最近的错误信息

**限制**：
```csharp
private const int MaxCapturedErrorChars = 16 * 1024;
```

### 3.5 超时处理

**实现**：使用 `CancellationTokenSource` 链接

```csharp
using var timeoutSource = new CancellationTokenSource(DefaultRpcTimeout);
using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
    cancellationToken,
    timeoutSource.Token);
var effectiveCancellationToken = linkedSource.Token;
```

**超时值**：
```csharp
private static readonly TimeSpan DefaultRpcTimeout = TimeSpan.FromSeconds(30);
```

**超时处理**：
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

### 3.6 失败日志记录

**实现**：`WriteFailureLog()`

**日志格式**：
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

**输出位置**：`Console.Error`（stderr）

**错误码类型**：
- `transport_failure`：传输层异常
- `rpc_failure`：RPC 服务返回错误
- `empty_payload`：空载荷
- `invalid_payload`：反序列化失败

## 4. 线程安全模型

**实例级别线程安全**：
- `RpcVueAnalysisClient` 是 sealed class
- 每个实例持有独立的 `_transport` 实例
- 方法调用不共享可变状态

**ProcessAnalysisRpcTransport 线程安全**：
- 每次调用创建新的子进程
- 无共享进程状态
- 超时和取消令牌独立

**线程安全保证**：
- 多个线程可以同时调用 `AnalyzeJazorAsync()`
- 每次调用独立执行，无竞态条件
- 进程隔离确保资源隔离

## 5. 错误处理

### 5.1 参数验证

```csharp
ArgumentNullException.ThrowIfNull(request);
cancellationToken.ThrowIfCancellationRequested();
```

### 5.2 传输失败处理

```csharp
catch (Exception exception)
{
    WriteFailureLog(rpcRequest.Id, rpcRequest.Method, "transport_failure", exception.Message, exception.GetType().FullName);
    throw;  // 重新抛出原始异常
}
```

**设计原则**：记录日志后重新抛出原始异常

### 5.3 RPC 错误处理

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

**错误信息格式**：`{ErrorCode}: {ErrorMessage}`

### 5.4 超时处理

```csharp
catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
{
    // 终止子进程
    if (processStarted)
    {
        await ChildProcessUtilities.TerminateProcessAsync(process);
    }

    // 抛出 TimeoutException
    throw new TimeoutException(
        BuildProcessFailureMessage(
            $"Analysis process '{_command}' timed out after {DefaultRpcTimeout.TotalSeconds:F0}s.",
            errorOutput));
}
```

### 5.5 进程启动失败

```csharp
processStarted = process.Start();
if (!processStarted)
{
    throw new InvalidOperationException($"Failed to start analysis process '{_command}'.");
}
```

### 5.6 响应验证失败

```csharp
if (string.IsNullOrWhiteSpace(responseJson))
{
    throw CreateProcessFailure(
        "Analysis process did not return a response.",
        errorOutput);
}
```

## 6. 配置选项

### 6.1 CLI 参数

**分析命令**：
```
--analysis-command=/path/to/analysis-service
```

**分析参数**：
```
--analysis-args=--port=1234 --verbose
```

### 6.2 超时配置

**默认超时**：30 秒

```csharp
private static readonly TimeSpan DefaultRpcTimeout = TimeSpan.FromSeconds(30);
```

**不可配置**：硬编码在 `ProcessAnalysisRpcTransport` 中

### 6.3 输出捕获限制

```csharp
private const int MaxResponseProbeLines = 1000;        // 最多探测的行数
private const int MaxCapturedErrorChars = 16 * 1024;  // 最多捕获的错误字符数
private const int MaxCapturedOutputLines = 200;        // 最多捕获的输出行数
```

### 6.4 工厂方法配置

**默认客户端**：
```csharp
VueAnalysisClientFactory.CreateDefault()
// 返回：new JazorVueAnalysisService()
```

**RPC 客户端**：
```csharp
VueAnalysisClientFactory.Create(new ProcessAnalysisRpcTransport("node", "server.js"))
// 返回：new RpcVueAnalysisClient(new ProcessAnalysisRpcTransport("node", "server.js"))
```

**自动选择**：
```csharp
VueAnalysisClientFactory.Create(args)
// 如果提供了 --analysis-command，返回 RPC 客户端
// 否则返回默认客户端
```

## 7. 与其他子系统的交互

### 7.1 与分析服务交互

**调用方**：`VueAnalysisClientFactory`

**实现选择**：
- 进程内：`JazorVueAnalysisService`
- 进程外：`RpcVueAnalysisClient` + `ProcessAnalysisRpcTransport`

**数据流**：
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

### 7.2 与 RPC 序列化器交互

**依赖**：`JoltRpcSerializer`

**序列化**：
```csharp
var requestJson = JoltRpcSerializer.Serialize(request);
```

**反序列化**：
```csharp
return JoltRpcSerializer.Deserialize<RpcResponseEnvelope>(responseJson);
response = JoltRpcSerializer.Deserialize<AnalyzeJazorResponse>(rpcResponse.PayloadJson);
```

**格式**：JSON（由 `JoltRpcSerializer` 实现）

### 7.3 与子进程工具交互

**依赖**：`ChildProcessUtilities`

**方法**：
- `WaitForExitOrTerminateOnCancellationAsync()`：等待进程退出或取消时终止
- `TerminateProcessAsync()`：强制终止进程

**用途**：
- 确保子进程不会成为僵尸进程
- 取消时快速清理资源

### 7.4 与遥测系统交互

**失败日志**：`WriteFailureLog()`

**输出位置**：`Console.Error`

**用途**：
- 调试 RPC 通信问题
- 监控分析服务健康状态
- 识别配置错误

### 7.5 与 LSP 服务交互

**消费者**：`LspSession`、`JoltWorkspaceResolver`

**用途**：
- 通过 RPC 分析服务获取诊断
- 获取导入符号
- 获取编译产物

**选择逻辑**：
```csharp
// 在 LSP 会话初始化时
var analysisClient = VueAnalysisClientFactory.Create(args);

// 在处理 LSP 请求时
var response = await analysisClient.AnalyzeJazorAsync(request, cancellationToken);
```

## 8. 设计权衡

### 8.1 进程隔离 vs 进程内共享

**设计决策**：默认进程内（`JazorVueAnalysisService`），支持进程外（`RpcVueAnalysisClient`）

**权衡**：
- **进程内**：
  - 优点：快速、低开销、调试简单
  - 缺点：共享内存、崩溃影响主进程

- **进程外**：
  - 优点：隔离、独立资源管理、支持多语言
  - 缺点：IPC 开销、启动延迟

**选择理由**：
- 开发环境使用进程内（快速迭代）
- 生产环境使用进程外（稳定性）

### 8.2 同步 IPC vs 异步 IPC

**设计决策**：使用异步 IPC（`async/await`）

**权衡**：
- **异步**：
  - 优点：不阻塞线程、支持取消、可扩展
  - 缺点：复杂性、状态管理

- **同步**：
  - 优点：简单、直接
  - 缺点：阻塞线程、无法取消

**选择理由**：
- LSP 和 DevServer 需要高并发
- 取消支持是关键需求（用户操作、超时）
- .NET 异步生态系统成熟

### 8.3 固定超时 vs 可配置超时

**设计决策**：使用固定的 30 秒超时

**权衡**：
- **固定超时**：
  - 优点：简单、可预测
  - 缺点：不适应不同场景

- **可配置超时**：
  - 优点：灵活性
  - 缺点：配置复杂性、误用风险

**选择理由**：
- 30 秒对大多数场景足够
- 避免配置过度设计
- 后期可扩展为可配置

### 8.4 失败快速 vs 失败静默

**设计决策**：所有失败都抛出异常（失败快速）

**权衡**：
- **失败快速**：
  - 优点：问题可见、强制处理
  - 缺点：可能中断流程

- **失败静默**：
  - 优点：容错性、降级
  - 缺点：隐藏问题、难以调试

**选择理由**：
- 分析是关键功能，失败应可见
- 日志记录提供调试信息
- 调用方可以选择处理异常

### 8.5 stderr 捕获 vs stderr 忽略

**设计决策**：捕获并限制 stderr 输出

**权衡**：
- **捕获**：
  - 优点：调试信息、错误上下文
  - 缺点：内存开销、缓冲区管理

- **忽略**：
  - 优点：简单、无开销
  - 缺点：丢失调试信息

**选择理由**：
- 子进程错误是常见问题
- 限制缓冲区大小控制开销
- 错误信息对调试至关重要

## 9. 附录：RPC 协议

### 9.1 请求信封

```json
{
  "id": "guid",
  "method": "AnalyzeJazor",
  "payloadJson": "{... serialized AnalyzeJazorRequest ...}"
}
```

### 9.2 响应信封（成功）

```json
{
  "id": "guid",
  "success": true,
  "payloadJson": "{... serialized AnalyzeJazorResponse ...}"
}
```

### 9.3 响应信封（失败）

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

### 9.4 方法名称

```csharp
public static class VueAnalysisRpcMethodNames
{
    public const string AnalyzeJazor = "AnalyzeJazor";
}
```

### 9.5 通信模式

**协议**：JSON over stdin/stdout

**流程**：
1. 客户端启动子进程
2. 客户端写入 JSON 请求到 stdin
3. 子进程读取 stdin，处理请求
4. 子进程写入 JSON 响应到 stdout
5. 客户端读取 stdout，解析响应
6. 子进程退出（或等待下一个请求）

**限制**：
- 单请求单响应（无持久连接）
- 每次调用创建新进程（无进程池）

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
