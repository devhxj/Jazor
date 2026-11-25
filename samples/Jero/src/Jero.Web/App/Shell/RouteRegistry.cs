namespace Jero.Web.App.Shell;

/// <summary>
/// Maps server menu paths and component names to the client routes registered by <see cref="AppBootstrap"/>.
/// </summary>
[ECMAScriptModule("components/app-route-registry.js")]
public static class RouteRegistry
{
    public const string FallbackRoute = "home";
    public const string IframeRoutePrefix = "iframe/";

    public static string Resolve(string? backendPath, string? component)
    {
        var normalizedPath = Normalize(backendPath);
        var normalizedComponent = Normalize(component);

        // The code generator persists the component path independently of the server menu URL.
        if (normalizedComponent.StartsWith("/Generated/") && normalizedComponent.EndsWith("/Index"))
            return normalizedComponent.Substring(1);

        if (normalizedPath == "/dashboard/home" ||
            normalizedPath == "/home" ||
            normalizedComponent == "/home/index")
        {
            return "home";
        }

        if (normalizedPath == "/profile" ||
            normalizedComponent == "/_core/profile/index")
        {
            return "profile";
        }

        if (normalizedPath == "/about" ||
            normalizedComponent == "/about/index")
        {
            return "about";
        }

        if (normalizedPath == "/analytics" ||
            normalizedComponent == "/dashboard/analytics/index")
        {
            return "analytics";
        }

        if (normalizedPath == "/workspace" ||
            normalizedComponent == "/dashboard/workspace/index")
        {
            return "workspace";
        }

        if (normalizedPath == "/system/user" ||
            normalizedPath == "/user" ||
            normalizedComponent == "/system/user/index")
        {
            return "user";
        }

        if (normalizedPath == "/system/role" ||
            normalizedPath == "/role" ||
            normalizedComponent == "/system/role/index")
        {
            return "role";
        }

        if (normalizedPath == "/system/org" ||
            normalizedPath == "/org" ||
            normalizedComponent == "/system/org/index")
        {
            return "org";
        }

        if (normalizedPath == "/platform/tenant" ||
            normalizedPath == "/tenant" ||
            normalizedComponent == "/system/tenant/index")
        {
            return "tenant";
        }

        if (normalizedPath == "/platform/menu" ||
            normalizedPath == "/menu" ||
            normalizedComponent == "/system/menu/index")
        {
            return "menu";
        }

        if (normalizedPath == "/platform/dict" ||
            normalizedPath == "/dict-type" ||
            normalizedComponent == "/system/dict/index")
        {
            return "dict-type";
        }

        if (normalizedPath == "/system/pos" ||
            normalizedPath == "/pos" ||
            normalizedComponent == "/system/pos/index")
        {
            return "pos";
        }

        if (normalizedPath == "/platform/config" ||
            normalizedPath == "/platform-config" ||
            normalizedComponent == "/system/config/index")
        {
            return "platform-config";
        }

        if (normalizedPath == "/system/tenantConfig" ||
            normalizedPath == "/tenant-config" ||
            normalizedComponent == "/system/tenantConfig/index")
        {
            return "tenant-config";
        }

        if (normalizedPath == "/platform/template" ||
            normalizedPath == "/template" ||
            normalizedComponent == "/system/template/index")
        {
            return "template";
        }

        if (normalizedPath == "/platform/regWay" ||
            normalizedPath == "/user-reg-way" ||
            normalizedComponent == "/system/userRegWay/index")
        {
            return "user-reg-way";
        }

        if (normalizedPath == "/system/lang" ||
            normalizedPath == "/lang" ||
            normalizedComponent == "/system/lang/index")
        {
            return "lang";
        }

        if (normalizedPath == "/system/langText" ||
            normalizedPath == "/lang-text" ||
            normalizedComponent == "/system/langText/index")
        {
            return "lang-text";
        }

        if (normalizedPath == "/system/notice" ||
            normalizedPath == "/notice" ||
            normalizedComponent == "/system/notice/index")
        {
            return "notice";
        }

        if (normalizedPath == "/system/onlineUser" ||
            normalizedPath == "/online-user" ||
            normalizedComponent == "/system/onlineUser/index")
        {
            return "online-user";
        }

        if (normalizedPath == "/dashboard/notice" ||
            normalizedPath == "/received-notice" ||
            normalizedComponent == "/dashboard/notice/index")
        {
            return "received-notice";
        }

        if (normalizedPath == "/system/weChatUser" ||
            normalizedPath == "/wechat-user" ||
            normalizedComponent == "/system/weChatUser/index")
        {
            return "wechat-user";
        }

        if (normalizedPath == "/system/ldap" ||
            normalizedPath == "/ldap" ||
            normalizedComponent == "/system/ldap/index")
        {
            return "ldap";
        }

        if (normalizedPath == "/platform/cache" ||
            normalizedPath == "/cache" ||
            normalizedComponent == "/system/cache/index")
        {
            return "cache";
        }

        if (normalizedPath == "/platform/file" ||
            normalizedPath == "/file" ||
            normalizedComponent == "/system/file/index")
        {
            return "file";
        }

        if (normalizedPath == "/platform/region" ||
            normalizedPath == "/region" ||
            normalizedComponent == "/system/region/index")
        {
            return "region";
        }

        if (normalizedPath == "/platform/openAccess" ||
            normalizedPath == "/open-access" ||
            normalizedComponent == "/system/openAccess/index")
        {
            return "open-access";
        }

        if (normalizedPath == "/platform/infoSetting" ||
            normalizedPath == "/info-setting" ||
            normalizedComponent == "/system/infoSetting/index")
        {
            return "info-setting";
        }

        if (normalizedPath == "/platform/job/dashboard" ||
            normalizedPath == "/job/dashboard")
        {
            return "job/dashboard";
        }

        if (normalizedPath == "/platform/job" ||
            normalizedPath == "/job" ||
            normalizedComponent == "/system/job/index")
        {
            return "job";
        }

        if (normalizedPath == "/platform/print" ||
            normalizedPath == "/print" ||
            normalizedComponent == "/system/print/index")
        {
            return "print";
        }

        if (normalizedPath == "/platform/approvalFlow" ||
            normalizedPath == "/approval-flow" ||
            normalizedComponent == "/approvalFlow/index")
        {
            return "approval-flow";
        }

        if (normalizedPath == "/platform/plugin" ||
            normalizedPath == "/plugin" ||
            normalizedComponent == "/system/plugin/index")
        {
            return "plugin";
        }

        if (normalizedPath == "/develop/database/visual" ||
            normalizedPath == "/database/visual")
        {
            return "database/visual";
        }

        if (normalizedPath == "/develop/database" ||
            normalizedPath == "/database" ||
            normalizedComponent == "/system/database/index")
        {
            return "database";
        }

        if (normalizedPath == "/develop/formDes" ||
            normalizedPath == "/form-design" ||
            normalizedComponent == "/system/formDes/index")
        {
            return "form-design";
        }

        if (normalizedPath == "/develop/stressTest" ||
            normalizedPath == "/stress-test" ||
            normalizedComponent == "/system/stressTest/index")
        {
            return "stress-test";
        }

        if (normalizedPath == "/platform/server" ||
            normalizedPath == "/server" ||
            normalizedComponent == "/system/server/index")
        {
            return "server";
        }

        if (normalizedPath == "/platform/wechatpay" ||
            normalizedPath == "/wechat-pay" ||
            normalizedComponent == "/system/weChatPay/index")
        {
            return "wechat-pay";
        }

        if (normalizedPath == "/platform/update" ||
            normalizedPath == "/update" ||
            normalizedComponent == "/system/update/index")
        {
            return "update";
        }

        if (normalizedPath == "/log/vislog" ||
            normalizedPath == "/vis-log" ||
            normalizedComponent == "/system/log/vislog/index")
        {
            return "vis-log";
        }

        if (normalizedPath == "/log/oplog" ||
            normalizedPath == "/op-log" ||
            normalizedComponent == "/system/log/oplog/index")
        {
            return "op-log";
        }

        if (normalizedPath == "/log/exlog" ||
            normalizedPath == "/ex-log" ||
            normalizedComponent == "/system/log/exlog/index")
        {
            return "ex-log";
        }

        if (normalizedPath == "/log/difflog" ||
            normalizedPath == "/diff-log" ||
            normalizedComponent == "/system/log/difflog/index")
        {
            return "diff-log";
        }

        return string.Empty;
    }

