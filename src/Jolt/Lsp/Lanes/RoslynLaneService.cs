using Jolt.Lsp;
using Jolt.Lsp.Routing;
using ECMAScript.Contract.VueContracts.Protocol;
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
        return await ExecuteCodeNullableRequestAsync(
            document,
            projectionTarget,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetHoverAsync(currentDocument, position, openDocuments, ct),
            (currentDocument, ct) => _inProcCodeService.GetHoverAsync(currentDocument, position, ct));
    }

    public async ValueTask<IReadOnlyList<LspDocumentHighlight>> GetDocumentHighlightsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        return await ExecuteCodeListRequestAsync(
            document,
            projectionTarget,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetDocumentHighlightsAsync(
                currentDocument,
                position,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.GetDocumentHighlightsAsync(
                currentDocument,
                position,
                ct));
    }

    public async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        return await ExecuteCodeListRequestAsync(
            document,
            projectionTarget,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetCompletionItemsAsync(
                currentDocument,
                position,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.GetCompletionItemsAsync(
                currentDocument,
                position,
                ct));
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
        return await ExecuteCodeListRequestAsync(
            document,
            projectionTarget,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetDefinitionAsync(
                currentDocument,
                position,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.GetDefinitionAsync(
                currentDocument,
                position,
                ct));
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

        return await ExecuteListRequestWithFallbackAsync(
            document,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetTypeDefinitionAsync(
                currentDocument,
                position,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.GetTypeDefinitionAsync(
                currentDocument,
                position,
                ct));
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

        return await ExecuteListRequestWithFallbackAsync(
            document,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetIncomingCallsAsync(
                currentDocument,
                position,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.GetIncomingCallsAsync(
                currentDocument,
                position,
                ct));
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

        return await ExecuteListRequestWithFallbackAsync(
            document,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetOutgoingCallsAsync(
                currentDocument,
                position,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.GetOutgoingCallsAsync(
                currentDocument,
                position,
                ct));
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

        return await ExecuteListRequestWithFallbackAsync(
            document,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.PrepareTypeHierarchyAsync(
                currentDocument,
                position,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.PrepareTypeHierarchyAsync(
                currentDocument,
                position,
                ct));
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

        return await ExecuteListRequestWithFallbackAsync(
            document,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetTypeHierarchySuperTypesAsync(
                currentDocument,
                position,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.GetTypeHierarchySuperTypesAsync(
                currentDocument,
                position,
                ct));
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

        return await ExecuteListRequestWithFallbackAsync(
            document,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetTypeHierarchySubTypesAsync(
                currentDocument,
                position,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.GetTypeHierarchySubTypesAsync(
                currentDocument,
                position,
                ct));
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetImplementationAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        return await ExecuteCodeListRequestAsync(
            document,
            projectionTarget,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetImplementationAsync(
                currentDocument,
                position,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.GetImplementationAsync(
                currentDocument,
                position,
                ct));
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        return await ExecuteCodeListRequestAsync(
            document,
            projectionTarget,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetReferencesAsync(
                currentDocument,
                position,
                includeDeclaration,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.GetReferencesAsync(
                currentDocument,
                position,
                includeDeclaration,
                ct));
    }

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        return await ExecuteCodeNullableRequestAsync(
            document,
            projectionTarget,
            cancellationToken,
            (currentDocument, openDocuments, ct) => _inProcCodeService.GetRenameAsync(
                currentDocument,
                position,
                newName,
                openDocuments,
                ct),
            (currentDocument, ct) => _inProcCodeService.GetRenameAsync(
                currentDocument,
                position,
                newName,
                ct));
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

    private async ValueTask<IReadOnlyList<T>> ExecuteCodeListRequestAsync<T>(
        DocumentSnapshot document,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken,
        Func<DocumentSnapshot, IReadOnlyList<DocumentSnapshot>, CancellationToken, ValueTask<IReadOnlyList<T>>> withOpenDocuments,
        Func<DocumentSnapshot, CancellationToken, ValueTask<IReadOnlyList<T>>> fallback)
    {
        if (!IsCodeTarget(projectionTarget))
        {
            return Array.Empty<T>();
        }

        return await ExecuteListRequestWithFallbackAsync(document, cancellationToken, withOpenDocuments, fallback);
    }

    private async ValueTask<IReadOnlyList<T>> ExecuteListRequestWithFallbackAsync<T>(
        DocumentSnapshot document,
        CancellationToken cancellationToken,
        Func<DocumentSnapshot, IReadOnlyList<DocumentSnapshot>, CancellationToken, ValueTask<IReadOnlyList<T>>> withOpenDocuments,
        Func<DocumentSnapshot, CancellationToken, ValueTask<IReadOnlyList<T>>> fallback)
    {
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await withOpenDocuments(document, openDocuments, cancellationToken);
        if (inProcResult.Count > 0)
        {
            return inProcResult;
        }

        inProcResult = await fallback(document, cancellationToken);
        return inProcResult.Count > 0
            ? inProcResult
            : Array.Empty<T>();
    }

    private async ValueTask<TResult?> ExecuteCodeNullableRequestAsync<TResult>(
        DocumentSnapshot document,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken,
        Func<DocumentSnapshot, IReadOnlyList<DocumentSnapshot>, CancellationToken, ValueTask<TResult?>> withOpenDocuments,
        Func<DocumentSnapshot, CancellationToken, ValueTask<TResult?>> fallback)
        where TResult : class
    {
        if (!IsCodeTarget(projectionTarget))
        {
            return null;
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var inProcResult = await withOpenDocuments(document, openDocuments, cancellationToken);
        return inProcResult ?? await fallback(document, cancellationToken);
    }
}
