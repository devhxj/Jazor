namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueCatalogOwnershipTests
{
    [TestMethod]
    public void AspNetComponentCatalog_KeepsCompilerOwnedSurfaceSeparateFromClrNavigation()
    {
        var repositoryRoot = FindRepositoryRoot();
        var catalogDirectory = Path.Combine(repositoryRoot, "src", "Jazor.RazorVue", "RazorSdk", "Catalog");
        var clrDirectory = Path.Combine(repositoryRoot, "src", "Jazor.CLR");

        Assert.IsTrue(File.Exists(Path.Combine(catalogDirectory, "ComponentBaseCatalog.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(catalogDirectory, "EventCallbackCatalog.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(catalogDirectory, "NavigationManagerCatalog.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(catalogDirectory, "RenderTreeBuilderCatalog.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(catalogDirectory, "WebRenderTreeBuilderExtensionsCatalog.cs")));

        var clrAspNetReferences = Directory.EnumerateFiles(clrDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => File.ReadAllText(path).Contains("Microsoft.AspNetCore.Components", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(repositoryRoot, path))
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[]
            {
                Path.Combine("src", "Jazor.CLR", "module", "NavigationManagerModule.cs"),
                Path.Combine("src", "Jazor.CLR", "module", "NavigationOptionsModule.cs"),
                Path.Combine("src", "Jazor.CLR", "module", "LocationChangedEventArgsModule.cs"),
                Path.Combine("src", "Jazor.CLR", "module", "LocationChangingContextModule.cs"),
                Path.Combine("src", "Jazor.CLR", "module", "NotFoundEventArgsModule.cs"),
                Path.Combine("src", "Jazor.CLR", "module", "NavigationManagerExtensionsModule.cs"),
                Path.Combine("src", "Jazor.CLR", "module", "ChangeEventArgsModule.cs"),
            },
            clrAspNetReferences);
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
