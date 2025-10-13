namespace ECMAScript;

/// <summary>
/// WakeLock
/// </summary>
[ECMAScript]
[Description("@#WakeLock")]
public class WakeLock
{
    /// <summary>
    /// request
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#request")]
    public extern PromiseResult<WakeLockSentinel> Request(WakeLockType type = WakeLockType.Screen);
}

/// <summary>
/// WakeLockSentinel
/// </summary>
[ECMAScript]
[Description("@#WakeLockSentinel")]
public class WakeLockSentinel : EventTarget
{
    /// <summary>
    /// released
    /// </summary>
    [Description("@#released")]
    public extern bool Released { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern WakeLockType Type { get; }

    /// <summary>
    /// release
    /// </summary>
    [Description("@#release")]
    public extern PromiseResult<object> Release();

    /// <summary>
    /// onrelease
    /// </summary>
    [Description("@#onrelease")]
    public extern EventHandler Onrelease { get; set; }
}