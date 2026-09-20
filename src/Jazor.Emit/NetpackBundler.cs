using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Jazor.Common;
using NetPack;
using NetPack.Graph;
using NetPack.Graph.Bundles;
using NetPack.Graph.Writers;

namespace Jazor.Emit;

/// <summary>
/// Bundles application and selected library modules through one Netpack graph.
/// </summary>
internal sealed class NetpackBundler
{
    private static readonly Regex ImportFromPattern = new(
        "(?<prefix>\\bfrom\\s+[\"'])(?<path>[^\"']+)(?<suffix>[\"'])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ImportOnlyPattern = new(
        "(?<prefix>\\bimport\\s+[\"'])(?<path>[^\"']+)(?<suffix>[\"'])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ImportExpressionPattern = new(
        "(?<prefix>\\bimport\\s*\\(\\s*[\"'])(?<path>[^\"']+)(?<suffix>[\"']\\s*\\))",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public async Task<BundleResult> BundleAsync(BundleOptions options)
    {
        ManifestModel manifest;
        try
        {
            manifest = ManifestModel.TryLoad(options.ManifestPath)
                ?? throw new FileNotFoundException("Manifest was not found.", options.ManifestPath);
        }
        catch (FileNotFoundException)
        {
            return BundleResult.Fail(6, $"Manifest was not found: '{options.ManifestPath}'.");
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException)
        {
            return BundleResult.Fail(6, $"Jazor manifest could not be read: '{options.ManifestPath}'. {ex.Message}");
        }

        var relativePaths = manifest.Modules
            .Select(static module => module.RelativePath.Replace('\\', '/'))
            .Where(static relativePath => !string.IsNullOrWhiteSpace(relativePath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static relativePath => relativePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (relativePaths.Length == 0)
            return BundleResult.Fail(7, $"No modules were found in '{options.ManifestPath}'.");

        Directory.CreateDirectory(options.ProjectRoot);

        var outputDirectory = Path.GetDirectoryName(options.OutputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        // 临时工作区留在项目根之下，NetPack 因此能通过普通的上溯 package 解析发现
        // 已经恢复好的 node_modules，而不必重建第二份投影。（阶段 C 将整体移除该工作区。）
        var bundleWorkspace = Path.Combine(options.ProjectRoot, "__jazor_netpack_bundle__");
        if (Directory.Exists(bundleWorkspace))
            Directory.Delete(bundleWorkspace, recursive: true);

        Directory.CreateDirectory(bundleWorkspace);

        try
        {
            var useMaterializedLibraries = options.MaterializedLibraries is not null;
            var libraries = options.MaterializedLibraries ?? new LibraryMaterializer().Materialize(
                options.LibraryManifests ?? [],
                bundleWorkspace,
                BuildMode.Production,
                manifest.Modules.SelectMany(static module => module.PackageImports ?? []),
                relativePaths);
            if (useMaterializedLibraries)
                CopyMaterializedLibraryFiles(options.ProjectRoot, bundleWorkspace, libraries);

            var assets = useMaterializedLibraries
                ? CopyMaterializedAssets(manifest, options.ProjectRoot, bundleWorkspace)
                : CopyAssets(manifest, options, bundleWorkspace);
            var externalPackageRewrites = new HashSet<string>(StringComparer.Ordinal);
            var importRewrites = new Dictionary<string, string>(
                CreateImportRewrites(
                    relativePaths,
                    assets.ImportRewrites,
                    libraries,
                    externalPackageRewrites),
                StringComparer.OrdinalIgnoreCase);
            PrepareBundledRouteRuntime(
                bundleWorkspace,
                libraries.ImportPaths,
                relativePaths,
                importRewrites,
                externalPackageRewrites);
            // 判据是"调用方是否已经恢复过依赖"：Emit 会先在自己的项目根完成 restore 并传入
            // 物化结果，此时直接复用那份 node_modules；直接调用 Toolchain 的调用方没有恢复过，
            // 因此必须补一次同样的 restore，否则 NetPack 解析不到任何上游包。
            var standalonePackageRoot = options.MaterializedLibraries is null;
            if (standalonePackageRoot)
            {
                LibraryPackageWriter.WritePackageProject(bundleWorkspace, libraries);
            }

            foreach (var relativePath in relativePaths)
            {
                var sourcePath = GetSafePath(options.ProjectRoot, relativePath);
                var targetPath = GetSafePath(bundleWorkspace, relativePath);
                var targetDirectory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrWhiteSpace(targetDirectory))
                    Directory.CreateDirectory(targetDirectory);

                var content = await File.ReadAllTextAsync(sourcePath);
                var rewritten = RewriteModuleImports(
                    content,
                    relativePath,
                    importRewrites,
                    externalPackageRewrites);
                await File.WriteAllTextAsync(targetPath, rewritten, Utf8WithoutBom);
            }

            if (standalonePackageRoot)
            {
                var checkEntries = relativePaths
                    .Select(relativePath => Path.Combine(bundleWorkspace, relativePath))
                    .ToArray();
                await DenoPackageRestorer.RestoreAndCheckAsync(
                    bundleWorkspace,
                    executablePath: null,
                    checkEntries,
                    libraries,
                    CancellationToken.None).ConfigureAwait(false);
            }

            var rootAssemblyName = GetRootAssemblyName(manifest);
            var entryRelativePaths = manifest.Modules
                .Where(module => StringComparer.OrdinalIgnoreCase.Equals(module.AssemblyName, rootAssemblyName))
                .Select(static module => module.RelativePath.Replace('\\', '/'))
                .Where(static relativePath => !string.IsNullOrWhiteSpace(relativePath))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static relativePath => relativePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (entryRelativePaths.Length == 0)
                entryRelativePaths = relativePaths;

            // Give the synthetic root the requested output stem. NetPack derives the primary
            // bundle name from the entry file; EntryNames only participates when a [hash]
            // placeholder is present. Keep the file in a private staging directory so it cannot
            // collide with an application module that happens to use the same stem.
            var requestedOutputName = Path.GetFileName(options.OutputPath);
            var requestedOutputStem = Path.GetFileNameWithoutExtension(requestedOutputName);
            if (string.IsNullOrWhiteSpace(requestedOutputStem))
                throw new InvalidOperationException($"Bundle output '{options.OutputPath}' must have a file name stem.");

            var syntheticEntryPath = Path.Combine(
                bundleWorkspace,
                "__jazor_entry__",
                requestedOutputStem + ".mjs");
            Directory.CreateDirectory(Path.GetDirectoryName(syntheticEntryPath)!);
            await File.WriteAllTextAsync(
                syntheticEntryPath,
                string.Join(
                    "\n",
                    entryRelativePaths.Select(relativePath =>
                    {
                        var importPath = Path.GetRelativePath(
                                Path.GetDirectoryName(syntheticEntryPath)!,
                                GetSafePath(bundleWorkspace, relativePath))
                            .Replace(Path.DirectorySeparatorChar, '/');
                        if (!importPath.StartsWith(".", StringComparison.Ordinal))
                            importPath = "./" + importPath;
                        return $"export * from \"{importPath}\";";
                    })),
                Utf8WithoutBom);

            var netpackOutputDirectory = Path.Combine(bundleWorkspace, "__jazor_netpack_output__");
            Directory.CreateDirectory(netpackOutputDirectory);
            var netpackOptions = new global::NetPack.BundleOptions
            {
                Format = ModuleFormat.Esm,
                Platform = Platform.Web,
                SourceMaps = options.SourceMaps,
                Minify = options.Minify && !string.Equals(Environment.GetEnvironmentVariable("JAZOR_NETPACK_NO_MINIFY"), "1", StringComparison.Ordinal),
                EntryNames = "[name]",
                // Browser bundles have no Node process object. NetPack already defines
                // process.env.NODE_ENV; provide the remaining environment namespace so
                // optional upstream checks such as `process.env.VITE_LOGGER_ENABLED` remain
                // valid after package-level tree shaking.
                Define = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["process.env"] = "{}"
                },
                // Selected entries are exposed through generated package.json `exports` maps.
                // NetPack therefore validates the same standard package graph used by Deno.
                ExternalPackages = false,
                MetafilePath = Path.Combine(netpackOutputDirectory, "metafile.json")
            };

            await WriteNetpackDirectoryAsync(
                    syntheticEntryPath,
                    netpackOutputDirectory,
                    netpackOptions)
                .ConfigureAwait(false);
            var netpackOutputs = Directory.EnumerateFiles(netpackOutputDirectory, "*", SearchOption.AllDirectories)
                .ToDictionary(
                    path => Path.GetRelativePath(netpackOutputDirectory, path).Replace('\\', '/'),
                    File.ReadAllBytes,
                    StringComparer.Ordinal);

            var wroteBundle = WriteOutputs(
                netpackOutputs,
                options.OutputPath,
                netpackOptions.MetafilePath!,
                requestedOutputStem,
                out var netpackCssPaths);
            if (!File.Exists(options.OutputPath))
            {
                return BundleResult.Fail(
                    8,
                    wroteBundle
                        ? $"Netpack did not materialize expected bundle '{options.OutputPath}'."
                        : $"Netpack did not emit a JavaScript entry bundle. Outputs: {string.Join(", ", netpackOutputs.Keys.OrderBy(static key => key, StringComparer.Ordinal))}.");
            }

            CopyLibraryPublishAssetsToOutput(options.OutputPath, bundleWorkspace, libraries.PublishAssets);
            CopyStaticAssetsToOutput(options.OutputPath, assets.StaticAssets);
            await WriteBundleCssAsync(
                options.OutputPath,
                netpackCssPaths
                    .Select(path => Path.Combine(netpackOutputDirectory, path))
                    .Concat(ResolveExternalStylesheetPaths(
                        // 直接在临时工作区里恢复时，恢复结果也在那里；否则复用项目根的 node_modules。
                        standalonePackageRoot ? bundleWorkspace : options.ProjectRoot,
                        libraries,
                        libraries.ExternalStyleModuleImports,
                        libraries.ExternalStylesheetPaths))
                    .Concat(libraries.StylePaths.Select(path => Path.Combine(bundleWorkspace, path)))
                    .ToArray());
            return BundleResult.Success(options.OutputPath, relativePaths.Length);
        }
        catch (Exception ex)
        {
            return BundleResult.Fail(8, ex.ToString());
        }
        finally
        {
            try
            {
                if (Directory.Exists(bundleWorkspace))
                    Directory.Delete(bundleWorkspace, recursive: true);
            }
            catch
            {
            }
        }
    }

    private static async Task<IReadOnlyList<global::NetPack.Graph.Writers.EmittedFile>> WriteNetpackDirectoryAsync(
        string entryPath,
        string outputDirectory,
        global::NetPack.BundleOptions options)
    {
        Directory.CreateDirectory(outputDirectory);

        // NetPack 0.8.2's ResultWriter renders bundles with Parallel.ForEachAsync while the
        // minifier mutates shared AST nodes. Build the graph through the public API, keep the
        // upstream tree-shaking pass, and render each asset/bundle in a stable sequence.
        using var traverse = await Traverse.From(
                entryPath,
                options.Externals,
                options.Shared,
                platform: options.Platform,
                defines: options.Define,
                aliases: options.Alias,
                loaders: options.Loader,
                conditions: options.Conditions,
                externalPackages: options.ExternalPackages,
                splitChunks: options.SplitChunks)
            .ConfigureAwait(false);

        var outputOptions = new OutputOptions
        {
            IsOptimizing = options.Minify,
            IsReloading = false,
            WithSourceMaps = options.SourceMaps,
            Format = options.Format,
            EntryNames = options.EntryNames,
            PublicPath = options.PublicPath,
            Banner = options.Banner,
            Licenses = options.Licenses,
            InlineLimit = options.InlineLimit,
            MetafilePath = options.MetafilePath
        };

        if (outputOptions.IsOptimizing)
            TreeShakePass.Run(traverse.Context);

        var emitted = new List<EmittedFile>();
        foreach (var asset in traverse.Context.Assets.Values
                     .OrderBy(static asset => asset.GetFileName(), StringComparer.Ordinal))
        {
            if (Bundle.IsInlined(asset.Root, asset, outputOptions))
                continue;

            var fileName = asset.GetFileName();
            var targetPath = GetSafePath(outputDirectory, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            await using var source = await asset.CreateStream(outputOptions).ConfigureAwait(false);
            await using var destination = File.Create(targetPath);
            await source.CopyToAsync(destination).ConfigureAwait(false);
            emitted.Add(new EmittedFile(fileName, destination.Length, 0, IsBundle: false));
        }

        foreach (var bundle in traverse.Context.Bundles.Values
                     .OrderBy(static bundle => bundle.GetFileName(), StringComparer.Ordinal)
                     .ThenBy(static bundle => bundle.Name, StringComparer.Ordinal))
        {
            var fileName = bundle.GetFileName();
            var targetPath = GetSafePath(outputDirectory, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            await using (var source = await bundle.CreateStream(outputOptions).ConfigureAwait(false))
            await using (var destination = File.Create(targetPath))
            {
                await source.CopyToAsync(destination).ConfigureAwait(false);
                emitted.Add(new EmittedFile(fileName, destination.Length, bundle.Items.Length, IsBundle: true));
            }

            if (bundle.SourceMap is { } sourceMap)
            {
                var mapName = fileName + ".map";
                var mapPath = GetSafePath(outputDirectory, mapName);
                Directory.CreateDirectory(Path.GetDirectoryName(mapPath)!);
                await File.WriteAllBytesAsync(mapPath, sourceMap).ConfigureAwait(false);
                emitted.Add(new EmittedFile(mapName, sourceMap.Length, 0, IsBundle: false));
            }
        }

        var ordered = emitted
            .OrderBy(static file => file.Name, StringComparer.Ordinal)
            .ToArray();
        if (!string.IsNullOrWhiteSpace(outputOptions.MetafilePath))
        {
            var metafilePath = Path.IsPathRooted(outputOptions.MetafilePath)
                ? outputOptions.MetafilePath
                : Path.Combine(Environment.CurrentDirectory, outputOptions.MetafilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(metafilePath)!);
            await File.WriteAllTextAsync(
                    metafilePath,
                    Traverse.BuildMetafile(traverse.Context, ordered, inlineLimit: outputOptions.InlineLimit))
                .ConfigureAwait(false);
        }

        return ordered;
    }

    private static void PrepareBundledRouteRuntime(
        string bundleWorkspace,
        IReadOnlyDictionary<string, string> libraryImportPaths,
        IReadOnlyList<string> moduleRelativePaths,
        IDictionary<string, string> importRewrites,
        ISet<string> externalPackageRewrites)
    {
        const string routingSpecifier = "@jazor/vue-runtime/blazor-routing.mjs";
        const string routeCatalogSpecifier = "@jazor/vue-runtime/routes.mjs";
        const string bundledRoutingPath = "__jazor_runtime/blazor-routing.mjs";

        // The routing helper imports the generated route catalog. The catalog is an application
        // module rather than a library alias, so rebase this one reverse edge into the graph.
        if (!moduleRelativePaths.Contains(routeCatalogSpecifier, StringComparer.OrdinalIgnoreCase) ||
            !libraryImportPaths.TryGetValue(routingSpecifier, out var routingRelativePath))
        {
            return;
        }

        var sourcePath = GetSafePath(bundleWorkspace, routingRelativePath);
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException($"Bundled route runtime was not materialized: '{routingRelativePath}'.", sourcePath);

        var targetPath = GetSafePath(bundleWorkspace, bundledRoutingPath);
        var targetDirectory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(targetDirectory))
            Directory.CreateDirectory(targetDirectory);

        var routeImportRewrites = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [routeCatalogSpecifier] = routeCatalogSpecifier
        };
        var source = File.ReadAllText(sourcePath);
        var rewritten = RewriteModuleImports(
            source,
            bundledRoutingPath,
            routeImportRewrites,
            new HashSet<string>(StringComparer.Ordinal));
        File.WriteAllText(targetPath, rewritten, Utf8WithoutBom);
        importRewrites[routingSpecifier] = bundledRoutingPath;
        externalPackageRewrites.Remove(routingSpecifier);
    }

    private static PreparedAssets CopyAssets(
        ManifestModel manifest,
        BundleOptions options,
        string bundleWorkspace)
    {
        var rewrites = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var staticAssets = new List<StaticAsset>();
        if (manifest.Assets.Count == 0)
            return new PreparedAssets(rewrites, staticAssets);

        if (string.IsNullOrWhiteSpace(options.ProjectRoot))
            throw new InvalidOperationException("Manifest assets require the project root.");

        foreach (var asset in manifest.Assets)
        {
            var sourcePath = GetSafePath(options.ProjectRoot, asset.SourcePath);
            var artifactPath = GetSafePath(bundleWorkspace, asset.ArtifactPath);
            var artifactDirectory = Path.GetDirectoryName(artifactPath);
            if (!string.IsNullOrWhiteSpace(artifactDirectory))
                Directory.CreateDirectory(artifactDirectory);

            File.Copy(sourcePath, artifactPath, overwrite: true);
            if (string.Equals(asset.Kind, AssetEntry.KindModuleSource, StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(asset.ImportPath))
                {
                    throw new InvalidOperationException(
                        $"Module-source asset '{asset.ArtifactPath}' must declare ImportPath.");
                }

                rewrites[asset.ImportPath.Replace('\\', '/')] = asset.ArtifactPath.Replace('\\', '/');
                continue;
            }

            staticAssets.Add(new StaticAsset(sourcePath, asset.ArtifactPath.Replace('\\', '/')));
        }

        return new PreparedAssets(rewrites, staticAssets);
    }

    private static PreparedAssets CopyMaterializedAssets(
        ManifestModel manifest,
        string artifactRoot,
        string bundleWorkspace)
    {
        var rewrites = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var staticAssets = new List<StaticAsset>();
        foreach (var asset in manifest.Assets)
        {
            var sourcePath = GetSafePath(artifactRoot, asset.ArtifactPath);
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Materialized asset was not found: '{asset.ArtifactPath}'.", sourcePath);
            if (!string.IsNullOrWhiteSpace(asset.Hash) &&
                !string.Equals(
                    ArtifactHash.ComputeSha256(File.ReadAllBytes(sourcePath)),
                    ArtifactHash.RequireSha256(asset.Hash, $"Manifest asset '{asset.ArtifactPath}' hash"),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Materialized asset '{asset.ArtifactPath}' hash does not match the application manifest.");
            }

            var artifactPath = GetSafePath(bundleWorkspace, asset.ArtifactPath);
            var artifactDirectory = Path.GetDirectoryName(artifactPath);
            if (!string.IsNullOrWhiteSpace(artifactDirectory))
                Directory.CreateDirectory(artifactDirectory);
            File.Copy(sourcePath, artifactPath, overwrite: true);

            if (string.Equals(asset.Kind, AssetEntry.KindModuleSource, StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(asset.ImportPath))
                    throw new InvalidOperationException($"Module-source asset '{asset.ArtifactPath}' must declare ImportPath.");
                rewrites[asset.ImportPath.Replace('\\', '/')] = asset.ArtifactPath.Replace('\\', '/');
                continue;
            }

            staticAssets.Add(new StaticAsset(artifactPath, asset.ArtifactPath.Replace('\\', '/')));
        }

        return new PreparedAssets(rewrites, staticAssets);
    }

    private static void CopyMaterializedLibraryFiles(
        string artifactRoot,
        string bundleWorkspace,
        LibraryAssets libraries)
    {
        foreach (var relativePath in libraries.MaterializedPaths
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            var sourcePath = GetSafePath(artifactRoot, relativePath);
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Materialized library file was not found: '{relativePath}'.", sourcePath);
            var targetPath = GetSafePath(bundleWorkspace, relativePath);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
                Directory.CreateDirectory(targetDirectory);
            File.Copy(sourcePath, targetPath, overwrite: true);
        }
    }

    private static IReadOnlyDictionary<string, string> CreateImportRewrites(
        IReadOnlyList<string> moduleRelativePaths,
        IReadOnlyDictionary<string, string> assetImportRewrites,
        LibraryAssets libraries,
        ISet<string> externalPackageRewrites)
    {
        var rewrites = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var relativePath in moduleRelativePaths)
            rewrites[relativePath] = relativePath;

        foreach (var (source, target) in assetImportRewrites)
            rewrites[source] = target;

        foreach (var (source, target) in libraries.ImportPaths)
        {
            if (LibraryPackageIdentity.TryGetReference(libraries, source, out var reference) &&
                string.Equals(reference.Source, "jsr", StringComparison.Ordinal))
            {
                // Deno materializes JSR packages under the npm compatibility identity. Keep
                // the package bare so NetPack still evaluates the restored package exports and
                // sideEffects graph instead of flattening a binding-owned file hint.
                rewrites[source] = LibraryPackageIdentity.GetCanonicalSpecifier(reference, source);
                externalPackageRewrites.Add(source);
                continue;
            }

            if (!string.Equals(source, target, StringComparison.Ordinal))
                rewrites[source] = target;
            if (IsExternalPackage(libraries, source))
                externalPackageRewrites.Add(source);
        }

        return rewrites;
    }

    private static void CopyLibraryPublishAssetsToOutput(
        string outputPath,
        string bundleWorkspace,
        IReadOnlyList<LibraryPublishAsset> publishAssets)
    {
        if (publishAssets.Count == 0)
            return;

        var outputRoot = Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? string.Empty;
        foreach (var asset in publishAssets
                     .DistinctBy(static asset => asset.RelativePath, StringComparer.OrdinalIgnoreCase)
                     .OrderBy(static asset => asset.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            var sourcePath = GetSafePath(bundleWorkspace, asset.RelativePath);
            var targetPath = GetSafePath(outputRoot, asset.RelativePath);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            if (File.Exists(targetPath))
            {
                if (!File.ReadAllBytes(sourcePath).AsSpan().SequenceEqual(File.ReadAllBytes(targetPath)))
                    throw new InvalidOperationException($"Library asset '{asset.RelativePath}' conflicts with a Netpack output.");
                continue;
            }

            File.Copy(sourcePath, targetPath, overwrite: true);
        }
    }

    private static async Task WriteBundleCssAsync(string outputPath, IReadOnlyList<string> stylePaths)
    {
        if (stylePaths.Count == 0)
            return;

        var content = new StringBuilder();
        foreach (var stylePath in stylePaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var style = await File.ReadAllTextAsync(stylePath);
            if (string.IsNullOrWhiteSpace(style))
                continue;
            if (content.Length > 0)
                content.Append('\n');
            content.Append(style.TrimEnd('\r', '\n')).Append('\n');
        }

        if (content.Length > 0)
            await File.WriteAllTextAsync(Path.ChangeExtension(outputPath, ".css"), content.ToString(), Utf8WithoutBom);
    }

    /// <summary>
    /// Resolves the stylesheet closure declared by selected external package entries.
    /// Style modules are ordinary ESM files (for example Element Plus' <c>style/css.mjs</c>)
    /// and can import other style modules or CSS files. The package manager owns the bytes;
    /// this resolver only follows the selected entry graph inside the restored node_modules.
    /// </summary>
    private static IReadOnlyList<string> ResolveExternalStylesheetPaths(
        string packageWorkspace,
        LibraryAssets libraries,
        IReadOnlyList<string> styleModuleSpecifiers,
        IReadOnlyList<string> stylesheetSpecifiers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageWorkspace);
        ArgumentNullException.ThrowIfNull(libraries);
        ArgumentNullException.ThrowIfNull(styleModuleSpecifiers);
        ArgumentNullException.ThrowIfNull(stylesheetSpecifiers);

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cssFiles = new List<string>();
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void VisitSpecifier(string specifier)
        {
            var file = ResolveExternalStyleFile(packageWorkspace, libraries, specifier, importer: null);
            VisitFile(file);
        }

        void VisitFile(string file)
        {
            var fullPath = Path.GetFullPath(file);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"External style file was not found: '{fullPath}'.", fullPath);

            if (!visiting.Add(fullPath))
                return;
            try
            {
                var extension = Path.GetExtension(fullPath);
                if (IsCssFileExtension(extension))
                {
                    if (visited.Add(fullPath))
                        cssFiles.Add(fullPath);
                    foreach (var import in ReadCssImports(fullPath))
                    {
                        var imported = ResolveExternalStyleFile(packageWorkspace, libraries, import, fullPath);
                        if (IsCssFileExtension(Path.GetExtension(imported)))
                            VisitFile(imported);
                    }

                    return;
                }

                if (!IsStyleModuleExtension(extension))
                    return;

                var source = File.ReadAllText(fullPath);
                foreach (var import in ReadModuleImports(source))
                {
                    var imported = ResolveExternalStyleFile(packageWorkspace, libraries, import, fullPath);
                    VisitFile(imported);
                }
            }
            finally
            {
                visiting.Remove(fullPath);
            }
        }

        foreach (var specifier in styleModuleSpecifiers
                     .Concat(stylesheetSpecifiers)
                     .Where(static value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.Ordinal)
                     .OrderBy(static value => value, StringComparer.Ordinal))
        {
            VisitSpecifier(specifier);
        }

        return cssFiles;
    }

    private static readonly Regex CssImportPattern = new(
        "@import\\s+(?:url\\(\\s*)?(?:[\"'](?<path>[^\"']+)[\"']|(?<pathBare>[^\\s;\\)]+))",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static IEnumerable<string> ReadModuleImports(string source)
    {
        foreach (Match match in ImportFromPattern.Matches(source))
            yield return match.Groups["path"].Value;
        foreach (Match match in ImportOnlyPattern.Matches(source))
            yield return match.Groups["path"].Value;
        foreach (Match match in ImportExpressionPattern.Matches(source))
            yield return match.Groups["path"].Value;
    }

    private static IEnumerable<string> ReadCssImports(string path)
    {
        var source = File.ReadAllText(path);
        foreach (Match match in CssImportPattern.Matches(source))
        {
            var value = match.Groups["path"].Success
                ? match.Groups["path"].Value
                : match.Groups["pathBare"].Value;
            if (string.IsNullOrWhiteSpace(value) ||
                value.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            var query = value.IndexOfAny(['?', '#']);
            yield return query < 0 ? value : value[..query];
        }
    }

    private static string ResolveExternalStyleFile(
        string packageWorkspace,
        LibraryAssets libraries,
        string specifier,
        string? importer)
    {
        var value = StripUrlSuffix(specifier);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("External style import cannot be empty.");

        if (value.StartsWith("./", StringComparison.Ordinal) ||
            value.StartsWith("../", StringComparison.Ordinal))
        {
            if (importer is null)
                throw new InvalidOperationException($"Relative external style import '{specifier}' has no importer.");
            var candidate = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(importer)!,
                value.Replace('/', Path.DirectorySeparatorChar)));
            return ResolveFileCandidate(candidate, packageWorkspace, specifier)
                ?? throw new FileNotFoundException(
                    $"External style '{specifier}' could not be resolved from '{importer}'.",
                    candidate);
        }

        var packageName = GetExternalPackageName(value);
        var packageRoot = ResolveRestoredPackageRoot(packageWorkspace, libraries, value, packageName);
        if (packageRoot is null)
            throw new DirectoryNotFoundException(
                $"Restored package '{packageName}' was not found below '{packageWorkspace}'.");

        var subpath = value.Length == packageName.Length
            ? "."
            : "./" + value[(packageName.Length + 1)..];
        var target = PackageExportsResolver.Resolve(packageRoot, subpath, browser: true, style: true);
        var resolved = target is null
            ? null
            : Path.Combine(packageRoot, target.Replace('/', Path.DirectorySeparatorChar));

        return resolved ?? throw new FileNotFoundException(
            $"Package style entry '{specifier}' could not be resolved from '{packageRoot}'.");
    }

    private static string? ResolveRestoredPackageRoot(
        string packageWorkspace,
        LibraryAssets libraries,
        string specifier,
        string authoredPackageName)
    {
        var candidateNames = new List<string>();
        if (LibraryPackageIdentity.TryGetReference(libraries, specifier, out var reference) &&
            reference.Source is "npm" or "jsr")
        {
            candidateNames.Add(reference.CanonicalName);
        }

        candidateNames.Add(authoredPackageName);
        foreach (var candidateName in candidateNames.Distinct(StringComparer.Ordinal))
        {
            var root = Path.Combine(
                packageWorkspace,
                "node_modules",
                candidateName.Replace('/', Path.DirectorySeparatorChar));
            if (Directory.Exists(root))
                return root;
        }

        return null;
    }

    private static string? ResolvePackageExport(string packageRoot, string subpath)
    {
        var packageJson = Path.Combine(packageRoot, "package.json");
        if (!File.Exists(packageJson))
            return null;

        using var document = JsonDocument.Parse(File.ReadAllText(packageJson));
        var root = document.RootElement;
        if (root.TryGetProperty("exports", out var exports))
        {
            var target = ResolveExportsValue(
                exports,
                subpath,
                ["style", "browser", "import", "module", "default", "deno", "node"],
                wildcard: null);
            if (target is not null)
            {
                var resolved = ResolveFileCandidate(
                    Path.Combine(packageRoot, target.TrimStart('.', '/').Replace('/', Path.DirectorySeparatorChar)),
                    packageRoot,
                    subpath,
                    allowMissing: true);
                if (resolved is not null)
                    return resolved;
            }
        }

        if (subpath != ".")
        {
            return ResolveFileCandidate(
                Path.Combine(packageRoot, subpath[2..].Replace('/', Path.DirectorySeparatorChar)),
                packageRoot,
                subpath,
                allowMissing: true);
        }

        foreach (var field in new[] { "style", "browser", "module", "main" })
        {
            if (root.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String)
            {
                var resolved = ResolveFileCandidate(
                    Path.Combine(packageRoot, value.GetString()!.Replace('/', Path.DirectorySeparatorChar)),
                    packageRoot,
                    field,
                    allowMissing: true);
                if (resolved is not null)
                    return resolved;
            }
        }

        return ResolveFileCandidate(Path.Combine(packageRoot, "index"), packageRoot, subpath, allowMissing: true);
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
                    JsonProperty? exact = null;
                    foreach (var property in properties)
                    {
                        if (string.Equals(property.Name, subpath, StringComparison.Ordinal))
                        {
                            exact = property;
                            break;
                        }
                    }

                    if (exact is { } exactProperty)
                    {
                        var result = ResolveExportsValue(exactProperty.Value, subpath, conditions, wildcard);
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
                        {
                            continue;
                        }

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

    private static string? ResolveFileCandidate(
        string candidate,
        string root,
        string source,
        bool allowMissing = false)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullCandidate = Path.GetFullPath(candidate);
        if (!fullCandidate.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"External style path '{source}' escaped package root '{root}'.");

        var candidates = new[]
        {
            fullCandidate,
            fullCandidate + ".css",
            fullCandidate + ".mjs",
            fullCandidate + ".js",
            fullCandidate + ".scss",
            Path.Combine(fullCandidate, "index.css"),
            Path.Combine(fullCandidate, "index.mjs"),
            Path.Combine(fullCandidate, "index.js")
        };
        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        if (allowMissing)
            return null;
        throw new FileNotFoundException($"External style '{source}' could not be resolved below '{root}'.", fullCandidate);
    }

    private static bool IsCssFileExtension(string extension)
        => extension.Equals(".css", StringComparison.OrdinalIgnoreCase);

    private static bool IsStyleModuleExtension(string extension)
        => extension.Equals(".mjs", StringComparison.OrdinalIgnoreCase) ||
           extension.Equals(".js", StringComparison.OrdinalIgnoreCase);

    private static string StripUrlSuffix(string value)
    {
        var index = value.IndexOfAny(['?', '#']);
        return (index < 0 ? value : value[..index]).Trim();
    }

    private static string GetExternalPackageName(string specifier)
    {
        var segments = specifier.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var count = specifier.StartsWith('@', StringComparison.Ordinal) ? 2 : 1;
        if (segments.Length < count)
            throw new InvalidOperationException($"External style specifier '{specifier}' does not contain a package name.");
        return string.Join('/', segments.Take(count));
    }

    private static void CopyStaticAssetsToOutput(string outputPath, IReadOnlyList<StaticAsset> staticAssets)
    {
        if (staticAssets.Count == 0)
            return;

        var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? string.Empty;
        foreach (var asset in staticAssets)
        {
            var targetPath = GetSafePath(outputDirectory, asset.OutputRelativePath);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            File.Copy(asset.SourcePath, targetPath, overwrite: true);
        }
    }

    private static string RewriteModuleImports(
        string content,
        string importerRelativePath,
        IReadOnlyDictionary<string, string> importRewrites,
        IReadOnlySet<string> externalPackageRewrites)
    {
        if (importRewrites.Count == 0)
            return content;

        string Rewrite(Match match)
        {
            var importPath = match.Groups["path"].Value;
            var rewrittenPath = RewriteImportPath(
                importPath,
                importerRelativePath,
                importRewrites,
                externalPackageRewrites);
            return ReferenceEquals(rewrittenPath, importPath)
                ? match.Value
                : match.Groups["prefix"].Value + rewrittenPath + match.Groups["suffix"].Value;
        }

        var rewritten = ImportOnlyPattern.Replace(ImportFromPattern.Replace(content, Rewrite), Rewrite);
        return ImportExpressionPattern.Replace(rewritten, Rewrite);
    }

    private static string RewriteImportPath(
        string importPath,
        string importerRelativePath,
        IReadOnlyDictionary<string, string> importRewrites,
        IReadOnlySet<string> externalPackageRewrites)
    {
        if (!importPath.StartsWith("./", StringComparison.Ordinal) &&
            !importPath.StartsWith("../", StringComparison.Ordinal))
        {
            if (!importRewrites.TryGetValue(importPath, out var rewrittenBarePath))
                return importPath;

            // Both generated module ids and npm package ids can look like bare specifiers.
            // Preserve only rewrites whose manifest identity is external; all other targets
            // are files in this workspace and must be relative to the importing module.
            return externalPackageRewrites.Contains(importPath)
                ? rewrittenBarePath
                : RebaseImportPath(rewrittenBarePath, importerRelativePath);
        }

        var resolvedPath = ResolveImportPath(importPath, importerRelativePath);
        return importRewrites.TryGetValue(resolvedPath, out var rewrittenPath)
            ? RebaseImportPath(rewrittenPath, importerRelativePath)
            : importPath;
    }

    private static bool IsExternalPackage(LibraryAssets libraries, string specifier)
    {
        if (libraries.PackageReferences.TryGetValue(specifier, out var reference))
            return reference.Source is "npm" or "jsr";

        var segments = specifier.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var count = specifier.StartsWith('@', StringComparison.Ordinal) ? 2 : 1;
        var packageName = segments.Length >= count
            ? string.Join('/', segments.Take(count))
            : specifier;
        return libraries.PackageReferences.TryGetValue(packageName, out reference) &&
               reference.Source is "npm" or "jsr";
    }

    private static string ResolveImportPath(string importPath, string importerRelativePath)
    {
        var importerDirectory = Path.GetDirectoryName(importerRelativePath.Replace('\\', '/'))?.Replace('\\', '/') ?? string.Empty;
        var segments = new List<string>();
        foreach (var segment in SplitPathSegments(importerDirectory))
            segments.Add(segment);

        foreach (var segment in SplitPathSegments(importPath))
        {
            if (segment == ".")
                continue;

            if (segment == "..")
            {
                if (segments.Count == 0)
                    throw new InvalidOperationException("Asset import path cannot escape the output directory.");

                segments.RemoveAt(segments.Count - 1);
                continue;
            }

            segments.Add(segment);
        }

        return string.Join("/", segments);
    }

    private static string RebaseImportPath(string targetRelativePath, string importerRelativePath)
    {
        var importerDirectory = Path.GetDirectoryName(importerRelativePath.Replace('\\', '/'))?.Replace('\\', '/') ?? string.Empty;
        var relativePath = string.IsNullOrEmpty(importerDirectory)
            ? targetRelativePath
            : Path.GetRelativePath(
                    importerDirectory.Replace('/', Path.DirectorySeparatorChar),
                    targetRelativePath.Replace('/', Path.DirectorySeparatorChar))
                .Replace('\\', '/');

        return relativePath.StartsWith("../", StringComparison.Ordinal) ||
               relativePath.StartsWith("./", StringComparison.Ordinal)
            ? relativePath
            : "./" + relativePath;
    }

    private static string[] SplitPathSegments(string path)
        => path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);

    private static bool WriteOutputs(
        IReadOnlyDictionary<string, byte[]> outputs,
        string outputPath,
        string metafilePath,
        string requestedOutputStem,
        out IReadOnlyList<string> cssPaths)
    {
        cssPaths = [];
        var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? string.Empty;
        var selection = SelectNetpackOutputs(outputs, metafilePath, requestedOutputStem);
        var bundleOutput = selection.JavaScriptPath is { } entryPath &&
                           outputs.TryGetValue(entryPath, out var entryBytes)
            ? new KeyValuePair<string, byte[]>(entryPath, entryBytes)
            : default;
        if (string.IsNullOrWhiteSpace(bundleOutput.Key))
            return false;

        cssPaths = selection.CssPaths;

        foreach (var (name, bytes) in outputs)
        {
            // Netpack may expose the synthetic entry module used to feed the bundler. It is an
            // internal staging input, never a public Jazor artifact; only the named bundle and
            // its map are committed below.
            if (name.StartsWith("__jazor_", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("metafile.json", StringComparison.OrdinalIgnoreCase))
                continue;

            var targetPath = GetSafePath(outputDirectory, name);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            File.WriteAllBytes(targetPath, bytes);
        }

        var bundleBytes = RewriteRazorSourceMapUrl(
            bundleOutput.Value,
            Path.GetFileName(bundleOutput.Key) + ".map",
            Path.GetFileName(outputPath) + ".map");
        File.WriteAllBytes(outputPath, bundleBytes);

        if (outputs.TryGetValue(bundleOutput.Key + ".map", out var mapBytes))
        {
            var materializedMap = RewriteSourceMapFile(mapBytes, Path.GetFileName(outputPath));
            File.WriteAllBytes(outputPath + ".map", materializedMap);
        }

        return true;
    }

    private static NetpackOutputSelection SelectNetpackOutputs(
        IReadOnlyDictionary<string, byte[]> outputs,
        string metafilePath,
        string requestedOutputStem)
    {
        var outputNames = outputs.Keys.ToHashSet(StringComparer.Ordinal);
        bool IsRequested(string path)
            => string.Equals(
                Path.GetFileNameWithoutExtension(path),
                requestedOutputStem,
                StringComparison.OrdinalIgnoreCase);

        var metadata = TryReadNetpackMetadata(metafilePath);
        var javascript = metadata
            .Where(item => IsJavaScriptOutput(item.Key) &&
                           string.Equals(item.Value.Flags, "entry", StringComparison.OrdinalIgnoreCase))
            .Select(static item => item.Key)
            .Where(outputNames.Contains)
            .OrderBy(path => IsRequested(path) ? 0 : 1)
            .ThenBy(static path => path, StringComparer.Ordinal)
            .FirstOrDefault();

        if (javascript is null)
        {
            javascript = metadata
                .Where(item => IsJavaScriptOutput(item.Key) &&
                               string.Equals(item.Value.EntryPoint, item.Key, StringComparison.Ordinal))
                .Select(static item => item.Key)
                .Where(outputNames.Contains)
                .OrderBy(path => IsRequested(path) ? 0 : 1)
                .ThenBy(static path => path, StringComparer.Ordinal)
                .FirstOrDefault();
        }

        javascript ??= outputs.Keys
            .Where(IsJavaScriptOutput)
            .Where(outputNames.Contains)
            .OrderBy(path => IsRequested(path) ? 0 : 1)
            .ThenBy(static path => path, StringComparer.Ordinal)
            .FirstOrDefault();

        var css = metadata
            .Where(item => IsCssOutput(item.Key))
            .Select(static item => item.Key)
            .Where(outputNames.Contains)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        if (css.Length == 0)
        {
            css = outputs.Keys
                .Where(IsCssOutput)
                .Where(outputNames.Contains)
                .OrderBy(static path => path, StringComparer.Ordinal)
                .ToArray();
        }

        return new NetpackOutputSelection(javascript, css);
    }

    private static IReadOnlyDictionary<string, NetpackOutputMetadata> TryReadNetpackMetadata(string metafilePath)
    {
        if (!File.Exists(metafilePath))
            return new Dictionary<string, NetpackOutputMetadata>(StringComparer.Ordinal);

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllBytes(metafilePath));
            if (!document.RootElement.TryGetProperty("outputs", out var outputs) ||
                outputs.ValueKind != JsonValueKind.Object)
            {
                return new Dictionary<string, NetpackOutputMetadata>(StringComparer.Ordinal);
            }

            var result = new Dictionary<string, NetpackOutputMetadata>(StringComparer.Ordinal);
            foreach (var output in outputs.EnumerateObject())
            {
                var entryPoint = output.Value.TryGetProperty("entryPoint", out var entry) &&
                                 entry.ValueKind == JsonValueKind.String
                    ? entry.GetString()
                    : null;
                var flags = output.Value.TryGetProperty("flags", out var flag) &&
                            flag.ValueKind == JsonValueKind.String
                    ? flag.GetString()
                    : null;
                result[output.Name.Replace('\\', '/')] = new NetpackOutputMetadata(entryPoint, flags);
            }

            return result;
        }
        catch (JsonException)
        {
            return new Dictionary<string, NetpackOutputMetadata>(StringComparer.Ordinal);
        }
    }

