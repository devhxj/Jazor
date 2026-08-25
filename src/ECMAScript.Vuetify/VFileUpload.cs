using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify labs 文件上传创作代理。
/// Vuetify labs file-upload authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/labs/components", "VFileUpload")]
public sealed class VFileUpload : ComponentBase, IVuetifyComponent
{
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
    /// 组件的圆角样式。
    /// Border radius style of the component.
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
    /// 组件的位置。
    /// Position of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("position")]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 组件的对齐位置。
    /// Location alignment of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 组件的海拔阴影等级。
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
    /// 组件的宽度。
    /// Width of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 组件的边框样式。
    /// Border style of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 进度指示器或边框的长度。
    /// Length of the progress indicator or border.
    /// </summary>
    [Parameter]
    [ECMAScriptName("length")]
    public VueStringNumberValue? Length { get; set; }

    /// <summary>
    /// 组件的透明度。
    /// Opacity of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("opacity")]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 边框或分隔线的粗细。
    /// Thickness of the border or divider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("thickness")]
    public VueStringNumberValue? Thickness { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// Density of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("density")]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 关闭延迟的毫秒数。
    /// Close delay in milliseconds.
    /// </summary>
    [Parameter]
    [ECMAScriptName("closeDelay")]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 打开延迟的毫秒数。
    /// Open delay in milliseconds.
    /// </summary>
    [Parameter]
    [ECMAScriptName("openDelay")]
    public VueStringNumberValue? OpenDelay { get; set; }

    /// <summary>
    /// 浏览按钮的文本。
    /// Text for the browse button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("browseText")]
    public string? BrowseText { get; set; }

    /// <summary>
    /// 分隔线的文本。
    /// Text for the divider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("dividerText")]
    public string? DividerText { get; set; }

    /// <summary>
    /// 上传区域的标题文本。
    /// Title text of the upload area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 上传区域的副标题文本。
    /// Subtitle text of the upload area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("subtitle")]
    public string? Subtitle { get; set; }

    /// <summary>
    /// 上传区域显示的图标。
    /// Icon displayed in the upload area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("icon")]
    public VuetifyIconValue? Icon { get; set; }

    /// <summary>
    /// 文件上传的绑定值。
    /// Bound value of the file upload.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyFileModelValue? ModelValue { get; set; }

    /// <summary>
    /// 文件上传绑定值变化时的回调。
    /// Callback when the file upload value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<JazorFile[]?> ModelValueChanged { get; set; }

    /// <summary>
    /// 是否显示清除按钮。
    /// Whether the upload is clearable.
    /// </summary>
    [Parameter]
    [ECMAScriptName("clearable")]
    public bool Clearable { get; set; }

    /// <summary>
    /// 是否禁用文件上传。
    /// Whether the file upload is disabled.
    /// </summary>
    [Parameter]
    [ECMAScriptName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否隐藏浏览按钮。
    /// Whether to hide the browse button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("hideBrowse")]
    public bool HideBrowse { get; set; }

    /// <summary>
    /// 是否允许选择多个文件。
    /// Whether to allow selecting multiple files.
    /// </summary>
    [Parameter]
    [ECMAScriptName("multiple")]
    public bool Multiple { get; set; }

    /// <summary>
    /// 遮罩层样式。
    /// Scrim style of the overlay.
    /// </summary>
    [Parameter]
    [ECMAScriptName("scrim")]
    public VuetifyScrimValue? Scrim { get; set; }

    /// <summary>
    /// 是否显示文件大小。
    /// Whether to show file sizes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("showSize")]
    public bool ShowSize { get; set; }

    /// <summary>
    /// 文件输入的 name 属性。
    /// Name attribute of the file input.
    /// </summary>
    [Parameter]
    [ECMAScriptName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 浏览按钮插槽内容。
    /// Slot content for the browse button.
    /// </summary>
    [Parameter]
    [ECMAScriptName("browse")]
    public RenderFragment<VFileUploadBrowseSlotContext>? Browse { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 图标插槽内容。
    /// Slot content for the icon.
    /// </summary>
    [Parameter]
    [ECMAScriptName("icon")]
    public RenderFragment? IconContent { get; set; }

    /// <summary>
    /// 输入插槽内容。
    /// Slot content for the input.
    /// </summary>
    [Parameter]
    [ECMAScriptName("input")]
    public RenderFragment<VFileUploadInputSlotContext>? InputContent { get; set; }

    /// <summary>
    /// 文件项插槽内容。
    /// Slot content for each file item.
    /// </summary>
    [Parameter]
    [ECMAScriptName("item")]
    public RenderFragment<VFileUploadItemSlotContext>? ItemContent { get; set; }

    /// <summary>
    /// 标题插槽内容。
    /// Slot content for the title.
    /// </summary>
    [Parameter]
    [ECMAScriptName("title")]
    public RenderFragment? TitleContent { get; set; }

    /// <summary>
    /// 分隔线插槽内容。
    /// Slot content for the divider.
    /// </summary>
    [Parameter]
    [ECMAScriptName("divider")]
    public RenderFragment? DividerContent { get; set; }
}
