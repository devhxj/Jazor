namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialReleaseWorkflowRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorReleaseWorkflow_ComposesFormBindingModelSlotsAndActionsOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseWorkflow.razor",
            documentText:
            """
            @using Demo.Components
            @using Microsoft.AspNetCore.Components.Web

            <form @formname="release-workflow" @onsubmit="Submit" @onsubmit:preventDefault="PreventSubmit">
                <label for="release-filter">Filter</label>
                <input id="release-filter" @bind="Filter" @bind:event="oninput" />
                <ReleasePanel @ref="panel" @bind-SelectedId="SelectedId">
                    <Header>
                        <strong data-environment="@Environment">@Filter</strong>
                    </Header>
                    <ItemTemplate Context="release">
                        @if (release.IsReady)
                        {
                            <button @key="release.Id" @onclick="() => QueueRelease(release.Id)">@release.Name</button>
                        }
                        else
                        {
                            <span data-state="pending">@release.Name</span>
                        }
                    </ItemTemplate>
                </ReleasePanel>
                <output data-queued-release="@QueuedReleaseId">@QueuedReleaseId</output>
                <output data-submit-count="@SubmitCount">@SubmitCount</output>
            </form>
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                public sealed record ReleaseEntry(int Id, string Name, bool IsReady);

                [ECMAScriptModule("./components/release-panel-workflow")]
                [VueLibraryEmit(nameof(SelectedIdChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
                public sealed class ReleasePanel : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("modelValue")]
                    [Parameter] public int SelectedId { get; set; }
                    [Parameter] public EventCallback<int> SelectedIdChanged { get; set; }
                    [Parameter] public RenderFragment? Header { get; set; }
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

                [ECMAScriptModule("./components/release-workflow")]
                public partial class ReleaseWorkflow : ComponentBase, IVueComponent
                {
                    private string Filter { get; set; } = "all";
                    private int SelectedId { get; set; } = 1;
                    private int QueuedReleaseId { get; set; }
                    private int SubmitCount { get; set; }
                    private string Environment { get; } = "production";
                    private ReleasePanel? panel;

                    private bool PreventSubmit => true;

                    private void QueueRelease(int releaseId)
                    {
                        QueuedReleaseId = releaseId;
                    }

                    private void Submit()
                    {
                        SubmitCount++;
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseWorkflow");

        StringAssert.Contains(observation.GeneratedCSharp, "AddNamedEvent", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "CreateBinder", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddComponentReferenceCapture", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "SetKey", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "from \"./release-panel-workflow.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "modelValue: state.selectedId", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onInput", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "header:", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "item:", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-workflow.mjs",
            observation.ModuleText,
            "official-release-workflow-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-workflow.mjs";
            import releasePanel from "./components/release-panel-workflow.mjs";

            const findPanel = form => form.children.find(child => child?.name === releasePanel);
            const findQueuedOutput = form => form.children.find(child => child?.props && "data-queued-release" in child.props);
            const findSubmitOutput = form => form.children.find(child => child?.props && "data-submit-count" in child.props);

            test("official Razor release workflow preserves form, model, slot, and action behavior", async () => {
                const render = component.setup({}, { slots: {} });
                let form = render();
                assert.equal(form.name, "form");
                assert.equal(form.children[1].name, "input");
                assert.equal(form.children[1].props.value, "all");

                let prevented = false;
                await Promise.resolve(form.props.onSubmit({ preventDefault: () => prevented = true }));
                assert.equal(prevented, true);
                form = render();
                assert.equal(findSubmitOutput(form).props["data-submit-count"], 1);

                form.children[1].props.onInput({ target: { value: "ready" } });
                form = render();
                assert.equal(form.children[1].props.value, "ready");

                let panel = findPanel(form);
                assert.equal(panel.props.modelValue, 1);
                assert.equal(typeof panel.props["onUpdate:modelValue"], "function");
                await Promise.resolve(panel.props["onUpdate:modelValue"](42));
                form = render();
                panel = findPanel(form);
                assert.equal(panel.props.modelValue, 42);

                const header = panel.children.header();
                assert.equal(header[0].name, "strong");
                assert.equal(header[0].props["data-environment"], "production");
                assert.deepEqual(header[0].children, ["ready"]);

                const ready = panel.children.item({ id: 7, name: "Deploy API", isReady: true });
                assert.equal(ready[0].name, "button");
                assert.equal(ready[0].props.key, 7);
                await Promise.resolve(ready[0].props.onClick());
                form = render();
                assert.equal(findQueuedOutput(form).props["data-queued-release"], 7);
                assert.deepEqual(findQueuedOutput(form).children, [7]);

                const pending = panel.children.item({ id: 8, name: "Audit Worker", isReady: false });
                assert.equal(pending[0].name, "span");
                assert.equal(pending[0].props["data-state"], "pending");
                assert.deepEqual(pending[0].children, ["Audit Worker"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-panel-workflow.mjs"] = "export default { name: \"release-panel-workflow\" };"
            });
    }
}
