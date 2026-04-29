using Jolt.Volar.Deno.Hosting;
using Jolt.Volar.Deno.Protocol;
using Jolt.Lsp;
using Jolt.Lsp.Lanes;
using ECMAScript.Contract.VueContracts.Protocol;
using Jolt.VirtualDocuments.Mapping;
using Jolt.VirtualDocuments.Models;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;

namespace Jolt.Test;

[TestClass]
public sealed class JoltVolarLaneDocumentSymbolProjectionTests
{
    [TestMethod]
    public async Task Jolt_VolarLaneService_GetDocumentSymbols_UsesPrimaryGVueProjectionWhenAvailable()
    {
        var document = CreateJazorDocument("<UserCard />");
        var projectedPath = "virtual:" + document.DocumentPath + ".g.vue";
        var registry = await CreateRegistryWithPrimaryProjectionAsync(document, projectedPath);
        var denoHost = new FakeDenoFrontendHost
        {
            DocumentSymbols =
            [
                new LspDocumentSymbol
                {
                    Name = "UserCard",
                    Kind = 5,
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 1 },
                        End = new LspPosition { Line = 0, Character = 9 }
                    },
                    SelectionRange = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 1 },
                        End = new LspPosition { Line = 0, Character = 9 }
                    }
                }
            ]
        };
        var lane = CreateLane(denoHost, registry);

        var symbols = await lane.GetDocumentSymbolsAsync(document, CancellationToken.None);

        Assert.IsNotNull(denoHost.LastDocument);
        Assert.AreEqual(projectedPath, denoHost.LastDocument.DocumentPath);
        Assert.AreEqual(DocumentKind.Vue, denoHost.LastDocument.DocumentKind);
        Assert.AreEqual(1, symbols.Count);
        Assert.AreEqual("UserCard", symbols[0].Name);
        Assert.AreEqual(0, symbols[0].Range.Start.Line);
        Assert.AreEqual(1, symbols[0].Range.Start.Character);
    }

    [TestMethod]
    public async Task Jolt_VolarLaneService_GetDocumentSymbols_FallsBackToSourceWhenPrimaryGVueProjectionMissing()
    {
        var document = CreateJazorDocument("<UserCard />");
        var registry = await CreateRegistryWithoutPrimaryProjectionAsync(document);
        var denoHost = new FakeDenoFrontendHost
        {
            DocumentSymbols =
            [
                new LspDocumentSymbol
                {
                    Name = "UserCard",
                    Kind = 5,
                    Range = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 1 },
                        End = new LspPosition { Line = 0, Character = 9 }
                    },
                    SelectionRange = new LspRange
                    {
                        Start = new LspPosition { Line = 0, Character = 1 },
                        End = new LspPosition { Line = 0, Character = 9 }
                    }
                }
            ]
        };
        var lane = CreateLane(denoHost, registry);

        var symbols = await lane.GetDocumentSymbolsAsync(document, CancellationToken.None);

        Assert.IsNotNull(denoHost.LastDocument);
        Assert.AreEqual(document.DocumentPath, denoHost.LastDocument.DocumentPath);
        Assert.AreEqual(DocumentKind.Jazor, denoHost.LastDocument.DocumentKind);
        Assert.AreEqual(1, symbols.Count);
        Assert.AreEqual("UserCard", symbols[0].Name);
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

        public IReadOnlyList<LspDocumentSymbol> DocumentSymbols { get; init; } = [];

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
        {
            LastDocument = document;
            return ValueTask.FromResult(DocumentSymbols);
        }

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
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<LspWorkspaceEdit?> GetTemplateRenameAsync(
            DocumentSnapshot document,
            LspPosition position,
            string newName,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<LspWorkspaceEdit?>(null);
    }
}
