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
    public void ToolchainCommand_RejectsLegacyRazorVueUpdatePlanArgument()
    {
        var parsed = ToolchainCommand.TryParse(
            [
                "build",
                "--manifest", "manifest.json",
                "--artifacts", "artifacts",
                "--source-root", "src",
                "--out-root", "dist",
                "--write-razorvue-update-plan", "plan.json"
            ],
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

    [TestMethod]
    public void SdkTargets_InvokeFixedNetpackContractForBundles()
    {
        var repositoryRoot = FindRepositoryRoot();
        var targets = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Jazor", "buildTransitive", "Jazor.targets"));
        var props = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Jazor", "buildTransitive", "Jazor.props"));

        StringAssert.Contains(props, "<JazorMode Condition=\"'$(JazorMode)' == ''\">none</JazorMode>", StringComparison.Ordinal);
        StringAssert.Contains(props, "<JazorDir Condition=\"'$(JazorDir)' == ''\">$(MSBuildProjectDirectory)\\jazor\\</JazorDir>", StringComparison.Ordinal);
        Assert.IsFalse(props.Contains("JazorTool", StringComparison.Ordinal), props);
        StringAssert.Contains(targets, "toolchain build --manifest", StringComparison.Ordinal);
        Assert.IsFalse(targets.Contains("--toolchain", StringComparison.Ordinal), targets);
        StringAssert.Contains(targets, "--manifest", StringComparison.Ordinal);
        StringAssert.Contains(targets, "--artifacts", StringComparison.Ordinal);
        StringAssert.Contains(targets, "--source-root", StringComparison.Ordinal);
        StringAssert.Contains(targets, "--out-root", StringComparison.Ordinal);
        Assert.IsFalse(targets.Contains(" bundle --in ", StringComparison.Ordinal), targets);
    }

    [TestMethod]
    public void SdkTargets_ResolveEmitToolForPackageAndRepositoryBuilds()
    {
        var repositoryRoot = FindRepositoryRoot();
        var targets = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Jazor", "buildTransitive", "Jazor.targets"));

        StringAssert.Contains(targets, "<Target Name=\"_ResolveJazorEmitTool\">", StringComparison.Ordinal);
        StringAssert.Contains(targets, "..\\tools\\net11.0\\Jazor.Emit.dll", StringComparison.Ordinal);
        StringAssert.Contains(targets, "<MSBuild Projects=\"@(_JazorEmitToolProjectReference)\"", StringComparison.Ordinal);
        StringAssert.Contains(targets, "Targets=\"GetTargetPath\"", StringComparison.Ordinal);
        StringAssert.Contains(targets, "DependsOnTargets=\"_ResolveJazorEmitTool\"", StringComparison.Ordinal);
        StringAssert.Contains(targets, "Could not locate Jazor.Emit.", StringComparison.Ordinal);
    }

    [TestMethod]
    public void SdkTargets_ExcludeNativeRuntimeAssetsFromEmit()
    {
        var repositoryRoot = FindRepositoryRoot();
        var targets = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Jazor", "buildTransitive", "Jazor.targets"));
        const string managedAssemblyCondition =
            "'%(ReferenceCopyLocalPaths.Extension)' == '.dll' and '%(ReferenceCopyLocalPaths.AssetType)' != 'native'";

        var conditionCount = targets.Split(managedAssemblyCondition, StringSplitOptions.None).Length - 1;

        Assert.AreEqual(2, conditionCount, "Both debug and release emission must exclude native runtime DLLs.");
        Assert.IsFalse(targets.Contains(".Contains('\\native\\')", StringComparison.Ordinal));
    }

    [TestMethod]
    public void SdkTargets_CollectNeutralRuntimeProvidersAndVueAdapterRegistersProviderByContractPath()
    {
        var repositoryRoot = FindRepositoryRoot();
        var targets = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Jazor", "buildTransitive", "Jazor.targets"));
        var vueTargets = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Jazor.Vue", "buildTransitive", "Jazor.Vue.targets"));
        Assert.AreEqual(
            2,
            targets.Split("@(JazorArtifactProviderAssembly)", StringSplitOptions.None).Length - 1,
            "Both debug and release emission must receive adapter-owned runtime providers through the neutral item.");
        Assert.IsFalse(targets.Contains("Jazor.RazorVue", StringComparison.Ordinal), targets);
        StringAssert.Contains(vueTargets, "RegisterJazorVueArtifactProvider", StringComparison.Ordinal);
        StringAssert.Contains(vueTargets, "..\\analyzers\\dotnet\\cs\\Jazor.RazorVue.dll", StringComparison.Ordinal);
        StringAssert.Contains(vueTargets, "..\\..\\Jazor.RazorVue\\bin\\$(Configuration)\\netstandard2.0\\Jazor.RazorVue.dll", StringComparison.Ordinal);
        Assert.IsFalse(vueTargets.Contains("%(Analyzer.Filename)", StringComparison.Ordinal), vueTargets);
        Assert.IsFalse(vueTargets.Contains("%(Analyzer.Extension)", StringComparison.Ordinal), vueTargets);
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
