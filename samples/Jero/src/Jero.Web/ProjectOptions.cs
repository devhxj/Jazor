


using Jero.Core;
using AspNetCoreRateLimit;
using Furion;
using Microsoft.Extensions.DependencyInjection;
namespace Jero.Web;

public static class ProjectOptions
{
    /// <summary>
    /// 注册项目配置选项
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddProjectOptions(this IServiceCollection services)
    {
        services.AddConfigurableOptions<SpaOptions>();
        services.AddConfigurableOptions<DbConnectionOptions>();
        services.AddConfigurableOptions<SnowIdOptions>();
        services.AddConfigurableOptions<CacheOptions>();
        services.AddConfigurableOptions<ClusterOptions>();
        services.AddConfigurableOptions<OSSProviderOptions>();
        services.AddConfigurableOptions<UploadOptions>();
        services.AddConfigurableOptions<WechatOptions>();
        services.AddConfigurableOptions<WechatPayOptions>();
        services.AddConfigurableOptions<PayCallBackOptions>();
        services.AddConfigurableOptions<CodeGenOptions>();
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
