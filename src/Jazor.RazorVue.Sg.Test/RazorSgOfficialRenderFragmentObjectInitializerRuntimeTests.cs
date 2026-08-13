namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialRenderFragmentObjectInitializerRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorBlockDescriptorFactory_ProjectsLocalFragmentFromObjectInitializerOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseSummaryDescriptor.razor",
            documentText:
            """
            @using Demo.Components

            <SlotPanel Header="@BuildSummaryDescriptor().Header" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-panel-object-initializer-runtime")]
                public sealed class SlotPanel : ComponentBase, IVueComponent
                {
                    [Parameter, System.ComponentModel.Description("@#header")] public RenderFragment? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/release-summary-descriptor-runtime")]
                public partial class ReleaseSummaryDescriptor : ComponentBase, IVueComponent
                {
                    [Parameter] public string ReleaseName { get; set; } = string.Empty;

                    private SummaryDescriptor BuildSummaryDescriptor()
                    {
                        RenderFragment header = builder =>
                        {
                            builder.OpenElement(0, "strong");
                            builder.AddAttribute(1, "data-summary", "release");
                            builder.AddContent(2, ReleaseName);
                            builder.CloseElement();
                        };

                        return new SummaryDescriptor
                        {
                            Header = header
                        };
                    }

                    private sealed class SummaryDescriptor
                    {
                        public RenderFragment Header { get; set; } = null!;
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseSummaryDescriptor");

        StringAssert.Contains(observation.GeneratedCSharp, "BuildSummaryDescriptor().Header", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-summary-descriptor-runtime.mjs",
            observation.ModuleText,
            "official-render-fragment-object-initializer-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-summary-descriptor-runtime.mjs";
            import slotPanel from "./components/slot-panel-object-initializer-runtime.mjs";

            test("official Razor object-initialized descriptors project their local slot fragment", () => {
                const panel = component.setup({ ReleaseName: "Gateway rollout" }, { slots: {} })();
                assert.equal(panel.name, slotPanel);
                assert.equal(typeof panel.children.header, "function");

                const nodes = panel.children.header();
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "strong");
                assert.equal(nodes[0].props["data-summary"], "release");
                assert.equal(nodes[0].children, "Gateway rollout");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-panel-object-initializer-runtime.mjs"] = "export default { name: \"slot-panel-object-initializer-runtime\" };"
            });
    }
}
