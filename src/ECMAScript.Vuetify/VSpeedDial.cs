using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 快速拨号组件的编写代理，基于 VMenu/VOverlay 构建。
/// Vuetify speed-dial authoring proxy built on the VMenu/VOverlay surface.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSpeedDial")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryProp(nameof(ZIndex), Name = "zIndex")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Activator), Name = "activator")]
public sealed class VSpeedDial : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 快速拨号的显示状态。
    /// Controls whether the speed dial is open.
    /// </summary>
    [Parameter]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 显示状态变化时触发的回调。
    /// Callback invoked when the open state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 快速拨号相对于锚点的偏移量。
    /// Offset of the speed dial relative to its anchor.
    /// </summary>
    [Parameter]
    public VuetifyOverlayOffsetValue? Offset { get; set; }

    /// <summary>
    /// 快速拨号出现的位置。
    /// Position where the speed dial appears.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 快速拨号动画的变换原点。
    /// Transform origin of the speed dial animation.
    /// </summary>
    [Parameter]
    public VuetifyOriginValue? Origin { get; set; }

    /// <summary>
    /// 快速拨号的高度。
    /// Height of the speed dial.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 快速拨号的宽度。
    /// Width of the speed dial.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 快速拨号的最大高度。
    /// Maximum height of the speed dial.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 快速拨号的最大宽度。
    /// Maximum width of the speed dial.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 快速拨号的最小高度。
    /// Minimum height of the speed dial.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 快速拨号的最小宽度。
    /// Minimum width of the speed dial.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 遮罩层的不透明度。
    /// Opacity of the overlay.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 快速拨号的过渡动画。
    /// Transition animation of the speed dial.
    /// </summary>
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 快速拨号的 z-index 层级。
    /// Z-index level of the speed dial.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? ZIndex { get; set; }

    /// <summary>
    /// 是否在首次渲染时强制加载内容。
    /// Whether to eagerly mount the content on first render.
    /// </summary>
    [Parameter]
    public bool Eager { get; set; }

    /// <summary>
    /// 是否禁用快速拨号。
    /// Whether the speed dial is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 是否在点击外部后保持打开。
    /// Whether the speed dial persists when clicking outside.
    /// </summary>
    [Parameter]
    public bool Persistent { get; set; }

    /// <summary>
    /// 快速拨号的位置策略。
    /// Location strategy used by the speed dial.
    /// </summary>
    [Parameter]
    public VuetifyLocationStrategy? LocationStrategy { get; set; }

    /// <summary>
    /// 快速拨号的滚动策略。
    /// Scroll strategy used by the speed dial.
    /// </summary>
    [Parameter]
    public VuetifyScrollStrategy? ScrollStrategy { get; set; }

    /// <summary>
    /// 关闭快速拨号的延迟毫秒数。
    /// Delay in milliseconds before closing the speed dial.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 打开快速拨号的延迟毫秒数。
    /// Delay in milliseconds before opening the speed dial.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 应用于激活器的属性。
    /// Props applied to the activator element.
    /// </summary>
    [Parameter]
    public VueProps? ActivatorProps { get; set; }

    /// <summary>
    /// 是否在点击时打开快速拨号。
    /// Whether to open the speed dial on click.
    /// </summary>
    [Parameter]
    public bool OpenOnClick { get; set; }

    /// <summary>
    /// 是否在悬停时打开快速拨号。
    /// Whether to open the speed dial on hover.
    /// </summary>
    [Parameter]
    public bool OpenOnHover { get; set; }

    /// <summary>
    /// 是否在聚焦时打开快速拨号。
    /// Whether to open the speed dial on focus.
    /// </summary>
    [Parameter]
    public bool OpenOnFocus { get; set; }

    /// <summary>
    /// 是否在点击内容时关闭快速拨号。
    /// Whether to close the speed dial when content is clicked.
    /// </summary>
    [Parameter]
    public bool CloseOnContentClick { get; set; }

    /// <summary>
    /// 是否在浏览器后退时关闭快速拨号。
    /// Whether to close the speed dial on browser back navigation.
    /// </summary>
    [Parameter]
    public bool CloseOnBack { get; set; }

    /// <summary>
    /// 是否将快速拨号限制在父容器内。
    /// Whether to contain the speed dial within its parent.
    /// </summary>
    [Parameter]
    public bool Contained { get; set; }

    /// <summary>
    /// 应用于内容区域的属性。
    /// Props applied to the content area.
    /// </summary>
    [Parameter]
    public VueProps? ContentProps { get; set; }

    /// <summary>
    /// 是否禁用点击动画。
    /// Whether to disable the click animation.
    /// </summary>
    [Parameter]
    public bool NoClickAnimation { get; set; }

    /// <summary>
    /// 遮罩层的蒙版配置。
    /// Scrim configuration of the overlay.
    /// </summary>
    [Parameter]
    public VuetifyScrimValue? Scrim { get; set; }

    /// <summary>
    /// 快速拨号挂载的容器目标。
    /// Container target where the speed dial is attached.
    /// </summary>
    [Parameter]
    public VuetifyAttachTarget? Attach { get; set; }

    /// <summary>
    /// 组件的唯一标识符。
    /// Unique identifier of the component.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// 是否以子菜单模式显示。
    /// Whether to display in submenu mode.
    /// </summary>
    [Parameter]
    public bool Submenu { get; set; }

    /// <summary>
    /// 捕获未匹配的额外属性。
    /// Captures unmatched additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽，快速拨号的内容区域。
    /// Default slot for the speed dial content area.
    /// </summary>
    [Parameter]
    public RenderFragment<VSpeedDialDefaultSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 激活器插槽，用于自定义触发元素。
    /// Activator slot for customizing the trigger element.
    /// </summary>
    [Parameter]
    public RenderFragment<VOverlayActivatorContext>? Activator { get; set; }
}
