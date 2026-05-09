namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueSampleBuildIsolationTests
{
    [TestMethod]
    public void TodoListBuildScript_ExposesIsolatedBuildOutputParameters()
    {
        var root = FindRepositoryRoot();
        var scriptPath = Path.Combine(root, "samples", "RazorVue.TodoList", "build-local.ps1");
        var script = File.ReadAllText(scriptPath);

        StringAssert.Contains(script, "[string]$BaseOutputPath = \"\"");
        StringAssert.Contains(script, "[string]$BaseIntermediateOutputPath = \"\"");
        StringAssert.Contains(script, "-p:JazorIsolatedBaseOutputRoot=$BaseOutputPath");
        StringAssert.Contains(script, "-p:JazorIsolatedBaseIntermediateOutputRoot=$BaseIntermediateOutputPath");
        StringAssert.Contains(script, "/nr:false");
        StringAssert.Contains(script, "-p:UseSharedCompilation=false");
    }

    [TestMethod]
    public void TodoListDirectoryBuildProps_ImportsRepositoryBuildProps()
    {
        var root = FindRepositoryRoot();
        var propsPath = Path.Combine(root, "samples", "RazorVue.TodoList", "Directory.Build.props");
        var props = File.ReadAllText(propsPath);

        StringAssert.Contains(props, "<Import Project=\"..\\..\\Directory.Build.props\"");
        StringAssert.Contains(props, "Exists('..\\..\\Directory.Build.props')");
    }

    [TestMethod]
    public void JazorPackageArtifactBuild_PreservesIsolatedBuildPropertiesForInnerProjects()
    {
        var root = FindRepositoryRoot();
        var projectPath = Path.Combine(root, "src", "Jazor", "Jazor.csproj");
        var project = File.ReadAllText(projectPath);

        Assert.IsFalse(
            project.Contains("RemoveProperties=\"TargetFramework;RuntimeIdentifier;SelfContained;PublishSingleFile;PackOnBuild;GeneratePackageOnBuild;NoBuild;PackageOutputPath;JazorIsolatedBaseOutputRoot;JazorIsolatedBaseIntermediateOutputRoot\"", StringComparison.Ordinal),
            "Package artifact builds must not remove isolated output properties from inner project builds.");
        Assert.IsFalse(
            project.Contains("RemoveProperties=\"TargetFramework;RuntimeIdentifier;PackOnBuild;GeneratePackageOnBuild;NoBuild;PackageOutputPath;JazorIsolatedBaseOutputRoot;JazorIsolatedBaseIntermediateOutputRoot\"", StringComparison.Ordinal),
            "Emit artifact publish must not remove isolated output properties from inner project builds.");
    }

    private static string FindRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "Jazor.slnx")))
                return current;

            var parent = Directory.GetParent(current);
            if (parent is null)
                break;

            current = parent.FullName;
        }

        throw new InvalidOperationException("Repository root containing Jazor.slnx could not be located.");
    }
}
