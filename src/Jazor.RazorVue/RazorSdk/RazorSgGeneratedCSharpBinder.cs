using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

internal static class RazorSgGeneratedCSharpBinder
{
    public static bool TryBindHandwritten(
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> components,
        out RazorSgGeneratedCSharpBinding? binding,
        out string? failure)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        binding = null;
        failure = null;
        var documents = ImmutableArray.CreateBuilder<RazorSgGeneratedDocument>();
        var boundComponents = ImmutableArray.CreateBuilder<RazorSgBoundComponent>();
        var documentByTree = new Dictionary<SyntaxTree, RazorSgGeneratedDocument>();
        foreach (var componentSymbol in components.OrderBy(
                     static component => component.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                     StringComparer.Ordinal))
        {
            var buildRenderTree = RazorSgComponentCandidateSelector.FindHandwrittenBuildRenderTreeMethod(componentSymbol);
            if (buildRenderTree is null ||
                !TryBindBuildRenderTreeBody(
                    compilation,
                    componentSymbol,
                    buildRenderTree,
                    documentByTree,
                    documents,
                    out var boundComponent,
                    out failure))
            {
                failure ??= "RazorVue component '" +
                            componentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) +
                            "' did not declare a handwritten BuildRenderTree(RenderTreeBuilder).";
                return false;
            }

            boundComponents.Add(boundComponent!);
        }

        binding = new RazorSgGeneratedCSharpBinding(
            compilation,
            RazorSgCompilationBindingMode.ReusedHookCompilation,
            documents.ToImmutable(),
            boundComponents.ToImmutable(),
            ReusedGeneratedTreeCount: documents.Count,
            DerivedGeneratedTreeCount: 0);
        return true;
    }

    public static bool TryBindFinalCompilation(
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> components,
        out RazorSgGeneratedCSharpBinding? binding,
        out string? failure)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        binding = null;
        failure = null;
        var documents = ImmutableArray.CreateBuilder<RazorSgGeneratedDocument>();
        var boundComponents = ImmutableArray.CreateBuilder<RazorSgBoundComponent>();
        var documentByTree = new Dictionary<SyntaxTree, RazorSgGeneratedDocument>();
        foreach (var componentSymbol in components.OrderBy(
                     static component => component.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                     StringComparer.Ordinal))
        {
            var buildRenderTree = RazorSgComponentCandidateSelector.FindBuildRenderTreeMethod(componentSymbol);
            if (buildRenderTree is null ||
                !TryBindBuildRenderTreeBody(
                    compilation,
                    componentSymbol,
                    buildRenderTree,
                    documentByTree,
                    documents,
                    out var boundComponent,
                    out failure))
            {
                failure ??= "RazorVue component '" +
                            componentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) +
                            "' did not declare BuildRenderTree(RenderTreeBuilder).";
                return false;
            }

            boundComponents.Add(boundComponent!);
        }

        binding = new RazorSgGeneratedCSharpBinding(
            compilation,
            RazorSgCompilationBindingMode.ReusedHookCompilation,
            documents.ToImmutable(),
            boundComponents.ToImmutable(),
            ReusedGeneratedTreeCount: documents.Count,
            DerivedGeneratedTreeCount: 0);
        return true;
    }

    private static bool TryBindBuildRenderTreeBody(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IMethodSymbol buildRenderTree,
        Dictionary<SyntaxTree, RazorSgGeneratedDocument> documentByTree,
        ImmutableArray<RazorSgGeneratedDocument>.Builder documents,
        out RazorSgBoundComponent? component,
        out string? failure)
    {
        component = null;
        failure = null;
        var syntaxReference = buildRenderTree.DeclaringSyntaxReferences
            .FirstOrDefault(reference => reference.GetSyntax() is MethodDeclarationSyntax);
        if (syntaxReference is null ||
            syntaxReference.GetSyntax() is not MethodDeclarationSyntax declaration ||
            declaration.Body is null)
        {
            failure = "RazorVue component '" +
                      componentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) +
                      "' did not expose a bindable BuildRenderTree body.";
            return false;
        }

        var syntaxTree = declaration.SyntaxTree;
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        if (semanticModel.GetOperation(declaration.Body) is not IBlockOperation body)
        {
            failure = "RazorVue component '" +
                      componentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) +
                      "' did not provide a bindable BuildRenderTree block operation.";
            return false;
        }

        if (!documentByTree.TryGetValue(syntaxTree, out var document))
        {
            // Razor SG keeps the physical generated tree path, but the BuildRenderTree
            // body retains #line mappings to the authored Razor document. Source-map
            // fallback must preserve that author-facing identity rather than expose .g.cs.
            document = new RazorSgGeneratedDocument(
                Path.GetFileName(syntaxTree.FilePath),
                GetSourceDocumentPath(componentSymbol, declaration),
                syntaxTree.GetText(),
                ImmutableArray<RazorSgSourceMapping>.Empty);
            documentByTree.Add(syntaxTree, document);
            documents.Add(document);
        }

        component = new RazorSgBoundComponent(document, componentSymbol, buildRenderTree, body);
        return true;
    }

    private static string GetSourceDocumentPath(
        INamedTypeSymbol componentSymbol,
        MethodDeclarationSyntax declaration)
    {
        var generatedPath = declaration.SyntaxTree.FilePath;
        if (!IsGeneratedSourcePath(generatedPath))
            return generatedPath;

        foreach (var nodeOrToken in declaration.DescendantNodesAndTokensAndSelf())
        {
            var location = nodeOrToken.GetLocation();
            if (location is null)
                continue;

            var mappedSpan = location.GetMappedLineSpan();
            var mappedPath = mappedSpan.Path;
            if (mappedSpan.HasMappedPath &&
                !string.IsNullOrWhiteSpace(mappedPath) &&
                mappedPath.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            {
                return mappedPath;
            }
        }

        // Static markup can legitimately have no usable #line span. A matching
        // Component.razor.cs partial is the only source-tree convention that identifies
        // the authored Razor file without guessing from the generated hint name.
        foreach (var syntaxReference in componentSymbol.DeclaringSyntaxReferences)
        {
            var sourcePath = syntaxReference.SyntaxTree.FilePath;
            if (sourcePath.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
                return sourcePath;
            if (!sourcePath.EndsWith(".razor.cs", StringComparison.OrdinalIgnoreCase))
                continue;

            var razorPath = sourcePath.Substring(0, sourcePath.Length - ".cs".Length);
            if (string.Equals(
                    Path.GetFileNameWithoutExtension(razorPath),
                    componentSymbol.Name,
                    StringComparison.Ordinal))
            {
                return razorPath;
            }
        }

        return generatedPath;
    }

    private static bool IsGeneratedSourcePath(string? path)
        => !string.IsNullOrWhiteSpace(path) &&
           (path!.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase));
}

internal enum RazorSgCompilationBindingMode
{
    ReusedHookCompilation
}

internal sealed record RazorSgGeneratedCSharpBinding(
    Compilation Compilation,
    RazorSgCompilationBindingMode BindingMode,
    ImmutableArray<RazorSgGeneratedDocument> Documents,
    ImmutableArray<RazorSgBoundComponent> Components,
    int ReusedGeneratedTreeCount,
    int DerivedGeneratedTreeCount);

internal sealed record RazorSgBoundComponent(
    RazorSgGeneratedDocument Document,
    INamedTypeSymbol ComponentSymbol,
    IMethodSymbol BuildRenderTreeMethod,
    IBlockOperation BuildRenderTreeBody);
