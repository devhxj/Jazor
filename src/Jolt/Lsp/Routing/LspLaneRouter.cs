using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Lsp.Routing;

internal sealed class LspLaneRouter : ILspLaneRouter
{
    private static readonly IReadOnlyList<LaneKind> JazorOnly = [LaneKind.Jazor];
    private static readonly IReadOnlyList<LaneKind> JazorSemanticTokenLanes = [LaneKind.Volar, LaneKind.Roslyn];
    private static readonly IReadOnlyList<LaneKind> VolarOnly = [LaneKind.Volar];
    private static readonly IReadOnlyList<LaneKind> RoslynOnly = [LaneKind.Roslyn];
    private static readonly IReadOnlyList<LaneKind> DiagnosticLanes = [LaneKind.Jazor, LaneKind.Roslyn, LaneKind.Volar];

    public IReadOnlyList<LaneKind> GetOrderedLanes(ProjectionTarget projectionTarget)
        => projectionTarget.LaneKind switch
        {
            LaneKind.Volar => VolarOnly,
            LaneKind.Roslyn => RoslynOnly,
            _ => JazorOnly
        };

    public IReadOnlyList<LaneKind> GetDiagnosticLanes(DocumentSnapshot document)
        => document.DocumentKind switch
        {
            DocumentKind.Jazor => DiagnosticLanes,
            DocumentKind.CSharp => RoslynOnly,
            DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css => VolarOnly,
            _ => JazorOnly
        };

    public IReadOnlyList<LaneKind> GetSemanticTokenLanes(DocumentSnapshot document)
        => document.DocumentKind switch
        {
            DocumentKind.Jazor => JazorSemanticTokenLanes,
            DocumentKind.CSharp => RoslynOnly,
            DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css => VolarOnly,
            _ => JazorOnly
        };
}
