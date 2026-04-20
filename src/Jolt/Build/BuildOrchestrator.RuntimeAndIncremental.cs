using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jazor.Vue;
using Jolt.DevServer;
using Jolt.Frontend.Deno.Hosting;

namespace Jolt.Build;

internal sealed partial class BuildOrchestrator
{
    /// <summary>
    /// Creates a DenoVolarHost for the build pipeline.
    /// </summary>
    private static DenoVolarHost CreateDenoHost()
    {
        var baseDirectory = ResolveDenoHostBaseDirectory();
        var parsedOptions = DenoVolarHostOptionsParser.Parse(["--deno-worker"], baseDirectory);
        var options = new DenoVolarHostOptions
        {
            Enabled = parsedOptions.Enabled,
            ExecutablePath = parsedOptions.ExecutablePath,
            HasExplicitExecutableOverride = parsedOptions.HasExplicitExecutableOverride,
            WorkerScriptPath = parsedOptions.WorkerScriptPath,
            CacheDirectory = parsedOptions.CacheDirectory,
            Arguments = parsedOptions.Arguments,
            WorkingDirectory = parsedOptions.WorkingDirectory,
            IgnoreStartupFailure = false
        };

        return new DenoVolarHost(options);
    }

    private static string ResolveDenoHostBaseDirectory()
    {
        var assemblyBaseDirectory = Path.GetDirectoryName(typeof(BuildOrchestrator).Assembly.Location)
            ?? AppContext.BaseDirectory;
        if (IsUsableDenoHostBaseDirectory(assemblyBaseDirectory))
        {
            return assemblyBaseDirectory;
        }

        var projectOutputBaseDirectory = TryResolveProjectOutputBaseDirectory(assemblyBaseDirectory);
        if (projectOutputBaseDirectory is not null && IsUsableDenoHostBaseDirectory(projectOutputBaseDirectory))
        {
            return projectOutputBaseDirectory;
        }

        var workspaceProjectOutputBaseDirectory = TryResolveWorkspaceProjectOutputBaseDirectory(assemblyBaseDirectory);
        if (workspaceProjectOutputBaseDirectory is not null)
        {
            return workspaceProjectOutputBaseDirectory;
        }

        var fallbackProjectOutputBaseDirectory = TryResolveFallbackProjectOutputBaseDirectory(projectOutputBaseDirectory)
            ?? TryResolveFallbackProjectOutputBaseDirectory(workspaceProjectOutputBaseDirectory);
        return fallbackProjectOutputBaseDirectory is not null
            ? fallbackProjectOutputBaseDirectory
            : assemblyBaseDirectory;
    }

