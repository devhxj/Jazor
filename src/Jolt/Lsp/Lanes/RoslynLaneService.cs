using Jazor.VueContracts.Protocol;
using Jolt.Lsp;
using Jolt.Lsp.Routing;
using Jolt.Roslyn.InProc;
using Jolt.Workspace;

namespace Jolt.Lsp.Lanes;

internal sealed class RoslynLaneService : ILspLane
{
    private readonly IJoltWorkspaceStore _workspaceStore;
    private readonly InProcRoslynCodeService _inProcCodeService;

    public RoslynLaneService(
        IJoltWorkspaceStore workspaceStore,
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

    public async ValueTask<IReadOnlyList<LspDocumentHighlight>> GetDocumentHighlightsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsCodeTarget(projectionTarget))
        {
            return Array.Empty<LspDocumentHighlight>();
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.GetDocumentHighlightsAsync(
            document,
            position,
            openDocuments,
            cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.GetDocumentHighlightsAsync(
                document,
                position,
                cancellationToken);
        }

        if (inProcResult.Count > 0)
        {
            return inProcResult;
        }

        return Array.Empty<LspDocumentHighlight>();
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

    internal async ValueTask<IReadOnlyList<LspLocation>> GetTypeDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.CSharp))
        {
            return Array.Empty<LspLocation>();
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.GetTypeDefinitionAsync(document, position, openDocuments, cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.GetTypeDefinitionAsync(document, position, cancellationToken);
        }

        return inProcResult.Count > 0
            ? inProcResult
            : Array.Empty<LspLocation>();
    }

    internal async ValueTask<IReadOnlyList<LspCallHierarchyItem>> PrepareCallHierarchyAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.CSharp))
        {
            return Array.Empty<LspCallHierarchyItem>();
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.PrepareCallHierarchyAsync(document, position, openDocuments, cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.PrepareCallHierarchyAsync(document, position, cancellationToken);
        }

        return inProcResult.Count > 0
            ? inProcResult
            : Array.Empty<LspCallHierarchyItem>();
    }

    internal async ValueTask<IReadOnlyList<LspCallHierarchyIncomingCall>> GetIncomingCallsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.CSharp))
        {
            return Array.Empty<LspCallHierarchyIncomingCall>();
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.GetIncomingCallsAsync(document, position, openDocuments, cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.GetIncomingCallsAsync(document, position, cancellationToken);
        }

        return inProcResult.Count > 0
            ? inProcResult
            : Array.Empty<LspCallHierarchyIncomingCall>();
    }

    internal async ValueTask<IReadOnlyList<LspCallHierarchyOutgoingCall>> GetOutgoingCallsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.CSharp))
        {
            return Array.Empty<LspCallHierarchyOutgoingCall>();
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.GetOutgoingCallsAsync(document, position, openDocuments, cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.GetOutgoingCallsAsync(document, position, cancellationToken);
        }

        return inProcResult.Count > 0
            ? inProcResult
            : Array.Empty<LspCallHierarchyOutgoingCall>();
    }

    internal async ValueTask<IReadOnlyList<LspTypeHierarchyItem>> PrepareTypeHierarchyAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.CSharp))
        {
            return Array.Empty<LspTypeHierarchyItem>();
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.PrepareTypeHierarchyAsync(document, position, openDocuments, cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.PrepareTypeHierarchyAsync(document, position, cancellationToken);
        }

        return inProcResult.Count > 0
            ? inProcResult
            : Array.Empty<LspTypeHierarchyItem>();
    }

    internal async ValueTask<IReadOnlyList<LspTypeHierarchyItem>> GetTypeHierarchySuperTypesAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.CSharp))
        {
            return Array.Empty<LspTypeHierarchyItem>();
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.GetTypeHierarchySuperTypesAsync(document, position, openDocuments, cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.GetTypeHierarchySuperTypesAsync(document, position, cancellationToken);
        }

        return inProcResult.Count > 0
            ? inProcResult
            : Array.Empty<LspTypeHierarchyItem>();
    }

    internal async ValueTask<IReadOnlyList<LspTypeHierarchyItem>> GetTypeHierarchySubTypesAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (document.DocumentKind is not (DocumentKind.Jazor or DocumentKind.CSharp))
        {
            return Array.Empty<LspTypeHierarchyItem>();
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await _inProcCodeService.GetTypeHierarchySubTypesAsync(document, position, openDocuments, cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.GetTypeHierarchySubTypesAsync(document, position, cancellationToken);
        }

        return inProcResult.Count > 0
            ? inProcResult
            : Array.Empty<LspTypeHierarchyItem>();
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetImplementationAsync(
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
        var inProcResult = await _inProcCodeService.GetImplementationAsync(document, position, openDocuments, cancellationToken);
        if (inProcResult.Count == 0)
        {
            inProcResult = await _inProcCodeService.GetImplementationAsync(document, position, cancellationToken);
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
