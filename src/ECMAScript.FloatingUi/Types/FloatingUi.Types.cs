using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// 浮层相对参考元素的摆放方位；字符串值与 Floating UI 的 <c>Placement</c> 值域一致。
/// The side and alignment where the floating element is placed; values mirror the Floating UI <c>Placement</c> domain.
/// </summary>
[String]
public enum FloatingPlacement
{
    /// <summary>顶部居中。Top, centered.</summary>
    [Description("@#top")]
    Top,

    /// <summary>顶部起点对齐（书写方向起点）。Top, aligned to start.</summary>
    [Description("@#top-start")]
    TopStart,

    /// <summary>顶部终点对齐（书写方向终点）。Top, aligned to end.</summary>
    [Description("@#top-end")]
    TopEnd,

    /// <summary>右侧居中。Right, centered.</summary>
    [Description("@#right")]
    Right,

    /// <summary>右侧起点对齐。Right, aligned to start.</summary>
    [Description("@#right-start")]
    RightStart,

    /// <summary>右侧终点对齐。Right, aligned to end.</summary>
    [Description("@#right-end")]
    RightEnd,

    /// <summary>底部居中。Bottom, centered.</summary>
    [Description("@#bottom")]
    Bottom,

    /// <summary>底部起点对齐。Bottom, aligned to start.</summary>
    [Description("@#bottom-start")]
    BottomStart,

    /// <summary>底部终点对齐。Bottom, aligned to end.</summary>
    [Description("@#bottom-end")]
    BottomEnd,

    /// <summary>左侧居中。Left, centered.</summary>
    [Description("@#left")]
    Left,

    /// <summary>左侧起点对齐。Left, aligned to start.</summary>
    [Description("@#left-start")]
    LeftStart,

    /// <summary>左侧终点对齐。Left, aligned to end.</summary>
    [Description("@#left-end")]
    LeftEnd
}

/// <summary>
/// 浮层摆放的基本边；字符串值与 Floating UI 的 <c>Side</c> 值域一致。
/// The base side of a placement; values mirror the Floating UI <c>Side</c> domain.
/// </summary>
[String]
public enum FloatingSide
{
    /// <summary>顶部。Top.</summary>
    [Description("@#top")]
    Top,

    /// <summary>右侧。Right.</summary>
    [Description("@#right")]
    Right,

    /// <summary>底部。Bottom.</summary>
    [Description("@#bottom")]
    Bottom,

    /// <summary>左侧。Left.</summary>
    [Description("@#left")]
    Left
}

/// <summary>
/// 对准方向；字符串值与 Floating UI 的 <c>Alignment</c> 值域一致。
/// The alignment axis; values mirror the Floating UI <c>Alignment</c> domain.
/// </summary>
[String]
public enum FloatingAlignment
{
    /// <summary>起点对齐。Start alignment.</summary>
    [Description("@#start")]
    Start,

    /// <summary>终点对齐。End alignment.</summary>
    [Description("@#end")]
    End
}

/// <summary>
/// 浮层定位使用的 CSS position 策略；字符串值与 Floating UI 的 <c>Strategy</c> 值域一致。
/// The CSS position strategy used for the floating element; values mirror the Floating UI <c>Strategy</c> domain.
/// </summary>
[String]
public enum FloatingStrategy
{
    /// <summary>绝对定位。Absolute positioning.</summary>
    [Description("@#absolute")]
    Absolute,

    /// <summary>固定定位。Fixed positioning.</summary>
    [Description("@#fixed")]
    Fixed
}

/// <summary>
/// 可作为参考或浮层目标的值：真实 DOM <c>Element</c> 或已挂载的 Vue 组件公共实例。
/// A value usable as a reference or floating target: a real DOM <c>Element</c> or a mounted Vue component public instance.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union FloatingMaybeElement(Element, Vue.VueComponentPublicInstance);

/// <summary>
/// 中间件对象；由 <c>offset()</c>、<c>shift()</c> 等工厂创建，由库内部消费。
/// A middleware object created by factories such as <c>offset()</c> or <c>shift()</c> and consumed internally.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class FloatingMiddleware
{
    private FloatingMiddleware()
    {
    }
}

