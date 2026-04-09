using Jazor.VueContracts.Protocol;
using Jazor.VueHost.LanguageServers;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.Roslyn.InProc;
using Jazor.VueHost.Workspace;

namespace Jazor.VueHost.Lsp.Lanes;

internal sealed class RoslynLaneService : ILspLane
{
    private readonly JazorLspDocumentService _documentService;
    private readonly IVueHostWorkspaceStore _workspaceStore;
    private readonly InProcRoslynCodeService _inProcCodeService;
    private readonly ProjectedLanguageServerLaneHost? _host;

    public RoslynLaneService(
        JazorLspDocumentService documentService,
        IVueHostWorkspaceStore workspaceStore,
        InProcRoslynCodeService? inProcCodeService = null,
        ProjectedLanguageServerLaneHost? host = null)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        _inProcCodeService = inProcCodeService ?? new InProcRoslynCodeService();
        _host = host;
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

        var inProcResult = await _inProcCodeService.GetHoverAsync(document, position, cancellationToken);
        if (inProcResult is not null)
        {
            return inProcResult;
        }

        if (_host is not null)
        {
            var result = await _host.GetHoverAsync(document, position, projectionTarget, cancellationToken);
            if (result is not null)
            {
                return result;
            }
        }

        return await _documentService.GetHoverAsync(document, position, cancellationToken);
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

        var inProcResult = await _inProcCodeService.GetCompletionItemsAsync(document, position, cancellationToken);
        if (inProcResult.Count > 0)
        {
            return inProcResult;
        }

        if (_host is not null)
        {
            var result = await _host.GetCompletionItemsAsync(document, position, projectionTarget, cancellationToken);
            if (result.Count > 0)
            {
                return result;
            }
        }

        return await _documentService.GetCompletionItemsAsync(document, position, cancellationToken);
    }

    public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => document.DocumentKind == DocumentKind.Jazor
            ? _inProcCodeService.GetDocumentSymbolsAsync(document, cancellationToken)
            : ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(Array.Empty<LspDocumentSymbol>());

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

        if (_host is not null)
        {
            var result = await _host.GetDefinitionAsync(document, position, projectionTarget, cancellationToken);
            if (result.Count > 0)
            {
                return result;
            }
        }

        return await _documentService.GetDefinitionAsync(document, position, cancellationToken);
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

        if (_host is not null)
        {
            var result = await _host.GetReferencesAsync(
                document,
                position,
                includeDeclaration,
                projectionTarget,
                cancellationToken);
            if (result.Count > 0)
            {
                return result;
            }
        }

        return await _documentService.GetReferencesAsync(document, position, includeDeclaration, cancellationToken);
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

        if (_host is not null)
        {
            var result = await _host.GetRenameAsync(document, position, newName, projectionTarget, cancellationToken);
            if (result is not null)
            {
                return result;
            }
        }

        return await _documentService.GetRenameAsync(document, position, newName, cancellationToken);
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

        if (_host is not null)
        {
            var result = await _host.GetCodeActionsAsync(
                document,
                range,
                diagnostics,
                projectionTarget,
                cancellationToken);
            if (result.Count > 0)
            {
                return result;
            }
        }

        return await _documentService.GetCodeActionsAsync(document, diagnostics, cancellationToken);
    }

    private static bool IsCodeTarget(ProjectionTarget projectionTarget)
        => projectionTarget.LaneKind == LaneKind.Roslyn
            || projectionTarget.RegionKind == DocumentRegionKind.Code;
}
