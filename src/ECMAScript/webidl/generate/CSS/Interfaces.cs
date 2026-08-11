namespace ECMAScript.CSS;

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#animationworkletglobalscope">CSS Animation Worklet API: 2 Animation Worklet</see>
/// </summary>
[ECMAScript]
[Description("@#AnimationWorkletGlobalScope")]
public class AnimationWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-animationworkletglobalscope-registeranimator">CSS Animation Worklet API: 3.4 Registering an Animator Definition</see>
    /// </summary>
    /// <param name="name"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-animationworkletglobalscope-registeranimator-name-animatorctor-name">CSS Animation Worklet API: 2 Animation Worklet</see></param>
    /// <param name="animatorCtor"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-animationworkletglobalscope-registeranimator-name-animatorctor-animatorctor">CSS Animation Worklet API: 2 Animation Worklet</see></param>
    [Description("@#registerAnimator")]
    public extern void RegisterAnimator(string name, AnimatorInstanceConstructor animatorCtor);
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#workletanimation">CSS Animation Worklet API: 5.1 Worklet Animation</see>
/// </summary>
[ECMAScript]
[Description("@#WorkletAnimation")]
public class WorkletAnimation(AnimationEffect? effect, AnimationTimeline? timeline) : Animation(effect, timeline)
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-workletanimation">CSS Animation Worklet API: 5.2 Creating a Worklet Animation</see>
    /// </summary>
    /// <param name="animatorName"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-workletanimation-animatorname-effects-timeline-options-animatorname">CSS Animation Worklet API: 5.1 Worklet Animation</see></param>
    /// <param name="effects"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-workletanimation-animatorname-effects-timeline-options-effects">CSS Animation Worklet API: 5.1 Worklet Animation</see></param>
    /// <param name="timeline"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-workletanimation-animatorname-effects-timeline-options-timeline">CSS Animation Worklet API: 5.1 Worklet Animation</see></param>
    /// <param name="options"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-workletanimation-animatorname-effects-timeline-options-options">CSS Animation Worklet API: 5.1 Worklet Animation</see></param>
    public extern WorkletAnimation(string animatorName, WorkletAnimationEffects? effects = default, AnimationTimeline? timeline = default, object? options = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-animatorname">CSS Animation Worklet API: 5.1 Worklet Animation</see>
    /// </summary>
    [Description("@#animatorName")]
    public extern string AnimatorName { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#workletanimationeffect">CSS Animation Worklet API: 3.5 Animator Effect</see>
/// </summary>
[ECMAScript]
[Description("@#WorkletAnimationEffect")]
public class WorkletAnimationEffect
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimationeffect-gettiming">CSS Animation Worklet API: 3.5 Animator Effect</see>
    /// </summary>
    [Description("@#getTiming")]
    public extern EffectTiming GetTiming();

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimationeffect-getcomputedtiming">CSS Animation Worklet API: 3.5 Animator Effect</see>
    /// </summary>
    [Description("@#getComputedTiming")]
    public extern ComputedEffectTiming GetComputedTiming();

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimationeffect-localtime">CSS Animation Worklet API: 3.5 Animator Effect</see>
    /// </summary>
    [Description("@#localTime")]
    public extern double? LocalTime { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#workletgroupeffect">CSS Animation Worklet API: 6.1 Worklet Group Effect</see>
/// </summary>
[ECMAScript]
[Description("@#WorkletGroupEffect")]
public class WorkletGroupEffect
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletgroupeffect-getchildren">CSS Animation Worklet API: 6.1 Worklet Group Effect</see>
    /// </summary>
    [Description("@#getChildren")]
    public extern WorkletAnimationEffect[] GetChildren();
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#breaktoken">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
/// </summary>
[ECMAScript]
[Description("@#BreakToken")]
public class BreakToken
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktoken-childbreaktokens">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </summary>
    [Description("@#childBreakTokens")]
    public extern FrozenSet<ChildBreakToken> ChildBreakTokens { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktoken-data">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#childbreaktoken">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
/// </summary>
[ECMAScript]
[Description("@#ChildBreakToken")]
public class ChildBreakToken
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-childbreaktoken-breaktype">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </summary>
    [Description("@#breakType")]
    public extern BreakType BreakType { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-childbreaktoken-child">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </summary>
    [Description("@#child")]
    public extern LayoutChild Child { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#fragmentresult">CSS Layout API Level 1: 6.2 Performing Layout</see>
/// </summary>
[ECMAScript]
[Description("@#FragmentResult")]
public class FragmentResult
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresult-fragmentresult">CSS Layout API Level 1: 6.2 Performing Layout</see>
    /// </summary>
    /// <param name="options"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresult-fragmentresult-options-options">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
    public extern FragmentResult(FragmentResultOptions? options = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresult-inlinesize">CSS Layout API Level 1: 6.2 Performing Layout</see>
    /// </summary>
    [Description("@#inlineSize")]
    public extern double InlineSize { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresult-blocksize">CSS Layout API Level 1: 6.2 Performing Layout</see>
    /// </summary>
    [Description("@#blockSize")]
    public extern double BlockSize { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#intrinsicsizes">CSS Layout API Level 1: 4.3 Intrinsic Sizes</see>
/// </summary>
[ECMAScript]
[Description("@#IntrinsicSizes")]
public class IntrinsicSizes
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-intrinsicsizes-mincontentsize">CSS Layout API Level 1: 4.3 Intrinsic Sizes</see>
    /// </summary>
    [Description("@#minContentSize")]
    public extern double MinContentSize { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-intrinsicsizes-maxcontentsize">CSS Layout API Level 1: 4.3 Intrinsic Sizes</see>
    /// </summary>
    [Description("@#maxContentSize")]
    public extern double MaxContentSize { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#layoutchild">CSS Layout API Level 1: 4.1 Layout Children</see>
/// </summary>
[ECMAScript]
[Description("@#LayoutChild")]
public class LayoutChild
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutchild-stylemap">CSS Layout API Level 1: 4.1 Layout Children</see>
    /// </summary>
    [Description("@#styleMap")]
    public extern StylePropertyMapReadOnly StyleMap { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutchild-intrinsicsizes">CSS Layout API Level 1: 4.1 Layout Children</see>
    /// </summary>
    [Description("@#intrinsicSizes")]
    public extern PromiseResult<IntrinsicSizes> IntrinsicSizes();

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutchild-layoutnextfragment">CSS Layout API Level 1: 4.1 Layout Children</see>
    /// </summary>
    /// <param name="constraints"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutchild-layoutnextfragment-constraints-breaktoken-constraints">CSS Layout API Level 1: 4.1 Layout Children</see></param>
    /// <param name="breakToken"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutchild-layoutnextfragment-constraints-breaktoken-breaktoken">CSS Layout API Level 1: 4.1 Layout Children</see></param>
    [Description("@#layoutNextFragment")]
    public extern PromiseResult<LayoutFragment> LayoutNextFragment(LayoutConstraintsOptions constraints, ChildBreakToken breakToken);
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#layoutconstraints">CSS Layout API Level 1: 4.4 Layout Constraints</see>
/// </summary>
[ECMAScript]
[Description("@#LayoutConstraints")]
public class LayoutConstraints
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-availableinlinesize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#availableInlineSize")]
    public extern double AvailableInlineSize { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-availableblocksize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#availableBlockSize")]
    public extern double AvailableBlockSize { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-fixedinlinesize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#fixedInlineSize")]
    public extern double? FixedInlineSize { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-fixedblocksize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#fixedBlockSize")]
    public extern double? FixedBlockSize { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-percentageinlinesize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#percentageInlineSize")]
    public extern double PercentageInlineSize { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-percentageblocksize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#percentageBlockSize")]
    public extern double PercentageBlockSize { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-blockfragmentationoffset">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#blockFragmentationOffset")]
    public extern double? BlockFragmentationOffset { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-blockfragmentationtype">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#blockFragmentationType")]
    public extern BlockFragmentationType BlockFragmentationType { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-data">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#layoutedges">CSS Layout API Level 1: 4.6 Edges</see>
/// </summary>
[ECMAScript]
[Description("@#LayoutEdges")]
public class LayoutEdges
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-inlinestart">CSS Layout API Level 1: 4.6 Edges</see>
    /// </summary>
    [Description("@#inlineStart")]
    public extern double InlineStart { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-inlineend">CSS Layout API Level 1: 4.6 Edges</see>
    /// </summary>
    [Description("@#inlineEnd")]
    public extern double InlineEnd { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-blockstart">CSS Layout API Level 1: 4.6 Edges</see>
    /// </summary>
    [Description("@#blockStart")]
    public extern double BlockStart { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-blockend">CSS Layout API Level 1: 4.6 Edges</see>
    /// </summary>
    [Description("@#blockEnd")]
    public extern double BlockEnd { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-inline">CSS Layout API Level 1: 4.6 Edges</see>
    /// </summary>
    [Description("@#inline")]
    public extern double Inline { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-block">CSS Layout API Level 1: 4.6 Edges</see>
    /// </summary>
    [Description("@#block")]
    public extern double Block { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#layoutfragment">CSS Layout API Level 1: 4.2 Layout Fragments</see>
/// </summary>
[ECMAScript]
[Description("@#LayoutFragment")]
public class LayoutFragment
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-inlinesize">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </summary>
    [Description("@#inlineSize")]
    public extern double InlineSize { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-blocksize">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </summary>
    [Description("@#blockSize")]
    public extern double BlockSize { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-inlineoffset">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </summary>
    [Description("@#inlineOffset")]
    public extern double InlineOffset { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-blockoffset">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </summary>
    [Description("@#blockOffset")]
    public extern double BlockOffset { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-data">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-breaktoken">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </summary>
    [Description("@#breakToken")]
    public extern ChildBreakToken? BreakToken { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#layoutworkletglobalscope">CSS Layout API Level 1: 3 Layout Worklet</see>
/// </summary>
[ECMAScript]
[Description("@#LayoutWorkletGlobalScope")]
public class LayoutWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutworkletglobalscope-registerlayout">CSS Layout API Level 1: 3.2 Registering A Layout</see>
    /// </summary>
    /// <param name="name"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutworkletglobalscope-registerlayout-name-layoutctor-name">CSS Layout API Level 1: 3 Layout Worklet</see></param>
    /// <param name="layoutCtor"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutworkletglobalscope-registerlayout-name-layoutctor-layoutctor">CSS Layout API Level 1: 3 Layout Worklet</see></param>
    [Description("@#registerLayout")]
    public extern void RegisterLayout(string name, Action layoutCtor);
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-paint-api-1/#paintrenderingcontext2d">CSS Painting API Level 1: 6 The 2D rendering context</see>
/// </summary>
[ECMAScript]
[Description("@#PaintRenderingContext2D")]
public class PaintRenderingContext2D
{
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-paint-api-1/#paintsize">CSS Painting API Level 1: 7 Drawing an image</see>
/// </summary>
[ECMAScript]
[Description("@#PaintSize")]
public class PaintSize
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-paint-api-1/#dom-paintsize-width">CSS Painting API Level 1: 7 Drawing an image</see>
    /// </summary>
    [Description("@#width")]
    public extern double Width { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-paint-api-1/#dom-paintsize-height">CSS Painting API Level 1: 7 Drawing an image</see>
    /// </summary>
    [Description("@#height")]
    public extern double Height { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-paint-api-1/#paintworkletglobalscope">CSS Painting API Level 1: 2 Paint Worklet</see>
/// </summary>
[ECMAScript]
[Description("@#PaintWorkletGlobalScope")]
public class PaintWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-paint-api-1/#dom-paintworkletglobalscope-registerpaint">CSS Painting API Level 1: 4 Registering Custom Paint</see>
    /// </summary>
    /// <param name="name"><see href="https://drafts.css-houdini.org/css-paint-api-1/#dom-paintworkletglobalscope-registerpaint-name-paintctor-name">CSS Painting API Level 1: 2 Paint Worklet</see></param>
    /// <param name="paintCtor"><see href="https://drafts.css-houdini.org/css-paint-api-1/#dom-paintworkletglobalscope-registerpaint-name-paintctor-paintctor">CSS Painting API Level 1: 2 Paint Worklet</see></param>
    [Description("@#registerPaint")]
    public extern void RegisterPaint(string name, Action paintCtor);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-paint-api-1/#dom-paintworkletglobalscope-devicepixelratio">CSS Painting API Level 1: 2 Paint Worklet</see>
    /// </summary>
    [Description("@#devicePixelRatio")]
    public extern double DevicePixelRatio { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#csspropertyrule">CSS Properties and Values API Level 1: 6.1 The CSSPropertyRule Interface</see>
/// </summary>
[ECMAScript]
[Description("@#CSSPropertyRule")]
public class CSSPropertyRule : CSSRule
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-csspropertyrule-name">CSS Properties and Values API Level 1: 6.1 The CSSPropertyRule Interface</see>
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-csspropertyrule-syntax">CSS Properties and Values API Level 1: 6.1 The CSSPropertyRule Interface</see>
    /// </summary>
    [Description("@#syntax")]
    public extern string Syntax { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-csspropertyrule-inherits">CSS Properties and Values API Level 1: 6.1 The CSSPropertyRule Interface</see>
    /// </summary>
    [Description("@#inherits")]
    public extern bool Inherits { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-csspropertyrule-initialvalue">CSS Properties and Values API Level 1: 6.1 The CSSPropertyRule Interface</see>
    /// </summary>
    [Description("@#initialValue")]
    public extern string? InitialValue { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csscolor">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSColor")]
public class CSSColor : CSSColorValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-csscolor">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    /// <param name="colorSpace"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-csscolor-colorspace-channels-alpha-colorspace">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="channels"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-csscolor-colorspace-channels-alpha-channels">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-csscolor-colorspace-channels-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSColor(CSSKeywordish colorSpace, CSSColorPercent[] channels, CSSNumberish? alpha = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-colorspace">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#colorSpace")]
    public extern CSSKeywordish ColorSpace { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-channels">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#channels")]
    public extern ObservableCollection<CSSColorPercent> Channels { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#alpha")]
    public extern CSSNumberish Alpha { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csscolorvalue">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSColorValue")]
public class CSSColorValue : CSSStyleValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolorvalue-parse">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    /// <param name="cssText"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolorvalue-parse-csstext-csstext">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    [Description("@#parse")]
    public static extern CSSColorValueParseResult Parse(string cssText);
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csshsl">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSHSL")]
public class CSSHSL : CSSColorValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-csshsl">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    /// <param name="h"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-csshsl-h-s-l-alpha-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="s"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-csshsl-h-s-l-alpha-s">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="l"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-csshsl-h-s-l-alpha-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-csshsl-h-s-l-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSHSL(CSSColorAngle h, CSSColorPercent s, CSSColorPercent l, CSSColorPercent? alpha = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#h")]
    public extern CSSColorAngle H { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-s">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#s")]
    public extern CSSColorPercent S { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csshwb">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSHWB")]
public class CSSHWB : CSSColorValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-csshwb">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    /// <param name="h"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-csshwb-h-w-b-alpha-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="w"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-csshwb-h-w-b-alpha-w">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="b"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-csshwb-h-w-b-alpha-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-csshwb-h-w-b-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSHWB(CSSNumericValue h, CSSNumberish w, CSSNumberish b, CSSNumberish? alpha = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#h")]
    public extern CSSNumericValue H { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-w">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#w")]
    public extern CSSNumberish W { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#b")]
    public extern CSSNumberish B { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#alpha")]
    public extern CSSNumberish Alpha { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssimagevalue">CSS Typed OM Level 1: 4.5 CSSImageValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSImageValue")]
public class CSSImageValue : CSSStyleValue
{
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csskeywordvalue">CSS Typed OM Level 1: 4.2 CSSKeywordValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSKeywordValue")]
public class CSSKeywordValue : CSSStyleValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csskeywordvalue-csskeywordvalue">CSS Typed OM Level 1: 4.2 CSSKeywordValue objects</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csskeywordvalue-csskeywordvalue-value-value">CSS Typed OM Level 1: 4.2 CSSKeywordValue objects</see></param>
    public extern CSSKeywordValue(string value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csskeywordvalue-value">CSS Typed OM Level 1: 4.2 CSSKeywordValue objects</see>
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csslab">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSLab")]
public class CSSLab : CSSColorValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-csslab">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    /// <param name="l"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-csslab-l-a-b-alpha-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="a"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-csslab-l-a-b-alpha-a">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="b"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-csslab-l-a-b-alpha-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-csslab-l-a-b-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSLab(CSSColorPercent l, CSSColorNumber a, CSSColorNumber b, CSSColorPercent? alpha = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-a">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#a")]
    public extern CSSColorNumber A { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#b")]
    public extern CSSColorNumber B { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csslch">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSLCH")]
public class CSSLCH : CSSColorValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-csslch">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    /// <param name="l"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-csslch-l-c-h-alpha-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="c"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-csslch-l-c-h-alpha-c">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="h"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-csslch-l-c-h-alpha-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-csslch-l-c-h-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSLCH(CSSColorPercent l, CSSColorPercent c, CSSColorAngle h, CSSColorPercent? alpha = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-c">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#c")]
    public extern CSSColorPercent C { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#h")]
    public extern CSSColorAngle H { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssmathclamp">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSMathClamp")]
public class CSSMathClamp : CSSMathValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathclamp-cssmathclamp">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    /// <param name="lower"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathclamp-cssmathclamp-lower-value-upper-lower">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathclamp-cssmathclamp-lower-value-upper-value">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    /// <param name="upper"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathclamp-cssmathclamp-lower-value-upper-upper">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    public extern CSSMathClamp(CSSNumberish lower, CSSNumberish value, CSSNumberish upper);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathclamp-lower">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#lower")]
    public extern CSSNumericValue Lower { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathclamp-value">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#value")]
    public extern CSSNumericValue Value { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathclamp-upper">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#upper")]
    public extern CSSNumericValue Upper { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssmathinvert">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSMathInvert")]
public class CSSMathInvert : CSSMathValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathinvert-cssmathinvert">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    /// <param name="arg"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathinvert-cssmathinvert-arg-arg">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    public extern CSSMathInvert(CSSNumberish arg);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathinvert-value">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#value")]
    public extern CSSNumericValue Value { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssmathmax">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSMathMax")]
public class CSSMathMax : CSSMathValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathmax-cssmathmax">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    /// <param name="args"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathmax-cssmathmax-args-args">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    public extern CSSMathMax(params CSSNumberish[] args);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathmax-values">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssmathmin">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSMathMin")]
public class CSSMathMin : CSSMathValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathmin-cssmathmin">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    /// <param name="args"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathmin-cssmathmin-args-args">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    public extern CSSMathMin(params CSSNumberish[] args);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathmin-values">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssmathnegate">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSMathNegate")]
public class CSSMathNegate : CSSMathValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathnegate-cssmathnegate">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    /// <param name="arg"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathnegate-cssmathnegate-arg-arg">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    public extern CSSMathNegate(CSSNumberish arg);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathnegate-value">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#value")]
    public extern CSSNumericValue Value { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssmathproduct">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSMathProduct")]
public class CSSMathProduct : CSSMathValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathproduct-cssmathproduct">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    /// <param name="args"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathproduct-cssmathproduct-args-args">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    public extern CSSMathProduct(params CSSNumberish[] args);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathproduct-values">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssmathsum">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSMathSum")]
public class CSSMathSum : CSSMathValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathsum-cssmathsum">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    /// <param name="args"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathsum-cssmathsum-args-args">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    public extern CSSMathSum(params CSSNumberish[] args);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathsum-values">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssmathvalue">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSMathValue")]
public class CSSMathValue : CSSNumericValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathvalue-operator">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#operator")]
    public extern CSSMathOperator Operator { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssmatrixcomponent">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSMatrixComponent")]
public class CSSMatrixComponent : CSSTransformComponent
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmatrixcomponent-cssmatrixcomponent">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    /// <param name="matrix"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmatrixcomponent-cssmatrixcomponent-matrix-options-matrix">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    /// <param name="options"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmatrixcomponent-cssmatrixcomponent-matrix-options-options">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    public extern CSSMatrixComponent(DOMMatrixReadOnly matrix, CSSMatrixComponentOptions? options = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmatrixcomponent-matrix">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#matrix")]
    public extern DOMMatrix Matrix { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssnumericarray">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSNumericArray")]
public class CSSNumericArray : IEnumerable<CSSNumericValue>
{
    extern IEnumerator<CSSNumericValue> IEnumerable<CSSNumericValue>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericarray-length">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern CSSNumericValue this[uint index] { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssnumericvalue">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
/// </summary>
[ECMAScript]
[Description("@#CSSNumericValue")]
public class CSSNumericValue : CSSStyleValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-add">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-add-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    [Description("@#add")]
    public extern CSSNumericValue Add(params CSSNumberish[] values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-sub">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-sub-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    [Description("@#sub")]
    public extern CSSNumericValue Sub(params CSSNumberish[] values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-mul">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-mul-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    [Description("@#mul")]
    public extern CSSNumericValue Mul(params CSSNumberish[] values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-div">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-div-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    [Description("@#div")]
    public extern CSSNumericValue Div(params CSSNumberish[] values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-min">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-min-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    [Description("@#min")]
    public extern CSSNumericValue Min(params CSSNumberish[] values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-max">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-max-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    [Description("@#max")]
    public extern CSSNumericValue Max(params CSSNumberish[] values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-equals">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-equals-value-value">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    [Description("@#equals")]
    public extern bool Equals(params CSSNumberish[] value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-to">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    /// <param name="unit"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-to-unit-unit">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    [Description("@#to")]
    public extern CSSUnitValue To(string unit);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-tosum">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    /// <param name="units"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-tosum-units-units">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    [Description("@#toSum")]
    public extern CSSMathSum ToSum(params string[] units);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-type">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    [Description("@#type")]
    public extern CSSNumericType Type();

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-parse">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    /// <param name="cssText"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-parse-csstext-csstext">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    [Description("@#parse")]
    public static extern CSSNumericValue Parse(string cssText);
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssoklab">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSOKLab")]
public class CSSOKLab : CSSColorValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-cssoklab">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    /// <param name="l"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-cssoklab-l-a-b-alpha-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="a"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-cssoklab-l-a-b-alpha-a">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="b"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-cssoklab-l-a-b-alpha-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-cssoklab-l-a-b-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSOKLab(CSSColorPercent l, CSSColorNumber a, CSSColorNumber b, CSSColorPercent? alpha = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-a">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#a")]
    public extern CSSColorNumber A { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#b")]
    public extern CSSColorNumber B { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssoklch">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSOKLCH")]
public class CSSOKLCH : CSSColorValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-cssoklch">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    /// <param name="l"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-cssoklch-l-c-h-alpha-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="c"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-cssoklch-l-c-h-alpha-c">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="h"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-cssoklch-l-c-h-alpha-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-cssoklch-l-c-h-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSOKLCH(CSSColorPercent l, CSSColorPercent c, CSSColorAngle h, CSSColorPercent? alpha = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-c">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#c")]
    public extern CSSColorPercent C { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#h")]
    public extern CSSColorAngle H { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssperspective">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSPerspective")]
public class CSSPerspective : CSSTransformComponent
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssperspective-cssperspective">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    /// <param name="length"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssperspective-cssperspective-length-length">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    public extern CSSPerspective(CSSPerspectiveValue length);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssperspective-length">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#length")]
    public extern CSSPerspectiveValue Length { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssrgb">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSRGB")]
public class CSSRGB : CSSColorValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-cssrgb">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    /// <param name="r"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-cssrgb-r-g-b-alpha-r">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="g"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-cssrgb-r-g-b-alpha-g">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="b"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-cssrgb-r-g-b-alpha-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-cssrgb-r-g-b-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSRGB(CSSColorRGBComp r, CSSColorRGBComp g, CSSColorRGBComp b, CSSColorPercent? alpha = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-r">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#r")]
    public extern CSSColorRGBComp R { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-g">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#g")]
    public extern CSSColorRGBComp G { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#b")]
    public extern CSSColorRGBComp B { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssrotate">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSRotate")]
public class CSSRotate : CSSTransformComponent
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrotate-cssrotate">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    /// <param name="angle"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrotate-cssrotate-angle-angle">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    public extern CSSRotate(CSSNumericValue angle);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrotate-cssrotate-x-y-z-angle">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    /// <param name="x"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrotate-cssrotate-x-y-z-angle-x">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    /// <param name="y"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrotate-cssrotate-x-y-z-angle-y">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    /// <param name="z"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrotate-cssrotate-x-y-z-angle-z">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    /// <param name="angle"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrotate-cssrotate-x-y-z-angle-angle">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    public extern CSSRotate(CSSNumberish x, CSSNumberish y, CSSNumberish z, CSSNumericValue angle);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrotate-x">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#x")]
    public extern CSSNumberish X { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrotate-y">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#y")]
    public extern CSSNumberish Y { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrotate-z">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#z")]
    public extern CSSNumberish Z { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrotate-angle">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#angle")]
    public extern CSSNumericValue Angle { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssscale">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSScale")]
public class CSSScale : CSSTransformComponent
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssscale-cssscale">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    /// <param name="x"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssscale-cssscale-x-y-z-x">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    /// <param name="y"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssscale-cssscale-x-y-z-y">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    /// <param name="z"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssscale-cssscale-x-y-z-z">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    public extern CSSScale(CSSNumberish x, CSSNumberish y, CSSNumberish? z = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssscale-x">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#x")]
    public extern CSSNumberish X { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssscale-y">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#y")]
    public extern CSSNumberish Y { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssscale-z">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#z")]
    public extern CSSNumberish Z { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssskew">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSSkew")]
public class CSSSkew : CSSTransformComponent
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssskew-cssskew">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    /// <param name="ax"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssskew-cssskew-ax-ay-ax">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    /// <param name="ay"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssskew-cssskew-ax-ay-ay">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    public extern CSSSkew(CSSNumericValue ax, CSSNumericValue ay);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssskew-ax">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#ax")]
    public extern CSSNumericValue Ax { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssskew-ay">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#ay")]
    public extern CSSNumericValue Ay { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssskewx">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSSkewX")]
public class CSSSkewX : CSSTransformComponent
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssskewx-cssskewx">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    /// <param name="ax"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssskewx-cssskewx-ax-ax">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    public extern CSSSkewX(CSSNumericValue ax);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssskewx-ax">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#ax")]
    public extern CSSNumericValue Ax { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssskewy">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSSkewY")]
