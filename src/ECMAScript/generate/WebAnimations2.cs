namespace ECMAScript;

/// <summary>
/// EffectTiming
/// </summary>
[ECMAScript]
[Description("@#EffectTiming")]
public record EffectTiming(
    [property: Description("@#delay")]double Delay = default,
	[property: Description("@#endDelay")]double EndDelay = default,
	[property: Description("@#playbackRate")]double PlaybackRate = 1.0d,
	[property: Description("@#duration")]Either<double, CSSNumericValue, string>? Duration = default,
	[property: Description("@#fill")]FillMode Fill = FillMode.Auto,
	[property: Description("@#iterationStart")]double IterationStart = 0.0d,
	[property: Description("@#iterations")]double Iterations = 1.0d,
	[property: Description("@#direction")]PlaybackDirection Direction = PlaybackDirection.Normal,
	[property: Description("@#easing")]string? Easing = default)
{
    [Category("optional")]
    public extern static EffectTiming OptionalDelayEndDelayPlaybackRate4(
        [Description("@#delay")]double Delay = default,
        [Description("@#endDelay")]double EndDelay = default,
        [Description("@#playbackRate")]double playbackRate = 1.0d,
        [Description("@#duration")]Either<double, CSSNumericValue, string>? duration = default);

    [Category("optional")]
    public extern static EffectTiming OptionalFillIterationStartIterations5(
        [Description("@#fill")]FillMode fill = FillMode.Auto,
        [Description("@#iterationStart")]double iterationStart = 0.0d,
        [Description("@#iterations")]double iterations = 1.0d,
        [Description("@#direction")]PlaybackDirection direction = PlaybackDirection.Normal,
        [Description("@#easing")]string? easing = default);
}

/// <summary>
/// OptionalEffectTiming
/// </summary>
[ECMAScript]
[Description("@#OptionalEffectTiming")]
public record OptionalEffectTiming(
    [property: Description("@#playbackRate")]double PlaybackRate = default,
	[property: Description("@#delay")]double Delay = default,
	[property: Description("@#endDelay")]double EndDelay = default,
	[property: Description("@#fill")]FillMode? Fill = default,
	[property: Description("@#iterationStart")]double IterationStart = default,
	[property: Description("@#iterations")]double Iterations = default,
	[property: Description("@#duration")]Either<double, string>? Duration = default,
	[property: Description("@#direction")]PlaybackDirection? Direction = default,
	[property: Description("@#easing")]string? Easing = default)
{
    [Category("optional")]
    public extern static OptionalEffectTiming OptionalPlaybackRate(
        [Description("@#playbackRate")]double PlaybackRate = default);

    [Category("optional")]
    public extern static OptionalEffectTiming OptionalDelayEndDelayFill8(
        [Description("@#delay")]double Delay = default,
        [Description("@#endDelay")]double EndDelay = default,
        [Description("@#fill")]FillMode? Fill = default,
        [Description("@#iterationStart")]double IterationStart = default,
        [Description("@#iterations")]double Iterations = default,
        [Description("@#duration")]Either<double, string>? Duration = default,
        [Description("@#direction")]PlaybackDirection? Direction = default,
        [Description("@#easing")]string? Easing = default);
}

/// <summary>
/// ComputedEffectTiming
/// </summary>
[ECMAScript]
[Description("@#ComputedEffectTiming")]
public record ComputedEffectTiming(
    [property: Description("@#startTime")]CSSNumberish? StartTime = default,
	[property: Description("@#endTime")]CSSNumberish? EndTime = default,
	[property: Description("@#activeDuration")]CSSNumberish? ActiveDuration = default,
	[property: Description("@#localTime")]CSSNumberish? LocalTime = default,
	[property: Description("@#progress")]double Progress = default,
	[property: Description("@#currentIteration")]double CurrentIteration = default): EffectTiming
{
    [Category("optional")]
    public extern static ComputedEffectTiming OptionalStartTimeEndTimeActiveDuration4(
        [Description("@#startTime")]CSSNumberish? StartTime = default,
        [Description("@#endTime")]CSSNumberish? EndTime = default,
        [Description("@#activeDuration")]CSSNumberish? ActiveDuration = default,
        [Description("@#localTime")]CSSNumberish? LocalTime = default);

    [Category("optional")]
    public extern static ComputedEffectTiming OptionalProgressCurrentIteration(
        [Description("@#progress")]double Progress = default,
        [Description("@#currentIteration")]double CurrentIteration = default);
}

