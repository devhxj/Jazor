using System.Net;
using Jazor.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

/// <summary>Registers and composes the generated Jazor frontend for development and deployment.</summary>
public static class JazorFrontendExtensions
{
    private const string PathBaseRegisteredKey = "__JazorFrontendPathBaseRegistered";
    private const string FrontendRegisteredKey = "__JazorFrontendRegistered";

    /// <summary>Registers the generated frontend and its DenoHost-owned Vite development server.</summary>
    public static WebApplicationBuilder AddJazorFrontend(
        this WebApplicationBuilder builder,
        Action<JazorFrontendOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddJazorFrontend(configure);
        return builder;
    }

    /// <summary>Registers the generated frontend for hosts that compose services through Startup or a framework.</summary>
    public static IServiceCollection AddJazorFrontend(
        this IServiceCollection services,
        Action<JazorFrontendOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = services.AddOptions<JazorFrontendOptions>();
        if (configure is not null)
            options.Configure(configure);
        options.ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<JazorFrontendOptions>, JazorFrontendOptionsValidator>());
        services.TryAddSingleton<JazorFrontendRegistration>();
        services.AddHttpClient(JazorViteDevelopmentServer.HttpClientName, client =>
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.None
            });
        services.AddHostedService<JazorViteDevelopmentServer>();
        return services;
    }

    /// <summary>Applies the configured public path base at the caller-selected point in the middleware pipeline.</summary>
    public static WebApplication UseJazorPathBase(this WebApplication app)
    {
        ((IApplicationBuilder)app).UseJazorPathBase();
        return app;
    }

    /// <summary>Applies the configured public path base to the middleware pipeline supplied by the host.</summary>
    public static IApplicationBuilder UseJazorPathBase(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        EnsureRegistered(app);
        var properties = app.Properties;
        if (properties.ContainsKey(PathBaseRegisteredKey))
            return app;
        properties[PathBaseRegisteredKey] = true;

        var options = app.ApplicationServices.GetRequiredService<IOptions<JazorFrontendOptions>>().Value;
        if (options.PathBase.HasValue)
            app.UsePathBase(options.PathBase);
        return app;
    }

    /// <summary>Proxies the generated project in Development and serves its built artifacts elsewhere.</summary>
    public static WebApplication UseJazorFrontend(
        this WebApplication app,
        Action<JazorHostOptions>? configure = null)
    {
        ((IApplicationBuilder)app).UseJazorFrontend(configure);
        return app;
    }

    /// <summary>Composes the development proxy or release artifact host in the middleware pipeline supplied by the host.</summary>
    /// <remarks>Startup and framework integrations must call this on their supplied builder so registration enters the executing pipeline.</remarks>
    public static IApplicationBuilder UseJazorFrontend(
        this IApplicationBuilder app,
        Action<JazorHostOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        EnsureRegistered(app);
        var properties = app.Properties;
        if (properties.ContainsKey(FrontendRegisteredKey))
            return app;
        properties[FrontendRegisteredKey] = true;

        var options = app.ApplicationServices.GetRequiredService<IOptions<JazorFrontendOptions>>().Value;
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        app.UseJazorHost(hostOptions =>
        {
            hostOptions.Assets.ServeArtifacts = !environment.IsDevelopment();
            hostOptions.Assets.ArtifactProbeRelativePaths.Clear();
            hostOptions.Assets.ArtifactProbeRelativePaths.Add(options.DevelopmentEntryRelativePath);
            if (!string.Equals(
                    options.ReleaseEntryRelativePath,
                    options.DevelopmentEntryRelativePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                hostOptions.Assets.ArtifactProbeRelativePaths.Add(options.ReleaseEntryRelativePath);
            }

            configure?.Invoke(hostOptions);

            var configureArtifacts = hostOptions.Assets.ConfigureArtifacts;
            hostOptions.Assets.ConfigureArtifacts = artifactOptions =>
            {
                artifactOptions.RequestPath = options.RequestPath;
                artifactOptions.RootPath = options.ResolveProjectRoot(environment.ContentRootPath);
                configureArtifacts?.Invoke(artifactOptions);
            };
        });

        if (environment.IsDevelopment())
        {
            // Keep the proxy behind the shared host pipeline so development responses receive
            // the same configured security headers as release assets and application routes.
            app.UseWebSockets();
            app.Map(options.RequestPath, branch =>
                branch.Run(context => JazorViteProxy.ForwardAsync(context, options)));
        }

        return app;
    }

    private static void EnsureRegistered(IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetService<JazorFrontendRegistration>() is null)
        {
            throw new InvalidOperationException(
                "Register the Jazor frontend with builder.AddJazorFrontend(...) or services.AddJazorFrontend(...) before building the application.");
        }
    }

    private sealed class JazorFrontendRegistration;
}