public class CSSSkewY : CSSTransformComponent
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssskewy-cssskewy">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    /// <param name="ay"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssskewy-cssskewy-ay-ay">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    public extern CSSSkewY(CSSNumericValue ay);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssskewy-ay">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#ay")]
    public extern CSSNumericValue Ay { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssstylevalue">CSS Typed OM Level 1: 2 CSSStyleValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSStyleValue")]
public class CSSStyleValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssstylevalue-parse">CSS Typed OM Level 1: 2 CSSStyleValue objects</see>
    /// </summary>
    /// <param name="property"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssstylevalue-parse-property-csstext-property">CSS Typed OM Level 1: 2 CSSStyleValue objects</see></param>
    /// <param name="cssText"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssstylevalue-parse-property-csstext-csstext">CSS Typed OM Level 1: 2 CSSStyleValue objects</see></param>
    [Description("@#parse")]
    public static extern CSSStyleValue Parse(string property, string cssText);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssstylevalue-parseall">CSS Typed OM Level 1: 2 CSSStyleValue objects</see>
    /// </summary>
    /// <param name="property"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssstylevalue-parseall-property-csstext-property">CSS Typed OM Level 1: 2 CSSStyleValue objects</see></param>
    /// <param name="cssText"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssstylevalue-parseall-property-csstext-csstext">CSS Typed OM Level 1: 2 CSSStyleValue objects</see></param>
    [Description("@#parseAll")]
    public static extern CSSStyleValue[] ParseAll(string property, string cssText);
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csstransformcomponent">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSTransformComponent")]
public class CSSTransformComponent
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstransformcomponent-is2d">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#is2D")]
    public extern bool Is2D { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstransformcomponent-tomatrix">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#toMatrix")]
    public extern DOMMatrix ToMatrix();
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csstransformvalue">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSTransformValue")]
public class CSSTransformValue : CSSStyleValue, IEnumerable<CSSTransformComponent>
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstransformvalue-csstransformvalue">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    /// <param name="transforms"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstransformvalue-csstransformvalue-transforms-transforms">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    public extern CSSTransformValue(CSSTransformComponent[] transforms);

    extern IEnumerator<CSSTransformComponent> IEnumerable<CSSTransformComponent>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstransformvalue-length">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern CSSTransformComponent this[uint index] { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstransformvalue-is2d">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#is2D")]
    public extern bool Is2D { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstransformvalue-tomatrix">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#toMatrix")]
    public extern DOMMatrix ToMatrix();
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csstranslate">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSTranslate")]
public class CSSTranslate : CSSTransformComponent
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstranslate-csstranslate">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    /// <param name="x"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstranslate-csstranslate-x-y-z-x">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    /// <param name="y"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstranslate-csstranslate-x-y-z-y">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    /// <param name="z"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstranslate-csstranslate-x-y-z-z">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
    public extern CSSTranslate(CSSNumericValue x, CSSNumericValue y, CSSNumericValue? z = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstranslate-x">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#x")]
    public extern CSSNumericValue X { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstranslate-y">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#y")]
    public extern CSSNumericValue Y { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csstranslate-z">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
    /// </summary>
    [Description("@#z")]
    public extern CSSNumericValue Z { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssunitvalue">CSS Typed OM Level 1: 4.3.3 Value + Unit: CSSUnitValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSUnitValue")]
