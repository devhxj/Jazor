namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialGenericRecursiveRenderFragmentRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorRecursiveScopedSlot_PreservesContextAndNestedChildrenOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/NavigationTemplateRuntime.razor"),
            documentText:
            """
            @using Demo.Components

            <NavigationPanel ItemTemplate="@RenderNode(MaxDepth)" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScript]
                public sealed class NavigationNode
                {
                    public string Title { get; set; } = string.Empty;
                    public NavigationNode? Child { get; set; }
                }

                [ECMAScriptModule("./components/navigation-panel-recursive-scoped-slot-runtime")]
                public sealed class NavigationPanel : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("item")]
                    [Parameter] public RenderFragment<NavigationNode>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/navigation-template-runtime")]
                public partial class NavigationTemplateRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public int MaxDepth { get; set; } = 2;

                    private RenderFragment<NavigationNode> RenderNode(int remainingDepth)
                    {
                        return node => builder =>
                        {
                            builder.OpenElement(0, "li");
                            builder.AddAttribute(1, "data-remaining-depth", remainingDepth);
                            builder.AddContent(2, node.Title);
                            if (node.Child is not null && remainingDepth > 0)
                            {
                                builder.AddContent(3, RenderNode(remainingDepth - 1), node.Child);
                            }
                            builder.CloseElement();
                        };
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NavigationTemplateRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "RenderNode(MaxDepth)", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "renderRenderNode", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "item:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/navigation-template-runtime.mjs",
            observation.ModuleText,
            "official-navigation-template-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/navigation-template-runtime.mjs";
            import navigationPanel from "./components/navigation-panel-recursive-scoped-slot-runtime.mjs";

            test("official Razor recursive scoped slot keeps the current node context", () => {
                const panel = component.setup({ MaxDepth: 2 }, { slots: {} })();
                assert.equal(panel.name, navigationPanel);
                assert.equal(typeof panel.children.item, "function");

                const roots = panel.children.item({
                    Title: "Workspace",
                    Child: {
                        Title: "Releases",
                        Child: {
                            Title: "Deployment",
                            Child: null
                        }
                    }
                });

                const root = roots[0];
                assert.equal(root.name, "li");
                assert.equal(root.props["data-remaining-depth"], 2);
                assert.equal(root.children[0], "Workspace");

                const release = root.children[1];
                assert.equal(release.name, "li");
                assert.equal(release.props["data-remaining-depth"], 1);
                assert.equal(release.children[0], "Releases");

                const deployment = release.children[1];
                assert.equal(deployment.name, "li");
                assert.equal(deployment.props["data-remaining-depth"], 0);
                assert.deepEqual(deployment.children, ["Deployment", null]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/navigation-panel-recursive-scoped-slot-runtime.mjs"] = "export default { name: \"navigation-panel-recursive-scoped-slot-runtime\" };"
            });
    }
}