/// <summary>
/// KeyframeEffectOptions
/// </summary>
[ECMAScript]
[Description("@#KeyframeEffectOptions")]
public record KeyframeEffectOptions(
    [property: Description("@#iterationComposite")]IterationCompositeOperation IterationComposite = IterationCompositeOperation.Replace,
	[property: Description("@#composite")]CompositeOperation Composite = CompositeOperation.Replace,
	[property: Description("@#pseudoElement")]string? PseudoElement = null): EffectTiming
{
    [Category("optional")]
    public extern static KeyframeEffectOptions OptionalIterationComposite(
        [Description("@#iterationComposite")]IterationCompositeOperation iterationComposite = IterationCompositeOperation.Replace);

    [Category("optional")]
    public extern static KeyframeEffectOptions OptionalCompositePseudoElement(
        [Description("@#composite")]CompositeOperation composite = CompositeOperation.Replace,
        [Description("@#pseudoElement")]string? pseudoElement = null);
}

/// <summary>
/// TimelineRangeOffset
/// </summary>
[ECMAScript]
[Description("@#TimelineRangeOffset")]
public record TimelineRangeOffset(
    [property: Description("@#rangeName")]string? RangeName = default,
	[property: Description("@#offset")]CSSNumericValue? Offset = default);

/// <summary>
/// KeyframeAnimationOptions
/// </summary>
[ECMAScript]
[Description("@#KeyframeAnimationOptions")]
public record KeyframeAnimationOptions(
    [property: Description("@#rangeStart")]Either<TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string>? RangeStart = default,
	[property: Description("@#rangeEnd")]Either<TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string>? RangeEnd = default,
	[property: Description("@#trigger")]AnimationTrigger? Trigger = default,
	[property: Description("@#id")]string? Id = default,
	[property: Description("@#timeline")]AnimationTimeline? Timeline = default): KeyframeEffectOptions
{
    [Category("optional")]
    public extern static KeyframeAnimationOptions OptionalRangeStartRangeEndTrigger(
        [Description("@#rangeStart")]Either<TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string>? rangeStart = default,
        [Description("@#rangeEnd")]Either<TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string>? rangeEnd = default,
        [Description("@#trigger")]AnimationTrigger? Trigger = default);

    [Category("optional")]
    public extern static KeyframeAnimationOptions OptionalIdTimeline(
        [Description("@#id")]string? id = default,
        [Description("@#timeline")]AnimationTimeline? Timeline = default);
}

/// <summary>
/// AnimationPlaybackEventInit
/// </summary>
[ECMAScript]
[Description("@#AnimationPlaybackEventInit")]
public record AnimationPlaybackEventInit(
    [property: Description("@#currentTime")]CSSNumberish? CurrentTime = null,
	[property: Description("@#timelineTime")]CSSNumberish? TimelineTime = null): EventInit;

/// <summary>
/// AnimationTriggerOptions
/// </summary>
[ECMAScript]
[Description("@#AnimationTriggerOptions")]
public record AnimationTriggerOptions(
    [property: Description("@#timeline")]AnimationTimeline? Timeline = default,
	[property: Description("@#behavior")]AnimationTriggerBehavior? Behavior = AnimationTriggerBehavior.Once,
	[property: Description("@#rangeStart")]Either<TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string>? RangeStart = default,
	[property: Description("@#rangeEnd")]Either<TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string>? RangeEnd = default,
	[property: Description("@#exitRangeStart")]Either<TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string>? ExitRangeStart = default,
	[property: Description("@#exitRangeEnd")]Either<TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string>? ExitRangeEnd = default);

/// <summary>
/// AnimationTimeline
/// </summary>
[ECMAScript]
[Description("@#AnimationTimeline")]
public partial class AnimationTimeline
{
    /// <summary>
    /// currentTime
    /// </summary>
    [Description("@#currentTime")]
    public extern CSSNumberish? CurrentTime { get; }

    /// <summary>
    /// duration
    /// </summary>
    [Description("@#duration")]
    public extern CSSNumberish? Duration { get; }

    /// <summary>
    /// play
    /// </summary>
    /// <param name="effect">effect</param>
    [Description("@#play")]
    public extern Animation Play(AnimationEffect? effect = null);
}

/// <summary>
/// Animation
/// </summary>
[ECMAScript]
[Description("@#Animation")]
public partial class Animation : EventTarget
{
    /// <summary>
    /// startTime
    /// </summary>
    [Description("@#startTime")]
    public extern CSSNumberish? StartTime { get; set; }

    /// <summary>
    /// currentTime
    /// </summary>
    [Description("@#currentTime")]
    public extern CSSNumberish? CurrentTime { get; set; }

    /// <summary>
    /// trigger
    /// </summary>
    [Description("@#trigger")]
    public extern AnimationTrigger? Trigger { get; set; }

