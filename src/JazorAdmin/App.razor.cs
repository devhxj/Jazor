using ECMAScript;
using Jazor.Admin;
using static ECMAScript.Vue3;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using static JazorAdmin.Routes;
using static ECMAScript.VueRoute;

namespace JazorAdmin;

[ECMAScriptModule("./components/jazor-admin-app")]
public partial class App : ComponentBase, IVueComponent
{
    private readonly RouteLocationNormalizedLoaded currentRoute = VueRoute.UseRoute();
    private readonly Router router = UseRouter();
    private string themeKey = "system";
    private string languageTag = "en-US";
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

    private ReleaseTableColumns ReleaseColumns { get; } = new ReleaseTableColumn[]
    {
        new()
        {
            Key = "name",
            Title = "Service",
            Width = "36%",
            Sortable = true
        },
        new()
        {
            Key = "version",
            Title = "Version",
            Width = "22%"
        },
        new()
        {
            Key = "status",
            Title = "Status",
            Width = "22%"
        },
        new()
        {
            Key = "owner",
            Title = "Owner",
            Width = "20%"
        }
    };

    private ReleaseTableRows ReleaseRows { get; } = new ReleaseTableRow[]
    {
        new()
        {
            Key = "release.api",
            Cells = new ReleaseTableCell[]
            {
                new() { ColumnKey = "name", Text = "Admin API" },
                new() { ColumnKey = "version", Text = "2026.07.28-alpha" },
                new() { ColumnKey = "status", Text = "Ready" },
                new() { ColumnKey = "owner", Text = "Platform" }
            }
        },
        new()
        {
            Key = "release.web",
            Cells = new ReleaseTableCell[]
            {
                new() { ColumnKey = "name", Text = "Admin Web" },
                new() { ColumnKey = "version", Text = "2026.07.28-alpha" },
                new() { ColumnKey = "status", Text = "Verifying" },
                new() { ColumnKey = "owner", Text = "Frontend" }
            }
        },
        new()
        {
            Key = "release.worker",
            Cells = new ReleaseTableCell[]
            {
                new() { ColumnKey = "name", Text = "Audit Worker" },
                new() { ColumnKey = "version", Text = "2026.07.27" },
                new() { ColumnKey = "status", Text = "Queued" },
                new() { ColumnKey = "owner", Text = "Operations" }
            }
        }
    };

    private bool IsErrorRoute
        => currentRoute.Path == InternalErrorPath ||
           !AdminRouteCatalog.ContainsPath(RouterItems, currentRoute.Path);

    private RenderFragment SelectedPageContent
        => AddSelectedPageContent;

    private string GetSelectedPageTitle() => SelectedRoute.Title ?? string.Empty;

    private string GetSelectedPageSubtitle() => SelectedRoute.Subtitle ?? string.Empty;

    private AdminBreadcrumbItem[] GetSelectedBreadcrumbItems()
    {
        return AdminRouteCatalog.BuildBreadcrumbs(
            LocalizedItems,
            SelectedKey,
            CreateHomeBreadcrumbItem());
    }

    private static AdminBreadcrumbItem CreateHomeBreadcrumbItem()
        => new()
        {
            Key = "home",
            Title = "JazorAdmin",
            RouteTarget = (RouteLocationRaw)"/"
        };

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
        {
            return;
        }

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

    private void AddSelectedPageContent(RenderTreeBuilder pageBuilder)
    {
        if (SelectedKey == ReleasesKey)
        {
            AddReleaseQueue(pageBuilder, "jazor-admin__release-section jazor-admin__release-section--focused");
            return;
        }

        if (SelectedKey == AuditKey)
        {
            AddAuditPage(pageBuilder);
            return;
        }

        if (SelectedKey == SettingsKey)
        {
            AddSettingsPage(pageBuilder);
            return;
        }

        if (SelectedKey == WorkspaceKey)
        {
            AddWorkspacePage(pageBuilder);
            return;
        }

        AddDashboardPage(pageBuilder);
    }

