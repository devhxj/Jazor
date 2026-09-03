// Defines the administration information architecture used by Vue Router and both navigation tiers.
// 定义 Vue Router 与两级导航共用的管理信息架构。
using ECMAScript;
using Jazor.Admin;

namespace JazorAdmin;

[ECMAScriptModule("components/routes.mjs")]
public static class Routes
{
    public const string DashboardKey = "dashboard";
    public const string OrganizationsKey = "organizations";
    public const string OrganizationStructureKey = "organizations.structure";
    public const string OrganizationMembersKey = "organizations.members";
    public const string AuthorizationKey = "authorization";
    public const string AuthorizationRolesKey = "authorization.roles";
    public const string AuthorizationResourcesKey = "authorization.resources";
    public const string AccountsKey = "accounts";
    public const string SsoKey = "sso";
    public const string SsoApplicationsKey = "sso.applications";
    public const string SsoScopesKey = "sso.scopes";
    public const string SsoAuthorizationsKey = "sso.authorizations";
    public const string SsoTokensKey = "sso.tokens";
    public const string SettingsKey = "settings";
    public const string SchedulesKey = "schedules";
    public const string AuditKey = "audit";
    public const string IamZoneKey = "iam";
    public const string OperationsZoneKey = "operations";
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

    // 两级导航按业务分区组织：IconBar 承载分区，次级菜单承载模块与页面。
    // 门户工作台、身份与访问、平台运营都走同一导航目录；后续新增模块只在对应分区扩展。
    public static AdminRouteDefinition[] CreateItems(AdminLanguage language)
        =>
        [
        new()
        {
            Key = DashboardKey,
            Path = "/",
            Icon = "dashboard",
            Title = Localization.Get(language, TextKey.Dashboard),
            Subtitle = Localization.Get(language, TextKey.DashboardSubtitle)
        },
        .. StarterCatalog.CreateRoutes(language),
        new()
        {
            Key = IamZoneKey,
            Icon = "secured",
            Title = Localization.Get(language, TextKey.IdentityAccessZone),
            Children =
            [
                new()
                {
                    Key = OrganizationsKey,
                    Icon = "tree-square-dot",
                    Title = Localization.Get(language, TextKey.Organizations),
                    Children =
                    [
                        new()
                        {
                            Key = OrganizationStructureKey,
                            Path = "/organizations/structure",
                            Title = Localization.Get(language, TextKey.OrganizationStructure),
                            Subtitle = Localization.Get(language, TextKey.OrganizationStructureSubtitle)
                        },
                        new()
                        {
                            Key = OrganizationMembersKey,
                            Path = "/organizations/members",
                            Title = Localization.Get(language, TextKey.Members),
                            Subtitle = Localization.Get(language, TextKey.MembersSubtitle)
                        }
                    ]
                },
                new()
                {
                    Key = AuthorizationKey,
                    Icon = "secured",
                    Title = Localization.Get(language, TextKey.Authorization),
                    Children =
                    [
                        new()
                        {
                            Key = AuthorizationRolesKey,
                            Path = "/authorization/roles",
                            Title = Localization.Get(language, TextKey.RolesAndGrants),
                            Subtitle = Localization.Get(language, TextKey.RolesAndGrantsSubtitle)
                        },
                        new()
                        {
                            Key = AuthorizationResourcesKey,
                            Path = "/authorization/resources",
                            Title = Localization.Get(language, TextKey.ResourceOperations),
                            Subtitle = Localization.Get(language, TextKey.ResourceOperationsSubtitle)
                        }
                    ]
                },
                new()
                {
                    Key = AccountsKey,
                    Path = "/accounts",
                    Icon = "usergroup",
                    Title = Localization.Get(language, TextKey.Accounts),
                    Subtitle = Localization.Get(language, TextKey.AccountsSubtitle)
                },
                new()
                {
                    Key = SsoKey,
                    Icon = "root-list",
                    Title = Localization.Get(language, TextKey.Sso),
                    Children =
                    [
                        new()
                        {
                            Key = SsoApplicationsKey,
                            Path = "/sso/applications",
                            Title = Localization.Get(language, TextKey.OpenIdApplications),
                            Subtitle = Localization.Get(language, TextKey.OpenIdApplicationsSubtitle)
                        },
                        new()
                        {
                            Key = SsoScopesKey,
                            Path = "/sso/scopes",
                            Title = Localization.Get(language, TextKey.OpenIdScopes),
                            Subtitle = Localization.Get(language, TextKey.OpenIdScopesSubtitle)
                        },
                        new()
                        {
                            Key = SsoAuthorizationsKey,
                            Path = "/sso/authorizations",
                            Title = Localization.Get(language, TextKey.OpenIdAuthorizations),
                            Subtitle = Localization.Get(language, TextKey.OpenIdAuthorizationsSubtitle)
                        },
                        new()
                        {
                            Key = SsoTokensKey,
                            Path = "/sso/tokens",
                            Title = Localization.Get(language, TextKey.OpenIdTokens),
                            Subtitle = Localization.Get(language, TextKey.OpenIdTokensSubtitle)
                        }
                    ]
                }
            ]
        },
        new()
        {
            Key = OperationsZoneKey,
            Icon = "setting",
            Title = Localization.Get(language, TextKey.PlatformOperationsZone),
            Children =
            [
                new()
                {
                    Key = SettingsKey,
                    Path = "/settings",
                    Icon = "setting",
                    Title = Localization.Get(language, TextKey.Settings),
                    Subtitle = Localization.Get(language, TextKey.SettingsSubtitle)
                },
                new()
                {
                    Key = SchedulesKey,
                    Path = "/schedules",
                    Icon = "time",
                    Title = Localization.Get(language, TextKey.Schedules),
                    Subtitle = Localization.Get(language, TextKey.SchedulesSubtitle)
                },
                new()
                {
                    Key = AuditKey,
                    Path = "/audit",
                    Icon = "file-paste",
                    Title = Localization.Get(language, TextKey.Audit),
                    Subtitle = Localization.Get(language, TextKey.AuditSubtitle)
                }
            ]
        },
        ];

    public static readonly AdminNavItems NavigationItems =
        AdminRouteCatalog.BuildNavigation(Items);
}
