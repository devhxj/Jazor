using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 颜色选择器创作代理。
/// Vuetify color-picker authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VColorPicker")]
public sealed class VColorPicker : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 选中的颜色绑定值。
    /// The bound value of the selected color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyColorValue? ModelValue { get; set; }

    /// <summary>
    /// 颜色值变更回调。
    /// Callback invoked when the color value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<VuetifyColorValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 颜色选择模式（如 RGB、HSL、HEX）。
    /// The color selection mode (e.g., RGB, HSL, HEX).
    /// </summary>
    [Parameter]
    [ECMAScriptName("mode")]
    public VuetifyColorPickerMode? Mode { get; set; }

    /// <summary>
    /// 选择模式变更回调。
    /// Callback invoked when the selection mode changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:mode")]
    public EventCallback<VuetifyColorPickerMode> ModeChanged { get; set; }

    /// <summary>
    /// 可用的颜色选择模式集合。
    /// The available color selection modes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modes")]
    public VuetifyColorPickerModes? Modes { get; set; }

    /// <summary>
    /// 画布高度。
    /// The height of the color canvas.
    /// </summary>
    [Parameter]
    [ECMAScriptName("canvasHeight")]
    public VueStringNumberValue? CanvasHeight { get; set; }

    /// <summary>
    /// 是否禁用选择器。
    /// Whether to disable the picker.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 指示点大小。
    /// The size of the indicator dot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("dotSize")]
    public VueStringNumberValue? DotSize { get; set; }

    /// <summary>
    /// 是否隐藏画布。
    /// Whether to hide the color canvas.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideCanvas")]
    public bool HideCanvas { get; set; }

    /// <summary>
    /// 是否隐藏滑块。
    /// Whether to hide the sliders.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideSliders")]
    public bool HideSliders { get; set; }

    /// <summary>
    /// 是否隐藏输入框。
    /// Whether to hide the input fields.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideInputs")]
    public bool HideInputs { get; set; }

    /// <summary>
    /// 是否显示色板。
    /// Whether to show the color swatches.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showSwatches")]
    public bool ShowSwatches { get; set; }

    /// <summary>
    /// 可选色板集合。
    /// The selectable color swatches.
    /// </summary>
    [Parameter]
    [ECMAScriptName("swatches")]
    public VuetifyColorPickerSwatches? Swatches { get; set; }

    /// <summary>
    /// 色板区域最大高度。
    /// The maximum height of the swatches area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("swatchesMaxHeight")]
    public VueStringNumberValue? SwatchesMaxHeight { get; set; }

    /// <summary>
    /// 组件主题名称。
    /// The component theme name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 圆角大小。
    /// The border radius size.
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
    /// 组件位置。
    /// The position of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("position")]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 组件定位位置。
    /// The location of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 组件的海拔阴影高度。
    /// The elevation shadow height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件高度。
    /// The height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件最大高度。
    /// The maximum height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件最大宽度。
    /// The maximum width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 组件最小高度。
    /// The minimum height of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 组件最小宽度。
    /// The minimum width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件宽度。
    /// The width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 边框样式。
    /// The border style.
    /// </summary>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 组件的主题色。
    /// The theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 组件背景色。
    /// The background color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bgColor")]
    public string? BgColor { get; set; }

    /// <summary>
    /// 是否显示分隔线。
    /// Whether to show dividers.
    /// </summary>
    [Parameter]
    [ECMAScriptName("divided")]
    public bool Divided { get; set; }

    /// <summary>
    /// 是否以横向布局显示。
    /// Whether to display in landscape orientation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("landscape")]
    public bool Landscape { get; set; }

    /// <summary>
    /// 标题文本。
    /// The title text.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 是否隐藏头部区域。
    /// Whether to hide the header area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideHeader")]
    public bool HideHeader { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 子内容插槽。
    /// Slot for child content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 头部内容插槽。
    /// Slot for the header content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("header")]
    public RenderFragment? Header { get; set; }

    /// <summary>
    /// 操作按钮插槽。
    /// Slot for action buttons.
    /// </summary>
    [Parameter]
    [ECMAScriptName("actions")]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// 标题内容插槽。
    /// Slot for the title content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public RenderFragment? TitleContent { get; set; }
}