public class CSSUnitValue : CSSNumericValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssunitvalue-cssunitvalue">CSS Typed OM Level 1: 4.3.3 Value + Unit: CSSUnitValue objects</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssunitvalue-cssunitvalue-value-unit-value">CSS Typed OM Level 1: 4.3.3 Value + Unit: CSSUnitValue objects</see></param>
    /// <param name="unit"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssunitvalue-cssunitvalue-value-unit-unit">CSS Typed OM Level 1: 4.3.3 Value + Unit: CSSUnitValue objects</see></param>
    public extern CSSUnitValue(double value, string unit);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssunitvalue-value">CSS Typed OM Level 1: 4.3.3 Value + Unit: CSSUnitValue objects</see>
    /// </summary>
    [Description("@#value")]
    public extern double Value { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssunitvalue-unit">CSS Typed OM Level 1: 4.3.3 Value + Unit: CSSUnitValue objects</see>
    /// </summary>
    [Description("@#unit")]
    public extern string Unit { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssunparsedvalue">CSS Typed OM Level 1: 4.1 CSSUnparsedValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSUnparsedValue")]
public class CSSUnparsedValue : CSSStyleValue, IEnumerable<CSSUnparsedSegment>
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssunparsedvalue-cssunparsedvalue">CSS Typed OM Level 1: 4.1 CSSUnparsedValue objects</see>
    /// </summary>
    /// <param name="members"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssunparsedvalue-cssunparsedvalue-members-members">CSS Typed OM Level 1: 4.1 CSSUnparsedValue objects</see></param>
    public extern CSSUnparsedValue(CSSUnparsedSegment[] members);

    extern IEnumerator<CSSUnparsedSegment> IEnumerable<CSSUnparsedSegment>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssunparsedvalue-length">CSS Typed OM Level 1: 4.1 CSSUnparsedValue objects</see>
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern CSSUnparsedSegment this[uint index] { get; set; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssvariablereferencevalue">CSS Typed OM Level 1: 4.1 CSSUnparsedValue objects</see>
/// </summary>
[ECMAScript]
[Description("@#CSSVariableReferenceValue")]
public class CSSVariableReferenceValue
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssvariablereferencevalue-cssvariablereferencevalue">CSS Typed OM Level 1: 4.1 CSSUnparsedValue objects</see>
    /// </summary>
    /// <param name="variable"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssvariablereferencevalue-cssvariablereferencevalue-variable-fallback-variable">CSS Typed OM Level 1: 4.1 CSSUnparsedValue objects</see></param>
    /// <param name="fallback"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssvariablereferencevalue-cssvariablereferencevalue-variable-fallback-fallback">CSS Typed OM Level 1: 4.1 CSSUnparsedValue objects</see></param>
    public extern CSSVariableReferenceValue(string variable, CSSUnparsedValue? fallback = default);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssvariablereferencevalue-variable">CSS Typed OM Level 1: 4.1 CSSUnparsedValue objects</see>
    /// </summary>
    [Description("@#variable")]
    public extern string Variable { get; set; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssvariablereferencevalue-fallback">CSS Typed OM Level 1: 4.1 CSSUnparsedValue objects</see>
    /// </summary>
    [Description("@#fallback")]
    public extern CSSUnparsedValue? Fallback { get; }
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#stylepropertymap">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
/// </summary>
[ECMAScript]
[Description("@#StylePropertyMap")]
public class StylePropertyMap : StylePropertyMapReadOnly
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymap-set">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    [Description("@#set")]
    public extern void Set(string property, params StylePropertyMapSetValues[] values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymap-set">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    [Description("@#set")]
    public extern void Set(string property, CSSStyleValue values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymap-set">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    [Description("@#set")]
    public extern void Set(string property, string values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymap-append">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    [Description("@#append")]
    public extern void Append(string property, params StylePropertyMapAppendValues[] values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymap-append">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    [Description("@#append")]
    public extern void Append(string property, CSSStyleValue values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymap-append">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    [Description("@#append")]
    public extern void Append(string property, string values);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymap-delete">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    /// <param name="property"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymap-delete-property-property">CSS Typed OM Level 1: 3 The StylePropertyMap</see></param>
    [Description("@#delete")]
    public extern void Delete(string property);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymap-clear">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#stylepropertymapreadonly">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
/// </summary>
[ECMAScript]
[Description("@#StylePropertyMapReadOnly")]
public class StylePropertyMapReadOnly : IEnumerable<(string, CSSStyleValue[])>
{
    extern IEnumerator<(string, CSSStyleValue[])> IEnumerable<(string, CSSStyleValue[])>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymapreadonly-get">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    /// <param name="property"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymapreadonly-get-property-property">CSS Typed OM Level 1: 3 The StylePropertyMap</see></param>
    [Description("@#get")]
    public extern CSSStyleValue? Get(string property);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymapreadonly-getall">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    /// <param name="property"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymapreadonly-getall-property-property">CSS Typed OM Level 1: 3 The StylePropertyMap</see></param>
    [Description("@#getAll")]
    public extern CSSStyleValue[] GetAll(string property);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymapreadonly-has">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    /// <param name="property"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymapreadonly-has-property-property">CSS Typed OM Level 1: 3 The StylePropertyMap</see></param>
    [Description("@#has")]
    public extern bool Has(string property);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-stylepropertymapreadonly-size">CSS Typed OM Level 1: 3 The StylePropertyMap</see>
    /// </summary>
    [Description("@#size")]
    public extern uint Size { get; }
}

/// <summary>
/// <see href="https://drafts.csswg.org/css-conditional-3/#cssconditionrule">CSS Conditional Rules Module Level 3: 7.2 The CSSConditionRule interface</see>
/// </summary>
[ECMAScript]
[Description("@#CSSConditionRule")]
public class CSSConditionRule : CSSGroupingRule
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/css-conditional-3/#dom-cssconditionrule-conditiontext">CSS Conditional Rules Module Level 3: 7.2 The CSSConditionRule interface</see>
    /// </summary>
    [Description("@#conditionText")]
    public extern string ConditionText { get; }
}

/// <summary>
/// <see href="https://drafts.csswg.org/css-conditional-3/#cssmediarule">CSS Conditional Rules Module Level 3: 7.3 The CSSMediaRule interface</see>
/// </summary>
[ECMAScript]
[Description("@#CSSMediaRule")]
public class CSSMediaRule : CSSConditionRule
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/css-conditional-3/#dom-cssmediarule-media">CSS Conditional Rules Module Level 3: 7.3 The CSSMediaRule interface</see>
    /// </summary>
    [Description("@#media")]
    public extern MediaList Media { get; }

    /// <summary>
    /// The matches attribute returns true if the rule is in an stylesheet attached to a document whose Window matches this rule&apos;s media media query, and returns false otherwise.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-conditional-3/#dom-cssmediarule-matches">CSS Conditional Rules Module Level 3: 7.3 The CSSMediaRule interface</see>
    /// </remarks>
    [Description("@#matches")]
    public extern bool Matches { get; }
}

/// <summary>
/// <see href="https://drafts.csswg.org/css-highlight-api-1/#highlightregistry">CSS Custom Highlight API Module Level 1: 3.2 Registering Custom Highlights</see>
/// </summary>
[ECMAScript]
[Description("@#HighlightRegistry")]
public partial class HighlightRegistry : IDictionary<string, Highlight>
{
    #region Dictionary
    extern Highlight IDictionary<string, Highlight>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, Highlight>.Keys { get; }
    extern ICollection<Highlight> IDictionary<string, Highlight>.Values { get; }
    extern int ICollection<KeyValuePair<string, Highlight>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, Highlight>>.IsReadOnly { get; }
    extern void IDictionary<string, Highlight>.Add(string key, Highlight value);
    extern void ICollection<KeyValuePair<string, Highlight>>.Add(KeyValuePair<string, Highlight> item);
    extern void ICollection<KeyValuePair<string, Highlight>>.Clear();
    extern bool ICollection<KeyValuePair<string, Highlight>>.Contains(KeyValuePair<string, Highlight> item);
    extern bool IDictionary<string, Highlight>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, Highlight>>.CopyTo(KeyValuePair<string, Highlight>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, Highlight>> IEnumerable<KeyValuePair<string, Highlight>>.GetEnumerator();
    extern bool IDictionary<string, Highlight>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, Highlight>>.Remove(KeyValuePair<string, Highlight> item);
    extern bool IDictionary<string, Highlight>.TryGetValue(string key, [MaybeNullWhen(false)] out Highlight value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlightregistry-highlightsfrompoint">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see>
    /// </summary>
    /// <param name="x"><see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlightregistry-highlightsfrompoint-x-y-options-x">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see></param>
    /// <param name="y"><see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlightregistry-highlightsfrompoint-x-y-options-y">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see></param>
    /// <param name="options"><see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlightregistry-highlightsfrompoint-x-y-options-options">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see></param>
    [Description("@#highlightsFromPoint")]
    public extern HighlightHitResult[] HighlightsFromPoint(float x, float y, HighlightsFromPointOptions? options = default);
}

/// <summary>
/// <see href="https://drafts.csswg.org/cssom-1/#csspagedescriptors">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
/// </summary>
[ECMAScript]
[Description("@#CSSPageDescriptors")]
public class CSSPageDescriptors : CSSStyleDeclaration
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margin">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#margin")]
    public extern string Margin { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margintop">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#marginTop")]
    public extern string MarginTop { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-marginright">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#marginRight")]
    public extern string MarginRight { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-marginbottom">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#marginBottom")]
    public extern string MarginBottom { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-marginleft">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#marginLeft")]
    public extern string MarginLeft { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margin-top">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#margin-top")]
    public extern string Margin_Top { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margin-right">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#margin-right")]
    public extern string Margin_Right { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margin-bottom">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#margin-bottom")]
    public extern string Margin_Bottom { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margin-left">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#margin-left")]
    public extern string Margin_Left { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-size">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#size")]
    public extern string Size { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-pageorientation">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#pageOrientation")]
    public extern string PageOrientation { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-page-orientation">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#page-orientation")]
    public extern string Page_Orientation { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-marks">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#marks")]
    public extern string Marks { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-bleed">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#bleed")]
    public extern string Bleed { get; set; }
}

