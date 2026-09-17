using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// 鼠标事件来源；字符串值与 VueUse <c>UseMouseSourceType</c> 值域一致。
/// The mouse event source; values mirror the VueUse <c>UseMouseSourceType</c> domain.
/// </summary>
[String]
public enum VueUseMouseSourceType
{
    /// <summary>鼠标事件。A mouse event.</summary>
    [Description("@#mouse")]
    Mouse,

    /// <summary>触摸事件。A touch event.</summary>
    [Description("@#touch")]
    Touch
}

/// <summary>
/// 鼠标坐标的取值基准；字符串值与 VueUse <c>UseMouseCoordType</c> 值域一致。
/// The coordinate basis used by <c>useMouse</c>; values mirror the VueUse <c>UseMouseCoordType</c> domain.
/// </summary>
[String]
public enum VueUseMouseCoordType
{
    /// <summary>相对整个文档。Relative to the whole document.</summary>
    [Description("@#page")]
    Page,

    /// <summary>相对视口。Relative to the viewport.</summary>
    [Description("@#client")]
    Client,

    /// <summary>相对物理屏幕。Relative to the physical screen.</summary>
    [Description("@#screen")]
    Screen,

    /// <summary>相对上一次鼠标位置。Relative to the previous mouse position.</summary>
    [Description("@#movement")]
    Movement
}

/// <summary>
/// 偏好配色方案；字符串值与 VueUse <c>ColorSchemeType</c> 值域一致。
/// The preferred color scheme; values mirror the VueUse <c>ColorSchemeType</c> domain.
/// </summary>
[String]
public enum VueUseColorScheme
{
    /// <summary>深色方案。The dark scheme.</summary>
    [Description("@#dark")]
    Dark,

    /// <summary>浅色方案。The light scheme.</summary>
    [Description("@#light")]
    Light,

    /// <summary>未声明偏好。No declared preference.</summary>
    [Description("@#no-preference")]
    NoPreference
}

/// <summary>
/// 基础配色模式；字符串值与 VueUse <c>BasicColorMode</c> 值域一致。
/// The basic color mode; values mirror the VueUse <c>BasicColorMode</c> domain.
/// </summary>
[String]
public enum VueUseColorMode
{
    /// <summary>深色模式。Dark mode.</summary>
    [Description("@#dark")]
    Dark,

    /// <summary>浅色模式。Light mode.</summary>
    [Description("@#light")]
    Light,

    /// <summary>跟随系统。Follow the system preference.</summary>
    [Description("@#auto")]
    Auto
}

/// <summary>
/// 文档可见性状态；字符串值与浏览器 <c>DocumentVisibilityState</c> 一致。
/// The document visibility state; values match the browser <c>DocumentVisibilityState</c>.
/// </summary>
[String]
public enum VueUseDocumentVisibility
{
    /// <summary>文档可见。The document is visible.</summary>
    [Description("@#visible")]
    Visible,

    /// <summary>文档隐藏。The document is hidden.</summary>
    [Description("@#hidden")]
    Hidden
}

/// <summary>
/// 网络有效连接类型；字符串值与浏览器 Network Information API 一致。
/// The effective network connection type; values match the browser Network Information API.
/// </summary>
[String]
public enum VueUseNetworkEffectiveType
{
    /// <summary>慢速 2G。Slow 2G.</summary>
    [Description("@#slow-2g")]
    Slow2G,

    /// <summary>2G。2G.</summary>
    [Description("@#2g")]
    TwoG,

    /// <summary>3G。3G.</summary>
    [Description("@#3g")]
    ThreeG,

    /// <summary>4G。4G.</summary>
    [Description("@#4g")]
    FourG
}

/// <summary>
/// 可作为 VueUse 元素目标的引用：DOM 元素、已挂载组件公共实例，或它们的只读引用。
/// A VueUse element target: a DOM element, a mounted component public instance, or a readonly ref to either.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VueUseMaybeElement(
    Element,
    Vue.VueComponentPublicInstance,
    Vue.VueReadonlyRef<Element>,
    Vue.VueReadonlyRef<Vue.VueComponentPublicInstance>);

