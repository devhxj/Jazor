using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.WebSockets;
using System.Text;
using Jolt.Debug;

namespace Jolt.Test;

[TestClass]
public sealed class JoltDebugCdpClientTests
{
    [TestMethod]
    public void CdpClient_ParsePausedCallFrames_ReadsFrameUrlAndLocation()
    {
        using var parameters = JsonDocument.Parse(
            """
            {
              "callFrames": [
                {
                  "callFrameId": "frame-1",
                  "functionName": "render",
                  "url": "http://localhost:5173/src/App.vue",
                  "location": {
                    "lineNumber": 12,
                    "columnNumber": 4
                  }
                }
              ]
            }
            """);

        var frames = CdpClient.ParsePausedCallFrames(parameters.RootElement);

        Assert.AreEqual(1, frames.Count);
        Assert.AreEqual("frame-1", frames[0].CallFrameId);
        Assert.AreEqual("render", frames[0].FunctionName);
        Assert.AreEqual("http://localhost:5173/src/App.vue", frames[0].Location.Url);
        Assert.AreEqual(12, frames[0].Location.LineNumber);
        Assert.AreEqual(4, frames[0].Location.ColumnNumber);
    }

    [TestMethod]
    public void CdpClient_ParsePausedCallFrames_RetainsScopeChainEntries()
    {
        using var parameters = JsonDocument.Parse(
            """
            {
              "callFrames": [
                {
                  "callFrameId": "frame-1",
                  "functionName": "render",
                  "url": "http://localhost:5173/src/App.vue",
                  "location": {
                    "lineNumber": 12,
                    "columnNumber": 4
                  },
                  "scopeChain": [
                    {
                      "type": "local",
                      "object": {
                        "type": "object",
                        "description": "Local",
                        "objectId": "scope-local-1"
                      }
                    },
                    {
                      "type": "closure",
                      "name": "setup",
                      "object": {
                        "type": "object",
                        "description": "Closure",
                        "objectId": "scope-closure-1"
                      }
                    }
                  ]
                }
              ]
            }
            """);

        var frames = CdpClient.ParsePausedCallFrames(parameters.RootElement);

        Assert.AreEqual(1, frames.Count);
        var scopeChain = frames[0].ScopeChain;
        Assert.IsNotNull(scopeChain);
        Assert.AreEqual(2, scopeChain.Count);
        Assert.AreEqual("local", scopeChain[0].Type);
        Assert.AreEqual("scope-local-1", scopeChain[0].Object.ObjectId);
        Assert.AreEqual("closure", scopeChain[1].Type);
        Assert.AreEqual("setup", scopeChain[1].Name);
        Assert.AreEqual("scope-closure-1", scopeChain[1].Object.ObjectId);
    }

    [TestMethod]
    public void CdpClient_ParseRemoteObject_PrefersTypedValue()
    {
        using var remoteObject = JsonDocument.Parse(
            """
            {
              "type": "number",
              "description": "42",
              "value": 42
            }
            """);

        var parsed = CdpClient.ParseRemoteObject(remoteObject.RootElement);

        Assert.AreEqual("number", parsed.Type);
        Assert.AreEqual("42", parsed.Value);
        Assert.AreEqual("42", parsed.Description);
    }

    [TestMethod]
    public void CdpClient_ParsePropertyDescriptors_ReadsExpandableObjectProperties()
    {
        using var properties = JsonDocument.Parse(
            """
            [
              {
                "name": "count",
                "value": {
                  "type": "number",
                  "description": "3",
                  "value": 3
                }
              },
              {
                "name": "model",
                "value": {
                  "type": "object",
                  "description": "Object",
                  "objectId": "remote-model-1"
                }
              }
            ]
            """);

        var parsed = CdpClient.ParsePropertyDescriptors(properties.RootElement);

        Assert.AreEqual(2, parsed.Count);
        Assert.AreEqual("count", parsed[0].Name);
        Assert.AreEqual("3", parsed[0].Value.Value);
        Assert.AreEqual("model", parsed[1].Name);
        Assert.AreEqual("remote-model-1", parsed[1].Value.ObjectId);
    }

