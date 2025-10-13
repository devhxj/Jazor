namespace ECMAScript;

/// <summary>
/// PerformanceServerTiming
/// </summary>
[ECMAScript]
[Description("@#PerformanceServerTiming")]
public class PerformanceServerTiming
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// duration
    /// </summary>
    [Description("@#duration")]
    public extern double Duration { get; }

    /// <summary>
    /// description
    /// </summary>
    [Description("@#description")]
    public extern string Description { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}