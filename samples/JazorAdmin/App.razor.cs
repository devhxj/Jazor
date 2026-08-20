using JazorAdmin.Features.Identity;
using JazorAdmin.Features.Notifications;
using static ECMAScript.VueRoute;
using static JazorAdmin.Routes;

namespace JazorAdmin;

[ECMAScriptModule("./components/app")]
public partial class App : ComponentBase, IVueComponent
{
    private readonly RouteLocationNormalizedLoaded currentRoute = VueRoute.UseRoute();
    private readonly Router router = UseRouter();
    private string themeKey = "light";
    private string brandTheme = "#0052D9";
    private string layoutKey = "mix";
    private string sideModeKey = "light";
    private string languageTag = "zh-CN";
    private string loginAccount = string.Empty;
    private string loginPassword = string.Empty;
    private string loginCaptcha = string.Empty;
    private string? loginCaptchaId;
    private string? loginCaptchaImageUrl;
    private string unlockPassword = string.Empty;
    private string? accessError;
    private bool sessionRestoring = true;
    private int captchaRequestVersion;
    private SessionResponse? session;
    private OrganizationSummary[] organizations = [];
    private string? selectedOrganizationId;
    private bool collapsed;
    private bool showSettingPanel;
    private bool showHelpDialog;
    private bool splitMenu;
    private bool isSidebarFixed = true;
    private bool showHeader = true;
    private bool showBreadcrumb;
    private bool showFooter = true;
    private bool isUseTabsRouter = true;
    private bool menuAutoCollapsed;
    private string[] expandedKeys = [];
    private string searchText = string.Empty;
    private bool notificationsOpen;
    private bool notificationsLoading;
    private int notificationRequestVersion;
    private NotificationView[] notifications = [];

    private const int SearchResultLimit = 8;
    private static readonly AdminRouteDefinition[] NoSearchResults = Array.Empty<AdminRouteDefinition>();

    private static readonly TDropdownMinColumnWidthValue UserMenuWidth = "152px";
    private const string StyleStoragePrefix = "jazoradmin.starter.style.";
    private const string PlatformAdministratorRole = "platform-administrator";

    private AdminThemeMode Theme
        => themeKey == "dark" ? AdminThemeMode.Dark : themeKey == "light" ? AdminThemeMode.Light : AdminThemeMode.System;

    private AdminThemeMode SidebarTheme
        => sideModeKey == "dark" ? AdminThemeMode.Dark : AdminThemeMode.Light;

    private AdminLanguage Language
        => languageTag == "zh-CN" ? AdminLanguage.Chinese : AdminLanguage.English;

    private AdminLayoutMode LayoutMode
        => layoutKey == "top" ? AdminLayoutMode.Top : layoutKey == "side" ? AdminLayoutMode.Sidebar : AdminLayoutMode.Mixed;

    private VueStyleValue ApplicationStyle
        => (VueStyleValue)("--ja-brand-color: " + brandTheme + ";");

    // Draw this at document scope because TDesign teleports drawers and popups under body.
    // 主题 token 不能只挂在应用容器，否则浮层仍会继承默认蓝。
    private string BrandThemeCss
        => ":root { --td-brand-color: " + brandTheme + "; " +
           "--td-brand-color-hover: color-mix(in srgb, " + brandTheme + " 86%, #ffffff); " +
           "--td-brand-color-active: color-mix(in srgb, " + brandTheme + " 86%, #000000); " +
           "--td-brand-color-light: color-mix(in srgb, " + brandTheme + " 10%, #ffffff); " +
           "--td-brand-color-light-hover: color-mix(in srgb, " + brandTheme + " 16%, #ffffff); }";

    private AdminRouteDefinition[] LocalizedItems => CreateItems(Language);

    // Keep stylesheet registration behind one setup member so Razor render lowering performs
    // the side effects in the component lifecycle rather than at module evaluation time.
    // 样式注册集中在 setup 成员中执行，避免模块加载阶段提前写入 document。
    private static void EnsureShellStyles()
    {
        Styles.EnsureLoaded();
        StarterStyles.EnsureLoaded();
    }

    // These derived values are shared by render and navigation state, so retain one named
    // projection instead of rebuilding the route/catalog lookup at every Razor call site.
    // 渲染与导航状态共用同一投影，避免 Razor 调用点重复拼装路由和模板查找。
    private AdminNavItems NavigationItems
        => AdminRouteCatalog.BuildNavigation(LocalizedItems);

    private AdminRouteDefinition SelectedRoute
        => AdminRouteCatalog.Resolve(LocalizedItems, currentRoute.Path, DashboardKey);

    private string SelectedKey => SelectedRoute.Key;

    private bool HasSession => session is not null;

    private bool CanConfigureAppearance
    {
        get
        {
            if (session?.Roles is not { Length: > 0 } roles)
                return false;

            foreach (var role in roles)
            {
                if (role == PlatformAdministratorRole)
                    return true;
            }

            return false;
        }
    }

