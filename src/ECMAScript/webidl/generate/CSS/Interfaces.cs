namespace ECMAScript.CSS;

/// <summary>
/// WebIDL interface AnimationWorkletGlobalScope。定义于 CSS Animation Worklet API。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#animationworkletglobalscope">CSS Animation Worklet API: 2 Animation Worklet</see>
/// </remarks>
[ECMAScript]
[Description("@#AnimationWorkletGlobalScope")]
public class AnimationWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// JavaScript AnimationWorkletGlobalScope.registerAnimator(name, animatorCtor) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-animationworkletglobalscope-registeranimator">CSS Animation Worklet API: 3.4 Registering an Animator Definition</see>
    /// </remarks>
    /// <param name="name"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-animationworkletglobalscope-registeranimator-name-animatorctor-name">CSS Animation Worklet API: 2 Animation Worklet</see></param>
    /// <param name="animatorCtor"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-animationworkletglobalscope-registeranimator-name-animatorctor-animatorctor">CSS Animation Worklet API: 2 Animation Worklet</see></param>
    [Description("@#registerAnimator")]
    public extern void RegisterAnimator(string name, AnimatorInstanceConstructor animatorCtor);
}

/// <summary>
/// WebIDL interface WorkletAnimation。定义于 CSS Animation Worklet API。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#workletanimation">CSS Animation Worklet API: 5.1 Worklet Animation</see>
/// </remarks>
[ECMAScript]
[Description("@#WorkletAnimation")]
public class WorkletAnimation(AnimationEffect? effect, AnimationTimeline? timeline) : Animation(effect, timeline)
{
    /// <summary>
    /// 构造浏览器提供的 WorkletAnimation 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-workletanimation">CSS Animation Worklet API: 5.2 Creating a Worklet Animation</see>
    /// </remarks>
    /// <param name="animatorName"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-workletanimation-animatorname-effects-timeline-options-animatorname">CSS Animation Worklet API: 5.1 Worklet Animation</see></param>
    /// <param name="effects"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-workletanimation-animatorname-effects-timeline-options-effects">CSS Animation Worklet API: 5.1 Worklet Animation</see></param>
    /// <param name="timeline"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-workletanimation-animatorname-effects-timeline-options-timeline">CSS Animation Worklet API: 5.1 Worklet Animation</see></param>
    /// <param name="options"><see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-workletanimation-animatorname-effects-timeline-options-options">CSS Animation Worklet API: 5.1 Worklet Animation</see></param>
    public extern WorkletAnimation(string animatorName, WorkletAnimationEffects? effects = default, AnimationTimeline? timeline = default, object? options = default);

    /// <summary>
    /// JavaScript 属性 WorkletAnimation.animatorName：DOMString。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimation-animatorname">CSS Animation Worklet API: 5.1 Worklet Animation</see>
    /// </remarks>
    [Description("@#animatorName")]
    public extern string AnimatorName { get; }
}

/// <summary>
/// WebIDL interface WorkletAnimationEffect。定义于 CSS Animation Worklet API。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#workletanimationeffect">CSS Animation Worklet API: 3.5 Animator Effect</see>
/// </remarks>
[ECMAScript]
[Description("@#WorkletAnimationEffect")]
public class WorkletAnimationEffect
{
    /// <summary>
    /// JavaScript WorkletAnimationEffect.getTiming() 的强类型绑定，WebIDL 返回类型为 EffectTiming。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimationeffect-gettiming">CSS Animation Worklet API: 3.5 Animator Effect</see>
    /// </remarks>
    [Description("@#getTiming")]
    public extern EffectTiming GetTiming();

    /// <summary>
    /// JavaScript WorkletAnimationEffect.getComputedTiming() 的强类型绑定，WebIDL 返回类型为 ComputedEffectTiming。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimationeffect-getcomputedtiming">CSS Animation Worklet API: 3.5 Animator Effect</see>
    /// </remarks>
    [Description("@#getComputedTiming")]
    public extern ComputedEffectTiming GetComputedTiming();

    /// <summary>
    /// JavaScript 属性 WorkletAnimationEffect.localTime：double?。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletanimationeffect-localtime">CSS Animation Worklet API: 3.5 Animator Effect</see>
    /// </remarks>
    [Description("@#localTime")]
    public extern double? LocalTime { get; set; }
}

/// <summary>
/// WebIDL interface WorkletGroupEffect。定义于 CSS Animation Worklet API。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#workletgroupeffect">CSS Animation Worklet API: 6.1 Worklet Group Effect</see>
/// </remarks>
[ECMAScript]
[Description("@#WorkletGroupEffect")]
public class WorkletGroupEffect
{
    /// <summary>
    /// JavaScript WorkletGroupEffect.getChildren() 的强类型绑定，WebIDL 返回类型为 sequence&lt;WorkletAnimationEffect&gt;。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-workletgroupeffect-getchildren">CSS Animation Worklet API: 6.1 Worklet Group Effect</see>
    /// </remarks>
    [Description("@#getChildren")]
    public extern WorkletAnimationEffect[] GetChildren();
}

/// <summary>
/// WebIDL interface BreakToken。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#breaktoken">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
/// </remarks>
[ECMAScript]
[Description("@#BreakToken")]
public class BreakToken
{
    /// <summary>
    /// JavaScript 属性 BreakToken.childBreakTokens：FrozenArray&lt;ChildBreakToken&gt;。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktoken-childbreaktokens">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </remarks>
    [Description("@#childBreakTokens")]
    public extern FrozenSet<ChildBreakToken> ChildBreakTokens { get; }

    /// <summary>
    /// JavaScript 属性 BreakToken.data：any。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktoken-data">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </remarks>
    [Description("@#data")]
    public extern object Data { get; }
}

/// <summary>
/// WebIDL interface ChildBreakToken。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#childbreaktoken">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
/// </remarks>
[ECMAScript]
[Description("@#ChildBreakToken")]
public class ChildBreakToken
{
    /// <summary>
    /// JavaScript 属性 ChildBreakToken.breakType：BreakType。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-childbreaktoken-breaktype">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </remarks>
    [Description("@#breakType")]
    public extern BreakType BreakType { get; }

    /// <summary>
    /// JavaScript 属性 ChildBreakToken.child：LayoutChild。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-childbreaktoken-child">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </remarks>
    [Description("@#child")]
    public extern LayoutChild Child { get; }
}

/// <summary>
/// WebIDL interface FragmentResult。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#fragmentresult">CSS Layout API Level 1: 6.2 Performing Layout</see>
/// </remarks>
[ECMAScript]
[Description("@#FragmentResult")]
public class FragmentResult
{
    /// <summary>
    /// 构造浏览器提供的 FragmentResult 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresult-fragmentresult">CSS Layout API Level 1: 6.2 Performing Layout</see>
    /// </remarks>
    /// <param name="options"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresult-fragmentresult-options-options">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
    public extern FragmentResult(FragmentResultOptions? options = default);

    /// <summary>
    /// JavaScript 属性 FragmentResult.inlineSize：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresult-inlinesize">CSS Layout API Level 1: 6.2 Performing Layout</see>
    /// </remarks>
    [Description("@#inlineSize")]
    public extern double InlineSize { get; }

    /// <summary>
    /// JavaScript 属性 FragmentResult.blockSize：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresult-blocksize">CSS Layout API Level 1: 6.2 Performing Layout</see>
    /// </remarks>
    [Description("@#blockSize")]
    public extern double BlockSize { get; }
}

/// <summary>
/// WebIDL interface IntrinsicSizes。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#intrinsicsizes">CSS Layout API Level 1: 4.3 Intrinsic Sizes</see>
/// </remarks>
[ECMAScript]
[Description("@#IntrinsicSizes")]
public class IntrinsicSizes
{
    /// <summary>
    /// JavaScript 属性 IntrinsicSizes.minContentSize：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-intrinsicsizes-mincontentsize">CSS Layout API Level 1: 4.3 Intrinsic Sizes</see>
    /// </remarks>
    [Description("@#minContentSize")]
    public extern double MinContentSize { get; }

    /// <summary>
    /// JavaScript 属性 IntrinsicSizes.maxContentSize：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-intrinsicsizes-maxcontentsize">CSS Layout API Level 1: 4.3 Intrinsic Sizes</see>
    /// </remarks>
    [Description("@#maxContentSize")]
    public extern double MaxContentSize { get; }
}

/// <summary>
/// WebIDL interface LayoutChild。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#layoutchild">CSS Layout API Level 1: 4.1 Layout Children</see>
/// </remarks>
[ECMAScript]
[Description("@#LayoutChild")]
public class LayoutChild
{
    /// <summary>
    /// JavaScript 属性 LayoutChild.styleMap：StylePropertyMapReadOnly。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutchild-stylemap">CSS Layout API Level 1: 4.1 Layout Children</see>
    /// </remarks>
    [Description("@#styleMap")]
    public extern StylePropertyMapReadOnly StyleMap { get; }

    /// <summary>
    /// JavaScript LayoutChild.intrinsicSizes() 的强类型绑定，WebIDL 返回类型为 Promise&lt;IntrinsicSizes&gt;。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutchild-intrinsicsizes">CSS Layout API Level 1: 4.1 Layout Children</see>
    /// </remarks>
    [Description("@#intrinsicSizes")]
    public extern PromiseResult<IntrinsicSizes> IntrinsicSizes();

    /// <summary>
    /// JavaScript LayoutChild.layoutNextFragment(constraints, breakToken) 的强类型绑定，WebIDL 返回类型为 Promise&lt;LayoutFragment&gt;。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutchild-layoutnextfragment">CSS Layout API Level 1: 4.1 Layout Children</see>
    /// </remarks>
    /// <param name="constraints"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutchild-layoutnextfragment-constraints-breaktoken-constraints">CSS Layout API Level 1: 4.1 Layout Children</see></param>
    /// <param name="breakToken"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutchild-layoutnextfragment-constraints-breaktoken-breaktoken">CSS Layout API Level 1: 4.1 Layout Children</see></param>
    [Description("@#layoutNextFragment")]
    public extern PromiseResult<LayoutFragment> LayoutNextFragment(LayoutConstraintsOptions constraints, ChildBreakToken breakToken);
}

/// <summary>
/// WebIDL interface LayoutConstraints。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#layoutconstraints">CSS Layout API Level 1: 4.4 Layout Constraints</see>
/// </remarks>
[ECMAScript]
[Description("@#LayoutConstraints")]
public class LayoutConstraints
{
    /// <summary>
    /// JavaScript 属性 LayoutConstraints.availableInlineSize：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-availableinlinesize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#availableInlineSize")]
    public extern double AvailableInlineSize { get; }

    /// <summary>
    /// JavaScript 属性 LayoutConstraints.availableBlockSize：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-availableblocksize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#availableBlockSize")]
    public extern double AvailableBlockSize { get; }

    /// <summary>
    /// JavaScript 属性 LayoutConstraints.fixedInlineSize：double?。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-fixedinlinesize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#fixedInlineSize")]
    public extern double? FixedInlineSize { get; }

    /// <summary>
    /// JavaScript 属性 LayoutConstraints.fixedBlockSize：double?。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-fixedblocksize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#fixedBlockSize")]
    public extern double? FixedBlockSize { get; }

    /// <summary>
    /// JavaScript 属性 LayoutConstraints.percentageInlineSize：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-percentageinlinesize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#percentageInlineSize")]
    public extern double PercentageInlineSize { get; }

    /// <summary>
    /// JavaScript 属性 LayoutConstraints.percentageBlockSize：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-percentageblocksize">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#percentageBlockSize")]
    public extern double PercentageBlockSize { get; }

    /// <summary>
    /// JavaScript 属性 LayoutConstraints.blockFragmentationOffset：double?。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-blockfragmentationoffset">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#blockFragmentationOffset")]
    public extern double? BlockFragmentationOffset { get; }

    /// <summary>
    /// JavaScript 属性 LayoutConstraints.blockFragmentationType：BlockFragmentationType。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-blockfragmentationtype">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#blockFragmentationType")]
    public extern BlockFragmentationType BlockFragmentationType { get; }

    /// <summary>
    /// JavaScript 属性 LayoutConstraints.data：any。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraints-data">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#data")]
    public extern object Data { get; }
}

/// <summary>
/// WebIDL interface LayoutEdges。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#layoutedges">CSS Layout API Level 1: 4.6 Edges</see>
/// </remarks>
[ECMAScript]
[Description("@#LayoutEdges")]
public class LayoutEdges
{
    /// <summary>
    /// JavaScript 属性 LayoutEdges.inlineStart：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-inlinestart">CSS Layout API Level 1: 4.6 Edges</see>
    /// </remarks>
    [Description("@#inlineStart")]
    public extern double InlineStart { get; }

    /// <summary>
    /// JavaScript 属性 LayoutEdges.inlineEnd：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-inlineend">CSS Layout API Level 1: 4.6 Edges</see>
    /// </remarks>
    [Description("@#inlineEnd")]
    public extern double InlineEnd { get; }

    /// <summary>
    /// JavaScript 属性 LayoutEdges.blockStart：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-blockstart">CSS Layout API Level 1: 4.6 Edges</see>
    /// </remarks>
    [Description("@#blockStart")]
    public extern double BlockStart { get; }

    /// <summary>
    /// JavaScript 属性 LayoutEdges.blockEnd：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-blockend">CSS Layout API Level 1: 4.6 Edges</see>
    /// </remarks>
    [Description("@#blockEnd")]
    public extern double BlockEnd { get; }

    /// <summary>
    /// JavaScript 属性 LayoutEdges.inline：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-inline">CSS Layout API Level 1: 4.6 Edges</see>
    /// </remarks>
    [Description("@#inline")]
    public extern double Inline { get; }

    /// <summary>
    /// JavaScript 属性 LayoutEdges.block：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutedges-block">CSS Layout API Level 1: 4.6 Edges</see>
    /// </remarks>
    [Description("@#block")]
    public extern double Block { get; }
}

/// <summary>
/// WebIDL interface LayoutFragment。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#layoutfragment">CSS Layout API Level 1: 4.2 Layout Fragments</see>
/// </remarks>
[ECMAScript]
[Description("@#LayoutFragment")]
public class LayoutFragment
{
    /// <summary>
    /// JavaScript 属性 LayoutFragment.inlineSize：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-inlinesize">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </remarks>
    [Description("@#inlineSize")]
    public extern double InlineSize { get; }

    /// <summary>
    /// JavaScript 属性 LayoutFragment.blockSize：double。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-blocksize">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </remarks>
    [Description("@#blockSize")]
    public extern double BlockSize { get; }

    /// <summary>
    /// JavaScript 属性 LayoutFragment.inlineOffset：double。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-inlineoffset">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </remarks>
    [Description("@#inlineOffset")]
    public extern double InlineOffset { get; set; }

