using System.Collections;
using System.Collections.Immutable;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;

namespace Jazor.RazorVue.RazorSdk;

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
        {
            return false;
        }

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
            importDocuments.Select(CreateImportDocument).ToImmutableArray(),
            tagHelpers.Length == 0 ? null : TagHelperCollection.Create(tagHelpers));
        var csharpDocument = GetRequiredCSharpDocument(codeDocument);
        var sourceMappings = GetSourceMappings(csharpDocument);
        var documentNode = GetDocumentNode(codeDocument);

        handle = new RazorVueRazorCodeDocumentHandle(
            document,
            importDocuments,
            tagHelpers,
            codeDocument,
            csharpDocument,
            sourceMappings,
            documentNode);
        return true;
    }

    internal static RazorProjectEngine CreateProjectEngine(
        string documentPath,
        CSharpParseOptions parseOptions,
        string? rootNamespace)
        => CreateProjectEngineCore(
            documentPath,
            parseOptions,
            rootNamespace,
            suppressPrimaryMethodBody: false);

    internal static RazorProjectEngine CreateDeclarationProjectEngine(
        string documentPath,
        CSharpParseOptions parseOptions,
        string? rootNamespace)
        => CreateProjectEngineCore(
            documentPath,
            parseOptions,
            rootNamespace,
            suppressPrimaryMethodBody: true);

    private static RazorProjectEngine CreateProjectEngineCore(
        string documentPath,
        CSharpParseOptions parseOptions,
        string? rootNamespace,
        bool suppressPrimaryMethodBody)
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
                    ? "Jazor.RazorVue.RazorSdk"
                    : rootNamespace);
                builder.SetSupportLocalizedComponentNames();

                builder.ConfigureCodeGenerationOptions(codegen =>
                {
                    codegen.SuppressChecksum = true;
                    codegen.SuppressPrimaryMethodBody = suppressPrimaryMethodBody;
                    codegen.SupportLocalizedComponentNames = true;
                });

                builder.ConfigureParserOptions(parser =>
                {
                    parser.CSharpParseOptions = parseOptions;
                    parser.UseRoslynTokenizer = true;
                });

                CompilerFeatures.Register(builder);
                RazorExtensions.Register(builder);
                builder.SetCSharpLanguageVersion(parseOptions.LanguageVersion);
            });
    }

    internal static ImmutableArray<TagHelperDescriptor> DiscoverTagHelpers(RazorProjectEngine projectEngine, Compilation compilation)
    {
        if (projectEngine is null)
            throw new ArgumentNullException(nameof(projectEngine));
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        var discoveryService = GetTagHelperDiscoveryService(projectEngine);
        var method = discoveryService.GetType().GetMethod(
            "GetTagHelpers",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(Compilation), typeof(CancellationToken)],
            modifiers: null);
        if (method is null)
            throw new InvalidOperationException("TagHelperDiscoveryService.GetTagHelpers(Compilation, CancellationToken) was not found.");

        var result = method.Invoke(discoveryService, [compilation, CancellationToken.None]);
        if (result is not IEnumerable discovered)
            return ImmutableArray<TagHelperDescriptor>.Empty;

        return discovered.Cast<object>()
            .OfType<TagHelperDescriptor>()
            .ToImmutableArray();
    }

    internal static DocumentIntermediateNode GetDocumentNode(RazorCodeDocument codeDocument)
    {
        if (codeDocument is null)
            throw new ArgumentNullException(nameof(codeDocument));

        var method = typeof(RazorCodeDocument).GetMethod(
            "GetDocumentNode",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method is null)
            throw new InvalidOperationException("RazorCodeDocument.GetDocumentNode() was not found.");

        return method.Invoke(codeDocument, null) as DocumentIntermediateNode
            ?? throw new InvalidOperationException("RazorCodeDocument.GetDocumentNode() returned null or did not return DocumentIntermediateNode.");
    }

    internal static RazorCSharpDocument GetRequiredCSharpDocument(RazorCodeDocument codeDocument)
    {
        if (codeDocument is null)
            throw new ArgumentNullException(nameof(codeDocument));

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

    internal static ImmutableArray<SourceMapping> GetSourceMappings(RazorCSharpDocument csharpDocument)
    {
        if (csharpDocument is null)
            throw new ArgumentNullException(nameof(csharpDocument));

        var propertyNames = new[]
        {
            "SourceMappingsSortedByOriginal",
            "SourceMappingsSortedByGenerated",
            "SourceMappings"
        };

        foreach (var propertyName in propertyNames)
        {
            var property = typeof(RazorCSharpDocument).GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property?.GetValue(csharpDocument) is not IEnumerable sourceMappings)
                continue;

            return sourceMappings.Cast<object>()
                .OfType<SourceMapping>()
                .ToImmutableArray();
        }

        return ImmutableArray<SourceMapping>.Empty;
    }

    private static CSharpParseOptions GetParseOptions(Compilation compilation)
        => compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions
           ?? CSharpParseOptions.Default;

    private static RazorSourceDocument CreateImportDocument(Jazor.RazorVue.RazorVueRazorDocument document)
        => CreateSourceDocument(document);

    internal static RazorSourceDocument CreateSourceDocument(Jazor.RazorVue.RazorVueRazorDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var propertiesFactory = typeof(RazorSourceDocumentProperties).GetMethod(
            "Create",
            BindingFlags.Static | BindingFlags.NonPublic);
        if (propertiesFactory?.Invoke(null, [document.Path, Path.GetFileName(document.Path)]) is not RazorSourceDocumentProperties properties)
            throw new InvalidOperationException("RazorSourceDocumentProperties.Create(filePath, relativePath) was not available.");

        return RazorSourceDocument.Create(document.Text, properties);
    }

    private static object GetTagHelperDiscoveryService(RazorProjectEngine projectEngine)
    {
        var engineProperty = typeof(RazorProjectEngine).GetProperty(
            "Engine",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (engineProperty?.GetValue(projectEngine) is null)
            throw new InvalidOperationException("RazorProjectEngine.Engine was not available.");

        var engine = engineProperty.GetValue(projectEngine)!;
        var featuresProperty = engine.GetType().GetProperty(
            "Features",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (featuresProperty?.GetValue(engine) is not IEnumerable features)
            throw new InvalidOperationException("Razor engine Features collection was not available.");

        return features.Cast<object>()
            .FirstOrDefault(static feature => string.Equals(
                feature.GetType().FullName,
                "Microsoft.AspNetCore.Razor.Language.TagHelperDiscoveryService",
                StringComparison.Ordinal))
            ?? throw new InvalidOperationException("TagHelperDiscoveryService was not exposed by the Razor project engine.");
    }
}

internal sealed record RazorVueRazorCodeDocumentHandle(
    Jazor.RazorVue.RazorVueRazorDocument PrimaryDocument,
    ImmutableArray<Jazor.RazorVue.RazorVueRazorDocument> ImportDocuments,
    ImmutableArray<TagHelperDescriptor> TagHelpers,
    RazorCodeDocument CodeDocument,
    RazorCSharpDocument CSharpDocument,
    ImmutableArray<SourceMapping> SourceMappings,
    DocumentIntermediateNode DocumentNode);
