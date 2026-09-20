using System.Text.Json;

namespace Jazor.Emit;

/// <summary>Resolves a restored package export to a file relative to its package root.</summary>
internal static class PackageExportsResolver
{
    public static string? Resolve(string packageRoot, string subpath, bool browser, bool style = false)
    {
        var packageJsonPath = Path.Combine(packageRoot, "package.json");
        if (!File.Exists(packageJsonPath))
            return null;

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(packageJsonPath));
            var conditions = style
                ? browser
                    ? new[] { "style", "browser", "import", "module", "default" }
                    : new[] { "style", "deno", "node", "import", "default" }
                : browser
                    ? new[] { "browser", "import", "module", "default" }
                    : new[] { "deno", "node", "import", "default" };
            var root = document.RootElement;
            if (root.TryGetProperty("exports", out var exports))
            {
                var target = ResolveExportsValue(exports, subpath, conditions, wildcard: null);
                if (target is not null)
                {
                    var resolved = ResolveFile(packageRoot, target, requireDotSlash: true);
                    if (resolved is not null)
                        return resolved;
                }

                // Once a package publishes `exports`, unlisted subpaths are intentionally
                // private. Do not recover by guessing a physical file below node_modules.
                return null;
            }

            if (!string.Equals(subpath, ".", StringComparison.Ordinal))
            {
                var normalizedSubpath = NormalizeRelativePath(subpath, requireDotSlash: true);
                return normalizedSubpath is null
                    ? null
                    : ResolveFile(packageRoot, normalizedSubpath, requireDotSlash: false);
            }

            var fallbackFields = style
                ? browser
                    ? new[] { "style", "browser", "module", "main" }
                    : new[] { "style", "module", "main" }
                : new[] { "browser", "module", "main" };
            foreach (var field in fallbackFields)
            {
                if (root.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    var resolved = ResolveFile(packageRoot, value.GetString()!, requireDotSlash: false, style);
                    if (resolved is not null)
                        return resolved;
                }
            }

            return ResolveFile(packageRoot, style ? "index.css" : "index.js", requireDotSlash: false, style);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ResolveExportsValue(
        JsonElement value,
        string subpath,
        IReadOnlyList<string> conditions,
        string? wildcard)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                return ApplyWildcard(value.GetString()!, wildcard);
            case JsonValueKind.Array:
                foreach (var candidate in value.EnumerateArray())
                {
                    var result = ResolveExportsValue(candidate, subpath, conditions, wildcard);
                    if (result is not null)
                        return result;
                }

                return null;
            case JsonValueKind.Object:
                var properties = value.EnumerateObject().ToArray();
                if (properties.Any(static property => property.Name.StartsWith('.', StringComparison.Ordinal)))
                {
                    foreach (var property in properties.Where(property =>
                                 string.Equals(property.Name, subpath, StringComparison.Ordinal)))
                    {
                        var result = ResolveExportsValue(property.Value, subpath, conditions, wildcard);
                        if (result is not null)
                            return result;
                    }

                    foreach (var property in properties.Where(static property => property.Name.Contains('*', StringComparison.Ordinal)))
                    {
                        var star = property.Name.IndexOf('*');
                        var prefix = property.Name[..star];
                        var suffix = property.Name[(star + 1)..];
                        if (!subpath.StartsWith(prefix, StringComparison.Ordinal) ||
                            !subpath.EndsWith(suffix, StringComparison.Ordinal) ||
                            subpath.Length < prefix.Length + suffix.Length)
                            continue;

                        var valueWildcard = subpath[prefix.Length..^suffix.Length];
                        var result = ResolveExportsValue(property.Value, subpath, conditions, valueWildcard);
                        if (result is not null)
                            return result;
                    }

                    return null;
                }

                foreach (var condition in conditions)
                {
                    if (!value.TryGetProperty(condition, out var candidate))
                        continue;
                    var result = ResolveExportsValue(candidate, subpath, conditions, wildcard);
                    if (result is not null)
                        return result;
                }

                return null;
            default:
                return null;
        }
    }

    private static string ApplyWildcard(string value, string? wildcard)
        => wildcard is null ? value : value.Replace("*", wildcard, StringComparison.Ordinal);

    private static string? ResolveFile(string packageRoot, string target, bool requireDotSlash, bool style = false)
    {
        var normalized = NormalizeRelativePath(target, requireDotSlash);
        if (normalized is null)
            return null;

        var candidate = Path.GetFullPath(Path.Combine(packageRoot, normalized.Replace('/', Path.DirectorySeparatorChar)));
        var root = Path.GetFullPath(packageRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            return null;

        var suffixes = style
            ? new[] { "", ".css", ".scss", ".sass", ".less", ".styl", ".stylus", ".mjs", ".js" }
            : new[] { "", ".js", ".mjs", ".cjs" };
        var indexNames = style
            ? new[] { "index.css", "index.mjs", "index.js" }
            : new[] { "index.js", "index.mjs" };
        foreach (var path in suffixes.Select(suffix => candidate + suffix).Concat(indexNames.Select(name => Path.Combine(candidate, name))))
        {
            if (File.Exists(path))
                return Path.GetRelativePath(packageRoot, path).Replace('\\', '/');
        }

        return null;
    }

    private static string? NormalizeRelativePath(string value, bool requireDotSlash)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Replace('\\', '/');
        if (requireDotSlash && !normalized.StartsWith("./", StringComparison.Ordinal))
            return null;
        if (!requireDotSlash && normalized.StartsWith("/", StringComparison.Ordinal))
            return null;

        if (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized[2..];
        if (string.IsNullOrWhiteSpace(normalized) ||
            normalized.Split('/', StringSplitOptions.None).Any(static segment => segment is "" or "." or ".."))
            return null;
        return normalized;
    }
}