    [TestMethod]
    public void CdpClient_BuildBreakpointUrlRegex_MatchesRelativePathSuffixWithQuery()
    {
        var regex = CdpClient.BuildBreakpointUrlRegex("/main.ts");

        Assert.IsNotNull(regex);
        Assert.IsTrue(Regex.IsMatch("http://localhost:5173/main.ts?t=1", regex));
        Assert.IsTrue(Regex.IsMatch("http://localhost:5173/main.ts#hash", regex));
        Assert.IsFalse(Regex.IsMatch("http://localhost:5173/main.tsx?t=1", regex));
    }

    [TestMethod]
    public void CdpClient_BuildBreakpointUrlRegex_LeavesAbsoluteHttpUrlOnExactMatchPath()
    {
        var regex = CdpClient.BuildBreakpointUrlRegex("http://localhost:5173/main.ts");

        Assert.IsNull(regex);
    }

    [TestMethod]
    public void CdpClient_BuildBreakpointUrlRegex_NormalizesWindowsSeparators()
    {
        var regex = CdpClient.BuildBreakpointUrlRegex(@"src\pages\main.ts");

        Assert.IsNotNull(regex);
        StringAssert.Contains(regex, "src/pages/main\\.ts");
        Assert.IsTrue(Regex.IsMatch("http://localhost:5173/src/pages/main.ts?import", regex));
    }

    [TestMethod]
    public void CdpClient_TryParseScriptParsed_ParsesScriptIdAndUrl()
    {
        using var message = JsonDocument.Parse(
            """
            {
              "method": "Debugger.scriptParsed",
              "params": {
                "scriptId": "16",
                "url": "http://127.0.0.1:5173/main.ts?t=1710000000"
              }
            }
            """);

        var parsed = CdpClient.TryParseScriptParsed(
            message.RootElement,
            out var scriptId,
            out var scriptUrl);

        Assert.IsTrue(parsed);
        Assert.AreEqual("16", scriptId);
        Assert.AreEqual("http://127.0.0.1:5173/main.ts?t=1710000000", scriptUrl);
    }

    [TestMethod]
    public void CdpClient_TryParseScriptParsed_ReturnsFalseWhenUrlMissing()
    {
        using var message = JsonDocument.Parse(
            """
            {
              "method": "Debugger.scriptParsed",
              "params": {
                "scriptId": "16"
              }
            }
            """);

        var parsed = CdpClient.TryParseScriptParsed(
            message.RootElement,
            out var scriptId,
            out var scriptUrl);

        Assert.IsFalse(parsed);
        Assert.AreEqual(string.Empty, scriptId);
        Assert.AreEqual(string.Empty, scriptUrl);
    }

    [TestMethod]
    public void CdpClient_ResolveScriptUrls_RewritesScriptIdLocationWithKnownScriptUrl()
    {
        var frames = new[]
        {
            new CdpCallFrame(
                "frame-1",
                "runIteration",
                new CdpLocation("16", 8, 18))
        };
        var mappings = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["16"] = "http://127.0.0.1:5173/main.ts?t=1710000000"
        };

        var rewrittenFrames = CdpClient.ResolveScriptUrls(frames, mappings);