    /// <summary>
    /// JavaScript 属性 LayoutFragment.blockOffset：double。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-blockoffset">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </remarks>
    [Description("@#blockOffset")]
    public extern double BlockOffset { get; set; }

    /// <summary>
    /// JavaScript 属性 LayoutFragment.data：any。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-data">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </remarks>
    [Description("@#data")]
    public extern object Data { get; }

    /// <summary>
    /// JavaScript 属性 LayoutFragment.breakToken：ChildBreakToken?。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutfragment-breaktoken">CSS Layout API Level 1: 4.2 Layout Fragments</see>
    /// </remarks>
    [Description("@#breakToken")]
    public extern ChildBreakToken? BreakToken { get; }
}

/// <summary>
/// WebIDL interface LayoutWorkletGlobalScope。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#layoutworkletglobalscope">CSS Layout API Level 1: 3 Layout Worklet</see>
/// </remarks>
[ECMAScript]
[Description("@#LayoutWorkletGlobalScope")]
public class LayoutWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// JavaScript LayoutWorkletGlobalScope.registerLayout(name, layoutCtor) 的强类型绑定，WebIDL 返回类型为 undefined。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutworkletglobalscope-registerlayout">CSS Layout API Level 1: 3.2 Registering A Layout</see>
    /// </remarks>
    /// <param name="name"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutworkletglobalscope-registerlayout-name-layoutctor-name">CSS Layout API Level 1: 3 Layout Worklet</see></param>
    /// <param name="layoutCtor"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutworkletglobalscope-registerlayout-name-layoutctor-layoutctor">CSS Layout API Level 1: 3 Layout Worklet</see></param>
    [Description("@#registerLayout")]
    public extern void RegisterLayout(string name, Action layoutCtor);
}

/// <summary>
/// The PaintRenderingContext2D interface of the CSS Painting API is the API&apos;s rendering context for drawing to the bitmap. It implements a subset of the CanvasRenderingContext2D API, with the following exceptions: It doesn&apos;t implement the CanvasImageData pixel manipulation, CanvasUserInterface focus, CanvasText text drawing, or CanvasTextDrawingStyles text style interface methods. The output bitmap is the size of the object it is rendering to. The value currentColor, when used as a color, is treated as opaque black. The interface is only available in PaintWorkletGlobalScope.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/PaintRenderingContext2D">MDN Web Docs: PaintRenderingContext2D</see>
/// </remarks>
[ECMAScript]
[Description("@#PaintRenderingContext2D")]
public class PaintRenderingContext2D
{
}

/// <summary>
/// The PaintSize interface of the CSS Painting API represents the size of the output bitmap that the author should draw.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/PaintSize">MDN Web Docs: PaintSize</see>
/// </remarks>
[ECMAScript]
[Description("@#PaintSize")]
public class PaintSize
{
    /// <summary>
    /// The width read-only property of the PaintSize interface returns the width of the output bitmap that the author should draw.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/PaintSize/width">MDN Web Docs: PaintSize.width</see>
    /// </remarks>
    [Description("@#width")]
    public extern double Width { get; }

    /// <summary>
    /// The height read-only property of the PaintSize interface returns the height of the output bitmap that the author should draw.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/PaintSize/height">MDN Web Docs: PaintSize.height</see>
    /// </remarks>
    [Description("@#height")]
    public extern double Height { get; }
}

/// <summary>
/// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. The PaintWorkletGlobalScope interface of the CSS Painting API represents the global object available inside a paint Worklet.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/PaintWorkletGlobalScope">MDN Web Docs: PaintWorkletGlobalScope</see>
/// </remarks>
[ECMAScript]
[Description("@#PaintWorkletGlobalScope")]
public class PaintWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. The registerPaint() method of the PaintWorkletGlobalScope interface registers a class to programmatically generate an image where a CSS property expects a file.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/PaintWorkletGlobalScope/registerPaint">MDN Web Docs: PaintWorkletGlobalScope.registerPaint</see>
    /// </remarks>
    /// <param name="name">The name of the worklet class to register. <see href="https://developer.mozilla.org/en-US/docs/Web/API/PaintWorkletGlobalScope/registerPaint">MDN Web Docs: name</see></param>
    /// <param name="paintCtor"><see href="https://drafts.css-houdini.org/css-paint-api-1/#dom-paintworkletglobalscope-registerpaint-name-paintctor-paintctor">CSS Painting API Level 1: 2 Paint Worklet</see></param>
    [Description("@#registerPaint")]
    public extern void RegisterPaint(string name, Action paintCtor);

    /// <summary>
    /// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. The devicePixelRatio read-only property of the PaintWorkletGlobalScope interface returns the current device&apos;s ratio of physical pixels to logical pixels.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/PaintWorkletGlobalScope/devicePixelRatio">MDN Web Docs: PaintWorkletGlobalScope.devicePixelRatio</see>
    /// </remarks>
    [Description("@#devicePixelRatio")]
    public extern double DevicePixelRatio { get; }
}

/// <summary>
/// The CSSPropertyRule interface of the CSS Properties and Values API represents a single CSS @property rule.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPropertyRule">MDN Web Docs: CSSPropertyRule</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSPropertyRule")]
public class CSSPropertyRule : CSSRule
{
    /// <summary>
    /// The read-only name property of the CSSPropertyRule interface represents the property name, this being the serialization of the name given to the custom property in the @property rule&apos;s prelude.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPropertyRule/name">MDN Web Docs: CSSPropertyRule.name</see>
    /// </remarks>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// The read-only syntax property of the CSSPropertyRule interface returns the literal syntax of the custom property registration represented by the @property rule, controlling how the property&apos;s value is parsed at computed-value time.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPropertyRule/syntax">MDN Web Docs: CSSPropertyRule.syntax</see>
    /// </remarks>
    [Description("@#syntax")]
    public extern string Syntax { get; }

    /// <summary>
    /// The read-only inherits property of the CSSPropertyRule interface returns the inherit flag of the custom property registration represented by the @property rule, a boolean describing whether or not the property inherits by default.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPropertyRule/inherits">MDN Web Docs: CSSPropertyRule.inherits</see>
    /// </remarks>
    [Description("@#inherits")]
    public extern bool Inherits { get; }

    /// <summary>
    /// The read-only initialValue nullable property of the CSSPropertyRule interface returns the initial value of the custom property registration represented by the @property rule, controlling the property&apos;s initial value.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPropertyRule/initialValue">MDN Web Docs: CSSPropertyRule.initialValue</see>
    /// </remarks>
    [Description("@#initialValue")]
    public extern string? InitialValue { get; }
}

/// <summary>
/// WebIDL interface CSSColor。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csscolor">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSColor")]
public class CSSColor : CSSColorValue
{
    /// <summary>
    /// 构造浏览器提供的 CSSColor 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-csscolor">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    /// <param name="colorSpace"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-csscolor-colorspace-channels-alpha-colorspace">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="channels"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-csscolor-colorspace-channels-alpha-channels">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-csscolor-colorspace-channels-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSColor(CSSKeywordish colorSpace, CSSColorPercent[] channels, CSSNumberish? alpha = default);

    /// <summary>
    /// JavaScript 属性 CSSColor.colorSpace：CSSKeywordish。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-colorspace">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#colorSpace")]
    public extern CSSKeywordish ColorSpace { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSColor.channels：ObservableArray&lt;CSSColorPercent&gt;。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-channels">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#channels")]
    public extern ObservableCollection<CSSColorPercent> Channels { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSColor.alpha：CSSNumberish。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolor-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#alpha")]
    public extern CSSNumberish Alpha { get; set; }
}

/// <summary>
/// WebIDL interface CSSColorValue。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csscolorvalue">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSColorValue")]
public class CSSColorValue : CSSStyleValue
{
    /// <summary>
    /// JavaScript CSSColorValue.parse(cssText) 的强类型绑定，WebIDL 返回类型为 CSSColorValue, CSSStyleValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolorvalue-parse">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    /// <param name="cssText"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csscolorvalue-parse-csstext-csstext">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    [Description("@#parse")]
    public static extern CSSColorValueParseResult Parse(string cssText);
}

/// <summary>
/// WebIDL interface CSSHSL。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csshsl">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSHSL")]
public class CSSHSL : CSSColorValue
{
    /// <summary>
    /// 构造浏览器提供的 CSSHSL 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-csshsl">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    /// <param name="h"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-csshsl-h-s-l-alpha-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="s"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-csshsl-h-s-l-alpha-s">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="l"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-csshsl-h-s-l-alpha-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-csshsl-h-s-l-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSHSL(CSSColorAngle h, CSSColorPercent s, CSSColorPercent l, CSSColorPercent? alpha = default);

    /// <summary>
    /// JavaScript 属性 CSSHSL.h：CSSColorAngle。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#h")]
    public extern CSSColorAngle H { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSHSL.s：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-s">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#s")]
    public extern CSSColorPercent S { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSHSL.l：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSHSL.alpha：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshsl-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// WebIDL interface CSSHWB。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csshwb">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSHWB")]
public class CSSHWB : CSSColorValue
{
    /// <summary>
    /// 构造浏览器提供的 CSSHWB 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-csshwb">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    /// <param name="h"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-csshwb-h-w-b-alpha-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="w"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-csshwb-h-w-b-alpha-w">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="b"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-csshwb-h-w-b-alpha-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-csshwb-h-w-b-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSHWB(CSSNumericValue h, CSSNumberish w, CSSNumberish b, CSSNumberish? alpha = default);

    /// <summary>
    /// JavaScript 属性 CSSHWB.h：CSSNumericValue。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#h")]
    public extern CSSNumericValue H { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSHWB.w：CSSNumberish。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-w">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#w")]
    public extern CSSNumberish W { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSHWB.b：CSSNumberish。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#b")]
    public extern CSSNumberish B { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSHWB.alpha：CSSNumberish。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csshwb-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#alpha")]
    public extern CSSNumberish Alpha { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSImageValue interface of the CSS Typed Object Model API represents values for CSS properties that take an &lt;image&gt; value, such as background-image, list-style-image, or border-image-source.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSImageValue">MDN Web Docs: CSSImageValue</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSImageValue")]
public class CSSImageValue : CSSStyleValue
{
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSKeywordValue interface of the CSS Typed Object Model API represents the value of a CSS keyword or other identifier. The interface instance name is a stringifier, so when used anywhere a string is expected it will return the value of CSSKeyword.value.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSKeywordValue">MDN Web Docs: CSSKeywordValue</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSKeywordValue")]
public class CSSKeywordValue : CSSStyleValue
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSKeywordValue() constructor creates a new CSSKeywordValue object which represents a CSS keyword or other identifier.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSKeywordValue/CSSKeywordValue">MDN Web Docs: CSSKeywordValue.CSSKeywordValue</see>
    /// </remarks>
    /// <param name="value">A String that will be used to set CSSKeywordValue.value. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSKeywordValue/CSSKeywordValue">MDN Web Docs: value</see></param>
    public extern CSSKeywordValue(string value);

    /// <summary>
    /// Note: This feature is available in Web Workers. The value property of the CSSKeywordValue interface represents the keyword as a string.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSKeywordValue/value">MDN Web Docs: CSSKeywordValue.value</see>
    /// </remarks>
    [Description("@#value")]
    public extern string Value { get; set; }
}

/// <summary>
/// WebIDL interface CSSLab。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csslab">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSLab")]
public class CSSLab : CSSColorValue
{
    /// <summary>
    /// 构造浏览器提供的 CSSLab 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-csslab">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    /// <param name="l"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-csslab-l-a-b-alpha-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="a"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-csslab-l-a-b-alpha-a">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="b"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-csslab-l-a-b-alpha-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-csslab-l-a-b-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSLab(CSSColorPercent l, CSSColorNumber a, CSSColorNumber b, CSSColorPercent? alpha = default);

