using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 输入创作代理，用于组合验证、消息和控件插槽。
/// Vuetify input authoring proxy for composing validation, messages, and control slots.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VInput")]
public sealed class VInput : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 输入控件的唯一标识符。
    /// Unique identifier for the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// 输入控件的名称属性。
    /// Name attribute of the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 输入控件的标签文本。
    /// Label text of the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 组件的密度样式，调整垂直间距。
    /// Component density style that adjusts vertical spacing.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 组件的文本方向。
    /// Text direction of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("direction")]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 组件的宽度。
    /// Width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 组件的最小宽度。
    /// Minimum width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件的最大宽度。
    /// Maximum width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 前置图标。
    /// Prepend icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prependIcon")]
    public VuetifyIconValue? PrependIcon { get; set; }

    /// <summary>
    /// 后置图标。
    /// Append icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("appendIcon")]
    public VuetifyIconValue? AppendIcon { get; set; }

    /// <summary>
    /// 处于非活跃状态时的颜色。
    /// Color when the component is in an inactive state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("baseColor")]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 图标的颜色。
    /// Color of the icons.
    /// </summary>
    [Parameter]
    [ECMAScriptName("iconColor")]
    public VuetifyIconColorValue? IconColor { get; set; }

    /// <summary>
    /// 是否将前缀/后缀图标居中对齐。
    /// Whether to center align prepend/append icons.
    /// </summary>
    [Parameter]
    [ECMAScriptName("centerAffix")]
    public bool CenterAffix { get; set; }

    /// <summary>
    /// 是否显示发光效果。
    /// Whether to show a glow effect.
    /// </summary>
    [Parameter]
    [ECMAScriptName("glow")]
    public bool Glow { get; set; }

    /// <summary>
    /// 是否隐藏数字输入的微调按钮。
    /// Whether to hide the spin buttons of number inputs.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideSpinButtons")]
    public bool HideSpinButtons { get; set; }

    /// <summary>
    /// 输入控件的提示文本。
    /// Hint text for the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hint")]
    public string? Hint { get; set; }

    /// <summary>
    /// 是否在未聚焦时持续显示提示文本。
    /// Whether to persistently show the hint text when not focused.
    /// </summary>
    [Parameter]
    [ECMAScriptName("persistentHint")]
    public bool PersistentHint { get; set; }

    /// <summary>
    /// 显示在输入控件下方的消息列表。
    /// Messages displayed below the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("messages")]
    public VuetifyMessagesValue? Messages { get; set; }

    /// <summary>
    /// 是否隐藏提示详情区域。
    /// Whether to hide the details area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideDetails")]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    /// <summary>
    /// 输入控件的聚焦状态。
    /// Focused state of the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("focused")]
    public bool Focused { get; set; }

    /// <summary>
    /// 聚焦状态变更时触发的回调。
    /// Callback invoked when the focused state changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:focused")]
    public EventCallback<bool> FocusedChanged { get; set; }

    /// <summary>
    /// 是否禁用输入控件。
    /// Whether to disable the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public VuetifyNullableBoolean? Disabled { get; set; }

    /// <summary>
    /// 是否将输入控件设为只读。
    /// Whether to make the input control read-only.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public VuetifyNullableBoolean? Readonly { get; set; }

    /// <summary>
    /// 是否将输入控件置于错误状态。
    /// Whether to put the input control in an error state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("error")]
    public bool Error { get; set; }

    /// <summary>
    /// 错误状态下显示的消息列表。
    /// Messages displayed when in an error state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("errorMessages")]
    public VuetifyMessagesValue? ErrorMessages { get; set; }

    /// <summary>
    /// 最大显示错误数量。
    /// Maximum number of errors to display.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxErrors")]
    public VueStringNumberValue? MaxErrors { get; set; }

    /// <summary>
    /// 输入值的验证规则数组。
    /// Array of validation rules for the input value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rules")]
    public VuetifyValidationRule[]? Rules { get; set; }

    /// <summary>
    /// 触发验证的时机。
    /// When to trigger validation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("validateOn")]
    public VuetifyValidateOn? ValidateOn { get; set; }

    /// <summary>
    /// 用于验证的值。
    /// Value used for validation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("validationValue")]
    public VueValue? ValidationValue { get; set; }

    /// <summary>
    /// 输入控件的双向绑定值。
    /// Two-way bound value of the input control.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VueValue? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变更时触发的回调。
    /// Callback invoked when the bound value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VueValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 点击前置图标时触发的回调。
    /// Callback invoked when the prepend icon is clicked.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onClick:prepend")]
    public EventCallback<MouseEvent> OnPrependClick { get; set; }

    /// <summary>
    /// 点击后置图标时触发的回调。
    /// Callback invoked when the append icon is clicked.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onClick:append")]
    public EventCallback<MouseEvent> OnAppendClick { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容，提供输入槽位上下文。
    /// Default slot content, providing input slot context.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VInputSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 前置插槽内容。
    /// Prepend slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prepend")]
    public RenderFragment<VInputSlotContext>? Prepend { get; set; }

    /// <summary>
    /// 后置插槽内容。
    /// Append slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("append")]
    public RenderFragment<VInputSlotContext>? Append { get; set; }

    /// <summary>
    /// 详情区域插槽内容。
    /// Details slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("details")]
    public RenderFragment<VInputDetailsSlotContext>? Details { get; set; }

    /// <summary>
    /// 单条消息插槽内容。
    /// Individual message slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("message")]
    public RenderFragment<VMessagesMessageSlotContext>? Message { get; set; }
}