    private void AddDashboardPage(RenderTreeBuilder pageBuilder)
    {
        pageBuilder.OpenElement(0, "section");
        pageBuilder.AddAttribute(1, "class", "jazor-admin__metrics");
        AddMetricCard(pageBuilder, "Direct VNode", "enabled", "Razor SG -> IOperation -> Vue h()");
        AddMetricCard(pageBuilder, "Shell", "TDesign", "Application-local layout, navigation, header, and page container");
        AddMetricCard(pageBuilder, "Smoke", "integration", "Local packages and generated .mjs artifacts");
        pageBuilder.CloseElement();

        AddReleaseQueue(pageBuilder, "jazor-admin__release-section");
    }

    private void AddReleaseQueue(RenderTreeBuilder pageBuilder, string cssClass)
    {
        pageBuilder.OpenElement(0, "section");
        pageBuilder.AddAttribute(1, "class", cssClass);

        pageBuilder.OpenElement(2, "h2");
        pageBuilder.AddContent(3, "Release Queue");
        pageBuilder.CloseElement();

        pageBuilder.OpenElement(4, "p");
        pageBuilder.AddAttribute(5, "class", "jazor-admin__selection");
        pageBuilder.AddContent(6, "Selected release: ");
        pageBuilder.OpenElement(7, "strong");
        pageBuilder.AddContent(8, selectedReleaseKey);
        pageBuilder.CloseElement();
        pageBuilder.AddContent(9, " Bulk selected: ");
        pageBuilder.OpenElement(10, "strong");
        pageBuilder.AddContent(11, selectedReleaseKeys.Length);
        pageBuilder.CloseElement();
        pageBuilder.CloseElement();

        pageBuilder.OpenComponent<ReleaseTable>(12);
        pageBuilder.AddComponentParameter(13, nameof(ReleaseTable.Columns), ReleaseColumns);
        pageBuilder.AddComponentParameter(14, nameof(ReleaseTable.Rows), ReleaseRows);
        pageBuilder.AddComponentParameter(15, nameof(ReleaseTable.SelectedRowKey), selectedReleaseKey);
        pageBuilder.AddComponentParameter(16, nameof(ReleaseTable.SelectedRowKeyChanged), EventCallback.Factory.Create<string>(this, value => selectedReleaseKey = value));
        pageBuilder.AddComponentParameter(17, nameof(ReleaseTable.SelectedRowKeys), selectedReleaseKeys);
        pageBuilder.AddComponentParameter(18, nameof(ReleaseTable.SelectedRowKeysChanged), EventCallback.Factory.Create<string[]>(this, value => selectedReleaseKeys = value));
        pageBuilder.AddComponentParameter(19, nameof(ReleaseTable.MultiSelectable), true);
        pageBuilder.AddComponentParameter(20, nameof(ReleaseTable.SearchText), releaseSearchText);
        pageBuilder.AddComponentParameter(21, nameof(ReleaseTable.SearchTextChanged), EventCallback.Factory.Create<string>(this, value => releaseSearchText = value));
        pageBuilder.AddComponentParameter(22, nameof(ReleaseTable.SearchPlaceholder), "Filter releases");
        pageBuilder.AddComponentParameter(23, nameof(ReleaseTable.PageIndex), releasePageIndex);
        pageBuilder.AddComponentParameter(24, nameof(ReleaseTable.PageIndexChanged), EventCallback.Factory.Create<int>(this, value => releasePageIndex = value));
        pageBuilder.AddComponentParameter(25, nameof(ReleaseTable.PageSize), 2);
        pageBuilder.AddComponentParameter(26, nameof(ReleaseTable.SortColumnKey), releaseSortColumnKey);
        pageBuilder.AddComponentParameter(27, nameof(ReleaseTable.SortColumnKeyChanged), EventCallback.Factory.Create<string>(this, value => releaseSortColumnKey = value));
        pageBuilder.AddComponentParameter(28, nameof(ReleaseTable.SortDescending), releaseSortDescending);
        pageBuilder.AddComponentParameter(29, nameof(ReleaseTable.SortDescendingChanged), EventCallback.Factory.Create<bool>(this, value => releaseSortDescending = value));
        pageBuilder.AddComponentParameter(30, nameof(ReleaseTable.Loading), releaseLoading);
        pageBuilder.AddComponentParameter(31, nameof(ReleaseTable.LoadingText), "Refreshing releases");
        pageBuilder.CloseComponent();

        pageBuilder.CloseElement();
    }

