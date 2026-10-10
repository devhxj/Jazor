using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 验证组件的创作代理，将验证组合式函数暴露为作用域插槽。
/// Vuetify validation authoring proxy exposing the validation composable as a scoped slot.
/// </summary>
[ECMAScript("vuetify/components/VValidation")]
public sealed class VValidation : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 聚焦。
    /// Focused state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("focused")]
    public bool Focused { get; set; }

    /// <summary>
    /// 聚焦变化事件。
    /// Focused changed event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:focused")]
    public EventCallback<bool> FocusedChanged { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables validation.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyNullableBoolean?；值域为 bool。变量用 Disabled="@value"，并保持声明的分支类型。投影：value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("disabled")]
    public VuetifyNullableBoolean? Disabled { get; set; }

    /// <summary>
    /// 错误。
    /// Error state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("error")]
    public bool Error { get; set; }

    /// <summary>
    /// 错误消息。
    /// Error messages.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMessagesValue?；值域为 string | string[]。变量用 ErrorMessages="@value"，并保持声明的分支类型。字符串用 ErrorMessages="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("errorMessages")]
    public VuetifyMessagesValue? ErrorMessages { get; set; }

    /// <summary>
    /// 最大错误数。
    /// Maximum number of errors to display.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MaxErrors="@(32)"；变量用 MaxErrors="@value"，无需 double 后缀。字符串用 MaxErrors="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("maxErrors")]
    public VueStringNumberValue? MaxErrors { get; set; }

    /// <summary>
    /// 名称。
    /// Name attribute.
    /// </summary>
    [Parameter]
    [ECMAScriptName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 标签。
    /// Label text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 只读。
    /// Readonly state.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyNullableBoolean?；值域为 bool。变量用 Readonly="@value"，并保持声明的分支类型。投影：value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("readonly")]
    public VuetifyNullableBoolean? Readonly { get; set; }

    /// <summary>
    /// 校验规则。
    /// Validation rules.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rules")]
    public VuetifyValidationRule[]? Rules { get; set; }

    /// <summary>
    /// 模型值。
    /// Model value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VueValue? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化事件。
    /// Model value changed event.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VueValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 校验时机。
    /// Validation trigger timing.
    /// </summary>
    [Parameter]
    [ECMAScriptName("validateOn")]
    public VuetifyValidateOn? ValidateOn { get; set; }

    /// <summary>
    /// 校验值。
    /// Validation value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("validationValue")]
    public VueValue? ValidationValue { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VValidationSlotContext>? ChildContent { get; set; }
}
