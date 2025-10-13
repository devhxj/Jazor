namespace ECMAScript;

/// <summary>
/// VirtualKeyboard
/// </summary>
[ECMAScript]
[Description("@#VirtualKeyboard")]
public class VirtualKeyboard : EventTarget
{
    /// <summary>
    /// show
    /// </summary>
    [Description("@#show")]
    public extern void Show();

    /// <summary>
    /// hide
    /// </summary>
    [Description("@#hide")]
    public extern void Hide();

    /// <summary>
    /// boundingRect
    /// </summary>
    [Description("@#boundingRect")]
    public extern DOMRect BoundingRect { get; }

    /// <summary>
    /// overlaysContent
    /// </summary>
    [Description("@#overlaysContent")]
    public extern bool OverlaysContent { get; set; }

    /// <summary>
    /// ongeometrychange
    /// </summary>
    [Description("@#ongeometrychange")]
    public extern EventHandler Ongeometrychange { get; set; }
}