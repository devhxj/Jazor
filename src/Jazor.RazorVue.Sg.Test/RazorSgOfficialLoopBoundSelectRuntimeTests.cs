namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialLoopBoundSelectRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLoopBoundSelect_UpdatesTheCapturedRowOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseEnvironmentEditor.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            @foreach (var release in releases)
            {
                <select data-release="@release.Id"
                        @bind:get="release.Environment"
                        @bind:set="@(value => SetEnvironment(release, value))">
                    @foreach (var environment in environments)
                    {
                        <option value="@environment">@environment</option>
                    }
                </select>
            }
            <p data-last-selection="@LastSelection">@LastSelection</p>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-environment-editor-runtime")]
            public partial class ReleaseEnvironmentEditor : ComponentBase, IVueComponent
            {
                private readonly ReleaseTarget[] releases =
                {
                    new ReleaseTarget(1, "staging"),
                    new ReleaseTarget(2, "production")
                };

                private readonly string[] environments = { "staging", "production", "canary" };

                private string LastSelection { get; set; } = "none";

                private void SetEnvironment(ReleaseTarget release, string? environment)
                {
                    release.Environment = environment ?? string.Empty;
                    LastSelection = release.Id.ToString() + ":" + release.Environment;
                }

                private sealed class ReleaseTarget
                {
                    public ReleaseTarget(int id, string environment)
                    {
                        Id = id;
                        Environment = environment;
                    }

                    public int Id { get; }

                    public string Environment { get; set; }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseEnvironmentEditor");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredBindSetter", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "SetEnvironment", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "Array.from(state.releases ?? [], release =>", StringComparison.Ordinal);
        Assert.IsTrue(
            observation.ModuleText.IndexOf("class ReleaseTarget", StringComparison.Ordinal) <
            observation.ModuleText.IndexOf("const state = reactive", StringComparison.Ordinal),
            observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-environment-editor-runtime.mjs",
            observation.ModuleText,
            "official-release-environment-editor-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-environment-editor-runtime.mjs";

            function collect(node, name) {
                if (node == null) return [];
                if (Array.isArray(node)) return node.flatMap(item => collect(item, name));
                const children = collect(node.children, name);
                return node.name === name ? [node, ...children] : children;
            }

            test("official Razor loop-bound selects retain each captured release", async () => {
                const render = component.setup({}, { slots: {} });
                const initialSelects = collect(render(), "select");
                assert.equal(initialSelects.length, 2);
                assert.equal(initialSelects[0].props.value, "staging");
                assert.equal(initialSelects[1].props.value, "production");
                assert.equal(typeof initialSelects[1].props.onChange, "function");

                await Promise.resolve(initialSelects[1].props.onChange({ target: { value: "canary" } }));

                const updated = render();
                const updatedSelects = collect(updated, "select");
                const summary = collect(updated, "p");
                assert.equal(updatedSelects[0].props.value, "staging");
                assert.equal(updatedSelects[1].props.value, "canary");
                assert.equal(summary.length, 1);
                assert.equal(summary[0].props["data-last-selection"], "2:canary");
                assert.equal(summary[0].children, "2:canary");
            });
            """);
    }
}
