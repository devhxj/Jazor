using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Lsp.Routing;

internal sealed class LspLaneRouter : ILspLaneRouter
{
    private static readonly IReadOnlyList<LaneKind> JazorOnly = [LaneKind.Jazor];
    private static readonly IReadOnlyList<LaneKind> JazorSemanticTokenLanes = [LaneKind.Jazor, LaneKind.Frontend, LaneKind.Roslyn];
    private static readonly IReadOnlyList<LaneKind> FrontendOnly = [LaneKind.Frontend];
    private static readonly IReadOnlyList<LaneKind> RoslynOnly = [LaneKind.Roslyn];
    private static readonly IReadOnlyList<LaneKind> DiagnosticLanes = [LaneKind.Jazor, LaneKind.Roslyn, LaneKind.Frontend];

    public IReadOnlyList<LaneKind> GetOrderedLanes(ProjectionTarget projectionTarget)
        => projectionTarget.LaneKind switch
        {
            LaneKind.Frontend => FrontendOnly,
            LaneKind.Roslyn => RoslynOnly,
            _ => JazorOnly
        };

    public IReadOnlyList<LaneKind> GetDiagnosticLanes(DocumentSnapshot document)
        => document.DocumentKind switch
        {
            DocumentKind.Jazor => DiagnosticLanes,
            DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript => FrontendOnly,
            _ => JazorOnly
        };

    public IReadOnlyList<LaneKind> GetSemanticTokenLanes(DocumentSnapshot document)
        => document.DocumentKind switch
        {
            DocumentKind.Jazor => JazorSemanticTokenLanes,
            DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript => FrontendOnly,
            _ => JazorOnly
        };
}
