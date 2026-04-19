using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Frontend.Deno.Protocol;
using Jazor.VueHost.Lsp;

namespace Jazor.VueHost.Frontend.Deno.Hosting;

internal sealed record DenoVolarIntelliSenseContext(
    SemanticContext SemanticContext,
    IReadOnlyList<ArtifactRecord> Artifacts);

internal interface IDenoVolarHost : IAsyncDisposable
{
    bool IsEnabled { get; }

    bool IsRunning { get; }

    ValueTask StartAsync(CancellationToken cancellationToken);

    ValueTask StopAsync(CancellationToken cancellationToken);

    ValueTask<DenoSfcCompileResult?> CompileSfcAsync(
        string documentPath,
        string sfcText,
        string filename,
        CancellationToken cancellationToken);

    ValueTask<DenoTypeScriptCompileResult?> CompileTypeScriptAsync(
        string documentPath,
        string text,
        string filename,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<DenoTypeScriptCompileResult?>(default);

    ValueTask<IReadOnlyList<LspDiagnostic>> GetTemplateDiagnosticsAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspCompletionItem>> GetTemplateCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspDocumentSymbol>> GetTemplateDocumentSymbolsAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspSemanticToken>> GetTemplateSemanticTokensAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspDocumentLink>> GetTemplateDocumentLinksAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspDocumentLink>>(Array.Empty<LspDocumentLink>());

    ValueTask<IReadOnlyList<LspInlayHint>> GetTemplateInlayHintsAsync(
        DocumentSnapshot document,
        LspRange range,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspInlayHint>>(Array.Empty<LspInlayHint>());

    ValueTask<IReadOnlyList<LspFoldingRange>> GetTemplateFoldingRangesAsync(
        DocumentSnapshot document,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspFoldingRange>>(Array.Empty<LspFoldingRange>());

    ValueTask<LspHoverResult?> GetTemplateHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspLocation>> GetTemplateDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspLocation>> GetTemplateImplementationAsync(
        DocumentSnapshot document,
        LspPosition position,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

    ValueTask<IReadOnlyList<LspLocation>> GetTemplateReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken);

    ValueTask<LspWorkspaceEdit?> GetTemplateRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        DenoVolarIntelliSenseContext? context,
        CancellationToken cancellationToken);
}
