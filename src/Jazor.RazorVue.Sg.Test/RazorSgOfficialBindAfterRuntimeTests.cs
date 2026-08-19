namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialBindAfterRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorBindAfter_UpdatesStateBeforeAwaitingPersistenceOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseNamePersistence.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @bind="ReleaseName" @bind:event="oninput" @bind:after="PersistReleaseNameAsync" data-persisted="@PersistedReleaseName" />
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-name-persistence-runtime")]
            public partial class ReleaseNamePersistence : ComponentBase, IVueComponent
            {
                private string ReleaseName { get; set; } = "Draft release";

                private string PersistedReleaseName { get; set; } = "none";

                private Task PersistReleaseNameAsync()
                {
                    PersistedReleaseName = "saved:" + ReleaseName;
                    return Task.CompletedTask;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseNamePersistence");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredBindSetter", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "PersistReleaseNameAsync", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "PersistReleaseNameAsync", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "state.ReleaseName", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "state.PersistedReleaseName", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-name-persistence-runtime.mjs",
            observation.ModuleText,
            "official-release-name-persistence-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-name-persistence-runtime.mjs";

            test("official Razor bind:after persists the updated DOM value", async () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.name, "input");
                assert.equal(initial.props.value, "Draft release");
                assert.equal(initial.props["data-persisted"], "none");
                assert.equal(typeof initial.props.onInput, "function");

                await Promise.resolve(initial.props.onInput({ target: { value: "Release approved" } }));

                const updated = render();
                assert.equal(updated.props.value, "Release approved");
                assert.equal(updated.props["data-persisted"], "saved:Release approved");
            });
            """);
    }
}
