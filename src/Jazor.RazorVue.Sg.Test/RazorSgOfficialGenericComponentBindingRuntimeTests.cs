namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialGenericComponentBindingRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorNestedGenericTypeInference_RendersInsideSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/NestedGenericInferenceRuntime.razor"),
            documentText:
            """
            @using Demo.Components

            <SlotHost>
                <GenericValue Value="@Value" />
            </SlotHost>
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/nested-generic-value-runtime")]
                public sealed class GenericValue<TValue> : ComponentBase, IVueComponent
                {
                    [Parameter] public TValue Value { get; set; } = default!;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }

                [ECMAScriptModule("./components/nested-generic-slot-host-runtime")]
                public sealed class SlotHost : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/nested-generic-inference-runtime")]
                public partial class NestedGenericInferenceRuntime : ComponentBase, IVueComponent
                {
                    private string Value { get; set; } = "inferred";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NestedGenericInferenceRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "TypeInference.CreateGenericValue_0", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "nested-generic-slot-host-runtime.mjs", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "nested-generic-value-runtime.mjs", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("__builder", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/nested-generic-inference-runtime.mjs",
            observation.ModuleText,
            "official-nested-generic-inference-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/nested-generic-inference-runtime.mjs";
            import slotHost from "./components/nested-generic-slot-host-runtime.mjs";
            import genericValue from "./components/nested-generic-value-runtime.mjs";

            test("official Razor generic TypeInference is evaluated in the active slot builder scope", () => {
                const host = component.setup({}, { slots: {} })();
                assert.equal(host.name, slotHost);
                assert.equal(typeof host.children.ChildContent, "function");

                const children = host.children.ChildContent();
                assert.equal(children.length, 1);
                assert.equal(children[0].name, genericValue);
                assert.equal(children[0].props.Value, "inferred");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/nested-generic-slot-host-runtime.mjs"] = "export default { name: \"nested-generic-slot-host-runtime\" };",
                ["components/nested-generic-value-runtime.mjs"] = "export default { name: \"nested-generic-value-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorGenericComponentBinding_UpdatesConstructedModelOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseEditorHost.razor"),
            documentText:
            """
            @using Demo.Components

            <ReleaseEditor TValue="string" @bind-Value="ReleaseName" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/release-editor-generic-runtime")]
                public sealed class ReleaseEditor<TValue> : ComponentBase, IVueComponent
                {
                    [System.ComponentModel.Description("@#modelValue")]
                    [Parameter] public TValue Value { get; set; } = default!;

                    [Parameter, System.ComponentModel.Description("@#onUpdate:modelValue")] public EventCallback<TValue> ValueChanged { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/release-editor-host-generic-runtime")]
                public partial class ReleaseEditorHost : ComponentBase, IVueComponent
                {
                    private string ReleaseName { get; set; } = "Queued";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseEditorHost");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "global::Demo.Components.ReleaseEditor<",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredEventCallback", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "modelValue: state.ReleaseName", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onUpdate:modelValue", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-editor-host-generic-runtime.mjs",
            observation.ModuleText,
            "official-release-editor-host-generic-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-editor-host-generic-runtime.mjs";
            import releaseEditor from "./components/release-editor-generic-runtime.mjs";

            test("official Razor generic component bindings preserve the constructed model contract", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.name, releaseEditor);
                assert.equal(initial.props.modelValue, "Queued");
                assert.equal(typeof initial.props["onUpdate:modelValue"], "function");

                initial.props["onUpdate:modelValue"]("Released");

                const updated = render();
                assert.equal(updated.name, releaseEditor);
                assert.equal(updated.props.modelValue, "Released");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-editor-generic-runtime.mjs"] = "export default { name: \"release-editor-generic-runtime\" };"
            });
    }
}
