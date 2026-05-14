using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 评分组件。
/// Vuetify rating component.
/// </summary>
[VueLibraryComponent("vuetify/components", "VRating")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueSlot(nameof(ItemContent), Name = "item")]
[VueSlot(nameof(ItemLabel), Name = "item-label")]
public sealed class VRating : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 当前评分值。
    /// The current rating value.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? ModelValue { get; set; }

    /// <summary>
    /// 评分值变更时触发的回调。
    /// Callback invoked when the rating value changes.
    /// </summary>
    [Parameter]
    public EventCallback<VueStringNumberValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 激活状态的图标颜色。
    /// The icon color when active/selected.
    /// </summary>
    [Parameter]
    public string? ActiveColor { get; set; }

    /// <summary>
    /// 未激活状态的图标颜色。
    /// The icon color when inactive.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 是否允许点击清除评分。
    /// Whether clicking again clears the rating.
    /// </summary>
    [Parameter]
    public bool Clearable { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// The density/compactness of the component.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 表单元素的 name 属性。
    /// The name attribute for the form element.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// 评分项标签的位置。
    /// The position of item labels.
    /// </summary>
    [Parameter]
    public VuetifyItemLabelPosition? ItemLabelPosition { get; set; }

    /// <summary>
    /// 各评分项的标签文本。
    /// The label text for each rating item.
    /// </summary>
    [Parameter]
    public VuetifyMessagesValue? ItemLabels { get; set; }

    /// <summary>
    /// 评分项的 ARIA 标签。
    /// The ARIA label for rating items.
    /// </summary>
    [Parameter]
    public string? ItemAriaLabel { get; set; }

    /// <summary>
    /// 是否允许半星递增。
    /// Whether to allow half-increment ratings.
    /// </summary>
    [Parameter]
    public bool HalfIncrements { get; set; }

    /// <summary>
    /// 未选中时显示的图标。
    /// The icon displayed for empty items.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? EmptyIcon { get; set; }

    /// <summary>
    /// 选中时显示的图标。
    /// The icon displayed for full items.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? FullIcon { get; set; }

    /// <summary>
    /// 半选时显示的图标。
    /// The icon displayed for half-filled items.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? HalfIcon { get; set; }

    /// <summary>
    /// 评分项的数量。
    /// The number of rating items.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Length { get; set; }

    /// <summary>
    /// 是否在悬停时预览评分。
    /// Whether to preview rating on hover.
    /// </summary>
    [Parameter]
    public bool Hover { get; set; }

    /// <summary>
    /// 是否为只读状态。
    /// Whether the rating is read-only.
    /// </summary>
    [Parameter]
    public bool Readonly { get; set; }

    /// <summary>
    /// 是否禁用评分组件。
    /// Whether the rating is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否启用涟漪效果。
    /// Whether to enable the ripple effect.
    /// </summary>
    [Parameter]
    public VuetifyRippleValue? Ripple { get; set; }

    /// <summary>
    /// 评分图标的大小。
    /// The size of the rating icons.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 渲染根元素时使用的 HTML 标签。
    /// The HTML tag used for the root element.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 自定义评分项的插槽内容。
    /// Custom content for each rating item.
    /// </summary>
    [Parameter]
    public RenderFragment<VRatingItemSlotContext>? ItemContent { get; set; }

    /// <summary>
    /// 自定义评分项标签的插槽内容。
    /// Custom content for each rating item label.
    /// </summary>
    [Parameter]
    public RenderFragment<VRatingItemLabelSlotContext>? ItemLabel { get; set; }
}
