using System.ComponentModel;
using static ECMAScript.Vue;
using static ECMAScript.VueRoute;

namespace Jero.Web.App;

[ECMAScript]
public sealed class RouteModules
{
    [Description("@#loadCodeLogin")]
    public extern IPromise<IVueComponent> LoadCodeLogin();

    [Description("@#loadForgetPassword")]
    public extern IPromise<IVueComponent> LoadForgetPassword();

    [Description("@#loadLogin")]
    public extern IPromise<IVueComponent> LoadLogin();

    [Description("@#loadRegister")]
    public extern IPromise<IVueComponent> LoadRegister();

    [Description("@#loadQrCodeLogin")]
    public extern IPromise<IVueComponent> LoadQrCodeLogin();

    [Description("@#loadShell")]
    public extern IPromise<IVueComponent> LoadShell();

    [Description("@#guardProtectedRoute")]
    public extern IPromise<NavigationGuardReturn?> GuardProtectedRoute(RouteLocationNormalized to);

    [Description("@#loadAbout")]
    public extern IPromise<IVueComponent> LoadAbout();

    [Description("@#loadAnalytics")]
    public extern IPromise<IVueComponent> LoadAnalytics();

    [Description("@#loadApprovalFlow")]
    public extern IPromise<IVueComponent> LoadApprovalFlow();

    [Description("@#loadCache")]
    public extern IPromise<IVueComponent> LoadCache();

    [Description("@#loadDatabase")]
    public extern IPromise<IVueComponent> LoadDatabase();

    [Description("@#loadDatabaseVisual")]
    public extern IPromise<IVueComponent> LoadDatabaseVisual();

    [Description("@#loadDictType")]
    public extern IPromise<IVueComponent> LoadDictType();

    [Description("@#loadDiffLog")]
    public extern IPromise<IVueComponent> LoadDiffLog();

    [Description("@#loadError")]
    public extern IPromise<IVueComponent> LoadError();

    [Description("@#loadExLog")]
    public extern IPromise<IVueComponent> LoadExLog();

    [Description("@#loadFile")]
    public extern IPromise<IVueComponent> LoadFile();

    [Description("@#loadFormDesign")]
    public extern IPromise<IVueComponent> LoadFormDesign();

    [Description("@#loadHome")]
    public extern IPromise<IVueComponent> LoadHome();

    [Description("@#loadIframe")]
    public extern IPromise<IVueComponent> LoadIframe();

    [Description("@#loadInfoSetting")]
    public extern IPromise<IVueComponent> LoadInfoSetting();

    [Description("@#loadJobDashboard")]
    public extern IPromise<IVueComponent> LoadJobDashboard();

    [Description("@#loadJob")]
    public extern IPromise<IVueComponent> LoadJob();

    [Description("@#loadLang")]
    public extern IPromise<IVueComponent> LoadLang();

    [Description("@#loadLangText")]
    public extern IPromise<IVueComponent> LoadLangText();

    [Description("@#loadLdap")]
    public extern IPromise<IVueComponent> LoadLdap();

    [Description("@#loadMenu")]
    public extern IPromise<IVueComponent> LoadMenu();

    [Description("@#loadNotice")]
    public extern IPromise<IVueComponent> LoadNotice();

    [Description("@#loadOnlineUser")]
    public extern IPromise<IVueComponent> LoadOnlineUser();

    [Description("@#loadOpLog")]
    public extern IPromise<IVueComponent> LoadOpLog();

    [Description("@#loadOpenAccess")]
    public extern IPromise<IVueComponent> LoadOpenAccess();

    [Description("@#loadOrg")]
    public extern IPromise<IVueComponent> LoadOrg();

    [Description("@#loadPlatformConfig")]
    public extern IPromise<IVueComponent> LoadPlatformConfig();

    [Description("@#loadPlugin")]
    public extern IPromise<IVueComponent> LoadPlugin();

    [Description("@#loadPos")]
    public extern IPromise<IVueComponent> LoadPos();

    [Description("@#loadPrint")]
    public extern IPromise<IVueComponent> LoadPrint();

    [Description("@#loadProfile")]
    public extern IPromise<IVueComponent> LoadProfile();

    [Description("@#loadReceivedNotice")]
    public extern IPromise<IVueComponent> LoadReceivedNotice();

    [Description("@#loadRegion")]
    public extern IPromise<IVueComponent> LoadRegion();

    [Description("@#loadRole")]
    public extern IPromise<IVueComponent> LoadRole();

    [Description("@#loadServer")]
    public extern IPromise<IVueComponent> LoadServer();

    [Description("@#loadStressTest")]
    public extern IPromise<IVueComponent> LoadStressTest();

    [Description("@#loadSystemUpdate")]
    public extern IPromise<IVueComponent> LoadSystemUpdate();

    [Description("@#loadTemplate")]
    public extern IPromise<IVueComponent> LoadTemplate();

    [Description("@#loadTenant")]
    public extern IPromise<IVueComponent> LoadTenant();

    [Description("@#loadTenantConfig")]
    public extern IPromise<IVueComponent> LoadTenantConfig();

    [Description("@#loadUser")]
    public extern IPromise<IVueComponent> LoadUser();

    [Description("@#loadUserRegWay")]
    public extern IPromise<IVueComponent> LoadUserRegWay();

    [Description("@#loadVisLog")]
    public extern IPromise<IVueComponent> LoadVisLog();

    [Description("@#loadWechatPay")]
    public extern IPromise<IVueComponent> LoadWechatPay();

    [Description("@#loadWechatUser")]
    public extern IPromise<IVueComponent> LoadWechatUser();

    [Description("@#loadWorkspace")]
    public extern IPromise<IVueComponent> LoadWorkspace();

    [Description("@#generatedPages")]
    public extern GeneratedPage[] GeneratedPages();
}

public sealed record GeneratedPage(string file, RouteComponentLoader load);
