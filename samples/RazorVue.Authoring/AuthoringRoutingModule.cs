using System.ComponentModel;
using ECMAScript;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace RazorVue.Authoring;

/// <summary>
/// Typed sample-local view of the existing route host. The routing runtime owns browser
/// history and route matching; this sample only chooses which generated page to render.
/// </summary>
[ECMAScript]
[Description("@#")]
internal sealed record AuthoringRouteHost : VueProps
{
    [Description("@#resolveRoute")]
    public extern Func<AuthoringRoute?> ResolveRoute { get; }
}

[ECMAScript]
[Description("@#")]
internal sealed record AuthoringRoute : VueProps
{
    [Description("@#component")]
    public extern IVueComponent Component { get; }

    [Description("@#layout")]
    public extern IVueComponent? Layout { get; }

    [Description("@#parameters")]
    public extern VueObject Parameters { get; }

    [Description("@#template")]
    public extern string Template { get; }
}

[ECMAScript("@jazor/vue-runtime/blazor-routing.mjs", Transform.Import)]
internal static class AuthoringRoutingModule
{
    [ECMAScriptName("createNavigationHost")]
    public extern static AuthoringRouteHost CreateNavigationHost(Action<NavigationManager>? onChange);
}
