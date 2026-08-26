using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

[ECMAScript("vuetify/components", Transform.Component, "VBadge")]
/// <summary>
/// Vuetify 徽章组件。
/// Vuetify badge component.
/// </summary>
public sealed class VBadge : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 过渡动画。
    /// Transition animation.
    /// </summary>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 渲染的根 HTML 元素标签名。
    /// Root HTML element tag name.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 组件的圆角大小。
    /// Border radius size.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Removes border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 组件在容器中的定位位置。
    /// Position within container.
    /// </summary>
    [Parameter]
    [ECMAScriptName("location")]
    public VuetifyLocation? Location { get; set; }

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
    /// 是否显示边框。
    /// Shows badge border.
    /// </summary>
    [Parameter]
    [ECMAScriptName("bordered")]
    public bool Bordered { get; set; }

    /// <summary>
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 徽章显示的内容。
    /// Content displayed in the badge.
    /// </summary>
    [Parameter]
    [ECMAScriptName("content")]
    public VueStringNumberValue? Content { get; set; }

    /// <summary>
    /// 是否显示为圆点。
    /// Shows as a dot.
    /// </summary>
    [Parameter]
    [ECMAScriptName("dot")]
    public bool Dot { get; set; }

    /// <summary>
    /// 浮动模式。
    /// Floating mode.
    /// </summary>
    [Parameter]
    [ECMAScriptName("floating")]
    public bool Floating { get; set; }

    /// <summary>
    /// 显示的图标。
    /// Icon to display.
    /// </summary>
    [Parameter]
    [ECMAScriptName("icon")]
    public VuetifyIconValue? Icon { get; set; }

    /// <summary>
    /// 是否行内显示。
    /// Displays inline.
    /// </summary>
    [Parameter]
    [ECMAScriptName("inline")]
    public bool Inline { get; set; }

    /// <summary>
    /// 徽章的无障碍标签。
    /// Accessibility label for the badge.
    /// </summary>
    [Parameter]
    [ECMAScriptName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// 最大值。
    /// Maximum value.
    /// </summary>
    [Parameter]
    [ECMAScriptName("max")]
    public VueStringNumberValue? Max { get; set; }

    /// <summary>
    /// 组件的模型值。
    /// Model value of the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("modelValue")]
    public bool ModelValue { get; set; } = true;

    /// <summary>
    /// 模型值变化时触发的事件。
    /// Event fired when model value changes.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onUpdate:modelValue")]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 水平偏移量。
    /// Horizontal offset.
    /// </summary>
    [Parameter]
    [ECMAScriptName("offsetX")]
    public VueStringNumberValue? OffsetX { get; set; }

    /// <summary>
    /// 垂直偏移量。
    /// Vertical offset.
    /// </summary>
    [Parameter]
    [ECMAScriptName("offsetY")]
    public VueStringNumberValue? OffsetY { get; set; }

    /// <summary>
    /// 徽章文字颜色。
    /// Badge text color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("textColor")]
    public string? TextColor { get; set; }

    /// <summary>
    /// 传递给根元素的额外 HTML 属性。
    /// Additional HTML attributes passed to root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 徽章插槽内容。
    /// Badge slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("badge")]
    public RenderFragment? BadgeContent { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
