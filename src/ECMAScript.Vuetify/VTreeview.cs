using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify labs treeview authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VTreeview")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(ActivatedChanged), VueEmitKind.ModelUpdate, Name = "update:activated")]
[VueLibraryEmit(nameof(SelectedChanged), VueEmitKind.ModelUpdate, Name = "update:selected")]
[VueLibraryEmit(nameof(OpenedChanged), VueEmitKind.ModelUpdate, Name = "update:opened")]
[VueLibraryEmit(nameof(OpenClicked), VueEmitKind.LibrarySpecific, Name = "click:open")]
[VueLibraryEmit(nameof(SelectClicked), VueEmitKind.LibrarySpecific, Name = "click:select")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Prepend), Name = "prepend")]
[VueLibrarySlot(nameof(Append), Name = "append")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
[VueLibrarySlot(nameof(SubtitleContent), Name = "subtitle")]
[VueLibrarySlot(nameof(ItemContent), Name = "item")]
[VueLibrarySlot(nameof(Header), Name = "header")]
[VueLibrarySlot(nameof(Divider), Name = "divider")]
[VueLibrarySlot(nameof(Subheader), Name = "subheader")]
public sealed class VTreeview : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VuetifyTreeviewValues? ModelValue { get; set; }

    [Parameter]
    public EventCallback<VuetifyTreeviewValues?> ModelValueChanged { get; set; }

    [Parameter]
    public VuetifyTreeviewItems? Items { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemTitle { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemValue { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemChildren { get; set; }

    [Parameter]
    public VuetifySelectItemPropsSelector? ItemProps { get; set; }

    [Parameter]
    public bool ReturnObject { get; set; }

    [Parameter]
    public VuetifyTreeviewValues? Activated { get; set; }

    [Parameter]
    public EventCallback<VuetifyTreeviewValues?> ActivatedChanged { get; set; }

    [Parameter]
    public VuetifyTreeviewValues? Selected { get; set; }

    [Parameter]
    public EventCallback<VuetifyTreeviewValues?> SelectedChanged { get; set; }

    [Parameter]
    public VuetifyTreeviewValues? Opened { get; set; }

    [Parameter]
    public EventCallback<VuetifyTreeviewValues?> OpenedChanged { get; set; }

    [Parameter]
    public bool Mandatory { get; set; }

    [Parameter]
    public bool Activatable { get; set; }

    [Parameter]
    public bool Selectable { get; set; }

    [Parameter]
    public VuetifyTreeviewActiveStrategyValue? ActiveStrategy { get; set; }

    [Parameter]
    public VuetifyTreeviewSelectStrategyValue? SelectStrategy { get; set; }

    [Parameter]
    public VuetifyTreeviewLoadChildrenCallback? LoadChildren { get; set; }

    [Parameter]
    public bool? OpenOnClick { get; set; }

    [Parameter]
    public bool OpenAll { get; set; }

    [Parameter]
    public string? Search { get; set; }

    [Parameter]
    public VueClassValue? Class { get; set; }

    [Parameter]
    public VuetifyStyleValue? Style { get; set; }

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
    public VuetifyIconValue? CollapseIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? ExpandIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? IndeterminateIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? FalseIcon { get; set; }

    [Parameter]
    public VuetifyIconValue? TrueIcon { get; set; }

    [Parameter]
    public string? LoadingIcon { get; set; }

    [Parameter]
    public string? SelectedColor { get; set; }

    [Parameter]
    public string? ActiveColor { get; set; }

    [Parameter]
    public string? ActiveClass { get; set; }

    [Parameter]
    public string? BaseColor { get; set; }

    [Parameter]
    public string? BgColor { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    [Parameter]
    public VuetifyDensity? Density { get; set; }

    [Parameter]
    public VuetifyListLines? Lines { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Slim { get; set; }

    [Parameter]
    public bool Fluid { get; set; }

    [Parameter]
    public string? Theme { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    [Parameter]
    public bool Tile { get; set; }

    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VuetifySelectValueComparator? ValueComparator { get; set; }

    [Parameter]
    public EventCallback<VuetifyTreeviewClickPayload> OpenClicked { get; set; }

    [Parameter]
    public EventCallback<VuetifyTreeviewClickPayload> SelectClicked { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<VTreeviewNodeSlotContext>? Prepend { get; set; }

    [Parameter]
    public RenderFragment<VTreeviewNodeSlotContext>? Append { get; set; }

    [Parameter]
    public RenderFragment<VTreeviewTitleSlotContext>? TitleContent { get; set; }

    [Parameter]
    public RenderFragment<VTreeviewSubtitleSlotContext>? SubtitleContent { get; set; }

    [Parameter]
    public RenderFragment<VTreeviewItemSlotContext>? ItemContent { get; set; }

    [Parameter]
    public RenderFragment<VTreeviewStructuralItemSlotContext>? Header { get; set; }

    [Parameter]
    public RenderFragment<VTreeviewStructuralItemSlotContext>? Divider { get; set; }

    [Parameter]
    public RenderFragment<VTreeviewStructuralItemSlotContext>? Subheader { get; set; }
}
