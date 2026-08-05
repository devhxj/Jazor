// Declares OpenIddict client and scope administration contracts.
// 定义 OpenIddict 客户端与 Scope 管理契约。
namespace JazorAdmin.Features.Configuration;

public sealed record CreateClientRequest(
    string ClientId,
    string DisplayName,
    string[] RedirectUris,
    string[] PostLogoutRedirectUris,
    string[] Scopes);

public sealed record OpenIdClientResponse(
    string Id,
    string ClientId,
    string DisplayName,
    string[] RedirectUris,
    string[] PostLogoutRedirectUris,
    string[] Scopes);

public sealed record CreateScopeRequest(string Name, string DisplayName);

public sealed record OpenIdScopeResponse(
    string Id,
    string Name,
    string DisplayName,
    string[] Resources);
