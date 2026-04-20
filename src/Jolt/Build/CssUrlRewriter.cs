using System.Text.RegularExpressions;

namespace Jolt.Build;

internal static class CssUrlRewriter
{
    private static readonly Regex CssUrlPattern = new(
        @"url\(\s*(?:(?<quote>[""'])(?<value>[^""']+)\k<quote>|(?<value>[^)]+?))\s*\)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Uri RootUri = new("https://jazor.build/");

    public static string RewriteAssetReferences(
        string css,
        string cssPublicPath,
        IReadOnlyList<AssetInfo> assets)
        => RewriteAssetReferences(css, cssPublicPath, cssPublicPath, assets);

    public static string RewriteAssetReferences(
        string css,
        string sourceCssPublicPath,
        string outputCssPublicPath,
        IReadOnlyList<AssetInfo> assets)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCssPublicPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputCssPublicPath);
        ArgumentNullException.ThrowIfNull(assets);

        if (assets.Count == 0)
        {
            return css;
        }

        var assetMap = assets
            .Where(static asset => !string.IsNullOrWhiteSpace(asset.OriginalPath)
                && !string.IsNullOrWhiteSpace(asset.FilePath))
            .Select(asset => new KeyValuePair<string, string>(
                NormalizeLookupPath(asset.OriginalPath!, sourceCssPublicPath)!,
                NormalizeOutputPath(asset.FilePath)))
            .ToDictionary(
                static pair => pair.Key,
                static pair => pair.Value,
                StringComparer.OrdinalIgnoreCase);
        if (assetMap.Count == 0)
        {
            return css;
        }

        return CssUrlPattern.Replace(css, match =>
        {
            var originalValue = match.Groups["value"].Value.Trim();
            var normalizedLookupPath = NormalizeLookupPath(originalValue, sourceCssPublicPath);
            if (normalizedLookupPath is null || !assetMap.TryGetValue(normalizedLookupPath, out var rewrittenAssetPath))
            {
                return match.Value;
            }

            var suffix = ExtractQueryAndHashSuffix(originalValue);
            var rewrittenPath = MakeRelativePath(outputCssPublicPath, rewrittenAssetPath) + suffix;
            var quote = match.Groups["quote"].Success
                ? match.Groups["quote"].Value
                : string.Empty;
            return $"url({quote}{rewrittenPath}{quote})";
        });
    }

    public static IReadOnlyList<string> ExtractAssetReferences(
        string css,
        string sourceCssPublicPath)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCssPublicPath);

        var assets = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in CssUrlPattern.Matches(css))
        {
            var originalValue = match.Groups["value"].Value.Trim();
            var normalizedLookupPath = NormalizeLookupPath(originalValue, sourceCssPublicPath);
            if (normalizedLookupPath is null || !seen.Add(normalizedLookupPath))
            {
                continue;
            }

            assets.Add(normalizedLookupPath);
        }

        return assets;
    }

    private static string? NormalizeLookupPath(string value, string cssPublicPath)
    {
        var normalized = StripQueryAndHash(value).Replace('\\', '/').Trim();
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("//", StringComparison.Ordinal)
            || normalized.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)
            || normalized.StartsWith('#')
            || normalized.StartsWith("var(", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (normalized.StartsWith("/", StringComparison.Ordinal))
        {
            return normalized;
        }

        var cssUri = new Uri(RootUri, NormalizeOutputPath(cssPublicPath));
        return new Uri(cssUri, normalized).AbsolutePath;
    }

    private static string NormalizeOutputPath(string value)
    {
        var normalized = value.Replace('\\', '/').Trim();
        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        return normalized.TrimStart('/');
    }

    private static string ExtractQueryAndHashSuffix(string value)
    {
        var index = value.IndexOfAny(['?', '#']);
        return index >= 0 ? value[index..] : string.Empty;
    }

    private static string StripQueryAndHash(string value)
    {
        var index = value.IndexOfAny(['?', '#']);
        return index >= 0 ? value[..index] : value;
    }

    private static string MakeRelativePath(string cssPublicPath, string assetPublicPath)
    {
        var cssDirectory = GetDirectoryPath(cssPublicPath);
        var cssDirectoryUri = new Uri(RootUri, EnsureTrailingSlash(cssDirectory));
        var assetUri = new Uri(RootUri, NormalizeOutputPath(assetPublicPath));
        var relativeUri = cssDirectoryUri.MakeRelativeUri(assetUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());
        return string.IsNullOrEmpty(relativePath)
            ? Path.GetFileName(assetPublicPath)
            : relativePath;
    }

    private static string GetDirectoryPath(string cssPublicPath)
    {
        var normalized = NormalizeOutputPath(cssPublicPath);
        var separatorIndex = normalized.LastIndexOf('/');
        return separatorIndex >= 0
            ? normalized[..separatorIndex]
            : string.Empty;
    }

    private static string EnsureTrailingSlash(string value)
    {
        var normalized = NormalizeOutputPath(value);
        return string.IsNullOrEmpty(normalized)
            ? string.Empty
            : normalized.TrimEnd('/') + "/";
    }
}
