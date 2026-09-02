using ECMAScript;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace RazorVue.Authoring;

[ECMAScriptModule("./components/task-details")]
public partial class TaskDetails : ComponentBase, IVueComponent
{
    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    public int TaskId { get; set; }

    [Parameter]
    [SupplyParameterFromQuery(Name = "highlight")]
    public bool Highlight { get; set; }

    private string HighlightState => Highlight ? "highlighted" : "standard";

    private void ClearHighlight()
        => Navigation.NavigateTo("/tasks/" + TaskId + "?highlight=false");

    private void BackToBoard()
        => Navigation.NavigateTo("/tasks");
}
