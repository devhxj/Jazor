// Maps platform account management APIs to the ASP.NET Core Identity store.
// 将平台账户管理 API 映射到 ASP.NET Core Identity 存储。
using JazorAdmin.Authorization;
using JazorAdmin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JazorAdmin.Features.Accounts;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var accounts = app.MapGroup("/api/accounts")
            .WithTags("Accounts")
            .RequireAuthorization(JazorAdminPolicies.PlatformAdministrator);

        accounts.MapGet("/", ListAsync);
        accounts.MapPost("/", CreateAsync);
        accounts.MapPut("/{userId}/enabled", UpdateStateAsync);
        accounts.MapPut("/{userId}/password", ResetPasswordAsync);
        return app;
    }

    private static async Task<IResult> ListAsync(
        UserManager<JazorAdminUser> users,
        CancellationToken cancellationToken)
    {
        var accounts = await users.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .ToArrayAsync(cancellationToken);
        var responses = new AccountResponse[accounts.Length];
        for (var index = 0; index < accounts.Length; index++)
            responses[index] = await ToResponseAsync(accounts[index], users);

        return Results.Ok(responses);
    }

    private static async Task<IResult> CreateAsync(
        CreateAccountRequest request,
        UserManager<JazorAdminUser> users)
    {
        if (!TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        var user = new JazorAdminUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            DisplayName = request.DisplayName.Trim(),
            EmailConfirmed = true,
            LockoutEnabled = true
        };
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Results.ValidationProblem(ToErrors(result));

        if (request.PlatformAdministrator)
        {
            result = await users.AddToRoleAsync(user, JazorAdminRoles.PlatformAdministrator);
            if (!result.Succeeded)
                return Results.ValidationProblem(ToErrors(result));
        }

        return Results.Created("/api/accounts/" + user.Id, await ToResponseAsync(user, users));
    }

    private static async Task<IResult> UpdateStateAsync(
        string userId,
        UpdateAccountStateRequest request,
        UserManager<JazorAdminUser> users)
    {
        var user = await users.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound();

        var result = await users.SetLockoutEndDateAsync(user, request.Enabled ? null : DateTimeOffset.MaxValue);
        return result.Succeeded ? Results.NoContent() : Results.ValidationProblem(ToErrors(result));
    }

    private static async Task<IResult> ResetPasswordAsync(
        string userId,
        ResetAccountPasswordRequest request,
        UserManager<JazorAdminUser> users)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["password"] = ["Password is required."]
            });
        }

        var user = await users.FindByIdAsync(userId);
        if (user is null)
            return Results.NotFound();

        var token = await users.GeneratePasswordResetTokenAsync(user);
        var result = await users.ResetPasswordAsync(user, token, request.Password);
        return result.Succeeded ? Results.NoContent() : Results.ValidationProblem(ToErrors(result));
    }

    private static async Task<AccountResponse> ToResponseAsync(JazorAdminUser user, UserManager<JazorAdminUser> users)
        => new(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow,
            await users.IsInRoleAsync(user, JazorAdminRoles.PlatformAdministrator));

    private static bool TryValidate(CreateAccountRequest request, out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Email))
            errors["email"] = ["Email is required."];
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            errors["displayName"] = ["Display name is required."];
        if (string.IsNullOrWhiteSpace(request.Password))
            errors["password"] = ["Password is required."];
        return errors.Count == 0;
    }

    private static Dictionary<string, string[]> ToErrors(IdentityResult result)
        => new()
        {
            ["identity"] = result.Errors.Select(error => error.Description).ToArray()
        };
}
