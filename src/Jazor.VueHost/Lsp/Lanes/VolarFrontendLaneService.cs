using Jazor.VueContracts.Protocol;
using Jazor.VueHost.LanguageServers;
using Jazor.VueHost.Lsp.Routing;

namespace Jazor.VueHost.Lsp.Lanes;

internal sealed class VolarFrontendLaneService : ILspLane
{
    private readonly ProjectedLanguageServerLaneHost? _host;
    private readonly FrontendLaneService _fallbackLane;

    public VolarFrontendLaneService(
        ProjectedLanguageServerLaneHost? host,
        FrontendLaneService fallbackLane)
    {
        _host = host;
        _fallbackLane = fallbackLane ?? throw new ArgumentNullException(nameof(fallbackLane));
    }

    public LaneKind LaneKind => LaneKind.Frontend;

    public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => _fallbackLane.GetDiagnosticsAsync(document, cancellationToken);

    public async ValueTask<LspHoverResult?> GetHoverAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (_host is not null && IsTemplateTarget(projectionTarget))
        {
            var result = await _host.GetHoverAsync(document, position, projectionTarget, cancellationToken);
            if (result is not null)
            {
                return result;
            }
        }

        return await _fallbackLane.GetHoverAsync(document, position, projectionTarget, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (_host is not null && IsTemplateTarget(projectionTarget))
        {
            var result = await _host.GetCompletionItemsAsync(document, position, projectionTarget, cancellationToken);
            if (result.Count > 0)
            {
                return result;
            }
        }

        return await _fallbackLane.GetCompletionItemsAsync(document, position, projectionTarget, cancellationToken);
    }

    public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => _fallbackLane.GetDocumentSymbolsAsync(document, cancellationToken);

    public ValueTask<LspSignatureHelp?> GetSignatureHelpAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
        => _fallbackLane.GetSignatureHelpAsync(document, position, projectionTarget, cancellationToken);

    public async ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (_host is not null && IsTemplateTarget(projectionTarget))
        {
            var result = await _host.GetDefinitionAsync(document, position, projectionTarget, cancellationToken);
            if (result.Count > 0)
            {
                return result;
            }
        }

        return await _fallbackLane.GetDefinitionAsync(document, position, projectionTarget, cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (_host is not null && IsTemplateTarget(projectionTarget))
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

        return await _fallbackLane.GetReferencesAsync(
            document,
            position,
            includeDeclaration,
            projectionTarget,
            cancellationToken);
    }

    public async ValueTask<LspWorkspaceEdit?> GetRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (_host is not null && IsTemplateTarget(projectionTarget))
        {
            var result = await _host.GetRenameAsync(
                document,
                position,
                newName,
                projectionTarget,
                cancellationToken);
            if (result is not null)
            {
                return result;
            }
        }

        return await _fallbackLane.GetRenameAsync(
            document,
            position,
            newName,
            projectionTarget,
            cancellationToken);
    }

    public async ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        if (_host is not null && IsTemplateTarget(projectionTarget))
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

        return await _fallbackLane.GetCodeActionsAsync(
            document,
            range,
            diagnostics,
            projectionTarget,
            cancellationToken);
    }

    private static bool IsTemplateTarget(ProjectionTarget projectionTarget)
        => projectionTarget.LaneKind == LaneKind.Frontend
            || projectionTarget.RegionKind == DocumentRegionKind.Template;
}