/// <summary>
/// <see href="https://drafts.csswg.org/cssom-1/#cssrule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
/// </summary>
[ECMAScript]
[Description("@#CSSRule")]
public partial class CSSRule
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/css-conditional-3/#dom-cssrule-supports_rule">CSS Conditional Rules Module Level 3: 7.1 Extensions to the CSSRule interface</see>
    /// </summary>
    [Description("@#SUPPORTS_RULE")]
    public const ushort SUPPORTS_RULE = 12;

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-csstext">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#cssText")]
    public extern string CssText { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-parentrule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#parentRule")]
    public extern CSSRule? ParentRule { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-parentstylesheet">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#parentStyleSheet")]
    public extern CSSStyleSheet? ParentStyleSheet { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-type">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#type")]
    public extern ushort Type { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-style_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#STYLE_RULE")]
    public const ushort STYLE_RULE = 1;

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-charset_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#CHARSET_RULE")]
    public const ushort CHARSET_RULE = 2;

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-import_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#IMPORT_RULE")]
    public const ushort IMPORT_RULE = 3;

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-media_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#MEDIA_RULE")]
    public const ushort MEDIA_RULE = 4;

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-font_face_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#FONT_FACE_RULE")]
    public const ushort FONT_FACE_RULE = 5;

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-page_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#PAGE_RULE")]
    public const ushort PAGE_RULE = 6;

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-margin_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#MARGIN_RULE")]
    public const ushort MARGIN_RULE = 9;

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-namespace_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </summary>
    [Description("@#NAMESPACE_RULE")]
    public const ushort NAMESPACE_RULE = 10;
}

