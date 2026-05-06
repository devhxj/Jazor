using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue;

internal static class RazorVueRazorDocumentLocator
{
    public static string? TryResolvePrimaryDocumentPath(RazorVueComponentCandidate candidate)
    {
        if (candidate is null)
            throw new ArgumentNullException(nameof(candidate));

        if (TryResolvePrimaryDocumentPath(candidate.BuildRenderTreeMethod) is string buildRenderTreePath)
            return buildRenderTreePath;

        foreach (var location in candidate.ComponentSymbol.Locations)
        {
            if (TryGetMappedRazorDocumentPath(location, out var componentPath))
                return componentPath;
        }

        return null;
    }

    public static ImmutableArray<string> ResolveImportDocumentPaths(RazorVueComponentCandidate candidate)
    {
        if (candidate is null)
            throw new ArgumentNullException(nameof(candidate));

        var builder = ImmutableArray.CreateBuilder<string>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var syntaxReference in candidate.ComponentSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not TypeDeclarationSyntax declaration ||
                declaration.SyntaxTree.GetRoot() is not CompilationUnitSyntax compilationUnit)
            {
                continue;
            }

            foreach (var usingDirective in compilationUnit.Usings)
            {
                if (!TryGetMappedImportsPath(usingDirective.GetLocation(), out var importPath) ||
                    !seenPaths.Add(importPath))
                {
                    continue;
                }

                builder.Add(importPath);
            }
        }

        return builder.ToImmutable();
    }

    private static string? TryResolvePrimaryDocumentPath(IMethodSymbol? buildRenderTreeMethod)
    {
        if (buildRenderTreeMethod is null)
            return null;

        foreach (var syntaxReference in buildRenderTreeMethod.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not MethodDeclarationSyntax methodDeclaration)
                continue;

            foreach (var location in EnumerateMappedTemplateLocations(methodDeclaration))
            {
                if (TryGetMappedRazorDocumentPath(location, out var razorPath))
                    return razorPath;
            }
        }

        return null;
    }

    private static IEnumerable<Location> EnumerateMappedTemplateLocations(MethodDeclarationSyntax methodDeclaration)
    {
        if (methodDeclaration.ExpressionBody is not null)
            yield return methodDeclaration.ExpressionBody.Expression.GetLocation();

        if (methodDeclaration.Body is null)
            yield break;

        foreach (var nodeOrToken in methodDeclaration.Body.DescendantNodesAndTokensAndSelf())
        {
            yield return nodeOrToken.GetLocation();
        }
    }

    private static bool TryGetMappedRazorDocumentPath(Location location, out string path)
        => TryGetMappedPath(location, out path) &&
           path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) &&
           !path.EndsWith("_Imports.razor", StringComparison.OrdinalIgnoreCase);

    private static bool TryGetMappedImportsPath(Location location, out string path)
        => TryGetMappedPath(location, out path) &&
           path.EndsWith("_Imports.razor", StringComparison.OrdinalIgnoreCase);

    private static bool TryGetMappedPath(Location location, out string path)
    {
        path = string.Empty;
        if (location is null || !location.IsInSource)
            return false;

        var mappedSpan = location.GetMappedLineSpan();
        if (mappedSpan.HasMappedPath &&
            !string.IsNullOrWhiteSpace(mappedSpan.Path))
        {
            path = mappedSpan.Path;
            return true;
        }

        return false;
    }
}
