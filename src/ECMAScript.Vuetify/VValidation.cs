using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 验证组件的创作代理，将验证组合式函数暴露为作用域插槽。
/// Vuetify validation authoring proxy exposing the validation composable as a scoped slot.
/// </summary>
[VueLibraryComponent("vuetify/components", "VValidation")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VValidation : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 聚焦。
    /// Focused state.
    /// </summary>
    [Parameter]
    public bool Focused { get; set; }

    /// <summary>
    /// 聚焦变化事件。
    /// Focused changed event.
    /// </summary>
    [Parameter]
    public EventCallback<bool> FocusedChanged { get; set; }

    /// <summary>
    /// 禁用。
    /// Disables validation.
    /// </summary>
    [Parameter]
    public VuetifyNullableBoolean? Disabled { get; set; }

    /// <summary>
    /// 错误。
    /// Error state.
    /// </summary>
    [Parameter]
    public bool Error { get; set; }

    /// <summary>
    /// 错误消息。
    /// Error messages.
    /// </summary>
    [Parameter]
    public VuetifyMessagesValue? ErrorMessages { get; set; }

    /// <summary>
    /// 最大错误数。
    /// Maximum number of errors to display.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxErrors { get; set; }

    /// <summary>
    /// 名称。
    /// Name attribute.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// 标签。
    /// Label text.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// 只读。
    /// Readonly state.
    /// </summary>
    [Parameter]
    public VuetifyNullableBoolean? Readonly { get; set; }

    /// <summary>
    /// 校验规则。
    /// Validation rules.
    /// </summary>
    [Parameter]
    public VuetifyValidationRule[]? Rules { get; set; }

    /// <summary>
    /// 模型值。
    /// Model value.
    /// </summary>
    [Parameter]
    public VueValue? ModelValue { get; set; }

    /// <summary>
    /// 模型值变化事件。
    /// Model value changed event.
    /// </summary>
    [Parameter]
    public EventCallback<VueValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 校验时机。
    /// Validation trigger timing.
    /// </summary>
    [Parameter]
    public VuetifyValidateOn? ValidateOn { get; set; }

    /// <summary>
    /// 校验值。
    /// Validation value.
    /// </summary>
    [Parameter]
    public VueValue? ValidationValue { get; set; }

    /// <summary>
    /// 额外属性。
    /// Additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽。
    /// Default slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VValidationSlotContext>? ChildContent { get; set; }
}
