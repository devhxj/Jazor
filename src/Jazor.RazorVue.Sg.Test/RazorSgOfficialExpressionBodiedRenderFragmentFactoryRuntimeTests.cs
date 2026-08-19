namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialExpressionBodiedRenderFragmentFactoryRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorExpressionBodiedRenderFragmentFactory_ProvidesSlotContentOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ExpressionBodiedReleaseHeader.razor"),
            documentText:
            """
            @using Demo.Components

            <ReleaseHeaderPanel Header="@CreateHeader()" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/expression-bodied-release-header-panel-runtime")]
                public sealed class ReleaseHeaderPanel : ComponentBase, IVueComponent
                {
                    [Parameter, System.ComponentModel.Description("@#header")] public RenderFragment? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/expression-bodied-release-header-runtime")]
                public partial class ExpressionBodiedReleaseHeader : ComponentBase, IVueComponent
                {
                    private RenderFragment CreateHeader() => builder =>
                    {
                        builder.OpenElement(0, "strong");
                        builder.AddAttribute(1, "data-release-header", "expression-bodied");
                        builder.AddContent(2, "Deployment ready");
                        builder.CloseElement();
                    };
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ExpressionBodiedReleaseHeader");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateHeader()", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/expression-bodied-release-header-runtime.mjs",
            observation.ModuleText,
            "official-expression-bodied-release-header-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/expression-bodied-release-header-runtime.mjs";
            import releaseHeaderPanel from "./components/expression-bodied-release-header-panel-runtime.mjs";

            test("official Razor expression-bodied RenderFragment factory provides a slot", () => {
                const panel = component.setup({}, { slots: {} })();
                assert.equal(panel.name, releaseHeaderPanel);
                assert.equal(typeof panel.children.header, "function");

                const nodes = panel.children.header();
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "strong");
                assert.equal(nodes[0].props["data-release-header"], "expression-bodied");
                assert.deepEqual(nodes[0].children, [{ name: "__text", children: "Deployment ready", patchFlag: undefined }]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/expression-bodied-release-header-panel-runtime.mjs"] = "export default { name: \"expression-bodied-release-header-panel-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorExpressionBodiedGenericRenderFragmentProperty_ProvidesScopedSlotContentOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ExpressionBodiedReleaseTemplate.razor"),
            documentText:
            """
            @using Demo.Components

            <ReleaseTemplatePanel ItemTemplate="@ItemTemplate" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                public sealed record ReleaseEntry(string Name, int Pending);

                [ECMAScriptModule("./components/expression-bodied-release-template-panel-runtime")]
                public sealed class ReleaseTemplatePanel : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("item")]
                    [Parameter] public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/expression-bodied-release-template-runtime")]
                public partial class ExpressionBodiedReleaseTemplate : ComponentBase, IVueComponent
                {
                    private RenderFragment<ReleaseEntry> ItemTemplate => release => builder =>
                    {
                        builder.OpenElement(0, "li");
                        builder.AddAttribute(1, "data-release", release.Name);
                        builder.AddContent(2, release.Pending);
                        builder.CloseElement();
                    };
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ExpressionBodiedReleaseTemplate");

        StringAssert.Contains(observation.GeneratedCSharp, "ItemTemplate", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "item:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/expression-bodied-release-template-runtime.mjs",
            observation.ModuleText,
            "official-expression-bodied-release-template-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/expression-bodied-release-template-runtime.mjs";
            import releaseTemplatePanel from "./components/expression-bodied-release-template-panel-runtime.mjs";

            test("official Razor expression-bodied RenderFragment<T> property provides a scoped slot", () => {
                const panel = component.setup({}, { slots: {} })();
                assert.equal(panel.name, releaseTemplatePanel);
                assert.equal(typeof panel.children.item, "function");

                const nodes = panel.children.item({ Name: "Release 2026.08", Pending: 3 });
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "li");
                assert.equal(nodes[0].props["data-release"], "Release 2026.08");
                assert.deepEqual(nodes[0].children, [3]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/expression-bodied-release-template-panel-runtime.mjs"] = "export default { name: \"expression-bodied-release-template-panel-runtime\" };"
            });
    }
}
