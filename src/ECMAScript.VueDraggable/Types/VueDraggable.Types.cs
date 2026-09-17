using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// SortableJS 拖拽事件的通用负载；具体字段随后续绑定按需补充。
/// Generic payload for SortableJS drag events; further fields are added as the binding grows.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class VueDraggableEvent
{
    private VueDraggableEvent()
    {
    }
}

/// <summary>
/// 列表在容器内的排序方向；字符串值与 SortableJS <c>direction</c> 值域一致。
/// The sort direction of a list inside its container; values mirror the SortableJS <c>direction</c> domain.
/// </summary>
[String]
public enum VueDraggableDirection
{
    /// <summary>水平排序。Horizontal sorting.</summary>
    [Description("@#horizontal")]
    Horizontal,

    /// <summary>垂直排序。Vertical sorting.</summary>
    [Description("@#vertical")]
    Vertical
}

/// <summary>
/// 跨列表拖拽时的 group 配置；<c>pull</c>/<c>put</c> 使用 SortableJS 的字符串或布尔值域。
/// Group configuration for cross-list dragging; <c>pull</c>/<c>put</c> use the SortableJS string or boolean domain.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueDraggableGroupOptions
{
    /// <summary>分组名称。The group name.</summary>
    [Description("@#name")]
    public string Name { get; init; } = default!;

    /// <summary>是否允许从本组拉出元素；<c>true</c>/<c>false</c>/<c>"clone"</c>。Whether elements can be pulled from this group; <c>true</c>/<c>false</c>/<c>"clone"</c>.</summary>
    [Description("@#pull")]
    public VueDraggablePullValue? Pull { get; init; }

    /// <summary>是否允许放入元素。Whether elements can be put into this group.</summary>
    [Description("@#put")]
    public VueDraggablePullValue? Put { get; init; }
}

/// <summary>
/// group 的 pull/put 值域：布尔值或 <c>"clone"</c>。
/// The pull/put domain for groups: a boolean or <c>"clone"</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VueDraggablePullValue(bool, string);

