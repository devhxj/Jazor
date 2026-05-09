# HMR (Hot Module Replacement)

`DevServerReloadHub` 实现（`src/Jolt/DevServer/DevServerReloadHub.cs`），管理 WebSocket 连接、广播 HMR 更新、心跳检测和过期客户端清理。

## 核心类型

### DevServerReloadHub

**职责**：WebSocket HMR 广播中心，管理客户端连接生命周期。

**核心成员**：
```csharp
internal sealed class DevServerReloadHub : IAsyncDisposable
{
    private static readonly TimeSpan DefaultSendTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan DefaultHeartbeatTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultHeartbeatSweepInterval = TimeSpan.FromSeconds(10);

    private readonly ConcurrentDictionary<WebSocket, HmrClientState> _sockets = new();
    private readonly TimeSpan _sendTimeout;
    private readonly TimeSpan _heartbeatTimeout;
    private readonly CancellationTokenSource _heartbeatSweepCancellationSource = new();
    private readonly Task _heartbeatSweepTask;

    public int ConnectedClientCount => _sockets.Count;

    public async Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken);
    public async Task BroadcastReloadAsync(string? reason, CancellationToken cancellationToken);
    public async Task BroadcastStyleUpdateAsync(
        IReadOnlyList<string> changedCssUrls,
        IReadOnlyList<InlineStyleUpdate> inlineStyleUpdates,
        long timestamp,
        CancellationToken cancellationToken);
    public async Task BroadcastJavaScriptUpdateAsync(
        IReadOnlyList<JavaScriptHotUpdate> updates,
        long timestamp,
        CancellationToken cancellationToken);
    public async Task BroadcastErrorAsync(string? message, CancellationToken cancellationToken);
}
```

### HmrClientState

**职责**：客户端连接状态跟踪。

**定义**（第 428-440 行）：
```csharp
internal sealed class HmrClientState(string clientId) : IDisposable
{
    public string ClientId { get; } = clientId;

    public bool IsReady { get; set; }

    public DateTimeOffset LastSeenUtc { get; set; } = DateTimeOffset.UtcNow;

    public SemaphoreSlim SendGate { get; } = new(1, 1);

    public void Dispose()
        => SendGate.Dispose();
}
```

**状态字段**：
- `ClientId`：8 位随机字符串（GUID 的前 8 位）
- `IsReady`：客户端是否已发送 `"ready"` 消息
- `LastSeenUtc`：最后一次活动时间（用于心跳检测）
- `SendGate`：信号量，防止并发发送导致消息乱序

### DevServerNotificationEnvelope

**职责**：服务端到客户端的通知消息封装。

**定义**（第 392-420 行）：
```csharp
internal sealed class DevServerNotificationEnvelope
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("clientId")]
    public string? ClientId { get; init; }

    [JsonPropertyName("connectedClientCount")]
    public int? ConnectedClientCount { get; init; }

    [JsonPropertyName("paths")]
    public IReadOnlyList<string>? Paths { get; init; }

    [JsonPropertyName("inlineStyles")]
    public IReadOnlyList<InlineStyleUpdate>? InlineStyles { get; init; }

    [JsonPropertyName("updates")]
    public IReadOnlyList<JavaScriptHotUpdate>? Updates { get; init; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}
```

**消息类型**：
- `"connected"`：连接建立，返回 clientId 和连接数
- `"full-reload"`：全页面重新加载
- `"style-update"`：CSS 样式更新
- `"update"`：JavaScript 模块更新
- `"error"`：编译或更新错误

### DevServerClientMessage

**职责**：客户端到服务器的消息封装。

**定义**（第 422-426 行）：
```csharp
internal sealed class DevServerClientMessage
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }
}
```

**消息类型**：
- `"ready"`：客户端准备就绪，可以接收 HMR 更新
- `"heartbeat"`：心跳包，保持连接活跃

## 核心算法

### 连接接受

