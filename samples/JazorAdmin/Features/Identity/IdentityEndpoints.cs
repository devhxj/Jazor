// Maps same-origin login, logout, and session APIs used by the RazorVue client shell.
// 映射 RazorVue 客户端壳层使用的同源登录、登出和会话 API。
using JazorAdmin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace JazorAdmin.Features.Identity;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Session");
        group.MapGet("/captcha", CreateCaptcha).AllowAnonymous();
        group.MapGet("/captcha/{id}.svg", GetCaptchaImage).AllowAnonymous();
        group.MapPost("/login", SignInAsync).AllowAnonymous();
        group.MapPost("/logout", SignOutAsync).RequireAuthorization();
        group.MapGet("/session", GetSessionAsync).RequireAuthorization();
        return app;
    }

    private static IResult CreateCaptcha(CaptchaService captcha)
    {
        var issue = captcha.Issue();
        return Results.Ok(new CaptchaChallengeResponse(issue.Id, "/api/auth/captcha/" + issue.Id + ".svg"));
    }

    private static IResult GetCaptchaImage(string id, CaptchaService captcha)
    {
        var svg = captcha.GetImage(id);
        return svg is null
            ? Results.NotFound()
            : Results.Text(svg, "image/svg+xml; charset=utf-8", Encoding.UTF8);
    }

    private static async Task<IResult> SignInAsync(
        HttpContext context,
        LoginRequest request,
        UserManager<JazorAdminUser> users,
        SignInManager<JazorAdminUser> signInManager,
        CaptchaService captcha)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["email"] = ["Email is required."],
                ["password"] = ["Password is required."]
            });
        }

        // Lock-screen confirmation retains the authenticated session; only an anonymous sign-in needs the challenge.
        // 锁屏确认仍保留认证会话，只有匿名首次登录需要验证码。
        if (context.User.Identity?.IsAuthenticated != true && !captcha.TryValidate(request.CaptchaId, request.CaptchaAnswer))
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Verification code is incorrect or expired.");

        var user = await users.FindByEmailAsync(request.Email);
        if (user is null)
            return Results.Unauthorized();

        var result = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);
        return result.Succeeded ? Results.NoContent() : Results.Unauthorized();
    }

    private static async Task<IResult> SignOutAsync(SignInManager<JazorAdminUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> GetSessionAsync(
        HttpContext context,
        UserManager<JazorAdminUser> users,
        JazorAdminDbContext database)
    {
        var user = await users.GetUserAsync(context.User);
        if (user is null)
            return Results.Unauthorized();

        var organizations = await database.OrganizationMemberships
            .Where(membership => membership.UserId == user.Id && membership.IsActive)
            .OrderBy(membership => membership.Organization.DisplayName)
            .Select(membership => new OrganizationSummary(
                membership.OrganizationId.ToString(),
                membership.Organization.Code,
                membership.Organization.DisplayName))
            .ToArrayAsync(context.RequestAborted);
        var roles = await users.GetRolesAsync(user);
        return Results.Ok(new SessionResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            roles.ToArray(),
            organizations));
    }
}
