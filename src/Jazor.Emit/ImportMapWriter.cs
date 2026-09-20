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
        var authoredPackageName = GetPackageName(mappedSpecifier);
        var hasReference = LibraryPackageIdentity.TryGetReference(
            materialization,
            mappedSpecifier,
            out var packageReference);
        var packageName = hasReference ? packageReference.CanonicalName : authoredPackageName;
        // Embedded packages can expose a generated projection, while npm/JSR packages are
        // resolved from their restored package.json. Keep both paths in one resolver so an
        // import map never invents a second package layout.
        string? target = null;
        if (materialization.PackageProjections.TryGetValue(authoredPackageName, out var projection))
        {
            var projectionSpecifier = string.Equals(targetSpecifier, authoredPackageName, StringComparison.Ordinal)
                ? "."
                : "./" + targetSpecifier[(authoredPackageName.Length + 1)..];
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
            if (targetSpecifier.StartsWith(authoredPackageName + "/", StringComparison.Ordinal))
                subpaths.Add("./" + targetSpecifier[(authoredPackageName.Length + 1)..]);
            else if (string.Equals(targetSpecifier, authoredPackageName, StringComparison.Ordinal))
                subpaths.Add(".");
            var authoredSubpath = string.Equals(mappedSpecifier, authoredPackageName, StringComparison.Ordinal)
                ? "."
                : "./" + mappedSpecifier[(authoredPackageName.Length + 1)..];
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
            var unresolvedSubpath = mappedSpecifier.StartsWith(authoredPackageName + "/", StringComparison.Ordinal)
                ? mappedSpecifier[(authoredPackageName.Length + 1)..]
                : string.Empty;
            var unresolved = "node_modules/" + packageName +
                (string.IsNullOrWhiteSpace(unresolvedSubpath) ? string.Empty : "/" + unresolvedSubpath);
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
        return PackageExportsResolver.Resolve(packageRoot, subpath, browser);
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

    /// <summary>
    /// 就地写入 import map 与 asset manifest。
    ///
    /// 每个文件先写同目录临时文件再 rename，避免半写文件；不做 staging、备份或回滚——
    /// 失败显式传播，下一次构建按同一规则收敛。
    /// </summary>
    private static async Task CommitAsync(
        string outputRoot,
        IReadOnlyDictionary<string, string> payloads,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputRoot);
        foreach (var payload in payloads.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var target = Path.Combine(outputRoot, payload.Key);
            var temporary = target + ".jazor-tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                await File.WriteAllTextAsync(temporary, payload.Value, cancellationToken).ConfigureAwait(false);
                File.Move(temporary, target, overwrite: true);
            }
            finally
            {
                DeleteFile(temporary);
            }
        }
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
