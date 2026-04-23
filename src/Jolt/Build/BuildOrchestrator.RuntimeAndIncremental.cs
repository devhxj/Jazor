using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jazor.Vue;
using Jolt.DevServer;
using Jolt.Volar.Deno.Hosting;

namespace Jolt.Build;

internal sealed partial class BuildOrchestrator
{
    internal sealed record IncrementalInputSnapshot(
        IReadOnlyDictionary<string, string> Inputs,
        bool HasReadFailure);

    private static readonly Lazy<string> CachedDenoHostBaseDirectory = new(ResolveDenoHostBaseDirectoryCore);

    /// <summary>
    /// Creates a DenoVolarHost for the build pipeline.
    /// </summary>
    private static DenoVolarHost CreateDenoHost(string rootDirectory)
    {
        var baseDirectory = CachedDenoHostBaseDirectory.Value;
        var parsedOptions = DenoVolarHostOptionsParser.Parse(
            ["--deno-worker", $"--dev-root={rootDirectory}"],
            baseDirectory);
        var options = new DenoVolarHostOptions
        {
            Enabled = parsedOptions.Enabled,
            ExecutablePath = parsedOptions.ExecutablePath,
            HasExplicitExecutableOverride = parsedOptions.HasExplicitExecutableOverride,
            WorkerScriptPath = parsedOptions.WorkerScriptPath,
            CacheDirectory = parsedOptions.CacheDirectory,
            Arguments = parsedOptions.Arguments,
            WorkingDirectory = parsedOptions.WorkingDirectory,
            IgnoreStartupFailure = false,
            RequestTimeout = parsedOptions.RequestTimeout
        };

        return new DenoVolarHost(options);
    }

    private static string ResolveDenoHostBaseDirectoryCore()
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
        var workerPath = Path.Combine(baseDirectory, "Volar", "Deno", "Worker", "volar-worker.ts");
        var workerDirectory = Path.GetDirectoryName(workerPath);
        var workerConfigPath = string.IsNullOrWhiteSpace(workerDirectory)
            ? null
            : Path.Combine(workerDirectory, "deno.json");
        return File.Exists(workerPath)
            && !string.IsNullOrWhiteSpace(workerConfigPath)
            && File.Exists(workerConfigPath)
            && DenoRuntimeAssetResolver.TryResolveBundledExecutablePath(baseDirectory, out _);
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

            // 编译图遍历也必须重新校验模块落点，不能因为依赖解析阶段给出了路径，
            // 就跳过生产构建对项目输入边界的最终确认。
            if (!TryResolveTrustedProjectInputFilePath(context.RootDirectory, modulePath, out var trustedModulePath))
            {
                continue;
            }

