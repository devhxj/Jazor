using System.ComponentModel;
using ECMAScript;
using static ECMAScript.Vue3;
using static ECMAScript.VueRoute;

namespace VueRoute.MemorySmoke.Host;

[ECMAScriptModule("router/memory-router.mjs")]
public static class RouteRuntimeModule
{
    public static readonly IVueRef<string> GlobalGuardLog = Ref("guard:idle");
    public static readonly IVueRef<string> AfterEachLog = Ref("after:idle");
    public static readonly IVueRef<string> ErrorLog = Ref("error:none");
    public static readonly IVueRef<string> ComponentGuardLog = Ref("component:idle");
    public static readonly IVueRef<string> LastLoadedPath = Ref("");

    public static Router CreateRouterRuntime()
    {
        GlobalGuardLog.Value = "guard:idle";
        AfterEachLog.Value = "after:idle";
        ErrorLog.Value = "error:none";
        ComponentGuardLog.Value = "component:idle";
        LastLoadedPath.Value = "";

        RouteRecordPropsResolver detailProps = (RouteLocationNormalized to) => new DetailRouteProps
        {
            Id = to.Path.Replace("/users/", ""),
            Source = "route-props"
        };
        RouteRecordPropsResolver queryProps = (RouteLocationNormalized to) => new QueryEchoProps
        {
            Tab = to.FullPath.Replace("/query?tab=", "").Replace("#focus", "")
        };
        RouteRedirectCallback legacyRedirect = (RouteLocation to, RouteLocationNormalizedLoaded from) => new RouteLocationAsPath
        {
            Path = "/users/11",
            Query = new LocationQueryRaw
            {
                { "via", "legacy-redirect" }
            },
            Hash = "#relay"
        };

        var router = CreateRouter(new RouterOptions
        {
            History = CreateMemoryHistory("/memory-smoke"),
            LinkActiveClass = "route-link--active",
            LinkExactActiveClass = "route-link--exact",
            Routes =
            [
                new RouteRecordSingleView
                {
                    Path = "/",
                    Name = "home",
                    Component = RawRouteComponent.From(RouteComponentsModule.HomeView),
                    Meta = new RouteMeta
                    {
                        { "section", "home" },
                        { "requiresAudit", true }
                    }
                },
                new RouteRecordSingleView
                {
                    Path = "/users/:id",
                    Name = "detail",
                    Component = RawRouteComponent.From(RouteComponentsModule.DetailView),
                    Props = detailProps,
                    BeforeEnter = RouteRecordBeforeEnter.From((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
                    {
                        GlobalGuardLog.Value = "beforeEnter:" + from.Path + "->" + to.Path;
                        return true;
                    })
                },
                new RouteRecordSingleView
                {
                    Path = "/query",
                    Name = "query",
                    Component = RawRouteComponent.From(RouteComponentsModule.QueryView),
                    Props = queryProps
                },
                new RouteRecordRedirect
                {
                    Path = "/legacy/:id",
                    Redirect = legacyRedirect
                },
                new RouteRecordSingleView
                {
                    Path = "/blocked",
                    Name = "blocked",
                    Component = RawRouteComponent.From(RouteComponentsModule.BlockedView)
                }
            ]
        });

        router.BeforeEach((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
        {
            GlobalGuardLog.Value = "beforeEach:" + from.Path + "->" + to.Path;
            if (to.Path == "/blocked")
            {
                GlobalGuardLog.Value = "beforeEach:blocked";
                return false;
            }

            return true;
        });
        router.BeforeResolve((RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
        {
            GlobalGuardLog.Value = "beforeResolve:" + from.Path + "->" + to.Path;
            return Promise<NavigationGuardReturn?>.Resolve(true);
        });
        router.AfterEach((RouteLocationNormalizedLoaded to, RouteLocationNormalizedLoaded from, NavigationFailure? failure) =>
        {
            AfterEachLog.Value = "afterEach:" + from.Path + "->" + to.Path + ":" + (failure is null ? "ok" : "failure");
        });
        router.OnError((Error error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from) =>
        {
            ErrorLog.Value = error.Message + "|" + from.Path + "->" + to.Path;
        });

        return router;
    }

    public static RouteRuntimeSnapshot Snapshot(Router router)
    {
        var currentRoute = router.CurrentRoute.Value;
        return new RouteRuntimeSnapshot
        {
            CurrentPath = currentRoute.Path,
            CurrentFullPath = currentRoute.FullPath,
            GlobalGuard = GlobalGuardLog.Value,
            AfterEach = AfterEachLog.Value,
            ComponentGuard = ComponentGuardLog.Value,
            LoadedPath = LastLoadedPath.Value,
            IsReady = router.Listening
        };
    }

    public static IPromise<RouteRuntimeSnapshot> NavigateScenario(Router router)
    {
        return router.Push(new RouteLocationAsRelative
        {
            Name = "detail",
            Params = new RouteParamsRaw
            {
                { "id", "42" }
            },
            Query = new LocationQueryRaw
            {
                { "via", "scenario" }
            },
            Hash = "#summary"
        }).Then(() =>
        {
            return LoadRouteLocation(router.CurrentRoute.Value);
        }).Then(loaded =>
        {
            LastLoadedPath.Value = loaded.Path;
            return router.Replace("/query?tab=summary#focus");
        }).Then(() =>
        {
            return Snapshot(router);
        });
    }
}

[ECMAScript]
[Description("@#")]
public sealed record RouteRuntimeSnapshot : ECMAScript.Vue3.VueProps
{
    [Description("@#currentPath")]
    public string CurrentPath { get; init; } = "";

    [Description("@#currentFullPath")]
    public string CurrentFullPath { get; init; } = "";

    [Description("@#globalGuard")]
    public string GlobalGuard { get; init; } = "";

    [Description("@#afterEach")]
    public string AfterEach { get; init; } = "";

    [Description("@#componentGuard")]
    public string ComponentGuard { get; init; } = "";

    [Description("@#loadedPath")]
    public string LoadedPath { get; init; } = "";

    [Description("@#isReady")]
    public bool IsReady { get; init; }
}
