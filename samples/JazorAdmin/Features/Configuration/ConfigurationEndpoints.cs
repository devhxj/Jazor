// Maps the OpenIddict administration resources behind one platform-admin boundary.
// 统一映射 OpenIddict 应用、Scope、授权和令牌管理资源。
using JazorAdmin.Authorization;

namespace JazorAdmin.Features.Configuration;

public static class ConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var configuration = app.MapGroup("/api/configuration")
            .WithTags("Configuration")
            .RequireAuthorization(JazorAdminPolicies.PlatformAdministrator);

        AppEndpoints.Map(configuration);
        ScopeEndpoints.Map(configuration);
        GrantEndpoints.Map(configuration);
        return app;
    }
}
