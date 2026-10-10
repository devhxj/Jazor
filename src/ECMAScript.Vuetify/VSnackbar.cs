using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 消息条组件的编写代理。
/// Vuetify snackbar authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VSnackbar")]
public sealed class VSnackbar : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 消息条的显示状态。
    /// Controls whether the snackbar is visible.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 显示状态变化时触发的回调。
    /// Callback invoked when the visibility state changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 消息条的视觉变体。
    /// Visual variant of the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 消息条相对于锚点的偏移量。
    /// Offset of the snackbar relative to its anchor.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyOverlayOffsetValue?；值域为 string | Number | Number[]。数值用 Offset="@(32)"；变量用 Offset="@value"，无需 double 后缀。字符串用 Offset="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// <remarks data-authoring="types">C# union VuetifyOriginValue?；值域为 VuetifyLocation | VuetifyOriginMode | string。变量用 Origin="@value"，并保持声明的分支类型。字符串用 Origin="text"；数字字符串保持 string。投影：value?.AsLocation、value?.AsMode、value?.AsCustom；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("origin")]
    public VuetifyOriginValue? Origin { get; set; }

    /// <summary>
    /// 消息条的高度。
    /// Height of the snackbar.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Height="@(32)"；变量用 Height="@value"，无需 double 后缀。字符串用 Height="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 消息条的宽度。
    /// Width of the snackbar.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Width="@(32)"；变量用 Width="@value"，无需 double 后缀。字符串用 Width="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MaxHeight="@(32)"；变量用 MaxHeight="@value"，无需 double 后缀。字符串用 MaxHeight="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 消息条的最大宽度。
    /// Maximum width of the snackbar.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MaxWidth="@(32)"；变量用 MaxWidth="@value"，无需 double 后缀。字符串用 MaxWidth="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 消息条的最小高度。
    /// Minimum height of the snackbar.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MinHeight="@(32)"；变量用 MinHeight="@value"，无需 double 后缀。字符串用 MinHeight="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 消息条的最小宽度。
    /// Minimum width of the snackbar.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MinWidth="@(32)"；变量用 MinWidth="@value"，无需 double 后缀。字符串用 MinWidth="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 消息条遮罩层的不透明度。
    /// Opacity of the snackbar overlay.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Opacity="@(32)"；变量用 Opacity="@value"，无需 double 后缀。字符串用 Opacity="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// <remarks data-authoring="types">C# union VuetifyTransitionValue?；值域为 bool | string | VueTransitionProps。变量用 Transition="@value"，并保持声明的分支类型。字符串用 Transition="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsProps；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 消息条的 z-index 层级。
    /// Z-index level of the snackbar.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 ZIndex="@(32)"；变量用 ZIndex="@value"，无需 double 后缀。字符串用 ZIndex="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("zIndex")]
    public VueStringNumberValue? ZIndex { get; set; }

    /// <summary>
    /// 应用于组件的 CSS 类。
    /// CSS classes applied to the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueClassValue?；值域为 string | string[] | VueProps | VueValue[]。变量用 CssClass="@value"，并保持声明的分支类型。字符串用 CssClass="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用于组件的内联样式。
    /// Inline styles applied to the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyStyleValue?；值域为 string | VueProps | VuetifyStyleValue[]。变量用 CssStyle="@value"，并保持声明的分支类型。字符串用 CssStyle="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

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
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Timeout="@(32)"；变量用 Timeout="@value"，无需 double 后缀。字符串用 Timeout="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// <remarks data-authoring="types">C# union VuetifyBooleanStringValue?；值域为 bool | string。变量用 Timer="@value"，并保持声明的分支类型。字符串用 Timer="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("timer")]
    public VuetifyBooleanStringValue? Timer { get; set; }

    /// <summary>
    /// 消息条定位的目标元素。
    /// Target element the snackbar positions itself against.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyOverlayTarget?；值域为 Element | VueComponentPublicInstance | string | Number[]。变量用 Target="@value"，并保持声明的分支类型。字符串用 Target="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsElement、value?.AsComponent、value?.AsString、value?.AsCoordinates；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("target")]
    public VuetifyOverlayTarget? Target { get; set; }

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
    /// <remarks data-authoring="types">C# union VuetifyRoundedValue?；值域为 bool | Number | string。数值用 Rounded="@(32)"；变量用 Rounded="@value"，无需 double 后缀。字符串用 Rounded="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 CloseDelay="@(32)"；变量用 CloseDelay="@value"，无需 double 后缀。字符串用 CloseDelay="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("closeDelay")]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 打开消息条的延迟毫秒数。
    /// Delay in milliseconds before opening the snackbar.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OpenDelay="@(32)"；变量用 OpenDelay="@value"，无需 double 后缀。字符串用 OpenDelay="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("openDelay")]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 激活器目标元素。
    /// Activator target element.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyOverlayActivatorTarget?；值域为 Element | VueComponentPublicInstance | string。变量用 ActivatorTarget="@value"，并保持声明的分支类型。字符串用 ActivatorTarget="text"；数字字符串保持 string。投影：value?.AsElement、value?.AsComponent、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("activator")]
    public VuetifyOverlayActivatorTarget? ActivatorTarget { get; set; }

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
    /// 应用于内容区域的 CSS 类。
    /// CSS classes applied to the content area.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueClassValue?；值域为 string | string[] | VueProps | VueValue[]。变量用 ContentClass="@value"，并保持声明的分支类型。字符串用 ContentClass="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("contentClass")]
    public VueClassValue? ContentClass { get; set; }

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
    /// <remarks data-authoring="types">C# union VuetifyAttachTarget?；值域为 bool | string | Element。变量用 Attach="@value"，并保持声明的分支类型。字符串用 Attach="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsSelector、value?.AsElement；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// 进入动画完成后触发的回调。
    /// Callback invoked after the enter transition completes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onAfterEnter")]
    public EventCallback OnAfterEnter { get; set; }

    /// <summary>
    /// 离开动画完成后触发的回调。
    /// Callback invoked after the leave transition completes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onAfterLeave")]
    public EventCallback OnAfterLeave { get; set; }

    /// <summary>
    /// 点击消息条外部时触发的回调。
    /// Callback invoked when clicking outside the snackbar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onClick:outside")]
    public EventCallback<MouseEvent> OnClickOutside { get; set; }

    /// <summary>
    /// 按键时触发的回调。
    /// Callback invoked on keydown events.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onKeydown")]
    public EventCallback<KeyboardEvent> OnKeydown { get; set; }

    /// <summary>
    /// 捕获未匹配的额外属性。
    /// Captures unmatched additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 激活器插槽，用于自定义触发元素。
    /// Activator slot for customizing the trigger element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activator")]
    public RenderFragment<VOverlayActivatorContext>? Activator { get; set; }

    /// <summary>
    /// 默认插槽，消息条的主体内容。
    /// Default slot for the snackbar body content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 文本插槽，消息条的文本区域。
    /// Text slot for the snackbar text area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("text")]
    public RenderFragment? TextContent { get; set; }

    /// <summary>
    /// 操作插槽，消息条的操作按钮区域。
    /// Actions slot for the snackbar action buttons area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("actions")]
    public RenderFragment<VSnackbarActionsSlotContext>? Actions { get; set; }
}