/// <summary>
/// <see href="https://drafts.csswg.org/cssom-1/#cssstyleproperties">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
/// </summary>
[ECMAScript]
[Description("@#CSSStyleProperties")]
public class CSSStyleProperties : CSSStyleDeclaration
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstyleproperties-cssfloat">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
    /// </summary>
    [Description("@#cssFloat")]
    public extern string CssFloat { get; set; }
}

/// <summary>
/// <see href="https://drafts.csswg.org/cssom-1/#stylesheet">CSS Object Model (CSSOM) Module Level 1: 6.1.1 The StyleSheet Interface</see>
/// </summary>
[ECMAScript]
[Description("@#StyleSheet")]
public class StyleSheet
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-stylesheet-type">CSS Object Model (CSSOM) Module Level 1: 6.1.1 The StyleSheet Interface</see>
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-stylesheet-href">CSS Object Model (CSSOM) Module Level 1: 6.1.1 The StyleSheet Interface</see>
    /// </summary>
    [Description("@#href")]
    public extern string? Href { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-stylesheet-ownernode">CSS Object Model (CSSOM) Module Level 1: 6.1.1 The StyleSheet Interface</see>
    /// </summary>
    [Description("@#ownerNode")]
    public extern StyleSheetOwnerNode? OwnerNode { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-stylesheet-parentstylesheet">CSS Object Model (CSSOM) Module Level 1: 6.1.1 The StyleSheet Interface</see>
    /// </summary>
    [Description("@#parentStyleSheet")]
    public extern CSSStyleSheet? ParentStyleSheet { get; }

    /// <summary>
    /// Set sheet&apos;s CSSStyleSheet/title to the empty string.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-stylesheet-title">CSS Object Model (CSSOM) Module Level 1: 6.1.1 The StyleSheet Interface</see>
    /// </remarks>
    [Description("@#title")]
    public extern string? Title { get; }

    /// <summary>
    /// If the media attribute of options is a string, create a MediaList object from the string and assign it as sheet&apos;s CSSStyleSheet/media. Otherwise, serialize a media query list from the attribute and then create a MediaList object from the resulting string and set it as sheet&apos;s CSSStyleSheet/media.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-stylesheet-media">CSS Object Model (CSSOM) Module Level 1: 6.1.1 The StyleSheet Interface</see>
    /// </remarks>
    [Description("@#media")]
    public extern MediaList Media { get; }

    /// <summary>
    /// If the disabled attribute of options is true, set sheet&apos;s CSSStyleSheet/disabled flag.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-stylesheet-disabled">CSS Object Model (CSSOM) Module Level 1: 6.1.1 The StyleSheet Interface</see>
    /// </remarks>
    [Description("@#disabled")]
    public extern bool Disabled { get; set; }
}

/// <summary>
/// <see href="https://drafts.csswg.org/cssom-1/#stylesheetlist">CSS Object Model (CSSOM) Module Level 1: 6.2.2 The StyleSheetList Interface</see>
/// </summary>
[ECMAScript]
[Description("@#StyleSheetList")]
public class StyleSheetList
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-stylesheetlist-item">CSS Object Model (CSSOM) Module Level 1: 6.2.2 The StyleSheetList Interface</see>
    /// </summary>
    /// <param name="index"><see href="https://drafts.csswg.org/cssom-1/#dom-stylesheetlist-item-index-index">CSS Object Model (CSSOM) Module Level 1: 6.2.2 The StyleSheetList Interface</see></param>
    [Description("@#item")]
    public extern CSSStyleSheet? GetItem(uint index);

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-stylesheetlist-length">CSS Object Model (CSSOM) Module Level 1: 6.2.2 The StyleSheetList Interface</see>
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }
}

