using Microsoft.AspNetCore.Http;

namespace Jero.Plugin.SaveLife;

public sealed class SaveLifeTraceIdMiddleware
{
    public const string HeaderName = "X-Trace-Id";

    private readonly RequestDelegate _next;

    public SaveLifeTraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier?.Trim();
        if (string.IsNullOrWhiteSpace(traceId))
            traceId = Guid.NewGuid().ToString("N");

        context.Response.Headers[HeaderName] = traceId.Length <= 64
            ? traceId
            : traceId[..64];
        await _next(context);
    }
}
