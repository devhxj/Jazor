using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Lsp.Coordination;

internal sealed class ReferenceCoordinator
{
    private readonly IReadOnlyDictionary<LaneKind, ILspLane> _lanes;
    private readonly ILspLaneRouter _laneRouter;
    private readonly MarkupBridgeFanoutCoordinator _markupBridgeFanout;

    public ReferenceCoordinator(
        IReadOnlyDictionary<LaneKind, ILspLane> lanes,
        ILspLaneRouter laneRouter,
        MarkupBridgeFanoutCoordinator markupBridgeFanout)
    {
        _lanes = lanes ?? throw new ArgumentNullException(nameof(lanes));
        _laneRouter = laneRouter ?? throw new ArgumentNullException(nameof(laneRouter));
        _markupBridgeFanout = markupBridgeFanout ?? throw new ArgumentNullException(nameof(markupBridgeFanout));
    }

    public async ValueTask<IReadOnlyList<LspLocation>> CoordinateAsync(
        DocumentSnapshot document,
        LspPosition position,
        bool includeDeclaration,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var locations = new List<LspLocation>();
        foreach (var laneKind in _laneRouter.GetOrderedLanes(projectionTarget))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_lanes.TryGetValue(laneKind, out var lane))
            {
                continue;
            }

            var laneLocations = await lane.GetReferencesAsync(
                document,
                position,
                includeDeclaration,
                projectionTarget,
                cancellationToken);
            if (laneLocations.Count > 0)
            {
                locations.AddRange(laneLocations);
            }
        }

        return await _markupBridgeFanout.CoordinateReferencesAsync(
            document,
            position,
            includeDeclaration,
            locations,
            cancellationToken);
    }
}
