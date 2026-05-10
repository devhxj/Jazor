using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VCombobox")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryProp(nameof(SelectedValue), VuePropKind.Model, Name = "modelValue", AcceptsBinding = true)]
[VueLibraryEmit(nameof(SelectedValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
public sealed class VCombobox : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public VuetifySelectItems? Items { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemTitle { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemValue { get; set; }

    [Parameter]
    public VuetifySelectItemPropsSelector? ItemProps { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public bool Chips { get; set; }

    [Parameter]
    public bool ReturnObject { get; set; }

    [Parameter]
    public bool Clearable { get; set; }

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public VueDictionary? MenuProps { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VuetifyFieldVariant? Variant { get; set; }

    [Parameter]
    public string? NoDataText { get; set; }

    [Parameter]
    public VuetifyAutoSelectFirstValue? AutoSelectFirst { get; set; }

    [Parameter]
    public bool ClearOnSelect { get; set; } = true;

    [Parameter]
    public string[]? Delimiters { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? Hint { get; set; }

    [Parameter]
    public bool PersistentHint { get; set; }

    [Parameter]
    public VuetifyHideDetailsValue? HideDetails { get; set; }

    [Parameter]
    public VuetifyMessagesValue? Messages { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifySelectModelValue? SelectedValue { get; set; }

    [Parameter]
    public EventCallback<VuetifySelectModelValue?> SelectedValueChanged { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
