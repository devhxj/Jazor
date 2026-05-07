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

	[Description("@#sensitive")]
	public bool? Sensitive { get; init; }

	[Description("@#strict")]
	public bool? Strict { get; init; }

	[Description("@#end")]
	[Obsolete("Vue Router 4 documents end as deprecated and always true. Do not author new router options with End.")]
	public bool? End { get; init; }

	[Description("@#linkActiveClass")]
	public string? LinkActiveClass { get; init; }

	[Description("@#linkExactActiveClass")]
	public string? LinkExactActiveClass { get; init; }

	[Description("@#scrollBehavior")]
	public RouterScrollHandler? ScrollBehavior { get; init; }

	[Description("@#parseQuery")]
	public RouteQueryParser? ParseQuery { get; init; }

	[Description("@#stringifyQuery")]
	public RouteQueryStringifier? StringifyQuery { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed class RouteMetaValue
{
	private RouteMetaValue()
	{
	}

	public extern static implicit operator RouteMetaValue(string value);

	public extern static implicit operator RouteMetaValue(bool value);

	public extern static implicit operator RouteMetaValue(Number value);

	public extern static implicit operator RouteMetaValue(BigInt value);

	public extern static implicit operator RouteMetaValue(Symbol value);

	public extern static implicit operator RouteMetaValue(char value);

	public extern static implicit operator RouteMetaValue(double value);

	public extern static implicit operator RouteMetaValue(float value);

	public extern static implicit operator RouteMetaValue(int value);

	public extern static implicit operator RouteMetaValue(long value);

	public extern static implicit operator RouteMetaValue(short value);

	public extern static implicit operator RouteMetaValue(ushort value);

	public extern static implicit operator RouteMetaValue(byte value);

	public extern static implicit operator RouteMetaValue(sbyte value);

	public extern static implicit operator RouteMetaValue(uint value);

	public extern static implicit operator RouteMetaValue(ulong value);

	public extern static implicit operator RouteMetaValue(decimal value);

	public extern static implicit operator RouteMetaValue(Action value);

	[ECMAScriptInline("__arg1")]
	public extern static RouteMetaValue From(Action value);

	public extern static implicit operator RouteMetaValue(Vue3.VueProps value);

	public extern static implicit operator RouteMetaValue(Array<RouteMetaValue?> value);

	public extern static implicit operator RouteMetaValue(RouteMetaValue?[] value);
}

[ECMAScript]
[Description("@#")]
public record RouteMeta : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern RouteMetaValue? this[string key] { get; set; }

	public extern RouteMetaValue? this[Number key] { get; set; }

	public extern RouteMetaValue? this[Symbol key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteMetaValue? value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, Action value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Number key, RouteMetaValue? value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Number key, Action value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Symbol key, RouteMetaValue? value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Symbol key, Action value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record HistoryState : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern HistoryStateValue? this[string key] { get; set; }

	public extern HistoryStateValue? this[Number key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, HistoryStateValue? value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Number key, HistoryStateValue? value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record LocationQuery : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern LocationQueryValue? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, LocationQueryValue? value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record LocationQueryRaw : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern LocationQueryValueRaw? this[string key] { get; set; }

	public extern LocationQueryValueRaw? this[Number key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, LocationQueryValueRaw? value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(Number key, LocationQueryValueRaw? value);

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
	public extern void Add(string key, RouteParamRaw? value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public abstract record RouteLocationOptions : Vue3.VueProps
{
	[Description("@#replace")]
	public bool? Replace { get; init; }

	[Description("@#force")]
	public bool? Force { get; init; }

	[Description("@#state")]
	public HistoryState? State { get; init; }
}

[ECMAScript]
[Description("@#")]
public abstract class RouteLocation
{
	protected RouteLocation()
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

	[Description("@#meta")]
	public extern RouteMeta Meta { get; }

	[Description("@#matched")]
	public extern RouteRecordNormalized[] Matched { get; }

	[Description("@#redirectedFrom")]
	public extern RouteLocation? RedirectedFrom { get; }

	[Description("@#replace")]
	public extern bool? Replace { get; }

	[Description("@#force")]
	public extern bool? Force { get; }

	[Description("@#state")]
	public extern HistoryState? State { get; }
}

[ECMAScript]
[Description("@#")]
public abstract record RouteLocationPathRawBase : RouteLocationOptions
{
	[Description("@#query")]
	public LocationQueryRaw? Query { get; init; }

	[Description("@#hash")]
	public string? Hash { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteLocationAsPath : RouteLocationPathRawBase
{
	[Description("@#path")]
	public string Path { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public abstract record RouteQueryAndHash : Vue3.VueProps
{
	[Description("@#query")]
	public LocationQueryRaw? Query { get; init; }

	[Description("@#hash")]
	public string? Hash { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteLocationPathRaw : RouteLocationPathRawBase
{
	[Description("@#path")]
	public string Path { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public abstract record LocationAsRelativeRaw : RouteLocationOptions
{
	[Description("@#name")]
	public RouteRecordName? Name { get; init; }

	[Description("@#params")]
	public RouteParamsRaw? Params { get; init; }

	[Description("@#query")]
	public LocationQueryRaw? Query { get; init; }

	[Description("@#hash")]
	public string? Hash { get; init; }

}

[ECMAScript]
[Description("@#")]
public record RouteLocationAsRelative : LocationAsRelativeRaw
{
}

[ECMAScript]
[Description("@#")]
public record RouteLocationNamedRaw : LocationAsRelativeRaw
{
}

[ECMAScript]
[Description("@#")]
public record PathParserOptions : Vue3.VueProps
{
	[Description("@#sensitive")]
	public bool? Sensitive { get; init; }

	[Description("@#strict")]
	public bool? Strict { get; init; }

	[Description("@#end")]
	[Obsolete("Vue Router 4 documents end as deprecated and always true. Do not author new path parser options with End.")]
	public bool? End { get; init; }
}

[ECMAScript]
[Description("@#")]
public record PathParserKey : Vue3.VueProps
{
	[Description("@#name")]
	public string Name { get; init; } = default!;

	[Description("@#repeatable")]
	public bool Repeatable { get; init; }

	[Description("@#optional")]
	public bool Optional { get; init; }
}

[ECMAScript]
[Description("@#")]
public abstract class PathParser
{
	protected PathParser()
	{
	}

	[Description("@#re")]
	public extern RegExp Re { get; }

	[Description("@#score")]
	public extern Array<Array<Number>> Score { get; }

	[Description("@#keys")]
	public extern PathParserKey[] Keys { get; }

	[Description("@#parse")]
	public extern RouteParams? Parse(string path);

	[Description("@#stringify")]
	public extern string Stringify(RouteParams routeParams);
}

[ECMAScript]
[Description("@#")]
public record MatcherLocationAsPath : Vue3.VueProps
{
	[Description("@#path")]
	public string Path { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public record MatcherLocationAsRelative : Vue3.VueProps
{
	[Description("@#params")]
	public RouteParams? Params { get; init; }
}

[ECMAScript]
[Description("@#")]
public record MatcherLocationAsName : Vue3.VueProps
{
	[Description("@#name")]
	public RouteRecordName Name { get; init; } = default!;

	[Description("@#params")]
	public RouteParams? Params { get; init; }
}

[ECMAScript]
[Description("@#")]
public record MatcherLocation : Vue3.VueProps
{
	[Description("@#name")]
	public RouteRecordName? Name { get; init; }

	[Description("@#path")]
	public string Path { get; init; } = default!;

	[Description("@#params")]
	public RouteParams Params { get; init; } = default!;

	[Description("@#meta")]
	public RouteMeta Meta { get; init; } = default!;

	[Description("@#matched")]
	public RouteRecordNormalized[] Matched { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public abstract class RouteRecordNormalized
{
	protected RouteRecordNormalized()
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
	public extern RawRouteComponents? Components { get; }

	[Description("@#children")]
	public extern RouteRecordRaw[] Children { get; }

	[Description("@#props")]
	public extern RouteNamedProps Props { get; }

	[Description("@#beforeEnter")]
	public extern RouteRecordBeforeEnter? BeforeEnter { get; }

	[Description("@#leaveGuards")]
	public extern Set<NavigationGuardHandler> LeaveGuards { get; }

	[Description("@#updateGuards")]
	public extern Set<NavigationGuardHandler> UpdateGuards { get; }

	[Description("@#enterCallbacks")]
	public extern NavigationGuardNextCallbackMap EnterCallbacks { get; }

	[Description("@#instances")]
	public extern RouteComponentInstanceMap Instances { get; }

	[Description("@#aliasOf")]
	public extern RouteRecordNormalized? AliasOf { get; }

	[Description("@#mods")]
	public extern Vue3.VueDictionary Mods { get; }
}

[ECMAScript]
[Description("@#")]
public abstract class RouteLocationMatched : RouteRecordNormalized
{
	protected RouteLocationMatched()
	{
	}

	[Description("@#components")]
	public extern new RouteComponents? Components { get; }
}

[ECMAScript]
[Description("@#")]
public abstract class RouteRecordMatcher : PathParser
{
	protected RouteRecordMatcher()
	{
	}

	[Description("@#record")]
	public extern RouteRecordNormalized Record { get; }

	[Description("@#parent")]
	public extern RouteRecordMatcher? Parent { get; }

	[Description("@#children")]
	public extern RouteRecordMatcher[] Children { get; }

	[Description("@#alias")]
	public extern RouteRecordMatcher[] Alias { get; }
}

[ECMAScript]
[Description("@#")]
public abstract class RouteLocationNormalized
{
	protected RouteLocationNormalized()
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

	[Description("@#meta")]
	public extern RouteMeta Meta { get; }

	[Description("@#matched")]
	public extern RouteRecordNormalized[] Matched { get; }

	[Description("@#redirectedFrom")]
	public extern RouteLocation? RedirectedFrom { get; }
}

[ECMAScript]
[Description("@#")]
public abstract class RouteLocationNormalizedLoaded : RouteLocationNormalized
{
	protected RouteLocationNormalizedLoaded()
	{
	}

	[Description("@#matched")]
	public extern new RouteLocationMatched[] Matched { get; }
}

[ECMAScript]
[Description("@#")]
public abstract class RouteLocationResolved : RouteLocation
{
	protected RouteLocationResolved()
	{
	}

	[Description("@#href")]
	public extern string Href { get; }
}

[ECMAScript]
[Description("@#")]
public abstract class RouterHistoryNavigationInformation
{
	protected RouterHistoryNavigationInformation()
	{
	}

	[Description("@#type")]
	public extern RouterHistoryNavigationType Type { get; }

	[Description("@#direction")]
	public extern RouterHistoryNavigationDirection Direction { get; }

	[Description("@#delta")]
	public extern Number Delta { get; }
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
	public extern HistoryState State { get; }

	[Description("@#push")]
	public extern void Push(string to);

	[Description("@#push")]
	public extern void Push(string to, HistoryState? data);

	[Description("@#replace")]
	public extern void Replace(string to);

	[Description("@#replace")]
	public extern void Replace(string to, HistoryState? data);

	[Description("@#listen")]
	public extern Action Listen(RouterHistoryNavigationCallback callback);

	[Description("@#createHref")]
	public extern string CreateHref(string location);

	[Description("@#go")]
	public extern void Go(Number delta);

	[Description("@#go")]
	public extern void Go(Number delta, bool triggerListeners);

	[Description("@#destroy")]
	public extern void Destroy();
}

[ECMAScript]
[Description("@#")]
public abstract record Router : Vue3.VuePlugin
{
	[Description("@#currentRoute")]
	public extern Vue3.VueReadonlyRef<RouteLocationNormalizedLoaded> CurrentRoute { get; }

	[Description("@#listening")]
	public extern bool Listening { get; set; }

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

	[Description("@#getRoutes")]
	public extern RouteRecordNormalized[] GetRoutes();

	[Description("@#clearRoutes")]
	public extern void ClearRoutes();

	[Description("@#resolve")]
	public extern RouteLocationResolved Resolve(RouteLocationRaw to);

	[Description("@#resolve")]
	public extern RouteLocationResolved Resolve(RouteLocationRaw to, RouteLocationNormalizedLoaded currentLocation);

	[Description("@#push")]
	public extern IPromise<RouteNavigationResult?> Push(RouteLocationRaw to);

	[Description("@#replace")]
	public extern IPromise<RouteNavigationResult?> Replace(RouteLocationRaw to);

	[Description("@#go")]
	public extern void Go(Number delta);

	[Description("@#back")]
	public extern void Back();

	[Description("@#forward")]
	public extern void Forward();

	[Description("@#beforeEach")]
	public extern Action BeforeEach(RouteNavigationGuard guard);

	[Description("@#beforeEach")]
	public extern Action BeforeEach(AsyncRouteNavigationGuard guard);

	[Description("@#beforeEach")]
	public extern Action BeforeEach(LegacyRouteNavigationGuard guard);

	[Description("@#beforeEach")]
	public extern Action BeforeEach(LegacyAsyncRouteNavigationGuard guard);

	[Description("@#beforeResolve")]
	public extern Action BeforeResolve(RouteNavigationGuard guard);

	[Description("@#beforeResolve")]
	public extern Action BeforeResolve(AsyncRouteNavigationGuard guard);

	[Description("@#beforeResolve")]
	public extern Action BeforeResolve(LegacyRouteNavigationGuard guard);

	[Description("@#beforeResolve")]
	public extern Action BeforeResolve(LegacyAsyncRouteNavigationGuard guard);

	[Description("@#afterEach")]
	public extern Action AfterEach(AfterNavigationHook hook);

	[Description("@#onError")]
	public extern Action OnError(ErrorRouterErrorHandler handler);

	[Description("@#onError")]
	public extern Action OnError(NavigationFailureRouterErrorHandler handler);

	[Description("@#onError")]
	public extern Action OnError(NavigationRedirectRouterErrorHandler handler);

	[Description("@#onError")]
	public extern Action OnError(StringRouterErrorHandler handler);

	[Description("@#onError")]
	public extern Action OnError(NumberRouterErrorHandler handler);

	[Description("@#onError")]
	public extern Action OnError(BooleanRouterErrorHandler handler);

	[Description("@#onError")]
	public extern Action OnError(BigIntRouterErrorHandler handler);

	[Description("@#onError")]
	public extern Action OnError(SymbolRouterErrorHandler handler);

	[Description("@#onError")]
	public extern Action OnError(ObjectRouterErrorHandler handler);

	[Description("@#onError")]
	public extern Action OnError(ArrayRouterErrorHandler handler);

	[Description("@#isReady")]
	public extern IPromise IsReady();
}

[ECMAScript]
[Description("@#")]
public abstract class RouterMatcher
{
	protected RouterMatcher()
	{
	}

	[Description("@#addRoute")]
	public extern Action AddRoute(RouteRecordRaw record);

	[Description("@#addRoute")]
	public extern Action AddRoute(RouteRecordRaw record, RouteRecordMatcher parent);

	[Description("@#removeRoute")]
	public extern void RemoveRoute(RouteRecordMatcher matcher);

	[Description("@#removeRoute")]
	public extern void RemoveRoute(RouteRecordName name);

	[Description("@#clearRoutes")]
	public extern void ClearRoutes();

	[Description("@#getRoutes")]
	public extern RouteRecordMatcher[] GetRoutes();

	[Description("@#getRecordMatcher")]
	public extern RouteRecordMatcher? GetRecordMatcher(RouteRecordName name);

	[Description("@#resolve")]
	public extern MatcherLocation Resolve(MatcherLocationRaw location, MatcherLocation currentLocation);
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
	public extern RouteLocationNormalized To { get; }

	[Description("@#from")]
	public extern RouteLocationNormalized From { get; }
}

[ECMAScript]
[Description("@#")]
public abstract class NavigationRedirectError : Error
{
	protected NavigationRedirectError()
	{
	}

	[Description("@#type")]
	public extern ErrorTypes Type { get; }

	[Description("@#to")]
	public extern RouteLocationRaw To { get; }

	[Description("@#from")]
	public extern RouteLocationNormalized From { get; }
}

[ECMAScript]
[Description("@#")]
public record RawRouteComponents : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern RawRouteComponent? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RawRouteComponent value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, ECMAScript.VueContract.IVueComponent value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteComponentLoader value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record RouteComponents : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern RouteComponent? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteComponent value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, ECMAScript.VueContract.IVueComponent value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteComponentLoader value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record NavigationGuardNextCallbackList : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern NavigationGuardNextCallback? this[Number index] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(NavigationGuardNextCallback value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record NavigationGuardNextCallbackMap : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern NavigationGuardNextCallbackList? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, NavigationGuardNextCallbackList value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record RouteComponentInstanceMap : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern Vue3.VueComponentPublicInstance? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, Vue3.VueComponentPublicInstance? value);

	extern System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#")]
public record RouteNamedProps : Vue3.VueProps, System.Collections.IEnumerable
{
	public extern RouteRecordProps? this[string key] { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteRecordProps value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, bool value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, Vue3.VueProps value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(string key, RouteRecordPropsResolver value);

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
	[Obsolete("Vue Router 4 documents end as deprecated and always true. Do not author new route records with End.")]
	public bool? End { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteRecordSingleView : RouteRecordBase
{
	[Description("@#component")]
	public RawRouteComponent Component { get; init; } = default!;

	[Description("@#props")]
	public RouteRecordProps? Props { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteRecordSingleViewWithChildren : RouteRecordBase
{
	[Description("@#component")]
	public RawRouteComponent? Component { get; init; }

	[Description("@#children")]
	public new RouteRecordRaw[] Children { get; init; } = default!;

	[Description("@#props")]
	public RouteRecordProps? Props { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteRecordMultipleViews : RouteRecordBase
{
	[Description("@#components")]
	public RawRouteComponents Components { get; init; } = default!;

	[Description("@#props")]
	public RouteRecordNamedViewProps? Props { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteRecordMultipleViewsWithChildren : RouteRecordBase
{
	[Description("@#components")]
	public RawRouteComponents? Components { get; init; }

	[Description("@#children")]
	public new RouteRecordRaw[] Children { get; init; } = default!;

	[Description("@#props")]
	public RouteRecordNamedViewProps? Props { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouteRecordRedirect : RouteRecordBase
{
	[Description("@#redirect")]
	public new RouteRedirectOption Redirect { get; init; } = default!;
}

[String]
public enum RouterLinkAriaCurrentValue
{
	[Description("@#page")]
	Page,

	[Description("@#step")]
	Step,

	[Description("@#location")]
	Location,

	[Description("@#date")]
	Date,

	[Description("@#time")]
	Time,

	[Description("@#true")]
	True,

	[Description("@#false")]
	False
}

[ECMAScript]
[Description("@#")]
public record RouterLinkOptions : Vue3.VueProps
{
	[Description("@#to")]
	public RouteLocationRaw To { get; init; } = default!;

	[Description("@#replace")]
	public bool? Replace { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouterLinkProps : RouterLinkOptions
{

	[Description("@#custom")]
	public bool? Custom { get; init; }

	[Description("@#activeClass")]
	public string? ActiveClass { get; init; }

	[Description("@#exactActiveClass")]
	public string? ExactActiveClass { get; init; }

	[Description("@#ariaCurrentValue")]
	public RouterLinkAriaCurrentValue? AriaCurrentValue { get; init; }

	[Description("@#viewTransition")]
	public bool? ViewTransition { get; init; }
}

[ECMAScript]
[Description("@#")]
public record UseLinkOptions : Vue3.VueProps
{
	/// <summary>
	/// Link target accepted by <c>useLink()</c>. Vue Router officially accepts both
	/// plain route-location values and reactive refs wrapping those values.
	/// </summary>
	[Description("@#to")]
	public RouteLocationRawMaybeRef To { get; init; } = default!;

	/// <summary>
	/// Whether <c>useLink()</c> should navigate via <c>router.replace()</c>. This
	/// option also supports reactive refs in the official Vue Router API.
	/// </summary>
	[Description("@#replace")]
	public RouteBooleanMaybeRef? Replace { get; init; }

	[Description("@#viewTransition")]
	public bool? ViewTransition { get; init; }
}

[ECMAScript]
[Description("@#")]
public abstract class UseLinkReturn
{
	protected UseLinkReturn()
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
	public extern IPromise<RouteNavigationResult?> Navigate();

	[Description("@#navigate")]
	public extern IPromise<RouteNavigationResult?> Navigate(MouseEvent @event);
}

[ECMAScript]
[Description("@#")]
public abstract class UseLinkResult : UseLinkReturn
{
	protected UseLinkResult()
	{
	}
}

[ECMAScript]
[Description("@#")]
public record ScrollPositionCoordinates : Vue3.VueProps
{
	[Description("@#left")]
	public double? Left { get; init; }

	[Description("@#top")]
	public double? Top { get; init; }

	[Description("@#behavior")]
	public ScrollBehavior? Behavior { get; init; }
}

[ECMAScript]
[Description("@#")]
public record ScrollPositionElement : ScrollPositionCoordinates
{
	[Description("@#el")]
	public ScrollPositionTarget El { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public record ScrollPositionNormalized : Vue3.VueProps
{
	[Description("@#left")]
	public double Left { get; init; }

	[Description("@#top")]
	public double Top { get; init; }

	[Description("@#behavior")]
	public ScrollBehavior? Behavior { get; init; }
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ScrollPositionTarget
{
	private readonly byte _kind;
	private readonly string? _selector;
	private readonly Element? _element;

	private ScrollPositionTarget(string value)
	{
		_kind = 1;
		_selector = value;
		_element = default;
	}

	private ScrollPositionTarget(Element value)
	{
		_kind = 2;
		_selector = default;
		_element = value;
	}

	public string? AsSelector => _kind == 1 ? _selector : default;

	public Element? AsElement => _kind == 2 ? _element : default;

	public static implicit operator ScrollPositionTarget(string value)
		=> new(value);

	public static implicit operator ScrollPositionTarget(Element value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouterScrollResult
{
	private readonly byte _kind;
	private readonly bool? _bool;
	private readonly ScrollPositionCoordinates? _coordinates;
	private readonly ScrollPositionElement? _element;
	private readonly ScrollPositionNormalized? _normalized;

	private RouterScrollResult(bool value)
	{
		_kind = 1;
		_bool = value;
		_coordinates = default;
		_element = default;
		_normalized = default;
	}

	private RouterScrollResult(ScrollPositionCoordinates value)
	{
		_kind = 2;
		_bool = default;
		_coordinates = value;
		_element = default;
		_normalized = default;
	}

	private RouterScrollResult(ScrollPositionElement value)
	{
		_kind = 3;
		_bool = default;
		_coordinates = default;
		_element = value;
		_normalized = default;
	}

	private RouterScrollResult(ScrollPositionNormalized value)
	{
		_kind = 4;
		_bool = default;
		_coordinates = default;
		_element = default;
		_normalized = value;
	}

	public bool? AsBool => _kind == 1 ? _bool : default;

	public ScrollPositionCoordinates? AsCoordinates => _kind == 2 ? _coordinates : default;

	public ScrollPositionElement? AsElement => _kind == 3 ? _element : default;

	public ScrollPositionNormalized? AsNormalized => _kind == 4 ? _normalized : default;

	public static implicit operator RouterScrollResult(bool value)
		=> new(value);

	public static implicit operator RouterScrollResult(ScrollPositionCoordinates value)
		=> new(value);

	public static implicit operator RouterScrollResult(ScrollPositionElement value)
		=> new(value);

	public static implicit operator RouterScrollResult(ScrollPositionNormalized value)
		=> new(value);
}

[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouterScrollHandler
{
	private readonly byte _kind;
	private readonly RouterScrollBehavior? _sync;
	private readonly AsyncRouterScrollBehavior? _async;

	private RouterScrollHandler(RouterScrollBehavior value)
	{
		_kind = 1;
		_sync = value;
		_async = default;
	}

	private RouterScrollHandler(AsyncRouterScrollBehavior value)
	{
		_kind = 2;
		_sync = default;
		_async = value;
	}

	public RouterScrollBehavior? AsSync => _kind == 1 ? _sync : default;

	public AsyncRouterScrollBehavior? AsAsync => _kind == 2 ? _async : default;

	public static implicit operator RouterScrollHandler(RouterScrollBehavior value)
		=> new(value);

	public static implicit operator RouterScrollHandler(AsyncRouterScrollBehavior value)
		=> new(value);

	[ECMAScriptInline("__arg1")]
	public extern static RouterScrollHandler From(RouterScrollBehavior value);

	[ECMAScriptInline("__arg1")]
	public extern static RouterScrollHandler From(AsyncRouterScrollBehavior value);
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
public record RouterLinkSlotScope : Vue3.VueProps
{
	[Description("@#route")]
	public RouteLocationResolved Route { get; init; } = default!;

	[Description("@#href")]
	public string Href { get; init; } = default!;

	[Description("@#isActive")]
	public bool IsActive { get; init; }

	[Description("@#isExactActive")]
	public bool IsExactActive { get; init; }

	[Description("@#navigate")]
	public RouterLinkNavigateCallback Navigate { get; init; } = default!;
}

[ECMAScript]
[Description("@#")]
public record RouterViewProps : Vue3.VueProps
{
	[Description("@#name")]
	public string? Name { get; init; }

	[Description("@#route")]
	public RouteLocationNormalized? Route { get; init; }
}

[ECMAScript]
[Description("@#")]
public record RouterViewSlotScope : Vue3.VueProps
{
	[Description("@#Component")]
	public Vue3.IVNode? Component { get; init; }

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
