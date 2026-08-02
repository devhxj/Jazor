namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialRenderFragmentFactoryRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorRenderFragmentFactory_BlockReturnProvidesScopedSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TemplateFactoryRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <TemplatePanel ItemTemplate="@CreateItemTemplate()" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                public sealed record ReleaseEntry(int Id, string Label);

                [ECMAScriptModule("./components/template-panel-factory-runtime")]
                [VueSlot(nameof(ItemTemplate), Name = "item", ContextTypeName = "Demo.Components.ReleaseEntry", ContextParameterName = "release")]
                public sealed class TemplatePanel : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/template-factory-runtime")]
                public partial class TemplateFactoryRuntime : ComponentBase, IVueComponent
                {
                    private RenderFragment<ReleaseEntry> CreateItemTemplate()
                    {
                        return release =>
                        {
                            return builder =>
                            {
                                builder.OpenElement(0, "strong");
                                builder.AddAttribute(1, "data-release-id", release.Id);
                                builder.AddContent(2, release.Label);
                                builder.CloseElement();
                            };
                        };
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TemplateFactoryRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateItemTemplate()", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "from \"./template-panel-factory-runtime.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "item:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/template-factory-runtime.mjs",
            observation.ModuleText,
            "official-template-factory-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/template-factory-runtime.mjs";
            import templatePanel from "./components/template-panel-factory-runtime.mjs";

            test("official Razor RenderFragment factory emits a scoped slot", () => {
                const panel = component.setup({}, { slots: {} })();
                assert.equal(panel.name, templatePanel);
                assert.equal(typeof panel.children.item, "function");

                const nodes = panel.children.item({ id: 42, label: "Deploy" });
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "strong");
                assert.equal(nodes[0].props["data-release-id"], 42);
                assert.deepEqual(nodes[0].children, ["Deploy"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/template-panel-factory-runtime.mjs"] = "export default { name: \"template-panel-factory-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorRenderFragmentProperty_BlockGetterProvidesScopedSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TemplatePropertyRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <TemplatePanel ItemTemplate="@ItemTemplate" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                public sealed record ReleaseEntry(int Id, string Label);

                [ECMAScriptModule("./components/template-panel-property-runtime")]
                [VueSlot(nameof(ItemTemplate), Name = "item", ContextTypeName = "Demo.Components.ReleaseEntry", ContextParameterName = "release")]
                public sealed class TemplatePanel : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/template-property-runtime")]
                public partial class TemplatePropertyRuntime : ComponentBase, IVueComponent
                {
                    private RenderFragment<ReleaseEntry> ItemTemplate
                    {
                        get
                        {
                            return release =>
                            {
                                return builder =>
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "data-release-label", release.Label);
                                    builder.AddContent(2, release.Id);
                                    builder.CloseElement();
                                };
                            };
                        }
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TemplatePropertyRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "ItemTemplate", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "from \"./template-panel-property-runtime.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "item:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/template-property-runtime.mjs",
            observation.ModuleText,
            "official-template-property-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/template-property-runtime.mjs";
            import templatePanel from "./components/template-panel-property-runtime.mjs";

            test("official Razor RenderFragment property emits a scoped slot", () => {
                const panel = component.setup({}, { slots: {} })();
                assert.equal(panel.name, templatePanel);
                assert.equal(typeof panel.children.item, "function");

                const nodes = panel.children.item({ id: 7, label: "Review" });
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "span");
                assert.equal(nodes[0].props["data-release-label"], "Review");
                assert.deepEqual(nodes[0].children, [7]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/template-panel-property-runtime.mjs"] = "export default { name: \"template-panel-property-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorRenderFragmentProperty_SelectsConditionalMethodGroupSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ConditionalTemplatePropertyRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <SlotPanel Header="@Header" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-panel-conditional-property-runtime")]
                [VueSlot(nameof(Header), Name = "header")]
                public sealed class SlotPanel : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/conditional-template-property-runtime")]
                public partial class ConditionalTemplatePropertyRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public bool Detailed { get; set; }

                    private RenderFragment Header
                    {
                        get
                        {
                            return Detailed ? RenderDetailedHeader : RenderCompactHeader;
                        }
                    }

                    private void RenderDetailedHeader(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "strong");
                        builder.AddAttribute(1, "data-variant", "detailed");
                        builder.AddContent(2, "Detailed release");
                        builder.CloseElement();
                    }

                    private void RenderCompactHeader(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "span");
                        builder.AddAttribute(1, "data-variant", "compact");
                        builder.AddContent(2, "Compact release");
                        builder.CloseElement();
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ConditionalTemplatePropertyRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "Header", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "props.detailed", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/conditional-template-property-runtime.mjs",
            observation.ModuleText,
            "official-conditional-template-property-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/conditional-template-property-runtime.mjs";
            import slotPanel from "./components/slot-panel-conditional-property-runtime.mjs";

            test("official Razor conditional RenderFragment property selects the matching slot", () => {
                const detailed = component.setup({ detailed: true }, { slots: {} })();
                assert.equal(detailed.name, slotPanel);
                const detailedNodes = detailed.children.header();
                assert.equal(detailedNodes[0].name, "strong");
                assert.equal(detailedNodes[0].props["data-variant"], "detailed");
                assert.deepEqual(detailedNodes[0].children, ["Detailed release"]);

                const compact = component.setup({ detailed: false }, { slots: {} })();
                const compactNodes = compact.children.header();
                assert.equal(compactNodes[0].name, "span");
                assert.equal(compactNodes[0].props["data-variant"], "compact");
                assert.deepEqual(compactNodes[0].children, ["Compact release"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-panel-conditional-property-runtime.mjs"] = "export default { name: \"slot-panel-conditional-property-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorConditionalRenderFragmentParameter_SelectsFactorySlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ConditionalTemplateParameterRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <SlotPanel Header="@(Detailed ? CreateDetailedHeader() : CreateCompactHeader())" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-panel-conditional-parameter-runtime")]
                [VueSlot(nameof(Header), Name = "header")]
                public sealed class SlotPanel : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/conditional-template-parameter-runtime")]
                public partial class ConditionalTemplateParameterRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public bool Detailed { get; set; }

                    private RenderFragment CreateDetailedHeader()
                    {
                        return builder =>
                        {
                            builder.OpenElement(0, "strong");
                            builder.AddAttribute(1, "data-variant", "detailed");
                            builder.AddContent(2, "Detailed release");
                            builder.CloseElement();
                        };
                    }

                    private RenderFragment CreateCompactHeader()
                    {
                        return builder =>
                        {
                            builder.OpenElement(0, "span");
                            builder.AddAttribute(1, "data-variant", "compact");
                            builder.AddContent(2, "Compact release");
                            builder.CloseElement();
                        };
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ConditionalTemplateParameterRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateDetailedHeader()", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "CreateCompactHeader()", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "props.detailed", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/conditional-template-parameter-runtime.mjs",
            observation.ModuleText,
            "official-conditional-template-parameter-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/conditional-template-parameter-runtime.mjs";
            import slotPanel from "./components/slot-panel-conditional-parameter-runtime.mjs";

            test("official Razor conditional RenderFragment parameter selects the matching slot", () => {
                const detailed = component.setup({ detailed: true }, { slots: {} })();
                assert.equal(detailed.name, slotPanel);
                const detailedNodes = detailed.children.header();
                assert.equal(detailedNodes[0].name, "strong");
                assert.equal(detailedNodes[0].props["data-variant"], "detailed");
                assert.deepEqual(detailedNodes[0].children, ["Detailed release"]);

                const compact = component.setup({ detailed: false }, { slots: {} })();
                const compactNodes = compact.children.header();
                assert.equal(compactNodes[0].name, "span");
                assert.equal(compactNodes[0].props["data-variant"], "compact");
                assert.deepEqual(compactNodes[0].children, ["Compact release"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-panel-conditional-parameter-runtime.mjs"] = "export default { name: \"slot-panel-conditional-parameter-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorRenderFragmentMethodGroups_EmitInstanceAndStaticSlotsOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TemplateMethodGroupRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <SlotPanel Header="@RenderHeader" Footer="@RenderFooter" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-panel-method-group-runtime")]
                [VueSlot(nameof(Header), Name = "header")]
                [VueSlot(nameof(Footer), Name = "footer")]
                public sealed class SlotPanel : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? Header { get; set; }
                    [Parameter] public RenderFragment? Footer { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/template-method-group-runtime")]
                public partial class TemplateMethodGroupRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public string Label { get; set; } = "Release queue";

                    private void RenderHeader(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "strong");
                        builder.AddAttribute(1, "data-source", "instance");
                        builder.AddContent(2, Label);
                        builder.CloseElement();
                    }

                    private static void RenderFooter(RenderTreeBuilder builder)
                        => builder.AddContent(0, "Static footer");
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TemplateMethodGroupRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "RenderHeader", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RenderFooter", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "footer:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/template-method-group-runtime.mjs",
            observation.ModuleText,
            "official-template-method-group-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/template-method-group-runtime.mjs";
            import slotPanel from "./components/slot-panel-method-group-runtime.mjs";

            test("official Razor method groups provide instance and static Vue slots", () => {
                const panel = component.setup({ label: "Deployments" }, { slots: {} })();
                assert.equal(panel.name, slotPanel);
                assert.equal(typeof panel.children.header, "function");
                assert.equal(typeof panel.children.footer, "function");

                const headerNodes = panel.children.header();
                assert.equal(headerNodes.length, 1);
                assert.equal(headerNodes[0].name, "strong");
                assert.equal(headerNodes[0].props["data-source"], "instance");
                assert.deepEqual(headerNodes[0].children, ["Deployments"]);

                assert.deepEqual(panel.children.footer(), ["Static footer"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-panel-method-group-runtime.mjs"] = "export default { name: \"slot-panel-method-group-runtime\" };"
            });
    }

}
