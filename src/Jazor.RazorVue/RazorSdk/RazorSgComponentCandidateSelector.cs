using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.RazorSdk;

// The G0 tail boundary must not instantiate the legacy Razor document/IR frontend.
internal static class RazorSgComponentCandidateSelector
{
    private static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;
    private const string ECMAScriptModuleAttributeMetadataName = "ECMAScript.ECMAScriptModuleAttribute";
    private const string ComponentBaseMetadataName = "Microsoft.AspNetCore.Components.ComponentBase";
    private const string VueComponentMarkerMetadataName = "ECMAScript.Vue3+IVueComponent";

    public static ImmutableArray<INamedTypeSymbol> DiscoverCurrentComponents(Compilation compilation)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        var moduleAttribute = compilation.GetTypeByMetadataName(ECMAScriptModuleAttributeMetadataName);
        var componentBase = compilation.GetTypeByMetadataName(ComponentBaseMetadataName);
        var vueComponentMarker = compilation.GetTypeByMetadataName(VueComponentMarkerMetadataName);
        if (moduleAttribute is null || componentBase is null || vueComponentMarker is null)
            return ImmutableArray<INamedTypeSymbol>.Empty;

        var components = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        foreach (var symbol in EnumerateNamedTypes(compilation.GlobalNamespace))
        {
            if (!IsRazorVueComponent(symbol, moduleAttribute, componentBase, vueComponentMarker) ||
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

        var buildRenderTree = FindBuildRenderTreeMethod(component);
        return buildRenderTree is not null && HasRazorSourceIdentity(buildRenderTree);
    }

    private static bool HasHandwrittenBuildRenderTree(INamedTypeSymbol component)
    {
        var buildRenderTree = FindBuildRenderTreeMethod(component);
        if (buildRenderTree is null)
            return false;

        foreach (var syntaxReference in buildRenderTree.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
                continue;

            if (HasMappedRazorPath(methodSyntax) || IsGeneratedSourcePath(methodSyntax.SyntaxTree.FilePath))
                continue;

            return true;
        }

        return false;
    }

    private static bool IsRazorVueComponent(
        INamedTypeSymbol symbol,
        INamedTypeSymbol moduleAttribute,
        INamedTypeSymbol componentBase,
        INamedTypeSymbol vueComponentMarker)
        => !symbol.IsStatic &&
           HasECMAScriptModuleAttribute(symbol, moduleAttribute) &&
           DerivesFrom(symbol, componentBase) &&
           Implements(symbol, vueComponentMarker);

    private static bool HasECMAScriptModuleAttribute(INamedTypeSymbol symbol, INamedTypeSymbol moduleAttribute)
        => symbol.GetAttributes().Any(attribute =>
            Comparer.Equals(attribute.AttributeClass, moduleAttribute) ||
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                ECMAScriptModuleAttributeMetadataName,
                StringComparison.Ordinal));

    private static bool DerivesFrom(INamedTypeSymbol symbol, INamedTypeSymbol componentBase)
    {
        for (var current = symbol; current is not null; current = current.BaseType)
        {
            if (Comparer.Equals(current.OriginalDefinition, componentBase))
                return true;
        }

        return false;
    }

    private static bool Implements(INamedTypeSymbol symbol, INamedTypeSymbol vueComponentMarker)
        => symbol.AllInterfaces.Any(candidate => Comparer.Equals(candidate.OriginalDefinition, vueComponentMarker));

    private static IMethodSymbol? FindBuildRenderTreeMethod(INamedTypeSymbol symbol)
    {
        for (var current = symbol; current is not null; current = current.BaseType)
        {
            var method = current.GetMembers("BuildRenderTree")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(candidate =>
                    !candidate.IsStatic &&
                    candidate.MethodKind == MethodKind.Ordinary &&
                    candidate.Parameters.Length == 1 &&
                    (candidate.Locations.Any(static location => location.IsInSource) ||
                     candidate.DeclaringSyntaxReferences.Length > 0));
            if (method is not null)
                return method;
        }

        return null;
    }

    private static bool HasMappedRazorPath(MethodDeclarationSyntax methodSyntax)
    {
        foreach (var nodeOrToken in methodSyntax.DescendantNodesAndTokensAndSelf())
        {
            var location = nodeOrToken.GetLocation();
            if (location is null || !location.IsInSource)
                continue;

            var mappedSpan = location.GetMappedLineSpan();
            if (mappedSpan.HasMappedPath && HasRazorSourcePath(mappedSpan.Path))
                return true;
        }

        return false;
    }

    private static bool IsGeneratedSourcePath(string? path)
        => !string.IsNullOrWhiteSpace(path) &&
           (path!.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase));

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
