namespace ECMAScript;

/// <summary>
/// ScreenOrientation
/// </summary>
[ECMAScript]
[Description("@#ScreenOrientation")]
public class ScreenOrientation : EventTarget
{
    /// <summary>
    /// lock
    /// </summary>
    /// <param name="orientation">orientation</param>
    [Description("@#lock")]
    public extern PromiseResult<object> Lock(OrientationLockType orientation);

    /// <summary>
    /// unlock
    /// </summary>
    [Description("@#unlock")]
    public extern void Unlock();

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern OrientationType Type { get; }

    /// <summary>
    /// angle
    /// </summary>
    [Description("@#angle")]
    public extern ushort Angle { get; }

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }
}