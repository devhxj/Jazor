namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialNullAttributeBagRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorNullAttributeBag_LeavesExplicitAttributesIntactOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseActionWithoutAttributes.razor",
            documentText:
            """
            <button @attributes="null" data-action="deploy">Deploy</button>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-action-without-attributes-runtime")]
            public partial class ReleaseActionWithoutAttributes : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseActionWithoutAttributes");

        StringAssert.Contains(observation.GeneratedCSharp, "AddMultipleAttributes", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("mergeProps", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-action-without-attributes-runtime.mjs",
            observation.ModuleText,
            "official-release-action-without-attributes-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-action-without-attributes-runtime.mjs";

            test("official Razor null attribute bags leave explicit attributes as the complete prop object", () => {
                const output = component.setup({}, { slots: {} })();
                assert.equal(output.name, "button");
                assert.deepEqual(output.props, { "data-action": "deploy" });
                assert.deepEqual(output.children, ["Deploy"]);
            });
            """);
    }
}
