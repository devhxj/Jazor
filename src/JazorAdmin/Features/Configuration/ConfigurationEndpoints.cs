// Maps the OpenIddict application and scope configuration center.
// 映射 OpenIddict application 与 scope 的配置中心。
using JazorAdmin.Authorization;
using OpenIddict.Abstractions;

namespace JazorAdmin.Features.Configuration;

public static class ConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var configuration = app.MapGroup("/api/configuration")
            .WithTags("Configuration")
            .RequireAuthorization(JazorAdminPolicies.PlatformAdministrator);

        configuration.MapGet("/clients", ListClientsAsync);
        configuration.MapPost("/clients", CreateClientAsync);
        configuration.MapGet("/scopes", ListScopesAsync);
        configuration.MapPost("/scopes", CreateScopeAsync);
        return app;
    }

    private static async Task<IResult> ListClientsAsync(
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        var clients = new List<OpenIdClientResponse>();
        await foreach (var application in applications.ListAsync(cancellationToken: cancellationToken))
        {
            clients.Add(new OpenIdClientResponse(
                (await applications.GetIdAsync(application, cancellationToken)) ?? string.Empty,
                (await applications.GetClientIdAsync(application, cancellationToken)) ?? string.Empty,
                (await applications.GetDisplayNameAsync(application, cancellationToken)) ?? string.Empty,
                (await applications.GetRedirectUrisAsync(application, cancellationToken)).Order().ToArray(),
                (await applications.GetPostLogoutRedirectUrisAsync(application, cancellationToken)).Order().ToArray(),
                (await applications.GetPermissionsAsync(application, cancellationToken))
                    .Where(static permission => permission.StartsWith(OpenIddictConstants.Permissions.Prefixes.Scope, StringComparison.Ordinal))
                    .Select(static permission => permission[OpenIddictConstants.Permissions.Prefixes.Scope.Length..])
                    .Order()
                    .ToArray()));
        }

        return Results.Ok(clients.OrderBy(client => client.ClientId));
    }

    private static async Task<IResult> CreateClientAsync(
        CreateClientRequest request,
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        if (!TryNormalizeUris(request.RedirectUris, "redirectUris", out var redirectUris, out var errors) ||
            !TryNormalizeUris(request.PostLogoutRedirectUris, "postLogoutRedirectUris", out var postLogoutRedirectUris, out errors))
        {
            return Results.ValidationProblem(errors);
        }
        if (string.IsNullOrWhiteSpace(request.ClientId))
            errors["clientId"] = ["Client ID is required."];
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            errors["displayName"] = ["Display name is required."];
        if (errors.Count > 0)
            return Results.ValidationProblem(errors);
        if (await applications.FindByClientIdAsync(request.ClientId.Trim(), cancellationToken) is not null)
            return Results.Conflict(new { message = "A client with this ID already exists." });

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = request.ClientId.Trim(),
            DisplayName = request.DisplayName.Trim(),
            ClientType = OpenIddictConstants.ClientTypes.Public,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit
        };
        descriptor.RedirectUris.UnionWith(redirectUris);
        descriptor.PostLogoutRedirectUris.UnionWith(postLogoutRedirectUris);
        descriptor.Permissions.UnionWith(
        [
            OpenIddictConstants.Permissions.Endpoints.Authorization,
            OpenIddictConstants.Permissions.Endpoints.EndSession,
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
            OpenIddictConstants.Permissions.ResponseTypes.Code
        ]);
        foreach (var scope in request.Scopes.Where(static scope => !string.IsNullOrWhiteSpace(scope)).Distinct(StringComparer.Ordinal))
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope.Trim());

        var application = await applications.CreateAsync(descriptor, cancellationToken);
        return Results.Created(
            "/api/configuration/clients/" + await applications.GetIdAsync(application, cancellationToken),
            new OpenIdClientResponse(
                (await applications.GetIdAsync(application, cancellationToken)) ?? string.Empty,
                descriptor.ClientId,
                descriptor.DisplayName,
                redirectUris.Select(static uri => uri.AbsoluteUri).ToArray(),
                postLogoutRedirectUris.Select(static uri => uri.AbsoluteUri).ToArray(),
                request.Scopes.Where(static scope => !string.IsNullOrWhiteSpace(scope)).Distinct(StringComparer.Ordinal).Order().ToArray()));
    }

    private static async Task<IResult> ListScopesAsync(
        IOpenIddictScopeManager scopes,
        CancellationToken cancellationToken)
    {
        var responses = new List<OpenIdScopeResponse>();
        await foreach (var scope in scopes.ListAsync(cancellationToken: cancellationToken))
        {
            responses.Add(new OpenIdScopeResponse(
                (await scopes.GetIdAsync(scope, cancellationToken)) ?? string.Empty,
                (await scopes.GetNameAsync(scope, cancellationToken)) ?? string.Empty,
                (await scopes.GetDisplayNameAsync(scope, cancellationToken)) ?? string.Empty,
                (await scopes.GetResourcesAsync(scope, cancellationToken)).Order().ToArray()));
        }

        return Results.Ok(responses.OrderBy(scope => scope.Name));
    }

    private static async Task<IResult> CreateScopeAsync(
        CreateScopeRequest request,
        IOpenIddictScopeManager scopes,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["name"] = ["Scope name is required."],
                ["displayName"] = ["Display name is required."]
            });
        }
        if (await scopes.FindByNameAsync(request.Name.Trim(), cancellationToken) is not null)
            return Results.Conflict(new { message = "A scope with this name already exists." });

        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = request.Name.Trim(),
            DisplayName = request.DisplayName.Trim()
        };
        descriptor.Resources.Add(JazorAdminScopes.Api);
        var scope = await scopes.CreateAsync(descriptor, cancellationToken);
        return Results.Created(
            "/api/configuration/scopes/" + await scopes.GetIdAsync(scope, cancellationToken),
            new OpenIdScopeResponse(
                (await scopes.GetIdAsync(scope, cancellationToken)) ?? string.Empty,
                descriptor.Name,
                descriptor.DisplayName,
                descriptor.Resources.ToArray()));
    }

    private static bool TryNormalizeUris(
        IReadOnlyList<string> values,
        string key,
        out Uri[] uris,
        out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();
        uris = values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) &&
                            (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                ? uri
                : null)
            .ToArray()!;
        if (uris.Length != values.Count || uris.Length == 0 || uris.Select(static uri => uri.AbsoluteUri).Distinct(StringComparer.Ordinal).Count() != uris.Length)
        {
            errors[key] = ["Provide one or more unique absolute HTTP or HTTPS URIs."];
            uris = [];
            return false;
        }

        return true;
    }
}
