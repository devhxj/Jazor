using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.Extensibility;

/// <summary>
/// Adapts the baseline-plus-enhancement render contract to the legacy
/// single-frontend shape used by current artifact factories.
/// </summary>
internal sealed class RazorVueBaselineFirstTemplateFrontend : IRazorVueTemplateFrontend
{
    private readonly IRazorVueRenderBaselineExtractor _baselineExtractor;
    private readonly ImmutableArray<IRazorVueRenderEnhancement> _enhancements;

    public RazorVueBaselineFirstTemplateFrontend(
        IRazorVueRenderBaselineExtractor baselineExtractor,
        params IRazorVueRenderEnhancement[] enhancements)
    {
        _baselineExtractor = baselineExtractor ?? throw new ArgumentNullException(nameof(baselineExtractor));
        _enhancements = CreateEnhancements(enhancements);
    }

    public string Name => "Jazor.RazorVue.BaselineFirstTemplateFrontend";

    public RazorVueRenderFragment CreateRenderTree(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        var renderTree = _baselineExtractor.CreateRenderBaseline(context, snapshot)
                         ?? throw new InvalidOperationException(
                             $"RazorVue render baseline extractor '{_baselineExtractor.Name}' returned null for component '{snapshot.Descriptor.FullName}'.");

        foreach (var enhancement in _enhancements)
        {
            if (!enhancement.TryEnhanceRenderTree(context, snapshot, renderTree, out var enhancedRenderTree))
                continue;

            renderTree = enhancedRenderTree
                         ?? throw new InvalidOperationException(
                             $"RazorVue render enhancement '{enhancement.Name}' returned null for component '{snapshot.Descriptor.FullName}'.");
        }

        return renderTree;
    }

    private static ImmutableArray<IRazorVueRenderEnhancement> CreateEnhancements(
        IRazorVueRenderEnhancement[]? enhancements)
    {
        if (enhancements is null || enhancements.Length == 0)
            return ImmutableArray<IRazorVueRenderEnhancement>.Empty;

        var builder = ImmutableArray.CreateBuilder<IRazorVueRenderEnhancement>(enhancements.Length);
        foreach (var enhancement in enhancements)
        {
            if (enhancement is null)
                throw new ArgumentException("RazorVue render enhancement list cannot contain null entries.", nameof(enhancements));

            builder.Add(enhancement);
        }

        return builder.MoveToImmutable();
    }
}
