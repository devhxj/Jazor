using System.IO.Pipelines;
using System.Reflection;
using System.Text;
using System.Text.Json;
using ECMAScript.Internal.VueContracts.Protocol;
using Jolt.Extensions;
using Jolt.Jazor.Projection;
using Jolt.Lsp;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;

namespace Jolt.Test;

[TestClass]
public sealed class JoltStdioLspServerTests
{
    [TestMethod]
    public async Task Jolt_StdioLspServer_CancelRequest_CancelsInFlightWorkspaceSymbolRequest()
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

        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
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
    public async Task Jolt_StdioLspServer_CancelRequest_CancelsQueuedRequestBeforeExecution()
    {
        var provider = new SequencedWorkspaceSymbolProvider();
        var session = CreateSession(provider);
        var server = new StdioLspServer(session, maxConcurrentRequests: 1);

        var clientToServerPipe = new Pipe();
        var serverToClientPipe = new Pipe();
        await using var serverInput = clientToServerPipe.Reader.AsStream(leaveOpen: true);
        await using var serverOutput = serverToClientPipe.Writer.AsStream(leaveOpen: true);
        await using var clientInput = clientToServerPipe.Writer.AsStream(leaveOpen: true);
        await using var clientOutput = serverToClientPipe.Reader.AsStream(leaveOpen: true);

        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
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
        await WaitUntilTrackedRequestCountAsync(server, expectedCount: 2, cancellationSource.Token);
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
        await WaitUntilRequestCancellationObservedAsync(server, requestId: 3, cancellationSource.Token);

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
    public async Task Jolt_StdioLspServer_WhenRequestQueueIsFull_ReturnsServerBusyError()
    {
        var provider = new SequencedWorkspaceSymbolProvider();
        var session = CreateSession(provider);
        var server = new StdioLspServer(session, maxConcurrentRequests: 1, maxQueuedRequests: 1);

        var clientToServerPipe = new Pipe();
        var serverToClientPipe = new Pipe();
        await using var serverInput = clientToServerPipe.Reader.AsStream(leaveOpen: true);
        await using var serverOutput = serverToClientPipe.Writer.AsStream(leaveOpen: true);
        await using var clientInput = clientToServerPipe.Writer.AsStream(leaveOpen: true);
        await using var clientOutput = serverToClientPipe.Reader.AsStream(leaveOpen: true);

        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
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
        await WaitUntilTrackedRequestCountAsync(server, expectedCount: 2, cancellationSource.Token);

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 4,
                method = "workspace/symbol",
                @params = new
                {
                    query = "third"
                }
            },
            cancellationSource.Token);

        using var rejectedResponse = await ReadResponseAsync(clientOutput, expectedId: 4, cancellationSource.Token);
        var error = rejectedResponse.RootElement.GetProperty("error");
        Assert.AreEqual(-32000, error.GetProperty("code").GetInt32());
        StringAssert.Contains(error.GetProperty("message").GetString() ?? string.Empty, "queue");

        provider.ReleaseFirstRequest();

        using var firstResponse = await ReadResponseAsync(clientOutput, expectedId: 2, cancellationSource.Token);
        Assert.AreEqual(JsonValueKind.Array, firstResponse.RootElement.GetProperty("result").ValueKind);

