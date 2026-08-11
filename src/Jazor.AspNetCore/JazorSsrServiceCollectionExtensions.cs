using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore;

public static class JazorSsrServiceCollectionExtensions
{
    /// <summary>Adds SSR services backed by the packaged DenoHost runtime.</summary>
    public static IServiceCollection AddJazorSsr(
        this IServiceCollection services,
        Action<JazorSsrOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = services.AddOptions<JazorSsrOptions>();
        if (configure is not null)
            options.Configure(configure);

        services.AddSingleton<JazorSsrArtifactLocator>();
        services.AddSingleton<IJazorSsrRenderer, JazorSsrRenderer>();
        return services;
    }
}
