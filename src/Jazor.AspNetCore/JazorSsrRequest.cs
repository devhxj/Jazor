using System.Text.Json.Serialization;

namespace Jazor.AspNetCore;

/// <summary>Describes one application-level Vue provider carried into SSR and hydration.</summary>
public sealed record JazorSsrProvider(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] object? Value);

/// <summary>Describes one server-rendered Vue root component and its serialized props.</summary>
public sealed record JazorSsrRequest(
    string ModulePath,
    object? Props = null,
    IReadOnlyList<JazorSsrProvider>? Providers = null);

/// <summary>Contains the HTML and serialized props/providers produced by one SSR application instance.</summary>
public sealed record JazorSsrRenderResult(
    string ModulePath,
    string Html,
    string SerializedProps,
    string SerializedProviders = "[]");