/// <summary>
/// 拖拽排序选项；对应 SortableJS <c>Options</c> 与 vue-draggable-plus 的扩展字段。
/// Drag-sort options; mirrors the SortableJS <c>Options</c> plus the vue-draggable-plus extensions.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueDraggableOptions
{
    /// <summary>拖拽动画时长（毫秒）。The drag animation duration in milliseconds.</summary>
    [Description("@#animation")]
    public Number? Animation { get; init; }

    /// <summary>被选中元素的 CSS 类名。The CSS class applied to the chosen element.</summary>
    [Description("@#chosenClass")]
    public string? ChosenClass { get; init; }

    /// <summary>拖拽占位元素的 CSS 类名。The CSS class applied to the drag placeholder.</summary>
    [Description("@#ghostClass")]
    public string? GhostClass { get; init; }

    /// <summary>被拖拽元素的 CSS 类名。The CSS class applied to the dragged element.</summary>
    [Description("@#dragClass")]
    public string? DragClass { get; init; }

    /// <summary>用于读取元素 id 的属性名。The attribute used to read an element's id.</summary>
    [Description("@#dataIdAttr")]
    public string? DataIdAttr { get; init; }

    /// <summary>延迟拖拽的毫秒数。The delay in milliseconds before dragging starts.</summary>
    [Description("@#delay")]
    public Number? Delay { get; init; }

    /// <summary>延迟是否只作用于触摸事件。Whether the delay applies to touch only.</summary>
    [Description("@#delayOnTouchOnly")]
    public bool? DelayOnTouchOnly { get; init; }

    /// <summary>拖拽方向。The drag direction.</summary>
    [Description("@#direction")]
    public VueDraggableDirection? Direction { get; init; }

    /// <summary>是否禁用拖拽。Whether dragging is disabled.</summary>
    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    /// <summary>可拖拽子元素的选择器。The selector for draggable children.</summary>
    [Description("@#draggable")]
    public string? Draggable { get; init; }

    /// <summary>拖拽事件是否冒泡。Whether drag events bubble.</summary>
    [Description("@#dragoverBubble")]
    public bool? DragoverBubble { get; init; }

    /// <summary>放下事件是否冒泡。Whether drop events bubble.</summary>
    [Description("@#dropBubble")]
    public bool? DropBubble { get; init; }

    /// <summary>插入到空列表的触发阈值。The threshold for inserting into an empty list.</summary>
    [Description("@#emptyInsertThreshold")]
    public Number? EmptyInsertThreshold { get; init; }

    /// <summary>回退动画的缓动函数。The easing function for the fallback animation.</summary>
    [Description("@#easing")]
    public string? Easing { get; init; }

    /// <summary>回退元素使用的 CSS 类名。The CSS class for the fallback element.</summary>
    [Description("@#fallbackClass")]
    public string? FallbackClass { get; init; }

    /// <summary>是否把回退元素挂到 body。Whether the fallback element is appended to the body.</summary>
    [Description("@#fallbackOnBody")]
    public bool? FallbackOnBody { get; init; }

    /// <summary>回退判定容差（像素）。The fallback tolerance in pixels.</summary>
    [Description("@#fallbackTolerance")]
    public Number? FallbackTolerance { get; init; }

    /// <summary>回退偏移量。The fallback offset.</summary>
    [Description("@#fallbackOffset")]
    public VueDraggableOffset? FallbackOffset { get; init; }

    /// <summary>不可拖拽元素的选择器。The selector for elements excluded from dragging.</summary>
    [Description("@#filter")]
    public string? Filter { get; init; }

    /// <summary>是否强制使用回退实现。Whether to force the fallback implementation.</summary>
    [Description("@#forceFallback")]
    public bool? ForceFallback { get; init; }

    /// <summary>分组配置。The group configuration.</summary>
    [Description("@#group")]
    public VueDraggableGroupOptions? Group { get; init; }

    /// <summary>拖拽把手的选择器。The selector for the drag handle.</summary>
    [Description("@#handle")]
    public string? Handle { get; init; }

    /// <summary>排序是否允许拖拽。Whether sorting is allowed.</summary>
    [Description("@#sort")]
    public bool? Sort { get; init; }

    /// <summary>交换阈值。The swap threshold.</summary>
    [Description("@#swapThreshold")]
    public Number? SwapThreshold { get; init; }

    /// <summary>触摸开始阈值。The touch-start threshold.</summary>
    [Description("@#touchStartThreshold")]
    public Number? TouchStartThreshold { get; init; }

    /// <summary>是否移除隐藏的克隆元素。Whether hidden clones are removed.</summary>
    [Description("@#removeCloneOnHide")]
    public bool? RemoveCloneOnHide { get; init; }

    /// <summary>被过滤元素是否阻止默认行为。Whether filtered elements prevent the default action.</summary>
    [Description("@#preventOnFilter")]
    public bool? PreventOnFilter { get; init; }

    /// <summary>是否使用反转交换。Whether inverted swapping is used.</summary>
    [Description("@#invertSwap")]
    public bool? InvertSwap { get; init; }

    /// <summary>反转交换阈值。The inverted swap threshold.</summary>
    [Description("@#invertedSwapThreshold")]
    public Number? InvertedSwapThreshold { get; init; }

    /// <summary>克隆元素的回调，用于复制元素数据。The clone callback used to copy element data.</summary>
    [Description("@#clone")]
    public Func<VueDraggableEvent, VueDraggableEvent>? CloneHandler { get; init; }

    /// <summary>是否在挂载后立即初始化。Whether to initialize immediately after mount.</summary>
    [Description("@#immediate")]
    public bool? Immediate { get; init; }

    /// <summary>自定义更新处理；提供后由调用方负责写回列表。Custom update handling; when supplied the caller owns list write-back.</summary>
    [Description("@#customUpdate")]
    public Action<VueDraggableEvent>? CustomUpdate { get; init; }

    /// <summary>拖拽开始。Dragging started.</summary>
    [Description("@#onStart")]
    public Action<VueDraggableEvent>? OnStart { get; init; }

    /// <summary>拖拽结束。Dragging ended.</summary>
    [Description("@#onEnd")]
    public Action<VueDraggableEvent>? OnEnd { get; init; }

    /// <summary>元素从其他列表放入。An element was added from another list.</summary>
    [Description("@#onAdd")]
    public Action<VueDraggableEvent>? OnAdd { get; init; }

    /// <summary>元素被克隆。An element was cloned.</summary>
    [Description("@#onClone")]
    public Action<VueDraggableEvent>? OnClone { get; init; }

    /// <summary>元素被选中。An element was chosen.</summary>
    [Description("@#onChoose")]
    public Action<VueDraggableEvent>? OnChoose { get; init; }

    /// <summary>元素取消选中。An element was unchosen.</summary>
    [Description("@#onUnchoose")]
    public Action<VueDraggableEvent>? OnUnchoose { get; init; }

    /// <summary>列表内排序变化。The sort order changed within the list.</summary>
    [Description("@#onUpdate")]
    public Action<VueDraggableEvent>? OnUpdate { get; init; }

    /// <summary>列表发生任意变化（增/改/删）。Any list change (add, update, or remove).</summary>
    [Description("@#onSort")]
    public Action<VueDraggableEvent>? OnSort { get; init; }

    /// <summary>元素被移出到其他列表。An element was removed into another list.</summary>
    [Description("@#onRemove")]
    public Action<VueDraggableEvent>? OnRemove { get; init; }

    /// <summary>拖拽被过滤元素。A filtered element was dragged.</summary>
    [Description("@#onFilter")]
    public Action<VueDraggableEvent>? OnFilter { get; init; }

    /// <summary>拖拽位置发生变化。The drag position changed.</summary>
    [Description("@#onChange")]
    public Action<VueDraggableEvent>? OnChange { get; init; }
}

/// <summary>
/// 二维偏移量。
/// A two-dimensional offset.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueDraggableOffset
{
    /// <summary>水平偏移。The horizontal offset.</summary>
    [Description("@#x")]
    public Number X { get; init; } = default!;

    /// <summary>垂直偏移。The vertical offset.</summary>
    [Description("@#y")]
    public Number Y { get; init; } = default!;
}

/// <summary>
/// <c>useDraggable()</c> 的返回值：Sortable 实例方法投影与生命周期控制。
/// The <c>useDraggable()</c> return value: the Sortable instance method projections plus lifecycle control.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueDraggableReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueDraggableReturn()
    {
    }

    /// <summary>启动排序；<c>target</c> 省略时使用宿主元素。Starts sorting; the host element is used when <c>target</c> is omitted.</summary>
    /// <param name="target">要排序的元素。The element to sort.</param>
    [Description("@#start")]
    public extern void Start(Element? target = null);

    /// <summary>暂停排序。Pauses sorting.</summary>
    [Description("@#pause")]
    public extern void Pause();

    /// <summary>恢复排序。Resumes sorting.</summary>
    [Description("@#resume")]
    public extern void Resume();

    /// <summary>销毁 Sortable 实例。Destroys the Sortable instance.</summary>
    [Description("@#destroy")]
    public extern void Destroy();
}
