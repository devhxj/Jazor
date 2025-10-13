namespace ECMAScript;

/// <summary>
/// DocumentTimelineOptions
/// </summary>
[ECMAScript]
[Description("@#DocumentTimelineOptions")]
public record DocumentTimelineOptions(
    [property: Description("@#originTime")]double OriginTime = 0d);

/// <summary>
/// BaseComputedKeyframe
/// </summary>
[ECMAScript]
[Description("@#BaseComputedKeyframe")]
public record BaseComputedKeyframe(
    [property: Description("@#offset")]double? Offset = null,
	[property: Description("@#computedOffset")]double ComputedOffset = default,
	[property: Description("@#easing")]string? Easing = default,
	[property: Description("@#composite")]CompositeOperationOrAuto Composite = CompositeOperationOrAuto.Auto);

/// <summary>
/// BasePropertyIndexedKeyframe
/// </summary>
[ECMAScript]
[Description("@#BasePropertyIndexedKeyframe")]
public record BasePropertyIndexedKeyframe(
    [property: Description("@#offset")]Either<double?, double?[]>? Offset = default,
	[property: Description("@#easing")]Either<string, string[]>? Easing = default,
	[property: Description("@#composite")]Either<CompositeOperationOrAuto, CompositeOperationOrAuto[]>? Composite = default);

/// <summary>
/// BaseKeyframe
/// </summary>
[ECMAScript]
[Description("@#BaseKeyframe")]
public record BaseKeyframe(
    [property: Description("@#offset")]double? Offset = null,
	[property: Description("@#easing")]string? Easing = default,
	[property: Description("@#composite")]CompositeOperationOrAuto Composite = CompositeOperationOrAuto.Auto);

/// <summary>
/// GetAnimationsOptions
/// </summary>
[ECMAScript]
[Description("@#GetAnimationsOptions")]
public record GetAnimationsOptions(
    [property: Description("@#subtree")]bool Subtree = false,
	[property: Description("@#pseudoElement")]string? PseudoElement = null);

/// <summary>
/// DocumentTimeline
/// </summary>
[ECMAScript]
[Description("@#DocumentTimeline")]
public class DocumentTimeline : AnimationTimeline
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern DocumentTimeline(DocumentTimelineOptions options);
}