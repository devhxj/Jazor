using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 组合框组件创作代理。
/// Vuetify combobox component authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCombobox")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryProp(nameof(SelectedValue), VuePropKind.Model, Name = "modelValue", AcceptsBinding = true)]
[VueLibraryEmit(nameof(SelectedValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(FocusedChanged), VueEmitKind.ModelUpdate, Name = "update:focused")]
[VueLibraryEmit(nameof(MenuChanged), VueEmitKind.ModelUpdate, Name = "update:menu")]
[VueLibraryEmit(nameof(SearchChanged), VueEmitKind.ModelUpdate, Name = "update:search")]
[VueLibrarySlot(nameof(Item), Name = "item")]
[VueLibrarySlot(nameof(Chip), Name = "chip")]
[VueLibrarySlot(nameof(Selection), Name = "selection")]
[VueLibrarySlot(nameof(PrependItem), Name = "prepend-item")]
[VueLibrarySlot(nameof(AppendItem), Name = "append-item")]
[VueLibrarySlot(nameof(NoData), Name = "no-data")]
public sealed class VCombobox : VSelectLikeComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyAutoSelectFirstValue? AutoSelectFirst { get; set; }

    [Parameter]
    public bool ClearOnSelect { get; set; } = true;

    [Parameter]
    public string[]? Delimiters { get; set; }

    [Parameter]
    public string? Search { get; set; }

    [Parameter]
    public EventCallback<string?> SearchChanged { get; set; }

    [Parameter]
    public VuetifyFilterFunction? CustomFilter { get; set; }

    [Parameter]
    public VuetifyFilterKeyFunctions? CustomKeyFilter { get; set; }

    [Parameter]
    public VuetifyFilterKeys? FilterKeys { get; set; }

    [Parameter]
    public VuetifyFilterMode? FilterMode { get; set; }

    [Parameter]
    public bool NoFilter { get; set; }

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
