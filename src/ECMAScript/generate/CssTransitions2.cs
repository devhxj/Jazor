namespace ECMAScript;

/// <summary>
/// CSSStartingStyleRule
/// </summary>
[ECMAScript]
[Description("@#CSSStartingStyleRule")]
public class CSSStartingStyleRule : CSSGroupingRule
{

}

/// <summary>
/// CSSTransition
/// </summary>
[ECMAScript]
[Description("@#CSSTransition")]
public class CSSTransition(AnimationEffect? effect, AnimationTimeline? timeline) : Animation(effect, timeline)
{
    /// <summary>
    /// transitionProperty
    /// </summary>
    [Description("@#transitionProperty")]
    public extern string TransitionProperty { get; }
}