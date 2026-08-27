namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueCatalogOwnershipTests
{
    [TestMethod]
    public void BlazorClrMappings_AreOwnedByGeneratedClrModules()
    {
        var repositoryRoot = FindRepositoryRoot();
        var catalogDirectory = Path.Combine(repositoryRoot, "src", "Jazor.RazorVue", "RazorSdk", "Catalog");
        var clrDirectory = Path.Combine(repositoryRoot, "src", "Jazor.CLR");
        var blazorProjectionDirectory = Path.Combine(repositoryRoot, "src", "ECMAScript.Blazor");

        var catalogSources = Directory.Exists(catalogDirectory)
            ? Directory.EnumerateFiles(catalogDirectory, "*.cs", SearchOption.AllDirectories).ToArray()
            : [];
        Assert.HasCount(0, catalogSources, "RazorVue must not retain a CLR whitelist catalog.");

        var blazorProjectionSources = Directory.EnumerateFiles(blazorProjectionDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(static path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(static path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        foreach (var source in blazorProjectionSources)
        {
            var text = File.ReadAllText(source);
            Assert.IsFalse(text.Contains("[Jazor", StringComparison.Ordinal), source);
            Assert.IsFalse(text.Contains("[ECMAScriptModule", StringComparison.Ordinal), source);
        }

        var sourceRoots = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Jazor.Compiler.Generator", "SharedGeneration.cs"));
        Assert.IsFalse(sourceRoots.Contains("Path.Combine(src, \"ECMAScript.Blazor\")", StringComparison.Ordinal));
        Assert.IsFalse(sourceRoots.Contains("Path.Combine(src, \"Jazor.RazorVue\", \"RazorSdk\", \"Catalog\")", StringComparison.Ordinal));

        var blazorProjectionProject = File.ReadAllText(
            Path.Combine(repositoryRoot, "src", "ECMAScript.Blazor", "ECMAScript.Blazor.csproj"));
        Assert.IsFalse(
            blazorProjectionProject.Contains(
                "<FrameworkReference Include=\"Microsoft.AspNetCore.App\"",
                StringComparison.Ordinal),
            "ECMAScript.Blazor is a standard ECMAScript projection library and must not carry an ASP.NET Core framework dependency.");

        var clrModules = Directory.EnumerateFiles(Path.Combine(clrDirectory, "module"), "*.cs", SearchOption.TopDirectoryOnly)
            .Select(path => Path.GetFileName(path))
            .ToHashSet(StringComparer.Ordinal);

        CollectionAssert.IsSubsetOf(
            new[]
            {
                "ComponentBaseModule.cs",
                "EventCallbackModule.cs",
                "EventCallbackT1Module.cs",
                "EventCallbackFactoryModule.cs",
                "RenderFragmentModule.cs",
                "RenderFragmentT1Module.cs",
                "MarkupStringModule.cs",
                "ParameterViewModule.cs",
                "RenderTreeBuilderModule.cs",
                "WebRenderTreeBuilderExtensionsModule.cs",
                "ChangeEventArgsModule.cs",
                "ElementReferenceModule.cs",
                "ElementReferenceExtensionsModule.cs",
                "MouseEventArgsModule.cs",
                "KeyboardEventArgsModule.cs",
                "FocusEventArgsModule.cs",
                "PointerEventArgsModule.cs",
                "WheelEventArgsModule.cs",
                "DragEventArgsModule.cs",
                "DataTransferModule.cs",
                "ClipboardEventArgsModule.cs",
                "TouchEventArgsModule.cs",
                "TouchPointModule.cs",
                "ErrorEventArgsModule.cs",
                "ProgressEventArgsModule.cs",
            },
            clrModules.ToArray());

        var clrDocs = Directory.EnumerateFiles(Path.Combine(clrDirectory, "doc"), "*.md", SearchOption.TopDirectoryOnly)
            .Select(path => Path.GetFileName(path))
            .ToHashSet(StringComparer.Ordinal);
        CollectionAssert.IsSubsetOf(
            new[]
            {
                "PointerEventArgsModule.md",
                "WheelEventArgsModule.md"
                 ,"DragEventArgsModule.md",
                "DataTransferModule.md",
                "ClipboardEventArgsModule.md",
                "TouchEventArgsModule.md",
                "TouchPointModule.md",
                "ErrorEventArgsModule.md",
                "ProgressEventArgsModule.md"
            },
            clrDocs.ToArray());
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
