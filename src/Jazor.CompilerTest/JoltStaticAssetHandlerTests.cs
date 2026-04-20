using System.Security.Cryptography;
using System.Text;
using Jolt.Build;

namespace Jazor.CompilerTest;

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
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task CopyPublicAssetsAsync_DoesNotHashFiles_AtOrAboveSizeThreshold()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var publicDir = Path.Combine(tempDir, "public");
            Directory.CreateDirectory(publicDir);

            var sourcePath = Path.Combine(publicDir, "large.png");
            await File.WriteAllTextAsync(sourcePath, new string('a', 4 * 1024));

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
            Assert.AreEqual("large.png", asset.FileName);
            Assert.AreEqual("/large.png", asset.OriginalPath);

            var copiedPath = Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(copiedPath));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-static-asset-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }

    private static string ComputeHashPrefix(string content, int hashLength)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash)[..hashLength].ToLowerInvariant();
    }
}
