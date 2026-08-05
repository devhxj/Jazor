namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialInheritedGenericSlotForwardingRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorInheritedGenericSlot_ForwardsDerivedMemberNameOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\InheritedReleaseTemplateForwarder.razor",
            documentText:
            """
            @using Demo.Components
            @inherits Demo.Shared.ReleaseTemplateBase

            <ReleaseList ItemTemplate="@ItemTemplate" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                public sealed record ReleaseEntry(int Id, string Label);

                [ECMAScriptModule("./components/release-list-inherited-generic-slot-runtime")]
                public sealed class ReleaseList : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("item")]
                    [Parameter] public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
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

                [ECMAScriptModule("./components/inherited-release-template-forwarder-runtime")]
                public partial class InheritedReleaseTemplateForwarder : ReleaseTemplateBase, IVueComponent
                {
                    [Parameter]
                    [ECMAScriptName("forwarded-item")]
                    public new RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InheritedReleaseTemplateForwarder");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.GeneratedCSharp, "RenderFragment<global::Demo.Components.ReleaseEntry>", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "slots[\"forwarded-item\"]", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "item:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/inherited-release-template-forwarder-runtime.mjs",
            observation.ModuleText,
            "official-inherited-generic-slot-forwarder-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/inherited-release-template-forwarder-runtime.mjs";
            import releaseList from "./components/release-list-inherited-generic-slot-runtime.mjs";

            test("official Razor inherited generic slot preserves the derived slot name and context", () => {
                const incoming = component.setup({}, {
                    slots: {
                        "forwarded-item": release => [
                            {
                                name: "strong",
                                props: { "data-release-id": release.id },
                                children: ["Inherited: " + release.label]
                            }
                        ]
                    }
                })();
                assert.equal(incoming.name, releaseList);
                assert.equal(typeof incoming.children.item, "function");
                assert.deepEqual(incoming.children.item({ id: 21, label: "Audit" }), [
                    {
                        name: "strong",
                        props: { "data-release-id": 21 },
                        children: ["Inherited: Audit"]
                    }
                ]);

                const absent = component.setup({}, { slots: {} })();
                assert.equal(absent.name, releaseList);
                assert.equal(absent.children.item, undefined);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-list-inherited-generic-slot-runtime.mjs"] = "export default { name: \"release-list-inherited-generic-slot-runtime\" };"
            });
    }
}