    /// <summary>
    /// overallProgress
    /// </summary>
    [Description("@#overallProgress")]
    public extern double? OverallProgress { get; }

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="effect">effect</param>
    /// <param name="timeline">timeline</param>
    public extern Animation(AnimationEffect? effect, AnimationTimeline? timeline);

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; set; }

    /// <summary>
    /// effect
    /// </summary>
    [Description("@#effect")]
    public extern AnimationEffect? Effect { get; set; }

    /// <summary>
    /// timeline
    /// </summary>
    [Description("@#timeline")]
    public extern AnimationTimeline? Timeline { get; set; }

    /// <summary>
    /// playbackRate
    /// </summary>
    [Description("@#playbackRate")]
    public extern double PlaybackRate { get; set; }

    /// <summary>
    /// playState
    /// </summary>
    [Description("@#playState")]
    public extern AnimationPlayState PlayState { get; }

    /// <summary>
    /// replaceState
    /// </summary>
    [Description("@#replaceState")]
    public extern AnimationReplaceState ReplaceState { get; }

    /// <summary>
    /// pending
    /// </summary>
    [Description("@#pending")]
    public extern bool Pending { get; }

    /// <summary>
    /// ready
    /// </summary>
    [Description("@#ready")]
    public extern PromiseResult<Animation> Ready { get; }

    /// <summary>
    /// finished
    /// </summary>
    [Description("@#finished")]
    public extern PromiseResult<Animation> Finished { get; }

    /// <summary>
    /// onfinish
    /// </summary>
    [Description("@#onfinish")]
    public extern EventHandler Onfinish { get; set; }

    /// <summary>
    /// oncancel
    /// </summary>
    [Description("@#oncancel")]
    public extern EventHandler Oncancel { get; set; }

    /// <summary>
    /// onremove
    /// </summary>
    [Description("@#onremove")]
    public extern EventHandler Onremove { get; set; }

    /// <summary>
    /// cancel
    /// </summary>
    [Description("@#cancel")]
    public extern void Cancel();

    /// <summary>
    /// finish
    /// </summary>
    [Description("@#finish")]
    public extern void Finish();

    /// <summary>
    /// play
    /// </summary>
    [Description("@#play")]
    public extern void Play();

    /// <summary>
    /// pause
    /// </summary>
    [Description("@#pause")]
    public extern void Pause();

    /// <summary>
    /// updatePlaybackRate
    /// </summary>
    /// <param name="playbackRate">playbackRate</param>
    [Description("@#updatePlaybackRate")]
    public extern void UpdatePlaybackRate(double playbackRate);

    /// <summary>
    /// reverse
    /// </summary>
    [Description("@#reverse")]
    public extern void Reverse();

    /// <summary>
    /// persist
    /// </summary>
    [Description("@#persist")]
    public extern void Persist();

    /// <summary>
    /// commitStyles
    /// </summary>
    [Description("@#commitStyles")]
    public extern void CommitStyles();
}

/// <summary>
/// AnimationEffect
/// </summary>
[ECMAScript]
[Description("@#AnimationEffect")]
public partial class AnimationEffect
{
    /// <summary>
    /// parent
    /// </summary>
    [Description("@#parent")]
    public extern GroupEffect? Parent { get; }

    /// <summary>
    /// previousSibling
    /// </summary>
    [Description("@#previousSibling")]
    public extern AnimationEffect? PreviousSibling { get; }

    /// <summary>
    /// nextSibling
    /// </summary>
    [Description("@#nextSibling")]
    public extern AnimationEffect? NextSibling { get; }

    /// <summary>
    /// before
    /// </summary>
    /// <param name="effects">effects</param>
    [Description("@#before")]
    public extern void Before(AnimationEffect effects);

    /// <summary>
    /// after
    /// </summary>
    /// <param name="effects">effects</param>
    [Description("@#after")]
    public extern void After(AnimationEffect effects);

    /// <summary>
    /// replace
    /// </summary>
    /// <param name="effects">effects</param>
    [Description("@#replace")]
    public extern void Replace(AnimationEffect effects);

    /// <summary>
    /// remove
    /// </summary>
    [Description("@#remove")]
    public extern void Remove();

    /// <summary>
    /// getTiming
    /// </summary>
    [Description("@#getTiming")]
    public extern EffectTiming GetTiming();

    /// <summary>
    /// getComputedTiming
    /// </summary>
    [Description("@#getComputedTiming")]
    public extern ComputedEffectTiming GetComputedTiming();

    /// <summary>
    /// updateTiming
    /// </summary>
    /// <param name="timing">timing</param>
    [Description("@#updateTiming")]
    public extern void UpdateTiming(OptionalEffectTiming? timing = default);
}

