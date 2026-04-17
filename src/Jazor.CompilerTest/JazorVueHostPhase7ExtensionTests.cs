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
        IExtensionRegistry extensionRegistry)
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
            extensionRegistry: extensionRegistry);
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

    private sealed class TestExtension(string id) : IExtension, ILspDiagnosticProvider, ILspCodeActionProvider
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
    }
}
