using Jero.Web.App.Auth;
using Jero.Web.App.Shell;
using static ECMAScript.Global;
using static ECMAScript.Vue;
using static ECMAScript.VueRoute;

namespace Jero.Web.App;

[ECMAScriptModule("components/app-bootstrap.js")]
public static class AppBootstrap
{
    public static void Start(RouteModules modules)
    {
        var baseElement = Document.QuerySelector("base") as HTMLBaseElement;
        var deploymentBase = new URL(baseElement?.Href ?? "/", Document.BaseURI);
        var router = CreateRouter(new RouterOptions
        {
            History = CreateWebHistory(deploymentBase.Pathname),
            Routes = Routes(modules)
        });

        // Vue Router accepts paths relative to the host's deployment prefix.
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
        router.BeforeEach((AsyncRouteNavigationGuard)((to, from) => Guard(to, modules)));
        router.AfterEach((to, from, failure) => BrowserSessionStore.SyncEmbeddedSession());
        CreateApp(RouterView).Use(router).Mount("#app");
    }

    private static IPromise<NavigationGuardReturn?> Guard(RouteLocationNormalized to, RouteModules modules)
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

        return modules.GuardProtectedRoute(to);
    }

    private static RouteRecordRaw[] Routes(RouteModules modules)
    {
        RouteRecordRaw[] children =
        [
                Page("about", modules.LoadAbout),
                Page("analytics", modules.LoadAnalytics),
                Page("approval-flow", modules.LoadApprovalFlow),
                Page("cache", modules.LoadCache),
                Page("database", modules.LoadDatabase),
                Page("database/visual", modules.LoadDatabaseVisual),
                Page("dict-type", modules.LoadDictType),
                Page("diff-log", modules.LoadDiffLog),
                Page("403", modules.LoadError, new[] { "/404", "/500", "/offline", "/coming-soon" }),
                Page("ex-log", modules.LoadExLog),
                Page("file", modules.LoadFile),
                Page("form-design", modules.LoadFormDesign),
                Page("home", modules.LoadHome),
                Page("iframe/:MenuId", modules.LoadIframe),
                Page("info-setting", modules.LoadInfoSetting),
                Page("job/dashboard", modules.LoadJobDashboard, "/platform/job/dashboard"),
                Page("job", modules.LoadJob),
                Page("lang", modules.LoadLang),
                Page("lang-text", modules.LoadLangText),
                Page("ldap", modules.LoadLdap),
                Page("menu", modules.LoadMenu),
                Page("notice", modules.LoadNotice),
                Page("online-user", modules.LoadOnlineUser),
                Page("op-log", modules.LoadOpLog),
                Page("open-access", modules.LoadOpenAccess),
                Page("org", modules.LoadOrg),
                Page("platform-config", modules.LoadPlatformConfig),
                Page("plugin", modules.LoadPlugin),
                Page("pos", modules.LoadPos),
                Page("print", modules.LoadPrint),
                Page("profile", modules.LoadProfile),
                Page("received-notice", modules.LoadReceivedNotice),
                Page("region", modules.LoadRegion),
                Page("role", modules.LoadRole),
                Page("server", modules.LoadServer),
                Page("stress-test", modules.LoadStressTest),
                Page("update", modules.LoadSystemUpdate),
                Page("template", modules.LoadTemplate),
                Page("tenant", modules.LoadTenant),
                Page("tenant-config", modules.LoadTenantConfig),
                Page("user", modules.LoadUser),
                Page("user-reg-way", modules.LoadUserRegWay),
                Page("vis-log", modules.LoadVisLog),
                Page("wechat-pay", modules.LoadWechatPay),
                Page("wechat-user", modules.LoadWechatUser),
                Page("workspace", modules.LoadWorkspace),
        ];
        foreach (var generated in modules.GeneratedPages())
        {
            var path = generated.file[2..^6];
            children = [..children, Page(path, generated.load)];
        }

        return
        [
            new RouteRecordRedirect { Path = "/", Redirect = "/home" },
                Page("/code-login", modules.LoadCodeLogin, "/auth/code-login", isPublic: true),
                Page("/forget-password", modules.LoadForgetPassword, "/auth/forget-password", isPublic: true),
                Page("/login", modules.LoadLogin, "/auth/login", isPublic: true),
                Page("/register", modules.LoadRegister, "/auth/register", isPublic: true),
                Page("/qrcode-login", modules.LoadQrCodeLogin, "/auth/qrcode-login", isPublic: true),
            new RouteRecordSingleViewWithChildren
            {
                Path = "/",
                Component = RawRouteComponent.From(modules.LoadShell),
                Children = children
            },
            new RouteRecordRedirect { Path = "/:pathMatch(.*)*", Redirect = "/404" }
        ];
    }

    private static RouteRecordSingleView Page(string path, RouteComponentLoader load)
        => new() { Path = path, Component = RawRouteComponent.From(load) };

    // Vue Router tests whether alias is present before iterating it. Omit absent optionals.
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
