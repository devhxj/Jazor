using Jero.Web.Infrastructure.Storage;
using Jero.Web.Pages.Auth;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;
using static ECMAScript.Vue;
using static ECMAScript.VueRoute;

namespace Jero.Web.Composition;

/// <summary>在浏览器中配置应用路由、会话守卫和根组件。</summary>
[ECMAScriptModule("components/app-bootstrap.js")]
public static class AppBootstrap
{
    /// <summary>模块初始化时挂载到 <c>#app</c> 的 Vue 实例。</summary>
    public static readonly VueComponentPublicInstance Application = Start();

    private static VueComponentPublicInstance Start()
    {
        var baseElement = Document.QuerySelector("base") as HTMLBaseElement;
        var deploymentBase = new URL(baseElement?.Href ?? "/", Document.BaseURI);
        var router = CreateRouter(new RouterOptions
        {
            History = CreateWebHistory(deploymentBase.Pathname),
            Routes = Routes()
        });

        // 同源且位于部署前缀内的链接可交给 Vue Router，传入前先剥离部署前缀。
        JeroNavigationModule.Bind((uri, forceLoad) =>
        {
            var target = new URL(uri, deploymentBase.Href);
            if (forceLoad || target.Origin != deploymentBase.Origin || !target.Pathname.StartsWith(deploymentBase.Pathname))
            {
                Document.Location!.Assign(target.Href);
                return;
            }

            router.Push("/" + target.Pathname[deploymentBase.Pathname.Length..] + target.Search + target.Hash);
        }, () => Document.DocumentURI, () => deploymentBase.Href);

        Document.DocumentElement!.ClassList.Toggle("dark", Window.LocalStorage.GetItem("jero-theme") == "dark");
        router.BeforeEach((AsyncRouteNavigationGuard)((to, from) => Guard(to)));
        router.AfterEach((to, from, failure) => BrowserSessionStore.SyncEmbeddedSession());
        return CreateApp(RouterView).Use(router).Mount("#app");
    }

    private static IPromise<NavigationGuardReturn?> Guard(RouteLocationNormalized to)
    {
        if (to.Path == "/")
        {
            NavigationGuardReturn destination = BrowserSessionStore.HasSession ? "/home" : "/login";
            return Promise<NavigationGuardReturn?>.Resolve(destination);
        }

        if ((object?)to.Meta["public"] is true)
            return Promise<NavigationGuardReturn?>.Resolve(true);
        if (!BrowserSessionStore.HasSession)
            return Promise<NavigationGuardReturn?>.Resolve((NavigationGuardReturn)"/login");

        return RouteModules.GuardProtectedRoute(to);
    }

    private static RouteRecordRaw[] Routes()
    {
        RouteRecordRaw[] children =
        [
                Page("about", RouteModules.LoadAbout),
                Page("analytics", RouteModules.LoadAnalytics),
                Page("approval-flow", RouteModules.LoadApprovalFlow),
                Page("cache", RouteModules.LoadCache),
                Page("database", RouteModules.LoadDatabase),
                Page("database/visual", RouteModules.LoadDatabaseVisual),
                Page("dict-type", RouteModules.LoadDictType),
                Page("diff-log", RouteModules.LoadDiffLog),
                Page("403", RouteModules.LoadError, new[] { "/404", "/500", "/offline", "/coming-soon" }),
                Page("ex-log", RouteModules.LoadExLog),
                Page("file", RouteModules.LoadFile),
                Page("form-design", RouteModules.LoadFormDesign),
                Page("home", RouteModules.LoadHome),
                Page("iframe/:MenuId", RouteModules.LoadIframe),
                Page("info-setting", RouteModules.LoadInfoSetting),
                Page("job/dashboard", RouteModules.LoadJobDashboard, "/platform/job/dashboard"),
                Page("job", RouteModules.LoadJob),
                Page("lang", RouteModules.LoadLang),
                Page("lang-text", RouteModules.LoadLangText),
                Page("ldap", RouteModules.LoadLdap),
                Page("menu", RouteModules.LoadMenu),
                Page("notice", RouteModules.LoadNotice),
                Page("online-user", RouteModules.LoadOnlineUser),
                Page("op-log", RouteModules.LoadOpLog),
                Page("open-access", RouteModules.LoadOpenAccess),
                Page("org", RouteModules.LoadOrg),
                Page("platform-config", RouteModules.LoadPlatformConfig),
                Page("plugin", RouteModules.LoadPlugin),
                Page("pos", RouteModules.LoadPos),
                Page("print", RouteModules.LoadPrint),
                Page("profile", RouteModules.LoadProfile),
                Page("received-notice", RouteModules.LoadReceivedNotice),
                Page("region", RouteModules.LoadRegion),
                Page("role", RouteModules.LoadRole),
                Page("server", RouteModules.LoadServer),
                Page("stress-test", RouteModules.LoadStressTest),
                Page("update", RouteModules.LoadSystemUpdate),
                Page("template", RouteModules.LoadTemplate),
                Page("tenant", RouteModules.LoadTenant),
                Page("tenant-config", RouteModules.LoadTenantConfig),
                Page("user", RouteModules.LoadUser),
                Page("user-reg-way", RouteModules.LoadUserRegWay),
                Page("vis-log", RouteModules.LoadVisLog),
                Page("wechat-pay", RouteModules.LoadWechatPay),
                Page("wechat-user", RouteModules.LoadWechatUser),
                Page("workspace", RouteModules.LoadWorkspace),
        ];

        return
        [
            new RouteRecordRedirect { Path = "/", Redirect = "/home" },
                Page("/code-login", RouteModules.LoadCodeLogin, "/auth/code-login", isPublic: true),
                Page("/forget-password", RouteModules.LoadForgetPassword, "/auth/forget-password", isPublic: true),
                Page("/login", RouteModules.LoadLogin, "/auth/login", isPublic: true),
                Page("/register", RouteModules.LoadRegister, "/auth/register", isPublic: true),
                Page("/qrcode-login", RouteModules.LoadQrCodeLogin, "/auth/qrcode-login", isPublic: true),
            new RouteRecordSingleViewWithChildren
            {
                Path = "/",
                Component = RawRouteComponent.From(RouteModules.LoadShell),
                Children = children
            },
            new RouteRecordRedirect { Path = "/:pathMatch(.*)*", Redirect = "/404" }
        ];
    }

    private static RouteRecordSingleView Page(string path, RouteComponentLoader load)
        => new() { Path = path, Component = RawRouteComponent.From(load) };

    // Vue Router 按属性是否存在判断要不要遍历别名，未配置时应省略该属性。
    private static RouteRecordSingleView Page(string path, RouteComponentLoader load, RouteRecordAlias alias, bool isPublic = false)
    {
        if (isPublic)
            return new()
            {
                Path = path,
                Component = RawRouteComponent.From(load),
                Alias = alias,
                Meta = new RouteMeta { { "public", true } }
            };
        return new() { Path = path, Component = RawRouteComponent.From(load), Alias = alias };
    }
}
