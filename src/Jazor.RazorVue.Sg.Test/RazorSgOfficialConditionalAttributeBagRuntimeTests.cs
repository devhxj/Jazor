namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialConditionalAttributeBagRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorConditionalAttributeBag_SelectsActiveAttributesAndPreservesExplicitPrecedenceOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseDeployAction.razor"),
            documentText:
            """
            <button @attributes="@(IsDeploying ? DeployingAttributes : ReadyAttributes)" data-phase="@Phase">Deploy</button>
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-deploy-action")]
            public partial class ReleaseDeployAction : ComponentBase, IVueComponent
            {
                [Parameter] public bool IsDeploying { get; set; }

                [Parameter] public IReadOnlyDictionary<string, object>? DeployingAttributes { get; set; }

                [Parameter] public IReadOnlyDictionary<string, object>? ReadyAttributes { get; set; }

                [Parameter] public string Phase { get; set; } = string.Empty;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseDeployAction");

        StringAssert.Contains(observation.GeneratedCSharp, "AddMultipleAttributes", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "mergeProps", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "props.IsDeploying", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-deploy-action.mjs",
            observation.ModuleText,
            "official-conditional-attribute-bag-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-deploy-action.mjs";

            test("official Razor conditional attribute bags retain the active value set", () => {
                const deploying = component.setup({
                    IsDeploying: true,
                    Phase: "deploying",
                    DeployingAttributes: {
                        disabled: true,
                        "aria-busy": true,
                        "data-mode": "deploying",
                        "data-phase": "from-deploying-bag"
                    },
                    ReadyAttributes: {
                        disabled: false,
                        "aria-busy": false,
                        "data-mode": "ready"
                    }
                }, { slots: {} })();

                assert.equal(deploying.name, "button");
                assert.equal(deploying.props.disabled, true);
                assert.equal(deploying.props["aria-busy"], true);
                assert.equal(deploying.props["data-mode"], "deploying");
                assert.equal(deploying.props["data-phase"], "deploying");
                assert.deepEqual(deploying.children, [{ name: "__text", children: "Deploy", patchFlag: undefined }]);

                const ready = component.setup({
                    IsDeploying: false,
                    Phase: "ready",
                    DeployingAttributes: {
                        disabled: true,
                        "aria-busy": true,
                        "data-mode": "deploying"
                    },
                    ReadyAttributes: {
                        disabled: false,
                        "aria-busy": false,
                        "data-mode": "ready",
                        "data-phase": "from-ready-bag"
                    }
                }, { slots: {} })();

                assert.equal(ready.props.disabled, false);
                assert.equal(ready.props["aria-busy"], false);
                assert.equal(ready.props["data-mode"], "ready");
                assert.equal(ready.props["data-phase"], "ready");
                assert.deepEqual(ready.children, [{ name: "__text", children: "Deploy", patchFlag: undefined }]);
            });
            """);
    }
}
