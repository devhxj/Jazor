using ECMAScript;
using Jazor.Admin;
using static ECMAScript.Vue3;
using Microsoft.AspNetCore.Components;
using static JazorAdmin.Routes;
using static ECMAScript.VueRoute;
using JazorAdmin.Features.Identity;

namespace JazorAdmin;

[ECMAScriptModule("./components/app")]
public partial class App : ComponentBase, IVueComponent
{
    private readonly RouteLocationNormalizedLoaded currentRoute = VueRoute.UseRoute();
    private readonly Router router = UseRouter();
    private string themeKey = "light";
    private string languageTag = "zh-CN";
    private bool grayscale;
    private string loginAccount = string.Empty;
    private string loginPassword = string.Empty;
    private string unlockPassword = string.Empty;
    private string? accessError;
    private bool sessionRestoring = true;
    private SessionResponse? session;
    private OrganizationSummary[] organizations = [];
    private string? selectedOrganizationId;
    private bool collapsed;
    private string[] expandedKeys = [];

    private AdminThemeMode Theme
        => themeKey == "dark" ? AdminThemeMode.Dark : themeKey == "light" ? AdminThemeMode.Light : AdminThemeMode.System;

    private AdminLanguage Language
        => languageTag == "zh-CN" ? AdminLanguage.Chinese : AdminLanguage.English;

    private AdminLayoutMode LayoutMode
        => AdminLayoutMode.Mixed;

    private AdminRouteDefinition[] LocalizedItems => CreateItems(Language);

    private AdminRouteDefinition SelectedRoute
        => AdminRouteCatalog.Resolve(LocalizedItems, currentRoute.Path, DashboardKey);

    private string SelectedKey => SelectedRoute.Key;

    private bool HasSession => session is not null;

    private string CurrentUserLabel => session?.DisplayName ?? session?.Email ?? string.Empty;

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

    private bool IsConfigurationPage
        => SelectedKey is ConfigurationClientsKey or ConfigurationScopesKey;

    private bool IsConfigurationScopesPage => SelectedKey == ConfigurationScopesKey;

    private bool IsDashboardPage => SelectedKey == DashboardKey;

    private string DashboardContainerClass
        => IsDashboardPage ? "jazor-admin-tdesign-page-container--dashboard" : string.Empty;

    private string GetSelectedPageTitle() => SelectedRoute.Title ?? string.Empty;

    private string GetSelectedPageSubtitle() => SelectedRoute.Subtitle ?? string.Empty;

    private AdminBreadcrumbItem[] GetSelectedBreadcrumbItems()
        => AdminRouteCatalog.BuildBreadcrumbs(
            LocalizedItems,
            SelectedKey,
            new AdminBreadcrumbItem
            {
                Key = "home",
                Title = "JazorAdmin",
                RouteTarget = (RouteLocationRaw)"/"
            });

    private AdminPageAction[] GetPageActions() => [];

}
