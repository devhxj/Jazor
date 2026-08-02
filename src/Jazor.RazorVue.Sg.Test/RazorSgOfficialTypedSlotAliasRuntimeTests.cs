namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialTypedSlotAliasRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorTypedSlotAlias_InvokesTheScopedSlotAfterLocalNullGuardOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Components\ReleaseRowTemplate.razor",
            documentText:
            """
            @{
                RenderFragment<ReleaseEntry>? activeTemplate = ItemTemplate;
            }
            @if (activeTemplate is not null)
            {
                @activeTemplate(Current)
            }
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                public sealed record ReleaseEntry(int Id, string Label);

                [ECMAScriptModule("./components/release-row-template")]
                [VueSlot(nameof(ItemTemplate), Name = "item", ContextTypeName = "Demo.Components.ReleaseEntry", ContextParameterName = "release")]
                public partial class ReleaseRowTemplate : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    private ReleaseEntry Current { get; } = new(17, "Deploy");
                }
            }
            """,
            rootNamespace: "Demo.Components",
            componentMetadataName: "Demo.Components.ReleaseRowTemplate");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "const activeTemplate = typeof slots.item", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "activeTemplate(state.current)", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-row-template.mjs",
            observation.ModuleText,
            "official-release-row-template-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-row-template.mjs";

            test("official Razor typed slot aliases retain the nullable component parameter contract", () => {
                const absent = component.setup({}, { slots: {} })();
                assert.equal(absent, null);

                const present = component.setup({}, {
                    slots: {
                        item: release => [{
                            name: "span",
                            props: { "data-release-id": release.id },
                            children: [release.label]
                        }]
                    }
                })();
                assert.equal(Array.isArray(present), true);
                assert.equal(present.length, 1);
                assert.equal(present[0].name, "span");
                assert.equal(present[0].props["data-release-id"], 17);
                assert.deepEqual(present[0].children, ["Deploy"]);
            });
            """);
    }
}
