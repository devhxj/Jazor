using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class LegacyRazorVueContractRetirementTests
{
    [TestMethod]
    public void EmitAssembly_DoesNotExposeLegacyRazorVueCatalogOrConsumerContracts()
    {
        var assembly = typeof(ModuleCollector).Assembly;

        foreach (var typeName in new[]
                 {
                     "Jazor.Emit.RazorVueCatalogReader",
                     "Jazor.Emit.RazorVueModuleWriter",
                     "Jazor.Emit.RazorVueSfcCatalogReader",
                     "Jazor.Emit.RazorVueSfcModuleWriter",
                     "Jazor.Emit.RazorVueSfcBridgeCompiler",
                     "Jazor.Emit.RazorVueConsumerEntryCompiler",
                     "Jazor.Emit.RazorVueHostRequirementsModuleWriter",
                     "Jazor.Emit.RazorVueHostAssetWriter",
                     "Jazor.Emit.RazorVueUpdatePlanWriter"
                 })
        {
            Assert.IsNull(assembly.GetType(typeName, throwOnError: false, ignoreCase: false), typeName);
        }
    }

    [TestMethod]
    public void BundleOptions_RejectLegacyRazorVueUpdatePlanArgument()
    {
        var parsed = BundleOptions.TryParse(
            ["--in", "input", "--manifest", "manifest.json", "--out", "bundle.mjs", "--write-razorvue-update-plan", "plan.json"],
            out _,
            out var error);

        Assert.IsFalse(parsed);
        StringAssert.Contains(error, "--write-razorvue-update-plan");
    }

    [TestMethod]
    public void SdkTargets_DoNotDefineLegacyRazorVueConsumerOrHostContracts()
    {
        var repositoryRoot = FindRepositoryRoot();
        var targets = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Jazor", "buildTransitive", "Jazor.targets"));
        var props = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Jazor", "buildTransitive", "Jazor.props"));

        foreach (var legacyContract in new[]
                 {
                     "JazorConsumer",
                     "JazorRazorVueHostRequirementsModulePath",
                     "JazorBundleRazorVue",
                     "write-razorvue-update-plan"
                 })
        {
            Assert.IsFalse(targets.Contains(legacyContract, StringComparison.Ordinal), legacyContract);
            Assert.IsFalse(props.Contains(legacyContract, StringComparison.Ordinal), legacyContract);
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
        }

        throw new InvalidOperationException("Could not locate the repository root.");
    }
}
