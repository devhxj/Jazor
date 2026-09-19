using Jazor.Common;

namespace Jazor.Emit;

/// <summary>
/// Coordinates one complete Jazor output request.
///
/// A producer has exactly one of two carriers (ModuleCatalog or a JS-resource manifest). This
/// class is the only boundary that turns those carriers into a host output directory. Individual
/// writers are deliberately used only inside the private staging tree; callers never observe
/// their intermediate files.
/// </summary>
internal sealed class EmitPipeline
{
    private const string ApplicationManifestFileName = "jazor-manifest.json";
    private const string SsrDirectoryName = "ssr";
    private const string SsrVueSpecifier = "vue";
    private const string SsrRendererSpecifier = "@vue/server-renderer";

    public async Task<EmitPipelineResult> ExecuteAsync(
        EmitOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        string? temporarySsrMaterializationRoot = null;
        try
        {
            ValidateOptions(options);
            cancellationToken.ThrowIfCancellationRequested();

            var outputRoot = Path.GetFullPath(options.OutputDirectory);
            var manifestPath = Path.GetFullPath(options.ManifestPath);
            EnsureManifestIsOwnedByOutput(outputRoot, manifestPath);

            await using var transaction = await OutputTransaction.CreateAsync(
                outputRoot,
                options.Clean,
                cancellationToken).ConfigureAwait(false);

            var collection = CollectModules(options);
            if (!collection.IsSuccess)
                return EmitPipelineResult.Fail(collection.ExitCode, collection.Error!);

            var stagedManifestPath = Path.Combine(transaction.StagingRoot, ApplicationManifestFileName);
            var moduleWrite = ModuleWriter.Write(
                options.RootAssemblyPath,
                transaction.StagingRoot,
                stagedManifestPath,
                collection.Modules,
                clean: true);
            if (!moduleWrite.IsSuccess)
                return EmitPipelineResult.Fail(moduleWrite.ExitCode, moduleWrite.Error!);

            cancellationToken.ThrowIfCancellationRequested();

            var applicationManifest = ManifestModel.TryLoad(stagedManifestPath)
                ?? throw new InvalidOperationException("Emit did not produce the application manifest.");
            var packageImports = GetPackageImports(applicationManifest);
            if (options.EnableSsr)
            {
                // SSR uses the same restored package graph as the browser profile. Include its
                // runtime packages before writing package.json so no second restore can occur.
                packageImports = packageImports
                    .Concat([SsrVueSpecifier, SsrRendererSpecifier])
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(static value => value, StringComparer.Ordinal)
                    .ToArray();
            }
            var reservedOutputPaths = GetReservedOutputPaths(applicationManifest);

            CopyCatalogAssets(
                transaction.StagingRoot,
                options.SourceRoot,
                collection.Assets,
                reservedOutputPaths,
                cancellationToken);

            // Resolve package resources only after the application graph is known. Passing an
            // explicit (possibly empty) root set prevents unused package entries from leaking
            // into the output merely because their manifest was transitively available.
            var materializer = new LibraryMaterializer();
            var browserLibraries = materializer.Materialize(
                options.LibraryManifests,
                transaction.StagingRoot,
                options.Mode,
                GetPackageImports(applicationManifest),
                applicationManifest.Modules.Select(static module => module.RelativePath));
            var packageLibraries = browserLibraries;
            string? ssrMaterializationRoot = null;
            if (options.EnableSsr)
            {
                // Build the wider SSR closure in an isolated package workspace. Merge its local
                // package files into the root project, while the browser materialization remains
                // limited to browser roots so SSR-only entries do not leak into the browser view.
                ssrMaterializationRoot = Path.Combine(
                    Path.GetDirectoryName(transaction.StagingRoot)!,
                    ".jazor-ssr-materialization-" + Guid.NewGuid().ToString("N"));
                temporarySsrMaterializationRoot = ssrMaterializationRoot;
                packageLibraries = materializer.Materialize(
                    options.LibraryManifests,
                    ssrMaterializationRoot,
                    options.Mode,
                    packageImports,
                    applicationManifest.Modules.Select(static module => module.RelativePath));
                foreach (var ownedRoot in new[] { "packages" })
                {
                    MergeDirectory(
                        Path.Combine(ssrMaterializationRoot, ownedRoot),
                        Path.Combine(transaction.StagingRoot, ownedRoot));
                }
            }
            // Keep the browser/debug artifact root package-aware as well. NetPack uses a
            // temporary projection for release, while DenoHost and local diagnostics consume
            // this same package.json/node_modules contract from jazor/.
            ResetPackageWorkspace(transaction.StagingRoot);
            LibraryPackageWriter.WritePackageProject(transaction.StagingRoot, packageLibraries);
            await DenoPackageRestorer.RestoreAndCheckAsync(
                transaction.StagingRoot,
                options.DenoExecutablePath,
                applicationManifest.Modules
                    .Select(module => Path.Combine(transaction.StagingRoot, module.RelativePath))
                    .ToArray(),
                packageLibraries,
                cancellationToken).ConfigureAwait(false);
            // External imports are mapped only after restore, so the map can follow the
            // package's real exports target from the shared node_modules tree.
            await ImportMapWriter.WriteAsync(
                transaction.StagingRoot,
                browserLibraries,
                applicationManifest.Modules,
                cancellationToken).ConfigureAwait(false);

            if (options.Mode == BuildMode.Production)
            {
                var bundleResult = await BuildBrowserBundleAsync(
                    options,
                    transaction.StagingRoot,
                    stagedManifestPath,
                    browserLibraries,
                    cancellationToken).ConfigureAwait(false);
                if (!bundleResult.IsSuccess)
                    return EmitPipelineResult.Fail(
                        bundleResult.ExitCode,
                        bundleResult.Diagnostic?.Message ?? "Jazor browser bundle failed.");

                // The browser release contract is the bundle projection. The raw application
                // graph is an input to Netpack, not a second release carrier; keep only assets
                // that the bundle still references (package/static/CSS) and remove generated
                // modules plus their debug manifests/maps before the outer atomic commit.
                // 浏览器 Release 只交付 bundle 投影，不能把调试 raw graph 一并暴露到 JazorDir。
                RemoveBrowserRawProjection(transaction.StagingRoot, applicationManifest);
            }

            if (options.EnableSsr)
            {
                var ssrResult = await BuildSsrProfileAsync(
                    options,
                    transaction.StagingRoot,
                    collection.Modules,
                    collection.Assets,
                    packageLibraries,
                    cancellationToken).ConfigureAwait(false);
                if (!ssrResult.IsSuccess)
                    return EmitPipelineResult.Fail(ssrResult.ExitCode, ssrResult.Error!);

                // The package project and node_modules retain the complete graph for SSR. The
                // browser-facing package view remains the selected browser closure.
            }

            cancellationToken.ThrowIfCancellationRequested();
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return EmitPipelineResult.Success(
                collection.AssemblyCount,
                collection.CatalogCount,
                collection.Modules.Count,
                collection.Assets.Count,
                moduleWrite.Written,
                moduleWrite.Skipped,
                moduleWrite.Deleted,
                outputRoot);
        }
        catch (LibraryException exception)
        {
            return EmitPipelineResult.Fail(5, $"{exception.Code}: {exception.Message}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return EmitPipelineResult.Fail(6, "Jazor Emit was cancelled before the output was committed.");
        }
        catch (Exception exception)
        {
            return EmitPipelineResult.Fail(5, exception.ToString());
        }
        finally
        {
            if (temporarySsrMaterializationRoot is not null && Directory.Exists(temporarySsrMaterializationRoot))
            {
                try
                {
                    Directory.Delete(temporarySsrMaterializationRoot, recursive: true);
                }
                catch
                {
                }
            }
        }
    }

    private static void MergeDirectory(string sourceRoot, string destinationRoot)
    {
        if (!Directory.Exists(sourceRoot))
            return;

        foreach (var sourcePath in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories)
                     .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relativePath = Path.GetRelativePath(sourceRoot, sourcePath);
            var targetPath = GetSafePath(destinationRoot, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            if (File.Exists(targetPath))
            {
                if (!FileBytesEqual(sourcePath, targetPath))
                    throw new InvalidOperationException($"Library materialization conflict at '{relativePath}'.");
                continue;
            }

            File.Copy(sourcePath, targetPath);
        }
    }

    private static void ResetPackageWorkspace(string workspaceRoot)
    {
        // The materializer owns `packages/`: it is the local package carrier for embedded-mjs
        // sources and has already been committed for the current closure. Only the package
        // manager output is reset here; deno.lock remains available for an identity comparison.
        foreach (var directory in new[] { "node_modules" })
        {
            var path = Path.Combine(workspaceRoot, directory);
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }

        foreach (var fileName in new[] { "package.json", "package-lock.json" })
        {
            var path = Path.Combine(workspaceRoot, fileName);
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static CollectResult CollectModules(EmitOptions options)
    {
        var loadContext = new EmitLoadContext(options.RootAssemblyPath);
        try
        {
            var collector = new ModuleCollector(loadContext);
            collector.AddAssembly(options.RootAssemblyPath);
            foreach (var assemblyPath in options.AssemblyPaths)
                collector.AddAssembly(assemblyPath);
            return collector.Collect(options.RootAssemblyPath);
        }
        finally
        {
            // ModuleCatalog values are copied into immutable string records during collection;
            // release the collectible context before any output transaction can hold a DLL open.
            loadContext.Unload();
        }
    }

    private static IReadOnlyList<string> GetPackageImports(ManifestModel manifest)
        => manifest.Modules
            .SelectMany(static module => module.PackageImports ?? [])
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();

    private static async Task<ToolchainResult> BuildBrowserBundleAsync(
        EmitOptions options,
        string stagingRoot,
        string manifestPath,
        LibraryAssets materializedLibraries,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sourceRoot = options.SourceRoot ?? Path.GetDirectoryName(options.RootAssemblyPath);
        if (string.IsNullOrWhiteSpace(sourceRoot))
            throw new InvalidOperationException("Release Emit requires a source root for bundle inputs.");

        var request = ToolchainRequest.Create(
            manifestPath,
            stagingRoot,
            sourceRoot,
            stagingRoot,
            mode: BuildMode.Production,
            sourceMaps: true,
            minify: true,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps,
                ToolchainCapability.Minify
            },
            libraryManifests: options.LibraryManifests,
            materializedLibraries: materializedLibraries,
            packageRoot: stagingRoot);
        return await new Toolchain().BuildAsync(request).ConfigureAwait(false);
    }

    private static async Task<EmitPipelineResult> BuildSsrProfileAsync(
        EmitOptions options,
        string browserStagingRoot,
        IReadOnlyList<ModuleRecord> modules,
        IReadOnlyList<AssetEntry> assets,
        LibraryAssets libraries,
        CancellationToken cancellationToken)
    {
        var ssrRoot = Path.Combine(browserStagingRoot, SsrDirectoryName);
        Directory.CreateDirectory(ssrRoot);
        var ssrManifestPath = Path.Combine(ssrRoot, ApplicationManifestFileName);
        var moduleWrite = ModuleWriter.Write(
            options.RootAssemblyPath,
            ssrRoot,
            ssrManifestPath,
            modules,
            clean: true);
        if (!moduleWrite.IsSuccess)
            return EmitPipelineResult.Fail(moduleWrite.ExitCode, moduleWrite.Error!);

        var manifest = ManifestModel.TryLoad(ssrManifestPath)
            ?? throw new InvalidOperationException("Emit did not produce the SSR application manifest.");
        CopyCatalogAssets(
            ssrRoot,
            options.SourceRoot,
            assets,
            GetReservedOutputPaths(manifest),
            cancellationToken);

        // Keep the SSR browser-facing asset URLs stable while the executable package imports
        // resolve through the single restored node_modules tree in the parent jazor root.
        CopyMaterializedLibraryFiles(browserStagingRoot, ssrRoot, libraries, cancellationToken);
        // The root profile already restored the complete package graph, including Vue SSR
        // packages. SSR only writes its module/import-map view and resolves node_modules from
        // the parent jazor directory.
        await ImportMapWriter.WriteSsrAsync(
            ssrRoot,
            libraries,
            manifest.Modules,
            cancellationToken).ConfigureAwait(false);
        return EmitPipelineResult.Success(
            assemblyCount: 0,
            catalogCount: 0,
            moduleCount: modules.Count,
            assetCount: assets.Count,
            written: moduleWrite.Written,
            skipped: moduleWrite.Skipped,
            deleted: moduleWrite.Deleted,
            outputDirectory: ssrRoot);
    }

    private static void CopyMaterializedLibraryFiles(
        string sourceRoot,
        string destinationRoot,
        LibraryAssets libraries,
        CancellationToken cancellationToken)
    {
        foreach (var relativePath in libraries.MaterializedPaths
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourcePath = GetSafePath(sourceRoot, relativePath);
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Materialized library file was not found: '{relativePath}'.", sourcePath);

            var targetPath = GetSafePath(destinationRoot, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.Copy(sourcePath, targetPath, overwrite: true);
        }
    }

    private static void CopyCatalogAssets(
        string destinationRoot,
        string? sourceRoot,
        IReadOnlyList<AssetEntry> assets,
        IReadOnlySet<string> reservedOutputPaths,
        CancellationToken cancellationToken)
    {
        if (assets.Count == 0)
            return;
        if (string.IsNullOrWhiteSpace(sourceRoot))
            throw new InvalidOperationException("ModuleCatalog assets require --source-root.");
        ArgumentNullException.ThrowIfNull(reservedOutputPaths);

        var sourceBase = Path.GetFullPath(sourceRoot);
        foreach (var asset in assets
                     .OrderBy(static value => value.ArtifactPath, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(static value => value.SourcePath, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourcePath = GetSafePath(sourceBase, asset.SourcePath);
            var artifactPath = NormalizeRelativePath(asset.ArtifactPath);
            if (reservedOutputPaths.Contains(artifactPath))
            {
                throw new InvalidOperationException(
                    $"ModuleCatalog asset output '{artifactPath}' conflicts with a generated or Emit-owned output file.");
            }

            var targetPath = GetSafePath(destinationRoot, artifactPath);
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException(
                    $"ModuleCatalog asset source was not found: '{asset.SourcePath}'.",
                    sourcePath);

            // Verify the producer bytes before looking at an existing destination. Otherwise a
            // stale destination with identical bytes could hide a changed or corrupted source.
            var expectedHash = string.IsNullOrWhiteSpace(asset.Hash)
                ? null
                : NormalizeHash(asset.Hash);
            if (expectedHash is not null &&
                !string.Equals(ComputeSha256(sourcePath), expectedHash, StringComparison.Ordinal))
            {
                throw new LibraryException(
                    "JAZOR_MODULE_ASSET_HASH_MISMATCH",
                    $"ModuleCatalog asset '{artifactPath}' hash does not match its source.");
            }

            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            if (File.Exists(targetPath))
            {
                if (!FileBytesEqual(sourcePath, targetPath))
                    throw new InvalidOperationException(
                        $"ModuleCatalog asset output '{artifactPath}' is claimed by incompatible files.");
                continue;
            }

            File.Copy(sourcePath, targetPath);
            if (expectedHash is not null &&
                !string.Equals(ComputeSha256(targetPath), expectedHash, StringComparison.Ordinal))
            {
                File.Delete(targetPath);
                throw new LibraryException(
                    "JAZOR_MODULE_ASSET_HASH_MISMATCH",
                    $"ModuleCatalog asset '{artifactPath}' changed while it was being copied.");
            }
        }
    }

    private static IReadOnlySet<string> GetReservedOutputPaths(ManifestModel manifest)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ApplicationManifestFileName,
            ImportMapWriter.BrowserImportMapFileName,
            ImportMapWriter.SsrImportMapFileName,
            ImportMapWriter.AssetManifestFileName
        };

        foreach (var module in manifest.Modules)
        {
            paths.Add(NormalizeRelativePath(module.RelativePath));
            if (!string.IsNullOrWhiteSpace(module.SourceMapPath))
                paths.Add(NormalizeRelativePath(module.SourceMapPath!));
        }

        return paths;
    }

    private static void RemoveBrowserRawProjection(
        string outputRoot,
        ManifestModel applicationManifest)
    {
        foreach (var module in applicationManifest.Modules)
        {
            DeleteOutputFile(outputRoot, module.RelativePath);
            if (!string.IsNullOrWhiteSpace(module.SourceMapPath))
                DeleteOutputFile(outputRoot, module.SourceMapPath!);
        }

        // These files are debug/SSR graph metadata. SSR gets its own complete projection under
        // `ssr/`; retaining the browser copies would make profile selection ambiguous.
        foreach (var fileName in new[]
                 {
                     ApplicationManifestFileName,
                     ImportMapWriter.BrowserImportMapFileName,
                     ImportMapWriter.SsrImportMapFileName,
                     ImportMapWriter.AssetManifestFileName
                 })
        {
            DeleteOutputFile(outputRoot, fileName);
        }
    }

    private static void DeleteOutputFile(string outputRoot, string relativePath)
    {
        var target = GetSafePath(outputRoot, relativePath);
        if (File.Exists(target))
            File.Delete(target);
    }

    private static void ValidateOptions(EmitOptions options)
    {
        if (!File.Exists(options.RootAssemblyPath))
            throw new FileNotFoundException("Root assembly was not found.", options.RootAssemblyPath);
        foreach (var assemblyPath in options.AssemblyPaths)
        {
            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException("Referenced assembly was not found.", assemblyPath);
        }
        foreach (var manifestPath in options.LibraryManifests)
        {
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("Library manifest was not found.", manifestPath);
        }
    }

    private static void EnsureManifestIsOwnedByOutput(string outputRoot, string manifestPath)
    {
        var normalizedRoot = EnsureDirectorySeparator(outputRoot);
        var normalizedManifest = Path.GetFullPath(manifestPath);
        if (!normalizedManifest.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(Path.GetFileName(normalizedManifest), ApplicationManifestFileName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Emit manifest must be '{ApplicationManifestFileName}' directly under the output root '{outputRoot}'.");
        }
    }

    private static string GetSafePath(string root, string relativePath)
    {
        var normalizedRoot = EnsureDirectorySeparator(Path.GetFullPath(root));
        var candidate = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Path escapes output root: '{relativePath}'.");
        return candidate;
    }

    private static string NormalizeRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("ModuleCatalog asset output path cannot be empty.");

        var normalized = path.Replace('\\', '/').Trim();
        if (normalized.StartsWith('/', StringComparison.Ordinal) ||
            Path.IsPathRooted(normalized) ||
            (normalized.Length > 1 && char.IsLetter(normalized[0]) && normalized[1] == ':'))
        {
            throw new InvalidOperationException(
                $"ModuleCatalog asset output path must be relative: '{path}'.");
        }

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0 || segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException(
                $"ModuleCatalog asset output path cannot escape the output directory: '{path}'.");

        return string.Join('/', segments);
    }

    private static string EnsureDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;

    private static bool FileBytesEqual(string left, string right)
    {
        var leftInfo = new FileInfo(left);
        var rightInfo = new FileInfo(right);
        if (leftInfo.Length != rightInfo.Length)
            return false;
        using var leftStream = File.OpenRead(left);
        using var rightStream = File.OpenRead(right);
        for (var leftByte = leftStream.ReadByte(); leftByte >= 0; leftByte = leftStream.ReadByte())
        {
            if (leftByte != rightStream.ReadByte())
                return false;
        }
        return rightStream.ReadByte() < 0;
    }

    private static string ComputeSha256(string path)
        => ArtifactHash.ComputeSha256(File.ReadAllBytes(path));

    private static string NormalizeHash(string value)
        => ArtifactHash.RequireSha256(value, "ModuleCatalog asset hash");

    private sealed class OutputTransaction : IAsyncDisposable
    {
        private readonly string _outputRoot;
        private bool _committed;

        private OutputTransaction(string outputRoot, string stagingRoot)
        {
            _outputRoot = outputRoot;
            StagingRoot = stagingRoot;
        }

        public string StagingRoot { get; }

        public static Task<OutputTransaction> CreateAsync(
            string outputRoot,
            bool clean,
            CancellationToken cancellationToken)
        {
            var parent = Directory.GetParent(outputRoot)?.FullName
                ?? throw new InvalidOperationException($"Could not determine output parent for '{outputRoot}'.");
            Directory.CreateDirectory(parent);
            var stagingRoot = Path.Combine(parent, ".jazor-output-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stagingRoot);
            var previousLock = Path.Combine(outputRoot, "deno.lock");
            if (clean && File.Exists(previousLock))
            {
                // Keep the frozen graph available for `deno ci` even when the outer Emit
                // request intentionally starts with a clean artifact projection.
                File.Copy(previousLock, Path.Combine(stagingRoot, "deno.lock"), overwrite: true);
            }
            if (!clean && Directory.Exists(outputRoot))
                CopyDirectory(outputRoot, stagingRoot, cancellationToken);
            return Task.FromResult(new OutputTransaction(outputRoot, stagingRoot));
        }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parent = Directory.GetParent(_outputRoot)?.FullName
                ?? throw new InvalidOperationException($"Could not determine output parent for '{_outputRoot}'.");
            var backupRoot = Path.Combine(parent, ".jazor-output-backup-" + Guid.NewGuid().ToString("N"));
            var movedOld = false;
            var movedStaging = false;
            var preserveBackup = false;
            try
            {
                if (Directory.Exists(_outputRoot))
                {
                    DirectoryTransaction.Move(_outputRoot, backupRoot);
                    movedOld = true;
                }
                else if (File.Exists(_outputRoot))
                {
                    throw new InvalidOperationException($"Output path is a file, not a directory: '{_outputRoot}'.");
                }

                DirectoryTransaction.Move(StagingRoot, _outputRoot);
                movedStaging = true;
                _committed = true;
                if (movedOld && Directory.Exists(backupRoot))
                    Directory.Delete(backupRoot, recursive: true);
                return Task.CompletedTask;
            }
            catch
            {
                // Rollback is best effort. A failed initial move (for example, because the
                // output directory is a process CWD) must leave the original exception intact.
                if (movedStaging && _committed == false)
                    TryDeleteDirectory(_outputRoot);
                if (movedOld && !Directory.Exists(_outputRoot) && Directory.Exists(backupRoot))
                    preserveBackup = !TryMoveDirectory(backupRoot, _outputRoot);
                throw;
            }
            finally
            {
                if (!preserveBackup)
                    TryDeleteDirectory(backupRoot);
            }
        }

        public ValueTask DisposeAsync()
        {
            if (!_committed && Directory.Exists(StagingRoot))
                TryDeleteDirectory(StagingRoot);
            return ValueTask.CompletedTask;
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch (Exception)
            {
                // Cleanup must never replace the commit failure that is being reported.
            }
        }

        private static bool TryMoveDirectory(string source, string destination)
        {
            try
            {
                if (Directory.Exists(source))
                {
                    DirectoryTransaction.Move(source, destination);
                    return true;
                }
            }
            catch (Exception)
            {
                // Preserve the original failure when rollback is also blocked by a handle.
            }

            return false;
        }

        private static void CopyDirectory(string sourceRoot, string destinationRoot, CancellationToken cancellationToken)
        {
            foreach (var directory in Directory.EnumerateDirectories(sourceRoot, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var relative = Path.GetRelativePath(sourceRoot, directory);
                Directory.CreateDirectory(Path.Combine(destinationRoot, relative));
            }

            foreach (var file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var relative = Path.GetRelativePath(sourceRoot, file);
                var target = Path.Combine(destinationRoot, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target, overwrite: true);
            }
        }
    }
}

internal sealed record EmitPipelineResult(
    bool IsSuccess,
    int ExitCode,
    string? Error,
    int AssemblyCount,
    int CatalogCount,
    int ModuleCount,
    int AssetCount,
    int Written,
    int Skipped,
    int Deleted,
    string? OutputDirectory)
{
    public static EmitPipelineResult Success(
        int assemblyCount,
        int catalogCount,
        int moduleCount,
        int assetCount,
        int written,
        int skipped,
        int deleted,
        string outputDirectory)
        => new(true, 0, null, assemblyCount, catalogCount, moduleCount, assetCount, written, skipped, deleted, outputDirectory);

    public static EmitPipelineResult Fail(int exitCode, string error)
        => new(false, exitCode, error, 0, 0, 0, 0, 0, 0, 0, null);
}
