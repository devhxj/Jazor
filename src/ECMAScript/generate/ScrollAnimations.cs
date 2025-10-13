namespace ECMAScript;

/// <summary>
/// ScrollTimelineOptions
/// </summary>
[ECMAScript]
[Description("@#ScrollTimelineOptions")]
public record ScrollTimelineOptions(
    [property: Description("@#source")]Element? Source = default,
	[property: Description("@#axis")]ScrollAxis Axis = ScrollAxis.Block);

/// <summary>
/// ViewTimelineOptions
/// </summary>
[ECMAScript]
[Description("@#ViewTimelineOptions")]
public record ViewTimelineOptions(
    [property: Description("@#subject")]Element? Subject = default,
	[property: Description("@#axis")]ScrollAxis Axis = ScrollAxis.Block,
	[property: Description("@#inset")]Either<string, Either<CSSNumericValue, CSSKeywordValue>[]>? Inset = default);

/// <summary>
/// ScrollTimeline
/// </summary>
[ECMAScript]
[Description("@#ScrollTimeline")]
public class ScrollTimeline : AnimationTimeline
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern ScrollTimeline(ScrollTimelineOptions options);

    /// <summary>
    /// source
    /// </summary>
    [Description("@#source")]
    public extern Element? Source { get; }

    /// <summary>
    /// axis
    /// </summary>
    [Description("@#axis")]
    public extern ScrollAxis Axis { get; }
}

/// <summary>
/// ViewTimeline
/// </summary>
[ECMAScript]
[Description("@#ViewTimeline")]
public class ViewTimeline(ScrollTimelineOptions options) : ScrollTimeline(options)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern ViewTimeline(ViewTimelineOptions options);

    /// <summary>
    /// subject
    /// </summary>
    [Description("@#subject")]
    public extern Element Subject { get; }

    /// <summary>
    /// startOffset
    /// </summary>
    [Description("@#startOffset")]
    public extern CSSNumericValue StartOffset { get; }

    /// <summary>
    /// endOffset
    /// </summary>
    [Description("@#endOffset")]
    public extern CSSNumericValue EndOffset { get; }
}