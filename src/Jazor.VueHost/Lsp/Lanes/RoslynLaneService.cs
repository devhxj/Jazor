using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp.Routing;

namespace Jazor.VueHost.Lsp.Lanes;

internal sealed class RoslynLaneService : ILspLane
{
    private readonly JazorLspDocumentService _documentService;

    public RoslynLaneService(JazorLspDocumentService documentService)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
    }

    public LaneKind LaneKind => LaneKind.Roslyn;

    public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());

    public ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => IsCodeTarget(projectionTarget)
            ? _documentService.GetHoverAsync(document, position, cancellationToken)
            : ValueTask.FromResult<LspHoverResult?>(null);

    public ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => IsCodeTarget(projectionTarget)
            ? _documentService.GetCompletionItemsAsync(document, position, cancellationToken)
            : ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());

    public ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => IsCodeTarget(projectionTarget)
            ? _documentService.GetDefinitionAsync(document, position, cancellationToken)
            : ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

    public ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => IsCodeTarget(projectionTarget)
            ? _documentService.GetReferencesAsync(document, position, includeDeclaration, cancellationToken)
            : ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

    public ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => IsCodeTarget(projectionTarget)
            ? _documentService.GetRenameAsync(document, position, newName, cancellationToken)
            : ValueTask.FromResult<LspWorkspaceEdit?>(null);

    public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => IsCodeTarget(projectionTarget)
            ? _documentService.GetCodeActionsAsync(document, diagnostics, cancellationToken)
            : ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());

    private static bool IsCodeTarget(ProjectionTarget projectionTarget)
        => projectionTarget.LaneKind == LaneKind.Roslyn
            || projectionTarget.RegionKind == DocumentRegionKind.Code;
}
