using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Extensions;
using Jazor.VueHost.Jazor.Projection;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Aggregation;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.VirtualDocuments.Registry;
using Jazor.VueHost.Workspace;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostStdioLspServerTests
{
    [TestMethod]
    public async Task JazorVueHost_StdioLspServer_CancelRequest_CancelsInFlightWorkspaceSymbolRequest()
    {
        var blockingProvider = new BlockingWorkspaceSymbolProvider();
        var session = CreateSession(blockingProvider);
        var server = new StdioLspServer(session);

        var clientToServerPipe = new Pipe();
        var serverToClientPipe = new Pipe();
        await using var serverInput = clientToServerPipe.Reader.AsStream(leaveOpen: true);
        await using var serverOutput = serverToClientPipe.Writer.AsStream(leaveOpen: true);
        await using var clientInput = clientToServerPipe.Writer.AsStream(leaveOpen: true);
        await using var clientOutput = serverToClientPipe.Reader.AsStream(leaveOpen: true);

        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var serverTask = server.RunAsync(serverInput, serverOutput, cancellationSource.Token).AsTask();

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new { }
            },
            cancellationSource.Token);
        using var initializeResponse = await ReadResponseAsync(clientOutput, expectedId: 1, cancellationSource.Token);
        Assert.IsTrue(initializeResponse.RootElement.TryGetProperty("result", out _));

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 2,
                method = "workspace/symbol",
                @params = new
                {
                    query = "UserCard"
                }
            },
            cancellationSource.Token);
        await blockingProvider.WaitUntilStartedAsync(cancellationSource.Token);

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                method = "$/cancelRequest",
                @params = new
                {
                    id = 2
                }
            },
            cancellationSource.Token);

        using var cancelledResponse = await ReadResponseAsync(clientOutput, expectedId: 2, cancellationSource.Token);
        var cancellationError = cancelledResponse.RootElement.GetProperty("error");
        Assert.AreEqual(-32800, cancellationError.GetProperty("code").GetInt32());
        Assert.AreEqual("Request cancelled.", cancellationError.GetProperty("message").GetString());

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 3,
                method = "shutdown",
                @params = new { }
            },
            cancellationSource.Token);
        using var shutdownResponse = await ReadResponseAsync(clientOutput, expectedId: 3, cancellationSource.Token);
        Assert.AreEqual(JsonValueKind.Null, shutdownResponse.RootElement.GetProperty("result").ValueKind);

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                method = "exit",
                @params = new { }
            },
            cancellationSource.Token);
        await clientInput.DisposeAsync();

        await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [TestMethod]
    public async Task JazorVueHost_StdioLspServer_CancelRequest_CancelsQueuedRequestBeforeExecution()
    {
        var provider = new SequencedWorkspaceSymbolProvider();
        var session = CreateSession(provider);
        var server = new StdioLspServer(session);

        var clientToServerPipe = new Pipe();
        var serverToClientPipe = new Pipe();
        await using var serverInput = clientToServerPipe.Reader.AsStream(leaveOpen: true);
        await using var serverOutput = serverToClientPipe.Writer.AsStream(leaveOpen: true);
        await using var clientInput = clientToServerPipe.Writer.AsStream(leaveOpen: true);
        await using var clientOutput = serverToClientPipe.Reader.AsStream(leaveOpen: true);

        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var serverTask = server.RunAsync(serverInput, serverOutput, cancellationSource.Token).AsTask();

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new { }
            },
            cancellationSource.Token);
        using var initializeResponse = await ReadResponseAsync(clientOutput, expectedId: 1, cancellationSource.Token);
        Assert.IsTrue(initializeResponse.RootElement.TryGetProperty("result", out _));

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 2,
                method = "workspace/symbol",
                @params = new
                {
                    query = "first"
                }
            },
            cancellationSource.Token);
        await provider.WaitUntilFirstStartedAsync(cancellationSource.Token);

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 3,
                method = "workspace/symbol",
                @params = new
                {
                    query = "second"
                }
            },
            cancellationSource.Token);
        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                method = "$/cancelRequest",
                @params = new
                {
                    id = 3
                }
            },
            cancellationSource.Token);

        provider.ReleaseFirstRequest();

        using var firstResponse = await ReadResponseAsync(clientOutput, expectedId: 2, cancellationSource.Token);
        Assert.IsTrue(firstResponse.RootElement.TryGetProperty("result", out var firstResult));
        Assert.AreEqual(JsonValueKind.Array, firstResult.ValueKind);

        using var queuedCancelledResponse = await ReadResponseAsync(clientOutput, expectedId: 3, cancellationSource.Token);
        var cancellationError = queuedCancelledResponse.RootElement.GetProperty("error");
        Assert.AreEqual(-32800, cancellationError.GetProperty("code").GetInt32());
        Assert.AreEqual("Request cancelled.", cancellationError.GetProperty("message").GetString());
        Assert.AreEqual(1, provider.InvocationCount, "Queued request should be cancelled before provider execution.");

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 4,
                method = "shutdown",
                @params = new { }
            },
            cancellationSource.Token);
        using var shutdownResponse = await ReadResponseAsync(clientOutput, expectedId: 4, cancellationSource.Token);
        Assert.AreEqual(JsonValueKind.Null, shutdownResponse.RootElement.GetProperty("result").ValueKind);

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                method = "exit",
                @params = new { }
            },
            cancellationSource.Token);
        await clientInput.DisposeAsync();

        await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [TestMethod]
    public async Task JazorVueHost_StdioLspServer_ShutdownExit_CompletesWithoutExternalCancellation()
    {
        var session = CreateSession(new NoOpWorkspaceSymbolProvider());
        var server = new StdioLspServer(session);

        var clientToServerPipe = new Pipe();
        var serverToClientPipe = new Pipe();
        await using var serverInput = clientToServerPipe.Reader.AsStream(leaveOpen: true);
        await using var serverOutput = serverToClientPipe.Writer.AsStream(leaveOpen: true);
        await using var clientInput = clientToServerPipe.Writer.AsStream(leaveOpen: true);
        await using var clientOutput = serverToClientPipe.Reader.AsStream(leaveOpen: true);

        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var serverTask = server.RunAsync(serverInput, serverOutput, cancellationSource.Token).AsTask();

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new { }
            },
            cancellationSource.Token);
        using var initializeResponse = await ReadResponseAsync(clientOutput, expectedId: 1, cancellationSource.Token);
        Assert.IsTrue(initializeResponse.RootElement.TryGetProperty("result", out _));

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 2,
                method = "shutdown",
                @params = new { }
            },
            cancellationSource.Token);
        using var shutdownResponse = await ReadResponseAsync(clientOutput, expectedId: 2, cancellationSource.Token);
        Assert.AreEqual(JsonValueKind.Null, shutdownResponse.RootElement.GetProperty("result").ValueKind);

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                method = "exit",
                @params = new { }
            },
            cancellationSource.Token);
        await clientInput.DisposeAsync();

        await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsTrue(serverTask.IsCompletedSuccessfully);
    }

    private static LspSession CreateSession(ILspWorkspaceSymbolProvider workspaceSymbolProvider)
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var laneRouter = new LspLaneRouter();
        var projectionResolver = new DocumentProjectionResolver(
            new DocumentRegionClassifier(),
            virtualDocumentRegistry);
        var projectionService = new JazorProjectionService();
        var resultAggregator = new LspResultAggregator();
        var markupBridgeService = new MarkupComponentBridgeService(workspaceStore);
        var markupBridgeFanout = new MarkupBridgeFanoutCoordinator(markupBridgeService, resultAggregator);
        var lanes = Array.Empty<ILspLane>();
        var extensionRegistry = new ExtensionRegistry();
        extensionRegistry.RegisterLspWorkspaceSymbolProvider(workspaceSymbolProvider);

        return new LspSession(
            workspaceStore,
            lanes,
            laneRouter,
            new LspMessageWriter(new MemoryStream()),
            projectionService,
            virtualDocumentRegistry,
            projectionResolver,
            resultAggregator,
            markupBridgeFanout,
            new ReferenceCoordinator(new Dictionary<LaneKind, ILspLane>(), laneRouter, markupBridgeFanout),
            new RenameCoordinator(new Dictionary<LaneKind, ILspLane>(), laneRouter, resultAggregator, markupBridgeFanout),
            new CodeActionCoordinator(new Dictionary<LaneKind, ILspLane>(), laneRouter, resultAggregator),
            extensionRegistry: extensionRegistry,
            extensionProviderTimeout: TimeSpan.FromSeconds(30));
    }

    private static async Task SendMessageAsync(
        Stream stream,
        object payload,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload);
        var body = Encoding.UTF8.GetBytes(json);
        var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
        await stream.WriteAsync(header, cancellationToken);
        await stream.WriteAsync(body, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    private static async Task<JsonDocument> ReadResponseAsync(
        Stream stream,
        int expectedId,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var message = await ReadMessageAsync(stream, cancellationToken);
            if (TryGetMessageId(message.RootElement, out var actualId) && actualId == expectedId)
            {
                return message;
            }

            message.Dispose();
        }
    }

    private static async Task<JsonDocument> ReadMessageAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var contentLength = await ReadContentLengthAsync(stream, cancellationToken);
        var body = new byte[contentLength];
        var offset = 0;
        while (offset < body.Length)
        {
            var read = await stream.ReadAsync(body.AsMemory(offset, body.Length - offset), cancellationToken);
            if (read == 0)
            {
                throw new EndOfStreamException("Unexpected EOF while reading LSP message body.");
            }

            offset += read;
        }

        return JsonDocument.Parse(body);
    }

    private static async Task<int> ReadContentLengthAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var headerBytes = new List<byte>();
        var buffer = new byte[1];
        while (true)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);
            if (read == 0)
            {
                throw new EndOfStreamException("Unexpected EOF while reading LSP message headers.");
            }

            headerBytes.Add(buffer[0]);
            var count = headerBytes.Count;
            if (count >= 4
                && headerBytes[count - 4] == '\r'
                && headerBytes[count - 3] == '\n'
                && headerBytes[count - 2] == '\r'
                && headerBytes[count - 1] == '\n')
            {
                break;
            }
        }

        var headerText = Encoding.ASCII.GetString(headerBytes.ToArray());
        foreach (var line in headerText.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries))
        {
            if (!line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return int.Parse(
                line["Content-Length:".Length..].Trim(),
                System.Globalization.CultureInfo.InvariantCulture);
        }

        throw new InvalidOperationException("Missing Content-Length header.");
    }

    private static bool TryGetMessageId(JsonElement element, out int messageId)
    {
        if (element.TryGetProperty("id", out var idProperty))
        {
            switch (idProperty.ValueKind)
            {
                case JsonValueKind.Number when idProperty.TryGetInt32(out messageId):
                    return true;
                case JsonValueKind.String when int.TryParse(idProperty.GetString(), out messageId):
                    return true;
            }
        }

        messageId = default;
        return false;
    }

    private sealed class BlockingWorkspaceSymbolProvider : ILspWorkspaceSymbolProvider
    {
        private readonly TaskCompletionSource<bool> _started = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public string Name => "BlockingWorkspaceSymbolProvider";

        public int Priority => 100;

        public async ValueTask<IReadOnlyList<LspWorkspaceSymbol>> ProvideWorkspaceSymbolsAsync(
            LspWorkspaceSymbolProviderContext context,
            CancellationToken cancellationToken)
        {
            _started.TrySetResult(true);
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            return Array.Empty<LspWorkspaceSymbol>();
        }

        public async Task WaitUntilStartedAsync(CancellationToken cancellationToken)
            => await _started.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
    }

    private sealed class SequencedWorkspaceSymbolProvider : ILspWorkspaceSymbolProvider
    {
        private readonly TaskCompletionSource<bool> _firstStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _firstRelease = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _invocationCount;

        public string Name => "SequencedWorkspaceSymbolProvider";

        public int Priority => 100;

        public int InvocationCount => Volatile.Read(ref _invocationCount);

        public async ValueTask<IReadOnlyList<LspWorkspaceSymbol>> ProvideWorkspaceSymbolsAsync(
            LspWorkspaceSymbolProviderContext context,
            CancellationToken cancellationToken)
        {
            var invocationIndex = Interlocked.Increment(ref _invocationCount);
            if (invocationIndex == 1)
            {
                _firstStarted.TrySetResult(true);
                await _firstRelease.Task.WaitAsync(cancellationToken);
            }

            return Array.Empty<LspWorkspaceSymbol>();
        }

        public async Task WaitUntilFirstStartedAsync(CancellationToken cancellationToken)
            => await _firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        public void ReleaseFirstRequest()
            => _firstRelease.TrySetResult(true);
    }

    private sealed class NoOpWorkspaceSymbolProvider : ILspWorkspaceSymbolProvider
    {
        public string Name => "NoOpWorkspaceSymbolProvider";

        public int Priority => 0;

        public ValueTask<IReadOnlyList<LspWorkspaceSymbol>> ProvideWorkspaceSymbolsAsync(
            LspWorkspaceSymbolProviderContext context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspWorkspaceSymbol>>(Array.Empty<LspWorkspaceSymbol>());
    }
}
