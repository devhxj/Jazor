using ECMAScript;
using Jazor.Admin;

namespace JazorAdmin;

[ECMAScriptModule("components/jazor-admin-routes.mjs")]
public static class Routes
{
    public const string DashboardKey = "dashboard";
    public const string ReleasesKey = "operations.releases";
    public const string AuditKey = "operations.audit";
    public const string SettingsKey = "settings";
    public const string WorkspaceKey = "workspace";
    public const string LoginKey = "login";
    public const string LockKey = "lock";
    public const string InternalErrorKey = "error.500";
    public const string NotFoundKey = "error.404";
    public const string InternalErrorPath = "/error/500";
    public const string NotFoundPath = "/:pathMatch(.*)*";

    public static readonly AdminRouteDefinition[] Items = CreateItems(AdminLanguage.English);

    public static readonly AdminRouteDefinition[] RouterItems =
    [
        new()
        {
            Key = LoginKey,
            Path = "/login",
            Title = "Login"
        },
        new()
        {
            Key = LockKey,
            Path = "/lock",
            Title = "Lock"
        },
        new()
        {
            Key = InternalErrorKey,
            Path = InternalErrorPath,
            Title = "Internal Server Error"
        },
        .. Items,
        new()
        {
            Key = NotFoundKey,
            Path = NotFoundPath,
            Title = "Not Found"
        }
    ];

    public static AdminRouteDefinition[] CreateItems(AdminLanguage language)
        =>
        [
        new()
        {
            Key = DashboardKey,
            Path = "/",
            Title = Localization.Get(language, TextKey.Dashboard),
            Subtitle = Localization.Get(language, TextKey.DashboardSubtitle)
        },
        new()
        {
            Key = "operations",
            Title = Localization.Get(language, TextKey.Operations),
            Children =
            [
                new()
                {
                    Key = ReleasesKey,
                    Path = "/operations/releases",
                    Title = Localization.Get(language, TextKey.Releases),
                    Subtitle = Localization.Get(language, TextKey.ReleasesSubtitle)
                },
                new()
                {
                    Key = AuditKey,
                    Path = "/operations/audit",
                    Title = Localization.Get(language, TextKey.AuditLog),
                    Subtitle = Localization.Get(language, TextKey.AuditLogSubtitle)
                }
            ]
        },
        new()
        {
            Key = SettingsKey,
            Path = "/settings",
            Title = Localization.Get(language, TextKey.Settings),
            Subtitle = Localization.Get(language, TextKey.SettingsSubtitle)
        },
        new()
        {
            Key = WorkspaceKey,
            Path = "/workspace",
            Title = Localization.Get(language, TextKey.Workspace),
            Subtitle = Localization.Get(language, TextKey.WorkspaceSubtitle)
        }
        ];

    public static readonly AdminNavItems NavigationItems =
        AdminRouteCatalog.BuildNavigation(Items);
}