    private string CurrentUserLabel => session?.DisplayName ?? session?.Email ?? string.Empty;

    private string CurrentUserInitial
        => string.IsNullOrWhiteSpace(CurrentUserLabel) ? "A" : CurrentUserLabel.Substring(0, 1).ToUpperInvariant();

    private OrganizationSummary? SelectedOrganization
        => organizations.FirstOrDefault(organization => organization.Id == selectedOrganizationId);

    private string[] EffectiveExpandedKeys
        => AdminRouteCatalog.BuildExpandedKeys(LocalizedItems, SelectedKey, expandedKeys);

    private bool IsErrorRoute
        => currentRoute.Path == InternalErrorPath ||
           !AdminRouteCatalog.ContainsPath(RouterItems, currentRoute.Path);

    private bool IsOrganizationPage
        => SelectedKey is OrganizationStructureKey or OrganizationMembersKey;

    private bool IsOrganizationMembersPage => SelectedKey == OrganizationMembersKey;

    private bool IsAuthorizationPage
        => SelectedKey is AuthorizationRolesKey or AuthorizationResourcesKey;

    private bool IsAuthorizationResourcesPage => SelectedKey == AuthorizationResourcesKey;

    private bool IsAccountsPage => SelectedKey == AccountsKey;

    private bool IsSsoPage
        => SelectedKey is SsoApplicationsKey or SsoScopesKey or SsoAuthorizationsKey or SsoTokensKey;

    private bool IsSsoApplicationsPage => SelectedKey == SsoApplicationsKey;

    private bool IsSsoScopesPage => SelectedKey == SsoScopesKey;

    private bool IsSsoAuthorizationsPage => SelectedKey == SsoAuthorizationsKey;

    private bool IsSettingsPage => SelectedKey == SettingsKey;

    private bool IsSchedulesPage => SelectedKey == SchedulesKey;

    private bool IsAuditPage => SelectedKey == AuditKey;

    private AdminBreadcrumbItem[] SelectedBreadcrumbItems
        => AdminRouteCatalog.BuildBreadcrumbs(LocalizedItems, SelectedKey);

    private bool HasSearchQuery => searchText.Trim().Length > 0;

    private AdminRouteDefinition[] SearchResults
    {
        get
        {
            var query = searchText.Trim();
            return query.Length == 0 ? NoSearchResults : CollectSearchResults(LocalizedItems, query);
        }
    }

    // 只匹配带具体 Path 的叶子路由；分支节点通过子级命中。上限 SearchResultLimit，
    // 递归按声明顺序收集，保证同一输入的结果顺序确定。
    private static AdminRouteDefinition[] CollectSearchResults(AdminRouteDefinition[] items, string query)
    {
        List<AdminRouteDefinition>? matches = null;
        foreach (var item in items)
        {
            if (matches is { Count: >= SearchResultLimit })
            {
                break;
            }

            if (item.Path is not null
                && (item.Disabled ?? false) == false
                && item.Title is not null
                && item.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                matches ??= new List<AdminRouteDefinition>();
                matches.Add(item);
            }

            if (item.Children is { Length: > 0 } children)
            {
                foreach (var child in CollectSearchResults(children, query))
                {
                    if (matches is { Count: >= SearchResultLimit })
                    {
                        break;
                    }

                    matches ??= new List<AdminRouteDefinition>();
                    matches.Add(child);
                }
            }
        }

        return matches is null ? NoSearchResults : [.. matches];
    }

    private void SubmitSearch()
    {
        if (SearchResults is { Length: > 0 } results)
        {
            NavigateToSearchResult(results[0]);
        }
    }

    private void NavigateToSearchResult(AdminRouteDefinition item)
    {
        searchText = string.Empty;
        _ = router.Push((RouteLocationRaw)item.Path!);
    }

    private void OpenAccounts()
        => _ = router.Push((RouteLocationRaw)"/accounts");

    private void ToggleNotifications()
    {
        notificationsOpen = !notificationsOpen;
        // 每次打开重新拉取：会话恢复只负责初始化徽标，通知不做后台轮询。
        if (notificationsOpen)
        {
            LoadNotifications();
        }
    }

    private void LoadNotifications()
    {
        notificationsLoading = true;
        var requestVersion = ++notificationRequestVersion;
        ApiClient.GetNotifications().Then(outcome =>
        {
            // Session changes and a later panel refresh supersede this response. Without this
            // guard, an older authenticated request could restore a stale badge after sign-out.
            // 会话切换或后续刷新会使旧请求失效，避免登出后回写旧会话的通知徽标。
            if (requestVersion != notificationRequestVersion)
                return;

            notificationsLoading = false;
            notifications = outcome.Ok
                ? ApiClient.ToNotifications(outcome.Data)
                : [];
        });
    }