    /// <summary>
    /// JavaScript 属性 CSSLab.l：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSLab.a：CSSColorNumber。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-a">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#a")]
    public extern CSSColorNumber A { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSLab.b：CSSColorNumber。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#b")]
    public extern CSSColorNumber B { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSLab.alpha：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslab-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// WebIDL interface CSSLCH。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#csslch">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSLCH")]
public class CSSLCH : CSSColorValue
{
    /// <summary>
    /// 构造浏览器提供的 CSSLCH 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-csslch">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    /// <param name="l"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-csslch-l-c-h-alpha-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="c"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-csslch-l-c-h-alpha-c">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="h"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-csslch-l-c-h-alpha-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-csslch-l-c-h-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSLCH(CSSColorPercent l, CSSColorPercent c, CSSColorAngle h, CSSColorPercent? alpha = default);

    /// <summary>
    /// JavaScript 属性 CSSLCH.l：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSLCH.c：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-c">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#c")]
    public extern CSSColorPercent C { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSLCH.h：CSSColorAngle。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#h")]
    public extern CSSColorAngle H { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSLCH.alpha：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-csslch-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSMathClamp interface of the CSS Typed Object Model API represents the CSS clamp() function.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathClamp">MDN Web Docs: CSSMathClamp</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSMathClamp")]
public class CSSMathClamp : CSSMathValue
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSMathClamp() constructor creates a new CSSMathClamp object representing a CSS clamp() function.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathClamp/CSSMathClamp">MDN Web Docs: CSSMathClamp.CSSMathClamp</see>
    /// </remarks>
    /// <param name="lower">A number or CSSNumericValue that represents the minimum value. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathClamp/CSSMathClamp">MDN Web Docs: lower</see></param>
    /// <param name="value">A number or CSSNumericValue that represents the preferred value. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathClamp/CSSMathClamp">MDN Web Docs: value</see></param>
    /// <param name="upper">A number or CSSNumericValue that represents the maximum value. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathClamp/CSSMathClamp">MDN Web Docs: upper</see></param>
    public extern CSSMathClamp(CSSNumberish lower, CSSNumberish value, CSSNumberish upper);

    /// <summary>
    /// Note: This feature is available in Web Workers. The lower read-only property of the CSSMathClamp interface returns a CSSNumericValue object representing its minimum value.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathClamp/lower">MDN Web Docs: CSSMathClamp.lower</see>
    /// </remarks>
    [Description("@#lower")]
    public extern CSSNumericValue Lower { get; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The value read-only property of the CSSMathClamp interface returns a CSSNumericValue instance representing its preferred value.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathClamp/value">MDN Web Docs: CSSMathClamp.value</see>
    /// </remarks>
    [Description("@#value")]
    public extern CSSNumericValue Value { get; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The upper read-only property of the CSSMathClamp interface returns a CSSNumericValue object representing its maximum value.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathClamp/upper">MDN Web Docs: CSSMathClamp.upper</see>
    /// </remarks>
    [Description("@#upper")]
    public extern CSSNumericValue Upper { get; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSMathInvert interface of the CSS Typed Object Model API represents the inverse (reciprocal) of a CSSNumericValue.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathInvert">MDN Web Docs: CSSMathInvert</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSMathInvert")]
public class CSSMathInvert : CSSMathValue
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSMathInvert() constructor creates a new CSSMathInvert object which represents the inverse (reciprocal) of a CSSNumericValue.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathInvert/CSSMathInvert">MDN Web Docs: CSSMathInvert.CSSMathInvert</see>
    /// </remarks>
    /// <param name="arg">A number or CSSNumericValue that represents the value to invert. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathInvert/CSSMathInvert">MDN Web Docs: arg</see></param>
    public extern CSSMathInvert(CSSNumberish arg);

    /// <summary>
    /// Note: This feature is available in Web Workers. The value read-only property of the CSSMathInvert interface returns the CSSNumericValue that is being inverted. This is the parameter that was passed to the constructor when this object was created.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathInvert/value">MDN Web Docs: CSSMathInvert.value</see>
    /// </remarks>
    [Description("@#value")]
    public extern CSSNumericValue Value { get; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSMathMax interface of the CSS Typed Object Model API represents the CSS max() function.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathMax">MDN Web Docs: CSSMathMax</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSMathMax")]
public class CSSMathMax : CSSMathValue
{
    /// <summary>
    /// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. Note: This feature is available in Web Workers. The CSSMathMax() constructor creates a new CSSMathMax object which represents the CSS max() function.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathMax/CSSMathMax">MDN Web Docs: CSSMathMax.CSSMathMax</see>
    /// </remarks>
    /// <param name="args"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathmax-cssmathmax-args-args">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    public extern CSSMathMax(params CSSNumberish[] args);

    /// <summary>
    /// Note: This feature is available in Web Workers. The values read-only property of the CSSMathMax interface returns a CSSNumericArray containing the CSSNumericValue objects being compared to find the maximum.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathMax/values">MDN Web Docs: CSSMathMax.values</see>
    /// </remarks>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSMathMin interface of the CSS Typed Object Model API represents the CSS min() function.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathMin">MDN Web Docs: CSSMathMin</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSMathMin")]
public class CSSMathMin : CSSMathValue
{
    /// <summary>
    /// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. Note: This feature is available in Web Workers. The CSSMathMin() constructor creates a new CSSMathMin object that represents the CSS min() function.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathMin/CSSMathMin">MDN Web Docs: CSSMathMin.CSSMathMin</see>
    /// </remarks>
    /// <param name="args"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathmin-cssmathmin-args-args">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    public extern CSSMathMin(params CSSNumberish[] args);

    /// <summary>
    /// Note: This feature is available in Web Workers. The values read-only property of the CSSMathMin interface returns a CSSNumericArray containing the CSSNumericValue objects being compared to find the minimum.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathMin/values">MDN Web Docs: CSSMathMin.values</see>
    /// </remarks>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSMathNegate interface of the CSS Typed Object Model API represents the negation of a CSSNumericValue.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathNegate">MDN Web Docs: CSSMathNegate</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSMathNegate")]
public class CSSMathNegate : CSSMathValue
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSMathNegate() constructor creates a new CSSMathNegate object which negates the value passed into it.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathNegate/CSSMathNegate">MDN Web Docs: CSSMathNegate.CSSMathNegate</see>
    /// </remarks>
    /// <param name="arg">A number or CSSNumericValue that represents the value to negate. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathNegate/CSSMathNegate">MDN Web Docs: arg</see></param>
    public extern CSSMathNegate(CSSNumberish arg);

    /// <summary>
    /// Note: This feature is available in Web Workers. The value read-only property of the CSSMathNegate interface returns the CSSNumericValue that is being negated. This is the value passed to the constructor, rectified to a CSSNumericValue (if it isn&apos;t one already). If a plain number was passed to the constructor the value returned by this property is the passed value wrapped in a CSSUnitValue with unit: &quot;number&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathNegate/value">MDN Web Docs: CSSMathNegate.value</see>
    /// </remarks>
    [Description("@#value")]
    public extern CSSNumericValue Value { get; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSMathProduct interface of the CSS Typed Object Model API represents the product of two or more CSSNumericValue values — in cases where the result can&apos;t be represented as a single value.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathProduct">MDN Web Docs: CSSMathProduct</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSMathProduct")]
public class CSSMathProduct : CSSMathValue
{
    /// <summary>
    /// Note: This feature is available in Web Workers. Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. The CSSMathProduct() constructor creates a new CSSMathProduct object representing the product of the arguments passed into it. Numeric arguments are wrapped into CSSUnitValue objects with a unit of &quot;number&quot;. All arguments are stored as separate items in its values property.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathProduct/CSSMathProduct">MDN Web Docs: CSSMathProduct.CSSMathProduct</see>
    /// </remarks>
    /// <param name="args"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathproduct-cssmathproduct-args-args">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    public extern CSSMathProduct(params CSSNumberish[] args);

    /// <summary>
    /// Note: This feature is available in Web Workers. The values read-only property of the CSSMathProduct interface returns a CSSNumericArray containing the CSSNumericValue objects being multiplied together.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathProduct/values">MDN Web Docs: CSSMathProduct.values</see>
    /// </remarks>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSMathSum interface of the CSS Typed Object Model API represents the sum of two or more CSSNumericValue values — in cases where the result can&apos;t be represented as a single value.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathSum">MDN Web Docs: CSSMathSum</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSMathSum")]
public class CSSMathSum : CSSMathValue
{
    /// <summary>
    /// Note: This feature is available in Web Workers. Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. The CSSMathSum() constructor creates a new CSSMathSum object representing the sum of the arguments passed into it. Numeric arguments are wrapped into CSSUnitValue objects with a unit of &quot;number&quot;. All arguments are stored as separate items in its values property.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathSum/CSSMathSum">MDN Web Docs: CSSMathSum.CSSMathSum</see>
    /// </remarks>
    /// <param name="args"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathsum-cssmathsum-args-args">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see></param>
    public extern CSSMathSum(params CSSNumberish[] args);

    /// <summary>
    /// Note: This feature is available in Web Workers. The values read-only property of the CSSMathSum interface returns a CSSNumericArray containing the CSSNumericValue objects being summed together.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathSum/values">MDN Web Docs: CSSMathSum.values</see>
    /// </remarks>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSMathValue interface of the CSS Typed Object Model API is the base interface for objects representing complex numeric values produced by the CSS calc(), min(), max(), and clamp() functions. Note: CSSMathValue cannot be constructed directly. Instances are returned by the platform (for example via StylePropertyMapReadOnly.get()) as one of its subtypes, listed below.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathValue">MDN Web Docs: CSSMathValue</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSMathValue")]
public class CSSMathValue : CSSNumericValue
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The operator read-only property of the CSSMathValue interface returns the operator that the current subtype represents. For example, if the current CSSMathValue subtype is CSSMathSum, this property will return the string &quot;sum&quot;.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMathValue/operator">MDN Web Docs: CSSMathValue.operator</see>
    /// </remarks>
    [Description("@#operator")]
    public extern CSSMathOperator Operator { get; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSMatrixComponent interface of the CSS Typed Object Model API represents the matrix() and matrix3d() values of the individual transform property in CSS.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMatrixComponent">MDN Web Docs: CSSMatrixComponent</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSMatrixComponent")]
public class CSSMatrixComponent : CSSTransformComponent
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSMatrixComponent() constructor creates a new CSSMatrixComponent object representing the matrix() and matrix3d() values of the individual transform property in CSS.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMatrixComponent/CSSMatrixComponent">MDN Web Docs: CSSMatrixComponent.CSSMatrixComponent</see>
    /// </remarks>
    /// <param name="matrix">A 2d or 3d matrix. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMatrixComponent/CSSMatrixComponent">MDN Web Docs: matrix</see></param>
    /// <param name="options">An object with the following property: is2D A boolean indicating whether the constructed CSSMatrixComponent should be treated as a 2D matrix. If omitted, this defaults to the value of matrix&apos;s own is2D property. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMatrixComponent/CSSMatrixComponent">MDN Web Docs: options</see></param>
    public extern CSSMatrixComponent(DOMMatrixReadOnly matrix, CSSMatrixComponentOptions? options = default);

    /// <summary>
    /// Note: This feature is available in Web Workers. The matrix property of the CSSMatrixComponent interface represents a DOMMatrix object containing a 2D or 3D matrix. See the matrix() and matrix3d() pages for examples.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMatrixComponent/matrix">MDN Web Docs: CSSMatrixComponent.matrix</see>
    /// </remarks>
    [Description("@#matrix")]
    public extern DOMMatrix Matrix { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSNumericArray interface of the CSS Typed Object Model API represents an iterable of CSSNumericValue-based objects. An object of this type is used to represent the operands of a mathematical operation in the values property of CSSMathSum, CSSMathProduct, CSSMathMin, and CSSMathMax. The items can be accessed by index (array[0]), and as an iterable it can be used with a for...of loop or the spread syntax.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericArray">MDN Web Docs: CSSNumericArray</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSNumericArray")]
public class CSSNumericArray : IEnumerable<CSSNumericValue>
{
    /// <summary>
    /// 遍历 CSSNumericArray 中由宿主定义顺序的条目；迭代元素类型为 CSSNumericValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: CSSNumericArray.iterable</see>
    /// </remarks>
extern IEnumerator<CSSNumericValue> IEnumerable<CSSNumericValue>.GetEnumerator();
    /// <summary>
    /// 遍历 CSSNumericArray 中由宿主定义顺序的条目；迭代元素类型为 CSSNumericValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: CSSNumericArray.iterable</see>
    /// </remarks>
extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// Note: This feature is available in Web Workers. The length read-only property of the CSSNumericArray interface returns the number of items in the object.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericArray/length">MDN Web Docs: CSSNumericArray.length</see>
    /// </remarks>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// JavaScript CSSNumericArray.(index) 的强类型绑定，WebIDL 返回类型为 CSSNumericValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: CSSNumericArray.</see>
    /// </remarks>
    /// <param name="index">WebIDL 参数 index：unsigned long。调用时必须传入。 <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: index</see></param>
[Description("@#")]
    public extern CSSNumericValue this[uint index] { get; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSNumericValue interface of the CSS Typed Object Model API represents operations that all numeric values can perform.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue">MDN Web Docs: CSSNumericValue</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSNumericValue")]
public class CSSNumericValue : CSSStyleValue
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The add() method of the CSSNumericValue interface adds a supplied number to the CSSNumericValue.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/add">MDN Web Docs: CSSNumericValue.add</see>
    /// </remarks>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-add-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    /// <returns>A CSSMathSum, or a CSSUnitValue if this and every argument share the same unit.</returns>
    [Description("@#add")]
    public extern CSSNumericValue Add(params CSSNumberish[] values);

    /// <summary>
    /// Note: This feature is available in Web Workers. The sub() method of the CSSNumericValue interface subtracts a supplied number from the CSSNumericValue.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/sub">MDN Web Docs: CSSNumericValue.sub</see>
    /// </remarks>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-sub-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    /// <returns>A CSSMathSum, or a CSSUnitValue if this and every argument share the same unit.</returns>
    [Description("@#sub")]
    public extern CSSNumericValue Sub(params CSSNumberish[] values);

    /// <summary>
    /// Note: This feature is available in Web Workers. The mul() method of the CSSNumericValue interface multiplies the CSSNumericValue by the supplied values.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/mul">MDN Web Docs: CSSNumericValue.mul</see>
    /// </remarks>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-mul-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    /// <returns>A CSSMathProduct, or a CSSUnitValue if this and every argument are plain numbers, or all but one of them are.</returns>
    [Description("@#mul")]
    public extern CSSNumericValue Mul(params CSSNumberish[] values);

    /// <summary>
    /// Note: This feature is available in Web Workers. The div() method of the CSSNumericValue interface divides the CSSNumericValue by the supplied value.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/div">MDN Web Docs: CSSNumericValue.div</see>
    /// </remarks>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-div-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    /// <returns>A CSSMathProduct, or a CSSUnitValue if this and every argument are plain numbers, or all but one of them are.</returns>
    [Description("@#div")]
    public extern CSSNumericValue Div(params CSSNumberish[] values);

    /// <summary>
    /// Note: This feature is available in Web Workers. The min() method of the CSSNumericValue interface returns the lowest value from among those values passed. The passed values must be of the same type.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/min">MDN Web Docs: CSSNumericValue.min</see>
    /// </remarks>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-min-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    /// <returns>A CSSUnitValue.</returns>
    [Description("@#min")]
    public extern CSSNumericValue Min(params CSSNumberish[] values);

    /// <summary>
    /// Note: This feature is available in Web Workers. The max() method of the CSSNumericValue interface returns the highest value from among the values passed. The passed values must be of the same type.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/max">MDN Web Docs: CSSNumericValue.max</see>
    /// </remarks>
    /// <param name="values"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-max-values-values">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    /// <returns>A CSSUnitValue.</returns>
    [Description("@#max")]
    public extern CSSNumericValue Max(params CSSNumberish[] values);

    /// <summary>
    /// Note: This feature is available in Web Workers. The equals() method of the CSSNumericValue interface returns a boolean indicating whether the passed values are strictly equal. To return a value of true, all passed values must be of the same type and value and must be in the same order. This allows structural equality to be tested quickly.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/equals">MDN Web Docs: CSSNumericValue.equals</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-equals-value-value">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    /// <returns>A boolean value.</returns>
    [Description("@#equals")]
    public extern bool Equals(params CSSNumberish[] value);

    /// <summary>
    /// Note: This feature is available in Web Workers. The to() method of the CSSNumericValue interface converts a numeric value from one unit to another.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/to">MDN Web Docs: CSSNumericValue.to</see>
    /// </remarks>
    /// <param name="unit">The unit to which you want to convert. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/to">MDN Web Docs: unit</see></param>
    /// <returns>A CSSUnitValue.</returns>
    [Description("@#to")]
    public extern CSSUnitValue To(string unit);

    /// <summary>
    /// Note: This feature is available in Web Workers. The toSum() method of the CSSNumericValue interface converts the object&apos;s value to a CSSMathSum of CSSUnitValues using only the specified units, if possible. If called with no units, it simplifies the value into a minimal sum of CSSUnitValues instead.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/toSum">MDN Web Docs: CSSNumericValue.toSum</see>
    /// </remarks>
    /// <param name="units"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericvalue-tosum-units-units">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
    /// <returns>A CSSMathSum.</returns>
    [Description("@#toSum")]
    public extern CSSMathSum ToSum(params string[] units);

    /// <summary>
    /// Note: This feature is available in Web Workers. The type() method of the CSSNumericValue interface returns the type of CSSNumericValue, one of angle, flex, frequency, length, resolution, percent, percentHint, or time.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/type">MDN Web Docs: CSSNumericValue.type</see>
    /// </remarks>
    /// <returns>A CSSNumericType dictionary, which contains the following properties: length angle time frequency resolution flex percent percentHint For each property except percentHint, the value is an integer representing the power of that unit. For example, a numeric value of calc(1px * 1em) will return { length: 2 }. The percentHint property is a string that indicates the type of value that the percent is applied to. The string value is the same as the type properties: &quot;length&quot;, &quot;angle&quot;, &quot;time&quot;, &quot;frequency&quot;, &quot;resolution&quot;, &quot;flex&quot;, or &quot;percent&quot;. It indicates that the type actually holds a percentage, but that percentage will eventually resolve to the hinted base type, and so has been replaced with it in the type.</returns>
    [Description("@#type")]
    public extern CSSNumericType Type();

    /// <summary>
    /// The parse() static method of the CSSNumericValue interface converts a value string into an object whose members are value and the units. Note: This method cannot be called in Worker or Worklet contexts — parsing CSS text is restricted to the main thread. All other methods in the CSSNumericValue interface are available in workers and worklets.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/parse_static">MDN Web Docs: CSSNumericValue.parse</see>
    /// </remarks>
    /// <param name="cssText">a string containing numeric and unit parts. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSNumericValue/parse_static">MDN Web Docs: cssText</see></param>
    /// <returns>A CSSNumericValue.</returns>
    [Description("@#parse")]
    public static extern CSSNumericValue Parse(string cssText);
}

/// <summary>
/// WebIDL interface CSSOKLab。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssoklab">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSOKLab")]
public class CSSOKLab : CSSColorValue
{
    /// <summary>
    /// 构造浏览器提供的 CSSOKLab 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-cssoklab">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    /// <param name="l"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-cssoklab-l-a-b-alpha-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="a"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-cssoklab-l-a-b-alpha-a">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="b"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-cssoklab-l-a-b-alpha-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-cssoklab-l-a-b-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSOKLab(CSSColorPercent l, CSSColorNumber a, CSSColorNumber b, CSSColorPercent? alpha = default);

    /// <summary>
    /// JavaScript 属性 CSSOKLab.l：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSOKLab.a：CSSColorNumber。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-a">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#a")]
    public extern CSSColorNumber A { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSOKLab.b：CSSColorNumber。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#b")]
    public extern CSSColorNumber B { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSOKLab.alpha：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklab-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// WebIDL interface CSSOKLCH。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssoklch">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSOKLCH")]
public class CSSOKLCH : CSSColorValue
{
    /// <summary>
    /// 构造浏览器提供的 CSSOKLCH 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-cssoklch">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    /// <param name="l"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-cssoklch-l-c-h-alpha-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="c"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-cssoklch-l-c-h-alpha-c">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="h"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-cssoklch-l-c-h-alpha-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-cssoklch-l-c-h-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSOKLCH(CSSColorPercent l, CSSColorPercent c, CSSColorAngle h, CSSColorPercent? alpha = default);

    /// <summary>
    /// JavaScript 属性 CSSOKLCH.l：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-l">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSOKLCH.c：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-c">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#c")]
    public extern CSSColorPercent C { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSOKLCH.h：CSSColorAngle。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-h">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#h")]
    public extern CSSColorAngle H { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSOKLCH.alpha：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssoklch-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSPerspective interface of the CSS Typed Object Model API represents the perspective() value of the individual transform property in CSS.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPerspective">MDN Web Docs: CSSPerspective</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSPerspective")]
public class CSSPerspective : CSSTransformComponent
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSPerspective() constructor creates a new CSSPerspective object representing the perspective() value of the individual transform property in CSS.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPerspective/CSSPerspective">MDN Web Docs: CSSPerspective.CSSPerspective</see>
    /// </remarks>
    /// <param name="length">A value for the distance from z=0 of the CSSPerspective object to be constructed. This must be a &lt;length&gt;. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPerspective/CSSPerspective">MDN Web Docs: length</see></param>
    public extern CSSPerspective(CSSPerspectiveValue length);

    /// <summary>
    /// Note: This feature is available in Web Workers. The length property of the CSSPerspective interface represents the distance from z=0. It is used to apply a perspective transform to the element and its content. If the value is 0 or a negative number, no perspective transform is applied.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPerspective/length">MDN Web Docs: CSSPerspective.length</see>
    /// </remarks>
    [Description("@#length")]
    public extern CSSPerspectiveValue Length { get; set; }
}

/// <summary>
/// WebIDL interface CSSRGB。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#cssrgb">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSRGB")]
public class CSSRGB : CSSColorValue
{
    /// <summary>
    /// 构造浏览器提供的 CSSRGB 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-cssrgb">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    /// <param name="r"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-cssrgb-r-g-b-alpha-r">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="g"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-cssrgb-r-g-b-alpha-g">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="b"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-cssrgb-r-g-b-alpha-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    /// <param name="alpha"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-cssrgb-r-g-b-alpha-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see></param>
    public extern CSSRGB(CSSColorRGBComp r, CSSColorRGBComp g, CSSColorRGBComp b, CSSColorPercent? alpha = default);

    /// <summary>
    /// JavaScript 属性 CSSRGB.r：CSSColorRGBComp。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-r">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#r")]
    public extern CSSColorRGBComp R { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSRGB.g：CSSColorRGBComp。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-g">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#g")]
    public extern CSSColorRGBComp G { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSRGB.b：CSSColorRGBComp。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-b">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#b")]
    public extern CSSColorRGBComp B { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSRGB.alpha：CSSColorPercent。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssrgb-alpha">CSS Typed OM Level 1: 4.6 CSSColorValue objects</see>
    /// </remarks>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSRotate interface of the CSS Typed Object Model API represents the value of a rotation function in the transform property in CSS.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate">MDN Web Docs: CSSRotate</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSRotate")]
public class CSSRotate : CSSTransformComponent
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSRotate() constructor creates a new CSSRotate object representing the rotate() value of the individual transform property in CSS. This can be specified as either a 2D rotation by a particular angle, or as a 3D rotation by an angle around a particular axis.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate/CSSRotate">MDN Web Docs: CSSRotate.CSSRotate</see>
    /// </remarks>
    /// <param name="angle">A value for the angle of rotation of the CSSRotate object to be constructed. This must be a CSSNumericValue. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate/CSSRotate">MDN Web Docs: angle</see></param>
    public extern CSSRotate(CSSNumericValue angle);

    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSRotate() constructor creates a new CSSRotate object representing the rotate() value of the individual transform property in CSS. This can be specified as either a 2D rotation by a particular angle, or as a 3D rotation by an angle around a particular axis.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate/CSSRotate">MDN Web Docs: CSSRotate.CSSRotate</see>
    /// </remarks>
    /// <param name="x">A number or CSSNumericValue value indicating the x-coordinate of the rotation axis vector of the CSSRotate object to be constructed. Only used, and required, when constructing a 3D rotation; the 2-argument form implies a rotation axis of (0, 0, 1) (the z-axis). <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate/CSSRotate">MDN Web Docs: x</see></param>
    /// <param name="y">A number or CSSNumericValue value indicating the y-coordinate of the rotation axis vector of the CSSRotate object to be constructed. Only used, and required, when constructing a 3D rotation. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate/CSSRotate">MDN Web Docs: y</see></param>
    /// <param name="z">A number or CSSNumericValue value indicating the z-coordinate of the rotation axis vector of the CSSRotate object to be constructed. Only used, and required, when constructing a 3D rotation. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate/CSSRotate">MDN Web Docs: z</see></param>
    /// <param name="angle">A value for the angle of rotation of the CSSRotate object to be constructed. This must be a CSSNumericValue. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate/CSSRotate">MDN Web Docs: angle</see></param>
    public extern CSSRotate(CSSNumberish x, CSSNumberish y, CSSNumberish z, CSSNumericValue angle);

    /// <summary>
    /// Note: This feature is available in Web Workers. The x property of the CSSRotate interface represents the x-coordinate of the vector denoting the axis of rotation.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate/x">MDN Web Docs: CSSRotate.x</see>
    /// </remarks>
    [Description("@#x")]
    public extern CSSNumberish X { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The y property of the CSSRotate interface represents the y-coordinate of the vector denoting the axis of rotation.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate/y">MDN Web Docs: CSSRotate.y</see>
    /// </remarks>
    [Description("@#y")]
    public extern CSSNumberish Y { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The z property of the CSSRotate interface represents the z-coordinate of the vector denoting the axis of rotation.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate/z">MDN Web Docs: CSSRotate.z</see>
    /// </remarks>
    [Description("@#z")]
    public extern CSSNumberish Z { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The angle property of the CSSRotate interface represents the angle of rotation. A positive angle denotes a clockwise rotation, a negative angle a counter-clockwise one.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRotate/angle">MDN Web Docs: CSSRotate.angle</see>
    /// </remarks>
    [Description("@#angle")]
    public extern CSSNumericValue Angle { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSScale interface of the CSS Typed Object Model API represents the scale() and scale3d() values of the individual transform property in CSS.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSScale">MDN Web Docs: CSSScale</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSScale")]
public class CSSScale : CSSTransformComponent
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSScale() constructor creates a new CSSScale object representing the scale() and scale3d() values of the individual transform property in CSS.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSScale/CSSScale">MDN Web Docs: CSSScale.CSSScale</see>
    /// </remarks>
    /// <param name="x">A value for the x-axis of the CSSScale object to be constructed. This must either be a number (which is wrapped into a CSSUnitValue of unit: &quot;number&quot;) or a CSSNumericValue. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSScale/CSSScale">MDN Web Docs: x</see></param>
    /// <param name="y">A value for the y-axis of the CSSScale object to be constructed. This must either be a number (which is wrapped into a CSSUnitValue of unit: &quot;number&quot;) or a CSSNumericValue. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSScale/CSSScale">MDN Web Docs: y</see></param>
    /// <param name="z">A value for the z-axis of the CSSScale object to be constructed. This must either be a number (which is wrapped into a CSSUnitValue of unit: &quot;number&quot;) or a CSSNumericValue. If a value is passed, the value of is2D will be set to false. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSScale/CSSScale">MDN Web Docs: z</see></param>
    public extern CSSScale(CSSNumberish x, CSSNumberish y, CSSNumberish? z = default);

    /// <summary>
    /// Note: This feature is available in Web Workers. The x property of the CSSScale interface gets and sets the abscissa or x-axis of the translating vector.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSScale/x">MDN Web Docs: CSSScale.x</see>
    /// </remarks>
    [Description("@#x")]
    public extern CSSNumberish X { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The y property of the CSSScale interface gets and sets the ordinate or y-axis of the translating vector.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSScale/y">MDN Web Docs: CSSScale.y</see>
    /// </remarks>
    [Description("@#y")]
    public extern CSSNumberish Y { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The z property of the CSSScale interface represents the z-component of the translating vector. A positive value moves the element towards the viewer, and a negative value farther away. If this value is present, then the transform is a 3D transform and the is2D property will be set to false.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSScale/z">MDN Web Docs: CSSScale.z</see>
    /// </remarks>
    [Description("@#z")]
    public extern CSSNumberish Z { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSSkew interface of the CSS Typed Object Model API represents the skew() value of the individual transform property in CSS.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkew">MDN Web Docs: CSSSkew</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSSkew")]
public class CSSSkew : CSSTransformComponent
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSSkew() constructor creates a new CSSSkew object which represents the skew() value of the individual transform property in CSS.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkew/CSSSkew">MDN Web Docs: CSSSkew.CSSSkew</see>
    /// </remarks>
    /// <param name="ax">A value for the ax (x-axis) angle of the CSSSkew object to be constructed. This must be a CSSNumericValue. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkew/CSSSkew">MDN Web Docs: ax</see></param>
    /// <param name="ay">A value for the ay (y-axis) angle of the CSSSkew object to be constructed. This must be a CSSNumericValue. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkew/CSSSkew">MDN Web Docs: ay</see></param>
    public extern CSSSkew(CSSNumericValue ax, CSSNumericValue ay);

    /// <summary>
    /// Note: This feature is available in Web Workers. The ax property of the CSSSkew interface gets and sets the angle used to distort the element along the x-axis (or abscissa).
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkew/ax">MDN Web Docs: CSSSkew.ax</see>
    /// </remarks>
    [Description("@#ax")]
    public extern CSSNumericValue Ax { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The ay property of the CSSSkew interface gets and sets the angle used to distort the element along the y-axis (or ordinate).
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkew/ay">MDN Web Docs: CSSSkew.ay</see>
    /// </remarks>
    [Description("@#ay")]
    public extern CSSNumericValue Ay { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSSkewX interface of the CSS Typed Object Model API represents the skewX() value of the individual transform property in CSS.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkewX">MDN Web Docs: CSSSkewX</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSSkewX")]
public class CSSSkewX : CSSTransformComponent
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSSkewX() constructor creates a new CSSSkewX object that represents the skewX() value of the individual transform property in CSS.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkewX/CSSSkewX">MDN Web Docs: CSSSkewX.CSSSkewX</see>
    /// </remarks>
    /// <param name="ax">A value for the ax angle of the CSSSkewX object to be constructed. This must be a CSSNumericValue. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkewX/CSSSkewX">MDN Web Docs: ax</see></param>
    public extern CSSSkewX(CSSNumericValue ax);

    /// <summary>
    /// Note: This feature is available in Web Workers. The ax property of the CSSSkewX interface gets and sets the angle used to distort the element along the x-axis (or abscissa).
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkewX/ax">MDN Web Docs: CSSSkewX.ax</see>
    /// </remarks>
    [Description("@#ax")]
    public extern CSSNumericValue Ax { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSSkewY interface of the CSS Typed Object Model API represents the skewY() value of the individual transform property in CSS.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkewY">MDN Web Docs: CSSSkewY</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSSkewY")]
public class CSSSkewY : CSSTransformComponent
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSSkewY() constructor creates a new CSSSkewY object that represents the skewY() value of the individual transform property in CSS.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkewY/CSSSkewY">MDN Web Docs: CSSSkewY.CSSSkewY</see>
    /// </remarks>
    /// <param name="ay">A value for the ay angle of the CSSSkewY object to be constructed. This must be a CSSNumericValue. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkewY/CSSSkewY">MDN Web Docs: ay</see></param>
    public extern CSSSkewY(CSSNumericValue ay);

    /// <summary>
    /// Note: This feature is available in Web Workers. The ay property of the CSSSkewY interface gets and sets the angle used to distort the element along the y-axis (or ordinate).
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSSkewY/ay">MDN Web Docs: CSSSkewY.ay</see>
    /// </remarks>
    [Description("@#ay")]
    public extern CSSNumericValue Ay { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSStyleValue interface of the CSS Typed Object Model API is the base class of all CSS values accessible through the Typed OM API. An instance of this class may be used anywhere a string is expected.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleValue">MDN Web Docs: CSSStyleValue</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSStyleValue")]
public class CSSStyleValue
{
    /// <summary>
    /// The parse() static method of the CSSStyleValue interface sets a specific CSS property to the specified values and returns the first value as a CSSStyleValue object. Note: This method cannot be called in Worker or Worklet contexts. The rest of the CSSStyleValue interface remains available in workers and worklets.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleValue/parse_static">MDN Web Docs: CSSStyleValue.parse</see>
    /// </remarks>
    /// <param name="property">A CSS property to set. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleValue/parse_static">MDN Web Docs: property</see></param>
    /// <param name="cssText">A comma-separated string containing one or more values to apply to the provided property. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleValue/parse_static">MDN Web Docs: cssText</see></param>
    /// <returns>A CSSStyleValue object containing the first supplied value.</returns>
    [Description("@#parse")]
    public static extern CSSStyleValue Parse(string property, string cssText);

    /// <summary>
    /// The parseAll() static method of the CSSStyleValue interface sets all occurrences of a specific CSS property to the specified value and returns an array of CSSStyleValue objects, each containing one of the supplied values. Note: This method cannot be called in Worker or Worklet contexts. The rest of the CSSStyleValue interface remains available in workers and worklets.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleValue/parseAll_static">MDN Web Docs: CSSStyleValue.parseAll</see>
    /// </remarks>
    /// <param name="property">A CSS property to set. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleValue/parseAll_static">MDN Web Docs: property</see></param>
    /// <param name="cssText"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssstylevalue-parseall-property-csstext-csstext">CSS Typed OM Level 1: 2 CSSStyleValue objects</see></param>
    /// <returns>An array of CSSStyleValue objects, each containing one of the supplied values.</returns>
    [Description("@#parseAll")]
    public static extern CSSStyleValue[] ParseAll(string property, string cssText);
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSTransformComponent interface of the CSS Typed Object Model API is the base interface for objects that represent individual transform functions, such as rotate() and scale().
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTransformComponent">MDN Web Docs: CSSTransformComponent</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSTransformComponent")]
public class CSSTransformComponent
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The is2D property of the CSSTransformComponent interface represents whether the transform is 2D or 3D.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTransformComponent/is2D">MDN Web Docs: CSSTransformComponent.is2D</see>
    /// </remarks>
    [Description("@#is2D")]
    public extern bool Is2D { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The toMatrix() method of the CSSTransformComponent interface returns a DOMMatrix object. All transform functions can be represented mathematically as a 4x4 transformation matrix. Note: The is2D property affects what transform, and therefore type of matrix that will be returned. CSS 2D and 3D transforms are different for legacy reasons. A brief explanation of 2D vs. 3D transforms can be found in Using CSS transforms.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTransformComponent/toMatrix">MDN Web Docs: CSSTransformComponent.toMatrix</see>
    /// </remarks>
    /// <returns>A DOMMatrix object.</returns>
    [Description("@#toMatrix")]
    public extern DOMMatrix ToMatrix();
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSTransformValue interface of the CSS Typed Object Model API represents transform-list values as used by the CSS transform property. It is an iterable of CSSTransformComponent objects, each representing a single &lt;transform-function&gt;. The items can be accessed and set by index (transformValue[0]), and as an iterable it can be used with a for...of loop or the spread syntax.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTransformValue">MDN Web Docs: CSSTransformValue</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSTransformValue")]
public class CSSTransformValue : CSSStyleValue, IEnumerable<CSSTransformComponent>
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSTransformValue() constructor creates a new CSSTransformValue object, representing a transform-list value made up of the given CSSTransformComponent objects.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTransformValue/CSSTransformValue">MDN Web Docs: CSSTransformValue.CSSTransformValue</see>
    /// </remarks>
    /// <param name="transforms">An array of CSSTransformComponent objects. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTransformValue/CSSTransformValue">MDN Web Docs: transforms</see></param>
    public extern CSSTransformValue(CSSTransformComponent[] transforms);

    /// <summary>
    /// 遍历 CSSTransformValue 中由宿主定义顺序的条目；迭代元素类型为 CSSTransformComponent。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: CSSTransformValue.iterable</see>
    /// </remarks>
extern IEnumerator<CSSTransformComponent> IEnumerable<CSSTransformComponent>.GetEnumerator();
    /// <summary>
    /// 遍历 CSSTransformValue 中由宿主定义顺序的条目；迭代元素类型为 CSSTransformComponent。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: CSSTransformValue.iterable</see>
    /// </remarks>
extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// Note: This feature is available in Web Workers. The length read-only property of the CSSTransformValue interface returns the number of items in the object.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTransformValue/length">MDN Web Docs: CSSTransformValue.length</see>
    /// </remarks>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// JavaScript CSSTransformValue.(index) 的强类型绑定，WebIDL 返回类型为 CSSTransformComponent。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: CSSTransformValue.</see>
    /// </remarks>
    /// <param name="index">WebIDL 参数 index：unsigned long。调用时必须传入。 <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: index</see></param>
[Description("@#")]
    public extern CSSTransformComponent this[uint index] { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The is2D read-only property of the CSSTransformValue interface returns whether the transform is 2D or 3D. is2D is true only if every CSSTransformComponent in the CSSTransformValue is itself 2D (see CSSTransformComponent.is2D), otherwise it is false.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTransformValue/is2D">MDN Web Docs: CSSTransformValue.is2D</see>
    /// </remarks>
    [Description("@#is2D")]
    public extern bool Is2D { get; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The toMatrix() method of the CSSTransformValue interface returns a DOMMatrix object. The returned matrix is the product of the matrices of each CSSTransformComponent in the CSSTransformValue, computed by calling CSSTransformComponent.toMatrix() on each component in turn and multiplying the results together, in order.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTransformValue/toMatrix">MDN Web Docs: CSSTransformValue.toMatrix</see>
    /// </remarks>
    /// <returns>A DOMMatrix object.</returns>
    [Description("@#toMatrix")]
    public extern DOMMatrix ToMatrix();
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSTranslate interface of the CSS Typed Object Model API represents the translate() value of the individual transform property in CSS.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTranslate">MDN Web Docs: CSSTranslate</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSTranslate")]
public class CSSTranslate : CSSTransformComponent
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSTranslate() constructor creates a new CSSTranslate object representing the translate() value of the individual transform property in CSS.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTranslate/CSSTranslate">MDN Web Docs: CSSTranslate.CSSTranslate</see>
    /// </remarks>
    /// <param name="x">A value for the x-axis of the CSSTranslate object to be constructed. This must be a &lt;length-percentage&gt;. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTranslate/CSSTranslate">MDN Web Docs: x</see></param>
    /// <param name="y">A value for the y-axis of the CSSTranslate object to be constructed. This must be a &lt;length-percentage&gt;. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTranslate/CSSTranslate">MDN Web Docs: y</see></param>
    /// <param name="z">A value for the z-axis of the CSSTranslate object to be constructed. This must be a &lt;length&gt;. If a value is passed for the z-axis this is a 3D transform. The value of is2D will be set to false. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTranslate/CSSTranslate">MDN Web Docs: z</see></param>
    public extern CSSTranslate(CSSNumericValue x, CSSNumericValue y, CSSNumericValue? z = default);

    /// <summary>
    /// Note: This feature is available in Web Workers. The x property of the CSSTranslate interface gets and sets the abscissa or x-axis of the translating vector.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTranslate/x">MDN Web Docs: CSSTranslate.x</see>
    /// </remarks>
    [Description("@#x")]
    public extern CSSNumericValue X { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The y property of the CSSTranslate interface gets and sets the ordinate or y-axis of the translating vector.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTranslate/y">MDN Web Docs: CSSTranslate.y</see>
    /// </remarks>
    [Description("@#y")]
    public extern CSSNumericValue Y { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The z property of the CSSTranslate interface represents the z-component of the translating vector. A positive value moves the element towards the viewer, and a negative value farther away. If this value is present then the transform is a 3D transform and the is2D property will be set to false.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSTranslate/z">MDN Web Docs: CSSTranslate.z</see>
    /// </remarks>
    [Description("@#z")]
    public extern CSSNumericValue Z { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSUnitValue interface of the CSS Typed Object Model API represents values that contain a single unit type. For example, the value 42px (a &lt;dimension&gt;) would be represented by a CSSNumericValue.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSUnitValue">MDN Web Docs: CSSUnitValue</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSUnitValue")]
public class CSSUnitValue : CSSNumericValue
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSUnitValue() constructor creates a new CSSUnitValue object which returns a new CSSUnitValue object which represents values that contain a single unit type. For example, &quot;42px&quot; would be represented by a CSSNumericValue.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSUnitValue/CSSUnitValue">MDN Web Docs: CSSUnitValue.CSSUnitValue</see>
    /// </remarks>
    /// <param name="value">A number indicating the number of units. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSUnitValue/CSSUnitValue">MDN Web Docs: value</see></param>
    /// <param name="unit">A string indicating the type of unit. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSUnitValue/CSSUnitValue">MDN Web Docs: unit</see></param>
    public extern CSSUnitValue(double value, string unit);

    /// <summary>
    /// Note: This feature is available in Web Workers. The value property of the CSSUnitValue interface represents the number of units.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSUnitValue/value">MDN Web Docs: CSSUnitValue.value</see>
    /// </remarks>
    [Description("@#value")]
    public extern double Value { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The unit read-only property of the CSSUnitValue interface returns a string indicating the unit type.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSUnitValue/unit">MDN Web Docs: CSSUnitValue.unit</see>
    /// </remarks>
    [Description("@#unit")]
    public extern string Unit { get; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSUnparsedValue interface of the CSS Typed Object Model API represents a property value that can&apos;t be parsed into a more specific type — typically the value of a custom property. The object is an iterable that may contain string fragments and variable references. The items can be accessed and set by index (unparsedValue[0]), and as an iterable it can be used with a for...of loop or the spread syntax.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSUnparsedValue">MDN Web Docs: CSSUnparsedValue</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSUnparsedValue")]
public class CSSUnparsedValue : CSSStyleValue, IEnumerable<CSSUnparsedSegment>
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The CSSUnparsedValue() constructor creates a new CSSUnparsedValue object, which represents a property value that can&apos;t be parsed into a more specific type — typically the value of a custom property.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSUnparsedValue/CSSUnparsedValue">MDN Web Docs: CSSUnparsedValue.CSSUnparsedValue</see>
    /// </remarks>
    /// <param name="members">An array whose values must be either a string or a CSSVariableReferenceValue. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSUnparsedValue/CSSUnparsedValue">MDN Web Docs: members</see></param>
    public extern CSSUnparsedValue(CSSUnparsedSegment[] members);

    /// <summary>
    /// 遍历 CSSUnparsedValue 中由宿主定义顺序的条目；迭代元素类型为 CSSUnparsedSegment。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: CSSUnparsedValue.iterable</see>
    /// </remarks>
extern IEnumerator<CSSUnparsedSegment> IEnumerable<CSSUnparsedSegment>.GetEnumerator();
    /// <summary>
    /// 遍历 CSSUnparsedValue 中由宿主定义顺序的条目；迭代元素类型为 CSSUnparsedSegment。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: CSSUnparsedValue.iterable</see>
    /// </remarks>
extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// Note: This feature is available in Web Workers. The length read-only property of the CSSUnparsedValue interface returns the number of items in the object.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSUnparsedValue/length">MDN Web Docs: CSSUnparsedValue.length</see>
    /// </remarks>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// JavaScript CSSUnparsedValue.(index) 的强类型绑定，WebIDL 返回类型为 CSSUnparsedSegment。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: CSSUnparsedValue.</see>
    /// </remarks>
    /// <param name="index">WebIDL 参数 index：unsigned long。调用时必须传入。 <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: index</see></param>
[Description("@#")]
    public extern CSSUnparsedSegment this[uint index] { get; set; }
}

/// <summary>
/// Note: This feature is available in Web Workers. The CSSVariableReferenceValue interface of the CSS Typed Object Model API allows you to create a custom name for a built-in CSS value. This object functionality is sometimes called a &quot;CSS variable&quot; and serves the same purpose as the var() function. The custom name must begin with two dashes.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSVariableReferenceValue">MDN Web Docs: CSSVariableReferenceValue</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSVariableReferenceValue")]
public class CSSVariableReferenceValue
{
    /// <summary>
    /// Note: This feature is available in Web Workers. Creates a new CSSVariableReferenceValue.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSVariableReferenceValue/CSSVariableReferenceValue">MDN Web Docs: CSSVariableReferenceValue.CSSVariableReferenceValue</see>
    /// </remarks>
    /// <param name="variable">A custom property name. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSVariableReferenceValue/CSSVariableReferenceValue">MDN Web Docs: variable</see></param>
    /// <param name="fallback">A custom property fallback value. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSVariableReferenceValue/CSSVariableReferenceValue">MDN Web Docs: fallback</see></param>
    public extern CSSVariableReferenceValue(string variable, CSSUnparsedValue? fallback = default);

    /// <summary>
    /// Note: This feature is available in Web Workers. The variable property of the CSSVariableReferenceValue interface returns the custom property name of the CSSVariableReferenceValue.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSVariableReferenceValue/variable">MDN Web Docs: CSSVariableReferenceValue.variable</see>
    /// </remarks>
    [Description("@#variable")]
    public extern string Variable { get; set; }

    /// <summary>
    /// Note: This feature is available in Web Workers. The fallback read-only property of the CSSVariableReferenceValue interface returns the custom property fallback value of the CSSVariableReferenceValue.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSVariableReferenceValue/fallback">MDN Web Docs: CSSVariableReferenceValue.fallback</see>
    /// </remarks>
    [Description("@#fallback")]
    public extern CSSUnparsedValue? Fallback { get; }
}

/// <summary>
/// The StylePropertyMap interface of the CSS Typed Object Model API provides a representation of a CSS declaration block that is an alternative to CSSStyleDeclaration. Note: This interface is only available on the window thread; unlike other interfaces in this API it cannot be accessed in Worker or Worklet contexts. Worklets receive a read-only snapshot of an element&apos;s style through StylePropertyMapReadOnly.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap">MDN Web Docs: StylePropertyMap</see>
/// </remarks>
[ECMAScript]
[Description("@#StylePropertyMap")]
public class StylePropertyMap : StylePropertyMapReadOnly
{
    /// <summary>
    /// The set() method of the StylePropertyMap interface changes the CSS declaration using the given property.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/set">MDN Web Docs: StylePropertyMap.set</see>
    /// </remarks>
    /// <param name="property">An identifier indicating the stylistic feature (e.g., font, width, background color) to change. <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/set">MDN Web Docs: property</see></param>
    /// <param name="values">WebIDL 参数 values：CSSStyleValue, USVString。调用时必须传入。 <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/set">MDN Web Docs: values</see></param>
    [Description("@#set")]
    public extern void Set(string property, params StylePropertyMapSetValues[] values);

    /// <summary>
    /// The set() method of the StylePropertyMap interface changes the CSS declaration using the given property.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/set">MDN Web Docs: StylePropertyMap.set</see>
    /// </remarks>
    /// <param name="property">An identifier indicating the stylistic feature (e.g., font, width, background color) to change. <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/set">MDN Web Docs: property</see></param>
    /// <param name="values">WebIDL 参数 values：CSSStyleValue, USVString。调用时必须传入。 <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/set">MDN Web Docs: values</see></param>
    [Description("@#set")]
    public extern void Set(string property, CSSStyleValue values);

    /// <summary>
    /// The set() method of the StylePropertyMap interface changes the CSS declaration using the given property.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/set">MDN Web Docs: StylePropertyMap.set</see>
    /// </remarks>
    /// <param name="property">An identifier indicating the stylistic feature (e.g., font, width, background color) to change. <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/set">MDN Web Docs: property</see></param>
    /// <param name="values">WebIDL 参数 values：CSSStyleValue, USVString。调用时必须传入。 <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/set">MDN Web Docs: values</see></param>
    [Description("@#set")]
    public extern void Set(string property, string values);

    /// <summary>
    /// The append() method of the StylePropertyMap interface adds one or more values to the end of a list-valued CSS property&apos;s value list. A list-valued CSS property is one whose value is a comma-separated list of terms, such as background-image or animation.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/append">MDN Web Docs: StylePropertyMap.append</see>
    /// </remarks>
    /// <param name="property">An identifier indicating the stylistic feature (e.g., font, width, background color) to add. <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/append">MDN Web Docs: property</see></param>
    /// <param name="values">WebIDL 参数 values：CSSStyleValue, USVString。调用时必须传入。 <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/append">MDN Web Docs: values</see></param>
    [Description("@#append")]
    public extern void Append(string property, params StylePropertyMapAppendValues[] values);

    /// <summary>
    /// The append() method of the StylePropertyMap interface adds one or more values to the end of a list-valued CSS property&apos;s value list. A list-valued CSS property is one whose value is a comma-separated list of terms, such as background-image or animation.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/append">MDN Web Docs: StylePropertyMap.append</see>
    /// </remarks>
    /// <param name="property">An identifier indicating the stylistic feature (e.g., font, width, background color) to add. <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/append">MDN Web Docs: property</see></param>
    /// <param name="values">WebIDL 参数 values：CSSStyleValue, USVString。调用时必须传入。 <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/append">MDN Web Docs: values</see></param>
    [Description("@#append")]
    public extern void Append(string property, CSSStyleValue values);

    /// <summary>
    /// The append() method of the StylePropertyMap interface adds one or more values to the end of a list-valued CSS property&apos;s value list. A list-valued CSS property is one whose value is a comma-separated list of terms, such as background-image or animation.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/append">MDN Web Docs: StylePropertyMap.append</see>
    /// </remarks>
    /// <param name="property">An identifier indicating the stylistic feature (e.g., font, width, background color) to add. <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/append">MDN Web Docs: property</see></param>
    /// <param name="values">WebIDL 参数 values：CSSStyleValue, USVString。调用时必须传入。 <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/append">MDN Web Docs: values</see></param>
    [Description("@#append")]
    public extern void Append(string property, string values);

    /// <summary>
    /// The delete() method of the StylePropertyMap interface removes the CSS declaration using the given property.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/delete">MDN Web Docs: StylePropertyMap.delete</see>
    /// </remarks>
    /// <param name="property">An identifier indicating the stylistic feature (e.g., font, width, background color) to remove. <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/delete">MDN Web Docs: property</see></param>
    [Description("@#delete")]
    public extern void Delete(string property);

    /// <summary>
    /// The clear() method of the StylePropertyMap interface removes all declarations in the StylePropertyMap.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMap/clear">MDN Web Docs: StylePropertyMap.clear</see>
    /// </remarks>
    [Description("@#clear")]
    public extern void Clear();
}

/// <summary>
/// Note: This feature is available in Web Workers. The StylePropertyMapReadOnly interface of the CSS Typed Object Model API provides a read-only representation of a CSS declaration block that is an alternative to CSSStyleDeclaration. Retrieve an instance of this interface using Element.computedStyleMap().
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMapReadOnly">MDN Web Docs: StylePropertyMapReadOnly</see>
/// </remarks>
[ECMAScript]
[Description("@#StylePropertyMapReadOnly")]
public class StylePropertyMapReadOnly : IEnumerable<(string, CSSStyleValue[])>
{
    /// <summary>
    /// 遍历 StylePropertyMapReadOnly 中由宿主定义顺序的条目；迭代元素类型为 USVString, CSSStyleValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: StylePropertyMapReadOnly.iterable</see>
    /// </remarks>
extern IEnumerator<(string, CSSStyleValue[])> IEnumerable<(string, CSSStyleValue[])>.GetEnumerator();
    /// <summary>
    /// 遍历 StylePropertyMapReadOnly 中由宿主定义顺序的条目；迭代元素类型为 USVString, CSSStyleValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/">CSS Typed OM Level 1: StylePropertyMapReadOnly.iterable</see>
    /// </remarks>
extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// Note: This feature is available in Web Workers. The get() method of the StylePropertyMapReadOnly interface returns a CSSStyleValue-derived object for the first value of the specified property. Use getAll() to get all the values of a CSS property that can have multiple values, such as background-image or transition.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMapReadOnly/get">MDN Web Docs: StylePropertyMapReadOnly.get</see>
    /// </remarks>
    /// <param name="property">The name of the property. Custom property names (starting with --) are matched case-sensitively; standard property names are not. <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMapReadOnly/get">MDN Web Docs: property</see></param>
    /// <returns>A CSSStyleValue-derived object, or undefined if property has no value in the map. The concrete type of the returned object depends on the property and its value. For example, a property assigned a keyword might return a CSSKeywordValue, while a property assigned the result of a mathematical operation might return a CSSMathSum.</returns>
    [Description("@#get")]
    public extern CSSStyleValue? Get(string property);

    /// <summary>
    /// Note: This feature is available in Web Workers. The getAll() method of the StylePropertyMapReadOnly interface returns an array of CSSStyleValue objects containing the values for the provided property.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMapReadOnly/getAll">MDN Web Docs: StylePropertyMapReadOnly.getAll</see>
    /// </remarks>
    /// <param name="property">The name of the property to retrieve all values of. <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMapReadOnly/getAll">MDN Web Docs: property</see></param>
    /// <returns>An array of CSSStyleValue objects.</returns>
    [Description("@#getAll")]
    public extern CSSStyleValue[] GetAll(string property);

    /// <summary>
    /// Note: This feature is available in Web Workers. The has() method of the StylePropertyMapReadOnly interface indicates whether the specified property is in the StylePropertyMapReadOnly object.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMapReadOnly/has">MDN Web Docs: StylePropertyMapReadOnly.has</see>
    /// </remarks>
    /// <param name="property">The name of a property. <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMapReadOnly/has">MDN Web Docs: property</see></param>
    /// <returns>A boolean value.</returns>
    [Description("@#has")]
    public extern bool Has(string property);

    /// <summary>
    /// Note: This feature is available in Web Workers. The size read-only property of the StylePropertyMapReadOnly interface returns a positive integer containing the size of the StylePropertyMapReadOnly object.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StylePropertyMapReadOnly/size">MDN Web Docs: StylePropertyMapReadOnly.size</see>
    /// </remarks>
    [Description("@#size")]
    public extern uint Size { get; }
}

/// <summary>
/// An object implementing the CSSConditionRule interface represents a single condition CSS at-rule, which consists of a condition and a statement block. Three objects derive from CSSConditionRule: CSSMediaRule, CSSContainerRule and CSSSupportsRule.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSConditionRule">MDN Web Docs: CSSConditionRule</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSConditionRule")]
public class CSSConditionRule : CSSGroupingRule
{
    /// <summary>
    /// The read-only conditionText property of the CSSConditionRule interface returns or sets the text of the CSS rule.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSConditionRule/conditionText">MDN Web Docs: CSSConditionRule.conditionText</see>
    /// </remarks>
    [Description("@#conditionText")]
    public extern string ConditionText { get; }
}

/// <summary>
/// The CSSMediaRule interface represents a single CSS @media rule.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMediaRule">MDN Web Docs: CSSMediaRule</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSMediaRule")]
public class CSSMediaRule : CSSConditionRule
{
    /// <summary>
    /// The read-only media property of the CSSMediaRule interface contains a MediaList object representing the media query list of the @media rule.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSMediaRule/media">MDN Web Docs: CSSMediaRule.media</see>
    /// </remarks>
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
/// The HighlightRegistry interface of the CSS Custom Highlight API is used to register Highlight objects to be styled using the API. It is accessed via CSS.highlights. A HighlightRegistry instance is a Map-like object, in which each key is the name string for a custom highlight, and the corresponding value is the associated Highlight object.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/HighlightRegistry">MDN Web Docs: HighlightRegistry</see>
/// </remarks>
[ECMAScript]
[Description("@#HighlightRegistry")]
public partial class HighlightRegistry : IDictionary<string, Highlight>
{
    #region Dictionary
    /// <summary>
    /// HighlightRegistry 的 WebIDL 映射集合接口；键和值类型为 DOMString, Highlight。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/">CSS Custom Highlight API Module Level 1: HighlightRegistry.maplike</see>
    /// </remarks>
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
    /// The highlightsFromPoint() method of the HighlightRegistry interface returns an array of objects representing the custom highlights applied at a specific point within the viewport.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/HighlightRegistry/highlightsFromPoint">MDN Web Docs: HighlightRegistry.highlightsFromPoint</see>
    /// </remarks>
    /// <param name="x">The x-coordinate of the point within the viewport from which to return custom highlight information. <see href="https://developer.mozilla.org/en-US/docs/Web/API/HighlightRegistry/highlightsFromPoint">MDN Web Docs: x</see></param>
    /// <param name="y">The y-coordinate of the point within the viewport from which to return custom highlight information. <see href="https://developer.mozilla.org/en-US/docs/Web/API/HighlightRegistry/highlightsFromPoint">MDN Web Docs: y</see></param>
    /// <param name="options">An object containing options, which can include: shadowRoots An array of ShadowRoot objects. Custom highlights that exist at the specified point inside shadow roots in the array will also be included in the return value, in addition to those present in the light DOM. By default, highlights inside shadow roots are not returned. <see href="https://developer.mozilla.org/en-US/docs/Web/API/HighlightRegistry/highlightsFromPoint">MDN Web Docs: options</see></param>
    /// <returns>An array of objects representing the custom highlights applied at the point in the viewport specified by the x and y parameters. Each object contains the following properties: highlight A Highlight object representing the applied custom highlight. ranges An array of AbstractRange objects representing the ranges to which the custom highlight is applied. If no custom highlights are applied at the specified point, or the specified point is outside the viewport, the method returns an empty array.</returns>
    [Description("@#highlightsFromPoint")]
    public extern HighlightHitResult[] HighlightsFromPoint(float x, float y, HighlightsFromPointOptions? options = default);
}

/// <summary>
/// The CSSPageDescriptors interface represents a CSS declaration block for an @page at-rule. The interface exposes style information and various style-related methods and properties for the page. Each multi-word property has versions in camel- and snake-case. This means, for example, that you can access the margin-top CSS property using the syntax style[&quot;margin-top&quot;] or style.marginTop (where style is a CSSPageDescriptor). A CSSPageDescriptors object is accessed through the style property of the CSSPageRule interface, which can in turn be found using the CSSStyleSheet API.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPageDescriptors">MDN Web Docs: CSSPageDescriptors</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSPageDescriptors")]
public class CSSPageDescriptors : CSSStyleDeclaration
{
    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.margin：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margin">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#margin")]
    public extern string Margin { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.marginTop：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margintop">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#marginTop")]
    public extern string MarginTop { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.marginRight：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-marginright">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#marginRight")]
    public extern string MarginRight { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.marginBottom：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-marginbottom">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#marginBottom")]
    public extern string MarginBottom { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.marginLeft：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-marginleft">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#marginLeft")]
    public extern string MarginLeft { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.margin-top：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margin-top">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#margin-top")]
    public extern string Margin_Top { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.margin-right：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margin-right">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#margin-right")]
    public extern string Margin_Right { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.margin-bottom：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margin-bottom">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#margin-bottom")]
    public extern string Margin_Bottom { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.margin-left：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-margin-left">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#margin-left")]
    public extern string Margin_Left { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.size：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-size">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#size")]
    public extern string Size { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.pageOrientation：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-pageorientation">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#pageOrientation")]
    public extern string PageOrientation { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.page-orientation：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-page-orientation">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#page-orientation")]
    public extern string Page_Orientation { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.marks：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-marks">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#marks")]
    public extern string Marks { get; set; }

    /// <summary>
    /// JavaScript 属性 CSSPageDescriptors.bleed：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-csspagedescriptors-bleed">CSS Object Model (CSSOM) Module Level 1: 6.4.7 The CSSPageRule Interface</see>
    /// </remarks>
    [Description("@#bleed")]
    public extern string Bleed { get; set; }
}

