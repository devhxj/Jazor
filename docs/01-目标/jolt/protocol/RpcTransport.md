# Jolt RPC 传输层


## 1. 文档定位

Jolt 的 RPC 传输层实现，这是编译器与前端分析器之间通信的核心基础设施。RPC 传输层定义了统一的通信协议，支持多种传输方式（当前实现 stdio，未来可扩展 HTTP/WebSocket）。

**核心职责**：
- 方法路由：将 RPC 方法名映射到具体的服务方法
- 序列化/反序列化：在强类型对象和 JSON 之间转换
- 错误处理：统一异常映射到 RPC 错误响应
- 传输抽象：支持底层传输方式替换

## 2. 核心类型

### 2.1 IJoltRpcProcessor (`IJoltRpcProcessor.cs`)

**职责**：RPC 处理器接口，定义处理单行 RPC 请求的抽象。

**接口定义**：
```csharp
public interface IJoltRpcProcessor
{
    Task<string> ProcessAsync(
        string requestLine,
        CancellationToken cancellationToken);
}
```

**代码位置**：`src/Jolt/Rpc/IJoltRpcProcessor.cs`

**设计决策**：
- 单行输入/输出：简化 stdio 传输协议
- 异步设计：支持 I/O 密集型操作
- 取消令牌：支持请求取消和超时

### 2.2 IJoltRpcDispatcher (`IJoltRpcDispatcher.cs`)

**职责**：RPC 调度器接口，定义方法名到服务方法的映射。

**接口定义**：
```csharp
public interface IJoltRpcDispatcher
{
    Task<object?> DispatchAsync(
        string methodName,
        object? payload,
        CancellationToken cancellationToken);
}
```

**代码位置**：`src/Jolt/Rpc/IJoltRpcDispatcher.cs`

**设计决策**：
- 返回 `object?`：支持无返回值方法（`null`）和不同类型响应
- `payload` 为 `object?`：支持无参数方法（`null`）
- 方法名路由：通过 `methodName` 字符串分发

### 2.3 IJoltRpcService (`IJoltRpcService.cs`)

**职责**：RPC 服务接口，定义所有 RPC 方法的业务逻辑契约。

**接口定义**：
```csharp
public interface IJoltRpcService
{
    // 健康检查和元信息
    Task<PingResponse> PingAsync(CancellationToken cancellationToken);
    Task<GetHostInfoResponse> GetHostInfoAsync(CancellationToken cancellationToken);

    // 文档生命周期管理
    Task OpenDocumentAsync(DocumentSnapshot documentSnapshot, CancellationToken cancellationToken);
    Task UpdateDocumentAsync(DocumentSnapshot documentSnapshot, CancellationToken cancellationToken);
    Task CloseDocumentAsync(string documentPath, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentSnapshot>> GetOpenDocumentsAsync(CancellationToken cancellationToken);

    // 分析和编译
    Task<GetFrontendContextResponse> GetFrontendContextAsync(
        GetFrontendContextRequest request,
        CancellationToken cancellationToken);
    Task<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken);
    Task<GetVirtualArtifactResponse> GetVirtualArtifactAsync(
        GetVirtualArtifactRequest request,
        CancellationToken cancellationToken);
    Task<GetHotUpdatePlanResponse> GetHotUpdatePlanAsync(
        GetHotUpdatePlanRequest request,
        CancellationToken cancellationToken);
}
```

**代码位置**：`src/Jolt/Rpc/IJoltRpcService.cs`

**设计决策**：
- 所有方法返回 `Task<T>`：异步操作
- 统一 `CancellationToken`：支持取消和超时
- 强类型请求/响应：编译时类型安全

### 2.4 JoltRpcProcessor (`JoltRpcProcessor.cs`)

**职责**：RPC 处理器实现，负责反序列化、方法路由、错误映射和序列化。

