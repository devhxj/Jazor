// Maps authorization-code, token, and logout endpoints for the in-host OpenIddict single sign-on server.
// 映射同宿主 OpenIddict 单点登录中心的授权码、令牌和登出端点。
using System.Collections.Immutable;
using System.Net;
using System.Security.Claims;
using System.Text;
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
        UserManager<JazorAdminUser> users,
        IOpenIddictApplicationManager applications,
        IOpenIddictAuthorizationManager authorizations,
        IOpenIddictScopeManager scopes,
        IWebHostEnvironment environment)
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

        var application = await applications.FindByClientIdAsync(request.ClientId!, context.RequestAborted)
            ?? throw new InvalidOperationException("The OpenIddict application is unavailable.");
        var applicationId = await applications.GetIdAsync(application, context.RequestAborted)
            ?? throw new InvalidOperationException("The OpenIddict application identifier is unavailable.");
        var consentType = await applications.GetConsentTypeAsync(application, context.RequestAborted);
        var existingAuthorizations = new List<object>();
        await foreach (var authorization in authorizations.FindAsync(
                           user.Id,
                           applicationId,
                           OpenIddictConstants.Statuses.Valid,
                           OpenIddictConstants.AuthorizationTypes.Permanent,
                           request.GetScopes(),
                           context.RequestAborted))
        {
            existingAuthorizations.Add(authorization);
        }

        var decision = await ReadConsentDecisionAsync(context);
        if (decision == "deny")
            return Forbid(OpenIddictConstants.Errors.AccessDenied, "The resource owner denied the authorization request.");

        var promptConsent = request.HasPromptValue(OpenIddictConstants.PromptValues.Consent);
        var needsConsent = consentType == OpenIddictConstants.ConsentTypes.Systematic ||
                           consentType == OpenIddictConstants.ConsentTypes.Explicit &&
                           (existingAuthorizations.Count == 0 || promptConsent);
        if (needsConsent && decision != "accept")
        {
            if (request.HasPromptValue(OpenIddictConstants.PromptValues.None))
                return Forbid(OpenIddictConstants.Errors.ConsentRequired, "Interactive user consent is required.");

            return RenderConsent(
                context,
                (await applications.GetDisplayNameAsync(application, context.RequestAborted)) ?? request.ClientId!,
                request.GetScopes(),
                request,
                environment);
        }

        var principal = await CreateUserPrincipalAsync(user, request, users, scopes);
        if (consentType == OpenIddictConstants.ConsentTypes.Explicit)
        {
            var authorization = existingAuthorizations.FirstOrDefault();
            if (authorization is null)
            {
                authorization = await authorizations.CreateAsync(
                    (ClaimsIdentity)principal.Identity!,
                    user.Id,
                    applicationId,
                    OpenIddictConstants.AuthorizationTypes.Permanent,
                    principal.GetScopes(),
                    context.RequestAborted);
            }
            principal.SetAuthorizationId(await authorizations.GetIdAsync(authorization, context.RequestAborted));
        }

        return Results.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> ExchangeTokenAsync(
        HttpContext context,
        UserManager<JazorAdminUser> users,
        IOpenIddictScopeManager scopes)
    {
        var request = context.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenIddict token request is unavailable.");
        if (request.IsClientCredentialsGrantType())
        {
            var clientPrincipal = await CreateClientPrincipalAsync(request, scopes);
            return Results.SignIn(clientPrincipal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

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

    private static async Task<ClaimsPrincipal> CreateUserPrincipalAsync(
        JazorAdminUser user,
        OpenIddictRequest request,
        UserManager<JazorAdminUser> users,
        IOpenIddictScopeManager scopes)
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
        principal.SetResources(await ResolveResourcesAsync(request, scopes));
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

    private static async Task<ClaimsPrincipal> CreateClientPrincipalAsync(
        OpenIddictRequest request,
        IOpenIddictScopeManager scopes)
    {
        var clientId = request.ClientId
            ?? throw new InvalidOperationException("The authenticated client identifier is unavailable.");
        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);
        identity.SetClaim(OpenIddictConstants.Claims.Subject, clientId)
            .SetClaim(OpenIddictConstants.Claims.Name, clientId);

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(request.GetScopes());
        principal.SetResources(await ResolveResourcesAsync(request, scopes));
        principal.SetDestinations(static _ => [OpenIddictConstants.Destinations.AccessToken]);
        return principal;
    }

    private static async Task<string[]> ResolveResourcesAsync(
        OpenIddictRequest request,
        IOpenIddictScopeManager scopes)
    {
        var resources = new List<string>();
        await foreach (var resource in scopes.ListResourcesAsync(request.GetScopes()))
            resources.Add(resource);
        return resources.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static async Task<string?> ReadConsentDecisionAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) || !context.Request.HasFormContentType)
            return null;

        var form = await context.Request.ReadFormAsync(context.RequestAborted);
        return form["decision"].ToString();
    }

    private static IResult RenderConsent(
        HttpContext context,
        string applicationName,
        ImmutableArray<string> scopes,
        OpenIddictRequest request,
        IWebHostEnvironment environment)
    {
        var action = WebUtility.HtmlEncode(context.Request.PathBase + context.Request.Path);
        var name = WebUtility.HtmlEncode(applicationName);
        var requestedScopes = WebUtility.HtmlEncode(string.Join(", ", scopes));
        var parameters = RenderRequestParameters(request);
        var styles = JazorAdminShell.GetStyleLinks(environment);
        var html = $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>Authorize {{name}}</title>
              {{styles}}
            </head>
            <body>
              <main class="ja-access ja-access--login" data-access-page="consent">
                <section class="ja-access__panel">
                  <strong class="ja-access__brand">JazorAdmin</strong>
                  <h1>Authorize {{name}}</h1>
                  <p>The application is requesting: {{requestedScopes}}</p>
                  <form method="post" action="{{action}}">
                    {{parameters}}
                    <button type="submit" name="decision" value="accept">Allow access</button>
                    <button type="submit" name="decision" value="deny">Deny</button>
                  </form>
                </section>
              </main>
            </body>
            </html>
            """;
        return Results.Content(html, "text/html; charset=utf-8");
    }

    private static string RenderRequestParameters(OpenIddictRequest request)
    {
        var fields = new StringBuilder();
        foreach (var parameter in request.GetParameters())
        {
            // OpenIddict only reads form values for a consent POST, so keep every validated OAuth parameter.
            // `decision` belongs to this form and must not be replayed from a custom authorization parameter.
            if (string.Equals(parameter.Key, "decision", StringComparison.Ordinal))
                continue;

            fields.Append("<input type=\"hidden\" name=\"")
                .Append(WebUtility.HtmlEncode(parameter.Key))
                .Append("\" value=\"")
                .Append(WebUtility.HtmlEncode(parameter.Value.ToString()))
                .AppendLine("\">");
        }

        return fields.ToString();
    }

    private static IResult Forbid(string error, string description)
        => Results.Forbid(
            new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            }),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
}