**AcceptAsync**（第 39-80 行）：
```csharp
public async Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(socket);
    var state = new HmrClientState(Guid.NewGuid().ToString("N")[..8]);
    _sockets.TryAdd(socket, state);
    var buffer = new byte[256];

    try
    {
        // 1. 发送连接确认消息
        await SendWithClientStateAsync(
            socket,
            state,
            new DevServerNotificationEnvelope
            {
                Type = "connected",
                ClientId = state.ClientId,
                ConnectedClientCount = ConnectedClientCount
            },
            cancellationToken);

        // 2. 消息接收循环
        while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
        {
            var result = await ReceiveMessageAsync(socket, buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }

            if (result.Text is not null)
            {
                ProcessClientMessage(state, result.Text);
            }
        }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
    }
    finally
    {
        RemoveSocket(socket);
        await CloseAndDisposeAsync(socket, CancellationToken.None);
    }
}
```

### 消息接收

**ReceiveMessageAsync**（第 235-263 行）：
```csharp
private static async Task<HmrReceivedMessage> ReceiveMessageAsync(
    WebSocket socket,
    byte[] buffer,
    CancellationToken cancellationToken)
{
    using var stream = new MemoryStream();
    while (true)
    {
        var result = await socket.ReceiveAsync(buffer, cancellationToken);
        if (result.MessageType == WebSocketMessageType.Close)
        {
            return new HmrReceivedMessage(result.MessageType, null);
        }

        if (result.MessageType == WebSocketMessageType.Text)
        {
            stream.Write(buffer, 0, result.Count);
        }

        if (result.EndOfMessage)
        {
            return new HmrReceivedMessage(
                result.MessageType,
                result.MessageType == WebSocketMessageType.Text
                    ? Encoding.UTF8.GetString(stream.ToArray())
                    : null);
        }
    }
}
```

### 客户端消息处理

**ProcessClientMessage**（第 265-293 行）：
```csharp
private static void ProcessClientMessage(HmrClientState state, string text)
{
    DevServerClientMessage? message;
    try
    {
        message = JsonSerializer.Deserialize<DevServerClientMessage>(text);
    }
    catch (JsonException)
    {
        return;
    }

    if (message?.Type is null)
    {
        return;
    }

    // 处理 "ready" 消息
    if (string.Equals(message.Type, "ready", StringComparison.OrdinalIgnoreCase))
    {
        state.IsReady = true;
        state.LastSeenUtc = DateTimeOffset.UtcNow;
        return;
    }

    // 处理 "heartbeat" 消息
    if (string.Equals(message.Type, "heartbeat", StringComparison.OrdinalIgnoreCase))
    {
        state.LastSeenUtc = DateTimeOffset.UtcNow;
    }
}
```

### 广播算法

**BroadcastAsync**（第 132-155 行）：
```csharp
private async Task BroadcastAsync(object payload, CancellationToken cancellationToken)
{
    // 1. 清理过期客户端
    await PruneExpiredClientsAsync(DateTimeOffset.UtcNow);

    // 2. 并行广播
    var broadcastTasks = new List<Task>(_sockets.Count);
    foreach (var entry in _sockets.ToArray())
    {
        var socket = entry.Key;
        var state = entry.Value;
        if (socket.State != WebSocketState.Open)
        {
            RemoveSocket(socket);
            continue;
        }

        broadcastTasks.Add(SendToClientAsync(socket, state, payload, cancellationToken));
    }

    if (broadcastTasks.Count == 0)
    {
        return;
    }

    await Task.WhenAll(broadcastTasks);
}
```

### 发送到单个客户端