**核心方法**：
```csharp
public sealed class JoltRpcProcessor : IJoltRpcProcessor
{
    private readonly IJoltRpcDispatcher _dispatcher;

    public async Task<string> ProcessAsync(
        string requestLine,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestLine);

        RpcRequestEnvelope? request = null;
        RpcResponseEnvelope response;
        try
        {
            // 1. 反序列化请求信封
            request = JoltRpcSerializer.Deserialize<RpcRequestEnvelope>(requestLine)
                ?? throw new JoltRpcException("invalid_request", "RPC request payload could not be deserialized.");

            // 2. 反序列化载荷（方法特定）
            var payload = DeserializePayload(request.Method, request.PayloadJson);

            // 3. 调度到具体服务方法
            var result = await _dispatcher.DispatchAsync(request.Method, payload, cancellationToken);

            // 4. 构造成功响应
            response = new RpcResponseEnvelope(
                id: request.Id,
                success: true,
                payloadJson: result is null ? null : JoltRpcSerializer.Serialize(result),
                error: null);
        }
        catch (OperationCanceledException exception)
        {
            response = CreateErrorResponse(request?.Id, "cancelled", exception);
        }
        catch (JoltRpcException exception)
        {
            response = CreateErrorResponse(request?.Id, exception.Code, exception);
        }
        catch (Exception exception)
        {
            response = CreateErrorResponse(request?.Id, "internal_error", exception);
        }

        return JoltRpcSerializer.Serialize(response);
    }
}
```

**载荷反序列化**：
```csharp
private static object? DeserializePayload(string methodName, string? payloadJson)
{
    return methodName switch
    {
        SharedJoltRpcMethodNames.Ping => null,
        SharedJoltRpcMethodNames.GetHostInfo => null,
        SharedJoltRpcMethodNames.OpenDocument => DeserializeRequired<DocumentSnapshot>(payloadJson),
        SharedJoltRpcMethodNames.UpdateDocument => DeserializeRequired<DocumentSnapshot>(payloadJson),
        SharedJoltRpcMethodNames.CloseDocument => DeserializeRequired<string>(payloadJson),
        SharedJoltRpcMethodNames.GetOpenDocuments => null,
        SharedJoltRpcMethodNames.GetFrontendContext => DeserializeRequired<GetFrontendContextRequest>(payloadJson),
        SharedJoltRpcMethodNames.AnalyzeJazor => DeserializeRequired<AnalyzeJazorRequest>(payloadJson),
        SharedJoltRpcMethodNames.GetVirtualArtifact => DeserializeRequired<GetVirtualArtifactRequest>(payloadJson),
        SharedJoltRpcMethodNames.GetHotUpdatePlan => DeserializeRequired<GetHotUpdatePlanRequest>(payloadJson),
        _ => throw new JoltRpcException("unknown_method", $"Unknown Jolt RPC method '{methodName}'.")
    };
}
```

**错误处理**：
```csharp
private static RpcResponseEnvelope CreateErrorResponse(
    string? requestId,
    string errorCode,
    Exception exception)
{
    return new RpcResponseEnvelope(
        id: requestId,
        success: false,
        payloadJson: null,
        error: new RpcErrorRecord(
            code: errorCode,
            message: exception.Message,
            details: exception.GetType().FullName));
}
```

**代码位置**：`src/Jolt/Rpc/JoltRpcProcessor.cs`

**设计决策**：
- **三层异常处理**：
  1. `OperationCanceledException` → `"cancelled"` 错误码
  2. `JoltRpcException` → 保持自定义错误码
  3. 其他异常 → `"internal_error"` 错误码
- **方法名路由**：使用 `switch` 表达式映射方法名到载荷类型
- **空值处理**：无载荷方法传入 `null`

### 2.5 JoltRpcDispatcher (`JoltRpcDispatcher.cs`)

**职责**：RPC 调度器实现，将方法名路由到 `IJoltRpcService` 的具体方法。

**核心方法**：
```csharp
public sealed class JoltRpcDispatcher : IJoltRpcDispatcher
{
    private readonly IJoltRpcService _rpcService;

    public async Task<object?> DispatchAsync(
        string methodName,
        object? payload,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        return methodName switch
        {
            SharedJoltRpcMethodNames.Ping => await _rpcService.PingAsync(cancellationToken),
            SharedJoltRpcMethodNames.GetHostInfo => await _rpcService.GetHostInfoAsync(cancellationToken),
            SharedJoltRpcMethodNames.OpenDocument => await DispatchOpenDocumentAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.UpdateDocument => await DispatchUpdateDocumentAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.CloseDocument => await DispatchCloseDocumentAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.GetOpenDocuments => await _rpcService.GetOpenDocumentsAsync(cancellationToken),
            SharedJoltRpcMethodNames.GetFrontendContext => await DispatchGetFrontendContextAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.AnalyzeJazor => await DispatchAnalyzeJazorAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.GetVirtualArtifact => await DispatchGetVirtualArtifactAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.GetHotUpdatePlan => await DispatchGetHotUpdatePlanAsync(payload, cancellationToken),
            _ => throw new JoltRpcException("unknown_method", $"Unknown Jolt RPC method '{methodName}'.")
        };
    }
}
```