/// <summary>
/// 浮层的二维坐标。
/// The two-dimensional coordinates of the floating element.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingCoords
{
    /// <summary>水平坐标。The x coordinate.</summary>
    [Description("@#x")]
    public Number X { get; init; } = default!;

    /// <summary>垂直坐标。The y coordinate.</summary>
    [Description("@#y")]
    public Number Y { get; init; } = default!;
}

/// <summary>
/// 元素尺寸。
/// The element dimensions.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingDimensions
{
    /// <summary>宽度。The width.</summary>
    [Description("@#width")]
    public Number Width { get; init; } = default!;

    /// <summary>高度。The height.</summary>
    [Description("@#height")]
    public Number Height { get; init; } = default!;
}

/// <summary>
/// 元素矩形：坐标与尺寸的组合。
/// An element rectangle combining coordinates and dimensions.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingRect
{
    /// <summary>水平坐标。The x coordinate.</summary>
    [Description("@#x")]
    public Number X { get; init; } = default!;

    /// <summary>垂直坐标。The y coordinate.</summary>
    [Description("@#y")]
    public Number Y { get; init; } = default!;

    /// <summary>宽度。The width.</summary>
    [Description("@#width")]
    public Number Width { get; init; } = default!;

    /// <summary>高度。The height.</summary>
    [Description("@#height")]
    public Number Height { get; init; } = default!;
}

/// <summary>
/// 四边溢出量；键为 top/right/bottom/left。
/// The overflow amounts for each side; keys are top/right/bottom/left.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingSideObject
{
    /// <summary>顶部溢出量。The top overflow.</summary>
    [Description("@#top")]
    public Number Top { get; init; } = default!;

    /// <summary>右侧溢出量。The right overflow.</summary>
    [Description("@#right")]
    public Number Right { get; init; } = default!;

    /// <summary>底部溢出量。The bottom overflow.</summary>
    [Description("@#bottom")]
    public Number Bottom { get; init; } = default!;

    /// <summary>左侧溢出量。The left overflow.</summary>
    [Description("@#left")]
    public Number Left { get; init; } = default!;
}

/// <summary>
/// 参考元素与浮层元素的矩形对。
/// The rectangle pair for the reference and floating elements.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingElementRects
{
    /// <summary>参考元素矩形。The reference element rectangle.</summary>
    [Description("@#reference")]
    public FloatingRect Reference { get; init; } = default!;

    /// <summary>浮层元素矩形。The floating element rectangle.</summary>
    [Description("@#floating")]
    public FloatingRect Floating { get; init; } = default!;
}

/// <summary>
/// 参与定位的元素对。
/// The element pair used for positioning.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingElements
{
    /// <summary>参考元素。The reference element.</summary>
    [Description("@#reference")]
    public FloatingMaybeElement Reference { get; init; } = default!;

    /// <summary>浮层元素。The floating element.</summary>
    [Description("@#floating")]
    public FloatingMaybeElement Floating { get; init; } = default!;
}

/// <summary>
/// 中间件返回的数据；按中间件名索引，未使用的键为 undefined。
/// Data returned by middleware, indexed by middleware name; unused keys are undefined.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingMiddlewareData
{
    /// <summary>arrow 中间件数据（含居中偏移）。Data from the <c>arrow</c> middleware, including the center offset.</summary>
    [Description("@#arrow")]
    public FloatingArrowData? Arrow { get; init; }

    /// <summary>flip 中间件数据（实际采用的索引）。Data from the <c>flip</c> middleware with the chosen index.</summary>
    [Description("@#flip")]
    public FloatingPlacementData? Flip { get; init; }

    /// <summary>shift 中间件数据（实际采用的索引）。Data from the <c>shift</c> middleware with the chosen index.</summary>
    [Description("@#shift")]
    public FloatingPlacementData? Shift { get; init; }

    /// <summary>hide 中间件数据（是否被裁剪）。Data from the <c>hide</c> middleware indicating whether the element is clipped.</summary>
    [Description("@#hide")]
    public FloatingHideData? Hide { get; init; }
}

