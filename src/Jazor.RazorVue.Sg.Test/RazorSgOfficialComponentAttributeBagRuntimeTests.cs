namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialComponentAttributeBagRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorComponentAttributeBag_NormalizesDescriptorPropsAndPreservesForeignAttributesOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleasePanelHost.razor",
            documentText:
            """
            @using Demo.Components

            <ReleasePanel @attributes="PanelAttributes" />
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/release-panel-attribute-bag")]
                [VueProp(nameof(Heading), Name = "heading")]
                public sealed class ReleasePanel : ComponentBase, IVueComponent
                {
                    [Parameter] public string Heading { get; set; } = string.Empty;

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/release-panel-host")]
                public partial class ReleasePanelHost : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public IReadOnlyDictionary<string, object>? PanelAttributes { get; set; }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleasePanelHost");

        StringAssert.Contains(observation.GeneratedCSharp, "AddMultipleAttributes", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "normalizeComponentAttributes", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "heading", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-panel-host.mjs",
            observation.ModuleText,
            "official-release-panel-attribute-bag-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-panel-host.mjs";
            import releasePanel from "./components/release-panel-attribute-bag.mjs";

            test("official Razor component attribute bags normalize known props without rewriting foreign attributes", () => {
                const panel = component.setup({
                    panelAttributes: {
                        Heading: "Release details",
                        "data-area": "summary"
                    }
                }, { slots: {} })();

                assert.equal(panel.name, releasePanel);
                assert.equal(panel.props.heading, "Release details");
                assert.equal(panel.props["data-area"], "summary");
                assert.equal("Heading" in panel.props, false);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-panel-attribute-bag.mjs"] = "export default { name: \"release-panel-attribute-bag\" };"
            });
    }
}
