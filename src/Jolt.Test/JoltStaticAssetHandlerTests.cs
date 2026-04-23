using System.Security.Cryptography;
using System.Text;
using Jolt.Build;

namespace Jolt.Test;

[TestClass]
public sealed class JoltStaticAssetHandlerTests
{
    [TestMethod]
    public async Task CopyPublicAssetsAsync_UsesConfiguredAssetHashLength_ForHashablePublicAssets()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var publicDir = Path.Combine(tempDir, "public");
            Directory.CreateDirectory(publicDir);

            const string fileName = "logo.png";
            const string fileContent = "fake-png-data";
            var sourcePath = Path.Combine(publicDir, fileName);
            await File.WriteAllTextAsync(sourcePath, fileContent);

            var options = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                AssetHashLength = 12
            };

            using var context = new BuildContext(options);
            var handler = new StaticAssetHandler(context);

            var assets = await handler.CopyPublicAssetsAsync(CancellationToken.None);

            Assert.AreEqual(1, assets.Count);

            var asset = assets.Single();
            var expectedHash = ComputeHashPrefix(fileContent, options.AssetHashLength);
            Assert.AreEqual($"logo-{expectedHash}.png", asset.FileName);
            Assert.AreEqual($"/{fileName}", asset.OriginalPath);
            Assert.AreEqual(12, expectedHash.Length);

            var copiedPath = Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(copiedPath));
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public async Task CopyPublicAssetsAsync_HashesLargeHashableFiles_ByDefault()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var publicDir = Path.Combine(tempDir, "public");
            Directory.CreateDirectory(publicDir);

            var sourcePath = Path.Combine(publicDir, "large.png");
            var content = new string('a', 4 * 1024);
            await File.WriteAllTextAsync(sourcePath, content);

            var options = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                AssetHashLength = 12
            };

            using var context = new BuildContext(options);
            var handler = new StaticAssetHandler(context);

            var assets = await handler.CopyPublicAssetsAsync(CancellationToken.None);

            Assert.AreEqual(1, assets.Count);

            var asset = assets.Single();
            var expectedHash = ComputeHashPrefix(content, options.AssetHashLength);
            Assert.AreEqual($"large-{expectedHash}.png", asset.FileName);
            Assert.AreEqual("/large.png", asset.OriginalPath);

            var copiedPath = Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(copiedPath));
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public async Task CopySourceAssetsAsync_SkipsLockedSourceAsset_WithWarningInsteadOfFailing()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var imagePath = Path.Combine(tempDir, "images", "logo.png");
            Directory.CreateDirectory(Path.GetDirectoryName(imagePath)!);
            await File.WriteAllTextAsync(imagePath, "fake-png-data");

            var options = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                AssetHashLength = 12
            };

            using var context = new BuildContext(options);
            var handler = new StaticAssetHandler(context);
            using var lockHandle = new FileStream(
                imagePath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);

            var assets = await handler.CopySourceAssetsAsync(
                [
                    new SourceAssetRequest
                    {
                        AbsolutePath = imagePath,
                        OriginalPath = "/images/logo.png"
                    }
                ],
                CancellationToken.None);

            Assert.AreEqual(0, assets.Count);
            CollectionAssert.Contains(
                context.Diagnostics.Select(static diagnostic => diagnostic.Message).ToArray(),
                $"Skipped source asset '/images/logo.png' from '{Path.GetFullPath(imagePath)}' because it became unavailable during build.");
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public async Task CopySourceAssetsAsync_SkipsSourceAssetOutsideWorkspace_WithWarningInsteadOfCopying()
    {
        var tempDir = CreateTemporaryDirectory();
        var externalDir = CreateTemporaryDirectory();
        try
        {
            var externalAssetPath = Path.Combine(externalDir, "logo.png");
            await File.WriteAllTextAsync(externalAssetPath, "fake-png-data");

            var options = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                AssetHashLength = 12
            };

            using var context = new BuildContext(options);
            var handler = new StaticAssetHandler(context);

            var assets = await handler.CopySourceAssetsAsync(
                [
                    new SourceAssetRequest
                    {
                        AbsolutePath = externalAssetPath,
                        OriginalPath = "/images/logo.png"
                    }
                ],
                CancellationToken.None);

            Assert.AreEqual(0, assets.Count);
            CollectionAssert.Contains(
                context.Diagnostics.Select(static diagnostic => diagnostic.Message).ToArray(),
                $"Skipped source asset '/images/logo.png' from '{Path.GetFullPath(externalAssetPath)}' because it traversed an untrusted reparse point or resolved outside the workspace boundary.");
        }
        finally
        {
            DeleteDirectory(tempDir);
            DeleteDirectory(externalDir);
        }
    }

    [TestMethod]
    public void TryResolvePublicAssetOutputPath_ReturnsFalse_WhenAssetEscapesPublicBoundary()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var publicDir = Path.Combine(tempDir, "public");
            var distDir = Path.Combine(tempDir, "dist");
            Directory.CreateDirectory(publicDir);
            Directory.CreateDirectory(distDir);

            var externalAssetPath = Path.Combine(tempDir, "..", "outside.png");

            var result = StaticAssetHandler.TryResolvePublicAssetOutputPath(
                publicDir,
                distDir,
                externalAssetPath,
                out var relativePath,
                out var destinationPath);

            Assert.IsFalse(result);
            Assert.AreEqual(string.Empty, relativePath);
            Assert.AreEqual(string.Empty, destinationPath);
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public void ShouldTraversePublicDirectory_ReturnsFalse_ForReparsePoint()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var publicDir = Path.Combine(tempDir, "public");
            var candidateDirectory = Path.Combine(publicDir, "linked");

            var result = StaticAssetHandler.ShouldTraversePublicDirectory(
                publicDir,
                candidateDirectory,
                FileAttributes.Directory | FileAttributes.ReparsePoint);

            Assert.IsFalse(result);
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public async Task CopyPublicAssetsAsync_SkipsDirectorySymlink()
    {
        var tempDir = CreateTemporaryDirectory();
        var externalDir = CreateTemporaryDirectory();
        try
        {
            var publicDir = Path.Combine(tempDir, "public");
            Directory.CreateDirectory(publicDir);
            var externalAssetPath = Path.Combine(externalDir, "logo.png");
            await File.WriteAllTextAsync(externalAssetPath, "external-png-data");
            var linkPath = Path.Combine(publicDir, "linked");
            CreateDirectorySymbolicLinkOrInconclusive(linkPath, externalDir);

            var options = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                AssetHashLength = 12
            };

            using var context = new BuildContext(options);
            var handler = new StaticAssetHandler(context);

            var assets = await handler.CopyPublicAssetsAsync(CancellationToken.None);

            Assert.AreEqual(0, assets.Count);
        }
        finally
        {
            DeleteDirectory(tempDir);
            DeleteDirectory(externalDir);
        }
    }

    [TestMethod]
    public void IsTrustedFilePath_ReturnsTrue_ForRegularFileInsideRoot()
    {
        var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "static-asset-root-" + Guid.NewGuid().ToString("N")));
        var candidatePath = Path.Combine(rootDirectory, "logo.png");

        var result = StaticAssetHandler.IsTrustedFilePath(
            rootDirectory,
            candidatePath,
            FileAttributes.Normal);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsTrustedFilePath_ReturnsFalse_ForPathOutsideRoot()
    {
        var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "static-asset-root-" + Guid.NewGuid().ToString("N")));
        var candidatePath = Path.GetFullPath(Path.Combine(rootDirectory, "..", "logo.png"));

        var result = StaticAssetHandler.IsTrustedFilePath(
            rootDirectory,
            candidatePath,
            FileAttributes.Normal);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsTrustedFilePath_ReturnsFalse_ForReparsePointFile()
    {
        var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "static-asset-root-" + Guid.NewGuid().ToString("N")));
        var candidatePath = Path.Combine(rootDirectory, "logo.png");

        var result = StaticAssetHandler.IsTrustedFilePath(
            rootDirectory,
            candidatePath,
            FileAttributes.ReparsePoint);

        Assert.IsFalse(result);
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-static-asset-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
        }
    }

    private static void CreateDirectorySymbolicLinkOrInconclusive(string linkPath, string targetPath)
    {
        try
        {
            Directory.CreateSymbolicLink(linkPath, targetPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
        {
            Assert.Inconclusive(
                $"Current environment cannot create a directory symbolic link for reparse-point verification: {ex.GetType().Name}: {ex.Message}");
        }

        var attributes = File.GetAttributes(linkPath);
        if ((attributes & FileAttributes.ReparsePoint) == 0)
        {
            Assert.Inconclusive("Current environment created the link path without FileAttributes.ReparsePoint.");
        }
    }

    private static string ComputeHashPrefix(string content, int hashLength)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash)[..hashLength].ToLowerInvariant();
    }
}
