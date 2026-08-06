using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 系统栏组件的编写代理。
/// Vuetify system bar authoring proxy.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSystemBar")]
public sealed class VSystemBar : ComponentBase
{
    /// <summary>
    /// 系统栏的颜色。
    /// Color of the system bar.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 系统栏的高度。
    /// Height of the system bar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 是否使用窗口模式样式。
    /// Whether to use window mode styling.
    /// </summary>
    [Parameter]
    public bool Window { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 组件根元素的 HTML 标签名。
    /// HTML tag name for the component root element.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 系统栏的圆角大小。
    /// Border radius of the system bar.
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
    /// 布局中系统栏的名称。
    /// Name of the system bar within the layout.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// 系统栏在布局中的排序顺序。
    /// Order of the system bar within the layout.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Order { get; set; }

    /// <summary>
    /// 是否使用绝对定位。
    /// Whether to use absolute positioning.
    /// </summary>
    [Parameter]
    public bool Absolute { get; set; }

    /// <summary>
    /// 系统栏的阴影高度。
    /// Elevation shadow of the system bar.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 捕获未匹配的额外属性。
    /// Captures unmatched additional attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽，系统栏的内容。
    /// Default slot for the system bar content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
