using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 条目组组件，用于管理一组可选项的选中状态。
/// Vuetify item group component for managing selection state across a group of items.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VItemGroup")]
public sealed class VItemGroup : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 条目组的绑定值。
    /// Bound value of the item group.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变化时的回调。
    /// Callback when the bound value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否强制至少选中一个条目。
    /// Whether at least one item must be selected.
    /// </summary>
    [Parameter]
    [ECMAScriptName("mandatory")]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    /// <summary>
    /// 可选中的最大条目数。
    /// Maximum number of selectable items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("max")]
    public VueStringNumberValue? Max { get; set; }

    /// <summary>
    /// 是否允许多选。
    /// Whether multiple selection is allowed.
    /// </summary>
    [Parameter]
    [ECMAScriptName("multiple")]
    public bool Multiple { get; set; }

    /// <summary>
    /// 选中条目时应用的 CSS 类名。
    /// CSS class applied to selected items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 渲染的 HTML 标签名。
    /// HTML tag name to render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 用于比较条目值的函数。
    /// Value comparator for item selection.
    /// </summary>
    [Parameter]
    [ECMAScriptName("valueComparator")]
    public VuetifyValueComparator? ValueComparator { get; set; }

    /// <summary>
    /// 附加到根元素上的额外 HTML 属性。
    /// Additional HTML attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 条目组的默认插槽内容。
    /// Default slot content for the item group.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment<VItemGroupDefaultSlotContext>? ChildContent { get; set; }
}