**无返回值方法包装**：
```csharp
private async Task<object?> DispatchOpenDocumentAsync(object? payload, CancellationToken cancellationToken)
{
    await _rpcService.OpenDocumentAsync(RequirePayload<DocumentSnapshot>(payload), cancellationToken);
    return null;  // 无返回值方法返回 null
}
```

**载荷类型验证**：
```csharp
private static T RequirePayload<T>(object? payload)
{
    if (payload is T typedPayload)
        return typedPayload;

    throw new JoltRpcException(
        "invalid_payload",
        $"Expected RPC payload of type '{typeof(T).FullName}', but received '{payload?.GetType().FullName ?? "<null>"}'.");
}
```

**代码位置**：`src/Jolt/Rpc/JoltRpcDispatcher.cs`

**设计决策**：
- **方法名常量**：使用 `SharedJoltRpcMethodNames` 避免字符串拼写错误
- **类型安全**：`RequirePayload<T>` 确保载荷类型正确
- **无返回值方法**：返回 `null` 而非 `Task<object?>`，统一类型系统

### 2.6 StdioJoltRpcServer (`StdioJoltRpcServer.cs`)

**职责**：基于标准输入输出的 RPC 服务器实现。

**核心方法**：
```csharp
public sealed class StdioJoltRpcServer
{
    private readonly IJoltRpcProcessor _rpcProcessor;

    public async Task RunAsync(
        TextReader input,
        TextWriter output,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // 1. 读取单行请求
                var requestLine = await input.ReadLineAsync(cancellationToken);
                if (requestLine is null)
                {
                    break;  // EOF
                }

                if (string.IsNullOrWhiteSpace(requestLine))
                {
                    continue;  // 跳过空行
                }

                // 2. 处理请求
                var responseLine = await _rpcProcessor.ProcessAsync(requestLine, cancellationToken);

                // 3. 写入响应
                await output.WriteLineAsync(responseLine.AsMemory(), cancellationToken);
                await output.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 正常取消，不处理
        }
    }
}
```

**代码位置**：`src/Jolt/Rpc/StdioJoltRpcServer.cs`

**设计决策**：
- **单行协议**：每行一个 JSON 请求，每行一个 JSON 响应
- **空行跳过**：简化日志和调试
- **取消友好**：`OperationCanceledException` 视为正常终止
- **立即刷新**：`FlushAsync` 确保响应及时发送

### 2.7 JoltRpcSerializer (`JoltRpcSerializer.cs`)

**职责**：RPC 序列化器，封装 `ProtocolJsonSerializer`。

**实现**：
```csharp
public static class JoltRpcSerializer
{
    public static JsonSerializerOptions DefaultOptions => ProtocolJsonSerializer.DefaultOptions;

    public static string Serialize<T>(T value)
        => ProtocolJsonSerializer.Serialize(value);

    public static T? Deserialize<T>(string json)
        => ProtocolJsonSerializer.Deserialize<T>(json);
}
```

**代码位置**：`src/Jolt/Rpc/JoltRpcSerializer.cs`

**设计决策**：
- **薄封装**：直接委托给 `ProtocolJsonSerializer`
- **命名隔离**：RPC 层使用 `JoltRpcSerializer`，契约层使用 `ProtocolJsonSerializer`
- **统一配置**：共享 `DefaultOptions`，确保序列化一致性

### 2.8 JoltRpcException (`JoltRpcException.cs`)

**职责**：RPC 特定异常，携带错误码。

**实现**：
```csharp
public sealed class JoltRpcException : Exception
{
    public JoltRpcException(string code, string message)
        : base(message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }

    public string Code { get; }
}
```

**代码位置**：`src/Jolt/Rpc/JoltRpcException.cs`

**设计决策**：
- **错误码机制**：支持程序化错误处理
- **继承 Exception**：兼容标准异常处理
- **sealed**：禁止继承，避免异常层次复杂化

## 3. 核心算法

### 3.1 RPC 请求处理流程

