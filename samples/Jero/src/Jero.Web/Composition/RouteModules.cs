using System.ComponentModel;
using static ECMAScript.Global;
using static ECMAScript.Vue;
using static ECMAScript.VueRoute;

namespace Jero.Web.Composition;

/// <summary>按路由加载页面、状态工厂以及应用外壳模块。</summary>
[ECMAScriptModule("components/route-modules.js")]
public static class RouteModules
{
    // 每个动态导入在调用点保留字面量路径，由 Vite 识别并拆分产物。
    private static IPromise<IVueComponent> LoadStatefulPage<TProps>(
        IPromise<ViewModule> view,
        IPromise<PageStateModule<TProps>> state,
        IPromise<ViewModule>? content = null) where TProps : VueProps
    {
        IPromise[] pending = content is null ? [view, state] : [view, state, content];

        // 先用 Promise.All 订阅所有导入结果，再从原 Promise 读取各自的强类型导出。
        return Promise.All(pending).Then<IVueComponent>(() =>
            view.Then<IVueComponent>(component =>
                state.Then<IVueComponent>(factory => content is null
                    ? Promise<IVueComponent>.Resolve(PageRoute.CreatePageRoute(component.Default, factory.Create))
                    : content.Then(slot => PageRoute.CreatePageRoute(component.Default, factory.Create, slot.Default)))));
    }

