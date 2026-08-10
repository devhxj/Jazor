namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialConditionalContentRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorConditionalContent_UsesFragmentOnlyForTheMultiNodeBranchOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseDetailsToggle.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            @if (ShowDetails)
            {
                <strong data-release="@ReleaseName">@ReleaseName</strong>
                <button type="button" @onclick="ToggleDetails">Hide details</button>
            }
            else
            {
                <span data-release="@ReleaseName">Details hidden</span>
            }
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-details-toggle-runtime")]
            public partial class ReleaseDetailsToggle : ComponentBase, IVueComponent
            {
                private string ReleaseName { get; set; } = "Accounts API";

                private bool ShowDetails { get; set; } = true;

                private void ToggleDetails()
                {
                    ShowDetails = !ShowDetails;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseDetailsToggle");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "Fragment", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "ToggleDetails", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-details-toggle-runtime.mjs",
            observation.ModuleText,
            "official-release-details-toggle-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { Fragment } from "vue";

            import component from "./components/release-details-toggle-runtime.mjs";

            test("official Razor conditional content creates a fragment only for multiple active nodes", () => {
                const render = component.setup({}, { slots: {} });
                const details = render();
                assert.equal(details.name, Fragment);
                const heading = details.children.find(node => node?.name === "strong");
                const toggle = details.children.find(node => node?.name === "button");
                assert.ok(heading);
                assert.ok(toggle);
                assert.equal(heading.props["data-release"], "Accounts API");
                assert.deepEqual(heading.children, ["Accounts API"]);
                assert.equal(typeof toggle.props.onClick, "function");

                toggle.props.onClick();

                const hidden = render();
                assert.equal(hidden.name, "span");
                assert.equal(hidden.props["data-release"], "Accounts API");
                assert.deepEqual(hidden.children, ["Details hidden"]);
            });
            """);
    }
}
