using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Auth;
using static ECMAScript.Global;
using Jero.Web.Infrastructure.Http;

namespace Jero.Web.Infrastructure.Api.Shell;

/// <summary>加载并转换登录后应用外壳的初始数据。</summary>
[ECMAScriptModule("components/app-shell-client.js")]
public static class ShellClient
{
    private const int MenuItemType = 2;

    public static IPromise<AppBootstrapState> RefreshUser()
        => AuthClient.UserInfo().Then(userOutcome => userOutcome.Ok
            ? new AppBootstrapState
            {
                Ok = true,
                User = ReadUser(userOutcome.Data)
            }
            : Failure(userOutcome));

    public static IPromise<AppBootstrapState> Load()
        => AuthClient.UserInfo().Then(userOutcome =>
        {
            if (!userOutcome.Ok)
                return Promise<AppBootstrapState>.Resolve(Failure(userOutcome));

            var user = ReadUser(userOutcome.Data);
            return BrowserApiClient.Get("/api/sysMenu/loginMenuTree").Then(menuOutcome =>
            {
                if (!menuOutcome.Ok)
                    return Promise<AppBootstrapState>.Resolve(Failure(menuOutcome));

                var menus = ReadMenus(menuOutcome.Data);
                return BrowserApiClient.Get("/api/sysConfig/sysInfo").Then(configOutcome => new AppBootstrapState
                {
                    Ok = true,
                    User = user,
                    Menus = menus,
                    Navigation = BuildNavigation(menus),
                    // UserInfo 已返回当前会话生效的按钮权限，可直接写入外壳状态。
                    Permissions = user.Buttons,
                    // 公开品牌配置读取失败时，已登录的应用外壳仍可启动，并显示默认 Jero 标题。
                    Brand = configOutcome.Ok ? ReadBrand(configOutcome.Data) : new AppBrandState()
                });
            });
        });

    private static AppBootstrapState Failure(ApiOutcome outcome)
        => new()
        {
            Ok = false,
            Unauthorized = outcome.Code == 401,
            Error = outcome.Error ?? "加载应用信息失败。"
        };

    private static CurrentUser ReadUser(object? data)
        => new()
        {
            Id = ReadString(data, "id"),
            Account = ReadString(data, "account"),
            RealName = ReadString(data, "realName"),
            Avatar = ReadString(data, "avatar"),
            OrgName = ReadString(data, "orgName"),
            PosName = ReadString(data, "posName"),
            WatermarkText = ReadString(data, "watermarkText"),
            LangCode = ReadString(data, "langCode"),
            HomePath = ReadString(data, "homePath"),
            TenantId = ReadString(data, "tenantId"),
            CurrentTenantId = ReadString(data, "currentTenantId"),
            AccountType = ReadInt(data, "accountType"),
            Buttons = ReadStrings(data is null ? null : Reflect.Get(data, "buttons")),
            RoleIds = ReadStrings(data is null ? null : Reflect.Get(data, "roleIds"))
        };

    private static AppBrandState ReadBrand(object? data)
    {
        var title = ReadString(data, "title");
        return new AppBrandState
        {
            Title = title.Length == 0 ? "Jero 管理平台" : title,
            Logo = NormalizeImageUrl(ReadString(data, "logo"))
        };
    }

    private static AppMenuItem[] ReadMenus(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new AppMenuItem[values.Length];

        for (var index = 0; index < values.Length; index++)
            result[index] = ReadMenu(values[index]);

        return result;
    }

    private static AppMenuItem ReadMenu(object? value)
    {
        var metaValue = value is null ? null : Reflect.Get(value, "meta");
        var childrenValue = value is null ? null : Reflect.Get(value, "children");

        return new AppMenuItem
        {
            Id = ReadString(value, "id"),
            ParentId = ReadString(value, "pid"),
            Type = ReadInt(value, "type"),
            Name = ReadString(value, "name"),
            Path = ReadString(value, "path"),
            Component = ReadString(value, "component"),
            Permission = ReadString(value, "permission"),
            Redirect = ReadString(value, "redirect"),
            OrderNo = ReadInt(value, "orderNo"),
            Meta = new AppMenuMeta
            {
                Title = ReadString(metaValue, "title"),
                Icon = ReadString(metaValue, "icon"),
                IsIframe = ReadBool(metaValue, "isIframe"),
                Link = ReadString(metaValue, "isLink"),
                IsHidden = ReadBool(metaValue, "isHide"),
                IsKeepAlive = ReadBool(metaValue, "isKeepAlive"),
                IsAffix = ReadBool(metaValue, "isAffix")
            },
            Children = ReadMenus(childrenValue)
        };
    }

