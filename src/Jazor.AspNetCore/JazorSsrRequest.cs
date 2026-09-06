using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Jazor.AspNetCore;

/// <summary>Describes one application-level Vue provider carried into SSR and hydration.</summary>
public sealed record JazorSsrProvider(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] object? Value);

/// <summary>Closed, host-owned authentication snapshot for one SSR request.</summary>
public sealed record JazorAuthenticationState(
    [property: JsonPropertyName("status")] JazorAuthenticationStatus Status,
    [property: JsonPropertyName("subject")] string? Subject = null,
    [property: JsonPropertyName("claims")] IReadOnlyDictionary<string, string[]>? Claims = null)
{
    public const string ProviderKey = "jazor:auth-state";

    public static JazorAuthenticationState FromPrincipal(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var identity = principal.Identity;
        if (identity?.IsAuthenticated != true)
            return new(JazorAuthenticationStatus.Anonymous);

        var claims = principal.Claims
            .GroupBy(static claim => claim.Type, StringComparer.Ordinal)
            .ToDictionary(
                static group => group.Key,
                static group => group.Select(static claim => claim.Value).ToArray(),
                StringComparer.Ordinal);
        return new(
            JazorAuthenticationStatus.Authenticated,
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? identity.Name,
            claims);
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JazorAuthenticationStatus
{
    Anonymous,
    Authenticated,
    Expired,
    Forbidden
}

/// <summary>Describes one server-rendered Vue root component and its serialized props.</summary>
public sealed record JazorSsrRequest(
    string ModulePath,
    object? Props = null,
    IReadOnlyList<JazorSsrProvider>? Providers = null,
    JazorAuthenticationState? Authentication = null);

/// <summary>
/// Versioned state handoff shared by the SSR runner and browser hydration entry.
/// Props/providers remain strongly typed request inputs; this record is the transport envelope.
/// </summary>
public sealed record JazorSsrStateEnvelope(
    [property: JsonPropertyName("schema")] string Schema,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("props")] object? Props,
    [property: JsonPropertyName("providers")] IReadOnlyList<JazorSsrProvider> Providers,
    [property: JsonPropertyName("authentication")] JazorAuthenticationState? Authentication)
{
    public const string CurrentSchema = "jazor-ssr-state";
    public const int CurrentVersion = 1;

    public static JazorSsrStateEnvelope Create(JazorSsrRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var providers = (request.Providers ?? []).ToList();
        if (providers.Any(static provider => provider is null || string.IsNullOrWhiteSpace(provider.Key)))
            throw new ArgumentException("Jazor SSR providers must have non-empty keys.", nameof(request));

        if (request.Authentication is not null &&
            providers.Any(static provider => string.Equals(provider.Key, JazorAuthenticationState.ProviderKey, StringComparison.Ordinal)))
            throw new ArgumentException(
                "Jazor SSR authentication provider key is reserved and cannot be supplied twice.",
                nameof(request));

        if (request.Authentication is not null)
            providers.Add(new JazorSsrProvider(JazorAuthenticationState.ProviderKey, request.Authentication));

        return new JazorSsrStateEnvelope(
            CurrentSchema,
            CurrentVersion,
            request.Props,
            providers,
            request.Authentication);
    }
}

/// <summary>Contains the HTML and serialized state produced by one SSR application instance.</summary>
public sealed record JazorSsrRenderResult(
    string ModulePath,
    string Html,
    string SerializedProps,
    string SerializedProviders = "[]",
    string SerializedState = "{}");
