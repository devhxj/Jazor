namespace ECMAScript;

/// <summary>
/// CapturedMouseEventInit
/// </summary>
[ECMAScript]
[Description("@#CapturedMouseEventInit")]
public record CapturedMouseEventInit(
    [property: Description("@#surfaceX")]int SurfaceX = -1,
	[property: Description("@#surfaceY")]int SurfaceY = -1): EventInit;

/// <summary>
/// CapturedMouseEvent
/// </summary>
[ECMAScript]
[Description("@#CapturedMouseEvent")]
public class CapturedMouseEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern CapturedMouseEvent(string type, CapturedMouseEventInit eventInitDict);

    /// <summary>
    /// surfaceX
    /// </summary>
    [Description("@#surfaceX")]
    public extern int SurfaceX { get; }

    /// <summary>
    /// surfaceY
    /// </summary>
    [Description("@#surfaceY")]
    public extern int SurfaceY { get; }
}

/// <summary>
/// CaptureController
/// </summary>
[ECMAScript]
[Description("@#CaptureController")]
public partial class CaptureController : EventTarget
{
    /// <summary>
    /// oncapturedmousechange
    /// </summary>
    [Description("@#oncapturedmousechange")]
    public extern EventHandler Oncapturedmousechange { get; set; }

    /// <summary>
    /// getSupportedZoomLevels
    /// </summary>
    [Description("@#getSupportedZoomLevels")]
    public extern int[] GetSupportedZoomLevels();

    /// <summary>
    /// zoomLevel
    /// </summary>
    [Description("@#zoomLevel")]
    public extern int? ZoomLevel { get; }

    /// <summary>
    /// increaseZoomLevel
    /// </summary>
    [Description("@#increaseZoomLevel")]
    public extern PromiseResult<object> IncreaseZoomLevel();

    /// <summary>
    /// decreaseZoomLevel
    /// </summary>
    [Description("@#decreaseZoomLevel")]
    public extern PromiseResult<object> DecreaseZoomLevel();

    /// <summary>
    /// resetZoomLevel
    /// </summary>
    [Description("@#resetZoomLevel")]
    public extern PromiseResult<object> ResetZoomLevel();

    /// <summary>
    /// onzoomlevelchange
    /// </summary>
    [Description("@#onzoomlevelchange")]
    public extern EventHandler Onzoomlevelchange { get; set; }

    /// <summary>
    /// Constructor 
    /// </summary>
    public extern CaptureController();

    /// <summary>
    /// forwardWheel
    /// </summary>
    /// <param name="element">element</param>
    [Description("@#forwardWheel")]
    public extern PromiseResult<object> ForwardWheel(HTMLElement? element);

    /// <summary>
    /// setFocusBehavior
    /// </summary>
    /// <param name="focusBehavior">focusBehavior</param>
    [Description("@#setFocusBehavior")]
    public extern void SetFocusBehavior(CaptureStartFocusBehavior focusBehavior);
}