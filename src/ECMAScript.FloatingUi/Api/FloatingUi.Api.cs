using System.ComponentModel;

namespace ECMAScript;

public static partial class FloatingUi
{
    /// <summary>
    /// 创建浮层定位组合式函数；返回响应式坐标、摆放状态、样式与手动更新入口。
    /// Creates the floating-positioning composable, returning reactive coordinates, placement state,
    /// styles, and a manual update entry.
    /// </summary>
    /// <param name="reference">参考元素模板 ref。The reference element template ref.</param>
    /// <param name="floating">浮层元素模板 ref。The floating element template ref.</param>
    /// <param name="options">定位选项。The positioning options.</param>
    [Description("@#useFloating")]
    public extern static FloatingUseReturn UseFloating<TReference, TFloating>(
        Vue.VueReadonlyRef<TReference?> reference,
        Vue.VueReadonlyRef<TFloating?> floating,
        FloatingUseOptions? options = null);

    /// <summary>
    /// 创建箭头中间件，使浮层内的箭头元素与参考元素居中。
    /// Creates the arrow middleware, centering an arrow element inside the floating element against the reference.
    /// </summary>
    [Description("@#arrow")]
    public extern static FloatingMiddleware Arrow(FloatingArrowOptions options);

    /// <summary>
    /// 创建按数值偏移的中间件。
    /// Creates a middleware that offsets the floating element by a number.
    /// </summary>
    [Description("@#offset")]
    public extern static FloatingMiddleware Offset(Number value);

    /// <summary>
    /// 创建按各轴偏移的中间件。
    /// Creates a middleware that offsets the floating element along the main, cross, and alignment axes.
    /// </summary>
    [Description("@#offset")]
    public extern static FloatingMiddleware Offset(FloatingOffsetValue value);

    /// <summary>
    /// 创建将浮层保持在可视范围内的位移中间件。
    /// Creates the shift middleware that keeps the floating element within the viewport.
    /// </summary>
    [Description("@#shift")]
    public extern static FloatingMiddleware Shift(FloatingShiftOptions? options = null);

    /// <summary>
    /// 创建在溢出时翻转摆放方位的中间件。
    /// Creates the flip middleware that changes the placement when overflow occurs.
    /// </summary>
    [Description("@#flip")]
    public extern static FloatingMiddleware Flip(FloatingFlipOptions? options = null);

    /// <summary>
    /// 创建按可用空间调整浮层尺寸的中间件。
    /// Creates the size middleware that resizes the floating element to the available space.
    /// </summary>
    [Description("@#size")]
    public extern static FloatingMiddleware Size(FloatingSizeOptions? options = null);

    /// <summary>
    /// 创建自动选择最佳摆放方位的中间件。
    /// Creates the auto-placement middleware that chooses the best-fitting placement.
    /// </summary>
    [Description("@#autoPlacement")]
    public extern static FloatingMiddleware AutoPlacement(FloatingAutoPlacementOptions? options = null);

    /// <summary>
    /// 创建在浮层被裁剪时提供可见性数据的中间件。
    /// Creates the hide middleware that reports whether the floating element is clipped.
    /// </summary>
    [Description("@#hide")]
    public extern static FloatingMiddleware Hide(FloatingHideOptions? options = null);

    /// <summary>
    /// 创建针对多行内联参考元素的改进定位中间件。
    /// Creates the inline middleware for improved positioning against multi-line inline references.
    /// </summary>
    [Description("@#inline")]
    public extern static FloatingMiddleware Inline(FloatingInlineOptions? options = null);

    /// <summary>
    /// 创建 shift 的位移限制器。
    /// Creates the <c>shift</c> limiter.
    /// </summary>
    [Description("@#limitShift")]
    public extern static FloatingLimiter LimitShift();

    /// <summary>
    /// 在元素挂载期间自动更新浮层位置；返回解绑函数。
    /// Automatically updates the floating position while the elements are mounted and returns a cleanup function.
    /// </summary>
    /// <param name="reference">参考元素。The reference element.</param>
    /// <param name="floating">浮层元素；尚未挂载时为 null。The floating element, or null before it is mounted.</param>
    /// <param name="update">位置更新回调。The position-update callback.</param>
    /// <param name="options">自动更新选项。The auto-update options.</param>
    /// <returns>调用后停止自动更新的清理函数。A cleanup function that stops automatic updates when invoked.</returns>
    [Description("@#autoUpdate")]
    public extern static Action AutoUpdate(Element reference, Element? floating, Action update, FloatingAutoUpdateOptions? options = null);

    /// <summary>
    /// 计算将浮层放在参考元素旁所需的坐标。
    /// Computes the coordinates required to place the floating element next to the reference element.
    /// </summary>
    /// <param name="reference">参考元素。The reference element.</param>
    /// <param name="floating">浮层元素。The floating element.</param>
    /// <param name="options">计算选项。The compute options.</param>
    /// <returns>Promise 包装的定位结果。Promise-wrapped positioning result.</returns>
    [Description("@#computePosition")]
    public extern static IPromise<FloatingComputePositionReturn> ComputePosition(
        Element reference,
        Element floating,
        FloatingComputePositionConfig? options = null);
}