    private static string? TryResolveFallbackProjectOutputBaseDirectory(string? projectOutputBaseDirectory)
    {
        if (string.IsNullOrWhiteSpace(projectOutputBaseDirectory))
        {
            return null;
        }

        var targetFramework = Path.GetFileName(projectOutputBaseDirectory);
        var configurationDirectory = Path.GetDirectoryName(projectOutputBaseDirectory);
        var binDirectory = string.IsNullOrWhiteSpace(configurationDirectory)
            ? null
            : Path.GetDirectoryName(configurationDirectory);
        if (string.IsNullOrWhiteSpace(targetFramework)
            || string.IsNullOrWhiteSpace(binDirectory)
            || !Directory.Exists(binDirectory))
        {
            return null;
        }

        try
        {
            return Directory.EnumerateDirectories(binDirectory)
                .Select(configurationPath => Path.Combine(configurationPath, targetFramework))
                .Where(candidate => !string.Equals(
                    Path.GetFullPath(candidate),
                    Path.GetFullPath(projectOutputBaseDirectory),
                    FilePathComparison))
                .FirstOrDefault(IsUsableDenoHostBaseDirectory);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static string? TryResolveProjectOutputBaseDirectory(string assemblyBaseDirectory)
    {
        var targetFramework = Path.GetFileName(assemblyBaseDirectory);
        var configurationDirectory = Path.GetDirectoryName(assemblyBaseDirectory);
        if (string.IsNullOrWhiteSpace(targetFramework) || string.IsNullOrWhiteSpace(configurationDirectory))
        {
            return null;
        }

        var configuration = Path.GetFileName(configurationDirectory);
        if (string.IsNullOrWhiteSpace(configuration))
        {
            return null;
        }

        var candidate = Path.GetFullPath(Path.Combine(
            assemblyBaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "Jolt",
            "bin",
            configuration,
            targetFramework));
        return Directory.Exists(candidate)
            ? candidate
            : null;
    }

    private static string? TryResolveWorkspaceProjectOutputBaseDirectory(string assemblyBaseDirectory)
    {
        var repositoryRoot = TryResolveRepositoryRoot(assemblyBaseDirectory);
        if (string.IsNullOrWhiteSpace(repositoryRoot))
        {
            return null;
        }

        var sourceProjectBinDirectory = Path.Combine(repositoryRoot, "src", "Jolt", "bin");
        if (!Directory.Exists(sourceProjectBinDirectory))
        {
            return null;
        }

        var preferredConfiguration = TryResolveBuildConfiguration(assemblyBaseDirectory);
        try
        {
            foreach (var configurationDirectory in Directory.EnumerateDirectories(sourceProjectBinDirectory)
                .OrderByDescending(path => string.Equals(
                    Path.GetFileName(path),
                    preferredConfiguration,
                    StringComparison.OrdinalIgnoreCase))
                .ThenBy(static path => path, FilePathComparer))
            {
                foreach (var candidate in Directory.EnumerateDirectories(configurationDirectory)
                    .OrderBy(static path => path, FilePathComparer))
                {
                    if (IsUsableDenoHostBaseDirectory(candidate))
                    {
                        return candidate;
                    }
                }
            }
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }

        return null;
    }

    private static string? TryResolveRepositoryRoot(string assemblyBaseDirectory)
    {
        var currentDirectory = new DirectoryInfo(assemblyBaseDirectory);
        while (currentDirectory is not null)
        {
            var sourceProjectPath = Path.Combine(
                currentDirectory.FullName,
                "src",
                "Jolt",
                "Jolt.csproj");
            if (File.Exists(sourceProjectPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    private static string? TryResolveBuildConfiguration(string assemblyBaseDirectory)
    {
        var directoryName = Path.GetFileName(assemblyBaseDirectory);
        if (IsBuildConfigurationName(directoryName))
        {
            return directoryName;
        }

        var parentDirectoryName = Path.GetFileName(Path.GetDirectoryName(assemblyBaseDirectory));
        return IsBuildConfigurationName(parentDirectoryName)
            ? parentDirectoryName
            : null;
    }

    private static bool IsBuildConfigurationName(string? value)
        => string.Equals(value, "Debug", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "Release", StringComparison.OrdinalIgnoreCase);

    private static bool IsUsableDenoHostBaseDirectory(string baseDirectory)
    {
        var workerPath = Path.Combine(baseDirectory, "Frontend", "Deno", "Worker", "frontend-worker.ts");
        var workerDirectory = Path.GetDirectoryName(workerPath);
        var workerConfigPath = string.IsNullOrWhiteSpace(workerDirectory)
            ? null
            : Path.Combine(workerDirectory, "deno.json");
        var workerNodeModulesDirectory = string.IsNullOrWhiteSpace(workerDirectory)
            ? null
            : Path.Combine(workerDirectory, "node_modules");
        var cacheDirectory = Path.Combine(baseDirectory, "Frontend", "Deno", "Cache");
        var npmCacheDirectory = Path.Combine(cacheDirectory, "npm");
        var registryCacheDirectory = Path.Combine(npmCacheDirectory, "registry.npmjs.org");
        return File.Exists(workerPath)
            && !string.IsNullOrWhiteSpace(workerConfigPath)
            && File.Exists(workerConfigPath)
            && HasReadyDenoWorkerDependencies(workerNodeModulesDirectory, registryCacheDirectory)
            && DenoRuntimeAssetResolver.TryResolveBundledExecutablePath(baseDirectory, out _);
    }

    private static bool HasReadyDenoWorkerDependencies(
        string? workerNodeModulesDirectory,
        string registryCacheDirectory)
    {
        if (!string.IsNullOrWhiteSpace(workerNodeModulesDirectory)
            && Directory.Exists(Path.Combine(workerNodeModulesDirectory, "@volar"))
            && Directory.Exists(Path.Combine(workerNodeModulesDirectory, "@vue")))
        {
            return true;
        }

        return Directory.Exists(Path.Combine(registryCacheDirectory, "@volar"))
            && Directory.Exists(Path.Combine(registryCacheDirectory, "@vue"));
    }

    private static async Task EnsureBuildGraphCompiledAsync(
        BuildContext context,
        OnDemandCompiler compiler,
        ModuleResolver moduleResolver,
        string entryPointPath,
        CancellationToken cancellationToken)
    {
        var pendingModulePaths = new Stack<string>();
        var visitedModulePaths = new HashSet<string>(FilePathComparer);
        pendingModulePaths.Push(entryPointPath);

        while (pendingModulePaths.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var modulePath = pendingModulePaths.Pop();
            if (!visitedModulePaths.Add(modulePath)
                || !File.Exists(modulePath)
                || !IsBuildGraphCompilablePath(modulePath))
            {
                continue;
            }

            CompilationResult result;
            if (string.Equals(Path.GetExtension(modulePath), ".jazor", StringComparison.OrdinalIgnoreCase))
            {
                var sourceText = await File.ReadAllTextAsync(modulePath, cancellationToken);
                result = await compiler.CompileAsync(modulePath, sourceText, cancellationToken);
                AppendLegacyImportDiagnostics(context, modulePath, sourceText);
            }
            else
            {
                result = await compiler.CompileAsync(modulePath, cancellationToken);
            }

            foreach (var dependency in result.Dependencies)
            {
                var resolved = moduleResolver.Resolve(dependency, modulePath);
                if (!resolved.Found
                    || resolved.IsVirtual
                    || !IsBuildGraphCompilablePath(resolved.AbsolutePath))
                {
                    continue;
                }

                pendingModulePaths.Push(resolved.AbsolutePath);
            }
        }
    }

    private static bool IsBuildGraphCompilablePath(string path)
        => BuildGraphCompilableExtensions.Contains(Path.GetExtension(path));

    private static async Task AppendProjectLegacyImportDiagnosticsAsync(
        BuildContext context,
        CancellationToken cancellationToken)
    {
        foreach (var filePath in EnumerateIncrementalInputFiles(context))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(Path.GetExtension(filePath), ".jazor", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string sourceText;
            try
            {
                sourceText = await File.ReadAllTextAsync(filePath, cancellationToken);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            AppendLegacyImportDiagnostics(context, filePath, sourceText);
        }
    }

    private static bool HasErrorDiagnostics(IReadOnlyList<BuildDiagnostic> diagnostics)
        => diagnostics.Any(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    private static void AppendLegacyImportDiagnostics(
        BuildContext context,
        string modulePath,
        string sourceText)
    {
        foreach (var occurrence in LegacyImportDirectiveCatalog.FindOccurrences(sourceText))
        {
            var location = ToOneBasedLocation(sourceText, occurrence.Start);
            var message = LegacyImportDirectiveCatalog.CreateDiagnosticMessage(occurrence.Kind);
            var alreadyExists = context.Diagnostics.Any(diagnostic =>
                diagnostic.Severity == DiagnosticSeverity.Error
                && string.Equals(diagnostic.Message, message, StringComparison.Ordinal)
                && string.Equals(diagnostic.File, modulePath, FilePathComparison)
                && diagnostic.Location.HasValue
                && diagnostic.Location.Value.Line == location.Line
                && diagnostic.Location.Value.Column == location.Column);
            if (alreadyExists)
            {
                continue;
            }

            context.Diagnostics.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = message,
                File = modulePath,
                Location = location
            });
        }
    }

    private static (int Line, int Column) ToOneBasedLocation(string sourceText, int offset)
    {
        var boundedOffset = Math.Clamp(offset, 0, sourceText.Length);
        var line = 1;
        var column = 1;
        for (var index = 0; index < boundedOffset; index++)
        {
            var character = sourceText[index];
            if (character == '\n')
            {
                line++;
                column = 1;
                continue;
            }

            if (character != '\r')
            {
                column++;
            }
        }

        return (line, column);
    }

    internal static IReadOnlyDictionary<string, string> CollectIncrementalInputSignatures(BuildContext context)
    {
        var inputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in EnumerateIncrementalInputFiles(context))
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var relativePath = Path.GetRelativePath(context.RootDirectory, filePath).Replace('\\', '/');
                var signature = fileInfo.Length.ToString(CultureInfo.InvariantCulture)
                    + "|"
                    + fileInfo.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture);
                inputs[relativePath] = signature;
            }
            catch (IOException)
            {
                // Skip transiently inaccessible files. A subsequent build run will re-evaluate.
            }
            catch (UnauthorizedAccessException)
            {
                // Skip inaccessible files. Fingerprint remains stable for accessible inputs.
            }
        }

        return inputs;
    }

    internal static string ComputeIncrementalFingerprint(
        BuildOptions options,
        IReadOnlyDictionary<string, string> incrementalInputs)
    {
        var fingerprintBuilder = new StringBuilder();
        fingerprintBuilder.Append(BuildIncrementalOptionsFingerprint(options));
        foreach (var (path, signature) in incrementalInputs
                     .OrderBy(static item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            fingerprintBuilder
                .Append(path)
                .Append('|')
                .Append(signature)
                .AppendLine();
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintBuilder.ToString())));
    }

    private static IReadOnlyList<string> GetIncrementalChangedPaths(
        IReadOnlyDictionary<string, string> previousInputs,
        IReadOnlyDictionary<string, string> currentInputs)
    {
        var changedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (path, previousSignature) in previousInputs)
        {
            if (!currentInputs.TryGetValue(path, out var currentSignature)
                || !string.Equals(previousSignature, currentSignature, StringComparison.Ordinal))
            {
                changedPaths.Add(path);
            }
        }

        foreach (var path in currentInputs.Keys)
        {
            if (!previousInputs.ContainsKey(path))
            {
                changedPaths.Add(path);
            }
        }

        return changedPaths
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string BuildIncrementalOptionsFingerprint(BuildOptions options)
    {
        var builder = new StringBuilder();
        builder
            .Append("outDir=").Append(options.OutDir).AppendLine()
            .Append("sourceMap=").Append(options.SourceMap).AppendLine()
            .Append("minify=").Append(options.Minify.ToString(CultureInfo.InvariantCulture)).AppendLine()
            .Append("target=").Append(options.Target).AppendLine()
            .Append("codeSplitting=").Append(options.CodeSplitting.ToString(CultureInfo.InvariantCulture)).AppendLine()
            .Append("assetsDir=").Append(options.AssetsDir).AppendLine()
            .Append("assetHashLength=").Append(options.AssetHashLength.ToString(CultureInfo.InvariantCulture)).AppendLine()
            .Append("chunkSizeWarningLimit=").Append(options.ChunkSizeWarningLimit.ToString(CultureInfo.InvariantCulture)).AppendLine();
        foreach (var (alias, target) in options.ResolveAliases
                     .OrderBy(static item => item.Key, StringComparer.Ordinal)
                     .ThenBy(static item => item.Value, StringComparer.Ordinal))
        {
            builder
                .Append("alias:")
                .Append(alias)
                .Append('=')
                .Append(target)
                .AppendLine();
        }

        return builder.ToString();
    }

    private static IEnumerable<string> EnumerateIncrementalInputFiles(BuildContext context)
    {
        var rootDirectory = Path.GetFullPath(context.RootDirectory);
        if (!Directory.Exists(rootDirectory))
        {
            yield break;
        }

        var outDirectory = Path.GetFullPath(context.OutDirectory);
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(rootDirectory);

        while (pendingDirectories.Count > 0)
        {
            var directory = pendingDirectories.Pop();
            if (string.Equals(directory, outDirectory, FilePathComparison))
            {
                continue;
            }

            var directoryName = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!string.Equals(directory, rootDirectory, FilePathComparison)
                && IsIgnoredIncrementalDirectory(directoryName))
            {
                continue;
            }

            IEnumerable<string> childDirectories;
            try
            {
                childDirectories = Directory.EnumerateDirectories(directory);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var childDirectory in childDirectories)
            {
                pendingDirectories.Push(Path.GetFullPath(childDirectory));
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(directory);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var filePath in files)
            {
                if (ShouldIncludeIncrementalInputFile(rootDirectory, outDirectory, filePath))
                {
                    yield return filePath;
                }
            }
        }
    }

