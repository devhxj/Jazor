using System.Collections.Immutable;
using Jazor.Common;
using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Finds component candidates whose render methods can be consumed from the final compilation.
/// The G0 tail boundary must not instantiate the legacy Razor document or IR frontend.
/// 它只选择当前 compilation 的 RazorVue 根组件，后续 binder 才负责取得 BuildRenderTree operation。
/// </summary>
internal static class ComponentSelector
{
    private static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;
    private const string ECMAScriptModuleAttributeMetadataName = "ECMAScript.ECMAScriptModuleAttribute";
    private const string RenderTreeBuilderMetadataName = "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder";

    public static ImmutableArray<INamedTypeSymbol> DiscoverCurrentComponents(Compilation compilation)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        var moduleAttribute = compilation.GetTypeByMetadataName(ECMAScriptModuleAttributeMetadataName);
        var vueComponentMarker = compilation.GetTypeByMetadataName(ComponentSymbolPolicy.VueComponentMarkerMetadataName);
        var componentBase = compilation.GetTypeByMetadataName(ComponentSymbolPolicy.ComponentBaseMetadataName);
        if (vueComponentMarker is null || componentBase is null)
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

    internal static ImmutableArray<RazorVueDiagnosticInfo> ValidateCurrentComponentContracts(Compilation compilation)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        var moduleAttribute = compilation.GetTypeByMetadataName(ECMAScriptModuleAttributeMetadataName);
        var vueComponentMarker = compilation.GetTypeByMetadataName(ComponentSymbolPolicy.VueComponentMarkerMetadataName);
        var componentBase = compilation.GetTypeByMetadataName(ComponentSymbolPolicy.ComponentBaseMetadataName);
        if (vueComponentMarker is null || componentBase is null)
            return ImmutableArray<RazorVueDiagnosticInfo>.Empty;

