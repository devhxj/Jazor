using System.Security.Cryptography;
namespace Jolt.Build;

/// <summary>
/// Copies public/ directory files to dist/ with optional content hash.
/// </summary>
internal sealed class StaticAssetHandler
{
    private static readonly HashSet<string> HashExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp",
        ".woff", ".woff2", ".ttf", ".eot",
        ".mp4", ".webm", ".ogg", ".mp3",
        ".pdf"
    };

    private const int HashSizeThreshold = 4 * 1024; // 4KB

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

        await foreach (var assetPath in EnumerateFilesAsync(publicDir, ct))
        {
            ct.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(publicDir, assetPath);
            var fileName = Path.GetFileName(assetPath);
            var extension = Path.GetExtension(assetPath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(assetPath);

            // Determine if we should hash this file
            var fileInfo = new FileInfo(assetPath);
            var shouldHash = ShouldHash(assetPath) && fileInfo.Length < HashSizeThreshold;

            var destFileName = fileName;
            if (shouldHash)
            {
                var hash = await ComputeFileHashAsync(assetPath, _context.Options.AssetHashLength, ct);
                destFileName = $"{fileNameWithoutExt}-{hash}{extension}";
            }

            var destPath = Path.Combine(distDir, relativePath);
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
            await CopyFileAsync(assetPath, destPath, ct);

            var assetInfo = new AssetInfo
            {
                FileName = destFileName,
                FilePath = Path.GetRelativePath(_context.RootDirectory, destPath).Replace('\\', '/'),
                Size = new FileInfo(destPath).Length,
                OriginalPath = NormalizePublicAssetPath(relativePath)
            };

            assets.Add(assetInfo);
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
            if (!IsInsideRoot(absolutePath) || !File.Exists(absolutePath))
            {
                continue;
            }

            var normalizedOriginalPath = NormalizePublicAssetPath(sourceAsset.OriginalPath);
            var relativeOutputDirectory = Path.GetDirectoryName(
                normalizedOriginalPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))
                ?? string.Empty;
            var fileName = Path.GetFileName(absolutePath);
            var extension = Path.GetExtension(absolutePath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(absolutePath);
            var fileInfo = new FileInfo(absolutePath);
            var shouldHash = ShouldHash(absolutePath) && fileInfo.Length < HashSizeThreshold;
            var outputDirectory = Path.Combine(_context.OutDirectory, relativeOutputDirectory);

            Directory.CreateDirectory(outputDirectory);

            var outputFileName = fileName;
            if (shouldHash)
            {
                var hash = await ComputeFileHashAsync(absolutePath, _context.Options.AssetHashLength, ct);
                outputFileName = $"{fileNameWithoutExtension}-{hash}{extension}";
            }

            var outputPath = Path.Combine(outputDirectory, outputFileName);
            await CopyFileAsync(absolutePath, outputPath, ct);

            assets.Add(new AssetInfo
            {
                FileName = outputFileName,
                FilePath = Path.GetRelativePath(_context.RootDirectory, outputPath).Replace('\\', '/'),
                Size = new FileInfo(outputPath).Length,
                OriginalPath = normalizedOriginalPath
            });
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
        var bytes = await File.ReadAllBytesAsync(filePath, ct);

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash)[..hashLength].ToLowerInvariant();
    }

    /// <summary>
    /// Copies a file asynchronously.
    /// </summary>
    private static async Task CopyFileAsync(string sourcePath, string destPath, CancellationToken ct)
    {
        using var sourceStream = File.OpenRead(sourcePath);
        using var destStream = File.Create(destPath);

        await sourceStream.CopyToAsync(destStream, ct);
        await destStream.FlushAsync(ct);
    }

    /// <summary>
    /// Enumerates files in a directory asynchronously.
    /// </summary>
    private static async IAsyncEnumerable<string> EnumerateFilesAsync(
        string directory,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var stack = new Stack<string>();
        stack.Push(directory);

        while (stack.Count > 0)
        {
            ct.ThrowIfCancellationRequested();

            var currentDir = stack.Pop();

            foreach (var file in Directory.GetFiles(currentDir))
            {
                yield return file;
            }

            // Add subdirectories to stack
            foreach (var subDir in Directory.GetDirectories(currentDir))
            {
                stack.Push(subDir);
            }

            // Small delay to prevent blocking
            await Task.Yield();
        }
    }

    private bool IsInsideRoot(string candidatePath)
    {
        var relativePath = Path.GetRelativePath(_context.RootDirectory, candidatePath);
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
