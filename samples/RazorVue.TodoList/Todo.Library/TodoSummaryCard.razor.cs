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

    [Parameter]
    public string? TotalText { get; set; }

    [Parameter]
    public string? CompletedText { get; set; }

    [Parameter]
    public string? OpenText { get; set; }

    [Parameter]
    public string? PinnedText { get; set; }
}
