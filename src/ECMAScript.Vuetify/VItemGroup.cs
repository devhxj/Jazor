using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 条目组组件，用于管理一组可选项的选中状态。
/// Vuetify item group component for managing selection state across a group of items.
/// </summary>
[ECMAScript("vuetify/components/VItemGroup")]
public sealed class VItemGroup : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 条目组的绑定值。
    /// Bound value of the item group.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyGroupModelValue?；值域为 string | Number | bool | Symbol | VueProps | VuetifyGroupModelValue[]。数值用 ModelValue="@(32)"；变量用 ModelValue="@value"，无需 double 后缀。字符串用 ModelValue="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyGroupModelValue? ModelValue { get; set; }

    /// <summary>
    /// 绑定值变化时的回调。
    /// Callback when the bound value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VuetifyGroupModelValue?；保持与模型相同的强类型。投影：value?.AsString、value?.AsNumber、value?.AsBool、value?.AsSymbol、value?.AsObject、value?.AsValues；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyGroupModelValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否强制至少选中一个条目。
    /// Whether at least one item must be selected.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMandatoryValue?；值域为 bool | VuetifyMandatoryMode。变量用 Mandatory="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("mandatory")]
    public VuetifyMandatoryValue? Mandatory { get; set; }

    /// <summary>
    /// 可选中的最大条目数。
    /// Maximum number of selectable items.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Max="@(32)"；变量用 Max="@value"，无需 double 后缀。字符串用 Max="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
