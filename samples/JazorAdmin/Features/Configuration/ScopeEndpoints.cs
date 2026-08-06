using OpenIddict.Abstractions;

namespace JazorAdmin.Features.Configuration;

internal static class ScopeEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/scopes", ListAsync);
        group.MapGet("/scopes/{id}", GetAsync);
        group.MapPost("/scopes", CreateAsync);
        group.MapPut("/scopes/{id}", UpdateAsync);
        group.MapDelete("/scopes/{id}", DeleteAsync);
    }

    private static async Task<IResult> ListAsync(
        IOpenIddictScopeManager scopes,
        CancellationToken cancellationToken)
    {
        var values = new List<ScopeView>();
        await foreach (var scope in scopes.ListAsync(cancellationToken: cancellationToken))
            values.Add(await ToViewAsync(scope, scopes, cancellationToken));

        return Results.Ok(values.OrderBy(static value => value.Name));
    }

    private static async Task<IResult> GetAsync(
        string id,
        IOpenIddictScopeManager scopes,
        CancellationToken cancellationToken)
    {
        var scope = await scopes.FindByIdAsync(id, cancellationToken);
        return scope is null
            ? Results.NotFound()
            : Results.Ok(await ToViewAsync(scope, scopes, cancellationToken));
    }

    private static async Task<IResult> CreateAsync(
        ScopeCreate request,
        IOpenIddictScopeManager scopes,
        CancellationToken cancellationToken)
    {
        if (!TryBuildDescriptor(request.Name, request.DisplayName, request.Description, request.Resources, out var descriptor, out var errors))
            return Results.ValidationProblem(errors);
        if (await scopes.FindByNameAsync(descriptor.Name!, cancellationToken) is not null)
            return Results.Conflict(new { message = "A scope with this name already exists." });

        var scope = await scopes.CreateAsync(descriptor, cancellationToken);
        var view = await ToViewAsync(scope, scopes, cancellationToken);
        return Results.Created("/api/configuration/scopes/" + view.Id, view);
    }

    private static async Task<IResult> UpdateAsync(
        string id,
        ScopeUpdate request,
        IOpenIddictScopeManager scopes,
        CancellationToken cancellationToken)
    {
        var scope = await scopes.FindByIdAsync(id, cancellationToken);
        if (scope is null)
            return Results.NotFound();

        var name = await scopes.GetNameAsync(scope, cancellationToken);
        if (!TryBuildDescriptor(name, request.DisplayName, request.Description, request.Resources, out var descriptor, out var errors))
            return Results.ValidationProblem(errors);

        await scopes.UpdateAsync(scope, descriptor, cancellationToken);
        return Results.Ok(await ToViewAsync(scope, scopes, cancellationToken));
    }

    private static async Task<IResult> DeleteAsync(
        string id,
        IOpenIddictScopeManager scopes,
        CancellationToken cancellationToken)
    {
        var scope = await scopes.FindByIdAsync(id, cancellationToken);
        if (scope is null)
            return Results.NotFound();

        await scopes.DeleteAsync(scope, cancellationToken);
        return Results.NoContent();
    }

    private static bool TryBuildDescriptor(
        string? name,
        string? displayName,
        string? description,
        string[]? resources,
        out OpenIddictScopeDescriptor descriptor,
        out Dictionary<string, string[]> errors)
    {
        descriptor = new OpenIddictScopeDescriptor();
        errors = new Dictionary<string, string[]>();
        name = name?.Trim();
        displayName = displayName?.Trim();
        description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (string.IsNullOrEmpty(name))
            errors["name"] = ["Scope name is required."];
        if (string.IsNullOrEmpty(displayName))
            errors["displayName"] = ["Display name is required."];
        if (errors.Count > 0)
            return false;

        descriptor.Name = name;
        descriptor.DisplayName = displayName;
        descriptor.Description = description;
        descriptor.Resources.UnionWith((resources ?? [])
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.Ordinal));
        return true;
    }

    private static async Task<ScopeView> ToViewAsync(
        object scope,
        IOpenIddictScopeManager scopes,
        CancellationToken cancellationToken)
        => new(
            (await scopes.GetIdAsync(scope, cancellationToken)) ?? string.Empty,
            (await scopes.GetNameAsync(scope, cancellationToken)) ?? string.Empty,
            (await scopes.GetDisplayNameAsync(scope, cancellationToken)) ?? string.Empty,
            await scopes.GetDescriptionAsync(scope, cancellationToken),
            (await scopes.GetResourcesAsync(scope, cancellationToken)).Order(StringComparer.Ordinal).ToArray());
}
