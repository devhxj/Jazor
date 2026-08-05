// Provides localized navigation and access-screen text for the RazorVue administration shell.
// 为 RazorVue 管理壳层提供导航与访问页面的本地化文本。
namespace JazorAdmin;

[String]
public enum AdminLanguage
{
    [Description("@#en-US")]
    English,

    [Description("@#zh-CN")]
    Chinese
}

public enum TextKey
{
    ShellSubtitle,
    Dashboard,
    DashboardSubtitle,
    Organizations,
    OrganizationStructure,
    OrganizationStructureSubtitle,
    Members,
    MembersSubtitle,
    Authorization,
    RolesAndGrants,
    RolesAndGrantsSubtitle,
    ResourceOperations,
    ResourceOperationsSubtitle,
    Accounts,
    AccountsSubtitle,
    Configuration,
    OpenIdClients,
    OpenIdClientsSubtitle,
    OpenIdScopes,
    OpenIdScopesSubtitle,
    Theme,
    Language,
    Grayscale,
    CollapseSidebar,
    ExpandSidebar,
    Lock,
    SignOut,
    LoginTitle,
    LoginSubtitle,
    Account,
    Password,
    SignIn,
    LoginRequired,
    LockTitle,
    LockSubtitle,
    Unlock,
    UnlockRequired,
    NotFoundTitle,
    NotFoundDescription,
    InternalErrorTitle,
    InternalErrorDescription,
    ReturnHome
}

[ECMAScriptModule("components/jazor-admin-localization.mjs")]
public static class Localization
{
    public static string Get(AdminLanguage language, TextKey key)
        => language == AdminLanguage.Chinese
            ? GetChinese(key)
            : GetEnglish(key);

    public static string GetLanguageTag(AdminLanguage language)
        => language == AdminLanguage.Chinese ? "zh-CN" : "en-US";

    private static string GetEnglish(TextKey key)
        => key switch
        {
            TextKey.ShellSubtitle => "RazorVue admin foundation",
            TextKey.Dashboard => "Dashboard",
            TextKey.DashboardSubtitle => "Session and administration access overview",
            TextKey.Organizations => "Organizations",
            TextKey.OrganizationStructure => "Organization structure",
            TextKey.OrganizationStructureSubtitle => "Manage the active organization and its child units",
            TextKey.Members => "Members",
            TextKey.MembersSubtitle => "Manage organization memberships and assigned roles",
            TextKey.Authorization => "Authorization",
            TextKey.RolesAndGrants => "Roles and grants",
            TextKey.RolesAndGrantsSubtitle => "Assign resource operations to organization roles",
            TextKey.ResourceOperations => "Resource operations",
            TextKey.ResourceOperationsSubtitle => "Review resource operations available for authorization",
            TextKey.Accounts => "Accounts",
            TextKey.AccountsSubtitle => "Manage platform accounts and their access state",
            TextKey.Configuration => "Configuration",
            TextKey.OpenIdClients => "OpenID clients",
            TextKey.OpenIdClientsSubtitle => "Register relying applications and their redirect URIs",
            TextKey.OpenIdScopes => "OpenID scopes",
            TextKey.OpenIdScopesSubtitle => "Manage API scopes issued to registered applications",
            TextKey.Theme => "Theme",
            TextKey.Language => "Language",
            TextKey.Grayscale => "Grayscale",
            TextKey.CollapseSidebar => "Collapse sidebar",
            TextKey.ExpandSidebar => "Expand sidebar",
            TextKey.Lock => "Lock",
            TextKey.SignOut => "Sign out",
            TextKey.LoginTitle => "Sign in to JazorAdmin",
            TextKey.LoginSubtitle => "Continue to the administration workspace",
            TextKey.Account => "Account",
            TextKey.Password => "Password",
            TextKey.SignIn => "Sign in",
            TextKey.LoginRequired => "Enter both account and password.",
            TextKey.LockTitle => "Session locked",
            TextKey.LockSubtitle => "Confirm your password to return to the workspace",
            TextKey.Unlock => "Unlock",
            TextKey.NotFoundTitle => "Page not found",
            TextKey.NotFoundDescription => "The requested administration page does not exist or has moved.",
            TextKey.InternalErrorTitle => "Something went wrong",
            TextKey.InternalErrorDescription => "The administration workspace could not complete this request.",
            TextKey.ReturnHome => "Return to dashboard",
            _ => "Enter your password."
        };

    private static string GetChinese(TextKey key)
        => key switch
        {
            TextKey.ShellSubtitle => "RazorVue 管理后台基础框架",
            TextKey.Dashboard => "仪表盘",
            TextKey.DashboardSubtitle => "当前会话与管理权限概览",
            TextKey.Organizations => "组织机构",
            TextKey.OrganizationStructure => "组织架构",
            TextKey.OrganizationStructureSubtitle => "管理当前组织及其下级机构",
            TextKey.Members => "成员管理",
            TextKey.MembersSubtitle => "管理组织成员与已分配的角色",
            TextKey.Authorization => "资源授权",
            TextKey.RolesAndGrants => "角色与授权",
            TextKey.RolesAndGrantsSubtitle => "为组织角色分配资源操作权限",
            TextKey.ResourceOperations => "资源操作",
            TextKey.ResourceOperationsSubtitle => "查看可用于授权的资源操作",
            TextKey.Accounts => "账户管理",
            TextKey.AccountsSubtitle => "管理平台账户及其启用状态",
            TextKey.Configuration => "配置中心",
            TextKey.OpenIdClients => "OpenID 客户端",
            TextKey.OpenIdClientsSubtitle => "登记接入应用及其回调地址",
            TextKey.OpenIdScopes => "OpenID Scope",
            TextKey.OpenIdScopesSubtitle => "管理已登记应用可申请的 API Scope",
            TextKey.Theme => "主题",
            TextKey.Language => "语言",
            TextKey.Grayscale => "灰色祭奠模式",
            TextKey.CollapseSidebar => "收起侧边栏",
            TextKey.ExpandSidebar => "展开侧边栏",
            TextKey.Lock => "锁屏",
            TextKey.SignOut => "退出登录",
            TextKey.LoginTitle => "登录 JazorAdmin",
            TextKey.LoginSubtitle => "进入管理工作台",
            TextKey.Account => "账号",
            TextKey.Password => "密码",
            TextKey.SignIn => "登录",
            TextKey.LoginRequired => "请输入账号和密码。",
            TextKey.LockTitle => "会话已锁定",
            TextKey.LockSubtitle => "确认密码后返回工作台",
            TextKey.Unlock => "解锁",
            TextKey.NotFoundTitle => "页面不存在",
            TextKey.NotFoundDescription => "请求的管理页面不存在或已被移动。",
            TextKey.InternalErrorTitle => "系统暂时无法处理请求",
            TextKey.InternalErrorDescription => "管理工作台未能完成本次操作。",
            TextKey.ReturnHome => "返回仪表盘",
            _ => "请输入密码。"
        };
}
