using System.ComponentModel;

namespace ECMAScript;

public static partial class VueUse
{
    // ---------- 浏览器与元素 Browser & element ----------

    /// <summary>
    /// 追踪鼠标位置。
    /// Tracks the mouse position.
    /// </summary>
    [Description("@#useMouse")]
    public extern static VueUseMouseReturn UseMouse(VueUseMouseOptions? options = null);

    /// <summary>
    /// 追踪鼠标相对目标元素的位置及元素尺寸。
    /// Tracks the mouse position relative to a target element along with the element dimensions.
    /// </summary>
    [Description("@#useMouseInElement")]
    public extern static VueUseMouseInElementReturn UseMouseInElement(VueUseMaybeElement? target = null, VueUseMouseInElementOptions? options = null);

    /// <summary>
    /// 响应式追踪窗口尺寸。
    /// Reactively tracks the window size.
    /// </summary>
    [Description("@#useWindowSize")]
    public extern static VueUseWindowSizeReturn UseWindowSize();

    /// <summary>
    /// 响应式追踪窗口滚动位置。
    /// Reactively tracks the window scroll position.
    /// </summary>
    [Description("@#useWindowScroll")]
    public extern static VueUseWindowScrollReturn UseWindowScroll();

    /// <summary>
    /// 响应式追踪元素尺寸。
    /// Reactively tracks the element size.
    /// </summary>
    [Description("@#useElementSize")]
    public extern static VueUseElementSizeReturn UseElementSize(VueUseMaybeElement target);

    /// <summary>
    /// 响应式追踪元素边界矩形。
    /// Reactively tracks the element bounding rectangle.
    /// </summary>
    [Description("@#useElementBounding")]
    public extern static VueUseElementBoundingReturn UseElementBounding(VueUseMaybeElement target, VueUseElementBoundingOptions? options = null);

    /// <summary>
    /// 判断元素是否进入视口。
    /// Determines whether the element is visible in the viewport.
    /// </summary>
    [Description("@#useElementVisibility")]
    public extern static Vue.VueShallowRef<bool> UseElementVisibility(VueUseMaybeElement element);

    /// <summary>
    /// 观测元素进入/离开视口。
    /// Observes the element's viewport intersection.
    /// </summary>
    [Description("@#useIntersectionObserver")]
    public extern static VueUseIntersectionObserverReturn UseIntersectionObserver(
        VueUseMaybeElement target,
        VueUseIntersectionObserverCallback callback,
        VueUseIntersectionObserverOptions? options = null);

    /// <summary>
    /// 观测元素尺寸变化。
    /// Observes element size changes.
    /// </summary>
    [Description("@#useResizeObserver")]
    public extern static VueUseIntersectionObserverReturn UseResizeObserver(
        VueUseMaybeElement target,
        VueUseResizeObserverCallback callback);

    /// <summary>
    /// 响应式追踪文档可见性。
    /// Reactively tracks the document visibility.
    /// </summary>
    [Description("@#useDocumentVisibility")]
    public extern static Vue.VueShallowRef<VueUseDocumentVisibility> UseDocumentVisibility();

    /// <summary>
    /// 响应式追踪窗口焦点。
    /// Reactively tracks window focus.
    /// </summary>
    [Description("@#useWindowFocus")]
    public extern static Vue.VueShallowRef<bool> UseWindowFocus();

    /// <summary>
    /// 响应式追踪当前获得焦点的元素。
    /// Reactively tracks the currently focused element.
    /// </summary>
    [Description("@#useActiveElement")]
    public extern static Vue.VueShallowRef<Element?> UseActiveElement();

    /// <summary>
    /// 响应式追踪媒体查询结果。
    /// Reactively tracks a media query result.
    /// </summary>
    [Description("@#useMediaQuery")]
    public extern static Vue.VueComputedRef<bool> UseMediaQuery(string query);

    /// <summary>
    /// 判断系统是否偏好深色方案。
    /// Determines whether the system prefers the dark color scheme.
    /// </summary>
    [Description("@#usePreferredDark")]
    public extern static Vue.VueComputedRef<bool> UsePreferredDark();