/// <summary>
/// The CSSRule interface represents a single CSS rule. There are several types of rules which inherit properties from CSSRule. CSSGroupingRule CSSStyleRule CSSImportRule CSSMediaRule CSSFontFaceRule CSSFunctionDeclarations CSSPageRule CSSNamespaceRule CSSKeyframesRule CSSKeyframeRule CSSCounterStyleRule CSSSupportsRule CSSFontFeatureValuesRule CSSFontPaletteValuesRule CSSLayerBlockRule CSSLayerStatementRule CSSPropertyRule CSSNestedDeclarations CSSViewTransitionRule
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRule">MDN Web Docs: CSSRule</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSRule")]
public partial class CSSRule
{
    /// <summary>
    /// CSSRule 的规范常量 NAMESPACE_RULE，WebIDL 类型为 unsigned short，值为 10。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-namespace_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </remarks>
    [Description("@#SUPPORTS_RULE")]
    public const ushort SUPPORTS_RULE = 12;

    /// <summary>
    /// The cssText property of the CSSRule interface returns the actual text of a CSSStyleSheet style-rule. Note: Do not confuse this property with element-style CSSStyleDeclaration.cssText. Be aware that this property used to be mutable but is now read-only. Attempting to set it does absolutely nothing, and doesn&apos;t even emit a warning or error. Furthermore, it has no settable sub-properties. Therefore, to modify it, use the stylesheet&apos;s cssRules[index] properties .selectorText and .style (or its sub-properties). See Using dynamic styling information for details.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRule/cssText">MDN Web Docs: CSSRule.cssText</see>
    /// </remarks>
    [Description("@#cssText")]
    public extern string CssText { get; set; }

