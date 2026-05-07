using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public delegate NavigationGuardReturn? RouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate IPromise<NavigationGuardReturn?> AsyncRouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate LocationQuery RouteQueryParser(string search);

public delegate string RouteQueryStringifier(LocationQueryRaw? query);

public delegate Vue3.VueProps RouteRecordPropsResolver(RouteLocationNormalized to);

public delegate RouterScrollResult? RouterScrollBehavior(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, ScrollPositionNormalized? savedPosition);

public delegate IPromise<RouterScrollResult?> AsyncRouterScrollBehavior(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, ScrollPositionNormalized? savedPosition);

public delegate void AfterNavigationHook(RouteLocationNormalizedLoaded to, RouteLocationNormalizedLoaded from, NavigationFailure? failure);

public delegate IPromise<IVueComponent> RouteComponentLoader();

public delegate RouteLocationRaw RouteRedirectCallback(RouteLocation to, RouteLocationNormalizedLoaded from);

[Obsolete("Vue Router 4 recommends return-based navigation guards. Use bool/RouteLocationRaw/Error returns instead of next(...).")]
public delegate void NavigationGuardNextCallback(Vue3.VueComponentPublicInstance instance);

[Obsolete("Vue Router 4 recommends return-based navigation guards. Use bool/RouteLocationRaw/Error returns instead of next(...).")]
public delegate void NavigationGuardNext(NavigationGuardNextArgument? value = default);

[Obsolete("Vue Router 4 keeps the third next parameter for backward compatibility only. Prefer RouteNavigationGuard return values.")]
public delegate NavigationGuardReturn? LegacyRouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next);

[Obsolete("Vue Router 4 keeps the third next parameter for backward compatibility only. Prefer AsyncRouteNavigationGuard return values.")]
public delegate IPromise<NavigationGuardReturn?> LegacyAsyncRouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next);

public delegate void ErrorRouterErrorHandler(Error error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate void NavigationFailureRouterErrorHandler(NavigationFailure error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate void NavigationRedirectRouterErrorHandler(NavigationRedirectError error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate void StringRouterErrorHandler(string error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate void NumberRouterErrorHandler(Number error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate void BooleanRouterErrorHandler(bool error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate void BigIntRouterErrorHandler(BigInt error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate void SymbolRouterErrorHandler(Symbol error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate void ObjectRouterErrorHandler(IObject error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate void ArrayRouterErrorHandler(Array<RouterErrorValue?> error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate IPromise<RouteNavigationResult?> RouterLinkNavigateCallback(MouseEvent? @event = null);

public delegate Vue3.IVNode[] RouterLinkSlotCallback(RouterLinkSlotScope link);

public delegate Vue3.IVNode[] RouterViewSlotCallback(RouterViewSlotScope scope);

public delegate void RouterHistoryNavigationCallback(string to, string from, RouterHistoryNavigationInformation information);

[String]
public enum RouterHistoryNavigationType
{
	[Description("@#pop")]
	Pop,

	[Description("@#push")]
	Push
}

[String]
public enum RouterHistoryNavigationDirection
{
	[Description("@#back")]
	Back,

	[Description("@#forward")]
	Forward,

	[ECMAScriptName("")]
	Unknown
}

[Flags]
public enum NavigationFailureType
{
	[Description("@#aborted")]
	Aborted = 4,

	[Description("@#cancelled")]
	Cancelled = 8,

	[Description("@#duplicated")]
	Duplicated = 16
}

/// <summary>
/// Internal Vue Router error categories also surfaced by the official public API docs.
/// These flags back navigation failure discrimination and matcher/runtime redirect errors.
/// </summary>
[Flags]
public enum ErrorTypes
{
	[Description("@#MATCHER_NOT_FOUND")]
	MATCHER_NOT_FOUND = 1,

	[Description("@#NAVIGATION_GUARD_REDIRECT")]
	NAVIGATION_GUARD_REDIRECT = 2,

	[Description("@#NAVIGATION_ABORTED")]
	NAVIGATION_ABORTED = 4,

	[Description("@#NAVIGATION_CANCELLED")]
	NAVIGATION_CANCELLED = 8,

	[Description("@#NAVIGATION_DUPLICATED")]
	NAVIGATION_DUPLICATED = 16
}

[ECMAScript("npm:vue-router@4")]
[Description("@#")]
public static partial class VueRoute
{
}
