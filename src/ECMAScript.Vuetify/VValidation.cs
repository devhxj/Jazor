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
    [Parameter]
    public bool Focused { get; set; }

    [Parameter]
    public EventCallback<bool> FocusedChanged { get; set; }

    [Parameter]
    public VuetifyNullableBoolean? Disabled { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public VuetifyMessagesValue? ErrorMessages { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxErrors { get; set; }

    [Parameter]
    public string? Name { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public VuetifyNullableBoolean? Readonly { get; set; }

    [Parameter]
    public VuetifyValidationRule[]? Rules { get; set; }

    [Parameter]
    public VueValue? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VueValue?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyValidateOn? ValidateOn { get; set; }

    [Parameter]
    public VueValue? ValidationValue { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VValidationSlotContext>? ChildContent { get; set; }
}
