using System.Text;
using System.Text.Json;
using Jazor.Common;

namespace Jazor.Emit;

/// <summary>
/// Materializes the modules collected from <c>Jazor.Generated.ModuleCatalog</c>.
///
/// 写入就地完成：每个文件先写同目录临时文件再 rename，避免半写文件；过期文件按清单差异删除。
/// 不做 staging、备份或回滚——失败显式返回，下一次构建按同一规则收敛。
/// </summary>
internal sealed class ModuleWriter
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public static WriteResult Write(
        string rootAssemblyPath,
        string outputDirectory,
        string manifestPath,
        IReadOnlyList<ModuleRecord> modules,
        bool clean)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootAssemblyPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestPath);
        ArgumentNullException.ThrowIfNull(modules);

        try
        {
            var outputRoot = Path.GetFullPath(outputDirectory);
            var manifestFile = Path.GetFullPath(manifestPath);
            var preparedModules = PrepareModules(modules);
            var preparedAssets = PrepareAssets(modules);
            var existingManifest = ManifestModel.TryLoad(manifestFile);
            var nextManifest = BuildManifest(rootAssemblyPath, preparedModules, preparedAssets);
            var desiredFiles = BuildDesiredFiles(outputRoot, preparedModules);
            ValidateManifestCollision(manifestFile, desiredFiles);

            var staleFiles = clean
                ? FindStaleFiles(outputRoot, existingManifest, desiredFiles)
                : [];
            var writer = new InPlaceMaterializer(outputRoot, manifestFile);
            return writer.Commit(
                desiredFiles,
                staleFiles,
                nextManifest,
                Utf8WithoutBom);
        }
        catch (Exception ex)
        {
            return WriteResult.Fail(5, ex.Message);
        }
    }

    private static IReadOnlyList<PreparedModule> PrepareModules(IReadOnlyList<ModuleRecord> modules)
    {
        var byId = new Dictionary<string, PreparedModule>(StringComparer.Ordinal);
        var byPath = new Dictionary<string, PreparedModule>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in modules)
        {
            if (module is null)
                throw new InvalidOperationException("ModuleCatalog returned a null module.");

            var relativePath = NormalizeRelativePath(module.RelativePath, "module");
            var id = Required(module.Id, "module id");
            var assemblyName = Required(module.AssemblyName, "module assembly name");
            var typeName = Required(module.TypeName, "module type name");
            var content = (module.Content ?? string.Empty).ReplaceLineEndings("\n");
            var declaredHash = ArtifactHash.RequireSha256(module.Hash, $"Module '{id}' hash");
            if (!string.Equals(declaredHash, ComputeSha256Hex(content), StringComparison.Ordinal))
                throw new InvalidOperationException($"Module '{id}' hash does not match its content.");
            var hasSourceMap = !string.IsNullOrWhiteSpace(module.SourceMapRelativePath) ||
                               !string.IsNullOrWhiteSpace(module.SourceMapContent) ||
                               !string.IsNullOrWhiteSpace(module.MapHash);
            if (hasSourceMap &&
                (string.IsNullOrWhiteSpace(module.SourceMapRelativePath) ||
                 string.IsNullOrWhiteSpace(module.SourceMapContent) ||
                 string.IsNullOrWhiteSpace(module.MapHash)))
            {
                throw new InvalidOperationException(
                    $"Module '{id}' must provide SourceMapRelativePath, SourceMapContent and MapHash together.");
            }

            var sourceMapPath = hasSourceMap
                ? NormalizeRelativePath(module.SourceMapRelativePath!, "source map")
                : null;
            if (sourceMapPath is not null &&
                string.Equals(relativePath, sourceMapPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Module '{id}' uses the same path for code and source map.");
            }

            var sourceMapContent = hasSourceMap
                ? module.SourceMapContent!.ReplaceLineEndings("\n")
                : null;
            if (sourceMapContent is not null)
            {
                var declaredMapHash = ArtifactHash.RequireSha256(module.MapHash!, $"Module '{id}' source map hash");
                if (!string.Equals(declaredMapHash, ComputeSha256Hex(sourceMapContent), StringComparison.Ordinal))
                    throw new InvalidOperationException($"Module '{id}' source map hash does not match its content.");
            }
            var emittedContent = hasSourceMap
                ? AppendSourceMapUrl(content, Path.GetFileName(sourceMapPath!))
                : content;
            var emittedHash = ComputeSha256Hex(emittedContent);
            var mapHash = sourceMapContent is null ? null : ComputeSha256Hex(sourceMapContent);
            var prepared = new PreparedModule(
                module,
                assemblyName,
                typeName,
                id,
                relativePath,
                emittedContent,
                emittedHash,
                sourceMapPath,
                sourceMapContent,
                mapHash,
                NormalizeStrings(module.PackageImports),
                NormalizePaths(module.Dependencies));

            var identity = assemblyName + "::" + id;
            if (byId.TryGetValue(identity, out var existingById))
            {
                if (!Equivalent(existingById, prepared))
                    throw new InvalidOperationException($"Module identity '{identity}' is declared with conflicting content or metadata.");
                continue;
            }

            if (byPath.TryGetValue(relativePath, out var existingByPath))
            {
                if (!Equivalent(existingByPath, prepared))
                    throw new InvalidOperationException($"Module output path '{relativePath}' is claimed by incompatible modules.");
                continue;
            }

            byId.Add(identity, prepared);
            byPath.Add(relativePath, prepared);
        }

        var modulesByPath = byPath.Values
            .OrderBy(static module => module.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static module => module.TypeName, StringComparer.Ordinal)
            .ThenBy(static module => module.Id, StringComparer.Ordinal)
            .ToArray();

        var knownPaths = modulesByPath
            .Select(static module => module.RelativePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var module in modulesByPath)
        {
            foreach (var dependency in module.Dependencies)
            {
                if (!knownPaths.Contains(dependency))
                {
                    throw new InvalidOperationException(
                        $"Module '{module.Id}' declares missing generated-module dependency '{dependency}'.");
                }
            }
        }

        return modulesByPath;
    }

    private static IReadOnlyList<AssetEntry> PrepareAssets(IReadOnlyList<ModuleRecord> modules)
    {
        var byPath = new Dictionary<string, AssetEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var asset in modules.SelectMany(static module => module.Assets ?? []))
        {
            var normalized = asset with
            {
                SourcePath = NormalizeRelativePath(asset.SourcePath, "asset source"),
                ArtifactPath = NormalizeRelativePath(asset.ArtifactPath, "asset output"),
                Kind = string.IsNullOrWhiteSpace(asset.Kind) ? AssetEntry.KindStatic : asset.Kind,
                ImportPath = string.IsNullOrWhiteSpace(asset.ImportPath)
                    ? null
                    : NormalizeRelativePath(asset.ImportPath!, "asset import")
            };
            if (normalized.Kind is not (AssetEntry.KindStatic or AssetEntry.KindModuleSource))
                throw new InvalidOperationException($"Unsupported ModuleCatalog asset kind '{normalized.Kind}'.");
            if (normalized.Kind == AssetEntry.KindModuleSource && normalized.ImportPath is null)
                throw new InvalidOperationException($"Module-source asset '{normalized.ArtifactPath}' must declare ImportPath.");
            if (!string.IsNullOrWhiteSpace(normalized.Hash))
            {
                normalized = normalized with
                {
                    // Asset hashes are part of the catalog contract; retain the validated
                    // canonical value instead of only checking it and carrying the raw input.
                    Hash = ArtifactHash.RequireSha256(
                        normalized.Hash,
                        $"ModuleCatalog asset '{normalized.ArtifactPath}' hash")
                };
            }

            if (byPath.TryGetValue(normalized.ArtifactPath, out var existing) && !Equivalent(existing, normalized))
                throw new InvalidOperationException($"Asset output path '{normalized.ArtifactPath}' is claimed by incompatible assets.");
            byPath[normalized.ArtifactPath] = normalized;
        }

        return byPath.Values
            .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static ManifestModel BuildManifest(
        string rootAssemblyPath,
        IReadOnlyList<PreparedModule> modules,
        IReadOnlyList<AssetEntry> assets)
    {
        var entries = modules
            .Select(static module => new ModuleEntry(
                module.AssemblyName,
                module.TypeName,
                module.Id,
                module.RelativePath,
                module.EmittedHash,
                module.SourceMapRelativePath,
                module.MapHash,
                module.PackageImports,
                module.Hmr,
                module.Dependencies))
            .ToList();
        var manifest = new ManifestModel(rootAssemblyPath, entries);
        manifest.Assets.AddRange(assets);
        return manifest;
    }

    private static IReadOnlyDictionary<string, DesiredFile> BuildDesiredFiles(
        string outputRoot,
        IReadOnlyList<PreparedModule> modules)
    {
        var files = new Dictionary<string, DesiredFile>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in modules)
        {
            AddDesiredFile(files, outputRoot, module.RelativePath, module.EmittedContent, module.EmittedHash);
            if (module.SourceMapRelativePath is not null)
            {
                AddDesiredFile(files, outputRoot, module.SourceMapRelativePath, module.SourceMapContent!, module.MapHash!);
            }
        }

        return files;
    }

    private static void AddDesiredFile(
        IDictionary<string, DesiredFile> files,
        string outputRoot,
        string relativePath,
        string content,
        string hash)
    {
        var target = GetSafePath(outputRoot, relativePath);
        var desired = new DesiredFile(target, relativePath, content, hash);
        if (files.TryGetValue(target, out var existing) &&
            (!string.Equals(existing.Hash, desired.Hash, StringComparison.OrdinalIgnoreCase) ||
             !string.Equals(existing.Content, desired.Content, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Output path '{relativePath}' is claimed by incompatible generated files.");
        }

        files[target] = desired;
    }

    private static IReadOnlyList<string> FindStaleFiles(
        string outputRoot,
        ManifestModel? existingManifest,
        IReadOnlyDictionary<string, DesiredFile> desiredFiles)
    {
        if (existingManifest is null)
            return [];

        var stale = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in existingManifest.Modules)
        {
            AddStalePath(module.RelativePath);
            if (module.SourceMapPath is not null)
                AddStalePath(module.SourceMapPath);
        }

        return stale.OrderBy(static path => path, StringComparer.OrdinalIgnoreCase).ToArray();

        void AddStalePath(string relativePath)
        {
            var normalized = NormalizeRelativePath(relativePath, "manifest module");
            var target = GetSafePath(outputRoot, normalized);
            if (!desiredFiles.ContainsKey(target))
                stale.Add(target);
        }
    }

    private static void ValidateManifestCollision(
        string manifestPath,
        IReadOnlyDictionary<string, DesiredFile> desiredFiles)
    {
        if (desiredFiles.ContainsKey(manifestPath))
            throw new InvalidOperationException("The application manifest cannot overwrite a generated module or source map.");
    }

    private static bool Equivalent(PreparedModule left, PreparedModule right)
        => string.Equals(left.AssemblyName, right.AssemblyName, StringComparison.Ordinal) &&
           string.Equals(left.TypeName, right.TypeName, StringComparison.Ordinal) &&
           string.Equals(left.Id, right.Id, StringComparison.Ordinal) &&
           string.Equals(left.RelativePath, right.RelativePath, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(left.EmittedContent, right.EmittedContent, StringComparison.Ordinal) &&
           string.Equals(left.SourceMapRelativePath, right.SourceMapRelativePath, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(left.SourceMapContent, right.SourceMapContent, StringComparison.Ordinal) &&
           string.Equals(left.EmittedHash, right.EmittedHash, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(left.MapHash, right.MapHash, StringComparison.OrdinalIgnoreCase) &&
           left.PackageImports.SequenceEqual(right.PackageImports, StringComparer.Ordinal) &&
           left.Dependencies.SequenceEqual(right.Dependencies, StringComparer.OrdinalIgnoreCase) &&
           Equals(left.Hmr, right.Hmr);

    private static bool Equivalent(AssetEntry left, AssetEntry right)
        => string.Equals(left.SourcePath, right.SourcePath, StringComparison.Ordinal) &&
           string.Equals(left.ArtifactPath, right.ArtifactPath, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(left.Kind, right.Kind, StringComparison.Ordinal) &&
           string.Equals(left.Hash, right.Hash, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(left.ImportPath, right.ImportPath, StringComparison.Ordinal);

    private static IReadOnlyList<string> NormalizeStrings(IEnumerable<string>? values)
        => values?
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray() ?? [];

    private static IReadOnlyList<string> NormalizePaths(IEnumerable<string>? values)
        => values?
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(value => NormalizeRelativePath(value, "module dependency"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

    private static string Required(string? value, string description)
        => string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"ModuleCatalog {description} cannot be empty.")
            : value;

    private static string NormalizeRelativePath(string value, string kind)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Generated {kind} path cannot be empty.");
        var normalized = value.Replace('\\', '/').Trim();
        if (normalized.StartsWith('/', StringComparison.Ordinal) ||
            Path.IsPathRooted(normalized) ||
            (normalized.Length > 1 && char.IsLetter(normalized[0]) && normalized[1] == ':'))
        {
            throw new InvalidOperationException($"Generated {kind} path must be relative: '{value}'.");
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0 || segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException($"Generated {kind} path cannot escape its owner: '{value}'.");
        return string.Join('/', segments);
    }

    private static string GetSafePath(string root, string relativePath)
    {
        var normalizedRoot = Path.GetFullPath(root);
        var rootWithSeparator = normalizedRoot.EndsWith(Path.DirectorySeparatorChar)
            ? normalizedRoot
            : normalizedRoot + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Refusing to write outside output directory: '{relativePath}'.");
        return candidate;
    }

    private static string AppendSourceMapUrl(string content, string mapFileName)
    {
        var normalized = content.TrimEnd('\r', '\n');
        return normalized.Length == 0
            ? $"//# sourceMappingURL={mapFileName}\n"
            : normalized + "\n//# sourceMappingURL=" + mapFileName + "\n";
    }

    private static string ComputeSha256Hex(string content)
        => ArtifactHash.ComputeSha256(content);

    private sealed record PreparedModule(
        ModuleRecord Source,
        string AssemblyName,
        string TypeName,
        string Id,
        string RelativePath,
        string EmittedContent,
        string EmittedHash,
        string? SourceMapRelativePath,
        string? SourceMapContent,
        string? MapHash,
        IReadOnlyList<string> PackageImports,
        IReadOnlyList<string> Dependencies)
    {
        public HmrMetadata? Hmr => Source.Hmr;
    }

    private sealed record DesiredFile(
        string TargetPath,
        string RelativePath,
        string Content,
        string Hash);

    /// <summary>
    /// 就地写入生成结果。
    ///
    /// 写入契约：单文件先写临时文件再 rename，避免出现半写文件；过期文件按清单差异就地删除。
    /// 不做目录级 staging、备份或回滚——失败显式返回，下一次构建按同一规则收敛。
    /// HMR 需要模块随时变更，整目录快照与之冲突（见 artifact-pipeline 的"写入与确定性"）。
    /// </summary>
    private sealed class InPlaceMaterializer(string outputRoot, string manifestPath)
    {
        private readonly string _outputRoot = Path.GetFullPath(outputRoot);
        private readonly string _manifestPath = Path.GetFullPath(manifestPath);

        public WriteResult Commit(
            IReadOnlyDictionary<string, DesiredFile> desiredFiles,
            IReadOnlyList<string> staleFiles,
            ManifestModel manifest,
            Encoding encoding)
        {
            try
            {
                var written = 0;
                var skipped = 0;
                var deleted = 0;

                foreach (var desired in desiredFiles.Values
                             .OrderBy(static file => file.RelativePath, StringComparer.OrdinalIgnoreCase))
                {
                    if (Directory.Exists(desired.TargetPath))
                        throw new InvalidOperationException(
                            $"Output target is a directory, not a file: '{desired.TargetPath}'.");

                    var directory = Path.GetDirectoryName(desired.TargetPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                        Directory.CreateDirectory(directory);

                    if (File.Exists(desired.TargetPath) && FilesEqual(desired.TargetPath, desired.Content, encoding))
                    {
                        skipped++;
                        continue;
                    }

                    WriteAtomically(desired.TargetPath, desired.Content, encoding);
                    written++;
                }

                // 清单最后写：它是"这一轮写了什么"的记录，只应在内容写入成功后更新。
                WriteManifestAtomically(manifest);

                foreach (var stale in staleFiles)
                {
                    if (File.Exists(stale))
                    {
                        File.Delete(stale);
                        deleted++;
                    }
                }

                return WriteResult.Success(written, skipped, deleted);
            }
            catch (Exception ex)
            {
                return WriteResult.Fail(5, ex.Message);
            }
        }

        private void WriteManifestAtomically(ManifestModel manifest)
        {
            var directory = Path.GetDirectoryName(_manifestPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
            manifest.Save(_manifestPath);
        }

        private static void WriteAtomically(string targetPath, string content, Encoding encoding)
        {
            // 临时文件与目标同目录，保证 rename 在同一卷上是原子操作。
            var temporaryPath = targetPath + ".jazor-tmp-" + Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(temporaryPath, content, encoding);
                File.Move(temporaryPath, targetPath, overwrite: true);
            }
            finally
            {
                DeleteFile(temporaryPath);
            }
        }

        private static bool FilesEqual(string path, string content, Encoding encoding)
        {
            try
            {
                return string.Equals(File.ReadAllText(path, encoding), content, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        private static void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}

/// <summary>Counts files affected by one materialization operation.</summary>
internal sealed record WriteResult(
    bool IsSuccess,
    int ExitCode,
    string? Error,
    int Written,
    int Skipped,
    int Deleted)
{
    public static WriteResult Success(int written, int skipped, int deleted)
        => new(true, 0, null, written, skipped, deleted);

    public static WriteResult Fail(int exitCode, string error)
        => new(false, exitCode, error, 0, 0, 0);
}
