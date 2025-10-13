namespace ECMAScript;

/// <summary>
/// PerformanceElementTiming
/// </summary>
[ECMAScript]
[Description("@#PerformanceElementTiming")]
public class PerformanceElementTiming : PerformanceEntry
{
    /// <summary>
    /// renderTime
    /// </summary>
    [Description("@#renderTime")]
    public extern double RenderTime { get; }

    /// <summary>
    /// loadTime
    /// </summary>
    [Description("@#loadTime")]
    public extern double LoadTime { get; }

    /// <summary>
    /// intersectionRect
    /// </summary>
    [Description("@#intersectionRect")]
    public extern DOMRectReadOnly IntersectionRect { get; }

    /// <summary>
    /// identifier
    /// </summary>
    [Description("@#identifier")]
    public extern string Identifier { get; }

    /// <summary>
    /// naturalWidth
    /// </summary>
    [Description("@#naturalWidth")]
    public extern uint NaturalWidth { get; }

    /// <summary>
    /// naturalHeight
    /// </summary>
    [Description("@#naturalHeight")]
    public extern uint NaturalHeight { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern new string Id { get; }

    /// <summary>
    /// element
    /// </summary>
    [Description("@#element")]
    public extern Element? Element { get; }

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string Url { get; }

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