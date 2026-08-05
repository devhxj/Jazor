// Defines the local account aggregate used by ASP.NET Core Identity and OpenIddict subjects.
// 定义由 ASP.NET Core Identity 与 OpenIddict subject 共用的本地账户聚合。
using Microsoft.AspNetCore.Identity;

namespace JazorAdmin.Data;

public sealed class JazorAdminUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
