using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public delegate NavigationGuardReturn? RouteNavigationGuard(RouteLocationNormalizedLoaded to, RouteLocationNormalizedLoaded from);

public delegate IPromise<NavigationGuardReturn?> AsyncRouteNavigationGuard(RouteLocationNormalizedLoaded to, RouteLocationNormalizedLoaded from);

public delegate void AfterNavigationHook(RouteLocationNormalizedLoaded to, RouteLocationNormalizedLoaded from, NavigationFailure? failure);

public delegate IPromise<IVueComponent> RouteComponentLoader();

public delegate RouteLocationRaw RouteRedirectCallback(RouteLocationNormalizedLoaded to);

public delegate Vue3.IVNode RouterLinkSlotCallback(UseLinkResult link);

public delegate Vue3.IVNode RouterViewSlotCallback(RouterViewSlotScope scope);

public enum NavigationFailureType
{
	[Description("@#aborted")]
	Aborted,

	[Description("@#cancelled")]
	Cancelled,

	[Description("@#duplicated")]
	Duplicated
}

[ECMAScript("npm:vue-router@4")]
[Description("@#")]
public static partial class VueRoute
{
}
