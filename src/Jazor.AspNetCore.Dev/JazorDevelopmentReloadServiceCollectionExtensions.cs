using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

public static class JazorDevelopmentReloadServiceCollectionExtensions
{
    public static IServiceCollection AddJazorDevelopmentReload(this IServiceCollection services)
        => services.AddJazorDevelopmentReload(configure: null);

    public static IServiceCollection AddJazorDevelopmentReload(
        this IServiceCollection services,
        Action<JazorDevelopmentReloadOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<JazorDevelopmentReloadOptions>();
        if (configure is not null)
            optionsBuilder.Configure(configure);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<JazorDevelopmentReloadOptions>, JazorDevelopmentReloadOptionsValidator>());
        services.TryAddSingleton<IJazorDevelopmentRuntimeSignals, JazorDevelopmentRuntimeEnvironmentSignals>();
        services.TryAddSingleton<JazorDevelopmentReloadService>();
        services.TryAddSingleton<IHostedService>(static serviceProvider => serviceProvider.GetRequiredService<JazorDevelopmentReloadService>());
        return services;
    }
}
