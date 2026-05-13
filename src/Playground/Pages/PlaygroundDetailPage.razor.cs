using ECMAScript;
using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;
using Playground.Models;
using static ECMAScript.Vue3;

namespace Playground.Pages;

[ECMAScriptModule("./pages/playground-detail-page")]
public partial class PlaygroundDetailPage : ComponentBase, IVueComponent
{
    [Parameter]
    public PlaygroundDetailViewModel Model { get; set; } = default!;

    [Parameter]
    public EventCallback ToggleFavorite { get; set; }
}
