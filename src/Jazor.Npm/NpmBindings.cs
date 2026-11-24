using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.Npm;

internal static class NpmBindings
{
    internal static string GenerateCachedSource(string projectDirectory, IReadOnlyList<string> namespaces,
        IReadOnlyList<NpmValueExport> exports, CancellationToken cancellationToken = default) =>
        NpmProjectionCache.LoadBindings(Path.Combine(Path.GetFullPath(projectDirectory), "obj", "Jazor", "projections"),
            namespaces, exports, cancellationToken);

    internal static string GenerateSource(IReadOnlyList<string> namespaces, IReadOnlyList<NpmValueExport> exports,
        CancellationToken cancellationToken = default) =>
        CreateTree(namespaces, exports).GetText(cancellationToken).ToString();

    internal static SyntaxTree CreateTree(IReadOnlyList<string> namespaces, IReadOnlyList<NpmValueExport> exports) =>
        new NpmValueProjection(namespaces, exports, new CSharpParseOptions(LanguageVersion.Preview), standalone: true).Tree;

    internal static string GenerateEditorSource(string directory, IReadOnlyList<string> namespaces,
        IReadOnlyList<NpmValueExport> exports, CancellationToken cancellationToken)
    {
        var declarations = new Dictionary<string, SourceText?>(StringComparer.Ordinal);
        return new NpmValueProjection(namespaces, exports, new CSharpParseOptions(LanguageVersion.Preview),
            standalone: true, declarationLink: DeclarationLink).Tree.GetText(cancellationToken).ToString();

        string? DeclarationLink(NpmDefinition definition)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.GetFullPath(definition.Path, directory);
            if (!declarations.TryGetValue(path, out var text))
            {
                try
                {
                    using var stream = File.OpenRead(path);
                    text = SourceText.From(stream);
                }
                catch (IOException)
                {
                    text = null;
                }
                declarations.Add(path, text);
            }
            if (text is null || definition.Start < 0 || definition.Start >= text.Length)
                return null;

            var position = text.Lines.GetLinePosition(definition.Start);
            var filePath = path.Replace(Path.DirectorySeparatorChar, '/');
            var segments = filePath.Split('/');
            var escaped = segments.Select((segment, index) =>
                index == 0 && segment.Length == 2 && segment[1] == ':' ? segment : Uri.EscapeDataString(segment));
            var uri = filePath.StartsWith("//", StringComparison.Ordinal)
                ? "file:" + string.Join("/", escaped)
                : "file:///" + string.Join("/", escaped).TrimStart('/');
            var fragment = FormattableString.Invariant($"L{position.Line + 1},{position.Character + 1}");
            return System.Security.SecurityElement.Escape(uri + "#" + fragment);
        }
    }
}
