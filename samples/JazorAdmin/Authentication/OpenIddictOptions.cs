// Binds the first-party RazorVue client registration without storing credentials in source control.
// 绑定第一方 RazorVue 客户端注册信息，不在源代码中存储任何凭据。
namespace JazorAdmin.Authentication;

public sealed class OpenIddictOptions
{
    public const string SectionName = "JazorAdmin:OpenIddict";

    public string ClientId { get; init; } = "jazoradmin-spa";

    public string[] RedirectUris { get; init; } = [];

    public string[] PostLogoutRedirectUris { get; init; } = [];
}
