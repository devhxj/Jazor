using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 表单组件。
/// Vuetify form component.
/// </summary>
[ECMAScript("vuetify/components/VForm")]
public sealed class VForm : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 是否禁用表单内所有输入控件。
    /// Whether to disable all input controls within the form.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否在首个验证失败时立即停止。
    /// Whether to stop validation on the first failure.
    /// </summary>
    [Parameter]
    [ECMAScriptName("fastFail")]
    public bool FastFail { get; set; }

    /// <summary>
    /// 是否将表单内所有输入控件设为只读。
    /// Whether to mark all input controls within the form as read-only.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 表单验证触发的时机。
    /// When to trigger form validation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("validateOn")]
    public VuetifyValidateOn? ValidateOn { get; set; }

    /// <summary>
    /// 表单的验证状态模型值。
    /// The validation state model value of the form.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public bool? ModelValue { get; set; }

    /// <summary>
    /// 表单验证状态变化时的回调。
    /// Callback when the form validation state changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<bool?> ModelValueChanged { get; set; }

    /// <summary>
    /// 应用于表单根元素的 CSS 类。
    /// CSS classes applied to the form root element.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueClassValue?；值域为 string | string[] | VueProps | VueValue[]。变量用 CssClass="@value"，并保持声明的分支类型。字符串用 CssClass="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用于表单根元素的行内样式。
    /// Inline styles applied to the form root element.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyStyleValue?；值域为 string | VueProps | VuetifyStyleValue[]。变量用 CssStyle="@value"，并保持声明的分支类型。字符串用 CssStyle="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsProps、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 表单提交时触发的事件回调。
    /// Event callback fired when the form is submitted.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onSubmit")]
    public EventCallback<VFormSubmitEvent> OnSubmit { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 表单默认插槽内容。
    /// Default slot content of the form.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VFormDefaultSlotContext>? ChildContent { get; set; }
}
