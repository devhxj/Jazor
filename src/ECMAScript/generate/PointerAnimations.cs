namespace ECMAScript;

/// <summary>
/// PointerTimelineOptions
/// </summary>
[ECMAScript]
[Description("@#PointerTimelineOptions")]
public record PointerTimelineOptions(
    [property: Description("@#source")]Element? Source = default,
	[property: Description("@#axis")]PointerAxis Axis = PointerAxis.Block);

/// <summary>
/// PointerTimeline
/// </summary>
[ECMAScript]
[Description("@#PointerTimeline")]
public class PointerTimeline : AnimationTimeline
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern PointerTimeline(PointerTimelineOptions options);

    /// <summary>
    /// source
    /// </summary>
    [Description("@#source")]
    public extern Element? Source { get; }

    /// <summary>
    /// axis
    /// </summary>
    [Description("@#axis")]
    public extern PointerAxis Axis { get; }
}