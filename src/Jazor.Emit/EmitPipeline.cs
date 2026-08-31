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
            var browserLibraries = new LibraryMaterializer().Materialize(
                options.LibraryManifests,
                transaction.StagingRoot,
                options.Mode,
                packageImports,
                applicationManifest.Modules.Select(static module => module.RelativePath));
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
                // that the bundle still references (vendor/static/CSS) and remove generated
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
                    cancellationToken).ConfigureAwait(false);
                if (!ssrResult.IsSuccess)
                    return EmitPipelineResult.Fail(ssrResult.ExitCode, ssrResult.Error!);
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
            minify: false,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps
            },
            libraryManifests: options.LibraryManifests,
            materializedLibraries: materializedLibraries);
        return await new Toolchain().BuildAsync(request).ConfigureAwait(false);
    }

    private static async Task<EmitPipelineResult> BuildSsrProfileAsync(
        EmitOptions options,
        string browserStagingRoot,
        IReadOnlyList<ModuleRecord> modules,
        IReadOnlyList<AssetEntry> assets,
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
        var requiredImports = GetPackageImports(manifest)
            .Concat([SsrVueSpecifier, SsrRendererSpecifier])
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        var modulePaths = manifest.Modules.Select(static module => module.RelativePath).ToArray();
        var libraries = new LibraryMaterializer().Materialize(
            options.LibraryManifests,
            ssrRoot,
            options.Mode,
            requiredImports,
            modulePaths);
        await ImportMapWriter.WriteAsync(ssrRoot, libraries, manifest.Modules, cancellationToken).ConfigureAwait(false);
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
            try
            {
                if (Directory.Exists(_outputRoot))
                {
                    Directory.Move(_outputRoot, backupRoot);
                    movedOld = true;
                }
                else if (File.Exists(_outputRoot))
                {
                    throw new InvalidOperationException($"Output path is a file, not a directory: '{_outputRoot}'.");
                }

                Directory.Move(StagingRoot, _outputRoot);
                _committed = true;
                if (movedOld && Directory.Exists(backupRoot))
                    Directory.Delete(backupRoot, recursive: true);
                return Task.CompletedTask;
            }
            catch
            {
                if (Directory.Exists(_outputRoot) && _committed == false)
                    Directory.Delete(_outputRoot, recursive: true);
                if (movedOld && !Directory.Exists(_outputRoot) && Directory.Exists(backupRoot))
                    Directory.Move(backupRoot, _outputRoot);
                throw;
            }
            finally
            {
                if (Directory.Exists(backupRoot))
                    Directory.Delete(backupRoot, recursive: true);
            }
        }

        public ValueTask DisposeAsync()
        {
            if (!_committed && Directory.Exists(StagingRoot))
                Directory.Delete(StagingRoot, recursive: true);
            return ValueTask.CompletedTask;
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
