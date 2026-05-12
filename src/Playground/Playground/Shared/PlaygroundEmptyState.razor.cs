using ECMAScript;
using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue3;

namespace Playground.Shared;

[ECMAScriptModule("./components/playground-empty-state")]
public partial class PlaygroundEmptyState : ComponentBase, IVueComponent
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Body { get; set; } = string.Empty;
}
