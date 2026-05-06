using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using Jazor.RazorVue.Protocol;

namespace Jolt.Lsp.Coordination;

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
        var laneKinds = GetOrderedLanes(projectionTarget, diagnostics);
        foreach (var laneKind in laneKinds)
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

    private IReadOnlyList<LaneKind> GetOrderedLanes(
        ProjectionTarget projectionTarget,
        IReadOnlyList<LspDiagnostic> diagnostics)
    {
        var laneKinds = _laneRouter.GetOrderedLanes(projectionTarget).ToList();
        if (ContainsVolarDiagnostic(diagnostics) && !laneKinds.Contains(LaneKind.Volar))
        {
            laneKinds.Add(LaneKind.Volar);
        }

        if (ContainsJazorDiagnostic(diagnostics) && !laneKinds.Contains(LaneKind.Jazor))
        {
            laneKinds.Add(LaneKind.Jazor);
        }

        return laneKinds;
    }

    private static bool ContainsVolarDiagnostic(IReadOnlyList<LspDiagnostic> diagnostics)
        => diagnostics.Any(diagnostic =>
            string.Equals(diagnostic.Source, "Jolt.Volar", StringComparison.Ordinal)
            || string.Equals(diagnostic.Code, "JAZORVUEFRONTEND001", StringComparison.Ordinal)
            || string.Equals(diagnostic.Code, "JAZORVUEFRONTEND002", StringComparison.Ordinal));

    private static bool ContainsJazorDiagnostic(IReadOnlyList<LspDiagnostic> diagnostics)
        => diagnostics.Any(diagnostic =>
            string.Equals(diagnostic.Source, "Jolt", StringComparison.Ordinal)
            || string.Equals(diagnostic.Code, "JAZORVUE001", StringComparison.Ordinal));
}