/// <summary>
/// <c>useMouse()</c> 的选项对象。
/// Options for <c>useMouse()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseMouseOptions
{
    /// <summary>坐标取值基准。The coordinate basis.</summary>
    [Description("@#type")]
    public VueUseMouseCoordType? Type { get; init; }

    /// <summary>监听触摸事件。Whether to also track touch events.</summary>
    [Description("@#touch")]
    public bool? Touch { get; init; }

    /// <summary>滚动时更新坐标。Whether to update on scroll.</summary>
    [Description("@#scroll")]
    public bool? Scroll { get; init; }

    /// <summary>触摸结束时重置为初始值。Whether to reset to the initial value when a touch ends.</summary>
    [Description("@#resetOnTouchEnds")]
    public bool? ResetOnTouchEnds { get; init; }
}

/// <summary>
/// <c>useMouseInElement()</c> 的选项对象。
/// Options for <c>useMouseInElement()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseMouseInElementOptions
{
    /// <summary>坐标取值基准。The coordinate basis.</summary>
    [Description("@#type")]
    public VueUseMouseCoordType? Type { get; init; }

    /// <summary>鼠标位于目标元素之外时是否继续处理事件。Whether to keep handling events while the cursor is outside the target.</summary>
    [Description("@#handleOutside")]
    public bool? HandleOutside { get; init; }

    /// <summary>监听触摸事件。Whether to also track touch events.</summary>
    [Description("@#touch")]
    public bool? Touch { get; init; }

    /// <summary>滚动时更新坐标。Whether to update on scroll.</summary>
    [Description("@#scroll")]
    public bool? Scroll { get; init; }

    /// <summary>触摸结束时重置为初始值。Whether to reset to the initial value when a touch ends.</summary>
    [Description("@#resetOnTouchEnds")]
    public bool? ResetOnTouchEnds { get; init; }
}

/// <summary>
/// <c>useMouse()</c> 的返回值：响应式坐标与事件来源。
/// The <c>useMouse()</c> return value: reactive coordinates and the event source.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseMouseReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseMouseReturn()
    {
    }

    /// <summary>鼠标的水平坐标。The mouse x coordinate.</summary>
    [Description("@#x")]
    public extern Vue.VueShallowRef<Number> X { get; }

    /// <summary>鼠标的垂直坐标。The mouse y coordinate.</summary>
    [Description("@#y")]
    public extern Vue.VueShallowRef<Number> Y { get; }

    /// <summary>最后一次事件来源；尚未产生事件时为 undefined。The last event source, undefined before any event.</summary>
    [Description("@#sourceType")]
    public extern Vue.VueShallowRef<VueUseMouseSourceType?> SourceType { get; }
}

/// <summary>
/// <c>useMouseInElement()</c> 的返回值：在 <see cref="VueUseMouseReturn"/> 之上追加元素内相对位置与尺寸。
/// The <c>useMouseInElement()</c> return value, extending <see cref="VueUseMouseReturn"/> with in-element
/// relative position and dimensions.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseMouseInElementReturn : VueUseMouseReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseMouseInElementReturn()
    {
    }

    /// <summary>相对元素左上角的水平坐标。The x coordinate relative to the element's top-left corner.</summary>
    [Description("@#elementX")]
    public extern Vue.VueShallowRef<Number> ElementX { get; }

    /// <summary>相对元素左上角的垂直坐标。The y coordinate relative to the element's top-left corner.</summary>
    [Description("@#elementY")]
    public extern Vue.VueShallowRef<Number> ElementY { get; }

    /// <summary>元素在文档中的水平位置。The element's x position in the document.</summary>
    [Description("@#elementPositionX")]
    public extern Vue.VueShallowRef<Number> ElementPositionX { get; }

    /// <summary>元素在文档中的垂直位置。The element's y position in the document.</summary>
    [Description("@#elementPositionY")]
    public extern Vue.VueShallowRef<Number> ElementPositionY { get; }

    /// <summary>元素高度。The element height.</summary>
    [Description("@#elementHeight")]
    public extern Vue.VueShallowRef<Number> ElementHeight { get; }

    /// <summary>元素宽度。The element width.</summary>
    [Description("@#elementWidth")]
    public extern Vue.VueShallowRef<Number> ElementWidth { get; }

    /// <summary>鼠标是否位于元素之外。Whether the cursor is outside the element.</summary>
    [Description("@#isOutside")]
    public extern Vue.VueShallowRef<bool> IsOutside { get; }

    /// <summary>停止位置更新。Stops the position tracking.</summary>
    [Description("@#stop")]
    public extern void Stop();
}

