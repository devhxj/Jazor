using System.Security.Cryptography;
using OpenIddict.Abstractions;

namespace JazorAdmin.Features.Sso;

internal static class AppEndpoints
{
    private static readonly IReadOnlyDictionary<string, string> EndpointPermissions =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["authorization"] = OpenIddictConstants.Permissions.Endpoints.Authorization,
            ["end_session"] = OpenIddictConstants.Permissions.Endpoints.EndSession,
            ["introspection"] = OpenIddictConstants.Permissions.Endpoints.Introspection,
            ["revocation"] = OpenIddictConstants.Permissions.Endpoints.Revocation,
            ["token"] = OpenIddictConstants.Permissions.Endpoints.Token
        };

    private static readonly IReadOnlyDictionary<string, string> GrantPermissions =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["authorization_code"] = OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
            ["client_credentials"] = OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
            ["refresh_token"] = OpenIddictConstants.Permissions.GrantTypes.RefreshToken
        };

    private static readonly IReadOnlyDictionary<string, string> ResponsePermissions =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["code"] = OpenIddictConstants.Permissions.ResponseTypes.Code
        };

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/applications", ListAsync);
        group.MapGet("/applications/{id}", GetAsync);
        group.MapPost("/applications", CreateAsync);
        group.MapPut("/applications/{id}", UpdateAsync);
        group.MapDelete("/applications/{id}", DeleteAsync);
        group.MapPost("/applications/{id}/secret", RotateSecretAsync);
    }

    private static async Task<IResult> ListAsync(
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        var values = new List<AppView>();
        await foreach (var application in applications.ListAsync(cancellationToken: cancellationToken))
            values.Add(await ToViewAsync(application, applications, cancellationToken));

        return Results.Ok(values.OrderBy(static value => value.ClientId));
    }

    private static async Task<IResult> GetAsync(
        string id,
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        var application = await applications.FindByIdAsync(id, cancellationToken);
        return application is null
            ? Results.NotFound()
            : Results.Ok(await ToViewAsync(application, applications, cancellationToken));
    }

    private static async Task<IResult> CreateAsync(
        AppCreate request,
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        var input = AppInput.From(request);
        if (!TryBuildDescriptor(input, out var descriptor, out var errors))
            return Results.ValidationProblem(errors);

        var clientId = request.ClientId?.Trim();
        if (string.IsNullOrEmpty(clientId))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["clientId"] = ["Client ID is required."] });
        if (await applications.FindByClientIdAsync(clientId, cancellationToken) is not null)
            return Results.Conflict(new { message = "An application with this client ID already exists." });

        descriptor.ClientId = clientId;
        var secret = descriptor.ClientType == OpenIddictConstants.ClientTypes.Confidential
            ? CreateSecret()
            : null;
        descriptor.ClientSecret = secret;

        var application = await applications.CreateAsync(descriptor, cancellationToken);
        var view = await ToViewAsync(application, applications, cancellationToken);
        return Results.Created("/api/sso/applications/" + view.Id, new AppSaved(view, secret));
    }

    private static async Task<IResult> UpdateAsync(
        string id,
        AppUpdate request,
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        var application = await applications.FindByIdAsync(id, cancellationToken);
        if (application is null)
            return Results.NotFound();

        var input = AppInput.From(request);
        if (!TryBuildDescriptor(input, out var requested, out var errors))
            return Results.ValidationProblem(errors);

        // Populate first so opaque OpenIddict state, including an existing secret hash, survives edits.
        // 先读取完整 descriptor，避免编辑普通配置时破坏 OpenIddict 管理的密钥状态。
        var descriptor = new OpenIddictApplicationDescriptor();
        await applications.PopulateAsync(descriptor, application, cancellationToken);
        var previousClientType = descriptor.ClientType;
        CopyConfiguration(requested, descriptor);
        await applications.UpdateAsync(application, descriptor, cancellationToken);

        string? secret = null;
        if (descriptor.ClientType == OpenIddictConstants.ClientTypes.Confidential &&
            previousClientType != OpenIddictConstants.ClientTypes.Confidential)
        {
            secret = CreateSecret();
            await applications.UpdateAsync(application, secret, cancellationToken);
        }
        else if (descriptor.ClientType == OpenIddictConstants.ClientTypes.Public &&
                 previousClientType == OpenIddictConstants.ClientTypes.Confidential)
        {
            await applications.UpdateAsync(application, secret: null, cancellationToken);
        }

        return Results.Ok(new AppSaved(
            await ToViewAsync(application, applications, cancellationToken),
            secret));
    }

    private static async Task<IResult> DeleteAsync(
        string id,
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        var application = await applications.FindByIdAsync(id, cancellationToken);
        if (application is null)
            return Results.NotFound();

        await applications.DeleteAsync(application, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RotateSecretAsync(
        string id,
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        var application = await applications.FindByIdAsync(id, cancellationToken);
        if (application is null)
            return Results.NotFound();
        if (!await applications.HasClientTypeAsync(
                application,
                OpenIddictConstants.ClientTypes.Confidential,
                cancellationToken))
        {
            return Results.Conflict(new { message = "Public applications do not use a client secret." });
        }

        var secret = CreateSecret();
        await applications.UpdateAsync(application, secret, cancellationToken);
        return Results.Ok(new SecretView(secret));
    }

    private static bool TryBuildDescriptor(
        AppInput input,
        out OpenIddictApplicationDescriptor descriptor,
        out Dictionary<string, string[]> errors)
    {
        descriptor = new OpenIddictApplicationDescriptor();
        errors = new Dictionary<string, string[]>();

        var displayName = input.DisplayName?.Trim();
        if (string.IsNullOrEmpty(displayName))
            errors["displayName"] = ["Display name is required."];

        var applicationType = NormalizeChoice(
            input.ApplicationType,
            [OpenIddictConstants.ApplicationTypes.Web, OpenIddictConstants.ApplicationTypes.Native],
            "applicationType",
            errors);
        var clientType = NormalizeChoice(
            input.ClientType,
            [OpenIddictConstants.ClientTypes.Public, OpenIddictConstants.ClientTypes.Confidential],
            "clientType",
            errors);
        var consentType = NormalizeChoice(
            input.ConsentType,
            [
                OpenIddictConstants.ConsentTypes.Explicit,
                OpenIddictConstants.ConsentTypes.Implicit,
                OpenIddictConstants.ConsentTypes.Systematic
            ],
            "consentType",
            errors);

        var endpointKeys = NormalizeKeys(input.Endpoints, EndpointPermissions, "endpoints", errors);
        var grantKeys = NormalizeKeys(input.GrantTypes, GrantPermissions, "grantTypes", errors);
        var responseKeys = NormalizeKeys(input.ResponseTypes, ResponsePermissions, "responseTypes", errors);
        var scopes = NormalizeValues(input.Scopes);
        var redirectUris = NormalizeUris(input.RedirectUris, applicationType, "redirectUris", errors);
        var postLogoutRedirectUris = NormalizeUris(input.PostLogoutRedirectUris, applicationType, "postLogoutRedirectUris", errors);

        if (grantKeys.Contains("authorization_code", StringComparer.Ordinal))
        {
            if (!endpointKeys.Contains("authorization", StringComparer.Ordinal) ||
                !endpointKeys.Contains("token", StringComparer.Ordinal) ||
                !responseKeys.Contains("code", StringComparer.Ordinal))
            {
                errors["grantTypes"] = ["Authorization code requires authorization/token endpoints and the code response type."];
            }
            if (redirectUris.Length == 0)
                errors["redirectUris"] = ["Authorization code applications require at least one redirect URI."];
        }
        if (grantKeys.Contains("client_credentials", StringComparer.Ordinal))
        {
            if (clientType != OpenIddictConstants.ClientTypes.Confidential)
                errors["clientType"] = ["Client credentials requires a confidential application."];
            if (!endpointKeys.Contains("token", StringComparer.Ordinal))
                errors["grantTypes"] = ["Client credentials requires the token endpoint."];
        }
        if (grantKeys.Contains("refresh_token", StringComparer.Ordinal) &&
            !endpointKeys.Contains("token", StringComparer.Ordinal))
        {
            errors["grantTypes"] = ["Refresh token requires the token endpoint."];
        }
        if (input.RequirePkce && !grantKeys.Contains("authorization_code", StringComparer.Ordinal))
            errors["requirePkce"] = ["PKCE can only be required for authorization code applications."];

        if (errors.Count > 0)
            return false;

        descriptor.DisplayName = displayName;
        descriptor.ApplicationType = applicationType;
        descriptor.ClientType = clientType;
        descriptor.ConsentType = consentType;
        descriptor.RedirectUris.UnionWith(redirectUris);
        descriptor.PostLogoutRedirectUris.UnionWith(postLogoutRedirectUris);
        descriptor.Permissions.UnionWith(endpointKeys.Select(key => EndpointPermissions[key]));
        descriptor.Permissions.UnionWith(grantKeys.Select(key => GrantPermissions[key]));
        descriptor.Permissions.UnionWith(responseKeys.Select(key => ResponsePermissions[key]));
        descriptor.Permissions.UnionWith(scopes.Select(static scope => OpenIddictConstants.Permissions.Prefixes.Scope + scope));
        if (input.RequirePkce)
            descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
        return true;
    }

    private static void CopyConfiguration(
        OpenIddictApplicationDescriptor source,
        OpenIddictApplicationDescriptor target)
    {
        target.DisplayName = source.DisplayName;
        target.ApplicationType = source.ApplicationType;
        target.ClientType = source.ClientType;
        target.ConsentType = source.ConsentType;
        target.RedirectUris.Clear();
        target.RedirectUris.UnionWith(source.RedirectUris);
        target.PostLogoutRedirectUris.Clear();
        target.PostLogoutRedirectUris.UnionWith(source.PostLogoutRedirectUris);
        target.Permissions.Clear();
        target.Permissions.UnionWith(source.Permissions);
        target.Requirements.Clear();
        target.Requirements.UnionWith(source.Requirements);
    }

    private static async Task<AppView> ToViewAsync(
        object application,
        IOpenIddictApplicationManager applications,
        CancellationToken cancellationToken)
    {
        var permissions = await applications.GetPermissionsAsync(application, cancellationToken);
        var endpoints = SelectKeys(permissions, EndpointPermissions);
        var grants = SelectKeys(permissions, GrantPermissions);
        var responses = SelectKeys(permissions, ResponsePermissions);
        var scopes = permissions
            .Where(static permission => permission.StartsWith(OpenIddictConstants.Permissions.Prefixes.Scope, StringComparison.Ordinal))
            .Select(static permission => permission[OpenIddictConstants.Permissions.Prefixes.Scope.Length..])
            .Order(StringComparer.Ordinal)
            .ToArray();
        var requirements = await applications.GetRequirementsAsync(application, cancellationToken);

        return new AppView(
            (await applications.GetIdAsync(application, cancellationToken)) ?? string.Empty,
            (await applications.GetClientIdAsync(application, cancellationToken)) ?? string.Empty,
            (await applications.GetDisplayNameAsync(application, cancellationToken)) ?? string.Empty,
            GetProfile(endpoints, grants),
            (await applications.GetApplicationTypeAsync(application, cancellationToken)) ?? OpenIddictConstants.ApplicationTypes.Web,
            (await applications.GetClientTypeAsync(application, cancellationToken)) ?? OpenIddictConstants.ClientTypes.Public,
            (await applications.GetConsentTypeAsync(application, cancellationToken)) ?? OpenIddictConstants.ConsentTypes.Explicit,
            requirements.Contains(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange, StringComparer.Ordinal),
            (await applications.GetRedirectUrisAsync(application, cancellationToken)).Order(StringComparer.Ordinal).ToArray(),
            (await applications.GetPostLogoutRedirectUrisAsync(application, cancellationToken)).Order(StringComparer.Ordinal).ToArray(),
            endpoints,
            grants,
            responses,
            scopes);
    }

    private static string[] SelectKeys(
        IReadOnlyCollection<string> permissions,
        IReadOnlyDictionary<string, string> map)
        => map.Where(pair => permissions.Contains(pair.Value, StringComparer.Ordinal))
            .Select(static pair => pair.Key)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string GetProfile(string[] endpoints, string[] grants)
    {
        if (endpoints.Contains("introspection", StringComparer.Ordinal) &&
            !endpoints.Contains("authorization", StringComparer.Ordinal))
        {
            return "api";
        }

        return grants.Contains("client_credentials", StringComparer.Ordinal) &&
               !grants.Contains("authorization_code", StringComparer.Ordinal)
            ? "machine"
            : "interactive";
    }

    private static string? NormalizeChoice(
        string? value,
        string[] allowed,
        string key,
        Dictionary<string, string[]> errors)
    {
        var normalized = value?.Trim();
        if (normalized is null || !allowed.Contains(normalized, StringComparer.Ordinal))
            errors[key] = ["Unsupported value."];
        return normalized;
    }

    private static string[] NormalizeKeys(
        string[]? values,
        IReadOnlyDictionary<string, string> allowed,
        string key,
        Dictionary<string, string[]> errors)
    {
        var normalized = NormalizeValues(values);
        if (normalized.Any(value => !allowed.ContainsKey(value)))
            errors[key] = ["One or more values are unsupported."];
        return normalized;
    }

    private static string[] NormalizeValues(string[]? values)
        => (values ?? [])
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static Uri[] NormalizeUris(
        string[]? values,
        string? applicationType,
        string key,
        Dictionary<string, string[]> errors)
    {
        var normalized = NormalizeValues(values);
        var uris = new List<Uri>(normalized.Length);
        foreach (var value in normalized)
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                applicationType == OpenIddictConstants.ApplicationTypes.Web &&
                uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                errors[key] = [applicationType == OpenIddictConstants.ApplicationTypes.Native
                    ? "URIs must be absolute."
                    : "Web application URIs must use HTTP or HTTPS."];
                return [];
            }
            uris.Add(uri);
        }
        return uris.ToArray();
    }

    private static string CreateSecret()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private sealed record AppInput(
        string DisplayName,
        string ApplicationType,
        string ClientType,
        string ConsentType,
        bool RequirePkce,
        string[] RedirectUris,
        string[] PostLogoutRedirectUris,
        string[] Endpoints,
        string[] GrantTypes,
        string[] ResponseTypes,
        string[] Scopes)
    {
        public static AppInput From(AppCreate value)
            => new(
                value.DisplayName,
                value.ApplicationType,
                value.ClientType,
                value.ConsentType,
                value.RequirePkce,
                value.RedirectUris,
                value.PostLogoutRedirectUris,
                value.Endpoints,
                value.GrantTypes,
                value.ResponseTypes,
                value.Scopes);

        public static AppInput From(AppUpdate value)
            => new(
                value.DisplayName,
                value.ApplicationType,
                value.ClientType,
                value.ConsentType,
                value.RequirePkce,
                value.RedirectUris,
                value.PostLogoutRedirectUris,
                value.Endpoints,
                value.GrantTypes,
                value.ResponseTypes,
                value.Scopes);
    }
}
