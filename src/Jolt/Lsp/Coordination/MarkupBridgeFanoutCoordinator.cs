using Jazor.VueContracts.Protocol;
using Jolt.Lsp.Aggregation;

namespace Jolt.Lsp.Coordination;

internal sealed class MarkupBridgeFanoutCoordinator
{
    private readonly MarkupComponentBridgeService _markupComponentBridge;
    private readonly LspResultAggregator _resultAggregator;

    public MarkupBridgeFanoutCoordinator(
        MarkupComponentBridgeService markupComponentBridge,
        LspResultAggregator resultAggregator)
    {
        _markupComponentBridge = markupComponentBridge ?? throw new ArgumentNullException(nameof(markupComponentBridge));
        _resultAggregator = resultAggregator ?? throw new ArgumentNullException(nameof(resultAggregator));
    }

    public async ValueTask<IReadOnlyList<LspLocation>> CoordinateDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<LspLocation> nativeLocations,
        CancellationToken cancellationToken)
        => await CoordinateDefinitionAsync(
            document,
            position,
            nativeLocations,
            allowMarkupFallback: true,
            cancellationToken);

    public async ValueTask<IReadOnlyList<LspLocation>> CoordinateDefinitionAsync(
        DocumentSnapshot document,
        LspPosition position,
        IReadOnlyList<LspLocation> nativeLocations,
        bool allowMarkupFallback,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(nativeLocations);

        if (nativeLocations.Count > 0 || !allowMarkupFallback)
        {
            return _resultAggregator.AggregateLocations(nativeLocations);
        }

        var symbol = await _markupComponentBridge.ResolveBridgeSymbolAsync(
            document,
            position,
            locationHints: null,
            allowWorkspaceScan: true,
            cancellationToken);
        if (symbol is null)
        {
            return Array.Empty<LspLocation>();
        }

        return
        [
            new LspLocation
            {
                Uri = LspProtocolHelpers.ToDocumentUri(symbol.Value.AbsolutePath),
                Range = new LspRange
                {
                    Start = new LspPosition { Line = 0, Character = 0 },
                    End = new LspPosition { Line = 0, Character = 0 }
                }
            }
        ];
    }

    public async ValueTask<IReadOnlyList<LspLocation>> CoordinateReferencesAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        IReadOnlyList<LspLocation> nativeLocations,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(nativeLocations);

        var locations = nativeLocations.ToList();
        var symbol = await _markupComponentBridge.ResolveBridgeSymbolAsync(
            document,
            position,
            includeDeclaration ? nativeLocations : null,
            allowWorkspaceScan: true,
            cancellationToken);
        if (symbol is not null)
        {
            locations.AddRange(await _markupComponentBridge.FindJazorReferencesAsync(
                document,
                symbol.Value.ComponentName,
                symbol.Value.AbsolutePath,
                includeDeclaration,
                cancellationToken));
        }

        return _resultAggregator.AggregateLocations(locations);
    }

    public async ValueTask<LspWorkspaceEdit?> CoordinateRenameAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        LspWorkspaceEdit? nativeEdit,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        var edits = new List<LspWorkspaceEdit>();
        if (nativeEdit is not null)
        {
            edits.Add(nativeEdit);
        }

        var symbol = await _markupComponentBridge.ResolveBridgeSymbolAsync(
            document,
            position,
            locationHints: null,
            allowWorkspaceScan: true,
            cancellationToken);
        if (symbol is not null)
        {
            var changes = await _markupComponentBridge.FindJazorRenameChangesAsync(
                document,
                symbol.Value.ComponentName,
                symbol.Value.AbsolutePath,
                newName,
                cancellationToken);
            if (changes.Count > 0)
            {
                edits.Add(new LspWorkspaceEdit
                {
                    Changes = changes
                });
            }
        }

        return _resultAggregator.AggregateWorkspaceEdits(edits);
    }
}
