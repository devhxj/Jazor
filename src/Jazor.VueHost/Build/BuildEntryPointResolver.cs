using System.Text.RegularExpressions;

namespace Jazor.VueHost.Build;

internal static class BuildEntryPointResolver
{
    private static readonly Regex ScriptTagPattern = new(
        """<script\b(?<attrs>[^>]*)\bsrc\s*=\s*["'](?<src>[^"']+)["'][^>]*>""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly string[] CandidateEntryPoints =
    [
        Path.Combine("src", "main.ts"),
        Path.Combine("src", "main.js"),
        Path.Combine("src", "main.mjs"),
        Path.Combine("src", "main.tsx"),
        Path.Combine("src", "main.jsx"),
        "main.ts",
        "main.js",
        "main.mjs",
        "main.tsx",
        "main.jsx"
    ];

    public static string ResolveEntryPoint(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        rootDirectory = Path.GetFullPath(rootDirectory);

        if (TryResolveFromIndexHtml(rootDirectory, out var indexHtmlEntryPoint))
        {
            return indexHtmlEntryPoint;
        }

        foreach (var candidate in CandidateEntryPoints)
        {
            var absoluteCandidate = Path.Combine(rootDirectory, candidate);
            if (File.Exists(absoluteCandidate))
            {
                return absoluteCandidate;
            }
        }

        throw new InvalidOperationException(
            $"Unable to locate a frontend entry point under '{rootDirectory}'. " +
            "Add a module script to index.html or create one of the standard entry files such as src/main.ts or src/main.js.");
    }

    private static bool TryResolveFromIndexHtml(string rootDirectory, out string entryPoint)
    {
        var indexHtmlPath = Path.Combine(rootDirectory, "index.html");
        if (!File.Exists(indexHtmlPath))
        {
            entryPoint = string.Empty;
            return false;
        }

        var html = File.ReadAllText(indexHtmlPath);
        foreach (Match match in ScriptTagPattern.Matches(html))
        {
            var src = match.Groups["src"].Value;
            if (string.IsNullOrWhiteSpace(src) || IsExternalSource(src))
            {
                continue;
            }

            var normalizedPath = src.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            var absolutePath = Path.GetFullPath(Path.Combine(rootDirectory, normalizedPath));
            if (!absolutePath.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (File.Exists(absolutePath))
            {
                entryPoint = absolutePath;
                return true;
            }
        }

        entryPoint = string.Empty;
        return false;
    }

    private static bool IsExternalSource(string src)
        => src.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || src.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || src.StartsWith("//", StringComparison.Ordinal)
            || src.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
}