    private static void AddAuditPage(RenderTreeBuilder pageBuilder)
    {
        pageBuilder.OpenElement(0, "section");
        pageBuilder.AddAttribute(1, "class", "jazor-admin__audit");

        pageBuilder.OpenElement(2, "h2");
        pageBuilder.AddContent(3, "Recent Changes");
        pageBuilder.CloseElement();

        AddStatusLine(pageBuilder, "Release approved", "Admin API promoted to alpha by Platform");
        AddStatusLine(pageBuilder, "Smoke completed", "Generated render modules passed browser verification");
        AddStatusLine(pageBuilder, "Navigation updated", "Workspace page selected through sidebar state");

        pageBuilder.CloseElement();
    }

    private void AddSettingsPage(RenderTreeBuilder pageBuilder)
    {
        pageBuilder.OpenElement(0, "section");
        pageBuilder.AddAttribute(1, "class", "jazor-admin__settings");

        pageBuilder.OpenElement(2, "h2");
        pageBuilder.AddContent(3, "Application Settings");
        pageBuilder.CloseElement();

        pageBuilder.OpenComponent<SettingsForm>(4);
        pageBuilder.AddComponentParameter(5, nameof(SettingsForm.Fields), GetSettingsFields());
        pageBuilder.AddComponentParameter(6, nameof(SettingsForm.SubmitText), "Save settings");
        pageBuilder.AddComponentParameter(7, nameof(SettingsForm.StatusText), settingsStatus);
        pageBuilder.AddComponentParameter(8, nameof(SettingsForm.Submit), EventCallback.Factory.Create(this, SaveSettings));
        pageBuilder.AddComponentParameter(9, nameof(SettingsForm.FieldChanged), EventCallback.Factory.Create<SettingsFieldChange>(this, OnSettingsFieldChanged));
        pageBuilder.CloseComponent();

        pageBuilder.CloseElement();
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

    private static void AddWorkspacePage(RenderTreeBuilder pageBuilder)
    {
        pageBuilder.OpenElement(0, "section");
        pageBuilder.AddAttribute(1, "class", "jazor-admin__workspace");

        pageBuilder.OpenElement(2, "h2");
        pageBuilder.AddContent(3, "Operator Workspace");
        pageBuilder.CloseElement();

        AddStatusLine(pageBuilder, "Pinned queue", "Release Queue");
        AddStatusLine(pageBuilder, "Current focus", "RazorVue direct render integration");
        AddStatusLine(pageBuilder, "Next checkpoint", "Admin shell page switching");

        pageBuilder.CloseElement();
    }

    private void DismissActionStatus()
        => actionStatus = null;

    private static void AddStatusLine(
        RenderTreeBuilder builder,
        string title,
        string detail)
    {
        builder.OpenElement(0, "article");
        builder.AddAttribute(1, "class", "jazor-admin__status-line");

        builder.OpenElement(2, "strong");
        builder.AddContent(3, title);
        builder.CloseElement();

        builder.OpenElement(4, "span");
        builder.AddContent(5, detail);
        builder.CloseElement();

        builder.CloseElement();
    }

    private static void AddMetricCard(
        RenderTreeBuilder builder,
        string title,
        string value,
        string detail)
    {
        builder.OpenElement(0, "article");
        builder.AddAttribute(1, "class", "jazor-admin__metric");

        builder.OpenElement(2, "h2");
        builder.AddContent(3, title);
        builder.CloseElement();

        builder.OpenElement(4, "strong");
        builder.AddContent(5, value);
        builder.CloseElement();

        builder.OpenElement(6, "p");
        builder.AddContent(7, detail);
        builder.CloseElement();

        builder.CloseElement();
    }
}
