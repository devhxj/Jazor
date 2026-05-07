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

public delegate void NavigationGuardNextCallback(Vue3.VueComponentPublicInstance instance);

public delegate void NavigationGuardNext(NavigationGuardNextArgument? value = default);

public delegate NavigationGuardReturn? LegacyRouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next);

public delegate IPromise<NavigationGuardReturn?> LegacyAsyncRouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next);

public delegate void ErrorRouterErrorHandler(Error error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate void NavigationFailureRouterErrorHandler(NavigationFailure error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

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

[ECMAScript("npm:vue-router@4")]
[Description("@#")]
public static partial class VueRoute
{
}
