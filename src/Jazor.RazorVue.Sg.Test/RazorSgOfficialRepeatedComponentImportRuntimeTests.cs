namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialRepeatedComponentImportRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorRepeatedExternalComponent_UsesOneStableImportOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseDashboard.razor",
            documentText:
            """
            <section data-dashboard="releases">
                <ReleaseBadge Label="Audit" />
                <ReleaseBadge Label="Deploy" />
            </section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-dashboard")]
            public partial class ReleaseDashboard : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseDashboard",
            supportingSources: new Dictionary<string, string>
            {
                ["ReleaseBadge.cs"] =
                """
                namespace Demo.Pages;

                [ECMAScriptModule("./components/release-badge")]
                public sealed class ReleaseBadge : ComponentBase
                {
                    [Parameter]
                    public string Label { get; set; } = string.Empty;
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        Assert.AreEqual(
            1,
            CountOccurrences(observation.ModuleText, "./release-badge.mjs"),
            observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-dashboard.mjs",
            observation.ModuleText,
            "official-repeated-component-import-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-dashboard.mjs";

            test("repeated Razor component use resolves one external module for both VNodes", () => {
                const dashboard = component.setup({}, { slots: {} })();
                assert.equal(dashboard.name, "section");
                assert.equal(dashboard.props["data-dashboard"], "releases");
                const badges = dashboard.children.filter(item => item.name === "release-badge");
                assert.equal(badges.length, 2);
                assert.deepEqual(badges.map(item => item.props.label), ["Audit", "Deploy"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-badge.mjs"] = "export default \"release-badge\";"
            });
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var startIndex = 0;
        while ((startIndex = text.IndexOf(value, startIndex, StringComparison.Ordinal)) >= 0)
        {
            count++;
            startIndex += value.Length;
        }

        return count;
    }
}