    public static IPromise<IVueComponent> LoadAnalytics() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Analytics/AnalyticsPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Analytics.AnalyticsPageProps>>("./analytics-page-state.js"));

    public static IPromise<IVueComponent> LoadApprovalFlow() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/ApprovalFlow/ApprovalFlowPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.ApprovalFlow.ApprovalFlowPageProps>>("./approval-flow-page-state.js"));

    public static IPromise<IVueComponent> LoadCache() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Cache/CachePage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Cache.CachePageProps>>("./cache-page-state.js"));

    public static IPromise<IVueComponent> LoadDatabase() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Database/DatabasePage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Database.DatabasePageProps>>("./database-page-state.js"));

    public static IPromise<IVueComponent> LoadDatabaseVisual() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Database/DatabaseVisualPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Database.DatabaseVisualPageProps>>("./database-visual-page-state.js"));

    public static IPromise<IVueComponent> LoadDictType() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Dict/DictTypePage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Dict.DictTypePageProps>>("./dict-type-page-state.js"));

    public static IPromise<IVueComponent> LoadDiffLog() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/DiffLog/DiffLogPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.DiffLog.DiffLogPageProps>>("./diff-log-page-state.js"));

    public static IPromise<IVueComponent> LoadExLog() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Logs/LogPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.ExLog.ExLogPageProps>>("./ex-log-page-state.js"));

    public static IPromise<IVueComponent> LoadHome() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Home/HomePage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Home.HomePageProps>>("./home-page-state.js"));

    public static IPromise<IVueComponent> LoadInfoSetting() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/InfoSetting/InfoSettingPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.InfoSetting.InfoSettingPageProps>>("./info-setting-page-state.js"));

    public static IPromise<IVueComponent> LoadJobDashboard() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Job/JobDashboardPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Job.JobDashboardPageProps>>("./job-dashboard-page-state.js"));

    public static IPromise<IVueComponent> LoadJob() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Job/JobPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Job.JobPageProps>>("./job-page-state.js"),
            Import<ViewModule>("/Pages/Job/JobPageContent.jazor"));

    public static IPromise<IVueComponent> LoadLang() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Lang/LangPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Lang.LangPageProps>>("./lang-page-state.js"));

    public static IPromise<IVueComponent> LoadLangText() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/LangText/LangTextPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.LangText.LangTextPageProps>>("./lang-text-page-state.js"));

    public static IPromise<IVueComponent> LoadLdap() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Ldap/LdapPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Ldap.LdapPageProps>>("./ldap-page-state.js"));

    public static IPromise<IVueComponent> LoadMenu() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Menu/MenuPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Menu.MenuPageProps>>("./menu-page-state.js"));

    public static IPromise<IVueComponent> LoadNotice() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Notice/NoticePage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Notice.NoticePageProps>>("./notice-page-state.js"));

    public static IPromise<IVueComponent> LoadOnlineUser() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/OnlineUser/OnlineUserPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.OnlineUser.OnlineUserPageProps>>("./online-user-page-state.js"));

    public static IPromise<IVueComponent> LoadOpLog() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Logs/LogPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.OpLog.OpLogPageProps>>("./op-log-page-state.js"));

    public static IPromise<IVueComponent> LoadOpenAccess() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/OpenAccess/OpenAccessPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.OpenAccess.OpenAccessPageProps>>("./open-access-page-state.js"));

    public static IPromise<IVueComponent> LoadOrg() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Org/OrgPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Org.OrgPageProps>>("./org-page-state.js"));

    public static IPromise<IVueComponent> LoadPlatformConfig() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/PlatformConfig/PlatformConfigPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.PlatformConfig.PlatformConfigPageProps>>("./platform-config-page-state.js"));

    public static IPromise<IVueComponent> LoadPlugin() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Plugin/PluginPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Plugin.PluginPageProps>>("./plugin-page-state.js"));

    public static IPromise<IVueComponent> LoadPos() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Pos/PosPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Pos.PosPageProps>>("./pos-page-state.js"));

    public static IPromise<IVueComponent> LoadPrint() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Print/PrintPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Print.PrintPageProps>>("./print-page-state.js"));

    public static IPromise<IVueComponent> LoadProfile() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Profile/ProfilePage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Profile.ProfilePageProps>>("./profile-page-state.js"));

    public static IPromise<IVueComponent> LoadReceivedNotice() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/ReceivedNotice/ReceivedNoticePage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.ReceivedNotice.ReceivedNoticePageProps>>("./received-notice-page-state.js"));

    public static IPromise<IVueComponent> LoadRegion() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Region/RegionPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Region.RegionPageProps>>("./region-page-state.js"));

    public static IPromise<IVueComponent> LoadRole() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Role/RolePage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Role.RolePageProps>>("./role-page-state.js"));

    public static IPromise<IVueComponent> LoadServer() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Server/ServerPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Server.ServerPageProps>>("./server-page-state.js"));

    public static IPromise<IVueComponent> LoadStressTest() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/StressTest/StressTestPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.StressTest.StressTestPageProps>>("./stress-test-page-state.js"));

    public static IPromise<IVueComponent> LoadSystemUpdate() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/SystemUpdate/SystemUpdatePage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.SystemUpdate.SystemUpdatePageProps>>("./system-update-page-state.js"));

    public static IPromise<IVueComponent> LoadTenant() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Tenant/TenantPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Tenant.TenantPageProps>>("./tenant-page-state.js"));

    public static IPromise<IVueComponent> LoadTenantConfig() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/TenantConfig/TenantConfigPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.TenantConfig.TenantConfigPageProps>>("./tenant-config-page-state.js"));

    public static IPromise<IVueComponent> LoadUser() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/User/UserPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.User.UserPageProps>>("./user-page-state.js"));

    public static IPromise<IVueComponent> LoadUserRegWay() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/UserRegWay/UserRegWayPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.UserRegWay.UserRegWayPageProps>>("./user-reg-way-page-state.js"));

    public static IPromise<IVueComponent> LoadVisLog() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Logs/LogPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.VisLog.VisLogPageProps>>("./vis-log-page-state.js"));

    public static IPromise<IVueComponent> LoadWechatPay() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/WechatPay/WechatPayPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.WechatPay.WechatPayPageProps>>("./wechat-pay-page-state.js"),
            Import<ViewModule>("/Pages/WechatPay/WechatPayPageContent.jazor"));

    public static IPromise<IVueComponent> LoadWechatUser() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/WechatUser/WechatUserPage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.WechatUser.WechatUserPageProps>>("./wechat-user-page-state.js"),
            Import<ViewModule>("/Pages/WechatUser/WechatUserPageContent.jazor"));

    public static IPromise<IVueComponent> LoadWorkspace() =>
        LoadStatefulPage(
            Import<ViewModule>("/Pages/Workspace/WorkspacePage.jazor"),
            Import<PageStateModule<Jero.Web.Pages.Workspace.WorkspacePageProps>>("./workspace-page-state.js"));

    public static IPromise<IVueComponent> LoadAbout() =>
        Import<ViewModule>("/Pages/About/AboutPage.jazor").Then(module => module.Default);

    public static IPromise<IVueComponent> LoadCodeLogin() =>
        Import<ViewModule>("/Pages/Auth/CodeLoginPage.jazor").Then(module => module.Default);

    public static IPromise<IVueComponent> LoadError() =>
        Import<ViewModule>("/Pages/Errors/ErrorPage.jazor").Then(module => module.Default);

    public static IPromise<IVueComponent> LoadFile() =>
        Import<ViewModule>("/Pages/File/FilePageRoute.jazor").Then(module => module.Default);

    public static IPromise<IVueComponent> LoadForgetPassword() =>
        Import<ViewModule>("/Pages/Auth/ForgetPasswordPage.jazor").Then(module => module.Default);

    public static IPromise<IVueComponent> LoadFormDesign() =>
        Import<ViewModule>("/Pages/FormDesign/FormDesignPage.jazor").Then(module => module.Default);

    public static IPromise<IVueComponent> LoadIframe() =>
        Import<ViewModule>("/Pages/Iframe/IframePage.jazor").Then(module => module.Default);

    public static IPromise<IVueComponent> LoadLogin() =>
        Import<ViewModule>("/Pages/Auth/LoginPage.jazor").Then(module => module.Default);

    public static IPromise<IVueComponent> LoadQrCodeLogin() =>
        Import<ViewModule>("/Pages/Auth/QrCodeLoginPage.jazor").Then(module => module.Default);

    public static IPromise<IVueComponent> LoadRegister() =>
        Import<ViewModule>("/Pages/Auth/RegisterPage.jazor").Then(module => module.Default);

    public static IPromise<IVueComponent> LoadTemplate() =>
        Import<ViewModule>("/Pages/Template/TemplatePageRoute.jazor").Then(module => module.Default);

    public static IPromise<IVueComponent> LoadShell() =>
        Import<ShellModule>("./shell-component.js").Then(module => module.Create());

    /// <summary>按需加载外壳状态，由它决定受保护路由能否继续导航。</summary>
    public static IPromise<NavigationGuardReturn?> GuardProtectedRoute(RouteLocationNormalized to) =>
        Import<ShellStateModule>("./shell-state.js").Then(module => module.GuardNavigation(to));

    /// <summary>交由外壳状态完成登录后的会话处理和导航。</summary>
    public static IPromise<RouteNavigationResult?> CompleteSignIn(Router router, string? accessToken, string? refreshToken, string? home) =>
        Import<ShellStateModule>("./shell-state.js").Then(module => module.CompleteSignIn(router, accessToken, refreshToken, home));
}

[ECMAScript]
public sealed record ViewModule
{
    [Description("@#default")]
    public required IVueComponent Default { get; init; }
}

[ECMAScript]
public sealed record PageStateModule<TProps> where TProps : VueProps
{
    public required Func<IPageState<TProps>> Create { get; init; }
}

[ECMAScript]
public sealed record ShellModule
{
    public required Func<IVueComponent> Create { get; init; }
}

[ECMAScript]
public sealed record ShellStateModule
{
    public required Func<RouteLocationNormalized, IPromise<NavigationGuardReturn?>> GuardNavigation { get; init; }
    public required Func<Router, string?, string?, string?, IPromise<RouteNavigationResult?>> CompleteSignIn { get; init; }
}
