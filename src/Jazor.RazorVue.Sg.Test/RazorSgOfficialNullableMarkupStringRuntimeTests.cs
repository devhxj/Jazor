namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialNullableMarkupStringRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorNullableMarkupString_PreservesMarkupAndEmptyOutputOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\NullableReleaseSummary.razor",
            documentText:
            """
            @Summary
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/nullable-release-summary-runtime")]
            public partial class NullableReleaseSummary : ComponentBase, IVueComponent
            {
                [Parameter] public MarkupString? Summary { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NullableReleaseSummary");

        StringAssert.Contains(observation.GeneratedCSharp, "AddContent(0,", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "Summary", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "createStaticVNode", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/nullable-release-summary-runtime.mjs",
            observation.ModuleText,
            "official-nullable-release-summary-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/nullable-release-summary-runtime.mjs";

            test("official Razor nullable MarkupString renders markup only when a value exists", () => {
                const populated = component.setup(
                    { summary: "<strong data-release=\"orders\">Orders ready</strong>" },
                    { slots: {} })();
                assert.equal(populated.name, "__static");
                assert.equal(populated.props.html, "<strong data-release=\"orders\">Orders ready</strong>");
                assert.equal(populated.props.count, 1);

                const empty = component.setup({ summary: null }, { slots: {} })();
                assert.equal(empty, null);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorNullableMarkupStringInsideElement_ExpandsEmptyContentOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\NullableReleaseSummaryPanel.razor",
            documentText:
            """
            <section data-release-summary="panel">
                @Summary
            </section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/nullable-release-summary-panel-runtime")]
            public partial class NullableReleaseSummaryPanel : ComponentBase, IVueComponent
            {
                [Parameter] public MarkupString? Summary { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NullableReleaseSummaryPanel");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/nullable-release-summary-panel-runtime.mjs",
            observation.ModuleText,
            "official-nullable-release-summary-panel-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/nullable-release-summary-panel-runtime.mjs";

            test("official Razor nullable MarkupString expands to zero or one element child", () => {
                const populated = component.setup(
                    { summary: "<em>Deployment pending</em>" },
                    { slots: {} })();
                assert.equal(populated.name, "section");
                assert.equal(populated.props["data-release-summary"], "panel");
                assert.equal(populated.children.length, 1);
                assert.equal(populated.children[0].name, "__static");
                assert.equal(populated.children[0].props.html, "<em>Deployment pending</em>");

                const empty = component.setup({ summary: null }, { slots: {} })();
                assert.equal(empty.name, "section");
                assert.deepEqual(empty.children, []);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorNullableMarkupStringMethod_EvaluatesContentOnceOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\NullableReleaseSummaryMethod.razor",
            documentText:
            """
            @ReadSummary()
            <span data-summary-reads="@SummaryReads"></span>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/nullable-release-summary-method-runtime")]
            public partial class NullableReleaseSummaryMethod : ComponentBase, IVueComponent
            {
                [Parameter] public MarkupString? Summary { get; set; }

                private int SummaryReads { get; set; }

                private MarkupString? ReadSummary()
                {
                    SummaryReads++;
                    return Summary;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NullableReleaseSummaryMethod");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "readSummary()", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/nullable-release-summary-method-runtime.mjs",
            observation.ModuleText,
            "official-nullable-release-summary-method-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { Fragment } from "vue";

            import component from "./components/nullable-release-summary-method-runtime.mjs";

            test("official Razor nullable MarkupString methods are evaluated once per render", () => {
                const render = component.setup(
                    { summary: "<b>Deploy ready</b>" },
                    { slots: {} });
                const output = render();

                assert.equal(output.name, Fragment);
                const markup = output.children.find(node => node?.name === "__static");
                const readCount = output.children.find(node => node?.name === "span");
                assert.ok(markup);
                assert.ok(readCount);
                assert.equal(markup.props.html, "<b>Deploy ready</b>");
                assert.equal(readCount.props["data-summary-reads"], 1);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorNullableMarkupStringSlot_ExpandsToAnEmptySlotResultOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\NullableReleaseSummarySlot.razor",
            documentText:
            """
            @using Demo.Components

            <ReleaseSummaryPanel>
                @Summary
            </ReleaseSummaryPanel>
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/nullable-release-summary-slot-panel-runtime")]
                public sealed class ReleaseSummaryPanel : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? ChildContent { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                using Demo.Components;

                [ECMAScriptModule("./components/nullable-release-summary-slot-runtime")]
                public partial class NullableReleaseSummarySlot : ComponentBase, IVueComponent
                {
                    [Parameter] public MarkupString? Summary { get; set; }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NullableReleaseSummarySlot");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "default:", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "createStaticVNode", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/nullable-release-summary-slot-runtime.mjs",
            observation.ModuleText,
            "official-nullable-release-summary-slot-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/nullable-release-summary-slot-runtime.mjs";
            import panelComponent from "./components/nullable-release-summary-slot-panel-runtime.mjs";

            test("official Razor nullable MarkupString child content emits an empty default slot when absent", () => {
                const populated = component.setup(
                    { summary: "<i>Release verified</i>" },
                    { slots: {} })();
                assert.equal(populated.name, panelComponent);
                assert.equal(typeof populated.children.default, "function");
                const populatedNodes = populated.children.default();
                assert.equal(populatedNodes.length, 1);
                assert.equal(populatedNodes[0].name, "__static");
                assert.equal(populatedNodes[0].props.html, "<i>Release verified</i>");

                const empty = component.setup({ summary: null }, { slots: {} })();
                assert.equal(empty.name, panelComponent);
                assert.deepEqual(empty.children.default(), []);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/nullable-release-summary-slot-panel-runtime.mjs"] = "export default { name: \"nullable-release-summary-slot-panel-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorNullableMarkupStringInLoop_ExpandsEachElementChildOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\NullableReleaseSummaryList.razor",
            documentText:
            """
            @foreach (var release in Releases)
            {
                <article data-release="@release.Id">
                    @release.Summary
                </article>
            }
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/nullable-release-summary-list-runtime")]
            public partial class NullableReleaseSummaryList : ComponentBase, IVueComponent
            {
                [Parameter] public IReadOnlyList<ReleaseSummary> Releases { get; set; } = [];
            }

            public sealed record ReleaseSummary(string Id, MarkupString? Summary);
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NullableReleaseSummaryList");

        StringAssert.Contains(observation.GeneratedCSharp, "foreach", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "Array.from(props.releases ?? []", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/nullable-release-summary-list-runtime.mjs",
            observation.ModuleText,
            "official-nullable-release-summary-list-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/nullable-release-summary-list-runtime.mjs";

            test("official Razor nullable MarkupString expands independently inside foreach element children", () => {
                const releases = component.setup(
                    {
                        releases: [
                            { id: "orders", summary: "<strong>Orders ready</strong>" },
                            { id: "billing", summary: null }
                        ]
                    },
                    { slots: {} })();

                assert.equal(releases.length, 2);
                assert.equal(releases[0].name, "article");
                assert.equal(releases[0].props["data-release"], "orders");
                assert.equal(releases[0].children.length, 1);
                assert.equal(releases[0].children[0].name, "__static");
                assert.equal(releases[0].children[0].props.html, "<strong>Orders ready</strong>");

                assert.equal(releases[1].name, "article");
                assert.equal(releases[1].props["data-release"], "billing");
                assert.deepEqual(releases[1].children, []);
            });
            """);
    }
}
