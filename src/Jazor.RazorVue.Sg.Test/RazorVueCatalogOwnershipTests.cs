namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueCatalogOwnershipTests
{
    [TestMethod]
    public void AspNetComponentCatalog_IsOwnedByRazorVueInsteadOfClrRuntime()
    {
        var repositoryRoot = FindRepositoryRoot();
        var catalogDirectory = Path.Combine(repositoryRoot, "src", "Jazor.RazorVue", "RazorSdk", "Catalog");
        var clrDirectory = Path.Combine(repositoryRoot, "src", "Jazor.CLR");

        Assert.IsTrue(File.Exists(Path.Combine(catalogDirectory, "EventCallbackCatalog.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(catalogDirectory, "RenderTreeBuilderCatalog.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(catalogDirectory, "WebRenderTreeBuilderExtensionsCatalog.cs")));

        var clrAspNetReferences = Directory.EnumerateFiles(clrDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => File.ReadAllText(path).Contains("Microsoft.AspNetCore.Components", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(repositoryRoot, path))
            .ToArray();

        Assert.AreEqual(0, clrAspNetReferences.Length, string.Join(Environment.NewLine, clrAspNetReferences));
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Unable to locate the repository root.");
    }
}