**SendToClientAsync**（第 157-201 行）：
```csharp
private async Task SendToClientAsync(
    WebSocket socket,
    HmrClientState state,
    object payload,
    CancellationToken cancellationToken)
{
    try
    {
        await SendWithClientStateAsync(socket, state, payload, cancellationToken);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        throw;
    }
    catch (OperationCanceledException)
    {
        RemoveSocket(socket);
        await CloseAndDisposeAsync(socket, CancellationToken.None);
    }
    catch (WebSocketException)
    {
        RemoveSocket(socket);
        await CloseAndDisposeAsync(socket, CancellationToken.None);
    }
    catch (ObjectDisposedException)
    {
        RemoveSocket(socket);
        await CloseAndDisposeAsync(socket, CancellationToken.None);
    }
    catch (InvalidOperationException)
    {
        RemoveSocket(socket);
        await CloseAndDisposeAsync(socket, CancellationToken.None);
    }
    catch (JsonException)
    {
        RemoveSocket(socket);
        await CloseAndDisposeAsync(socket, CancellationToken.None);
    }
    catch (NotSupportedException)
    {
        RemoveSocket(socket);
        await CloseAndDisposeAsync(socket, CancellationToken.None);
    }
}
```

**SendWithClientStateAsync**（第 203-220 行）：
```csharp
private async Task SendWithClientStateAsync(
    WebSocket socket,
    HmrClientState state,
    object payload,
    CancellationToken cancellationToken)
{
    // 1. 创建链接的取消令牌（超时取消）
    using var sendTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    sendTokenSource.CancelAfter(_sendTimeout);

    // 2. 等待发送许可（防止并发）
    await state.SendGate.WaitAsync(sendTokenSource.Token);
    try
    {
        await SendAsync(socket, payload, sendTokenSource.Token);
    }
    finally
    {
        state.SendGate.Release();
    }
}
```

### 心跳检测

**RunHeartbeatSweepAsync**（第 317-332 行）：
```csharp
private async Task RunHeartbeatSweepAsync(
    TimeSpan sweepInterval,
    CancellationToken cancellationToken)
{
    using var timer = new PeriodicTimer(sweepInterval);
    try
    {
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await PruneExpiredClientsAsync(DateTimeOffset.UtcNow);
        }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
    }
}
```

**PruneExpiredClientsAsync**（第 334-348 行）：
```csharp
private async Task PruneExpiredClientsAsync(DateTimeOffset now)
{
    foreach (var entry in _sockets.ToArray())
    {
        if (now - entry.Value.LastSeenUtc <= _heartbeatTimeout)
        {
            continue;
        }

        if (RemoveSocket(entry.Key))
        {
            await CloseAndDisposeAsync(entry.Key, CancellationToken.None);
        }
    }
}
```

### Socket 清理

**RemoveSocket**（第 350-359 行）：
```csharp
private bool RemoveSocket(WebSocket socket)
{
    if (_sockets.TryRemove(socket, out var state))
    {
        state.Dispose(); // 释放 SemaphoreSlim
        return true;
    }

    return false;
}
```

**CloseAndDisposeAsync**（第 361-389 行）：
```csharp
private static async Task CloseAndDisposeAsync(WebSocket socket, CancellationToken cancellationToken)
{
    try
    {
        if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            await socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Jolt dev server shutdown",
                cancellationToken);
        }
    }
    catch (OperationCanceledException)
    {
    }
    catch (WebSocketException)
    {
    }
    catch (ObjectDisposedException)
    {
    }
    catch (InvalidOperationException)
    {
    }
    finally
    {
        socket.Dispose();
    }
}
```

## 线程安全模型

### ConcurrentDictionary

**线程安全集合**：
```csharp
private readonly ConcurrentDictionary<WebSocket, HmrClientState> _sockets = new();
```

**特点**：
- 无锁读写
- 细粒度锁（每个桶独立锁）
- 适用于高并发场景

### SemaphoreSlim 防止并发发送

**SendGate 信号量**（第 436 行）：
```csharp
public SemaphoreSlim SendGate { get; } = new(1, 1);
```

**作用**：
- 确保同一客户端的消息顺序性
- 防止并发发送导致 WebSocket 帧乱序
- 配合超时机制实现背压处理

### 超时取消

**链接取消令牌**（第 209-211 行）：
```csharp
using var sendTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
sendTokenSource.CancelAfter(_sendTimeout); // 默认 2 秒
```

