namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgDirectRenderSlotParameterTests
{
    [TestMethod]
    public async Task BuildComponent_DirectRenderChildContentParameter_UsesVueSlotWithoutBuilderBridge()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Components\ChildContentBridgeRuntime.razor",
            documentText:
            """
            <section data-shell="panel">
                @ChildContent
            </section>
            """,
            codeBehindSource:
            """
            using System.ComponentModel;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/child-content-bridge-runtime")]
                public partial class ChildContentBridgeRuntime : ComponentBase, IVueComponent
                {
                    [Parameter]
                    [Description("@#default")]
                    public RenderFragment? ChildContent { get; set; }
                }
            }
            """,
            rootNamespace: "Demo.Components",
            componentMetadataName: "Demo.Components.ChildContentBridgeRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "slots.default", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("componentProps", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("syncSlotParameters", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/child-content-bridge-runtime.mjs",
            observation.ModuleText,
            "official-child-content-bridge-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/child-content-bridge-runtime.mjs";

            test("direct child content renders the Vue default slot without a builder adapter", () => {
                const render = component.setup({}, {
                  slots: {
                    default: () => [{ name: "strong", props: { "data-slot": "default" }, children: ["Release"] }]
                  }
                });
                const section = render();
                assert.equal(section.name, "section");
                assert.equal(section.props["data-shell"], "panel");
                assert.equal(section.children.length, 1);
                assert.equal(section.children[0].name, "strong");
                assert.deepEqual(section.children[0].children, ["Release"]);
            });
            """);
    }
}
