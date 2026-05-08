using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.AspNetCore.Dev;

internal sealed class JazorDevelopmentReloadHub : IAsyncDisposable
{
    private static readonly TimeSpan DefaultSendTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan DefaultHeartbeatTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultHeartbeatSweepInterval = TimeSpan.FromSeconds(10);
    private const int DefaultMaxIncomingMessageBytes = 64 * 1024;
    private readonly ConcurrentDictionary<WebSocket, ClientState> _sockets = new();
    private readonly TimeSpan _sendTimeout;
    private readonly TimeSpan _heartbeatTimeout;
    private readonly int _maxIncomingMessageBytes;
    private readonly CancellationTokenSource _heartbeatSweepCancellationSource = new();
    private readonly Task _heartbeatSweepTask;
    private int _disposeState;

    public JazorDevelopmentReloadHub(
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

    public async Task AcceptAsync(
        WebSocket socket,
        string serverInstanceId,
        long reloadSequence,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(socket);
        ArgumentException.ThrowIfNullOrWhiteSpace(serverInstanceId);

        var state = new ClientState(Guid.NewGuid().ToString("N")[..8]);
        _sockets.TryAdd(socket, state);
        var buffer = new byte[256];
        var closeStatus = WebSocketCloseStatus.NormalClosure;
        var closeDescription = "Jazor development reload shutdown";

        try
        {
            await SendWithClientStateAsync(
                socket,
                state,
                new DevelopmentReloadNotificationEnvelope
                {
                    Type = "connected",
                    ClientId = state.ClientId,
                    ConnectedClientCount = ConnectedClientCount,
                    ServerInstanceId = serverInstanceId,
                    ReloadSequence = reloadSequence
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
                    break;

                if (result.Text is not null)
                    ProcessClientMessage(state, result.Text);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (ReceivedMessageTooLargeException exception)
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

    public Task BroadcastReloadAsync(
        string serverInstanceId,
        long reloadSequence,
        string? reason,
        CancellationToken cancellationToken)
        => BroadcastAsync(
            new DevelopmentReloadNotificationEnvelope
            {
                Type = "full-reload",
                ServerInstanceId = serverInstanceId,
                ReloadSequence = reloadSequence,
                Reason = string.IsNullOrWhiteSpace(reason) ? null : reason
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
            return;

        await Task.WhenAll(broadcastTasks);
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeState, 1) != 0)
            return;

        try
        {
            _heartbeatSweepCancellationSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        try
        {
            await _heartbeatSweepTask;
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
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

    private async Task SendToClientAsync(
        WebSocket socket,
        ClientState state,
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
        ClientState state,
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

    private static async Task<ReceivedMessage> ReceiveMessageAsync(
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
                return new ReceivedMessage(result.MessageType, null);

            totalMessageBytes += result.Count;
            if (totalMessageBytes > maxIncomingMessageBytes)
            {
                throw new ReceivedMessageTooLargeException(
                    $"Jazor development reload client message exceeds the {maxIncomingMessageBytes} byte limit.");
            }

            if (result.MessageType == WebSocketMessageType.Text)
                stream.Write(buffer, 0, result.Count);

            if (result.EndOfMessage)
            {
                return new ReceivedMessage(
                    result.MessageType,
                    result.MessageType == WebSocketMessageType.Text
                        ? Encoding.UTF8.GetString(stream.ToArray())
                        : null);
            }
        }
    }

    private static void ProcessClientMessage(ClientState state, string text)
    {
        DevelopmentReloadClientMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<DevelopmentReloadClientMessage>(text);
        }
        catch (JsonException)
        {
            return;
        }

        if (message?.Type is null)
            return;

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
                continue;

            if (RemoveSocket(entry.Key))
                await CloseAndDisposeAsync(entry.Key, CancellationToken.None);
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
        string closeDescription = "Jazor development reload shutdown",
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
        catch (IOException)
        {
        }
        finally
        {
            try
            {
                socket.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private static Task CloseAndDisposeAsync(WebSocket socket, CancellationToken cancellationToken)
        => CloseAndDisposeAsync(
            socket,
            WebSocketCloseStatus.NormalClosure,
            "Jazor development reload shutdown",
            cancellationToken);
}

internal sealed class DevelopmentReloadNotificationEnvelope
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("clientId")]
    public string? ClientId { get; init; }

    [JsonPropertyName("connectedClientCount")]
    public int? ConnectedClientCount { get; init; }

    [JsonPropertyName("serverInstanceId")]
    public string? ServerInstanceId { get; init; }

    [JsonPropertyName("reloadSequence")]
    public long? ReloadSequence { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}

internal sealed class DevelopmentReloadClientMessage
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }
}

internal sealed class ClientState(string clientId) : IDisposable
{
    public string ClientId { get; } = clientId;

    public bool IsReady { get; set; }

    public DateTimeOffset LastSeenUtc { get; set; } = DateTimeOffset.UtcNow;

    public SemaphoreSlim SendGate { get; } = new(1, 1);

    public void Dispose()
        => SendGate.Dispose();
}

internal readonly record struct ReceivedMessage(
    WebSocketMessageType MessageType,
    string? Text);

internal sealed class ReceivedMessageTooLargeException(string message) : InvalidOperationException(message);
