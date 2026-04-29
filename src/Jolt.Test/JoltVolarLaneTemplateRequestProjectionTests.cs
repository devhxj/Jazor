using Jolt.Volar.Deno.Hosting;
using Jolt.Volar.Deno.Protocol;
using Jolt.Lsp;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using ECMAScript.Contract.VueContracts.Protocol;
using Jolt.VirtualDocuments.Mapping;
using Jolt.VirtualDocuments.Models;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;

namespace Jolt.Test;

[TestClass]
public sealed class JoltVolarLaneTemplateRequestProjectionTests
{
    [TestMethod]
    public async Task Jolt_VolarLaneService_UsesPrimaryGVueProjection_ForReferencesAndRename()
    {
        var document = CreateJazorDocument("<UserCard />");
        var projectedPath = "virtual:" + document.DocumentPath + ".g.vue";
        var projectedUri = LspProtocolHelpers.ToDocumentUri(projectedPath);
        var denoProjectedUri = LowercaseWindowsDriveUri(projectedUri);
        var sourceUri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
        var registry = await CreateRegistryWithPrimaryProjectionAsync(document, projectedPath);
        var denoHost = new FakeDenoFrontendHost
        {
            References =
            [
                new LspLocation
                {
                    Uri = projectedUri,
                    Range = CreateInlineRange(0, 1, 0, 9)
                }
            ],
            RenameResult = new LspWorkspaceEdit
            {
                Changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal)
                {
                    [denoProjectedUri] =
                    [
                        new LspTextEdit
                        {
                            Range = CreateInlineRange(0, 1, 0, 9),
                            NewText = "AccountCard"
                        }
                    ]
                }
            }
        };
        var lane = CreateLane(denoHost, registry);

