using ECMAScript;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace Todo.Library;

[ECMAScriptModule("./components/todo-summary-card")]
public partial class TodoSummaryCard : ComponentBase, IVueComponent
{
    [Parameter]
    public string Title { get; set; } = "Cascade child";

    [CascadingParameter(Name = "theme")]
    public string Theme { get; set; } = "missing";
}
