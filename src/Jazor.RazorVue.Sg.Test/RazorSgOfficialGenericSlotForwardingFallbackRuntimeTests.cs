namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialGenericSlotForwardingFallbackRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorGenericSlotForwardingWithFallback_RebindsScopedSlotValueOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseTemplateForwarder.razor",
            documentText:
            """
            @using Demo.Components

            <ReleaseList ItemTemplate="@EffectiveTemplate" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                public sealed record ReleaseEntry(int Id, string Label);

                [ECMAScriptModule("./components/release-list-generic-slot-forwarding-runtime")]
                [VueSlot(nameof(ItemTemplate), Name = "item", ContextTypeName = "Demo.Components.ReleaseEntry", ContextParameterName = "row")]
                public sealed class ReleaseList : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/release-template-forwarder-runtime")]
                [VueProp(nameof(UseIncomingTemplate), Name = "useIncomingTemplate")]
                [VueSlot(nameof(ItemTemplate), Name = "item", ContextTypeName = "Demo.Components.ReleaseEntry", ContextParameterName = "value")]
                public partial class ReleaseTemplateForwarder : ComponentBase, IVueComponent
                {
                    [Parameter] public bool UseIncomingTemplate { get; set; }

                    [Parameter] public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    // The forwarded slot and the fallback lambda intentionally use different
                    // parameter symbols. Vue must invoke the selected fragment with the slot value.
                    private RenderFragment<ReleaseEntry> EffectiveTemplate =>
                        UseIncomingTemplate && ItemTemplate is not null
                            ? ItemTemplate
                            : release => builder =>
                            {
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "data-release-id", release.Id);
                                builder.AddContent(2, "Fallback: " + release.Label);
                                builder.CloseElement();
                            };
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseTemplateForwarder");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "useIncomingTemplate", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "item:", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "Fallback: ", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-template-forwarder-runtime.mjs",
            observation.ModuleText,
            "official-release-template-forwarder-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-template-forwarder-runtime.mjs";
            import releaseList from "./components/release-list-generic-slot-forwarding-runtime.mjs";

            test("official Razor generic slot forwarding preserves incoming and fallback contexts", () => {
                const incoming = component.setup(
                    { useIncomingTemplate: true },
                    {
                        slots: {
                            item: release => [
                                {
                                    name: "strong",
                                    props: { "data-release-id": release.id },
                                    children: ["Incoming: " + release.label]
                                }
                            ]
                        }
                    })();
                assert.equal(incoming.name, releaseList);
                assert.equal(typeof incoming.children.item, "function");
                assert.deepEqual(incoming.children.item({ id: 11, label: "Staging" }), [
                    {
                        name: "strong",
                        props: { "data-release-id": 11 },
                        children: ["Incoming: Staging"]
                    }
                ]);

                const fallback = component.setup(
                    { useIncomingTemplate: false },
                    { slots: {} })();
                assert.equal(fallback.name, releaseList);
                const fallbackNodes = fallback.children.item({ id: 12, label: "Production" });
                assert.equal(fallbackNodes.length, 1);
                assert.equal(fallbackNodes[0].name, "span");
                assert.equal(fallbackNodes[0].props["data-release-id"], 12);
                assert.deepEqual(fallbackNodes[0].children, ["Fallback: Production"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-list-generic-slot-forwarding-runtime.mjs"] = "export default { name: \"release-list-generic-slot-forwarding-runtime\" };"
            });
    }
}