/// <summary>
/// arrow 中间件的定位数据。
/// Positioning data produced by the <c>arrow</c> middleware.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingArrowData
{
    /// <summary>箭头相对浮层中心的偏移。The arrow offset from the floating element center.</summary>
    [Description("@#centerOffset")]
    public Number CenterOffset { get; init; } = default!;

    /// <summary>对准轴上的额外偏移。The additional offset along the alignment axis.</summary>
    [Description("@#alignmentOffset")]
    public Number? AlignmentOffset { get; init; }

    /// <summary>水平坐标。The x coordinate.</summary>
    [Description("@#x")]
    public Number? X { get; init; }

    /// <summary>垂直坐标。The y coordinate.</summary>
    [Description("@#y")]
    public Number? Y { get; init; }
}

/// <summary>
/// 摆放修正中间件（flip、shift）的索引数据。
/// Index data produced by placement-modifying middleware such as <c>flip</c> and <c>shift</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingPlacementData
{
    /// <summary>实际选中的候选索引；undefined 表示未修正。The chosen candidate index; undefined when no adjustment was made.</summary>
    [Description("@#index")]
    public Number? Index { get; init; }
}

/// <summary>
/// hide 中间件的裁剪数据。
/// Clipping data produced by the <c>hide</c> middleware.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingHideData
{
    /// <summary>参考元素是否被裁剪。Whether the reference element is clipped.</summary>
    [Description("@#referenceHidden")]
    public bool? ReferenceHidden { get; init; }

    /// <summary>浮层元素是否被裁剪。Whether the floating element is clipped.</summary>
    [Description("@#escaped")]
    public bool? Escaped { get; init; }
}

/// <summary>
/// 应用于浮层元素的定位样式。
/// The positioning styles to apply to the floating element.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingStyles
{
    /// <summary>CSS position 策略。The CSS position strategy.</summary>
    [Description("@#position")]
    public FloatingStrategy Position { get; init; } = default!;

    /// <summary>top 样式值。The <c>top</c> style value.</summary>
    [Description("@#top")]
    public string Top { get; init; } = default!;

    /// <summary>left 样式值。The <c>left</c> style value.</summary>
    [Description("@#left")]
    public string Left { get; init; } = default!;

    /// <summary>transform 样式值；使用 transform 定位时存在。The <c>transform</c> style value, present when positioning via transform.</summary>
    [Description("@#transform")]
    public string? Transform { get; init; }

    /// <summary>will-change 样式值；库按需设置。The <c>will-change</c> style value, set by the library when needed.</summary>
    [Description("@#willChange")]
    public string? WillChange { get; init; }
}

/// <summary>
/// <c>computePosition()</c> 的返回结果。
/// The result returned by <c>computePosition()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingComputePositionReturn
{
    /// <summary>水平坐标。The x coordinate.</summary>
    [Description("@#x")]
    public Number X { get; init; } = default!;

    /// <summary>垂直坐标。The y coordinate.</summary>
    [Description("@#y")]
    public Number Y { get; init; } = default!;

    /// <summary>最终采用的摆放方位。The final resolved placement.</summary>
    [Description("@#placement")]
    public FloatingPlacement Placement { get; init; } = default!;

    /// <summary>使用的 CSS position 策略。The CSS position strategy in use.</summary>
    [Description("@#strategy")]
    public FloatingStrategy Strategy { get; init; } = default!;

    /// <summary>中间件返回的附加数据。Additional data returned by middleware.</summary>
    [Description("@#middlewareData")]
    public FloatingMiddlewareData MiddlewareData { get; init; } = default!;
}

/// <summary>
/// <c>computePosition()</c> 的选项对象。
/// Options for <c>computePosition()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingComputePositionConfig
{
    /// <summary>初始摆放方位。The initial placement.</summary>
    [Description("@#placement")]
    public FloatingPlacement? Placement { get; init; }

    /// <summary>使用的 CSS position 策略。The CSS position strategy to use.</summary>
    [Description("@#strategy")]
    public FloatingStrategy? Strategy { get; init; }

    /// <summary>参与定位的中间件列表。The middleware list used for positioning.</summary>
    [Description("@#middleware")]
    public FloatingMiddleware[]? Middleware { get; init; }
}

