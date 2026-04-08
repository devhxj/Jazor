using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp.Aggregation;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;

namespace Jazor.VueHost.Lsp.Coordination;

internal sealed class RenameCoordinator
{
    private readonly IReadOnlyDictionary<LaneKind, ILspLane> _lanes;
    private readonly ILspLaneRouter _laneRouter;
    private readonly LspResultAggregator _resultAggregator;

    public RenameCoordinator(
        IReadOnlyDictionary<LaneKind, ILspLane> lanes,
        ILspLaneRouter laneRouter,
        LspResultAggregator resultAggregator)
    {
        _lanes = lanes ?? throw new ArgumentNullException(nameof(lanes));
        _laneRouter = laneRouter ?? throw new ArgumentNullException(nameof(laneRouter));
        _resultAggregator = resultAggregator ?? throw new ArgumentNullException(nameof(resultAggregator));
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

        return _resultAggregator.AggregateWorkspaceEdits(edits);
    }
}
