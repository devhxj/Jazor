namespace ECMAScript;

/// <summary>
/// DevicePosture
/// </summary>
[ECMAScript]
[Description("@#DevicePosture")]
public class DevicePosture : EventTarget
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern DevicePostureType Type { get; }

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }
}