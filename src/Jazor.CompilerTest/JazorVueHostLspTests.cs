using System.Diagnostics;
using System.Text;
using System.Text.Json;

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
        Assert.IsTrue(result.GetProperty("capabilities").GetProperty("renameProvider").GetBoolean());
        Assert.IsTrue(result.GetProperty("capabilities").GetProperty("codeActionProvider").GetBoolean());
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
            CollectionAssert.Contains(directiveLabels, "@vueimport");
            CollectionAssert.Contains(directiveLabels, "@jsimport");

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
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <
                </template>
                """;
            await File.WriteAllTextAsync(documentPath, diskText);
            await client.OpenDocumentAsync(documentUri, diskText, version: 1);

            var initialLabels = await client.RequestCompletionLabelsAsync(
                requestId: 31,
                documentUri,
                line: 3,
                character: 3);
            CollectionAssert.Contains(initialLabels, "UserCard");
            CollectionAssert.DoesNotContain(initialLabels, "ProfileCard");

            var updatedText =
                """
                @vueimport ProfileCard from "./ProfileCard.vue"

                <template>
                  <
                </template>
                """;
            await client.ChangeDocumentAsync(documentUri, updatedText, version: 2);

            var updatedLabels = await client.RequestCompletionLabelsAsync(
                requestId: 32,
                documentUri,
                line: 3,
                character: 3);
            CollectionAssert.Contains(updatedLabels, "ProfileCard");
            CollectionAssert.DoesNotContain(updatedLabels, "UserCard");

            await client.CloseDocumentAsync(documentUri);

            var afterCloseLabels = await client.RequestCompletionLabelsAsync(
                requestId: 33,
                documentUri,
                line: 3,
                character: 3);
            CollectionAssert.Contains(afterCloseLabels, "UserCard");
            CollectionAssert.DoesNotContain(afterCloseLabels, "ProfileCard");
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
            CollectionAssert.Contains(directiveLabels, "@vueimport");
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
            Assert.IsTrue(templateRenameEdits.GetArrayLength() >= 2);
            Assert.IsTrue(templateRenameEdits.EnumerateArray().Any(edit =>
                edit.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() == 3));

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
            Assert.IsTrue(changes.GetArrayLength() >= 2);
            Assert.AreEqual("ProfileCard", changes[0].GetProperty("newText").GetString());
            Assert.AreEqual(documentUri, renameResponse.RootElement
                .GetProperty("result")
                .GetProperty("changes")
                .EnumerateObject()
                .Single()
                .Name);

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

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-vuehost-lsp-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
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
            using var _ = await ReadMessageAsync();
        }

        public async Task OpenDocumentAsync(string uri, string text, int version)
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
                        languageId = "jazor",
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
            using var response = await ReadMessageAsync();
            return response.RootElement
                .GetProperty("result")
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
            using var response = await ReadMessageAsync();
            var result = response.RootElement.GetProperty("result");
            return result.ValueKind == JsonValueKind.Null ? null : result.Clone();
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
                    throw new EndOfStreamException("Unexpected end of stream while reading LSP body.");
                }

                offset += read;
            }

            return JsonDocument.Parse(body);
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
            using var _ = await ReadMessageAsync();
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
                    throw new EndOfStreamException("Unexpected end of stream while reading LSP headers.");
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
    }
}
