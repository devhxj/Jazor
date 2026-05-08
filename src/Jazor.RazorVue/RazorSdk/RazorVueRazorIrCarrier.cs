using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.RazorSdk;

internal sealed record RazorVueRazorIrCarrier(
    string DocumentPath,
    ImmutableArray<string> ImportDocumentPaths,
    string DocumentText,
    ImmutableArray<RazorVueRazorIrCarrierImport> Imports)
{
    private const string MetadataTypeName = "Jazor.RazorVue.Runtime.RazorVueRazorIrCarrierAttribute";

    public static bool TryResolve(
        INamedTypeSymbol componentSymbol,
        out RazorVueRazorIrCarrier carrier)
    {
        if (componentSymbol is null)
            throw new ArgumentNullException(nameof(componentSymbol));

        foreach (var attribute in componentSymbol.GetAttributes())
        {
            if (!string.Equals(
                    attribute.AttributeClass?.ToDisplayString(),
                    MetadataTypeName,
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (TryCreate(attribute, out carrier))
                return true;
        }

        carrier = default!;
        return false;
    }

    public static ImmutableArray<string> ResolveImportTextsByPath(
        RazorVueRazorIrCarrier carrier,
        ImmutableArray<string> importPaths)
    {
        if (importPaths.IsDefaultOrEmpty || carrier.Imports.IsDefaultOrEmpty)
            return ImmutableArray<string>.Empty;

        var builder = ImmutableArray.CreateBuilder<string>(importPaths.Length);
        foreach (var path in importPaths)
        {
            var match = carrier.Imports.FirstOrDefault(item =>
                string.Equals(item.Path, path, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
                builder.Add(match.Text);
        }

        return builder.ToImmutable();
    }

    internal static AttributeSyntax? FindCarrierAttributeSyntax(SyntaxTree syntaxTree)
    {
        if (syntaxTree is null)
            throw new ArgumentNullException(nameof(syntaxTree));

        var root = syntaxTree.GetRoot();
        foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
        {
            var name = attribute.Name.ToString();
            if (name.EndsWith("RazorVueRazorIrCarrier", StringComparison.Ordinal) ||
                name.EndsWith("RazorVueRazorIrCarrierAttribute", StringComparison.Ordinal))
            {
                return attribute;
            }
        }

        return null;
    }

    private static bool TryCreate(
        AttributeData attribute,
        out RazorVueRazorIrCarrier carrier)
    {
        carrier = default!;
        if (attribute.ConstructorArguments.Length < 3)
            return false;

        var documentPath = attribute.ConstructorArguments[0].Value as string;
        var importsJson = attribute.ConstructorArguments[1].Value as string;
        var documentText = attribute.ConstructorArguments[2].Value as string;

        if (string.IsNullOrWhiteSpace(documentPath) || documentText is null)
            return false;

        var resolvedDocumentPath = documentPath!;
        var imports = ParseImports(importsJson);
        carrier = new RazorVueRazorIrCarrier(
            resolvedDocumentPath,
            imports.Select(static item => item.Path).ToImmutableArray(),
            documentText,
            imports);
        return true;
    }

    private static ImmutableArray<RazorVueRazorIrCarrierImport> ParseImports(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return ImmutableArray<RazorVueRazorIrCarrierImport>.Empty;

        try
        {
            var parsed = JsonSerializer.Deserialize<RazorVueRazorIrCarrierImportDto[]>(json!);
            if (parsed is null || parsed.Length == 0)
                return ImmutableArray<RazorVueRazorIrCarrierImport>.Empty;

            return parsed
                .Where(static item => !string.IsNullOrWhiteSpace(item.Path))
                .Select(static item => new RazorVueRazorIrCarrierImport(
                    item.Path!,
                    item.Text ?? string.Empty))
                .ToImmutableArray();
        }
        catch (JsonException)
        {
            return ImmutableArray<RazorVueRazorIrCarrierImport>.Empty;
        }
    }

    private sealed class RazorVueRazorIrCarrierImportDto
    {
        public string? Path { get; set; }

        public string? Text { get; set; }
    }
}

internal sealed record RazorVueRazorIrCarrierImport(string Path, string Text);
