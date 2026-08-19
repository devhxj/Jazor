namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialLoopDeconstructionRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLoopDeconstruction_PreservesEachTupleBindingOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseEnvironmentSummary.razor"),
            documentText:
            """
            <ul data-summary="release-environments">
                @foreach (var (environment, count) in ReleaseCounts)
                {
                    <li data-environment="@environment" data-count="@count">@environment: @count</li>
                }
            </ul>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-environment-summary-loop-runtime")]
            public partial class ReleaseEnvironmentSummary : ComponentBase, IVueComponent
            {
                private readonly (string Environment, int Count)[] ReleaseCounts =
                [
                    ("staging", 2),
                    ("production", 5)
                ];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseEnvironmentSummary");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "Array.from(state.ReleaseCounts ?? []", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-environment-summary-loop-runtime.mjs",
            observation.ModuleText,
            "official-release-environment-summary-loop-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-environment-summary-loop-runtime.mjs";

            function collect(node, name) {
                if (node == null) return [];
                if (Array.isArray(node)) return node.flatMap(item => collect(item, name));
                const children = collect(node.children, name);
                return node.name === name ? [node, ...children] : children;
            }

            test("official Razor tuple loop bindings retain each environment and count", () => {
                const root = component.setup({}, { slots: {} })();
                assert.equal(root.name, "ul");
                assert.equal(root.props["data-summary"], "release-environments");

                const rows = collect(root, "li");
                assert.equal(rows.length, 2);
                assert.equal(rows[0].props["data-environment"], "staging");
                assert.equal(rows[0].props["data-count"], 2);
                assert.deepEqual(rows[0].children, ["staging", ": ", 2]);
                assert.equal(rows[1].props["data-environment"], "production");
                assert.equal(rows[1].props["data-count"], 5);
                assert.deepEqual(rows[1].children, ["production", ": ", 5]);
            });
            """);
    }
}
