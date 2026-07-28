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
            document = new RazorSgGeneratedDocument(
                Path.GetFileName(syntaxTree.FilePath),
                syntaxTree.FilePath,
                syntaxTree.GetText(),
                ImmutableArray<RazorSgSourceMapping>.Empty);
            documentByTree.Add(syntaxTree, document);
            documents.Add(document);
        }

        component = new RazorSgBoundComponent(document, componentSymbol, buildRenderTree, body);
        return true;
    }
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
