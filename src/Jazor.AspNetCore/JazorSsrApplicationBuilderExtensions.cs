using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore;

public static class JazorSsrApplicationBuilderExtensions
{
    /// <summary>Uses one fixed generated root component for all SPA-fallback navigation requests.</summary>
    public static IApplicationBuilder UseJazorSsr(
        this IApplicationBuilder app,
        string modulePath,
        object? props = null)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseJazorSsr(new JazorSsrRequest(modulePath, props));
    }

    /// <summary>Uses one fixed generated root component for all SPA-fallback navigation requests.</summary>
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

    /// <summary>Uses a request-specific generated root component and props for SPA-fallback navigation requests.</summary>
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
                var artifacts = services.GetService<JazorSsrArtifactLocator>()
                    ?? throw new InvalidOperationException(
                        "Jazor SSR artifact services were not registered. Call AddJazorSsr() before UseJazorSsr().");
                var options = services.GetService<IOptions<JazorSsrOptions>>()?.Value
                    ?? throw new InvalidOperationException(
                        "Jazor SSR options were not registered. Call AddJazorSsr() before UseJazorSsr().");
                var request = await requestFactory(context, cancellationToken, state).ConfigureAwait(false);
                var result = await renderer.RenderAsync(request, cancellationToken).ConfigureAwait(false);
                var artifactRoot = artifacts.Resolve();
                await JazorSsrDocumentWriter.WriteAsync(
                    context,
                    artifactRoot,
                    artifacts,
                    options,
                    result,
                    cancellationToken).ConfigureAwait(false);
            },
            configure);
}

internal static class JazorSsrDocumentWriter
{
    private const string PropsElementId = "__jazor_ssr_props";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Default
    };

    public static async Task WriteAsync(
        HttpContext context,
        JazorSsrArtifacts artifactRoot,
        JazorSsrArtifactLocator artifacts,
        JazorSsrOptions options,
        JazorSsrRenderResult result,
        CancellationToken cancellationToken)
    {
        var mountElementId = NormalizeMountElementId(options.MountElementId);
        var mountElementIdJson = JsonSerializer.Serialize(mountElementId, JsonOptions);
        var componentUrl = artifacts.CreateBrowserArtifactUrl(
            artifactRoot,
            context.Request.PathBase,
            result.ModulePath);
        var componentUrlJson = JsonSerializer.Serialize(componentUrl, JsonOptions);
        var importMap = artifacts.ReadBrowserImportMap(artifactRoot, context.Request.PathBase);
        var styles = artifacts.ReadStylePaths(artifactRoot);
        var response = context.Response;

        await response.WriteAsync("<!doctype html>\n<html><head><meta charset=\"utf-8\">\n", cancellationToken);
        foreach (var stylePath in styles)
        {
            var styleUrl = artifacts.CreateBrowserArtifactUrl(artifactRoot, context.Request.PathBase, stylePath);
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
