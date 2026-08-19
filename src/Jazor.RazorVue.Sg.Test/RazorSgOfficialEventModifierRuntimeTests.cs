namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialEventModifierRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorDynamicFormEventModifiers_ApplyOnlyEnabledGuardsBeforeTheHandlerOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseDeployForm.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <form data-submissions="@SubmissionCount"
                  @onsubmit="Submit"
                  @onsubmit:preventDefault="@PreventNativeSubmit"
                  @onsubmit:stopPropagation="@StopSubmit">
                <button type="submit">Deploy</button>
            </form>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-deploy-form-event-modifiers-runtime")]
            public partial class ReleaseDeployForm : ComponentBase, IVueComponent
            {
                [Parameter] public bool PreventNativeSubmit { get; set; }

                [Parameter] public bool StopSubmit { get; set; }

                private int SubmissionCount { get; set; }

                private void Submit()
                {
                    SubmissionCount++;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseDeployForm");

        StringAssert.Contains(observation.GeneratedCSharp, "AddEventPreventDefaultAttribute", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddEventStopPropagationAttribute", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "props.PreventNativeSubmit", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "props.StopSubmit", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-deploy-form-event-modifiers-runtime.mjs",
            observation.ModuleText,
            "official-release-deploy-form-event-modifiers-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-deploy-form-event-modifiers-runtime.mjs";

            test("official Razor dynamic form modifiers run before the submit callback", () => {
                const enabledRender = component.setup({
                    PreventNativeSubmit: true,
                    StopSubmit: true
                }, { slots: {} });
                const enabledForm = enabledRender();
                const calls = [];
                enabledForm.props.onSubmit({
                    preventDefault() { calls.push("prevent"); },
                    stopPropagation() { calls.push("stop"); }
                });
                assert.deepEqual(calls, ["prevent", "stop"]);
                assert.equal(enabledRender().props["data-submissions"], 1);

                const disabledRender = component.setup({
                    PreventNativeSubmit: false,
                    StopSubmit: false
                }, { slots: {} });
                const disabledForm = disabledRender();
                const disabledCalls = [];
                disabledForm.props.onSubmit({
                    preventDefault() { disabledCalls.push("prevent"); },
                    stopPropagation() { disabledCalls.push("stop"); }
                });
                assert.deepEqual(disabledCalls, []);
                assert.equal(disabledRender().props["data-submissions"], 1);
            });
            """);
    }
}
