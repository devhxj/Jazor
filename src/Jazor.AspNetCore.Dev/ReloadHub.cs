using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jazor.AspNetCore.Dev;

/// <summary>
/// Owns the reload WebSocket transport. CLR names may evolve, but the JSON field names
/// and message types below are the browser protocol and must remain stable.
/// </summary>
internal sealed class ReloadHub : IAsyncDisposable
{
    private static readonly TimeSpan DefaultSendTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan DefaultHeartbeatTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultHeartbeatSweepInterval = TimeSpan.FromSeconds(10);
    private const int DefaultMaxIncomingMessageBytes = 64 * 1024;
    internal const string ModuleUpdateCapability = "module-update";
    private readonly ConcurrentDictionary<WebSocket, ReloadClient> _sockets = new();
    private readonly TimeSpan _sendTimeout;
    private readonly TimeSpan _heartbeatTimeout;
    private readonly int _maxIncomingMessageBytes;
    private readonly CancellationTokenSource _heartbeatSweepCancellationSource = new();
    private readonly Task _heartbeatSweepTask;
    private int _disposeState;

    public ReloadHub(
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

        var client = new ReloadClient(Guid.NewGuid().ToString("N")[..8]);
        _sockets.TryAdd(socket, client);
        var buffer = new byte[256];
        var closeStatus = WebSocketCloseStatus.NormalClosure;
        var closeDescription = "Jazor reload shutdown";

        try
        {
            await SendWithGateAsync(
                socket,
                client,
                new ReloadMessage
                {
                    Type = "connected",
                    ClientId = client.ClientId,
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
                    ProcessClientMessage(client, result.Text);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (SocketMessageTooLargeException exception)
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
            new ReloadMessage
            {
                Type = "full-reload",
                ServerInstanceId = serverInstanceId,
                ReloadSequence = reloadSequence,
                Reason = string.IsNullOrWhiteSpace(reason) ? null : reason
            },
            cancellationToken);

    public Task BroadcastModuleUpdateAsync(
        string serverInstanceId,
        long reloadSequence,
        string? reason,
        IReadOnlyList<HmrModuleUpdate> moduleUpdates,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serverInstanceId);
        ArgumentNullException.ThrowIfNull(moduleUpdates);
        if (moduleUpdates.Count == 0)
            throw new ArgumentException("At least one module update is required.", nameof(moduleUpdates));

        var updates = moduleUpdates
            .Select(static update => new ReloadModuleUpdate
            {
                Path = update.Path,
                Url = update.Url,
                ComponentId = update.ComponentId,
                ModuleId = update.ModuleId,
                DescriptorHash = update.DescriptorHash,
                TemplateHash = update.TemplateHash,
                LogicHash = update.LogicHash,
                BoundaryKind = update.BoundaryKind
            })
            .ToArray();

        // Older clients only understand full reload. The server selects their payload
        // individually so a newer client can keep component state during the same change.
        var moduleUpdate = new ReloadMessage
        {
            Type = "module-update",
            ServerInstanceId = serverInstanceId,
            ReloadSequence = reloadSequence,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason,
            ChangedPaths = updates.Select(static update => update.Path).ToArray(),
            ModuleUpdates = updates
        };
        var fullReloadFallback = new ReloadMessage
        {
            Type = "full-reload",
            ServerInstanceId = serverInstanceId,
            ReloadSequence = reloadSequence,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason
        };

        return BroadcastAsync(
            client => client.SupportsCapability(ModuleUpdateCapability) ? moduleUpdate : fullReloadFallback,
            cancellationToken);
    }

    private async Task BroadcastAsync(
        ReloadMessage payload,
        CancellationToken cancellationToken)
        => await BroadcastAsync(_ => payload, cancellationToken);

    private async Task BroadcastAsync(
        Func<ReloadClient, ReloadMessage> payloadFactory,
        CancellationToken cancellationToken)
    {
        await PruneExpiredClientsAsync(DateTimeOffset.UtcNow);
        var broadcastTasks = new List<Task>(_sockets.Count);
        foreach (var entry in _sockets.ToArray())
        {
            var socket = entry.Key;
            var client = entry.Value;
            if (socket.State != WebSocketState.Open)
            {
                RemoveSocket(socket);
                continue;
            }

            broadcastTasks.Add(SendToClientAsync(socket, client, payloadFactory(client), cancellationToken));
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
        ReloadClient client,
        ReloadMessage payload,
        CancellationToken cancellationToken)
    {
        try
        {
            await SendWithGateAsync(socket, client, payload, cancellationToken);
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

    private async Task SendWithGateAsync(
        WebSocket socket,
        ReloadClient client,
        ReloadMessage payload,
        CancellationToken cancellationToken)
    {
        using var sendTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        sendTokenSource.CancelAfter(_sendTimeout);
        await client.SendGate.WaitAsync(sendTokenSource.Token);
        try
        {
            await SendAsync(socket, payload, sendTokenSource.Token);
        }
        finally
        {
            client.SendGate.Release();
        }
    }

    private static async Task SendAsync(
        WebSocket socket,
        ReloadMessage payload,
        CancellationToken cancellationToken)
    {
        var message = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
            payload,
            ReloadJsonSerializerContext.Default.ReloadMessage));
        await socket.SendAsync(
            message,
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken);
    }

    private static async Task<SocketMessage> ReceiveMessageAsync(
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
                return new SocketMessage(result.MessageType, null);

            totalMessageBytes += result.Count;
            if (totalMessageBytes > maxIncomingMessageBytes)
            {
                throw new SocketMessageTooLargeException(
                    $"Jazor reload client message exceeds the {maxIncomingMessageBytes} byte limit.");
            }

            if (result.MessageType == WebSocketMessageType.Text)
                stream.Write(buffer, 0, result.Count);

            if (result.EndOfMessage)
            {
                return new SocketMessage(
                    result.MessageType,
                    result.MessageType == WebSocketMessageType.Text
                        ? Encoding.UTF8.GetString(stream.ToArray())
                        : null);
            }
        }
    }

    private static void ProcessClientMessage(ReloadClient client, string text)
    {
        ReloadClientMessage? message;
        try
        {
            message = JsonSerializer.Deserialize(
                text,
                ReloadJsonSerializerContext.Default.ReloadClientMessage);
        }
        catch (JsonException)
        {
            return;
        }

        if (message?.Type is null)
            return;

        if (string.Equals(message.Type, "ready", StringComparison.OrdinalIgnoreCase))
        {
            client.MarkReady(message.Capabilities);
            client.LastSeenUtc = DateTimeOffset.UtcNow;
            return;
        }

        if (string.Equals(message.Type, "heartbeat", StringComparison.OrdinalIgnoreCase))
        {
            client.LastSeenUtc = DateTimeOffset.UtcNow;
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
        string closeDescription = "Jazor reload shutdown",
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
            "Jazor reload shutdown",
            cancellationToken);
}

/// <summary>Outbound JSON envelope for the stable browser reload protocol.</summary>
internal sealed class ReloadMessage
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

    [JsonPropertyName("changedPaths")]
    public IReadOnlyList<string>? ChangedPaths { get; init; }

    [JsonPropertyName("moduleUpdates")]
    public IReadOnlyList<ReloadModuleUpdate>? ModuleUpdates { get; init; }
}

/// <summary>One capability-gated Vue module update carried by <see cref="ReloadMessage"/>.</summary>
internal sealed class ReloadModuleUpdate
{
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("componentId")]
    public required string ComponentId { get; init; }

    [JsonPropertyName("moduleId")]
    public required string ModuleId { get; init; }

    [JsonPropertyName("descriptorHash")]
    public required string DescriptorHash { get; init; }

    [JsonPropertyName("templateHash")]
    public required string TemplateHash { get; init; }

    [JsonPropertyName("logicHash")]
    public required string LogicHash { get; init; }

    [JsonPropertyName("boundaryKind")]
    public required string BoundaryKind { get; init; }
}

/// <summary>Inbound JSON envelope sent by the browser reload client.</summary>
internal sealed class ReloadClientMessage
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("capabilities")]
    public string[]? Capabilities { get; init; }
}