    // ISO "O" 时间仅在展示层截断为可读形式；不做时区换算，管理端以 UTC 为准。
    private static string FormatNotificationTime(string startedAt)
        => startedAt.Length <= 16 ? startedAt : startedAt.Substring(0, 16).Replace("T", " ");

    private TDropdownOption[] LanguageOptions =>
    [
        DropdownOption("English", "en-US", () => languageTag = "en-US"),
        DropdownOption("中文", "zh-CN", () => languageTag = "zh-CN")
    ];

    private TDropdownOption[] UserMenuOptions =>
    [
        DropdownOption(Localization.Get(Language, TextKey.Lock), "lock", OpenLockScreen),
        DropdownOption(Localization.Get(Language, TextKey.SignOut), "sign-out", SignOut)
    ];

    private static TDropdownOption DropdownOption(string text, string value, Action action)
        => new()
        {
            Content = (TdDropdownItemPropsContent)text,
            Value = (TdDropdownItemPropsValue)value,
            OnClick = (_, context) => action()
        };

    private void RestoreStarterStyleConfig()
    {
        themeKey = ReadStyleValue("mode", themeKey);
        brandTheme = ReadStyleValue("brandTheme", brandTheme);
        layoutKey = ReadStyleValue("layout", layoutKey);
        sideModeKey = ReadStyleValue("sideMode", sideModeKey);
        splitMenu = ReadStyleBoolean("splitMenu", splitMenu);
        isSidebarFixed = ReadStyleBoolean("isSidebarFixed", isSidebarFixed);
        showHeader = ReadStyleBoolean("showHeader", showHeader);
        showBreadcrumb = ReadStyleBoolean("showBreadcrumb", showBreadcrumb);
        showFooter = ReadStyleBoolean("showFooter", showFooter);
        isUseTabsRouter = ReadStyleBoolean("isUseTabsRouter", isUseTabsRouter);
        menuAutoCollapsed = ReadStyleBoolean("menuAutoCollapsed", menuAutoCollapsed);
    }

    private void ChangeTheme(string value) => UpdateStyle(() => themeKey = value);
    private void ChangeBrandTheme(string value) => UpdateStyle(() => brandTheme = value);
    private void ChangeLayout(string value) => UpdateStyle(() => layoutKey = value);
    private void ChangeSplitMenu(bool value) => UpdateStyle(() => splitMenu = value);
    private void ChangeSideMode(string value) => UpdateStyle(() => sideModeKey = value);
    private void ChangeSidebarFixed(bool value) => UpdateStyle(() => isSidebarFixed = value);
    private void ChangeShowHeader(bool value) => UpdateStyle(() => showHeader = value);
    private void ChangeShowBreadcrumb(bool value) => UpdateStyle(() => showBreadcrumb = value);
    private void ChangeShowFooter(bool value) => UpdateStyle(() => showFooter = value);
    private void ChangeUseTabs(bool value) => UpdateStyle(() => isUseTabsRouter = value);
    private void ChangeMenuAutoCollapsed(bool value) => UpdateStyle(() => menuAutoCollapsed = value);

    private void UpdateStyle(Action update)
    {
        update();
        PersistStarterStyleConfig();
    }

    private void PersistStarterStyleConfig()
    {
        try
        {
            var storage = Global.Window.LocalStorage;
            if (storage is null)
                return;

            storage.SetItem(StyleStoragePrefix + "mode", themeKey);
            storage.SetItem(StyleStoragePrefix + "brandTheme", brandTheme);
            storage.SetItem(StyleStoragePrefix + "layout", layoutKey);
            storage.SetItem(StyleStoragePrefix + "sideMode", sideModeKey);
            storage.SetItem(StyleStoragePrefix + "splitMenu", splitMenu ? "true" : "false");
            storage.SetItem(StyleStoragePrefix + "isSidebarFixed", isSidebarFixed ? "true" : "false");
            storage.SetItem(StyleStoragePrefix + "showHeader", showHeader ? "true" : "false");
            storage.SetItem(StyleStoragePrefix + "showBreadcrumb", showBreadcrumb ? "true" : "false");
            storage.SetItem(StyleStoragePrefix + "showFooter", showFooter ? "true" : "false");
            storage.SetItem(StyleStoragePrefix + "isUseTabsRouter", isUseTabsRouter ? "true" : "false");
            storage.SetItem(StyleStoragePrefix + "menuAutoCollapsed", menuAutoCollapsed ? "true" : "false");
        }
        catch
        {
        }
    }

    private static string ReadStyleValue(string key, string fallback)
    {
        try
        {
            var storage = Global.Window.LocalStorage;
            return storage?.GetItem(StyleStoragePrefix + key) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private static bool ReadStyleBoolean(string key, bool fallback)
    {
        try
        {
            var stored = Global.Window.LocalStorage?.GetItem(StyleStoragePrefix + key);
            return stored is null ? fallback : stored == "true";
        }
        catch
        {
            return fallback;
        }
    }

}
