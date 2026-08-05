namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialInheritedGenericSlotProjectionRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorInheritedGenericSlot_UsesDerivedMemberNameForDirectProjectionOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\InheritedReleaseTemplate.razor",
            documentText:
            """
            @using Demo.Components
            @inherits Demo.Shared.ReleaseTemplateBase

            <section data-area="release-template">
                @if (ItemTemplate is not null)
                {
                    @ItemTemplate(Current)
                }
            </section>
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                public sealed record ReleaseEntry(int Id, string Label);
            }

            namespace Demo.Shared
            {
                using Demo.Components;

                public abstract class ReleaseTemplateBase : ComponentBase
                {
                    [Parameter] public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;
                using Demo.Shared;

                [ECMAScriptModule("./components/inherited-release-template-runtime")]
                public partial class InheritedReleaseTemplate : ReleaseTemplateBase, IVueComponent
                {
                    [Parameter]
                    [ECMAScriptName("release-item")]
                    public new RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    private ReleaseEntry Current { get; } = new(34, "Payments API");
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InheritedReleaseTemplate");

        StringAssert.Contains(observation.GeneratedCSharp, "ItemTemplate(Current)", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "slots[\"release-item\"](state.current)", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/inherited-release-template-runtime.mjs",
            observation.ModuleText,
            "official-inherited-release-template-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/inherited-release-template-runtime.mjs";

            test("official Razor inherited generic slots use the derived member name and retain context", () => {
                const withoutSlot = component.setup({}, { slots: {} })();
                assert.equal(withoutSlot.name, "section");
                assert.equal(withoutSlot.props["data-area"], "release-template");
                assert.deepEqual(withoutSlot.children, [null]);

                const render = component.setup({}, {
                    slots: {
                        "release-item": release => [
                            {
                                name: "strong",
                                props: { "data-release-id": release.id },
                                children: ["Inherited: " + release.label]
                            }
                        ]
                    }
                });
                const section = render();
                assert.equal(section.name, "section");
                assert.equal(section.children.length, 1);
                assert.deepEqual(section.children[0], [
                    {
                        name: "strong",
                        props: { "data-release-id": 34 },
                        children: ["Inherited: Payments API"]
                    }
                ]);
            });
            """);
    }
}