/// <summary>
/// <c>useWindowSize()</c> 的返回值。
/// The <c>useWindowSize()</c> return value.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseWindowSizeReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseWindowSizeReturn()
    {
    }

    /// <summary>窗口宽度。The window width.</summary>
    [Description("@#width")]
    public extern Vue.VueShallowRef<Number> Width { get; }

    /// <summary>窗口高度。The window height.</summary>
    [Description("@#height")]
    public extern Vue.VueShallowRef<Number> Height { get; }
}

/// <summary>
/// <c>useElementSize()</c> 的返回值。
/// The <c>useElementSize()</c> return value.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseElementSizeReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseElementSizeReturn()
    {
    }

    /// <summary>元素宽度。The element width.</summary>
    [Description("@#width")]
    public extern Vue.VueShallowRef<Number> Width { get; }

    /// <summary>元素高度。The element height.</summary>
    [Description("@#height")]
    public extern Vue.VueShallowRef<Number> Height { get; }

    /// <summary>停止尺寸观测。Stops observing the element size.</summary>
    [Description("@#stop")]
    public extern void Stop();
}

/// <summary>
/// <c>useElementBounding()</c> 的返回值：元素的边界矩形。
/// The <c>useElementBounding()</c> return value: the element's bounding rectangle.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseElementBoundingReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseElementBoundingReturn()
    {
    }

    /// <summary>元素高度。The element height.</summary>
    [Description("@#height")]
    public extern Vue.VueShallowRef<Number> Height { get; }

    /// <summary>下边界坐标。The bottom edge coordinate.</summary>
    [Description("@#bottom")]
    public extern Vue.VueShallowRef<Number> Bottom { get; }

    /// <summary>左边界坐标。The left edge coordinate.</summary>
    [Description("@#left")]
    public extern Vue.VueShallowRef<Number> Left { get; }

    /// <summary>右边界坐标。The right edge coordinate.</summary>
    [Description("@#right")]
    public extern Vue.VueShallowRef<Number> Right { get; }

    /// <summary>上边界坐标。The top edge coordinate.</summary>
    [Description("@#top")]
    public extern Vue.VueShallowRef<Number> Top { get; }

    /// <summary>元素宽度。The element width.</summary>
    [Description("@#width")]
    public extern Vue.VueShallowRef<Number> Width { get; }

    /// <summary>水平坐标。The x coordinate.</summary>
    [Description("@#x")]
    public extern Vue.VueShallowRef<Number> X { get; }

    /// <summary>垂直坐标。The y coordinate.</summary>
    [Description("@#y")]
    public extern Vue.VueShallowRef<Number> Y { get; }

    /// <summary>手动更新边界。Updates the bounding rectangle manually.</summary>
    [Description("@#update")]
    public extern void Update();
}

/// <summary>
/// <c>useElementBounding()</c> 的选项对象。
/// Options for <c>useElementBounding()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseElementBoundingOptions
{
    /// <summary>停止观测时是否重置为 0。Whether to reset values to zero when tracking stops.</summary>
    [Description("@#reset")]
    public bool? Reset { get; init; }

    /// <summary>窗口尺寸变化时更新。Whether to update on window resize.</summary>
    [Description("@#windowResize")]
    public bool? WindowResize { get; init; }

    /// <summary>窗口滚动时更新。Whether to update on window scroll.</summary>
    [Description("@#windowScroll")]
    public bool? WindowScroll { get; init; }

    /// <summary>立即执行一次更新。Whether to update immediately.</summary>
    [Description("@#immediate")]
    public bool? Immediate { get; init; }
}

/// <summary>
/// <c>useWindowScroll()</c> 的返回值。
/// The <c>useWindowScroll()</c> return value.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseWindowScrollReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseWindowScrollReturn()
    {
    }

    /// <summary>水平滚动位置。The horizontal scroll position.</summary>
    [Description("@#x")]
    public extern Vue.VueShallowRef<Number> X { get; }

    /// <summary>垂直滚动位置。The vertical scroll position.</summary>
    [Description("@#y")]
    public extern Vue.VueShallowRef<Number> Y { get; }
}

