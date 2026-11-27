using System.Net;
using Jazor.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

/// <summary>Registers and serves a Jolt SPA or a Jazor ES module project.</summary>
public static class JazorSPAExtensions
{
    private const string PathBaseRegisteredKey = "__JazorSPAPathBaseRegistered";
    private const string SpaRegisteredKey = "__JazorSPARegistered";

    /// <summary>Registers SPA hosting and the project's development server.</summary>
    public static WebApplicationBuilder AddJazorSPA(
        this WebApplicationBuilder builder,
        Action<JazorSPAOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddJazorSPA(configure);
        return builder;
    }

    /// <summary>Registers the generated frontend for hosts that compose services through Startup or a framework.</summary>
    public static IServiceCollection AddJazorSPA(
        this IServiceCollection services,
        Action<JazorSPAOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = services.AddOptions<JazorSPAOptions>()
            .Configure<IWebHostEnvironment>((settings, environment) => settings.ApplyApplicationDefaults(environment.ApplicationName));
        if (configure is not null)
            options.Configure(configure);
        options.ValidateOnStart();

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<JazorSPAOptions>, JazorSPAOptionsValidator>());
        services.TryAddSingleton<JazorSPARegistration>();
        services.TryAddSingleton<JazorSPAAssets>();
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

        var options = app.ApplicationServices.GetRequiredService<IOptions<JazorSPAOptions>>().Value;
        if (options.PathBase.HasValue)
            app.UsePathBase(options.PathBase);
        return app;
    }

    /// <summary>Proxies the generated project in Development and serves its built artifacts elsewhere.</summary>
    public static WebApplication UseJazorSPA(
        this WebApplication app,
        Action<JazorHostOptions>? configure = null)
    {
        ((IApplicationBuilder)app).UseJazorSPA(configure);
        return app;
    }

    /// <summary>Composes the development proxy or release artifact host in the middleware pipeline supplied by the host.</summary>
    /// <remarks>Startup and framework integrations must call this on their supplied builder so registration enters the executing pipeline.</remarks>
    public static IApplicationBuilder UseJazorSPA(
        this IApplicationBuilder app,
        Action<JazorHostOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        EnsureRegistered(app);
        var properties = app.Properties;
        if (properties.ContainsKey(SpaRegisteredKey))
            return app;
        properties[SpaRegisteredKey] = true;

        var options = app.ApplicationServices.GetRequiredService<IOptions<JazorSPAOptions>>().Value;
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        app.UseJazorHost(hostOptions =>
        {
            if (options.IsJoltApp)
            {
                // The shell serves only Vite's build output. The project directory contains server sources.
                hostOptions.Assets.ServeDefaultFiles = false;
                if (!environment.IsDevelopment() && !options.RequestPath.HasValue &&
                    string.Equals(options.ResolveReleaseRoot(environment.ContentRootPath, environment.WebRootPath), environment.WebRootPath, StringComparison.Ordinal))
                    hostOptions.Assets.ServeWebRoot = false;
                configure?.Invoke(hostOptions);
                hostOptions.Assets.ServeArtifacts = false;
                return;
            }

            hostOptions.Assets.ServeArtifacts = !environment.IsDevelopment();
            hostOptions.Assets.ArtifactProbeRelativePaths.Clear();
            hostOptions.Assets.ArtifactProbeRelativePaths.Add(options.GetReleaseEntryName());

            configure?.Invoke(hostOptions);

            var configureArtifacts = hostOptions.Assets.ConfigureArtifacts;
            hostOptions.Assets.ConfigureArtifacts = artifactOptions =>
            {
                artifactOptions.RequestPath = options.RequestPath;
                artifactOptions.RootPath = options.ResolveReleaseRoot(environment.ContentRootPath, environment.WebRootPath);
                configureArtifacts?.Invoke(artifactOptions);
            };
        });

        if (environment.IsDevelopment())
        {
            // Keep the proxy behind the shared host pipeline so development responses receive
            // the same configured security headers as release assets and application routes.
            app.UseWebSockets();
            app.Map(options.GetDevelopmentRequestPath(), branch =>
                branch.Run(context => JazorViteProxy.ForwardAsync(context, options)));
        }

        if (options.IsJoltApp)
            JazorSPAShell.Use(app, options, environment);

        return app;
    }

    private static void EnsureRegistered(IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetService<JazorSPARegistration>() is null)
        {
            throw new InvalidOperationException(
                "Register Jazor SPA hosting with builder.AddJazorSPA(...) or services.AddJazorSPA(...) before building the application.");
        }
    }

    private sealed class JazorSPARegistration;
}
