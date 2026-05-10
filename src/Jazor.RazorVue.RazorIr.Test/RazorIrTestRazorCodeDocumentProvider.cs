using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;

namespace Jazor.RazorVue.RazorIr.Test;

internal sealed class RazorVueRazorCodeDocumentProvider
{
    public bool TryCreate(
        Jazor.RazorVue.RazorVueCompilationContext context,
        Jazor.RazorVue.Artifacts.RazorVueSemanticSnapshot snapshot,
        out RazorVueRazorCodeDocumentHandle handle)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));

        handle = default!;
        if (snapshot.RazorIrCarrier is not { } carrier)
            return false;

        var document = new Jazor.RazorVue.RazorVueRazorDocument(
            carrier.DocumentPath,
            Microsoft.CodeAnalysis.Text.SourceText.From(carrier.DocumentText));
        var importDocuments = carrier.Imports
            .Select(static item => new Jazor.RazorVue.RazorVueRazorDocument(
                item.Path,
                Microsoft.CodeAnalysis.Text.SourceText.From(item.Text)))
            .ToImmutableArray();
        var projectEngine = CreateProjectEngine(
            document.Path,
            GetParseOptions(context.Compilation),
            rootNamespace: snapshot.ComponentSymbol.ContainingNamespace?.ToDisplayString());
        var tagHelpers = DiscoverTagHelpers(projectEngine, context.Compilation);
        var codeDocument = projectEngine.Process(
            CreateSourceDocument(document),
            RazorFileKind.Component,
            importDocuments.Select(CreateSourceDocument).ToImmutableArray(),
            tagHelpers.Length == 0 ? null : TagHelperCollection.Create(tagHelpers));
        var csharpDocument = GetRequiredCSharpDocument(codeDocument);

        handle = new RazorVueRazorCodeDocumentHandle(
            document,
            importDocuments,
            tagHelpers,
            codeDocument,
            csharpDocument,
            GetDocumentNode(codeDocument));
        return true;
    }

    internal static RazorProjectEngine CreateProjectEngine(
        string documentPath,
        CSharpParseOptions parseOptions,
        string? rootNamespace)
    {
        var rootPath = Path.GetDirectoryName(documentPath);
        if (string.IsNullOrWhiteSpace(rootPath))
            rootPath = Directory.GetCurrentDirectory();

        return RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Create(rootPath),
            builder =>
            {
                builder.SetRootNamespace(string.IsNullOrWhiteSpace(rootNamespace)
                    ? "Jazor.RazorVue.RazorIr.Test"
                    : rootNamespace);
                builder.SetSupportLocalizedComponentNames();
                builder.ConfigureCodeGenerationOptions(codegen =>
                {
                    codegen.SuppressChecksum = true;
                    codegen.SupportLocalizedComponentNames = true;
                });
                builder.ConfigureParserOptions(parser =>
                {
                    parser.CSharpParseOptions = parseOptions;
                    parser.UseRoslynTokenizer = true;
                });
                CompilerFeatures.Register(builder);
                Microsoft.AspNetCore.Mvc.Razor.Extensions.RazorExtensions.Register(builder);
                builder.SetCSharpLanguageVersion(parseOptions.LanguageVersion);
            });
    }

    internal static ImmutableArray<TagHelperDescriptor> DiscoverTagHelpers(
        RazorProjectEngine projectEngine,
        Compilation compilation)
    {
        var discoveryService = GetTagHelperDiscoveryService(projectEngine);
        var method = discoveryService.GetType().GetMethod(
            "GetTagHelpers",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(Compilation), typeof(CancellationToken)],
            modifiers: null);
        if (method is null)
            throw new InvalidOperationException("TagHelperDiscoveryService.GetTagHelpers(Compilation, CancellationToken) was not found.");

        if (method.Invoke(discoveryService, [compilation, CancellationToken.None]) is not IEnumerable discovered)
            return ImmutableArray<TagHelperDescriptor>.Empty;

        return discovered.Cast<object>().OfType<TagHelperDescriptor>().ToImmutableArray();
    }

    internal static RazorSourceDocument CreateSourceDocument(Jazor.RazorVue.RazorVueRazorDocument document)
    {
        var propertiesFactory = typeof(RazorSourceDocumentProperties).GetMethod(
            "Create",
            BindingFlags.Static | BindingFlags.NonPublic);
        if (propertiesFactory?.Invoke(null, [document.Path, Path.GetFileName(document.Path)]) is not RazorSourceDocumentProperties properties)
            throw new InvalidOperationException("RazorSourceDocumentProperties.Create(filePath, relativePath) was not available.");

        return RazorSourceDocument.Create(document.Text.ToString(), properties.FilePath);
    }

    internal static RazorCSharpDocument GetRequiredCSharpDocument(RazorCodeDocument codeDocument)
    {
        var requiredMethod = typeof(RazorCodeDocument).GetMethod(
            "GetRequiredCSharpDocument",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (requiredMethod?.Invoke(codeDocument, null) is RazorCSharpDocument requiredDocument)
            return requiredDocument;

        var optionalMethod = typeof(RazorCodeDocument).GetMethod(
            "GetCSharpDocument",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (optionalMethod?.Invoke(codeDocument, null) is RazorCSharpDocument optionalDocument)
            return optionalDocument;

        throw new InvalidOperationException("RazorCodeDocument.GetRequiredCSharpDocument()/GetCSharpDocument() did not return RazorCSharpDocument.");
    }

    internal static object GetDocumentNode(RazorCodeDocument codeDocument)
        => RazorIrTestHost.GetDocumentNode(codeDocument);

    private static CSharpParseOptions GetParseOptions(Compilation compilation)
        => compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
           ?? CSharpParseOptions.Default;

    private static object GetTagHelperDiscoveryService(RazorProjectEngine projectEngine)
        => RazorIrTestHost.GetTagHelperDiscoveryService(projectEngine);
}

internal sealed record RazorVueRazorCodeDocumentHandle(
    Jazor.RazorVue.RazorVueRazorDocument PrimaryDocument,
    ImmutableArray<Jazor.RazorVue.RazorVueRazorDocument> ImportDocuments,
    ImmutableArray<TagHelperDescriptor> TagHelpers,
    RazorCodeDocument CodeDocument,
    RazorCSharpDocument CSharpDocument,
    object DocumentNode);
