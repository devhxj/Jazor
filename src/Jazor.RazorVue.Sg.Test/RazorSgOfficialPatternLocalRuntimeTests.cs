namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialPatternLocalRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorPropertyPatternLocal_RendersOnlyForMatchingValueOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\PatternLocalRuntime.razor",
            documentText:
            """
            @if (ReleaseName is { Length: > 0 } label)
            {
                <span data-release="@label">@label</span>
            }
            """,
            codeBehindSource:
            """
            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/pattern-local-runtime")]
                public partial class PatternLocalRuntime : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? ReleaseName { get; set; }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.PatternLocalRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "props.ReleaseName", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/pattern-local-runtime.mjs",
            observation.ModuleText,
            "official-pattern-local-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/pattern-local-runtime.mjs";

            test("Razor property patterns bind locals only for matching values", () => {
                const missing = component.setup({}, { slots: {} })();
                assert.equal(missing, null);

                const empty = component.setup({ ReleaseName: "" }, { slots: {} })();
                assert.equal(empty, null);

                const matched = component.setup({ ReleaseName: "Deploy" }, { slots: {} })();
                assert.equal(matched.name, "span");
                assert.equal(matched.props["data-release"], "Deploy");
                assert.equal(matched.children, "Deploy");
            });
            """);
    }
}
