using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jolt.DevServer;

internal sealed class DevServerReloadHub : IAsyncDisposable
{
    private static readonly TimeSpan DefaultSendTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan DefaultHeartbeatTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultHeartbeatSweepInterval = TimeSpan.FromSeconds(10);
    private const int DefaultMaxIncomingMessageBytes = 64 * 1024;
    private readonly ConcurrentDictionary<WebSocket, HmrClientState> _sockets = new();
    private readonly TimeSpan _sendTimeout;
    private readonly TimeSpan _heartbeatTimeout;
    private readonly int _maxIncomingMessageBytes;
    private readonly CancellationTokenSource _heartbeatSweepCancellationSource = new();
    private readonly Task _heartbeatSweepTask;

    public DevServerReloadHub(
        TimeSpan? sendTimeout = null,
        TimeSpan? heartbeatTimeout = null,
        TimeSpan? heartbeatSweepInterval = null,
        int? maxIncomingMessageBytes = null)
    {
        _sendTimeout = sendTimeout is { } timeout && timeout > TimeSpan.Zero
            ? timeout
            : DefaultSendTimeout;
        _heartbeatTimeout = heartbeatTimeout is { } heartbeat && heartbeat > TimeSpan.Zero
            ? heartbeat
            : DefaultHeartbeatTimeout;
        _maxIncomingMessageBytes = maxIncomingMessageBytes is { } maxBytes && maxBytes > 0
            ? maxBytes
            : DefaultMaxIncomingMessageBytes;
        var sweepInterval = heartbeatSweepInterval is { } interval && interval > TimeSpan.Zero
            ? interval
            : DefaultHeartbeatSweepInterval;
        _heartbeatSweepTask = RunHeartbeatSweepAsync(sweepInterval, _heartbeatSweepCancellationSource.Token);
    }

    public int ConnectedClientCount => _sockets.Count;

    public async Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(socket);
        var state = new HmrClientState(Guid.NewGuid().ToString("N")[..8]);
        _sockets.TryAdd(socket, state);
        var buffer = new byte[256];
        var closeStatus = WebSocketCloseStatus.NormalClosure;
        var closeDescription = "Jolt dev server shutdown";

        try
        {
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
            while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var result = await ReceiveMessageAsync(
                    socket,
                    buffer,
                    _maxIncomingMessageBytes,
                    cancellationToken);
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
        catch (HmrMessageTooLargeException exception)
        {
            closeStatus = WebSocketCloseStatus.MessageTooBig;
            closeDescription = exception.Message;
        }
        finally
        {
            RemoveSocket(socket);
            await CloseAndDisposeAsync(socket, closeStatus, closeDescription, CancellationToken.None);
        }
    }

    public async Task BroadcastReloadAsync(
        string? reason,
        CancellationToken cancellationToken)
        => await BroadcastAsync(
            new DevServerNotificationEnvelope
            {
                Type = "full-reload",
                Reason = string.IsNullOrWhiteSpace(reason) ? null : reason
            },
            cancellationToken);

    public async Task BroadcastStyleUpdateAsync(
        IReadOnlyList<string> changedCssUrls,
        IReadOnlyList<InlineStyleUpdate> inlineStyleUpdates,
        long timestamp,
        CancellationToken cancellationToken)
        => await BroadcastAsync(
            new DevServerNotificationEnvelope
            {
                Type = "style-update",
                Paths = changedCssUrls,
                InlineStyles = inlineStyleUpdates,
                Timestamp = timestamp
            },
            cancellationToken);

    public async Task BroadcastJavaScriptUpdateAsync(
        IReadOnlyList<JavaScriptHotUpdate> updates,
        long timestamp,
        CancellationToken cancellationToken)
        => await BroadcastAsync(
            new DevServerNotificationEnvelope
            {
                Type = "update",
                Updates = updates,
                Timestamp = timestamp
            },
            cancellationToken);

    public async Task BroadcastErrorAsync(
        string? message,
        CancellationToken cancellationToken)
        => await BroadcastAsync(
            new DevServerNotificationEnvelope
            {
                Type = "error",
                Message = string.IsNullOrWhiteSpace(message) ? "Hot update failed." : message
            },
            cancellationToken);

    private async Task BroadcastAsync(object payload, CancellationToken cancellationToken)
    {
        await PruneExpiredClientsAsync(DateTimeOffset.UtcNow);
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

    private async Task SendWithClientStateAsync(
        WebSocket socket,
        HmrClientState state,
        object payload,
        CancellationToken cancellationToken)
    {
        using var sendTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        sendTokenSource.CancelAfter(_sendTimeout);
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

    private static async Task SendAsync(
        WebSocket socket,
        object payload,
        CancellationToken cancellationToken)
    {
        var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        await socket.SendAsync(
            message,
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken);
    }

    private static async Task<HmrReceivedMessage> ReceiveMessageAsync(
        WebSocket socket,
        byte[] buffer,
        int maxIncomingMessageBytes,
        CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();
        var totalMessageBytes = 0;
        while (true)
        {
            var result = await socket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                return new HmrReceivedMessage(result.MessageType, null);
            }

            totalMessageBytes += result.Count;
            if (totalMessageBytes > maxIncomingMessageBytes)
            {
                throw new HmrMessageTooLargeException(
                    $"HMR client message exceeds the {maxIncomingMessageBytes} byte limit.");
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

        if (string.Equals(message.Type, "ready", StringComparison.OrdinalIgnoreCase))
        {
            state.IsReady = true;
            state.LastSeenUtc = DateTimeOffset.UtcNow;
            return;
        }

        if (string.Equals(message.Type, "heartbeat", StringComparison.OrdinalIgnoreCase))
        {
            state.LastSeenUtc = DateTimeOffset.UtcNow;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _heartbeatSweepCancellationSource.Cancel();
        try
        {
            await _heartbeatSweepTask;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _heartbeatSweepCancellationSource.Dispose();
        }

        foreach (var entry in _sockets.ToArray())
        {
            RemoveSocket(entry.Key);
            await CloseAndDisposeAsync(entry.Key, CancellationToken.None);
        }
    }

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

    private bool RemoveSocket(WebSocket socket)
    {
        if (_sockets.TryRemove(socket, out var state))
        {
            state.Dispose();
            return true;
        }

        return false;
    }

    private static async Task CloseAndDisposeAsync(
        WebSocket socket,
        WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure,
        string closeDescription = "Jolt dev server shutdown",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(
                    closeStatus,
                    closeDescription,
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

    private static Task CloseAndDisposeAsync(WebSocket socket, CancellationToken cancellationToken)
        => CloseAndDisposeAsync(
            socket,
            WebSocketCloseStatus.NormalClosure,
            "Jolt dev server shutdown",
            cancellationToken);
}

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

internal sealed class DevServerClientMessage
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }
}

internal sealed class HmrClientState(string clientId) : IDisposable
{
    public string ClientId { get; } = clientId;

    public bool IsReady { get; set; }

    public DateTimeOffset LastSeenUtc { get; set; } = DateTimeOffset.UtcNow;

    public SemaphoreSlim SendGate { get; } = new(1, 1);

    public void Dispose()
        => SendGate.Dispose();
}

internal readonly record struct HmrReceivedMessage(
    WebSocketMessageType MessageType,
    string? Text);

internal sealed class HmrMessageTooLargeException(string message) : InvalidOperationException(message);
