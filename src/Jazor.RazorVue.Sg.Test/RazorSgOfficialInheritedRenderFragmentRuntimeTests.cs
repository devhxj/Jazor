namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialInheritedRenderFragmentRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorInheritedRenderFragmentMethodGroup_ProjectsBaseTemplateSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/InheritedTemplateRuntime.razor"),
            documentText:
            """
            @using Demo.Components
            @inherits Demo.Shared.ReleaseTemplateBase

            <SlotPanel Header="@RenderHeader" Footer="@CreateFooter()" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-panel-inherited-template-runtime")]
                public sealed class SlotPanel : ComponentBase, IVueComponent
                {
                    [Parameter, System.ComponentModel.Description("@#header")] public RenderFragment? Header { get; set; }
                    [Parameter, System.ComponentModel.Description("@#footer")] public RenderFragment? Footer { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Shared
            {
                public abstract class ReleaseTemplateBase : ComponentBase
                {
                    protected static void RenderHeader(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "strong");
                        builder.AddAttribute(1, "data-origin", "base");
                        builder.AddContent(2, "Inherited release template");
                        builder.CloseElement();
                    }

                    protected RenderFragment CreateFooter()
                        => builder =>
                        {
                            builder.OpenElement(0, "small");
                            builder.AddAttribute(1, "data-origin", "base-factory");
                            builder.AddContent(2, "Inherited release footer");
                            builder.CloseElement();
                        };
                }
            }

            namespace Demo.Pages
            {
                using Demo.Shared;

                [ECMAScriptModule("./components/inherited-template-runtime")]
                public partial class InheritedTemplateRuntime : ReleaseTemplateBase, IVueComponent
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InheritedTemplateRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "RenderHeader", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "footer:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/inherited-template-runtime.mjs",
            observation.ModuleText,
            "official-inherited-template-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/inherited-template-runtime.mjs";
            import slotPanel from "./components/slot-panel-inherited-template-runtime.mjs";

            test("official Razor inherited RenderFragment members provide base slots", () => {
                const panel = component.setup({}, { slots: {} })();
                assert.equal(panel.name, slotPanel);
                assert.equal(typeof panel.children.header, "function");

                const nodes = panel.children.header();
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "strong");
                assert.equal(nodes[0].props["data-origin"], "base");
                assert.deepEqual(nodes[0].children, [{ name: "__text", children: "Inherited release template", patchFlag: undefined }]);

                assert.equal(typeof panel.children.footer, "function");
                const footerNodes = panel.children.footer();
                assert.equal(footerNodes.length, 1);
                assert.equal(footerNodes[0].name, "small");
                assert.equal(footerNodes[0].props["data-origin"], "base-factory");
                assert.deepEqual(footerNodes[0].children, [{ name: "__text", children: "Inherited release footer", patchFlag: undefined }]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-panel-inherited-template-runtime.mjs"] = "export default { name: \"slot-panel-inherited-template-runtime\" };"
            });
    }
}
