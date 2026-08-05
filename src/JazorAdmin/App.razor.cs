using ECMAScript;
using Jazor.Admin;
using static ECMAScript.Vue3;
using Microsoft.AspNetCore.Components;
using static JazorAdmin.Routes;
using static ECMAScript.VueRoute;

namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-app")]
public partial class App : ComponentBase, IVueComponent
{
    private readonly RouteLocationNormalizedLoaded currentRoute = VueRoute.UseRoute();
    private readonly Router router = UseRouter();
    private string themeKey = "light";
    private string languageTag = "zh-CN";
    private bool grayscale;
    private string loginAccount = "admin@jazor";
    private string loginPassword = string.Empty;
    private string unlockPassword = string.Empty;
    private string? accessError;
    private bool collapsed;
    private string[] expandedKeys = [];
    private string selectedReleaseKey = "release.api";
    private string[] selectedReleaseKeys = [];
    private string releaseSearchText = string.Empty;
    private int releasePageIndex;
    private string releaseSortColumnKey = string.Empty;
    private bool releaseSortDescending;
    private bool releaseLoading;
    private int refreshCount;
    private int deployCount;
    private int settingsSaveCount;
    private string settingsTheme = "System";
    private string settingsNavigationMode = "Mixed";
    private string settingsReleaseChannel = "stable";
    private bool settingsSmokeRequired = true;
    private string settingsStatus = "No settings have been saved.";
    private string? actionStatus;
    private ActionNoticeKind actionStatusKind = ActionNoticeKind.Info;

    private AdminThemeMode Theme
        => themeKey == "dark" ? AdminThemeMode.Dark : themeKey == "light" ? AdminThemeMode.Light : AdminThemeMode.System;

    private AdminLanguage Language
        => languageTag == "zh-CN" ? AdminLanguage.Chinese : AdminLanguage.English;

    private AdminLayoutMode LayoutMode
        => settingsNavigationMode == "Header"
            ? AdminLayoutMode.Top
            : settingsNavigationMode == "Sidebar"
                ? AdminLayoutMode.Sidebar
                : AdminLayoutMode.Mixed;

    private AdminRouteDefinition[] LocalizedItems => CreateItems(Language);

    private AdminRouteDefinition SelectedRoute
        => AdminRouteCatalog.Resolve(LocalizedItems, currentRoute.Path, DashboardKey);

    private string SelectedKey => SelectedRoute.Key;

    private string[] EffectiveExpandedKeys
        => AdminRouteCatalog.BuildExpandedKeys(LocalizedItems, SelectedKey, expandedKeys);

    private bool IsErrorRoute
        => currentRoute.Path == InternalErrorPath ||
           !AdminRouteCatalog.ContainsPath(RouterItems, currentRoute.Path);

    private bool IsReleasesPage => SelectedKey == ReleasesKey;

    private bool IsAuditPage => SelectedKey == AuditKey;

    private bool IsSettingsPage => SelectedKey == SettingsKey;

    private bool IsWorkspacePage => SelectedKey == WorkspaceKey;

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

    private AdminPageAction[] GetPageActions()
        =>
        [
            new()
            {
                Key = "refresh",
                Text = "Refresh",
                Kind = AdminPageActionKind.Secondary,
                Disabled = releaseLoading,
                Click = EventCallback.Factory.Create(this, RefreshPage)
            },
            new()
            {
                Key = "deploy",
                Text = "Deploy",
                Kind = AdminPageActionKind.Primary,
                Disabled = releaseLoading,
                Click = EventCallback.Factory.Create(this, DeploySelectedRelease)
            }
        ];

    private async Task RefreshPage()
    {
        if (releaseLoading)
            return;

        releaseLoading = true;
        actionStatus = null;
        await Task.Delay(75);
        refreshCount++;
        releaseLoading = false;
        actionStatusKind = ActionNoticeKind.Success;
        actionStatus = "Refreshed " + GetSelectedPageTitle()
                       + ". Refreshes: " + refreshCount
                       + " Deploys: " + deployCount;
    }

    private void DeploySelectedRelease()
    {
        deployCount++;
        actionStatusKind = ActionNoticeKind.Success;
        var requestStatus = selectedReleaseKeys.Length == 0
            ? "Deploy requested for " + selectedReleaseKey
            : "Bulk deploy requested for " + selectedReleaseKeys.Length + " releases";
        actionStatus = requestStatus
                       + ". Refreshes: " + refreshCount
                       + " Deploys: " + deployCount;
    }

    private SettingsFields GetSettingsFields()
        => new SettingsField[]
        {
            new()
            {
                Key = "theme",
                Kind = SettingsFieldKind.Select,
                Label = "Theme",
                Value = settingsTheme,
                Options = new SettingsOption[]
                {
                    new() { Value = "System" },
                    new() { Value = "Light" },
                    new() { Value = "Dark" }
                }
            },
            new()
            {
                Key = "navigation-mode",
                Kind = SettingsFieldKind.Select,
                Label = "Navigation mode",
                Value = settingsNavigationMode,
                Options = new SettingsOption[]
                {
                    new() { Value = "Mixed" },
                    new() { Value = "Sidebar" },
                    new() { Value = "Header" }
                }
            },
            new()
            {
                Key = "release-channel",
                Kind = SettingsFieldKind.Text,
                Label = "Default release channel",
                HelpText = "Used when a release does not provide an explicit channel.",
                Value = settingsReleaseChannel,
                Autocomplete = "off"
            },
            new()
            {
                Key = "smoke-required",
                Kind = SettingsFieldKind.Checkbox,
                Label = "Require smoke verification before release",
                Checked = settingsSmokeRequired
            }
        };

    private void OnSettingsFieldChanged(SettingsFieldChange change)
    {
        if (change.Key == "theme" && change.Value.AsString is { } theme)
        {
            settingsTheme = theme;
            return;
        }

        if (change.Key == "navigation-mode" && change.Value.AsString is { } navigationMode)
        {
            settingsNavigationMode = navigationMode;
            return;
        }

        if (change.Key == "release-channel" && change.Value.AsString is { } releaseChannel)
        {
            settingsReleaseChannel = releaseChannel;
            return;
        }

        if (change.Key == "smoke-required" && change.Value.AsBoolean is { } smokeRequired)
        {
            settingsSmokeRequired = smokeRequired;
        }
    }

    private void SaveSettings()
    {
        settingsSaveCount++;
        settingsStatus = "Saved settings " + settingsSaveCount + ": "
                         + settingsTheme + ", "
                         + (settingsSmokeRequired ? "smoke required" : "smoke optional") + ", "
                         + settingsNavigationMode + ", "
                         + settingsReleaseChannel;
    }

    private void DismissActionStatus()
        => actionStatus = null;

}