    /// <summary>
    /// 响应式追踪系统偏好配色方案。
    /// Reactively tracks the preferred color scheme.
    /// </summary>
    [Description("@#usePreferredColorScheme")]
    public extern static Vue.VueComputedRef<VueUseColorScheme> UsePreferredColorScheme();

    /// <summary>
    /// 响应式追踪浏览器语言首选项。
    /// Reactively tracks the browser language preferences.
    /// </summary>
    [Description("@#usePreferredLanguages")]
    public extern static Vue.VueShallowRef<string[]> UsePreferredLanguages();

    /// <summary>
    /// 响应式追踪网络在线状态。
    /// Reactively tracks the network online state.
    /// </summary>
    [Description("@#useOnline")]
    public extern static Vue.VueReadonlyRef<bool> UseOnline();

    /// <summary>
    /// 响应式追踪网络连接信息。
    /// Reactively tracks network connection information.
    /// </summary>
    [Description("@#useNetwork")]
    public extern static VueUseNetworkReturn UseNetwork();

    /// <summary>
    /// 响应式追踪电池状态。
    /// Reactively tracks battery status.
    /// </summary>
    [Description("@#useBattery")]
    public extern static VueUseBatteryReturn UseBattery();

    /// <summary>
    /// 响应式追踪设备像素比。
    /// Reactively tracks the device pixel ratio.
    /// </summary>
    [Description("@#useDevicePixelRatio")]
    public extern static Vue.VueShallowRef<Number> UseDevicePixelRatio();

    /// <summary>
    /// 响应式追踪每秒帧率。
    /// Reactively tracks the frames per second.
    /// </summary>
    [Description("@#useFps")]
    public extern static Vue.VueShallowRef<Number> UseFps();

    /// <summary>
    /// 追踪用户是否处于空闲状态。
    /// Tracks whether the user is idle.
    /// </summary>
    [Description("@#useIdle")]
    public extern static VueUseIdleReturn UseIdle(Number? timeout = null);

    /// <summary>
    /// 响应式判断鼠标是否离开页面。
    /// Reactively determines whether the mouse has left the page.
    /// </summary>
    [Description("@#usePageLeave")]
    public extern static Vue.VueShallowRef<bool> UsePageLeave();

    // ---------- 存储与设备 Storage & device ----------

    /// <summary>
    /// 面向字符串的响应式存储；键或值变化时写回底层 storage。
    /// A string-oriented reactive storage that writes back when the key or value changes.
    /// </summary>
    [Description("@#useStorage")]
    public extern static Vue.IVueRef<string> UseStorage(string key, string defaults, VueUseStorageOptions? options = null);

    /// <summary>
    /// 面向字符串的响应式 localStorage。
    /// A string-oriented reactive localStorage.
    /// </summary>
    [Description("@#useLocalStorage")]
    public extern static Vue.IVueRef<string> UseLocalStorage(string key, string initialValue, VueUseStorageOptions? options = null);

    /// <summary>
    /// 面向字符串的响应式 sessionStorage。
    /// A string-oriented reactive sessionStorage.
    /// </summary>
    [Description("@#useSessionStorage")]
    public extern static Vue.IVueRef<string> UseSessionStorage(string key, string initialValue, VueUseStorageOptions? options = null);

    /// <summary>
    /// 响应式剪贴板读写。
    /// Reactive clipboard read/write.
    /// </summary>
    [Description("@#useClipboard")]
    public extern static VueUseClipboardReturn UseClipboard(VueUseClipboardOptions? options = null);

    /// <summary>
    /// 可写的深色模式开关，并同步存储与 <c>html</c> 类名。
    /// A writable dark-mode toggle that syncs storage and the <c>html</c> class.
    /// </summary>
    [Description("@#useDark")]
    public extern static Vue.IVueRef<bool> UseDark();

    /// <summary>
    /// 响应式配色模式（含自动模式）。
    /// Reactive color mode, including the automatic mode.
    /// </summary>
    [Description("@#useColorMode")]
    public extern static VueUseColorModeReturn UseColorMode();

