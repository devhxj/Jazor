namespace ECMAScript;

/// <summary>
/// PreferenceManager
/// </summary>
[ECMAScript]
[Description("@#PreferenceManager")]
public class PreferenceManager
{
    /// <summary>
    /// colorScheme
    /// </summary>
    [Description("@#colorScheme")]
    public extern PreferenceObject ColorScheme { get; }

    /// <summary>
    /// contrast
    /// </summary>
    [Description("@#contrast")]
    public extern PreferenceObject Contrast { get; }

    /// <summary>
    /// reducedMotion
    /// </summary>
    [Description("@#reducedMotion")]
    public extern PreferenceObject ReducedMotion { get; }

    /// <summary>
    /// reducedTransparency
    /// </summary>
    [Description("@#reducedTransparency")]
    public extern PreferenceObject ReducedTransparency { get; }

    /// <summary>
    /// reducedData
    /// </summary>
    [Description("@#reducedData")]
    public extern PreferenceObject ReducedData { get; }
}

/// <summary>
/// PreferenceObject
/// </summary>
[ECMAScript]
[Description("@#PreferenceObject")]
public class PreferenceObject : EventTarget
{
    /// <summary>
    /// override
    /// </summary>
    [Description("@#override")]
    public extern string? Override { get; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; }

    /// <summary>
    /// validValues
    /// </summary>
    [Description("@#validValues")]
    public extern FrozenSet<string> ValidValues { get; }

    /// <summary>
    /// clearOverride
    /// </summary>
    [Description("@#clearOverride")]
    public extern void ClearOverride();

    /// <summary>
    /// requestOverride
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#requestOverride")]
    public extern PromiseResult<object> RequestOverride(string? value);

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }
}