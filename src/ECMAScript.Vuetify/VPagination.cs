using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// 首批 Vuetify 分页组件桩，用于 RazorVue 编写。
/// First-wave Vuetify pagination stub for RazorVue authoring.
/// </summary>
[VueLibraryComponent("vuetify/components", "VPagination")]
public sealed class VPagination : ComponentBase
{
    /// <summary>
    /// 当前选中的页码。
    /// The currently selected page number.
    /// </summary>
    [Parameter]
    public int ModelValue { get; set; }

    /// <summary>
    /// 页码变更时触发的回调。
    /// Callback invoked when the page number changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> ModelValueChanged { get; set; }

    /// <summary>
    /// 总页数。
    /// The total number of pages.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Length { get; set; }

    /// <summary>
    /// 可见页码按钮的数量。
    /// The number of visible pagination buttons.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? TotalVisible { get; set; }

    /// <summary>
    /// 是否禁用分页组件。
    /// Whether the pagination is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
