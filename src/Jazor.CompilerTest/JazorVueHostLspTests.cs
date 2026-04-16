using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Directory = Jazor.CompilerTest.TestDirectory;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Razor.InProc;
using Jazor.VueHost.Workspace;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostLspTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [TestMethod]
    public async Task JazorVueHost_Lsp_Initialize_ReturnsCapabilities()
    {
        await using var client = await LspTestClient.StartAsync();

        await client.SendAsync(new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "initialize",
            @params = new { }
        });
        using var response = await client.ReadMessageAsync();

        Assert.AreEqual("2.0", response.RootElement.GetProperty("jsonrpc").GetString());
        Assert.AreEqual(1, response.RootElement.GetProperty("id").GetInt32());
        var result = response.RootElement.GetProperty("result");
        Assert.IsTrue(result.GetProperty("capabilities").GetProperty("hoverProvider").GetBoolean());
        Assert.IsTrue(result.GetProperty("capabilities").GetProperty("definitionProvider").GetBoolean());
        Assert.IsTrue(result.GetProperty("capabilities").GetProperty("referencesProvider").GetBoolean());
        var renameProvider = result.GetProperty("capabilities").GetProperty("renameProvider");
        Assert.IsTrue(renameProvider.GetProperty("prepareProvider").GetBoolean());
        Assert.IsTrue(result.GetProperty("capabilities").GetProperty("codeActionProvider").GetBoolean());
        Assert.IsTrue(result.GetProperty("capabilities").GetProperty("documentSymbolProvider").GetBoolean());
        var semanticTokensProvider = result.GetProperty("capabilities").GetProperty("semanticTokensProvider");
        Assert.IsTrue(semanticTokensProvider.GetProperty("full").GetBoolean());
        Assert.IsFalse(semanticTokensProvider.GetProperty("range").GetBoolean());
        var semanticTokenTypes = semanticTokensProvider
            .GetProperty("legend")
            .GetProperty("tokenTypes")
            .EnumerateArray()
            .Select(static item => item.GetString() ?? string.Empty)
            .ToArray();
        CollectionAssert.Contains(semanticTokenTypes, "class");
        CollectionAssert.Contains(semanticTokenTypes, "method");
        var signatureHelpProvider = result.GetProperty("capabilities").GetProperty("signatureHelpProvider");
        var triggerCharacters = signatureHelpProvider
            .GetProperty("triggerCharacters")
            .EnumerateArray()
            .Select(static item => item.GetString() ?? string.Empty)
            .ToArray();
        CollectionAssert.Contains(triggerCharacters, "(");
        CollectionAssert.Contains(triggerCharacters, ",");
        Assert.AreEqual(1, result.GetProperty("capabilities").GetProperty("textDocumentSync").GetProperty("change").GetInt32());
        Assert.AreEqual("Jazor.VueHost", result.GetProperty("serverInfo").GetProperty("name").GetString());

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_Initialize_SucceedsWhenDenoIsEnabledWithInvalidCommand()
    {
        var client = await LspTestClient.StartAsync(
            "--deno-worker",
            "--deno-command=missing-deno-command-for-tests");
        try
        {
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 101,
                method = "initialize",
                @params = new { }
            });
            using var response = await client.ReadMessageAsync();

            Assert.AreEqual("2.0", response.RootElement.GetProperty("jsonrpc").GetString());
            Assert.AreEqual(101, response.RootElement.GetProperty("id").GetInt32());
            Assert.AreEqual("Jazor.VueHost", response.RootElement.GetProperty("result").GetProperty("serverInfo").GetProperty("name").GetString());
        }
        finally
        {
            await client.DisposeIgnoringExitCodeAsync();
        }
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_TemplateRequests_ReturnNullOrEmptyWhenFrontendLaneHasNoAnswer()
    {
        await using var client = await LspTestClient.StartAsync("--no-deno-worker");
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var componentPath = Path.Combine(tempDirectory, "UserCard.vue");
            await File.WriteAllTextAsync(componentPath, "<template><div>UserCard</div></template>");

            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <template>
                  <UserCard />
                </template>
                """;
            await client.SendAsync(new
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
                        text
                    }
                }
            });
            using var openDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", openDiagnostics.RootElement.GetProperty("method").GetString());

            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 901,
                uri: documentUri,
                line: 1,
                character: 3);
            Assert.AreEqual(0, completionLabels.Length);

            var hover = await client.RequestHoverAsync(
                requestId: 902,
                uri: documentUri,
                line: 1,
                character: 3);
            Assert.IsNull(hover);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 903,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 3
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 903);
            Assert.AreEqual(0, definitionResponse.RootElement.GetProperty("result").GetArrayLength());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_SemanticTokens_ReturnEmptyWhenFrontendLaneHasNoAnswer()
    {
        await using var client = await LspTestClient.StartAsync("--no-deno-worker");
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <template>
                  <UserCard />
                </template>
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1);
            var tokens = await client.RequestSemanticTokensAsync(requestId: 904, uri: documentUri);

            Assert.AreEqual(0, tokens.Count);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_SemanticTokens_ReturnRoslynCodeTokensWithoutFrontendWorker()
    {
        await using var client = await LspTestClient.StartAsync("--no-deno-worker");
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <UserCard />

                @code {
                    private static readonly int count = 42;

                    private void Increment()
                    {
                        count++;
                    }
                }
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1);
            var tokens = await client.RequestSemanticTokensAsync(requestId: 905, uri: documentUri);

            AssertHasSemanticToken(tokens, GetPosition(text, "count = 42"), "count".Length, "variable", "declaration", "static", "readonly");
            AssertHasSemanticToken(tokens, GetPosition(text, "Increment()"), "Increment".Length, "method", "declaration");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_SemanticTokens_ReturnFrontendTokensFromBundledDenoWorker()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var componentsDirectory = Path.Combine(tempDirectory, "Components");
            Directory.CreateDirectory(componentsDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(componentsDirectory, "UserCard.vue"),
                "<template><div>UserCard</div></template>");

            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <UserCard />
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1);
            var tokens = await client.RequestSemanticTokensAsync(requestId: 906, uri: documentUri);

            AssertHasSemanticToken(tokens, GetPosition(text, "UserCard"), "UserCard".Length, "class");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_DidOpenDidChangeAndDidClose_PublishDiagnostics()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var initialText =
                """
                <template>
                  <div>ok</div>
                </template>
                """;
            await client.SendAsync(new
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
                        text = initialText
                    }
                }
            });
            using var openDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", openDiagnostics.RootElement.GetProperty("method").GetString());
            Assert.AreEqual(0, openDiagnostics.RootElement.GetProperty("params").GetProperty("diagnostics").GetArrayLength());

            var updatedText =
                """
                <template>
                  <div>broken</div>
                </template>

                @code {
                    private void Hidden()
                    {
                    }
                }
                """;
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didChange",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri,
                        version = 2
                    },
                    contentChanges = new[]
                    {
                        new
                        {
                            text = updatedText
                        }
                    }
                }
            });
            using var changeDiagnostics = await client.ReadMessageAsync();
            var diagnostics = changeDiagnostics.RootElement.GetProperty("params").GetProperty("diagnostics");
            Assert.AreEqual(1, diagnostics.GetArrayLength());
            Assert.AreEqual("JAZORVUE001", diagnostics[0].GetProperty("code").GetString());
            StringAssert.Contains(diagnostics[0].GetProperty("message").GetString() ?? string.Empty, "No public methods were lowered");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didClose",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    }
                }
            });
            using var closeDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual(0, closeDiagnostics.RootElement.GetProperty("params").GetProperty("diagnostics").GetArrayLength());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_StillServesHttpAndProcessesLspMessages()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");

            var brokenText =
                """
                <template>
                  <div>broken</div>
                </template>

                @code {
                    private void Hidden()
                    {
                    }
                }
                """;
            await client.OpenDocumentAsync(documentUri, brokenText, version: 1);

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            var response = await httpClient.GetStringAsync("/");

            StringAssert.Contains(response, "combined");
            StringAssert.Contains(response, "/@jazor/client");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedJazorCodeBehindDidChange_BroadcastsHmrJavaScriptUpdate()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(tempDirectory, "Counter.jazor.cs");
            var codeBehindUri = new Uri(codeBehindPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                  <div>@count</div>
                </template>
                """);

            var initialCodeBehindText =
                """
                public partial class Counter
                {
                    [State] private int count = 1;

                    public int Increment(int delta)
                    {
                        return count + delta;
                    }
                }
                """;
            await File.WriteAllTextAsync(codeBehindPath, initialCodeBehindText);

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            _ = await httpClient.GetStringAsync("/Counter.jazor");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(codeBehindUri, initialCodeBehindText, version: 1, languageId: "csharp");

            var updatedCodeBehindText =
                """
                public partial class Counter
                {
                    [State] private int count = 2;

                    public int Increment(int delta)
                    {
                        return count + delta;
                    }
                }
                """;
            await client.ChangeDocumentAsync(codeBehindUri, updatedCodeBehindText, version: 2);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("path").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("acceptedPath").GetString());
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedJazorCodeBehindSignatureDidChange_BroadcastsFullReload()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(tempDirectory, "Counter.jazor.cs");
            var codeBehindUri = new Uri(codeBehindPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                </template>
                """);

            var initialCodeBehindText =
                """
                public partial class Counter
                {
                    public int Increment(int delta)
                    {
                        return delta + 1;
                    }
                }
                """;
            await File.WriteAllTextAsync(codeBehindPath, initialCodeBehindText);

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            _ = await httpClient.GetStringAsync("/Counter.jazor");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(codeBehindUri, initialCodeBehindText, version: 1, languageId: "csharp");

            var updatedCodeBehindText =
                """
                public partial class Counter
                {
                    public long Increment(long delta)
                    {
                        return delta + 1;
                    }
                }
                """;
            await client.ChangeDocumentAsync(codeBehindUri, updatedCodeBehindText, version: 2);

            var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());
            Assert.AreEqual("Public component descriptor changed.", reloadMessage.GetProperty("reason").GetString());
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedJazorCodeBehindDidChangeIsLaterSaved_DoesNotBroadcastDuplicateHmrMessage()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(tempDirectory, "Counter.jazor.cs");
            var codeBehindUri = new Uri(codeBehindPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                  <div>@count</div>
                </template>
                """);

            var initialCodeBehindText =
                """
                public partial class Counter
                {
                    [State] private int count = 1;

                    public int Increment(int delta)
                    {
                        return count + delta;
                    }
                }
                """;
            await File.WriteAllTextAsync(codeBehindPath, initialCodeBehindText);

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            _ = await httpClient.GetStringAsync("/Counter.jazor");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(codeBehindUri, initialCodeBehindText, version: 1, languageId: "csharp");

            var updatedCodeBehindText =
                """
                public partial class Counter
                {
                    [State] private int count = 2;

                    public int Increment(int delta)
                    {
                        return count + delta;
                    }
                }
                """;
            await client.ChangeDocumentAsync(codeBehindUri, updatedCodeBehindText, version: 2);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(codeBehindPath, updatedCodeBehindText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedVueDidChange_BroadcastsHmrJavaScriptUpdate()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
        var documentPath = Path.Combine(tempDirectory, "Counter.vue");
        var documentUri = new Uri(documentPath).AbsoluteUri;
        var initialText = "<template><div>Counter</div></template>";
        await File.WriteAllTextAsync(documentPath, initialText);
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            var initialModule = await httpClient.GetStringAsync("/Counter.vue");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(documentUri, initialText, version: 1, languageId: "vue");

            var updatedText = "<template><div>Counter updated</div></template>";
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/Counter.vue", updates[0].GetProperty("path").GetString());
            Assert.AreEqual("/Counter.vue", updates[0].GetProperty("acceptedPath").GetString());
            var updatedModule = await httpClient.GetStringAsync("/Counter.vue?t=2");
            Assert.AreNotEqual(initialModule, updatedModule);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedVueDidChangeIsLaterSaved_DoesNotBroadcastDuplicateHmrMessage()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
            var documentPath = Path.Combine(tempDirectory, "Counter.vue");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var initialText = "<template><div>Counter</div></template>";
            await File.WriteAllTextAsync(documentPath, initialText);

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            _ = await httpClient.GetStringAsync("/Counter.vue");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(documentUri, initialText, version: 1, languageId: "vue");

            var updatedText = "<template><div>Counter updated</div></template>";
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(documentPath, updatedText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedVueStyleOnlyDidChange_BroadcastsStyleUpdate()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        var documentPath = Path.Combine(tempDirectory, "Counter.vue");
        var documentUri = new Uri(documentPath).AbsoluteUri;
        var initialText =
            """
            <template><div>Counter</div></template>
            <style>
            .counter { color: red; }
            </style>
            """;
        await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
        await File.WriteAllTextAsync(documentPath, initialText);
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            _ = await httpClient.GetStringAsync("/Counter.vue");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(documentUri, initialText, version: 1, languageId: "vue");

            var updatedText =
                """
                <template><div>Counter</div></template>
                <style>
                .counter { color: blue; }
                </style>
                """;
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("style-update", updateMessage.GetProperty("type").GetString());
            var inlineStyles = updateMessage.GetProperty("inlineStyles").EnumerateArray().ToArray();
            Assert.AreEqual(1, inlineStyles.Length);
            Assert.AreEqual("/Counter.vue", inlineStyles[0].GetProperty("path").GetString());
            Assert.AreEqual(".counter { color: blue; }", inlineStyles[0].GetProperty("content").GetString());
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedVueStyleOnlyDidChangeIsLaterSaved_DoesNotBroadcastDuplicateHmrMessage()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        var documentPath = Path.Combine(tempDirectory, "Counter.vue");
        var documentUri = new Uri(documentPath).AbsoluteUri;
        var initialText =
            """
            <template><div>Counter</div></template>
            <style>
            .counter { color: red; }
            </style>
            """;
        await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
        await File.WriteAllTextAsync(documentPath, initialText);
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            _ = await httpClient.GetStringAsync("/Counter.vue");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(documentUri, initialText, version: 1, languageId: "vue");

            var updatedText =
                """
                <template><div>Counter</div></template>
                <style>
                .counter { color: blue; }
                </style>
                """;
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("style-update", updateMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(documentPath, updatedText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedCssDidChange_UsesWorkspaceTextAndSuppressesDuplicateHmrMessage()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        var documentPath = Path.Combine(tempDirectory, "site.css");
        var documentUri = new Uri(documentPath).AbsoluteUri;
        const string initialText = "body { color: red; }";
        const string updatedText = "body { color: blue; }";
        await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
        await File.WriteAllTextAsync(documentPath, initialText);
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            Assert.AreEqual(initialText, await httpClient.GetStringAsync("/site.css"));

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(documentUri, initialText, version: 1, languageId: "css");
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("style-update", updateMessage.GetProperty("type").GetString());
            CollectionAssert.AreEqual(
                new[] { "/site.css" },
                updateMessage.GetProperty("paths").EnumerateArray().Select(static item => item.GetString()).ToArray());
            Assert.AreEqual(updatedText, await httpClient.GetStringAsync("/site.css?t=2"));

            await File.WriteAllTextAsync(documentPath, updatedText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedTypeScriptDidChange_BroadcastsHmrJavaScriptUpdate()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
        var documentPath = Path.Combine(tempDirectory, "main.ts");
        var documentUri = new Uri(documentPath).AbsoluteUri;
        var initialText = "export const count: number = 1;";
        await File.WriteAllTextAsync(documentPath, initialText);
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            _ = await httpClient.GetStringAsync("/main.ts");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(documentUri, initialText, version: 1, languageId: "typescript");

            var updatedText = "export const count: number = 2;";
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/main.ts", updates[0].GetProperty("path").GetString());
            Assert.AreEqual("/main.ts", updates[0].GetProperty("acceptedPath").GetString());
            Assert.AreEqual(updatedText, await httpClient.GetStringAsync("/main.ts?t=2"));
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedTypeScriptDidChangeIsLaterSaved_DoesNotBroadcastDuplicateHmrMessage()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
            var documentPath = Path.Combine(tempDirectory, "main.ts");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var initialText = "export const count: number = 1;";
            await File.WriteAllTextAsync(documentPath, initialText);

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            _ = await httpClient.GetStringAsync("/main.ts");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(documentUri, initialText, version: 1, languageId: "typescript");

            var updatedText = "export const count: number = 2;";
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(documentPath, updatedText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedJazorDidChange_BroadcastsHmrJavaScriptUpdate()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
        var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
        var documentUri = new Uri(documentPath).AbsoluteUri;
        var initialText =
            """
            <template>
              <button>@Count</button>
            </template>

            @code {
                [Prop] public int Count { get; set; }

                public int Increment()
                {
                    return Count + 1;
                }
            }
            """;
        await File.WriteAllTextAsync(documentPath, initialText);
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            var initialModule = await httpClient.GetStringAsync("/Counter.jazor");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(documentUri, initialText, version: 1);

            var updatedText =
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 2;
                    }
                }
                """;
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("path").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("acceptedPath").GetString());
            var updatedModule = await httpClient.GetStringAsync("/Counter.jazor?t=2");
            Assert.AreNotEqual(initialModule, updatedModule);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedJazorDescriptorDidChange_BroadcastsFullReload()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var initialText =
                """
                <template>
                  <div>Hello</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """;
            await File.WriteAllTextAsync(documentPath, initialText);

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            _ = await httpClient.GetStringAsync("/Counter.jazor");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(documentUri, initialText, version: 1);

            var updatedText =
                """
                <template>
                  <div>Hello updated</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                    [Prop] public string Title { get; set; } = string.Empty;
                }
                """;
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());
            Assert.AreEqual("Public component descriptor changed.", reloadMessage.GetProperty("reason").GetString());
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_WithDevMode_WhenUnsavedJazorDidChangeIsLaterSaved_DoesNotBroadcastDuplicateHmrMessage()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var port = GetFreePort();
        await using var client = await LspTestClient.StartAsync(
            "--dev",
            $"--dev-root={tempDirectory}",
            $"--dev-port={port}",
            "--dev-host=127.0.0.1",
            "--dev-frontend=stub");
        await client.InitializeAsync();

        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDirectory, "index.html"), "<html><body>combined</body></html>");
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var initialText =
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 1;
                    }
                }
                """;
            await File.WriteAllTextAsync(documentPath, initialText);

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}")
            };
            _ = await httpClient.GetStringAsync("/Counter.jazor");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(CreateWebSocketUri(port, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await client.OpenDocumentAsync(documentUri, initialText, version: 1);

            var updatedText =
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 2;
                    }
                }
                """;
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(documentPath, updatedText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_TemplateImportedComponent_RemainsCompletionHoverAndDefinitionCapable()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var vuePath = Path.Combine(tempDirectory, "UserCard.vue");
            await File.WriteAllTextAsync(vuePath, "<template><div>UserCard</div></template>");
            var text =
                """
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <
                  <UserCard />
                </template>
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 102,
                documentUri,
                line: 3,
                character: 3);
            CollectionAssert.Contains(completionLabels, "UserCard");

            var hover = await client.RequestHoverAsync(
                requestId: 103,
                documentUri,
                line: 4,
                character: 5);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "./UserCard.vue");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 104,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 4,
                        character = 5
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 104);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(new Uri(vuePath).AbsoluteUri, definitions[0].GetProperty("uri").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_RazorMarkupComponent_RemainsCompletionHoverAndDefinitionCapable()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var vuePath = Path.Combine(tempDirectory, "UserCard.vue");
            await File.WriteAllTextAsync(vuePath, "<template><div>UserCard</div></template>");
            var text =
                """
                @page "/counter"

                <
                <UserCard />

                @code {
                    private int count;
                }
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 150,
                documentUri,
                line: 2,
                character: 1);
            CollectionAssert.Contains(completionLabels, "UserCard");

            var hover = await client.RequestHoverAsync(
                requestId: 151,
                documentUri,
                line: 3,
                character: 5);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "./UserCard.vue");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 152,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 3,
                        character = 5
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 152);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(new Uri(vuePath).AbsoluteUri, definitions[0].GetProperty("uri").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_RazorMarkupComponentWithoutTemplateOrVueImport_RemainsFeatureCapable()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var vuePath = Path.Combine(tempDirectory, "UserCard.vue");
            await File.WriteAllTextAsync(vuePath, "<template><div>UserCard</div></template>");
            var text =
                """
                <
                <UserCard />
                <UserCard />

                @code {
                    private void Hidden()
                    {
                    }
                }
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 110,
                documentUri,
                line: 0,
                character: 1);
            CollectionAssert.Contains(completionLabels, "UserCard");

            var hover = await client.RequestHoverAsync(
                requestId: 111,
                documentUri,
                line: 1,
                character: 5);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "./UserCard.vue");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 112,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 5
                    }
                }
            });
            using var definitionResponse = await client.ReadMessageAsync();
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(new Uri(vuePath).AbsoluteUri, definitions[0].GetProperty("uri").GetString());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 113,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 5
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 113);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(references.GetArrayLength() >= 2);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 114,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 5
                    },
                    newName = "ProfileCard"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 114);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes")
                .GetProperty(documentUri);
            Assert.IsTrue(changes.GetArrayLength() >= 2);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_VueDocument_RemainsCompletionHoverDefinitionAndDiagnosticCapable()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var userCardPath = Path.Combine(tempDirectory, "UserCard.vue");
            await File.WriteAllTextAsync(userCardPath, "<template><div>UserCard</div></template>");

            var hostPath = Path.Combine(tempDirectory, "Host.vue");
            var hostUri = new Uri(hostPath).AbsoluteUri;
            var text =
                """
                <template>
                  <
                  <UserCard />
                  <MissingCard />
                </template>
                """;

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri,
                        languageId = "vue",
                        version = 1,
                        text
                    }
                }
            });
            using var diagnosticsMessage = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", diagnosticsMessage.RootElement.GetProperty("method").GetString());
            var diagnostics = diagnosticsMessage.RootElement.GetProperty("params").GetProperty("diagnostics");
            Assert.AreEqual(1, diagnostics.GetArrayLength());
            Assert.AreEqual("JAZORVUEFRONTEND001", diagnostics[0].GetProperty("code").GetString());

            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 170,
                hostUri,
                line: 1,
                character: 3);
            CollectionAssert.Contains(completionLabels, "UserCard");

            var hover = await client.RequestHoverAsync(
                requestId: 171,
                hostUri,
                line: 2,
                character: 5);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "./UserCard.vue");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 172,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = 2,
                        character = 5
                    }
                }
            });
            using var definitionResponse = await client.ReadMessageAsync();
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(new Uri(userCardPath).AbsoluteUri, definitions[0].GetProperty("uri").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_TypeScriptDocument_RemainsCompletionHoverDefinitionReferencesAndRenameCapable()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "counter.ts");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                const total = 1;

                function renderLabel(step) {
                  const snapshot = total + step;
                  return total.toString();
                }

                tot
                """;

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri,
                        languageId = "typescript",
                        version = 1,
                        text
                    }
                }
            });
            using var diagnosticsMessage = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", diagnosticsMessage.RootElement.GetProperty("method").GetString());
            Assert.AreEqual(
                0,
                diagnosticsMessage.RootElement.GetProperty("params").GetProperty("diagnostics").GetArrayLength());

            var completionPosition = GetLastPosition(text, "tot", advance: "tot".Length);
            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 183,
                documentUri,
                completionPosition.Line,
                completionPosition.Character);
            CollectionAssert.Contains(completionLabels, "total");

            var hoverPosition = GetPosition(text, "return total", advance: "return ".Length + 1);
            var hover = await client.RequestHoverAsync(
                requestId: 184,
                documentUri,
                hoverPosition.Line,
                hoverPosition.Character);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "total");

            var definitionPosition = GetPosition(text, "snapshot = total", advance: "snapshot = ".Length + 1);
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 185,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = definitionPosition.Line,
                        character = definitionPosition.Character
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 185);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(documentUri, definitions[0].GetProperty("uri").GetString());
            Assert.AreEqual(0, definitions[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());

            var referencesPosition = GetPosition(text, "return total", advance: "return ".Length + 1);
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 186,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = referencesPosition.Line,
                        character = referencesPosition.Character
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 186);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.AreEqual(3, references.GetArrayLength());
            Assert.IsTrue(references.EnumerateArray().All(reference => reference.GetProperty("uri").GetString() == documentUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 187,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = referencesPosition.Line,
                        character = referencesPosition.Character
                    },
                    newName = "grandTotal"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 187);
            var edits = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes")
                .GetProperty(documentUri);
            Assert.AreEqual(3, edits.GetArrayLength());
            Assert.IsTrue(edits.EnumerateArray().All(edit => edit.GetProperty("newText").GetString() == "grandTotal"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_VueDocument_ScriptBlock_RemainsCompletionHoverAndDefinitionCapable()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Host.vue");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <template>
                  <div>{{ label }}</div>
                </template>

                <script setup lang="ts">
                const total = 1;

                function formatLabel() {
                  return total.toString();
                }

                const label = formatLabel();
                form
                </script>
                """;

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri,
                        languageId = "vue",
                        version = 1,
                        text
                    }
                }
            });
            using var diagnosticsMessage = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", diagnosticsMessage.RootElement.GetProperty("method").GetString());
            Assert.AreEqual(
                0,
                diagnosticsMessage.RootElement.GetProperty("params").GetProperty("diagnostics").GetArrayLength());

            var completionPosition = GetLastPosition(text, "form", advance: "form".Length);
            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 188,
                documentUri,
                completionPosition.Line,
                completionPosition.Character);
            CollectionAssert.Contains(completionLabels, "formatLabel");

            var hoverPosition = GetPosition(text, "formatLabel();", advance: 1);
            var hover = await client.RequestHoverAsync(
                requestId: 189,
                documentUri,
                hoverPosition.Line,
                hoverPosition.Character);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "formatLabel");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 190,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = hoverPosition.Line,
                        character = hoverPosition.Character
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 190);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(documentUri, definitions[0].GetProperty("uri").GetString());
            Assert.AreEqual(7, definitions[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_VueDocument_ReferencesAndRename_IncludeNearbyJazorDocumentsOnDisk()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "UserBadge.vue");
            var declarationUri = new Uri(declarationPath).AbsoluteUri;
            await File.WriteAllTextAsync(declarationPath, "<template><div>UserBadge</div></template>");

            var jazorPath = Path.Combine(tempDirectory, "Counter.jazor");
            var jazorUri = new Uri(jazorPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                jazorPath,
                """
                <UserBadge />
                """);

            var hostPath = Path.Combine(tempDirectory, "Host.vue");
            var hostUri = new Uri(hostPath).AbsoluteUri;
            var hostText =
                """
                <template>
                  <UserBadge />
                </template>
                """;
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri,
                        languageId = "vue",
                        version = 1,
                        text = hostText
                    }
                }
            });
            using var diagnosticsMessage = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", diagnosticsMessage.RootElement.GetProperty("method").GetString());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 175,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 5
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 175);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == declarationUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == hostUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == jazorUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 176,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 5
                    },
                    newName = "ProfileBadge"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 176);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == hostUri));
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == jazorUri));
            Assert.AreEqual(
                "ProfileBadge",
                changes.GetProperty(jazorUri)[0].GetProperty("newText").GetString());

            await client.ShutdownAsync();
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_VueScriptImport_ReferencesAndRename_IncludeNearbyJazorDocumentsOnDisk()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "UserBadge.vue");
            var declarationUri = new Uri(declarationPath).AbsoluteUri;
            await File.WriteAllTextAsync(declarationPath, "<template><div>UserBadge</div></template>");

            var jazorPath = Path.Combine(tempDirectory, "Counter.jazor");
            var jazorUri = new Uri(jazorPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                jazorPath,
                """
                <UserBadge />
                """);

            var hostPath = Path.Combine(tempDirectory, "Host.vue");
            var hostUri = new Uri(hostPath).AbsoluteUri;
            var hostText =
                """
                <script setup lang="ts">
                import UserBadge from "./UserBadge.vue";
                const current = UserBadge;
                </script>

                <template>
                  <section />
                </template>
                """;
            var usagePosition = GetPosition(hostText, "UserBadge;", advance: 1);
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri,
                        languageId = "vue",
                        version = 1,
                        text = hostText
                    }
                }
            });
            using var diagnosticsMessage = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", diagnosticsMessage.RootElement.GetProperty("method").GetString());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 177,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 8
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 177);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == declarationUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == hostUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == jazorUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 178,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 8
                    },
                    newName = "ProfileBadge"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 178);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == hostUri));
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == jazorUri));
            Assert.AreEqual(
                "ProfileBadge",
                changes.GetProperty(jazorUri)[0].GetProperty("newText").GetString());

            await client.ShutdownAsync();
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_VueScriptImport_DefinitionRemainsNativeWhileReferencesAndRenameBridgeIntoJazor()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "UserBadge.vue");
            var declarationUri = new Uri(declarationPath).AbsoluteUri;
            await File.WriteAllTextAsync(declarationPath, "<template><div>UserBadge</div></template>");

            var jazorPath = Path.Combine(tempDirectory, "Counter.jazor");
            var jazorUri = new Uri(jazorPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                jazorPath,
                """
                <UserBadge />
                """);

            var hostPath = Path.Combine(tempDirectory, "Host.vue");
            var hostUri = new Uri(hostPath).AbsoluteUri;
            var hostText =
                """
                <script setup lang="ts">
                import UserBadge from "./UserBadge.vue";
                const current = UserBadge;
                </script>

                <template>
                  <section />
                </template>
                """;
            var usagePosition = GetPosition(hostText, "UserBadge;", advance: 1);
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri,
                        languageId = "vue",
                        version = 1,
                        text = hostText
                    }
                }
            });
            using var diagnosticsMessage = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", diagnosticsMessage.RootElement.GetProperty("method").GetString());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 1781,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 1781);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.IsTrue(string.Equals(
                VueHostWorkspaceResolver.NormalizePath(declarationPath),
                VueHostWorkspaceResolver.NormalizePath(LspProtocolHelpers.ToDocumentPath(definitions[0].GetProperty("uri").GetString()!)),
                StringComparison.OrdinalIgnoreCase));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 1782,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 8
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 1782);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(ContainsNormalizedUri(references, hostPath));
            Assert.IsTrue(ContainsNormalizedUri(references, jazorPath));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 1783,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 8
                    },
                    newName = "ProfileBadge"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 1783);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(ContainsNormalizedChange(changes, hostPath));
            Assert.IsTrue(ContainsNormalizedChange(changes, jazorPath));
            Assert.AreEqual(
                "ProfileBadge",
                GetChangeEntry(changes, jazorPath)[0].GetProperty("newText").GetString());

            await client.ShutdownAsync();
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_TypeScriptVueImport_ReferencesAndRename_IncludeNearbyJazorDocumentsOnDisk()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "UserBadge.vue");
            var declarationUri = new Uri(declarationPath).AbsoluteUri;
            await File.WriteAllTextAsync(declarationPath, "<template><div>UserBadge</div></template>");

            var jazorPath = Path.Combine(tempDirectory, "Counter.jazor");
            var jazorUri = new Uri(jazorPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                jazorPath,
                """
                <UserBadge />
                """);

            var scriptPath = Path.Combine(tempDirectory, "consumer.ts");
            var scriptUri = new Uri(scriptPath).AbsoluteUri;
            var scriptText =
                """
                import UserBadge from "./UserBadge.vue";
                export const current = UserBadge;
                """;
            var usagePosition = GetPosition(scriptText, "UserBadge;", advance: 1);
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = scriptUri,
                        languageId = "typescript",
                        version = 1,
                        text = scriptText
                    }
                }
            });
            using var diagnosticsMessage = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", diagnosticsMessage.RootElement.GetProperty("method").GetString());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 179,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = scriptUri
                    },
                    position = new
                    {
                        line = 0,
                        character = 8
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 179);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == scriptUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == jazorUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 180,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = scriptUri
                    },
                    position = new
                    {
                        line = 0,
                        character = 8
                    },
                    newName = "ProfileBadge"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 180);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == scriptUri));
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == jazorUri));
            Assert.AreEqual(
                "ProfileBadge",
                changes.GetProperty(jazorUri)[0].GetProperty("newText").GetString());

            await client.ShutdownAsync();
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_JavaScriptVueImport_DefinitionRemainsNativeWhileReferencesAndRenameBridgeIntoJazor()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "UserBadge.vue");
            var declarationUri = new Uri(declarationPath).AbsoluteUri;
            await File.WriteAllTextAsync(declarationPath, "<template><div>UserBadge</div></template>");

            var jazorPath = Path.Combine(tempDirectory, "Counter.jazor");
            var jazorUri = new Uri(jazorPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                jazorPath,
                """
                <UserBadge />
                """);

            var scriptPath = Path.Combine(tempDirectory, "consumer.js");
            var scriptUri = new Uri(scriptPath).AbsoluteUri;
            var scriptText =
                """
                import UserBadge from "./UserBadge.vue";
                export const current = UserBadge;
                """;
            var usagePosition = GetPosition(scriptText, "UserBadge;", advance: 1);
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = scriptUri,
                        languageId = "javascript",
                        version = 1,
                        text = scriptText
                    }
                }
            });
            using var scriptDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", scriptDiagnostics.RootElement.GetProperty("method").GetString());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 1784,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = scriptUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 1784);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(declarationUri, definitions[0].GetProperty("uri").GetString());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 1785,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = scriptUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 1785);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(ContainsNormalizedUri(references, scriptPath));
            Assert.IsTrue(ContainsNormalizedUri(references, jazorPath));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 1786,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = scriptUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    newName = "ProfileBadge"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 1786);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(ContainsNormalizedChange(changes, scriptPath));
            Assert.IsTrue(ContainsNormalizedChange(changes, jazorPath));
            Assert.AreEqual(
                "ProfileBadge",
                GetChangeEntry(changes, jazorPath)[0].GetProperty("newText").GetString());

            await client.ShutdownAsync();
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                DeleteDirectoryWithRetries(tempDirectory);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_TypeScriptDocument_ReturnsFrontendScriptCompletionHoverAndDefinition()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "counter.ts");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                const count = 1;

                function increment(step: number) {
                  return count + step;
                }

                const snapshot = count;

                inc
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1, languageId: "typescript");

            var symbols = await client.RequestDocumentSymbolsAsync(
                requestId: 189,
                documentUri);
            CollectionAssert.AreEquivalent(
                new[] { "count", "increment", "snapshot" },
                symbols.EnumerateArray()
                    .Select(static symbol => symbol.GetProperty("name").GetString() ?? string.Empty)
                    .ToArray());

            var semanticTokens = await client.RequestSemanticTokensAsync(
                requestId: 198,
                uri: documentUri);
            AssertHasSemanticToken(semanticTokens, GetPosition(text, "count = 1"), "count".Length, "variable");
            AssertHasSemanticToken(semanticTokens, GetPosition(text, "increment(step"), "increment".Length, "method");

            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 190,
                documentUri,
                line: 8,
                character: 3);
            CollectionAssert.Contains(completionLabels, "increment");

            var hoverPosition = GetPosition(text, "return count + step;", advance: "return ".Length + 1);
            var hover = await client.RequestHoverAsync(
                requestId: 191,
                documentUri,
                hoverPosition.Line,
                hoverPosition.Character);
            Assert.IsNotNull(hover);
            var hoverContents = hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty;
            StringAssert.Contains(hoverContents, "count");
            StringAssert.Contains(hoverContents, "const");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 192,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = hoverPosition.Line,
                        character = hoverPosition.Character
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 192);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(documentUri, definitions[0].GetProperty("uri").GetString());
            Assert.AreEqual(0, definitions[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 195,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = hoverPosition.Line,
                        character = hoverPosition.Character
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 195);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.AreEqual(3, references.GetArrayLength());
            Assert.IsTrue(references.EnumerateArray().All(reference => reference.GetProperty("uri").GetString() == documentUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 196,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = hoverPosition.Line,
                        character = hoverPosition.Character
                    },
                    newName = "total"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 196);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == documentUri));
            Assert.AreEqual(3, changes.GetProperty(documentUri).GetArrayLength());
            Assert.IsTrue(changes.GetProperty(documentUri).EnumerateArray().All(edit => edit.GetProperty("newText").GetString() == "total"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_CSharpDocument_DefinitionReferencesAndRename_WorkEndToEnd()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "CounterLogic.cs");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                internal static class CounterLogic
                {
                    private static int count = 1;

                    public static int Increment()
                    {
                        count++;
                        return count;
                    }
                }
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1, languageId: "csharp");

            var usagePosition = GetPosition(text, "count++;", advance: 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 1791,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 1791);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(documentUri, definitions[0].GetProperty("uri").GetString());
            Assert.AreEqual(2, definitions[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 1792,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 1792);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.AreEqual(3, references.GetArrayLength());
            Assert.IsTrue(references.EnumerateArray().All(reference => reference.GetProperty("uri").GetString() == documentUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 1793,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    newName = "totalCount"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 1793);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(ContainsNormalizedChange(changes, documentPath));
            Assert.AreEqual(3, GetChangeEntry(changes, documentPath).GetArrayLength());
            Assert.IsTrue(GetChangeEntry(changes, documentPath)
                .EnumerateArray()
                .All(change => change.GetProperty("newText").GetString() == "totalCount"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_CSharpDocument_DefinitionReferencesAndRename_IncludeUnopenedDiskCSharpDocument()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "CounterLogic.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                internal static class CounterLogic
                {
                    public static int Count = 1;
                }
                """);

            var documentPath = Path.Combine(tempDirectory, "CounterLogicConsumer.cs");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                internal static class CounterLogicConsumer
                {
                    public static int Read()
                    {
                        return CounterLogic.Count;
                    }
                }
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1, languageId: "csharp");

            var usagePosition = GetPosition(text, "Count", advance: 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 17931,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 17931);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(new Uri(declarationPath).AbsoluteUri, definitions[0].GetProperty("uri").GetString());
            Assert.AreEqual(2, definitions[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 17932,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 17932);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.AreEqual(2, references.GetArrayLength());
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == documentUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == new Uri(declarationPath).AbsoluteUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 17933,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    newName = "TotalCount"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 17933);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(ContainsNormalizedChange(changes, documentPath));
            Assert.IsTrue(ContainsNormalizedChange(changes, declarationPath));
            Assert.IsTrue(GetChangeEntry(changes, documentPath)
                .EnumerateArray()
                .All(change => change.GetProperty("newText").GetString() == "TotalCount"));
            Assert.IsTrue(GetChangeEntry(changes, declarationPath)
                .EnumerateArray()
                .All(change => change.GetProperty("newText").GetString() == "TotalCount"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_CSharpDocument_DefinitionReferencesAndRename_IncludeUnopenedDiskBackedWorkspaceDocuments()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var featuresDirectory = Path.Combine(tempDirectory, "Features");
            var sharedDirectory = Path.Combine(tempDirectory, "Shared");
            var dashboardsDirectory = Path.Combine(tempDirectory, "Dashboards");
            Directory.CreateDirectory(featuresDirectory);
            Directory.CreateDirectory(sharedDirectory);
            Directory.CreateDirectory(dashboardsDirectory);

            var declarationPath = Path.Combine(sharedDirectory, "SharedState.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                namespace Demo;

                internal static class SharedState
                {
                    internal static int Count = 1;
                }
                """);

            var unopenedReferencePath = Path.Combine(dashboardsDirectory, "DashboardConsumer.cs");
            await File.WriteAllTextAsync(
                unopenedReferencePath,
                """
                namespace Demo;

                internal static class DashboardConsumer
                {
                    internal static int Read()
                    {
                        return SharedState.Count + SharedState.Count;
                    }
                }
                """);

            var documentPath = Path.Combine(featuresDirectory, "CounterConsumer.cs");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                namespace Demo;

                internal static class CounterConsumer
                {
                    internal static int Read()
                    {
                        return SharedState.Count;
                    }
                }
                """;
            await File.WriteAllTextAsync(documentPath, text);
            await client.OpenDocumentAsync(documentUri, text, version: 1, languageId: "csharp");

            var usagePosition = GetPosition(text, "SharedState.Count", advance: "SharedState.".Length + 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 17931,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 17931);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.IsTrue(ContainsNormalizedUri(definitions, declarationPath));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 17932,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 17932);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(ContainsNormalizedUri(references, documentPath));
            Assert.IsTrue(ContainsNormalizedUri(references, declarationPath));
            Assert.IsTrue(ContainsNormalizedUri(references, unopenedReferencePath));
            Assert.IsTrue(references.EnumerateArray().All(reference =>
                string.Equals(
                    Path.GetExtension(LspProtocolHelpers.ToDocumentPath(reference.GetProperty("uri").GetString()!)),
                    ".cs",
                    StringComparison.OrdinalIgnoreCase)));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 17933,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    newName = "TotalCount"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 17933);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(ContainsNormalizedChange(changes, documentPath));
            Assert.IsTrue(ContainsNormalizedChange(changes, declarationPath));
            Assert.IsTrue(ContainsNormalizedChange(changes, unopenedReferencePath));
            Assert.IsTrue(changes.EnumerateObject().All(change =>
                string.Equals(
                    Path.GetExtension(LspProtocolHelpers.ToDocumentPath(change.Name)),
                    ".cs",
                    StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(changes.EnumerateObject()
                .SelectMany(static change => change.Value.EnumerateArray())
                .All(change => change.GetProperty("newText").GetString() == "TotalCount"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_JazorCodeDeclaration_ReferencesAndRename_IncludeUnopenedDiskBackedCSharpAndJazorDocuments()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var sharedDirectory = Path.Combine(tempDirectory, "Shared");
            var featuresDirectory = Path.Combine(tempDirectory, "Features");
            var dashboardsDirectory = Path.Combine(tempDirectory, "Dashboards");
            Directory.CreateDirectory(sharedDirectory);
            Directory.CreateDirectory(featuresDirectory);
            Directory.CreateDirectory(dashboardsDirectory);

            var documentPath = Path.Combine(sharedDirectory, "SharedState.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                @code {
                    public static int Count => 1;
                }
                """;
            await File.WriteAllTextAsync(documentPath, text);

            var declarationDocument = new DocumentSnapshot(documentPath, DocumentKind.Jazor, text, "1");
            var referencedTypeName = GetProjectedComponentTypeName(declarationDocument);

            var unopenedCSharpPath = Path.Combine(featuresDirectory, "CounterConsumer.cs");
            await File.WriteAllTextAsync(
                unopenedCSharpPath,
                $$"""
                internal static class CounterConsumer
                {
                    internal static int Read()
                    {
                        return {{referencedTypeName}}.Count;
                    }
                }
                """);

            var unopenedJazorPath = Path.Combine(dashboardsDirectory, "DashboardPanel.jazor");
            await File.WriteAllTextAsync(
                unopenedJazorPath,
                $$"""
                @code {
                    private int Read()
                    {
                        return {{referencedTypeName}}.Count + {{referencedTypeName}}.Count;
                    }
                }
                """);

            await client.OpenDocumentAsync(documentUri, text, version: 1);
            var declarationPosition = GetPosition(text, "Count =>", advance: 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 17934,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = declarationPosition.Line,
                        character = declarationPosition.Character
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 17934);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(ContainsNormalizedUri(references, documentPath));
            Assert.IsTrue(ContainsNormalizedUri(references, unopenedCSharpPath));
            Assert.IsTrue(ContainsNormalizedUri(references, unopenedJazorPath));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 17935,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = declarationPosition.Line,
                        character = declarationPosition.Character
                    },
                    newName = "TotalCount"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 17935);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(ContainsNormalizedChange(changes, documentPath));
            Assert.IsTrue(ContainsNormalizedChange(changes, unopenedCSharpPath));
            Assert.IsTrue(ContainsNormalizedChange(changes, unopenedJazorPath));
            Assert.IsTrue(changes.EnumerateObject()
                .SelectMany(static change => change.Value.EnumerateArray())
                .All(change => change.GetProperty("newText").GetString() == "TotalCount"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_JazorCodeUsage_Definition_ResolvesUnopenedDiskBackedCSharpDeclaration()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var sharedDirectory = Path.Combine(tempDirectory, "Shared");
            var featuresDirectory = Path.Combine(tempDirectory, "Features");
            Directory.CreateDirectory(sharedDirectory);
            Directory.CreateDirectory(featuresDirectory);

            var declarationPath = Path.Combine(sharedDirectory, "SharedState.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                namespace Demo;

                internal static class SharedState
                {
                    internal static int Count = 1;
                }
                """);

            var documentPath = Path.Combine(featuresDirectory, "DashboardPanel.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                @using Demo

                @code {
                    private int Read()
                    {
                        return SharedState.Count;
                    }
                }
                """;
            await File.WriteAllTextAsync(documentPath, text);
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            var usagePosition = GetPosition(text, "SharedState.Count", advance: "SharedState.".Length + 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 17936,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 17936);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.IsTrue(ContainsNormalizedUri(definitions, declarationPath));
            Assert.IsTrue(definitions.EnumerateArray().All(definition =>
                string.Equals(
                    Path.GetExtension(LspProtocolHelpers.ToDocumentPath(definition.GetProperty("uri").GetString()!)),
                    ".cs",
                    StringComparison.OrdinalIgnoreCase)));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_CSharpDocument_CompletionHoverAndDocumentSymbols_WorkEndToEnd()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "CounterLogic.cs");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                internal static class CounterLogic
                {
                    private static int count = 1;

                    public static int Increment()
                    {
                        cou
                        return count;
                    }
                }
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1, languageId: "csharp");

            var completionPosition = GetLastPosition(text, "cou", advance: "cou".Length);
            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 1794,
                documentUri,
                completionPosition.Line,
                completionPosition.Character);
            CollectionAssert.Contains(completionLabels, "count");

            var hoverPosition = GetPosition(text, "return count", advance: "return ".Length + 1);
            var hover = await client.RequestHoverAsync(
                requestId: 1795,
                documentUri,
                hoverPosition.Line,
                hoverPosition.Character);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "count");

            var symbols = await client.RequestDocumentSymbolsAsync(
                requestId: 1796,
                documentUri);
            CollectionAssert.AreEqual(
                new[] { "count", "Increment" },
                symbols.EnumerateArray()
                    .Select(static symbol => symbol.GetProperty("name").GetString() ?? string.Empty)
                    .ToArray());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_CSharpDocument_CompletionAndHover_UseUnopenedDiskBackedWorkspaceDeclaration()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "SharedState.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                namespace Demo;

                internal static class SharedState
                {
                    internal static int Count = 1;
                }
                """);

            var documentPath = Path.Combine(tempDirectory, "CounterConsumer.cs");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var completionText =
                """
                namespace Demo;

                internal static class CounterConsumer
                {
                    internal static int Read()
                    {
                        return SharedState.Cou
                    }
                }
                """;
            await File.WriteAllTextAsync(documentPath, completionText);
            await client.OpenDocumentAsync(documentUri, completionText, version: 1, languageId: "csharp");

            var completionPosition = GetPosition(completionText, "Cou", advance: "Cou".Length);
            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 17961,
                documentUri,
                completionPosition.Line,
                completionPosition.Character);
            CollectionAssert.Contains(completionLabels, "Count");

            var hoverText = completionText.Replace("Cou", "Count", StringComparison.Ordinal);
            await client.ChangeDocumentAsync(documentUri, hoverText, version: 2);
            var hoverPosition = GetPosition(hoverText, "Count", advance: 1);
            var hover = await client.RequestHoverAsync(
                requestId: 17962,
                documentUri,
                hoverPosition.Line,
                hoverPosition.Character);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "Count");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_VueScriptBlock_ReturnsFrontendScriptCompletionAndHover()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Host.vue");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <template>
                  <div>{{ count }}</div>
                </template>
                <script setup lang="ts">
                const count = 1;

                function increment(step: number) {
                  return count + step;
                }

                const next = increment(count);

                inc
                </script>
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1, languageId: "vue");

            var symbols = await client.RequestDocumentSymbolsAsync(
                requestId: 199,
                documentUri);
            Assert.AreEqual("Template", symbols[0].GetProperty("name").GetString());
            Assert.AreEqual("Script", symbols[1].GetProperty("name").GetString());
            var scriptChildren = symbols[1].GetProperty("children");
            CollectionAssert.AreEquivalent(
                new[] { "count", "increment", "next" },
                scriptChildren.EnumerateArray()
                    .Select(static symbol => symbol.GetProperty("name").GetString() ?? string.Empty)
                    .ToArray());

            var semanticTokens = await client.RequestSemanticTokensAsync(
                requestId: 200,
                uri: documentUri);
            AssertHasSemanticToken(semanticTokens, GetPosition(text, "count = 1"), "count".Length, "variable");
            AssertHasSemanticToken(semanticTokens, GetPosition(text, "increment(step"), "increment".Length, "method");

            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 193,
                documentUri,
                line: 12,
                character: 3);
            CollectionAssert.Contains(completionLabels, "increment");

            var hoverPosition = GetPosition(text, "increment(count)", advance: 1);
            var hover = await client.RequestHoverAsync(
                requestId: 194,
                documentUri,
                hoverPosition.Line,
                hoverPosition.Character);
            Assert.IsNotNull(hover);
            var hoverContents = hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty;
            StringAssert.Contains(hoverContents, "increment");
            StringAssert.Contains(hoverContents, "function");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 197,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = hoverPosition.Line,
                        character = hoverPosition.Character
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 197);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(documentUri, definitions[0].GetProperty("uri").GetString());
            Assert.AreEqual(6, definitions[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_VueScriptBlock_ResolvesRelativeImportSymbolsConservatively()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var importedPath = Path.Combine(tempDirectory, "label.ts");
            var importedUri = new Uri(importedPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                importedPath,
                """
                export function formatLabel(value: number) {
                  return value.toString();
                }
                """);

            var documentPath = Path.Combine(tempDirectory, "Host.vue");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <template>
                  <div>{{ label }}</div>
                </template>
                <script setup lang="ts">
                import { formatLabel } from "./label";

                const label = formatLabel(1);
                </script>
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1, languageId: "vue");

            var usagePosition = GetPosition(text, "formatLabel(1)", advance: 1);
            var hover = await client.RequestHoverAsync(
                requestId: 201,
                documentUri,
                usagePosition.Line,
                usagePosition.Character);
            Assert.IsNotNull(hover);
            var hoverContents = hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty;
            StringAssert.Contains(hoverContents, "function formatLabel(value: number)");
            StringAssert.Contains(hoverContents, "./label.ts");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 202,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 202);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(importedUri, definitions[0].GetProperty("uri").GetString());
            Assert.AreEqual(0, definitions[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 203,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 203);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.AreEqual(3, references.GetArrayLength());
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == importedUri));
            Assert.AreEqual(
                2,
                references.EnumerateArray().Count(reference => reference.GetProperty("uri").GetString() == documentUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 204,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    newName = "renderLabel"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 204);
            Assert.AreEqual(JsonValueKind.Null, renameResponse.RootElement.GetProperty("result").ValueKind);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_VueScriptBlock_ResolvesReExportedAliasAndDefaultImportSymbolsConservatively()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var importedPath = Path.Combine(tempDirectory, "format.ts");
            var importedUri = new Uri(importedPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                importedPath,
                """
                export function formatLabel(value: number) {
                  return value.toString();
                }
                """);

            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "label.ts"),
                """
                import { formatLabel } from "./format";

                export { formatLabel as renderLabel };
                export default formatLabel;
                """);

            var documentPath = Path.Combine(tempDirectory, "Host.vue");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <template>
                  <div>{{ label }}</div>
                </template>
                <script setup lang="ts">
                import formatLabel, { renderLabel } from "./label";

                const direct = formatLabel(1);
                const aliased = renderLabel(2);
                </script>
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1, languageId: "vue");

            var defaultUsagePosition = GetPosition(text, "formatLabel(1)", advance: 1);
            var defaultHover = await client.RequestHoverAsync(
                requestId: 205,
                documentUri,
                defaultUsagePosition.Line,
                defaultUsagePosition.Character);
            Assert.IsNotNull(defaultHover);
            var defaultHoverContents = defaultHover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty;
            StringAssert.Contains(defaultHoverContents, "function formatLabel(value: number)");
            StringAssert.Contains(defaultHoverContents, "./format.ts");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 206,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = defaultUsagePosition.Line,
                        character = defaultUsagePosition.Character
                    }
                }
            });
            using var defaultDefinitionResponse = await client.ReadResponseAsync(expectedId: 206);
            var defaultDefinitions = defaultDefinitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, defaultDefinitions.GetArrayLength());
            Assert.AreEqual(importedUri, defaultDefinitions[0].GetProperty("uri").GetString());
            Assert.AreEqual(0, defaultDefinitions[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 207,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = defaultUsagePosition.Line,
                        character = defaultUsagePosition.Character
                    },
                    newName = "formatMessage"
                }
            });
            using var defaultRenameResponse = await client.ReadResponseAsync(expectedId: 207);
            var defaultEdits = defaultRenameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes")
                .GetProperty(documentUri);
            Assert.AreEqual(2, defaultEdits.GetArrayLength());
            Assert.IsTrue(defaultEdits.EnumerateArray().All(edit => edit.GetProperty("newText").GetString() == "formatMessage"));

            var aliasUsagePosition = GetPosition(text, "renderLabel(2)", advance: 1);
            var aliasHover = await client.RequestHoverAsync(
                requestId: 208,
                documentUri,
                aliasUsagePosition.Line,
                aliasUsagePosition.Character);
            Assert.IsNotNull(aliasHover);
            var aliasHoverContents = aliasHover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty;
            StringAssert.Contains(aliasHoverContents, "function formatLabel(value: number)");
            StringAssert.Contains(aliasHoverContents, "./format.ts");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 209,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = aliasUsagePosition.Line,
                        character = aliasUsagePosition.Character
                    }
                }
            });
            using var aliasDefinitionResponse = await client.ReadResponseAsync(expectedId: 209);
            var aliasDefinitions = aliasDefinitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, aliasDefinitions.GetArrayLength());
            Assert.AreEqual(importedUri, aliasDefinitions[0].GetProperty("uri").GetString());
            Assert.AreEqual(0, aliasDefinitions[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 210,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = aliasUsagePosition.Line,
                        character = aliasUsagePosition.Character
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var aliasReferencesResponse = await client.ReadResponseAsync(expectedId: 210);
            var aliasReferences = aliasReferencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(aliasReferences.GetArrayLength() >= 4);
            Assert.IsTrue(aliasReferences.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == importedUri));
            Assert.AreEqual(
                2,
                aliasReferences.EnumerateArray().Count(reference => reference.GetProperty("uri").GetString() == documentUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 211,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = aliasUsagePosition.Line,
                        character = aliasUsagePosition.Character
                    },
                    newName = "renderMessage"
                }
            });
            using var aliasRenameResponse = await client.ReadResponseAsync(expectedId: 211);
            Assert.AreEqual(JsonValueKind.Null, aliasRenameResponse.RootElement.GetProperty("result").ValueKind);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_VueScriptBlock_UsesBundledTypeScriptServiceForImportedMemberCompletionAndHover()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "palette.ts"),
                """
                export function createPalette() {
                  return {
                    primary: "#ffffff",
                    secondary: "#000000",
                  };
                }
                """);

            var documentPath = Path.Combine(tempDirectory, "Host.vue");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <template>
                  <div>{{ swatch }}</div>
                </template>
                <script setup lang="ts">
                import { createPalette } from "./palette";

                const palette = createPalette();
                const swatch = palette.primary;

                palette.pr
                </script>
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1, languageId: "vue");

            var completionPosition = GetLastPosition(text, "palette.pr", advance: "palette.pr".Length);
            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 212,
                documentUri,
                completionPosition.Line,
                completionPosition.Character);
            CollectionAssert.Contains(completionLabels, "primary");

            var hoverPosition = GetPosition(text, "primary;", advance: 1);
            var hover = await client.RequestHoverAsync(
                requestId: 213,
                documentUri,
                hoverPosition.Line,
                hoverPosition.Character);
            Assert.IsNotNull(hover);
            var hoverContents = hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty;
            StringAssert.Contains(hoverContents, "primary");
            StringAssert.Contains(hoverContents, "string");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_Hover_ReturnsImportDetailsForComponentTag()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "UserCard.vue"),
                "<template><div>UserCard</div></template>");
            var text =
                """
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 2,
                method = "textDocument/hover",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 3,
                        character = 5
                    }
                }
            });
            using var response = await client.ReadMessageAsync();
            var contents = response.RootElement
                .GetProperty("result")
                .GetProperty("contents")
                .GetProperty("value")
                .GetString();
            Assert.IsNotNull(contents);
            StringAssert.Contains(contents, "UserCard");
            StringAssert.Contains(contents, "./UserCard.vue");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_Completion_ReturnsDirectiveAndTemplateItems()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "UserCard.vue"),
                "<template><div>UserCard</div></template>");
            var text =
                """
                @
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <
                </template>
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 3,
                method = "textDocument/completion",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 0,
                        character = 1
                    }
                }
            });
            using var directiveResponse = await client.ReadMessageAsync();
            var directiveLabels = directiveResponse.RootElement
                .GetProperty("result")
                .EnumerateArray()
                .Select(static item => item.GetProperty("label").GetString() ?? string.Empty)
                .ToArray();
            CollectionAssert.Contains(directiveLabels, "@code");
            CollectionAssert.DoesNotContain(directiveLabels, "@vueimport");
            CollectionAssert.DoesNotContain(directiveLabels, "@jsimport");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 4,
                method = "textDocument/completion",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 4,
                        character = 3
                    }
                }
            });
            using var templateResponse = await client.ReadMessageAsync();
            var templateLabels = templateResponse.RootElement
                .GetProperty("result")
                .EnumerateArray()
                .Select(static item => item.GetProperty("label").GetString() ?? string.Empty)
                .ToArray();
            CollectionAssert.Contains(templateLabels, "UserCard");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_Completion_FiltersTemplateItemsByTypedPrefix()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "UserCard.vue"),
                "<template><div>UserCard</div></template>");
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "ProfileCard.vue"),
                "<template><div>ProfileCard</div></template>");
            var text =
                """
                @vueimport UserCard from "./UserCard.vue"
                @vueimport ProfileCard from "./ProfileCard.vue"

                <template>
                  <Use
                </template>
                """;
            await File.WriteAllTextAsync(documentPath, text);
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            var templateLabels = await client.RequestCompletionLabelsAsync(
                requestId: 34,
                documentUri,
                line: 4,
                character: 6);

            CollectionAssert.Contains(templateLabels, "UserCard");
            CollectionAssert.DoesNotContain(templateLabels, "ProfileCard");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_DidChangeAndDidClose_UpdateObservableProjectionBackedResults()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "UserCard.vue"),
                "<template><div>UserCard</div></template>");
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "ProfileCard.vue"),
                "<template><div>ProfileCard</div></template>");

            var diskText =
                """
                <UserCard />
                """;
            await File.WriteAllTextAsync(documentPath, diskText);
            await client.OpenDocumentAsync(documentUri, diskText, version: 1);

            var initialHover = await client.RequestHoverAsync(
                requestId: 31,
                documentUri,
                line: 0,
                character: 1);
            Assert.IsNotNull(initialHover);
            StringAssert.Contains(
                initialHover.Value.GetProperty("contents").GetProperty("value").GetString(),
                "./UserCard.vue");

            var updatedText =
                """
                <ProfileCard />
                """;
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var updatedHover = await client.RequestHoverAsync(
                requestId: 32,
                documentUri,
                line: 0,
                character: 1);
            Assert.IsNotNull(updatedHover);
            StringAssert.Contains(
                updatedHover.Value.GetProperty("contents").GetProperty("value").GetString(),
                "./ProfileCard.vue");

            await client.CloseDocumentAsync(documentUri);

            var afterCloseHover = await client.RequestHoverAsync(
                requestId: 33,
                documentUri,
                line: 0,
                character: 1);
            Assert.IsNotNull(afterCloseHover);
            StringAssert.Contains(
                afterCloseHover.Value.GetProperty("contents").GetProperty("value").GetString(),
                "./UserCard.vue");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_TrackedVueWorkspaceDocument_SupportsCompletionHoverAndDefinition()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var sharedDirectory = Path.Combine(tempDirectory, "Shared");
            var pagesDirectory = Path.Combine(tempDirectory, "Pages");
            Directory.CreateDirectory(sharedDirectory);
            Directory.CreateDirectory(pagesDirectory);

            var vuePath = Path.Combine(sharedDirectory, "UserBadge.vue");
            var vueUri = new Uri(vuePath).AbsoluteUri;
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = vueUri,
                        languageId = "vue",
                        version = 1,
                        text = "<template><div>UserBadge</div></template>"
                    }
                }
            });
            using var trackedVueDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", trackedVueDiagnostics.RootElement.GetProperty("method").GetString());

            var documentPath = Path.Combine(pagesDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <Use
                <UserBadge />
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 170,
                documentUri,
                line: 0,
                character: 4);
            CollectionAssert.Contains(completionLabels, "UserBadge");

            var hover = await client.RequestHoverAsync(
                requestId: 171,
                documentUri,
                line: 1,
                character: 5);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "../Shared/UserBadge.vue");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 172,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 5
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 172);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(vueUri, definitions[0].GetProperty("uri").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_TrackedVueWorkspaceDocument_SuppressesMissingComponentDiagnostics_AndReappearsOnClose()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var pagesDirectory = Path.Combine(tempDirectory, "Pages");
            var sharedDirectory = Path.Combine(tempDirectory, "Shared");
            Directory.CreateDirectory(pagesDirectory);
            Directory.CreateDirectory(sharedDirectory);

            var componentName = "MissingBadge" + Guid.NewGuid().ToString("N")[..8];
            var documentPath = Path.Combine(pagesDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await client.SendAsync(new
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
                        text = $"<{componentName} />"
                    }
                }
            });
            using var unresolvedDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual(documentUri, unresolvedDiagnostics.RootElement.GetProperty("params").GetProperty("uri").GetString());
            Assert.IsTrue(unresolvedDiagnostics.RootElement
                .GetProperty("params")
                .GetProperty("diagnostics")
                .EnumerateArray()
                .Any(diagnostic => diagnostic.GetProperty("code").GetString() == "JAZORVUEFRONTEND001"));

            var vuePath = Path.Combine(sharedDirectory, componentName + ".vue");
            var vueUri = new Uri(vuePath).AbsoluteUri;
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = vueUri,
                        languageId = "vue",
                        version = 1,
                        text = $"<template><div>{componentName}</div></template>"
                    }
                }
            });
            using var trackedVueDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual(vueUri, trackedVueDiagnostics.RootElement.GetProperty("params").GetProperty("uri").GetString());

            using var resolvedDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual(documentUri, resolvedDiagnostics.RootElement.GetProperty("params").GetProperty("uri").GetString());
            Assert.AreEqual(0, resolvedDiagnostics.RootElement.GetProperty("params").GetProperty("diagnostics").GetArrayLength());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didClose",
                @params = new
                {
                    textDocument = new
                    {
                        uri = vueUri
                    }
                }
            });
            using var closedVueDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual(vueUri, closedVueDiagnostics.RootElement.GetProperty("params").GetProperty("uri").GetString());
            Assert.AreEqual(0, closedVueDiagnostics.RootElement.GetProperty("params").GetProperty("diagnostics").GetArrayLength());

            using var reintroducedDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual(documentUri, reintroducedDiagnostics.RootElement.GetProperty("params").GetProperty("uri").GetString());
            Assert.IsTrue(reintroducedDiagnostics.RootElement
                .GetProperty("params")
                .GetProperty("diagnostics")
                .EnumerateArray()
                .Any(diagnostic => diagnostic.GetProperty("code").GetString() == "JAZORVUEFRONTEND001"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_JazorDocument_ResolvesWorkspaceVueComponentOutsideNearbyDirectories()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var sharedDirectory = Path.Combine(tempDirectory, "Shared", "UI");
            var pagesDirectory = Path.Combine(tempDirectory, "Pages", "Admin");
            Directory.CreateDirectory(sharedDirectory);
            Directory.CreateDirectory(pagesDirectory);

            var vuePath = Path.Combine(sharedDirectory, "UserBadge.vue");
            var vueUri = new Uri(vuePath).AbsoluteUri;
            await File.WriteAllTextAsync(vuePath, "<template><div>UserBadge</div></template>");

            var documentPath = Path.Combine(pagesDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await client.SendAsync(new
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
                        text = "<UserBadge />"
                    }
                }
            });
            using var diagnosticsMessage = await client.ReadMessageAsync();
            Assert.AreEqual(documentUri, diagnosticsMessage.RootElement.GetProperty("params").GetProperty("uri").GetString());
            Assert.AreEqual(0, diagnosticsMessage.RootElement.GetProperty("params").GetProperty("diagnostics").GetArrayLength());

            var hover = await client.RequestHoverAsync(
                requestId: 179,
                documentUri,
                line: 0,
                character: 5);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "../../Shared/UI/UserBadge.vue");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 180,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 0,
                        character = 5
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 180);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(vueUri, definitions[0].GetProperty("uri").GetString());

            await client.ShutdownAsync();
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_VueDocument_ReferencesAndRename_IncludeOpenUnsavedJazorDocuments_WithoutTouchingCodeRegion()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "UserBadge.vue");
            var declarationUri = new Uri(declarationPath).AbsoluteUri;
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = declarationUri,
                        languageId = "vue",
                        version = 1,
                        text = "<template><div>UserBadge</div></template>"
                    }
                }
            });
            using var declarationDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual(declarationUri, declarationDiagnostics.RootElement.GetProperty("params").GetProperty("uri").GetString());

            var hostPath = Path.Combine(tempDirectory, "Host.vue");
            var hostUri = new Uri(hostPath).AbsoluteUri;
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri,
                        languageId = "vue",
                        version = 1,
                        text =
                        """
                        <template>
                          <UserBadge />
                        </template>
                        """
                    }
                }
            });
            using var hostDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual(hostUri, hostDiagnostics.RootElement.GetProperty("params").GetProperty("uri").GetString());

            var counterPath = Path.Combine(tempDirectory, "Counter.jazor");
            var counterUri = new Uri(counterPath).AbsoluteUri;
            await client.OpenDocumentAsync(
                counterUri,
                """
                <UserBadge />

                @code {
                    private void UserBadge()
                    {
                    }
                }
                """,
                version: 1);

            var dashboardPath = Path.Combine(tempDirectory, "Dashboard.jazor");
            var dashboardUri = new Uri(dashboardPath).AbsoluteUri;
            await client.OpenDocumentAsync(
                dashboardUri,
                """
                <section>
                  <UserBadge />
                </section>

                @code {
                    private string UserBadge => nameof(UserBadge);
                }
                """,
                version: 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 177,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 5
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 177);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == declarationUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == hostUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == counterUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == dashboardUri));
            Assert.IsFalse(references.EnumerateArray().Any(reference =>
                reference.GetProperty("uri").GetString() == counterUri
                && reference.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() >= 3));
            Assert.IsFalse(references.EnumerateArray().Any(reference =>
                reference.GetProperty("uri").GetString() == dashboardUri
                && reference.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() >= 4));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 178,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 5
                    },
                    newName = "ProfileBadge"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 178);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == hostUri));
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == counterUri));
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == dashboardUri));
            Assert.AreEqual(1, changes.GetProperty(counterUri).GetArrayLength());
            Assert.AreEqual(1, changes.GetProperty(dashboardUri).GetArrayLength());
            Assert.AreEqual(0, changes.GetProperty(counterUri)[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());
            Assert.AreEqual(1, changes.GetProperty(dashboardUri)[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());
            Assert.AreEqual("ProfileBadge", changes.GetProperty(counterUri)[0].GetProperty("newText").GetString());
            Assert.AreEqual("ProfileBadge", changes.GetProperty(dashboardUri)[0].GetProperty("newText").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_VueDocument_ReferencesAndRename_IncludeWorkspaceDiskJazorDocumentsOutsideNearbyDirectories()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var sharedDirectory = Path.Combine(tempDirectory, "Shared", "UI");
            var appDirectory = Path.Combine(tempDirectory, "App");
            var pagesDirectory = Path.Combine(tempDirectory, "Pages", "Admin");
            var reportsDirectory = Path.Combine(tempDirectory, "Features", "Reports");
            Directory.CreateDirectory(sharedDirectory);
            Directory.CreateDirectory(appDirectory);
            Directory.CreateDirectory(pagesDirectory);
            Directory.CreateDirectory(reportsDirectory);

            var declarationPath = Path.Combine(sharedDirectory, "UserBadge.vue");
            await File.WriteAllTextAsync(declarationPath, "<template><div>UserBadge</div></template>");

            var counterPath = Path.Combine(pagesDirectory, "Counter.jazor");
            var counterUri = new Uri(counterPath).AbsoluteUri;
            await File.WriteAllTextAsync(counterPath, "<UserBadge />");

            var dashboardPath = Path.Combine(reportsDirectory, "Dashboard.jazor");
            var dashboardUri = new Uri(dashboardPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                dashboardPath,
                """
                <section>
                  <UserBadge />
                </section>
                """);

            var hostPath = Path.Combine(appDirectory, "Host.vue");
            var hostUri = new Uri(hostPath).AbsoluteUri;
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri,
                        languageId = "vue",
                        version = 1,
                        text =
                        """
                        <template>
                          <UserBadge />
                        </template>
                        """
                    }
                }
            });
            using var diagnosticsMessage = await client.ReadMessageAsync();
            Assert.AreEqual(hostUri, diagnosticsMessage.RootElement.GetProperty("params").GetProperty("uri").GetString());

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 181,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 5
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 181);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == hostUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == counterUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == dashboardUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 182,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = hostUri
                    },
                    position = new
                    {
                        line = 1,
                        character = 5
                    },
                    newName = "ProfileBadge"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 182);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == hostUri));
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == counterUri));
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == dashboardUri));
            Assert.AreEqual("ProfileBadge", changes.GetProperty(counterUri)[0].GetProperty("newText").GetString());
            Assert.AreEqual("ProfileBadge", changes.GetProperty(dashboardUri)[0].GetProperty("newText").GetString());

            await client.ShutdownAsync();
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_ReferencesAndRename_IncludeOtherOpenJazorDocuments()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var sharedDirectory = Path.Combine(tempDirectory, "Shared");
            Directory.CreateDirectory(sharedDirectory);
            var vuePath = Path.Combine(sharedDirectory, "UserBadge.vue");
            var vueUri = new Uri(vuePath).AbsoluteUri;
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri = vueUri,
                        languageId = "vue",
                        version = 1,
                        text = "<template><div>UserBadge</div></template>"
                    }
                }
            });
            using var trackedVueDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", trackedVueDiagnostics.RootElement.GetProperty("method").GetString());

            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await client.OpenDocumentAsync(
                documentUri,
                """
                <UserBadge />
                """,
                version: 1);

            var secondDocumentPath = Path.Combine(tempDirectory, "Dashboard.jazor");
            var secondDocumentUri = new Uri(secondDocumentPath).AbsoluteUri;
            await client.OpenDocumentAsync(
                secondDocumentUri,
                """
                <section>
                  <UserBadge />
                </section>
                """,
                version: 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 173,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 0,
                        character = 5
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 173);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == secondDocumentUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == vueUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 174,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 0,
                        character = 5
                    },
                    newName = "ProfileBadge"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 174);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == documentUri));
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == secondDocumentUri));
            Assert.AreEqual(
                "ProfileBadge",
                changes.GetProperty(secondDocumentUri)[0].GetProperty("newText").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_Definition_ReturnsResolvedImportTarget()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var vuePath = Path.Combine(tempDirectory, "UserCard.vue");
            await File.WriteAllTextAsync(vuePath, "<template><div>UserCard</div></template>");
            var text =
                """
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 5,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 3,
                        character = 5
                    }
                }
            });
            using var response = await client.ReadMessageAsync();
            var locations = response.RootElement.GetProperty("result");
            Assert.AreEqual(1, locations.GetArrayLength());
            Assert.AreEqual(new Uri(vuePath).AbsoluteUri, locations[0].GetProperty("uri").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_DefaultFallbackAnalysis_SupportsDiagnosticsHoverAndDefinition()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var vuePath = Path.Combine(tempDirectory, "UserCard.vue");
            await File.WriteAllTextAsync(vuePath, "<template><div>UserCard</div></template>");
            var text =
                """
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <UserCard />
                </template>

                @code {
                    private void Hidden()
                    {
                    }
                }
                """;

            await client.SendAsync(new
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
                        text
                    }
                }
            });
            using var diagnosticsMessage = await client.ReadMessageAsync();
            var diagnostics = diagnosticsMessage.RootElement
                .GetProperty("params")
                .GetProperty("diagnostics");
            Assert.AreEqual(1, diagnostics.GetArrayLength());
            Assert.AreEqual("JAZORVUE001", diagnostics[0].GetProperty("code").GetString());

            var hover = await client.RequestHoverAsync(
                requestId: 34,
                documentUri,
                line: 3,
                character: 5);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "./UserCard.vue");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 35,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 3,
                        character = 5
                    }
                }
            });
            using var definitionResponse = await client.ReadMessageAsync();
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(new Uri(vuePath).AbsoluteUri, definitions[0].GetProperty("uri").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_RoutesDirectiveTemplateAndCodeRegionsThroughDistinctObservableBehaviors()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "UserCard.vue"),
                "<template><div>UserCard</div></template>");
            var text =
                """
                @
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <
                  <UserCard />
                </template>

                @code {
                    void Render()
                    {
                        UserCard();
                    }
                }
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            var directiveLabels = await client.RequestCompletionLabelsAsync(
                requestId: 41,
                documentUri,
                line: 0,
                character: 1);
            CollectionAssert.Contains(directiveLabels, "@code");
            CollectionAssert.DoesNotContain(directiveLabels, "UserCard");

            var templateLabels = await client.RequestCompletionLabelsAsync(
                requestId: 42,
                documentUri,
                line: 4,
                character: 3);
            CollectionAssert.Contains(templateLabels, "UserCard");
            CollectionAssert.DoesNotContain(templateLabels, "@vueimport");

            var codeHover = await client.RequestHoverAsync(
                requestId: 43,
                documentUri,
                line: 10,
                character: 10);
            Assert.IsNull(codeHover);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_TemplateAndCodePositions_RemainFeatureCapableAfterLaneDispatch()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "UserCard.vue"),
                "<template><div>UserCard</div></template>");
            var text =
                """
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <UserCard />
                </template>

                @code {
                    private void Hidden()
                    {
                    }
                }
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 44,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 3,
                        character = 5
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadMessageAsync();
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(references.GetArrayLength() >= 2);
            Assert.IsTrue(references.EnumerateArray().Any(reference =>
                reference.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() == 3));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 45,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 3,
                        character = 5
                    },
                    newName = "ProfileCard"
                }
            });
            using var renameResponse = await client.ReadMessageAsync();
            var templateRenameEdits = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes")
                .GetProperty(documentUri);
            Assert.AreEqual(1, templateRenameEdits.GetArrayLength());
            Assert.AreEqual(
                3,
                templateRenameEdits[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());

            var diagnostic = await client.ChangeAndReadFirstDiagnosticAsync(documentUri, text, version: 2);
            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 46,
                method = "textDocument/codeAction",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    range = new
                    {
                        start = new
                        {
                            line = 6,
                            character = 4
                        },
                        end = new
                        {
                            line = 8,
                            character = 5
                        }
                    },
                    context = new
                    {
                        diagnostics = new[] { JsonSerializer.Deserialize<object>(diagnostic.GetRawText(), JsonOptions)! }
                    }
                }
            });
            using var codeActionResponse = await client.ReadMessageAsync();
            var actions = codeActionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, actions.GetArrayLength());
            Assert.AreEqual("Make method public for bridge lowering", actions[0].GetProperty("title").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_CodeRegion_UsesInProcRoslynForCompletionHoverAndDefinition()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                @page "/counter"

                @code {
                    private int count = 1;

                    public int Increment()
                    {
                        cou
                        return count;
                    }
                }
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 600,
                documentUri,
                line: 7,
                character: 11);
            CollectionAssert.Contains(completionLabels, "count");

            var hover = await client.RequestHoverAsync(
                requestId: 601,
                documentUri,
                line: 8,
                character: 17);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "count");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 602,
                method = "textDocument/definition",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 8,
                        character = 17
                    }
                }
            });
            using var definitionResponse = await client.ReadResponseAsync(expectedId: 602);
            var definitions = definitionResponse.RootElement.GetProperty("result");
            Assert.AreEqual(1, definitions.GetArrayLength());
            Assert.AreEqual(documentUri, definitions[0].GetProperty("uri").GetString());
            Assert.AreEqual(3, definitions[0].GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_SignatureHelp_InCodeBlock_TracksActiveParameterAcrossInvocationArguments()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                @page "/counter"

                @code {
                    private static string FormatValue(int count, string prefix, bool includeUnits)
                        => string.Empty;

                    public string Render()
                    {
                        return FormatValue(1, "draft", true);
                    }
                }
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            var firstArgumentPosition = GetPosition(text, "FormatValue(1", advance: "FormatValue(".Length);
            var secondArgumentPosition = GetPosition(text, "\"draft\"", advance: 1);
            var thirdArgumentPosition = GetPosition(text, "true", advance: 1);

            var firstArgumentHelp = await client.RequestSignatureHelpAsync(
                requestId: 603,
                documentUri,
                firstArgumentPosition.Line,
                firstArgumentPosition.Character);
            var secondArgumentHelp = await client.RequestSignatureHelpAsync(
                requestId: 604,
                documentUri,
                secondArgumentPosition.Line,
                secondArgumentPosition.Character);
            var thirdArgumentHelp = await client.RequestSignatureHelpAsync(
                requestId: 605,
                documentUri,
                thirdArgumentPosition.Line,
                thirdArgumentPosition.Character);

            AssertSignatureHelp(firstArgumentHelp, expectedActiveParameter: 0);
            AssertSignatureHelp(secondArgumentHelp, expectedActiveParameter: 1);
            AssertSignatureHelp(thirdArgumentHelp, expectedActiveParameter: 2);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_DocumentSymbols_AggregateJazorStructureAndCodeMembers()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                @page "/counter"

                <template>
                  <UserCard />
                </template>

                @code {
                    private int count;
                    public int Total => count;

                    public void Increment()
                    {
                    }
                }
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            var symbols = await client.RequestDocumentSymbolsAsync(
                requestId: 606,
                documentUri);
            Assert.AreEqual(5, symbols.GetArrayLength());

            Assert.AreEqual("Template", symbols[0].GetProperty("name").GetString());
            var templateChildren = symbols[0].GetProperty("children");
            Assert.AreEqual(1, templateChildren.GetArrayLength());
            Assert.AreEqual("UserCard", templateChildren[0].GetProperty("name").GetString());

            Assert.AreEqual("Code", symbols[1].GetProperty("name").GetString());
            CollectionAssert.AreEqual(
                new[] { "count", "Total", "Increment" },
                symbols.EnumerateArray()
                    .Skip(2)
                    .Select(static symbol => symbol.GetProperty("name").GetString() ?? string.Empty)
                    .ToArray());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_CodeRegion_ReferencesAndRename_StayInsideCodeLane()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "UserCard.vue"),
                "<template><div>UserCard</div></template>");
            var text =
                """
                @page "/counter"

                <UserCard />

                @code {
                    private int UserCard = 1;

                    public int Increment()
                    {
                        UserCard++;
                        return UserCard;
                    }
                }
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 603,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 9,
                        character = 12
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 603);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.AreEqual(3, references.GetArrayLength());
            Assert.IsTrue(references.EnumerateArray().All(reference => reference.GetProperty("uri").GetString() == documentUri));
            Assert.IsFalse(references.EnumerateArray().Any(reference =>
                reference.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() == 2));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 604,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 9,
                        character = 12
                    },
                    newName = "TotalCount"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 604);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes")
                .GetProperty(documentUri);
            Assert.AreEqual(3, changes.GetArrayLength());
            Assert.IsTrue(changes.EnumerateArray().All(change => change.GetProperty("newText").GetString() == "TotalCount"));
            Assert.IsFalse(changes.EnumerateArray().Any(change =>
                change.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() == 2));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_CodeRegion_ReferencesAndRename_WorkAcrossOpenJazorDocuments()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "SharedSource.jazor");
            var declarationUri = new Uri(declarationPath).AbsoluteUri;
            var declarationText =
                """
                @code {
                    public static int Shared => 42;
                }
                """;
            await client.OpenDocumentAsync(declarationUri, declarationText, version: 1);

            var generatedTypeName = GetProjectedComponentTypeName(new DocumentSnapshot(
                declarationPath,
                DocumentKind.Jazor,
                declarationText,
                "1"));
            var referencePath = Path.Combine(tempDirectory, "SharedConsumer.jazor");
            var referenceUri = new Uri(referencePath).AbsoluteUri;
            var referenceText =
                "@code {\n" +
                "    private int Read()\n" +
                "    {\n" +
                $"        return {generatedTypeName}.Shared + {generatedTypeName}.Shared;\n" +
                "    }\n" +
                "}\n";
            await client.OpenDocumentAsync(referenceUri, referenceText, version: 1);

            var sharedPosition = GetPosition(referenceText, ".Shared", advance: 2);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 605,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = referenceUri
                    },
                    position = new
                    {
                        line = sharedPosition.Line,
                        character = sharedPosition.Character
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 605);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == declarationUri));
            Assert.IsTrue(references.EnumerateArray().Any(reference => reference.GetProperty("uri").GetString() == referenceUri));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 606,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = referenceUri
                    },
                    position = new
                    {
                        line = sharedPosition.Line,
                        character = sharedPosition.Character
                    },
                    newName = "Total"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 606);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == declarationUri));
            Assert.IsTrue(changes.EnumerateObject().Any(change => change.Name == referenceUri));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_CodeBehindFile_ReferencesAndRename_StayInsideRoslynLane_WithoutMarkupBridge()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var vuePath = Path.Combine(tempDirectory, "UserCard.vue");
            await File.WriteAllTextAsync(vuePath, "<template><div>UserCard</div></template>");

            var jazorPath = Path.Combine(tempDirectory, "Counter.jazor");
            var jazorUri = new Uri(jazorPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                jazorPath,
                """
                <UserCard />
                """);

            var codeBehindPath = Path.Combine(tempDirectory, "Counter.jazor.cs");
            var codeBehindUri = new Uri(codeBehindPath).AbsoluteUri;
            var codeBehindText =
                """
                public partial class Counter
                {
                    private int UserCard = 1;

                    public int Read()
                    {
                        UserCard++;
                        return UserCard;
                    }
                }
                """;
            var usagePosition = GetPosition(codeBehindText, "UserCard++;", advance: 1);
            await client.OpenDocumentAsync(codeBehindUri, codeBehindText, version: 1, languageId: "csharp");

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 607,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = codeBehindUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadResponseAsync(expectedId: 607);
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(references.GetArrayLength() >= 3);
            Assert.IsTrue(references.EnumerateArray().All(reference =>
                string.Equals(reference.GetProperty("uri").GetString(), codeBehindUri, StringComparison.Ordinal)));

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 608,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = codeBehindUri
                    },
                    position = new
                    {
                        line = usagePosition.Line,
                        character = usagePosition.Character
                    },
                    newName = "TotalCount"
                }
            });
            using var renameResponse = await client.ReadResponseAsync(expectedId: 608);
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes");
            Assert.IsTrue(changes.EnumerateObject().All(change =>
                string.Equals(change.Name, codeBehindUri, StringComparison.Ordinal)));
            Assert.IsFalse(changes.EnumerateObject().Any(change =>
                string.Equals(change.Name, jazorUri, StringComparison.Ordinal)));
            Assert.IsTrue(changes.GetProperty(codeBehindUri).EnumerateArray().All(change =>
                string.Equals(change.GetProperty("newText").GetString(), "TotalCount", StringComparison.Ordinal)));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_ReferencesAndRename_ReturnWorkspaceLocationsAndEdits()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            await File.WriteAllTextAsync(
                Path.Combine(tempDirectory, "UserCard.vue"),
                "<template><div>UserCard</div></template>");
            var text =
                """
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """;
            await client.OpenDocumentAsync(documentUri, text, version: 1);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 6,
                method = "textDocument/references",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 3,
                        character = 5
                    },
                    context = new
                    {
                        includeDeclaration = true
                    }
                }
            });
            using var referencesResponse = await client.ReadMessageAsync();
            var references = referencesResponse.RootElement.GetProperty("result");
            Assert.IsTrue(references.GetArrayLength() >= 2);

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 7,
                method = "textDocument/rename",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    position = new
                    {
                        line = 3,
                        character = 5
                    },
                    newName = "ProfileCard"
                }
            });
            using var renameResponse = await client.ReadMessageAsync();
            var changes = renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes")
                .GetProperty(documentUri);
            Assert.AreEqual(1, changes.GetArrayLength());
            Assert.AreEqual("ProfileCard", changes[0].GetProperty("newText").GetString());
            Assert.IsTrue(renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes")
                .EnumerateObject()
                .Any(change => change.Name == documentUri));

            var startOffsets = changes
                .EnumerateArray()
                .Select(change => ToOffset(text, change.GetProperty("range").GetProperty("start")))
                .ToArray();
            CollectionAssert.AreEqual(
                startOffsets.OrderByDescending(static offset => offset).ToArray(),
                startOffsets);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_CodeAction_ReturnsQuickFixForPrivateMethodDiagnostic()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <template>
                  <div />
                </template>

                @code {
                    private void Hidden()
                    {
                    }
                }
                """;
            await client.SendAsync(new
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
                        text
                    }
                }
            });
            using var diagnosticsMessage = await client.ReadMessageAsync();
            var diagnostic = diagnosticsMessage.RootElement
                .GetProperty("params")
                .GetProperty("diagnostics")[0];

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 8,
                method = "textDocument/codeAction",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    range = diagnostic.GetProperty("range"),
                    context = new
                    {
                        diagnostics = new[] { JsonSerializer.Deserialize<object>(diagnostic.GetRawText(), JsonOptions)! }
                    }
                }
            });
            using var response = await client.ReadMessageAsync();
            var actions = response.RootElement.GetProperty("result");
            Assert.AreEqual(1, actions.GetArrayLength());
            Assert.AreEqual("Make method public for bridge lowering", actions[0].GetProperty("title").GetString());
            var editChanges = actions[0]
                .GetProperty("edit")
                .GetProperty("changes");
            Assert.AreEqual(documentUri, editChanges.EnumerateObject().Single().Name);
            Assert.AreEqual("public", editChanges
                .GetProperty(documentUri)[0]
                .GetProperty("newText")
                .GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_CodeAction_DoesNotOfferLegacyVueImportQuickFix()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var componentsDirectory = Path.Combine(tempDirectory, "Components");
            Directory.CreateDirectory(componentsDirectory);
            var componentPath = Path.Combine(componentsDirectory, "MissingCard.vue");
            await File.WriteAllTextAsync(componentPath, "<template><div>MissingCard</div></template>");

            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                <template>
                  <UnknownCard />
                </template>
                """;
            await client.SendAsync(new
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
                        text
                    }
                }
            });
            using var openDiagnostics = await client.ReadMessageAsync();
            Assert.AreEqual("textDocument/publishDiagnostics", openDiagnostics.RootElement.GetProperty("method").GetString());

            var diagnostic = new
            {
                range = new
                {
                    start = new { line = 1, character = 3 },
                    end = new { line = 1, character = 14 }
                },
                severity = 2,
                code = "JAZORVUEFRONTEND001",
                source = "Jazor.VueHost.Frontend",
                message = "Razor component 'UnknownCard' could not be resolved to a nearby Vue file."
            };

            await client.SendAsync(new
            {
                jsonrpc = "2.0",
                id = 108,
                method = "textDocument/codeAction",
                @params = new
                {
                    textDocument = new
                    {
                        uri = documentUri
                    },
                    range = diagnostic.range,
                    context = new
                    {
                        diagnostics = new[] { diagnostic }
                    }
                }
            });
            using var response = await client.ReadMessageAsync();
            var actions = response.RootElement.GetProperty("result");
            Assert.AreEqual(0, actions.GetArrayLength());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_JazorCode_CompletionHoverAndSignatureHelp_UseUnopenedDiskBackedCSharpDeclaration()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "SharedState.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                namespace Demo;

                internal static class SharedState
                {
                    internal static int Count = 1;

                    internal static string FormatValue(int count, string prefix, bool includeUnits)
                        => $"{prefix}:{count}:{includeUnits}";
                }
                """);

            var completionPath = Path.Combine(tempDirectory, "CounterCompletion.jazor");
            var completionUri = new Uri(completionPath).AbsoluteUri;
            var completionText =
                """
                @using Demo

                @code {
                    private string Render()
                    {
                        return SharedState.Cou
                    }
                }
                """;
            await File.WriteAllTextAsync(completionPath, completionText);
            await client.OpenDocumentAsync(completionUri, completionText, version: 1);

            var completionPosition = GetPosition(completionText, "Cou", advance: "Cou".Length);
            var completionLabels = await client.RequestCompletionLabelsAsync(
                requestId: 17971,
                completionUri,
                completionPosition.Line,
                completionPosition.Character);
            CollectionAssert.Contains(completionLabels, "Count");

            var hoverPath = Path.Combine(tempDirectory, "CounterHover.jazor");
            var hoverUri = new Uri(hoverPath).AbsoluteUri;
            var hoverText =
                """
                @using Demo

                @code {
                    private int Render()
                    {
                        return SharedState.Count;
                    }
                }
                """;
            await File.WriteAllTextAsync(hoverPath, hoverText);
            await client.OpenDocumentAsync(hoverUri, hoverText, version: 1);
            var hoverPosition = GetPosition(hoverText, "SharedState.Count", advance: "SharedState.".Length + 1);
            var hover = await client.RequestHoverAsync(
                requestId: 17972,
                hoverUri,
                hoverPosition.Line,
                hoverPosition.Character);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "Count");

            var signaturePath = Path.Combine(tempDirectory, "CounterSignature.jazor");
            var signatureUri = new Uri(signaturePath).AbsoluteUri;
            var signatureText =
                """
                @using Demo

                @code {
                    private string Render()
                    {
                        return SharedState.FormatValue(1, "draft", true);
                    }
                }
                """;
            await File.WriteAllTextAsync(signaturePath, signatureText);
            await client.OpenDocumentAsync(signatureUri, signatureText, version: 1);

            var firstArgumentPosition = GetPosition(signatureText, "FormatValue(1", advance: "FormatValue(".Length);
            var secondArgumentPosition = GetPosition(signatureText, "\"draft\"", advance: 1);
            var thirdArgumentPosition = GetPosition(signatureText, "true", advance: 1);

            var firstArgumentHelp = await client.RequestSignatureHelpAsync(
                requestId: 17973,
                signatureUri,
                firstArgumentPosition.Line,
                firstArgumentPosition.Character);
            var secondArgumentHelp = await client.RequestSignatureHelpAsync(
                requestId: 17974,
                signatureUri,
                secondArgumentPosition.Line,
                secondArgumentPosition.Character);
            var thirdArgumentHelp = await client.RequestSignatureHelpAsync(
                requestId: 17975,
                signatureUri,
                thirdArgumentPosition.Line,
                thirdArgumentPosition.Character);

            AssertSignatureHelp(firstArgumentHelp, expectedActiveParameter: 0);
            AssertSignatureHelp(secondArgumentHelp, expectedActiveParameter: 1);
            AssertSignatureHelp(thirdArgumentHelp, expectedActiveParameter: 2);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_JazorCode_Diagnostics_UseUnopenedDiskBackedCSharpDeclaration()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "SharedState.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                namespace Demo;

                internal static class SharedState
                {
                    internal static int Count = 1;
                }
                """);

            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                @using Demo

                @code {
                    public int Render()
                    {
                        return SharedState.Count;
                    }
                }
                """;

            await client.OpenDocumentAsync(documentUri, text, version: 1);
            using var diagnosticsMessage = await client.ReadMessageAsync();
            Assert.AreEqual(documentUri, diagnosticsMessage.RootElement.GetProperty("params").GetProperty("uri").GetString());
            Assert.IsFalse(diagnosticsMessage.RootElement
                .GetProperty("params")
                .GetProperty("diagnostics")
                .EnumerateArray()
                .Any(diagnostic =>
                    string.Equals(diagnostic.GetProperty("code").GetString(), "CS0103", StringComparison.Ordinal)
                    || string.Equals(diagnostic.GetProperty("code").GetString(), "CS0246", StringComparison.Ordinal)
                    || (diagnostic.GetProperty("message").GetString() ?? string.Empty).Contains("SharedState", StringComparison.Ordinal)
                    || (diagnostic.GetProperty("message").GetString() ?? string.Empty).Contains("Count", StringComparison.Ordinal)));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    [TestMethod]
    public async Task JazorVueHost_Lsp_LegacyVueImportText_DoesNotProduceImportRewriteDiagnostics()
    {
        await using var client = await LspTestClient.StartAsync();
        await client.InitializeAsync();

        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var componentsDirectory = Path.Combine(tempDirectory, "Components");
            Directory.CreateDirectory(componentsDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(componentsDirectory, "MissingCard.vue"),
                "<template><div>MissingCard</div></template>");

            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var documentUri = new Uri(documentPath).AbsoluteUri;
            var text =
                """
                @vueimport MissingCard from "./MissingCard.vue"

                <template>
                  <MissingCard />
                </template>
                """;
            await client.SendAsync(new
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
                        text
                    }
                }
            });
            using var openDiagnostics = await client.ReadMessageAsync();
            var diagnostics = openDiagnostics.RootElement
                .GetProperty("params")
                .GetProperty("diagnostics");
            Assert.AreEqual(0, diagnostics.GetArrayLength());

            var hover = await client.RequestHoverAsync(
                requestId: 109,
                documentUri,
                line: 3,
                character: 5);
            Assert.IsNotNull(hover);
            StringAssert.Contains(
                hover.Value.GetProperty("contents").GetProperty("value").GetString() ?? string.Empty,
                "./Components/MissingCard.vue");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }

        await client.ShutdownAsync();
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-vuehost-lsp-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static Uri CreateWebSocketUri(int port, string path)
        => new UriBuilder(Uri.UriSchemeWs, "127.0.0.1", port, path).Uri;

    private static void DeleteDirectoryWithRetries(string path)
        => Directory.Delete(path, recursive: true);

    private static bool ContainsNormalizedUri(JsonElement locations, string documentPath)
        => locations.EnumerateArray().Any(location =>
            string.Equals(
                VueHostWorkspaceResolver.NormalizePath(LspProtocolHelpers.ToDocumentPath(location.GetProperty("uri").GetString()!)),
                VueHostWorkspaceResolver.NormalizePath(documentPath),
                StringComparison.OrdinalIgnoreCase));

    private static bool ContainsNormalizedChange(JsonElement changes, string documentPath)
        => changes.EnumerateObject().Any(change =>
            string.Equals(
                VueHostWorkspaceResolver.NormalizePath(LspProtocolHelpers.ToDocumentPath(change.Name)),
                VueHostWorkspaceResolver.NormalizePath(documentPath),
                StringComparison.OrdinalIgnoreCase));

    private static JsonElement GetChangeEntry(JsonElement changes, string documentPath)
    {
        foreach (var change in changes.EnumerateObject())
        {
            if (string.Equals(
                    VueHostWorkspaceResolver.NormalizePath(LspProtocolHelpers.ToDocumentPath(change.Name)),
                    VueHostWorkspaceResolver.NormalizePath(documentPath),
                    StringComparison.OrdinalIgnoreCase))
            {
                return change.Value;
            }
        }

        Assert.Fail($"Expected changes to contain '{documentPath}'.");
        return default;
    }

    private static string GetProjectedComponentTypeName(DocumentSnapshot document)
    {
        var projectionService = new RazorDesignTimeCodeProjectionService();
        Assert.IsTrue(projectionService.TryCreateProjection(document, out var projection));

        var namespaceMatches = Regex.Matches(
            projection.SourceText,
            @"namespace\s+(?<name>[A-Za-z0-9_.]+)",
            RegexOptions.CultureInvariant);
        var classMatches = Regex.Matches(
            projection.SourceText,
            @"public\s+partial\s+class\s+(?<name>[A-Za-z0-9_]+)",
            RegexOptions.CultureInvariant);
        Assert.IsTrue(namespaceMatches.Count > 0);
        Assert.IsTrue(classMatches.Count > 0);

        var sharedIndex = projection.SourceText.IndexOf("Shared", StringComparison.Ordinal);
        Assert.IsTrue(sharedIndex >= 0);
        var classMatch = classMatches
            .Where(match => match.Index <= sharedIndex)
            .LastOrDefault();
        Assert.IsNotNull(classMatch);
        var namespaceMatch = namespaceMatches
            .Where(match => match.Index <= sharedIndex)
            .LastOrDefault();
        Assert.IsNotNull(namespaceMatch);

        return $"global::{namespaceMatch.Groups["name"].Value}.{classMatch.Groups["name"].Value}";
    }

    private static LspPosition GetPosition(string text, string marker, int advance = 0)
    {
        var offset = text.IndexOf(marker, StringComparison.Ordinal);
        Assert.IsTrue(offset >= 0, $"Expected marker '{marker}' to exist.");
        return LspProtocolHelpers.GetPosition(text, offset + advance);
    }

    private static LspPosition GetLastPosition(string text, string marker, int advance = 0)
    {
        var offset = text.LastIndexOf(marker, StringComparison.Ordinal);
        Assert.IsTrue(offset >= 0, $"Expected marker '{marker}' to exist.");
        return LspProtocolHelpers.GetPosition(text, offset + advance);
    }

    private static string GetBuiltAssemblyPath(string projectDirectoryName, string assemblyFileName)
    {
        var assemblyPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            projectDirectoryName,
            "bin",
            "Debug",
            "net10.0",
            assemblyFileName);
        Assert.IsTrue(File.Exists(assemblyPath), $"Expected built assembly '{assemblyPath}' to exist.");
        return assemblyPath;
    }

    private static string GetRepositoryRoot()
        => Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            ".."));

    private static int ToOffset(string text, JsonElement position)
    {
        var line = position.GetProperty("line").GetInt32();
        var character = position.GetProperty("character").GetInt32();
        var currentLine = 0;
        var currentCharacter = 0;

        for (var index = 0; index < text.Length; index++)
        {
            if (currentLine == line && currentCharacter == character)
            {
                return index;
            }

            if (text[index] == '\n')
            {
                currentLine++;
                currentCharacter = 0;
                continue;
            }

            currentCharacter++;
        }

        if (currentLine == line && currentCharacter == character)
        {
            return text.Length;
        }

        Assert.Fail($"Position ({line}, {character}) did not map into the provided text.");
        return -1;
    }

    private static void AssertSignatureHelp(JsonElement? signatureHelp, int expectedActiveParameter)
    {
        Assert.IsNotNull(signatureHelp);
        var result = signatureHelp.Value;
        Assert.AreEqual(0, result.GetProperty("activeSignature").GetInt32());
        Assert.AreEqual(expectedActiveParameter, result.GetProperty("activeParameter").GetInt32());
        var signatures = result.GetProperty("signatures");
        Assert.AreEqual(1, signatures.GetArrayLength());
        var signature = signatures[0];
        StringAssert.Contains(signature.GetProperty("label").GetString() ?? string.Empty, "FormatValue");
        var parameters = signature.GetProperty("parameters");
        Assert.AreEqual(3, parameters.GetArrayLength());
        Assert.AreEqual("int count", parameters[0].GetProperty("label").GetString());
        Assert.AreEqual("string prefix", parameters[1].GetProperty("label").GetString());
        Assert.AreEqual("bool includeUnits", parameters[2].GetProperty("label").GetString());
    }

    private static void AssertHasSemanticToken(
        IReadOnlyList<LspSemanticToken> tokens,
        LspPosition position,
        int length,
        string tokenType,
        params string[] modifiers)
    {
        var token = tokens.FirstOrDefault(candidate =>
            candidate.Line == position.Line
            && candidate.Character == position.Character
            && candidate.Length == length
            && string.Equals(candidate.TokenType, tokenType, StringComparison.Ordinal));
        Assert.IsNotNull(token, $"Expected semantic token '{tokenType}' at {position.Line}:{position.Character}.");
        CollectionAssert.AreEquivalent(modifiers, token.TokenModifiers);
    }

    private static async Task<JsonElement> ReceiveWebSocketJsonAsync(WebSocket socket, TimeSpan timeout)
    {
        using var timeoutSource = new CancellationTokenSource(timeout);
        var buffer = new byte[4096];
        using var messageStream = new MemoryStream();

        while (true)
        {
            var result = await socket.ReceiveAsync(buffer, timeoutSource.Token);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                throw new AssertFailedException("Expected a websocket payload before close.");
            }

            messageStream.Write(buffer, 0, result.Count);
            if (result.EndOfMessage)
            {
                break;
            }
        }

        var json = Encoding.UTF8.GetString(messageStream.ToArray());
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static async Task AssertNoWebSocketJsonAsync(WebSocket socket, TimeSpan timeout)
    {
        try
        {
            var message = await ReceiveWebSocketJsonAsync(socket, timeout);
            Assert.Fail("Expected no websocket payload, but received: " + message.GetRawText());
        }
        catch (OperationCanceledException)
        {
        }
    }

    private sealed class LspTestClient : IAsyncDisposable
    {
        private readonly Process _process;
        private readonly Stream _input;
        private readonly Stream _output;

        private LspTestClient(Process process, Stream input, Stream output)
        {
            _process = process;
            _input = input;
            _output = output;
        }

        public static async Task<LspTestClient> StartAsync(params string[] additionalArguments)
        {
            var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add(hostAssemblyPath);
            process.StartInfo.ArgumentList.Add("--lsp");
            foreach (var argument in additionalArguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            Assert.IsTrue(process.Start(), "Expected VueHost LSP process to start.");
            await Task.Yield();
            return new LspTestClient(
                process,
                process.StandardInput.BaseStream,
                process.StandardOutput.BaseStream);
        }

        public async Task InitializeAsync()
        {
            await SendAsync(new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new { }
            });
            using var _ = await ReadResponseAsync(expectedId: 1);
        }

        public async Task OpenDocumentAsync(string uri, string text, int version, string languageId = "jazor")
        {
            await SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri,
                        languageId,
                        version,
                        text
                    }
                }
            });
            using var _ = await ReadMessageAsync();
        }

        public async Task ChangeDocumentAsync(string uri, string text, int version)
        {
            await SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didChange",
                @params = new
                {
                    textDocument = new
                    {
                        uri,
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
            });
            using var _ = await ReadMessageAsync();
        }

        public async Task CloseDocumentAsync(string uri)
        {
            await SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didClose",
                @params = new
                {
                    textDocument = new
                    {
                        uri
                    }
                }
            });
            using var _ = await ReadMessageAsync();
        }

        public async Task<JsonElement> ChangeAndReadFirstDiagnosticAsync(string uri, string text, int version)
        {
            await SendAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didChange",
                @params = new
                {
                    textDocument = new
                    {
                        uri,
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
            });
            using var diagnosticsMessage = await ReadMessageAsync();
            return diagnosticsMessage.RootElement
                .GetProperty("params")
                .GetProperty("diagnostics")[0]
                .Clone();
        }

        public async Task<string[]> RequestCompletionLabelsAsync(int requestId, string uri, int line, int character)
        {
            await SendAsync(new
            {
                jsonrpc = "2.0",
                id = requestId,
                method = "textDocument/completion",
                @params = new
                {
                    textDocument = new
                    {
                        uri
                    },
                    position = new
                    {
                        line,
                        character
                    }
                }
            });
            using var response = await ReadResponseAsync(expectedId: requestId);
            if (!response.RootElement.TryGetProperty("result", out var result))
            {
                Assert.Fail("Expected LSP completion result. Raw response: " + response.RootElement.GetRawText());
            }

            return result
                .EnumerateArray()
                .Select(static item => item.GetProperty("label").GetString() ?? string.Empty)
                .ToArray();
        }

        public async Task<JsonElement?> RequestHoverAsync(int requestId, string uri, int line, int character)
        {
            await SendAsync(new
            {
                jsonrpc = "2.0",
                id = requestId,
                method = "textDocument/hover",
                @params = new
                {
                    textDocument = new
                    {
                        uri
                    },
                    position = new
                    {
                        line,
                        character
                    }
                }
            });
            using var response = await ReadResponseAsync(expectedId: requestId);
            if (!response.RootElement.TryGetProperty("result", out var result))
            {
                Assert.Fail("Expected LSP hover result. Raw response: " + response.RootElement.GetRawText());
            }

            return result.ValueKind == JsonValueKind.Null ? null : result.Clone();
        }

        public async Task<JsonElement?> RequestSignatureHelpAsync(int requestId, string uri, int line, int character)
        {
            await SendAsync(new
            {
                jsonrpc = "2.0",
                id = requestId,
                method = "textDocument/signatureHelp",
                @params = new
                {
                    textDocument = new
                    {
                        uri
                    },
                    position = new
                    {
                        line,
                        character
                    }
                }
            });
            using var response = await ReadResponseAsync(expectedId: requestId);
            if (!response.RootElement.TryGetProperty("result", out var result))
            {
                Assert.Fail("Expected LSP signatureHelp result. Raw response: " + response.RootElement.GetRawText());
            }

            return result.ValueKind == JsonValueKind.Null ? null : result.Clone();
        }

        public async Task<JsonElement> RequestDocumentSymbolsAsync(int requestId, string uri)
        {
            await SendAsync(new
            {
                jsonrpc = "2.0",
                id = requestId,
                method = "textDocument/documentSymbol",
                @params = new
                {
                    textDocument = new
                    {
                        uri
                    }
                }
            });
            using var response = await ReadResponseAsync(expectedId: requestId);
            if (!response.RootElement.TryGetProperty("result", out var result))
            {
                Assert.Fail("Expected LSP documentSymbol result. Raw response: " + response.RootElement.GetRawText());
            }

            return result.Clone();
        }

        public async Task<IReadOnlyList<LspSemanticToken>> RequestSemanticTokensAsync(int requestId, string uri)
        {
            await SendAsync(new
            {
                jsonrpc = "2.0",
                id = requestId,
                method = "textDocument/semanticTokens/full",
                @params = new
                {
                    textDocument = new
                    {
                        uri
                    }
                }
            });
            using var response = await ReadResponseAsync(expectedId: requestId);
            if (!response.RootElement.TryGetProperty("result", out var result))
            {
                Assert.Fail("Expected LSP semanticTokens result. Raw response: " + response.RootElement.GetRawText());
            }

            var data = result
                .GetProperty("data")
                .EnumerateArray()
                .Select(static item => item.GetInt32())
                .ToArray();
            return LspSemanticTokenLegend.Decode(data);
        }

        public async Task SendAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var body = Encoding.UTF8.GetBytes(json);
            var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");
            await _input.WriteAsync(header);
            await _input.WriteAsync(body);
            await _input.FlushAsync();
        }

        public async Task<JsonDocument> ReadMessageAsync()
        {
            var contentLength = await ReadContentLengthAsync();
            var body = new byte[contentLength];
            var offset = 0;
            while (offset < body.Length)
            {
                var read = await _output.ReadAsync(body.AsMemory(offset, body.Length - offset));
                if (read == 0)
                {
                    var stderr = _process.HasExited
                        ? await _process.StandardError.ReadToEndAsync()
                        : string.Empty;
                    throw new EndOfStreamException("Unexpected end of stream while reading LSP body. stderr: " + stderr);
                }

                offset += read;
            }

            return JsonDocument.Parse(body);
        }

        public async Task<JsonDocument> ReadResponseAsync(int expectedId)
        {
            while (true)
            {
                var message = await ReadMessageAsync();
                if (TryGetMessageId(message.RootElement, out var messageId) && messageId == expectedId)
                {
                    return message;
                }

                message.Dispose();
            }
        }

        public async Task ShutdownAsync()
        {
            if (_process.HasExited)
            {
                return;
            }

            await SendAsync(new
            {
                jsonrpc = "2.0",
                id = 99,
                method = "shutdown",
                @params = new { }
            });
            using var _ = await ReadResponseAsync(expectedId: 99);
            await SendAsync(new
            {
                jsonrpc = "2.0",
                method = "exit",
                @params = new { }
            });
            await _process.WaitForExitAsync(CancellationToken.None);
            if (_process.ExitCode != 0)
            {
                var error = await _process.StandardError.ReadToEndAsync();
                Assert.Fail($"Expected clean LSP shutdown. Exit code: {_process.ExitCode}. stderr: {error}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            await ShutdownAsync();
            _process.Dispose();
        }

        public async Task DisposeIgnoringExitCodeAsync()
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                    await _process.WaitForExitAsync(CancellationToken.None);
                }
            }
            finally
            {
                _process.Dispose();
            }
        }

        private async Task<int> ReadContentLengthAsync()
        {
            var headerBytes = new List<byte>();
            var buffer = new byte[1];
            while (true)
            {
                var read = await _output.ReadAsync(buffer.AsMemory(0, 1));
                if (read == 0)
                {
                    var stderr = _process.HasExited
                        ? await _process.StandardError.ReadToEndAsync()
                        : string.Empty;
                    throw new EndOfStreamException("Unexpected end of stream while reading LSP headers. stderr: " + stderr);
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

            throw new InvalidOperationException("Expected Content-Length header in LSP response.");
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
    }
}

file static class TestDirectory
{
    public static bool Exists(string path)
        => System.IO.Directory.Exists(path);

    public static DirectoryInfo CreateDirectory(string path)
        => System.IO.Directory.CreateDirectory(path);

    public static void Delete(string path, bool recursive)
    {
        if (!recursive)
        {
            System.IO.Directory.Delete(path, recursive: false);
            return;
        }

        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                System.IO.Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
            catch (IOException)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
        }
    }
}