        var references = await lane.GetReferencesAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            includeDeclaration: true,
            CreateTemplateTarget(document),
            CancellationToken.None);
        var rename = await lane.GetRenameAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            "AccountCard",
            CreateTemplateTarget(document),
            CancellationToken.None);
        var codeActions = await lane.GetCodeActionsAsync(
            document,
            CreateInlineRange(0, 0, 0, 10),
            diagnostics: [],
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.IsNotNull(denoHost.LastDocument);
        Assert.AreEqual(projectedPath, denoHost.LastDocument.DocumentPath);
        Assert.AreEqual(DocumentKind.Vue, denoHost.LastDocument.DocumentKind);

        Assert.AreEqual(1, references.Count);
        Assert.AreEqual(sourceUri, references[0].Uri);
        Assert.AreEqual(0, references[0].Range.Start.Line);
        Assert.AreEqual(1, references[0].Range.Start.Character);

        Assert.IsNotNull(rename);
        Assert.IsTrue(rename.Changes.ContainsKey(sourceUri));
        Assert.IsFalse(rename.Changes.ContainsKey(denoProjectedUri));
        Assert.AreEqual(1, rename.Changes[sourceUri].Length);
        Assert.AreEqual("AccountCard", rename.Changes[sourceUri][0].NewText);

        Assert.AreEqual(0, codeActions.Count);
    }

    [TestMethod]
    public async Task Jolt_VolarLaneService_FallsBackToSource_ForReferencesAndRename_WhenPrimaryGVueProjectionMissing()
    {
        var document = CreateJazorDocument("<UserCard />");
        var sourceUri = LspProtocolHelpers.ToDocumentUri(document.DocumentPath);
        var registry = await CreateRegistryWithoutPrimaryProjectionAsync(document);
        var denoHost = new FakeDenoFrontendHost
        {
            References =
            [
                new LspLocation
                {
                    Uri = sourceUri,
                    Range = CreateInlineRange(0, 1, 0, 9)
                }
            ],
            RenameResult = new LspWorkspaceEdit
            {
                Changes = new Dictionary<string, LspTextEdit[]>(StringComparer.Ordinal)
                {
                    [sourceUri] =
                    [
                        new LspTextEdit
                        {
                            Range = CreateInlineRange(0, 1, 0, 9),
                            NewText = "AccountCard"
                        }
                    ]
                }
            }
        };
        var lane = CreateLane(denoHost, registry);

        var references = await lane.GetReferencesAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            includeDeclaration: true,
            CreateTemplateTarget(document),
            CancellationToken.None);
        var rename = await lane.GetRenameAsync(
            document,
            new LspPosition { Line = 0, Character = 2 },
            "AccountCard",
            CreateTemplateTarget(document),
            CancellationToken.None);
        var codeActions = await lane.GetCodeActionsAsync(
            document,
            CreateInlineRange(0, 0, 0, 10),
            diagnostics: [],
            CreateTemplateTarget(document),
            CancellationToken.None);

        Assert.IsNotNull(denoHost.LastDocument);
        Assert.AreEqual(document.DocumentPath, denoHost.LastDocument.DocumentPath);
        Assert.AreEqual(DocumentKind.Jazor, denoHost.LastDocument.DocumentKind);

        Assert.AreEqual(1, references.Count);
        Assert.AreEqual(sourceUri, references[0].Uri);
        Assert.IsNotNull(rename);
        Assert.IsTrue(rename.Changes.ContainsKey(sourceUri));
        Assert.AreEqual(0, codeActions.Count);
    }

    private static VolarLaneService CreateLane(
        IDenoVolarHost denoHost,
        IVirtualDocumentRegistry registry)
        => new(
            new InMemoryWorkspaceStore(),
            volarContextProvider: null,
            registry,
            denoHost);

    private static DocumentSnapshot CreateJazorDocument(string text)
        => new(
            @"D:\temp\Counter.jazor",
            DocumentKind.Jazor,
            text,
            "1");

    private static ProjectionTarget CreateTemplateTarget(DocumentSnapshot document)
        => new(
            LaneKind.Volar,
            DocumentRegionKind.Template,
            document.DocumentPath,
            document.DocumentPath);

    private static LspRange CreateInlineRange(
        int startLine,
        int startCharacter,
        int endLine,
        int endCharacter)
        => new()
        {
            Start = new LspPosition { Line = startLine, Character = startCharacter },
            End = new LspPosition { Line = endLine, Character = endCharacter }
        };

    private static string LowercaseWindowsDriveUri(string uri)
    {
        const string prefix = "file:///";
        return uri.Length > prefix.Length + 1
            && uri.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && uri[prefix.Length + 1] == ':'
            ? prefix + char.ToLowerInvariant(uri[prefix.Length]) + uri[(prefix.Length + 1)..]
            : uri;
    }

    private static async Task<InMemoryVirtualDocumentRegistry> CreateRegistryWithPrimaryProjectionAsync(
        DocumentSnapshot sourceDocument,
        string projectedPath)
    {
        var registry = new InMemoryVirtualDocumentRegistry();
        await registry.UpsertAsync(
        [
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    sourceDocument.DocumentPath,
                    projectedPath,
                    VirtualDocumentKind.Vue),
                sourceDocument.Text,
                ProjectionMap.CreateWholeDocument(
                    sourceDocument.DocumentPath,
                    projectedPath,
                    sourceDocument.Text.Length,
                    sourceDocument.Text.Length),
                "1"),
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    sourceDocument.DocumentPath,
                    "virtual:" + sourceDocument.DocumentPath + ".template-only.vue",
                    VirtualDocumentKind.Vue),
                "<template><FallbackOnly /></template>",
                ProjectionMap.CreateWholeDocument(
                    sourceDocument.DocumentPath,
                    "virtual:" + sourceDocument.DocumentPath + ".template-only.vue",
                    sourceDocument.Text.Length,
                    "<template><FallbackOnly /></template>".Length),
                "1")
        ],
            CancellationToken.None);

        return registry;
    }

    private static async Task<InMemoryVirtualDocumentRegistry> CreateRegistryWithoutPrimaryProjectionAsync(
        DocumentSnapshot sourceDocument)
    {
        var registry = new InMemoryVirtualDocumentRegistry();
        await registry.UpsertAsync(
        [
            new VirtualDocument(
                new VirtualDocumentIdentity(
                    sourceDocument.DocumentPath,
                    "virtual:" + sourceDocument.DocumentPath + ".template-only.vue",
                    VirtualDocumentKind.Vue),
                "<template><FallbackOnly /></template>",
                ProjectionMap.CreateWholeDocument(
                    sourceDocument.DocumentPath,
                    "virtual:" + sourceDocument.DocumentPath + ".template-only.vue",
                    sourceDocument.Text.Length,
                    "<template><FallbackOnly /></template>".Length),
                "1")
        ],
            CancellationToken.None);

        return registry;
    }

    private sealed class FakeDenoFrontendHost : IDenoVolarHost
    {
        public bool IsEnabled => true;

        public bool IsRunning => true;

        public IReadOnlyList<LspLocation> References { get; init; } = [];

        public LspWorkspaceEdit? RenameResult { get; init; }

        public DocumentSnapshot? LastDocument { get; private set; }

        public ValueTask StartAsync(CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        public ValueTask StopAsync(CancellationToken cancellationToken)
            => ValueTask.CompletedTask;

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;

        public ValueTask<DenoSfcCompileResult?> CompileSfcAsync(
            string documentPath,
            string sfcText,
            string filename,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<DenoSfcCompileResult?>(default);

        public ValueTask<DenoTypeScriptCompileResult?> CompileTypeScriptAsync(
            string documentPath,
            string text,
            string filename,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<DenoTypeScriptCompileResult?>(default);

        public ValueTask<IReadOnlyList<LspDiagnostic>> GetTemplateDiagnosticsAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());

        public ValueTask<IReadOnlyList<LspCompletionItem>> GetTemplateCompletionItemsAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());

        public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetTemplateDocumentSymbolsAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(Array.Empty<LspDocumentSymbol>());

        public ValueTask<IReadOnlyList<LspSemanticToken>> GetTemplateSemanticTokensAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(Array.Empty<LspSemanticToken>());

        public ValueTask<LspHoverResult?> GetTemplateHoverAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<LspHoverResult?>(null);

        public ValueTask<IReadOnlyList<LspLocation>> GetTemplateDefinitionAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<IReadOnlyList<LspLocation>> GetTemplateReferencesAsync(
            DocumentSnapshot document,
            LspPosition position,
            bool includeDeclaration,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
        {
            LastDocument = document;
            return ValueTask.FromResult(References);
        }

        public ValueTask<LspWorkspaceEdit?> GetTemplateRenameAsync(
            DocumentSnapshot document,
            LspPosition position,
            string newName,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
        {
            LastDocument = document;
            return ValueTask.FromResult(RenameResult);
        }
    }
}