        var diagnostics = ImmutableArray.CreateBuilder<RazorVueDiagnosticInfo>();
        foreach (var symbol in EnumerateNamedTypes(compilation.GlobalNamespace)
                     .Where(symbol => Comparer.Equals(symbol.ContainingAssembly, compilation.Assembly) &&
                                      HasCurrentCompilationSource(symbol))
                     .OrderBy(static symbol => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal))
        {
            // Abstract source bases and external bindings are contracts, not missing output roots.
            // 只审计当前程序集的具体组件；抽象共用基类不需要自己的模块声明。
            if (symbol.TypeKind != TypeKind.Class || symbol.IsStatic || symbol.IsAbstract)
                continue;

            var hasMarker = ComponentSymbolPolicy.Implements(symbol, vueComponentMarker);
            var inheritsComponentBase = ComponentSymbolPolicy.InheritsFrom(symbol, componentBase);
            var hasDescriptor = HasComponentImportDescriptor(symbol, moduleAttribute);
            // A code-behind file may declare helper types too. Its .razor.cs path alone does
            // not make every type in that tree an authored component candidate.
            var isRazorAuthored = symbol.GetMembers("BuildRenderTree")
                .OfType<IMethodSymbol>().Any(HasRazorSourceIdentity) ||
                symbol.DeclaringSyntaxReferences.Any(reference => string.Equals(
                    System.IO.Path.GetFileName(reference.SyntaxTree.FilePath),
                    symbol.Name + ".razor.cs", StringComparison.OrdinalIgnoreCase));
            if (!hasMarker && !(inheritsComponentBase && (hasDescriptor || isRazorAuthored)))
                continue;

            string? failure = !inheritsComponentBase
                ? "must derive from Microsoft.AspNetCore.Components.ComponentBase"
                : !hasMarker
                    ? "must implement ECMAScript.Vue.IVueComponent (for example: ComponentBase, IVueComponent)"
                    : !hasDescriptor
                        ? "must declare [ECMAScriptModule(\"./components/name\")] for a generated component or [ECMAScript(\"package\")] for an external binding"
                        : null;
            if (failure is not null)
            {
                diagnostics.Add(RazorVueDiagnosticFactory.Create(
                    RazorVueDiagnosticCategory.ComponentCandidate,
                    "Component '" + symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) + "' " + failure + ".",
                    component: symbol));
            }
        }

        return diagnostics.ToImmutable();
    }

    public static ImmutableArray<INamedTypeSymbol> DiscoverTailRequiredComponents(Compilation compilation)
        => DiscoverCurrentComponents(compilation)
            .Where(static component => IsLikelyRazorAuthored(component) && !HasHandwrittenBuildRenderTree(component))
            .ToImmutableArray();

    public static ImmutableArray<INamedTypeSymbol> DiscoverHandwrittenComponents(Compilation compilation)
        => DiscoverCurrentComponents(compilation)
            .Where(static component => HasHandwrittenBuildRenderTree(component))
            .ToImmutableArray();

    public static ImmutableArray<INamedTypeSymbol> DiscoverTailOutputComponents(Compilation compilation)
        => DiscoverCurrentComponents(compilation)
            .Where(static component => FindBuildRenderTreeMethod(component) is not null)
            .ToImmutableArray();

    public static IMethodSymbol? FindHandwrittenBuildRenderTreeMethod(INamedTypeSymbol component)
    {
        var buildRenderTree = FindBuildRenderTreeMethod(component);
        if (buildRenderTree is null)
            return null;

        foreach (var syntaxReference in buildRenderTree.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
                continue;

            if (HasMappedRazorPath(methodSyntax) || IsGeneratedSourcePath(methodSyntax.SyntaxTree.FilePath))
                continue;

            return buildRenderTree;
        }

        return null;
    }

    public static bool TrySelect(
        GeneratedCSharpBinding binding,
        out GeneratedCSharpBinding? selectedBinding,
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
        => FindHandwrittenBuildRenderTreeMethod(component) is not null;

    private static bool IsRazorVueComponent(
        INamedTypeSymbol symbol,
        INamedTypeSymbol? moduleAttribute,
        INamedTypeSymbol componentBase,
        INamedTypeSymbol vueComponentMarker)
        => !symbol.IsStatic &&
           ComponentSymbolPolicy.IsRazorVueComponent(symbol, componentBase, vueComponentMarker) &&
           HasComponentImportDescriptor(symbol, moduleAttribute);

    private static bool HasComponentImportDescriptor(
        INamedTypeSymbol symbol,
        INamedTypeSymbol? moduleAttribute)
        => HasECMAScriptModuleAttribute(symbol, moduleAttribute) ||
           symbol.GetAttributes().Any(ECMAScriptComponentMetadata.IsComponentAttribute);

    private static bool HasECMAScriptModuleAttribute(INamedTypeSymbol symbol, INamedTypeSymbol? moduleAttribute)
        => symbol.GetAttributes().Any(attribute =>
            (moduleAttribute is not null && Comparer.Equals(attribute.AttributeClass, moduleAttribute)) ||
            string.Equals(
                attribute.AttributeClass?.ToDisplayString(),
                ECMAScriptModuleAttributeMetadataName,
                StringComparison.Ordinal));

    internal static IMethodSymbol? FindBuildRenderTreeMethod(INamedTypeSymbol symbol)
    {
        for (var current = symbol; current is not null; current = current.BaseType)
        {
            var method = current.GetMembers("BuildRenderTree")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(candidate =>
                    !candidate.IsStatic &&
                    candidate.MethodKind == MethodKind.Ordinary &&
                    candidate.Parameters.Length == 1 &&
                    string.Equals(
                        candidate.Parameters[0].Type.ToDisplayString(),
                        RenderTreeBuilderMetadataName,
                        StringComparison.Ordinal) &&
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
            var location = nodeOrToken.GetLocation()!;
            // Syntax nodes and tokens belong to their source tree, so this path is always
            // source-backed; metadata filtering is only needed for symbol locations below.
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

    private static string DescribeGeneratedComponents(IEnumerable<BoundComponent> components)
        => string.Join(
            ", ",
            components
                .Select(static component => component.ComponentSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat))
                .OrderBy(static component => component, StringComparer.Ordinal));
}
