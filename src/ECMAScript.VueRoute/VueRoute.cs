using System;
using System.ComponentModel;
using ECMAScript.Contract;
using static ECMAScript.Vue;

namespace ECMAScript;

/// <summary>
/// 路由导航守卫回调，返回导航结果。
/// Route navigation guard callback that returns a navigation result.
/// </summary>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
/// <returns>导航守卫返回值，控制导航行为。Navigation guard return value that controls navigation behavior.</returns>
public delegate NavigationGuardReturn? RouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// 异步路由导航守卫回调，返回 Promise 包装的导航结果。
/// Asynchronous route navigation guard callback that returns a Promise-wrapped navigation result.
/// </summary>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
/// <returns>Promise 包装的导航守卫返回值。Promise-wrapped navigation guard return value.</returns>
public delegate IPromise<NavigationGuardReturn?> AsyncRouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// 路由查询字符串解析器回调，将原始查询字符串解析为查询对象。
/// Route query string parser callback that parses a raw query string into a query object.
/// </summary>
/// <param name="search">原始查询字符串。Raw query string.</param>
/// <returns>解析后的路由查询对象。Parsed route query object.</returns>
public delegate LocationQuery RouteQueryParser(string search);

/// <summary>
/// 路由查询对象序列化器回调，将查询对象序列化为查询字符串。
/// Route query object serializer callback that serializes a query object into a query string.
/// </summary>
/// <param name="query">原始查询对象。Raw query object.</param>
/// <returns>序列化后的查询字符串。Serialized query string.</returns>
public delegate string RouteQueryStringifier(LocationQueryRaw? query);

/// <summary>
/// 路由记录 props 解析器回调，根据路由位置动态计算传递给组件的 props。
/// Route record props resolver callback that dynamically computes props to pass to the component based on the route location.
/// </summary>
/// <param name="to">当前路由位置。Current route location.</param>
/// <returns>解析后的 Vue props 对象。Resolved Vue props object.</returns>
public delegate Vue.VueProps RouteRecordPropsResolver(RouteLocationNormalized to);

/// <summary>
/// 路由器滚动行为回调，控制路由切换时的滚动位置。
/// Router scroll behavior callback that controls scroll position during route transitions.
/// </summary>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
/// <param name="savedPosition">浏览器前进/后退时保存的滚动位置。Saved scroll position when using browser back/forward.</param>
/// <returns>滚动行为结果，指定期望的滚动位置。Scroll behavior result specifying the desired scroll position.</returns>
public delegate RouterScrollResult? RouterScrollBehavior(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, ScrollPositionNormalized? savedPosition);

/// <summary>
/// 异步路由器滚动行为回调，返回 Promise 包装的滚动位置结果。
/// Asynchronous router scroll behavior callback that returns a Promise-wrapped scroll position result.
/// </summary>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
/// <param name="savedPosition">浏览器前进/后退时保存的滚动位置。Saved scroll position when using browser back/forward.</param>
/// <returns>Promise 包装的滚动行为结果。Promise-wrapped scroll behavior result.</returns>
public delegate IPromise<RouterScrollResult?> AsyncRouterScrollBehavior(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, ScrollPositionNormalized? savedPosition);

/// <summary>
/// 导航后钩子回调，在导航完成后执行。
/// After-navigation hook callback that executes after navigation completes.
/// </summary>
/// <param name="to">导航完成后的目标路由位置。Target route location after navigation completes.</param>
/// <param name="from">导航前的来源路由位置。Source route location before navigation.</param>
/// <param name="failure">导航失败信息，导航成功时为 null。Navigation failure info, null when navigation succeeds.</param>
public delegate void AfterNavigationHook(RouteLocationNormalizedLoaded to, RouteLocationNormalizedLoaded from, NavigationFailure? failure);

/// <summary>
/// 路由组件懒加载回调，返回 Promise 包装的 Vue 组件。
/// Route component lazy-loading callback that returns a Promise-wrapped Vue component.
/// </summary>
/// <returns>Promise 包装的 Vue 组件定义。Promise-wrapped Vue component definition.</returns>
public delegate IPromise<IVueComponent> RouteComponentLoader();

