namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialOverloadedRecursiveRenderFragmentRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorOverloadedRecursiveRenderFragments_KeepSymbolBoundSlotHelpersOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseOverloadTemplateRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <DualSlotPanel Header="@RenderRelease(ReleaseName)" Footer="@RenderRelease(ReleaseId)" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/dual-slot-panel-overloaded-template-runtime")]
                public sealed class DualSlotPanel : ComponentBase, IVueComponent
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
                using Demo.Components;

                [ECMAScriptModule("./components/release-overload-template-runtime")]
                public partial class ReleaseOverloadTemplateRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public string ReleaseName { get; set; } = "API";
                    [Parameter] public int ReleaseId { get; set; } = 2;

                    private RenderFragment RenderRelease(string value)
                        => builder =>
                        {
                            builder.OpenElement(0, "strong");
                            builder.AddAttribute(1, "data-release-name", value);
                            builder.AddContent(2, value);
                            if (value.Length > 1)
                                builder.AddContent(3, RenderRelease(value.Substring(1)));
                            builder.CloseElement();
                        };

                    private RenderFragment RenderRelease(int value)
                        => builder =>
                        {
                            builder.OpenElement(0, "small");
                            builder.AddAttribute(1, "data-release-id", value);
                            builder.AddContent(2, value);
                            if (value > 0)
                                builder.AddContent(3, RenderRelease(value - 1));
                            builder.CloseElement();
                        };
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseOverloadTemplateRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "RenderRelease(ReleaseName)", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RenderRelease(ReleaseId)", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "function renderRenderRelease(", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "function renderRenderRelease$1(value$1)", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "footer:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-overload-template-runtime.mjs",
            observation.ModuleText,
            "official-release-overload-template-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-overload-template-runtime.mjs";
            import panel from "./components/dual-slot-panel-overloaded-template-runtime.mjs";

            test("official Razor overloads bind recursive RenderFragment helpers by symbol", () => {
                const result = component.setup(
                    { releaseName: "API", releaseId: 2 },
                    { slots: {} })();
                assert.equal(result.name, panel);

                const header = result.children.header()[0];
                assert.equal(header.name, "strong");
                assert.equal(header.props["data-release-name"], "API");
                assert.equal(header.children[0], "API");
                assert.equal(header.children[1].name, "strong");
                assert.equal(header.children[1].props["data-release-name"], "PI");
                assert.equal(header.children[1].children[1].name, "strong");
                assert.deepEqual(header.children[1].children[1].children, ["I", null]);

                const footer = result.children.footer()[0];
                assert.equal(footer.name, "small");
                assert.equal(footer.props["data-release-id"], 2);
                assert.equal(footer.children[0], 2);
                assert.equal(footer.children[1].name, "small");
                assert.equal(footer.children[1].props["data-release-id"], 1);
                assert.equal(footer.children[1].children[1].name, "small");
                assert.deepEqual(footer.children[1].children[1].children, [0, null]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/dual-slot-panel-overloaded-template-runtime.mjs"] = "export default { name: \"dual-slot-panel-overloaded-template-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorConditionalRenderFragmentMember_SelectsTheActiveNamedSlotOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseHeaderVariantRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <SlotPanel Header="@(UseCompactHeader ? CompactHeader : DetailedHeader)" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-panel-header-variant-runtime")]
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
                using Demo.Components;

                [ECMAScriptModule("./components/release-header-variant-runtime")]
                public partial class ReleaseHeaderVariantRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public bool UseCompactHeader { get; set; }

                    [Parameter] public string ReleaseName { get; set; } = "Orders API";

                    private RenderFragment CompactHeader => builder =>
                    {
                        builder.OpenElement(0, "strong");
                        builder.AddAttribute(1, "data-variant", "compact");
                        builder.AddContent(2, ReleaseName);
                        builder.CloseElement();
                    };

                    private RenderFragment DetailedHeader => builder =>
                    {
                        builder.OpenElement(3, "section");
                        builder.AddAttribute(4, "data-variant", "detailed");
                        builder.OpenElement(5, "h2");
                        builder.AddContent(6, ReleaseName);
                        builder.CloseElement();
                        builder.OpenElement(7, "p");
                        builder.AddContent(8, "Ready for deployment");
                        builder.CloseElement();
                        builder.CloseElement();
                    };
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseHeaderVariantRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "UseCompactHeader ? CompactHeader : DetailedHeader", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "props.useCompactHeader", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-header-variant-runtime.mjs",
            observation.ModuleText,
            "official-release-header-variant-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-header-variant-runtime.mjs";
            import panel from "./components/slot-panel-header-variant-runtime.mjs";

            test("official Razor conditional RenderFragment members select the active slot", () => {
                const compact = component.setup({
                    useCompactHeader: true,
                    releaseName: "Orders API"
                }, { slots: {} })();
                assert.equal(compact.name, panel);
                const compactHeader = compact.children.header()[0];
                assert.equal(compactHeader.name, "strong");
                assert.equal(compactHeader.props["data-variant"], "compact");
                assert.deepEqual(compactHeader.children, ["Orders API"]);

                const detailed = component.setup({
                    useCompactHeader: false,
                    releaseName: "Orders API"
                }, { slots: {} })();
                assert.equal(detailed.name, panel);
                const detailedHeader = detailed.children.header()[0];
                assert.equal(detailedHeader.name, "section");
                assert.equal(detailedHeader.props["data-variant"], "detailed");
                assert.equal(detailedHeader.children[0].name, "h2");
                assert.deepEqual(detailedHeader.children[0].children, ["Orders API"]);
                assert.equal(detailedHeader.children[1].name, "p");
                assert.deepEqual(detailedHeader.children[1].children, ["Ready for deployment"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-panel-header-variant-runtime.mjs"] = "export default { name: \"slot-panel-header-variant-runtime\" };"
            });
    }
}
