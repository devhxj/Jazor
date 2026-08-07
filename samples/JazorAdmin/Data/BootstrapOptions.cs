// Declares the first platform administrator supplied by the deployment configuration.
// 声明由部署配置提供的首个平台注册管理员，不将生产凭据写入应用代码。
namespace JazorAdmin.Data;

public sealed class BootstrapOptions
{
    public const string SectionName = "JazorAdmin:Bootstrap";

    public string? Email { get; init; }

    public string? Password { get; init; }

    public string DisplayName { get; init; } = "Platform Administrator";
}