/// <summary>
/// 路由重定向回调，根据当前路由位置返回重定向目标。
/// Route redirect callback that returns a redirect target based on the current route location.
/// </summary>
/// <param name="to">当前路由位置。Current route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
/// <returns>重定向目标路由位置。Redirect target route location.</returns>
public delegate RouteLocationRaw RouteRedirectCallback(RouteLocation to, RouteLocationNormalizedLoaded from);

/// <summary>
/// 导航守卫 next 回调，Vue Router 4 推荐使用返回值方式。
/// Navigation guard next callback. Vue Router 4 recommends using return values instead.
/// </summary>
/// <param name="instance">当前 Vue 组件公共实例。Current Vue component public instance.</param>
[Obsolete("Vue Router 4 recommends return-based navigation guards. Use bool/RouteLocationRaw/Error returns instead of next(...).")]
public delegate void NavigationGuardNextCallback(Vue.VueComponentPublicInstance instance);

/// <summary>
/// 导航守卫 next 函数，用于在旧版 API 中控制导航行为。
/// Navigation guard next function used in the legacy API to control navigation behavior.
/// </summary>
/// <param name="value">导航守卫参数，可传递布尔值、路由位置或错误。Navigation guard argument; can pass a boolean, route location, or error.</param>
[Obsolete("Vue Router 4 recommends return-based navigation guards. Use bool/RouteLocationRaw/Error returns instead of next(...).")]
public delegate void NavigationGuardNext(NavigationGuardNextArgument? value = default);

/// <summary>
/// 遗留路由导航守卫，保留第三个 next 参数以兼容 Vue Router 3 的用法。
/// Legacy route navigation guard that retains the third next parameter for Vue Router 3 compatibility.
/// </summary>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
/// <param name="next">导航守卫 next 函数，用于控制导航行为。Navigation guard next function to control navigation behavior.</param>
/// <returns>导航守卫返回值。Navigation guard return value.</returns>
[Obsolete("Vue Router 4 keeps the third next parameter for backward compatibility only. Prefer RouteNavigationGuard return values.")]
public delegate NavigationGuardReturn? LegacyRouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next);

/// <summary>
/// 遗留异步路由导航守卫，保留第三个 next 参数以兼容 Vue Router 3 的用法。
/// Legacy asynchronous route navigation guard that retains the third next parameter for Vue Router 3 compatibility.
/// </summary>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
/// <param name="next">导航守卫 next 函数，用于控制导航行为。Navigation guard next function to control navigation behavior.</param>
/// <returns>Promise 包装的导航守卫返回值。Promise-wrapped navigation guard return value.</returns>
[Obsolete("Vue Router 4 keeps the third next parameter for backward compatibility only. Prefer AsyncRouteNavigationGuard return values.")]
public delegate IPromise<NavigationGuardReturn?> LegacyAsyncRouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next);

