using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Jazor.AspNetCore;

public static class JazorSSRApplicationBuilderExtensions
{
    /// <summary>Uses one fixed generated root component for all SPA-fallback navigation requests.</summary>
    public static IApplicationBuilder UseJazorSSR(
        this IApplicationBuilder app,
        string modulePath,
        object? props = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseJazorSSR(new JazorSSRRequest(modulePath, props));
    }

    /// <summary>Uses one fixed generated root component for all SPA-fallback navigation requests.</summary>
    public static IApplicationBuilder UseJazorSSR(
        this IApplicationBuilder app,
        JazorSSRRequest request,
        Action<JazorSpaFallbackOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(request);
        return app.UseJazorSSR(
            static (_, _, state) => Task.FromResult(state),
            request,
            configure);
    }

    /// <summary>Uses a request-specific generated root component and props for SPA-fallback navigation requests.</summary>
    public static IApplicationBuilder UseJazorSSR(
        this IApplicationBuilder app,
        Func<HttpContext, CancellationToken, Task<JazorSSRRequest>> requestFactory,
        Action<JazorSpaFallbackOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(requestFactory);
        return app.UseJazorSSR(
            static (context, cancellationToken, state) => state(context, cancellationToken),
            requestFactory,
            configure);
    }

    private static IApplicationBuilder UseJazorSSR<TState>(
        this IApplicationBuilder app,
        Func<HttpContext, CancellationToken, TState, Task<JazorSSRRequest>> requestFactory,
        TState state,
        Action<JazorSpaFallbackOptions>? configure)
        => app.UseJazorSpaFallback(
            async (context, cancellationToken) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                if (HttpMethods.IsHead(context.Request.Method))
                    return;

                var services = context.RequestServices;
                var renderer = services.GetService<IJazorSSRRenderer>()
                    ?? throw new InvalidOperationException(
                        "Jazor SSR requires AddJazorSSR() before UseJazorSSR().");
                var artifacts = services.GetService<JazorSSRArtifactLocator>()
                    ?? throw new InvalidOperationException(
                        "Jazor SSR artifact services were not registered. Call AddJazorSSR() before UseJazorSSR().");
                var options = services.GetService<IOptions<JazorSSROptions>>()?.Value
                    ?? throw new InvalidOperationException(
                        "Jazor SSR options were not registered. Call AddJazorSSR() before UseJazorSSR().");
                var request = await requestFactory(context, cancellationToken, state).ConfigureAwait(false);
                var result = await renderer.RenderAsync(request, cancellationToken).ConfigureAwait(false);
                var artifactRoot = artifacts.Resolve();
                await JazorSSRDocumentWriter.WriteAsync(
                    context,
                    artifactRoot,
                    artifacts,
                    options,
                    result,
                    cancellationToken).ConfigureAwait(false);
            },
            configure);
}

internal static class JazorSSRDocumentWriter
{
    private const string PropsElementId = "__jazor_ssr_props";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Default
    };

    public static async Task WriteAsync(
        HttpContext context,
        JazorSSRArtifacts artifactRoot,
        JazorSSRArtifactLocator artifacts,
        JazorSSROptions options,
        JazorSSRRenderResult result,
        CancellationToken cancellationToken)
    {
        var mountElementId = NormalizeMountElementId(options.MountElementId);
        var mountElementIdJson = JsonSerializer.Serialize(mountElementId, JsonOptions);
        var componentUrl = JazorSSRArtifactLocator.CreateBrowserArtifactUrl(
            artifactRoot,
            context.Request.PathBase,
            result.ModulePath);
        var componentUrlJson = JsonSerializer.Serialize(componentUrl, JsonOptions);
        var importMap = JazorSSRArtifactLocator.ReadBrowserImportMap(artifactRoot, context.Request.PathBase);
        var styles = JazorSSRArtifactLocator.ReadStylePaths(artifactRoot);
        var response = context.Response;

        await response.WriteAsync("<!doctype html>\n<html><head><meta charset=\"utf-8\">\n", cancellationToken);
        foreach (var stylePath in styles)
        {
            var styleUrl = JazorSSRArtifactLocator.CreateBrowserArtifactUrl(artifactRoot, context.Request.PathBase, stylePath);
            await response.WriteAsync(
                "<link rel=\"stylesheet\" href=\"" + HtmlEncoder.Default.Encode(styleUrl) + "\">\n",
                cancellationToken);
        }

        await response.WriteAsync("<script type=\"importmap\">", cancellationToken);
        await response.WriteAsync(importMap, cancellationToken);
        await response.WriteAsync("</script>\n</head><body>\n<div id=\"", cancellationToken);
        await response.WriteAsync(HtmlEncoder.Default.Encode(mountElementId), cancellationToken);
        await response.WriteAsync("\">", cancellationToken);
        await response.WriteAsync(result.Html, cancellationToken);
        await response.WriteAsync("</div>\n<script id=\"" + PropsElementId + "\" type=\"application/json\">", cancellationToken);
        await response.WriteAsync(result.SerializedProps, cancellationToken);
        await response.WriteAsync("</script>\n<script type=\"module\">\n", cancellationToken);
        await response.WriteAsync("import { createSSRApp } from \"vue\";\n", cancellationToken);
        await response.WriteAsync("const mountElement = document.getElementById(" + mountElementIdJson + ");\n", cancellationToken);
        await response.WriteAsync("if (!mountElement) throw new Error(\"Jazor SSR mount element was not found.\");\n", cancellationToken);
        await response.WriteAsync("const { default: component } = await import(" + componentUrlJson + ");\n", cancellationToken);
        await response.WriteAsync("const props = JSON.parse(document.getElementById(\"" + PropsElementId + "\").textContent);\n", cancellationToken);
        await response.WriteAsync("createSSRApp(component, props).mount(mountElement);\n", cancellationToken);
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
