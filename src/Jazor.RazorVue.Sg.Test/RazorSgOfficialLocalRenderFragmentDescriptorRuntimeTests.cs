namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialLocalRenderFragmentDescriptorRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLocalTemplateDescriptor_ProjectsCachedFragmentMemberOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseSummaryLocalDescriptor.razor",
            documentText:
            """
            @using Demo.Components

            @{
                var descriptor = BuildSummaryDescriptor();
            }

            <SlotPanel Header="@descriptor.Header" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-panel-local-descriptor-runtime")]
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

                [ECMAScriptModule("./components/release-summary-local-descriptor-runtime")]
                public partial class ReleaseSummaryLocalDescriptor : ComponentBase, IVueComponent
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
                        : SummaryDescriptorBase
                    {
                    }

                    private abstract class SummaryDescriptorBase
                    {
                        public RenderFragment Header { get; set; } = null!;
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseSummaryLocalDescriptor");

        StringAssert.Contains(observation.GeneratedCSharp, "var descriptor = BuildSummaryDescriptor();", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "descriptor.Header", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-summary-local-descriptor-runtime.mjs",
            observation.ModuleText,
            "official-render-fragment-local-descriptor-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-summary-local-descriptor-runtime.mjs";
            import panel from "./components/slot-panel-local-descriptor-runtime.mjs";

            test("official Razor local template descriptors retain their fragment provenance", () => {
                const rendered = component.setup(
                    { ReleaseName: "Gateway rollout" },
                    { slots: {} })();
                assert.equal(rendered.name, panel);
                assert.equal(typeof rendered.children.header, "function");

                const nodes = rendered.children.header();
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "strong");
                assert.equal(nodes[0].props["data-summary"], "release");
                assert.equal(nodes[0].children, "Gateway rollout");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-panel-local-descriptor-runtime.mjs"] = "export default { name: \"slot-panel-local-descriptor-runtime\" };"
            });
    }
}
