// Defines the scheme selector that accepts either the local Identity cookie or an OpenIddict bearer token.
// 定义认证方案选择器，使同一 API 同时支持本地 Identity Cookie 与 OpenIddict Bearer Token。
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Validation.AspNetCore;

namespace JazorAdmin.Authentication;

public static class JazorAdminAuthentication
{
    public const string ApiScheme = "JazorAdmin.Api";

    public static AuthenticationBuilder AddJazorAdminAuthentication(this IServiceCollection services)
        => services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiScheme;
                options.DefaultChallengeScheme = ApiScheme;
            })
            .AddPolicyScheme(ApiScheme, ApiScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                    context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
                        : IdentityConstants.ApplicationScheme;
            });
}
