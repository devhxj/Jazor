


using Jero.Core;
using AspNetCoreRateLimit;
using Furion;
using Microsoft.Extensions.DependencyInjection;
namespace Jero.Web;

public static class ProjectOptions
{
    /// <summary>
    /// 将应用配置绑定到各模块的选项类型，并分别绑定 IP 和客户端限流的默认配置与策略表。
    /// </summary>
    public static IServiceCollection AddProjectOptions(this IServiceCollection services)
    {
        services.AddConfigurableOptions<DbConnectionOptions>();
        services.AddConfigurableOptions<SnowIdOptions>();
        services.AddConfigurableOptions<CacheOptions>();
        services.AddConfigurableOptions<ClusterOptions>();
        services.AddConfigurableOptions<OSSProviderOptions>();
        services.AddConfigurableOptions<UploadOptions>();
        services.AddConfigurableOptions<WechatOptions>();
        services.AddConfigurableOptions<WechatPayOptions>();
        services.AddConfigurableOptions<PayCallBackOptions>();
        services.AddConfigurableOptions<EnumOptions>();
        services.AddConfigurableOptions<APIJSONOptions>();
        services.AddConfigurableOptions<EmailOptions>();
        services.AddConfigurableOptions<OAuthOptions>();
        services.AddConfigurableOptions<CryptogramOptions>();
        services.AddConfigurableOptions<SMSOptions>();
        services.AddConfigurableOptions<EventBusOptions>();
        services.AddConfigurableOptions<AlipayOptions>();
        services.AddConfigurableOptions<CDConfigOptions>();
        services.AddConfigurableOptions<DeepSeekOptions>();
        services.AddConfigurableOptions<LocalizationSettingsOptions>();
        services.Configure<IpRateLimitOptions>(Furion.App.Configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(Furion.App.Configuration.GetSection("IpRateLimitPolicies"));
        services.Configure<ClientRateLimitOptions>(Furion.App.Configuration.GetSection("ClientRateLimiting"));
        services.Configure<ClientRateLimitPolicies>(Furion.App.Configuration.GetSection("ClientRateLimitPolicies"));

        return services;
    }
}
