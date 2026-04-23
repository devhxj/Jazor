using System.Security.Cryptography;
namespace Jolt.Build;

/// <summary>
/// Copies public/ directory files to dist/ with optional content hash.
/// </summary>
internal sealed class StaticAssetHandler
{
    private const int FileIoBufferSize = 64 * 1024;
    private static readonly HashSet<string> HashExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp",
        ".woff", ".woff2", ".ttf", ".eot",
        ".mp4", ".webm", ".ogg", ".mp3",
        ".pdf"
    };

    private readonly BuildContext _context;

    public StaticAssetHandler(BuildContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Copies public assets to the dist directory with optional content hashing.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of copied asset information.</returns>
    public async Task<IReadOnlyList<AssetInfo>> CopyPublicAssetsAsync(CancellationToken ct)
    {
        var publicDir = Path.Combine(_context.RootDirectory, "public");

        if (!Directory.Exists(publicDir))
        {
            _context.Diagnostics.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Info,
                Message = "No public directory found, skipping static asset copying"
            });
            return [];
        }

        var assets = new List<AssetInfo>();
        var distDir = _context.OutDirectory;

        foreach (var assetPath in EnumerateFiles(publicDir, ct))
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var fullAssetPath = Path.GetFullPath(assetPath);
                if (!File.Exists(fullAssetPath))
                {
                    AddSkippedAssetDiagnostic(assetPath);
                    continue;
                }

                if (!TryGetTrustedFilePath(publicDir, fullAssetPath, out var trustedAssetPath))
                {
                    AddSkippedAssetTrustDiagnostic(assetPath);
                    continue;
                }

                if (!TryResolvePublicAssetOutputPath(publicDir, distDir, assetPath, out var relativePath, out var destPath))
                {
                    AddSkippedAssetBoundaryDiagnostic(assetPath);
                    continue;
                }

                var fileName = Path.GetFileName(assetPath);
                var extension = Path.GetExtension(assetPath);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(assetPath);

                var shouldHash = ShouldHash(trustedAssetPath);

                var destFileName = fileName;
                if (shouldHash)
                {
                    var hash = await ComputeFileHashAsync(trustedAssetPath, _context.Options.AssetHashLength, ct);
                    destFileName = $"{fileNameWithoutExt}-{hash}{extension}";
                }

                var destDir = Path.GetDirectoryName(destPath);

                if (!string.IsNullOrEmpty(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                // Update destination path with hashed filename
                if (shouldHash)
                {
                    destPath = Path.Combine(destDir!, destFileName);
                }

                // Copy file
                await CopyFileAsync(trustedAssetPath, destPath, ct);

                var assetInfo = new AssetInfo
                {
                    FileName = destFileName,
                    FilePath = Path.GetRelativePath(_context.RootDirectory, destPath).Replace('\\', '/'),
                    Size = new FileInfo(destPath).Length,
                    OriginalPath = NormalizePublicAssetPath(relativePath)
                };

                assets.Add(assetInfo);
            }
            catch (DirectoryNotFoundException)
            {
                AddSkippedAssetDiagnostic(assetPath);
            }
            catch (FileNotFoundException)
            {
                AddSkippedAssetDiagnostic(assetPath);
            }
            catch (IOException)
            {
                AddSkippedAssetDiagnostic(assetPath);
            }
            catch (UnauthorizedAccessException)
            {
                AddSkippedAssetDiagnostic(assetPath);
            }
        }

        return assets;
    }

    public async Task<IReadOnlyList<AssetInfo>> CopySourceAssetsAsync(
        IReadOnlyList<SourceAssetRequest> sourceAssets,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(sourceAssets);

        if (sourceAssets.Count == 0)
        {
            return [];
        }

        var assets = new List<AssetInfo>();
        var copiedOriginalPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var sourceAsset in sourceAssets)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(sourceAsset.OriginalPath)
                || !copiedOriginalPaths.Add(sourceAsset.OriginalPath))
            {
                continue;
            }

            var absolutePath = Path.GetFullPath(sourceAsset.AbsolutePath);
            if (!File.Exists(absolutePath))
            {
                continue;
            }

            if (!TryGetTrustedFilePath(_context.RootDirectory, absolutePath, out var trustedAbsolutePath))
            {
                AddSkippedSourceAssetTrustDiagnostic(sourceAsset.OriginalPath, absolutePath);
                continue;
            }

            try
            {
                var normalizedOriginalPath = NormalizePublicAssetPath(sourceAsset.OriginalPath);
                var relativeOutputDirectory = Path.GetDirectoryName(
                    normalizedOriginalPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))
                    ?? string.Empty;
                var fileName = Path.GetFileName(trustedAbsolutePath);
                var extension = Path.GetExtension(trustedAbsolutePath);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(trustedAbsolutePath);
                var shouldHash = ShouldHash(trustedAbsolutePath);
                var outputDirectory = Path.Combine(_context.OutDirectory, relativeOutputDirectory);

                Directory.CreateDirectory(outputDirectory);

                var outputFileName = fileName;
                if (shouldHash)
                {
                    var hash = await ComputeFileHashAsync(trustedAbsolutePath, _context.Options.AssetHashLength, ct);
                    outputFileName = $"{fileNameWithoutExtension}-{hash}{extension}";
                }

                var outputPath = Path.Combine(outputDirectory, outputFileName);
                await CopyFileAsync(trustedAbsolutePath, outputPath, ct);

                assets.Add(new AssetInfo
                {
                    FileName = outputFileName,
                    FilePath = Path.GetRelativePath(_context.RootDirectory, outputPath).Replace('\\', '/'),
                    Size = new FileInfo(outputPath).Length,
                    OriginalPath = normalizedOriginalPath
                });
            }
            catch (DirectoryNotFoundException)
            {
                AddSkippedSourceAssetDiagnostic(sourceAsset.OriginalPath, absolutePath);
            }
            catch (FileNotFoundException)
            {
                AddSkippedSourceAssetDiagnostic(sourceAsset.OriginalPath, absolutePath);
            }
            catch (IOException)
            {
                AddSkippedSourceAssetDiagnostic(sourceAsset.OriginalPath, absolutePath);
            }
            catch (UnauthorizedAccessException)
            {
                AddSkippedSourceAssetDiagnostic(sourceAsset.OriginalPath, absolutePath);
            }
        }

        return assets;
    }

    private static string NormalizePublicAssetPath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').Trim();
        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        return normalized.StartsWith("/", StringComparison.Ordinal)
            ? normalized
            : "/" + normalized.TrimStart('/');
    }

    /// <summary>
    /// Determines if a file should be content-hashed based on its extension.
    /// </summary>
    private static bool ShouldHash(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return !string.IsNullOrEmpty(extension) && HashExtensions.Contains(extension);
    }

    /// <summary>
    /// Computes SHA256 hash of a file and returns the configured hex prefix length.
    /// </summary>
    private static async Task<string> ComputeFileHashAsync(string filePath, int hashLength, CancellationToken ct)
    {
        // 生产构建里静态资源数量可能很多，哈希改成流式读取，避免为每个文件分配整块 byte[]。
        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            FileIoBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, ct);
        return Convert.ToHexString(hash)[..hashLength].ToLowerInvariant();
    }

    /// <summary>
    /// Copies a file asynchronously.
    /// </summary>
    private static async Task CopyFileAsync(string sourcePath, string destPath, CancellationToken ct)
    {
        using var sourceStream = new FileStream(
            sourcePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            FileIoBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var destStream = new FileStream(
            destPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            FileIoBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        await sourceStream.CopyToAsync(destStream, ct);
    }

    /// <summary>
    /// Enumerates files in a directory asynchronously.
    /// </summary>
    private static IEnumerable<string> EnumerateFiles(
        string directory,
        CancellationToken ct)
    {
        var stack = new Stack<string>();
        stack.Push(Path.GetFullPath(directory));

        while (stack.Count > 0)
        {
            ct.ThrowIfCancellationRequested();

            var currentDir = stack.Pop();

            foreach (var file in SafeEnumerate(() => Directory.EnumerateFiles(currentDir)))
            {
                yield return file;
            }

            // Add subdirectories to stack
            foreach (var subDir in SafeEnumerate(() => Directory.EnumerateDirectories(currentDir)))
            {
                var fullSubDirectory = Path.GetFullPath(subDir);
                if (!ShouldTraversePublicDirectory(directory, fullSubDirectory))
                {
                    continue;
                }

                stack.Push(fullSubDirectory);
            }
        }
    }

    private void AddSkippedAssetDiagnostic(string assetPath)
    {
        _context.Diagnostics.Add(new BuildDiagnostic
        {
            Severity = DiagnosticSeverity.Warning,
            Message = $"Skipped static asset '{assetPath}' because it became unavailable during traversal."
        });
    }

    private void AddSkippedAssetBoundaryDiagnostic(string assetPath)
    {
        _context.Diagnostics.Add(new BuildDiagnostic
        {
            Severity = DiagnosticSeverity.Warning,
            Message = $"Skipped static asset '{assetPath}' because it resolved outside the public directory boundary."
        });
    }

    private void AddSkippedAssetTrustDiagnostic(string assetPath)
    {
        _context.Diagnostics.Add(new BuildDiagnostic
        {
            Severity = DiagnosticSeverity.Warning,
            Message = $"Skipped static asset '{assetPath}' because it traversed an untrusted reparse point."
        });
    }

    private void AddSkippedSourceAssetDiagnostic(string originalPath, string absolutePath)
    {
        _context.Diagnostics.Add(new BuildDiagnostic
        {
            Severity = DiagnosticSeverity.Warning,
            Message = $"Skipped source asset '{originalPath}' from '{absolutePath}' because it became unavailable during build."
        });
    }

    private void AddSkippedSourceAssetTrustDiagnostic(string originalPath, string absolutePath)
    {
        _context.Diagnostics.Add(new BuildDiagnostic
        {
            Severity = DiagnosticSeverity.Warning,
            Message = $"Skipped source asset '{originalPath}' from '{absolutePath}' because it traversed an untrusted reparse point or resolved outside the workspace boundary."
        });
    }

    private static IEnumerable<string> SafeEnumerate(Func<IEnumerable<string>> factory)
    {
        IEnumerator<string>? enumerator = null;
        try
        {
            enumerator = factory().GetEnumerator();
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }
        catch (IOException)
        {
            yield break;
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }

        using (enumerator)
        {
            while (true)
            {
                string current;
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        yield break;
                    }

                    current = enumerator.Current;
                }
                catch (DirectoryNotFoundException)
                {
                    yield break;
                }
                catch (IOException)
                {
                    yield break;
                }
                catch (UnauthorizedAccessException)
                {
                    yield break;
                }

                yield return current;
            }
        }
    }

    internal static bool TryResolvePublicAssetOutputPath(
        string publicDirectory,
        string distDirectory,
        string assetPath,
        out string relativePath,
        out string destinationPath)
    {
        var fullPublicDirectory = Path.GetFullPath(publicDirectory);
        var fullDistDirectory = Path.GetFullPath(distDirectory);
        var fullAssetPath = Path.GetFullPath(assetPath);
        if (!IsInsideDirectory(fullPublicDirectory, fullAssetPath))
        {
            relativePath = string.Empty;
            destinationPath = string.Empty;
            return false;
        }

        relativePath = Path.GetRelativePath(fullPublicDirectory, fullAssetPath);
        destinationPath = Path.GetFullPath(Path.Combine(fullDistDirectory, relativePath));
        return IsInsideDirectory(fullDistDirectory, destinationPath);
    }

    internal static bool ShouldTraversePublicDirectory(
        string publicDirectory,
        string candidateDirectory,
        FileAttributes attributes)
    {
        var fullPublicDirectory = Path.GetFullPath(publicDirectory);
        var fullCandidateDirectory = Path.GetFullPath(candidateDirectory);
        if (!IsInsideDirectory(fullPublicDirectory, fullCandidateDirectory))
        {
            return false;
        }

        // 不跟随 public/ 下的 reparse point，避免把仓库外资源或循环目录带进产物复制。
        return (attributes & FileAttributes.ReparsePoint) == 0;
    }

    internal static bool IsTrustedFilePath(
        string rootDirectory,
        string candidatePath,
        FileAttributes attributes)
    {
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullCandidatePath = Path.GetFullPath(candidatePath);
        return IsInsideDirectory(fullRootDirectory, fullCandidatePath)
            && (attributes & FileAttributes.ReparsePoint) == 0;
    }

    private static bool TryGetTrustedFilePath(
        string rootDirectory,
        string candidatePath,
        out string trustedPath)
    {
        trustedPath = string.Empty;
        var fullCandidatePath = Path.GetFullPath(candidatePath);
        if (!IsInsideDirectory(Path.GetFullPath(rootDirectory), fullCandidatePath))
        {
            return false;
        }

        try
        {
            // 目录不跟随 reparse point 还不够，文件本身如果是联接/符号链接，也可能把仓库外内容带进产物。
            if (!IsTrustedFilePath(rootDirectory, fullCandidatePath, File.GetAttributes(fullCandidatePath)))
            {
                return false;
            }

            trustedPath = fullCandidatePath;
            return true;
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

    private static bool ShouldTraversePublicDirectory(
        string publicDirectory,
        string candidateDirectory)
    {
        try
        {
            return ShouldTraversePublicDirectory(
                publicDirectory,
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

    private static bool IsInsideDirectory(string rootDirectory, string candidatePath)
    {
        var relativePath = Path.GetRelativePath(rootDirectory, candidatePath);
        return string.Equals(relativePath, ".", StringComparison.Ordinal)
            || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath));
    }
}

internal sealed class SourceAssetRequest
{
    public required string AbsolutePath { get; init; }

    public required string OriginalPath { get; init; }
}
