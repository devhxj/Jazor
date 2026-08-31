using System.Text.Encodings.Web;
using System.Text.Json;

namespace Jazor.Emit;

/// <summary>Writes browser and SSR import maps for one validated materialization.</summary>
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

    public static Task WriteAsync(
        string outputRoot,
        LibraryAssets materialization,
        IReadOnlyList<ModuleEntry>? generatedModules = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputRoot);
        ArgumentNullException.ThrowIfNull(materialization);
        cancellationToken.ThrowIfCancellationRequested();

        var browserImports = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["style.mjs"] = "/jazor/style.mjs",
            ["components/"] = "/jazor/components/"
        };
        var ssrImports = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["style.mjs"] = "./style.mjs",
            ["components/"] = "./components/"
        };

        foreach (var pair in materialization.ImportPaths.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = pair.Value.Replace('\\', '/').TrimStart('/');
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException($"Import map target for '{pair.Key}' is empty.");
            AddImport(browserImports, pair.Key, "/jazor/" + path);
            AddImport(ssrImports, pair.Key, "./" + path);
        }

        // Generated modules are the pure-Jazor carrier's logical imports. They are not package
        // manifest entries, but resource-library modules may legitimately import them (for
        // example the Vue runtime helper importing the generated route registry), so expose the
        // same exact identities in both profile maps.
        foreach (var module in (generatedModules ?? [])
                     .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(static module => module.Id, StringComparer.Ordinal))
        {
            var path = module.RelativePath.Replace('\\', '/').TrimStart('/');
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException($"Generated module import-map target for '{module.Id}' is empty.");
            AddImport(browserImports, module.RelativePath, "/jazor/" + path);
            AddImport(ssrImports, module.RelativePath, "./" + path);
        }

        var payloads = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [BrowserImportMapFileName] = JsonSerializer.Serialize(new { imports = browserImports }, JsonOptions),
            [SsrImportMapFileName] = JsonSerializer.Serialize(new { imports = ssrImports }, JsonOptions),
            [AssetManifestFileName] = JsonSerializer.Serialize(
                new
                {
                    styles = materialization.StylePaths
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(static path => path, StringComparer.Ordinal)
                        .Select(static path => "/jazor/" + path.Replace('\\', '/').TrimStart('/'))
                        .ToArray()
                },
                JsonOptions)
        };

        return CommitAsync(Path.GetFullPath(outputRoot), payloads, cancellationToken);
    }

    private static void AddImport(IDictionary<string, string> imports, string specifier, string target)
    {
        if (imports.TryGetValue(specifier, out var existing))
        {
            if (!StringComparer.Ordinal.Equals(existing, target))
                throw new InvalidOperationException($"Import map specifier '{specifier}' has conflicting targets '{existing}' and '{target}'.");
            return;
        }

        imports.Add(specifier, target);
    }

    private static async Task CommitAsync(
        string outputRoot,
        IReadOnlyDictionary<string, string> payloads,
        CancellationToken cancellationToken)
    {
        var parent = Directory.GetParent(outputRoot)?.FullName
            ?? throw new InvalidOperationException($"Could not determine parent directory for '{outputRoot}'.");
        Directory.CreateDirectory(outputRoot);
        var staging = Path.Combine(parent, ".jazor-importmap-" + Guid.NewGuid().ToString("N"));
        var backups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var committed = new List<string>();

        try
        {
            Directory.CreateDirectory(staging);
            foreach (var payload in payloads.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await File.WriteAllTextAsync(
                    Path.Combine(staging, payload.Key),
                    payload.Value,
                    cancellationToken).ConfigureAwait(false);
            }

            foreach (var name in payloads.Keys.OrderBy(static name => name, StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var target = Path.Combine(outputRoot, name);
                if (File.Exists(target))
                {
                    var backup = Path.Combine(parent, ".jazor-importmap-backup-" + Guid.NewGuid().ToString("N"));
                    File.Move(target, backup);
                    backups[target] = backup;
                }

                File.Move(Path.Combine(staging, name), target);
                committed.Add(target);
            }

            foreach (var backup in backups.Values)
                DeleteFile(backup);
        }
        catch
        {
            foreach (var target in committed)
                DeleteFile(target);
            foreach (var backup in backups)
            {
                if (File.Exists(backup.Value) && !File.Exists(backup.Key))
                    File.Move(backup.Value, backup.Key);
            }
            throw;
        }
        finally
        {
            if (Directory.Exists(staging))
                Directory.Delete(staging, recursive: true);
            foreach (var backup in backups.Values)
                DeleteFile(backup);
        }
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
