using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 虚拟滚动组件的创作代理，用于大型项目集合。
/// Vuetify virtual-scroll authoring proxy for large item collections.
/// </summary>
[VueLibraryComponent("vuetify/components", "VVirtualScroll")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VVirtualScroll : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    [Parameter]
    public VueStringNumberValue? ItemHeight { get; set; }

    [Parameter]
    public VuetifySelectItemKey? ItemKey { get; set; }

    [Parameter]
    public VueValue[]? Items { get; set; }

    [Parameter]
    public bool Renderless { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    [Parameter]
    public RenderFragment<VVirtualScrollSlotContext>? ChildContent { get; set; }
}
