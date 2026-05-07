using ECMAScript;
using static ECMAScript.Vue3;
using static ECMAScript.VueRoute;

namespace VueRoute.MemorySmoke.Host;

[ECMAScriptModule("components/route-shell.mjs")]
public static class RouteComponentsModule
{
    public static readonly IVueComponent HomeView = DefineComponent(new VueComponentOptions
    {
        Name = "MemorySmokeHomeView",
        Setup = CreateHomeSetup
    });

    public static readonly IVueComponent<DetailRouteProps> DetailView = DefineComponent(new VueComponentOptions<DetailRouteProps>
    {
        Name = "MemorySmokeDetailView",
        Props = ["id", "source"],
        Setup = CreateDetailSetup
    });

    public static readonly IVueComponent<QueryEchoProps> QueryView = DefineComponent(new VueComponentOptions<QueryEchoProps>
    {
        Name = "MemorySmokeQueryView",
        Props = ["tab"],
        Setup = CreateQuerySetup
    });

    public static readonly IVueComponent BlockedView = DefineComponent(new VueComponentOptions
    {
        Name = "MemorySmokeBlockedView",
        Setup = () => () => H("section", new VueObject
        {
            Class = "route-card route-card--blocked"
        }, new IVNode[]
        {
            H("h2", "Blocked"),
            H("p", "Navigation should not land here while the guard is active.")
        })
    });

    public static readonly IVueComponent RouteProbe = DefineComponent(new VueComponentOptions
    {
        Name = "MemorySmokeRouteProbe",
        Setup = CreateProbeSetup
    });

    private static VueRenderCallback CreateHomeSetup()
    {
        var router = UseRouter();
        var route = UseRoute();
        var greeting = Computed(() => "home:" + route.Name);

        Action navigateToDetail = () =>
        {
            _ = router.Push(new RouteLocationAsRelative
            {
                Name = "detail",
                Params = new RouteParamsRaw
                {
                    { "id", "7" }
                },
                Query = new LocationQueryRaw
                {
                    { "via", "button" }
                },
                Hash = "#summary"
            });
        };

        return () => H("section", new VueObject
        {
            Class = "route-card route-card--home"
        }, new IVNode[]
        {
            H("h2", "Home"),
            H("p", "current name: " + route.Name),
            H("p", "current path: " + route.Path),
            H("p", "message: " + greeting.Value),
            CreateButton("Go detail", "action-button action-button--accent", navigateToDetail)
        });
    }

    private static VueRenderCallback CreateDetailSetup(DetailRouteProps props, VueSetupContext context)
    {
        var router = UseRouter();
        var route = UseRoute();
        var viewDepth = Inject(ViewDepthKey);
        var matchedRoute = Inject(MatchedRouteKey);
        var routedLocation = Inject(RouterViewLocationKey);
        var composedLink = UseLink(new UseLinkOptions
        {
            To = ToRef(() => new RouteLocationAsRelative
            {
                Name = "query",
                Query = new LocationQueryRaw
                {
                    { "tab", props.Id + "-details" }
                }
            }),
            Replace = Computed(() => false)
        });

        OnBeforeRouteUpdate((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
        {
            RouteRuntimeModule.ComponentGuardLog.Value = "update:" + from.Path + "->" + to.Path;
            return true;
        });
        OnBeforeRouteLeave((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
        {
            RouteRuntimeModule.ComponentGuardLog.Value = "leave:" + from.Path + "->" + to.Path;
            return true;
        });

        Action pushBlocked = () =>
        {
            _ = router.Push("/blocked");
        };

        Action followComposedLink = () =>
        {
            _ = composedLink.Navigate();
        };

        return () => H("section", new VueObject
        {
            Class = "route-card route-card--detail"
        }, new IVNode[]
        {
            H("h2", "Detail " + props.Id),
            H("p", "source: " + props.Source),
            H("p", "route path: " + route.Path),
            H("p", "query via: " + (route.Query["via"] ?? "")),
            H("p", "matched path: " + (matchedRoute!.Value is null ? "" : matchedRoute.Value.Path)),
            H("p", "view depth: " + viewDepth!.AsNumber),
            H("p", "injected route path: " + routedLocation!.Value.Path),
            H("p", "useLink href: " + composedLink.Href.Value),
            H("div", new VueObject
            {
                Class = "route-actions"
            }, new IVNode[]
            {
                CreateButton("Blocked target", "action-button", pushBlocked),
                CreateButton("Follow composed link", "action-button action-button--accent", followComposedLink)
            }),
            H("div", new VueObject
            {
                Class = "route-slot-probe"
            }, new IVNode[]
            {
                H("strong", "slot keys visible: "),
                H("span", context.Slots.Default is null ? "none" : "default")
            })
        });
    }

    private static VueRenderCallback CreateQuerySetup(QueryEchoProps props, VueSetupContext context)
    {
        _ = context;
        var route = UseRoute();

        return () => H("section", new VueObject
        {
            Class = "route-card route-card--query"
        }, new IVNode[]
        {
            H("h2", "Query"),
            H("p", "tab prop: " + props.Tab),
            H("p", "hash: " + route.Hash),
            H("p", "fullPath: " + route.FullPath)
        });
    }

    private static VueRenderCallback CreateProbeSetup()
    {
        var route = UseRoute();
        var router = UseRouter();
        var currentRoute = router.CurrentRoute;

        TriggerRef(currentRoute);

        return () => H("aside", new VueObject
        {
            Class = "route-probe"
        }, new IVNode[]
        {
            H("p", "probe current route: " + currentRoute.Value.FullPath),
            H("p", "probe useRoute path: " + route.Path)
        });
    }

    private static IVNode CreateButton(string label, string className, Action handler)
        => H("button", new VueObject
        {
            Type = "button",
            Class = className,
            Events = new VueEventHandlers
            {
                ["onClick"] = handler
            }
        }, label);
}
