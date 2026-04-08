using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Lsp.Routing;

namespace Jazor.VueHost.Lsp.Lanes;

internal sealed class FrontendLaneService : ILspLane
{
    private readonly JazorLspDocumentService _documentService;
    private readonly IDenoFrontendHost? _denoFrontendHost;

    public FrontendLaneService(
        JazorLspDocumentService documentService,
        IDenoFrontendHost? denoFrontendHost = null)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _denoFrontendHost = denoFrontendHost;
    }

    public LaneKind LaneKind => LaneKind.Frontend;

    public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());

    public async ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return null;
        }

        var denoResult = await TryGetDenoHoverAsync(document, position, cancellationToken);
        if (denoResult is not null)
        {
            return denoResult;
        }

        return await _documentService.GetHoverAsync(document, position, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspCompletionItem>();
        }

        var denoResult = await TryGetDenoCompletionItemsAsync(document, position, cancellationToken);
        if (denoResult is { Count: > 0 })
        {
            return denoResult;
        }

        return await _documentService.GetCompletionItemsAsync(document, position, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspLocation>();
        }

        var denoResult = await TryGetDenoDefinitionsAsync(document, position, cancellationToken);
        if (denoResult is { Count: > 0 })
        {
            return denoResult;
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
        if (!IsTemplateTarget(projectionTarget))
        {
            return Array.Empty<LspLocation>();
        }

        var denoResult = await TryGetDenoReferencesAsync(document, position, includeDeclaration, cancellationToken);
        if (denoResult is { Count: > 0 })
        {
            return denoResult;
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
        if (!IsTemplateTarget(projectionTarget))
        {
            return null;
        }

        var denoResult = await TryGetDenoRenameAsync(document, position, newName, cancellationToken);
        if (denoResult is not null)
        {
            return denoResult;
        }

        return await _documentService.GetRenameAsync(document, position, newName, cancellationToken);
    }

    public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => IsTemplateTarget(projectionTarget)
            ? _documentService.GetCodeActionsAsync(document, diagnostics, cancellationToken)
            : ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());

    private static bool IsTemplateTarget(ProjectionTarget projectionTarget)
        => projectionTarget.LaneKind == LaneKind.Frontend
            || projectionTarget.RegionKind == DocumentRegionKind.Template;

    private async ValueTask<IReadOnlyList<LspCompletionItem>> TryGetDenoCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null || !_denoFrontendHost.IsRunning)
        {
            return Array.Empty<LspCompletionItem>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateCompletionItemsAsync(document, position, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspCompletionItem>();
        }
    }

    private async ValueTask<LspHoverResult?> TryGetDenoHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null || !_denoFrontendHost.IsRunning)
        {
            return null;
        }

        try
        {
            return await _denoFrontendHost.GetTemplateHoverAsync(document, position, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private async ValueTask<IReadOnlyList<LspLocation>> TryGetDenoDefinitionsAsync(
        DocumentSnapshot document,
        LspPosition position,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null || !_denoFrontendHost.IsRunning)
        {
            return Array.Empty<LspLocation>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateDefinitionAsync(document, position, cancellationToken);
        }
        catch
        {
            return Array.Empty<LspLocation>();
        }
    }

    private async ValueTask<IReadOnlyList<LspLocation>> TryGetDenoReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null || !_denoFrontendHost.IsRunning)
        {
            return Array.Empty<LspLocation>();
        }

        try
        {
            return await _denoFrontendHost.GetTemplateReferencesAsync(
                document,
                position,
                includeDeclaration,
                cancellationToken);
        }
        catch
        {
            return Array.Empty<LspLocation>();
        }
    }

    private async ValueTask<LspWorkspaceEdit?> TryGetDenoRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        CancellationToken cancellationToken)
    {
        if (_denoFrontendHost is null || !_denoFrontendHost.IsRunning)
        {
            return null;
        }

        try
        {
            return await _denoFrontendHost.GetTemplateRenameAsync(document, position, newName, cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}
