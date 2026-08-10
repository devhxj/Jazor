namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialRenderFragmentStateHelperRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorRenderStateHelper_ProjectsConstructedTemplatePropertyToScopedSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TemplateStateHelperRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <TemplatePanel ItemTemplate="@CreateTemplateState(Prefix).ItemTemplate" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                public sealed record ReleaseEntry(int Id, string Label);

                [ECMAScriptModule("./components/template-panel-render-state-runtime")]
                public sealed class TemplatePanel : ComponentBase, IVueComponent
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
                [ECMAScriptModule("./components/template-state-helper-runtime")]
                public partial class TemplateStateHelperRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public string Prefix { get; set; } = string.Empty;

                    private sealed class TemplateState
                    {
                        public TemplateState(RenderFragment<ReleaseEntry> itemTemplate)
                        {
                            ItemTemplate = itemTemplate;
                        }

                        public RenderFragment<ReleaseEntry> ItemTemplate { get; }
                    }

                    private TemplateState CreateTemplateState(string prefix)
                    {
                        RenderFragment<ReleaseEntry> itemTemplate = release => builder =>
                        {
                            builder.OpenElement(0, "strong");
                            builder.AddAttribute(1, "data-release-id", release.Id);
                            builder.AddContent(2, prefix + ": " + release.Label);
                            builder.CloseElement();
                        };

                        return new TemplateState(itemTemplate);
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TemplateStateHelperRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateTemplateState(Prefix).ItemTemplate", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "from \"./template-panel-render-state-runtime.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "item:", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("CreateTemplateState", StringComparison.Ordinal), observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "get ItemTemplate()", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/template-state-helper-runtime.mjs",
            observation.ModuleText,
            "official-template-state-helper-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/template-state-helper-runtime.mjs";
            import templatePanel from "./components/template-panel-render-state-runtime.mjs";

            test("official Razor render-state helper expands the constructed generic template", () => {
                const panel = component.setup({ Prefix: "Queue" }, { slots: {} })();
                assert.equal(panel.name, templatePanel);
                assert.equal(typeof panel.children.item, "function");

                const nodes = panel.children.item({ Id: 42, Label: "Deploy API" });
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "strong");
                assert.equal(nodes[0].props["data-release-id"], 42);
                assert.deepEqual(nodes[0].children, ["Queue: Deploy API"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/template-panel-render-state-runtime.mjs"] = "export default { name: \"template-panel-render-state-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorRenderStateHelper_ProjectsInitializedTemplatePropertyToScopedSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TemplateStateInitializerRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <TemplatePanel ItemTemplate="@CreateTemplateState().ItemTemplate" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                public sealed record ReleaseEntry(string Id, string Label);

                [ECMAScriptModule("./components/template-panel-render-state-initializer-runtime")]
                public sealed class TemplatePanel : ComponentBase, IVueComponent
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
                [ECMAScriptModule("./components/template-state-initializer-runtime")]
                public partial class TemplateStateInitializerRuntime : ComponentBase, IVueComponent
                {
                    private sealed class TemplateState
                    {
                        public RenderFragment<ReleaseEntry> ItemTemplate { get; set; } = default!;
                    }

                    private TemplateState CreateTemplateState()
                    {
                        return new TemplateState
                        {
                            ItemTemplate = release => builder =>
                            {
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "data-release", release.Id);
                                builder.AddContent(2, release.Label);
                                builder.CloseElement();
                            }
                        };
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TemplateStateInitializerRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateTemplateState().ItemTemplate", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "from \"./template-panel-render-state-initializer-runtime.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "item:", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("CreateTemplateState", StringComparison.Ordinal), observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "get ItemTemplate()", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/template-state-initializer-runtime.mjs",
            observation.ModuleText,
            "official-template-state-initializer-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/template-state-initializer-runtime.mjs";
            import templatePanel from "./components/template-panel-render-state-initializer-runtime.mjs";

            test("official Razor render-state initializer expands the generic template", () => {
                const panel = component.setup({}, { slots: {} })();
                assert.equal(panel.name, templatePanel);
                assert.equal(typeof panel.children.item, "function");

                const nodes = panel.children.item({ Id: "release-17", Label: "Review CLI" });
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "span");
                assert.equal(nodes[0].props["data-release"], "release-17");
                assert.deepEqual(nodes[0].children, ["Review CLI"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/template-panel-render-state-initializer-runtime.mjs"] = "export default { name: \"template-panel-render-state-initializer-runtime\" };"
            });
    }
}
