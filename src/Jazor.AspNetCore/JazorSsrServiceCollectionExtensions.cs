using Microsoft.Extensions.DependencyInjection;

namespace Jazor.AspNetCore;

public static class JazorSSRServiceCollectionExtensions
{
    /// <summary>Adds SSR services backed by the packaged DenoHost runtime.</summary>
    public static IServiceCollection AddJazorSSR(
        this IServiceCollection services,
        Action<JazorSSROptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = services.AddOptions<JazorSSROptions>();
        if (configure is not null)
            options.Configure(configure);

        services.AddSingleton<JazorSSRArtifactLocator>();
        services.AddSingleton<IJazorSSRRenderer, JazorSSRRenderer>();
        return services;
    }
}
