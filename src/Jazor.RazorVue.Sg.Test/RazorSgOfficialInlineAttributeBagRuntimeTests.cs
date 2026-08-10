namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialInlineAttributeBagRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorInlineAttributeBag_ExpandsKnownAttributesAndPreservesExplicitPrecedenceOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseDeploymentInlineAttributes.razor",
            documentText:
            """
            @using System.Collections.Generic

            <button @attributes="@(new Dictionary<string, object> { ["data-source"] = "inline", ["aria-busy"] = IsDeploying, ["data-phase"] = Phase })" data-phase="@Phase" data-action="@Action">@Label</button>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-deployment-inline-attributes")]
            public partial class ReleaseDeploymentInlineAttributes : ComponentBase, IVueComponent
            {
                [Parameter] public bool IsDeploying { get; set; }

                [Parameter] public string Phase { get; set; } = string.Empty;

                [Parameter] public string Action { get; set; } = string.Empty;

                [Parameter] public string Label { get; set; } = string.Empty;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseDeploymentInlineAttributes");

        StringAssert.Contains(observation.GeneratedCSharp, "AddMultipleAttributes", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "\"data-source\": \"inline\"", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("new Dictionary", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-deployment-inline-attributes.mjs",
            observation.ModuleText,
            "official-release-deployment-inline-attributes-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-deployment-inline-attributes.mjs";

            test("official Razor inline attribute bags expand known entries before explicit Razor attributes", () => {
                const idle = component.setup({
                    IsDeploying: false,
                    Phase: "ready",
                    Action: "deploy",
                    Label: "Deploy"
                }, { slots: {} })();
                assert.equal(idle.name, "button");
                assert.equal(idle.props["data-source"], "inline");
                assert.equal(idle.props["aria-busy"], false);
                assert.equal(idle.props["data-phase"], "ready");
                assert.equal(idle.props["data-action"], "deploy");
                assert.deepEqual(idle.children, ["Deploy"]);

                const deploying = component.setup({
                    IsDeploying: true,
                    Phase: "deploying",
                    Action: "cancel",
                    Label: "Cancel"
                }, { slots: {} })();
                assert.equal(deploying.props["aria-busy"], true);
                assert.equal(deploying.props["data-phase"], "deploying");
                assert.equal(deploying.props["data-action"], "cancel");
                assert.deepEqual(deploying.children, ["Cancel"]);
            });
            """);
    }
}
