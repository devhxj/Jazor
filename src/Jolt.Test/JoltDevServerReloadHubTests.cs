using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Jolt.DevServer;

namespace Jolt.Test;

[TestClass]
public sealed class JoltDevServerReloadHubTests
{
    [TestMethod]
    public async Task DevServerReloadHub_BroadcastReload_WhenClientBackpressures_KeepsOtherClientsResponsive()
    {
        await using var hub = new DevServerReloadHub(TimeSpan.FromMilliseconds(120));
        var fastClient = new ControlledWebSocket();
        var slowClient = new ControlledWebSocket(blockAfterSendCount: 1);
        using var cancellationSource = new CancellationTokenSource();

        var fastLoop = hub.AcceptAsync(fastClient, cancellationSource.Token);
        var slowLoop = hub.AcceptAsync(slowClient, cancellationSource.Token);

        await WaitUntilAsync(
            () => hub.ConnectedClientCount == 2
                && fastClient.SentMessages.Count >= 1
                && slowClient.SentMessages.Count >= 1,
            TimeSpan.FromSeconds(2));

        var stopwatch = Stopwatch.StartNew();
        await hub.BroadcastReloadAsync("backpressure", CancellationToken.None);
        stopwatch.Stop();

        Assert.IsTrue(slowClient.BlockedSendStarted, "Expected slow client send to enter backpressure state.");
        Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(1), "Broadcast should not stall on a slow client.");
        await WaitUntilAsync(() => hub.ConnectedClientCount == 1, TimeSpan.FromSeconds(2));

        Assert.AreEqual(2, fastClient.SentMessages.Count);
        StringAssert.Contains(fastClient.SentMessages[1], "\"type\":\"full-reload\"");
        Assert.AreEqual(WebSocketState.Closed, slowClient.State);

