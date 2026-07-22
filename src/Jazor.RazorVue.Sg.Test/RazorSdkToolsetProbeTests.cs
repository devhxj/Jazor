namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSdkToolsetProbeTests
{
    public TestContext TestContext { get; set; } = default!;

    [TestMethod]
    public void ResolveToolsetProbe_CanLocateExpectedRazorSdkArtifacts()
    {
        var toolset = RazorSdkToolsetProbeResolver.Resolve();

        Assert.IsNotNull(toolset, "The Razor SG test fixture could not locate a usable Razor SDK toolset.");

        TestContext.WriteLine(toolset.Describe());

        Assert.IsTrue(File.Exists(toolset.RazorSourceGeneratorPath), "The resolved Razor source generator binary does not exist.");
        Assert.IsTrue(File.Exists(toolset.RazorTasksPath), "The resolved Razor SDK tasks assembly does not exist.");
        Assert.IsTrue(File.Exists(toolset.RazorDesignTimeTargetsPath), "The resolved Razor design-time targets file does not exist.");
        Assert.IsTrue(File.Exists(toolset.RazorComponentTargetsPath), "The resolved Razor component targets file does not exist.");
    }

    [TestMethod]
    public void LoadedRazorCompilerAssembly_MatchesResolvedSdkSourceGeneratorBinary()
    {
        var toolset = RazorSdkToolsetProbeResolver.Resolve();
        Assert.IsNotNull(toolset, "The Razor SG test fixture could not locate a usable Razor SDK toolset.");

        var loadedAssemblyPath = RazorSgTestHost.GetLoadedRazorCompilerAssemblyPath();
        var loadedAssemblyHash = RazorSgTestHost.ComputeFileSha256(loadedAssemblyPath);
        var resolvedSdkAssemblyHash = RazorSgTestHost.ComputeFileSha256(toolset.RazorSourceGeneratorPath);

        TestContext.WriteLine("Loaded compiler assembly: " + loadedAssemblyPath);
        TestContext.WriteLine("Resolved SDK assembly:   " + toolset.RazorSourceGeneratorPath);
        TestContext.WriteLine("Loaded hash:             " + loadedAssemblyHash);
        TestContext.WriteLine("Resolved SDK hash:       " + resolvedSdkAssemblyHash);

        Assert.AreEqual(
            resolvedSdkAssemblyHash,
            loadedAssemblyHash,
            "The test host is no longer running against the same Razor compiler binary as the resolved SDK source generator path. Re-check whether the spike is still SDK-aligned before interpreting discovery behavior.");
    }
}
