using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Lsp.Routing;

internal sealed class LspLaneRouter : ILspLaneRouter
{
    private static readonly IReadOnlyList<LaneKind> JazorOnly = [LaneKind.Jazor];
    private static readonly IReadOnlyList<LaneKind> FrontendThenJazor = [LaneKind.Frontend, LaneKind.Jazor];
    private static readonly IReadOnlyList<LaneKind> RoslynThenJazor = [LaneKind.Roslyn, LaneKind.Jazor];
    private static readonly IReadOnlyList<LaneKind> DiagnosticLanes = [LaneKind.Jazor, LaneKind.Roslyn, LaneKind.Frontend];

    public IReadOnlyList<LaneKind> GetOrderedLanes(ProjectionTarget projectionTarget)
        => projectionTarget.LaneKind switch
        {
            LaneKind.Frontend => FrontendThenJazor,
            LaneKind.Roslyn => RoslynThenJazor,
            _ => JazorOnly
        };

    public IReadOnlyList<LaneKind> GetDiagnosticLanes(DocumentSnapshot document)
        => document.DocumentKind == DocumentKind.Jazor
            ? DiagnosticLanes
            : JazorOnly;
}