/// <summary>
/// <c>offset()</c> 中间件的选项：数值或按主轴/交叉轴/对准轴给出的偏移。
/// Options for the <c>offset()</c> middleware: a number or per-axis offsets.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingOffsetValue
{
    /// <summary>主轴（摆放方向）偏移。The offset along the main axis.</summary>
    [Description("@#mainAxis")]
    public Number? MainAxis { get; init; }

    /// <summary>交叉轴偏移。The offset along the cross axis.</summary>
    [Description("@#crossAxis")]
    public Number? CrossAxis { get; init; }

    /// <summary>对准轴偏移。The offset along the alignment axis.</summary>
    [Description("@#alignmentAxis")]
    public Number? AlignmentAxis { get; init; }
}

/// <summary>
/// detectOverflow 的溢出检测选项，被 shift/flip/size 等中间件共享。
/// Overflow-detection options shared by middleware such as shift, flip, and size.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingDetectOverflowOptions
{
    /// <summary>裁剪边界：指定元素、元素数组或 <c>"clippingAncestors"</c>。The clipping boundary: an element, an element array, or <c>"clippingAncestors"</c>.</summary>
    [Description("@#boundary")]
    public FloatingBoundary? Boundary { get; init; }

    /// <summary>根边界：<c>"viewport"</c> 或 <c>"document"</c>。The root boundary: <c>"viewport"</c> or <c>"document"</c>.</summary>
    [Description("@#rootBoundary")]
    public FloatingRootBoundary? RootBoundary { get; init; }

    /// <summary>检测的边：<c>"t"</c>/<c>"b"</c>/<c>"l"</c>/<c>"r"</c> 的组合。The sides to detect, a combination of <c>"t"</c>/<c>"b"</c>/<c>"l"</c>/<c>"r"</c>.</summary>
    [Description("@#elementContext")]
    public FloatingElementContext? ElementContext { get; init; }

    /// <summary>间距，数值或按边给出的对象。The padding, a number or a per-side object.</summary>
    [Description("@#padding")]
    public Number? Padding { get; init; }

    /// <summary>将裁剪边界视为“包含浮层元素自身”。Whether to treat the clipping boundary as containing the floating element.</summary>
    [Description("@#altBoundary")]
    public bool? AltBoundary { get; init; }
}

/// <summary>
/// 裁剪边界值域：字符串（<c>"clippingAncestors"</c>）、元素或元素数组。
/// The clipping boundary domain: a string (<c>"clippingAncestors"</c>), an element, or an element array.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union FloatingBoundary(string, Element, Element[]);

/// <summary>
/// 根边界值域。
/// The root boundary domain.
/// </summary>
[String]
public enum FloatingRootBoundary
{
    /// <summary>以视口为根边界。Use the viewport as the root boundary.</summary>
    [Description("@#viewport")]
    Viewport,

    /// <summary>以文档为根边界。Use the document as the root boundary.</summary>
    [Description("@#document")]
    Document
}

/// <summary>
/// 溢出检测的元素上下文。
/// The element context used for overflow detection.
/// </summary>
[String]
public enum FloatingElementContext
{
    /// <summary>以参考元素为上下文。Use the reference element as the context.</summary>
    [Description("@#reference")]
    Reference,

    /// <summary>以浮层元素为上下文。Use the floating element as the context.</summary>
    [Description("@#floating")]
    Floating
}

/// <summary>
/// <c>shift()</c> 中间件的选项。
/// Options for the <c>shift()</c> middleware.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingShiftOptions
{
    /// <summary>主轴偏移；设为 true 时保持参考元素与浮层在主轴上的对齐。The main-axis offset; <c>true</c> keeps the main axis aligned with the reference.</summary>
    [Description("@#mainAxis")]
    public bool? MainAxis { get; init; }

    /// <summary>交叉轴偏移；设为 true 时保持交叉轴对齐。The cross-axis offset; <c>true</c> keeps the cross axis aligned.</summary>
    [Description("@#crossAxis")]
    public bool? CrossAxis { get; init; }

    /// <summary>是否限定在裁剪祖先范围内。Whether to limit shifts to clipping ancestors.</summary>
    [Description("@#limiter")]
    public FloatingLimiter? Limiter { get; init; }

    /// <summary>溢出检测选项。The overflow-detection options.</summary>
    [Description("@#boundary")]
    public FloatingBoundary? Boundary { get; init; }

    /// <summary>根边界。The root boundary.</summary>
    [Description("@#rootBoundary")]
    public FloatingRootBoundary? RootBoundary { get; init; }

    /// <summary>检测间距。The detection padding.</summary>
    [Description("@#padding")]
    public Number? Padding { get; init; }

    /// <summary>是否改用另一侧边界进行检测。Whether to detect against the opposite boundary.</summary>
    [Description("@#altBoundary")]
    public bool? AltBoundary { get; init; }
}

