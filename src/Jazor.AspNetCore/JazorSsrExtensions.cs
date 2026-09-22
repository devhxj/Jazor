using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Jazor.AspNetCore;

/// <summary>Provides Jazor SSR service registration and middleware for generated Vue modules.</summary>
public static class JazorSsrExtensions
{
    /// <summary>Renders a fixed generated root component for eligible SPA navigation requests.</summary>
    /// <remarks>先调用 services.AddJazorSsr，并在此中间件之前托管同一套产物。复用 SPA fallback 的导航/404 规则；HEAD 不创建渲染请求。每次渲染创建 Vue app，Deno worker 持久复用。固定 request/props 被各请求共享；请求相关数据应使用 requestFactory。异常沿 ASP.NET Core 管线传播。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="modulePath">相对 SSR 产物根目录的生成模块路径，例如 components/app.js；不是浏览器 URL。</param>
    /// <param name="props">可由 System.Text.Json 序列化的组件 props；同样发送给浏览器 hydration。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSsr(
        this IApplicationBuilder app,
        string modulePath,
        object? props = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseJazorSsr(new JazorSsrRequest(modulePath, props));
    }

    /// <summary>Renders the supplied fixed request for eligible SPA navigation requests.</summary>
    /// <remarks>先调用 services.AddJazorSsr，并在此中间件之前托管同一套产物。复用 SPA fallback 的导航/404 规则；HEAD 不创建渲染请求。每次渲染创建 Vue app，Deno worker 持久复用。固定 request/props 被各请求共享；请求相关数据应使用 requestFactory。异常沿 ASP.NET Core 管线传播。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="request">根模块、props 和 provider 快照；HTTP 固定请求重载会跨请求复用此对象。</param>
    /// <param name="configure">配置此入口的选项；允许 null 的重载使用默认值。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSsr(
        this IApplicationBuilder app,
        JazorSsrRequest request,
        Action<JazorSpaFallbackOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(request);
        return app.UseJazorSsr(
            static (_, _, state) => Task.FromResult(state),
            request,
            configure);
    }

    /// <summary>Renders a request-specific generated root component for eligible SPA navigation requests.</summary>
    /// <remarks>先调用 services.AddJazorSsr，并在此中间件之前托管同一套产物。复用 SPA fallback 的导航/404 规则；HEAD 不创建渲染请求。每次渲染创建 Vue app，Deno worker 持久复用。固定 request/props 被各请求共享；请求相关数据应使用 requestFactory。异常沿 ASP.NET Core 管线传播。</remarks>
    /// <param name="app">要注册中间件的应用管线。</param>
    /// <param name="requestFactory">按请求创建 SSR 输入的工厂，可读取 HttpContext；取消令牌为 RequestAborted。</param>
    /// <param name="configure">配置此入口的选项；允许 null 的重载使用默认值。</param>
    /// <returns>原应用管线，供继续注册中间件。</returns>
    public static IApplicationBuilder UseJazorSsr(
        this IApplicationBuilder app,
        Func<HttpContext, CancellationToken, Task<JazorSsrRequest>> requestFactory,
        Action<JazorSpaFallbackOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(requestFactory);
        return app.UseJazorSsr(
            static (context, cancellationToken, state) => state(context, cancellationToken),
            requestFactory,
            configure);
    }

    private static IApplicationBuilder UseJazorSsr<TState>(
        this IApplicationBuilder app,
        Func<HttpContext, CancellationToken, TState, Task<JazorSsrRequest>> requestFactory,
        TState state,
        Action<JazorSpaFallbackOptions>? configure)
        => app.UseJazorSpaFallback(
            async (context, cancellationToken) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                if (HttpMethods.IsHead(context.Request.Method))
                    return;

                var services = context.RequestServices;
                var renderer = services.GetService<IJazorSsrRenderer>()
                    ?? throw new InvalidOperationException(
                        "Jazor SSR requires AddJazorSsr() before UseJazorSsr().");
                var artifactLocator = services.GetService<SsrArtifactLocator>()
                    ?? throw new InvalidOperationException(
                        "Jazor SSR artifact services were not registered. Call AddJazorSsr() before UseJazorSsr().");
                var options = services.GetService<IOptions<JazorSsrOptions>>()?.Value
                    ?? throw new InvalidOperationException(
                        "Jazor SSR options were not registered. Call AddJazorSsr() before UseJazorSsr().");
                var request = await requestFactory(context, cancellationToken, state).ConfigureAwait(false);
                // Render before composing the shell so any SSR failure preserves normal ASP.NET Core error handling.
                var result = await renderer.RenderAsync(request, cancellationToken).ConfigureAwait(false);
                var artifactGraph = artifactLocator.Resolve();
                await SsrDocumentWriter.WriteAsync(
                    context,
                    artifactGraph,
                    options,
                    result,
                    cancellationToken).ConfigureAwait(false);
            },
            configure);
    // Keep DI and pipeline entry points together: callers only need one SSR extension surface.
    /// <summary>Adds SSR services backed by the packaged DenoHost runtime.</summary>
    /// <remarks>注册由 DI 管理生命周期的单例 renderer 与产物定位服务。Deno worker 按需创建并受 WorkerCount 限制；此方法不注册 HTTP 中间件。</remarks>
    /// <param name="services">构建宿主前配置的服务集合。</param>
    /// <param name="configure">配置此入口的选项；允许 null 的重载使用默认值。</param>
    /// <returns>原服务集合，供继续注册服务。</returns>
    public static IServiceCollection AddJazorSsr(
        this IServiceCollection services,
        Action<JazorSsrOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = services.AddOptions<JazorSsrOptions>();
        options.Validate(
            static value => value.WorkerCount > 0,
            "Jazor SSR WorkerCount must be greater than zero.");
        if (configure is not null)
            options.Configure(configure);

        services.AddSingleton<SsrArtifactLocator>();
        services.AddSingleton<IJazorSsrRenderer, SsrRenderer>();
        return services;
    }
}

