using Jazor.RazorVue.RazorSdk;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueHmrMetadataTests
{
    [TestMethod]
    public void BoundaryKindWireValues_RemainStableAcrossCatalogConsumers()
    {
        Assert.AreEqual("unknown", VueHmrBoundaryKind.Unknown.ToWireValue());
        Assert.AreEqual("template-only", VueHmrBoundaryKind.TemplateOnly.ToWireValue());
        Assert.AreEqual("logic-safe", VueHmrBoundaryKind.LogicSafe.ToWireValue());
        Assert.AreEqual("full-reload-required", VueHmrBoundaryKind.FullReloadRequired.ToWireValue());
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazor_RegistersVueHmrComponentWithStableModuleId()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/HmrCounter.razor"),
            documentText: "<span>counter</span>",
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/hmr-counter")]
            public partial class HmrCounter : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.HmrCounter");

        StringAssert.Contains(observation.ModuleText, "const __jazorComponent = defineComponent(", StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "globalThis.JazorHmr && typeof globalThis.JazorHmr.registerVueComponent === \"function\"",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "globalThis.JazorHmr.registerVueComponent(\"RazorSg.OfficialAuthoring.Tests:components/hmr-counter.js\", __jazorComponent);",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "import.meta.hot.accept", StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "__jazorComponent.__hmrId = \"RazorSg.OfficialAuthoring.Tests:components/hmr-counter.js\";",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "runtime.reload(\"RazorSg.OfficialAuthoring.Tests:components/hmr-counter.js\", updated.default)", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "export default __jazorComponent;", StringComparison.Ordinal);
    }
}
