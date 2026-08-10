namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialPatternDeclarationAndListRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorPatternLocals_RenderReleaseQueueSummaryOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseQueueSummary.razor",
            documentText:
            """
            @if (QueueTitle is string title)
            {
                <h2 data-queue-title="@title">@title</h2>
            }
            @if (ReleaseIds is [var firstRelease, ..])
            {
                <span data-first-release="@firstRelease">@firstRelease</span>
            }
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-queue-summary")]
            public partial class ReleaseQueueSummary : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? QueueTitle { get; set; }

                [Parameter]
                public int[]? ReleaseIds { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseQueueSummary");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "props.QueueTitle", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "props.ReleaseIds", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-queue-summary.mjs",
            observation.ModuleText,
            "official-pattern-declaration-and-list-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { Fragment } from "vue";

            import component from "./components/release-queue-summary.mjs";

            test("Razor pattern locals render only when queue values match", () => {
                const empty = component.setup({}, { slots: {} })();
                assert.equal(empty.name, Fragment);
                assert.deepEqual(empty.children, [null, null]);

                const queue = component.setup({
                  QueueTitle: "Release Queue",
                  ReleaseIds: [42, 84]
                }, { slots: {} })();
                const nodes = queue.children.filter(node => node != null);

                assert.equal(nodes.length, 2);
                assert.equal(nodes[0].name, "h2");
                assert.equal(nodes[0].props["data-queue-title"], "Release Queue");
                assert.deepEqual(nodes[0].children, ["Release Queue"]);
                assert.equal(nodes[1].name, "span");
                assert.equal(nodes[1].props["data-first-release"], 42);
                assert.deepEqual(nodes[1].children, [42]);
            });
            """);
    }
}
