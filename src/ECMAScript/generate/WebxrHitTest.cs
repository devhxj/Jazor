namespace ECMAScript;

/// <summary>
/// XRHitTestOptionsInit
/// </summary>
[ECMAScript]
[Description("@#XRHitTestOptionsInit")]
public record XRHitTestOptionsInit(
    [property: Description("@#space")]XRSpace? Space = default,
	[property: Description("@#entityTypes")]XRHitTestTrackableType[]? EntityTypes = default,
	[property: Description("@#offsetRay")]XRRay? OffsetRay = default);

/// <summary>
/// XRTransientInputHitTestOptionsInit
/// </summary>
[ECMAScript]
[Description("@#XRTransientInputHitTestOptionsInit")]
public record XRTransientInputHitTestOptionsInit(
    [property: Description("@#profile")]string? Profile = default,
	[property: Description("@#entityTypes")]XRHitTestTrackableType[]? EntityTypes = default,
	[property: Description("@#offsetRay")]XRRay? OffsetRay = default);

/// <summary>
/// XRRayDirectionInit
/// </summary>
[ECMAScript]
[Description("@#XRRayDirectionInit")]
public record XRRayDirectionInit(
    [property: Description("@#x")]double X = 0d,
	[property: Description("@#y")]double Y = 0d,
	[property: Description("@#z")]double Z = -1d,
	[property: Description("@#w")]double W = 0d);

/// <summary>
/// XRHitTestSource
/// </summary>
[ECMAScript]
[Description("@#XRHitTestSource")]
public class XRHitTestSource
{
    /// <summary>
    /// cancel
    /// </summary>
    [Description("@#cancel")]
    public extern void Cancel();
}

/// <summary>
/// XRTransientInputHitTestSource
/// </summary>
[ECMAScript]
[Description("@#XRTransientInputHitTestSource")]
public class XRTransientInputHitTestSource
{
    /// <summary>
    /// cancel
    /// </summary>
    [Description("@#cancel")]
    public extern void Cancel();
}

/// <summary>
/// XRTransientInputHitTestResult
/// </summary>
[ECMAScript]
[Description("@#XRTransientInputHitTestResult")]
public class XRTransientInputHitTestResult
{
    /// <summary>
    /// inputSource
    /// </summary>
    [Description("@#inputSource")]
    public extern XRInputSource InputSource { get; }

    /// <summary>
    /// results
    /// </summary>
    [Description("@#results")]
    public extern FrozenSet<XRHitTestResult> Results { get; }
}

/// <summary>
/// XRRay
/// </summary>
[ECMAScript]
[Description("@#XRRay")]
public class XRRay
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="origin">origin</param>
    /// <param name="direction">direction</param>
    public extern XRRay(DOMPointInit origin, XRRayDirectionInit direction);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="transform">transform</param>
    public extern XRRay(XRRigidTransform transform);

    /// <summary>
    /// origin
    /// </summary>
    [Description("@#origin")]
    public extern DOMPointReadOnly Origin { get; }

    /// <summary>
    /// direction
    /// </summary>
    [Description("@#direction")]
    public extern DOMPointReadOnly Direction { get; }

    /// <summary>
    /// matrix
    /// </summary>
    [Description("@#matrix")]
    public extern Float32Array Matrix { get; }
}