    /// <summary>
    /// 响应式文档标题。
    /// Reactive document title.
    /// </summary>
    [Description("@#useTitle")]
    public extern static Vue.IVueRef<string?> UseTitle(string? newTitle, VueUseTitleOptions? options = null);

    /// <summary>
    /// 响应式站点图标。
    /// Reactive site favicon.
    /// </summary>
    [Description("@#useFavicon")]
    public extern static Vue.IVueRef<string?> UseFavicon(string? newIcon, VueUseFaviconOptions? options = null);

    /// <summary>
    /// 响应式全屏状态与控制入口。
    /// Reactive fullscreen state and control entry.
    /// </summary>
    [Description("@#useFullscreen")]
    public extern static VueUseFullscreenReturn UseFullscreen(VueUseMaybeElement? target = null, VueUseFullscreenOptions? options = null);

    // ---------- 时间 Time ----------

    /// <summary>
    /// 响应式当前时间，按间隔更新。
    /// Reactive current time, updated at an interval.
    /// </summary>
    [Description("@#useNow")]
    public extern static Vue.VueShallowRef<Date> UseNow();

    /// <summary>
    /// 响应式当前时间戳，按间隔更新。
    /// Reactive current timestamp, updated at an interval.
    /// </summary>
    [Description("@#useTimestamp")]
    public extern static Vue.VueShallowRef<Number> UseTimestamp();

    /// <summary>
    /// 以自然语言返回相对当前时间的时间描述。
    /// Returns a human-readable time description relative to now.
    /// </summary>
    [Description("@#useTimeAgo")]
    public extern static Vue.IVueRef<string> UseTimeAgo(Date time);

    /// <summary>
    /// 按固定间隔重复执行回调，支持暂停与恢复。
    /// Repeatedly invokes a callback at a fixed interval with pause and resume control.
    /// </summary>
    [Description("@#useIntervalFn")]
    public extern static VueUsePausableControl UseIntervalFn(Action callback, Number interval);

    /// <summary>
    /// 延迟执行回调，支持暂停与恢复。
    /// Invokes a callback after a delay with pause and resume control.
    /// </summary>
    [Description("@#useTimeoutFn")]
    public extern static VueUsePausableControl UseTimeoutFn(Action callback, Number interval);

    /// <summary>
    /// 每个动画帧调用回调，支持暂停与恢复。
    /// Invokes a callback on every animation frame with pause and resume control.
    /// </summary>
    [Description("@#useRafFn")]
    public extern static VueUsePausableControl UseRafFn(Action callback);

    // ---------- 事件 Events ----------

    /// <summary>
    /// 注册元素外点击处理器；返回解绑函数。
    /// Registers an outside-click handler and returns a cleanup function.
    /// </summary>
    [Description("@#onClickOutside")]
    public extern static Action OnClickOutside(VueUseMaybeElement target, Action<PointerEvent> handler);

    /// <summary>
    /// 注册按键按下处理器；返回解绑函数。
    /// Registers a keydown handler and returns a cleanup function.
    /// </summary>
    [Description("@#onKeyDown")]
    public extern static Action OnKeyDown(string key, Action<KeyboardEvent> handler);

    /// <summary>
    /// 注册按键按下（含重复）处理器；返回解绑函数。
    /// Registers a keydown handler that also fires on repeat and returns a cleanup function.
    /// </summary>
    [Description("@#onKeyStroke")]
    public extern static Action OnKeyStroke(string key, Action<KeyboardEvent> handler);

    /// <summary>
    /// 注册按键抬起处理器；返回解绑函数。
    /// Registers a keyup handler and returns a cleanup function.
    /// </summary>
    [Description("@#onKeyUp")]
    public extern static Action OnKeyUp(string key, Action<KeyboardEvent> handler);

    /// <summary>
    /// 注册元素长按处理器；返回解绑函数。
    /// Registers a long-press handler on the element and returns a cleanup function.
    /// </summary>
    [Description("@#onLongPress")]
    public extern static Action OnLongPress(VueUseMaybeElement target, Action<PointerEvent> handler);

