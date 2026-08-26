using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 列表项组件，用于列表中的单个交互条目。
/// Vuetify list item component for a single interactive entry within a list.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VListItem")]
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
    [Parameter]
    [ECMAScriptName("subtitle")]
    public VuetifyTextValue? Subtitle { get; set; }

    /// <summary>
    /// 列表项的标题文本。
    /// Title text of the list item.
    /// </summary>
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
    /// 组件的海拔阴影级别。
    /// Elevation shadow level of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件的圆角样式。
    /// Border radius style of the component.
    /// </summary>
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
