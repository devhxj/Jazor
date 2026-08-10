namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgDirectRenderTypedSlotParameterTests
{
    [TestMethod]
    public async Task BuildComponent_DirectRenderTypedTemplateParameter_InvokesScopedVueSlotWithoutBuilderBridge()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Components\TypedTemplateBridgeRuntime.razor",
            documentText:
            """
            <section data-shell="panel">
                @if (ItemTemplate is not null)
                {
                    @ItemTemplate(Current)
                }
            </section>
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                public sealed record ReleaseEntry(int Id, string Label);

                [ECMAScriptModule("./components/typed-template-bridge-runtime")]
                public partial class TypedTemplateBridgeRuntime : ComponentBase, IVueComponent
                {
                    [Parameter]
                    [ECMAScriptName("item")]
                    public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    private ReleaseEntry Current { get; } = new(42, "Deploy");
                }
            }
            """,
            rootNamespace: "Demo.Components",
            componentMetadataName: "Demo.Components.TypedTemplateBridgeRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "slots.item(state.Current)", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("componentProps", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("syncSlotParameters", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/typed-template-bridge-runtime.mjs",
            observation.ModuleText,
            "official-typed-template-bridge-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/typed-template-bridge-runtime.mjs";

            test("direct typed template renders the Vue scoped slot with the current context", () => {
                const withoutTemplate = component.setup({}, { slots: {} })();
                assert.deepEqual(withoutTemplate.children, [null]);

                const render = component.setup({}, {
                  slots: {
                    item: release => [{
                      name: "strong",
                      props: { "data-release-id": release.Id },
                      children: [release.Label]
                    }]
                  }
                });
                const section = render();
                assert.equal(section.name, "section");
                assert.equal(section.props["data-shell"], "panel");
                assert.equal(section.children.length, 1);
                const [templateNodes] = section.children;
                assert.equal(Array.isArray(templateNodes), true);
                assert.equal(templateNodes.length, 1);
                assert.equal(templateNodes[0].name, "strong");
                assert.equal(templateNodes[0].props["data-release-id"], 42);
                assert.deepEqual(templateNodes[0].children, ["Deploy"]);
            });
            """);
    }
}