    /// <summary>
    /// The parentRule property of the CSSRule interface returns the containing rule of the current rule if this exists, or otherwise returns null.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRule/parentRule">MDN Web Docs: CSSRule.parentRule</see>
    /// </remarks>
    [Description("@#parentRule")]
    public extern CSSRule? ParentRule { get; }

    /// <summary>
    /// The parentStyleSheet property of the CSSRule interface returns the StyleSheet object in which the current rule is defined.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRule/parentStyleSheet">MDN Web Docs: CSSRule.parentStyleSheet</see>
    /// </remarks>
    [Description("@#parentStyleSheet")]
    public extern CSSStyleSheet? ParentStyleSheet { get; }

    /// <summary>
    /// The read-only type property of the CSSRule interface is a deprecated property that returns an integer indicating which type of rule the CSSRule represents. If you need to distinguish different types of CSS rule, a good alternative is to use constructor.name: jsconst sheets = Array.from(document.styleSheets); const rules = sheets.map((sheet) =&gt; Array.from(sheet.cssRules)).flat(); for (const rule of rules) { console.log(rule.constructor.name); }
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRule/type">MDN Web Docs: CSSRule.type</see>
    /// </remarks>
    [Description("@#type")]
    public extern ushort Type { get; }

    /// <summary>
    /// CSSRule 的规范常量 NAMESPACE_RULE，WebIDL 类型为 unsigned short，值为 10。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-namespace_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </remarks>
    [Description("@#STYLE_RULE")]
    public const ushort STYLE_RULE = 1;

