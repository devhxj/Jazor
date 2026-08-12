using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

/// <summary>Provides development-only Jazor reload service registration and middleware.</summary>
public static class JazorReloadExtensions
{
    private const string MiddlewareRegisteredKey = "__JazorReloadMiddlewareRegistered";

    /// <summary>Registers the development reload endpoints and HTML injection middleware.</summary>
    public static IApplicationBuilder UseJazorReload(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Hosts may compose this extension through multiple packages; register once per pipeline.
        if (app.Properties.ContainsKey(MiddlewareRegisteredKey))
            return app;

        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        // Never expose the reload transport in a production request pipeline.
        if (!string.Equals(environment.EnvironmentName, Environments.Development, StringComparison.OrdinalIgnoreCase))
            return app;

        var service = app.ApplicationServices.GetService<ReloadService>()
            ?? throw new InvalidOperationException(
                "Jazor reload services are not registered. Call AddJazorReload() during service configuration.");

        var options = service.Options;

        app.Properties[MiddlewareRegisteredKey] = true;
        app.UseWebSockets(new WebSocketOptions
        {
            KeepAliveInterval = options.KeepAliveInterval
        });

        app.Map(
            options.ClientScriptPath.Value!,
            branch => branch.Run(context => HandleClientScriptRequestAsync(context, service)));

        app.Map(
            options.WebSocketPath.Value!,
            branch => branch.Run(context => HandleWebSocketRequestAsync(context, service)));

        if (options.InjectHtml)
        {
            app.UseMiddleware<ReloadHtmlMiddleware>();
        }

        return app;
    }

    private static Task HandleClientScriptRequestAsync(
        HttpContext context,
        ReloadService service)
    {
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
        {
            context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            return Task.CompletedTask;
        }

        context.Response.ContentType = "text/javascript; charset=utf-8";
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";
        context.Response.Headers.XContentTypeOptions = "nosniff";

        if (HttpMethods.IsHead(context.Request.Method))
            return Task.CompletedTask;

        return context.Response.WriteAsync(service.ClientScriptContent, context.RequestAborted);
    }

    private static async Task HandleWebSocketRequestAsync(
        HttpContext context,
        ReloadService service)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        await service.AcceptWebSocketAsync(socket, context.RequestAborted);
    }
    /// <summary>Adds development reload services with the default configuration.</summary>
    public static IServiceCollection AddJazorReload(this IServiceCollection services)
        => services.AddJazorReload(configure: null);

    /// <summary>Adds development reload services with explicit configuration.</summary>
    public static IServiceCollection AddJazorReload(
        this IServiceCollection services,
        Action<JazorReloadOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<JazorReloadOptions>();
        if (configure is not null)
            optionsBuilder.Configure(configure);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<JazorReloadOptions>, ReloadOptionsValidator>());
        services.TryAddSingleton<IReloadRuntimeSignals, RuntimeEnvironmentSignals>();
        services.TryAddSingleton<ReloadService>();
        services.TryAddSingleton<IHostedService>(static serviceProvider => serviceProvider.GetRequiredService<ReloadService>());
        return services;
    }
}
