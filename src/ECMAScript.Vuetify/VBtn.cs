using ECMAScript.VueContract;
using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 按钮创作代理。
/// Vuetify button authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VBtn")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueSlot(nameof(Prepend), Name = "prepend")]
[VueSlot(nameof(Append), Name = "append")]
[VueSlot(nameof(Loader), Name = "loader")]
[VueSlot(nameof(ChildContent), IsDefault = true)]
public sealed class VBtn : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 激活状态。
    /// Active state.
    /// </summary>
    [Parameter]
    public bool Active { get; set; }

    /// <summary>
    /// 激活状态颜色。
    /// Color when active.
    /// </summary>
    [Parameter]
    public string? ActiveColor { get; set; }

    /// <summary>
    /// 激活状态下是否只读。
    /// Read-only when active.
    /// </summary>
    [Parameter]
    public bool ActiveReadonly { get; set; }

    /// <summary>
    /// 基础颜色。
    /// Base color.
    /// </summary>
    [Parameter]
    public string? BaseColor { get; set; }

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
    /// 组件尺寸。
    /// Component size.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Size { get; set; }

    /// <summary>
    /// 加载状态。
    /// Loading state.
    /// </summary>
    [Parameter]
    public VuetifyBooleanStringValue? Loading { get; set; }

    /// <summary>
    /// 占据全部可用宽度。
    /// Fills available width.
    /// </summary>
    [Parameter]
    public bool Block { get; set; }

    /// <summary>
    /// 边框设置。
    /// Border configuration.
    /// </summary>
    [Parameter]
    public VuetifyBorderValue? Border { get; set; }

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
    /// 组件的圆角大小。
    /// Border radius size.
    /// </summary>
    [Parameter]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 组件的阴影高度级别。
    /// Elevation shadow level.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 是否精确匹配路由。
    /// Exact route match.
    /// </summary>
    [Parameter]
    public bool Exact { get; set; }

    /// <summary>
    /// 链接目标 URL。
    /// Link target URL.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// 链接打开目标。
    /// Link target attribute.
    /// </summary>
    [Parameter]
    public string? Target { get; set; }

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
    /// 显示的图标。
    /// Icon to display.
    /// </summary>
    [Parameter]
    public VuetifyIconValue? Icon { get; set; }

    /// <summary>
    /// 减小内边距。
    /// Reduced padding.
    /// </summary>
    [Parameter]
    public bool Slim { get; set; }

    /// <summary>
    /// 堆叠排列内容。
    /// Stacks content vertically.
    /// </summary>
    [Parameter]
    public bool Stacked { get; set; }

    /// <summary>
    /// 符号模式渲染。
    /// Symbol mode rendering.
    /// </summary>
    [Parameter]
    public bool Symbol { get; set; }

    /// <summary>
    /// 组件的紧凑程度。
    /// Component density level.
    /// </summary>
    [Parameter]
    public VuetifyDensity? Density { get; set; }

    /// <summary>
    /// 组件在容器中的定位位置。
    /// Position within container.
    /// </summary>
    [Parameter]
    public VuetifyLocation? Location { get; set; }

    /// <summary>
    /// 组件的 CSS 定位策略。
    /// CSS position strategy.
    /// </summary>
    [Parameter]
    public VuetifyPosition? Position { get; set; }

    /// <summary>
    /// 渲染的根 HTML 元素标签名。
    /// Root HTML element tag name.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 类型属性。
    /// Type attribute.
    /// </summary>
    [Parameter]
    public string? Type { get; set; }

    /// <summary>
    /// 组件值。
    /// Component value.
    /// </summary>
    [Parameter]
    public VueValue? Value { get; set; }

    /// <summary>
    /// 波纹点击效果。
    /// Ripple click effect.
    /// </summary>
    [Parameter]
    public VuetifyRippleValue? Ripple { get; set; }

    /// <summary>
    /// 点击事件。
    /// Click event.
    /// </summary>
    [Parameter]
    public EventCallback OnClick { get; set; }

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
    /// 后追加插槽内容。
    /// Append slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? Append { get; set; }

    /// <summary>
    /// 加载插槽内容。
    /// Loader slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? Loader { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