    /// <summary>
    /// CSSRule 的规范常量 NAMESPACE_RULE，WebIDL 类型为 unsigned short，值为 10。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-namespace_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </remarks>
    [Description("@#CHARSET_RULE")]
    public const ushort CHARSET_RULE = 2;

    /// <summary>
    /// CSSRule 的规范常量 NAMESPACE_RULE，WebIDL 类型为 unsigned short，值为 10。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-namespace_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </remarks>
    [Description("@#IMPORT_RULE")]
    public const ushort IMPORT_RULE = 3;

    /// <summary>
    /// CSSRule 的规范常量 NAMESPACE_RULE，WebIDL 类型为 unsigned short，值为 10。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-namespace_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </remarks>
    [Description("@#MEDIA_RULE")]
    public const ushort MEDIA_RULE = 4;

    /// <summary>
    /// CSSRule 的规范常量 NAMESPACE_RULE，WebIDL 类型为 unsigned short，值为 10。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-namespace_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </remarks>
    [Description("@#FONT_FACE_RULE")]
    public const ushort FONT_FACE_RULE = 5;

    /// <summary>
    /// CSSRule 的规范常量 NAMESPACE_RULE，WebIDL 类型为 unsigned short，值为 10。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-namespace_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </remarks>
    [Description("@#PAGE_RULE")]
    public const ushort PAGE_RULE = 6;

