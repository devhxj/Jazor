using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp.Aggregation;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;

namespace Jazor.VueHost.Lsp.Coordination;

internal sealed class CodeActionCoordinator
{
    private readonly IReadOnlyDictionary<LaneKind, ILspLane> _lanes;
    private readonly ILspLaneRouter _laneRouter;
    private readonly LspResultAggregator _resultAggregator;

    public CodeActionCoordinator(
        IReadOnlyDictionary<LaneKind, ILspLane> lanes,
        ILspLaneRouter laneRouter,
        LspResultAggregator resultAggregator)
    {
        _lanes = lanes ?? throw new ArgumentNullException(nameof(lanes));
        _laneRouter = laneRouter ?? throw new ArgumentNullException(nameof(laneRouter));
        _resultAggregator = resultAggregator ?? throw new ArgumentNullException(nameof(resultAggregator));
    }

    public async ValueTask<IReadOnlyList<LspCodeAction>> CoordinateAsync(
        DocumentSnapshot document,
        LspRange range,
        IReadOnlyList<LspDiagnostic> diagnostics,
        ProjectionTarget projectionTarget,
        CancellationToken cancellationToken)
    {
        var actions = new List<LspCodeAction>();
        foreach (var laneKind in _laneRouter.GetOrderedLanes(projectionTarget))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_lanes.TryGetValue(laneKind, out var lane))
            {
                continue;
            }

            var laneActions = await lane.GetCodeActionsAsync(
                document,
                range,
                diagnostics,
                projectionTarget,
                cancellationToken);
            if (laneActions.Count > 0)
            {
                actions.AddRange(laneActions);
            }
        }

        return _resultAggregator.AggregateCodeActions(actions);
    }
}