/// <summary>
/// <c>useIntersectionObserver()</c> 的选项对象。
/// Options for <c>useIntersectionObserver()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseIntersectionObserverOptions
{
    /// <summary>立即开始观测。Whether to start observing immediately.</summary>
    [Description("@#immediate")]
    public bool? Immediate { get; init; }

    /// <summary>根元素。The root element.</summary>
    [Description("@#root")]
    public VueUseMaybeElement? Root { get; init; }

    /// <summary>根元素的边距。The root margin.</summary>
    [Description("@#rootMargin")]
    public string? RootMargin { get; init; }

    /// <summary>交叉阈值。The intersection threshold.</summary>
    [Description("@#threshold")]
    public Number? Threshold { get; init; }
}

/// <summary>
/// <c>useIntersectionObserver()</c> 的返回值：可暂停的观测控制。
/// The <c>useIntersectionObserver()</c> return value: pausable observation control.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseIntersectionObserverReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseIntersectionObserverReturn()
    {
    }

    /// <summary>当前环境是否支持该 API。Whether the current environment supports this API.</summary>
    [Description("@#isSupported")]
    public extern Vue.VueComputedRef<bool> IsSupported { get; }

    /// <summary>观测当前是否处于活跃状态。Whether observation is currently active.</summary>
    [Description("@#isActive")]
    public extern Vue.VueShallowRef<bool> IsActive { get; }

    /// <summary>暂停观测。Pauses observation.</summary>
    [Description("@#pause")]
    public extern void Pause();

    /// <summary>恢复观测。Resumes observation.</summary>
    [Description("@#resume")]
    public extern void Resume();

    /// <summary>停止观测并释放资源。Stops observation and releases resources.</summary>
    [Description("@#stop")]
    public extern void Stop();
}

/// <summary>
/// 交叉观察器回调。
/// The intersection-observer callback.
/// </summary>
/// <param name="entries">当前批次的交叉记录。The intersection entries in this batch.</param>
/// <param name="observer">触发回调的观察器实例。The observer instance that triggered the callback.</param>
public delegate void VueUseIntersectionObserverCallback(Array<IntersectionObserverEntry?> entries, IntersectionObserver observer);

/// <summary>
/// 尺寸观察器回调。
/// The resize-observer callback.
/// </summary>
/// <param name="entries">当前批次的尺寸记录。The resize entries in this batch.</param>
/// <param name="observer">触发回调的观察器实例。The observer instance that triggered the callback.</param>
public delegate void VueUseResizeObserverCallback(Array<ResizeObserverEntry?> entries, ResizeObserver observer);

/// <summary>
/// <c>useStorage()</c> 系列及 <c>useLocalStorage()</c>、<c>useSessionStorage()</c> 的选项对象。
/// Options shared by <c>useStorage()</c>, <c>useLocalStorage()</c>, and <c>useSessionStorage()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseStorageOptions
{
    /// <summary>深层监听存储值变化。Whether to watch storage values deeply.</summary>
    [Description("@#deep")]
    public bool? Deep { get; init; }

    /// <summary>监听其他标签页的存储变更。Whether to listen for storage changes from other tabs.</summary>
    [Description("@#listenToStorageChanges")]
    public bool? ListenToStorageChanges { get; init; }

    /// <summary>键不存在时写入默认值。Whether to write the default value when the key is absent.</summary>
    [Description("@#writeDefaults")]
    public bool? WriteDefaults { get; init; }

    /// <summary>合并已存储的默认值。Whether to merge stored values with the defaults.</summary>
    [Description("@#mergeDefaults")]
    public bool? MergeDefaults { get; init; }
}

