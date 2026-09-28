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

        var options = builder.Services.AddOptions<JazorFrontendOptions>();
        if (configure is not null)
            options.Configure(configure);
        options.ValidateOnStart();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<JazorFrontendOptions>, JazorFrontendOptionsValidator>());
        builder.Services.TryAddSingleton<JazorFrontendRegistration>();
        builder.Services.AddHttpClient(JazorViteDevelopmentServer.HttpClientName, client =>
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.None
            });
        builder.Services.AddHostedService<JazorViteDevelopmentServer>();
        return builder;
    }

    /// <summary>Applies the configured public path base at the caller-selected point in the middleware pipeline.</summary>
    public static WebApplication UseJazorPathBase(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        EnsureRegistered(app);
        var properties = ((IApplicationBuilder)app).Properties;
        if (properties.ContainsKey(PathBaseRegisteredKey))
            return app;
        properties[PathBaseRegisteredKey] = true;

        var options = app.Services.GetRequiredService<IOptions<JazorFrontendOptions>>().Value;
        if (options.PathBase.HasValue)
            app.UsePathBase(options.PathBase);
        return app;
    }

    /// <summary>Proxies the generated project in Development and serves its built artifacts elsewhere.</summary>
    public static WebApplication UseJazorFrontend(
        this WebApplication app,
        Action<JazorHostOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        EnsureRegistered(app);
        var properties = ((IApplicationBuilder)app).Properties;
        if (properties.ContainsKey(FrontendRegisteredKey))
            return app;
        properties[FrontendRegisteredKey] = true;

        var options = app.Services.GetRequiredService<IOptions<JazorFrontendOptions>>().Value;
        if (app.Environment.IsDevelopment())
        {
            app.UseWebSockets();
            app.Map(options.RequestPath, branch =>
                branch.Run(context => JazorViteProxy.ForwardAsync(context, options)));
        }

        app.UseJazorHost(hostOptions =>
        {
            hostOptions.Assets.ServeArtifacts = !app.Environment.IsDevelopment();
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
                artifactOptions.RootPath = options.ResolveProjectRoot(app.Environment.ContentRootPath);
                configureArtifacts?.Invoke(artifactOptions);
            };
        });
        return app;
    }

    private static void EnsureRegistered(WebApplication app)
    {
        if (app.Services.GetService<JazorFrontendRegistration>() is null)
        {
            throw new InvalidOperationException(
                "Register the Jazor frontend with builder.AddJazorFrontend(...) before building the application.");
        }
    }

    private sealed class JazorFrontendRegistration;
}
