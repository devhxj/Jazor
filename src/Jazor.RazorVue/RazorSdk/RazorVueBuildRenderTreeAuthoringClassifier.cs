using System;
using Jazor.RazorVue.Artifacts;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.RazorSdk;

internal static class RazorVueBuildRenderTreeAuthoringClassifier
{
    public static bool IsHandwrittenBuildRenderTree(RazorVueSemanticSnapshot snapshot)
    {
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        if (snapshot.BuildRenderTreeMethod is null)
            return false;

        foreach (var syntaxReference in snapshot.BuildRenderTreeMethod.DeclaringSyntaxReferences)
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
