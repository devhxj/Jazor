using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 评分组件。
/// Vuetify rating component.
/// </summary>
[ECMAScript("vuetify/components/VRating")]
public sealed class VRating : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 当前评分值。
    /// The current rating value.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 ModelValue="@(32)"；变量用 ModelValue="@value"，无需 double 后缀。字符串用 ModelValue="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VueStringNumberValue? ModelValue { get; set; }

    /// <summary>
    /// 评分值变更时触发的回调。
    /// Callback invoked when the rating value changes.
    /// </summary>
    /// <remarks data-authoring="types">C# 回调参数为 VueStringNumberValue?；保持与模型相同的强类型。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。</remarks>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VueStringNumberValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 激活状态的图标颜色。
    /// The icon color when active/selected.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeColor")]
    public string? ActiveColor { get; set; }

    /// <summary>
    /// 未激活状态的图标颜色。
    /// The icon color when inactive.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 是否允许点击清除评分。
    /// Whether clicking again clears the rating.
    /// </summary>
    [Parameter]
    [ECMAScriptName("clearable")]
    public bool Clearable { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// The density/compactness of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 表单元素的 name 属性。
    /// The name attribute for the form element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 评分项标签的位置。
    /// The position of item labels.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemLabelPosition")]
    public VuetifyItemLabelPosition? ItemLabelPosition { get; set; }

    /// <summary>
    /// 各评分项的标签文本。
    /// The label text for each rating item.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyMessagesValue?；值域为 string | string[]。变量用 ItemLabels="@value"，并保持声明的分支类型。字符串用 ItemLabels="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsStrings；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("itemLabels")]
    public VuetifyMessagesValue? ItemLabels { get; set; }

    /// <summary>
    /// 评分项的 ARIA 标签。
    /// The ARIA label for rating items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemAriaLabel")]
    public string? ItemAriaLabel { get; set; }

    /// <summary>
    /// 是否允许半星递增。
    /// Whether to allow half-increment ratings.
    /// </summary>
    [Parameter]
    [ECMAScriptName("halfIncrements")]
    public bool HalfIncrements { get; set; }

    /// <summary>
    /// 未选中时显示的图标。
    /// The icon displayed for empty items.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 EmptyIcon="@value"，并保持声明的分支类型。字符串用 EmptyIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("emptyIcon")]
    public VuetifyIconValue? EmptyIcon { get; set; }

    /// <summary>
    /// 选中时显示的图标。
    /// The icon displayed for full items.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 FullIcon="@value"，并保持声明的分支类型。字符串用 FullIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("fullIcon")]
    public VuetifyIconValue? FullIcon { get; set; }

    /// <summary>
    /// 半选时显示的图标。
    /// The icon displayed for half-filled items.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 HalfIcon="@value"，并保持声明的分支类型。字符串用 HalfIcon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("halfIcon")]
    public VuetifyIconValue? HalfIcon { get; set; }

    /// <summary>
    /// 评分项的数量。
    /// The number of rating items.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Length="@(32)"；变量用 Length="@value"，无需 double 后缀。字符串用 Length="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("length")]
    public VueStringNumberValue? Length { get; set; }

    /// <summary>
    /// 是否在悬停时预览评分。
    /// Whether to preview rating on hover.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hover")]
    public bool Hover { get; set; }

    /// <summary>
    /// 是否为只读状态。
    /// Whether the rating is read-only.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 是否禁用评分组件。
    /// Whether the rating is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否启用涟漪效果。
    /// Whether to enable the ripple effect.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyRippleValue?；值域为 bool | VueProps。变量用 Ripple="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsOptions；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("ripple")]
    public VuetifyRippleValue? Ripple { get; set; }

    /// <summary>
    /// 评分图标的大小。
    /// The size of the rating icons.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Size="@(32)"；变量用 Size="@value"，无需 double 后缀。字符串用 Size="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("size")]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 渲染根元素时使用的 HTML 标签。
    /// The HTML tag used for the root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 自定义评分项的插槽内容。
    /// Custom content for each rating item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("item")]
    public RenderFragment<VRatingItemSlotContext>? ItemContent { get; set; }

    /// <summary>
    /// 自定义评分项标签的插槽内容。
    /// Custom content for each rating item label.
    /// </summary>
    [Parameter]
    [ECMAScriptName("item-label")]
    public RenderFragment<VRatingItemLabelSlotContext>? ItemLabel { get; set; }
}
