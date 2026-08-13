namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialCheckboxBindingRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorCheckboxBinding_UpdatesBooleanStateFromCheckedTargetOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseApprovalToggle.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input type="checkbox" @bind="Approved" data-approved="@Approved" />
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-approval-toggle-runtime")]
            public partial class ReleaseApprovalToggle : ComponentBase, IVueComponent
            {
                private bool Approved { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseApprovalToggle");

        StringAssert.Contains(observation.GeneratedCSharp, "\"checked\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "SetUpdatesAttributeName(\"checked\")", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "event => state.Approved = event.target[\"checked\"]",
            StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("eventOrValue", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-approval-toggle-runtime.mjs",
            observation.ModuleText,
            "official-release-approval-toggle-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-approval-toggle-runtime.mjs";

            test("official Razor checkbox binding reads target.checked and keeps state in sync", async () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.name, "input");
                assert.equal(initial.props.type, "checkbox");
                assert.equal(initial.props.checked, false);
                assert.equal(initial.props["data-approved"], false);
                assert.equal(typeof initial.props.onChange, "function");

                await Promise.resolve(initial.props.onChange({ target: { checked: true } }));

                const approved = render();
                assert.equal(approved.props.checked, true);
                assert.equal(approved.props["data-approved"], true);

                await Promise.resolve(approved.props.onChange({ target: { checked: false } }));

                const unapproved = render();
                assert.equal(unapproved.props.checked, false);
                assert.equal(unapproved.props["data-approved"], false);
            });
            """);
    }
}
