namespace ECMAScript;

/// <summary>
/// XRInputSource
/// </summary>
[ECMAScript]
[Description("@#XRInputSource")]
public partial class XRInputSource
{
    /// <summary>
    /// gamepad
    /// </summary>
    [Description("@#gamepad")]
    public extern Gamepad? Gamepad { get; }

    /// <summary>
    /// hand
    /// </summary>
    [Description("@#hand")]
    public extern XRHand? Hand { get; }

    /// <summary>
    /// handedness
    /// </summary>
    [Description("@#handedness")]
    public extern XRHandedness Handedness { get; }

    /// <summary>
    /// targetRayMode
    /// </summary>
    [Description("@#targetRayMode")]
    public extern XRTargetRayMode TargetRayMode { get; }

    /// <summary>
    /// targetRaySpace
    /// </summary>
    [Description("@#targetRaySpace")]
    public extern XRSpace TargetRaySpace { get; }

    /// <summary>
    /// gripSpace
    /// </summary>
    [Description("@#gripSpace")]
    public extern XRSpace? GripSpace { get; }

    /// <summary>
    /// profiles
    /// </summary>
    [Description("@#profiles")]
    public extern FrozenSet<string> Profiles { get; }

    /// <summary>
    /// skipRendering
    /// </summary>
    [Description("@#skipRendering")]
    public extern bool SkipRendering { get; }
}