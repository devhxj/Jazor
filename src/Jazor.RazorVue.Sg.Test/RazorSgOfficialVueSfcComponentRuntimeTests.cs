namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialVueSfcComponentRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorVueSfcComponent_ImportsAndRendersTheDefaultExportOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseDashboardSfc.razor",
            documentText:
            """
            @using Demo.Components

            <ReleaseCard Title="@ReleaseTitle" />
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-dashboard-sfc")]
            public partial class ReleaseDashboardSfc : ComponentBase, IVueComponent
            {
                [Parameter]
                public string ReleaseTitle { get; set; } = "May deployment";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseDashboardSfc",
            supportingSources: new Dictionary<string, string>
            {
                ["Components/ReleaseCard.cs"] =
                """
                namespace Demo.Components;

                [ECMAScriptModule("./components/release-card.vue")]
                public sealed class ReleaseCard : ComponentBase, IVueComponent
                {
                    [Parameter, System.ComponentModel.Description("@#title")]
                    public string Title { get; set; } = string.Empty;
                }
                """
            });

        StringAssert.Contains(observation.GeneratedCSharp, "OpenComponent<global::Demo.Components.ReleaseCard>", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "from \"./release-card.vue.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "title: props.ReleaseTitle", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("scope.buildRenderTree(builder)", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("builder.finish()", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-dashboard-sfc.mjs",
            observation.ModuleText,
            "official-release-dashboard-sfc.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-dashboard-sfc.mjs";
            import releaseCard from "./components/release-card.vue.mjs";

            test("official Razor SFC component reference uses the input module default export", () => {
                const vnode = component.setup({ ReleaseTitle: "June deployment" }, { slots: {} })();
                assert.equal(vnode.name, releaseCard);
                assert.deepEqual(vnode.props, { title: "June deployment" });
                assert.equal(vnode.children, null);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-card.vue.mjs"] = "export default { name: \"release-card\" };"
            });
    }
}
