using ECMAScript.VueContract;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 页脚组件。
/// Vuetify footer component.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VFooter")]
public sealed class VFooter : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 是否作为应用布局的一部分。
    /// Whether to include as part of the application layout.
    /// </summary>
    [Parameter]
    [ECMAScriptName("app")]
    public bool App { get; set; }

    /// <summary>
    /// 页脚的边框样式。
    /// Border style of the footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("border")]
    public VuetifyBorderValue? Border { get; set; }

    /// <summary>
    /// 页脚的主题颜色。
    /// Theme color of the footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 页脚的海拔阴影等级。
    /// Elevation shadow level of the footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 页脚的高度。
    /// Height of the footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 页脚的最大高度。
    /// Maximum height of the footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 页脚的最大宽度。
    /// Maximum width of the footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 页脚的最小高度。
    /// Minimum height of the footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 页脚的最小宽度。
    /// Minimum width of the footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 页脚的圆角样式。
    /// Border radius style of the footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 根元素使用的 HTML 标签。
    /// HTML tag used for the root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// 组件的名称，用于布局定位。
    /// Name of the component for layout positioning.
    /// </summary>
    [Parameter]
    [ECMAScriptName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 组件在布局中的排序顺序。
    /// Order of the component in the layout.
    /// </summary>
    [Parameter]
    [ECMAScriptName("order")]
    public VueStringNumberValue? Order { get; set; }

    /// <summary>
    /// 是否使用绝对定位。
    /// Whether to use absolute positioning.
    /// </summary>
    [Parameter]
    [ECMAScriptName("absolute")]
    public bool Absolute { get; set; }

    /// <summary>
    /// 是否使用固定定位。
    /// Whether to use fixed positioning.
    /// </summary>
    [Parameter]
    [ECMAScriptName("fixed")]
    public bool Fixed { get; set; }

    /// <summary>
    /// 页脚的宽度。
    /// Width of the footer.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 组件使用的主题名称。
    /// Theme name used by the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    [ECMAScriptName("additionalAttributes")]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    [ECMAScriptName("default")]
    public RenderFragment? ChildContent { get; set; }
}
