using ECMAScript;
using ECMAScript.Vuetify;
using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;
using Playground.Models;
using static ECMAScript.Vue3;

namespace Playground.Pages;

[ECMAScriptModule("./pages/playground-catalog-page")]
public partial class PlaygroundCatalogPage : ComponentBase, IVueComponent
{
    [Parameter]
    public PlaygroundCatalogViewModel Model { get; set; } = default!;

    [Parameter]
    public EventCallback<string?> QueryChanged { get; set; }

    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> CategorySelected { get; set; }

    [Parameter]
    public string DetailQuerySuffix { get; set; } = string.Empty;
}
