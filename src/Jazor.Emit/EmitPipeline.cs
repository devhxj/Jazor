using Jazor.Common;

namespace Jazor.Emit;

/// <summary>
/// Coordinates one complete Jazor output request.
///
/// A producer has exactly one of two carriers (ModuleCatalog or a JS-resource manifest). This
/// class is the only boundary that turns those carriers into a host output directory. Individual
/// writers update the standard project in place, with atomic writes for individual files.
/// </summary>
internal sealed class EmitPipeline
{
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
            var collection = CollectModules(options);
            if (!collection.IsSuccess)
                return EmitPipelineResult.Fail(collection.ExitCode, collection.Error!);

            // Write-phase boundary: all pure validation runs first so a rejected request never
            // leaves partial output. Failures after the first write are reported and converge
            // on the next build (see artifact-pipeline "写入与确定性").
            var validatedAssets = ValidateCatalogAssets(
                options.SourceRoot,
                collection.Assets,
                GetReservedOutputPaths(collection.Modules),
                cancellationToken);

            // 就地写入最终 jazor/：不做 staging、备份或回滚。失败显式返回，下一次构建收敛。
            Directory.CreateDirectory(outputRoot);

            var moduleWrite = ModuleWriter.Write(
                options.RootAssemblyPath,
                outputRoot,
                manifestPath,
                collection.Modules);
            if (!moduleWrite.IsSuccess)
                return EmitPipelineResult.Fail(moduleWrite.ExitCode, moduleWrite.Error!);

            cancellationToken.ThrowIfCancellationRequested();

            var applicationManifest = ManifestModel.TryLoad(manifestPath)
                ?? throw new InvalidOperationException("Emit did not produce the application manifest.");
            var entryPaths = ProjectEntryWriter.Write(
                outputRoot,
                applicationManifest.Entries,
                options.EnableSsr,
                applicationManifest.Modules.Where(module => module.Hmr is not null)
                    .Select(module => module.RelativePath).ToArray());
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

            CopyCatalogAssets(outputRoot, validatedAssets, cancellationToken);

            // Resolve package resources only after the application graph is known. Passing an
            // explicit (possibly empty) root set prevents unused package entries from leaking
            // into the output merely because their manifest was transitively available.
            var materializer = new LibraryMaterializer();
            var browserLibraries = materializer.Materialize(
                options.LibraryManifests,
                outputRoot,
                options.Mode,
                GetPackageImports(applicationManifest),
                applicationManifest.Modules.Select(static module => module.RelativePath));
            var packageLibraries = browserLibraries;
            if (options.EnableSsr)
            {
                // SSR uses the wider package closure in the same project root; there is no second
                // workspace to merge from. The browser view stays limited to browser roots via
                // the browserLibraries computed above.
                packageLibraries = materializer.Materialize(
                    options.LibraryManifests,
                    outputRoot,
                    options.Mode,
                    packageImports,
                    applicationManifest.Modules.Select(static module => module.RelativePath));
            }
            // Standard JavaScript tooling and DenoHost consume this same package.json/node_modules from jazor/.
            LibraryPackageWriter.WritePackageProject(outputRoot, packageLibraries, options.EnableSsr);
            await DenoPackageRestorer.RestoreAndCheckAsync(
                outputRoot,
                options.DenoExecutablePath,
                entryPaths,
                packageLibraries,
                cancellationToken).ConfigureAwait(false);
            if (options.Mode == BuildMode.Production)
            {
                var bundleResult = await BuildBrowserBundleAsync(
                    options,
                    outputRoot,
                    manifestPath,
                    cancellationToken).ConfigureAwait(false);
                if (!bundleResult.IsSuccess)
                    return EmitPipelineResult.Fail(
                        bundleResult.ExitCode,
                        bundleResult.Diagnostic?.Message ?? "Jazor browser bundle failed.");

            }

            cancellationToken.ThrowIfCancellationRequested();

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
            collector.AddRootAssembly(options.RootAssemblyPath);
            foreach (var assemblyPath in options.AssemblyPaths)
                collector.AddAssembly(assemblyPath);
            foreach (var assemblyPath in options.ModuleAssemblyPaths ?? [])
                collector.AddRootAssembly(assemblyPath);
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
    {
        // The application manifest is the authoritative carrier for generated project modules.
        // A module path may also appear in PackageImports when a direct Razor render edge was
        // discovered before the catalog was assembled; keep it in the application graph and do
        // not ask a library manifest to provide the same authored path.
        var applicationModulePaths = manifest.Modules
            .Select(static module => NormalizeModuleIdentity(module.RelativePath))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return manifest.Modules
            .SelectMany(static module => module.PackageImports ?? [])
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Where(value => !applicationModulePaths.Contains(NormalizeModuleIdentity(value)))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();

        static string NormalizeModuleIdentity(string value)
        {
            var normalized = value.Replace('\\', '/').Trim();
            while (normalized.StartsWith("./", StringComparison.Ordinal))
                normalized = normalized[2..];
            return normalized.TrimStart('/');
        }
    }