/// <summary>
/// <see href="https://wicg.github.io/css-parser-api/#cssparseratrule">CSS Parser API: 3 Parser Values</see>
/// </summary>
[ECMAScript]
[Description("@#CSSParserAtRule")]
public class CSSParserAtRule : CSSParserRule
{
    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-cssparseratrule">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    /// <param name="name"><see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-cssparseratrule-name-prelude-body-name">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="prelude"><see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-cssparseratrule-name-prelude-body-prelude">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="body"><see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-cssparseratrule-name-prelude-body-body">CSS Parser API: 3 Parser Values</see></param>
    public extern CSSParserAtRule(string name, CSSToken[] prelude, CSSParserRule[]? body = default);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-name">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-prelude">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    [Description("@#prelude")]
    public extern FrozenSet<CSSParserValue> Prelude { get; }

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-body">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserRule>? Body { get; }
}

/// <summary>
/// <see href="https://wicg.github.io/css-parser-api/#cssparserblock">CSS Parser API: 3 Parser Values</see>
/// </summary>
[ECMAScript]
[Description("@#CSSParserBlock")]
public class CSSParserBlock : CSSParserValue
{
    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserblock-cssparserblock">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    /// <param name="name"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserblock-cssparserblock-name-body-name">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="body"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserblock-cssparserblock-name-body-body">CSS Parser API: 3 Parser Values</see></param>
    public extern CSSParserBlock(string name, CSSParserValue[] body);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserblock-name">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserblock-body">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserValue> Body { get; }
}

/// <summary>
/// <see href="https://wicg.github.io/css-parser-api/#cssparserdeclaration">CSS Parser API: 3 Parser Values</see>
/// </summary>
[ECMAScript]
[Description("@#CSSParserDeclaration")]
public class CSSParserDeclaration : CSSParserRule
{
    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserdeclaration-cssparserdeclaration">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    /// <param name="name"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserdeclaration-cssparserdeclaration-name-body-name">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="body"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserdeclaration-cssparserdeclaration-name-body-body">CSS Parser API: 3 Parser Values</see></param>
    public extern CSSParserDeclaration(string name, CSSParserRule[]? body = default);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserdeclaration-name">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserdeclaration-body">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserValue> Body { get; }
}

/// <summary>
/// <see href="https://wicg.github.io/css-parser-api/#cssparserfunction">CSS Parser API: 3 Parser Values</see>
/// </summary>
[ECMAScript]
[Description("@#CSSParserFunction")]
public class CSSParserFunction : CSSParserValue
{
    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserfunction-cssparserfunction">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    /// <param name="name"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserfunction-cssparserfunction-name-args-name">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="args"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserfunction-cssparserfunction-name-args-args">CSS Parser API: 3 Parser Values</see></param>
    public extern CSSParserFunction(string name, CSSParserValue[][] args);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserfunction-name">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserfunction-args">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    [Description("@#args")]
    public extern FrozenSet<FrozenSet<CSSParserValue>> Args { get; }
}

/// <summary>
/// <see href="https://wicg.github.io/css-parser-api/#cssparserqualifiedrule">CSS Parser API: 3 Parser Values</see>
/// </summary>
[ECMAScript]
[Description("@#CSSParserQualifiedRule")]
public class CSSParserQualifiedRule : CSSParserRule
{
    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserqualifiedrule-cssparserqualifiedrule">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    /// <param name="prelude"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserqualifiedrule-cssparserqualifiedrule-prelude-body-prelude">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="body"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserqualifiedrule-cssparserqualifiedrule-prelude-body-body">CSS Parser API: 3 Parser Values</see></param>
    public extern CSSParserQualifiedRule(CSSToken[] prelude, CSSParserRule[]? body = default);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserqualifiedrule-prelude">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    [Description("@#prelude")]
    public extern FrozenSet<CSSParserValue> Prelude { get; }

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserqualifiedrule-body">CSS Parser API: 3 Parser Values</see>
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserRule> Body { get; }
}

/// <summary>
/// <see href="https://wicg.github.io/css-parser-api/#cssparserrule">CSS Parser API: 3 Parser Values</see>
/// </summary>
[ECMAScript]
[Description("@#CSSParserRule")]
public abstract class CSSParserRule
{
}

/// <summary>
/// <see href="https://wicg.github.io/css-parser-api/#cssparservalue">CSS Parser API: 3 Parser Values</see>
/// </summary>
[ECMAScript]
[Description("@#CSSParserValue")]
public abstract class CSSParserValue
{
}