```
客户端请求 (JSON 字符串)
    ↓
StdioJoltRpcServer.RunAsync (读取行)
    ↓
JoltRpcProcessor.ProcessAsync
    ↓ 1. 反序列化 RpcRequestEnvelope
    ↓ 2. 反序列化载荷 (方法特定)
    ↓ 3. 调度方法
    ↓ 4. 序列化响应
    ↓
JoltRpcDispatcher.DispatchAsync
    ↓ 方法名路由
    ↓
IJoltRpcService (业务逻辑)
    ↓ 返回强类型响应
    ↓
JoltRpcProcessor.ProcessAsync
    ↓ 构造 RpcResponseEnvelope
    ↓
StdioJoltRpcServer.RunAsync (写入行)
    ↓
客户端响应 (JSON 字符串)
```

### 3.2 错误码映射策略

| 异常类型 | 错误码 | 示例场景 |
|---------|--------|---------|
| `OperationCanceledException` | `cancelled` | 客户端取消请求、超时 |
| `JoltRpcException("unknown_method")` | `unknown_method` | 未知 RPC 方法名 |
| `JoltRpcException("invalid_payload")` | `invalid_payload` | 载荷类型不匹配、反序列化失败 |
| `JoltRpcException("invalid_request")` | `invalid_request` | 请求信封格式错误 |
| 其他 `Exception` | `internal_error` | 未捕获的异常、业务逻辑错误 |

**设计原则**：
- 客户端错误（4xx）：`invalid_request`, `invalid_payload`, `unknown_method`
- 服务器错误（5xx）：`internal_error`
- 取消状态：`cancelled`

### 3.3 方法名路由算法

**步骤 1：JoltRpcProcessor 载荷反序列化**
```csharp
private static object? DeserializePayload(string methodName, string? payloadJson)
{
    return methodName switch
    {
        SharedJoltRpcMethodNames.Ping => null,  // 无载荷
        SharedJoltRpcMethodNames.OpenDocument => DeserializeRequired<DocumentSnapshot>(payloadJson),
        SharedJoltRpcMethodNames.AnalyzeJazor => DeserializeRequired<AnalyzeJazorRequest>(payloadJson),
        _ => throw new JoltRpcException("unknown_method", $"Unknown Jolt RPC method '{methodName}'.")
    };
}
```

**步骤 2：JoltRpcDispatcher 方法路由**
```csharp
public async Task<object?> DispatchAsync(string methodName, object? payload, CancellationToken cancellationToken)
{
    return methodName switch
    {
        SharedJoltRpcMethodNames.Ping => await _rpcService.PingAsync(cancellationToken),
        SharedJoltRpcMethodNames.OpenDocument => await DispatchOpenDocumentAsync(payload, cancellationToken),
        SharedJoltRpcMethodNames.AnalyzeJazor => await DispatchAnalyzeJazorAsync(payload, cancellationToken),
        _ => throw new JoltRpcException("unknown_method", $"Unknown Jolt RPC method '{methodName}'.")
    };
}
```

**双重路由原因**：
- **Processor 层**：确定载荷类型（反序列化需要）
- **Dispatcher 层**：确定服务方法（业务逻辑调用）

**优化方向**：可将两层合并为单层（牺牲类型安全）

## 4. 线程安全模型

### 4.1 单请求线程安全

**假设**：单个 `IJoltRpcService` 实例一次处理一个请求

**JoltRpcProcessor**：
- 无状态（`_dispatcher` 只读字段）
- `ProcessAsync` 方法无共享状态
- 天然线程安全

**JoltRpcDispatcher**：
- 无状态（`_rpcService` 只读字段）
- `DispatchAsync` 方法无共享状态
- 天然线程安全

### 4.2 多请求并发安全

**StdioJoltRpcServer**：
- 串行处理（`while` 循环，单线程）
- 同一时刻只有一个 `ProcessAsync` 调用

**并发扩展**：
```csharp
// 方案 A：有界队列（生产环境推荐）
public async Task RunAsyncConcurrent(
    TextReader input,
    TextWriter output,
    CancellationToken cancellationToken,
    int maxConcurrency = 10)
{
    var semaphore = new SemaphoreSlim(maxConcurrency);
    var tasks = new List<Task>();

    while (!cancellationToken.IsCancellationRequested)
    {
        var requestLine = await input.ReadLineAsync(cancellationToken);
        if (requestLine is null) break;

        await semaphore.WaitAsync(cancellationToken);
        tasks.Add(Task.Run(async () =>
        {
            try
            {
                var responseLine = await _rpcProcessor.ProcessAsync(requestLine, cancellationToken);
                lock (output)  // 同步写入
                {
                    output.WriteLine(responseLine);
                    output.Flush();
                }
            }
            finally
            {
                semaphore.Release();
            }
        }, cancellationToken));
    }

    await Task.WhenAll(tasks);
}
```

