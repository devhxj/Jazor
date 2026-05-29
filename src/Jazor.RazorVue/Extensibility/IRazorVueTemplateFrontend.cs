using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.Extensibility;

/// <summary>
/// Compatibility seam for callers that still expect one render-tree-producing frontend.
/// New RazorVue component lowering should prefer <see cref="IRazorVueRenderBaselineExtractor"/>
/// plus optional <see cref="IRazorVueRenderEnhancement"/> layers so Roslyn/BuildRenderTree
/// remains the semantic baseline and Razor IR stays an enhancement input.
/// </summary>
internal interface IRazorVueTemplateFrontend
{
    string Name { get; }

    RazorVueRenderFragment CreateRenderTree(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot);
}

internal interface IRazorVueRenderBaselineExtractor
{
    string Name { get; }

    RazorVueRenderFragment CreateRenderBaseline(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot);
}

internal interface IRazorVueRenderEnhancement
{
    string Name { get; }

    bool TryEnhanceRenderTree(
        RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRenderFragment baselineRenderTree,
        out RazorVueRenderFragment enhancedRenderTree);
}
