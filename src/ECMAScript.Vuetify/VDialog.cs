using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 对话框创作代理，基于遮罩层的模态内容。
/// Vuetify dialog authoring proxy for overlay-backed modal content.
/// </summary>
[ECMAScript("vuetify/components/VDialog")]
public sealed class VDialog : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 对话框的显示状态。
    /// Whether the dialog is visible.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 显示状态变化时的回调。
    /// Callback when visibility changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否使用绝对定位。
    /// Whether to use absolute positioning.
    /// </summary>
    [Parameter]
    [ECMAScriptName("absolute")]
    public bool Absolute { get; set; }

    /// <summary>
    /// 对话框挂载的目标元素。
    /// Target element to attach the dialog to.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyAttachTarget?；值域为 bool | string | Element。变量用 Attach="@value"，并保持声明的分支类型。字符串用 Attach="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsSelector、value?.AsElement；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("attach")]
    public VuetifyAttachTarget? Attach { get; set; }

    /// <summary>
    /// 是否限制在父容器内。
    /// Whether to contain the dialog within its parent.
    /// </summary>
    [Parameter]
    [ECMAScriptName("contained")]
    public bool Contained { get; set; }

    /// <summary>
    /// 是否禁用对话框。
    /// Whether to disable the dialog.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否立即渲染内容而非懒加载。
    /// Whether to render content eagerly instead of lazily.
    /// </summary>
    [Parameter]
    [ECMAScriptName("eager")]
    public bool Eager { get; set; }

    /// <summary>
    /// 是否全屏显示。
    /// Whether to display fullscreen.
    /// </summary>
    [Parameter]
    [ECMAScriptName("fullscreen")]
    public bool Fullscreen { get; set; }

    /// <summary>
    /// 是否禁用点击动画效果。
    /// Whether to disable the click animation effect.
    /// </summary>
    [Parameter]
    [ECMAScriptName("noClickAnimation")]
    public bool NoClickAnimation { get; set; }

    /// <summary>
    /// 点击外部时是否保持打开。
    /// Whether to remain open when clicking outside.
    /// </summary>
    [Parameter]
    [ECMAScriptName("persistent")]
    public bool Persistent { get; set; }

    /// <summary>
    /// 是否在对话框内保持焦点。
    /// Whether to retain focus within the dialog.
    /// </summary>
    [Parameter]
    [ECMAScriptName("retainFocus")]
    public bool RetainFocus { get; set; } = true;

    /// <summary>
    /// 是否允许内容滚动。
    /// Whether to allow content scrolling.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scrollable")]
    public bool Scrollable { get; set; }

    /// <summary>
    /// 是否在浏览器后退时关闭。
    /// Whether to close on browser back navigation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeOnBack")]
    public bool CloseOnBack { get; set; } = true;

    /// <summary>
    /// 是否点击内容区域时关闭。
    /// Whether to close when clicking the content area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeOnContentClick")]
    public bool CloseOnContentClick { get; set; }

    /// <summary>
    /// 是否点击激活元素时打开。
    /// Whether to open when clicking the activator.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openOnClick")]
    public bool? OpenOnClick { get; set; }

    /// <summary>
    /// 是否聚焦时打开。
    /// Whether to open on focus.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openOnFocus")]
    public bool? OpenOnFocus { get; set; }

    /// <summary>
    /// 是否悬停时打开。
    /// Whether to open on hover.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openOnHover")]
    public bool OpenOnHover { get; set; }

    /// <summary>
    /// 打开延迟时间（毫秒）。
    /// Delay before opening (in milliseconds).
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OpenDelay="@(32)"；变量用 OpenDelay="@value"，无需 double 后缀。字符串用 OpenDelay="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("openDelay")]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 关闭延迟时间（毫秒）。
    /// Delay before closing (in milliseconds).
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 CloseDelay="@(32)"；变量用 CloseDelay="@value"，无需 double 后缀。字符串用 CloseDelay="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("closeDelay")]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 激活元素的属性。
    /// Props for the activator element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activatorProps")]
    public VueProps? ActivatorProps { get; set; }

    /// <summary>
    /// 内容区域的属性。
    /// Props for the content area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("contentProps")]
    public VueProps? ContentProps { get; set; }

    /// <summary>
    /// 内容区域的 CSS 类。
    /// CSS class for the content area.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueClassValue?；值域为 string | string[] | VueProps | VueValue[]。变量用 ContentClass="@value"，并保持声明的分支类型。字符串用 ContentClass="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("contentClass")]
    public VueClassValue? ContentClass { get; set; }

    /// <summary>
    /// 对话框的弹出位置。
    /// Popup location of the dialog.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 对话框的变换原点。
    /// Transform origin of the dialog.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyOriginValue?；值域为 VuetifyLocation | VuetifyOriginMode | string。变量用 Origin="@value"，并保持声明的分支类型。字符串用 Origin="text"；数字字符串保持 string。投影：value?.AsLocation、value?.AsMode、value?.AsCustom；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("origin")]
    public VuetifyOriginValue? Origin { get; set; }

    /// <summary>
    /// 对话框的偏移量。
    /// Offset of the dialog.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyOverlayOffsetValue?；值域为 string | Number | Number[]。数值用 Offset="@(32)"；变量用 Offset="@value"，无需 double 后缀。字符串用 Offset="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("offset")]
    public VuetifyOverlayOffsetValue? Offset { get; set; }

    /// <summary>
    /// 定位策略。
    /// Location strategy.
    /// </summary>
    [Parameter]
    [ECMAScriptName("locationStrategy")]
    public VuetifyLocationStrategy? LocationStrategy { get; set; }

    /// <summary>
    /// 滚动策略。
    /// Scroll strategy.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scrollStrategy")]
    public VuetifyScrollStrategy? ScrollStrategy { get; set; }

    /// <summary>
    /// 遮罩层样式。
    /// Scrim overlay style.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyScrimValue?；值域为 bool | string。变量用 Scrim="@value"，并保持声明的分支类型。字符串用 Scrim="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("scrim")]
    public VuetifyScrimValue? Scrim { get; set; }

    /// <summary>
    /// 组件主题名称。
    /// Component theme name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 过渡动画名称。
    /// Transition animation name.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyTransitionValue?；值域为 bool | string | VueTransitionProps。变量用 Transition="@value"，并保持声明的分支类型。字符串用 Transition="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsProps；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 层叠顺序。
    /// Z-index stacking order.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 ZIndex="@(32)"；变量用 ZIndex="@value"，无需 double 后缀。字符串用 ZIndex="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("zIndex")]
    public VueStringNumberValue? ZIndex { get; set; }

    /// <summary>
    /// 组件的 CSS 类。
    /// CSS class for the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueClassValue?；值域为 string | string[] | VueProps | VueValue[]。变量用 CssClass="@value"，并保持声明的分支类型。字符串用 CssClass="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 组件的内联样式。
    /// Inline style for the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyStyleValue?；值域为 string | VueProps | VuetifyStyleValue[]。变量用 CssStyle="@value"，并保持声明的分支类型。字符串用 CssStyle="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 对话框高度。
    /// Dialog height.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Height="@(32)"；变量用 Height="@value"，无需 double 后缀。字符串用 Height="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 对话框最大宽度。
    /// Maximum width of the dialog.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MaxWidth="@(32)"；变量用 MaxWidth="@value"，无需 double 后缀。字符串用 MaxWidth="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 对话框宽度。
    /// Dialog width.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Width="@(32)"；变量用 Width="@value"，无需 double 后缀。字符串用 Width="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 对话框最大高度。
    /// Maximum height of the dialog.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MaxHeight="@(32)"；变量用 MaxHeight="@value"，无需 double 后缀。字符串用 MaxHeight="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 对话框最小高度。
    /// Minimum height of the dialog.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MinHeight="@(32)"；变量用 MinHeight="@value"，无需 double 后缀。字符串用 MinHeight="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 对话框最小宽度。
    /// Minimum width of the dialog.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MinWidth="@(32)"；变量用 MinWidth="@value"，无需 double 后缀。字符串用 MinWidth="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 遮罩层不透明度。
    /// Overlay opacity.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Opacity="@(32)"；变量用 Opacity="@value"，无需 double 后缀。字符串用 Opacity="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("opacity")]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 对话框的目标定位元素。
    /// Target element for positioning the dialog.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDialogTarget?；值域为 Element | VueComponentPublicInstance | string | Number[]。变量用 Target="@value"，并保持声明的分支类型。字符串用 Target="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsElement、value?.AsComponent、value?.AsString、value?.AsCoordinates；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("target")]
    public VuetifyDialogTarget? Target { get; set; }

    /// <summary>
    /// 激活器目标元素。
    /// Activator target element.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyDialogActivatorTarget?；值域为 Element | VueComponentPublicInstance | string。变量用 ActivatorTarget="@value"，并保持声明的分支类型。字符串用 ActivatorTarget="text"；数字字符串保持 string。投影：value?.AsElement、value?.AsComponent、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("activator")]
    public VuetifyDialogActivatorTarget? ActivatorTarget { get; set; }

    /// <summary>
    /// 进入动画完成后触发的事件回调。
    /// Event callback fired after enter transition completes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onAfterEnter")]
    public EventCallback OnAfterEnter { get; set; }

    /// <summary>
    /// 离开动画完成后触发的事件回调。
    /// Event callback fired after leave transition completes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onAfterLeave")]
    public EventCallback OnAfterLeave { get; set; }

    /// <summary>
    /// 点击对话框外部时触发的事件回调。
    /// Event callback fired when clicking outside the dialog.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onClick:outside")]
    public EventCallback<MouseEvent> OnClickOutside { get; set; }

    /// <summary>
    /// 按键时触发的事件回调。
    /// Event callback fired on keydown.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onKeydown")]
    public EventCallback<KeyboardEvent> OnKeydown { get; set; }

    /// <summary>
    /// 激活器插槽内容。
    /// Slot content for the activator.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activator")]
    public RenderFragment<VDialogActivatorContext>? Activator { get; set; }

    /// <summary>
    /// 附加到组件的额外 HTML 属性。
    /// Additional HTML attributes attached to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认子内容插槽。
    /// Default child content slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
