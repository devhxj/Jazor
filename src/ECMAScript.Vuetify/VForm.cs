using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 表单组件。
/// Vuetify form component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VForm")]
public sealed class VForm : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 是否禁用表单内所有输入控件。
    /// Whether to disable all input controls within the form.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否在首个验证失败时立即停止。
    /// Whether to stop validation on the first failure.
    /// </summary>
    [Parameter]
    public bool FastFail { get; set; }

    /// <summary>
    /// 是否将表单内所有输入控件设为只读。
    /// Whether to mark all input controls within the form as read-only.
    /// </summary>
    [Parameter]
    public bool Readonly { get; set; }

    /// <summary>
    /// 表单验证触发的时机。
    /// When to trigger form validation.
    /// </summary>
    [Parameter]
    public VuetifyValidateOn? ValidateOn { get; set; }

    /// <summary>
    /// 表单的验证状态模型值。
    /// The validation state model value of the form.
    /// </summary>
    [Parameter]
    public bool? ModelValue { get; set; }

    /// <summary>
    /// 表单验证状态变化时的回调。
    /// Callback when the form validation state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool?> ModelValueChanged { get; set; }

    /// <summary>
    /// 应用于表单根元素的 CSS 类。
    /// CSS classes applied to the form root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用于表单根元素的行内样式。
    /// Inline styles applied to the form root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 表单提交时触发的事件回调。
    /// Event callback fired when the form is submitted.
    /// </summary>
    [Parameter]
    public EventCallback<VFormSubmitEvent> OnSubmit { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 表单默认插槽内容。
    /// Default slot content of the form.
    /// </summary>
    [Parameter]
    public RenderFragment<VFormDefaultSlotContext>? ChildContent { get; set; }
}
