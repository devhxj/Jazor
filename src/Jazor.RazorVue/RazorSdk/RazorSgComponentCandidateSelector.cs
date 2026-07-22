using System.Collections.Immutable;
using Jazor.RazorVue.Discovery;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorSdk;

// The G0 tail boundary must not instantiate the legacy Razor document/IR frontend.
internal static class RazorSgComponentCandidateSelector
{
    private static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;

    public static ImmutableArray<INamedTypeSymbol> DiscoverCurrentComponents(Compilation compilation)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        var symbols = RazorVueCompilationSymbols.TryCreate(compilation);
        if (symbols is null)
            return ImmutableArray<INamedTypeSymbol>.Empty;

        var components = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        foreach (var symbol in EnumerateNamedTypes(compilation.GlobalNamespace))
        {
            if (RazorVueEntryClassifier.Classify(symbol, symbols) != RazorVueEntryKind.RazorVueComponent ||
                !Comparer.Equals(symbol.ContainingAssembly, compilation.Assembly) ||
                !HasCurrentCompilationSource(symbol))
            {
                continue;
            }

            components.Add(symbol);
        }

        return components.ToImmutable();
    }

    public static ImmutableArray<INamedTypeSymbol> DiscoverTailRequiredComponents(Compilation compilation)
        => DiscoverCurrentComponents(compilation)
            .Where(static component => IsLikelyRazorAuthored(component) && !HasHandwrittenBuildRenderTree(component))
            .ToImmutableArray();

    public static bool TrySelect(
        RazorSgGeneratedCSharpBinding binding,
        out RazorSgGeneratedCSharpBinding? selectedBinding,
        out string? failure)
    {
        if (binding is null)
            throw new ArgumentNullException(nameof(binding));

        selectedBinding = null;
        failure = null;
        var candidates = DiscoverCurrentComponents(binding.Compilation);
        if (candidates.IsDefaultOrEmpty)
        {
            failure = "The Razor SG generated BuildRenderTree documents did not match any RazorVue component candidate. " +
                      "RazorVue candidates: <none>. " +
                      "Generated BuildRenderTree components: " +
                      DescribeGeneratedComponents(binding.Components) + ".";
            return false;
        }

        var candidateSet = new HashSet<INamedTypeSymbol>(candidates, Comparer);
        var components = binding.Components
            .Where(component => candidateSet.Contains(component.ComponentSymbol))
            .ToImmutableArray();
        if (!components.IsDefaultOrEmpty)
        {
            selectedBinding = binding with { Components = components };
            return true;
        }

        failure = "The Razor SG generated BuildRenderTree documents did not match any RazorVue component candidate. " +
                  "RazorVue candidates: " + DescribeComponents(candidates) + ". " +
                  "Generated BuildRenderTree components: " + DescribeGeneratedComponents(binding.Components) + ".";
        return false;
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNamedTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var typeSymbol in namespaceSymbol.GetTypeMembers())
        {
            yield return typeSymbol;
            foreach (var nestedType in EnumerateNestedTypes(typeSymbol))
                yield return nestedType;
        }

        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var childType in EnumerateNamedTypes(childNamespace))
                yield return childType;
        }
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNestedTypes(INamedTypeSymbol typeSymbol)
    {
        foreach (var nestedType in typeSymbol.GetTypeMembers())
        {
            yield return nestedType;
            foreach (var nestedChild in EnumerateNestedTypes(nestedType))
                yield return nestedChild;
        }
    }

    private static bool HasCurrentCompilationSource(INamedTypeSymbol symbol)
        => symbol.Locations.Any(static location => location.IsInSource) ||
           symbol.DeclaringSyntaxReferences.Length > 0;

    private static bool IsLikelyRazorAuthored(INamedTypeSymbol component)
    {
        if (HasRazorSourceIdentity(component))
            return true;

        var buildRenderTree = RazorVueEntryClassifier.FindBuildRenderTreeMethod(component);
        return buildRenderTree is not null && HasRazorSourceIdentity(buildRenderTree);
    }

    private static bool HasHandwrittenBuildRenderTree(INamedTypeSymbol component)
        => RazorVueBuildRenderTreeAuthoringClassifier.IsHandwrittenBuildRenderTree(
            RazorVueEntryClassifier.FindBuildRenderTreeMethod(component));

    private static bool HasRazorSourceIdentity(INamedTypeSymbol symbol)
    {
        foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
        {
            if (HasRazorSourcePath(syntaxReference.SyntaxTree.FilePath))
                return true;
        }

        foreach (var location in symbol.Locations)
        {
            if (!location.IsInSource)
                continue;

            var lineSpan = location.GetLineSpan();
            if (HasRazorSourcePath(lineSpan.Path))
                return true;

            var mappedLineSpan = location.GetMappedLineSpan();
            if (mappedLineSpan.HasMappedPath && HasRazorSourcePath(mappedLineSpan.Path))
                return true;
        }

        return false;
    }

    private static bool HasRazorSourceIdentity(IMethodSymbol method)
    {
        foreach (var syntaxReference in method.DeclaringSyntaxReferences)
        {
            if (HasRazorSourcePath(syntaxReference.SyntaxTree.FilePath))
                return true;
        }

        foreach (var location in method.Locations)
        {
            if (!location.IsInSource)
                continue;

            var lineSpan = location.GetLineSpan();
            if (HasRazorSourcePath(lineSpan.Path))
                return true;

            var mappedLineSpan = location.GetMappedLineSpan();
            if (mappedLineSpan.HasMappedPath && HasRazorSourcePath(mappedLineSpan.Path))
                return true;
        }

        return false;
    }

    private static bool HasRazorSourcePath(string? path)
        => !string.IsNullOrWhiteSpace(path) &&
           (path!.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".razor.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase));

    private static string DescribeComponents(IEnumerable<INamedTypeSymbol> components)
        => string.Join(
            ", ",
            components
                .Select(static component => component.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat))
                .OrderBy(static component => component, StringComparer.Ordinal));

    private static string DescribeGeneratedComponents(IEnumerable<RazorSgBoundComponent> components)
        => string.Join(
            ", ",
            components
                .Select(static component => component.ComponentSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat))
                .OrderBy(static component => component, StringComparer.Ordinal));
}
