using OpenIddict.Abstractions;

namespace JazorAdmin.Features.Sso;

internal static class GrantEndpoints
{
    private const int ListLimit = 200;

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/authorizations", ListAuthorizationsAsync);
        group.MapPost("/authorizations/{id}/revoke", RevokeAuthorizationAsync);
        group.MapGet("/tokens", ListTokensAsync);
        group.MapPost("/tokens/{id}/revoke", RevokeTokenAsync);
    }

    private static async Task<IResult> ListAuthorizationsAsync(
        IOpenIddictAuthorizationManager authorizations,
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        var values = new List<AuthorizationView>();
        await foreach (var authorization in authorizations.ListAsync(ListLimit, 0, cancellationToken))
        {
            var applicationId = await authorizations.GetApplicationIdAsync(authorization, cancellationToken);
            values.Add(new AuthorizationView(
                (await authorizations.GetIdAsync(authorization, cancellationToken)) ?? string.Empty,
                applicationId,
                await ResolveClientIdAsync(applicationId, applications, cancellationToken),
                await authorizations.GetSubjectAsync(authorization, cancellationToken),
                (await authorizations.GetStatusAsync(authorization, cancellationToken)) ?? string.Empty,
                NormalizeType(await authorizations.GetTypeAsync(authorization, cancellationToken)),
                (await authorizations.GetScopesAsync(authorization, cancellationToken)).Order(StringComparer.Ordinal).ToArray(),
                Format(await authorizations.GetCreationDateAsync(authorization, cancellationToken))));
        }

        return Results.Ok(values.OrderByDescending(static value => value.CreatedAt));
    }

    private static async Task<IResult> RevokeAuthorizationAsync(
        string id,
        IOpenIddictAuthorizationManager authorizations,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizations.FindByIdAsync(id, cancellationToken);
        if (authorization is null)
            return Results.NotFound();

        return await authorizations.TryRevokeAsync(authorization, cancellationToken)
            ? Results.NoContent()
            : Results.Conflict(new { message = "The authorization could not be revoked." });
    }

    private static async Task<IResult> ListTokensAsync(
        IOpenIddictTokenManager tokens,
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        var values = new List<TokenView>();
        await foreach (var token in tokens.ListAsync(ListLimit, 0, cancellationToken))
        {
            var applicationId = await tokens.GetApplicationIdAsync(token, cancellationToken);
            values.Add(new TokenView(
                (await tokens.GetIdAsync(token, cancellationToken)) ?? string.Empty,
                applicationId,
                await ResolveClientIdAsync(applicationId, applications, cancellationToken),
                await tokens.GetAuthorizationIdAsync(token, cancellationToken),
                await tokens.GetSubjectAsync(token, cancellationToken),
                (await tokens.GetStatusAsync(token, cancellationToken)) ?? string.Empty,
                NormalizeType(await tokens.GetTypeAsync(token, cancellationToken)),
                Format(await tokens.GetCreationDateAsync(token, cancellationToken)),
                Format(await tokens.GetExpirationDateAsync(token, cancellationToken)),
                Format(await tokens.GetRedemptionDateAsync(token, cancellationToken))));
        }

        return Results.Ok(values.OrderByDescending(static value => value.CreatedAt));
    }

    private static async Task<IResult> RevokeTokenAsync(
        string id,
        IOpenIddictTokenManager tokens,
        CancellationToken cancellationToken)
    {
        var token = await tokens.FindByIdAsync(id, cancellationToken);
        if (token is null)
            return Results.NotFound();

        return await tokens.TryRevokeAsync(token, cancellationToken)
            ? Results.NoContent()
            : Results.Conflict(new { message = "The token could not be revoked." });
    }

    private static async Task<string?> ResolveClientIdAsync(
        string? applicationId,
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(applicationId))
            return null;

        var application = await applications.FindByIdAsync(applicationId, cancellationToken);
        return application is null
            ? null
            : await applications.GetClientIdAsync(application, cancellationToken);
    }

    private static string? Format(DateTimeOffset? value)
        => value?.ToString("O");

    private static string NormalizeType(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var separator = value.LastIndexOf(':');
        return separator < 0 ? value : value[(separator + 1)..];
    }
}
