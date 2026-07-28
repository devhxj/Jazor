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
    Operations,
    Releases,
    ReleasesSubtitle,
    AuditLog,
    AuditLogSubtitle,
    Settings,
    SettingsSubtitle,
    Workspace,
    WorkspaceSubtitle,
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
            TextKey.DashboardSubtitle => "Current render-function integration slice",
            TextKey.Operations => "Operations",
            TextKey.Releases => "Releases",
            TextKey.ReleasesSubtitle => "Release readiness, ownership, and rollout queue",
            TextKey.AuditLog => "Audit Log",
            TextKey.AuditLogSubtitle => "Operational changes emitted by the admin workflow",
            TextKey.Settings => "Settings",
            TextKey.SettingsSubtitle => "Tenant-level preferences for the admin shell",
            TextKey.Workspace => "Workspace",
            TextKey.WorkspaceSubtitle => "Personal workspace state for the signed-in operator",
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
            TextKey.DashboardSubtitle => "当前渲染函数集成状态",
            TextKey.Operations => "运维管理",
            TextKey.Releases => "发布队列",
            TextKey.ReleasesSubtitle => "发布就绪度、负责人和上线队列",
            TextKey.AuditLog => "审计日志",
            TextKey.AuditLogSubtitle => "管理工作流产生的操作变更",
            TextKey.Settings => "设置",
            TextKey.SettingsSubtitle => "管理后台的租户级偏好",
            TextKey.Workspace => "工作台",
            TextKey.WorkspaceSubtitle => "当前操作员的个人工作状态",
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