/// <summary>
/// shift 的位移限制器；目前仅支持 <c>limitShift()</c> 的返回值。
/// The shift limiter; currently only the value returned by <c>limitShift()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class FloatingLimiter
{
    private FloatingLimiter()
    {
    }
}

/// <summary>
/// <c>flip()</c> 中间件的选项。
/// Options for the <c>flip()</c> middleware.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingFlipOptions
{
    /// <summary>主轴：是否在主轴溢出时翻转。The main axis: whether to flip along the main axis.</summary>
    [Description("@#mainAxis")]
    public bool? MainAxis { get; init; }

    /// <summary>交叉轴：是否在交叉轴溢出时翻转。The cross axis: whether to flip along the cross axis.</summary>
    [Description("@#crossAxis")]
    public bool? CrossAxis { get; init; }

    /// <summary>是否允许回退到初始摆放。Whether to fall back to the initial placement.</summary>
    [Description("@#fallbackPlacements")]
    public FloatingPlacement[]? FallbackPlacements { get; init; }

    /// <summary>回退策略：<c>"bestFit"</c> 或 <c>"initialPlacement"</c>。The fallback strategy: <c>"bestFit"</c> or <c>"initialPlacement"</c>.</summary>
    [Description("@#fallbackStrategy")]
    public FloatingFallbackStrategy? FallbackStrategy { get; init; }

    /// <summary>是否允许允许的摆放边界。Whether to allow the allowed placements.</summary>
    [Description("@#allowedPlacements")]
    public FloatingPlacement[]? AllowedPlacements { get; init; }

    /// <summary>溢出检测选项。The overflow-detection options.</summary>
    [Description("@#boundary")]
    public FloatingBoundary? Boundary { get; init; }

    /// <summary>根边界。The root boundary.</summary>
    [Description("@#rootBoundary")]
    public FloatingRootBoundary? RootBoundary { get; init; }

    /// <summary>检测间距。The detection padding.</summary>
    [Description("@#padding")]
    public Number? Padding { get; init; }

    /// <summary>是否改用另一侧边界进行检测。Whether to detect against the opposite boundary.</summary>
    [Description("@#altBoundary")]
    public bool? AltBoundary { get; init; }
}

/// <summary>
/// flip 的回退策略。
/// The fallback strategy for <c>flip</c>.
/// </summary>
[String]
public enum FloatingFallbackStrategy
{
    /// <summary>选择溢出最少的摆放。Choose the placement with the least overflow.</summary>
    [Description("@#bestFit")]
    BestFit,

    /// <summary>回退到初始摆放。Fall back to the initial placement.</summary>
    [Description("@#initialPlacement")]
    InitialPlacement
}

/// <summary>
/// <c>size()</c> 中间件的选项。
/// Options for the <c>size()</c> middleware.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingSizeOptions
{
    /// <summary>溢出检测选项。The overflow-detection options.</summary>
    [Description("@#boundary")]
    public FloatingBoundary? Boundary { get; init; }

    /// <summary>根边界。The root boundary.</summary>
    [Description("@#rootBoundary")]
    public FloatingRootBoundary? RootBoundary { get; init; }

    /// <summary>检测间距。The detection padding.</summary>
    [Description("@#padding")]
    public Number? Padding { get; init; }

    /// <summary>是否改用另一侧边界进行检测。Whether to detect against the opposite boundary.</summary>
    [Description("@#altBoundary")]
    public bool? AltBoundary { get; init; }
}

