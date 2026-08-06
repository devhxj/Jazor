using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 遮罩层组件。
/// Vuetify overlay component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VOverlay")]
[VueLibraryEmit(nameof(OnClickOutside), Name = "click:outside")]
public sealed class VOverlay : ComponentBase
{
    /// <summary>
    /// 遮罩层是否可见。
    /// Whether the overlay is visible.
    /// </summary>
    [Parameter]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 遮罩层可见性变化时的回调。
    /// Callback when the overlay visibility changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否使用绝对定位。
    /// Whether to use absolute positioning.
    /// </summary>
    [Parameter]
    public bool Absolute { get; set; }

    /// <summary>
    /// 遮罩层挂载的目标元素。
    /// Target element to attach the overlay to.
    /// </summary>
    [Parameter]
    public VuetifyAttachTarget? Attach { get; set; }

    /// <summary>
    /// 是否将遮罩层限制在父容器内。
    /// Whether to contain the overlay within its parent.
    /// </summary>
    [Parameter]
    public bool Contained { get; set; }

    /// <summary>
    /// 是否禁用遮罩层。
    /// Whether the overlay is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否在初始渲染时立即加载内容。
    /// Whether to eagerly load content on initial render.
    /// </summary>
    [Parameter]
    public bool Eager { get; set; }

    /// <summary>
    /// 点击遮罩层时是否禁用动画。
    /// Whether to disable the click animation on the overlay.
    /// </summary>
    [Parameter]
    public bool NoClickAnimation { get; set; }

    /// <summary>
    /// 点击遮罩层外部时是否保持打开。
    /// Whether to persist the overlay when clicking outside.
    /// </summary>
    [Parameter]
    public bool Persistent { get; set; }

    /// <summary>
    /// 是否在浏览器后退时关闭遮罩层。
    /// Whether to close the overlay on browser back navigation.
    /// </summary>
    [Parameter]
    public bool CloseOnBack { get; set; }

    /// <summary>
    /// 是否在点击内容区域时关闭遮罩层。
    /// Whether to close the overlay when clicking the content area.
    /// </summary>
    [Parameter]
    public bool CloseOnContentClick { get; set; }

    /// <summary>
    /// 是否在点击遮罩层外部时关闭。
    /// Whether to close the overlay when clicking outside.
    /// </summary>
    [Parameter]
    public bool CloseOnClick { get; set; }

    /// <summary>
    /// 是否在点击时打开遮罩层。
    /// Whether to open the overlay on click.
    /// </summary>
    [Parameter]
    public bool OpenOnClick { get; set; }

    /// <summary>
    /// 是否在获得焦点时打开遮罩层。
    /// Whether to open the overlay on focus.
    /// </summary>
    [Parameter]
    public bool OpenOnFocus { get; set; }

    /// <summary>
    /// 是否在悬停时打开遮罩层。
    /// Whether to open the overlay on hover.
    /// </summary>
    [Parameter]
    public bool OpenOnHover { get; set; }

    /// <summary>
    /// 打开遮罩层的延迟毫秒数。
    /// Open delay in milliseconds.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 关闭遮罩层的延迟毫秒数。
    /// Close delay in milliseconds.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 应用于激活器元素的属性。
    /// Props applied to the activator element.
    /// </summary>
    [Parameter]
    public VueProps? ActivatorProps { get; set; }

    /// <summary>
    /// 应用于内容区域的属性。
    /// Props applied to the content area.
    /// </summary>
    [Parameter]
    public VueProps? ContentProps { get; set; }

    /// <summary>
    /// 遮罩层的唯一标识符。
    /// Unique identifier of the overlay.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// 遮罩层内容的位置。
    /// Location of the overlay content.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 遮罩层过渡动画的起始位置。
    /// Origin point of the overlay transition.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Origin { get; set; }

    /// <summary>
    /// 遮罩层相对于激活器的偏移距离。
    /// Offset distance from the activator.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Offset { get; set; }

    /// <summary>
    /// 遮罩层的定位策略。
    /// Location strategy of the overlay.
    /// </summary>
    [Parameter]
    public VuetifyLocationStrategy? LocationStrategy { get; set; }

    /// <summary>
    /// 遮罩层的滚动策略。
    /// Scroll strategy of the overlay.
    /// </summary>
    [Parameter]
    public VuetifyScrollStrategy? ScrollStrategy { get; set; }

    /// <summary>
    /// 遮罩层的背景遮罩样式。
    /// Scrim style of the overlay background.
    /// </summary>
    [Parameter]
    public VuetifyScrimValue? Scrim { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 遮罩层的过渡动画。
    /// Transition animation of the overlay.
    /// </summary>
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 遮罩层的 z-index 层级。
    /// Z-index level of the overlay.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? ZIndex { get; set; }

    /// <summary>
    /// 遮罩层的高度。
    /// Height of the overlay.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 遮罩层的最大高度。
    /// Maximum height of the overlay.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 遮罩层的最大宽度。
    /// Maximum width of the overlay.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 遮罩层的最小高度。
    /// Minimum height of the overlay.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 遮罩层的最小宽度。
    /// Minimum width of the overlay.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 遮罩层的宽度。
    /// Width of the overlay.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 遮罩层进入后触发的事件回调。
    /// Event callback fired after the overlay enters.
    /// </summary>
    [Parameter]
    public EventCallback OnAfterEnter { get; set; }

    /// <summary>
    /// 遮罩层离开后触发的事件回调。
    /// Event callback fired after the overlay leaves.
    /// </summary>
    [Parameter]
    public EventCallback OnAfterLeave { get; set; }

    /// <summary>
    /// 点击遮罩层外部时触发的事件回调。
    /// Event callback fired when clicking outside the overlay.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEvent> OnClickOutside { get; set; }

    /// <summary>
    /// 遮罩层上按键时触发的事件回调。
    /// Event callback fired on keydown within the overlay.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEvent> OnKeydown { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 激活器插槽内容。
    /// Slot content for the activator element.
    /// </summary>
    [Parameter]
    public RenderFragment<VOverlayActivatorContext>? Activator { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
