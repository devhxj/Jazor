namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialNullPatternRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OptionalParameterNullPatternTreatsMissingPropAsNullOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\OptionalLabelRuntime.razor",
            documentText:
            """
            @if (Label is null)
            {
                <span data-state="missing">missing</span>
            }
            else
            {
                <span data-state="present">@Label</span>
            }
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/optional-label-runtime")]
            public partial class OptionalLabelRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Label { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.OptionalLabelRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "label == null", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/optional-label-runtime.mjs",
            observation.ModuleText,
            "official-optional-label-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/optional-label-runtime.mjs";

            test("official Razor optional parameters take the null branch when the host omits the prop", () => {
                const missing = component.setup({}, { slots: {} })();
                assert.equal(missing.name, "__static");
                assert.match(missing.props.html, /data-state="missing"/);

                const present = component.setup({ label: "release" }, { slots: {} })();
                assert.equal(present.name, "span");
                assert.equal(present.props["data-state"], "present");
                assert.deepEqual(present.children, ["release"]);
            });
            """);
    }
}
