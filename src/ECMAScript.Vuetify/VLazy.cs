using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 懒加载组件，基于可视区域按需渲染内容。
/// Vuetify lazy component that renders content on demand based on viewport visibility.
/// </summary>
[VueLibraryComponent("vuetify/components", "VLazy")]
public sealed class VLazy : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 组件是否已激活（内容已渲染）。
    /// Whether the component is activated (content has been rendered).
    /// </summary>
    [Parameter]
    public bool ModelValue { get; set; }

    /// <summary>
    /// 激活状态变更时触发的回调。
    /// Callback invoked when the activation state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    /// <summary>
    /// 组件激活前的最小占位高度。
    /// Minimum placeholder height before the component is activated.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 交叉观察器的配置选项。
    /// Intersection observer configuration options.
    /// </summary>
    [Parameter]
    public VuetifyIntersectionObserverOptions? Options { get; set; }

    /// <summary>
    /// 组件的 HTML 标签名。
    /// HTML tag name for the component.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// 内容出现时的过渡动画。
    /// Transition animation when content appears.
    /// </summary>
    [Parameter]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 组件的高度。
    /// Height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 组件的最大高度。
    /// Maximum height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 组件的最大宽度。
    /// Maximum width of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 组件的最小宽度。
    /// Minimum width of the component.
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
    /// 捕获未匹配的额外 HTML 属性。
    /// Captures unmatched additional HTML attributes.
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