**方案 B：通道队列（高级）**
```csharp
public async Task RunAsyncWithChannel(
    TextReader input,
    TextWriter output,
    CancellationToken cancellationToken)
{
    var channel = System.Threading.Channels.Channel.CreateUnbounded<string>();
    var consumer = ConsumeAsync(channel, output, cancellationToken);

    while (!cancellationToken.IsCancellationRequested)
    {
        var requestLine = await input.ReadLineAsync(cancellationToken);
        if (requestLine is null) break;

        await channel.Writer.WriteAsync(requestLine, cancellationToken);
    }

    channel.Writer.Complete();
    await consumer;
}

private async Task ConsumeAsync(
    System.Threading.Channels.Channel<string> channel,
    TextWriter output,
    CancellationToken cancellationToken)
{
    await foreach (var requestLine in channel.Reader.ReadAllAsync(cancellationToken))
    {
        var responseLine = await _rpcProcessor.ProcessAsync(requestLine, cancellationToken);
        await output.WriteLineAsync(responseLine.AsMemory(), cancellationToken);
        await output.FlushAsync(cancellationToken);
    }
}
```

### 4.3 IJoltRpcService 线程安全

**假设**：`IJoltRpcService` 实现负责自身线程安全

**建议**：
- 无状态服务：线程安全
- 有状态服务（如文档缓存）：使用 `ConcurrentDictionary`、锁、不可变数据结构

## 5. 错误处理

### 5.1 异常层次结构

```
Exception
├── OperationCanceledException (特殊处理)
├── JoltRpcException (RPC 层异常)
│   ├── Code: "unknown_method"
│   ├── Code: "invalid_payload"
│   └── Code: "invalid_request"
└── 其他 Exception (业务逻辑异常)
    └── Code: "internal_error"
```

### 5.2 错误传播链

```
业务层异常 (Exception)
    ↓
JoltRpcDispatcher.DispatchAsync (向上传播)
    ↓
JoltRpcProcessor.ProcessAsync (捕获并映射)
    ↓
RpcResponseEnvelope (错误响应)
    ↓
StdioJoltRpcServer.RunAsync (写入错误响应)
    ↓
客户端接收错误
```

### 5.3 错误码标准

| 错误码 | HTTP 类比 | 语义 | 客户端处理 |
|--------|---------|------|----------|
| `invalid_request` | 400 Bad Request | 请求信封格式错误 | 不重试，修复请求 |
| `unknown_method` | 404 Not Found | 未知方法名 | 不重试，检查方法名 |
| `invalid_payload` | 422 Unprocessable Entity | 载荷类型错误 | 不重试，修复载荷 |
| `internal_error` | 500 Internal Server Error | 服务器内部错误 | 可重试（幂等方法） |
| `cancelled` | 499 Client Closed Request | 请求被取消 | 视场景重试或放弃 |

### 5.4 错误响应示例

**未知方法**：
```json
{
  "id": "req-123",
  "success": false,
  "payloadJson": null,
  "error": {
    "code": "unknown_method",
    "message": "Unknown Jolt RPC method 'jolt/unknownMethod'.",
    "details": "Jolt.Rpc.JoltRpcException"
  }
}
```

**载荷反序列化失败**：
```json
{
  "id": "req-456",
  "success": false,
  "payloadJson": null,
  "error": {
    "code": "invalid_payload",
    "message": "Expected RPC payload for 'Jazor.VueContracts.Protocol.DocumentSnapshot', but received <null>.",
    "details": "Jolt.Rpc.JoltRpcException"
  }
}
```

**业务逻辑异常**：
```json
{
  "id": "req-789",
  "success": false,
  "payloadJson": null,
  "error": {
    "code": "internal_error",
    "message": "Index was outside the bounds of the array.",
    "details": "System.IndexOutOfRangeException"
  }
}
```

## 6. 配置选项

### 6.1 序列化配置

