


using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
namespace Jero.Core;

/// <summary>
/// 配置中间件扩展
/// </summary>
public static class UseApplicationBuilder
{
    // 配置限流中间件策略
    public static void UsePolicyRateLimit(this IApplicationBuilder app)
    {
        var ipPolicyStore = app.ApplicationServices.GetRequiredService<IIpPolicyStore>();
        ipPolicyStore.SeedAsync().GetAwaiter().GetResult();

        var clientPolicyStore = app.ApplicationServices.GetRequiredService<IClientPolicyStore>();
        clientPolicyStore.SeedAsync().GetAwaiter().GetResult();
    }
}