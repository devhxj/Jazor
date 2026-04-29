using Jolt.Lsp.Routing;
using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Lsp.Lanes;

internal interface ILspLane
{
    LaneKind LaneKind { get; }

    ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken);

    ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspDocumentHighlight>> GetDocumentHighlightsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspDocumentLink>> GetDocumentLinksAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspDocumentLink>>(Array.Empty<LspDocumentLink>());

    ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspSemanticToken>> GetSemanticTokensAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken);

    ValueTask<LspSignatureHelp?> GetSignatureHelpAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspInlayHint>> GetInlayHintsAsync(
        DocumentSnapshot document,
        LspRange range,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspInlayHint>>(Array.Empty<LspInlayHint>());

    ValueTask<IReadOnlyList<LspFoldingRange>> GetFoldingRangesAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspFoldingRange>>(Array.Empty<LspFoldingRange>());

    ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspLocation>> GetImplementationAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

    ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken);

    ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken);
}