通过 `ProtocolJsonSerializer.DefaultOptions` 全局配置：

```csharp
// 默认配置（Web 标准）
JsonSerializerDefaults.Web
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
}

// 可扩展配置
options.Converters.Add(new JsonStringEnumConverter());
```

### 6.2 传输层配置

**StdioJoltRpcServer** 当前无配置选项，硬编码行为：
- 跳过空行
- 无超时控制（依赖 `CancellationToken`）
- 无并发限制（串行处理）

**扩展方向**：
```csharp
public sealed class StdioJoltRpcServerOptions
{
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxConcurrency { get; set; } = 1;
    public bool SkipEmptyLines { get; set; } = true;
    public Encoding Encoding { get; set; } = Encoding.UTF8;
}
```

### 6.3 错误处理配置

**当前**：所有异常都转换为错误响应，无日志记录

**扩展方向**：
```csharp
public sealed class JoltRpcProcessorOptions
{
    public bool IncludeStackTraceInErrors { get; set; } = false;
    public ILogger? Logger { get; set; }
    public Func<Exception, string>? ErrorCodeMapper { get; set; }
}
```

## 7. 与其他子系统的交互

### 7.1 与契约层交互

**RPC 层 → 契约层**：
- 使用 `RpcRequestEnvelope`、`RpcResponseEnvelope`
- 使用 `JoltRpcSerializer` (封装 `ProtocolJsonSerializer`)

**契约层 → RPC 层**：
- 定义强类型请求/响应（如 `AnalyzeJazorRequest`）
- RPC 层根据方法名选择正确类型反序列化

### 7.2 与业务逻辑交互

**RPC 层 → 业务逻辑**：
- `IJoltRpcService` 接口定义业务方法
- `JoltRpcDispatcher` 调用服务方法

**业务逻辑 → RPC 层**：
- 返回强类型响应对象
- 抛出异常（RPC 层自动映射）

### 7.3 与传输层交互

**RPC 层 → 传输层**：
- `IJoltRpcProcessor.ProcessAsync` 处理单行请求
- `StdioJoltRpcServer` 实现 stdio 传输

**扩展传输**：
```csharp
// HTTP 传输
public sealed class HttpJoltRpcServer
{
    private readonly IJoltRpcProcessor _rpcProcessor;

    public async Task HandleAsync(HttpContext context)
    {
        var requestLine = await context.Request.Body.ReadAllTextAsync();
        var responseLine = await _rpcProcessor.ProcessAsync(requestLine, context.RequestAborted);
        await context.Response.WriteAsync(responseLine);
    }
}

// WebSocket 传输
public sealed class WebSocketJoltRpcServer
{
    private readonly IJoltRpcProcessor _rpcProcessor;

    public async Task HandleAsync(WebSocket webSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4];
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await webSocket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close) break;

            var requestLine = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var responseLine = await _rpcProcessor.ProcessAsync(requestLine, cancellationToken);
            var responseBytes = Encoding.UTF8.GetBytes(responseLine);
            await webSocket.SendAsync(responseBytes, WebSocketMessageType.Text, true, cancellationToken);
        }
    }
}
```

### 7.4 与 LSP 集成

**LSP 客户端 → Jolt RPC 服务器**：
- LSP 客户端启动 Jolt 子进程
- 通过 stdio 发送 RPC 请求
- 接收 RPC 响应并转换为 LSP 结果

**示例流程**：
```
VS Code (LSP Client)
    ↓ LSP request: textDocument/codeAction
    ↓
LspSession (Jolt LSP 服务器)
    ↓ 转换为 RPC 请求
    ↓
JoltRpcDispatcher (RPC 调度)
    ↓
IJoltRpcService (业务逻辑)
    ↓ 返回代码修复建议
    ↓
LspSession (转换为 LSP response)
    ↓
VS Code (显示代码操作)
```

## 8. 设计权衡

### 8.1 单行协议 vs 多行协议

**选择：单行协议**

**方案 A**：单行协议（当前实现）
```json
{"id":"1","method":"jolt/ping","payloadJson":null}
```

**方案 B**：多行协议（头部分离）
```
Content-Length: 57

{"id":"1","method":"jolt/ping","payloadJson":null}
```

**选择原因**：
- 简化实现：无需 `Content-Length` 解析
- 调试友好：可直接在终端测试
- 适合场景：Jolt 请求/响应较小（< 1MB）

