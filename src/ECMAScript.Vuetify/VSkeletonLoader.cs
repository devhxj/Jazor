using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 骨架加载器组件的编写代理，用于加载占位符和延迟内容。
/// Vuetify skeleton-loader authoring proxy for loading placeholders and deferred content.
/// </summary>
[ECMAScript("vuetify/components", Transform.Component, "VSkeletonLoader")]
public sealed class VSkeletonLoader : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 组件主题名称。
    /// Theme name for the component.
    /// </summary>
    [Parameter]
    [ECMAScriptName("theme")]
    public string? Theme { get; set; }

    /// <summary>
    /// 海拔阴影高度。
    /// Elevation shadow level.
    /// </summary>
    [Parameter]
    [ECMAScriptName("elevation")]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件高度。
    /// Component height.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 最大高度。
    /// Maximum height.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 最大宽度。
    /// Maximum width.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 最小高度。
    /// Minimum height.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 最小宽度。
    /// Minimum width.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 组件宽度。
    /// Component width.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 是否渲染为纯 HTML 模板（无样式和交互）。
    /// Renders as plain HTML boilerplate without styles or interactivity.
    /// </summary>
    [Parameter]
    [ECMAScriptName("boilerplate")]
    public bool Boilerplate { get; set; }

    /// <summary>
    /// 组件颜色。
    /// Component color.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 是否处于加载状态。
    /// Whether the component is in loading state.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loading")]
    public bool Loading { get; set; }

    /// <summary>
    /// 加载时显示的文本。
    /// Text displayed while loading.
    /// </summary>
    [Parameter]
    [ECMAScriptName("loadingText")]
    public string? LoadingText { get; set; }

    /// <summary>
    /// 骨架加载器的预设类型。
    /// Preset skeleton loader type.
    /// </summary>
    [Parameter]
    [ECMAScriptName("type")]
    public VuetifySkeletonLoaderTypeSetting? Type { get; set; }

    /// <summary>
    /// 附加的额外 HTML 属性。
    /// Additional unmatched HTML attributes.
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