    private static bool IsCssOutput(string name)
        => name.EndsWith(".css", StringComparison.OrdinalIgnoreCase);

    private static bool IsJavaScriptOutput(string name)
        => (name.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase)) &&
           !name.EndsWith(".map", StringComparison.OrdinalIgnoreCase);

    private sealed record NetpackOutputMetadata(string? EntryPoint, string? Flags);

    private sealed record NetpackOutputSelection(string? JavaScriptPath, IReadOnlyList<string> CssPaths);

    private static byte[] RewriteRazorSourceMapUrl(byte[] bytes, string originalMapName, string materializedMapName)
    {
        var text = Encoding.UTF8.GetString(bytes);
        if (!text.Contains(originalMapName, StringComparison.Ordinal))
            return bytes;

        return Utf8WithoutBom.GetBytes(text.Replace(originalMapName, materializedMapName, StringComparison.Ordinal));
    }

    private static byte[] RewriteSourceMapFile(byte[] bytes, string materializedBundleName)
    {
        var sourceMap = JsonNode.Parse(bytes) as JsonObject
            ?? throw new InvalidDataException("Netpack emitted an invalid source map.");

        sourceMap["file"] = materializedBundleName;
        return Utf8WithoutBom.GetBytes(sourceMap.ToJsonString());
    }

    private static string GetRootAssemblyName(ManifestModel manifest)
        => string.IsNullOrWhiteSpace(manifest.RootAssemblyName)
            ? manifest.Modules.FirstOrDefault()?.AssemblyName ?? string.Empty
            : manifest.RootAssemblyName;

    private static string GetSafePath(string root, string relativePath)
    {
        var normalizedRoot = EnsureDirectorySeparator(Path.GetFullPath(root));
        var fullPath = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Path escapes the toolchain root: '{relativePath}'.");

        return fullPath;
    }

    private static string EnsureDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

    private static bool IsAncestorDirectory(string ancestor, string child)
    {
        var normalizedAncestor = EnsureDirectorySeparator(Path.GetFullPath(ancestor));
        var normalizedChild = Path.GetFullPath(child);
        return normalizedChild.StartsWith(normalizedAncestor, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Temporary import rewrites and static files prepared for Netpack.</summary>
    private sealed record PreparedAssets(
        IReadOnlyDictionary<string, string> ImportRewrites,
        IReadOnlyList<StaticAsset> StaticAssets);

    /// <summary>Maps one source file to its publish-relative path.</summary>
    private sealed record StaticAsset(
        string SourcePath,
        string OutputRelativePath);
}

/// <summary>Common success or failure result returned by the Netpack bundler.</summary>
internal sealed record BundleResult(
    bool IsSuccess,
    int ExitCode,
    string? Error,
    string? OutputPath,
    int ModuleCount)
{
    public static BundleResult Success(string outputPath, int moduleCount)
        => new(true, 0, null, outputPath, moduleCount);

    public static BundleResult Fail(int exitCode, string error)
        => new(false, exitCode, error, null, 0);
}
