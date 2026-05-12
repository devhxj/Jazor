using ECMAScript.VueContract.Descriptor;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 无限滚动创作代理，用于增量列表加载。
/// Vuetify infinite-scroll authoring proxy for incremental list loading.
/// </summary>
[VueLibraryComponent("vuetify/components", "VInfiniteScroll")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
[VueLibraryEmit(nameof(Load), VueEmitKind.LibrarySpecific, Name = "load")]
[VueLibrarySlot(nameof(ChildContent), IsDefault = true)]
[VueLibrarySlot(nameof(Loading), Name = "loading")]
[VueLibrarySlot(nameof(Error), Name = "error")]
[VueLibrarySlot(nameof(Empty), Name = "empty")]
[VueLibrarySlot(nameof(LoadMore), Name = "load-more")]
public sealed class VInfiniteScroll : ComponentBase, IVueLibraryComponent
{
    /// <summary>
    /// 根元素使用的 HTML 标签。
    /// HTML tag used for the root element.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

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
    /// 组件的最小高度。
    /// Minimum height of the component.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? MinHeight { get; set; }

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
    /// 组件的主题颜色。
    /// Theme color of the component.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// 滚动加载的方向。
    /// Direction of infinite scroll loading.
    /// </summary>
    [Parameter]
    public VuetifyInputDirection? Direction { get; set; }

    /// <summary>
    /// 触发加载的滚动侧。
    /// Side of the scroll that triggers loading.
    /// </summary>
    [Parameter]
    public VuetifyInfiniteScrollSide? Side { get; set; }

    /// <summary>
    /// 无限滚动的加载模式。
    /// Loading mode of the infinite scroll.
    /// </summary>
    [Parameter]
    public VuetifyInfiniteScrollMode? Mode { get; set; }

    /// <summary>
    /// 触发加载的边距距离。
    /// Margin distance that triggers loading.
    /// </summary>
    [Parameter]
    public VueStringNumberValue? Margin { get; set; }

    /// <summary>
    /// "加载更多"按钮的文本。
    /// Text for the load-more button.
    /// </summary>
    [Parameter]
    public string? LoadMoreText { get; set; }

    /// <summary>
    /// 无数据时的空状态文本。
    /// Text shown when there is no more data.
    /// </summary>
    [Parameter]
    public string? EmptyText { get; set; }

    /// <summary>
    /// 需要加载更多数据时触发的事件回调。
    /// Event callback fired when more data needs to be loaded.
    /// </summary>
    [Parameter]
    public EventCallback<VInfiniteScrollLoadOptions> Load { get; set; }

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

    /// <summary>
    /// 加载中状态的插槽内容。
    /// Slot content shown during loading.
    /// </summary>
    [Parameter]
    public RenderFragment<VInfiniteScrollSlotContext>? Loading { get; set; }

    /// <summary>
    /// 加载错误状态的插槽内容。
    /// Slot content shown on load error.
    /// </summary>
    [Parameter]
    public RenderFragment<VInfiniteScrollSlotContext>? Error { get; set; }

    /// <summary>
    /// 数据为空时的插槽内容。
    /// Slot content shown when data is empty.
    /// </summary>
    [Parameter]
    public RenderFragment<VInfiniteScrollSlotContext>? Empty { get; set; }

    /// <summary>
    /// "加载更多"按钮的插槽内容。
    /// Slot content for the load-more button.
    /// </summary>
    [Parameter]
    public RenderFragment<VInfiniteScrollSlotContext>? LoadMore { get; set; }
}
