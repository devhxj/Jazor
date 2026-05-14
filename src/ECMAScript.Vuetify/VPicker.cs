using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify Labs 选择器外壳组件的编写代理。
/// Vuetify labs picker shell authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VPicker")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueSlot(nameof(Header), Name = "header")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
[VueSlot(nameof(Actions), Name = "actions")]
[VueSlot(nameof(TitleContent), Name = "title")]
public sealed class VPicker : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 组件使用的主题名称。
    /// The theme name used by the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 渲染根元素时使用的 HTML 标签。
    /// The HTML tag used for the root element.
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
    /// 是否移除圆角，使边角为直角。
    /// Whether to remove border radius for sharp corners.
    /// </summary>
    [Parameter]
    public bool Tile { get; set; }

    /// <summary>
    /// 选择器面板的位置。
    /// The position of the picker panel.
    /// </summary>
    [Parameter]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 选择器面板的方位。
    /// The location of the picker panel.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 组件的海拔阴影高度。
    /// The elevation shadow of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件的高度。
    /// The height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件的最大高度。
    /// The maximum height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件的最大宽度。
    /// The maximum width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 组件的最小高度。
    /// The minimum height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 组件的最小宽度。
    /// The minimum width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件的宽度。
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
    /// 组件的前景色。
    /// The foreground color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 组件的背景色。
    /// The background color of the component.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 是否在头部和主体之间显示分割线。
    /// Whether to show a divider between header and body.
    /// </summary>
    [Parameter]
    public bool Divided { get; set; }

    /// <summary>
    /// 是否使用横向布局。
    /// Whether to use landscape orientation.
    /// </summary>
    [Parameter]
    public bool Landscape { get; set; }

    /// <summary>
    /// 选择器的标题文本。
    /// The title text of the picker.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// 是否隐藏头部区域。
    /// Whether to hide the header section.
    /// </summary>
    [Parameter]
    public bool HideHeader { get; set; }

    /// <summary>
    /// 附加到根元素上的额外属性。
    /// Additional attributes applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 头部区域的自定义内容。
    /// Custom content for the header section.
    /// </summary>
    [Parameter]
    public RenderFragment? Header { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// The default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 操作按钮区域的自定义内容。
    /// Custom content for the actions section.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// 标题区域的自定义内容。
    /// Custom content for the title section.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }
}