        using var secondResponse = await ReadResponseAsync(clientOutput, expectedId: 3, cancellationSource.Token);
        Assert.AreEqual(JsonValueKind.Array, secondResponse.RootElement.GetProperty("result").ValueKind);
        Assert.AreEqual(2, provider.InvocationCount);

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 5,
                method = "shutdown",
                @params = new { }
            },
            cancellationSource.Token);
        using var shutdownResponse = await ReadResponseAsync(clientOutput, expectedId: 5, cancellationSource.Token);
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
    public async Task Jolt_StdioLspServer_WorkspaceSymbolRequests_RunConcurrentlyWhenCapacityAllows()
    {
        var provider = new ConcurrentWorkspaceSymbolProvider(expectedConcurrentInvocations: 2);
        var session = CreateSession(provider);
        var server = new StdioLspServer(session, maxConcurrentRequests: 2);

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

        await provider.WaitUntilConcurrentAsync(cancellationSource.Token);
        provider.ReleaseAll();

        using var firstObservedResponse = await ReadMessageAsync(clientOutput, cancellationSource.Token);
        using var secondObservedResponse = await ReadMessageAsync(clientOutput, cancellationSource.Token);
        Assert.IsTrue(TryGetMessageId(firstObservedResponse.RootElement, out var firstObservedId));
        Assert.IsTrue(TryGetMessageId(secondObservedResponse.RootElement, out var secondObservedId));
        CollectionAssert.AreEquivalent(new[] { 2, 3 }, new[] { firstObservedId, secondObservedId });
        Assert.AreEqual(JsonValueKind.Array, firstObservedResponse.RootElement.GetProperty("result").ValueKind);
        Assert.AreEqual(JsonValueKind.Array, secondObservedResponse.RootElement.GetProperty("result").ValueKind);
        Assert.IsTrue(provider.MaxObservedConcurrency >= 2, "Expected both requests to overlap on the server.");

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
    public async Task Jolt_StdioLspServer_ShutdownExit_CompletesWithoutExternalCancellation()
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

    [TestMethod]
    public async Task Jolt_StdioLspServer_InvalidHeader_ShutsDownGracefully()
    {
        var session = CreateSession(new NoOpWorkspaceSymbolProvider());
        var server = new StdioLspServer(session);

        var clientToServerPipe = new Pipe();
        var serverToClientPipe = new Pipe();
        await using var serverInput = clientToServerPipe.Reader.AsStream(leaveOpen: true);
        await using var serverOutput = serverToClientPipe.Writer.AsStream(leaveOpen: true);
        await using var clientInput = clientToServerPipe.Writer.AsStream(leaveOpen: true);

        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var serverTask = server.RunAsync(serverInput, serverOutput, cancellationSource.Token).AsTask();

        var malformedHeader = Encoding.ASCII.GetBytes("Content-Length: -1\r\n\r\n");
        await clientInput.WriteAsync(malformedHeader, cancellationSource.Token);
        await clientInput.DisposeAsync();

        await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsTrue(serverTask.IsCompletedSuccessfully);
    }

    [TestMethod]
    public async Task LspMessageReader_ReadMessageAsync_RejectsNegativeContentLength()
    {
        await using var input = new MemoryStream(Encoding.ASCII.GetBytes("Content-Length: -1\r\n\r\n"));
        var reader = new LspMessageReader(input);

        await Assert.ThrowsAsync<InvalidDataException>(
            async () => await reader.ReadMessageAsync(CancellationToken.None).AsTask());
    }

    [TestMethod]
    public async Task Jolt_StdioLspServer_InvalidParams_ReturnsInvalidParamsErrorAndKeepsServingRequests()
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
                method = "textDocument/hover",
                @params = new
                {
                    textDocument = new
                    {
                        uri = ""
                    },
                    position = new
                    {
                        line = 0,
                        character = 0
                    }
                }
            },
            cancellationSource.Token);
        using var invalidParamsResponse = await ReadResponseAsync(clientOutput, expectedId: 2, cancellationSource.Token);
        var error = invalidParamsResponse.RootElement.GetProperty("error");
        Assert.AreEqual(-32602, error.GetProperty("code").GetInt32());
        StringAssert.Contains(
            error.GetProperty("message").GetString() ?? string.Empty,
            "uri",
            StringComparison.OrdinalIgnoreCase);

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 3,
                method = "workspace/symbol",
                @params = new
                {
                    query = "any"
                }
            },
            cancellationSource.Token);
        using var healthyResponse = await ReadResponseAsync(clientOutput, expectedId: 3, cancellationSource.Token);
        Assert.AreEqual(JsonValueKind.Array, healthyResponse.RootElement.GetProperty("result").ValueKind);

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
    public async Task Jolt_StdioLspServer_WorkspaceFolderChanges_PropagatesUpdatedFoldersToProviderContext()
    {
        var provider = new WorkspaceFolderEchoSymbolProvider();
        var session = CreateSession(provider);
        var server = new StdioLspServer(session);

        var workspaceAPath = Path.Combine(Path.GetTempPath(), $"jazor-lsp-workspace-a-{Guid.NewGuid():N}");
        var workspaceBPath = Path.Combine(Path.GetTempPath(), $"jazor-lsp-workspace-b-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workspaceAPath);
        Directory.CreateDirectory(workspaceBPath);

        var workspaceAUri = new Uri(workspaceAPath).AbsoluteUri;
        var workspaceBUri = new Uri(workspaceBPath).AbsoluteUri;

        var clientToServerPipe = new Pipe();
        var serverToClientPipe = new Pipe();
        await using var serverInput = clientToServerPipe.Reader.AsStream(leaveOpen: true);
        await using var serverOutput = serverToClientPipe.Writer.AsStream(leaveOpen: true);
        await using var clientInput = clientToServerPipe.Writer.AsStream(leaveOpen: true);
        await using var clientOutput = serverToClientPipe.Reader.AsStream(leaveOpen: true);

        try
        {
            using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var serverTask = server.RunAsync(serverInput, serverOutput, cancellationSource.Token).AsTask();

            await SendMessageAsync(
                clientInput,
                new
                {
                    jsonrpc = "2.0",
                    id = 1,
                    method = "initialize",
                    @params = new
                    {
                        workspaceFolders = new[]
                        {
                            new
                            {
                                uri = workspaceAUri,
                                name = "workspace-a"
                            }
                        }
                    }
                },
                cancellationSource.Token);
            using var initializeResponse = await ReadResponseAsync(clientOutput, expectedId: 1, cancellationSource.Token);
            Assert.IsTrue(initializeResponse.RootElement.TryGetProperty("result", out _));

            await SendMessageAsync(
                clientInput,
                new
                {
                    jsonrpc = "2.0",
                    method = "workspace/didChangeWorkspaceFolders",
                    @params = new
                    {
                        @event = new
                        {
                            added = new[]
                            {
                                new
                                {
                                    uri = workspaceBUri,
                                    name = "workspace-b"
                                }
                            },
                            removed = new[]
                            {
                                new
                                {
                                    uri = workspaceAUri,
                                    name = "workspace-a"
                                }
                            }
                        }
                    }
                },
                cancellationSource.Token);

            await SendMessageAsync(
                clientInput,
                new
                {
                    jsonrpc = "2.0",
                    id = 2,
                    method = "workspace/symbol",
                    @params = new
                    {
                        query = "folder"
                    }
                },
                cancellationSource.Token);
            using var symbolResponse = await ReadResponseAsync(clientOutput, expectedId: 2, cancellationSource.Token);
            var symbols = symbolResponse.RootElement.GetProperty("result");
            Assert.AreEqual(JsonValueKind.Array, symbols.ValueKind);
            Assert.AreEqual(1, symbols.GetArrayLength());
            Assert.AreEqual("workspace-b", symbols[0].GetProperty("name").GetString());
            Assert.AreEqual(workspaceBUri, symbols[0].GetProperty("location").GetProperty("uri").GetString());

            var observedFolders = provider.GetLastWorkspaceFolders();
            Assert.AreEqual(1, observedFolders.Count);
            Assert.AreEqual(workspaceBUri, observedFolders[0].Uri);
            Assert.AreEqual("workspace-b", observedFolders[0].Name);

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
        finally
        {
            if (Directory.Exists(workspaceAPath))
            {
                Directory.Delete(workspaceAPath, recursive: true);
            }

            if (Directory.Exists(workspaceBPath))
            {
                Directory.Delete(workspaceBPath, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task Jolt_StdioLspServer_InvalidNotification_DoesNotTerminateRequestLoop()
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
                method = "textDocument/didChange",
                @params = new
                {
                    textDocument = new
                    {
                        uri = "file:///tmp/invalid-notification.jazor",
                        version = 1
                    }
                }
            },
            cancellationSource.Token);

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 2,
                method = "workspace/symbol",
                @params = new
                {
                    query = "alive"
                }
            },
            cancellationSource.Token);
        using var response = await ReadResponseAsync(clientOutput, expectedId: 2, cancellationSource.Token);
        Assert.AreEqual(JsonValueKind.Array, response.RootElement.GetProperty("result").ValueKind);

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
    public async Task Jolt_StdioLspServer_StateChangingNotifications_AreBackpressuredInsteadOfDropped()
    {
        var workspaceSink = new BlockingWorkspaceDocumentChangeSink();
        var session = CreateSession(
            new NoOpWorkspaceSymbolProvider(),
            out var workspaceStore,
            workspaceSink);
        var server = new StdioLspServer(session, maxConcurrentRequests: 1, maxQueuedRequests: 1);

        var clientToServerPipe = new Pipe();
        var serverToClientPipe = new Pipe();
        await using var serverInput = clientToServerPipe.Reader.AsStream(leaveOpen: true);
        await using var serverOutput = serverToClientPipe.Writer.AsStream(leaveOpen: true);
        await using var clientInput = clientToServerPipe.Writer.AsStream(leaveOpen: true);
        await using var clientOutput = serverToClientPipe.Reader.AsStream(leaveOpen: true);

        var documentPath = Path.Combine(
            Path.GetTempPath(),
            "jolt-lsp-notification-" + Guid.NewGuid().ToString("N") + ".jazor");
        var documentUri = new Uri(documentPath).AbsoluteUri;

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
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri,
                        languageId = "jazor",
                        version = 1,
                        text = "initial"
                    }
                }
            },
            cancellationSource.Token);

        await SendDidChangeAsync(clientInput, documentUri, version: 2, text: "one", cancellationSource.Token);
        await workspaceSink.WaitUntilFirstChangeStartedAsync(cancellationSource.Token);

        await SendDidChangeAsync(clientInput, documentUri, version: 3, text: "two", cancellationSource.Token);
        await SendDidChangeAsync(clientInput, documentUri, version: 4, text: "three", cancellationSource.Token);
        await SendDidChangeAsync(clientInput, documentUri, version: 5, text: "four", cancellationSource.Token);

        await Task.Delay(100, cancellationSource.Token);
        workspaceSink.ReleaseFirstChange();

        await WaitUntilDocumentTextAsync(workspaceStore, documentPath, "four", cancellationSource.Token);

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 6,
                method = "shutdown",
                @params = new { }
            },
            cancellationSource.Token);
        using var shutdownResponse = await ReadResponseAsync(clientOutput, expectedId: 6, cancellationSource.Token);
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
    public async Task Jolt_StdioLspServer_CancelRequest_WithUnknownIds_DoesNotAccumulatePendingCancellations()
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

        for (var index = 0; index < 256; index++)
        {
            await SendMessageAsync(
                clientInput,
                new
                {
                    jsonrpc = "2.0",
                    method = "$/cancelRequest",
                    @params = new
                    {
                        id = 100_000 + index
                    }
                },
                cancellationSource.Token);
        }

        await SendMessageAsync(
            clientInput,
            new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "shutdown",
                @params = new { }
            },
            cancellationSource.Token);
        using var shutdownResponse = await ReadResponseAsync(clientOutput, expectedId: 1, cancellationSource.Token);
        Assert.AreEqual(JsonValueKind.Null, shutdownResponse.RootElement.GetProperty("result").ValueKind);
        Assert.AreEqual(0, GetPendingCancellationRequestCount(server));

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
    public async Task LspSession_TextDocumentRequest_ForDiskOnlyFile_DoesNotTrackOpenDocument()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "jolt-lsp-disk-doc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var documentPath = Path.Combine(tempDir, "DiskOnly.jazor");
            await File.WriteAllTextAsync(documentPath, "<template><div /></template>");
            var session = CreateSession(new NoOpWorkspaceSymbolProvider(), out var workspaceStore);

            var response = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 1,
                    Method = "textDocument/documentSymbol",
                    Params = JsonSerializer.SerializeToElement(new
                    {
                        textDocument = new
                        {
                            uri = new Uri(documentPath).AbsoluteUri
                        }
                    })
                },
                CancellationToken.None);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Result is not null);
            var openDocuments = await workspaceStore.GetOpenDocumentsAsync(CancellationToken.None);
            Assert.AreEqual(0, openDocuments.Count);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static LspSession CreateSession(ILspWorkspaceSymbolProvider workspaceSymbolProvider)
        => CreateSession(workspaceSymbolProvider, out _);

    private static LspSession CreateSession(
        ILspWorkspaceSymbolProvider workspaceSymbolProvider,
        out InMemoryWorkspaceStore workspaceStore,
        IWorkspaceDocumentChangeSink? workspaceDocumentChangeSink = null)
    {
        workspaceStore = new InMemoryWorkspaceStore();
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
            workspaceDocumentChangeSink,
            extensionRegistry: extensionRegistry,
            extensionProviderTimeout: TimeSpan.FromSeconds(30));
    }

    private static Task SendDidChangeAsync(
        Stream stream,
        string documentUri,
        int version,
        string text,
        CancellationToken cancellationToken)
        => SendMessageAsync(
            stream,
            new
            {
                jsonrpc = "2.0",
                method = "textDocument/didChange",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri,
                        version
                    },
                    contentChanges = new[]
                    {
                        new
                        {
                            text
                        }
                    }
                }
            },
            cancellationToken);

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

    private static async Task WaitUntilTrackedRequestCountAsync(
        StdioLspServer server,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        var activeRequestsField = typeof(StdioLspServer).GetField(
            "_activeRequests",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(activeRequestsField);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var activeRequests = activeRequestsField.GetValue(server) as System.Collections.IDictionary;
            Assert.IsNotNull(activeRequests);
            if (activeRequests.Count >= expectedCount)
            {
                return;
            }

            await Task.Delay(10, cancellationToken);
        }
    }

    private static async Task WaitUntilDocumentTextAsync(
        InMemoryWorkspaceStore workspaceStore,
        string documentPath,
        string expectedText,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = await workspaceStore.GetDocumentAsync(documentPath, cancellationToken);
            if (string.Equals(document?.Text, expectedText, StringComparison.Ordinal))
            {
                return;
            }

            await Task.Delay(10, cancellationToken);
        }
    }

    private static int GetPendingCancellationRequestCount(StdioLspServer server)
    {
        var pendingCancellationRequestsField = typeof(StdioLspServer).GetField(
            "_pendingCancellationRequests",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(pendingCancellationRequestsField);
        var pendingCancellationRequests = pendingCancellationRequestsField.GetValue(server);
        Assert.IsNotNull(pendingCancellationRequests);
        var countProperty = pendingCancellationRequests.GetType().GetProperty("Count");
        Assert.IsNotNull(countProperty);
        return (int)countProperty.GetValue(pendingCancellationRequests)!;
    }

    private static async Task WaitUntilRequestCancellationObservedAsync(
        StdioLspServer server,
        int requestId,
        CancellationToken cancellationToken)
    {
        var activeRequestsField = typeof(StdioLspServer).GetField(
            "_activeRequests",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(activeRequestsField);

        var pendingCancellationRequestsField = typeof(StdioLspServer).GetField(
            "_pendingCancellationRequests",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(pendingCancellationRequestsField);

        var createRequestKeyMethod = typeof(StdioLspServer).GetMethod(
            "CreateRequestKey",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(createRequestKeyMethod);
        using var requestIdJson = JsonDocument.Parse(
            requestId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        var requestKey = createRequestKeyMethod.Invoke(null, [requestIdJson.RootElement.Clone()]) as string;
        Assert.IsFalse(string.IsNullOrWhiteSpace(requestKey));

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var activeRequests = activeRequestsField.GetValue(server) as System.Collections.IDictionary;
            Assert.IsNotNull(activeRequests);
            if (activeRequests.Contains(requestKey))
            {
                var cancellationSource = activeRequests[requestKey] as CancellationTokenSource;
                if (cancellationSource is not null && cancellationSource.IsCancellationRequested)
                {
                    return;
                }
            }

            var pendingCancellationRequests = pendingCancellationRequestsField.GetValue(server) as System.Collections.IEnumerable;
            Assert.IsNotNull(pendingCancellationRequests);
            foreach (var item in pendingCancellationRequests)
            {
                if (string.Equals(item as string, requestKey, StringComparison.Ordinal))
                {
                    return;
                }
            }

            await Task.Delay(10, cancellationToken);
        }
    }

    private sealed class BlockingWorkspaceDocumentChangeSink : IWorkspaceDocumentChangeSink
    {
        private readonly TaskCompletionSource<bool> _firstChangeStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _firstChangeRelease = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _changeCount;

        public async ValueTask OnWorkspaceDocumentChangedAsync(
            DocumentSnapshot document,
            IReadOnlyList<DocumentSnapshot> openDocuments,
            CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _changeCount) == 1)
            {
                _firstChangeStarted.TrySetResult(true);
                await _firstChangeRelease.Task.WaitAsync(cancellationToken);
            }
        }

        public async Task WaitUntilFirstChangeStartedAsync(CancellationToken cancellationToken)
            => await _firstChangeStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        public void ReleaseFirstChange()
            => _firstChangeRelease.TrySetResult(true);
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

    private sealed class ConcurrentWorkspaceSymbolProvider : ILspWorkspaceSymbolProvider
    {
        private readonly int _expectedConcurrentInvocations;
        private readonly TaskCompletionSource<bool> _concurrentReached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _activeInvocations;
        private int _maxObservedConcurrency;

        public ConcurrentWorkspaceSymbolProvider(int expectedConcurrentInvocations)
        {
            _expectedConcurrentInvocations = expectedConcurrentInvocations;
        }

        public string Name => "ConcurrentWorkspaceSymbolProvider";

        public int Priority => 100;

        public int MaxObservedConcurrency => Volatile.Read(ref _maxObservedConcurrency);

        public async ValueTask<IReadOnlyList<LspWorkspaceSymbol>> ProvideWorkspaceSymbolsAsync(
            LspWorkspaceSymbolProviderContext context,
            CancellationToken cancellationToken)
        {
            var activeInvocations = Interlocked.Increment(ref _activeInvocations);
            UpdateMaxObservedConcurrency(activeInvocations);
            if (activeInvocations >= _expectedConcurrentInvocations)
            {
                _concurrentReached.TrySetResult(true);
            }

            try
            {
                await _release.Task.WaitAsync(cancellationToken);
                return Array.Empty<LspWorkspaceSymbol>();
            }
            finally
            {
                Interlocked.Decrement(ref _activeInvocations);
            }
        }

        public async Task WaitUntilConcurrentAsync(CancellationToken cancellationToken)
            => await _concurrentReached.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        public void ReleaseAll()
            => _release.TrySetResult(true);

        private void UpdateMaxObservedConcurrency(int activeInvocations)
        {
            while (true)
            {
                var currentMax = Volatile.Read(ref _maxObservedConcurrency);
                if (activeInvocations <= currentMax)
                {
                    return;
                }

                if (Interlocked.CompareExchange(ref _maxObservedConcurrency, activeInvocations, currentMax) == currentMax)
                {
                    return;
                }
            }
        }
    }

    private sealed class WorkspaceFolderEchoSymbolProvider : ILspWorkspaceSymbolProvider
    {
        private readonly Lock _gate = new();
        private IReadOnlyList<LspWorkspaceFolder> _lastWorkspaceFolders = Array.Empty<LspWorkspaceFolder>();

        public string Name => "WorkspaceFolderEchoSymbolProvider";

        public int Priority => 0;

        public ValueTask<IReadOnlyList<LspWorkspaceSymbol>> ProvideWorkspaceSymbolsAsync(
            LspWorkspaceSymbolProviderContext context,
            CancellationToken cancellationToken)
        {
            var folders = (context.WorkspaceFolders ?? [])
                .Where(static folder => !string.IsNullOrWhiteSpace(folder.Uri))
                .Select(static folder => new LspWorkspaceFolder
                {
                    Uri = folder.Uri,
                    Name = folder.Name
                })
                .ToArray();
            lock (_gate)
            {
                _lastWorkspaceFolders = folders;
            }

            var symbols = folders
                .Select(static folder => new LspWorkspaceSymbol
                {
                    Name = folder.Name,
                    Kind = 5,
                    Location = new LspLocation
                    {
                        Uri = folder.Uri,
                        Range = new LspRange
                        {
                            Start = new LspPosition { Line = 0, Character = 0 },
                            End = new LspPosition { Line = 0, Character = 1 }
                        }
                    },
                    ContainerName = "workspace-folders"
                })
                .ToArray();
            return ValueTask.FromResult<IReadOnlyList<LspWorkspaceSymbol>>(symbols);
        }

        public IReadOnlyList<LspWorkspaceFolder> GetLastWorkspaceFolders()
        {
            lock (_gate)
            {
                return _lastWorkspaceFolders
                    .Select(static folder => new LspWorkspaceFolder
                    {
                        Uri = folder.Uri,
                        Name = folder.Name
                    })
                    .ToArray();
            }
        }
    }
}
