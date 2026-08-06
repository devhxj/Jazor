using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VAlert", StyleUrls = [VuetifyLibraryAssets.StyleUrl])]
[VueLibraryEmit(nameof(OnClickClose), Name = "click:close")]
/// <summary>
/// Vuetify 警告提示组件。
/// Vuetify alert component.
/// </summary>
public sealed class VAlert : ComponentBase
{
    /// <summary>
    /// 组件的模型值。
    /// Model value of the component.
    /// </summary>
    [Parameter]
    public bool ModelValue { get; set; } = true;

    /// <summary>
    /// 模型值变化时触发的事件。
    /// Event fired when model value changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 警告的类型（如 success、info、warning、error）。
    /// Alert type (e.g. success, info, warning, error).
    /// </summary>
    [Parameter]
    public VuetifyAlertType? Type { get; set; }

    /// <summary>
    /// 组件的视觉变体样式。
    /// Visual variant style.
    /// </summary>
    [Parameter]
    public VuetifyVariant? Variant { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 渲染的根 HTML 元素标签名。
    /// Root HTML element tag name.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 组件的圆角大小。
    /// Border radius size.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Removes border radius.
    /// </summary>
    [Parameter]
    public bool Tile { get; set; }

    /// <summary>
    /// 组件的 CSS 定位策略。
    /// CSS position strategy.
    /// </summary>
    [Parameter]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 组件在容器中的定位位置。
    /// Position within container.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 组件的阴影高度级别。
    /// Elevation shadow level.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件的高度。
    /// Height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

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
    /// 组件的宽度。
    /// Width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// Component density level.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 自定义 CSS 类。
    /// Custom CSS class(es).
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 自定义行内样式。
    /// Custom inline style(s).
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 边框设置。
    /// Border configuration.
    /// </summary>
    [Parameter]
    public VuetifyAlertBorderValue? Border { get; set; }

    /// <summary>
    /// 边框颜色。
    /// Border color.
    /// </summary>
    [Parameter]
    public string? BorderColor { get; set; }

    /// <summary>
    /// 显示关闭按钮。
    /// Shows close button.
    /// </summary>
    [Parameter]
    public bool Closable { get; set; }

    /// <summary>
    /// 关闭按钮图标。
    /// Close button icon.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? CloseIcon { get; set; }

    /// <summary>
    /// 关闭按钮的无障碍标签。
    /// Accessibility label for close button.
    /// </summary>
    [Parameter]
    public string? CloseLabel { get; set; }

    /// <summary>
    /// 显示的图标。
    /// Icon to display.
    /// </summary>
    [Parameter]
    public VuetifyAlertIconValue? Icon { get; set; }

    /// <summary>
    /// 突出显示模式。
    /// Prominent display mode.
    /// </summary>
    [Parameter]
    public bool Prominent { get; set; }

    /// <summary>
    /// 标题文本。
    /// Title text.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// 文本内容。
    /// Text content.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// 点击关闭按钮时触发的事件。
    /// Event fired when close button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEvent> OnClickClose { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 前置插槽内容。
    /// Prepend slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? Prepend { get; set; }

    /// <summary>
    /// 标题插槽内容。
    /// Title slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    /// <summary>
    /// 文本插槽内容。
    /// Text slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? TextContent { get; set; }

    /// <summary>
    /// 后追加插槽内容。
    /// Append slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? Append { get; set; }

    /// <summary>
    /// 关闭按钮插槽内容。
    /// Close slot content.
    /// </summary>
    [Parameter]
    public RenderFragment<VAlertCloseSlotContext>? Close { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
