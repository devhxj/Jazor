using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 消息条组件的编写代理。
/// Vuetify snackbar authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSnackbar")]
[VueLibraryEmit(nameof(OnClickOutside), Name = "click:outside")]
public sealed class VSnackbar : ComponentBase
{
    /// <summary>
    /// 消息条的显示状态。
    /// Controls whether the snackbar is visible.
    /// </summary>
    [Parameter]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 显示状态变化时触发的回调。
    /// Callback invoked when the visibility state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 消息条的视觉变体。
    /// Visual variant of the snackbar.
    /// </summary>
    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 消息条相对于锚点的偏移量。
    /// Offset of the snackbar relative to its anchor.
    /// </summary>
    [Parameter]
    public VuetifyOverlayOffsetValue? Offset { get; set; }

    /// <summary>
    /// 是否使用绝对定位。
    /// Whether to use absolute positioning.
    /// </summary>
    [Parameter]
    public bool Absolute { get; set; }

    /// <summary>
    /// 消息条出现的位置。
    /// Position where the snackbar appears.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 消息条动画的变换原点。
    /// Transform origin of the snackbar animation.
    /// </summary>
    [Parameter]
    public VuetifyOriginValue? Origin { get; set; }

    /// <summary>
    /// 消息条的高度。
    /// Height of the snackbar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 消息条的宽度。
    /// Width of the snackbar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 消息条的背景颜色。
    /// Background color of the snackbar.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 消息条的最大高度。
    /// Maximum height of the snackbar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 消息条的最大宽度。
    /// Maximum width of the snackbar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 消息条的最小高度。
    /// Minimum height of the snackbar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 消息条的最小宽度。
    /// Minimum width of the snackbar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 消息条遮罩层的不透明度。
    /// Opacity of the snackbar overlay.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 消息条的定位方式。
    /// Positioning strategy of the snackbar.
    /// </summary>
    [Parameter]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 消息条的过渡动画。
    /// Transition animation of the snackbar.
    /// </summary>
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 消息条的 z-index 层级。
    /// Z-index level of the snackbar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? ZIndex { get; set; }

    /// <summary>
    /// 应用于组件的 CSS 类。
    /// CSS classes applied to the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用于组件的内联样式。
    /// Inline styles applied to the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 消息条的文本内容。
    /// Text content of the snackbar.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// 是否在首次渲染时强制加载内容。
    /// Whether to eagerly mount the content on first render.
    /// </summary>
    [Parameter]
    public bool Eager { get; set; }

    /// <summary>
    /// 是否禁用消息条。
    /// Whether the snackbar is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 消息条自动关闭的延迟毫秒数。
    /// Duration in milliseconds before the snackbar auto-closes.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Timeout { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 是否使用垂直布局。
    /// Whether to use a vertical layout.
    /// </summary>
    [Parameter]
    public bool Vertical { get; set; }

    /// <summary>
    /// 是否显示自动关闭的倒计时指示器。
    /// Whether to show the auto-close countdown timer.
    /// </summary>
    [Parameter]
    public VuetifyBooleanStringValue? Timer { get; set; }

    /// <summary>
    /// 消息条定位的目标元素。
    /// Target element the snackbar positions itself against.
    /// </summary>
    [Parameter]
    public VuetifyOverlayTarget? Target { get; set; }

    /// <summary>
    /// 消息条的位置策略。
    /// Location strategy used by the snackbar.
    /// </summary>
    [Parameter]
    public VuetifyLocationStrategy? LocationStrategy { get; set; }

    /// <summary>
    /// 消息条的圆角大小。
    /// Border radius of the snackbar.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    public bool Tile { get; set; }

    /// <summary>
    /// 关闭消息条的延迟毫秒数。
    /// Delay in milliseconds before closing the snackbar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 打开消息条的延迟毫秒数。
    /// Delay in milliseconds before opening the snackbar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 激活器目标元素。
    /// Activator target element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activator")]
    public VuetifyOverlayActivatorTarget? ActivatorTarget { get; set; }

