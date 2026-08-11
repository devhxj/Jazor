namespace Jazor.AspNetCore;

/// <summary>Describes one server-rendered Vue root component and its serialized props.</summary>
public sealed record JazorSSRRequest(string ModulePath, object? Props = null);

/// <summary>Contains the HTML and prop payload produced by one isolated SSR execution.</summary>
public sealed record JazorSSRRenderResult(
    string ModulePath,
    string Html,
    string SerializedProps);
