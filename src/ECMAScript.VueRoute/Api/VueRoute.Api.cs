using System.ComponentModel;

namespace ECMAScript;

public static partial class VueRoute
{
	/// <summary>
	/// 根据提供的强类型路由选项创建 Vue Router 实例。
	/// Creates a Vue Router instance from the supplied strongly typed router options.
	/// </summary>
	[Description("@#createRouter")]
	public extern static Router CreateRouter(RouterOptions options);

	/// <summary>
	/// 根据提供的路由表和全局路径解析器选项创建底层路由匹配器。
	/// Creates a low-level route matcher with the supplied route table and global path-parser options.
	/// </summary>
	[Description("@#createRouterMatcher")]
	public extern static RouterMatcher CreateRouterMatcher(RouteRecordRaw[] routes, PathParserOptions globalOptions);

	/// <summary>
	/// 使用浏览器 History API 创建 HTML5 历史记录实现。
	/// Creates an HTML5 history implementation using the browser History API.
	/// </summary>
	[Description("@#createWebHistory")]
	public extern static RouterHistory CreateWebHistory();

	/// <summary>
	/// 使用提供的基础路径创建 HTML5 历史记录实现。
	/// Creates an HTML5 history implementation using the supplied base path.
	/// </summary>
	[Description("@#createWebHistory")]
	public extern static RouterHistory CreateWebHistory(string basePath);

	/// <summary>
	/// 创建基于哈希的历史记录实现。
	/// Creates a hash-based history implementation.
	/// </summary>
	[Description("@#createWebHashHistory")]
	public extern static RouterHistory CreateWebHashHistory();

	/// <summary>
	/// 使用提供的基础路径创建基于哈希的历史记录实现。
	/// Creates a hash-based history implementation using the supplied base path.
	/// </summary>
	[Description("@#createWebHashHistory")]
	public extern static RouterHistory CreateWebHashHistory(string basePath);

	/// <summary>
	/// 创建适用于测试和 SSR 流程的内存历史记录实现。
	/// Creates an in-memory history implementation suitable for tests and SSR flows.
	/// </summary>
	[Description("@#createMemoryHistory")]
	public extern static RouterHistory CreateMemoryHistory();

	/// <summary>
	/// 使用提供的基础路径创建内存历史记录实现。
	/// Creates an in-memory history implementation using the supplied base path.
	/// </summary>
	[Description("@#createMemoryHistory")]
	public extern static RouterHistory CreateMemoryHistory(string basePath);

	/// <summary>
	/// 返回注入到当前 Vue 组件树中的路由器实例。
	/// Returns the router instance injected into the current Vue component tree.
	/// </summary>
	[Description("@#useRouter")]
	public extern static Router UseRouter();

	/// <summary>
	/// 返回活动组件树中当前匹配的已加载路由。
	/// Returns the currently matched loaded route for the active component tree.
	/// </summary>
	[Description("@#useRoute")]
	public extern static RouteLocationNormalizedLoaded UseRoute();

	/// <summary>
	/// 返回 <c>&lt;RouterLink&gt;</c> 背后的底层类型化响应式契约。
	/// Returns the low-level typed reactive contract behind <c>&lt;RouterLink&gt;</c>.
	/// </summary>
	[Description("@#useLink")]
	public extern static UseLinkReturn UseLink(UseLinkOptions options);

	/// <summary>
	/// 在当前匹配的路由记录上注册组合式 API 的离开守卫。
	/// Registers a composition API leave guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteLeave")]
	public extern static void OnBeforeRouteLeave(RouteNavigationGuard guard);

	/// <summary>
	/// 在当前匹配的路由记录上注册异步组合式 API 的离开守卫。
	/// Registers an async composition API leave guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteLeave")]
	public extern static void OnBeforeRouteLeave(AsyncRouteNavigationGuard guard);

	/// <summary>
	/// 在当前匹配的路由记录上注册旧式 next 回调组合式 API 的离开守卫。
	/// Registers a legacy next-callback composition API leave guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteLeave")]
	public extern static void OnBeforeRouteLeave(LegacyRouteNavigationGuard guard);

	/// <summary>
	/// 在当前匹配的路由记录上注册异步旧式 next 回调组合式 API 的离开守卫。
	/// Registers an async legacy next-callback composition API leave guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteLeave")]
	public extern static void OnBeforeRouteLeave(LegacyAsyncRouteNavigationGuard guard);

	/// <summary>
	/// 在当前匹配的路由记录上注册组合式 API 的更新守卫。
	/// Registers a composition API update guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteUpdate")]
	public extern static void OnBeforeRouteUpdate(RouteNavigationGuard guard);

	/// <summary>
	/// 在当前匹配的路由记录上注册异步组合式 API 的更新守卫。
	/// Registers an async composition API update guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteUpdate")]
	public extern static void OnBeforeRouteUpdate(AsyncRouteNavigationGuard guard);

	/// <summary>
	/// 在当前匹配的路由记录上注册旧式 next 回调组合式 API 的更新守卫。
	/// Registers a legacy next-callback composition API update guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteUpdate")]
	public extern static void OnBeforeRouteUpdate(LegacyRouteNavigationGuard guard);