    /// <summary>
    /// 应用于激活器的属性。
    /// Props applied to the activator element.
    /// </summary>
    [Parameter]
    public VueProps? ActivatorProps { get; set; }

    /// <summary>
    /// 是否在点击时打开消息条。
    /// Whether to open the snackbar on click.
    /// </summary>
    [Parameter]
    public bool OpenOnClick { get; set; }

    /// <summary>
    /// 是否在悬停时打开消息条。
    /// Whether to open the snackbar on hover.
    /// </summary>
    [Parameter]
    public bool OpenOnHover { get; set; }

    /// <summary>
    /// 是否在聚焦时打开消息条。
    /// Whether to open the snackbar on focus.
    /// </summary>
    [Parameter]
    public bool OpenOnFocus { get; set; }

    /// <summary>
    /// 是否在点击内容时关闭消息条。
    /// Whether to close the snackbar when content is clicked.
    /// </summary>
    [Parameter]
    public bool CloseOnContentClick { get; set; }

    /// <summary>
    /// 是否在浏览器后退时关闭消息条。
    /// Whether to close the snackbar on browser back navigation.
    /// </summary>
    [Parameter]
    public bool CloseOnBack { get; set; }

    /// <summary>
    /// 是否将消息条限制在父容器内。
    /// Whether to contain the snackbar within its parent.
    /// </summary>
    [Parameter]
    public bool Contained { get; set; }

    /// <summary>
    /// 应用于内容区域的 CSS 类。
    /// CSS classes applied to the content area.
    /// </summary>
    [Parameter]
    public VueClassValue? ContentClass { get; set; }

    /// <summary>
    /// 应用于内容区域的属性。
    /// Props applied to the content area.
    /// </summary>
    [Parameter]
    public VueProps? ContentProps { get; set; }

    /// <summary>
    /// 消息条挂载的容器目标。
    /// Container target where the snackbar is attached.
    /// </summary>
    [Parameter]
    public VuetifyAttachTarget? Attach { get; set; }

    /// <summary>
    /// 是否允许多行文本。
    /// Whether to allow multi-line text.
    /// </summary>
    [Parameter]
    public bool MultiLine { get; set; }

    /// <summary>
    /// 进入动画完成后触发的回调。
    /// Callback invoked after the enter transition completes.
    /// </summary>
    [Parameter]
    public EventCallback OnAfterEnter { get; set; }

    /// <summary>
    /// 离开动画完成后触发的回调。
    /// Callback invoked after the leave transition completes.
    /// </summary>
    [Parameter]
    public EventCallback OnAfterLeave { get; set; }

    /// <summary>
    /// 点击消息条外部时触发的回调。
    /// Callback invoked when clicking outside the snackbar.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEvent> OnClickOutside { get; set; }

    /// <summary>
    /// 按键时触发的回调。
    /// Callback invoked on keydown events.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEvent> OnKeydown { get; set; }

    /// <summary>
    /// 捕获未匹配的额外属性。
    /// Captures unmatched additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 激活器插槽，用于自定义触发元素。
    /// Activator slot for customizing the trigger element.
    /// </summary>
    [Parameter]
    public RenderFragment<VOverlayActivatorContext>? Activator { get; set; }

    /// <summary>
    /// 默认插槽，消息条的主体内容。
    /// Default slot for the snackbar body content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 文本插槽，消息条的文本区域。
    /// Text slot for the snackbar text area.
    /// </summary>
    [Parameter]
    public RenderFragment? TextContent { get; set; }

    /// <summary>
    /// 操作插槽，消息条的操作按钮区域。
    /// Actions slot for the snackbar action buttons area.
    /// </summary>
    [Parameter]
    public RenderFragment<VSnackbarActionsSlotContext>? Actions { get; set; }
}