/// <summary>Composes the browser hydration document around an already-rendered Vue root.</summary>
internal static class SsrDocumentWriter
{
    private const string StateElementId = "__jazor_ssr_state";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Default
    };

    public static async Task WriteAsync(
        HttpContext context,
        SsrArtifacts artifactGraph,
        JazorSsrOptions options,
        JazorSsrRenderResult result,
        CancellationToken cancellationToken)
    {
        var mountElementId = NormalizeMountElementId(options.MountElementId);
        var mountElementIdJson = JsonSerializer.Serialize(mountElementId, JsonOptions);
        var hydrationUrl = SsrArtifactLocator.CreateBrowserArtifactUrl(
            artifactGraph,
            context.Request.PathBase,
            options.HydrationEntryPath);
        var hydrationUrlJson = JsonSerializer.Serialize(hydrationUrl, JsonOptions);
        var modulePathJson = JsonSerializer.Serialize(result.ModulePath, JsonOptions);
        var response = context.Response;

        await response.WriteAsync("<!doctype html>\n<html><head><meta charset=\"utf-8\">\n", cancellationToken);
        await response.WriteAsync("</head><body>\n<div id=\"", cancellationToken);
        await response.WriteAsync(HtmlEncoder.Default.Encode(mountElementId), cancellationToken);
        await response.WriteAsync("\">", cancellationToken);
        await response.WriteAsync(result.Html, cancellationToken);
        await response.WriteAsync("</div>\n<script id=\"" + StateElementId + "\" type=\"application/json\">", cancellationToken);
        await response.WriteAsync(result.SerializedState, cancellationToken);
        await response.WriteAsync("</script>\n<script type=\"module\">\n", cancellationToken);
        await response.WriteAsync("import { hydrate } from " + hydrationUrlJson + ";\n", cancellationToken);
        await response.WriteAsync("await hydrate(" + modulePathJson + ", " + mountElementIdJson + ");\n", cancellationToken);
        await response.WriteAsync("</script>\n</body></html>", cancellationToken);
    }

    private static string NormalizeMountElementId(string mountElementId)
    {
        if (string.IsNullOrWhiteSpace(mountElementId))
            throw new ArgumentException("Jazor SSR mount element id cannot be empty.", nameof(mountElementId));

        var normalized = mountElementId.Trim();
        if (normalized.Any(char.IsWhiteSpace))
            throw new ArgumentException("Jazor SSR mount element id cannot contain whitespace.", nameof(mountElementId));

        return normalized;
    }
}