    /// <summary>
    /// CSSRule 的规范常量 NAMESPACE_RULE，WebIDL 类型为 unsigned short，值为 10。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-namespace_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </remarks>
    [Description("@#MARGIN_RULE")]
    public const ushort MARGIN_RULE = 9;

    /// <summary>
    /// CSSRule 的规范常量 NAMESPACE_RULE，WebIDL 类型为 unsigned short，值为 10。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssrule-namespace_rule">CSS Object Model (CSSOM) Module Level 1: 6.4.2 The CSSRule Interface</see>
    /// </remarks>
    [Description("@#NAMESPACE_RULE")]
    public const ushort NAMESPACE_RULE = 10;
}

/// <summary>
/// The CSSStyleProperties interface of the CSS Object Model (CSSOM) represents inline or computed styles available on an element, or the styles associated with a CSS style rule.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleProperties">MDN Web Docs: CSSStyleProperties</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSStyleProperties")]
public class CSSStyleProperties : CSSStyleDeclaration
{
    /// <summary>
    /// JavaScript 属性 CSSStyleProperties.cssFloat：CSSOMString。可读取或赋值；赋值按 WebIDL 类型转换规则处理。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssstyleproperties-cssfloat">CSS Object Model (CSSOM) Module Level 1: 6.6.1 The CSSStyleDeclaration Interface</see>
    /// </remarks>
    [Description("@#cssFloat")]
    public extern string CssFloat { get; set; }
}

/// <summary>
/// An object implementing the StyleSheet interface represents a single style sheet. CSS style sheets will further implement the more specialized CSSStyleSheet interface.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StyleSheet">MDN Web Docs: StyleSheet</see>
/// </remarks>
[ECMAScript]
[Description("@#StyleSheet")]
public class StyleSheet
{
    /// <summary>
    /// The type property of the StyleSheet interface specifies the style sheet language for the given style sheet.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StyleSheet/type">MDN Web Docs: StyleSheet.type</see>
    /// </remarks>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// The href property of the StyleSheet interface returns the location of the style sheet. This property is read-only.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StyleSheet/href">MDN Web Docs: StyleSheet.href</see>
    /// </remarks>
    [Description("@#href")]
    public extern string? Href { get; }

    /// <summary>
    /// The ownerNode property of the StyleSheet interface returns the node that associates this style sheet with the document. This is usually an HTML &lt;link&gt; or &lt;style&gt; element, but can also return a processing instruction node in the case of &lt;?xml-stylesheet ?&gt;.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StyleSheet/ownerNode">MDN Web Docs: StyleSheet.ownerNode</see>
    /// </remarks>
    [Description("@#ownerNode")]
    public extern StyleSheetOwnerNode? OwnerNode { get; }

    /// <summary>
    /// The parentStyleSheet property of the StyleSheet interface returns the style sheet, if any, that is including the given style sheet.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StyleSheet/parentStyleSheet">MDN Web Docs: StyleSheet.parentStyleSheet</see>
    /// </remarks>
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
/// The StyleSheetList interface represents a list of CSSStyleSheet objects. An instance of this object can be returned by Document.styleSheets. It is an array-like object but can&apos;t be iterated over using Array methods. However it can be iterated over in a standard for loop over its indices, or converted to an Array. Note: Typically list interfaces like StyleSheetList wrap around Array types, so you can use Array methods on them. This is not the case here for historical reasons. However, you can convert StyleSheetList to an Array in order to use those methods (see the example below).
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StyleSheetList">MDN Web Docs: StyleSheetList</see>
/// </remarks>
[ECMAScript]
[Description("@#StyleSheetList")]
public class StyleSheetList
{
    /// <summary>
    /// The item() method of the StyleSheetList interface returns a single CSSStyleSheet object.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StyleSheetList/item">MDN Web Docs: StyleSheetList.item</see>
    /// </remarks>
    /// <param name="index">An integer which is the index of the item in the collection to be returned. <see href="https://developer.mozilla.org/en-US/docs/Web/API/StyleSheetList/item">MDN Web Docs: index</see></param>
    /// <returns>A CSSStyleSheet object, or null if one does not exist for this index.</returns>
    [Description("@#item")]
    public extern CSSStyleSheet? GetItem(uint index);

    /// <summary>
    /// The length read-only property of the StyleSheetList interface returns the number of CSSStyleSheet objects in the collection.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/StyleSheetList/length">MDN Web Docs: StyleSheetList.length</see>
    /// </remarks>
    [Description("@#length")]
    public extern uint Length { get; }
}

/// <summary>
/// WebIDL interface CSSParserAtRule。定义于 CSS Parser API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/css-parser-api/#cssparseratrule">CSS Parser API: 3 Parser Values</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSParserAtRule")]
public class CSSParserAtRule : CSSParserRule
{
    /// <summary>
    /// 构造浏览器提供的 CSSParserAtRule 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-cssparseratrule">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    /// <param name="name"><see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-cssparseratrule-name-prelude-body-name">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="prelude"><see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-cssparseratrule-name-prelude-body-prelude">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="body"><see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-cssparseratrule-name-prelude-body-body">CSS Parser API: 3 Parser Values</see></param>
    public extern CSSParserAtRule(string name, CSSToken[] prelude, CSSParserRule[]? body = default);

    /// <summary>
    /// JavaScript 属性 CSSParserAtRule.name：DOMString。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-name">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// JavaScript 属性 CSSParserAtRule.prelude：FrozenArray&lt;CSSParserValue&gt;。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-prelude">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    [Description("@#prelude")]
    public extern FrozenSet<CSSParserValue> Prelude { get; }

    /// <summary>
    /// JavaScript 属性 CSSParserAtRule.body：FrozenArray&lt;CSSParserRule&gt;?。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparseratrule-body">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    [Description("@#body")]
    public extern FrozenSet<CSSParserRule>? Body { get; }
}

/// <summary>
/// WebIDL interface CSSParserBlock。定义于 CSS Parser API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/css-parser-api/#cssparserblock">CSS Parser API: 3 Parser Values</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSParserBlock")]
public class CSSParserBlock : CSSParserValue
{
    /// <summary>
    /// 构造浏览器提供的 CSSParserBlock 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserblock-cssparserblock">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    /// <param name="name"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserblock-cssparserblock-name-body-name">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="body"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserblock-cssparserblock-name-body-body">CSS Parser API: 3 Parser Values</see></param>
    public extern CSSParserBlock(string name, CSSParserValue[] body);

    /// <summary>
    /// JavaScript 属性 CSSParserBlock.name：DOMString。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserblock-name">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// JavaScript 属性 CSSParserBlock.body：FrozenArray&lt;CSSParserValue&gt;。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserblock-body">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    [Description("@#body")]
    public extern FrozenSet<CSSParserValue> Body { get; }
}

/// <summary>
/// WebIDL interface CSSParserDeclaration。定义于 CSS Parser API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/css-parser-api/#cssparserdeclaration">CSS Parser API: 3 Parser Values</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSParserDeclaration")]
public class CSSParserDeclaration : CSSParserRule
{
    /// <summary>
    /// 构造浏览器提供的 CSSParserDeclaration 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserdeclaration-cssparserdeclaration">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    /// <param name="name"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserdeclaration-cssparserdeclaration-name-body-name">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="body"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserdeclaration-cssparserdeclaration-name-body-body">CSS Parser API: 3 Parser Values</see></param>
    public extern CSSParserDeclaration(string name, CSSParserRule[]? body = default);

    /// <summary>
    /// JavaScript 属性 CSSParserDeclaration.name：DOMString。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserdeclaration-name">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// JavaScript 属性 CSSParserDeclaration.body：FrozenArray&lt;CSSParserValue&gt;。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserdeclaration-body">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    [Description("@#body")]
    public extern FrozenSet<CSSParserValue> Body { get; }
}

/// <summary>
/// WebIDL interface CSSParserFunction。定义于 CSS Parser API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/css-parser-api/#cssparserfunction">CSS Parser API: 3 Parser Values</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSParserFunction")]
public class CSSParserFunction : CSSParserValue
{
    /// <summary>
    /// 构造浏览器提供的 CSSParserFunction 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserfunction-cssparserfunction">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    /// <param name="name"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserfunction-cssparserfunction-name-args-name">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="args"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserfunction-cssparserfunction-name-args-args">CSS Parser API: 3 Parser Values</see></param>
    public extern CSSParserFunction(string name, CSSParserValue[][] args);

    /// <summary>
    /// JavaScript 属性 CSSParserFunction.name：DOMString。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserfunction-name">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// JavaScript 属性 CSSParserFunction.args：FrozenArray&lt;CSSParserValue&gt;。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserfunction-args">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    [Description("@#args")]
    public extern FrozenSet<FrozenSet<CSSParserValue>> Args { get; }
}

/// <summary>
/// WebIDL interface CSSParserQualifiedRule。定义于 CSS Parser API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/css-parser-api/#cssparserqualifiedrule">CSS Parser API: 3 Parser Values</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSParserQualifiedRule")]
public class CSSParserQualifiedRule : CSSParserRule
{
    /// <summary>
    /// 构造浏览器提供的 CSSParserQualifiedRule 对象；参数按对应 WebIDL 构造签名传递。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserqualifiedrule-cssparserqualifiedrule">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    /// <param name="prelude"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserqualifiedrule-cssparserqualifiedrule-prelude-body-prelude">CSS Parser API: 3 Parser Values</see></param>
    /// <param name="body"><see href="https://wicg.github.io/css-parser-api/#dom-cssparserqualifiedrule-cssparserqualifiedrule-prelude-body-body">CSS Parser API: 3 Parser Values</see></param>
    public extern CSSParserQualifiedRule(CSSToken[] prelude, CSSParserRule[]? body = default);

    /// <summary>
    /// JavaScript 属性 CSSParserQualifiedRule.prelude：FrozenArray&lt;CSSParserValue&gt;。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserqualifiedrule-prelude">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    [Description("@#prelude")]
    public extern FrozenSet<CSSParserValue> Prelude { get; }

    /// <summary>
    /// JavaScript 属性 CSSParserQualifiedRule.body：FrozenArray&lt;CSSParserRule&gt;。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-cssparserqualifiedrule-body">CSS Parser API: 3 Parser Values</see>
    /// </remarks>
    [Description("@#body")]
    public extern FrozenSet<CSSParserRule> Body { get; }
}

/// <summary>
/// WebIDL interface CSSParserRule。定义于 CSS Parser API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/css-parser-api/#cssparserrule">CSS Parser API: 3 Parser Values</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSParserRule")]
public abstract class CSSParserRule
{
}

/// <summary>
/// WebIDL interface CSSParserValue。定义于 CSS Parser API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/css-parser-api/#cssparservalue">CSS Parser API: 3 Parser Values</see>
/// </remarks>
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
    /// The insertRule() method of the CSSGroupingRule interface adds a new CSS rule to a list of CSS rules.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSGroupingRule/insertRule">MDN Web Docs: CSSGroupingRule.insertRule</see>
    /// </remarks>
    /// <param name="rule">A string <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSGroupingRule/insertRule">MDN Web Docs: rule</see></param>
    /// <param name="index">An optional index at which to insert the rule; defaults to 0. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSGroupingRule/insertRule">MDN Web Docs: index</see></param>
    /// <returns>The index of the new rule.</returns>
    [Description("@#insertRule")]
    public extern uint InsertRule(string rule, uint index = 0);

    /// <summary>
    /// The deleteRule() method of the CSSGroupingRule interface removes a CSS rule from a list of child CSS rules.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSGroupingRule/deleteRule">MDN Web Docs: CSSGroupingRule.deleteRule</see>
    /// </remarks>
    /// <param name="index">The index of the rule to delete. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSGroupingRule/deleteRule">MDN Web Docs: index</see></param>
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
    /// The read-only CSSStyleSheet property ownerRule returns the CSSImportRule corresponding to the @import at-rule which imported the stylesheet into the document. If the stylesheet wasn&apos;t imported into the document using @import, the returned value is null.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/ownerRule">MDN Web Docs: CSSStyleSheet.ownerRule</see>
    /// </remarks>
    [Description("@#ownerRule")]
    public extern CSSRule? OwnerRule { get; }

