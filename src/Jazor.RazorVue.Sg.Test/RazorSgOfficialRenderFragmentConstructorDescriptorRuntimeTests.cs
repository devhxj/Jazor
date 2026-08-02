namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialRenderFragmentConstructorDescriptorRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorConstructorContentDescriptor_ProjectsMethodGroupSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ConstructorContentDescriptorRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <SlotPanel Header="@BuildPanelContent().Header" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-panel-constructor-descriptor-runtime")]
                [VueSlot(nameof(Header), Name = "header")]
                public sealed class SlotPanel : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/constructor-content-descriptor-runtime")]
                public partial class ConstructorContentDescriptorRuntime : ComponentBase, IVueComponent
                {
                    private string ReleaseName { get; } = "API gateway";

                    private SlotContent BuildPanelContent()
                        => new(RenderHeader, "release-details");

                    private void RenderHeader(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "strong");
                        builder.AddAttribute(1, "data-scope", "release-details");
                        builder.AddContent(2, ReleaseName);
                        builder.CloseElement();
                    }

                    private sealed class SlotContent
                    {
                        public SlotContent(RenderFragment header, string scope)
                        {
                            Header = header;
                            Scope = scope;
                        }

                        public RenderFragment Header { get; }

                        public string Scope { get; }
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ConstructorContentDescriptorRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "BuildPanelContent().Header", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/constructor-content-descriptor-runtime.mjs",
            observation.ModuleText,
            "official-constructor-content-descriptor-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/constructor-content-descriptor-runtime.mjs";
            import slotPanel from "./components/slot-panel-constructor-descriptor-runtime.mjs";

            test("official Razor constructor content descriptor provides its named slot", () => {
                const panel = component.setup({}, { slots: {} })();
                assert.equal(panel.name, slotPanel);
                assert.equal(typeof panel.children.header, "function");

                const nodes = panel.children.header();
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "strong");
                assert.equal(nodes[0].props["data-scope"], "release-details");
                assert.deepEqual(nodes[0].children, ["API gateway"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-panel-constructor-descriptor-runtime.mjs"] = "export default { name: \"slot-panel-constructor-descriptor-runtime\" };"
            });
    }
}
