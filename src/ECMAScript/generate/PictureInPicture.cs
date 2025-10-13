namespace ECMAScript;

/// <summary>
/// PictureInPictureEventInit
/// </summary>
[ECMAScript]
[Description("@#PictureInPictureEventInit")]
public record PictureInPictureEventInit(
    [property: Description("@#pictureInPictureWindow")]PictureInPictureWindow? PictureInPictureWindow = default): EventInit;

/// <summary>
/// PictureInPictureWindow
/// </summary>
[ECMAScript]
[Description("@#PictureInPictureWindow")]
public class PictureInPictureWindow : EventTarget
{
    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern int Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern int Height { get; }

    /// <summary>
    /// onresize
    /// </summary>
    [Description("@#onresize")]
    public extern EventHandler Onresize { get; set; }
}

/// <summary>
/// PictureInPictureEvent
/// </summary>
[ECMAScript]
[Description("@#PictureInPictureEvent")]
public class PictureInPictureEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PictureInPictureEvent(string type, PictureInPictureEventInit eventInitDict);

    /// <summary>
    /// pictureInPictureWindow
    /// </summary>
    [Description("@#pictureInPictureWindow")]
    public extern PictureInPictureWindow PictureInPictureWindow { get; }
}