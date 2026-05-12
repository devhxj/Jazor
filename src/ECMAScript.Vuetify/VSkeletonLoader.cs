using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 骨架加载器组件的编写代理，用于加载占位符和延迟内容。
/// Vuetify skeleton-loader authoring proxy for loading placeholders and deferred content.
/// </summary>
[VueLibraryComponent("vuetify/components", "VSkeletonLoader")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
public sealed class VSkeletonLoader : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 组件主题名称。
    /// Theme name for the component.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// 海拔阴影高度。
    /// Elevation shadow level.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Elevation { get; set; }

    /// <summary>
    /// 组件高度。
    /// Component height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 最大高度。
    /// Maximum height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 最大宽度。
    /// Maximum width.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 最小高度。
    /// Minimum height.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 最小宽度。
    /// Minimum width.
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
    /// 是否渲染为纯 HTML 模板（无样式和交互）。
    /// Renders as plain HTML boilerplate without styles or interactivity.
    /// </summary>
    [Parameter]
    public bool Boilerplate { get; set; }

    /// <summary>
    /// 组件颜色。
    /// Component color.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 是否处于加载状态。
    /// Whether the component is in loading state.
    /// </summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>
    /// 加载时显示的文本。
    /// Text displayed while loading.
    /// </summary>
    [Parameter]
    public string? LoadingText { get; set; }

    /// <summary>
    /// 骨架加载器的预设类型。
    /// Preset skeleton loader type.
    /// </summary>
    [Parameter]
    public VuetifySkeletonLoaderTypeSetting? Type { get; set; }

    /// <summary>
    /// 附加的额外 HTML 属性。
    /// Additional unmatched HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <summary>
    /// 默认插槽内容。
    /// Default slot content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