            CompilationResult result;
            try
            {
                if (string.Equals(Path.GetExtension(trustedModulePath), ".jazor", StringComparison.OrdinalIgnoreCase))
                {
                    var sourceText = await File.ReadAllTextAsync(trustedModulePath, cancellationToken);
                    result = await compiler.CompileAsync(trustedModulePath, sourceText, cancellationToken);
                    AppendLegacyImportDiagnostics(context, trustedModulePath, sourceText);
                }
                else
                {
                    result = await compiler.CompileAsync(trustedModulePath, cancellationToken);
                }
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var dependency in result.Dependencies)
            {
                var resolved = moduleResolver.Resolve(dependency, trustedModulePath);
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
        IReadOnlyList<string> incrementalInputFiles,
        CancellationToken cancellationToken)
    {
        foreach (var filePath in incrementalInputFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(Path.GetExtension(filePath), ".jazor", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string sourceText;
            string diagnosticFilePath;
            try
            {
                if (!TryResolveTrustedProjectInputFilePath(context.RootDirectory, filePath, out var trustedFilePath))
                {
                    continue;
                }

                diagnosticFilePath = trustedFilePath;
                sourceText = await File.ReadAllTextAsync(trustedFilePath, cancellationToken);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            AppendLegacyImportDiagnostics(context, diagnosticFilePath, sourceText);
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
        => CollectIncrementalInputSnapshot(context).Inputs;

    internal static IReadOnlyDictionary<string, string> CollectIncrementalInputSignatures(
        BuildContext context,
        IReadOnlyList<string> incrementalInputFiles)
        => CollectIncrementalInputSnapshot(context, incrementalInputFiles).Inputs;

    internal static IncrementalInputSnapshot CollectIncrementalInputSnapshot(BuildContext context)
        => CollectIncrementalInputSnapshot(context, CollectIncrementalInputFiles(context));

    internal static IncrementalInputSnapshot CollectIncrementalInputSnapshot(
        BuildContext context,
        IReadOnlyList<string> incrementalInputFiles)
    {
        var inputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var hasReadFailure = false;
        foreach (var filePath in incrementalInputFiles)
        {
            try
            {
                if (!TryResolveTrustedProjectInputFilePath(context.RootDirectory, filePath, out var trustedFilePath))
                {
                    hasReadFailure = true;
                    // 只要增量快照里混入工作区外/不可信输入，就必须降级为不完整，
                    // 否则生产构建可能错误复用旧缓存。
                    continue;
                }

                var fileInfo = new FileInfo(trustedFilePath);
                var relativePath = Path.GetRelativePath(context.RootDirectory, trustedFilePath).Replace('\\', '/');
                inputs[relativePath] = ComputeIncrementalInputSignature(fileInfo);
            }
            catch (IOException)
            {
                hasReadFailure = true;
                // 文件被短暂占用时直接降级为不完整快照，下一次构建再重新评估。
            }
            catch (UnauthorizedAccessException)
            {
                hasReadFailure = true;
                // 不可访问输入同样不能参与缓存命中，避免用“部分可见”的输入集复用旧产物。
            }
        }

        return new IncrementalInputSnapshot(inputs, hasReadFailure);
    }

    private static string ComputeIncrementalInputSignature(FileInfo fileInfo)
    {
        // 不能只依赖长度和 mtime：某些同步工具/编辑器会保留元数据，
        // 如果缺少内容哈希，生产构建可能错误复用旧产物。
        using var stream = new FileStream(
            fileInfo.FullName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 64 * 1024,
            FileOptions.SequentialScan);
        using var sha256 = SHA256.Create();
        var contentHash = Convert.ToHexString(sha256.ComputeHash(stream));
        return fileInfo.Length.ToString(CultureInfo.InvariantCulture)
            + "|"
            + fileInfo.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture)
            + "|"
            + contentHash;
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

    private static IReadOnlyList<string> CollectIncrementalInputFiles(BuildContext context)
        => EnumerateIncrementalInputFiles(context).ToArray();

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

            string[] childDirectories;
            try
            {
                childDirectories = Directory.EnumerateDirectories(directory).ToArray();
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
                var fullChildDirectory = Path.GetFullPath(childDirectory);
                if (!ShouldTraverseIncrementalDirectory(rootDirectory, fullChildDirectory))
                {
                    continue;
                }

                pendingDirectories.Push(fullChildDirectory);
            }

            string[] files;
            try
            {
                files = Directory.EnumerateFiles(directory).ToArray();
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
        // 这些目录属于工具缓存、测试结果或构建产物，不应该参与业务输入指纹。
        => string.Equals(directoryName, ".git", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".jazor", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".omx", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".omc", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".dotnet", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "node_modules", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".vs", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".idea", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "TestResults", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".test-results", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".tmp", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, ".verify", StringComparison.OrdinalIgnoreCase)
            || directoryName?.StartsWith(".artifacts", StringComparison.OrdinalIgnoreCase) == true
            || directoryName?.StartsWith(".dotnet-out", StringComparison.OrdinalIgnoreCase) == true
            || directoryName?.StartsWith(".dotnet-obj", StringComparison.OrdinalIgnoreCase) == true
            || string.Equals(directoryName, "bin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(directoryName, "obj", StringComparison.OrdinalIgnoreCase);

    internal static bool ShouldTraverseIncrementalDirectory(
        string rootDirectory,
        string candidateDirectory,
        FileAttributes attributes)
    {
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullCandidateDirectory = Path.GetFullPath(candidateDirectory);
        if (!IsInsideRoot(fullRootDirectory, fullCandidateDirectory))
        {
            return false;
        }

        // 不跟随 reparse point，避免把仓库外目录或循环链接带进生产构建扫描。
        return (attributes & FileAttributes.ReparsePoint) == 0;
    }

    private static bool ShouldTraverseIncrementalDirectory(
        string rootDirectory,
        string candidateDirectory)
    {
        try
        {
            return ShouldTraverseIncrementalDirectory(
                rootDirectory,
                candidateDirectory,
                File.GetAttributes(candidateDirectory));
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static bool ShouldIncludeIncrementalInputFile(
        string rootDirectory,
        string outDirectory,
        string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (IsInsideRoot(outDirectory, fullPath))
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
            || string.Equals(fileName, JoltConfigFile.FileName, StringComparison.OrdinalIgnoreCase))
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
        string statePath;
        try
        {
            statePath = EnsureTrustedBuildOutputPath(
                context.RootDirectory,
                context.OutDirectory,
                Path.Combine(context.OutDirectory, IncrementalStateFileName),
                allowMissingLeaf: true);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

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
        if (!TryResolveTrustedIncrementalOutputPath(context, state.ManifestPath, out _))
        {
            return false;
        }

        foreach (var chunk in state.Chunks)
        {
            if (string.IsNullOrWhiteSpace(chunk.FilePath)
                || !TryResolveTrustedIncrementalOutputPath(context, chunk.FilePath, out _))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(chunk.SourceMapPath)
                && !TryResolveTrustedIncrementalOutputPath(context, chunk.SourceMapPath!, out _))
            {
                return false;
            }
        }

        foreach (var asset in state.CssAssets.Concat(state.StaticAssets))
        {
            if (string.IsNullOrWhiteSpace(asset.FilePath)
                || !TryResolveTrustedIncrementalOutputPath(context, asset.FilePath, out _))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(asset.SourceMapPath)
                && !TryResolveTrustedIncrementalOutputPath(context, asset.SourceMapPath!, out _))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryResolveTrustedIncrementalOutputPath(
        BuildContext context,
        string path,
        [NotNullWhen(true)] out string? absolutePath)
    {
        try
        {
            // 增量状态文件属于内部缓存，命中前必须确认路径仍然落在当前输出目录内，
            // 不能因为状态文件被篡改或污染而信任仓库外/输出目录外的文件。
            absolutePath = ResolveTrustedBuildOutputPath(context, path);
            return IsReadableFilePresent(absolutePath);
        }
        catch (ArgumentException)
        {
            absolutePath = null;
            return false;
        }
        catch (InvalidOperationException)
        {
            absolutePath = null;
            return false;
        }
        catch (NotSupportedException)
        {
            absolutePath = null;
            return false;
        }
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
        var statePath = EnsureTrustedBuildOutputPath(
            context.RootDirectory,
            context.OutDirectory,
            Path.Combine(context.OutDirectory, IncrementalStateFileName),
            allowMissingLeaf: true);
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

        var trustedOutDirectory = EnsureTrustedBuildOutputPath(
            rootDirectory,
            rootDirectory,
            outDirectory,
            allowMissingLeaf: true);
        // OutDir 指向现有普通文件时不能尝试清理或覆写；直接给出明确配置错误。
        if (File.Exists(trustedOutDirectory))
        {
            throw new InvalidOperationException(
                $"Resolved build output path '{trustedOutDirectory}' is an existing file. Configure OutDir to a directory inside project root.");
        }

        // 只允许清理已经通过信任边界校验的输出目录，避免把“准备输出目录”
        // 变成对项目根内任意路径的递归删除入口。
        DeleteTrustedBuildOutputDirectory(trustedOutDirectory);

        Directory.CreateDirectory(trustedOutDirectory);
    }

    internal static void DeleteTrustedBuildOutputDirectory(string trustedOutDirectory)
    {
        if (!Directory.Exists(trustedOutDirectory))
        {
            return;
        }

        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Directory.Delete(trustedOutDirectory, recursive: true);
                return;
            }
            catch (DirectoryNotFoundException)
            {
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                // 输出目录可能刚被 bundler、杀毒或索引器释放，短暂退避比立即失败更适合生产构建。
                Thread.Sleep(100 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                // Windows 上权限/句柄状态可能短暂滞后，重试后仍失败再交给最终异常。
                Thread.Sleep(100 * attempt);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException(
                    $"Build output directory '{trustedOutDirectory}' could not be cleaned after {maxAttempts} attempts.",
                    ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException(
                    $"Build output directory '{trustedOutDirectory}' could not be cleaned after {maxAttempts} attempts.",
                    ex);
            }
        }
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

    internal static bool IsTrustedProjectInputPath(
        string rootDirectory,
        string candidatePath,
        FileAttributes attributes)
    {
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullCandidatePath = Path.GetFullPath(candidatePath);
        return IsInsideRoot(fullRootDirectory, fullCandidatePath)
            && (attributes & FileAttributes.ReparsePoint) == 0;
    }

    private static bool TryResolveTrustedProjectInputFilePath(
        string rootDirectory,
        string candidatePath,
        [NotNullWhen(true)] out string? trustedPath)
    {
        trustedPath = null;
        string fullRootDirectory;
        string fullCandidatePath;
        try
        {
            fullRootDirectory = Path.GetFullPath(rootDirectory);
            fullCandidatePath = Path.GetFullPath(candidatePath);
            if (!IsInsideRoot(fullRootDirectory, fullCandidatePath) || !File.Exists(fullCandidatePath))
            {
                return false;
            }
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        var inspectionPath = fullCandidatePath;
        while (!string.IsNullOrWhiteSpace(inspectionPath))
        {
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(inspectionPath);
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            // 构建输入文件也不跟随 reparse point，避免把工作区外源码伪装成本地输入。
            if (!IsTrustedProjectInputPath(fullRootDirectory, inspectionPath, attributes))
            {
                return false;
            }

            if (string.Equals(inspectionPath, fullRootDirectory, FilePathComparison))
            {
                trustedPath = fullCandidatePath;
                return true;
            }

            inspectionPath = GetContainingDirectoryPath(inspectionPath);
        }

        return false;
    }

    internal static bool IsTrustedBuildOutputPath(
        string rootDirectory,
        string candidatePath,
        FileAttributes attributes)
    {
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullCandidatePath = Path.GetFullPath(candidatePath);
        return IsInsideRoot(fullRootDirectory, fullCandidatePath)
            && (attributes & FileAttributes.ReparsePoint) == 0;
    }

    private static string ResolveTrustedBuildOutputPath(
        BuildContext context,
        string rootRelativePath,
        bool requireInsideAssetsDirectory = false,
        bool allowMissingLeaf = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootRelativePath);

        var absolutePath = Path.GetFullPath(Path.Combine(
            context.RootDirectory,
            rootRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        return EnsureTrustedBuildOutputPath(
            context.RootDirectory,
            requireInsideAssetsDirectory ? context.AssetsDirectory : context.OutDirectory,
            absolutePath,
            allowMissingLeaf);
    }

    private static string EnsureTrustedBuildOutputPath(
        string rootDirectory,
        string boundaryDirectory,
        string candidatePath,
        bool allowMissingLeaf = false)
    {
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullBoundaryDirectory = Path.GetFullPath(boundaryDirectory);
        var fullCandidatePath = Path.GetFullPath(candidatePath);
        if (!IsInsideRoot(fullRootDirectory, fullBoundaryDirectory))
        {
            throw new InvalidOperationException(
                $"Resolved build output boundary '{fullBoundaryDirectory}' must stay inside project root '{fullRootDirectory}'.");
        }

        if (!IsInsideRoot(fullBoundaryDirectory, fullCandidatePath))
        {
            throw new InvalidOperationException(
                $"Resolved build output '{fullCandidatePath}' must stay inside trusted output boundary '{fullBoundaryDirectory}'.");
        }

        var inspectionPath = GetExistingBuildOutputTrustInspectionPath(fullCandidatePath, allowMissingLeaf);
        while (!string.IsNullOrWhiteSpace(inspectionPath))
        {
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(inspectionPath);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new InvalidOperationException(
                    $"Build output path '{fullCandidatePath}' became unavailable while validating '{inspectionPath}'.",
                    ex);
            }
            catch (FileNotFoundException ex)
            {
                throw new InvalidOperationException(
                    $"Build output path '{fullCandidatePath}' became unavailable while validating '{inspectionPath}'.",
                    ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException(
                    $"Build output path '{fullCandidatePath}' could not be validated because '{inspectionPath}' is not readable.",
                    ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException(
                    $"Build output path '{fullCandidatePath}' could not be validated because '{inspectionPath}' is not accessible.",
                    ex);
            }

            // 生产构建只接受“仍在项目根目录内，且链路上不存在 reparse point”的输出路径。
            if (!IsTrustedBuildOutputPath(fullRootDirectory, inspectionPath, attributes))
            {
                throw new InvalidOperationException(
                    $"Build output path '{fullCandidatePath}' traverses an untrusted reparse point inside project root '{fullRootDirectory}'.");
            }

            if (string.Equals(inspectionPath, fullRootDirectory, FilePathComparison))
            {
                return fullCandidatePath;
            }

            inspectionPath = GetContainingDirectoryPath(inspectionPath);
        }

        throw new InvalidOperationException(
            $"Build output path '{fullCandidatePath}' could not be validated within project root '{fullRootDirectory}'.");
    }

    private static string GetExistingBuildOutputTrustInspectionPath(
        string candidatePath,
        bool allowMissingLeaf)
    {
        if (File.Exists(candidatePath) || Directory.Exists(candidatePath))
        {
            return candidatePath;
        }

        if (!allowMissingLeaf)
        {
            throw new FileNotFoundException($"Build output '{candidatePath}' was not found.", candidatePath);
        }

        return GetContainingDirectoryPath(candidatePath);
    }

    private static string ToHtmlPath(BuildContext context, string rootRelativePath)
    {
        var absolutePath = ResolveTrustedBuildOutputPath(context, rootRelativePath);
        var relativePath = Path.GetRelativePath(context.OutDirectory, absolutePath).Replace('\\', '/');
        return relativePath.StartsWith("./", StringComparison.Ordinal)
            ? relativePath[2..]
            : relativePath;
    }

    private static long GetAssetSize(BuildContext context, string rootRelativePath)
    {
        try
        {
            var absolutePath = ResolveTrustedBuildOutputPath(context, rootRelativePath);
            return File.Exists(absolutePath)
                ? new FileInfo(absolutePath).Length
                : 0;
        }
        catch (ArgumentException)
        {
            return 0;
        }
        catch (IOException)
        {
            return 0;
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
        catch (NotSupportedException)
        {
            return 0;
        }
        catch (UnauthorizedAccessException)
        {
            return 0;
        }
    }

    private static long GetOptionalFileSize(BuildContext context, string? rootRelativePath)
        => string.IsNullOrWhiteSpace(rootRelativePath)
            ? 0
            : GetAssetSize(context, rootRelativePath);

    private static bool IsReadableFilePresent(string path)
    {
        try
        {
            return File.Exists(path);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

}
