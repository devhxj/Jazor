using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace JazorAdmin.DemoClient;

public static class DemoEndpoints
{
    public static IEndpointRouteBuilder MapDemoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health/live", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
        app.MapGet("/auth/signin", SignIn).AllowAnonymous();
        app.MapGet("/auth/signout", SignOut).RequireAuthorization();
        app.MapGet("/api/session", (Func<HttpContext, Task<IResult>>)GetSession).RequireAuthorization();
        app.MapGet("/api/platform/overview", GetPlatformOverviewAsync).RequireAuthorization();
        return app;
    }

    private static IResult SignIn(HttpContext context)
    {
        var returnUrl = context.Request.Query["returnUrl"].ToString();
        if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/', StringComparison.Ordinal))
            returnUrl = "/";

        return Results.Challenge(
            new AuthenticationProperties { RedirectUri = returnUrl },
            [OpenIdConnectDefaults.AuthenticationScheme]);
    }

    private static IResult SignOut()
        => Results.SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);

    private static async Task<IResult> GetSession(HttpContext context)
    {
        var authentication = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = authentication.Principal ?? context.User;
        var subject = principal.FindFirstValue("sub")
                      ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? string.Empty;
        var roles = principal.FindAll("role").Select(static claim => claim.Value).Distinct(StringComparer.Ordinal).ToArray();
        var hasAccessToken = authentication.Properties?.GetTokenValue("access_token") is { Length: > 0 };
        return Results.Ok(new DemoSessionView(
            subject,
            principal.FindFirstValue("name") ?? principal.Identity?.Name,
            principal.FindFirstValue("email") ?? principal.FindFirstValue(ClaimTypes.Email),
            roles,
            hasAccessToken));
    }

    private static async Task<IResult> GetPlatformOverviewAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory)
    {
        var authentication = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var accessToken = authentication.Properties?.GetTokenValue("access_token");
        if (string.IsNullOrWhiteSpace(accessToken))
            return Results.Unauthorized();

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/overview/");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClientFactory.CreateClient("JazorAdmin").SendAsync(request, context.RequestAborted);
        var payload = await response.Content.ReadAsStringAsync(context.RequestAborted);
        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem(
                statusCode: (int)response.StatusCode,
                detail: "The downstream access token was rejected by JazorAdmin.");
        }

        // Keep the demo contract deliberately narrow. The client consumes an identity claim and
        // a real bearer-protected API response, without importing the platform's internal DTOs.
        // 客户端只消费身份 claims 和真实 Bearer API 的窄投影，不耦合平台内部 DTO。
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        return Results.Ok(new ProtectedOverviewView(
            root.GetProperty("accounts").GetInt32(),
            root.GetProperty("applications").GetInt32(),
            root.GetProperty("tokens").GetInt32(),
            root.GetProperty("auditEvents").GetInt32(),
            root.GetProperty("tokenIssuances").GetInt32()));
    }
}
