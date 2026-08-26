using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 浮动操作按钮创作代理。
/// Vuetify floating action button authoring proxy.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VFab")]
public sealed class VFab : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 按钮的展开/收起绑定值。
    /// Bound value controlling the expanded/collapsed state of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public bool ModelValue { get; set; } = true;

    /// <summary>
    /// 当 ModelValue 变化时触发的事件回调。
    /// Event callback fired when ModelValue changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否将按钮注册为应用布局元素。
    /// Whether to register the button as an application layout element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("app")]
    public bool App { get; set; }

    /// <summary>
    /// 按钮是否在初始渲染时就可见。
    /// Whether the button is visible on initial render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("appear")]
    public bool Appear { get; set; }

    /// <summary>
    /// 是否使按钮显示为扩展模式（带文本标签）。
    /// Whether to display the button in extended mode with a text label.
    /// </summary>
    [Parameter]
    [ECMAScriptName("extended")]
    public bool Extended { get; set; }

    /// <summary>
    /// 是否参与布局计算。
    /// Whether the button participates in layout calculation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("layout")]
    public bool Layout { get; set; }

    /// <summary>
    /// 是否应用布局偏移量。
    /// Whether to apply layout offset.
    /// </summary>
    [Parameter]
    [ECMAScriptName("offset")]
    public bool Offset { get; set; }

    /// <summary>
    /// 按钮显隐时使用的过渡动画。
    /// Transition animation used when the button appears or disappears.
    /// </summary>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 按钮在屏幕上的定位位置。
    /// Position of the button on the screen.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 组件的名称，用于布局定位。
    /// Component name used for layout positioning.
    /// </summary>
    [Parameter]
    [ECMAScriptName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 组件在布局中的排序优先级。
    /// Ordering priority of the component in layout.
    /// </summary>
    [Parameter]
    [ECMAScriptName("order")]
    public VueStringNumberValue? Order { get; set; }

    /// <summary>
    /// 是否使用绝对定位。
    /// Whether to use absolute positioning.
    /// </summary>
    [Parameter]
    [ECMAScriptName("absolute")]
    public bool Absolute { get; set; }

    /// <summary>
    /// 按钮是否处于激活状态。
    /// Whether the button is in an active state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("active")]
    public bool Active { get; set; } = true;

    /// <summary>
    /// 按钮激活状态时的颜色。
    /// Color applied when the button is active.
    /// </summary>
    [Parameter]
    [ECMAScriptName("activeColor")]
    public string? ActiveColor { get; set; }

    /// <summary>
    /// 按钮的基础颜色。
    /// Base color of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("baseColor")]
    public string? BaseColor { get; set; }

    /// <summary>
    /// 扩展模式下按钮的文本内容。
    /// Text content of the button in extended mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("text")]
    public VuetifyTextValue? Text { get; set; }

    /// <summary>
    /// 按钮文本前方的前置图标。
    /// Icon prepended before the button text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("prependIcon")]
    public string? PrependIcon { get; set; }

    /// <summary>
    /// 按钮文本后方的追加图标。
    /// Icon appended after the button text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("appendIcon")]
    public string? AppendIcon { get; set; }

    /// <summary>
    /// 按钮的主题颜色。
    /// Theme color of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 按钮的视觉变体样式。
    /// Visual variant style of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("variant")]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name applied to the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 按钮的尺寸。
    /// Size of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("size")]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 是否显示加载状态指示器。
    /// Whether to show a loading indicator.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loading")]
    public VuetifyBooleanStringValue? Loading { get; set; }

    /// <summary>
    /// 是否将按钮显示为块级元素并占满可用宽度。
    /// Whether to display the button as a block-level element spanning full width.
    /// </summary>
    [Parameter]
    [ECMAScriptName("block")]
    public bool Block { get; set; }

    /// <summary>
    /// 按钮的边框样式。
    /// Border style of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

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
    /// 按钮的最小高度。
    /// Minimum height of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 按钮的最小宽度。
    /// Minimum width of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 按钮的最大高度。
    /// Maximum height of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 按钮的最大宽度。
    /// Maximum width of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 按钮的圆角样式。
    /// Border radius style of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 按钮的阴影高度。
    /// Elevation shadow level of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 路由链接是否要求精确匹配。
    /// Whether the router link requires an exact match.
    /// </summary>
    [Parameter]
    [ECMAScriptName("exact")]
    public bool Exact { get; set; }

    /// <summary>
    /// 按钮链接的 URL 地址。
    /// URL href for the button link.
    /// </summary>
    [Parameter]
    [ECMAScriptName("href")]
    public string? Href { get; set; }

    /// <summary>
    /// 按钮的路由链接目标路径。
    /// Router link destination path.
    /// </summary>
    [Parameter]
    [ECMAScriptName("to")]
    public string? To { get; set; }

    /// <summary>
    /// 路由导航时是否替换当前历史记录。
    /// Whether to replace the current history entry on navigation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("replace")]
    public bool Replace { get; set; }

    /// <summary>
    /// 是否禁用按钮。
    /// Whether to disable the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否移除按钮的阴影（扁平模式）。
    /// Whether to remove the button shadow (flat mode).
    /// </summary>
    [Parameter]
    [ECMAScriptName("flat")]
    public bool Flat { get; set; }

    /// <summary>
    /// 按钮显示的图标。
    /// Icon displayed on the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("icon")]
    public VuetifyIconValue? Icon { get; set; }

    /// <summary>
    /// 是否将按钮设为只读状态。
    /// Whether the button is in read-only state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("readonly")]
    public bool Readonly { get; set; }

    /// <summary>
    /// 是否减少按钮的内边距使其更紧凑。
    /// Whether to reduce padding for a more compact appearance.
    /// </summary>
    [Parameter]
    [ECMAScriptName("slim")]
    public bool Slim { get; set; }

    /// <summary>
    /// 是否将图标和文本垂直堆叠排列。
    /// Whether to stack icon and text vertically.
    /// </summary>
    [Parameter]
    [ECMAScriptName("stacked")]
    public bool Stacked { get; set; }

    /// <summary>
    /// 是否移除按钮的圆角。
    /// Whether to remove border radius from the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 按钮选中时应用的 CSS 类名。
    /// CSS class applied when the button is selected.
    /// </summary>
    [Parameter]
    [ECMAScriptName("selectedClass")]
    public string? SelectedClass { get; set; }

    /// <summary>
    /// 按钮的紧凑程度。
    /// Density controlling the compactness of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 浮动按钮的定位方式。
    /// Positioning mode of the floating button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("position")]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 组件渲染使用的 HTML 标签。
    /// HTML tag used to render the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 按钮的绑定值。
    /// Bound value of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("value")]
    public VueValue? Value { get; set; }

    /// <summary>
    /// 按钮的水波纹点击效果配置。
    /// Ripple click effect configuration of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("ripple")]
    public VuetifyRippleValue? Ripple { get; set; }

    /// <summary>
    /// 附加到根元素的自定义属性。
    /// Additional custom attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 按钮的默认子内容插槽。
    /// Default child content slot of the button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
