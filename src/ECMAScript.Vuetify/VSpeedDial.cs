using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 快速拨号组件的编写代理，基于 VMenu/VOverlay 构建。
/// Vuetify speed-dial authoring proxy built on the VMenu/VOverlay surface.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VSpeedDial")]
public sealed class VSpeedDial : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 快速拨号的显示状态。
    /// Controls whether the speed dial is open.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 显示状态变化时触发的回调。
    /// Callback invoked when the open state changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 快速拨号相对于锚点的偏移量。
    /// Offset of the speed dial relative to its anchor.
    /// </summary>
    [Parameter]
    [ECMAScriptName("offset")]
    public VuetifyOverlayOffsetValue? Offset { get; set; }

    /// <summary>
    /// 快速拨号出现的位置。
    /// Position where the speed dial appears.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 快速拨号动画的变换原点。
    /// Transform origin of the speed dial animation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("origin")]
    public VuetifyOriginValue? Origin { get; set; }

    /// <summary>
    /// 快速拨号的高度。
    /// Height of the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 快速拨号的宽度。
    /// Width of the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 快速拨号的最大高度。
    /// Maximum height of the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 快速拨号的最大宽度。
    /// Maximum width of the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 快速拨号的最小高度。
    /// Minimum height of the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 快速拨号的最小宽度。
    /// Minimum width of the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 遮罩层的不透明度。
    /// Opacity of the overlay.
    /// </summary>
    [Parameter]
    [ECMAScriptName("opacity")]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 快速拨号的过渡动画。
    /// Transition animation of the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 快速拨号的 z-index 层级。
    /// Z-index level of the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("zIndex")]
    public VueStringNumberValue? ZIndex { get; set; }

    /// <summary>
    /// 是否在首次渲染时强制加载内容。
    /// Whether to eagerly mount the content on first render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("eager")]
    public bool Eager { get; set; }

    /// <summary>
    /// 是否禁用快速拨号。
    /// Whether the speed dial is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 是否在点击外部后保持打开。
    /// Whether the speed dial persists when clicking outside.
    /// </summary>
    [Parameter]
    [ECMAScriptName("persistent")]
    public bool Persistent { get; set; }

    /// <summary>
    /// 快速拨号的位置策略。
    /// Location strategy used by the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("locationStrategy")]
    public VuetifyLocationStrategy? LocationStrategy { get; set; }

    /// <summary>
    /// 快速拨号的滚动策略。
    /// Scroll strategy used by the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scrollStrategy")]
    public VuetifyScrollStrategy? ScrollStrategy { get; set; }

    /// <summary>
    /// 关闭快速拨号的延迟毫秒数。
    /// Delay in milliseconds before closing the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeDelay")]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 打开快速拨号的延迟毫秒数。
    /// Delay in milliseconds before opening the speed dial.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openDelay")]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 应用于激活器的属性。
    /// Props applied to the activator element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activatorProps")]
    public VueProps? ActivatorProps { get; set; }

    /// <summary>
    /// 是否在点击时打开快速拨号。
    /// Whether to open the speed dial on click.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openOnClick")]
    public bool OpenOnClick { get; set; }

    /// <summary>
    /// 是否在悬停时打开快速拨号。
    /// Whether to open the speed dial on hover.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openOnHover")]
    public bool OpenOnHover { get; set; }

    /// <summary>
    /// 是否在聚焦时打开快速拨号。
    /// Whether to open the speed dial on focus.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openOnFocus")]
    public bool OpenOnFocus { get; set; }

    /// <summary>
    /// 是否在点击内容时关闭快速拨号。
    /// Whether to close the speed dial when content is clicked.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeOnContentClick")]
    public bool CloseOnContentClick { get; set; }

    /// <summary>
    /// 是否在浏览器后退时关闭快速拨号。
    /// Whether to close the speed dial on browser back navigation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeOnBack")]
    public bool CloseOnBack { get; set; }

    /// <summary>
    /// 是否将快速拨号限制在父容器内。
    /// Whether to contain the speed dial within its parent.
    /// </summary>
    [Parameter]
    [ECMAScriptName("contained")]
    public bool Contained { get; set; }

    /// <summary>
    /// 应用于内容区域的属性。
    /// Props applied to the content area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("contentProps")]
    public VueProps? ContentProps { get; set; }

    /// <summary>
    /// 是否禁用点击动画。
    /// Whether to disable the click animation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("noClickAnimation")]
    public bool NoClickAnimation { get; set; }

    /// <summary>
    /// 遮罩层的蒙版配置。
    /// Scrim configuration of the overlay.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scrim")]
    public VuetifyScrimValue? Scrim { get; set; }

    /// <summary>
    /// 快速拨号挂载的容器目标。
    /// Container target where the speed dial is attached.
    /// </summary>
    [Parameter]
    [ECMAScriptName("attach")]
    public VuetifyAttachTarget? Attach { get; set; }

    /// <summary>
    /// 组件的唯一标识符。
    /// Unique identifier of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 是否以子菜单模式显示。
    /// Whether to display in submenu mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("submenu")]
    public bool Submenu { get; set; }

    /// <summary>
    /// 捕获未匹配的额外属性。
    /// Captures unmatched additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽，快速拨号的内容区域。
    /// Default slot for the speed dial content area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VSpeedDialDefaultSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 激活器插槽，用于自定义触发元素。
    /// Activator slot for customizing the trigger element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activator")]
    public RenderFragment<VOverlayActivatorContext>? Activator { get; set; }
}
