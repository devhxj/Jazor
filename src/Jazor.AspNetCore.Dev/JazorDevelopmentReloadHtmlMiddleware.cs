using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Jazor.AspNetCore.Dev;

internal sealed class JazorDevelopmentReloadHtmlMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JazorDevelopmentReloadService _service;

    public JazorDevelopmentReloadHtmlMiddleware(
        RequestDelegate next,
        JazorDevelopmentReloadService service)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldInspectRequest(context))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);

            if (!ShouldTransformResponse(context))
            {
                await CopyBufferedResponseAsync(buffer, originalBody, context.RequestAborted);
                return;
            }

            buffer.Position = 0;
            using var reader = new StreamReader(buffer, JazorDevelopmentHtmlInjector.ResolveEncoding(context.Response.ContentType), detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var html = await reader.ReadToEndAsync(context.RequestAborted);

            var nonce = JazorDevelopmentHtmlInjector.TryExtractScriptNonce(context.Response.Headers.ContentSecurityPolicy);
            var transformedHtml = JazorDevelopmentHtmlInjector.InjectClientScript(
                html,
                _service.Options.ClientScriptPath.Value!,
                JazorDevelopmentReloadService.PathBaseAttributeName,
                context.Request.PathBase.Value ?? string.Empty,
                nonce);

            context.Response.Headers.Remove("Content-Length");
            if (context.Response.Headers.TryGetValue("Content-Security-Policy", out var policyValues))
            {
                context.Response.Headers["Content-Security-Policy"] =
                    JazorDevelopmentHtmlInjector.AugmentContentSecurityPolicy(policyValues);
            }

            context.Response.Body = originalBody;
            var encoding = JazorDevelopmentHtmlInjector.ResolveEncoding(context.Response.ContentType);
            var payload = encoding.GetBytes(transformedHtml);
            context.Response.ContentLength = payload.Length;

            if (!HttpMethods.IsHead(context.Request.Method))
            {
                await context.Response.Body.WriteAsync(payload, context.RequestAborted);
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private bool ShouldInspectRequest(HttpContext context)
        => JazorDevelopmentBrowserDocumentRequestClassifier.ShouldInspect(
            context,
            _service.Options.ClientScriptPath,
            _service.Options.WebSocketPath,
            _service.IsEnabled);

    private static bool ShouldTransformResponse(HttpContext context)
    {
        if (HttpMethods.IsHead(context.Request.Method))
            return false;

        if (context.Response.StatusCode is StatusCodes.Status204NoContent or StatusCodes.Status304NotModified)
            return false;

        if (context.Response.Headers.ContainsKey("Content-Encoding"))
            return false;

        var contentType = context.Response.ContentType;
        return !string.IsNullOrWhiteSpace(contentType)
            && contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task CopyBufferedResponseAsync(
        MemoryStream buffer,
        Stream destination,
        CancellationToken cancellationToken)
    {
        buffer.Position = 0;
        await buffer.CopyToAsync(destination, cancellationToken);
    }
}
