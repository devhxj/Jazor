// Maps authorization-code, token, and logout endpoints for the in-host OpenIddict single sign-on server.
// 映射同宿主 OpenIddict 单点登录中心的授权码、令牌和登出端点。
using System.Collections.Immutable;
using System.Security.Claims;
using JazorAdmin.Authorization;
using JazorAdmin.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace JazorAdmin.Authentication;

public static class OpenIddictEndpoints
{
    public static IEndpointRouteBuilder MapOpenIddictEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMethods("/connect/authorize", ["GET", "POST"], AuthorizeAsync).AllowAnonymous();
        app.MapPost("/connect/token", ExchangeTokenAsync).AllowAnonymous();
        app.MapMethods("/connect/logout", ["GET", "POST"], LogoutAsync).AllowAnonymous();
        return app;
    }

    private static async Task<IResult> AuthorizeAsync(
        HttpContext context,
        UserManager<JazorAdminUser> users)
    {
        var request = context.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenIddict authorization request is unavailable.");
        var cookie = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!cookie.Succeeded || cookie.Principal is null)
        {
            var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
            return Results.Challenge(
                new AuthenticationProperties { RedirectUri = returnUrl },
                [IdentityConstants.ApplicationScheme]);
        }

        var user = await users.GetUserAsync(cookie.Principal);
        if (user is null)
        {
            return Results.Challenge(
                new AuthenticationProperties { RedirectUri = context.Request.PathBase + context.Request.Path + context.Request.QueryString },
                [IdentityConstants.ApplicationScheme]);
        }

        var principal = await CreatePrincipalAsync(user, request, users);
        return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> ExchangeTokenAsync(
        HttpContext context,
        UserManager<JazorAdminUser> users)
    {
        var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = result.Principal;
        var subject = principal?.GetClaim(OpenIddictConstants.Claims.Subject);
        if (principal is null || string.IsNullOrWhiteSpace(subject) || await users.FindByIdAsync(subject) is null)
        {
            return Results.Forbid(
                new AuthenticationProperties(),
                [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        }

        return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext context,
        SignInManager<JazorAdminUser> signInManager)
    {
        var request = context.GetOpenIddictServerRequest();
        await signInManager.SignOutAsync();
        return Results.SignOut(
            new AuthenticationProperties { RedirectUri = request?.PostLogoutRedirectUri },
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    private static async Task<ClaimsPrincipal> CreatePrincipalAsync(
        JazorAdminUser user,
        OpenIddictRequest request,
        UserManager<JazorAdminUser> users)
    {
        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);
        identity.SetClaim(OpenIddictConstants.Claims.Subject, user.Id)
            .SetClaim(OpenIddictConstants.Claims.Name, user.DisplayName)
            .SetClaim(OpenIddictConstants.Claims.Email, user.Email)
            .SetClaims(
                OpenIddictConstants.Claims.Role,
                ImmutableArray.CreateRange(await users.GetRolesAsync(user)));

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(request.GetScopes());
        principal.SetResources(JazorAdminScopes.Api);
        principal.SetDestinations(static claim => claim.Type switch
        {
            OpenIddictConstants.Claims.Subject => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],
            OpenIddictConstants.Claims.Name when claim.Subject!.HasScope(OpenIddictConstants.Scopes.Profile)
                => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],
            OpenIddictConstants.Claims.Email when claim.Subject!.HasScope(OpenIddictConstants.Scopes.Email)
                => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],
            OpenIddictConstants.Claims.Role when claim.Subject!.HasScope(OpenIddictConstants.Scopes.Roles)
                => [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],
            _ => [OpenIddictConstants.Destinations.AccessToken]
        });
        return principal;
    }
}
