using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// 第一波 Vuetify 卡片存根，用于子内容组合。
/// First-wave Vuetify card stub for child-content composition.
/// </summary>
[VueLibraryComponent("vuetify/components", "VCard")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueSlot(nameof(TextContent), Name = "text")]
[VueSlot(nameof(TitleContent), Name = "title")]
[VueSlot(nameof(SubtitleContent), Name = "subtitle")]
[VueSlot(nameof(ImageContent), Name = "image")]
[VueSlot(nameof(Prepend), Name = "prepend")]
[VueSlot(nameof(Append), Name = "append")]
[VueSlot(nameof(Actions), Name = "actions")]
[VueSlot(nameof(Item), Name = "item")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class VCard : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 标题文本。
    /// Title text.
    /// </summary>
    [Parameter]
    public VuetifyTextValue? Title { get; set; }

    /// <summary>
    /// 副标题文本。
    /// Subtitle text.
    /// </summary>
    [Parameter]
    public VuetifyTextValue? Subtitle { get; set; }

    /// <summary>
    /// 文本内容。
    /// Text content.
    /// </summary>
    [Parameter]
    public VuetifyTextValue? Text { get; set; }

    /// <summary>
    /// 前追加的图标。
    /// Icon appended to front.
    /// </summary>
    [Parameter]
    public string? PrependIcon { get; set; }

    /// <summary>
    /// 后追加的图标。
    /// Icon appended to end.
    /// </summary>
    [Parameter]
    public string? AppendIcon { get; set; }

    /// <summary>
    /// 前追加的头像。
    /// Avatar appended to front.
    /// </summary>
    [Parameter]
    public string? PrependAvatar { get; set; }

    /// <summary>
    /// 后追加的头像。
    /// Avatar appended to end.
    /// </summary>
    [Parameter]
    public string? AppendAvatar { get; set; }

    /// <summary>
    /// 卡片图片 URL。
    /// Card image URL.
    /// </summary>
    [Parameter]
    public string? Image { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 组件的视觉变体样式。
    /// Visual variant style.
    /// </summary>
    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// Component density level.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 组件的阴影高度级别。
    /// Elevation shadow level.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件的圆角大小。
    /// Border radius size.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 组件的高度。
    /// Height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件的宽度。
    /// Width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 组件的最小高度。
    /// Min height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 组件的最小宽度。
    /// Min width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件的最大高度。
    /// Max height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件的最大宽度。
    /// Max width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 是否禁用组件。
    /// Disables the component.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否移除阴影效果。
    /// Removes box-shadow.
    /// </summary>
    [Parameter]
    public bool Flat { get; set; }

    /// <summary>
    /// 是否在悬停时显示阴影效果。
    /// Shows elevation on hover.
    /// </summary>
    [Parameter]
    public bool Hover { get; set; }

    /// <summary>
    /// 是否渲染为链接。
    /// Renders as a link.
    /// </summary>
    [Parameter]
    public bool Link { get; set; }

    /// <summary>
    /// 链接目标 URL。
    /// Link target URL.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// 路由跳转目标。
    /// Router link destination.
    /// </summary>
    [Parameter]
    public string? To { get; set; }

    /// <summary>
    /// 是否使用 replace 而非 push 跳转。
    /// Uses replace instead of push.
    /// </summary>
    [Parameter]
    public bool Replace { get; set; }

    /// <summary>
    /// 是否精确匹配路由。
    /// Exact route match.
    /// </summary>
    [Parameter]
    public bool Exact { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 文本插槽内容。
    /// Text slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? TextContent { get; set; }

    /// <summary>
    /// 标题插槽内容。
    /// Title slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    /// <summary>
    /// 副标题插槽内容。
    /// Subtitle slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? SubtitleContent { get; set; }

    /// <summary>
    /// 图片插槽内容。
    /// Image slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ImageContent { get; set; }

    /// <summary>
    /// 前置插槽内容。
    /// Prepend slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? Prepend { get; set; }

    /// <summary>
    /// 后追加插槽内容。
    /// Append slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? Append { get; set; }

    /// <summary>
    /// 操作按钮插槽内容。
    /// Actions slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// 列表项插槽内容。
    /// Item slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? Item { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
