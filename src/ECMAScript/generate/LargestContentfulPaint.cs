namespace ECMAScript;

/// <summary>
/// LargestContentfulPaint
/// </summary>
[ECMAScript]
[Description("@#LargestContentfulPaint")]
public class LargestContentfulPaint : PerformanceEntry
{
    /// <summary>
    /// loadTime
    /// </summary>
    [Description("@#loadTime")]
    public extern double LoadTime { get; }

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern uint Size { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern new string Id { get; }

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string Url { get; }

    /// <summary>
    /// element
    /// </summary>
    [Description("@#element")]
    public extern Element? Element { get; }

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