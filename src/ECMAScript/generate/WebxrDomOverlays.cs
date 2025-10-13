namespace ECMAScript;

/// <summary>
/// XRDOMOverlayInit
/// </summary>
[ECMAScript]
[Description("@#XRDOMOverlayInit")]
public record XRDOMOverlayInit(
    [property: Description("@#root")]Element? Root = default);

/// <summary>
/// XRDOMOverlayState
/// </summary>
[ECMAScript]
[Description("@#XRDOMOverlayState")]
public record XRDOMOverlayState(
    [property: Description("@#type")]XRDOMOverlayType? Type = default);