**超时策略**：
- 发送操作必须在 `_sendTimeout` 内完成
- 超时后自动取消发送任务
- 异常处理会移除慢速客户端

## 错误处理

### 慢速客户端断开

**场景**：客户端处理消息缓慢，导致发送队列积压

**处理策略**（第 171-200 行）：
```csharp
try
{
    await SendWithClientStateAsync(socket, state, payload, cancellationToken);
}
catch (OperationCanceledException)
{
    // 超时：断开慢速客户端
    RemoveSocket(socket);
    await CloseAndDisposeAsync(socket, CancellationToken.None);
}
```

**原因**：
- 防止内存泄漏（发送队列无限增长）
- 保护服务器性能（避免阻塞其他客户端）
- 强制客户端重新连接（可能解决临时性问题）

### JSON 序列化容错

**ProcessClientMessage**（第 265-293 行）：
```csharp
DevServerClientMessage? message;
try
{
    message = JsonSerializer.Deserialize<DevServerClientMessage>(text);
}
catch (JsonException)
{
    return; // 忽略无效消息
}
```

**原因**：
- 客户端可能发送非 JSON 数据
- 避免因单个客户端错误导致整个连接中断
- 容错设计确保服务器稳定性

### WebSocket 异常处理

**全覆盖异常处理**（第 361-389 行）：
```csharp
try
{
    if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
    {
        await socket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Jolt dev server shutdown",
            cancellationToken);
    }
}
catch (OperationCanceledException) { }
catch (WebSocketException) { }
catch (ObjectDisposedException) { }
catch (InvalidOperationException) { }
finally
{
    socket.Dispose();
}
```

**原因**：
- WebSocket 状态可能异步变化
- 关闭操作可能竞争失败
- 确保 Socket 总是被释放（避免资源泄漏）

## 配置选项

### 超时配置

**默认值**（第 11-13 行）：
```csharp
private static readonly TimeSpan DefaultSendTimeout = TimeSpan.FromSeconds(2);
private static readonly TimeSpan DefaultHeartbeatTimeout = TimeSpan.FromSeconds(30);
private static readonly TimeSpan DefaultHeartbeatSweepInterval = TimeSpan.FromSeconds(10);
```

**可配置**（第 20-34 行）：
```csharp
public DevServerReloadHub(
    TimeSpan? sendTimeout = null,
    TimeSpan? heartbeatTimeout = null,
    TimeSpan? heartbeatSweepInterval = null)
{
    _sendTimeout = sendTimeout is { } timeout && timeout > TimeSpan.Zero
        ? timeout
        : DefaultSendTimeout;
    _heartbeatTimeout = heartbeatTimeout is { } heartbeat && heartbeat > TimeSpan.Zero
        ? heartbeat
        : DefaultHeartbeatTimeout;
    var sweepInterval = heartbeatSweepInterval is { } interval && interval > TimeSpan.Zero
        ? interval
        : DefaultHeartbeatSweepInterval;
    _heartbeatSweepTask = RunHeartbeatSweepAsync(sweepInterval, _heartbeatSweepCancellationSource.Token);
}
```

**配置建议**：
- `SendTimeout`：2 秒（默认），适用于本地网络
- `HeartbeatTimeout`：30 秒（默认），3 倍于客户端心跳间隔
- `HeartbeatSweepInterval`：10 秒（默认），平衡检测精度和 CPU 开销

## 与其他子系统的交互

### 与 ChangeProcessor 的集成

