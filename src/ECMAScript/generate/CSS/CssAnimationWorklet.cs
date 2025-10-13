namespace ECMAScript.CSS;

/// <summary>
/// AnimationWorkletGlobalScope
/// </summary>
[ECMAScript]
[Description("@#AnimationWorkletGlobalScope")]
public class AnimationWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// registerAnimator
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="animatorCtor">animatorCtor</param>
    [Description("@#registerAnimator")]
    public extern void RegisterAnimator(string name, AnimatorInstanceConstructor animatorCtor);
}

/// <summary>
/// WorkletAnimationEffect
/// </summary>
[ECMAScript]
[Description("@#WorkletAnimationEffect")]
public class WorkletAnimationEffect
{
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
    /// localTime
    /// </summary>
    [Description("@#localTime")]
    public extern double? LocalTime { get; set; }
}

/// <summary>
/// WorkletAnimation
/// </summary>
[ECMAScript]
[Description("@#WorkletAnimation")]
public class WorkletAnimation(AnimationEffect? effect, AnimationTimeline? timeline) : Animation(effect, timeline)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="animatorName">animatorName</param>
    /// <param name="effects">effects</param>
    /// <param name="timeline">timeline</param>
    /// <param name="options">options</param>
    public extern WorkletAnimation(string animatorName, Either<AnimationEffect, AnimationEffect[]>? effects, AnimationTimeline? timeline, object options);

    /// <summary>
    /// animatorName
    /// </summary>
    [Description("@#animatorName")]
    public extern string AnimatorName { get; }
}

/// <summary>
/// WorkletGroupEffect
/// </summary>
[ECMAScript]
[Description("@#WorkletGroupEffect")]
public class WorkletGroupEffect
{
    /// <summary>
    /// getChildren
    /// </summary>
    [Description("@#getChildren")]
    public extern WorkletAnimationEffect[] GetChildren();
}