    /// <summary>
    /// The read-only CSSStyleSheet property cssRules returns a live CSSRuleList which provides a real-time, up-to-date list of every CSS rule which comprises the stylesheet. Each item in the list is a CSSRule defining a single rule.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/cssRules">MDN Web Docs: CSSStyleSheet.cssRules</see>
    /// </remarks>
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
    /// The CSSStyleSheet method deleteRule() removes a rule from the stylesheet object.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/deleteRule">MDN Web Docs: CSSStyleSheet.deleteRule</see>
    /// </remarks>
    /// <param name="index">The index into the stylesheet&apos;s CSSRuleList indicating the rule to be removed. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/deleteRule">MDN Web Docs: index</see></param>
    [Description("@#deleteRule")]
    public extern void DeleteRule(uint index);

    /// <summary>
    /// The replace() method of the CSSStyleSheet interface asynchronously replaces the content of the stylesheet with the content passed into it. The method returns a promise that resolves with the CSSStyleSheet object. The replace() and CSSStyleSheet.replaceSync() methods can only be used on a stylesheet created with the CSSStyleSheet() constructor.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/replace">MDN Web Docs: CSSStyleSheet.replace</see>
    /// </remarks>
    /// <param name="text">A string containing the style rules to replace the content of the stylesheet. If the string does not contain a parsable list of rules, then the value will be set to an empty string. Note: If any of the rules passed in text are an external stylesheet imported with the @import rule, those rules will be removed, and a warning printed to the console. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/replace">MDN Web Docs: text</see></param>
    /// <returns>A Promise that resolves with the CSSStyleSheet.</returns>
    [Description("@#replace")]
    public extern PromiseResult<CSSStyleSheet> Replace(string text);

    /// <summary>
    /// The replaceSync() method of the CSSStyleSheet interface synchronously replaces the content of the stylesheet with the content passed into it. The replaceSync() and CSSStyleSheet.replace() methods can only be used on a stylesheet created with the CSSStyleSheet() constructor.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/replaceSync">MDN Web Docs: CSSStyleSheet.replaceSync</see>
    /// </remarks>
    /// <param name="text">A string containing the style rules to replace the content of the stylesheet. If the string does not contain a parsable list of rules, then the value will be set to an empty string. Note: If any of the rules passed in text are an external stylesheet imported with the @import rule, those rules will be removed, and a warning printed to the console. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/replaceSync">MDN Web Docs: text</see></param>
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
    /// The obsolete CSSStyleSheet interface&apos;s addRule() legacy method adds a new rule to the stylesheet. You should avoid using this method, and should instead use the more standard insertRule() method.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/addRule">MDN Web Docs: CSSStyleSheet.addRule</see>
    /// </remarks>
    /// <param name="selector">A string specifying the selector portion of the CSS rule. The default is the string undefined. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/addRule">MDN Web Docs: selector</see></param>
    /// <param name="style"><see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheet-addrule-selector-style-index-style">CSS Object Model (CSSOM) Module Level 1: 6.1.2.1 Deprecated CSSStyleSheet members</see></param>
    /// <param name="index">An optional index into the stylesheet&apos;s CSSRuleList at which to insert the new rule. If index is not specified, the next index after the last item currently in the list is used (that is, the value of cssStyleSheet.cssRules.length). <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/addRule">MDN Web Docs: index</see></param>
    /// <returns>Always returns -1. Note that due to somewhat esoteric rules about where you can legally insert rules, it&apos;s possible that an exception may be thrown. See insertRule() for more information.</returns>
    [Description("@#addRule")]
    public extern int AddRule(string selector = "undefined", string style = "undefined", uint? index = default);

    /// <summary>
    /// The obsolete CSSStyleSheet method removeRule() removes a rule from the stylesheet object. It is functionally identical to the standard, preferred method deleteRule(). Note: This is a legacy method which has been replaced by the standard method deleteRule(). You should use that instead.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/removeRule">MDN Web Docs: CSSStyleSheet.removeRule</see>
    /// </remarks>
    /// <param name="index">The index into the stylesheet&apos;s CSSRuleList indicating the rule to be removed. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleSheet/removeRule">MDN Web Docs: index</see></param>
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
    /// The read-only href property of the CSSImportRule interface returns the URL specified by the @import at-rule. The resolved URL will be the href attribute of the associated stylesheet.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSImportRule/href">MDN Web Docs: CSSImportRule.href</see>
    /// </remarks>
    [Description("@#href")]
    public extern string Href { get; }

    /// <summary>
    /// The read-only media property of the CSSImportRule interface returns a MediaList object representing the media query list of the @import rule.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSImportRule/media">MDN Web Docs: CSSImportRule.media</see>
    /// </remarks>
    [Description("@#media")]
    public extern MediaList Media { get; }

    /// <summary>
    /// The read-only styleSheet property of the CSSImportRule interface returns the CSS Stylesheet specified by the @import at-rule. This will be in the form of a CSSStyleSheet object. An @import at-rule always has an associated stylesheet.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSImportRule/styleSheet">MDN Web Docs: CSSImportRule.styleSheet</see>
    /// </remarks>
    [Description("@#styleSheet")]
    public extern CSSStyleSheet? StyleSheet { get; }

    /// <summary>
    /// The read-only layerName property of the CSSImportRule interface returns the name of the cascade layer created by the @import at-rule. If the created layer is anonymous, the string is empty (&quot;&quot;), if no layer has been created, it is the null object.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSImportRule/layerName">MDN Web Docs: CSSImportRule.layerName</see>
    /// </remarks>
    [Description("@#layerName")]
    public extern string? LayerName { get; }

    /// <summary>
    /// The read-only supportsText property of the CSSImportRule interface returns the supports condition specified by the @import at-rule.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSImportRule/supportsText">MDN Web Docs: CSSImportRule.supportsText</see>
    /// </remarks>
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
/// The CSSStyleRule interface represents a single CSS style rule.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleRule">MDN Web Docs: CSSStyleRule</see>
/// </remarks>
[ECMAScript]
[Description("@#CSSStyleRule")]
public partial class CSSStyleRule : CSSGroupingRule
{
    /// <summary>
    /// The styleMap read-only property of the CSSStyleRule interface returns a StylePropertyMap object which provides access to the rule&apos;s property-value pairs.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleRule/styleMap">MDN Web Docs: CSSStyleRule.styleMap</see>
    /// </remarks>
    [Description("@#styleMap")]
    public extern StylePropertyMap StyleMap { get; }

    /// <summary>
    /// The selectorText property of the CSSStyleRule interface gets and sets the selectors associated with the CSSStyleRule.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleRule/selectorText">MDN Web Docs: CSSStyleRule.selectorText</see>
    /// </remarks>
    [Description("@#selectorText")]
    public extern string SelectorText { get; set; }

    /// <summary>
    /// The read-only style property of the CSSStyleRule interface contains a CSSStyleProperties object representing the properties list in this style rule&apos;s body. Each CSS property supported by the browser is present on the object. The properties that are not defined inline in the corresponding CSS declaration are set to the empty string (&quot;&quot;).
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleRule/style">MDN Web Docs: CSSStyleRule.style</see>
    /// </remarks>
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
    /// The read-only length property of the MediaList interface returns the number of media queries in the list.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/MediaList/length">MDN Web Docs: MediaList.length</see>
    /// </remarks>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// The item() method of the MediaList interface returns the media query at the specified index, or null if the specified index doesn&apos;t exist.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/MediaList/item">MDN Web Docs: MediaList.item</see>
    /// </remarks>
    /// <param name="index">An integer. <see href="https://developer.mozilla.org/en-US/docs/Web/API/MediaList/item">MDN Web Docs: index</see></param>
    /// <returns>If the bracket ([]) syntax is used and there is no entry for the given index, undefined is returned.</returns>
    [Description("@#item")]
    public extern string? GetItem(uint index);

    /// <summary>
    /// The appendMedium() method of the MediaList interface adds a media query to the list. If the media query is already in the collection, this method does nothing.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/MediaList/appendMedium">MDN Web Docs: MediaList.appendMedium</see>
    /// </remarks>
    /// <param name="medium">A string containing the media query to add. <see href="https://developer.mozilla.org/en-US/docs/Web/API/MediaList/appendMedium">MDN Web Docs: medium</see></param>
    [Description("@#appendMedium")]
    public extern void AppendMedium(string medium);

    /// <summary>
    /// The deleteMedium() method of the MediaList interface removes from this MediaList the given media query.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/MediaList/deleteMedium">MDN Web Docs: MediaList.deleteMedium</see>
    /// </remarks>
    /// <param name="medium">A string containing the media query to remove from the list. <see href="https://developer.mozilla.org/en-US/docs/Web/API/MediaList/deleteMedium">MDN Web Docs: medium</see></param>
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
    /// JavaScript 属性 CSSMarginRule.name：CSSOMString。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssmarginrule-name">CSS Object Model (CSSOM) Module Level 1: 6.4.8 The CSSMarginRule Interface</see>
    /// </remarks>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// JavaScript 属性 CSSMarginRule.style：CSSStyleDeclaration。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-cssmarginrule-style">CSS Object Model (CSSOM) Module Level 1: 6.4.8 The CSSMarginRule Interface</see>
    /// </remarks>
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
    /// The selectorText property of the CSSPageRule interface gets and sets the selectors associated with the CSSPageRule.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPageRule/selectorText">MDN Web Docs: CSSPageRule.selectorText</see>
    /// </remarks>
    [Description("@#selectorText")]
    public extern string SelectorText { get; set; }

    /// <summary>
    /// The read-only style property of the CSSPageRule interface contains a CSSPageDescriptors object representing the descriptors available in the @page rule&apos;s body.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSPageRule/style">MDN Web Docs: CSSPageRule.style</see>
    /// </remarks>
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
    /// <summary>
    /// Highlight 的 WebIDL 集合接口；元素类型为 AbstractRange。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/">CSS Custom Highlight API Module Level 1: Highlight.setlike</see>
    /// </remarks>
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
    /// The item() method of the CSSRuleList interface returns the CSSRule object at the specified index or null if the specified index doesn&apos;t exist.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRuleList/item">MDN Web Docs: CSSRuleList.item</see>
    /// </remarks>
    /// <param name="index">An integer. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRuleList/item">MDN Web Docs: index</see></param>
    /// <returns>A CSSRule.</returns>
    [Description("@#item")]
    public extern CSSRule? GetItem(uint index);

    /// <summary>
    /// The length property of the CSSRuleList interface returns the number of CSSRule objects in the list.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSRuleList/length">MDN Web Docs: CSSRuleList.length</see>
    /// </remarks>
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
    /// The cssText property of the CSSStyleDeclaration interface returns or sets the text of the element&apos;s inline style declaration only. To be able to set a stylesheet rule dynamically, see Using dynamic styling information. Not to be confused with stylesheet style-rule CSSRule.cssText.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleDeclaration/cssText">MDN Web Docs: CSSStyleDeclaration.cssText</see>
    /// </remarks>
    [Description("@#cssText")]
    public extern string CssText { get; set; }

    /// <summary>
    /// The read-only property returns an integer that represents the number of style declarations in this CSS declaration block.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleDeclaration/length">MDN Web Docs: CSSStyleDeclaration.length</see>
    /// </remarks>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// The CSSStyleDeclaration.item() method interface returns a CSS property name from a CSSStyleDeclaration by index. This method doesn&apos;t throw exceptions as long as you provide arguments; the empty string is returned if the index is out of range and a TypeError is thrown if no argument is provided.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleDeclaration/item">MDN Web Docs: CSSStyleDeclaration.item</see>
    /// </remarks>
    /// <param name="index">The index of the node to be fetched. The index is zero-based. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleDeclaration/item">MDN Web Docs: index</see></param>
    /// <returns>A string that is the name of the CSS property at the specified index. JavaScript has a special simpler syntax for obtaining an item from a NodeList by index: jsconst propertyName = style[index];</returns>
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
    /// The CSSStyleDeclaration.parentRule read-only property returns a CSSRule that is the parent of this style block, e.g., a CSSStyleRule representing the style for a CSS selector.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSSStyleDeclaration/parentRule">MDN Web Docs: CSSStyleDeclaration.parentRule</see>
    /// </remarks>
    [Description("@#parentRule")]
    public extern CSSRule? ParentRule { get; }
}

/// <summary>
/// Element is the most general base class from which all element objects (i.e., objects that represent elements) in a Document inherit. It only has methods and properties common to all kinds of elements. More specific classes inherit from Element. For example, the HTMLElement interface is the base interface for HTML elements. Similarly, the SVGElement interface is the basis for all SVG elements, and the MathMLElement interface is the base interface for MathML elements. Most functionality is specified further down the class hierarchy. Languages outside the realm of the Web platform, like XUL through the XULElement interface, also implement Element.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/Element">MDN Web Docs: Element</see>
/// </remarks>
[ECMAScript]
[Description("@#Element")]
public partial class Element
{
    /// <summary>
    /// The computedStyleMap() method of the Element interface returns a StylePropertyMapReadOnly interface which provides a read-only representation of a CSS declaration block that is an alternative to CSSStyleDeclaration.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/Element/computedStyleMap">MDN Web Docs: Element.computedStyleMap</see>
    /// </remarks>
    /// <returns>A StylePropertyMapReadOnly object. Unlike Window.getComputedStyle, the return value contains computed values, not resolved values. For most properties, they are the same, except a few layout-related properties, where the resolved value is the used value instead of the computed value. See the comparison with getComputedStyle() example for details.</returns>
    [Description("@#computedStyleMap")]
    public extern StylePropertyMapReadOnly ComputedStyleMap();
}

/// <summary>
/// The Window interface represents a window containing a DOM document; the document property points to the DOM document loaded in that window. A window for a given document can be obtained using the document.defaultView property. A global variable, window, representing the window in which the script is running, is exposed to JavaScript code. The Window interface is home to a variety of functions, namespaces, objects, and constructors which are not necessarily directly associated with the concept of a user interface window. However, the Window interface is a suitable place to include these items that need to be globally available. Many of these are documented in the JavaScript Reference and the DOM Reference. In a tabbed browser, each tab is represented by its own Window object; the global window seen by JavaScript code running within a given tab always represents the tab in which the code is running. That said, even in a tabbed browser, some properties and methods still apply to the overall window that contains the tab, such as resizeTo() and innerHeight. Generally, anything that can&apos;t reasonably pertain to a tab pertains to the window instead.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/Window">MDN Web Docs: Window</see>
/// </remarks>
[ECMAScript]
[Description("@#Window")]
public partial class WindowRef
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