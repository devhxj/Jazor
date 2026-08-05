// Maps same-origin login, logout, and session APIs used by the RazorVue client shell.
// 映射 RazorVue 客户端壳层使用的同源登录、登出和会话 API。
using JazorAdmin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JazorAdmin.Features.Identity;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Session");
        group.MapPost("/login", SignInAsync).AllowAnonymous();
        group.MapPost("/logout", SignOutAsync).RequireAuthorization();
        group.MapGet("/session", GetSessionAsync).RequireAuthorization();
        return app;
    }

    private static async Task<IResult> SignInAsync(
        LoginRequest request,
        UserManager<JazorAdminUser> users,
        SignInManager<JazorAdminUser> signInManager)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["email"] = ["Email is required."],
                ["password"] = ["Password is required."]
            });
        }

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
