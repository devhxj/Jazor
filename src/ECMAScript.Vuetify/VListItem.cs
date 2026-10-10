using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 列表项组件，用于列表中的单个交互条目。
/// Vuetify list item component for a single interactive entry within a list.
/// </summary>
[ECMAScript("vuetify/components/VList")]
public sealed class VListItem : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 列表项是否处于活跃状态。
    /// Whether the list item is in an active state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("active")]
    public bool? Active { get; set; }

    /// <summary>
    /// 活跃状态时应用的 CSS 类名。
    /// CSS class applied when in an active state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeClass")]
    public string? ActiveClass { get; set; }

    /// <summary>
    /// 活跃状态时的颜色。
    /// Color when in an active state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeColor")]
    public string? ActiveColor { get; set; }

    /// <summary>
    /// 后置头像的 URL。
    /// URL of the append avatar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("appendAvatar")]
    public string? AppendAvatar { get; set; }

    /// <summary>
    /// 后置图标的名称。
    /// Name of the append icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("appendIcon")]
    public string? AppendIcon { get; set; }

    /// <summary>
    /// 处于非活跃状态时的颜色。
    /// Color when the component is in an inactive state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("baseColor")]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 是否禁用列表项交互。
    /// Whether to disable list item interaction.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 列表项的行间距样式。
    /// Line spacing style for the list item.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyListLines?；值域为 bool | VuetifyListLineMode。变量用 Lines="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsMode；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("lines")]
    public VuetifyListLines? Lines { get; set; }

    /// <summary>
    /// 是否将列表项渲染为链接。
    /// Whether to render the list item as a link.
    /// </summary>
    [Parameter]
    [ECMAScriptName("link")]
    public bool? Link { get; set; }

    /// <summary>
    /// 是否为导航模式列表项。
    /// Whether the list item is in navigation mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("nav")]
    public bool Nav { get; set; }

    /// <summary>
    /// 前置头像的 URL。
    /// URL of the prepend avatar.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prependAvatar")]
    public string? PrependAvatar { get; set; }

    /// <summary>
    /// 前置图标的名称。
    /// Name of the prepend icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prependIcon")]
    public string? PrependIcon { get; set; }

    /// <summary>
    /// 是否显示涟漪点击效果。
    /// Whether to show a ripple click effect.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyRippleValue?；值域为 bool | VueProps。变量用 Ripple="@value"，并保持声明的分支类型。投影：value?.AsBool、value?.AsOptions；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("ripple")]
    public VuetifyRippleValue? Ripple { get; set; }

    /// <summary>
    /// 是否使用紧凑的细长样式。
    /// Whether to use a slim compact style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("slim")]
    public bool Slim { get; set; }

    /// <summary>
    /// 列表项的副标题文本。
    /// Subtitle text of the list item.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyTextValue?；值域为 string | Number | bool。数值用 Subtitle="@(32)"；变量用 Subtitle="@value"，无需 double 后缀。字符串用 Subtitle="text"；数字字符串保持 string。投影：value?.AsString、value?.AsNumber、value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("subtitle")]
    public VuetifyTextValue? Subtitle { get; set; }

    /// <summary>
    /// 列表项的标题文本。
    /// Title text of the list item.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyTextValue?；值域为 string | Number | bool。数值用 Title="@(32)"；变量用 Title="@value"，无需 double 后缀。字符串用 Title="text"；数字字符串保持 string。投影：value?.AsString、value?.AsNumber、value?.AsBool；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("title")]
    public VuetifyTextValue? Title { get; set; }

    /// <summary>
    /// 列表项的值，用于选中状态标识。
    /// Value of the list item, used for selection state identification.
    /// </summary>
    [Parameter]
    [ECMAScriptName("value")]
    public VueValue? Value { get; set; }

    /// <summary>
    /// 组件的密度样式，调整垂直间距。
    /// Component density style that adjusts vertical spacing.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

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
    /// 组件的海拔阴影级别。
    /// Elevation shadow level of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Elevation="@(32)"；变量用 Elevation="@value"，无需 double 后缀。字符串用 Elevation="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件的圆角样式。
    /// Border radius style of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyRoundedValue?；值域为 bool | Number | string。数值用 Rounded="@(32)"；变量用 Rounded="@value"，无需 double 后缀。字符串用 Rounded="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 列表项的链接地址。
    /// Link URL of the list item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("href")]
    public string? Href { get; set; }

    /// <summary>
    /// 路由导航的目标路径。
    /// Target path for router navigation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("to")]
    public string? To { get; set; }

    /// <summary>
    /// 导航时是否替换当前历史记录。
    /// Whether to replace the current history entry on navigation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("replace")]
    public bool Replace { get; set; }

    /// <summary>
    /// 是否要求精确匹配路由。
    /// Whether to require exact route matching.
    /// </summary>
    [Parameter]
    [ECMAScriptName("exact")]
    public bool Exact { get; set; }

    /// <summary>
    /// 列表项的视觉变体样式。
    /// Visual variant style of the list item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 点击列表项时触发的回调。
    /// Callback invoked when the list item is clicked.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onClick")]
    public EventCallback OnClick { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 前置插槽内容。
    /// Prepend slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prepend")]
    public RenderFragment<VListItemSlotContext>? Prepend { get; set; }

    /// <summary>
    /// 后置插槽内容。
    /// Append slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("append")]
    public RenderFragment<VListItemSlotContext>? Append { get; set; }

    /// <summary>
    /// 标题插槽内容。
    /// Title slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public RenderFragment<VListItemTitleSlotContext>? TitleContent { get; set; }

    /// <summary>
    /// 副标题插槽内容。
    /// Subtitle slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("subtitle")]
    public RenderFragment<VListItemSubtitleSlotContext>? SubtitleContent { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
