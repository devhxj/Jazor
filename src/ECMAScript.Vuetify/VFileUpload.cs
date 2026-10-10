using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify labs 文件上传创作代理。
/// Vuetify labs file-upload authoring proxy.
/// </summary>
[ECMAScript("vuetify/components/VFileUpload")]
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
    /// <remarks data-authoring="types">C# union VuetifyRoundedValue?；值域为 bool | Number | string。数值用 Rounded="@(32)"；变量用 Rounded="@value"，无需 double 后缀。字符串用 Rounded="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// 组件的宽度。
    /// Width of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Width="@(32)"；变量用 Width="@value"，无需 double 后缀。字符串用 Width="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 组件的边框样式。
    /// Border style of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyBorderValue?；值域为 bool | Number | string。数值用 Border="@(32)"；变量用 Border="@value"，无需 double 后缀。字符串用 Border="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Length="@(32)"；变量用 Length="@value"，无需 double 后缀。字符串用 Length="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("length")]
    public VueStringNumberValue? Length { get; set; }

    /// <summary>
    /// 组件的透明度。
    /// Opacity of the component.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Opacity="@(32)"；变量用 Opacity="@value"，无需 double 后缀。字符串用 Opacity="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("opacity")]
    public VueStringNumberValue? Opacity { get; set; }

    /// <summary>
    /// 边框或分隔线的粗细。
    /// Thickness of the border or divider.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 Thickness="@(32)"；变量用 Thickness="@value"，无需 double 后缀。字符串用 Thickness="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 CloseDelay="@(32)"；变量用 CloseDelay="@value"，无需 double 后缀。字符串用 CloseDelay="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("closeDelay")]
    public VueStringNumberValue? CloseDelay { get; set; }

    /// <summary>
    /// 打开延迟的毫秒数。
    /// Open delay in milliseconds.
    /// </summary>
    /// <remarks data-authoring="types">C# union VueStringNumberValue?；值域为 double | string。数值用 OpenDelay="@(32)"；变量用 OpenDelay="@value"，无需 double 后缀。字符串用 OpenDelay="text"；数字字符串保持 string。投影：value?.AsNumber、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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
    /// <remarks data-authoring="types">C# union VuetifyIconValue?；值域为 bool | string | Symbol | VueProps。变量用 Icon="@value"，并保持声明的分支类型。字符串用 Icon="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString、value?.AsSymbol、value?.AsComponent；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("icon")]
    public VuetifyIconValue? Icon { get; set; }

    /// <summary>
    /// 文件上传的绑定值。
    /// Bound value of the file upload.
    /// </summary>
    /// <remarks data-authoring="types">C# union VuetifyFileModelValue?；值域为 FileRef | FileRef[]。变量用 ModelValue="@value"，并保持声明的分支类型。数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。投影：value?.AsFile、value?.AsFiles；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public VuetifyFileModelValue? ModelValue { get; set; }

    /// <summary>
    /// 文件上传绑定值变化时的回调。
    /// Callback when the file upload value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<FileRef[]?> ModelValueChanged { get; set; }

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
    /// <remarks data-authoring="types">C# union VuetifyScrimValue?；值域为 bool | string。变量用 Scrim="@value"，并保持声明的分支类型。字符串用 Scrim="text"；数字字符串保持 string。投影：value?.AsBool、value?.AsString；不匹配返回 null，0/false 保留。清空值由宿主组件的公开参数决定。</remarks>
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

