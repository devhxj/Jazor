using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[VueLibraryComponent("vuetify/components", "VBadge")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueProp(nameof(CssClass), Name = "class")]
[VueProp(nameof(CssStyle), Name = "style")]
[VueLibraryEmit(nameof(ModelValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
[VueSlot(nameof(BadgeContent), Name = "badge")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
/// <summary>
/// Vuetify 徽章组件。
/// Vuetify badge component.
/// </summary>
public sealed class VBadge : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 过渡动画。
    /// Transition animation.
    /// </summary>
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

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
    /// 组件在容器中的定位位置。
    /// Position within container.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 自定义 CSS 类。
    /// Custom CSS class(es).
    /// </summary>
    [Parameter]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 自定义行内样式。
    /// Custom inline style(s).
    /// </summary>
    [Parameter]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 是否显示边框。
    /// Shows badge border.
    /// </summary>
    [Parameter]
    public bool Bordered { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 徽章显示的内容。
    /// Content displayed in the badge.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Content { get; set; }

    /// <summary>
    /// 是否显示为圆点。
    /// Shows as a dot.
    /// </summary>
    [Parameter]
    public bool Dot { get; set; }

    /// <summary>
    /// 浮动模式。
    /// Floating mode.
    /// </summary>
    [Parameter]
    public bool Floating { get; set; }

    /// <summary>
    /// 显示的图标。
    /// Icon to display.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? Icon { get; set; }

    /// <summary>
    /// 是否行内显示。
    /// Displays inline.
    /// </summary>
    [Parameter]
    public bool Inline { get; set; }

    /// <summary>
    /// 徽章的无障碍标签。
    /// Accessibility label for the badge.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// 最大值。
    /// Maximum value.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Max { get; set; }

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
    /// 水平偏移量。
    /// Horizontal offset.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OffsetX { get; set; }

    /// <summary>
    /// 垂直偏移量。
    /// Vertical offset.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? OffsetY { get; set; }

    /// <summary>
    /// 徽章文字颜色。
    /// Badge text color.
    /// </summary>
    [Parameter]
    public string? TextColor { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 徽章插槽内容。
    /// Badge slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? BadgeContent { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
