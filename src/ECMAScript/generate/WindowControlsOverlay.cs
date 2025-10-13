namespace ECMAScript;

/// <summary>
/// WindowControlsOverlayGeometryChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#WindowControlsOverlayGeometryChangeEventInit")]
public record WindowControlsOverlayGeometryChangeEventInit(
    [property: Description("@#titlebarAreaRect")]DOMRect? TitlebarAreaRect = default,
	[property: Description("@#visible")]bool Visible = false): EventInit;

/// <summary>
/// WindowControlsOverlay
/// </summary>
[ECMAScript]
[Description("@#WindowControlsOverlay")]
public class WindowControlsOverlay : EventTarget
{
    /// <summary>
    /// visible
    /// </summary>
    [Description("@#visible")]
    public extern bool Visible { get; }

    /// <summary>
    /// getTitlebarAreaRect
    /// </summary>
    [Description("@#getTitlebarAreaRect")]
    public extern DOMRect GetTitlebarAreaRect();

    /// <summary>
    /// ongeometrychange
    /// </summary>
    [Description("@#ongeometrychange")]
    public extern EventHandler Ongeometrychange { get; set; }
}

/// <summary>
/// WindowControlsOverlayGeometryChangeEvent
/// </summary>
[ECMAScript]
[Description("@#WindowControlsOverlayGeometryChangeEvent")]
public class WindowControlsOverlayGeometryChangeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern WindowControlsOverlayGeometryChangeEvent(string type, WindowControlsOverlayGeometryChangeEventInit eventInitDict);

    /// <summary>
    /// titlebarAreaRect
    /// </summary>
    [Description("@#titlebarAreaRect")]
    public extern DOMRect TitlebarAreaRect { get; }

    /// <summary>
    /// visible
    /// </summary>
    [Description("@#visible")]
    public extern bool Visible { get; }
}