/// <summary>
/// <c>autoPlacement()</c> 中间件的选项。
/// Options for the <c>autoPlacement()</c> middleware.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingAutoPlacementOptions
{
    /// <summary>候选摆放范围：<c>"all"</c>、<c>"sides"</c> 或 <c>"alignments"</c>。The candidate placement scope: <c>"all"</c>, <c>"sides"</c>, or <c>"alignments"</c>.</summary>
    [Description("@#allowedPlacements")]
    public FloatingPlacement[]? AllowedPlacements { get; init; }

    /// <summary>选择策略：<c>"bestFit"</c>。The selection strategy: <c>"bestFit"</c>.</summary>
    [Description("@#alignment")]
    public FloatingAlignment? Alignment { get; init; }

    /// <summary>自动批处理布局，减少中间重排。Whether to batch layout automatically to reduce intermediate reflows.</summary>
    [Description("@#auto")]
    public bool? Auto { get; init; }

    /// <summary>溢出检测选项。The overflow-detection options.</summary>
    [Description("@#boundary")]
    public FloatingBoundary? Boundary { get; init; }

    /// <summary>根边界。The root boundary.</summary>
    [Description("@#rootBoundary")]
    public FloatingRootBoundary? RootBoundary { get; init; }

    /// <summary>检测间距。The detection padding.</summary>
    [Description("@#padding")]
    public Number? Padding { get; init; }

    /// <summary>是否改用另一侧边界进行检测。Whether to detect against the opposite boundary.</summary>
    [Description("@#altBoundary")]
    public bool? AltBoundary { get; init; }
}

/// <summary>
/// <c>hide()</c> 中间件的选项。
/// Options for the <c>hide()</c> middleware.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingHideOptions
{
    /// <summary>参考元素溢出检测选项。Overflow-detection options for the reference element.</summary>
    [Description("@#boundary")]
    public FloatingBoundary? Boundary { get; init; }

    /// <summary>根边界。The root boundary.</summary>
    [Description("@#rootBoundary")]
    public FloatingRootBoundary? RootBoundary { get; init; }

    /// <summary>检测间距。The detection padding.</summary>
    [Description("@#padding")]
    public Number? Padding { get; init; }

    /// <summary>是否改用另一侧边界进行检测。Whether to detect against the opposite boundary.</summary>
    [Description("@#altBoundary")]
    public bool? AltBoundary { get; init; }
}

/// <summary>
/// <c>arrow()</c> 中间件的选项。
/// Options for the <c>arrow()</c> middleware.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingArrowOptions
{
    /// <summary>箭头元素。The arrow element.</summary>
    [Description("@#element")]
    public Element Element { get; init; } = default!;

    /// <summary>箭头与浮层边缘的间距。The padding between the arrow and the floating element edges.</summary>
    [Description("@#padding")]
    public Number? Padding { get; init; }
}

/// <summary>
/// <c>inline()</c> 中间件的选项。
/// Options for the <c>inline()</c> middleware.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingInlineOptions
{
    /// <summary>是否应用基于行的改进定位。Whether to apply line-based positioning improvements.</summary>
    [Description("@#x")]
    public bool? X { get; init; }

    /// <summary>是否应用垂直方向的改进定位。Whether to apply vertical positioning improvements.</summary>
    [Description("@#y")]
    public bool? Y { get; init; }

    /// <summary>溢出检测选项。The overflow-detection options.</summary>
    [Description("@#boundary")]
    public FloatingBoundary? Boundary { get; init; }

    /// <summary>根边界。The root boundary.</summary>
    [Description("@#rootBoundary")]
    public FloatingRootBoundary? RootBoundary { get; init; }

    /// <summary>检测间距。The detection padding.</summary>
    [Description("@#padding")]
    public Number? Padding { get; init; }

    /// <summary>是否改用另一侧边界进行检测。Whether to detect against the opposite boundary.</summary>
    [Description("@#altBoundary")]
    public bool? AltBoundary { get; init; }
}

/// <summary>
/// <c>autoUpdate()</c> 的选项。
/// Options for <c>autoUpdate()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingAutoUpdateOptions
{
    /// <summary>祖先滚动时是否更新位置。Whether to update when an ancestor scrolls.</summary>
    [Description("@#ancestorScroll")]
    public bool? AncestorScroll { get; init; }

    /// <summary>祖先尺寸变化时是否更新位置。Whether to update when an ancestor resizes.</summary>
    [Description("@#ancestorResize")]
    public bool? AncestorResize { get; init; }

    /// <summary>元素尺寸变化时是否更新位置。Whether to update when an element resizes.</summary>
    [Description("@#elementResize")]
    public bool? ElementResize { get; init; }

    /// <summary>布局变化时是否更新位置。Whether to update on layout shift.</summary>
    [Description("@#layoutShift")]
    public bool? LayoutShift { get; init; }

    /// <summary>动画帧中是否更新位置。Whether to update within animation frames.</summary>
    [Description("@#animationFrame")]
    public bool? AnimationFrame { get; init; }
}