    private static bool IsIgnoredIncrementalDirectory(string? directoryName)
        => string.Equals(directoryName, ".git", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".jazor", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "node_modules", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".vs", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".idea", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "bin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "obj", StringComparison.OrdinalIgnoreCase);

    private static bool ShouldIncludeIncrementalInputFile(
        string rootDirectory,
        string outDirectory,
        string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        if (string.Equals(Path.GetDirectoryName(fullPath), outDirectory, FilePathComparison))
        {
            return false;
        }

        var relativePath = Path.GetRelativePath(rootDirectory, fullPath).Replace('\\', '/');
        if (relativePath.StartsWith("public/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var fileName = Path.GetFileName(fullPath);
        if (string.Equals(fileName, "index.html", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "package.json", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fileName, "jazor.config.json", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return IncrementalFingerprintExtensions.Contains(Path.GetExtension(fullPath));
    }

    private static bool TryReadIncrementalState(
        BuildContext context,
        [NotNullWhen(true)] out BuildIncrementalState? state)
    {
        state = null;
        var statePath = Path.Combine(context.OutDirectory, IncrementalStateFileName);
        if (!File.Exists(statePath))
        {
            return false;
        }

        try
        {
            var json = File.ReadAllText(statePath);
            var deserialized = JsonSerializer.Deserialize<BuildIncrementalState>(json);
            if (deserialized is null
                || string.IsNullOrWhiteSpace(deserialized.Fingerprint)
                || string.IsNullOrWhiteSpace(deserialized.ManifestPath))
            {
                return false;
            }

            state = deserialized;
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    private static bool AreIncrementalOutputsAvailable(
        BuildContext context,
        BuildIncrementalState state)
    {
        if (!File.Exists(ResolveAbsolutePath(context.RootDirectory, state.ManifestPath)))
        {
            return false;
        }

        foreach (var chunk in state.Chunks)
        {
            if (string.IsNullOrWhiteSpace(chunk.FilePath)
                || !File.Exists(ResolveAbsolutePath(context.RootDirectory, chunk.FilePath)))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(chunk.SourceMapPath)
                && !File.Exists(ResolveAbsolutePath(context.RootDirectory, chunk.SourceMapPath!)))
            {
                return false;
            }
        }

        foreach (var asset in state.CssAssets.Concat(state.StaticAssets))
        {
            if (string.IsNullOrWhiteSpace(asset.FilePath)
                || !File.Exists(ResolveAbsolutePath(context.RootDirectory, asset.FilePath)))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(asset.SourceMapPath)
                && !File.Exists(ResolveAbsolutePath(context.RootDirectory, asset.SourceMapPath!)))
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<BuildResult?> TryBuildHtmlRefreshIncrementalResultAsync(
        BuildContext context,
        BuildOptions options,
        BuildIncrementalState state,
        IReadOnlyDictionary<string, string> incrementalInputs,
        string incrementalFingerprint,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(state.EntryRequestPath))
        {
            return null;
        }

        var changedPaths = GetIncrementalChangedPaths(state.Inputs, incrementalInputs);
        if (changedPaths.Count != 1
            || !string.Equals(changedPaths[0], "index.html", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string entryPointPath;
        try
        {
            entryPointPath = BuildEntryPointResolver.ResolveEntryPoint(options.RootDirectory);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (PathTooLongException)
        {
            return null;
        }

        var currentEntryRequestPath = ResolveEntryRequestPath(options.RootDirectory, entryPointPath);
        if (!string.Equals(currentEntryRequestPath, state.EntryRequestPath, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        await GenerateHtmlAsync(
            context,
            state.Chunks,
            state.CssAssets,
            state.StaticAssets,
            currentEntryRequestPath,
            cancellationToken);
        var manifestPath = await WriteManifestAsync(
            context,
            state.Chunks,
            state.CssAssets,
            state.StaticAssets,
            state.TotalSize,
            cancellationToken);

        var result = new BuildResult
        {
            Success = true,
            OutDirectory = context.OutDirectory,
            ManifestPath = manifestPath,
            Chunks = state.Chunks,
            CssAssets = state.CssAssets,
            StaticAssets = state.StaticAssets,
            Diagnostics =
            [
                .. context.Diagnostics,
                new BuildDiagnostic
                {
                    Severity = DiagnosticSeverity.Info,
                    Message = IncrementalHtmlRefreshMessage
                }
            ],
            TotalSize = state.TotalSize
        };

        await PersistIncrementalStateAsync(
            context,
            result,
            incrementalFingerprint,
            incrementalInputs,
            currentEntryRequestPath,
            cancellationToken);
        return result;
    }

    internal static async Task PersistIncrementalStateAsync(
        BuildContext context,
        BuildResult buildResult,
        string fingerprint,
        IReadOnlyDictionary<string, string> incrementalInputs,
        string entryRequestPath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(buildResult.ManifestPath))
        {
            return;
        }

        var state = new BuildIncrementalState
        {
            Fingerprint = fingerprint,
            ManifestPath = ResolveRootRelativePath(context.RootDirectory, buildResult.ManifestPath),
            EntryRequestPath = entryRequestPath,
            Inputs = incrementalInputs,
            Chunks = buildResult.Chunks,
            CssAssets = buildResult.CssAssets,
            StaticAssets = buildResult.StaticAssets,
            TotalSize = buildResult.TotalSize
        };
        var statePath = Path.Combine(context.OutDirectory, IncrementalStateFileName);
        var stateJson = JsonSerializer.Serialize(
            state,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
        await File.WriteAllTextAsync(statePath, stateJson, cancellationToken);
    }

    private static string ResolveRootRelativePath(string rootDirectory, string absoluteOrRelativePath)
    {
        if (string.IsNullOrWhiteSpace(absoluteOrRelativePath))
        {
            return string.Empty;
        }

        if (!Path.IsPathRooted(absoluteOrRelativePath))
        {
            return absoluteOrRelativePath.Replace('\\', '/');
        }

        var fullRootPath = Path.GetFullPath(rootDirectory);
        var fullPath = Path.GetFullPath(absoluteOrRelativePath);
        return IsInsideRoot(fullRootPath, fullPath)
            ? Path.GetRelativePath(fullRootPath, fullPath).Replace('\\', '/')
            : fullPath.Replace('\\', '/');
    }

    private static string ResolveAbsolutePath(string rootDirectory, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        return Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(
                rootDirectory,
                path.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static void PrepareOutputDirectory(BuildContext context)
    {
        var rootDirectory = Path.GetFullPath(context.RootDirectory);
        var outDirectory = Path.GetFullPath(context.OutDirectory);
        if (!IsInsideRoot(rootDirectory, outDirectory)
            || string.Equals(rootDirectory, outDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Resolved build output directory '{outDirectory}' must stay inside project root '{rootDirectory}' and cannot point at the project root itself.");
        }

        if (Directory.Exists(outDirectory))
        {
            Directory.Delete(outDirectory, recursive: true);
        }

        Directory.CreateDirectory(outDirectory);
    }

    private static bool IsInsideRoot(string rootDirectory, string candidatePath)
    {
        var relativePath = Path.GetRelativePath(rootDirectory, candidatePath);
        return string.Equals(relativePath, ".", StringComparison.Ordinal)
            || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath));
    }

    private static string ResolveEntryRequestPath(string rootDirectory, string entryPointPath)
        => "/" + Path.GetRelativePath(rootDirectory, entryPointPath).Replace('\\', '/');

    private static string ToHtmlPath(BuildContext context, string rootRelativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootRelativePath);

        var absolutePath = Path.GetFullPath(Path.Combine(
            context.RootDirectory,
            rootRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var relativePath = Path.GetRelativePath(context.OutDirectory, absolutePath).Replace('\\', '/');
        return relativePath.StartsWith("./", StringComparison.Ordinal)
            ? relativePath[2..]
            : relativePath;
    }

    private static long GetAssetSize(BuildContext context, string rootRelativePath)
    {
        var absolutePath = Path.Combine(
            context.RootDirectory,
            rootRelativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(absolutePath)
            ? new FileInfo(absolutePath).Length
            : 0;
    }

    private static long GetOptionalFileSize(BuildContext context, string? rootRelativePath)
        => string.IsNullOrWhiteSpace(rootRelativePath)
            ? 0
            : GetAssetSize(context, rootRelativePath);

}