/// <summary>
/// GroupEffect
/// </summary>
[ECMAScript]
[Description("@#GroupEffect")]
public class GroupEffect
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="children">children</param>
    /// <param name="timing">timing</param>
    public extern GroupEffect(AnimationEffect[]? children, Either<double, EffectTiming> timing);

    /// <summary>
    /// children
    /// </summary>
    [Description("@#children")]
    public extern AnimationNodeList Children { get; }

    /// <summary>
    /// firstChild
    /// </summary>
    [Description("@#firstChild")]
    public extern AnimationEffect? FirstChild { get; }

    /// <summary>
    /// lastChild
    /// </summary>
    [Description("@#lastChild")]
    public extern AnimationEffect? LastChild { get; }

    /// <summary>
    /// clone
    /// </summary>
    [Description("@#clone")]
    public extern GroupEffect Clone();

    /// <summary>
    /// prepend
    /// </summary>
    /// <param name="effects">effects</param>
    [Description("@#prepend")]
    public extern void Prepend(AnimationEffect effects);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="effects">effects</param>
    [Description("@#append")]
    public extern void Append(AnimationEffect effects);
}

/// <summary>
/// AnimationNodeList
/// </summary>
[ECMAScript]
[Description("@#AnimationNodeList")]
public class AnimationNodeList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern AnimationEffect? GetItem(uint index);
}

/// <summary>
/// SequenceEffect
/// </summary>
[ECMAScript]
[Description("@#SequenceEffect")]
public class SequenceEffect : GroupEffect
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="children">children</param>
    /// <param name="timing">timing</param>
    public extern SequenceEffect(AnimationEffect[]? children, Either<double, EffectTiming> timing);

    /// <summary>
    /// clone
    /// </summary>
    [Description("@#clone")]
    public extern new SequenceEffect Clone();
}

/// <summary>
/// KeyframeEffect
/// </summary>
[ECMAScript]
[Description("@#KeyframeEffect")]
public partial class KeyframeEffect : AnimationEffect
{
    /// <summary>
    /// iterationComposite
    /// </summary>
    [Description("@#iterationComposite")]
    public extern IterationCompositeOperation IterationComposite { get; set; }

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="keyframes">keyframes</param>
    /// <param name="options">options</param>
    public extern KeyframeEffect(Element? target, object? keyframes, Either<double, KeyframeEffectOptions> options);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="source">source</param>
    public extern KeyframeEffect(KeyframeEffect source);

    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern Element? Target { get; set; }

    /// <summary>
    /// pseudoElement
    /// </summary>
    [Description("@#pseudoElement")]
    public extern string? PseudoElement { get; set; }

    /// <summary>
    /// composite
    /// </summary>
    [Description("@#composite")]
    public extern CompositeOperation Composite { get; set; }

    /// <summary>
    /// getKeyframes
    /// </summary>
    [Description("@#getKeyframes")]
    public extern object[] GetKeyframes();

    /// <summary>
    /// setKeyframes
    /// </summary>
    /// <param name="keyframes">keyframes</param>
    [Description("@#setKeyframes")]
    public extern void SetKeyframes(object? keyframes);
}

/// <summary>
/// AnimationPlaybackEvent
/// </summary>
[ECMAScript]
[Description("@#AnimationPlaybackEvent")]
public class AnimationPlaybackEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern AnimationPlaybackEvent(string type, AnimationPlaybackEventInit eventInitDict);

    /// <summary>
    /// currentTime
    /// </summary>
    [Description("@#currentTime")]
    public extern CSSNumberish? CurrentTime { get; }

    /// <summary>
    /// timelineTime
    /// </summary>
    [Description("@#timelineTime")]
    public extern CSSNumberish? TimelineTime { get; }
}

/// <summary>
/// AnimationTrigger
/// </summary>
[ECMAScript]
[Description("@#AnimationTrigger")]
public class AnimationTrigger
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern AnimationTrigger(AnimationTriggerOptions options);

    /// <summary>
    /// timeline
    /// </summary>
    [Description("@#timeline")]
    public extern AnimationTimeline Timeline { get; set; }

    /// <summary>
    /// behavior
    /// </summary>
    [Description("@#behavior")]
    public extern AnimationTriggerBehavior Behavior { get; set; }

    /// <summary>
    /// rangeStart
    /// </summary>
    [Description("@#rangeStart")]
    public extern object RangeStart { get; set; }

    /// <summary>
    /// rangeEnd
    /// </summary>
    [Description("@#rangeEnd")]
    public extern object RangeEnd { get; set; }

    /// <summary>
    /// exitRangeStart
    /// </summary>
    [Description("@#exitRangeStart")]
    public extern object ExitRangeStart { get; set; }

    /// <summary>
    /// exitRangeEnd
    /// </summary>
    [Description("@#exitRangeEnd")]
    public extern object ExitRangeEnd { get; set; }
}