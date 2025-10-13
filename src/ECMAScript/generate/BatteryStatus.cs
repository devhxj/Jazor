namespace ECMAScript;

/// <summary>
/// BatteryManager
/// </summary>
[ECMAScript]
[Description("@#BatteryManager")]
public class BatteryManager : EventTarget
{
    /// <summary>
    /// charging
    /// </summary>
    [Description("@#charging")]
    public extern bool Charging { get; }

    /// <summary>
    /// chargingTime
    /// </summary>
    [Description("@#chargingTime")]
    public extern double ChargingTime { get; }

    /// <summary>
    /// dischargingTime
    /// </summary>
    [Description("@#dischargingTime")]
    public extern double DischargingTime { get; }

    /// <summary>
    /// level
    /// </summary>
    [Description("@#level")]
    public extern double Level { get; }

    /// <summary>
    /// onchargingchange
    /// </summary>
    [Description("@#onchargingchange")]
    public extern EventHandler Onchargingchange { get; set; }

    /// <summary>
    /// onchargingtimechange
    /// </summary>
    [Description("@#onchargingtimechange")]
    public extern EventHandler Onchargingtimechange { get; set; }

    /// <summary>
    /// ondischargingtimechange
    /// </summary>
    [Description("@#ondischargingtimechange")]
    public extern EventHandler Ondischargingtimechange { get; set; }

    /// <summary>
    /// onlevelchange
    /// </summary>
    [Description("@#onlevelchange")]
    public extern EventHandler Onlevelchange { get; set; }
}