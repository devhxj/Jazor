using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp.Routing;

namespace Jazor.VueHost.Lsp.Lanes;

internal sealed class JazorLaneService : ILspLane
{
    private readonly JazorLspDocumentService _documentService;

    public JazorLaneService(JazorLspDocumentService documentService)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
    }

    public LaneKind LaneKind => LaneKind.Jazor;

    public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => _documentService.GetDiagnosticsAsync(document, cancellationToken);

    public ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => _documentService.GetHoverAsync(document, position, cancellationToken);

    public ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => _documentService.GetCompletionItemsAsync(document, position, cancellationToken);

    public ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => _documentService.GetDefinitionAsync(document, position, cancellationToken);

    public ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => _documentService.GetReferencesAsync(document, position, includeDeclaration, cancellationToken);

    public ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => _documentService.GetRenameAsync(document, position, newName, cancellationToken);

    public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => _documentService.GetCodeActionsAsync(document, diagnostics, cancellationToken);
}
