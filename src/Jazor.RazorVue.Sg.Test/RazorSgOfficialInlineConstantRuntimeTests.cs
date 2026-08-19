namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialInlineConstantRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorBooleanInlineContent_EmitsAnImmutableTextChildOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/StaticBooleanContent.razor"),
            documentText:
            """
            <p>@(true)</p>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/static-boolean-content-runtime")]
            public partial class StaticBooleanContent : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.StaticBooleanContent");

        StringAssert.Contains(observation.GeneratedCSharp, "AddContent", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "true", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/static-boolean-content-runtime.mjs",
            observation.ModuleText,
            "official-static-boolean-content-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/static-boolean-content-runtime.mjs";

            test("official Razor boolean content remains an immutable VNode child", () => {
                const paragraph = component.setup({}, { slots: {} })();
                assert.equal(paragraph.name, "p");
                assert.deepEqual(paragraph.children, [{ name: "__text", children: true, patchFlag: undefined }]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorInlineFrameConstants_InlineValuesWithoutCreatingRuntimeLocalsOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleasePolicySummary.razor"),
            documentText:
            """
            <section data-area="release-policy">
                @{
                    const string approval = "manual";
                    const int retryLimit = 2;
                }
                <span data-approval="@approval">Retries: @retryLimit</span>
            </section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-policy-summary-runtime")]
            public partial class ReleasePolicySummary : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleasePolicySummary");

        StringAssert.Contains(observation.GeneratedCSharp, "const string approval", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "const int retryLimit", StringComparison.Ordinal);
        var sectionOpen = observation.GeneratedCSharp.IndexOf("OpenElement(0, \"section\")", StringComparison.Ordinal);
        var approvalDeclaration = observation.GeneratedCSharp.IndexOf("const string approval", StringComparison.Ordinal);
        Assert.IsGreaterThanOrEqualTo(0, sectionOpen, observation.GeneratedCSharp);
        Assert.IsGreaterThan(sectionOpen, approvalDeclaration, observation.GeneratedCSharp);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("approval =", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("retryLimit =", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-policy-summary-runtime.mjs",
            observation.ModuleText,
            "official-release-policy-summary-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-policy-summary-runtime.mjs";

            test("official Razor frame-local constants are emitted as stable child values", () => {
                const section = component.setup({}, { slots: {} })();
                assert.equal(section.name, "section");
                assert.equal(section.props["data-area"], "release-policy");
                assert.equal(section.children.length, 1);

                const span = section.children[0];
                assert.equal(span.name, "span");
                assert.equal(span.props["data-approval"], "manual");
                assert.deepEqual(span.children, [{ name: "__text", children: "Retries: ", patchFlag: undefined }, { name: "__text", children: 2, patchFlag: undefined }]);
            });
            """);
    }
}
