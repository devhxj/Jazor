namespace ECMAScript;

/// <summary>
/// Keyboard
/// </summary>
[ECMAScript]
[Description("@#Keyboard")]
public partial class Keyboard : EventTarget
{
    /// <summary>
    /// lock
    /// </summary>
    /// <param name="keyCodes">keyCodes</param>
    [Description("@#lock")]
    public extern PromiseResult<object> Lock(string[]? keyCodes = default);

    /// <summary>
    /// unlock
    /// </summary>
    [Description("@#unlock")]
    public extern void Unlock();

    /// <summary>
    /// getLayoutMap
    /// </summary>
    [Description("@#getLayoutMap")]
    public extern PromiseResult<KeyboardLayoutMap> GetLayoutMap();

    /// <summary>
    /// onlayoutchange
    /// </summary>
    [Description("@#onlayoutchange")]
    public extern EventHandler Onlayoutchange { get; set; }
}