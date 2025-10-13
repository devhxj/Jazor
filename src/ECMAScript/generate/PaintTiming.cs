namespace ECMAScript;

/// <summary>
/// PerformancePaintTiming
/// </summary>
[ECMAScript]
[Description("@#PerformancePaintTiming")]
public class PerformancePaintTiming : PerformanceEntry
{


    #region mixin PaintTimingMixin
    /// <summary>
    /// paintTime
    /// </summary>
    [Description("@#paintTime")]
    public extern double PaintTime { get; }

    /// <summary>
    /// presentationTime
    /// </summary>
    [Description("@#presentationTime")]
    public extern double? PresentationTime { get; }
    #endregion
}