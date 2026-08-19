namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialRenderFragmentCompositionRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorComposedLocalRenderFragment_ForwardsNamedSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseSummaryRuntime.razor"),
            documentText:
            """
            @using Demo.Components

            <SlotHost Content="@ComposeSummary()" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-host-render-fragment-composition-runtime")]
                public sealed class SlotHost : ComponentBase, IVueComponent
                {
                    [Parameter, System.ComponentModel.Description("@#content")] public RenderFragment? Content { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/release-summary-render-fragment-composition-runtime")]
                public partial class ReleaseSummaryRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public string ReleaseName { get; set; } = "April deployment";

                    private RenderFragment ComposeSummary()
                    {
                        RenderFragment title = builder =>
                        {
                            builder.OpenElement(0, "strong");
                            builder.AddAttribute(1, "data-summary-part", "title");
                            builder.AddContent(2, ReleaseName);
                            builder.CloseElement();
                        };

                        return builder =>
                        {
                            builder.OpenElement(3, "section");
                            builder.AddAttribute(4, "data-summary", "release");
                            title(builder);
                            builder.OpenElement(5, "span");
                            builder.AddAttribute(6, "data-summary-part", "status");
                            builder.AddContent(7, "Ready");
                            builder.CloseElement();
                            builder.CloseElement();
                        };
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseSummaryRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "ComposeSummary()", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "content:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-summary-render-fragment-composition-runtime.mjs",
            observation.ModuleText,
            "official-release-summary-render-fragment-composition-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-summary-render-fragment-composition-runtime.mjs";
            import slotHost from "./components/slot-host-render-fragment-composition-runtime.mjs";

            test("official Razor local RenderFragment composition forwards the complete named slot", () => {
                const host = component.setup({ ReleaseName: "May deployment" }, { slots: {} })();
                assert.equal(host.name, slotHost);
                assert.equal(typeof host.children.content, "function");

                const nodes = host.children.content();
                assert.equal(nodes.length, 1);
                const summary = nodes[0];
                assert.equal(summary.name, "section");
                assert.equal(summary.props["data-summary"], "release");
                assert.equal(summary.children.length, 2);

                const title = summary.children[0];
                assert.equal(title.name, "strong");
                assert.equal(title.props["data-summary-part"], "title");
                assert.equal(title.children, "May deployment");

                const status = summary.children[1];
                assert.equal(status.name, "span");
                assert.equal(status.props["data-summary-part"], "status");
                assert.deepEqual(status.children, [{ name: "__text", children: "Ready", patchFlag: undefined }]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-host-render-fragment-composition-runtime.mjs"] = "export default { name: \"slot-host-render-fragment-composition-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorComposedLocalGenericRenderFragment_ForwardsScopedSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseItemTemplateRuntime.razor"),
            documentText:
            """
            @using Demo.Components

            <ItemPanel ItemTemplate="@ComposeItemTemplate()" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScript]
                public sealed class ReleaseEntry
                {
                    public int Id { get; set; }
                    public string Label { get; set; } = string.Empty;
                }

                [ECMAScriptModule("./components/item-panel-render-fragment-composition-runtime")]
                public sealed class ItemPanel : ComponentBase, IVueComponent
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
                using Demo.Components;

                [ECMAScriptModule("./components/release-item-template-render-fragment-composition-runtime")]
                public partial class ReleaseItemTemplateRuntime : ComponentBase, IVueComponent
                {
                    private RenderFragment<ReleaseEntry> ComposeItemTemplate()
                    {
                        RenderFragment<ReleaseEntry> label = release => builder =>
                        {
                            builder.OpenElement(0, "strong");
                            builder.AddAttribute(1, "data-release-id", release.Id);
                            builder.AddContent(2, release.Label);
                            builder.CloseElement();
                        };

                        return release => builder =>
                        {
                            builder.OpenElement(3, "li");
                            builder.AddAttribute(4, "data-template", "release");
                            label(release)(builder);
                            builder.CloseElement();
                        };
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseItemTemplateRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "ComposeItemTemplate()", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "item:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-item-template-render-fragment-composition-runtime.mjs",
            observation.ModuleText,
            "official-release-item-template-render-fragment-composition-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-item-template-render-fragment-composition-runtime.mjs";
            import itemPanel from "./components/item-panel-render-fragment-composition-runtime.mjs";

            test("official Razor local generic RenderFragment composition forwards the scoped slot context", () => {
                const panel = component.setup({}, { slots: {} })();
                assert.equal(panel.name, itemPanel);
                assert.equal(typeof panel.children.item, "function");

                const nodes = panel.children.item({ Id: 42, Label: "Production" });
                assert.equal(nodes.length, 1);
                const item = nodes[0];
                assert.equal(item.name, "li");
                assert.equal(item.props["data-template"], "release");
                assert.equal(item.children.length, 1);
                assert.equal(item.children[0].name, "strong");
                assert.equal(item.children[0].props["data-release-id"], 42);
                assert.equal(item.children[0].children, "Production");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/item-panel-render-fragment-composition-runtime.mjs"] = "export default { name: \"item-panel-render-fragment-composition-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorConditionalLocalGenericRenderFragment_PreservesScopedSlotSelectionOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseItemVariantRuntime.razor"),
            documentText:
            """
            @using Demo.Components

            <ItemPanel ItemTemplate="@ComposeItemTemplate()" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScript]
                public sealed class ReleaseEntry
                {
                    public int Id { get; set; }
                    public string Label { get; set; } = string.Empty;
                }

                [ECMAScriptModule("./components/item-panel-render-fragment-variant-runtime")]
                public sealed class ItemPanel : ComponentBase, IVueComponent
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
                using Demo.Components;

                [ECMAScriptModule("./components/release-item-variant-render-fragment-runtime")]
                public partial class ReleaseItemVariantRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public bool Detailed { get; set; }

                    private RenderFragment<ReleaseEntry> ComposeItemTemplate()
                    {
                        RenderFragment<ReleaseEntry> compact = release => builder =>
                        {
                            builder.OpenElement(0, "span");
                            builder.AddAttribute(1, "data-variant", "compact");
                            builder.AddContent(2, release.Label);
                            builder.CloseElement();
                        };
                        RenderFragment<ReleaseEntry> detailed = release => builder =>
                        {
                            builder.OpenElement(3, "strong");
                            builder.AddAttribute(4, "data-variant", "detailed");
                            builder.AddContent(5, release.Id + ":" + release.Label);
                            builder.CloseElement();
                        };
                        RenderFragment<ReleaseEntry> selected = Detailed ? detailed : compact;

                        return release => builder => selected(release)(builder);
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseItemVariantRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "ComposeItemTemplate()", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "props.Detailed", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-item-variant-render-fragment-runtime.mjs",
            observation.ModuleText,
            "official-release-item-variant-render-fragment-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-item-variant-render-fragment-runtime.mjs";

            test("official Razor conditional local generic RenderFragment selects the matching scoped slot content", () => {
                const compactPanel = component.setup({ Detailed: false }, { slots: {} })();
                const compact = compactPanel.children.item({ Id: 9, Label: "Review" })[0];
                assert.equal(compact.name, "span");
                assert.equal(compact.props["data-variant"], "compact");
                assert.equal(compact.children, "Review");

                const detailedPanel = component.setup({ Detailed: true }, { slots: {} })();
                const detailed = detailedPanel.children.item({ Id: 9, Label: "Review" })[0];
                assert.equal(detailed.name, "strong");
                assert.equal(detailed.props["data-variant"], "detailed");
                assert.equal(detailed.children, "9:Review");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/item-panel-render-fragment-variant-runtime.mjs"] = "export default { name: \"item-panel-render-fragment-variant-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorRecursiveLocalRenderFragment_PreservesNestedSlotContentOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseTreeTemplateRuntime.razor"),
            documentText:
            """
            @using Demo.Components

            <TreePanel Content="@RenderTree(Depth)" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/tree-panel-local-render-fragment-runtime")]
                public sealed class TreePanel : ComponentBase, IVueComponent
                {
                    [Parameter, System.ComponentModel.Description("@#content")] public RenderFragment? Content { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/release-tree-local-render-fragment-runtime")]
                public partial class ReleaseTreeTemplateRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public int Depth { get; set; } = 2;

                    private RenderFragment RenderTree(int depth)
                    {
                        RenderFragment children = builder =>
                        {
                            if (depth > 0)
                            {
                                builder.AddContent(0, RenderTree(depth - 1));
                            }
                        };

                        return builder =>
                        {
                            builder.OpenElement(1, "li");
                            builder.AddAttribute(2, "data-depth", depth);
                            builder.AddContent(3, depth);
                            children(builder);
                            builder.CloseElement();
                        };
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseTreeTemplateRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "RenderTree(Depth)", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "renderRenderTree", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-tree-local-render-fragment-runtime.mjs",
            observation.ModuleText,
            "official-release-tree-local-render-fragment-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-tree-local-render-fragment-runtime.mjs";

            test("official Razor recursive local RenderFragment keeps nested node order", () => {
                const panel = component.setup({ Depth: 2 }, { slots: {} })();
                const root = panel.children.content()[0];
                assert.equal(root.name, "li");
                assert.equal(root.props["data-depth"], 2);
                assert.equal(root.children[0], 2);

                const child = root.children[1];
                assert.equal(child.name, "li");
                assert.equal(child.props["data-depth"], 1);
                assert.equal(child.children[0], 1);

                const leaf = child.children[1];
                assert.equal(leaf.name, "li");
                assert.equal(leaf.props["data-depth"], 0);
                assert.deepEqual(leaf.children, [0, null]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/tree-panel-local-render-fragment-runtime.mjs"] = "export default { name: \"tree-panel-local-render-fragment-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorExpressionBodiedRenderFragmentFactory_ForwardsNamedSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseNoticeRuntime.razor"),
            documentText:
            """
            @using Demo.Components

            <SlotHost Content="@CreateNotice()" />
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-host-expression-fragment-runtime")]
                public sealed class SlotHost : ComponentBase, IVueComponent
                {
                    [Parameter, System.ComponentModel.Description("@#content")] public RenderFragment? Content { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/release-notice-expression-fragment-runtime")]
                public partial class ReleaseNoticeRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public string Message { get; set; } = "Deployment ready";

                    private RenderFragment CreateNotice() => builder =>
                    {
                        builder.OpenElement(0, "aside");
                        builder.AddAttribute(1, "data-notice", "release");
                        builder.AddContent(2, Message);
                        builder.CloseElement();
                    };
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseNoticeRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateNotice()", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-notice-expression-fragment-runtime.mjs",
            observation.ModuleText,
            "official-release-notice-expression-fragment-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-notice-expression-fragment-runtime.mjs";

            test("official Razor expression-bodied RenderFragment factory forwards current props", () => {
                const host = component.setup({ Message: "Approval pending" }, { slots: {} })();
                const notice = host.children.content()[0];
                assert.equal(notice.name, "aside");
                assert.equal(notice.props["data-notice"], "release");
                assert.equal(notice.children, "Approval pending");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-host-expression-fragment-runtime.mjs"] = "export default { name: \"slot-host-expression-fragment-runtime\" };"
            });
    }
}
