namespace ECMAScript;

/// <summary>
/// ScreenDetails
/// </summary>
[ECMAScript]
[Description("@#ScreenDetails")]
public class ScreenDetails : EventTarget
{
    /// <summary>
    /// screens
    /// </summary>
    [Description("@#screens")]
    public extern FrozenSet<ScreenDetailed> Screens { get; }

    /// <summary>
    /// currentScreen
    /// </summary>
    [Description("@#currentScreen")]
    public extern ScreenDetailed CurrentScreen { get; }

    /// <summary>
    /// onscreenschange
    /// </summary>
    [Description("@#onscreenschange")]
    public extern EventHandler Onscreenschange { get; set; }

    /// <summary>
    /// oncurrentscreenchange
    /// </summary>
    [Description("@#oncurrentscreenchange")]
    public extern EventHandler Oncurrentscreenchange { get; set; }
}

/// <summary>
/// ScreenDetailed
/// </summary>
[ECMAScript]
[Description("@#ScreenDetailed")]
public class ScreenDetailed : Screen
{
    /// <summary>
    /// availLeft
    /// </summary>
    [Description("@#availLeft")]
    public extern int AvailLeft { get; }

    /// <summary>
    /// availTop
    /// </summary>
    [Description("@#availTop")]
    public extern int AvailTop { get; }

    /// <summary>
    /// left
    /// </summary>
    [Description("@#left")]
    public extern int Left { get; }

    /// <summary>
    /// top
    /// </summary>
    [Description("@#top")]
    public extern int Top { get; }

    /// <summary>
    /// isPrimary
    /// </summary>
    [Description("@#isPrimary")]
    public extern bool IsPrimary { get; }

    /// <summary>
    /// isInternal
    /// </summary>
    [Description("@#isInternal")]
    public extern bool IsInternal { get; }

    /// <summary>
    /// devicePixelRatio
    /// </summary>
    [Description("@#devicePixelRatio")]
    public extern float DevicePixelRatio { get; }

    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; }
}