using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class EmitOptionsTests
{
    [TestMethod]
    public void TryParse_BrowserEntries_PreserveRelativeCatalogPaths()
    {
        var listPath = Path.Combine(RepositoryTemp.Root, "browser-entries-" + Guid.NewGuid().ToString("N") + ".txt");
        Directory.CreateDirectory(Path.GetDirectoryName(listPath)!);
        try
        {
            File.WriteAllLines(listPath, ["pages/editor.js", "", "  app.js  "]);
            var parsed = EmitOptions.TryParse(
                ["--root", "root.dll", "--out", "out", "--write-manifest", "state.json",
                    "--browser-entry", "./app.js", "--browser-entry", "shell.js", "--browser-entry-list", listPath],
                out var options, out var error);

            Assert.IsTrue(parsed, error);
            Assert.IsNotNull(options);
            CollectionAssert.AreEqual(new[] { "./app.js", "shell.js", "pages/editor.js", "app.js" }, options.BrowserEntries!.ToArray());
        }
        finally
        {
            File.Delete(listPath);
        }
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void TryParse_EmptyBrowserEntry_ReportsInputError(string entry)
    {
        var parsed = EmitOptions.TryParse(
            ["--root", "root.dll", "--out", "out", "--write-manifest", "state.json", "--browser-entry", entry],
            out _, out var error);

        Assert.IsFalse(parsed);
        StringAssert.Contains(error, "Browser entry must name a generated module path");
    }

    [TestMethod]
    public void TryParse_PathLists_PreservePathsAndIgnoreBlankLines()
    {
        var root = Path.Combine(RepositoryTemp.Root, "jazor-emit-options-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var firstAssembly = Path.Combine(root, "first.dll");
            var secondAssembly = Path.Combine(root, "second.dll");
            var firstManifest = Path.Combine(root, "first.json");
            var secondManifest = Path.Combine(root, "second.json");
            var assemblyList = Path.Combine(root, "assemblies.txt");
            var manifestList = Path.Combine(root, "manifests.txt");
            File.WriteAllLines(assemblyList, [firstAssembly, "", $"  {secondAssembly}  "]);
            File.WriteAllLines(manifestList, [firstManifest, secondManifest]);

            var parsed = EmitOptions.TryParse(
                [
                    "--root", Path.Combine(root, "root.dll"),
                    "--assembly-list", assemblyList,
                    "--out", Path.Combine(root, "out"),
                    "--write-manifest", Path.Combine(root, "state.json"),
                    "--library-manifest-list", manifestList
                ],
                out var options,
                out var error);

            Assert.IsTrue(parsed, error);
            Assert.IsNotNull(options);
            CollectionAssert.AreEqual(new[] { firstAssembly, secondAssembly }, options.AssemblyPaths.ToArray());
            CollectionAssert.AreEqual(new[] { firstManifest, secondManifest }, options.LibraryManifests.ToArray());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void TryParse_MissingPathList_ReportsInputError()
    {
        var parsed = EmitOptions.TryParse(
            [
                "--root", "root.dll",
                "--assembly-list", Path.Combine(RepositoryTemp.Root, Guid.NewGuid().ToString("N"), "missing.txt"),
                "--out", "out",
                "--write-manifest", "state.json"
            ],
            out _,
            out var error);

        Assert.IsFalse(parsed);
        StringAssert.Contains(error, "Could not read Emit path list", StringComparison.Ordinal);
    }
}
