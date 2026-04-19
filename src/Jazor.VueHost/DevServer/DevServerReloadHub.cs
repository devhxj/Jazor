using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.VueHost.DevServer;

internal sealed class DevServerReloadHub : IAsyncDisposable
{
    private static readonly TimeSpan DefaultSendTimeout = TimeSpan.FromSeconds(2);
    private readonly ConcurrentDictionary<WebSocket, HmrClientState> _sockets = new();
    private readonly TimeSpan _sendTimeout;

    public DevServerReloadHub(TimeSpan? sendTimeout = null)
    {
        _sendTimeout = sendTimeout is { } timeout && timeout > TimeSpan.Zero
            ? timeout
            : DefaultSendTimeout;
    }

    public int ConnectedClientCount => _sockets.Count;

    public async Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(socket);
        var state = new HmrClientState(Guid.NewGuid().ToString("N")[..8]);
        _sockets.TryAdd(socket, state);
        var buffer = new byte[256];

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
        catch (Exception) {
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
        foreach (var entry in _sockets.ToArray())
        {
            RemoveSocket(entry.Key);
            await CloseAndDisposeAsync(entry.Key, CancellationToken.None);
        }
    }

    private void RemoveSocket(WebSocket socket)
    {
        if (_sockets.TryRemove(socket, out var state))
        {
            state.Dispose();
        }
    }

    private static async Task CloseAndDisposeAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        try
        {
            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "VueHost dev server shutdown",
                    cancellationToken);
            }
        }
        catch (Exception) {
        }
        finally
        {
            socket.Dispose();
        }
    }
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