/// <summary>
/// <c>useClipboard()</c> 的选项对象。
/// Options for <c>useClipboard()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseClipboardOptions
{
    /// <summary>挂载时读取剪贴板内容。Whether to read the clipboard content on mount.</summary>
    [Description("@#read")]
    public bool? Read { get; init; }

    /// <summary>copied 状态保持的毫秒数。The number of milliseconds the <c>copied</c> state is held.</summary>
    [Description("@#copiedDuring")]
    public Number? CopiedDuring { get; init; }

    /// <summary>为不支持异步剪贴板 API 的环境启用回退实现。Whether to enable the fallback for environments without the async clipboard API.</summary>
    [Description("@#legacy")]
    public bool? Legacy { get; init; }
}

/// <summary>
/// <c>useClipboard()</c> 的返回值。
/// The <c>useClipboard()</c> return value.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseClipboardReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseClipboardReturn()
    {
    }

    /// <summary>当前环境是否支持该 API。Whether the current environment supports this API.</summary>
    [Description("@#isSupported")]
    public extern Vue.VueComputedRef<bool> IsSupported { get; }

    /// <summary>剪贴板当前文本。The current clipboard text.</summary>
    [Description("@#text")]
    public extern Vue.VueReadonlyRef<string> Text { get; }

    /// <summary>最近一次复制是否成功。Whether the most recent copy succeeded.</summary>
    [Description("@#copied")]
    public extern Vue.VueReadonlyRef<bool> Copied { get; }

    /// <summary>复制操作是否仍在进行。Whether a copy operation is still pending.</summary>
    [Description("@#copyPending")]
    public extern Vue.VueReadonlyRef<bool> CopyPending { get; }

    /// <summary>将文本写入剪贴板。Writes the text to the clipboard.</summary>
    /// <param name="text">要复制的文本。The text to copy.</param>
    /// <returns>完成后解析的 promise 结果。A promise result that resolves on completion.</returns>
    [Description("@#copy")]
    public extern PromiseResult Copy(string text);
}

/// <summary>
/// <c>useTitle()</c> 的选项对象。
/// Options for <c>useTitle()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseTitleOptions
{
    /// <summary>在其上设置标题的文档。The document whose title is set.</summary>
    [Description("@#document")]
    public DocumentRef? Document { get; init; }

    /// <summary>是否在组件卸载时恢复原标题。Whether to restore the previous title on unmount.</summary>
    [Description("@#restoreOnUnmount")]
    public bool? RestoreOnUnmount { get; init; }
}

/// <summary>
/// <c>useFavicon()</c> 的选项对象。
/// Options for <c>useFavicon()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseFaviconOptions
{
    /// <summary>图标 URL 的基础路径。The base path prepended to the icon URL.</summary>
    [Description("@#baseUrl")]
    public string? BaseUrl { get; init; }

    /// <summary>图标的 link rel 属性值。The <c>rel</c> attribute of the icon link element.</summary>
    [Description("@#rel")]
    public string? Rel { get; init; }
}

/// <summary>
/// <c>useFullscreen()</c> 的选项对象。
/// Options for <c>useFullscreen()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseFullscreenOptions
{
    /// <summary>目标元素。The target element.</summary>
    [Description("@#target")]
    public VueUseMaybeElement? Target { get; init; }

    /// <summary>退出全屏时是否同时结束浏览器的全屏状态。Whether exiting also leaves the browser fullscreen state.</summary>
    [Description("@#autoExit")]
    public bool? AutoExit { get; init; }
}

/// <summary>
/// <c>useFullscreen()</c> 的返回值。
/// The <c>useFullscreen()</c> return value.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseFullscreenReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseFullscreenReturn()
    {
    }

    /// <summary>当前环境是否支持该 API。Whether the current environment supports this API.</summary>
    [Description("@#isSupported")]
    public extern Vue.VueComputedRef<bool> IsSupported { get; }

    /// <summary>当前是否处于全屏。Whether the document is currently fullscreen.</summary>
    [Description("@#isFullscreen")]
    public extern Vue.VueShallowRef<bool> IsFullscreen { get; }

    /// <summary>进入全屏。Enters fullscreen.</summary>
    [Description("@#enter")]
    public extern PromiseResult Enter();

    /// <summary>退出全屏。Exits fullscreen.</summary>
    [Description("@#exit")]
    public extern PromiseResult Exit();

    /// <summary>切换全屏状态。Toggles the fullscreen state.</summary>
    [Description("@#toggle")]
    public extern PromiseResult Toggle();
}
