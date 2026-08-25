using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 消息条队列组件的编写代理，用于顺序显示通知。
/// Vuetify snackbar-queue authoring proxy for sequential notifications.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSnackbarQueue")]
public sealed class VSnackbarQueue : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 消息队列中待显示的通知列表。
    /// List of pending notifications in the queue.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifySnackbarQueueMessages? ModelValue { get; set; }

    /// <summary>
    /// 通知列表变化时触发的回调。
    /// Callback invoked when the notification list changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifySnackbarQueueMessages?> ModelValueChanged { get; set; }

    /// <summary>
    /// 消息条的视觉变体。
    /// Visual variant of the snackbar queue.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 消息条相对于锚点的偏移量。
    /// Offset of the snackbar relative to its anchor.
    /// </summary>
    [Parameter]
    [ECMAScriptName("offset")]
    public VuetifyOverlayOffsetValue? Offset { get; set; }

    /// <summary>
    /// 是否使用绝对定位。
    /// Whether to use absolute positioning.
    /// </summary>
    [Parameter]
    [ECMAScriptName("absolute")]
    public bool Absolute { get; set; }

    /// <summary>
    /// 消息条出现的位置。
    /// Position where the snackbar appears.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 消息条动画的变换原点。
    /// Transform origin of the snackbar animation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("origin")]
    public VuetifyOriginValue? Origin { get; set; }

    /// <summary>
    /// 消息条的高度。
    /// Height of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 消息条的宽度。
    /// Width of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 消息条的背景颜色。
    /// Background color of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 消息条的最大高度。
    /// Maximum height of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 消息条的最大宽度。
    /// Maximum width of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 消息条的最小高度。
    /// Minimum height of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 消息条的最小宽度。
    /// Minimum width of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 消息条遮罩层的不透明度。
    /// Opacity of the snackbar overlay.
    /// </summary>
    [Parameter]
    [ECMAScriptName("opacity")]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 消息条的定位方式。
    /// Positioning strategy of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("position")]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 消息条的过渡动画。
    /// Transition animation of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 消息条的 z-index 层级。
    /// Z-index level of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("zIndex")]
    public VueStringNumberValue? ZIndex { get; set; }

    /// <summary>
    /// 消息条的文本内容。
    /// Text content of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 是否在首次渲染时强制加载内容。
    /// Whether to eagerly mount the content on first render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("eager")]
    public bool Eager { get; set; }

    /// <summary>
    /// 是否禁用消息条。
    /// Whether the snackbar is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 消息条自动关闭的延迟毫秒数。
    /// Duration in milliseconds before the snackbar auto-closes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("timeout")]
    public VueStringNumberValue? Timeout { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 是否使用垂直布局。
    /// Whether to use a vertical layout.
    /// </summary>
    [Parameter]
    [ECMAScriptName("vertical")]
    public bool Vertical { get; set; }

    /// <summary>
    /// 是否显示自动关闭的倒计时指示器。
    /// Whether to show the auto-close countdown timer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("timer")]
    public VuetifyBooleanStringValue? Timer { get; set; }

    /// <summary>
    /// 消息条的位置策略。
    /// Location strategy used by the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("locationStrategy")]
    public VuetifyLocationStrategy? LocationStrategy { get; set; }

    /// <summary>
    /// 消息条的圆角大小。
    /// Border radius of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 关闭消息条的延迟毫秒数。
    /// Delay in milliseconds before closing the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeDelay")]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 打开消息条的延迟毫秒数。
    /// Delay in milliseconds before opening the snackbar.
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
    /// 是否在点击时打开消息条。
    /// Whether to open the snackbar on click.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openOnClick")]
    public bool OpenOnClick { get; set; }

    /// <summary>
    /// 是否在悬停时打开消息条。
    /// Whether to open the snackbar on hover.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openOnHover")]
    public bool OpenOnHover { get; set; }

    /// <summary>
    /// 是否在聚焦时打开消息条。
    /// Whether to open the snackbar on focus.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openOnFocus")]
    public bool OpenOnFocus { get; set; }

    /// <summary>
    /// 是否在点击内容时关闭消息条。
    /// Whether to close the snackbar when content is clicked.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeOnContentClick")]
    public bool CloseOnContentClick { get; set; }

    /// <summary>
    /// 是否在浏览器后退时关闭消息条。
    /// Whether to close the snackbar on browser back navigation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeOnBack")]
    public bool CloseOnBack { get; set; }

    /// <summary>
    /// 是否将消息条限制在父容器内。
    /// Whether to contain the snackbar within its parent.
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
    /// 消息条挂载的容器目标。
    /// Container target where the snackbar is attached.
    /// </summary>
    [Parameter]
    [ECMAScriptName("attach")]
    public VuetifyAttachTarget? Attach { get; set; }

    /// <summary>
    /// 是否允许多行文本。
    /// Whether to allow multi-line text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("multiLine")]
    public bool MultiLine { get; set; }

    /// <summary>
    /// 是否显示关闭按钮。
    /// Whether to show the close button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closable")]
    public VuetifyBooleanStringValue? Closable { get; set; }

    /// <summary>
    /// 关闭按钮的文本标签。
    /// Text label for the close button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeText")]
    public string? CloseText { get; set; }

    /// <summary>
    /// 捕获未匹配的额外属性。
    /// Captures unmatched additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽，消息条队列的主体内容。
    /// Default slot for the snackbar queue body content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VSnackbarQueueSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 文本插槽，消息条的文本区域。
    /// Text slot for the snackbar text area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("text")]
    public RenderFragment<VSnackbarQueueSlotContext>? TextContent { get; set; }

    /// <summary>
    /// 操作插槽，消息条的操作按钮区域。
    /// Actions slot for the snackbar action buttons area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("actions")]
    public RenderFragment<VSnackbarQueueActionsSlotContext>? Actions { get; set; }
}
