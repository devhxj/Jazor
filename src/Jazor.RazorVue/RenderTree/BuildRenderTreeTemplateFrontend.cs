using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Extensibility;

namespace Jazor.RazorVue.RenderTree;

internal sealed class BuildRenderTreeTemplateFrontend : IRazorVueTemplateFrontend, IRazorVueRenderBaselineExtractor
{
    private readonly RazorVueRenderTreeExtractor _renderTreeExtractor = new();

    public static BuildRenderTreeTemplateFrontend Instance { get; } = new();

    private BuildRenderTreeTemplateFrontend()
    {
    }

    public string Name => "Jazor.RazorVue.BuildRenderTreeTemplateFrontend";

    public RazorVueRenderFragment CreateRenderTree(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
        => CreateRenderBaseline(context, snapshot);

    public RazorVueRenderFragment CreateRenderBaseline(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        return _renderTreeExtractor.Extract(context, snapshot);
    }
}