        Assert.AreEqual(1, rewrittenFrames.Count);
        Assert.AreEqual("http://127.0.0.1:5173/main.ts?t=1710000000", rewrittenFrames[0].Location.Url);
        Assert.AreEqual(8, rewrittenFrames[0].Location.LineNumber);
        Assert.AreEqual(18, rewrittenFrames[0].Location.ColumnNumber);
    }

    [TestMethod]
    public async Task CdpClient_ContinueAsync_WhenConnectionCloses_FailsPendingRequest()
    {
        var connection = new FakeCdpConnection();
        await using var client = new CdpClient(connection);
        await client.ConnectAsync(new Uri("ws://localhost:9222/devtools/page/1"), CancellationToken.None);

        var continueTask = client.ContinueAsync(CancellationToken.None);
        await connection.WaitForSendAsync();
        connection.CompleteReceive(messageJson: null);

        var exception = await Assert.ThrowsAsync<IOException>(
            async () => await continueTask.WaitAsync(TimeSpan.FromSeconds(1)));

        StringAssert.Contains(exception.Message, "closed", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public async Task CdpClient_ContinueAsync_WhenConnectionStalls_TimesOut()
    {
        var connection = new FakeCdpConnection();
        await using var client = new CdpClient(connection, TimeSpan.FromMilliseconds(50));
        await client.ConnectAsync(new Uri("ws://localhost:9222/devtools/page/1"), CancellationToken.None);

        var continueTask = client.ContinueAsync(CancellationToken.None);
        await connection.WaitForSendAsync();

        var exception = await Assert.ThrowsAsync<TimeoutException>(
            async () => await continueTask.WaitAsync(TimeSpan.FromSeconds(1)));

        StringAssert.Contains(exception.Message, "timed out", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public async Task CdpConnection_ReceiveAsync_WhenMessageExceedsConfiguredLimit_ThrowsIOException()
    {
        await using var connection = new CdpConnection(
            new FragmentedWebSocket(
                Encoding.UTF8.GetBytes("""{"id":1,"result":"payload"}"""),
                fragmentSize: 8),
            ownsWebSocket: true,
            maxMessageBytes: 16);

        var exception = await Assert.ThrowsAsync<IOException>(
            async () => await connection.ReceiveAsync(CancellationToken.None));

        StringAssert.Contains(exception.Message, "exceeds", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public async Task CdpConnection_ReceiveAsync_WhenFragmentedMessageWithinLimit_ReturnsPayload()
    {
        const string payload = """{"id":1,"result":{"ok":true}}""";
        await using var connection = new CdpConnection(
            new FragmentedWebSocket(
                Encoding.UTF8.GetBytes(payload),
                fragmentSize: 5),
            ownsWebSocket: true,
            maxMessageBytes: 1024);

        var received = await connection.ReceiveAsync(CancellationToken.None);

        Assert.AreEqual(payload, received);
    }

    private sealed class FakeCdpConnection : ICdpConnection
    {
        private readonly TaskCompletionSource<bool> _sendObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<string?> _nextReceive = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool IsConnected { get; private set; }

        public Task ConnectAsync(Uri endpoint, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsConnected = true;
            return Task.CompletedTask;
        }

        public Task SendAsync(string payloadJson, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _sendObserved.TrySetResult(true);
            return Task.CompletedTask;
        }

        public Task<string?> ReceiveAsync(CancellationToken cancellationToken)
            => _nextReceive.Task.WaitAsync(cancellationToken);

        public async Task WaitForSendAsync()
            => await _sendObserved.Task.WaitAsync(TimeSpan.FromSeconds(1));

        public void CompleteReceive(string? messageJson)
            => _nextReceive.TrySetResult(messageJson);

        public ValueTask DisposeAsync()
        {
            IsConnected = false;
            _nextReceive.TrySetCanceled();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FragmentedWebSocket : WebSocket
    {
        private readonly byte[] _payload;
        private readonly int _fragmentSize;
        private int _position;
        private WebSocketState _state = WebSocketState.Open;

        public FragmentedWebSocket(byte[] payload, int fragmentSize)
        {
            _payload = payload;
            _fragmentSize = fragmentSize;
        }

        public override WebSocketCloseStatus? CloseStatus => null;

        public override string? CloseStatusDescription => null;

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
            _state = WebSocketState.Closed;
            return Task.CompletedTask;
        }

        public override Task CloseOutputAsync(
            WebSocketCloseStatus closeStatus,
            string? statusDescription,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _state = WebSocketState.CloseSent;
            return Task.CompletedTask;
        }

        public override void Dispose()
            => _state = WebSocketState.Closed;

        public override Task<WebSocketReceiveResult> ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_position >= _payload.Length)
            {
                _state = WebSocketState.CloseReceived;
                return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, endOfMessage: true));
            }

            var bytesToCopy = Math.Min(Math.Min(buffer.Count, _fragmentSize), _payload.Length - _position);
            _payload.AsSpan(_position, bytesToCopy).CopyTo(buffer.AsSpan(0, bytesToCopy));
            _position += bytesToCopy;
            return Task.FromResult(
                new WebSocketReceiveResult(
                    bytesToCopy,
                    WebSocketMessageType.Text,
                    endOfMessage: _position >= _payload.Length));
        }

        public override Task SendAsync(
            ArraySegment<byte> buffer,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public override ValueTask SendAsync(
            ReadOnlyMemory<byte> buffer,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        }
    }
}
