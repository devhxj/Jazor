using System.Text;
using System.Text.Json;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.DevServer;
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
public sealed class JazorVueHostPhase7ExtensionTests
{
    [TestMethod]
    public void ExtensionRegistry_RegisterExtension_RegistersLspProviders()
    {
        var registry = new ExtensionRegistry();
        var extension = new TestExtension("phase7.registry");

        registry.RegisterExtension(extension);

        Assert.AreEqual(1, registry.GetExtensions().Count);
        Assert.AreEqual(1, registry.GetLspDiagnosticProviders().Count);
        Assert.AreEqual(1, registry.GetLspCodeActionProviders().Count);
        Assert.AreEqual(1, registry.GetLspHoverProviders().Count);
        Assert.AreEqual(1, registry.GetLspCompletionProviders().Count);
        Assert.AreEqual(1, registry.GetLspDocumentSymbolProviders().Count);
        Assert.AreEqual(1, registry.GetLspReferenceProviders().Count);
        Assert.AreEqual(1, registry.GetLspRenameProviders().Count);
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadBuiltinExtensionsAsync_InitializesAndActivatesExtension()
    {
        var registry = new ExtensionRegistry();
        var loader = new ExtensionLoader(registry);
        var extension = new TestExtension("phase7.loader");
        var root = Path.GetFullPath(Path.GetTempPath());

        await loader.LoadBuiltinExtensionsAsync(
            [extension],
            root,
            CancellationToken.None);

        Assert.IsTrue(extension.Initialized);
        Assert.IsTrue(extension.Activated);
        Assert.AreEqual(1, registry.GetExtensions().Count);
        Assert.AreEqual(1, registry.GetLspDiagnosticProviders().Count);
        Assert.AreEqual(1, registry.GetLspCodeActionProviders().Count);
        Assert.AreEqual(1, registry.GetLspHoverProviders().Count);
        Assert.AreEqual(1, registry.GetLspCompletionProviders().Count);
        Assert.AreEqual(1, registry.GetLspDocumentSymbolProviders().Count);
        Assert.AreEqual(1, registry.GetLspReferenceProviders().Count);
        Assert.AreEqual(1, registry.GetLspRenameProviders().Count);
    }

    [TestMethod]
    public void ExtensionHostOptionsResolver_Resolve_MergesConfigAndCliOverrides()
    {
        var options = ExtensionHostOptionsResolver.Resolve(
            [
                "--extensions-enabled=true",
                "--extensions-dir=.custom/extensions",
                "--extensions-disabled=ext.c,ext.d"
            ],
            rootDirectory: @"D:\repo\sample",
            config: new JazorConfig
            {
                Extensions = new JazorExtensionsConfig
                {
                    Enabled = false,
                    Directory = ".jazor/extensions",
                    Disabled = ["ext.a", "ext.b"]
                }
            });

        Assert.IsTrue(options.Enabled);
        StringAssert.EndsWith(
            options.ExtensionsDirectory.Replace('\\', '/'),
            "/.custom/extensions",
            StringComparison.OrdinalIgnoreCase);
        CollectionAssert.AreEquivalent(
            new[] { "ext.c", "ext.d" },
            options.DisabledExtensionIds.ToArray());
    }

    [TestMethod]
    public async Task LspSession_CodeAction_Request_MergesExtensionProviderActions()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var extensionRegistry = new ExtensionRegistry();
        extensionRegistry.RegisterLspCodeActionProvider(new TestCodeActionProvider("Phase7 Action", priority: 10));

        var documentPath = Path.Combine(Path.GetTempPath(), $"phase7-{Guid.NewGuid():N}.jazor");
        var document = new DocumentSnapshot(
            documentPath,
            DocumentKind.Jazor,
            "<div>@value</div>",
            "1");
        await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            extensionRegistry);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 101,
                Method = "textDocument/codeAction",
                Params = new LspCodeActionParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 4 }
                    },
                    Context = new LspCodeActionContext
                    {
                        Diagnostics = []
                    }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var actions = response.Result as IReadOnlyList<LspCodeAction>;
        Assert.IsNotNull(actions);
        Assert.IsTrue(actions.Any(static action => string.Equals(action.Title, "Phase7 Action", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task LspSession_Hover_Request_UsesExtensionProvider()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var extensionRegistry = new ExtensionRegistry();
        extensionRegistry.RegisterLspHoverProvider(new TestHoverProvider("Phase7 Hover", priority: 10));

        var documentPath = Path.Combine(Path.GetTempPath(), $"phase7-{Guid.NewGuid():N}.jazor");
        var document = new DocumentSnapshot(
            documentPath,
            DocumentKind.Jazor,
            "<div>@value</div>",
            "1");
        await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            extensionRegistry);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 102,
                Method = "textDocument/hover",
                Params = new LspHoverParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Position = new LspPosition { Line = 0, Character = 6 }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var hover = response.Result as LspHoverResult;
        Assert.IsNotNull(hover);
        Assert.AreEqual("Phase7 Hover", hover.Contents.Value);
    }

    [TestMethod]
    public async Task LspSession_Hover_Request_WithTimeoutingExtensionProvider_RecordsHealthAndContinues()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var extensionRegistry = new ExtensionRegistry();
        extensionRegistry.RegisterLspHoverProvider(new TestSlowHoverProvider("Slow Hover", priority: 20, delay: TimeSpan.FromMilliseconds(150)));
        extensionRegistry.RegisterLspHoverProvider(new TestHoverProvider("Fast Hover", priority: 10));

        var documentPath = Path.Combine(Path.GetTempPath(), $"phase7-{Guid.NewGuid():N}.jazor");
        var document = new DocumentSnapshot(
            documentPath,
            DocumentKind.Jazor,
            "<div>@value</div>",
            "1");
        await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            extensionRegistry,
            extensionProviderTimeout: TimeSpan.FromMilliseconds(30));

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 1021,
                Method = "textDocument/hover",
                Params = new LspHoverParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Position = new LspPosition { Line = 0, Character = 6 }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var hover = response.Result as LspHoverResult;
        Assert.IsNotNull(hover);
        Assert.AreEqual("Fast Hover", hover.Contents.Value);

        var providerHealth = extensionRegistry.GetProviderHealth();
        var slowHealth = providerHealth.Single(static entry =>
            string.Equals(entry.ProviderName, "Phase7SlowHoverProvider", StringComparison.Ordinal)
            && string.Equals(entry.Capability, "hover", StringComparison.Ordinal));
        Assert.AreEqual(1, slowHealth.FailureCount);
        Assert.AreEqual(1, slowHealth.TimeoutCount);

        var fastHealth = providerHealth.Single(static entry =>
            string.Equals(entry.ProviderName, "Phase7HoverProvider", StringComparison.Ordinal)
            && string.Equals(entry.Capability, "hover", StringComparison.Ordinal));
        Assert.AreEqual(1, fastHealth.SuccessCount);
        Assert.AreEqual(0, fastHealth.FailureCount);
    }

    [TestMethod]
    public async Task LspSession_Completion_Request_MergesExtensionProviderItems()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var extensionRegistry = new ExtensionRegistry();
        extensionRegistry.RegisterLspCompletionProvider(new TestCompletionProvider("phase7Completion", priority: 10));

        var documentPath = Path.Combine(Path.GetTempPath(), $"phase7-{Guid.NewGuid():N}.jazor");
        var document = new DocumentSnapshot(
            documentPath,
            DocumentKind.Jazor,
            "<div>@value</div>",
            "1");
        await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            extensionRegistry);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 103,
                Method = "textDocument/completion",
                Params = new LspCompletionParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Position = new LspPosition { Line = 0, Character = 6 }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var items = response.Result as IReadOnlyList<LspCompletionItem>;
        Assert.IsNotNull(items);
        Assert.IsTrue(items.Any(static item => string.Equals(item.Label, "phase7Completion", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task LspSession_DocumentSymbol_Request_MergesExtensionProviderSymbols()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var extensionRegistry = new ExtensionRegistry();
        extensionRegistry.RegisterLspDocumentSymbolProvider(new TestDocumentSymbolProvider("Phase7Symbol", priority: 10));

        var documentPath = Path.Combine(Path.GetTempPath(), $"phase7-{Guid.NewGuid():N}.jazor");
        var document = new DocumentSnapshot(
            documentPath,
            DocumentKind.Jazor,
            "<div>@value</div>",
            "1");
        await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            extensionRegistry);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 104,
                Method = "textDocument/documentSymbol",
                Params = new LspDocumentSymbolParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var symbols = response.Result as IReadOnlyList<LspDocumentSymbol>;
        Assert.IsNotNull(symbols);
        Assert.IsTrue(symbols.Any(static symbol => string.Equals(symbol.Name, "Phase7Symbol", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task LspSession_References_Request_MergesExtensionProviderLocations()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var extensionRegistry = new ExtensionRegistry();
        extensionRegistry.RegisterLspReferenceProvider(new TestReferenceProvider(priority: 10));

        var documentPath = Path.Combine(Path.GetTempPath(), $"phase7-{Guid.NewGuid():N}.jazor");
        var document = new DocumentSnapshot(
            documentPath,
            DocumentKind.Jazor,
            "<div>@value</div>",
            "1");
        await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            extensionRegistry);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 105,
                Method = "textDocument/references",
                Params = new LspReferenceParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Position = new LspPosition { Line = 0, Character = 6 },
                    Context = new LspReferenceContext
                    {
                        IncludeDeclaration = true
                    }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var locations = response.Result as IReadOnlyList<LspLocation>;
        Assert.IsNotNull(locations);
        Assert.IsTrue(
            locations.Any(location => string.Equals(
                location.Uri,
                LspProtocolHelpers.ToDocumentUri(documentPath),
                StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task LspSession_Rename_Request_MergesExtensionProviderWorkspaceEdit()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var extensionRegistry = new ExtensionRegistry();
        extensionRegistry.RegisterLspRenameProvider(new TestRenameProvider(priority: 10));

        var documentPath = Path.Combine(Path.GetTempPath(), $"phase7-{Guid.NewGuid():N}.jazor");
        var document = new DocumentSnapshot(
            documentPath,
            DocumentKind.Jazor,
            "<div>@value</div>",
            "1");
        await workspaceStore.UpsertDocumentAsync(document, CancellationToken.None);

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            extensionRegistry);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 106,
                Method = "textDocument/rename",
                Params = new LspRenameParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Position = new LspPosition { Line = 0, Character = 6 },
                    NewName = "renamedValue"
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var edit = response.Result as LspWorkspaceEdit;
        Assert.IsNotNull(edit);
        var uri = LspProtocolHelpers.ToDocumentUri(documentPath);
        Assert.IsTrue(edit.Changes.ContainsKey(uri));
        Assert.IsTrue(edit.Changes[uri].Any(static textEdit => string.Equals(textEdit.NewText, "renamedValue", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task LspSession_DidOpen_PublishesDiagnosticsFromExtensionProvider()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var extensionRegistry = new ExtensionRegistry();
        extensionRegistry.RegisterLspDiagnosticProvider(new TestDiagnosticProvider("JAZORVUEEXT001", priority: 10));

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            extensionRegistry);

        var documentPath = Path.Combine(Path.GetTempPath(), $"phase7-{Guid.NewGuid():N}.jazor");
        var didOpen = new LspRequestMessage
        {
            Method = "textDocument/didOpen",
            Params = new LspDidOpenTextDocumentParams
            {
                TextDocument = new LspTextDocumentItem
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(documentPath),
                    LanguageId = "jazor",
                    Version = 1,
                    Text = "<div />"
                }
            }
        };

        _ = await session.HandleNotificationAsync(didOpen, CancellationToken.None);

        using var message = ReadSingleLspMessage(outputStream);
        Assert.AreEqual("textDocument/publishDiagnostics", message.RootElement.GetProperty("method").GetString());
        var diagnostics = message.RootElement
            .GetProperty("params")
            .GetProperty("diagnostics")
            .EnumerateArray()
            .ToArray();
        Assert.IsTrue(
            diagnostics.Any(diagnostic => string.Equals(diagnostic.GetProperty("code").GetString(), "JAZORVUEEXT001", StringComparison.Ordinal)),
            "Expected extension provider diagnostic to be present in published diagnostics.");
    }

    private static LspSession CreateSession(
        IVueHostWorkspaceStore workspaceStore,
        IVirtualDocumentRegistry virtualDocumentRegistry,
        ILspLane[] lanes,
        Stream outputStream,
        IExtensionRegistry extensionRegistry,
        TimeSpan? extensionProviderTimeout = null)
    {
        var laneRouter = new LspLaneRouter();
        var projectionResolver = new DocumentProjectionResolver(
            new DocumentRegionClassifier(),
            virtualDocumentRegistry);
        var projectionService = new JazorProjectionService();
        var resultAggregator = new LspResultAggregator();
        var markupBridgeService = new MarkupComponentBridgeService(workspaceStore);
        var markupBridgeFanout = new MarkupBridgeFanoutCoordinator(markupBridgeService, resultAggregator);
        var laneMap = lanes.ToDictionary(static lane => lane.LaneKind);

        return new LspSession(
            workspaceStore,
            lanes,
            laneRouter,
            new LspMessageWriter(outputStream),
            projectionService,
            virtualDocumentRegistry,
            projectionResolver,
            resultAggregator,
            markupBridgeFanout,
            new ReferenceCoordinator(laneMap, laneRouter, markupBridgeFanout),
            new RenameCoordinator(laneMap, laneRouter, resultAggregator, markupBridgeFanout),
            new CodeActionCoordinator(laneMap, laneRouter, resultAggregator),
            workspaceDocumentChangeSink: null,
            extensionRegistry: extensionRegistry,
            extensionProviderTimeout: extensionProviderTimeout);
    }

    private static JsonDocument ReadSingleLspMessage(MemoryStream stream)
    {
        var payload = stream.ToArray();
        var separator = IndexOf(payload, [13, 10, 13, 10]);
        Assert.IsTrue(separator >= 0, "Expected an LSP Content-Length header separator.");

        var headerText = Encoding.ASCII.GetString(payload, 0, separator);
        var contentLengthLine = headerText
            .Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(static line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase));
        Assert.IsNotNull(contentLengthLine);

        var contentLength = int.Parse(
            contentLengthLine["Content-Length:".Length..].Trim(),
            System.Globalization.CultureInfo.InvariantCulture);
        var bodyStart = separator + 4;
        var body = payload.AsSpan(bodyStart, contentLength);
        return JsonDocument.Parse(body.ToArray());
    }

    private static int IndexOf(byte[] haystack, byte[] needle)
    {
        for (var index = 0; index <= haystack.Length - needle.Length; index++)
        {
            var matched = true;
            for (var offset = 0; offset < needle.Length; offset++)
            {
                if (haystack[index + offset] != needle[offset])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return index;
            }
        }

        return -1;
    }

    private sealed class EmptyJazorLane : ILspLane
    {
        public LaneKind LaneKind => LaneKind.Jazor;

        public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(DocumentSnapshot document, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());

        public ValueTask<LspHoverResult?> GetHoverAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<LspHoverResult?>(null);

        public ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());

        public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(DocumentSnapshot document, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(Array.Empty<LspDocumentSymbol>());

        public ValueTask<IReadOnlyList<LspSemanticToken>> GetSemanticTokensAsync(DocumentSnapshot document, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(Array.Empty<LspSemanticToken>());

        public ValueTask<LspSignatureHelp?> GetSignatureHelpAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<LspSignatureHelp?>(null);

        public ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(DocumentSnapshot document, LspPosition position, bool includeDeclaration, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<LspWorkspaceEdit?> GetRenameAsync(DocumentSnapshot document, LspPosition position, string newName, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<LspWorkspaceEdit?>(null);

        public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(DocumentSnapshot document, LspRange range, IReadOnlyList<LspDiagnostic> diagnostics, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());
    }

    private sealed class TestCodeActionProvider(string title, int priority) : ILspCodeActionProvider
    {
        public string Name => "Phase7CodeActionProvider";

        public int Priority => priority;

        public ValueTask<IReadOnlyList<LspCodeAction>> ProvideCodeActionsAsync(
            LspCodeActionProviderContext context,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<LspCodeAction> actions =
            [
                new LspCodeAction
                {
                    Title = title,
                    Kind = "quickfix"
                }
            ];
            return ValueTask.FromResult(actions);
        }
    }

    private sealed class TestHoverProvider(string value, int priority) : ILspHoverProvider
    {
        public string Name => "Phase7HoverProvider";

        public int Priority => priority;

        public ValueTask<LspHoverResult?> ProvideHoverAsync(
            LspHoverProviderContext context,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult<LspHoverResult?>(new LspHoverResult
            {
                Contents = new LspMarkupContent
                {
                    Kind = "plaintext",
                    Value = value
                }
            });
        }
    }

    private sealed class TestSlowHoverProvider(string value, int priority, TimeSpan delay) : ILspHoverProvider
    {
        public string Name => "Phase7SlowHoverProvider";

        public int Priority => priority;

        public async ValueTask<LspHoverResult?> ProvideHoverAsync(
            LspHoverProviderContext context,
            CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);
            return new LspHoverResult
            {
                Contents = new LspMarkupContent
                {
                    Kind = "plaintext",
                    Value = value
                }
            };
        }
    }

    private sealed class TestCompletionProvider(string label, int priority) : ILspCompletionProvider
    {
        public string Name => "Phase7CompletionProvider";

        public int Priority => priority;

        public ValueTask<IReadOnlyList<LspCompletionItem>> ProvideCompletionItemsAsync(
            LspCompletionProviderContext context,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<LspCompletionItem> items =
            [
                new LspCompletionItem
                {
                    Label = label,
                    Kind = 10
                }
            ];

            return ValueTask.FromResult(items);
        }
    }

    private sealed class TestDocumentSymbolProvider(string symbolName, int priority) : ILspDocumentSymbolProvider
    {
        public string Name => "Phase7DocumentSymbolProvider";

        public int Priority => priority;

        public ValueTask<IReadOnlyList<LspDocumentSymbol>> ProvideDocumentSymbolsAsync(
            LspDocumentSymbolProviderContext context,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<LspDocumentSymbol> symbols =
            [
                new LspDocumentSymbol
                {
                    Name = symbolName,
                    Kind = 5,
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 5 }
                    },
                    SelectionRange = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 5 }
                    }
                }
            ];

            return ValueTask.FromResult(symbols);
        }
    }

    private sealed class TestReferenceProvider(int priority) : ILspReferenceProvider
    {
        public string Name => "Phase7ReferenceProvider";

        public int Priority => priority;

        public ValueTask<IReadOnlyList<LspLocation>> ProvideReferencesAsync(
            LspReferenceProviderContext context,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<LspLocation> locations =
            [
                new LspLocation
                {
                    Uri = LspProtocolHelpers.ToDocumentUri(context.Document.DocumentPath),
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 5 }
                    }
                }
            ];

            return ValueTask.FromResult(locations);
        }
    }

    private sealed class TestRenameProvider(int priority) : ILspRenameProvider
    {
        public string Name => "Phase7RenameProvider";

        public int Priority => priority;

        public ValueTask<LspWorkspaceEdit?> ProvideRenameAsync(
            LspRenameProviderContext context,
            CancellationToken cancellationToken)
        {
            var uri = LspProtocolHelpers.ToDocumentUri(context.Document.DocumentPath);
            return ValueTask.FromResult<LspWorkspaceEdit?>(new LspWorkspaceEdit
            {
                Changes = new Dictionary<string, LspTextEdit[]>
                {
                    [uri] =
                    [
                        new LspTextEdit
                        {
                            Range = new LspRange
                            {
                                Start = new LspPosition { Line = 0, Character = 5 },
                                End = new LspPosition { Line = 0, Character = 10 }
                            },
                            NewText = context.NewName
                        }
                    ]
                }
            });
        }
    }

    private sealed class TestDiagnosticProvider(string code, int priority) : ILspDiagnosticProvider
    {
        public string Name => "Phase7DiagnosticProvider";

        public int Priority => priority;

        public ValueTask<IReadOnlyList<LspDiagnostic>> ProvideDiagnosticsAsync(
            LspDiagnosticProviderContext context,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<LspDiagnostic> diagnostics =
            [
                new LspDiagnostic
                {
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 0 },
                        End = new LspPosition { Line = 0, Character = 1 }
                    },
                    Severity = 2,
                    Code = code,
                    Source = "Jazor.VueHost.Extension",
                    Message = "phase7-extension-diagnostic"
                }
            ];

            return ValueTask.FromResult(diagnostics);
        }
    }

    private sealed class TestExtension(string id) : IExtension, ILspDiagnosticProvider, ILspCodeActionProvider, ILspHoverProvider, ILspCompletionProvider, ILspDocumentSymbolProvider, ILspReferenceProvider, ILspRenameProvider
    {
        public bool Initialized { get; private set; }

        public bool Activated { get; private set; }

        public ExtensionMetadata Metadata { get; } = new(
            Id: id,
            Name: "Test Extension",
            Version: "1.0.0");

        public string Name => "TestExtensionProvider";

        public int Priority => 0;

        public ValueTask InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
        {
            Initialized = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask ActivateAsync(CancellationToken cancellationToken)
        {
            Activated = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask DeactivateAsync(CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        public ValueTask<IReadOnlyList<LspDiagnostic>> ProvideDiagnosticsAsync(LspDiagnosticProviderContext context, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());

        public ValueTask<IReadOnlyList<LspCodeAction>> ProvideCodeActionsAsync(LspCodeActionProviderContext context, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());

        public ValueTask<LspHoverResult?> ProvideHoverAsync(LspHoverProviderContext context, CancellationToken cancellationToken)
            => ValueTask.FromResult<LspHoverResult?>(null);

        public ValueTask<IReadOnlyList<LspCompletionItem>> ProvideCompletionItemsAsync(LspCompletionProviderContext context, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());

        public ValueTask<IReadOnlyList<LspDocumentSymbol>> ProvideDocumentSymbolsAsync(LspDocumentSymbolProviderContext context, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(Array.Empty<LspDocumentSymbol>());

        public ValueTask<IReadOnlyList<LspLocation>> ProvideReferencesAsync(LspReferenceProviderContext context, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<LspWorkspaceEdit?> ProvideRenameAsync(LspRenameProviderContext context, CancellationToken cancellationToken)
            => ValueTask.FromResult<LspWorkspaceEdit?>(null);
    }
}
