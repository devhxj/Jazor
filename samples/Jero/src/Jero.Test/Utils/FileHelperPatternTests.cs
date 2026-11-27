using Jero.Core;
using Xunit;

namespace Jero.Test.Utils;

public class FileHelperPatternTests
{
    [Fact]
    public void FindPathsByPatterns_MatchesNestedGlobAndRuntimeDirectory()
    {
        var root = TestWorkspace.Create("files");
        try
        {
            var configDir = Path.Combine(root, "Configuration");
            var uploadDir = Path.Combine(root, "wwwroot", "upload");
            Directory.CreateDirectory(configDir);
            Directory.CreateDirectory(uploadDir);
            File.WriteAllText(Path.Combine(configDir, "App.json"), "{}");
            File.WriteAllText(Path.Combine(configDir, "readme.txt"), "keep");
            File.WriteAllText(Path.Combine(uploadDir, "logo.png"), "logo");
            File.WriteAllText(Path.Combine(root, "Jero.db"), "db");

            var matches = FileHelper.FindPathsByPatterns(root,
            [
                "Configuration\\*.json",
                "wwwroot/upload",
                "Jero.db",
            ]);

            Assert.Contains(FullPath(root, "Configuration", "App.json"), matches);
            Assert.Contains(FullPath(root, "wwwroot", "upload"), matches);
            Assert.Contains(FullPath(root, "Jero.db"), matches);
            Assert.DoesNotContain(FullPath(root, "Configuration", "readme.txt"), matches);
        }
        finally
        {
            FileHelper.TryDelete(root);
        }
    }

    [Fact]
    public void DeleteByPatterns_RemovesMatchedPathsOnly()
    {
        var root = TestWorkspace.Create("files");
        try
        {
            var configDir = Path.Combine(root, "Configuration");
            var uploadDir = Path.Combine(root, "wwwroot", "upload");
            var spaDir = Path.Combine(root, "spa-dist");
            Directory.CreateDirectory(configDir);
            Directory.CreateDirectory(uploadDir);
            Directory.CreateDirectory(spaDir);
            File.WriteAllText(Path.Combine(configDir, "App.json"), "{}");
            File.WriteAllText(Path.Combine(configDir, "readme.txt"), "keep");
            File.WriteAllText(Path.Combine(uploadDir, "logo.png"), "logo");
            File.WriteAllText(Path.Combine(spaDir, "index.html"), "keep");

            FileHelper.DeleteByPatterns(root,
            [
                "Configuration/*.json",
                "wwwroot/upload/**",
            ]);

            Assert.False(File.Exists(Path.Combine(configDir, "App.json")));
            Assert.False(Directory.Exists(uploadDir));
            Assert.True(File.Exists(Path.Combine(configDir, "readme.txt")));
            Assert.True(File.Exists(Path.Combine(spaDir, "index.html")));
        }
        finally
        {
            FileHelper.TryDelete(root);
        }
    }

    private static string FullPath(params string[] paths)
    {
        return Path.GetFullPath(Path.Combine(paths));
    }
}
