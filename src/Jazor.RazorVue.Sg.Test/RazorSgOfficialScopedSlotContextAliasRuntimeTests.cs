namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialScopedSlotContextAliasRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorScopedSlotContextAlias_BindsTypedContextOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseListPage.razor"),
            documentText:
            """
            @using Demo.Components

            <ReleaseList>
                <ItemTemplate Context="entry">
                    <article data-release-id="@entry.Id">@entry.Label</article>
                </ItemTemplate>
            </ReleaseList>
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScript]
                public sealed record ReleaseEntry(int Id, string Label);

                [ECMAScriptModule("./components/release-list-scoped-slot-context-alias-runtime")]
                public sealed class ReleaseList : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("item")]
                    [Parameter] public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/release-list-page-scoped-slot-context-alias-runtime")]
                public partial class ReleaseListPage : ComponentBase, IVueComponent
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseListPage");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "item:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-list-page-scoped-slot-context-alias-runtime.mjs",
            observation.ModuleText,
            "official-release-list-scoped-slot-context-alias-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-list-page-scoped-slot-context-alias-runtime.mjs";
            import releaseList from "./components/release-list-scoped-slot-context-alias-runtime.mjs";

            test("official Razor scoped-slot Context aliases bind the declared Vue slot context", () => {
                const list = component.setup({}, { slots: {} })();
                assert.equal(list.name, releaseList);
                assert.equal(typeof list.children.item, "function");

                const nodes = list.children.item({ Id: 42, Label: "Production" });
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "article");
                assert.equal(nodes[0].props["data-release-id"], 42);
                assert.equal(nodes[0].children, "Production");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-list-scoped-slot-context-alias-runtime.mjs"] = "export default { name: \"release-list-scoped-slot-context-alias-runtime\" };"
            });
    }
}
