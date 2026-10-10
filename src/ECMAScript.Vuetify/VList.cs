using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 列表组件，用于展示可交互的列表项集合。
/// Vuetify list component for displaying interactive collections of list items.
/// </summary>
[ECMAScript("vuetify/components/VList")]
public sealed class VList : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 列表中显示的选项数据源。
    /// Data source items to display in the list.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySelectItems?；值域为 VuetifySelectItemValue[]。变量用 Items="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsArray；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("items")]
    public VuetifySelectItems? Items { get; set; }

    /// <summary>
    /// 用于显示标题的数据项属性名或键。
    /// Property name or key for displaying item titles.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySelectItemKey?；值域为 string | string[] | VuetifySelectItemKeySelector | bool。变量用 ItemTitle="@value"，并保持声明的分支类型。字符串用 ItemTitle="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsPath、value?.AsSelector、value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("itemTitle")]
    public VuetifySelectItemKey? ItemTitle { get; set; }

    /// <summary>
    /// 用于标识值的数据项属性名或键。
    /// Property name or key for identifying item values.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySelectItemKey?；值域为 string | string[] | VuetifySelectItemKeySelector | bool。变量用 ItemValue="@value"，并保持声明的分支类型。字符串用 ItemValue="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsPath、value?.AsSelector、value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("itemValue")]
    public VuetifySelectItemKey? ItemValue { get; set; }

    /// <summary>
    /// 用于嵌套子项的数据项属性名或键。
    /// Property name or key for nested child items.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySelectItemKey?；值域为 string | string[] | VuetifySelectItemKeySelector | bool。变量用 ItemChildren="@value"，并保持声明的分支类型。字符串用 ItemChildren="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsPath、value?.AsSelector、value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("itemChildren")]
    public VuetifySelectItemKey? ItemChildren { get; set; }

    /// <summary>
    /// 用于传递额外属性的数据项属性选择器。
    /// Property selector for passing extra props to items.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifySelectItemPropsSelector?；值域为 string | string[] | VuetifySelectItemPropsCallback | bool。变量用 ItemProps="@value"，并保持声明的分支类型。字符串用 ItemProps="text"；数字字符串保持 string。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsString、value?.AsPath、value?.AsCallback、value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// <remarks data-authoring="types">C# union VuetifyListLines?；值域为 bool | VuetifyListLineMode。变量用 Lines="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// <remarks data-authoring="types">C# union VuetifyRoundedValue?；值域为 bool | Number | string。数值用 Rounded="@(32)"；变量用 Rounded="@value"，无需 double 后缀。字符串用 Rounded="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 组件的海拔阴影级别。
    /// Elevation shadow level of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Elevation="@(32)"；变量用 Elevation="@value"，无需 double 后缀。字符串用 Elevation="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件的高度。
    /// Height of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Height="@(32)"；变量用 Height="@value"，无需 double 后缀。字符串用 Height="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件的宽度。
    /// Width of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Width="@(32)"；变量用 Width="@value"，无需 double 后缀。字符串用 Width="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 组件的最小高度。
    /// Minimum height of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MinHeight="@(32)"；变量用 MinHeight="@value"，无需 double 后缀。字符串用 MinHeight="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 组件的最小宽度。
    /// Minimum width of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MinWidth="@(32)"；变量用 MinWidth="@value"，无需 double 后缀。字符串用 MinWidth="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件的最大高度。
    /// Maximum height of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MaxHeight="@(32)"；变量用 MaxHeight="@value"，无需 double 后缀。字符串用 MaxHeight="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件的最大宽度。
    /// Maximum width of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 MaxWidth="@(32)"；变量用 MaxWidth="@value"，无需 double 后缀。字符串用 MaxWidth="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