/// <summary>Tracks one connected browser and the protocol capabilities it has acknowledged.</summary>
internal sealed class ReloadClient(string clientId) : IDisposable
{
    private string[] _capabilities = [];
    private int _isReady;

    public string ClientId { get; } = clientId;

    public DateTimeOffset LastSeenUtc { get; set; } = DateTimeOffset.UtcNow;

    public SemaphoreSlim SendGate { get; } = new(1, 1);

    public void MarkReady(IEnumerable<string>? capabilities)
    {
        var normalizedCapabilities = capabilities?
            .Where(static capability => !string.IsNullOrWhiteSpace(capability))
            .Select(static capability => capability.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray()
            ?? [];
        Volatile.Write(ref _capabilities, normalizedCapabilities);
        Volatile.Write(ref _isReady, 1);
    }

    public bool SupportsCapability(string capability)
        => Volatile.Read(ref _isReady) != 0
            && Array.IndexOf(Volatile.Read(ref _capabilities), capability) >= 0;

    public void Dispose()
        => SendGate.Dispose();
}

/// <summary>Represents one fully assembled WebSocket message before JSON protocol processing.</summary>
internal readonly record struct SocketMessage(
    WebSocketMessageType MessageType,
    string? Text);

/// <summary>Signals that a client exceeded the fixed inbound WebSocket message limit.</summary>
internal sealed class SocketMessageTooLargeException(string message) : InvalidOperationException(message);
