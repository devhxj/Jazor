// Declares stable HTTP contracts for local session login, logout, and identity discovery.
// 声明本地会话登录、登出和身份发现所使用的稳定 HTTP 契约。
namespace JazorAdmin.Features.Identity;

public sealed record LoginRequest(
    string Email,
    string Password,
    bool RememberMe = false,
    string? CaptchaId = null,
    string? CaptchaAnswer = null);

public sealed record CaptchaChallengeResponse(string Id, string ImageUrl);

public sealed record OrganizationSummary(string Id, string Code, string DisplayName);

public sealed record SessionResponse(
    string UserId,
    string Email,
    string DisplayName,
    string[] Roles,
    OrganizationSummary[] Organizations);
