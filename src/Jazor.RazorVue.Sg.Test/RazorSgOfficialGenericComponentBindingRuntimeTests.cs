namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialGenericComponentBindingRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorGenericComponentBinding_UpdatesConstructedModelOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseEditorHost.razor",
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
                    [ECMAScriptName("modelValue")]
                    [Parameter] public TValue Value { get; set; } = default!;

                    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

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
        StringAssert.Contains(observation.ModuleText, "modelValue: state.releaseName", StringComparison.Ordinal);
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
