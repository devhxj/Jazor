using System.Collections;
using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.RazorSdk;

internal static class RazorVueRazorSourceGeneratorTailBridge
{
    public static RazorVueRazorSourceGeneratorTailBridgeResult ExecuteSfcPipeline(
        Compilation compilation,
        ImmutableArray<RazorVueRazorSourceGeneratorDocumentInput> documents)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        if (documents.IsDefaultOrEmpty)
        {
            return RazorVueRazorSourceGeneratorTailBridgeResult.Succeed(
                new RazorVueSfcCatalog(
                    compilation.AssemblyName ?? "Jazor.Assembly",
                    ImmutableArray<VueSfcArtifact>.Empty),
                generatorDocumentCount: 0);
        }

        var generatorDocuments = ImmutableArray.CreateBuilder<RazorVueRazorSourceGeneratorDocument>(documents.Length);
        foreach (var document in documents)
        {
            if (!TryCreateDocument(compilation, document, out var generatorDocument, out var failure))
            {
                return RazorVueRazorSourceGeneratorTailBridgeResult.Fail(
                    failure ?? "The Razor source generator document could not be converted to RazorVue IR input.",
                    generatorDocuments.Count);
            }

            generatorDocuments.Add(generatorDocument!);
        }

        var immutableGeneratorDocuments = generatorDocuments.ToImmutable();
        var boundCompilation = BindGeneratedRazorSources(compilation, immutableGeneratorDocuments);
        var context = RazorVueCompilationContext.TryCreate(boundCompilation);
        if (context is null)
        {
            return RazorVueRazorSourceGeneratorTailBridgeResult.Succeed(
                new RazorVueSfcCatalog(
                    boundCompilation.AssemblyName ?? "Jazor.Assembly",
                    ImmutableArray<VueSfcArtifact>.Empty),
                generatorDocuments.Count);
        }

        var pipeline = new RazorVueSfcPipeline(
            new RazorVueRazorSourceGeneratorSemanticFrontend(immutableGeneratorDocuments),
            new RazorVueBaselineFirstTemplateFrontend(
                BuildRenderTreeTemplateFrontend.Instance,
                new RazorVueRazorIrTemplateFrontend()));
        return RazorVueRazorSourceGeneratorTailBridgeResult.Succeed(
            pipeline.Execute(context),
            generatorDocuments.Count);
    }

    private static bool TryCreateDocument(
        Compilation compilation,
        RazorVueRazorSourceGeneratorDocumentInput input,
        out RazorVueRazorSourceGeneratorDocument? document,
        out string? failure)
    {
        document = null;
        failure = null;

        return RazorVueReflectedRazorIrReader.TryCreateDocument(
            input.HintName,
            input.CodeDocument,
            input.CSharpDocument,
            out document,
            out failure);
    }

    private static Compilation BindGeneratedRazorSources(
        Compilation compilation,
        ImmutableArray<RazorVueRazorSourceGeneratorDocument> documents)
    {
        if (documents.IsDefaultOrEmpty)
            return compilation;

        var parseOptions = compilation.SyntaxTrees
            .Select(static tree => tree.Options)
            .OfType<CSharpParseOptions>()
            .FirstOrDefault();
        var trees = ImmutableArray.CreateBuilder<SyntaxTree>(documents.Length);
        var existingTexts = compilation.SyntaxTrees
            .Select(static tree => tree.GetText())
            .ToImmutableArray();

        foreach (var document in documents)
        {
            if (existingTexts.Any(text => text.ContentEquals(document.CSharpText)))
                continue;

            trees.Add(CSharpSyntaxTree.ParseText(
                document.CSharpText,
                options: parseOptions,
                path: string.IsNullOrWhiteSpace(document.HintName)
                    ? document.PrimaryDocument.Path + ".g.cs"
                    : document.HintName));
        }

        return trees.Count == 0
            ? compilation
            : compilation.AddSyntaxTrees(trees);
    }

    private static string? TryReadGeneratedNamespace(object? csharpDocument)
    {
        var text = csharpDocument?.ToString();
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var root = CSharpSyntaxTree.ParseText(text!).GetRoot();
        return root.DescendantNodes()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault()
            ?.Name
            .ToString();
    }

    private static object? GetPropertyValue(object value, string name)
        => value.GetType()
            .GetProperty(
                name,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic)
            ?.GetValue(value);
}

internal sealed record RazorVueRazorSourceGeneratorDocumentInput(
    string HintName,
    object CodeDocument,
    object CSharpDocument);

internal sealed record RazorVueRazorSourceGeneratorTailBridgeResult(
    bool Success,
    RazorVueSfcCatalog Catalog,
    int GeneratorDocumentCount,
    string? Failure)
{
    public static RazorVueRazorSourceGeneratorTailBridgeResult Succeed(
        RazorVueSfcCatalog catalog,
        int generatorDocumentCount)
        => new(true, catalog, generatorDocumentCount, null);

    public static RazorVueRazorSourceGeneratorTailBridgeResult Fail(
        string failure,
        int generatorDocumentCount)
        => new(
            false,
            new RazorVueSfcCatalog("Jazor.Assembly", ImmutableArray<VueSfcArtifact>.Empty),
            generatorDocumentCount,
            failure);
}
