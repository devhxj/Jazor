namespace JazorAdmin.Features.Sso;

public sealed record AppCreate(
    string ClientId,
    string DisplayName,
    string ApplicationType,
    string ClientType,
    string ConsentType,
    bool RequirePkce,
    string[] RedirectUris,
    string[] PostLogoutRedirectUris,
    string[] Endpoints,
    string[] GrantTypes,
    string[] ResponseTypes,
    string[] Scopes);

public sealed record AppUpdate(
    string DisplayName,
    string ApplicationType,
    string ClientType,
    string ConsentType,
    bool RequirePkce,
    string[] RedirectUris,
    string[] PostLogoutRedirectUris,
    string[] Endpoints,
    string[] GrantTypes,
    string[] ResponseTypes,
    string[] Scopes);

public sealed record AppView(
    string Id,
    string ClientId,
    string DisplayName,
    string Profile,
    string ApplicationType,
    string ClientType,
    string ConsentType,
    bool RequirePkce,
    string[] RedirectUris,
    string[] PostLogoutRedirectUris,
    string[] Endpoints,
    string[] GrantTypes,
    string[] ResponseTypes,
    string[] Scopes);

public sealed record AppSaved(AppView App, string? Secret);

public sealed record SecretView(string Secret);
