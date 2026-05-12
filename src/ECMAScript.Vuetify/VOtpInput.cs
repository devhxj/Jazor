using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 一次性密码输入组件。
/// Vuetify OTP input component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VOtpInput")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
public sealed class VOtpInput : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VueStringNumberValue? Length { get; set; }

    [Parameter]
    public bool Autofocus { get; set; }

    [Parameter]
    public string? Divider { get; set; }

    [Parameter]
    public bool FocusAll { get; set; }

    [Parameter]
    public VuetifyBooleanStringValue? Loading { get; set; }

    [Parameter]
    public VuetifyFieldVariant? Variant { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public VuetifyInputType? Type { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
