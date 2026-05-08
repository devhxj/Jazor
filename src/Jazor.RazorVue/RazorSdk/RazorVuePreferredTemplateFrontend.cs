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

        if (snapshot.RazorIrCarrier is not null)
            return _razorIrFrontend.CreateRenderTree(context, snapshot);

        if (snapshot.BuildRenderTreeMethod is null)
            return RazorVueRenderFragment.Empty;

        if (IsHandwrittenBuildRenderTree(snapshot))
            return BuildRenderTreeTemplateFrontend.Instance.CreateRenderTree(context, snapshot);

        throw new InvalidOperationException(
            $"RazorVue preferred template frontend only falls back to BuildRenderTree for source-authored components. " +
            $"Component '{snapshot.Descriptor.FullName}' did not resolve a Razor document and was not classified as handwritten BuildRenderTree authoring.");
    }

    private static bool IsHandwrittenBuildRenderTree(RazorVueSemanticSnapshot snapshot)
    {
        foreach (var syntaxReference in snapshot.BuildRenderTreeMethod!.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
                continue;

            if (HasMappedRazorPath(methodSyntax))
                return false;

            if (IsGeneratedSourcePath(methodSyntax.SyntaxTree.FilePath))
                continue;

            return true;
        }

        return false;
    }

    private static bool HasMappedRazorPath(MethodDeclarationSyntax methodSyntax)
    {
        foreach (var nodeOrToken in methodSyntax.DescendantNodesAndTokensAndSelf())
        {
            var location = nodeOrToken.GetLocation();
            if (location is null || !location.IsInSource)
                continue;

            var mappedSpan = location.GetMappedLineSpan();
            if (!mappedSpan.HasMappedPath || string.IsNullOrWhiteSpace(mappedSpan.Path))
                continue;

            if (mappedSpan.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool IsGeneratedSourcePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var currentPath = path!;
        return currentPath.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase) ||
               currentPath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
               currentPath.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) ||
               currentPath.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase);
    }
}
