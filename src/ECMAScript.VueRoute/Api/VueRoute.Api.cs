using System.ComponentModel;

namespace ECMAScript;

public static partial class VueRoute
{
	/// <summary>
	/// Creates a Vue Router instance from the supplied strongly typed router options.
	/// </summary>
	[Description("@#createRouter")]
	public extern static Router CreateRouter(RouterOptions options);

	/// <summary>
	/// Creates an HTML5 history implementation using the browser History API.
	/// </summary>
	[Description("@#createWebHistory")]
	public extern static RouterHistory CreateWebHistory();

	/// <summary>
	/// Creates an HTML5 history implementation using the supplied base path.
	/// </summary>
	[Description("@#createWebHistory")]
	public extern static RouterHistory CreateWebHistory(string basePath);

	/// <summary>
	/// Creates a hash-based history implementation.
	/// </summary>
	[Description("@#createWebHashHistory")]
	public extern static RouterHistory CreateWebHashHistory();

	/// <summary>
	/// Creates a hash-based history implementation using the supplied base path.
	/// </summary>
	[Description("@#createWebHashHistory")]
	public extern static RouterHistory CreateWebHashHistory(string basePath);

	/// <summary>
	/// Creates an in-memory history implementation suitable for tests and SSR flows.
	/// </summary>
	[Description("@#createMemoryHistory")]
	public extern static RouterHistory CreateMemoryHistory();

	/// <summary>
	/// Creates an in-memory history implementation using the supplied base path.
	/// </summary>
	[Description("@#createMemoryHistory")]
	public extern static RouterHistory CreateMemoryHistory(string basePath);

	/// <summary>
	/// Returns the router instance injected into the current Vue component tree.
	/// </summary>
	[Description("@#useRouter")]
	public extern static Router UseRouter();

	/// <summary>
	/// Returns the currently matched loaded route for the active component tree.
	/// </summary>
	[Description("@#useRoute")]
	public extern static RouteLocationNormalizedLoaded UseRoute();

	/// <summary>
	/// Returns the low-level typed reactive contract behind <c>&lt;RouterLink&gt;</c>.
	/// </summary>
	[Description("@#useLink")]
	public extern static UseLinkReturn UseLink(UseLinkOptions options);

	/// <summary>
	/// Registers a composition API leave guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteLeave")]
	public extern static void OnBeforeRouteLeave(RouteNavigationGuard guard);

	/// <summary>
	/// Registers an async composition API leave guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteLeave")]
	public extern static void OnBeforeRouteLeave(AsyncRouteNavigationGuard guard);

	/// <summary>
	/// Registers a legacy next-callback composition API leave guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteLeave")]
	public extern static void OnBeforeRouteLeave(LegacyRouteNavigationGuard guard);

	/// <summary>
	/// Registers an async legacy next-callback composition API leave guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteLeave")]
	public extern static void OnBeforeRouteLeave(LegacyAsyncRouteNavigationGuard guard);

	/// <summary>
	/// Registers a composition API update guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteUpdate")]
	public extern static void OnBeforeRouteUpdate(RouteNavigationGuard guard);

	/// <summary>
	/// Registers an async composition API update guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteUpdate")]
	public extern static void OnBeforeRouteUpdate(AsyncRouteNavigationGuard guard);

	/// <summary>
	/// Registers a legacy next-callback composition API update guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteUpdate")]
	public extern static void OnBeforeRouteUpdate(LegacyRouteNavigationGuard guard);

	/// <summary>
	/// Registers an async legacy next-callback composition API update guard on the active matched route record.
	/// </summary>
	[Description("@#onBeforeRouteUpdate")]
	public extern static void OnBeforeRouteUpdate(LegacyAsyncRouteNavigationGuard guard);

	/// <summary>
	/// Determines whether the supplied error is a Vue Router navigation failure.
	/// </summary>
	[Description("@#isNavigationFailure")]
	public extern static bool IsNavigationFailure(Error error);

	/// <summary>
	/// Determines whether the supplied error is a specific Vue Router navigation failure kind.
	/// </summary>
	[Description("@#isNavigationFailure")]
	public extern static bool IsNavigationFailure(Error error, NavigationFailureType type);

	/// <summary>
	/// Parses a query string into Vue Router's normalized query object shape.
	/// </summary>
	[Description("@#parseQuery")]
	public extern static LocationQuery ParseQuery(string search);

	/// <summary>
	/// Serializes a raw query object using Vue Router's official query-string rules.
	/// </summary>
	[Description("@#stringifyQuery")]
	public extern static string StringifyQuery(LocationQueryRaw query);

	/// <summary>
	/// Ensures a normalized route has loaded all lazy route components and is ready for rendering.
	/// </summary>
	[Description("@#loadRouteLocation")]
	public extern static IPromise<RouteLocationNormalizedLoaded> LoadRouteLocation(RouteLocationNormalized route);

	/// <summary>
	/// Ensures a resolved route has loaded all lazy route components and is ready for rendering.
	/// </summary>
	[Description("@#loadRouteLocation")]
	public extern static IPromise<RouteLocationNormalizedLoaded> LoadRouteLocation(RouteLocationResolved route);

	/// <summary>
	/// Sentinel route representing Vue Router's start location before the first navigation resolves.
	/// </summary>
	[Description("@#START_LOCATION")]
	public extern static RouteLocationNormalizedLoaded START_LOCATION { get; }

	/// <summary>
	/// Built-in component that renders a typed router link.
	/// </summary>
	[Description("@#RouterLink")]
	public extern static Vue3.IVueComponent<RouterLinkProps, RouterLinkSlots> RouterLink { get; }

	/// <summary>
	/// Built-in component that renders the active route component tree.
	/// </summary>
	[Description("@#RouterView")]
	public extern static Vue3.IVueComponent<RouterViewProps, RouterViewSlots> RouterView { get; }
}
