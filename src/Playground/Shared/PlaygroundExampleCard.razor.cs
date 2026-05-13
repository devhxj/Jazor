using ECMAScript;
using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;
using Playground.Models;
using static ECMAScript.Vue3;

namespace Playground.Shared;

[ECMAScriptModule("./components/playground-example-card")]
public partial class PlaygroundExampleCard : ComponentBase, IVueComponent
{
    [Parameter]
    public PlaygroundExampleSummary Example { get; set; } = default!;

    [Parameter]
    public string DetailHref { get; set; } = string.Empty;
}
