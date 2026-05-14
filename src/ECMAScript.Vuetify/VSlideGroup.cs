using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 滑动分组组件的编写代理，用于水平或垂直可滚动的分组内容。
/// Vuetify slide-group authoring proxy for horizontally or vertically scrollable grouped content.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSlideGroup")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
[VueSlot(nameof(Prev), Name = "prev")]
[VueSlot(nameof(Next), Name = "next")]
public sealed class VSlideGroup : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 当前选中的值。
    /// Currently selected value.
    /// </summary>
    [Parameter]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 选中值变更回调。
    /// Callback when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否允许多选。
    /// Allows multiple selections.
    /// </summary>
    [Parameter]
    public bool Multiple { get; set; }

    /// <summary>
    /// 是否强制选中。
    /// Whether selection is mandatory.
    /// </summary>
    [Parameter]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    /// <summary>
    /// 最大可选数量。
    /// Maximum number of selectable items.
    /// </summary>
    [Parameter]
    public int? Max { get; set; }

    /// <summary>
    /// 选中项应用的 CSS 类名。
    /// CSS class applied to selected items.
    /// </summary>
    [Parameter]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 是否禁用组件。
    /// Disables the component.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 渲染的 HTML 标签名。
    /// HTML tag name to render.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 是否启用移动端布局。
    /// Whether mobile layout is active.
    /// </summary>
    [Parameter]
    public VuetifyMobileValue? Mobile { get; set; }

    /// <summary>
    /// 移动端断点阈值。
    /// Mobile breakpoint threshold.
    /// </summary>
    [Parameter]
    public VuetifyDisplayBreakpoint? MobileBreakpoint { get; set; }

    /// <summary>
    /// 是否始终将活动项居中显示。
    /// Always center the active item.
    /// </summary>
    [Parameter]
    public bool CenterActive { get; set; }

    /// <summary>
    /// 滑动方向。
    /// Slide direction.
    /// </summary>
    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 下一个导航图标。
    /// Icon for the next navigation arrow.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? NextIcon { get; set; }

    /// <summary>
    /// 上一个导航图标。
    /// Icon for the previous navigation arrow.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? PrevIcon { get; set; }

    /// <summary>
    /// 箭头显示条件。
    /// When to show navigation arrows.
    /// </summary>
    [Parameter]
    public VuetifyShowArrowsValue? ShowArrows { get; set; }

    /// <summary>
    /// 附加的额外 HTML 属性。
    /// Additional unmatched HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment<VSlideGroupSlotContext>? ChildContent { get; set; }

    /// <summary>
    /// 上一个导航插槽。
    /// Previous navigation slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VSlideGroupSlotContext>? Prev { get; set; }

    /// <summary>
    /// 下一个导航插槽。
    /// Next navigation slot.
    /// </summary>
    [Parameter]
    public RenderFragment<VSlideGroupSlotContext>? Next { get; set; }
}