**权衡**：
- 不支持二进制载荷（Jolt 不需要）
- 不支持流式传输（Jolt 不需要）

### 8.2 双层路由 vs 单层路由

**选择：双层路由（JoltRpcProcessor + JoltRpcDispatcher）**

**方案 A**：双层路由（当前实现）
```
JoltRpcProcessor.DeserializePayload (载荷类型)
    ↓
JoltRpcDispatcher.DispatchAsync (方法调用)
```

**方案 B**：单层路由
```csharp
public async Task<object?> DispatchAsync(string methodName, string? payloadJson)
{
    return methodName switch
    {
        "jolt/ping" => await _rpcService.PingAsync(),
        "jolt/openDocument" => await _rpcService.OpenDocumentAsync(
            Deserialize<DocumentSnapshot>(payloadJson)),
        // ...
    };
}
```

**选择原因**：
- **关注点分离**：Processor 处理协议，Dispatcher 处理路由
- **类型安全**：载荷反序列化失败时，错误信息更精确
- **可测试性**：Dispatcher 可独立测试（无需反序列化）

**权衡**：
- 代码量增加（两个类而非一个）
- 方法名常量重复（Processor 和 Dispatcher 都需要）

### 8.3 强类型载荷 vs 弱类型载荷

**选择：强类型载荷**

**方案 A**：强类型（当前实现）
```csharp
public sealed class AnalyzeJazorRequest
{
    public DocumentSnapshot JazorDocument { get; }
    public IReadOnlyList<DocumentSnapshot> RelatedDocuments { get; }
}
```

**方案 B**：弱类型（字典）
```csharp
public Dictionary<string, object> Payload { get; }
// 使用:
var jazorDoc = (DocumentSnapshot)Payload["jazorDocument"];
var relatedDocs = (IReadOnlyList<DocumentSnapshot>)Payload["relatedDocuments"];
```

**选择原因**：
- **类型安全**：编译时检查，减少运行时错误
- **IDE 支持**：自动完成、重构、导航
- **文档化**：类型定义即文档
- **性能**：避免字典查找和反射

**权衡**：
- 新增方法需要定义类型（代码量增加）
- 但类型复用度高（如 `DocumentSnapshot` 多处使用）

### 8.4 异常映射策略

**选择：三层异常映射**

**方案 A**：三层映射（当前实现）
```csharp
catch (OperationCanceledException ex) → "cancelled"
catch (JoltRpcException ex) → ex.Code
catch (Exception ex) → "internal_error"
```

**方案 B**：统一映射
```csharp
catch (Exception ex)
{
    var errorCode = ex is JoltRpcException joltEx ? joltEx.Code : "error";
    return CreateErrorResponse(requestId, errorCode, ex);
}
```

**选择原因**：
- **取消语义**：`OperationCanceledException` 表示用户主动取消，不是错误
- **错误码精确性**：`JoltRpcException` 携带业务错误码（如 `unknown_method`）
- **统一处理**：其他异常统一为 `internal_error`，避免信息泄露

**权衡**：
- 无法自定义业务异常错误码（需在业务层捕获并转为 `JoltRpcException`）

**扩展方向**：
```csharp
public static class JoltErrorCodes
{
    public const string CompilationFailed = "compilation_failed";
    public const string DocumentNotFound = "document_not_found";
    public const string InvalidVersion = "invalid_version";
}

// 业务层使用
throw new JoltRpcException(
    JoltErrorCodes.DocumentNotFound,
    $"Document '{path}' is not open.");
```

### 8.5 同步 vs 异步 API

**选择：异步 API**

**方案 A**：异步（当前实现）
```csharp
Task<AnalyzeJazorResponse> AnalyzeJazorAsync(
    AnalyzeJazorRequest request,
    CancellationToken cancellationToken);
```

**方案 B**：同步
```csharp
AnalyzeJazorResponse AnalyzeJazor(AnalyzeJazorRequest request);
```

**选择原因**：
- **I/O 密集**：RPC 调用涉及网络/进程通信
- **可取消**：`CancellationToken` 支持超时和取消
- **可扩展**：未来支持并发处理

**权衡**：
- 异步代码复杂度略高（但 `async/await` 已简化）

### 8.6 接口 vs 抽象类

**选择：接口（`IJoltRpcService`、`IJoltRpcProcessor`、`IJoltRpcDispatcher`）**

