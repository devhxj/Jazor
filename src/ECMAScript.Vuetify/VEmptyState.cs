using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 空状态创作代理，用于无数据和引导界面。
/// Vuetify empty-state authoring proxy for no-data and onboarding surfaces.
/// </summary>
[VueLibraryComponent("vuetify/components", "VEmptyState")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(ActionClick), VueEmitKind.LibrarySpecific, Name = "click:action")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
[VueSlot(nameof(Actions), Name = "actions")]
[VueSlot(nameof(HeadlineContent), Name = "headline")]
[VueSlot(nameof(TitleContent), Name = "title")]
[VueSlot(nameof(Media), Name = "media")]
[VueSlot(nameof(TextContent), Name = "text")]
public sealed class VEmptyState : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 组件主题名称。
    /// Component theme name.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 图标或媒体尺寸。
    /// Size of the icon or media.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 组件高度。
    /// Component height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件最大高度。
    /// Maximum height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件最大宽度。
    /// Maximum width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 组件最小高度。
    /// Minimum height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 组件最小宽度。
    /// Minimum width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件宽度。
    /// Component width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 操作按钮的文本。
    /// Text for the action button.
    /// </summary>
    [Parameter]
    public string? ActionText { get; set; }

    /// <summary>
    /// 组件背景色。
    /// Component background color.
    /// </summary>
    [Parameter]
    public string? BgColor { get; set; }

    /// <summary>
    /// 组件主题色。
    /// Component theme color.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 显示的图标。
    /// Icon to display.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? Icon { get; set; }

    /// <summary>
    /// 显示的图片 URL。
    /// Image URL to display.
    /// </summary>
    [Parameter]
    public string? Image { get; set; }

    /// <summary>
    /// 内容对齐方式。
    /// Content justification alignment.
    /// </summary>
    [Parameter]
    public VuetifyJustify? Justify { get; set; }

    /// <summary>
    /// 标题行文本。
    /// Headline text.
    /// </summary>
    [Parameter]
    public string? Headline { get; set; }

    /// <summary>
    /// 标题文本。
    /// Title text.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// 描述文本。
    /// Description text.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// 文本区域的宽度。
    /// Width of the text area.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? TextWidth { get; set; }

    /// <summary>
    /// 链接跳转地址。
    /// Link navigation URL.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// 路由导航目标。
    /// Router navigation target.
    /// </summary>
    [Parameter]
    public string? To { get; set; }

    /// <summary>
    /// 操作按钮点击时触发的事件回调。
    /// Event callback fired when the action button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<Event> ActionClick { get; set; }

    /// <summary>
    /// 附加到组件的额外 HTML 属性。
    /// Additional HTML attributes attached to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认子内容插槽。
    /// Default child content slot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// 操作区域插槽内容。
    /// Slot content for the actions area.
    /// </summary>
    [Parameter]
    public RenderFragment<VEmptyStateActionsSlotContext>? Actions { get; set; }

    /// <summary>
    /// 标题行插槽内容。
    /// Slot content for the headline area.
    /// </summary>
    [Parameter]
    public RenderFragment? HeadlineContent { get; set; }

    /// <summary>
    /// 标题插槽内容。
    /// Slot content for the title area.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    /// <summary>
    /// 媒体插槽内容。
    /// Slot content for the media area.
    /// </summary>
    [Parameter]
    public RenderFragment? Media { get; set; }

    /// <summary>
    /// 文本插槽内容。
    /// Slot content for the text area.
    /// </summary>
    [Parameter]
    public RenderFragment? TextContent { get; set; }
}
