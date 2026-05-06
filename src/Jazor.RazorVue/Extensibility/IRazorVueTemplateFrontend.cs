using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.Extensibility;

/// <summary>
/// Owns the template-side projection for a RazorVue semantic snapshot.
/// This seam allows RazorVue to keep the downstream lowering chain stable
/// while swapping the upstream template source from BuildRenderTree to Razor IR.
/// </summary>
internal interface IRazorVueTemplateFrontend
{
    string Name { get; }

    RazorVueRenderFragment CreateRenderTree(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot);
}