    private static AppNavigationItem[] BuildNavigation(AppMenuItem[] menus)
    {
        var result = new List<AppNavigationItem>();

        for (var index = 0; index < menus.Length; index++)
        {
            var root = menus[index];
            var section = MenuTitle(root);
            FillNavigation(root, section, result);
        }

        return result.ToArray();
    }

    private static void FillNavigation(
        AppMenuItem item,
        string section,
        List<AppNavigationItem> target)
    {
        if (!item.Meta.IsHidden && item.Type == MenuItemType)
        {
            var externalUrl = item.Meta.Link;
            var isIframe = item.Meta.IsIframe && externalUrl.Length > 0;
            var route = isIframe
                ? RouteRegistry.ResolveIframe(item.Id)
                : RouteRegistry.Resolve(item.Path, item.Component);
            var isExternal = !isIframe && externalUrl.Length > 0;
            var lastSection = target.Count == 0 ? string.Empty : target[target.Count - 1].SectionTitle;

            target.Add(new AppNavigationItem
            {
                Id = item.Id,
                Title = MenuTitle(item),
                SectionTitle = section,
                StartsSection = lastSection != section,
                BackendPath = item.Path,
                Route = route,
                IsSupported = route.Length > 0,
                IsExternal = isExternal,
                IsIframe = isIframe,
                IsAffix = item.Meta.IsAffix,
                ExternalUrl = isIframe ? NormalizeIframeTarget(externalUrl) : externalUrl
            });
        }

        for (var index = 0; index < item.Children.Length; index++)
            FillNavigation(item.Children[index], section, target);
    }

    private static string NormalizeIframeTarget(string target)
    {
        var value = target.Trim();
        if (value.Length == 0 || value.StartsWith('/'))
            return value;

        var schemeIndex = value.IndexOf("://");
        if (schemeIndex < 0)
            return value;

        var hostStart = schemeIndex + 3;
        var pathStart = value.IndexOf('/', hostStart);
        if (pathStart < 0)
            return value;

        var host = value.Substring(hostStart, pathStart - hostStart).ToLowerInvariant();
        return host.StartsWith("localhost:") ||
               host.StartsWith("127.0.0.1:") ||
               host.StartsWith("[::1]:")
            ? value.Substring(pathStart)
            : value;
    }

    private static string MenuTitle(AppMenuItem item)
    {
        if (item.Meta.Title.Length > 0)
            return item.Meta.Title;
        if (item.Name.Length > 0)
            return item.Name;
        return item.Path;
    }

    private static string[] ReadStrings(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new string[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = values[index] is null ? string.Empty : StringValue(values[index]!);
        return result;
    }

    private static string ReadString(object? data, string key)
    {
        if (data is null)
            return string.Empty;

        var value = Reflect.Get(data, key);
        return value is null ? string.Empty : StringValue(value);
    }

    private static int ReadInt(object? data, string key)
    {
        if (data is null)
            return 0;

        var value = Reflect.Get(data, key);
        return value is null ? 0 : (int)NumberValue(value);
    }

    private static bool ReadBool(object? data, string key)
    {
        if (data is null)
            return false;

        var value = Reflect.Get(data, key);
        return value is not null && BooleanValue(value);
    }

    private static string NormalizeImageUrl(string value)
    {
        // 种子数据的默认 Logo 路径仍是 /upload/logo.png，随应用发布的默认图片位于 /images/logo.png。
        // 只替换这个种子路径，保留用户上传的自定义路径。
        if (value == "/upload/logo.png")
            return "/images/logo.png";
        if (value.Length == 0 || value.StartsWith('/') || value.StartsWith("http://") || value.StartsWith("https://") || value.StartsWith("data:"))
            return value;
        return "/" + value;
    }
}
