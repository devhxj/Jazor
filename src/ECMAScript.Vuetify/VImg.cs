using Microsoft.AspNetCore.Components;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 图片组件，支持懒加载、宽高比和响应式源。
/// Vuetify image component with lazy loading, aspect ratio, and responsive sources.
/// </summary>
[VueLibraryComponent("vuetify/components", "VImg")]
public sealed class VImg : ComponentBase, IVuetifyComponent
{
    /// <summary>
    /// 图片的源地址或源对象。
    /// Image source URL or source object.
    /// </summary>
    [Parameter]
    [ECMAScriptName("src")]
    public VImgSource? Src { get; set; }

    /// <summary>
    /// 图片的替代文本。
    /// Alt text for the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("alt")]
    public string? Alt { get; set; }

    /// <summary>
    /// 图片加载前显示的懒加载占位图地址。
    /// Lazy-load placeholder image URL shown before the main image loads.
    /// </summary>
    [Parameter]
    [ECMAScriptName("lazySrc")]
    public string? LazySrc { get; set; }

    /// <summary>
    /// 响应式图片的 srcset 属性。
    /// Srcset attribute for responsive images.
    /// </summary>
    [Parameter]
    [ECMAScriptName("srcset")]
    public string? Srcset { get; set; }

    /// <summary>
    /// 响应式图片的 sizes 属性。
    /// Sizes attribute for responsive images.
    /// </summary>
    [Parameter]
    [ECMAScriptName("sizes")]
    public string? Sizes { get; set; }

    /// <summary>
    /// 图片的高度。
    /// Height of the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("height")]
    public VueStringNumberValue? Height { get; set; }

    /// <summary>
    /// 图片的宽度。
    /// Width of the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("width")]
    public VueStringNumberValue? Width { get; set; }

    /// <summary>
    /// 图片的最大高度。
    /// Maximum height of the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxHeight")]
    public VueStringNumberValue? MaxHeight { get; set; }

    /// <summary>
    /// 图片的最大宽度。
    /// Maximum width of the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("maxWidth")]
    public VueStringNumberValue? MaxWidth { get; set; }

    /// <summary>
    /// 图片的最小高度。
    /// Minimum height of the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minHeight")]
    public VueStringNumberValue? MinHeight { get; set; }

    /// <summary>
    /// 图片的最小宽度。
    /// Minimum width of the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("minWidth")]
    public VueStringNumberValue? MinWidth { get; set; }

    /// <summary>
    /// 图片的宽高比。
    /// Aspect ratio of the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("aspectRatio")]
    public VueStringNumberValue? AspectRatio { get; set; }

    /// <summary>
    /// 图片加载时的过渡动画。
    /// Transition animation when the image loads.
    /// </summary>
    [Parameter]
    [ECMAScriptName("transition")]
    public VuetifyTransitionValue? Transition { get; set; }

    /// <summary>
    /// 是否裁剪图片以填充容器。
    /// Whether to crop the image to fill the container.
    /// </summary>
    [Parameter]
    [ECMAScriptName("cover")]
    public bool Cover { get; set; }

    /// <summary>
    /// 是否在初始渲染时立即加载图片。
    /// Whether to load the image eagerly on initial render.
    /// </summary>
    [Parameter]
    [ECMAScriptName("eager")]
    public bool Eager { get; set; }

    /// <summary>
    /// 是否移除圆角。
    /// Whether to remove border radius.
    /// </summary>
    [Parameter]
    [ECMAScriptName("tile")]
    public bool Tile { get; set; }

    /// <summary>
    /// 是否以内联方式显示图片。
    /// Whether to display the image inline.
    /// </summary>
    [Parameter]
    [ECMAScriptName("inline")]
    public bool Inline { get; set; }

    /// <summary>
    /// 是否使用绝对定位。
    /// Whether to use absolute positioning.
    /// </summary>
    [Parameter]
    [ECMAScriptName("absolute")]
    public bool Absolute { get; set; }

    /// <summary>
    /// 图片的圆角样式。
    /// Border radius style of the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("rounded")]
    public VuetifyRoundedValue? Rounded { get; set; }

    /// <summary>
    /// 应用于图片根元素的 CSS 类。
    /// CSS classes applied to the image root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("class")]
    public VueClassValue? CssClass { get; set; }

    /// <summary>
    /// 应用于图片根元素的行内样式。
    /// Inline styles applied to the image root element.
    /// </summary>
    [Parameter]
    [ECMAScriptName("style")]
    public VuetifyStyleValue? CssStyle { get; set; }

    /// <summary>
    /// 应用于图片内容区域的 CSS 类。
    /// CSS classes applied to the image content area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("contentClass")]
    public VueClassValue? ContentClass { get; set; }

    /// <summary>
    /// 占位区域的主题颜色。
    /// Theme color of the placeholder area.
    /// </summary>
    [Parameter]
    [ECMAScriptName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// 应用于图片的渐变效果。
    /// Gradient effect applied to the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("gradient")]
    public string? Gradient { get; set; }

    /// <summary>
    /// IntersectionObserver 的配置选项。
    /// IntersectionObserver options for lazy loading.
    /// </summary>
    [Parameter]
    [ECMAScriptName("options")]
    public VuetifyIntersectionObserverOptions? Options { get; set; }

    /// <summary>
    /// 图片的定位方式。
    /// Position of the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("position")]
    public string? Position { get; set; }

    /// <summary>
    /// 图片是否可拖拽。
    /// Whether the image is draggable.
    /// </summary>
    [Parameter]
    [ECMAScriptName("draggable")]
    public VImgDraggableValue? Draggable { get; set; }

    /// <summary>
    /// 图片的跨域策略。
    /// Cross-origin policy of the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("crossorigin")]
    public VImgCrossOrigin? CrossOrigin { get; set; }

    /// <summary>
    /// 图片的引用者策略。
    /// Referrer policy of the image.
    /// </summary>
    [Parameter]
    [ECMAScriptName("referrerpolicy")]
    public VImgReferrerPolicy? ReferrerPolicy { get; set; }

    /// <summary>
    /// 图片开始加载时触发的事件回调。
    /// Event callback fired when the image starts loading.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onLoadstart")]
    public EventCallback<string?> OnLoadStart { get; set; }

    /// <summary>
    /// 图片加载完成时触发的事件回调。
    /// Event callback fired when the image has loaded.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onLoad")]
    public EventCallback<string?> OnLoad { get; set; }

    /// <summary>
    /// 图片加载失败时触发的事件回调。
    /// Event callback fired when the image fails to load.
    /// </summary>
    [Parameter]
    [ECMAScriptName("onError")]
    public EventCallback<string?> OnError { get; set; }

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

    /// <summary>
    /// 图片加载时的占位插槽内容。
    /// Slot content shown while the image is loading.
    /// </summary>
    [Parameter]
    [ECMAScriptName("placeholder")]
    public RenderFragment? Placeholder { get; set; }

    /// <summary>
    /// 图片加载失败时的插槽内容。
    /// Slot content shown when the image fails to load.
    /// </summary>
    [Parameter]
    [ECMAScriptName("error")]
    public RenderFragment? ErrorContent { get; set; }

    /// <summary>
    /// 额外图片源插槽内容。
    /// Slot content for additional image sources.
    /// </summary>
    [Parameter]
    [ECMAScriptName("sources")]
    public RenderFragment? Sources { get; set; }
}