    public static string ResolveHome(string? homePath)
    {
        var route = Resolve(homePath, null);
        return route.Length == 0 ? FallbackRoute : route;
    }

    public static string ResolveIframe(string menuId)
        => string.IsNullOrWhiteSpace(menuId) ? string.Empty : IframeRoutePrefix + menuId;

    public static AppNavigationItem? ResolveSupplemental(string route)
    {
        var normalized = Normalize(route);
        if (normalized == "/analytics")
        {
            return new AppNavigationItem
            {
                Id = "supplemental-analytics",
                Title = "分析页",
                SectionTitle = "工作台",
                BackendPath = "/analytics",
                Route = "analytics",
                IsSupported = true,
                IsAffix = true
            };
        }

        if (normalized == "/workspace")
        {
            return new AppNavigationItem
            {
                Id = "supplemental-workspace",
                Title = "工作空间",
                SectionTitle = "工作台",
                BackendPath = "/workspace",
                Route = "workspace",
                IsSupported = true
            };
        }

        if (normalized == "/job/dashboard" || normalized == "/platform/job/dashboard")
        {
            return new AppNavigationItem
            {
                Id = "supplemental-job-dashboard",
                Title = "任务看板",
                SectionTitle = "系统工具",
                BackendPath = "/platform/job/dashboard",
                Route = "job/dashboard",
                IsSupported = true
            };
        }

        return null;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Trim();
        return normalized.StartsWith('/') ? normalized : "/" + normalized;
    }
}
