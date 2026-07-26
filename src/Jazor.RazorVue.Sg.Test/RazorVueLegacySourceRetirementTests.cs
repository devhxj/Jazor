using Jazor.RazorVue.RazorSdk;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueLegacySourceRetirementTests
{
    [TestMethod]
    public void RazorVueAssembly_DoesNotExposeRetiredRazorToSfcOrIrPipelineTypes()
    {
        var assembly = typeof(RazorSgGeneratedCSharpBinder).Assembly;

        var retiredTypeNames = new[]
        {
            "Jazor.RazorVue.Artifacts.RazorVueCatalog",
            "Jazor.RazorVue.Artifacts.RazorVueSfcCatalog",
            "Jazor.RazorVue.JazorVueCompiler",
            "Jazor.RazorVue.JazorVuePipeline",
            "Jazor.RazorVue.JazorVueSfcPipeline",
            "Jazor.RazorVue.Lowering.RazorVueArtifactFactory",
            "Jazor.RazorVue.Lowering.RazorVueSfcArtifactFactory",
            "Jazor.RazorVue.Emit.RazorVueManifestModel",
            "Jazor.RazorVue.Emit.RazorVueManifestEntry",
            "Jazor.RazorVue.Emit.RazorVueManifestSerializer",
            "Jazor.RazorVue.Emit.RazorVueManifestLoadStatus",
            "Jazor.RazorVue.Emit.RazorVueManifestLoadResult",
            "Jazor.RazorVue.Emit.RazorVueManifestDiffer",
            "Jazor.RazorVue.Emit.RazorVueManifestDiffResult",
            "Jazor.RazorVue.Emit.RazorVueManifestModuleDiff",
            "Jazor.RazorVue.Emit.RazorVueHotUpdateAction",
            "Jazor.RazorVue.RazorVueHmrBoundaryKind",
            "Jazor.RazorVue.RazorSdk.RazorVueLegacyIrFirstTemplateFrontend",
            "Jazor.RazorVue.RazorSdk.RazorVueRazorDocumentSemanticFrontend",
            "Jazor.RazorVue.RazorSdk.RazorVueRazorIrTemplateFrontend",
            "Jazor.RazorVue.RazorSdk.RazorVueRazorSourceGeneratorTailBridge",
            "Jazor.RazorVue.RazorSdk.RazorVueReflectedRazorIrReader",
            "Jazor.RazorVue.RenderTree.RazorVueRenderTreeExtractor"
        };

        foreach (var retiredTypeName in retiredTypeNames)
        {
            Assert.IsNull(
                assembly.GetType(retiredTypeName, throwOnError: false),
                $"The retired Razor-to-SFC/IR pipeline type '{retiredTypeName}' must not re-enter the Razor SG result path.");
        }
    }

    [TestMethod]
    public void AnalyzerAssembly_DoesNotExposeRetiredRazorVueAuthoringOrRpcTypes()
    {
        var assembly = typeof(Jazor.Analyzer.Analyzer).Assembly;

        var retiredTypeNames = new[]
        {
            "Jazor.Analyzer.RazorVueAuthoringAnalyzer",
            "Jazor.Analyzer.RazorVueDiagnosticDescriptors",
            "Jazor.Analyzer.RazorVueEntryAnalyzer",
            "Jazor.Analyzer.RazorVueKnownSymbols",
            "Jazor.Analyzer.RazorVueMisuseAnalyzer",
            "Jazor.Vue.IVueAnalysisClient",
            "Jazor.Vue.IVueAnalysisRpcProcessor",
            "Jazor.Vue.IVueAnalysisRpcService",
            "Jazor.Vue.InProcJazorVueAnalysisRuntime",
            "Jazor.Vue.JazorVueAnalysisService",
            "Jazor.Vue.StdioVueAnalysisRpcServer",
            "Jazor.Vue.VueAnalysisRpcException",
            "Jazor.Vue.VueAnalysisRpcProcessor",
            "Jazor.Vue.VueAnalysisRpcSerializer"
        };

        foreach (var retiredTypeName in retiredTypeNames)
        {
            Assert.IsNull(
                assembly.GetType(retiredTypeName, throwOnError: false),
                $"The retired RazorVue analyzer/RPC type '{retiredTypeName}' must not re-enter the analyzer assembly.");
        }
    }
}