/// <summary>
/// <c>whileElementsMounted</c> 回调：在元素挂载期间负责更新定位，返回清理函数。
/// The <c>whileElementsMounted</c> callback: maintains positioning while elements are mounted and returns a cleanup function.
/// </summary>
/// <param name="reference">参考元素。The reference element.</param>
/// <param name="floating">浮层元素。The floating element.</param>
/// <param name="update">手动触发定位更新的回调。Callback that triggers a positioning update.</param>
/// <returns>卸载时调用的清理函数。A cleanup function invoked on unmount.</returns>
public delegate Action FloatingWhileElementsMounted(Element reference, Element floating, Action update);

/// <summary>
/// <c>useFloating()</c> 的选项对象。
/// Options for <c>useFloating()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FloatingUseOptions
{
    /// <summary>浮层的开合状态；关闭时暂停定位。The open/close state of the floating element; positioning is paused when closed.</summary>
    [Description("@#open")]
    public Vue.VueReadonlyRef<bool>? Open { get; init; }

    /// <summary>浮层相对参考元素的摆放方位。Where to place the floating element relative to the reference element.</summary>
    [Description("@#placement")]
    public FloatingPlacement? Placement { get; init; }

    /// <summary>使用的 CSS position 策略。The CSS position strategy to use.</summary>
    [Description("@#strategy")]
    public FloatingStrategy? Strategy { get; init; }

    /// <summary>参与定位的中间件列表。The middleware list used for positioning.</summary>
    [Description("@#middleware")]
    public FloatingMiddleware[]? Middleware { get; init; }

    /// <summary>是否用 transform 而非 top/left 定位。Whether to position with <c>transform</c> instead of <c>top</c>/<c>left</c>.</summary>
    [Description("@#transform")]
    public bool? Transform { get; init; }

    /// <summary>元素挂载期间维持定位的回调。Callback that maintains positioning while the elements are mounted.</summary>
    [Description("@#whileElementsMounted")]
    public FloatingWhileElementsMounted? WhileElementsMounted { get; init; }
}

/// <summary>
/// <c>useFloating()</c> 的返回值：响应式坐标、摆放状态、样式与手动更新入口。
/// The <c>useFloating()</c> return value: reactive coordinates, placement state, styles, and a manual update entry.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class FloatingUseReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected FloatingUseReturn()
    {
    }

    /// <summary>浮层的水平坐标。The floating element's x coordinate.</summary>
    [Description("@#x")]
    public extern Vue.VueReadonlyRef<Number> X { get; }

    /// <summary>浮层的垂直坐标。The floating element's y coordinate.</summary>
    [Description("@#y")]
    public extern Vue.VueReadonlyRef<Number> Y { get; }

    /// <summary>最终采用的摆放方位。The final resolved placement.</summary>
    [Description("@#placement")]
    public extern Vue.VueReadonlyRef<FloatingPlacement> Placement { get; }

    /// <summary>使用的 CSS position 策略。The CSS position strategy in use.</summary>
    [Description("@#strategy")]
    public extern Vue.VueReadonlyRef<FloatingStrategy> Strategy { get; }

    /// <summary>中间件返回的附加数据。Additional data returned by middleware.</summary>
    [Description("@#middlewareData")]
    public extern Vue.VueReadonlyRef<FloatingMiddlewareData> MiddlewareData { get; }

    /// <summary>浮层是否已完成定位。Whether the floating element has been positioned.</summary>
    [Description("@#isPositioned")]
    public extern Vue.VueReadonlyRef<bool> IsPositioned { get; }

    /// <summary>应用于浮层元素的定位样式。The positioning styles to apply to the floating element.</summary>
    [Description("@#floatingStyles")]
    public extern Vue.VueReadonlyRef<FloatingStyles> FloatingStyles { get; }

    /// <summary>手动触发定位更新。Triggers a positioning update manually.</summary>
    [Description("@#update")]
    public extern void Update();
}
