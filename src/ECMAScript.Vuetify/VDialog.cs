using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 对话框创作代理，基于遮罩层的模态内容。
/// Vuetify dialog authoring proxy for overlay-backed modal content.
/// </summary>
[VueLibraryComponent("vuetify/components", "VDialog")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryProp(nameof(CssClass), Name = "class")]
[VueLibraryProp(nameof(CssStyle), Name = "style")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(AfterEnter), VueEmitKind.LibrarySpecific, Name = "afterEnter")]
[VueLibraryEmit(nameof(AfterLeave), VueEmitKind.LibrarySpecific, Name = "afterLeave")]
[VueLibraryEmit(nameof(ClickOutside), VueEmitKind.LibrarySpecific, Name = "click:outside")]
[VueLibraryEmit(nameof(Keydown), VueEmitKind.LibrarySpecific, Name = "keydown")]
[VueLibraryProp(nameof(ZIndex), Name = "zIndex")]
[VueLibraryProp(nameof(ActivatorTarget), Name = "activator")]
[VueLibrarySlot(nameof(Activator), Name = "activator")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VDialog : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 对话框的显示状态。
    /// Whether the dialog is visible.
    /// </summary>
    [Parameter]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 显示状态变化时的回调。
    /// Callback when visibility changes.
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
    /// 对话框挂载的目标元素。
    /// Target element to attach the dialog to.
    /// </summary>
    [Parameter]
    public VuetifyAttachTarget? Attach { get; set; }

    /// <summary>
    /// 是否限制在父容器内。
    /// Whether to contain the dialog within its parent.
    /// </summary>
    [Parameter]
    public bool Contained { get; set; }

    /// <summary>
    /// 是否禁用对话框。
    /// Whether to disable the dialog.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否立即渲染内容而非懒加载。
    /// Whether to render content eagerly instead of lazily.
    /// </summary>
    [Parameter]
    public bool Eager { get; set; }

    /// <summary>
    /// 是否全屏显示。
    /// Whether to display fullscreen.
    /// </summary>
    [Parameter]
    public bool Fullscreen { get; set; }

    /// <summary>
    /// 是否禁用点击动画效果。
    /// Whether to disable the click animation effect.
    /// </summary>
    [Parameter]
    public bool NoClickAnimation { get; set; }

    /// <summary>
    /// 点击外部时是否保持打开。
    /// Whether to remain open when clicking outside.
    /// </summary>
    [Parameter]
    public bool Persistent { get; set; }

    /// <summary>
    /// 是否在对话框内保持焦点。
    /// Whether to retain focus within the dialog.
    /// </summary>
    [Parameter]
    public bool RetainFocus { get; set; } = true;

    /// <summary>
    /// 是否允许内容滚动。
    /// Whether to allow content scrolling.
    /// </summary>
    [Parameter]
    public bool Scrollable { get; set; }

    /// <summary>
    /// 是否在浏览器后退时关闭。
    /// Whether to close on browser back navigation.
    /// </summary>
    [Parameter]
    public bool CloseOnBack { get; set; } = true;

    /// <summary>
    /// 是否点击内容区域时关闭。
    /// Whether to close when clicking the content area.
    /// </summary>
    [Parameter]
    public bool CloseOnContentClick { get; set; }

    /// <summary>
    /// 是否点击激活元素时打开。
    /// Whether to open when clicking the activator.
    /// </summary>
    [Parameter]
    public bool? OpenOnClick { get; set; }

    /// <summary>
    /// 是否聚焦时打开。
    /// Whether to open on focus.
    /// </summary>
    [Parameter]
    public bool? OpenOnFocus { get; set; }

    /// <summary>
    /// 是否悬停时打开。
    /// Whether to open on hover.
    /// </summary>
    [Parameter]
    public bool OpenOnHover { get; set; }

    /// <summary>
    /// 打开延迟时间（毫秒）。
    /// Delay before opening (in milliseconds).
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 关闭延迟时间（毫秒）。
    /// Delay before closing (in milliseconds).
    /// </summary>
    [Parameter]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 激活元素的属性。
    /// Props for the activator element.
    /// </summary>
    [Parameter]
    public VueProps? ActivatorProps { get; set; }

    /// <summary>
    /// 内容区域的属性。
    /// Props for the content area.
    /// </summary>
    [Parameter]
    public VueProps? ContentProps { get; set; }

    /// <summary>
    /// 内容区域的 CSS 类。
    /// CSS class for the content area.
    /// </summary>
    [Parameter]
    public VueClassValue? ContentClass { get; set; }

    /// <summary>
    /// 对话框的弹出位置。
    /// Popup location of the dialog.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 对话框的变换原点。
    /// Transform origin of the dialog.
    /// </summary>
    [Parameter]
    public VuetifyOriginValue? Origin { get; set; }

    /// <summary>
    /// 对话框的偏移量。
    /// Offset of the dialog.
    /// </summary>
    [Parameter]
    public VuetifyOverlayOffsetValue? Offset { get; set; }

    /// <summary>
    /// 定位策略。
    /// Location strategy.
    /// </summary>
    [Parameter]
    public VuetifyLocationStrategy? LocationStrategy { get; set; }

    /// <summary>
    /// 滚动策略。
    /// Scroll strategy.
    /// </summary>
    [Parameter]
    public VuetifyScrollStrategy? ScrollStrategy { get; set; }

    /// <summary>
    /// 遮罩层样式。
    /// Scrim overlay style.
    /// </summary>
    [Parameter]
    public VuetifyScrimValue? Scrim { get; set; }

    /// <summary>
    /// 组件主题名称。
    /// Component theme name.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 过渡动画名称。
    /// Transition animation name.
    /// </summary>
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 层叠顺序。
    /// Z-index stacking order.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? ZIndex { get; set; }

    /// <summary>
    /// 组件的 CSS 类。
    /// CSS class for the component.
    /// </summary>
    [Parameter]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 组件的内联样式。
    /// Inline style for the component.
    /// </summary>
    [Parameter]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 对话框高度。
    /// Dialog height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 对话框最大宽度。
    /// Maximum width of the dialog.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 对话框宽度。
    /// Dialog width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 对话框最大高度。
    /// Maximum height of the dialog.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 对话框最小高度。
    /// Minimum height of the dialog.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 对话框最小宽度。
    /// Minimum width of the dialog.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 遮罩层不透明度。
    /// Overlay opacity.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 对话框的目标定位元素。
    /// Target element for positioning the dialog.
    /// </summary>
    [Parameter]
    public VuetifyDialogTarget? Target { get; set; }

    /// <summary>
    /// 激活器目标元素。
    /// Activator target element.
    /// </summary>
    [Parameter]
    public VuetifyDialogActivatorTarget? ActivatorTarget { get; set; }

    /// <summary>
    /// 进入动画完成后触发的事件回调。
    /// Event callback fired after enter transition completes.
    /// </summary>
    [Parameter]
    public EventCallback AfterEnter { get; set; }

    /// <summary>
    /// 离开动画完成后触发的事件回调。
    /// Event callback fired after leave transition completes.
    /// </summary>
    [Parameter]
    public EventCallback AfterLeave { get; set; }

    /// <summary>
    /// 点击对话框外部时触发的事件回调。
    /// Event callback fired when clicking outside the dialog.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEvent> ClickOutside { get; set; }

    /// <summary>
    /// 按键时触发的事件回调。
    /// Event callback fired on keydown.
    /// </summary>
    [Parameter]
    public EventCallback<KeyboardEvent> Keydown { get; set; }

    /// <summary>
    /// 激活器插槽内容。
    /// Slot content for the activator.
    /// </summary>
    [Parameter]
    public RenderFragment<VDialogActivatorContext>? Activator { get; set; }

    /// <summary>
    /// 附加到组件的额外 HTML 属性。
    /// Additional HTML attributes attached to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认子内容插槽。
    /// Default child content slot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
