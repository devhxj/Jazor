using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 实验室图标按钮创作代理。
/// Vuetify labs icon-btn authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VIconBtn")]
public sealed class VIconBtn : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 按钮的主题颜色。
    /// Theme color of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 按钮的外观变体。
    /// Visual variant of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 根元素使用的 HTML 标签。
    /// HTML tag used for the root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 按钮的圆角样式。
    /// Border radius style of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 按钮的海拔阴影等级。
    /// Elevation shadow level of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 按钮的边框样式。
    /// Border style of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 按钮是否处于激活状态。
    /// Whether the button is in the active state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("active")]
    public bool Active { get; set; }

    /// <summary>
    /// 按钮激活状态变化时的回调。
    /// Callback when the button active state changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:active")]
    public EventCallback<bool> ActiveChanged { get; set; }

    /// <summary>
    /// 激活状态时的主题颜色。
    /// Theme color when the button is active.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeColor")]
    public string? ActiveColor { get; set; }

    /// <summary>
    /// 激活状态时显示的图标。
    /// Icon displayed when the button is active.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeIcon")]
    public VuetifyIconValue? ActiveIcon { get; set; }

    /// <summary>
    /// 激活状态时的外观变体。
    /// Visual variant when the button is active.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeVariant")]
    public VuetifyVariant? ActiveVariant { get; set; }

    /// <summary>
    /// 非激活状态时的外观变体。
    /// Visual variant when the button is inactive.
    /// </summary>
    [Parameter]
    [ECMAScriptName("baseVariant")]
    public VuetifyVariant? BaseVariant { get; set; }

    /// <summary>
    /// 是否禁用按钮。
    /// Whether the button is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 按钮的高度。
    /// Height of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 按钮的宽度。
    /// Width of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 是否隐藏叠加层。
    /// Whether to hide the overlay.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideOverlay")]
    public bool HideOverlay { get; set; }

    /// <summary>
    /// 按钮显示的图标。
    /// Icon displayed on the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("icon")]
    public VuetifyIconValue? Icon { get; set; }

    /// <summary>
    /// 图标的主题颜色。
    /// Theme color of the icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("iconColor")]
    public string? IconColor { get; set; }

    /// <summary>
    /// 图标的尺寸。
    /// Size of the icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("iconSize")]
    public VueStringNumberValue? IconSize { get; set; }

    /// <summary>
    /// 不同尺寸下图标尺寸的映射。
    /// Icon size mapping for different sizes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("iconSizes")]
    public VIconBtnSizeMap? IconSizes { get; set; }

    /// <summary>
    /// 是否显示加载状态。
    /// Whether to show the loading state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loading")]
    public bool Loading { get; set; }

    /// <summary>
    /// 按钮的透明度。
    /// Opacity of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("opacity")]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 是否将按钮设为只读。
    /// Whether the button is read-only.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 图标的旋转角度。
    /// Rotation angle of the icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rotate")]
    public VueStringNumberValue? Rotate { get; set; }

    /// <summary>
    /// 按钮的尺寸。
    /// Size of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("size")]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 不同尺寸下按钮尺寸的映射。
    /// Button size mapping for different sizes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("sizes")]
    public VIconBtnSizeMap? Sizes { get; set; }

    /// <summary>
    /// 按钮的文本内容。
    /// Text content of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("text")]
    public VIconBtnTextValue? Text { get; set; }

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

    /// <summary>
    /// 加载状态插槽内容。
    /// Slot content for the loading state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loader")]
    public RenderFragment? Loader { get; set; }
}
