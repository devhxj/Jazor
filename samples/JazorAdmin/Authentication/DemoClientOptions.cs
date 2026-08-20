// Describes the separately hosted confidential RazorVue reference client. Secrets are supplied
// by user-secrets/deployment configuration and never live in the sample source tree.
// 描述独立宿主的 confidential RazorVue 参考客户端；密钥只从 user-secrets/部署配置读取，绝不写入示例源码。
namespace JazorAdmin.Authentication;

public sealed class DemoClientOptions
{
    public const string SectionName = "JazorAdmin:DemoClient";

    public string ClientId { get; init; } = "jazoradmin-demo-client";

    public string? ClientSecret { get; init; }

    public string[] RedirectUris { get; init; } = [];

    public string[] PostLogoutRedirectUris { get; init; } = [];

    // The optional confidential client is enabled only when it can actually be registered. A launch
    // URL alone is not enough: otherwise the portal could expose a dead application entry.
    // 可选 confidential client 只有在可实际注册时才启用；仅有启动 URL 不足以展示入口，避免门户
    // 暴露无法登录的应用链接。
    public bool IsRegistrationEnabled =>
        !string.IsNullOrWhiteSpace(ClientSecret) &&
        RedirectUris.Length > 0 &&
        PostLogoutRedirectUris.Length > 0;

    // The portal only renders a launch entry for a configured registration with a reachable origin.
    // 门户仅在客户端已注册且配置了可达源时展示启动入口。
    public string? LaunchUri { get; init; }

    public bool HasPortalLaunch =>
        IsRegistrationEnabled && !string.IsNullOrWhiteSpace(LaunchUri);
}
