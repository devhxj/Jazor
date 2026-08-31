using System.Text;
using System.Text.Json;
using Jazor.Common;

namespace Jazor.Emit;

/// <summary>
/// Materializes the modules collected from <c>Jazor.Generated.ModuleCatalog</c>.
///
/// The writer deliberately has one commit boundary: all module files, source maps and the
/// application manifest are prepared and validated in a sibling staging directory, then moved
/// into place with backups. A failed write therefore leaves the previous materialization intact.
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
            var transaction = new MaterializationTransaction(outputRoot, manifestFile);
            return transaction.Commit(
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

    private sealed class MaterializationTransaction(string outputRoot, string manifestPath)
    {
        private readonly string _outputRoot = Path.GetFullPath(outputRoot);
        private readonly string _manifestPath = Path.GetFullPath(manifestPath);

        public WriteResult Commit(
            IReadOnlyDictionary<string, DesiredFile> desiredFiles,
            IReadOnlyList<string> staleFiles,
            ManifestModel manifest,
            Encoding encoding)
        {
            var outputParent = Directory.GetParent(_outputRoot)?.FullName
                ?? throw new InvalidOperationException($"Could not determine output parent for '{_outputRoot}'.");
            var manifestParent = Directory.GetParent(_manifestPath)?.FullName
                ?? throw new InvalidOperationException($"Could not determine manifest parent for '{_manifestPath}'.");
            Directory.CreateDirectory(outputParent);
            Directory.CreateDirectory(manifestParent);

            var transactionRoot = Path.Combine(outputParent, ".jazor-emit-" + Guid.NewGuid().ToString("N"));
            var stagedRoot = Path.Combine(transactionRoot, "files");
            var backupRoot = Path.Combine(transactionRoot, "backup");
            var stagedManifest = Path.Combine(transactionRoot, "jazor-manifest.json");
            var backups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var committed = new List<string>();
            var written = 0;
            var skipped = 0;
            var deleted = 0;

            try
            {
                Directory.CreateDirectory(stagedRoot);
                Directory.CreateDirectory(backupRoot);
                foreach (var desired in desiredFiles.Values.OrderBy(static file => file.RelativePath, StringComparer.OrdinalIgnoreCase))
                {
                    var stagedPath = GetSafePath(stagedRoot, desired.RelativePath);
                    var directory = Path.GetDirectoryName(stagedPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                        Directory.CreateDirectory(directory);
                    File.WriteAllText(stagedPath, desired.Content, encoding);
                    var actual = ComputeSha256Hex(File.ReadAllText(stagedPath, encoding));
                    if (!string.Equals(actual, desired.Hash, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException($"Staged generated file '{desired.RelativePath}' failed hash verification.");
                }

                manifest.Save(stagedManifest);
                var manifestBytes = File.ReadAllBytes(stagedManifest);

                var targets = desiredFiles.Values
                    .Select(static file => file.TargetPath)
                    .Concat(staleFiles)
                    .Append(_manifestPath)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                foreach (var target in targets)
                {
                    if (File.Exists(target))
                    {
                        var backup = Path.Combine(backupRoot, backups.Count.ToString("D8"));
                        Directory.CreateDirectory(Path.GetDirectoryName(backup)!);
                        File.Move(target, backup);
                        backups[target] = backup;
                    }
                    else if (Directory.Exists(target))
                    {
                        throw new InvalidOperationException($"Output target is a directory, not a file: '{target}'.");
                    }
                }

                foreach (var desired in desiredFiles.Values.OrderBy(static file => file.TargetPath, StringComparer.OrdinalIgnoreCase))
                {
                    var directory = Path.GetDirectoryName(desired.TargetPath);
                    if (!string.IsNullOrWhiteSpace(directory))
                        Directory.CreateDirectory(directory);
                    var stagedPath = GetSafePath(stagedRoot, desired.RelativePath);
                    File.Move(stagedPath, desired.TargetPath);
                    committed.Add(desired.TargetPath);
                    if (backups.ContainsKey(desired.TargetPath))
                        written++;
                    else
                        written++;
                }

                File.Move(stagedManifest, _manifestPath);
                committed.Add(_manifestPath);
                foreach (var target in staleFiles)
                {
                    if (backups.ContainsKey(target))
                        deleted++;
                }

                // A file that was already byte-identical still participates in the atomic swap,
                // but report it as skipped for the caller's incremental diagnostics.
                foreach (var desired in desiredFiles.Values)
                {
                    var old = backups.TryGetValue(desired.TargetPath, out var backup)
                        ? backup
                        : null;
                    if (old is not null && FilesEqual(old, desired.Content, encoding))
                    {
                        written--;
                        skipped++;
                    }
                }

                DeleteDirectory(transactionRoot);
                return WriteResult.Success(written, skipped, deleted);
            }
            catch (Exception ex)
            {
                foreach (var target in committed.AsEnumerable().Reverse())
                    DeleteFile(target);
                foreach (var pair in backups.OrderByDescending(static pair => pair.Value, StringComparer.OrdinalIgnoreCase))
                {
                    if (File.Exists(pair.Value) && !File.Exists(pair.Key))
                    {
                        var directory = Path.GetDirectoryName(pair.Key);
                        if (!string.IsNullOrWhiteSpace(directory))
                            Directory.CreateDirectory(directory);
                        File.Move(pair.Value, pair.Key);
                    }
                }

                DeleteDirectory(transactionRoot);
                return WriteResult.Fail(5, ex.Message);
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

        private static string GetSafePath(string root, string relativePath)
        {
            var normalizedRoot = Path.GetFullPath(root);
            var rootWithSeparator = normalizedRoot.EndsWith(Path.DirectorySeparatorChar)
                ? normalizedRoot
                : normalizedRoot + Path.DirectorySeparatorChar;
            var candidate = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            if (!candidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Refusing to stage outside transaction directory: '{relativePath}'.");
            return candidate;
        }

        private static void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
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
