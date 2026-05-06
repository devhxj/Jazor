using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue;

internal sealed record RazorVueRazorDocument(string Path, SourceText Text)
{
    public string NormalizedPath { get; } = RazorVueRazorDocumentSet.NormalizePath(Path);
}

internal sealed class RazorVueRazorDocumentSet
{
    private static readonly StringComparer PathComparer = Path.DirectorySeparatorChar == '\\'
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private readonly ImmutableDictionary<string, RazorVueRazorDocument> _documents;

    private RazorVueRazorDocumentSet(ImmutableDictionary<string, RazorVueRazorDocument> documents)
    {
        _documents = documents ?? throw new ArgumentNullException(nameof(documents));
    }

    public static RazorVueRazorDocumentSet Empty { get; } = new(
        ImmutableDictionary<string, RazorVueRazorDocument>.Empty.WithComparers(PathComparer));

    public bool IsEmpty => _documents.IsEmpty;

    public bool TryGetDocument(string? path, out RazorVueRazorDocument document)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            document = null!;
            return false;
        }

        var normalizedPath = NormalizePath(path!);
        return _documents.TryGetValue(normalizedPath, out document!);
    }

    public static RazorVueRazorDocumentSet Create(IEnumerable<RazorVueRazorDocument> documents)
    {
        if (documents is null)
            throw new ArgumentNullException(nameof(documents));

        var builder = ImmutableDictionary.CreateBuilder<string, RazorVueRazorDocument>(PathComparer);
        foreach (var document in documents)
        {
            if (document is null || string.IsNullOrWhiteSpace(document.Path))
                continue;

            builder[NormalizePath(document.Path)] = document;
        }

        return builder.Count == 0
            ? Empty
            : new RazorVueRazorDocumentSet(builder.ToImmutable());
    }

    public static bool IsRazorDocumentPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return path!.EndsWith(".razor", StringComparison.OrdinalIgnoreCase);
    }

    internal static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        try
        {
            if (Path.IsPathRooted(normalized))
                normalized = Path.GetFullPath(normalized);
        }
        catch (Exception)
        {
            // Fall back to the original textual path when Roslyn gives a virtual or non-normalizable path.
        }

        return normalized;
    }
}
