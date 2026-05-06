using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

[ECMAScript]
[Description("@#")]
public record RouterOptions : Vue3.VueProps
{
	[Description("@#history")]
	public RouterHistory History { get; init; } = default!;

	[Description("@#routes")]
	public RouteRecordRaw[] Routes { get; init; } = default!;

	[Description("@#linkActiveClass")]
	public string? LinkActiveClass { get; init; }

	[Description("@#linkExactActiveClass")]
	public string? LinkExactActiveClass { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteMeta : Vue3.VueDictionary<Vue3.VueValue>;

[ECMAScript]
[Description("@#")]
public record LocationQuery : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern LocationQueryValue? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, LocationQueryValue value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record LocationQueryRaw : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern LocationQueryValueRaw? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, LocationQueryValueRaw value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record RouteParams : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern RouteParam? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteParam value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record RouteParamsRaw : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern RouteParamRaw? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteParamRaw value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record RouteLocationAsPath : Vue3.VueProps
{
	[Description("@#path")]
	public string Path { get; init; } = default!;

	[Description("@#query")]
	public LocationQueryRaw? Query { get; init; }

	[Description("@#hash")]
	public string? Hash { get; init; }

	[Description("@#replace")]
	public bool? Replace { get; init; }

	[Description("@#force")]
	public bool? Force { get; init; }

	[Description("@#state")]
	public Vue3.VueValue? State { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteLocationAsRelative : Vue3.VueProps
{
	[Description("@#name")]
	public RouteRecordName? Name { get; init; }

	[Description("@#params")]
	public RouteParamsRaw? Params { get; init; }

	[Description("@#query")]
	public LocationQueryRaw? Query { get; init; }

	[Description("@#hash")]
	public string? Hash { get; init; }

	[Description("@#replace")]
	public bool? Replace { get; init; }

	[Description("@#force")]
	public bool? Force { get; init; }

	[Description("@#state")]
	public Vue3.VueValue? State { get; init; }
}

[ECMAScript]
[Description("@#")]
public abstract class RouteLocationMatched
{
	protected RouteLocationMatched()
	{
	}

	[Description("@#path")]
	public extern string Path { get; }

	[Description("@#name")]
	public extern RouteRecordName? Name { get; }

	[Description("@#meta")]
	public extern RouteMeta Meta { get; }

	[Description("@#redirect")]
	public extern RouteRedirectOption? Redirect { get; }

	[Description("@#components")]
	public extern RouteComponents? Components { get; }
}

[ECMAScript]
[Description("@#")]
public abstract class RouteLocationNormalizedLoaded
{
	protected RouteLocationNormalizedLoaded()
	{
	}

	[Description("@#fullPath")]
	public extern string FullPath { get; }

	[Description("@#path")]
	public extern string Path { get; }

	[Description("@#query")]
	public extern LocationQuery Query { get; }

	[Description("@#hash")]
	public extern string Hash { get; }

	[Description("@#name")]
	public extern RouteRecordName? Name { get; }

	[Description("@#params")]
	public extern RouteParams Params { get; }

	[Description("@#matched")]
	public extern RouteLocationMatched[] Matched { get; }

	[Description("@#meta")]
	public extern RouteMeta Meta { get; }

	[Description("@#redirectedFrom")]
	public extern RouteLocationNormalizedLoaded? RedirectedFrom { get; }
}

[ECMAScript]
[Description("@#")]
public abstract class RouteLocationResolved : RouteLocationNormalizedLoaded
{
	protected RouteLocationResolved()
	{
	}

	[Description("@#href")]
	public extern string Href { get; }
}

[ECMAScript]
[Description("@#")]
public abstract class RouterHistory
{
	protected RouterHistory()
	{
	}

	[Description("@#base")]
	public extern string Base { get; }

	[Description("@#location")]
	public extern string Location { get; }

	[Description("@#state")]
	public extern Vue3.VueValue? State { get; }

	[Description("@#createHref")]
	public extern string CreateHref(string location);

	[Description("@#go")]
	public extern void Go(Number delta);

	[Description("@#destroy")]
	public extern void Destroy();
}

[ECMAScript]
[Description("@#")]
public abstract record Router : Vue3.VuePlugin
{
	[Description("@#currentRoute")]
	public extern Vue3.VueReadonlyRef<RouteLocationNormalizedLoaded> CurrentRoute { get; }

	[Description("@#options")]
	public extern RouterOptions Options { get; }

	[Description("@#addRoute")]
	public extern Action AddRoute(RouteRecordRaw route);

	[Description("@#addRoute")]
	public extern Action AddRoute(RouteRecordName parentName, RouteRecordRaw route);

	[Description("@#removeRoute")]
	public extern void RemoveRoute(RouteRecordName routeName);

	[Description("@#hasRoute")]
	public extern bool HasRoute(RouteRecordName routeName);

	[Description("@#resolve")]
	public extern RouteLocationResolved Resolve(RouteLocationRaw to);

	[Description("@#push")]
	public extern IPromise<NavigationFailure?> Push(RouteLocationRaw to);

	[Description("@#replace")]
	public extern IPromise<NavigationFailure?> Replace(RouteLocationRaw to);

	[Description("@#go")]
	public extern void Go(Number delta);

	[Description("@#back")]
	public extern void Back();

	[Description("@#forward")]
	public extern void Forward();

	[Description("@#beforeEach")]
	public extern Action BeforeEach(NavigationGuardHandler guard);

	[Description("@#beforeResolve")]
	public extern Action BeforeResolve(NavigationGuardHandler guard);

	[Description("@#afterEach")]
	public extern Action AfterEach(AfterNavigationHook hook);

	[Description("@#isReady")]
	public extern IPromise IsReady();
}

[ECMAScript]
[Description("@#")]
public abstract class NavigationFailure : Error
{
	protected NavigationFailure()
	{
	}

	[Description("@#type")]
	public extern NavigationFailureType Type { get; }

	[Description("@#to")]
	public extern RouteLocationNormalizedLoaded To { get; }

	[Description("@#from")]
	public extern RouteLocationNormalizedLoaded From { get; }
}

[ECMAScript]
[Description("@#")]
public record RouteComponents : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern RouteComponent? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteComponent value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record RouteNamedProps : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern RouteRecordProps? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteRecordProps value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public abstract record RouteRecordBase : Vue3.VueProps
{
	[Description("@#name")]
	public RouteRecordName? Name { get; init; }

	[Description("@#path")]
	public string Path { get; init; } = default!;

	[Description("@#alias")]
	public RouteRecordAlias? Alias { get; init; }

	[Description("@#redirect")]
	public RouteRedirectOption? Redirect { get; init; }

	[Description("@#children")]
	public RouteRecordRaw[]? Children { get; init; }

	[Description("@#meta")]
	public RouteMeta? Meta { get; init; }

	[Description("@#beforeEnter")]
	public RouteRecordBeforeEnter? BeforeEnter { get; init; }

	[Description("@#sensitive")]
	public bool? Sensitive { get; init; }

	[Description("@#strict")]
	public bool? Strict { get; init; }

	[Description("@#end")]
	public bool? End { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteRecordSingleView : RouteRecordBase
{
	[Description("@#component")]
	public RouteComponent? Component { get; init; }

	[Description("@#props")]
	public RouteRecordProps? Props { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteRecordSingleViewWithChildren : RouteRecordSingleView;

[ECMAScript]
[Description("@#")]
public record RouteRecordMultipleViews : RouteRecordBase
{
	[Description("@#components")]
	public RouteComponents? Components { get; init; }

	[Description("@#props")]
	public RouteNamedProps? Props { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteRecordMultipleViewsWithChildren : RouteRecordMultipleViews;

[ECMAScript]
[Description("@#")]
public record RouteRecordRedirect : RouteRecordBase;

[ECMAScript]
[Description("@#")]
public record RouterLinkProps : Vue3.VueProps
{
	[Description("@#to")]
	public RouteLocationRaw To { get; init; } = default!;

	[Description("@#replace")]
	public bool? Replace { get; init; }

	[Description("@#custom")]
	public bool? Custom { get; init; }

	[Description("@#activeClass")]
	public string? ActiveClass { get; init; }

	[Description("@#exactActiveClass")]
	public string? ExactActiveClass { get; init; }

	[Description("@#ariaCurrentValue")]
	public string? AriaCurrentValue { get; init; }

	[Description("@#viewTransition")]
	public bool? ViewTransition { get; init; }
}

[ECMAScript]
[Description("@#")]
public record UseLinkOptions : Vue3.VueProps
{
	[Description("@#to")]
	public RouteLocationRaw To { get; init; } = default!;

	[Description("@#replace")]
	public bool? Replace { get; init; }

	[Description("@#viewTransition")]
	public bool? ViewTransition { get; init; }
}

[ECMAScript]
[Description("@#")]
public abstract class UseLinkResult
{
	protected UseLinkResult()
	{
	}

	[Description("@#route")]
	public extern Vue3.VueReadonlyRef<RouteLocationResolved> Route { get; }

	[Description("@#href")]
	public extern Vue3.VueReadonlyRef<string> Href { get; }

	[Description("@#isActive")]
	public extern Vue3.VueReadonlyRef<bool> IsActive { get; }

	[Description("@#isExactActive")]
	public extern Vue3.VueReadonlyRef<bool> IsExactActive { get; }

	[Description("@#navigate")]
	public extern IPromise<NavigationFailure?> Navigate();

	[Description("@#navigate")]
	public extern IPromise<NavigationFailure?> Navigate(MouseEvent @event);
}

[ECMAScript]
[Description("@#")]
public record RouterLinkSlots : Vue3.VueSlots
{
	[Description("@#default")]
	public RouterLinkSlotCallback? Default { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouterViewProps : Vue3.VueProps
{
	[Description("@#name")]
	public string? Name { get; init; }

	[Description("@#route")]
	public RouteLocationNormalizedLoaded? Route { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouterViewSlotScope : Vue3.VueProps
{
	[Description("@#Component")]
	public IVueComponent? Component { get; init; }

	[Description("@#route")]
	public RouteLocationNormalizedLoaded Route { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public record RouterViewSlots : Vue3.VueSlots
{
	[Description("@#default")]
	public RouterViewSlotCallback? Default { get; init; }
}