        cancellationSource.Cancel();
        await IgnoreCancellationAsync(fastLoop);
        await IgnoreCancellationAsync(slowLoop);
    }

    [TestMethod]
    public async Task DevServerReloadHub_BroadcastError_WhenBroadcastsRace_SerializesPerClientSends()
    {
        await using var hub = new DevServerReloadHub(TimeSpan.FromMilliseconds(300));
        var client = new ControlledWebSocket(failOnConcurrentSend: true);
        using var cancellationSource = new CancellationTokenSource();

        var receiveLoop = hub.AcceptAsync(client, cancellationSource.Token);
        await WaitUntilAsync(
            () => hub.ConnectedClientCount == 1 && client.SentMessages.Count >= 1,
            TimeSpan.FromSeconds(2));

        var broadcastTasks = Enumerable.Range(0, 24)
            .Select(index => hub.BroadcastErrorAsync($"E-{index}", CancellationToken.None))
            .ToArray();
        await Task.WhenAll(broadcastTasks);

        Assert.AreEqual(1, hub.ConnectedClientCount, "Client should stay connected when sends are serialized.");
        Assert.AreEqual(25, client.SentMessages.Count, "Expected one connected event plus all broadcast events.");

        var seenMessages = new HashSet<string>(StringComparer.Ordinal);
        foreach (var json in client.SentMessages.Skip(1))
        {
            using var document = JsonDocument.Parse(json);
            Assert.AreEqual("error", document.RootElement.GetProperty("type").GetString());
            seenMessages.Add(document.RootElement.GetProperty("message").GetString() ?? string.Empty);
        }

        Assert.AreEqual(24, seenMessages.Count);

        cancellationSource.Cancel();
        await IgnoreCancellationAsync(receiveLoop);
    }

    [TestMethod]
    public async Task DevServerReloadHub_HeartbeatSweep_RemovesExpiredClients()
    {
        await using var hub = new DevServerReloadHub(
            sendTimeout: TimeSpan.FromMilliseconds(300),
            heartbeatTimeout: TimeSpan.FromMilliseconds(40),
            heartbeatSweepInterval: TimeSpan.FromMilliseconds(10));
        var client = new ControlledWebSocket();
        using var cancellationSource = new CancellationTokenSource();

        var receiveLoop = hub.AcceptAsync(client, cancellationSource.Token);
        await WaitUntilAsync(
            () => hub.ConnectedClientCount == 1 && client.SentMessages.Count >= 1,
            TimeSpan.FromSeconds(2));

        await WaitUntilAsync(
            () => hub.ConnectedClientCount == 0 && client.State == WebSocketState.Closed,
            TimeSpan.FromSeconds(2));

        cancellationSource.Cancel();
        await IgnoreCancellationAsync(receiveLoop);
    }

    private static async Task IgnoreCancellationAsync(Task task)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (!condition())
        {
            if (DateTime.UtcNow >= deadline)
            {
                Assert.Fail("Condition was not satisfied within timeout.");
            }

            await Task.Delay(10);
        }
    }

    private sealed class ControlledWebSocket : WebSocket
    {
        private readonly object _sentMessagesLock = new();
        private readonly List<string> _sentMessages = [];
        private readonly int _blockAfterSendCount;
        private readonly bool _failOnConcurrentSend;

        private int _activeSenders;
        private int _sendCount;
        private WebSocketState _state = WebSocketState.Open;
        private WebSocketCloseStatus? _closeStatus;
        private string? _closeStatusDescription;

        public ControlledWebSocket(int blockAfterSendCount = int.MaxValue, bool failOnConcurrentSend = false)
        {
            _blockAfterSendCount = blockAfterSendCount;
            _failOnConcurrentSend = failOnConcurrentSend;
        }

        public bool BlockedSendStarted { get; private set; }

        public IReadOnlyList<string> SentMessages
        {
            get
            {
                lock (_sentMessagesLock)
                {
                    return _sentMessages.ToArray();
                }
            }
        }

        public override WebSocketCloseStatus? CloseStatus => _closeStatus;

        public override string? CloseStatusDescription => _closeStatusDescription;

        public override WebSocketState State => _state;

        public override string? SubProtocol => null;

        public override void Abort()
            => _state = WebSocketState.Aborted;

        public override Task CloseAsync(
            WebSocketCloseStatus closeStatus,
            string? statusDescription,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _closeStatus = closeStatus;
            _closeStatusDescription = statusDescription;
            _state = WebSocketState.Closed;
            return Task.CompletedTask;
        }

        public override Task CloseOutputAsync(
            WebSocketCloseStatus closeStatus,
            string? statusDescription,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _closeStatus = closeStatus;
            _closeStatusDescription = statusDescription;
            _state = WebSocketState.CloseSent;
            return Task.CompletedTask;
        }

        public override void Dispose()
            => _state = WebSocketState.Closed;

        public override async Task<WebSocketReceiveResult> ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Delay should always be canceled.");
        }

        public override Task SendAsync(
            ArraySegment<byte> buffer,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken)
            => SendCoreAsync(buffer.AsMemory(), cancellationToken);

        public override ValueTask SendAsync(
            ReadOnlyMemory<byte> buffer,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken)
            => new(SendCoreAsync(buffer, cancellationToken));

        private async Task SendCoreAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_state != WebSocketState.Open)
            {
                throw new WebSocketException("Socket is not open.");
            }

            if (_failOnConcurrentSend && Interlocked.Increment(ref _activeSenders) > 1)
            {
                Interlocked.Decrement(ref _activeSenders);
                throw new InvalidOperationException("Concurrent send detected.");
            }

            try
            {
                var sendCount = Interlocked.Increment(ref _sendCount);
                if (sendCount > _blockAfterSendCount)
                {
                    BlockedSendStarted = true;
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                }

                lock (_sentMessagesLock)
                {
                    _sentMessages.Add(Encoding.UTF8.GetString(buffer.Span));
                }
            }
            finally
            {
                if (_failOnConcurrentSend)
                {
                    Interlocked.Decrement(ref _activeSenders);
                }
            }
        }
    }
}
