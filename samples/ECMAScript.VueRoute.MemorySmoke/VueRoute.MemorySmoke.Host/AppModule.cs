using ECMAScript;
using static ECMAScript.Vue;
using static ECMAScript.VueRoute;

namespace VueRoute.MemorySmoke.Host;

[ECMAScriptModule("host/app.mjs")]
public static class AppModule
{
    public static VueApp CreateConfiguredApp()
    {
        var router = RouteRuntimeModule.CreateRouterRuntime();
        return CreateConfiguredApp(router);
    }

    public static void Boot(string selector)
    {
        var router = RouteRuntimeModule.CreateRouterRuntime();
        var app = CreateConfiguredApp(router);
        _ = router.IsReady().Then(() =>
        {
            app.Mount(selector);
        });
    }

    private static VueApp CreateConfiguredApp(Router router)
    {
        var app = CreateApp(DefineComponent(new VueComponentOptions
        {
            Name = "VueRouteMemorySmokeRoot",
            Render = RenderRoot
        }));

        app.Use(router);
        _ = router.Push("/");
        app.OnUnmount(() => router.ClearRoutes());
        return app;
    }

    private static RouterLinkSlots CreateNavLinkSlots(string label)
    {
        return new RouterLinkSlots
        {
            Default = scope => new IVNode[]
            {
                H("span", label + " " + scope.Href)
            }
        };
    }

    private static IVNode RenderRoot()
        => H("main", new VueObject
        {
            Class = "route-root"
        }, new IVNode[]
        {
            H("section", new VueObject
            {
                Class = "route-hero"
            }, new IVNode[]
            {
                H("p", new VueObject
                {
                    Class = "route-kicker"
                }, "ECMAScript.VueRoute production sample"),
                H("h1", new VueObject
                {
                    Class = "route-title"
                }, "Typed Vue Router authoring, guards, links, router-view slots, and runtime smoke coverage"),
                H("p", new VueObject
                {
                    Class = "route-copy"
                }, "The sample keeps Vue Router as a normal external runtime while exercising strongly typed route objects, route props, global and component guards, useLink(), RouterLink, RouterView, injection keys, and loadRouteLocation().")
            }),
            H(RouteComponentsModule.RouteProbe),
            H("nav", new VueObject
            {
                Class = "route-nav"
            }, new IVNode[]
            {
                H(RouterLink, new RouterLinkProps
                {
                    To = "/",
                    ActiveClass = "route-link--active",
                    ExactActiveClass = "route-link--exact"
                }, CreateNavLinkSlots("Home")),
                H(RouterLink, new RouterLinkProps
                {
                    To = new RouteLocationAsRelative
                    {
                        Name = "detail",
                        Params = new RouteParamsRaw
                        {
                            { "id", "5" }
                        },
                        Query = new LocationQueryRaw
                        {
                            { "via", "nav" }
                        }
                    },
                    ActiveClass = "route-link--active",
                    ExactActiveClass = "route-link--exact"
                }, CreateNavLinkSlots("Detail 5")),
                H(RouterLink, new RouterLinkProps
                {
                    To = new RouteLocationAsRelative
                    {
                        Name = "query",
                        Query = new LocationQueryRaw
                        {
                            { "tab", "overview" }
                        }
                    },
                    ActiveClass = "route-link--active",
                    ExactActiveClass = "route-link--exact"
                }, CreateNavLinkSlots("Query"))
            }),
            H("section", new VueObject
            {
                Class = "route-state"
            }, new IVNode[]
            {
                H("p", "global guard: " + RouteRuntimeModule.GlobalGuardLog.Value),
                H("p", "afterEach: " + RouteRuntimeModule.AfterEachLog.Value),
                H("p", "component guard: " + RouteRuntimeModule.ComponentGuardLog.Value),
                H("p", "loaded path: " + RouteRuntimeModule.LastLoadedPath.Value)
            }),
            H(RouterView)
        });
}
