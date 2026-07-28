using ECMAScript;
using static ECMAScript.Vue3;
using static ECMAScript.VueRoute;

namespace JazorAdmin;

[ECMAScriptModule("components/jazor-admin-bootstrap.mjs")]
public static class Bootstrap
{
    public static Router CreateRouterRuntime(IVueComponent shellComponent)
        => CreateRouter(new RouterOptions
        {
            History = CreateWebHistory(),
            LinkActiveClass = "is-route-active",
            LinkExactActiveClass = "is-route-exact-active",
            Routes = Jazor.Admin.AdminRouteCatalog.BuildRouteRecords(
                Routes.RouterItems,
                shellComponent,
                Routes.DashboardKey)
        });

    public static void Boot(string selector, IVueComponent shellComponent)
    {
        var router = CreateRouterRuntime(shellComponent);
        var app = CreateApp(DefineComponent(new VueComponentOptions
        {
            Name = "JazorAdminRoot",
            Render = RenderRoot
        }));

        app.Use(router);
        app.OnUnmount(() => router.ClearRoutes());
        _ = router.IsReady().Then(() => app.Mount(selector));
    }

    private static IVNode RenderRoot()
        => H(RouterView);
}
