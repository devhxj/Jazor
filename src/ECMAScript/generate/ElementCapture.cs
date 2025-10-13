namespace ECMAScript;

/// <summary>
/// RestrictionTarget
/// </summary>
[ECMAScript]
[Description("@#RestrictionTarget")]
public class RestrictionTarget
{
    /// <summary>
    /// fromElement
    /// </summary>
    /// <param name="element">element</param>
    [Description("@#fromElement")]
    public extern static PromiseResult<RestrictionTarget> FromElement(Element element);
}

/// <summary>
/// BrowserCaptureMediaStreamTrack
/// </summary>
[ECMAScript]
[Description("@#BrowserCaptureMediaStreamTrack")]
public partial class BrowserCaptureMediaStreamTrack : MediaStreamTrack
{
    /// <summary>
    /// restrictTo
    /// </summary>
    /// <param name="restrictionTarget">RestrictionTarget</param>
    [Description("@#restrictTo")]
    public extern PromiseResult<object> RestrictTo(RestrictionTarget? restrictionTarget);

    /// <summary>
    /// cropTo
    /// </summary>
    /// <param name="cropTarget">cropTarget</param>
    [Description("@#cropTo")]
    public extern PromiseResult<object> CropTo(CropTarget? cropTarget);

    /// <summary>
    /// clone
    /// </summary>
    [Description("@#clone")]
    public extern new BrowserCaptureMediaStreamTrack Clone();
}