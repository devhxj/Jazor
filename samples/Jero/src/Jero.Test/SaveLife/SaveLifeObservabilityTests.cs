using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeObservabilityTests
{
    [Fact]
    public async Task TraceMiddleware_AddsResponseTraceHeader()
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-123"
        };
        var middleware = new SaveLifeTraceIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("trace-123", context.Response.Headers[SaveLifeTraceIdMiddleware.HeaderName]);
    }

    [Fact]
    public void ClientDiagnosticPolicy_StoresOnlyWhitelistedNormalizedFields()
    {
        var input = new SubmitSaveLifeClientDiagnosticInput
        {
            Route = "/save-life/enrollments/824663558844485?phone=13800138000",
            DurationMs = 999999,
            StatusCode = 503,
            Version = "1.2.3 production build with extra text",
            Environment = "production",
            NetworkType = "wifi"
        };

        var diagnostic = SaveLifeClientDiagnosticPolicy.Sanitize(input);
        var json = JsonSerializer.Serialize(diagnostic);
        var inputProperties = typeof(SubmitSaveLifeClientDiagnosticInput)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(u => u.Name)
            .OrderBy(u => u)
            .ToArray();

        Assert.Equal(
        [
            "DurationMs",
            "Environment",
            "NetworkType",
            "Route",
            "StatusCode",
            "Version",
        ], inputProperties);
        Assert.Equal("/save-life/enrollments/:id", diagnostic.Route);
        Assert.Equal(120000, diagnostic.DurationMs);
        Assert.Equal(503, diagnostic.StatusCode);
        Assert.Equal("unknown", diagnostic.Version);
        Assert.DoesNotContain("phone", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("content", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ClientDiagnosticEndpoint_UsesDedicatedNonRecursiveRoute()
    {
        var method = typeof(SaveLifeFeedbackService).GetMethod(
            "MiniAppDiagnostic",
            BindingFlags.Instance | BindingFlags.Public);
        var route = method?.GetCustomAttribute<HttpPostAttribute>();

        Assert.NotNull(method);
        Assert.Equal("/api/v1/save-life/diagnostics", route?.Template);
    }
}
