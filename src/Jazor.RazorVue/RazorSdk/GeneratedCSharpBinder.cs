using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Binds final-compilation BuildRenderTree methods to Roslyn operations and source-map documents.
/// This is the boundary from Razor SG output into RazorVue lowering.
/// 同时保留 authored Razor source identity，避免 sourcemap 把用户导向 SDK 的 .g.cs 文件。
/// </summary>
internal static class GeneratedCSharpBinder
{
    public static bool TryBindHandwritten(
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> components,
        out GeneratedCSharpBinding? binding,
        out string? failure)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        binding = null;
        failure = null;
        var documents = ImmutableArray.CreateBuilder<GeneratedDocument>();
        var boundComponents = ImmutableArray.CreateBuilder<BoundComponent>();
        var documentByTree = new Dictionary<SyntaxTree, GeneratedDocument>();
        foreach (var componentSymbol in components.OrderBy(
                     static component => component.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                     StringComparer.Ordinal))
        {
            var buildRenderTree = ComponentSelector.FindHandwrittenBuildRenderTreeMethod(componentSymbol);
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

        binding = new GeneratedCSharpBinding(
            compilation,
            CompilationBindingMode.ReusedHookCompilation,
            documents.ToImmutable(),
            boundComponents.ToImmutable(),
            ReusedGeneratedTreeCount: documents.Count,
            DerivedGeneratedTreeCount: 0);
        return true;
    }

    public static bool TryBindFinalCompilation(
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> components,
        out GeneratedCSharpBinding? binding,
        out string? failure)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        binding = null;
        failure = null;
        var documents = ImmutableArray.CreateBuilder<GeneratedDocument>();
        var boundComponents = ImmutableArray.CreateBuilder<BoundComponent>();
        var documentByTree = new Dictionary<SyntaxTree, GeneratedDocument>();
        foreach (var componentSymbol in components.OrderBy(
                     static component => component.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                     StringComparer.Ordinal))
        {
            var buildRenderTree = ComponentSelector.FindBuildRenderTreeMethod(componentSymbol);
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

        binding = new GeneratedCSharpBinding(
            compilation,
            CompilationBindingMode.ReusedHookCompilation,
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
        Dictionary<SyntaxTree, GeneratedDocument> documentByTree,
        ImmutableArray<GeneratedDocument>.Builder documents,
        out BoundComponent? component,
        out string? failure)
    {
        component = null;
        failure = null;
        // Bind the Roslyn IBlockOperation once at the SG-produced method declaration. All later
        // lowering consumes this semantic body rather than re-parsing generated source text.
        // operation binding 是 SG C# 到 lowering 的唯一语义入口，后续不得再做文本解析。
        var syntaxReference = buildRenderTree.DeclaringSyntaxReferences
            .FirstOrDefault(reference => reference.GetSyntax() is MethodDeclarationSyntax);
        var declaration = syntaxReference?.GetSyntax() as MethodDeclarationSyntax;
        if (declaration?.Body is null)
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
            document = new GeneratedDocument(
                Path.GetFileName(syntaxTree.FilePath),
                GetSourceDocumentPath(componentSymbol, declaration),
                syntaxTree.GetText(),
                ImmutableArray<RazorSourceMap>.Empty);
            documentByTree.Add(syntaxTree, document);
            documents.Add(document);
        }

        component = new BoundComponent(document, componentSymbol, buildRenderTree, body);
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

/// <summary>Records how a generated compilation was obtained for a binding. 区分 Razor SG 与手写 BuildRenderTree 输入。</summary>
internal enum CompilationBindingMode
{
    ReusedHookCompilation
}

/// <summary>Bound source documents and render methods from one final compilation. 是后续 module build 的统一输入快照。</summary>
internal sealed record GeneratedCSharpBinding(
    Compilation Compilation,
    CompilationBindingMode BindingMode,
    ImmutableArray<GeneratedDocument> Documents,
    ImmutableArray<BoundComponent> Components,
    int ReusedGeneratedTreeCount,
    int DerivedGeneratedTreeCount);

/// <summary>One component render method after Roslyn operation binding. 保留符号、operation body 与 source document 的对应关系。</summary>
internal sealed record BoundComponent(
    GeneratedDocument Document,
    INamedTypeSymbol ComponentSymbol,
    IMethodSymbol BuildRenderTreeMethod,
    IBlockOperation BuildRenderTreeBody);
