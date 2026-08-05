using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 虚拟滚动组件的创作代理，用于大型项目集合。
/// Vuetify virtual-scroll authoring proxy for large item collections.
/// </summary>
[VueLibraryComponent("vuetify/components", "VVirtualScroll")]
public sealed class VVirtualScroll : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 高。
    /// Height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 最大高。
    /// Maximum height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 最大宽。
    /// Maximum width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 最小高。
    /// Minimum height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 最小宽。
    /// Minimum width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 宽。
    /// Width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 项高度。
    /// Item height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? ItemHeight { get; set; }

    /// <summary>
    /// 项键。
    /// Item key.
    /// </summary>
    [Parameter]
    public VuetifySelectItemKey? ItemKey { get; set; }

    /// <summary>
    /// 数据项列表。
    /// Data items list.
    /// </summary>
    [Parameter]
    public VueValue[]? Items { get; set; }

    /// <summary>
    /// 无渲染模式。
    /// Renderless mode.
    /// </summary>
    [Parameter]
    public bool Renderless { get; set; }

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
    public RenderFragment<VVirtualScrollSlotContext>? ChildContent { get; set; }
}
