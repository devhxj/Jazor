// Maps the OpenIddict administration resources behind one platform-admin boundary.
// 统一映射 OpenIddict 应用、Scope、授权和令牌管理资源。
using JazorAdmin.Authorization;

namespace JazorAdmin.Features.Sso;

public static class SsoEndpoints
{
    public static IEndpointRouteBuilder MapSsoEndpoints(this IEndpointRouteBuilder app)
    {
        var sso = app.MapGroup("/api/sso")
            .WithTags("SSO")
            .RequireAuthorization(PolicyKeys.PlatformAdministrator);

        AppEndpoints.Map(sso);
        ScopeEndpoints.Map(sso);
        GrantEndpoints.Map(sso);
        return app;
    }
}
