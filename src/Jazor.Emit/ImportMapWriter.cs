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
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputRoot);
        ArgumentNullException.ThrowIfNull(materialization);

        var browserImports = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["style.mjs"] = "/jazor/style.mjs",
            ["@jazor/vue-runtime/"] = "/jazor/@jazor/vue-runtime/",
            ["components/"] = "/jazor/components/",
            ["System/"] = "/jazor/System/"
        };
        var ssrImports = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["style.mjs"] = "./style.mjs",
            ["@jazor/vue-runtime/"] = "./@jazor/vue-runtime/",
            ["components/"] = "./components/",
            ["System/"] = "./System/"
        };
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
}
