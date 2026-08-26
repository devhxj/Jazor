using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 列表组件，用于展示可交互的列表项集合。
/// Vuetify list component for displaying interactive collections of list items.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VList")]
public sealed class VList : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 列表中显示的选项数据源。
    /// Data source items to display in the list.
    /// </summary>
    [Parameter]
    [ECMAScriptName("items")]
    public VuetifySelectItems? Items { get; set; }

    /// <summary>
    /// 用于显示标题的数据项属性名或键。
    /// Property name or key for displaying item titles.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemTitle")]
    public VuetifySelectItemKey? ItemTitle { get; set; }

    /// <summary>
    /// 用于标识值的数据项属性名或键。
    /// Property name or key for identifying item values.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemValue")]
    public VuetifySelectItemKey? ItemValue { get; set; }

    /// <summary>
    /// 用于嵌套子项的数据项属性名或键。
    /// Property name or key for nested child items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemChildren")]
    public VuetifySelectItemKey? ItemChildren { get; set; }

    /// <summary>
    /// 用于传递额外属性的数据项属性选择器。
    /// Property selector for passing extra props to items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemProps")]
    public VuetifySelectItemPropsSelector? ItemProps { get; set; }

    /// <summary>
    /// 数据项的类型标识符。
    /// Type identifier for items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("itemType")]
    public string? ItemType { get; set; }

    /// <summary>
    /// 处于非活跃状态时的颜色。
    /// Color when the component is in an inactive state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("baseColor")]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 活跃状态时的颜色。
    /// Color when the component is in an active state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeColor")]
    public string? ActiveColor { get; set; }

    /// <summary>
    /// 活跃列表项应用的 CSS 类名。
    /// CSS class applied to active list items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeClass")]
    public string? ActiveClass { get; set; }

    /// <summary>
    /// 列表背景颜色。
    /// Background color of the list.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bgColor")]
    public string? BgColor { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 展开子列表时显示的图标。
    /// Icon displayed when expanding sub-lists.
    /// </summary>
    [Parameter]
    [ECMAScriptName("expandIcon")]
    public string? ExpandIcon { get; set; }

    /// <summary>
    /// 折叠子列表时显示的图标。
    /// Icon displayed when collapsing sub-lists.
    /// </summary>
    [Parameter]
    [ECMAScriptName("collapseIcon")]
    public string? CollapseIcon { get; set; }

    /// <summary>
    /// 列表项的行间距样式。
    /// Line spacing style for list items.
    /// </summary>
    [Parameter]
    [ECMAScriptName("lines")]
    public VuetifyListLines? Lines { get; set; }

    /// <summary>
    /// 是否使用紧凑的细长样式。
    /// Whether to use a slim compact style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("slim")]
    public bool Slim { get; set; }

    /// <summary>
    /// 组件的密度样式，调整垂直间距。
    /// Component density style that adjusts vertical spacing.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 是否为导航模式列表。
    /// Whether the list is in navigation mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("nav")]
    public bool Nav { get; set; }

    /// <summary>
    /// 是否禁用列表交互。
    /// Whether to disable list interaction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 列表的视觉变体样式。
    /// Visual variant style of the list.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 组件的圆角样式。
    /// Border radius style of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 组件的海拔阴影级别。
    /// Elevation shadow level of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件的高度。
    /// Height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件的宽度。
    /// Width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 组件的最小高度。
    /// Minimum height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 组件的最小宽度。
    /// Minimum width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件的最大高度。
    /// Maximum height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件的最大宽度。
    /// Maximum width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
