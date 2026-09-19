using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

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
            if (IsExternalPackage(materialization, pair.Key))
            {
                // Resolve the restored package using the authored logical export (`pkg/subpath`).
                // The materializer value is an upstream file hint for npm entries and may bypass
                // the package's `exports` map when used as the resolver key.
                var browserTarget = materialization.BrowserImportPaths.TryGetValue(pair.Key, out var explicitTarget)
                    ? explicitTarget
                    : pair.Value;
                AddImport(browserImports, pair.Key, ResolveNodeModulesTarget(outputRoot, materialization, pair.Key, browserTarget, browser: true));
                AddImport(ssrImports, pair.Key, ResolveNodeModulesTarget(outputRoot, materialization, pair.Key, pair.Value, browser: false));
                continue;
            }

            var path = pair.Value.Replace('\\', '/').TrimStart('/');
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException($"Import map target for '{pair.Key}' is empty.");
            AddImport(browserImports, pair.Key, "/jazor/" + path);
            AddImport(ssrImports, pair.Key, "./" + path);
        }

        AddRestoredBrowserPackageImports(outputRoot, browserImports);
        AddRestoredPackageImports(outputRoot, ssrImports, browser: false);

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
                        .Concat(materialization.ExternalStylesheetPaths
                            .Distinct(StringComparer.Ordinal)
                            .OrderBy(static path => path, StringComparer.Ordinal)
                            .Select(path => ResolveNodeModulesTarget(outputRoot, materialization, path, path, browser: true)))
                        .ToArray()
                },
                JsonOptions)
        };

        return CommitAsync(Path.GetFullPath(outputRoot), payloads, cancellationToken);
    }

    /// <summary>
    /// Writes the SSR view for a profile whose package graph lives in the parent jazor root.
    /// Package entries deliberately point at that root's node_modules; SSR must not create a
    /// second package project or restore a second dependency graph.
    /// </summary>
    public static Task WriteSsrAsync(
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
            if (IsExternalPackage(materialization, pair.Key))
            {
                var browserTarget = materialization.BrowserImportPaths.TryGetValue(pair.Key, out var explicitTarget)
                    ? explicitTarget
                    : pair.Value;
                AddImport(browserImports, pair.Key, ResolveNodeModulesTarget(outputRoot, materialization, pair.Key, browserTarget, browser: true));
                AddImport(ssrImports, pair.Key, ResolveNodeModulesTarget(outputRoot, materialization, pair.Key, pair.Value, browser: false));
                continue;
            }

            var path = pair.Value.Replace('\\', '/').TrimStart('/');
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException($"Import map target for '{pair.Key}' is empty.");

            // Generated embedded modules keep the browser/debug URL contract. SSR resolves the
            // same logical entry through the restored package workspace.
            AddImport(browserImports, pair.Key, "/jazor/" + path);
            AddImport(ssrImports, pair.Key, "./" + path);
        }

        AddRestoredBrowserPackageImports(outputRoot, browserImports);
        AddRestoredPackageImports(outputRoot, ssrImports, browser: false);

        foreach (var module in (generatedModules ?? [])
                     .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(static module => module.Id, StringComparer.Ordinal))
        {
            var path = module.RelativePath.Replace('\\', '/').TrimStart('/');
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException($"Generated module import-map target for '{module.Id}' is empty.");
            AddImport(browserImports, module.RelativePath, "/jazor/ssr/" + path);
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
                        .Select(static path => "/jazor/ssr/" + path.Replace('\\', '/').TrimStart('/'))
                        .Concat(materialization.ExternalStylesheetPaths
                            .Distinct(StringComparer.Ordinal)
                            .OrderBy(static path => path, StringComparer.Ordinal)
                            .Select(path => ResolveNodeModulesTarget(outputRoot, materialization, path, path, browser: true)))
                        .ToArray()
                },
                JsonOptions)
        };

        return CommitAsync(Path.GetFullPath(outputRoot), payloads, cancellationToken);
    }

    private static void AddRestoredBrowserPackageImports(
        string outputRoot,
        IDictionary<string, string> browserImports)
        => AddRestoredPackageImports(outputRoot, browserImports, browser: true);

    private static void AddRestoredPackageImports(
        string outputRoot,
        IDictionary<string, string> imports,
        bool browser)
    {
        var packageWorkspace = GetPackageWorkspace(outputRoot);
        var nodeModulesRoot = Path.Combine(packageWorkspace, "node_modules");
        if (!Directory.Exists(nodeModulesRoot))
            return;

        foreach (var packageRoot in EnumerateTopLevelPackageRoots(nodeModulesRoot))
        {
            var packageJsonPath = Path.Combine(packageRoot, "package.json");
            if (!File.Exists(packageJsonPath))
                continue;

            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(packageJsonPath));
                if (!document.RootElement.TryGetProperty("name", out var nameElement) ||
                    nameElement.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(nameElement.GetString()))
                {
                    continue;
                }

                var packageName = nameElement.GetString()!;
                var entry = ResolveRestoredPackageTarget(packageRoot, ".", browser);
                if (entry is not null && !imports.ContainsKey(packageName))
                {
                    var entryPath = Path.Combine(packageRoot, entry.Replace('/', Path.DirectorySeparatorChar));
                    AddImport(imports, packageName, ToPackageUrl(outputRoot, packageWorkspace, entryPath, browser));
                }

                // Exact manifest entries still follow exports. The prefix lets browser-loaded
                // ESM resolve ordinary package file subpaths used by transitive dependencies.
                if (!imports.ContainsKey(packageName + "/"))
                    AddImport(imports, packageName + "/", ToPackageUrl(outputRoot, packageWorkspace, packageRoot, browser) + "/");
            }
            catch (JsonException)
            {
                // Deno restore already validates packages used by the graph. Ignore unrelated
                // malformed package metadata here; a selected entry still fails explicitly.
            }
        }
    }

    private static IEnumerable<string> EnumerateTopLevelPackageRoots(string nodeModulesRoot)
    {
        foreach (var directory in Directory.EnumerateDirectories(nodeModulesRoot)
                     .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(directory);
            if (name.StartsWith('.', StringComparison.Ordinal))
                continue;

            if (!name.StartsWith('@', StringComparison.Ordinal))
            {
                yield return directory;
                continue;
            }

            foreach (var scopedPackage in Directory.EnumerateDirectories(directory)
                         .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
            {
                yield return scopedPackage;
            }
        }
    }

    private static string GetPackageWorkspace(string outputRoot)
        => Path.GetFileName(Path.TrimEndingDirectorySeparator(outputRoot))
            .Equals("ssr", StringComparison.OrdinalIgnoreCase)
                ? Path.GetDirectoryName(Path.GetFullPath(outputRoot)) ?? Path.GetFullPath(outputRoot)
                : Path.GetFullPath(outputRoot);

    private static string ToBrowserPackageUrl(string packageWorkspace, string path)
    {
        var relative = Path.GetRelativePath(packageWorkspace, path).Replace('\\', '/');
        if (relative.StartsWith("../", StringComparison.Ordinal) || string.Equals(relative, "..", StringComparison.Ordinal))
            throw new InvalidOperationException($"Restored browser package path escaped the package workspace: '{path}'.");
        return "/jazor/" + relative.TrimStart('/');
    }

    private static string ToPackageUrl(string outputRoot, string packageWorkspace, string path, bool browser)
    {
        var relative = Path.GetRelativePath(
            browser ? packageWorkspace : Path.GetFullPath(outputRoot),
            path).Replace('\\', '/');
        if (browser && (relative.StartsWith("../", StringComparison.Ordinal) || string.Equals(relative, "..", StringComparison.Ordinal)))
            throw new InvalidOperationException($"Restored package path escaped the package workspace: '{path}'.");
        return browser ? "/jazor/" + relative.TrimStart('/') : (relative.StartsWith(".", StringComparison.Ordinal) ? relative : "./" + relative);
    }

    private static string ResolveNodeModulesTarget(
        string outputRoot,
        LibraryAssets materialization,
        string mappedSpecifier,
        string targetSpecifier,
        bool browser)
    {
        var packageName = GetPackageName(mappedSpecifier);
        // Embedded packages can expose a generated projection, while npm/JSR packages are
        // resolved from their restored package.json. Keep both paths in one resolver so an
        // import map never invents a second package layout.
        string? target = null;
        if (materialization.PackageProjections.TryGetValue(packageName, out var projection))
        {
            var projectionSpecifier = string.Equals(targetSpecifier, packageName, StringComparison.Ordinal)
                ? "."
                : "./" + targetSpecifier[(packageName.Length + 1)..];
            var exportName = projectionSpecifier;
            projection.Exports.TryGetValue(exportName, out target);
            target = target?.TrimStart('.', '/');
        }

        if (target is null)
        {
            // SSR maps are written below the shared package root. Resolve files from the
            // parent jazor/node_modules tree while keeping browser URLs rooted at /jazor.
            var packageWorkspace = GetPackageWorkspace(outputRoot);
            var packageRoot = Path.Combine(packageWorkspace, "node_modules", packageName.Replace('/', Path.DirectorySeparatorChar));
            var subpaths = new List<string>();
            if (targetSpecifier.StartsWith(packageName + "/", StringComparison.Ordinal))
                subpaths.Add("./" + targetSpecifier[(packageName.Length + 1)..]);
            else if (string.Equals(targetSpecifier, packageName, StringComparison.Ordinal))
                subpaths.Add(".");
            var authoredSubpath = string.Equals(mappedSpecifier, packageName, StringComparison.Ordinal)
                ? "."
                : "./" + mappedSpecifier[(packageName.Length + 1)..];
            if (!subpaths.Contains(authoredSubpath, StringComparer.Ordinal))
                subpaths.Add(authoredSubpath);
            foreach (var subpath in subpaths)
            {
                target = ResolveRestoredPackageTarget(packageRoot, subpath, browser);
                if (target is not null)
                    break;
            }
        }

        // A package without a readable export map is still allowed to participate in the
        // standard project. The direct subpath fallback handles packages whose exports are
        // intentionally omitted; unresolved roots remain a hard diagnostic for browser maps.
        if (target is null)
        {
            // Preserve the upstream package specifier as a deterministic URL fallback. This is
            // useful for packages whose export target is generated at install time; the package
            // graph still remains visible to the browser under the restored node_modules tree.
            var unresolved = "node_modules/" + mappedSpecifier.TrimStart('.', '/');
            if (browser)
                return "/jazor/" + unresolved;

            var unresolvedWorkspace = GetPackageWorkspace(outputRoot);
            var unresolvedPath = Path.Combine(unresolvedWorkspace, unresolved.Replace('/', Path.DirectorySeparatorChar));
            return ToPackageUrl(outputRoot, unresolvedWorkspace, unresolvedPath, browser: false);
        }

        var relative = "node_modules/" + packageName + "/" + target.TrimStart('.', '/');
        if (browser)
            return "/jazor/" + relative;

        var resolvedWorkspace = GetPackageWorkspace(outputRoot);
        var resolvedPath = Path.Combine(resolvedWorkspace, relative.Replace('/', Path.DirectorySeparatorChar));
        return ToPackageUrl(outputRoot, resolvedWorkspace, resolvedPath, browser: false);
    }

    private static string? ResolveRestoredPackageTarget(string packageRoot, string subpath, bool browser)
    {
        var packageJsonPath = Path.Combine(packageRoot, "package.json");
        if (!File.Exists(packageJsonPath))
            return null;

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(packageJsonPath));
            var root = document.RootElement;
            var conditions = browser
                ? new[] { "browser", "import", "module", "default" }
                : new[] { "deno", "node", "import", "default" };

            if (root.TryGetProperty("exports", out var exports))
            {
                var target = ResolveExportsValue(exports, subpath, conditions, wildcard: null);
                if (target is not null)
                    return ResolvePackageFile(packageRoot, target);
            }

            if (!string.Equals(subpath, ".", StringComparison.Ordinal))
            {
                var direct = subpath.TrimStart('.', '/');
                return ResolvePackageFile(packageRoot, direct);
            }

            foreach (var field in new[] { "browser", "module", "main" })
            {
                if (root.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    var target = ResolvePackageFile(packageRoot, value.GetString()!);
                    if (target is not null)
                        return target;
                }
            }

            return ResolvePackageFile(packageRoot, "index.js");
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
                var hasSubpathKeys = properties.Any(static property => property.Name.StartsWith('.', StringComparison.Ordinal));
                if (hasSubpathKeys)
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
                        var prefix = property.Name[..property.Name.IndexOf('*')];
                        var suffix = property.Name[(property.Name.IndexOf('*') + 1)..];
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
                    JsonProperty? matchingProperty = null;
                    foreach (var candidate in properties)
                    {
                        if (string.Equals(candidate.Name, condition, StringComparison.Ordinal))
                        {
                            matchingProperty = candidate;
                            break;
                        }
                    }

                    if (matchingProperty is null)
                        continue;

                    var result = ResolveExportsValue(matchingProperty.Value.Value, subpath, conditions, wildcard);
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

    private static string? ResolvePackageFile(string packageRoot, string target)
    {
        var normalized = target.Replace('\\', '/').TrimStart('.', '/');
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Split('/').Any(static segment => segment is ".." or ""))
            return null;

        var candidate = Path.GetFullPath(Path.Combine(packageRoot, normalized.Replace('/', Path.DirectorySeparatorChar)));
        var root = Path.GetFullPath(packageRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            return null;

        foreach (var path in new[]
                 {
                     candidate,
                     candidate + ".js",
                     candidate + ".mjs",
                     candidate + ".cjs",
                     Path.Combine(candidate, "index.js"),
                     Path.Combine(candidate, "index.mjs")
                 })
        {
            if (File.Exists(path))
                return Path.GetRelativePath(packageRoot, path).Replace('\\', '/');
        }

        return null;
    }

    private static bool IsExternalPackage(LibraryAssets materialization, string specifier)
    {
        if (materialization.PackageReferences.TryGetValue(specifier, out var reference))
            return reference.Source is "npm" or "jsr";

        var packageName = GetPackageName(specifier);
        return materialization.PackageReferences.TryGetValue(packageName, out reference) &&
               reference.Source is "npm" or "jsr";
    }

    private static string GetPackageName(string specifier)
    {
        var segments = specifier.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var count = specifier.StartsWith('@', StringComparison.Ordinal) ? 2 : 1;
        return segments.Length >= count ? string.Join('/', segments.Take(count)) : specifier;
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
