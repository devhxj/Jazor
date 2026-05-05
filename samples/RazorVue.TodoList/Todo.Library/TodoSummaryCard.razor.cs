using ECMAScript;
using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace Todo.Library;

[ECMAScriptModule("./components/todo-summary-card")]
public partial class TodoSummaryCard : ComponentBase, IVueComponent
{
    [Parameter]
    public int TotalCount { get; set; }

    [Parameter]
    public int CompletedCount { get; set; }

    [Parameter]
    public int OpenCount { get; set; }

    [Parameter]
    public int PinnedCount { get; set; }

    private string TotalLabel => TotalCount + " tasks in scope";

    private string CompletedLabel => CompletedCount + " completed";

    private string OpenLabel => OpenCount + " still active";

    private string PinnedLabel => PinnedCount + " pinned for focus";
}
