namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialBindSetMethodGroupRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorBindSetMethodGroup_UpdatesStateThroughAsyncSetterOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseNameEditor.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @bind:get="ReleaseName" @bind:set="SetReleaseNameAsync" @bind:event="oninput" data-saved="@SavedReleaseName" />
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-name-editor-runtime")]
            public partial class ReleaseNameEditor : ComponentBase, IVueComponent
            {
                private string ReleaseName { get; set; } = "Draft release";

                private string SavedReleaseName { get; set; } = "none";

                private Task SetReleaseNameAsync(string value)
                {
                    ReleaseName = value.Trim();
                    SavedReleaseName = ReleaseName;
                    return Task.CompletedTask;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseNameEditor");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredBindSetter", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "SetReleaseNameAsync", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "SetReleaseNameAsync(__value)", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "state.ReleaseName", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "state.SavedReleaseName", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-name-editor-runtime.mjs",
            observation.ModuleText,
            "official-release-name-editor-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-name-editor-runtime.mjs";

            test("official Razor bind:set method group receives the DOM value and updates state", async () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.name, "input");
                assert.equal(initial.props.value, "Draft release");
                assert.equal(initial.props["data-saved"], "none");
                assert.equal(typeof initial.props.onInput, "function");

                await Promise.resolve(initial.props.onInput({ target: { value: "  Accounts API  " } }));

                const updated = render();
                assert.equal(updated.props.value, "Accounts API");
                assert.equal(updated.props["data-saved"], "Accounts API");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorBindSetMethodGroup_UpdatesStateThroughSynchronousSetterOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseNameSyncEditor.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @bind:get="ReleaseName" @bind:set="SetReleaseName" @bind:event="oninput" data-saved="@SavedReleaseName" />
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-name-sync-editor-runtime")]
            public partial class ReleaseNameSyncEditor : ComponentBase, IVueComponent
            {
                private string ReleaseName { get; set; } = "Draft release";

                private string SavedReleaseName { get; set; } = "none";

                private void SetReleaseName(string value)
                {
                    ReleaseName = value.Trim();
                    SavedReleaseName = "saved:" + ReleaseName;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseNameSyncEditor");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredBindSetter", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "SetReleaseName", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "SetReleaseName(__value)", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "state.ReleaseName", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "state.SavedReleaseName", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-name-sync-editor-runtime.mjs",
            observation.ModuleText,
            "official-release-name-sync-editor-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-name-sync-editor-runtime.mjs";

            test("official Razor bind:set synchronous method group updates state before the handler returns", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.name, "input");
                assert.equal(initial.props.value, "Draft release");
                assert.equal(initial.props["data-saved"], "none");
                assert.equal(typeof initial.props.onInput, "function");

                assert.equal(initial.props.onInput({ target: { value: "  Release approved  " } }), undefined);

                const updated = render();
                assert.equal(updated.props.value, "Release approved");
                assert.equal(updated.props["data-saved"], "saved:Release approved");
            });
            """);
    }
}
