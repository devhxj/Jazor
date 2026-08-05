namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialLocalGenericSlotAliasRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLocalGenericSlotAlias_ExpandsScopedSlotResultsInLoopOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseTemplateAlias.razor",
            documentText:
            """
            @{
                var template = ItemTemplate;
            }

            <ul data-release-list="pending">
                @foreach (var release in Releases)
                {
                    @template(release)
                }
            </ul>
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Pages;

            public sealed record ReleaseEntry(string Name, int Pending);

            [ECMAScriptModule("./components/release-template-alias-runtime")]
            public partial class ReleaseTemplateAlias : ComponentBase, IVueComponent
            {
                [ECMAScriptName("item")]
                [Parameter] public RenderFragment<ReleaseEntry> ItemTemplate { get; set; } = default!;

                private readonly ReleaseEntry[] Releases =
                [
                    new("Deploy API", 2),
                    new("Publish portal", 1)
                ];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseTemplateAlias");

        StringAssert.Contains(observation.GeneratedCSharp, "template(release)", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "slots.item", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-template-alias-runtime.mjs",
            observation.ModuleText,
            "official-release-template-alias-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-template-alias-runtime.mjs";

            test("official Razor local RenderFragment<T> aliases retain every scoped slot result", () => {
                const list = component.setup({}, {
                    slots: {
                        item: release => [
                            {
                                name: "li",
                                props: {
                                    "data-release": release.name,
                                    "data-pending": release.pending
                                },
                                children: [release.name]
                            }
                        ]
                    }
                })();

                assert.equal(list.name, "ul");
                assert.equal(list.props["data-release-list"], "pending");
                assert.equal(list.children.length, 1);
                assert.equal(Array.isArray(list.children[0]), true);
                assert.equal(list.children[0].length, 2);
                const [firstSlotNodes, secondSlotNodes] = list.children[0];
                assert.equal(firstSlotNodes[0].name, "li");
                assert.equal(firstSlotNodes[0].props["data-release"], "Deploy API");
                assert.equal(firstSlotNodes[0].props["data-pending"], 2);
                assert.deepEqual(secondSlotNodes[0].children, ["Publish portal"]);
            });
            """);
    }
}
