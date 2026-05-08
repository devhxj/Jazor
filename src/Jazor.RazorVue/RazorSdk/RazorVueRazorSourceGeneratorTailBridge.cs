using System.Collections;
using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Text;

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

        var context = RazorVueCompilationContext.TryCreate(compilation);
        if (context is null)
        {
            return RazorVueRazorSourceGeneratorTailBridgeResult.Succeed(
                new RazorVueSfcCatalog(
                    compilation.AssemblyName ?? "Jazor.Assembly",
                    ImmutableArray<VueSfcArtifact>.Empty),
                generatorDocuments.Count);
        }

        var pipeline = new RazorVueSfcPipeline(
            new RazorVueRazorSourceGeneratorSemanticFrontend(generatorDocuments.ToImmutable()),
            RazorVuePreferredTemplateFrontend.Instance);
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

        if (input.CodeDocument is RazorCodeDocument codeDocument &&
            input.CSharpDocument is RazorCSharpDocument csharpDocument)
        {
            document = CreateDocument(input.HintName, codeDocument, csharpDocument);
            return true;
        }

        return TryCreateLocalDocument(compilation, input, out document, out failure);
    }

    private static RazorVueRazorSourceGeneratorDocument CreateDocument(
        string hintName,
        RazorCodeDocument codeDocument,
        RazorCSharpDocument csharpDocument)
        => new(
            hintName,
            codeDocument,
            csharpDocument,
            GetTagHelpers(codeDocument),
            RazorVueRazorCodeDocumentProvider.GetDocumentNode(codeDocument),
            RazorVueRazorCodeDocumentProvider.GetSourceMappings(csharpDocument));

    private static bool TryCreateLocalDocument(
        Compilation compilation,
        RazorVueRazorSourceGeneratorDocumentInput input,
        out RazorVueRazorSourceGeneratorDocument? document,
        out string? failure)
    {
        document = null;
        failure = null;

        var source = GetPropertyValue(input.CodeDocument, "Source");
        if (source is null)
        {
            failure = "RazorCodeDocument.Source was not available from the official Razor SG output.";
            return false;
        }

        var documentPath = ReadSourcePath(source) ?? input.HintName;
        var documentText = ReadSourceText(source);
        if (documentText is null)
        {
            failure = "RazorCodeDocument.Source.Text was not readable from the official Razor SG output.";
            return false;
        }

        var parseOptions = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
                           ?? CSharpParseOptions.Default;
        var projectEngine = RazorVueRazorCodeDocumentProvider.CreateProjectEngine(
            documentPath,
            parseOptions,
            TryReadGeneratedNamespace(input.CSharpDocument));
        var primaryDocument = new Jazor.RazorVue.RazorVueRazorDocument(
            documentPath,
            SourceText.From(documentText));
        var importDocuments = ReadImports(input.CodeDocument)
            .Select(static item => RazorVueRazorCodeDocumentProvider.CreateSourceDocument(item))
            .ToImmutableArray();
        var tagHelpers = RazorVueRazorCodeDocumentProvider.DiscoverTagHelpers(projectEngine, compilation);

        var codeDocument = projectEngine.Process(
            RazorVueRazorCodeDocumentProvider.CreateSourceDocument(primaryDocument),
            RazorFileKind.Component,
            importDocuments,
            tagHelpers.Length == 0 ? null : TagHelperCollection.Create(tagHelpers));
        var csharpDocument = RazorVueRazorCodeDocumentProvider.GetRequiredCSharpDocument(codeDocument);
        document = CreateDocument(input.HintName, codeDocument, csharpDocument);
        return true;
    }

    private static ImmutableArray<TagHelperDescriptor> GetTagHelpers(RazorCodeDocument codeDocument)
    {
        var method = typeof(RazorCodeDocument).GetMethod(
            "GetRequiredTagHelpers",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (method?.Invoke(codeDocument, null) is not IEnumerable tagHelpers)
            return ImmutableArray<TagHelperDescriptor>.Empty;

        return tagHelpers.OfType<TagHelperDescriptor>().ToImmutableArray();
    }

    private static ImmutableArray<Jazor.RazorVue.RazorVueRazorDocument> ReadImports(object codeDocument)
    {
        if (GetPropertyValue(codeDocument, "Imports") is not IEnumerable imports)
            return ImmutableArray<Jazor.RazorVue.RazorVueRazorDocument>.Empty;

        var builder = ImmutableArray.CreateBuilder<Jazor.RazorVue.RazorVueRazorDocument>();
        foreach (var import in imports)
        {
            if (import is null)
                continue;

            var importPath = ReadSourcePath(import);
            var importText = ReadSourceText(import);
            if (string.IsNullOrWhiteSpace(importPath) || importText is null)
                continue;

            builder.Add(new Jazor.RazorVue.RazorVueRazorDocument(importPath!, SourceText.From(importText)));
        }

        return builder.ToImmutable();
    }

    private static string? TryReadGeneratedNamespace(object? csharpDocument)
    {
        var text = ReadCSharpText(csharpDocument);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var root = CSharpSyntaxTree.ParseText(text!).GetRoot();
        return root.DescendantNodes()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault()
            ?.Name
            .ToString();
    }

    private static string? ReadCSharpText(object? csharpDocument)
    {
        if (csharpDocument is null)
            return null;

        var textValue = GetPropertyValue(csharpDocument, "Text");
        return textValue?.ToString();
    }

    private static string? ReadSourcePath(object sourceDocument)
        => GetPropertyValue(sourceDocument, "FilePath") as string
           ?? GetPropertyValue(sourceDocument, "RelativePath") as string;

    private static string? ReadSourceText(object sourceDocument)
    {
        var textValue = GetPropertyValue(sourceDocument, "Text");
        if (textValue is not null)
            return textValue.ToString() ?? string.Empty;

        var lengthValue = GetPropertyValue(sourceDocument, "Length");
        if (lengthValue is not int length || length <= 0)
            return string.Empty;

        var copyTo = sourceDocument.GetType().GetMethod(
            "CopyTo",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic,
            binder: null,
            types: [typeof(int), typeof(char[]), typeof(int), typeof(int)],
            modifiers: null);
        if (copyTo is null)
            return null;

        var buffer = new char[length];
        copyTo.Invoke(sourceDocument, [0, buffer, 0, length]);
        return new string(buffer);
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
