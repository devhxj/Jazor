namespace Jazor.ComplierTest;

[TestClass]
public sealed class DocumentationLayoutTests
{
    [TestMethod]
    public void Documentation_UsesFiveEnglishDirectories()
    {
        var root = ResolveRepositoryRoot();

        var directories = new[]
        {
            "01-overview",
            "02-architecture",
            "03-guides",
            "04-roadmap",
            "05-history"
        };

        foreach (var directory in directories)
        {
            var path = Path.Combine(root, "docs", directory);
            Assert.IsTrue(Directory.Exists(path), $"Missing documentation directory: {path}");
            Assert.IsTrue(File.Exists(Path.Combine(path, "README.md")), $"Missing documentation index: {path}");
        }

        Assert.IsTrue(File.Exists(Path.Combine(root, "docs", "02-architecture", "compiler.md")));
        Assert.IsTrue(File.Exists(Path.Combine(root, "docs", "02-architecture", "framework-integrations.md")));
        Assert.IsTrue(File.Exists(Path.Combine(root, "docs", "03-guides", "installation-and-configuration.md")));
        Assert.IsTrue(File.Exists(Path.Combine(root, "docs", "05-history", "evolution.md")));

        foreach (var legacyDirectory in new[] { "01-目标", "02-计划", "03-完成", "04-补充", "05-遗弃" })
        {
            Assert.IsFalse(
                Directory.Exists(Path.Combine(root, "docs", legacyDirectory)),
                $"Legacy documentation directory should not exist: {legacyDirectory}");
        }
    }

    [TestMethod]
    public void Documentation_Entrypoints_UseCurrentPaths()
    {
        var root = ResolveRepositoryRoot();
        var docsReadme = File.ReadAllText(Path.Combine(root, "docs", "README.md"));

        foreach (var directory in new[] { "01-overview", "02-architecture", "03-guides", "04-roadmap", "05-history" })
        {
            StringAssert.Contains(docsReadme, directory);
        }

        foreach (var legacyDirectory in new[] { "01-目标", "02-计划", "03-完成", "04-补充", "05-遗弃" })
        {
            Assert.IsFalse(
                docsReadme.Contains(legacyDirectory, StringComparison.Ordinal),
                $"docs/README.md still contains legacy directory: {legacyDirectory}");
        }
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
                return current.FullName;
            current = current.Parent;
        }

        throw new InvalidOperationException("Cannot locate repository root (Jazor.slnx).");
    }
}