**方案 A**：接口（当前实现）
```csharp
public interface IJoltRpcService { ... }
```

**方案 B**：抽象类
```csharp
public abstract class JoltRpcServiceBase { ... }
```

**选择原因**：
- **解耦**：接口定义契约，实现可替换
- **多重继承**：类可实现多个接口
- **测试友好**：接口易于 Mock

**权衡**：
- 无法提供默认实现（C# 8+ 接口默认方法可缓解）

---

## 附录

### A. 完整类型清单

| 类型 | 文件 | 用途 |
|------|------|------|
| `IJoltRpcProcessor` | `IJoltRpcProcessor.cs` | RPC 处理器接口 |
| `IJoltRpcDispatcher` | `IJoltRpcDispatcher.cs` | RPC 调度器接口 |
| `IJoltRpcService` | `IJoltRpcService.cs` | RPC 服务接口 |
| `JoltRpcProcessor` | `JoltRpcProcessor.cs` | RPC 处理器实现 |
| `JoltRpcDispatcher` | `JoltRpcDispatcher.cs` | RPC 调度器实现 |
| `JoltRpcSerializer` | `JoltRpcSerializer.cs` | RPC 序列化器 |
| `JoltRpcException` | `JoltRpcException.cs` | RPC 异常 |
| `StdioJoltRpcServer` | `StdioJoltRpcServer.cs` | stdio 传输服务器 |

### B. RPC 方法清单

| 方法名 | 请求类型 | 响应类型 | 无载荷 |
|--------|---------|---------|--------|
| `jolt/ping` | - | `PingResponse` | ✓ |
| `jolt/getHostInfo` | - | `GetHostInfoResponse` | ✓ |
| `jolt/openDocument` | `DocumentSnapshot` | - | ✗ |
| `jolt/updateDocument` | `DocumentSnapshot` | - | ✗ |
| `jolt/closeDocument` | `string` | - | ✗ |
| `jolt/getOpenDocuments` | - | `IReadOnlyList<DocumentSnapshot>` | ✓ |
| `jolt/getFrontendContext` | `GetFrontendContextRequest` | `GetFrontendContextResponse` | ✗ |
| `jolt/analyzeJazor` | `AnalyzeJazorRequest` | `AnalyzeJazorResponse` | ✗ |
| `jolt/getVirtualArtifact` | `GetVirtualArtifactRequest` | `GetVirtualArtifactResponse` | ✗ |
| `jolt/getHotUpdatePlan` | `GetHotUpdatePlanRequest` | `GetHotUpdatePlanResponse` | ✗ |

### C. 错误码清单

| 错误码 | 异常类型 | 可重试 | HTTP 类比 |
|--------|---------|--------|---------|
| `cancelled` | `OperationCanceledException` | 视场景 | 499 |
| `invalid_request` | `JoltRpcException` | ✗ | 400 |
| `unknown_method` | `JoltRpcException` | ✗ | 404 |
| `invalid_payload` | `JoltRpcException` | ✗ | 422 |
| `internal_error` | 其他 `Exception` | ✓ | 500 |

### D. 传输协议示例

**完整会话示例**：
```
# 客户端 → 服务器
{"id":"1","method":"jolt/ping","payloadJson":null}

# 服务器 → 客户端
{"id":"1","success":true,"payloadJson":"{\"message\":\"pong\",\"protocolVersion\":\"1.0.0\"}","error":null}

# 客户端 → 服务器
{"id":"2","method":"jolt/openDocument","payloadJson":"{\"documentPath\":\"/path/to/file.jazor\",\"documentKind\":0,\"text\":\"@code {}\",\"version\":null}"}

# 服务器 → 客户端
{"id":"2","success":true,"payloadJson":null,"error":null}

# 客户端 → 服务器
{"id":"3","method":"jolt/unknownMethod","payloadJson":null}

# 服务器 → 客户端
{"id":"3","success":false,"payloadJson":null,"error":{"code":"unknown_method","message":"Unknown Jolt RPC method 'jolt/unknownMethod'.","details":"Jolt.Rpc.JoltRpcException"}}
```

### E. 相关文档

- `Contracts.md` - RPC 契约类型定义
- `Documents.md` - 文档版本管理
- Jazor.VueHost 客户端实现：`src/Jolt/Analysis/RpcVueAnalysisClient.cs`

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
