using ECMAScript;
using static ECMAScript.Vue3;
using static ECMAScript.VueRoute;

namespace VueRoute.MemorySmoke.Host;

[ECMAScriptModule("tests/router-testing.mjs")]
public static class RouteHarnessModule
{
    public static Router CreateTestingRouter()
        => RouteRuntimeModule.CreateRouterRuntime();

    public static IPromise<RouteRuntimeSnapshot> RunScenario()
    {
        var router = CreateTestingRouter();
        return RouteRuntimeModule.NavigateScenario(router);
    }

    public static IPromise<string> NavigateLegacyRedirect()
    {
        var router = CreateTestingRouter();
        return router.Push("/legacy/11#relay").Then(() =>
        {
            return LoadRouteLocation(router.CurrentRoute.Value);
        }).Then(loaded =>
        {
            return loaded.FullPath;
        });
    }

    public static IPromise<string> NavigateBlockedPath()
    {
        var router = CreateTestingRouter();
        return router.Push("/").Then(() =>
        {
            return router.Push("/blocked");
        }).Then(_ =>
        {
            return router.CurrentRoute.Value.Path + "|" + RouteRuntimeModule.GlobalGuardLog.Value;
        });
    }
}
