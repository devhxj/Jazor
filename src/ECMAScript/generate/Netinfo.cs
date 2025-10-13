namespace ECMAScript;

/// <summary>
/// NetworkInformation
/// </summary>
[ECMAScript]
[Description("@#NetworkInformation")]
public class NetworkInformation : EventTarget
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern ConnectionType Type { get; }

    /// <summary>
    /// effectiveType
    /// </summary>
    [Description("@#effectiveType")]
    public extern EffectiveConnectionType EffectiveType { get; }

    /// <summary>
    /// downlinkMax
    /// </summary>
    [Description("@#downlinkMax")]
    public extern Megabit DownlinkMax { get; }

    /// <summary>
    /// downlink
    /// </summary>
    [Description("@#downlink")]
    public extern Megabit Downlink { get; }

    /// <summary>
    /// rtt
    /// </summary>
    [Description("@#rtt")]
    public extern Millisecond Rtt { get; }

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }

    #region mixin NetworkInformationSaveData
    /// <summary>
    /// saveData
    /// </summary>
    [Description("@#saveData")]
    public extern bool SaveData { get; }
    #endregion
}