namespace ECMAScript;

/// <summary>
/// AnimationEventInit
/// </summary>
[ECMAScript]
[Description("@#AnimationEventInit")]
public record AnimationEventInit(
    [property: Description("@#animationName")]string? AnimationName = default,
	[property: Description("@#elapsedTime")]double ElapsedTime = 0.0d,
	[property: Description("@#pseudoElement")]string? PseudoElement = default): EventInit;

/// <summary>
/// AnimationEvent
/// </summary>
[ECMAScript]
[Description("@#AnimationEvent")]
public class AnimationEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="animationEventInitDict">animationEventInitDict</param>
    public extern AnimationEvent(string type, AnimationEventInit animationEventInitDict);

    /// <summary>
    /// animationName
    /// </summary>
    [Description("@#animationName")]
    public extern string AnimationName { get; }

    /// <summary>
    /// elapsedTime
    /// </summary>
    [Description("@#elapsedTime")]
    public extern double ElapsedTime { get; }

    /// <summary>
    /// pseudoElement
    /// </summary>
    [Description("@#pseudoElement")]
    public extern string PseudoElement { get; }
}

/// <summary>
/// CSSRule
/// </summary>
[ECMAScript]
[Description("@#CSSRule")]
public partial class CSSRule
{
    /// <summary>
    /// KEYFRAMES_RULE
    /// </summary>
    [Description("@#KEYFRAMES_RULE")]
    public const ushort KEYFRAMES_RULE = 7;

    /// <summary>
    /// KEYFRAME_RULE
    /// </summary>
    [Description("@#KEYFRAME_RULE")]
    public const ushort KEYFRAME_RULE = 8;

    /// <summary>
    /// COUNTER_STYLE_RULE
    /// </summary>
    [Description("@#COUNTER_STYLE_RULE")]
    public const ushort COUNTER_STYLE_RULE = 11;

    /// <summary>
    /// FONT_FEATURE_VALUES_RULE
    /// </summary>
    [Description("@#FONT_FEATURE_VALUES_RULE")]
    public const ushort FONT_FEATURE_VALUES_RULE = 14;
}

/// <summary>
/// CSSKeyframeRule
/// </summary>
[ECMAScript]
[Description("@#CSSKeyframeRule")]
public class CSSKeyframeRule : CSSRule
{
    /// <summary>
    /// keyText
    /// </summary>
    [Description("@#keyText")]
    public extern string KeyText { get; set; }

    /// <summary>
    /// style
    /// </summary>
    [Description("@#style")]
    public extern CSSStyleProperties Style { get; }
}

/// <summary>
/// CSSKeyframesRule
/// </summary>
[ECMAScript]
[Description("@#CSSKeyframesRule")]
public class CSSKeyframesRule : CSSRule
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// cssRules
    /// </summary>
    [Description("@#cssRules")]
    public extern CSSRuleList CssRules { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern CSSKeyframeRule this[uint index] { get; }

    /// <summary>
    /// appendRule
    /// </summary>
    /// <param name="rule">rule</param>
    [Description("@#appendRule")]
    public extern void AppendRule(string rule);

    /// <summary>
    /// deleteRule
    /// </summary>
    /// <param name="select">select</param>
    [Description("@#deleteRule")]
    public extern void DeleteRule(string select);

    /// <summary>
    /// findRule
    /// </summary>
    /// <param name="select">select</param>
    [Description("@#findRule")]
    public extern CSSKeyframeRule? FindRule(string select);
}