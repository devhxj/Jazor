using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jolt.Frontend.Deno.Hosting;
using Jolt.Hosting;

namespace Jolt.Build;

internal sealed class DenoBundleRunner
{
    private static readonly Regex CssSourceMapCommentPattern = new(
        @"/\*#\s*sourceMappingURL=(?<value>[^*]+?)\s*\*/",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    private readonly BuildContext _context;

    public DenoBundleRunner(BuildContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<DenoBundleResult> RunAsync(
        Uri entryUri,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entryUri);
        cancellationToken.ThrowIfCancellationRequested();

        var denoExecutablePath = DenoRuntimeAssetResolver.ResolveBundledExecutablePath();
        if (!File.Exists(denoExecutablePath))
        {
            return Failure($"Bundled Deno runtime was not found at '{denoExecutablePath}'.");
        }

        var diagnostics = new List<BuildDiagnostic>();
        var useCodeSplitting = _context.Options.CodeSplitting;

        var assetsDirectory = _context.AssetsDirectory;
        Directory.CreateDirectory(assetsDirectory);

        await using var bundlerProxy = await BundlerModuleProxyServer.StartAsync(entryUri, cancellationToken);
        var bundlerEntryUri = bundlerProxy.CreateBundlerEntryUri(entryUri);
        var importMapPath = await DenoBuildImportMapGenerator.GenerateAsync(_context.RootDirectory, cancellationToken);
        var provisionalOutputPath = Path.Combine(assetsDirectory, "index.js");

        var startInfo = new ProcessStartInfo
        {
            FileName = denoExecutablePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardErrorEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8,
            WorkingDirectory = _context.RootDirectory
        };

        startInfo.ArgumentList.Add("bundle");
        startInfo.ArgumentList.Add("--platform");
        startInfo.ArgumentList.Add("browser");
        startInfo.ArgumentList.Add("--format");
        startInfo.ArgumentList.Add("esm");
        startInfo.ArgumentList.Add("--conditions");
        startInfo.ArgumentList.Add("production");
        startInfo.ArgumentList.Add("--quiet");
        startInfo.ArgumentList.Add("--no-config");
        startInfo.ArgumentList.Add("--import-map");
        startInfo.ArgumentList.Add(importMapPath);
        startInfo.ArgumentList.Add($"--allow-import={bundlerEntryUri.Host}:{bundlerEntryUri.Port}");
        if (_context.Options.Minify)
        {
            startInfo.ArgumentList.Add("--minify");
        }

        if (useCodeSplitting)
        {
            startInfo.ArgumentList.Add("--code-splitting");
        }

        var sourceMapArgument = MapSourceMapOption(_context.Options.SourceMap);
        if (sourceMapArgument is not null)
        {
            startInfo.ArgumentList.Add($"--sourcemap={sourceMapArgument}");
        }

        if (useCodeSplitting)
        {
            startInfo.ArgumentList.Add("--outdir");
            startInfo.ArgumentList.Add(assetsDirectory);
        }
        else
        {
            startInfo.ArgumentList.Add("--output");
            startInfo.ArgumentList.Add(provisionalOutputPath);
        }

        startInfo.ArgumentList.Add(bundlerEntryUri.AbsoluteUri);

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return Failure("Failed to start the bundled Deno bundler process.");
        }

        string stdout;
        string stderr;
        try
        {
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await ChildProcessUtilities.WaitForExitOrTerminateOnCancellationAsync(process, cancellationToken);
            stdout = await stdoutTask;
            stderr = await stderrTask;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await ChildProcessUtilities.TerminateProcessAsync(process);
            throw;
        }

        if (process.ExitCode != 0)
        {
            return Failure(
                $"Bundled Deno bundle failed with exit code {process.ExitCode}{(string.IsNullOrWhiteSpace(stderr) ? string.Empty : $": {stderr}")}");
        }

        if (!useCodeSplitting && !File.Exists(provisionalOutputPath))
        {
            return Failure(
                $"Bundled Deno bundle completed without producing '{provisionalOutputPath}'.{(string.IsNullOrWhiteSpace(stdout) ? string.Empty : $" Output: {stdout}")}");
        }

        if (!string.IsNullOrWhiteSpace(stderr))
        {
            diagnostics.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Info,
                Message = stderr.Trim()
            });
        }

        IReadOnlyList<ChunkInfo> chunks;
        if (useCodeSplitting)
        {
            chunks = await FinalizeCodeSplitBundleOutputAsync(assetsDirectory, entryUri, cancellationToken);
            if (chunks.Count == 0)
            {
                return Failure($"Bundled Deno code splitting completed without producing JavaScript chunks in '{assetsDirectory}'.");
            }
        }
        else
        {
            var chunk = await FinalizeSingleBundleOutputAsync(provisionalOutputPath, cancellationToken);
            chunks = [chunk];
        }

        AddChunkSizeDiagnostics(chunks, diagnostics);

        var cssAssets = await FinalizeCssAssetsAsync(assetsDirectory, cancellationToken);
        var totalSize = chunks.Sum(static chunk => chunk.Size)
            + chunks.Sum(chunk => GetOptionalFileSize(ToAbsolutePath(chunk.SourceMapPath)))
            + cssAssets.Sum(static asset => asset.Size)
            + cssAssets.Sum(asset => GetOptionalFileSize(ToAbsolutePath(asset.SourceMapPath)));

        return new DenoBundleResult
        {
            Success = true,
            Chunks = chunks,
            CssAssets = cssAssets,
            Diagnostics = diagnostics,
            TotalSize = totalSize
        };
    }

    private async Task<ChunkInfo> FinalizeSingleBundleOutputAsync(
        string provisionalOutputPath,
        CancellationToken cancellationToken)
    {
        var chunks = await FinalizeBundleOutputsAsync([provisionalOutputPath], provisionalOutputPath, cancellationToken);
        return chunks[0];
    }

    private async Task<IReadOnlyList<ChunkInfo>> FinalizeCodeSplitBundleOutputAsync(
        string assetsDirectory,
        Uri entryUri,
        CancellationToken cancellationToken)
    {
        var provisionalOutputPaths = await CollectCodeSplitOutputPathsAsync(assetsDirectory, cancellationToken);
        if (provisionalOutputPaths.Length == 0)
        {
            return [];
        }

        var provisionalEntryOutputPath = ResolveProvisionalEntryOutputPath(provisionalOutputPaths, entryUri);
        return await FinalizeBundleOutputsAsync(provisionalOutputPaths, provisionalEntryOutputPath, cancellationToken);
    }

    private static async Task<string[]> CollectCodeSplitOutputPathsAsync(
        string assetsDirectory,
        CancellationToken cancellationToken)
        => await CollectStableOutputPathsAsync(assetsDirectory, "*.js", cancellationToken);

    private static async Task<string[]> CollectStableOutputPathsAsync(
        string assetsDirectory,
        string searchPattern,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 120;
        const int maxDelayMilliseconds = 50;
        // The Deno bundler process has already exited before we poll for files here,
        // so we only need a short quiet window to avoid catching transient filesystem lag.
        const int quiescenceDurationMilliseconds = 100;

        IReadOnlyList<OutputFileSnapshot> previousSnapshot = [];
        string[] bestPaths = [];
        var bestTotalSize = -1L;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentSnapshot = CaptureOutputFileSnapshots(assetsDirectory, searchPattern);
            if (currentSnapshot.Count > 0)
            {
                var currentTotalSize = currentSnapshot.Sum(static snapshot => snapshot.Length);
                if (currentSnapshot.Count > bestPaths.Length
                    || (currentSnapshot.Count == bestPaths.Length && currentTotalSize > bestTotalSize))
                {
                    bestPaths = currentSnapshot.Select(static snapshot => snapshot.FilePath).ToArray();
                    bestTotalSize = currentTotalSize;
                }
            }

            if (currentSnapshot.Count > 0
                && AreOutputFilesReadable(currentSnapshot)
                && GetSnapshotQuietAgeMilliseconds(currentSnapshot) >= quiescenceDurationMilliseconds
                && (previousSnapshot.Count == 0 || AreOutputFileSnapshotsEqual(previousSnapshot, currentSnapshot)))
            {
                return currentSnapshot.Select(static snapshot => snapshot.FilePath).ToArray();
            }

            previousSnapshot = currentSnapshot;
            if (attempt == maxAttempts - 1)
            {
                return bestPaths;
            }

            await Task.Delay(
                GetNextSnapshotDelayMilliseconds(currentSnapshot, quiescenceDurationMilliseconds, maxDelayMilliseconds),
                cancellationToken);
        }

        return bestPaths;
    }

    private static int GetNextSnapshotDelayMilliseconds(
        IReadOnlyList<OutputFileSnapshot> snapshots,
        int quiescenceDurationMilliseconds,
        int maxDelayMilliseconds)
    {
        if (snapshots.Count == 0)
        {
            return maxDelayMilliseconds;
        }

        var remainingQuietTime = quiescenceDurationMilliseconds - GetSnapshotQuietAgeMilliseconds(snapshots);
        return Math.Clamp(remainingQuietTime, 1, maxDelayMilliseconds);
    }

    private static int GetSnapshotQuietAgeMilliseconds(IReadOnlyList<OutputFileSnapshot> snapshots)
    {
        if (snapshots.Count == 0)
        {
            return 0;
        }

        var latestWriteTimeUtcTicks = snapshots.Max(static snapshot => snapshot.LastWriteTimeUtcTicks);
        if (latestWriteTimeUtcTicks <= 0)
        {
            return 0;
        }

        var quietAge = DateTime.UtcNow - new DateTime(latestWriteTimeUtcTicks, DateTimeKind.Utc);
        return quietAge <= TimeSpan.Zero
            ? 0
            : (int)Math.Min(quietAge.TotalMilliseconds, int.MaxValue);
    }

    private static IReadOnlyList<OutputFileSnapshot> CaptureOutputFileSnapshots(
        string assetsDirectory,
        string searchPattern)
    {
        if (!Directory.Exists(assetsDirectory))
        {
            return [];
        }

        try
        {
            return Directory.EnumerateFiles(assetsDirectory, searchPattern, SearchOption.AllDirectories)
                .Select(Path.GetFullPath)
                .OrderBy(static path => path, PathComparer)
                .Select(path =>
                {
                    var fileInfo = new FileInfo(path);
                    return new OutputFileSnapshot(
                        path,
                        fileInfo.Exists ? fileInfo.Length : 0,
                        fileInfo.Exists ? fileInfo.LastWriteTimeUtc.Ticks : 0);
                })
                .ToArray();
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    private static bool AreOutputFileSnapshotsEqual(
        IReadOnlyList<OutputFileSnapshot> left,
        IReadOnlyList<OutputFileSnapshot> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Count; index++)
        {
            var leftSnapshot = left[index];
            var rightSnapshot = right[index];
            if (!string.Equals(leftSnapshot.FilePath, rightSnapshot.FilePath, PathComparison)
                || leftSnapshot.Length != rightSnapshot.Length
                || leftSnapshot.LastWriteTimeUtcTicks != rightSnapshot.LastWriteTimeUtcTicks)
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreOutputFilesReadable(IReadOnlyList<OutputFileSnapshot> snapshots)
    {
        foreach (var snapshot in snapshots)
        {
            try
            {
                using var stream = new FileStream(
                    snapshot.FilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
                if (stream.Length != snapshot.Length)
                {
                    return false;
                }
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

        return true;
    }

    private async Task<IReadOnlyList<ChunkInfo>> FinalizeBundleOutputsAsync(
        IReadOnlyList<string> provisionalOutputPaths,
        string provisionalEntryOutputPath,
        CancellationToken cancellationToken)
    {
        var bundleFiles = new List<ProvisionalBundleFile>(provisionalOutputPaths.Count);
        foreach (var provisionalOutputPath in provisionalOutputPaths.OrderBy(static path => path, PathComparer))
        {
            var jsContent = await File.ReadAllTextAsync(provisionalOutputPath, cancellationToken);
            var hashedFileName = CreateHashedFileName(provisionalOutputPath, jsContent);
            var hashedOutputPath = Path.Combine(GetContainingDirectoryPath(provisionalOutputPath), hashedFileName);
            var sourceMapPath = provisionalOutputPath + ".map";
            var hashedSourceMapPath = File.Exists(sourceMapPath)
                ? hashedOutputPath + ".map"
                : null;

            bundleFiles.Add(new ProvisionalBundleFile
            {
                OriginalPath = Path.GetFullPath(provisionalOutputPath),
                HashedPath = Path.GetFullPath(hashedOutputPath),
                HashedFileName = hashedFileName,
                OriginalContent = jsContent,
                OriginalSourceMapPath = File.Exists(sourceMapPath)
                    ? Path.GetFullPath(sourceMapPath)
                    : null,
                HashedSourceMapPath = hashedSourceMapPath is null
                    ? null
                    : Path.GetFullPath(hashedSourceMapPath),
                IsEntry = string.Equals(
                    Path.GetFullPath(provisionalOutputPath),
                    Path.GetFullPath(provisionalEntryOutputPath),
                    PathComparison)
            });
        }

        var pathMap = bundleFiles.ToDictionary(
            static file => file.OriginalPath,
            static file => file.HashedPath,
            PathComparer);

        foreach (var bundleFile in bundleFiles)
        {
            bundleFile.RewrittenContent = RewriteBundleContent(bundleFile, pathMap, out var imports);
            bundleFile.Imports = imports;
        }

        foreach (var bundleFile in bundleFiles)
        {
            await WriteFinalChunkAsync(bundleFile, cancellationToken);
        }

        foreach (var bundleFile in bundleFiles)
        {
            DeleteIfExists(bundleFile.OriginalPath);
            DeleteIfExists(bundleFile.OriginalSourceMapPath);
        }

        return bundleFiles
            .OrderByDescending(static file => file.IsEntry)
            .ThenBy(static file => file.HashedFileName, StringComparer.Ordinal)
            .Select(file => new ChunkInfo
            {
                FileName = file.HashedFileName,
                FilePath = Path.GetRelativePath(_context.RootDirectory, file.HashedPath).Replace('\\', '/'),
                Size = new FileInfo(file.HashedPath).Length,
                IsEntry = file.IsEntry,
                IsDynamic = !file.IsEntry && bundleFiles.Count > 1,
                Imports = file.Imports,
                Css = [],
                SourceMapPath = file.HashedSourceMapPath is null
                    ? null
                    : Path.GetRelativePath(_context.RootDirectory, file.HashedSourceMapPath).Replace('\\', '/')
            })
            .ToArray();
    }

    private string RewriteBundleContent(
        ProvisionalBundleFile bundleFile,
        IReadOnlyDictionary<string, string> pathMap,
        out IReadOnlyList<string> imports)
    {
        var importedChunks = new HashSet<string>(StringComparer.Ordinal);
        var currentDirectory = GetContainingDirectoryPath(bundleFile.OriginalPath);

        var rewrittenContent = JavaScriptModuleSpecifierScanner.RewriteSpecifiers(
            bundleFile.OriginalContent,
            specifier =>
            {
                var (originalSpecifier, suffix) = JavaScriptModuleSpecifierScanner.SplitPathAndSuffix(specifier.Value);
                if (!IsRelativeJavaScriptSpecifier(originalSpecifier))
                {
                    return null;
                }

                var resolvedImportPath = Path.GetFullPath(Path.Combine(
                    currentDirectory,
                    originalSpecifier.Replace('/', Path.DirectorySeparatorChar)));
                if (!pathMap.TryGetValue(resolvedImportPath, out var rewrittenImportPath))
                {
                    return null;
                }

                var rewrittenSpecifier = Path.GetRelativePath(currentDirectory, rewrittenImportPath).Replace('\\', '/');
                if (!rewrittenSpecifier.StartsWith("./", StringComparison.Ordinal)
                    && !rewrittenSpecifier.StartsWith("../", StringComparison.Ordinal))
                {
                    rewrittenSpecifier = "./" + rewrittenSpecifier;
                }

                importedChunks.Add(Path.GetRelativePath(_context.RootDirectory, rewrittenImportPath).Replace('\\', '/'));
                return rewrittenSpecifier + suffix;
            });

        if (bundleFile.OriginalSourceMapPath is not null && bundleFile.HashedSourceMapPath is not null)
        {
            rewrittenContent = rewrittenContent.Replace(
                $"//# sourceMappingURL={Path.GetFileName(bundleFile.OriginalSourceMapPath)}",
                $"//# sourceMappingURL={Path.GetFileName(bundleFile.HashedSourceMapPath)}",
                StringComparison.Ordinal);
        }

        imports = importedChunks.OrderBy(static path => path, StringComparer.Ordinal).ToArray();
        return rewrittenContent;
    }

    private static bool IsRelativeJavaScriptSpecifier(string specifier)
        => (specifier.StartsWith("./", StringComparison.Ordinal)
                || specifier.StartsWith("../", StringComparison.Ordinal))
            && specifier.EndsWith(".js", StringComparison.OrdinalIgnoreCase);

    private static async Task WriteFinalChunkAsync(
        ProvisionalBundleFile bundleFile,
        CancellationToken cancellationToken)
    {
        if (File.Exists(bundleFile.HashedPath))
        {
            File.Delete(bundleFile.HashedPath);
        }

        await File.WriteAllTextAsync(bundleFile.HashedPath, bundleFile.RewrittenContent, cancellationToken);

        if (bundleFile.OriginalSourceMapPath is not null && bundleFile.HashedSourceMapPath is not null)
        {
            await RewriteSourceMapAsync(
                bundleFile.OriginalSourceMapPath,
                bundleFile.HashedSourceMapPath,
                bundleFile.HashedFileName,
                cancellationToken);
        }
    }

    private static async Task RewriteSourceMapAsync(
        string originalSourceMapPath,
        string hashedSourceMapPath,
        string hashedFileName,
        CancellationToken cancellationToken)
    {
        using var sourceMapDocument = JsonDocument.Parse(await File.ReadAllTextAsync(originalSourceMapPath, cancellationToken));
        var sourceMapObject = JsonSerializer.Deserialize<Dictionary<string, object?>>(sourceMapDocument.RootElement.GetRawText())
            ?? new Dictionary<string, object?>(StringComparer.Ordinal);
        sourceMapObject["file"] = hashedFileName;

        if (File.Exists(hashedSourceMapPath))
        {
            File.Delete(hashedSourceMapPath);
        }

        await File.WriteAllTextAsync(hashedSourceMapPath, JsonSerializer.Serialize(sourceMapObject), cancellationToken);
    }

    private string CreateHashedFileName(string provisionalOutputPath, string content)
    {
        var hashLength = Math.Max(1, Math.Min(_context.Options.AssetHashLength, 64));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)))[..hashLength].ToLowerInvariant();
        return $"{Path.GetFileNameWithoutExtension(provisionalOutputPath)}-{hash}{Path.GetExtension(provisionalOutputPath)}";
    }

    private static string GetContainingDirectoryPath(string path)
        => Path.GetDirectoryName(path)
            ?? Path.GetPathRoot(path)
            ?? string.Empty;

    private static string ResolveProvisionalEntryOutputPath(
        IReadOnlyList<string> provisionalOutputPaths,
        Uri entryUri)
    {
        var expectedFileName = Path.GetFileNameWithoutExtension(Uri.UnescapeDataString(entryUri.AbsolutePath)) + ".js";
        var directMatch = provisionalOutputPaths.FirstOrDefault(
            path => string.Equals(Path.GetFileName(path), expectedFileName, PathComparison));
        if (directMatch is not null)
        {
            return directMatch;
        }

        var expectedStem = Path.GetFileNameWithoutExtension(expectedFileName);
        var stemMatch = provisionalOutputPaths.FirstOrDefault(
            path => string.Equals(Path.GetFileNameWithoutExtension(path), expectedStem, PathComparison));
        if (stemMatch is not null)
        {
            return stemMatch;
        }

        return provisionalOutputPaths
            .OrderBy(path => Path.GetFileName(path).Contains('-', StringComparison.Ordinal) ? 1 : 0)
            .ThenBy(static path => path, PathComparer)
            .First();
    }

    private void AddChunkSizeDiagnostics(
        IReadOnlyList<ChunkInfo> chunks,
        List<BuildDiagnostic> diagnostics)
    {
        foreach (var chunk in chunks)
        {
            if (chunk.Size <= _context.Options.ChunkSizeWarningLimit)
            {
                continue;
            }

            diagnostics.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Warning,
                Message = $"Chunk '{chunk.FileName}' is {chunk.Size} bytes, exceeding chunkSizeWarningLimit {_context.Options.ChunkSizeWarningLimit} bytes."
            });
        }
    }

    private static void DeleteIfExists(string? path)
    {
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private async Task<IReadOnlyList<AssetInfo>> FinalizeCssAssetsAsync(
        string assetsDirectory,
        CancellationToken cancellationToken)
    {
        var provisionalOutputPaths = await CollectStableOutputPathsAsync(
            assetsDirectory,
            "*.css",
            cancellationToken);
        if (provisionalOutputPaths.Length == 0)
        {
            return [];
        }

        var cssFiles = new List<ProvisionalCssFile>(provisionalOutputPaths.Length);
        foreach (var provisionalOutputPath in provisionalOutputPaths)
        {
            var cssContent = await File.ReadAllTextAsync(provisionalOutputPath, cancellationToken);
            var hashedFileName = CreateHashedFileName(provisionalOutputPath, cssContent);
            var hashedOutputPath = Path.Combine(GetContainingDirectoryPath(provisionalOutputPath), hashedFileName);
            var sourceMapPath = provisionalOutputPath + ".map";
            var hashedSourceMapPath = File.Exists(sourceMapPath)
                ? hashedOutputPath + ".map"
                : null;

            cssFiles.Add(new ProvisionalCssFile
            {
                OriginalPath = provisionalOutputPath,
                HashedPath = hashedOutputPath,
                HashedFileName = hashedFileName,
                OriginalContent = cssContent,
                OriginalSourceMapPath = File.Exists(sourceMapPath)
                    ? Path.GetFullPath(sourceMapPath)
                    : null,
                HashedSourceMapPath = hashedSourceMapPath is null
                    ? null
                    : Path.GetFullPath(hashedSourceMapPath)
            });
        }

        foreach (var cssFile in cssFiles)
        {
            cssFile.RewrittenContent = RewriteCssContent(cssFile);
            await WriteFinalCssAsync(cssFile, cancellationToken);
        }

        foreach (var cssFile in cssFiles)
        {
            DeleteIfExists(cssFile.OriginalPath);
            DeleteIfExists(cssFile.OriginalSourceMapPath);
        }

        return cssFiles
            .Select(file => new AssetInfo
            {
                FileName = file.HashedFileName,
                FilePath = Path.GetRelativePath(_context.RootDirectory, file.HashedPath).Replace('\\', '/'),
                Size = new FileInfo(file.HashedPath).Length,
                SourceMapPath = file.HashedSourceMapPath is null
                    ? null
                    : Path.GetRelativePath(_context.RootDirectory, file.HashedSourceMapPath).Replace('\\', '/')
            })
            .ToArray();
    }

    private static string RewriteCssContent(ProvisionalCssFile cssFile)
    {
        if (cssFile.OriginalSourceMapPath is null || cssFile.HashedSourceMapPath is null)
        {
            return cssFile.OriginalContent;
        }

        var hashedSourceMapName = Path.GetFileName(cssFile.HashedSourceMapPath);
        return CssSourceMapCommentPattern.Replace(
            cssFile.OriginalContent,
            match => $"/*# sourceMappingURL={hashedSourceMapName} */");
    }

    private static async Task WriteFinalCssAsync(
        ProvisionalCssFile cssFile,
        CancellationToken cancellationToken)
    {
        if (File.Exists(cssFile.HashedPath))
        {
            File.Delete(cssFile.HashedPath);
        }

        await File.WriteAllTextAsync(cssFile.HashedPath, cssFile.RewrittenContent, cancellationToken);

        if (cssFile.OriginalSourceMapPath is not null && cssFile.HashedSourceMapPath is not null)
        {
            await RewriteSourceMapAsync(
                cssFile.OriginalSourceMapPath,
                cssFile.HashedSourceMapPath,
                cssFile.HashedFileName,
                cancellationToken);
        }
    }

    private string? ToAbsolutePath(string? relativePath)
        => string.IsNullOrWhiteSpace(relativePath)
            ? null
            : Path.Combine(_context.RootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static long GetOptionalFileSize(string? path)
        => !string.IsNullOrWhiteSpace(path) && File.Exists(path)
            ? new FileInfo(path).Length
            : 0;

    private static string? MapSourceMapOption(SourceMapOption option)
        => option switch
        {
            SourceMapOption.None => null,
            SourceMapOption.Inline => "inline",
            SourceMapOption.External => "linked",
            _ => null
        };

    private static DenoBundleResult Failure(string message)
        => new()
        {
            Success = false,
            Diagnostics =
            [
                new BuildDiagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = message
                }
            ]
        };
}

internal sealed class DenoBundleResult
{
    public bool Success { get; init; }

    public IReadOnlyList<ChunkInfo> Chunks { get; init; } = [];

    public IReadOnlyList<AssetInfo> CssAssets { get; init; } = [];

    public IReadOnlyList<BuildDiagnostic> Diagnostics { get; init; } = [];

    public long TotalSize { get; init; }
}

internal readonly record struct OutputFileSnapshot(
    string FilePath,
    long Length,
    long LastWriteTimeUtcTicks);

internal sealed class ProvisionalBundleFile
{
    public required string OriginalPath { get; init; }

    public required string HashedPath { get; init; }

    public required string HashedFileName { get; init; }

    public required string OriginalContent { get; init; }

    public required bool IsEntry { get; init; }

    public string? OriginalSourceMapPath { get; init; }

    public string? HashedSourceMapPath { get; init; }

    public string RewrittenContent { get; set; } = string.Empty;

    public IReadOnlyList<string> Imports { get; set; } = [];
}

internal sealed class ProvisionalCssFile
{
    public required string OriginalPath { get; init; }

    public required string HashedPath { get; init; }

    public required string HashedFileName { get; init; }

    public required string OriginalContent { get; init; }

    public string? OriginalSourceMapPath { get; init; }

    public string? HashedSourceMapPath { get; init; }

    public string RewrittenContent { get; set; } = string.Empty;
}