    private static async Task<ToolchainResult> BuildBrowserBundleAsync(
        EmitOptions options,
        string projectRoot,
        string manifestPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // The authored package script owns the tool and output layout.
        var result = await new JavaScriptProjectBuilder().BuildAsync(
            projectRoot,
            options.DenoExecutablePath,
            cancellationToken).ConfigureAwait(false);
        return result with { ModuleCount = ManifestModel.TryLoad(manifestPath)?.Modules.Count ?? 0 };
    }

    /// <summary>
    /// 写出前的纯校验：asset 路径冲突、源文件存在性、源字节 hash。
    ///
    /// 这些检查不依赖输出状态，因此在任何写入之前完成——"写入前校验失败不产生输出"这条契约
    /// 靠它保持；写入开始后的失败则按就地写入契约显式返回，由下一次构建收敛。
    /// </summary>
    private static IReadOnlyList<ValidatedAsset> ValidateCatalogAssets(
        string? sourceRoot,
        IReadOnlyList<AssetEntry> assets,
        IReadOnlySet<string> reservedOutputPaths,
        CancellationToken cancellationToken)
    {
        if (assets.Count == 0)
            return [];
        if (string.IsNullOrWhiteSpace(sourceRoot))
            throw new InvalidOperationException("ModuleCatalog assets require --source-root.");
        ArgumentNullException.ThrowIfNull(reservedOutputPaths);

        var sourceBase = Path.GetFullPath(sourceRoot);
        var validated = new List<ValidatedAsset>(assets.Count);
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

            validated.Add(new ValidatedAsset(sourcePath, artifactPath, expectedHash));
        }

        return validated;
    }

    private static void CopyCatalogAssets(
        string destinationRoot,
        IReadOnlyList<ValidatedAsset> assets,
        CancellationToken cancellationToken)
    {
        foreach (var asset in assets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetPath = GetSafePath(destinationRoot, asset.ArtifactPath);

            // 校验阶段已确认源字节与声明的 hash 一致；源在两次读取之间变化属于竞态，
            // 复制后再确认一次，保证落盘内容与声明相符。
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            if (File.Exists(targetPath) && FileBytesEqual(asset.SourcePath, targetPath))
                continue;

            File.Copy(asset.SourcePath, targetPath, overwrite: true);
            if (asset.ExpectedHash is not null &&
                !string.Equals(ComputeSha256(targetPath), asset.ExpectedHash, StringComparison.Ordinal))
            {
                File.Delete(targetPath);
                throw new LibraryException(
                    "JAZOR_MODULE_ASSET_HASH_MISMATCH",
                    $"ModuleCatalog asset '{asset.ArtifactPath}' changed while it was being copied.");
            }
        }
    }

    private readonly record struct ValidatedAsset(
        string SourcePath,
        string ArtifactPath,
        string? ExpectedHash);

    /// <summary>
    /// 在首次写入前检查模块是否覆盖项目入口/配置，并保留模块与 map 路径供 asset 冲突检查。
    /// </summary>
    private static IReadOnlySet<string> GetReservedOutputPaths(IReadOnlyList<ModuleRecord> modules)
    {
        var owned = CreateReservedPathSet();
        var paths = new HashSet<string>(owned, StringComparer.OrdinalIgnoreCase);
        foreach (var module in modules)
        {
            AddModulePath(module.RelativePath);
            if (!string.IsNullOrWhiteSpace(module.SourceMapRelativePath))
                AddModulePath(module.SourceMapRelativePath!);
        }

        return paths;

        void AddModulePath(string path)
        {
            var normalized = NormalizeRelativePath(path);
            if (owned.Contains(normalized))
                throw new InvalidOperationException($"Module path '{path}' conflicts with a generated or Emit-owned output file.");
            paths.Add(normalized);
        }
    }

    private static HashSet<string> CreateReservedPathSet()
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ProjectEntryWriter.BrowserEntryFileName,
            ProjectEntryWriter.SsrEntryFileName,
            ProjectEntryWriter.HydrationEntryFileName,
            "package.json",
            "package-lock.json",
            "deno.lock"
        };

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
