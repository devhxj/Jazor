using ECMAScript;
using Jazor.Admin;

namespace JazorAdmin;

/// <summary>
/// The checked-in TDesign Starter page inventory. Keep this catalog one-to-one with the
/// upstream `src/pages/*/index.vue` entries so a route cannot silently become a placeholder.
/// </summary>
[ECMAScriptModule("components/starter-catalog.mjs")]
internal static class StarterCatalog
{
    public const string RootKey = "starter";

    public static AdminRouteDefinition[] CreateRoutes(AdminLanguage language) =>
    [
        new()
        {
            Key = RootKey,
            Icon = "application",
            Title = T(language, "TDesign Starter", "TDesign Starter"),
            Children =
            [
                Group(language, "starter.dashboard", "Dashboard", "仪表盘", "dashboard",
                [
                    Page(language, "starter.dashboard.base", "/starter/dashboard/base", "Overview", "概览", "dashboard-base"),
                    Page(language, "starter.dashboard.detail", "/starter/dashboard/detail", "Dashboard Detail", "统计报表", "dashboard-detail")
                ]),
                Group(language, "starter.list", "List", "列表页", "view-list",
                [
                    Page(language, "starter.list.base", "/starter/list/base", "Base List", "基础列表", "list-base"),
                    Page(language, "starter.list.card", "/starter/list/card", "Card List", "卡片列表", "list-card"),
                    Page(language, "starter.list.filter", "/starter/list/filter", "Filter List", "筛选列表", "list-filter"),
                    Page(language, "starter.list.tree", "/starter/list/tree", "Tree List", "树形列表", "list-tree")
                ]),
                Group(language, "starter.form", "Form", "表单页", "edit-1",
                [
                    Page(language, "starter.form.base", "/starter/form/base", "Base Form", "基础表单", "form-base"),
                    Page(language, "starter.form.step", "/starter/form/step", "Step Form", "分步表单", "form-step")
                ]),
                Group(language, "starter.detail", "Detail", "详情页", "file-copy",
                [
                    Page(language, "starter.detail.base", "/starter/detail/base", "Base Detail", "基础详情", "detail-base"),
                    Page(language, "starter.detail.advanced", "/starter/detail/advanced", "Advanced Detail", "高级详情", "detail-advanced"),
                    Page(language, "starter.detail.deploy", "/starter/detail/deploy", "Deploy Detail", "部署详情", "detail-deploy"),
                    Page(language, "starter.detail.secondary", "/starter/detail/secondary", "Secondary Detail", "二级详情", "detail-secondary")
                ]),
                Group(language, "starter.result", "Result", "结果页", "check-circle",
                [
                    Page(language, "starter.result.success", "/starter/result/success", "Success", "成功页", "result-success"),
                    Page(language, "starter.result.fail", "/starter/result/fail", "Fail", "失败页", "result-fail"),
                    Page(language, "starter.result.network", "/starter/result/network-error", "Network Error", "网络异常", "result-network"),
                    Page(language, "starter.result.403", "/starter/result/403", "Forbidden", "无权限", "result-403"),
                    Page(language, "starter.result.404", "/starter/result/404", "Not Found", "访问页面不存在", "result-404"),
                    Page(language, "starter.result.500", "/starter/result/500", "Server Error", "服务器出错", "result-500"),
                    Page(language, "starter.result.browser", "/starter/result/browser-incompatible", "Browser Incompatible", "浏览器不兼容", "result-browser"),
                    Page(language, "starter.result.maintenance", "/starter/result/maintenance", "Maintenance", "系统维护", "result-maintenance")
                ]),
                Group(language, "starter.account", "Account", "个人中心", "user-circle",
                [
                    Page(language, "starter.user", "/starter/user", "User Center", "个人中心", "user"),
                    Page(language, "starter.login", "/starter/login", "Login", "登录页", "login")
                ])
            ]
        }
    ];

    public static bool IsStarter(string key)
        => key.StartsWith(RootKey + ".", StringComparison.Ordinal);

    public static string GetTemplate(string key) => key switch
    {
        "starter.dashboard.base" => "dashboard-base",
        "starter.dashboard.detail" => "dashboard-detail",
        "starter.list.base" => "list-base",
        "starter.list.card" => "list-card",
        "starter.list.filter" => "list-filter",
        "starter.list.tree" => "list-tree",
        "starter.form.base" => "form-base",
        "starter.form.step" => "form-step",
        "starter.detail.base" => "detail-base",
        "starter.detail.advanced" => "detail-advanced",
        "starter.detail.deploy" => "detail-deploy",
        "starter.detail.secondary" => "detail-secondary",
        "starter.result.success" => "result-success",
        "starter.result.fail" => "result-fail",
        "starter.result.network" => "result-network",
        "starter.result.403" => "result-403",
        "starter.result.404" => "result-404",
        "starter.result.500" => "result-500",
        "starter.result.browser" => "result-browser",
        "starter.result.maintenance" => "result-maintenance",
        "starter.user" => "user",
        "starter.login" => "login",
        _ => string.Empty
    };

    private static AdminRouteDefinition Group(
        AdminLanguage language,
        string key,
        string english,
        string chinese,
        string icon,
        AdminRouteDefinition[] children) => new()
        {
            Key = key,
            Icon = icon,
            Title = T(language, english, chinese),
            Children = children
        };

    private static AdminRouteDefinition Page(
        AdminLanguage language,
        string key,
        string path,
        string english,
        string chinese,
        string template) => new()
        {
            Key = key,
            Path = path,
            Title = T(language, english, chinese),
            Subtitle = template
        };

    private static string T(AdminLanguage language, string english, string chinese)
        => language == AdminLanguage.Chinese ? chinese : english;
}
