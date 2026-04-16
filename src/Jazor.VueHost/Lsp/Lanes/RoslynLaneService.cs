using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.Roslyn.InProc;
using Jazor.VueHost.Workspace;

namespace Jazor.VueHost.Lsp.Lanes;

internal sealed class RoslynLaneService : ILspLane
{
    private readonly IVueHostWorkspaceStore _workspaceStore;
    private readonly InProcRoslynCodeService _inProcCodeService;

    public RoslynLaneService(
        IVueHostWorkspaceStore workspaceStore,
        InProcRoslynCodeService? inProcCodeService = null)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        _inProcCodeService = inProcCodeService ?? new InProcRoslynCodeService();
    }

    public LaneKind LaneKind => LaneKind.Roslyn;

    public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => _inProcCodeService.GetDiagnosticsAsync(document, cancellationToken);

    public async ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsCodeTarget(projectionTarget))
        {
            return null;
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.GetHoverAsync(document, position, openDocuments, cancellationToken);
        if (inProcResult is null)
        {
            inProcResult = await _inProcCodeService.GetHoverAsync(document, position, cancellationToken);
        }

        if (inProcResult is not null)
        {
            return inProcResult;
        }

        return null;
    }

    public async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsCodeTarget(projectionTarget))
        {
            return Array.Empty<LspCompletionItem>();
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.GetCompletionItemsAsync(document, position, openDocuments, cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.GetCompletionItemsAsync(document, position, cancellationToken);
        }

        if (inProcResult.Count > 0)
        {
            return inProcResult;
        }

        return Array.Empty<LspCompletionItem>();
    }

    public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => document.DocumentKind is DocumentKind.Jazor or DocumentKind.CSharp
            ? _inProcCodeService.GetDocumentSymbolsAsync(document, cancellationToken)
            : ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(Array.Empty<LspDocumentSymbol>());

    public ValueTask<IReadOnlyList<LspSemanticToken>> GetSemanticTokensAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => document.DocumentKind is DocumentKind.Jazor or DocumentKind.CSharp
            ? _inProcCodeService.GetSemanticTokensAsync(document, cancellationToken)
            : ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(Array.Empty<LspSemanticToken>());

    public async ValueTask<LspSignatureHelp?> GetSignatureHelpAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsCodeTarget(projectionTarget))
        {
            return null;
        }

        return await _inProcCodeService.GetSignatureHelpAsync(document, position, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsCodeTarget(projectionTarget))
        {
            return Array.Empty<LspLocation>();
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.GetDefinitionAsync(document, position, openDocuments, cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.GetDefinitionAsync(document, position, cancellationToken);
        }

        if (inProcResult.Count > 0)
        {
            return inProcResult;
        }

        return Array.Empty<LspLocation>();
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsCodeTarget(projectionTarget))
        {
            return Array.Empty<LspLocation>();
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.GetReferencesAsync(
            document,
            position,
            includeDeclaration,
            openDocuments,
            cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.GetReferencesAsync(
                document,
                position,
                includeDeclaration,
                cancellationToken);
        }

        if (inProcResult.Count > 0)
        {
            return inProcResult;
        }

        return Array.Empty<LspLocation>();
    }

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsCodeTarget(projectionTarget))
        {
            return null;
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.GetRenameAsync(
            document,
            position,
            newName,
            openDocuments,
            cancellationToken);
        if (inProcResult is null)
        {
            inProcResult = await _inProcCodeService.GetRenameAsync(
                document,
                position,
                newName,
                cancellationToken);
        }

        if (inProcResult is not null)
        {
            return inProcResult;
        }

        return null;
    }

    public async ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsCodeTarget(projectionTarget))
        {
            return Array.Empty<LspCodeAction>();
        }

        return Array.Empty<LspCodeAction>();
    }

    private static bool IsCodeTarget(ProjectionTarget projectionTarget)
        => projectionTarget.LaneKind == LaneKind.Roslyn
            || projectionTarget.RegionKind == DocumentRegionKind.Code;
}
