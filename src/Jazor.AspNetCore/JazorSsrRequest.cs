namespace Jazor.AspNetCore;

/// <summary>Describes one server-rendered Vue root component and its serialized props.</summary>
public sealed record JazorSsrRequest(string ModulePath, object? Props = null);

/// <summary>Contains the HTML and prop payload produced by one SSR application instance.</summary>
public sealed record JazorSsrRenderResult(
    string ModulePath,
    string Html,
    string SerializedProps);
