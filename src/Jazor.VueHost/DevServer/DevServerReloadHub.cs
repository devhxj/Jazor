using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.VueHost.DevServer;

internal sealed class DevServerReloadHub : IAsyncDisposable
{
    private readonly ConcurrentDictionary<WebSocket, byte> _sockets = new();

    public async Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(socket);
        _sockets.TryAdd(socket, 0);
        var buffer = new byte[256];

        try
        {
            await SendAsync(socket, new DevServerNotificationEnvelope { Type = "connected" }, cancellationToken);
            while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            _sockets.TryRemove(socket, out _);
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

    private async Task BroadcastAsync(object payload, CancellationToken cancellationToken)
    {
        foreach (var socket in _sockets.Keys)
        {
            if (socket.State != WebSocketState.Open)
            {
                _sockets.TryRemove(socket, out _);
                continue;
            }

            try
            {
                await SendAsync(socket, payload, cancellationToken);
            }
            catch
            {
                _sockets.TryRemove(socket, out _);
            }
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

    public async ValueTask DisposeAsync()
    {
        foreach (var socket in _sockets.Keys)
        {
            _sockets.TryRemove(socket, out _);
            await CloseAndDisposeAsync(socket, CancellationToken.None);
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
        catch
        {
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
}
