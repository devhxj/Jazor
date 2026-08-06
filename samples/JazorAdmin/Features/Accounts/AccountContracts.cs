// Defines platform-account administration contracts without leaking Identity persistence models.
// 定义平台账户管理契约，避免将 Identity 持久化模型直接暴露给前端。
namespace JazorAdmin.Features.Accounts;

public sealed record CreateAccountRequest(
    string Email,
    string DisplayName,
    string Password,
    bool PlatformAdministrator);

public sealed record UpdateAccountStateRequest(bool Enabled);

public sealed record ResetAccountPasswordRequest(string Password);

public sealed record AccountResponse(
    string Id,
    string Email,
    string DisplayName,
    bool Enabled,
    bool PlatformAdministrator);