/// <summary>
/// Authors should not use these members and should instead use and teach the standard CSSStyleSheet interface defined earlier, which is consistent with CSSGroupingRule.
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#cssgroupingrule">CSS Object Model (CSSOM) Module Level 1: 6.4.5 The CSSGroupingRule Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSGroupingRule")]
public class CSSGroupingRule : CSSRule
{
    /// <summary>
    /// The result of performing serialize a CSS rule on each rule in the rule&apos;s cssRules list, filtering out empty strings, indenting each item with two spaces, all joined with newline.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssgroupingrule-cssrules">CSS Object Model (CSSOM) Module Level 1: 6.4.5 The CSSGroupingRule Interface</see>
    /// </remarks>
    [Description("@#cssRules")]
    public extern CSSRuleList CssRules { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssgroupingrule-insertrule">CSS Object Model (CSSOM) Module Level 1: 6.4.5 The CSSGroupingRule Interface</see>
    /// </summary>
    /// <param name="rule"><see href="https://drafts.csswg.org/cssom-1/#dom-cssgroupingrule-insertrule-rule-index-rule">CSS Object Model (CSSOM) Module Level 1: 6.4.5 The CSSGroupingRule Interface</see></param>
    /// <param name="index"><see href="https://drafts.csswg.org/cssom-1/#dom-cssgroupingrule-insertrule-rule-index-index">CSS Object Model (CSSOM) Module Level 1: 6.4.5 The CSSGroupingRule Interface</see></param>
    [Description("@#insertRule")]
    public extern uint InsertRule(string rule, uint index = 0);

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssgroupingrule-deleterule">CSS Object Model (CSSOM) Module Level 1: 6.4.5 The CSSGroupingRule Interface</see>
    /// </summary>
    /// <param name="index"><see href="https://drafts.csswg.org/cssom-1/#dom-cssgroupingrule-deleterule-index-index">CSS Object Model (CSSOM) Module Level 1: 6.4.5 The CSSGroupingRule Interface</see></param>
    [Description("@#deleteRule")]
    public extern void DeleteRule(uint index);
}

/// <summary>
/// Authors should not use these members and should instead use and teach the standard CSSStyleSheet interface defined earlier, which is consistent with CSSGroupingRule.
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#cssstylesheet">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSStyleSheet")]
public partial class CSSStyleSheet : StyleSheet
{
    /// <summary>
    /// Set sheet&apos;s CSSStyleSheet/Constructor document to the associated Document for the current global object.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-cssstylesheet">CSS Object Model (CSSOM) Module Level 1: 6.1 CSS Style Sheets</see>
    /// </remarks>
    /// <param name="options"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-cssstylesheet-options-options">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see></param>
    public extern CSSStyleSheet(CSSStyleSheetInit? options = default);

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-ownerrule">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see>
    /// </summary>
    [Description("@#ownerRule")]
    public extern CSSRule? OwnerRule { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-cssrules">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see>
    /// </summary>
    [Description("@#cssRules")]
    public extern CSSRuleList CssRules { get; }

    /// <summary>
    /// Call insertRule(), with rule and index as arguments.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-insertrule">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see>
    /// </remarks>
    /// <param name="rule">Call insertRule(), with rule and index as arguments. <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-insertrule-rule-index-rule">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see></param>
    /// <param name="index">Call insertRule(), with rule and index as arguments. <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-insertrule-rule-index-index">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see></param>
    [Description("@#insertRule")]
    public extern uint InsertRule(string rule, uint index = 0);

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-deleterule">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see>
    /// </summary>
    /// <param name="index"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-deleterule-index-index">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see></param>
    [Description("@#deleteRule")]
    public extern void DeleteRule(uint index);

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-replace">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see>
    /// </summary>
    /// <param name="text"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-replace-text-text">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see></param>
    [Description("@#replace")]
    public extern PromiseResult<CSSStyleSheet> Replace(string text);

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-replacesync">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see>
    /// </summary>
    /// <param name="text"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-replacesync-text-text">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see></param>
    [Description("@#replaceSync")]
    public extern void ReplaceSync(string text);

    /// <summary>
    /// The CSS rules associated with the CSS style sheet.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-rules">CSS Object Model (CSSOM) Module Level 1: 6.1.2.1 Deprecated CSSStyleSheet members</see>
    /// </remarks>
    [Description("@#rules")]
    public extern CSSRuleList Rules { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-addrule">CSS Object Model (CSSOM) Module Level 1: 6.1.2.1 Deprecated CSSStyleSheet members</see>
    /// </summary>
    /// <param name="selector"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-addrule-selector-style-index-selector">CSS Object Model (CSSOM) Module Level 1: 6.1.2.1 Deprecated CSSStyleSheet members</see></param>
    /// <param name="style"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-addrule-selector-style-index-style">CSS Object Model (CSSOM) Module Level 1: 6.1.2.1 Deprecated CSSStyleSheet members</see></param>
    /// <param name="index"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-addrule-selector-style-index-index">CSS Object Model (CSSOM) Module Level 1: 6.1.2.1 Deprecated CSSStyleSheet members</see></param>
    [Description("@#addRule")]
    public extern int AddRule(string selector = "undefined", string style = "undefined", uint? index = default);

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-removerule">CSS Object Model (CSSOM) Module Level 1: 6.1.2.1 Deprecated CSSStyleSheet members</see>
    /// </summary>
    /// <param name="index"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-removerule-index-index">CSS Object Model (CSSOM) Module Level 1: 6.1.2.1 Deprecated CSSStyleSheet members</see></param>
    [Description("@#removeRule")]
    public extern void RemoveRule(uint index = 0);
}

/// <summary>
/// CSSImportRule
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#cssimportrule">CSS Object Model (CSSOM) Module Level 1: 6.4.4 The CSSImportRule Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSImportRule")]
public class CSSImportRule : CSSRule
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssimportrule-href">CSS Object Model (CSSOM) Module Level 1: 6.4.4 The CSSImportRule Interface</see>
    /// </summary>
    [Description("@#href")]
    public extern string Href { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssimportrule-media">CSS Object Model (CSSOM) Module Level 1: 6.4.4 The CSSImportRule Interface</see>
    /// </summary>
    [Description("@#media")]
    public extern MediaList Media { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssimportrule-stylesheet">CSS Object Model (CSSOM) Module Level 1: 6.4.4 The CSSImportRule Interface</see>
    /// </summary>
    [Description("@#styleSheet")]
    public extern CSSStyleSheet? StyleSheet { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssimportrule-layername">CSS Object Model (CSSOM) Module Level 1: 6.4.4 The CSSImportRule Interface</see>
    /// </summary>
    [Description("@#layerName")]
    public extern string? LayerName { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssimportrule-supportstext">CSS Object Model (CSSOM) Module Level 1: 6.4.4 The CSSImportRule Interface</see>
    /// </summary>
    [Description("@#supportsText")]
    public extern string? SupportsText { get; }
}

/// <summary>
/// CSSNamespaceRule
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#cssnamespacerule">CSS Object Model (CSSOM) Module Level 1: 6.4.9 The CSSNamespaceRule Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSNamespaceRule")]
public class CSSNamespaceRule : CSSRule
{
    /// <summary>
    /// The literal string &quot;@namespace&quot;, followed by a single SPACE (U+0020), followed by the serialization as an identifier of the prefix attribute (if any), followed by a single SPACE (U+0020) if there is a prefix, followed by the serialization as URL of the namespaceURI attribute, followed the character &quot;;&quot; (U+003B).
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssnamespacerule-namespaceuri">CSS Object Model (CSSOM) Module Level 1: 6.4.9 The CSSNamespaceRule Interface</see>
    /// </remarks>
    [Description("@#namespaceURI")]
    public extern string NamespaceURI { get; }

    /// <summary>
    /// The literal string &quot;@namespace&quot;, followed by a single SPACE (U+0020), followed by the serialization as an identifier of the prefix attribute (if any), followed by a single SPACE (U+0020) if there is a prefix, followed by the serialization as URL of the namespaceURI attribute, followed the character &quot;;&quot; (U+003B).
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssnamespacerule-prefix">CSS Object Model (CSSOM) Module Level 1: 6.4.9 The CSSNamespaceRule Interface</see>
    /// </remarks>
    [Description("@#prefix")]
    public extern string Prefix { get; }
}

/// <summary>
/// CSSStyleRule
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#cssstylerule">CSS Object Model (CSSOM) Module Level 1: 6.4.3 The CSSStyleRule Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSStyleRule")]
public partial class CSSStyleRule : CSSGroupingRule
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssstylerule-stylemap">CSS Typed OM Level 1: 3.2 Declared &amp; Inline StylePropertyMap objects</see>
    /// </summary>
    [Description("@#styleMap")]
    public extern StylePropertyMap StyleMap { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylerule-selectortext">CSS Object Model (CSSOM) Module Level 1: 6.4.3 The CSSStyleRule Interface</see>
    /// </summary>
    [Description("@#selectorText")]
    public extern string SelectorText { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylerule-style">CSS Object Model (CSSOM) Module Level 1: 6.4.3 The CSSStyleRule Interface</see>
    /// </summary>
    [Description("@#style")]
    public extern CSSStyleProperties Style { get; }
}

/// <summary>
/// Create a new MediaList object.
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#medialist">CSS Object Model (CSSOM) Module Level 1: 4.4 The MediaList Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#MediaList")]
public class MediaList
{
    /// <summary>
    /// Set its mediaText attribute to text.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-medialist-mediatext">CSS Object Model (CSSOM) Module Level 1: 4.4 The MediaList Interface</see>
    /// </remarks>
    [Description("@#mediaText")]
    public extern string MediaText { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-medialist-length">CSS Object Model (CSSOM) Module Level 1: 4.4 The MediaList Interface</see>
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-medialist-item">CSS Object Model (CSSOM) Module Level 1: 4.4 The MediaList Interface</see>
    /// </summary>
    /// <param name="index"><see href="https://drafts.csswg.org/cssom-1/#dom-medialist-item-index-index">CSS Object Model (CSSOM) Module Level 1: 4.4 The MediaList Interface</see></param>
    [Description("@#item")]
    public extern string? GetItem(uint index);

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-medialist-appendmedium">CSS Object Model (CSSOM) Module Level 1: 4.4 The MediaList Interface</see>
    /// </summary>
    /// <param name="medium"><see href="https://drafts.csswg.org/cssom-1/#dom-medialist-appendmedium-medium-medium">CSS Object Model (CSSOM) Module Level 1: 4.4 The MediaList Interface</see></param>
    [Description("@#appendMedium")]
    public extern void AppendMedium(string medium);

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-medialist-deletemedium">CSS Object Model (CSSOM) Module Level 1: 4.4 The MediaList Interface</see>
    /// </summary>
    /// <param name="medium"><see href="https://drafts.csswg.org/cssom-1/#dom-medialist-deletemedium-medium-medium">CSS Object Model (CSSOM) Module Level 1: 4.4 The MediaList Interface</see></param>
    [Description("@#deleteMedium")]
    public extern void DeleteMedium(string medium);
}

/// <summary>
/// If the object is a CSSMarginRule
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#cssmarginrule">CSS Object Model (CSSOM) Module Level 1: 6.4.8 The CSSMarginRule Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSMarginRule")]
public class CSSMarginRule : CSSRule
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssmarginrule-name">CSS Object Model (CSSOM) Module Level 1: 6.4.8 The CSSMarginRule Interface</see>
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssmarginrule-style">CSS Object Model (CSSOM) Module Level 1: 6.4.8 The CSSMarginRule Interface</see>
    /// </summary>
    [Description("@#style")]
    public extern CSSStyleDeclaration Style { get; }
}

/// <summary>
/// If the object is a CSSPageRule
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#csspagerule">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSPageRule")]
public class CSSPageRule : CSSGroupingRule
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagerule-selectortext">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#selectorText")]
    public extern string SelectorText { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagerule-style">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </summary>
    [Description("@#style")]
    public extern CSSPageDescriptors Style { get; }
}

/// <summary>
/// Let highlight be the new Highlight object.
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/css-highlight-api-1/#highlight">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see>
/// </remarks>
[ECMAScript]
[Description("@#Highlight")]
public class Highlight : ISet<AbstractRange>
{
    /// <summary>
    /// Let highlight be the new Highlight object.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlight-highlight">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see>
    /// </remarks>
    /// <param name="initialRanges"><see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlight-highlight-initialranges-initialranges">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see></param>
    public extern Highlight(params AbstractRange[] initialRanges);

    #region Set
    extern int ICollection<AbstractRange>.Count { get; }
    extern bool ICollection<AbstractRange>.IsReadOnly { get; }
    extern bool ISet<AbstractRange>.Add(AbstractRange item);
    extern void ICollection<AbstractRange>.Clear();
    extern bool ICollection<AbstractRange>.Contains(AbstractRange item);
    extern void ICollection<AbstractRange>.CopyTo(AbstractRange[] array, int arrayIndex);
    extern void ISet<AbstractRange>.ExceptWith(IEnumerable<AbstractRange> other);
    extern IEnumerator<AbstractRange> IEnumerable<AbstractRange>.GetEnumerator();
    extern void ISet<AbstractRange>.IntersectWith(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsProperSubsetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsProperSupersetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsSubsetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsSupersetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.Overlaps(IEnumerable<AbstractRange> other);
    extern bool ICollection<AbstractRange>.Remove(AbstractRange item);
    extern bool ISet<AbstractRange>.SetEquals(IEnumerable<AbstractRange> other);
    extern void ISet<AbstractRange>.SymmetricExceptWith(IEnumerable<AbstractRange> other);
    extern void ISet<AbstractRange>.UnionWith(IEnumerable<AbstractRange> other);
    extern void ICollection<AbstractRange>.Add(AbstractRange item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion

    /// <summary>
    /// Set highlight&apos;s priority to 0.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlight-priority">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see>
    /// </remarks>
    [Description("@#priority")]
    public extern int Priority { get; set; }

    /// <summary>
    /// Set highlight&apos;s type to highlight.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlight-type">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see>
    /// </remarks>
    [Description("@#type")]
    public extern HighlightType Type { get; set; }
}

/// <summary>
/// Return a read-only, live CSSRuleList object representing the CSS rules.
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#cssrulelist">CSS Object Model (CSSOM) Module Level 1: 6.4.1 The CSSRuleList Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSRuleList")]
public class CSSRuleList
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrulelist-item">CSS Object Model (CSSOM) Module Level 1: 6.4.1 The CSSRuleList Interface</see>
    /// </summary>
    /// <param name="index"><see href="https://drafts.csswg.org/cssom-1/#dom-cssrulelist-item-index-index">CSS Object Model (CSSOM) Module Level 1: 6.4.1 The CSSRuleList Interface</see></param>
    [Description("@#item")]
    public extern CSSRule? GetItem(uint index);

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrulelist-length">CSS Object Model (CSSOM) Module Level 1: 6.4.1 The CSSRuleList Interface</see>
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }
}

/// <summary>
/// conditionText of type CSSOMString (CSSSupportsRule-specific definition for attribute on CSSConditionRule)
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/css-conditional-3/#csssupportsrule">CSS Conditional Rules Module Level 3: 7.4 The CSSSupportsRule interface</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSSupportsRule")]
public class CSSSupportsRule : CSSConditionRule
{
    /// <summary>
    /// The matches attribute returns the evaluation of the CSS feature query represented in conditionText.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-conditional-3/#dom-csssupportsrule-matches">CSS Conditional Rules Module Level 3: 7.4 The CSSSupportsRule interface</see>
    /// </remarks>
    [Description("@#matches")]
    public extern bool Matches { get; }
}

/// <summary>
/// setPropertyValue and setPropertyPriority are added to CSSStyleDeclaration.
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#cssstyledeclaration">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSStyleDeclaration")]
public class CSSStyleDeclaration
{
    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-csstext">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
    /// </summary>
    [Description("@#cssText")]
    public extern string CssText { get; set; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-length">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-item">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
    /// </summary>
    /// <param name="index"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-item-index-index">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see></param>
    [Description("@#item")]
    public extern string GetItem(uint index);

    /// <summary>
    /// Let value be the return value of invoking getPropertyValue() with property as argument.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-getpropertyvalue">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
    /// </remarks>
    /// <param name="property"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-getpropertyvalue-property-property">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see></param>
    [Description("@#getPropertyValue")]
    public extern string GetPropertyValue(string property);

    /// <summary>
    /// For each longhand property longhand that property maps to, append the result of invoking getPropertyPriority() with longhand as argument to list.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-getpropertypriority">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
    /// </remarks>
    /// <param name="property">For each longhand property longhand that property maps to, append the result of invoking getPropertyPriority() with longhand as argument to list. <see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-getpropertypriority-property-property">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see></param>
    [Description("@#getPropertyPriority")]
    public extern string GetPropertyPriority(string property);

    /// <summary>
    /// Shorthands are now supported in setProperty(), getPropertyValue(), et al.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-setproperty">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
    /// </remarks>
    /// <param name="property"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-setproperty-property-value-priority-property">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see></param>
    /// <param name="value"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-setproperty-property-value-priority-value">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see></param>
    /// <param name="priority"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-setproperty-property-value-priority-priority">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see></param>
    [Description("@#setProperty")]
    public extern void SetProperty(string property, string value, string priority = "");

    /// <summary>
    /// If value is the empty string, invoke removeProperty() with property as argument and return.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-removeproperty">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
    /// </remarks>
    /// <param name="property"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-removeproperty-property-property">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see></param>
    [Description("@#removeProperty")]
    public extern string RemoveProperty(string property);

    /// <summary>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstyledeclaration-parentrule">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
    /// </summary>
    [Description("@#parentRule")]
    public extern CSSRule? ParentRule { get; }
}

[ECMAScript]
[Description("@#Element")]
public partial class Element
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-element-computedstylemap">CSS Typed OM Level 1: 3.1 Computed StylePropertyMapReadOnly objects</see>
    /// </summary>
    [Description("@#computedStyleMap")]
    public extern StylePropertyMapReadOnly ComputedStyleMap();
}

[ECMAScript]
[Description("@#Window")]
public partial class Window
{
    /// <summary>
    /// The getComputedStyle() method exposes information from CSS style sheets with the origin-clean flag unset.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-window-getcomputedstyle">CSS Object Model (CSSOM) Module Level 1: 7.2 Extensions to the Window Interface</see>
    /// </remarks>
    /// <param name="elt"><see href="https://drafts.csswg.org/cssom-1/#dom-window-getcomputedstyle-elt-pseudoelt-elt">CSS Object Model (CSSOM) Module Level 1: 7.2 Extensions to the Window Interface</see></param>
    /// <param name="pseudoElt"><see href="https://drafts.csswg.org/cssom-1/#dom-window-getcomputedstyle-elt-pseudoelt-pseudoelt">CSS Object Model (CSSOM) Module Level 1: 7.2 Extensions to the Window Interface</see></param>
    [Description("@#getComputedStyle")]
    public extern CSSStyleProperties GetComputedStyle(Element elt, string? pseudoElt = default);
}
