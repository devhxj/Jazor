namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialRecursiveRenderFragmentRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorRecursiveRenderFragmentFactory_RendersNestedSlotContentOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/RecursiveTemplateRuntime.razor"),
            documentText:
            """
            @using Demo.Components

            <TreePanel Content="@RenderNested(Label, Depth)" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/tree-panel-recursive-template-runtime")]
                public sealed class TreePanel : ComponentBase, IVueComponent
                {
                    [Parameter, System.ComponentModel.Description("@#content")] public RenderFragment? Content { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/recursive-template-runtime")]
                public partial class RecursiveTemplateRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public string Label { get; set; } = "Release";
                    [Parameter] public int Depth { get; set; } = 2;

                    private RenderFragment RenderNested(string label, int depth)
                    {
                        return builder =>
                        {
                            builder.OpenElement(0, "li");
                            builder.AddAttribute(1, "data-depth", depth);
                            builder.AddContent(2, label + depth);
                            if (depth > 0)
                            {
                                builder.AddContent(3, RenderNested(label, depth - 1));
                            }
                            builder.CloseElement();
                        };
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.RecursiveTemplateRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "RenderNested(Label, Depth)", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "renderRenderNested", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "content:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/recursive-template-runtime.mjs",
            observation.ModuleText,
            "official-recursive-template-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/recursive-template-runtime.mjs";
            import treePanel from "./components/tree-panel-recursive-template-runtime.mjs";

            test("official Razor recursive RenderFragment factory preserves nested vnode order", () => {
                const panel = component.setup({ Label: "Deploy", Depth: 2 }, { slots: {} })();
                assert.equal(panel.name, treePanel);
                assert.equal(typeof panel.children.content, "function");

                const roots = panel.children.content();
                assert.equal(roots.length, 1);
                const root = roots[0];
                assert.equal(root.name, "li");
                assert.equal(root.props["data-depth"], 2);
                assert.equal(root.children[0], "Deploy2");

                const child = root.children[1];
                assert.equal(child.name, "li");
                assert.equal(child.props["data-depth"], 1);
                assert.equal(child.children[0], "Deploy1");

                const leaf = child.children[1];
                assert.equal(leaf.name, "li");
                assert.equal(leaf.props["data-depth"], 0);
                assert.deepEqual(leaf.children, ["Deploy0", null]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/tree-panel-recursive-template-runtime.mjs"] = "export default { name: \"tree-panel-recursive-template-runtime\" };"
            });
    }
}
