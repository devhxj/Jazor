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

public delegate RouteLocationRaw RouteRedirectCallback(RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate void NavigationGuardNextCallback(Vue3.VueComponentPublicInstance instance);

public delegate void NavigationGuardNext(NavigationGuardNextArgument? value);

public delegate NavigationGuardReturn? LegacyRouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next);

public delegate IPromise<NavigationGuardReturn?> LegacyAsyncRouteNavigationGuard(RouteLocationNormalized to, RouteLocationNormalizedLoaded from, NavigationGuardNext next);

public delegate void RouterErrorHandler(Vue3.VueValue? error, RouteLocationNormalized to, RouteLocationNormalizedLoaded from);

public delegate Vue3.IVNode RouterLinkSlotCallback(UseLinkResult link);

public delegate Vue3.IVNode RouterViewSlotCallback(RouterViewSlotScope scope);

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

	[Description("@#unknown")]
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
