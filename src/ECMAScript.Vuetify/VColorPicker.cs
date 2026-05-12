using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 颜色选择器创作代理。
/// Vuetify color-picker authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VColorPicker")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueLibraryEmit(nameof(ModeChanged), VueEmitKind.ModelUpdate, Name = "update:mode")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Header), Name = "header")]
[VueLibrarySlot(nameof(Actions), Name = "actions")]
[VueLibrarySlot(nameof(TitleContent), Name = "title")]
public sealed class VColorPicker : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 选中的颜色绑定值。
    /// The bound value of the selected color.
    /// </summary>
    [Parameter]
    public VuetifyColorValue? ModelValue { get; set; }

    /// <summary>
    /// 颜色值变更回调。
    /// Callback invoked when the color value changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyColorValue?> ModelValueChanged { get; set; }

    /// <summary>
    /// 颜色选择模式（如 RGB、HSL、HEX）。
    /// The color selection mode (e.g., RGB, HSL, HEX).
    /// </summary>
    [Parameter]
    public VuetifyColorPickerMode? Mode { get; set; }

    /// <summary>
    /// 选择模式变更回调。
    /// Callback invoked when the selection mode changes.
    /// </summary>
    [Parameter]
    public EventCallback<VuetifyColorPickerMode> ModeChanged { get; set; }

    /// <summary>
    /// 可用的颜色选择模式集合。
    /// The available color selection modes.
    /// </summary>
    [Parameter]
    public VuetifyColorPickerModes? Modes { get; set; }

    /// <summary>
    /// 画布高度。
    /// The height of the color canvas.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? CanvasHeight { get; set; }

    /// <summary>
    /// 是否禁用选择器。
    /// Whether to disable the picker.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 指示点大小。
    /// The size of the indicator dot.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? DotSize { get; set; }

    /// <summary>
    /// 是否隐藏画布。
    /// Whether to hide the color canvas.
    /// </summary>
    [Parameter]
    public bool HideCanvas { get; set; }

    /// <summary>
    /// 是否隐藏滑块。
    /// Whether to hide the sliders.
    /// </summary>
    [Parameter]
    public bool HideSliders { get; set; }

    /// <summary>
    /// 是否隐藏输入框。
    /// Whether to hide the input fields.
    /// </summary>
    [Parameter]
    public bool HideInputs { get; set; }

    /// <summary>
    /// 是否显示色板。
    /// Whether to show the color swatches.
    /// </summary>
    [Parameter]
    public bool ShowSwatches { get; set; }

    /// <summary>
    /// 可选色板集合。
    /// The selectable color swatches.
    /// </summary>
    [Parameter]
    public VuetifyColorPickerSwatches? Swatches { get; set; }

    /// <summary>
    /// 色板区域最大高度。
    /// The maximum height of the swatches area.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? SwatchesMaxHeight { get; set; }

    /// <summary>
    /// 组件主题名称。
    /// The component theme name.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 渲染的 HTML 标签名。
    /// The HTML tag name to render.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 圆角大小。
    /// The border radius size.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    public bool Tile { get; set; }

    /// <summary>
    /// 组件位置。
    /// The position of the component.
    /// </summary>
    [Parameter]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 组件定位位置。
    /// The location of the component.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 组件的海拔阴影高度。
    /// The elevation shadow height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件高度。
    /// The height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件最大高度。
    /// The maximum height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件最大宽度。
    /// The maximum width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 组件最小高度。
    /// The minimum height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 组件最小宽度。
    /// The minimum width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件宽度。
    /// The width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 边框样式。
    /// The border style.
    /// </summary>
    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 组件的主题色。
    /// The theme color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 组件背景色。
    /// The background color of the component.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 是否显示分隔线。
    /// Whether to show dividers.
    /// </summary>
    [Parameter]
    public bool Divided { get; set; }

    /// <summary>
    /// 是否以横向布局显示。
    /// Whether to display in landscape orientation.
    /// </summary>
    [Parameter]
    public bool Landscape { get; set; }

    /// <summary>
    /// 标题文本。
    /// The title text.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// 是否隐藏头部区域。
    /// Whether to hide the header area.
    /// </summary>
    [Parameter]
    public bool HideHeader { get; set; }

    /// <summary>
    /// 附加的自定义属性。
    /// Additional custom attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 子内容插槽。
    /// Slot for child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 头部内容插槽。
    /// Slot for the header content.
    /// </summary>
    [Parameter]
    public RenderFragment? Header { get; set; }

    /// <summary>
    /// 操作按钮插槽。
    /// Slot for action buttons.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// 标题内容插槽。
    /// Slot for the title content.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }
}
