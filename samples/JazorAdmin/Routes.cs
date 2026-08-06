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
    public const string ConfigurationKey = "configuration";
    public const string ConfigurationApplicationsKey = "configuration.applications";
    public const string ConfigurationScopesKey = "configuration.scopes";
    public const string ConfigurationAuthorizationsKey = "configuration.authorizations";
    public const string ConfigurationTokensKey = "configuration.tokens";
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
            Icon = "dashboard",
            Title = Localization.Get(language, TextKey.Dashboard),
            Subtitle = Localization.Get(language, TextKey.DashboardSubtitle)
        },
        new()
        {
            Key = OrganizationsKey,
            Icon = "organizations",
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
            Icon = "authorization",
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
            Icon = "accounts",
            Title = Localization.Get(language, TextKey.Accounts),
            Subtitle = Localization.Get(language, TextKey.AccountsSubtitle)
        },
        new()
        {
            Key = ConfigurationKey,
            Icon = "configuration",
            Title = Localization.Get(language, TextKey.Configuration),
            Children =
            [
                new()
                {
                    Key = ConfigurationApplicationsKey,
                    Path = "/configuration/applications",
                    Title = Localization.Get(language, TextKey.OpenIdApplications),
                    Subtitle = Localization.Get(language, TextKey.OpenIdApplicationsSubtitle)
                },
                new()
                {
                    Key = ConfigurationScopesKey,
                    Path = "/configuration/scopes",
                    Title = Localization.Get(language, TextKey.OpenIdScopes),
                    Subtitle = Localization.Get(language, TextKey.OpenIdScopesSubtitle)
                },
                new()
                {
                    Key = ConfigurationAuthorizationsKey,
                    Path = "/configuration/authorizations",
                    Title = Localization.Get(language, TextKey.OpenIdAuthorizations),
                    Subtitle = Localization.Get(language, TextKey.OpenIdAuthorizationsSubtitle)
                },
                new()
                {
                    Key = ConfigurationTokensKey,
                    Path = "/configuration/tokens",
                    Title = Localization.Get(language, TextKey.OpenIdTokens),
                    Subtitle = Localization.Get(language, TextKey.OpenIdTokensSubtitle)
                }
            ]
        },
        ];

    public static readonly AdminNavItems NavigationItems =
        AdminRouteCatalog.BuildNavigation(Items);
}
