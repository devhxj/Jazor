using ECMAScript;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace Todo.Library;

/// <summary>Small interactive RazorVue app used by the Windows development-host gate.</summary>
[ECMAScriptModule("./components/todo-app")]
public partial class TodoApp : ComponentBase, IVueComponent
{
    private readonly List<TodoTask> tasks =
    [
        new("Verify the generated module", false),
        new("Exercise template HMR", false),
        new("Keep this state during HMR", true)
    ];

    private string draftTitle = string.Empty;

    // This marker is intentionally code-behind owned. The browser gate changes it to prove
    // that a logic boundary triggers a full navigation instead of Vue component replacement.
    private string LogicMarker => "logic-v1";

    private string TemplateLabel => "Template marker v1";

    private int OpenCount => tasks.Count(static task => !task.IsDone);

    private int DoneCount => tasks.Count(static task => task.IsDone);

    private void AddTask()
    {
        var title = draftTitle.Trim();
        if (title.Length == 0)
            return;

        tasks.Add(new TodoTask(title, false));
        draftTitle = string.Empty;
    }

    private sealed class TodoTask(string title, bool isDone)
    {
        public string Title { get; } = title;

        public bool IsDone { get; set; } = isDone;
    }
}
