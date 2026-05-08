using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jazor.AspNetCore.Dev;

public static class JazorDevelopmentReloadApplicationBuilderExtensions
{
    private const string MiddlewareRegisteredKey = "__JazorDevelopmentReloadMiddlewareRegistered";

    public static IApplicationBuilder UseJazorDevelopmentReload(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.Properties.ContainsKey(MiddlewareRegisteredKey))
            return app;

        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        if (!string.Equals(environment.EnvironmentName, Environments.Development, StringComparison.OrdinalIgnoreCase))
            return app;

        var service = app.ApplicationServices.GetService<JazorDevelopmentReloadService>()
            ?? throw new InvalidOperationException(
                "Jazor development reload services are not registered. Call AddJazorDevelopmentReload() during service configuration.");

        var options = service.Options;

        app.Properties[MiddlewareRegisteredKey] = true;
        app.UseWebSockets(new WebSocketOptions
        {
            KeepAliveInterval = options.WebSocketKeepAliveInterval
        });

        app.Map(
            options.ClientScriptPath.Value!,
            branch => branch.Run(context => HandleClientScriptRequestAsync(context, service)));

        app.Map(
            options.WebSocketPath.Value!,
            branch => branch.Run(context => HandleWebSocketRequestAsync(context, service)));

        if (options.InjectHtmlResponses)
        {
            app.UseMiddleware<JazorDevelopmentReloadHtmlMiddleware>();
        }

        return app;
    }

    private static Task HandleClientScriptRequestAsync(
        HttpContext context,
        JazorDevelopmentReloadService service)
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
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        if (HttpMethods.IsHead(context.Request.Method))
            return Task.CompletedTask;

        return context.Response.WriteAsync(service.ClientScriptContent, context.RequestAborted);
    }

    private static async Task HandleWebSocketRequestAsync(
        HttpContext context,
        JazorDevelopmentReloadService service)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        await service.AcceptWebSocketAsync(socket, context.RequestAborted);
    }
}
