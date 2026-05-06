using Jazor.RazorVue.Protocol;

namespace Jolt.Lsp.Routing;

internal interface ILspLaneRouter
{
    IReadOnlyList<LaneKind> GetOrderedLanes(ProjectionTarget projectionTarget);

    IReadOnlyList<LaneKind> GetDiagnosticLanes(DocumentSnapshot document);

    IReadOnlyList<LaneKind> GetSemanticTokenLanes(DocumentSnapshot document);
}

