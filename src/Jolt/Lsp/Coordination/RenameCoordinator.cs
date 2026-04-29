using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Lsp.Coordination;

internal sealed class RenameCoordinator
{
    private readonly IReadOnlyDictionary<LaneKind, ILspLane> _lanes;
    private readonly ILspLaneRouter _laneRouter;
    private readonly LspResultAggregator _resultAggregator;
    private readonly MarkupBridgeFanoutCoordinator _markupBridgeFanout;

    public RenameCoordinator(
        IReadOnlyDictionary<LaneKind, ILspLane> lanes,
        ILspLaneRouter laneRouter,
        LspResultAggregator resultAggregator,
        MarkupBridgeFanoutCoordinator markupBridgeFanout)
    {
        _lanes = lanes ?? throw new ArgumentNullException(nameof(lanes));
        _laneRouter = laneRouter ?? throw new ArgumentNullException(nameof(laneRouter));
        _resultAggregator = resultAggregator ?? throw new ArgumentNullException(nameof(resultAggregator));
        _markupBridgeFanout = markupBridgeFanout ?? throw new ArgumentNullException(nameof(markupBridgeFanout));
    }

    public async ValueTask<LspWorkspaceEdit?> CoordinateAsync(
        DocumentSnapshot document,
        LspPosition position,
        string newName,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var edits = new List<LspWorkspaceEdit>();
        foreach (var laneKind in _laneRouter.GetOrderedLanes(projectionTarget))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_lanes.TryGetValue(laneKind, out var lane))
            {
                continue;
            }

            var edit = await lane.GetRenameAsync(document, position, newName, projectionTarget, cancellationToken);
            if (edit is not null)
            {
                edits.Add(edit);
            }
        }

        return await _markupBridgeFanout.CoordinateRenameAsync(
            document,
            position,
            newName,
            edits.Count == 0
                ? null
                : _resultAggregator.AggregateWorkspaceEdits(edits),
            cancellationToken);
    }
}