/// <summary>
/// Error 类型路由错误处理器，处理路由导航过程中抛出的 Error 对象。
/// Error-type router error handler that handles Error objects thrown during route navigation.
/// </summary>
/// <param name="error">路由错误对象。Router error object.</param>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
public delegate void ErrorRouterErrorHandler(Error error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// NavigationFailure 类型路由错误处理器，处理导航失败场景。
/// NavigationFailure-type router error handler that handles navigation failure scenarios.
/// </summary>
/// <param name="error">导航失败对象。Navigation failure object.</param>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
public delegate void NavigationFailureRouterErrorHandler(NavigationFailure error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// NavigationRedirectError 类型路由错误处理器，处理导航重定向错误。
/// NavigationRedirectError-type router error handler that handles navigation redirect errors.
/// </summary>
/// <param name="error">导航重定向错误对象。Navigation redirect error object.</param>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
public delegate void NavigationRedirectRouterErrorHandler(NavigationRedirectError error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// String 类型路由错误处理器，处理字符串形式的路由错误。
/// String-type router error handler that handles string-form route errors.
/// </summary>
/// <param name="error">字符串形式的错误信息。String-form error message.</param>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
public delegate void StringRouterErrorHandler(string error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// Number 类型路由错误处理器，处理数字形式的路由错误。
/// Number-type router error handler that handles numeric route errors.
/// </summary>
/// <param name="error">数字形式的错误值。Numeric error value.</param>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
public delegate void NumberRouterErrorHandler(Number error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// Boolean 类型路由错误处理器，处理布尔形式的路由错误。
/// Boolean-type router error handler that handles boolean route errors.
/// </summary>
/// <param name="error">布尔形式的错误值。Boolean error value.</param>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
public delegate void BooleanRouterErrorHandler(bool error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// BigInt 类型路由错误处理器，处理 BigInt 形式的路由错误。
/// BigInt-type router error handler that handles BigInt route errors.
/// </summary>
/// <param name="error">BigInt 形式的错误值。BigInt error value.</param>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
public delegate void BigIntRouterErrorHandler(BigInt error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// Symbol 类型路由错误处理器，处理 Symbol 形式的路由错误。
/// Symbol-type router error handler that handles Symbol route errors.
/// </summary>
/// <param name="error">Symbol 形式的错误值。Symbol error value.</param>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
public delegate void SymbolRouterErrorHandler(Symbol error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// Object 类型路由错误处理器，处理普通对象形式的路由错误。
/// Object-type router error handler that handles plain object route errors.
/// </summary>
/// <param name="error">对象形式的错误值。Object error value.</param>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
public delegate void ObjectRouterErrorHandler(IObject error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// Array 类型路由错误处理器，处理数组形式的路由错误。
/// Array-type router error handler that handles array-form route errors.
/// </summary>
/// <param name="error">数组形式的错误值。Array error value.</param>
/// <param name="to">目标路由位置。Target route location.</param>
/// <param name="from">来源路由位置。Source route location.</param>
public delegate void ArrayRouterErrorHandler(Array<RouterErrorValue?> error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

/// <summary>
/// RouterLink 导航回调，执行导航并返回 Promise 包装的结果。
/// RouterLink navigate callback that performs navigation and returns a Promise-wrapped result.
/// </summary>
/// <param name="event">触发导航的鼠标事件。Mouse event that triggered the navigation.</param>
/// <returns>Promise 包装的导航结果。Promise-wrapped navigation result.</returns>
public delegate IPromise<RouteNavigationResult?> RouterLinkNavigateCallback(MouseEvent? @event = null);

/// <summary>
/// RouterLink 插槽回调，根据链接作用域自定义渲染内容。
/// RouterLink slot callback that customizes rendered content based on the link scope.
/// </summary>
/// <param name="link">RouterLink 插槽作用域对象，提供导航状态和属性。RouterLink slot scope object providing navigation state and properties.</param>
/// <returns>渲染的虚拟节点数组。Array of rendered virtual nodes.</returns>
public delegate Vue.IVNode[] RouterLinkSlotCallback(RouterLinkSlotScope link);

/// <summary>
/// RouterView 插槽回调，根据路由视图作用域自定义渲染内容。
/// RouterView slot callback that customizes rendered content based on the router view scope.
/// </summary>
/// <param name="scope">RouterView 插槽作用域对象，提供当前路由组件和状态。RouterView slot scope object providing the current route component and state.</param>
/// <returns>渲染的虚拟节点数组。Array of rendered virtual nodes.</returns>
public delegate Vue.IVNode[] RouterViewSlotCallback(RouterViewSlotScope scope);

/// <summary>
/// 路由历史导航回调，在历史记录导航发生时触发。
/// Router history navigation callback triggered when a history navigation occurs.
/// </summary>
/// <param name="to">导航目标路径。Navigation target path.</param>
/// <param name="from">导航来源路径。Navigation source path.</param>
/// <param name="information">导航类型和方向的附加信息。Additional information about the navigation type and direction.</param>
public delegate void RouterHistoryNavigationCallback(string to, string from, RouterHistoryNavigationInformation information);

/// <summary>
/// 路由历史导航类型。
/// Router history navigation type.
/// </summary>
[String]
public enum RouterHistoryNavigationType
{
	/// <summary>
	/// 浏览器前进/后退触发的 pop 导航。
	/// Navigation triggered by browser back/forward.
	/// </summary>
	[Description("@#pop")]
	Pop,

	/// <summary>
	/// 通过 pushState 触发的程序化导航。
	/// Programmatic navigation triggered via pushState.
	/// </summary>
	[Description("@#push")]
	Push
}

/// <summary>
/// 路由历史导航方向。
/// Router history navigation direction.
/// </summary>
[String]
public enum RouterHistoryNavigationDirection
{
	/// <summary>
	/// 向后导航（浏览器后退）。
	/// Backward navigation (browser back).
	/// </summary>
	[Description("@#back")]
	Back,

	/// <summary>
	/// 向前导航（浏览器前进）。
	/// Forward navigation (browser forward).
	/// </summary>
	[Description("@#forward")]
	Forward,

	/// <summary>
	/// 方向未知（例如程序化导航）。
	/// Unknown direction (e.g., programmatic navigation).
	/// </summary>
	[ECMAScriptName("")]
	Unknown
}

/// <summary>
/// 导航失败类型标志，用于区分不同原因的导航中断。
/// Navigation failure type flags used to distinguish navigation interruptions by cause.
/// </summary>
[Flags]
public enum NavigationFailureType
{
	/// <summary>
	/// 导航被导航守卫中止。
	/// Navigation was aborted by a navigation guard.
	/// </summary>
	[Description("@#aborted")]
	Aborted = 4,

	/// <summary>
	/// 导航被取消（例如在完成前发起了新的导航）。
	/// Navigation was cancelled (e.g., a new navigation started before completion).
	/// </summary>
	[Description("@#cancelled")]
	Cancelled = 8,

	/// <summary>
	/// 导航因目标与当前路由重复而被阻止。
	/// Navigation was prevented because the target duplicates the current route.
	/// </summary>
	[Description("@#duplicated")]
	Duplicated = 16
}

/// <summary>
/// Vue Router 内部错误类别，同时也由官方公共 API 文档暴露。
/// 这些标志用于导航失败区分和匹配器/运行时重定向错误。
/// Internal Vue Router error categories also surfaced by the official public API docs.
/// These flags back navigation failure discrimination and matcher/runtime redirect errors.
/// </summary>
[Flags]
public enum ErrorTypes
{
	/// <summary>
	/// 路由匹配器未找到目标路由记录。
	/// Route matcher could not find the target route record.
	/// </summary>
	[Description("@#MATCHER_NOT_FOUND")]
	MATCHER_NOT_FOUND = 1,

	/// <summary>
	/// 导航守卫执行了重定向。
	/// A navigation guard performed a redirect.
	/// </summary>
	[Description("@#NAVIGATION_GUARD_REDIRECT")]
	NAVIGATION_GUARD_REDIRECT = 2,

	/// <summary>
	/// 导航被导航守卫中止。
	/// Navigation was aborted by a navigation guard.
	/// </summary>
	[Description("@#NAVIGATION_ABORTED")]
	NAVIGATION_ABORTED = 4,

	/// <summary>
	/// 导航被取消（例如在完成前发起了新的导航）。
	/// Navigation was cancelled (e.g., a new navigation started before completion).
	/// </summary>
	[Description("@#NAVIGATION_CANCELLED")]
	NAVIGATION_CANCELLED = 8,

	/// <summary>
	/// 导航因目标与当前路由重复而被阻止。
	/// Navigation was prevented because the target duplicates the current route.
	/// </summary>
	[Description("@#NAVIGATION_DUPLICATED")]
	NAVIGATION_DUPLICATED = 16
}

[ECMAScript("vue-router")]
[Description("@#")]
public static partial class VueRoute
{
}
