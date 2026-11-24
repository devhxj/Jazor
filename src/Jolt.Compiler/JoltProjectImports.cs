using Jazor.Npm;
using System.Runtime.CompilerServices;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

public sealed record JoltImport(JoltDocument Document, JoltUsing Directive)
{
    public string Code => Directive.Code.Trim().TrimEnd(';').TrimEnd();
    private UsingDirectiveSyntax? UsingSyntax { get; } = SyntaxFactory.ParseCompilationUnit(
        "using " + Directive.Code + "\n;").Usings.FirstOrDefault();
    public string? Alias => UsingSyntax?.Alias?.Name.Identifier.ValueText;
    private string? NpmTarget => GetNpmTarget(UsingSyntax);
    public bool IsNpmValue => Alias != null && NpmTarget?.EndsWith(".Exports", StringComparison.Ordinal) == true;
    public string? NpmNamespace => IsNpmValue ? NpmTarget![..^".Exports".Length] : Alias == null ? NpmTarget : null;
    public bool IsNpm => NpmNamespace != null;

    private static string? GetNpmTarget(UsingDirectiveSyntax? directive)
    {
        if (directive?.Name is not QualifiedNameSyntax name || name.ContainsDiagnostics || directive.StaticKeyword.RawKind != 0)
            return null;

        var target = string.Concat(name.DescendantTokens().Select(token => token.ValueText));
        if (target.StartsWith("global::", StringComparison.Ordinal))
            target = target["global::".Length..];
        return target.StartsWith("Npm.", StringComparison.Ordinal) ? target : null;
    }

    public Location Location(TextSpan span)
        => Microsoft.CodeAnalysis.Location.Create(Document.Path, span, SourceText.From(Document.Source).Lines.GetLinePositionSpan(span));
}

public sealed class JoltProjectImports
{
    // Each source instance owns its parse. Record copies and new overlays have independent
    // entries, and closing a snapshot lets its source and parse be collected together.
    private static readonly ConditionalWeakTable<JoltSource, Lazy<JoltDocument>> ParsedSources = new();
    private static readonly ConditionalWeakTable<JoltDocument, JoltImport[]> ParsedImports = new();
    private readonly Dictionary<string, JoltDocument> _imports;
    public IReadOnlyList<JoltDocument> Documents { get; }

    private JoltProjectImports(JoltDocument[] documents)
    {
        Documents = documents;
        _imports = documents.Where(document => IsImportsFile(document.Path))
            .ToDictionary(document => Normalize(document.Path), StringComparer.Ordinal);
    }

    public static JoltProjectImports Parse(IEnumerable<JoltSource> sources, CancellationToken cancellationToken = default)
        => new(sources.OrderBy(source => source.Path, StringComparer.Ordinal).Select(source =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ParseDocument(source);
        }).ToArray());

    internal static JoltDocument ParseDocument(JoltSource source) =>
        ParsedSources.GetValue(source, static input =>
            new Lazy<JoltDocument>(() => JoltParser.Parse(input.Content, input.Path))).Value;

    public static bool IsImportsFile(string path)
        => Path.GetFileName(Normalize(path)) == "_Imports.jazor";

    public IEnumerable<JoltImport> ImportsFor(JoltDocument document)
    {
        if (!IsImportsFile(document.Path))
        {
            var path = Normalize(document.Path);
            if (_imports.TryGetValue("_Imports.jazor", out var root))
                foreach (var import in OwnImports(root))
                    yield return import;

            for (var slash = path.IndexOf('/'); slash >= 0; slash = path.IndexOf('/', slash + 1))
                if (_imports.TryGetValue(path[..(slash + 1)] + "_Imports.jazor", out var ancestor))
                    foreach (var import in OwnImports(ancestor))
                        yield return import;
        }
        foreach (var import in OwnImports(document))
            yield return import;
    }

    private static IEnumerable<JoltImport> OwnImports(JoltDocument document)
        => ParsedImports.GetValue(document, static input =>
            input.Nodes.OfType<JoltUsing>().Select(use => new JoltImport(input, use)).ToArray());

    private static string Normalize(string path) => path.Replace('\\', '/');
}