    /// <summary>
    /// 注册任意事件目标的监听器；返回解绑函数。
    /// Registers a listener on any event target and returns a cleanup function.
    /// </summary>
    [Description("@#useEventListener")]
    public extern static Action UseEventListener(EventTarget target, string @event, Action<EventRef> listener);

    // ---------- 状态与工具 State & utility ----------

    /// <summary>
    /// 布尔值切换。
    /// A boolean toggle.
    /// </summary>
    [Description("@#useToggle")]
    public extern static Vue.IVueRef<bool> UseToggle(bool value = false);

    /// <summary>
    /// 返回值的上一帧快照。
    /// Returns the previous value snapshot.
    /// </summary>
    [Description("@#usePrevious")]
    public extern static Vue.VueReadonlyRef<string?> UsePrevious(Vue.IVueRef<string?> value);

    /// <summary>
    /// 防抖包装函数；返回包装后的函数。
    /// Wraps a function with debounce and returns the wrapped function.
    /// </summary>
    [Description("@#useDebounceFn")]
    public extern static Action UseDebounceFn(Action callback, Number delay);

    /// <summary>
    /// 节流包装函数；返回包装后的函数。
    /// Wraps a function with throttle and returns the wrapped function.
    /// </summary>
    [Description("@#useThrottleFn")]
    public extern static Action UseThrottleFn(Action callback, Number delay);

    /// <summary>
    /// 创建强类型事件钩子，供组件或组合式函数发布与订阅事件。
    /// Creates a strongly typed event hook that components or composables can publish to and subscribe on.
    /// </summary>
    /// <typeparam name="T">事件负载类型。The event payload type.</typeparam>
    [Description("@#createEventHook")]
    public extern static VueUseEventHook<T> CreateEventHook<T>();

    /// <summary>
    /// 异步状态管理：加载中、就绪、错误与手动执行入口。
    /// Asynchronous state management: loading, ready, error, and a manual execute entry.
    /// </summary>
    /// <typeparam name="T">异步结果类型。The asynchronous result type.</typeparam>
    /// <param name="promise">返回结果的 promise。The promise that resolves the result.</param>
    /// <param name="initialState">初始状态值。The initial state value.</param>
    /// <param name="options">异步状态选项。The asynchronous state options.</param>
    [Description("@#useAsyncState")]
    public extern static VueUseAsyncStateReturn<T> UseAsyncState<T>(IPromise<T> promise, T initialState, VueUseAsyncStateOptions? options = null);

    /// <summary>
    /// 惰性求值并在依赖变化时异步重新计算。
    /// Lazily evaluates and asynchronously recomputes when dependencies change.
    /// </summary>
    /// <typeparam name="T">计算值类型。The computed value type.</typeparam>
    /// <param name="evaluationCallback">返回 promise 的求值回调。The evaluation callback returning a promise.</param>
    /// <param name="initialState">初始值。The initial value.</param>
    [Description("@#computedAsync")]
    public extern static Vue.VueComputedRef<T> ComputedAsync<T>(Func<IPromise<T>> evaluationCallback, T initialState);

    /// <summary>
    /// 立即求值的计算属性；无依赖追踪，写入即重算。
    /// An eagerly evaluated computed value without dependency tracking; writes trigger recomputation.
    /// </summary>
    /// <typeparam name="T">计算值类型。The computed value type.</typeparam>
    /// <param name="getter">求值回调。The evaluation callback.</param>
    [Description("@#computedEager")]
    public extern static Vue.IVueRef<T> ComputedEager<T>(Func<T> getter);

    /// <summary>
    /// 判断当前是否运行在浏览器客户端。
    /// Determines whether the code runs on the browser client.
    /// </summary>
    [Description("@#isClient")]
    public extern static bool IsClient();

    /// <summary>
    /// 判断值是否为已定义的 JavaScript 值（非 null/undefined）。
    /// Determines whether the value is defined (neither null nor undefined).
    /// </summary>
    [Description("@#isDef")]
    public extern static bool IsDef(string? value);

    /// <summary>
    /// 判断当前是否运行在 iOS 设备上。
    /// Determines whether the code runs on an iOS device.
    /// </summary>
    [Description("@#isIOS")]
    public extern static bool IsIOS();

