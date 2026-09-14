using ECMAScript;
using ECMAScript.TDesign;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace RazorVue.Authoring;

[ECMAScriptModule("./components/task-table")]
public partial class TaskTable : ComponentBase, IVueComponent
{
    [Parameter, EditorRequired]
    public TaskRow[] Rows { get; set; } = [];

    [CascadingParameter(Name = "workspace")]
    public string WorkspaceName { get; set; } = "Unknown workspace";

    private TPrimaryTableCol<TaskRow>[] Columns =>
    [
        new()
        {
            ColKey = nameof(TaskRow.Title),
            Title = "Task",
            Cell = "title"
        },
        new() { ColKey = nameof(TaskRow.Owner), Title = "Owner" },
        new() { ColKey = nameof(TaskRow.Status), Title = "Status" }
    ];

    private void OnRowClick(TRowEventContext<TaskRow> context)
        => SelectedRowId = context.Row.Id;

    private int SelectedRowId { get; set; }
}
