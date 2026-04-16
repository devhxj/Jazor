using System.Security.Cryptography;
using System.Text;

namespace Jazor.VueHost.Build;

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
        var distDir = Path.Combine(_context.RootDirectory, _context.Options.OutDir);

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
                var hash = await ComputeFileHashAsync(assetPath, ct);
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
                FilePath = Path.GetRelativePath(_context.RootDirectory, destPath),
                Size = new FileInfo(destPath).Length
            };

            assets.Add(assetInfo);
        }

        return assets;
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
    /// Computes SHA256 hash of a file and returns the first 8 hex characters.
    /// </summary>
    private static async Task<string> ComputeFileHashAsync(string filePath, CancellationToken ct)
    {
        var bytes = await File.ReadAllBytesAsync(filePath, ct);

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(bytes);

        // Take first 8 characters (4 bytes) of the hash
        var hashBuilder = new StringBuilder();
        for (int i = 0; i < 4; i++)
        {
            hashBuilder.Append(hash[i].ToString("x2"));
        }

        return hashBuilder.ToString();
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
}
