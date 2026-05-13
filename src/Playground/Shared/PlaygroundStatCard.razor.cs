using ECMAScript;
using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue3;

namespace Playground.Shared;

[ECMAScriptModule("./components/playground-stat-card")]
public partial class PlaygroundStatCard : ComponentBase, IVueComponent
{
    [Parameter]
    public string Eyebrow { get; set; } = string.Empty;

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public string Caption { get; set; } = string.Empty;
}