	/// <summary>
	/// 在当前匹配的路由记录上注册异步旧式 next 回调组合式 API 的更新守卫。
	/// Registers an async legacy next-callback composition API update guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteUpdate")]
	public extern static void OnBeforeRouteUpdate(LegacyAsyncRouteNavigationGuard guard);

	/// <summary>
	/// 判断提供的错误是否为 Vue Router 导航失败。
	/// Determines whether the supplied error is a Vue Router navigation failure.
	/// </summary>
	[Description("@#isNavigationFailure")]
	public extern static bool IsNavigationFailure(Error error);

	/// <summary>
	/// 判断提供的错误是否为特定类型的 Vue Router 导航失败。
	/// Determines whether the supplied error is a specific Vue Router navigation failure kind.
	/// </summary>
	[Description("@#isNavigationFailure")]
	public extern static bool IsNavigationFailure(Error error, NavigationFailureType type);

	/// <summary>
	/// 判断提供的错误是否为特定类别的 Vue Router 内部错误。
	/// Determines whether the supplied error is a specific Vue Router internal error category.
	/// </summary>
	[Description("@#isNavigationFailure")]
	public extern static bool IsNavigationFailure(Error error, ErrorTypes type);

	/// <summary>
	/// 将查询字符串解析为 Vue Router 的标准化查询对象结构。
	/// Parses a query string into Vue Router's normalized query object shape.
	/// </summary>
	[Description("@#parseQuery")]
	public extern static LocationQuery ParseQuery(string search);

	/// <summary>
	/// 使用 Vue Router 的官方查询字符串规则序列化原始查询对象。
	/// Serializes a raw query object using Vue Router's official query-string rules.
	/// </summary>
	[Description("@#stringifyQuery")]
	public extern static string StringifyQuery(LocationQueryRaw query);

	/// <summary>
	/// 确保标准化路由已加载所有惰性路由组件并准备好渲染。
	/// Ensures a normalized route has loaded all lazy route components and is ready for rendering.
	/// </summary>
	[Description("@#loadRouteLocation")]
	public extern static IPromise<RouteLocationNormalizedLoaded> LoadRouteLocation(RouteLocationNormalized route);

	/// <summary>
	/// 确保路由位置对象结构已加载所有惰性路由组件并准备好渲染。
	/// Ensures a route-location object shape has loaded all lazy route components and is ready for rendering.
	/// </summary>
	[Description("@#loadRouteLocation")]
	public extern static IPromise<RouteLocationNormalizedLoaded> LoadRouteLocation(RouteLocation route);

	/// <summary>
	/// 表示 Vue Router 在首次导航解析之前的起始位置的哨兵路由。
	/// Sentinel route representing Vue Router's start location before the first navigation resolves.
	/// </summary>
	[Description("@#START_LOCATION")]
	public extern static RouteLocationNormalizedLoaded START_LOCATION { get; }

	/// <summary>
	/// 由 Vue Router 提供的活动路由器实例的类型化注入键。
	/// Typed injection key for the active router instance provided by Vue Router.
	/// </summary>
	[Description("@#routerKey")]
	public extern static Vue.VueInjectionKey<Router> RouterKey { get; }

	/// <summary>
	/// 暴露给组合式消费者的当前已加载路由的类型化注入键。
	/// Typed injection key for the current loaded route exposed to composition consumers.
	/// </summary>
	[Description("@#routeLocationKey")]
	public extern static Vue.VueInjectionKey<RouteLocationNormalizedLoaded> RouteLocationKey { get; }

	/// <summary>
	/// 由 <c>RouterView</c> 消费的响应式路由位置源的类型化注入键。
	/// Typed injection key for the reactive route location source consumed by <c>RouterView</c>.
	/// </summary>
	[Description("@#routerViewLocationKey")]
	public extern static Vue.VueInjectionKey<Vue.IVueRef<RouteLocationNormalizedLoaded>> RouterViewLocationKey { get; }

	/// <summary>
	/// 由最近路由视图渲染的当前匹配路由记录的类型化注入键。
	/// Typed injection key for the matched route record currently rendered by the nearest router view.
	/// </summary>
	[Description("@#matchedRouteKey")]
	public extern static Vue.VueInjectionKey<Vue.VueComputedRef<RouteRecordNormalized?>> MatchedRouteKey { get; }

	/// <summary>
	/// 当前 router-view 嵌套深度的类型化注入键。
	/// Typed injection key for the current router-view nesting depth.
	/// </summary>
	[Description("@#viewDepthKey")]
	public extern static Vue.VueInjectionKey<RouterViewDepthValue> ViewDepthKey { get; }

	/// <summary>
	/// 渲染类型化路由链接的内置组件。
	/// Built-in component that renders a typed router link.
	/// </summary>
	[Description("@#RouterLink")]
	public extern static Vue.IVueComponent<RouterLinkProps, RouterLinkSlots> RouterLink { get; }

	/// <summary>
	/// 渲染活动路由组件树的内置组件。
	/// Built-in component that renders the active route component tree.
	/// </summary>
	[Description("@#RouterView")]
	public extern static Vue.IVueComponent<RouterViewProps, RouterViewSlots> RouterView { get; }
}
