using ECMAScript;
using static ECMAScript.Vue;

namespace RazorVue.Authoring;

/// <summary>Vue framing owned by the sample host; page behavior remains ordinary Razor.</summary>
[ECMAScriptModule("app.mjs")]
public static class Bootstrap
{
    private static readonly IVueComponent Root = DefineComponent(new VueComponentOptions
    {
        Name = "RazorVueAuthoringRoot",
        Setup = Setup
    });

    private static readonly bool Started = Start();

    private static VueRenderCallback Setup()
    {
        var routeHost = AuthoringRoutingModule.CreateNavigationHost(null);
        return () =>
        {
            var route = routeHost.ResolveRoute();
            return route is null
                ? H("main", new VueObject { Class = "authoring-not-found" }, "Page not found")
                : RenderRoute(route);
        };
    }

    private static IVNode RenderRoute(AuthoringRoute route)
    {
        // Route matching stays in the runtime. The host only turns the generated page/layout
        // pair into Vue's final render-function shape; LayoutComponentBase.Body is a named slot.
        var component = route.Component;
        var parameters = route.Parameters;
        var layout = route.Layout;
        return layout is null
            ? H(component, parameters)
            : H(layout, new VueSlots
            {
                ["Body"] = () => H(component, parameters)
            });
    }

    private static bool Start()
    {
        AuthoringStyleSheet.EnsureLoaded();
        CreateApp(Root).Mount("#app");
        return true;
    }
}
