namespace ECMAScript;

/// <summary>
/// CSSAnimation
/// </summary>
[ECMAScript]
[Description("@#CSSAnimation")]
public class CSSAnimation(AnimationEffect? effect, AnimationTimeline? timeline) : Animation(effect, timeline)
{
    /// <summary>
    /// animationName
    /// </summary>
    [Description("@#animationName")]
    public extern string AnimationName { get; }
}