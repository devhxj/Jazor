using static ECMAScript.VueRoute;

namespace JazorAdmin;

/// <summary>Imports the generated root component used by the application entry module.</summary>
[ECMAScript("components/app.mjs")]
[Description("@#")]
internal static class AppModule
{
#pragma warning disable CS0626 // The ECMAScript module binding supplies this generated module export at runtime.
    [ECMAScriptName("default")]
    public extern static IVueComponent Default { get; }
#pragma warning restore CS0626
}

/// <summary>Creates and mounts the Vue application from the generated root component.</summary>
[ECMAScriptModule("app.mjs")]
public static class Bootstrap
{
    // Importing this generated module is the whole startup contract. The field initializer keeps
    // the host-rendered HTML shell declarative and avoids a handwritten JavaScript boot wrapper.
    // 浏览器导入生成模块即完成启动；字段初始化让后端动态 HTML 壳保持声明式，不需要手写 JS 启动层。
    private static readonly bool started = Start();

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
            Name = "AdminRoot",
            Render = RenderRoot
        }));

        app.Use(router);
        app.OnUnmount(() => router.ClearRoutes());
        _ = router.IsReady().Then(() => app.Mount(selector));
    }

    private static IVNode RenderRoot()
        => H(RouterView);

    private static bool Start()
    {
        Boot("#app", AppModule.Default);
        return true;
    }
}