    /// <summary>
    /// 判断当前是否运行在 Web Worker 中。
    /// Determines whether the code runs inside a Web Worker.
    /// </summary>
    [Description("@#isWorker")]
    public extern static bool IsWorker();

    /// <summary>
    /// 组件挂载后执行副作用；服务端渲染时跳过。
    /// Runs a side effect after the component mounts and skips it during server rendering.
    /// </summary>
    [Description("@#tryOnMounted")]
    public extern static void TryOnMounted(Action callback);

    /// <summary>
    /// 作用域销毁时执行清理；无活动作用域时跳过。
    /// Runs cleanup when the scope is disposed and skips when no scope is active.
    /// </summary>
    [Description("@#tryOnScopeDispose")]
    public extern static void TryOnScopeDispose(Action callback);

    /// <summary>
    /// 组件卸载时执行清理；无活动组件实例时跳过。
    /// Runs cleanup when the component unmounts and skips when no instance is active.
    /// </summary>
    [Description("@#tryOnUnmounted")]
    public extern static void TryOnUnmounted(Action callback);

    /// <summary>
    /// 响应式追踪 <c>base64</c> 编码与解码。
    /// Reactively tracks <c>base64</c> encoding and decoding.
    /// </summary>
    [Description("@#useBase64")]
    public extern static VueUseBase64Return UseBase64(string target, VueUseBase64Options? options = null);

    /// <summary>
    /// 倒计时，按秒递减并暴露剩余秒数。
    /// A countdown that decrements by the second and exposes the remaining seconds.
    /// </summary>
    [Description("@#useCountdown")]
    public extern static VueUseCountdownReturn UseCountdown(Number initialCountdown, VueUseCountdownOptions? options = null);

    /// <summary>
    /// 响应式判断某个浏览器特性是否受支持。
    /// Reactively determines whether a browser feature is supported.
    /// </summary>
    [Description("@#useSupported")]
    public extern static Vue.VueComputedRef<bool> UseSupported(Func<bool> callback);

    /// <summary>
    /// 响应式追踪用户减少动效偏好。
    /// Reactively tracks the reduced-motion preference.
    /// </summary>
    [Description("@#usePreferredReducedMotion")]
    public extern static Vue.VueComputedRef<VueUseReducedMotion> UsePreferredReducedMotion();

    /// <summary>
    /// 响应式读取 CSS 自定义属性值。
    /// Reactively reads a CSS custom-property value.
    /// </summary>
    [Description("@#useCssVar")]
    public extern static Vue.VueShallowRef<string?> UseCssVar(string prop, VueUseMaybeElement? target = null);

    /// <summary>
    /// 响应式判断元素是否被悬停。
    /// Reactively determines whether the element is hovered.
    /// </summary>
    [Description("@#useElementHover")]
    public extern static Vue.VueShallowRef<bool> UseElementHover(VueUseMaybeElement element);

    /// <summary>
    /// 响应式追踪键盘修饰键的按下状态。
    /// Reactively tracks the pressed state of a keyboard modifier key.
    /// </summary>
    [Description("@#useKeyModifier")]
    public extern static Vue.VueShallowRef<bool?> UseKeyModifier(VueUseKeyModifier modifier);

    /// <summary>
    /// 用户开始输入时触发回调（无需聚焦输入框）。
    /// Invokes the callback when the user starts typing, without requiring a focused input.
    /// </summary>
    [Description("@#onStartTyping")]
    public extern static void OnStartTyping(Action<KeyboardEvent> callback);

    /// <summary>
    /// 响应式排序后的数组。
    /// A reactively sorted array.
    /// </summary>
    /// <typeparam name="T">元素类型。The element type.</typeparam>
    /// <param name="source">源数组引用。The source array ref.</param>
    [Description("@#useSorted")]
    public extern static Vue.IVueRef<T[]> UseSorted<T>(Vue.IVueRef<T[]> source);

    /// <summary>
    /// 响应式追踪当前文本选择。
    /// Reactively tracks the current text selection.
    /// </summary>
    [Description("@#useTextSelection")]
    public extern static VueUseTextSelectionReturn UseTextSelection();
}
