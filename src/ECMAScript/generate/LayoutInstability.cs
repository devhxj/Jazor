namespace ECMAScript;

/// <summary>
/// LayoutShift
/// </summary>
[ECMAScript]
[Description("@#LayoutShift")]
public class LayoutShift : PerformanceEntry
{
    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern double Value { get; }

    /// <summary>
    /// hadRecentInput
    /// </summary>
    [Description("@#hadRecentInput")]
    public extern bool HadRecentInput { get; }

    /// <summary>
    /// lastInputTime
    /// </summary>
    [Description("@#lastInputTime")]
    public extern double LastInputTime { get; }

    /// <summary>
    /// sources
    /// </summary>
    [Description("@#sources")]
    public extern FrozenSet<LayoutShiftAttribution> Sources { get; }
}

/// <summary>
/// LayoutShiftAttribution
/// </summary>
[ECMAScript]
[Description("@#LayoutShiftAttribution")]
public class LayoutShiftAttribution
{
    /// <summary>
    /// node
    /// </summary>
    [Description("@#node")]
    public extern Node? Node { get; }

    /// <summary>
    /// previousRect
    /// </summary>
    [Description("@#previousRect")]
    public extern DOMRectReadOnly PreviousRect { get; }

    /// <summary>
    /// currentRect
    /// </summary>
    [Description("@#currentRect")]
    public extern DOMRectReadOnly CurrentRect { get; }
}