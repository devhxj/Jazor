using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;

namespace Jazor.RazorVue.Test;

internal static class RazorVueRazorCodeDocumentProvider
{
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
                    ? "Jazor.RazorVue.Test"
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
        var engineProperty = typeof(RazorProjectEngine).GetProperty(
            "Engine",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var engine = engineProperty?.GetValue(projectEngine)
                     ?? throw new InvalidOperationException("RazorProjectEngine.Engine was not available.");
        var featuresProperty = engine.GetType().GetProperty(
            "Features",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (featuresProperty?.GetValue(engine) is not IEnumerable features)
            throw new InvalidOperationException("Razor engine Features collection was not available.");

        var discoveryService = features
            .Cast<object>()
            .FirstOrDefault(static feature => string.Equals(
                feature.GetType().FullName,
                "Microsoft.AspNetCore.Razor.Language.TagHelperDiscoveryService",
                StringComparison.Ordinal))
            ?? throw new InvalidOperationException("TagHelperDiscoveryService was not exposed by the Razor project engine.");
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

        if (string.IsNullOrWhiteSpace(properties.FilePath))
            throw new InvalidOperationException("RazorSourceDocumentProperties.Create(filePath, relativePath) returned an empty file path.");

        return RazorSourceDocument.Create(document.Text.ToString(), properties);
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
}
