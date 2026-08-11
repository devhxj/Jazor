using System.Text.Encodings.Web;
using System.Text.Json;

namespace Jazor.Emit;

/// <summary>Writes browser and Deno import maps for one materialized artifact graph.</summary>
internal static class ImportMapWriter
{
    public const string BrowserImportMapFileName = "importmap.json";
    public const string SsrImportMapFileName = "ssr-importmap.json";
    public const string AssetManifestFileName = "manifest.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Default
    };

    public static async Task WriteAsync(
        string outputRoot,
        LibraryAssets materialization,
        IReadOnlyList<ImportMapEntry>? providerEntries = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputRoot);
        ArgumentNullException.ThrowIfNull(materialization);

        var browserImports = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["style.mjs"] = "/jazor/style.mjs",
            ["components/"] = "/jazor/components/",
            ["System/"] = "/jazor/System/"
        };
        var ssrImports = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["style.mjs"] = "./style.mjs",
            ["components/"] = "./components/",
            ["System/"] = "./System/"
        };
        AddProviderEntries(browserImports, ssrImports, providerEntries ?? []);
        foreach (var (specifier, path) in materialization.ImportPaths)
        {
            var normalizedPath = path.Replace('\\', '/');
            browserImports[specifier] = "/jazor/" + normalizedPath;
            ssrImports[specifier] = "./" + normalizedPath;
        }

        var assets = new
        {
            styles = materialization.StylePaths
                .Distinct(StringComparer.Ordinal)
                .Select(static path => "/jazor/" + path.Replace('\\', '/'))
                .ToArray()
        };
        Directory.CreateDirectory(outputRoot);
        await File.WriteAllTextAsync(
            Path.Combine(outputRoot, BrowserImportMapFileName),
            JsonSerializer.Serialize(new { imports = browserImports }, JsonOptions),
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            Path.Combine(outputRoot, SsrImportMapFileName),
            JsonSerializer.Serialize(new { imports = ssrImports }, JsonOptions),
            cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(
            Path.Combine(outputRoot, AssetManifestFileName),
            JsonSerializer.Serialize(assets, JsonOptions),
            cancellationToken).ConfigureAwait(false);
    }

    private static void AddProviderEntries(
        IDictionary<string, string> browserImports,
        IDictionary<string, string> ssrImports,
        IEnumerable<ImportMapEntry> entries)
    {
        foreach (var entry in entries
                     .OrderBy(static entry => entry.Specifier, StringComparer.Ordinal)
                     .ThenBy(static entry => entry.ProviderId, StringComparer.Ordinal)
                     .ThenBy(static entry => entry.ArtifactPath, StringComparer.Ordinal))
        {
            var specifier = entry.Specifier.Trim();
            var artifactPath = entry.ArtifactPath.Replace('\\', '/').TrimStart('/');
            if (string.IsNullOrWhiteSpace(specifier) || string.IsNullOrWhiteSpace(artifactPath))
                throw new InvalidOperationException("Runtime provider import-map entries must have a specifier and artifact path.");

            AddImport(browserImports, specifier, "/jazor/" + artifactPath, entry.ProviderId);
            AddImport(ssrImports, specifier, "./" + artifactPath, entry.ProviderId);
        }
    }

    private static void AddImport(
        IDictionary<string, string> imports,
        string specifier,
        string path,
        string providerId)
    {
        if (imports.TryGetValue(specifier, out var existing))
        {
            if (!StringComparer.Ordinal.Equals(existing, path))
            {
                throw new InvalidOperationException(
                    $"Runtime provider '{providerId}' conflicts with existing import-map entry '{specifier}'.");
            }

            return;
        }

        imports.Add(specifier, path);
    }
}
