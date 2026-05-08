using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue;

internal sealed record RazorVueRazorDocument(string Path, SourceText Text)
{
    public string NormalizedPath { get; } = NormalizePath(Path);

    public static string NormalizePath(string rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
            return string.Empty;

        var normalized = rawPath.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
        try
        {
            if (System.IO.Path.IsPathRooted(normalized))
                normalized = System.IO.Path.GetFullPath(normalized);
        }
        catch (Exception)
        {
            // Fall back to the original textual path when Roslyn gives a virtual or non-normalizable path.
        }

        return normalized;
    }
}
