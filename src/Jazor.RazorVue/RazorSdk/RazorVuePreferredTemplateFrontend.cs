using System;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.RazorSdk;

internal sealed class RazorVuePreferredTemplateFrontend : IRazorVueTemplateFrontend
{
    private readonly RazorVueRazorIrTemplateFrontend _razorIrFrontend = new();

    public static RazorVuePreferredTemplateFrontend Instance { get; } = new();

    private RazorVuePreferredTemplateFrontend()
    {
    }

    public string Name => "Jazor.RazorVue.RazorSdk.RazorVuePreferredTemplateFrontend";

    public RazorVueRenderFragment CreateRenderTree(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        if (snapshot.RazorIrCarrier is not null || snapshot.RazorSourceGeneratorDocument is not null)
            return _razorIrFrontend.CreateRenderTree(context, snapshot);

        if (snapshot.BuildRenderTreeMethod is null)
            return RazorVueRenderFragment.Empty;

        if (RazorVueBuildRenderTreeAuthoringClassifier.IsHandwrittenBuildRenderTree(snapshot))
            return BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        throw new InvalidOperationException(
            $"RazorVue preferred template frontend only falls back to BuildRenderTree for source-authored components. " +
            $"Component '{snapshot.Descriptor.FullName}' did not resolve a Razor document and was not classified as handwritten BuildRenderTree authoring.");
    }
}
