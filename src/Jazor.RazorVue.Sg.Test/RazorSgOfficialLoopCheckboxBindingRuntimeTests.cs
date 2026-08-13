namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialLoopCheckboxBindingRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLoopCheckboxBinding_UpdatesTheCapturedTaskOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TodoBoard.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            @foreach (var task in tasks)
            {
                <input type="checkbox" @bind="task.IsDone" data-title="@task.Title" />
            }
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/todo-board-runtime")]
            public partial class TodoBoard : ComponentBase, IVueComponent
            {
                private readonly TodoTask[] tasks =
                [
                    new("Verify direct binding", false),
                    new("Keep primary constructor support", true)
                ];

                private sealed class TodoTask(string title, bool isDone)
                {
                    public string Title { get; } = title;

                    public bool IsDone { get; set; } = isDone;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TodoBoard");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateBinder", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "task.IsDone", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "eventOrValue.target[\"checked\"]", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "task.IsDone =", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("    Title: null", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("    IsDone: false", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/todo-board-runtime.mjs",
            observation.ModuleText,
            "official-todo-board-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/todo-board-runtime.mjs";

            function collect(node, name) {
                if (node == null) return [];
                if (Array.isArray(node)) return node.flatMap(item => collect(item, name));
                const children = collect(node.children, name);
                return node.name === name ? [node, ...children] : children;
            }

            test("official Razor loop checkbox bind updates only the captured task", async () => {
                const render = component.setup({}, { slots: {} });
                const initialInputs = collect(render(), "input");
                assert.equal(initialInputs.length, 2);
                assert.equal(initialInputs[0].props.checked, false);
                assert.equal(initialInputs[1].props.checked, true);
                assert.equal(initialInputs[0].props["data-title"], "Verify direct binding");
                assert.equal(typeof initialInputs[0].props.onChange, "function");

                await Promise.resolve(initialInputs[0].props.onChange({ target: { checked: true } }));

                const updatedInputs = collect(render(), "input");
                assert.equal(updatedInputs[0].props.checked, true);
                assert.equal(updatedInputs[1].props.checked, true);

                await Promise.resolve(updatedInputs[1].props.onChange({ target: { checked: false } }));

                const toggledInputs = collect(render(), "input");
                assert.equal(toggledInputs[0].props.checked, true);
                assert.equal(toggledInputs[1].props.checked, false);
            });
            """);
    }
}