**HMR 广播触发**（`DevHttpServer.BroadcastChangeResultAsync`，第 520-550 行）：
```csharp
private async Task BroadcastChangeResultAsync(
    ChangeProcessingResult result,
    CancellationToken cancellationToken)
{
    if (result.UpdateKind == ChangeUpdateKind.StyleUpdate)
    {
        await _reloadHub.BroadcastStyleUpdateAsync(
            result.ChangedCssUrls,
            result.InlineStyleUpdates,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            cancellationToken);
        return;
    }

    if (result.UpdateKind == ChangeUpdateKind.JavaScriptUpdate)
    {
        await _reloadHub.BroadcastJavaScriptUpdateAsync(
            result.JavaScriptUpdates,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            cancellationToken);
        return;
    }

    if (result.UpdateKind == ChangeUpdateKind.Error)
    {
        await _reloadHub.BroadcastErrorAsync(result.ErrorMessage, cancellationToken);
        return;
    }

    await _reloadHub.BroadcastReloadAsync(result.FullReloadReason, cancellationToken);
}
```

### 与 Dev 客户端的集成

**Dev 客户端脚本**（`HtmlTransformer.GetDevClientScript()`，第 95-363 行）：

**WebSocket 连接**（第 337-360 行）：
```javascript
function connect() {
  try {
    socket = new WebSocket(socketUrl);
    socket.addEventListener("open", () => {
      if (reconnectTimer) {
        clearTimeout(reconnectTimer);
        reconnectTimer = undefined;
      }
      sendMessage({ type: "ready" });
      startHeartbeat();
    });
    socket.addEventListener("message", handleSocketMessage);
    socket.addEventListener("close", () => {
      stopHeartbeat();
      scheduleReconnect();
    });
    socket.addEventListener("error", () => {
      stopHeartbeat();
      socket?.close();
    });
  } catch {
    scheduleReconnect();
  }
}
```

**心跳发送**（第 287-292 行）：
```javascript
function startHeartbeat() {
  stopHeartbeat();
  heartbeatTimer = setInterval(() => {
    sendMessage({ type: "heartbeat" });
  }, 15000); // 15 秒间隔
}
```

**重连机制**（第 293-302 行）：
```javascript
function scheduleReconnect() {
  if (reconnectTimer) {
    return;
  }
  stopHeartbeat();
  reconnectTimer = setTimeout(() => {
    reconnectTimer = undefined;
    connect();
  }, 2000); // 2 秒后重连
}
```

## 设计权衡

### 背压处理 vs 连接稳定性

**当前设计**：超时断开慢速客户端

**优点**：
- 保护服务器资源
- 防止内存泄漏
- 强制客户端恢复健康状态

**缺点**：
- 可能误伤临时慢速的客户端（如 GC 暂停）
- 需要客户端实现重连逻辑

**替代方案**（未采用）：
- **消息队列**：增加内存开销和复杂度
- **流量控制**：需要协议层支持
- **降级服务**：降低所有客户端体验

### 心跳检测精度 vs CPU 开销

**当前设计**：10 秒扫描间隔

**权衡**：
- 更短间隔（如 1 秒）：更快检测断开，但 CPU 开销高
- 更长间隔（如 60 秒）：降低 CPU 开销，但检测延迟高

**选择理由**：
- 开发服务器通常负载不高
- 10 秒延迟可接受
- 与 30 秒超时配合（3 倍关系）

### 并行广播 vs 顺序广播

**当前设计**：`Task.WhenAll` 并行广播

**优点**：
- 低延迟（所有客户端几乎同时收到更新）
- 高吞吐（充分利用网络带宽）

**缺点**：
- 单个慢速客户端不影响其他客户端（已被超时机制缓解）
- 内存占用高（并发创建多个发送任务）

**替代方案**（未采用）：
- **顺序广播**：延迟高，但内存占用低
- **分批广播**：复杂度增加，收益有限

### 客户端 ID 生成

**当前设计**：GUID 的前 8 位

**代码**（第 42 行）：
```csharp
var state = new HmrClientState(Guid.NewGuid().ToString("N")[..8]);
```

**优点**：
- 简洁易读（日志友好）
- 足够唯一（16^8 = 4,294,967,296 种可能）
- 无需额外状态管理

**缺点**：
- 理论上可能冲突（极低概率）

**替代方案**（未采用）：
- **完整 GUID**：过长，不易读
- **自增 ID**：需要状态管理，重启后重复
- **哈希值**：冲突概率略高
