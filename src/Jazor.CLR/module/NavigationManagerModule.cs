namespace Jazor.CLR;

/// <summary>
/// Browser implementation of Blazor's NavigationManager contract.
/// The generated CLR signatures stay unchanged; this module owns the runtime behavior.
/// </summary>
[ECMAScriptModule("Microsoft/AspNetCore/Components/NavigationManagerModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.NavigationManager", "Object")]
public static class NavigationManagerModule
{
	private static readonly WeakMap<object, Array<object>> LocationHandlers = new();
	private static readonly WeakMap<object, Array<object>> NotFoundHandlers = new();
	// location-changing handler 不是 CLR 事件：注册返回 IDisposable，注销只能通过该句柄，
	// 因此与事件家族分开保存，避免 add/remove 事件语义混入。
	private static readonly WeakMap<object, Array<object>> LocationChangingHandlers = new();
	// LocationChangingContext.CancellationToken 的宿主载体：每次内部导航的 dispatch 独占一个 controller，
	// 被后续导航取代时立即 abort，与 Blazor 的 _locationChangingCts 行为一致。
	private static readonly WeakMap<object, AbortController> LocationChangingCancellations = new();
	// Public NavigationManager overloads only receive the service instance. Keep the
	// Router invalidation callback with that instance so all entry points refresh alike.
	private static readonly WeakMap<object, Action?> RefreshHandlers = new();

	/// <summary>
	/// Creates the service object consumed by ordinary [Inject] NavigationManager properties.
	/// Router lifecycle remains in RazorVue, while all navigation semantics stay here.
	/// </summary>
	internal static object CreateNavigationManager(Action? refresh)
	{
		var instance = Object.Create(null);
		LocationHandlers.Set(instance, []);
		NotFoundHandlers.Set(instance, []);
		LocationChangingHandlers.Set(instance, []);
		RefreshHandlers.Set(instance, refresh);

		Object.DefineProperty(instance, "baseUri", new JazorPropertyDescriptor
		{
			Get = () => GetBaseUri()
		});
		Object.DefineProperty(instance, "uri", new JazorPropertyDescriptor
		{
			Get = GetUri
		});
		Object.DefineProperty(instance, "historyEntryState", new JazorPropertyDescriptor
		{
			Get = GetHistoryEntryState
		});
		Object.DefineProperty(instance, "version", new JazorPropertyDescriptor
		{
			Get = () => GetVersion(instance)
		});
		Object.DefineProperty(instance, "addLocationChanged", new JazorPropertyDescriptor
		{
			Value = (Action<object>)((value) => AddLocationChanged(instance, value))
		});
		Object.DefineProperty(instance, "removeLocationChanged", new JazorPropertyDescriptor
		{
			Value = (Action<object>)((value) => RemoveLocationChanged(instance, value))
		});
		Object.DefineProperty(instance, "addOnNotFound", new JazorPropertyDescriptor
		{
			Value = (Action<object>)((value) => AddNotFound(instance, value))
		});
		Object.DefineProperty(instance, "removeOnNotFound", new JazorPropertyDescriptor
		{
			Value = (Action<object>)((value) => RemoveNotFound(instance, value))
		});
		Object.DefineProperty(instance, "notFound", new JazorPropertyDescriptor
		{
			Value = (Action)(() => NotFound(instance))
		});
		Object.DefineProperty(instance, "registerLocationChangingHandler", new JazorPropertyDescriptor
		{
			Value = (Func<object, object>)((handler) => RegisterLocationChangingHandler(instance, handler))
		});
		Object.DefineProperty(instance, "notifyLocationChanged", new JazorPropertyDescriptor
		{
			Value = (Action<bool>)((intercepted) => NotifyLocationChanged(instance, intercepted))
		});
		Object.DefineProperty(instance, "navigateTo", new JazorPropertyDescriptor
		{
			Value = (Action<string, object?, object?>)((uri, optionsOrForceLoad, replace) =>
				NavigateTo(instance, uri, optionsOrForceLoad, replace))
		});
		Object.DefineProperty(instance, "toAbsoluteUri", new JazorPropertyDescriptor
		{
			Value = (Func<string?, URL>)((uri) => ToAbsoluteUri(instance, uri))
		});
		Object.DefineProperty(instance, "toBaseRelativePath", new JazorPropertyDescriptor
		{
			Value = (Func<string, string>)((uri) => ToBaseRelativePath(instance, uri))
		});
		Object.DefineProperty(instance, "getUriWithQueryParameter", new JazorPropertyDescriptor
		{
			Value = (Func<string, object?, string>)((name, value) =>
				NavigationManagerExtensionsModule.GetUriWithQueryParameterCore(GetUri(), name, value))
		});
		Object.DefineProperty(instance, "getUriWithQueryParameters", new JazorPropertyDescriptor
		{
			Value = (Func<object, string>)((parameters) =>
				NavigationManagerExtensionsModule.GetUriWithQueryParametersObjectCore(GetUri(), parameters))
		});
		Object.DefineProperty(instance, "getUriWithQueryParametersFromUri", new JazorPropertyDescriptor
		{
			Value = (Func<string, object, string>)((uri, parameters) =>
				NavigationManagerExtensionsModule.GetUriWithQueryParametersObjectCore(uri, parameters))
		});
		Object.DefineProperty(instance, "getUriWithFragment", new JazorPropertyDescriptor
		{
			Value = (Func<string?, string>)((fragment) =>
				NavigationManagerExtensionsModule.GetUriWithFragmentCore(GetUri(), fragment))
		});

		Object.DefineProperty(instance, "__jazorNavigationVersion", new JazorPropertyDescriptor
		{
			Value = 0d,
			Writable = true
		});
		return instance;
	}

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.LocationChanged.add", "addLocationChanged")]
	public static void _db3bb72f62f144de(object instance, object value)
		=> AddLocationChanged(instance, value);

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.LocationChanged.remove", "removeLocationChanged")]
	public static void _2b4e6a7b7c69da10(object instance, object value)
		=> RemoveLocationChanged(instance, value);

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.OnNotFound.add", "addOnNotFound")]
	public static void _918e5c63c185e5fc(object instance, object value)
		=> AddNotFound(instance, value);

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.OnNotFound.remove", "removeOnNotFound")]
	public static void _9c4f3f19f8f29f6c(object instance, object value)
		=> RemoveNotFound(instance, value);

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.BaseUri.get", "getBaseUri")]
	public static string _ab8ef4bd82ceca73(object instance) => GetBaseUri();

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.Uri.get", "getUri")]
	public static string _624f7ef8de91de98(object instance) => GetUri();

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.HistoryEntryState.get", "getHistoryEntryState")]
	public static string? _5f6ce77267562aa7(object instance) => GetHistoryEntryState();

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string, bool)", "navigateToForceLoad")]
	public static void _ddf7a5e6033b75d4(object instance, string uri, bool forceLoad)
		=> NavigateTo(instance, uri, forceLoad, false);

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string, bool, bool)", "navigateToForceLoadReplace")]
	public static void _88acd8ea0ddaabf5(object instance, string uri, bool forceLoad, bool replace)
		=> NavigateTo(instance, uri, forceLoad, replace);

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string, Microsoft.AspNetCore.Components.NavigationOptions)", "navigateToOptions")]
	public static void _8820227fd0b2bf2c(object instance, string uri, object options)
		=> NavigateTo(instance, uri, options, null);

	[Jazor(Op.Import, "virtual Microsoft.AspNetCore.Components.NavigationManager.Refresh(bool)", "refresh")]
	public static void _f9e277a2ad23f3f2(object instance, bool forceReload)
		=> Refresh(instance, forceReload);

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.NotFound()", "notFound")]
	public static void _5a573c4876e1e50b(object instance)
		=> NotFound(instance);

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.ToAbsoluteUri(string)", "toAbsoluteUri")]
	public static URL _d13389b43547427e(object instance, string? relativeUri)
		=> ToAbsoluteUri(instance, relativeUri);

	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.ToBaseRelativePath(string)", "toBaseRelativePath")]
	public static string _0da3e56124eaf41a(object instance, string uri)
		=> ToBaseRelativePath(instance, uri);

	/// <summary>
	/// C#: navigation.RegisterLocationChangingHandler(handler)
	/// JS: 登记 handler 并返回带 dispose 的注销句柄。
	/// </summary>
	/// <remarks>
	/// 返回值声明为 object：System.IDisposable 擦除为 Object，IDisposableModule.Dispose() 只探测实例上的
	/// dispose 方法，因此 { dispose } 字面量就是合法的 IDisposable 载体。
	/// </remarks>
	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.NavigationManager.RegisterLocationChangingHandler(System.Func<Microsoft.AspNetCore.Components.Routing.LocationChangingContext, System.Threading.Tasks.ValueTask>)", "registerLocationChangingHandler")]
	public static object _eaafc9868e2e1ebe(object instance, object locationChangingHandler)
		=> RegisterLocationChangingHandler(instance, locationChangingHandler);

	private static void AddLocationChanged(object instance, object value)
		=> AddHandler(LocationHandlers, instance, value);

	private static void RemoveLocationChanged(object instance, object value)
		=> RemoveHandler(LocationHandlers, instance, value);

	private static void AddNotFound(object instance, object value)
		=> AddHandler(NotFoundHandlers, instance, value);

	private static object RegisterLocationChangingHandler(object instance, object handler)
	{
		AddHandler(LocationChangingHandlers, instance, handler);
		return new
		{
			dispose = (Action)(() => RemoveHandler(LocationChangingHandlers, instance, handler))
		};
	}

	private static void RemoveNotFound(object instance, object value)
		=> RemoveHandler(NotFoundHandlers, instance, value);

	private static void AddHandler(WeakMap<object, Array<object>> registry, object instance, object value)
	{
		if (instance is null || value is null)
			return;

		var handlers = GetHandlers(registry, instance);
		// CLR events keep duplicate delegate subscriptions as distinct invocation-list entries.
		handlers.Splice(handlers.Length, 0, value);
	}

	private static void RemoveHandler(WeakMap<object, Array<object>> registry, object instance, object value)
	{
		if (instance is null || value is null || !registry.Has(instance))
			return;

		var handlers = registry.Get(instance)!;
		for (var index = handlers.Length - 1; index >= 0; index--)
		{
			if (Object.Is(handlers[index], value))
			{
				handlers.Splice(index, 1);
				return;
			}
		}
	}

	private static Array<object> GetHandlers(WeakMap<object, Array<object>> registry, object instance)
	{
		if (!registry.Has(instance))
			registry.Set(instance, []);
		return registry.Get(instance)!;
	}

	private static Number GetVersion(object instance)
		=> (Number?)ECMAScript.Reflect.Get(instance, "__jazorNavigationVersion") ?? 0;

	private static Action? GetRefreshHandler(object instance)
		=> RefreshHandlers.Has(instance) ? RefreshHandlers.Get(instance) : null;

	private static void NotifyLocationChanged(object instance, bool intercepted)
	{
		var version = GetVersion(instance) + 1;
		ECMAScript.Reflect.Set(instance, "__jazorNavigationVersion", version);
		GetRefreshHandler(instance)?.Invoke();

		var args = LocationChangedEventArgsModule._16454e1af5169b10(GetUri(), intercepted);
		// Capture the invocation list before dispatch. A handler can subscribe or remove
		// callbacks, but that must only affect a later LocationChanged notification.
		var handlers = GetHandlers(LocationHandlers, instance).Slice();
		for (var handlerIndex = 0; handlerIndex < handlers.Length; handlerIndex++)
		{
			var handler = handlers[handlerIndex];
			if (handler is null)
				continue;
			ECMAScript.Reflect.Apply(handler, null, [instance, args]);
		}
	}

	private static void NotFound(object instance)
	{
		// The CLR event field stays null until the first subscription, and NotFound() is a
		// no-op in that state. An empty invocation list keeps that observable behavior.
		var handlers = GetHandlers(NotFoundHandlers, instance).Slice();
		if (handlers.Length == 0)
			return;

		// NotFoundCore constructs a fresh NotFoundEventArgs per call, and its Path stays at
		// the CLR default because no router re-execution path exists in the browser.
		var args = NotFoundEventArgsModule._8ed2c94001d3c848();
		for (var handlerIndex = 0; handlerIndex < handlers.Length; handlerIndex++)
		{
			var handler = handlers[handlerIndex];
			if (handler is null)
				continue;
			ECMAScript.Reflect.Apply(handler, null, [instance, args]);
		}
	}

	private static void NavigateTo(
		object instance,
		string uri,
		object? optionsOrForceLoad,
		object? replaceArgument)
	{
		object? options = null;
		if (optionsOrForceLoad != null &&
			ECMAScript.Global.TypeOf(optionsOrForceLoad) == "object")
		{
			options = optionsOrForceLoad;
		}
		var forceLoad = options is not null
			? GetBool(options, "forceLoad")
			: IsBooleanTrue(optionsOrForceLoad);
		var replace = options is not null
			? GetBool(options, "replaceHistoryEntry")
			: IsBooleanTrue(replaceArgument);
		var historyState = options is null
			? null
			: ECMAScript.Reflect.Get(options, "historyEntryState");
		// A missing property is JavaScript undefined, while the CLR NavigationOptions
		// default is null. Keep an explicit string state unchanged before pushState.
		if (ECMAScript.Global.IsUndefined(historyState))
			historyState = null;

		var target = URL.Parse(uri ?? "", options is not null && GetBool(options, "relativeToCurrentUri")
			? GetUri()
			: GetBaseUri());
		if (target is null)
			throw new Error($"Navigation URI '{uri ?? ""}' is invalid.");

		if (forceLoad)
		{
			AssignWindowLocation(target.Href, replace);
			return;
		}

		var baseUri = URL.Parse(GetBaseUri(), GetUri());
		if (baseUri is null || !IsWithinBaseUriSpace(target, baseUri))
		{
			// Full navigation keeps the original author-supplied URI and its replace mode.
			AssignWindowLocation(uri ?? target.Href, replace);
			return;
		}

		// location-changing handler 只覆盖内部导航：forceLoad 与跨 base URI 的整页导航由浏览器
		// location 直接接管，Blazor 同样不在这两条路径上运行 handler。
		var handlers = GetHandlers(LocationChangingHandlers, instance).Slice();
		if (handlers.Length == 0)
		{
			CommitInternalNavigation(instance, target, replace, historyState);
			return;
		}

		var cancellation = BeginLocationChangingCancellation(instance);
		var context = LocationChangingContextModule.CreateLocationChangingContext(
			target.Href,
			historyState as string,
			false,
			cancellation.Signal);
		DispatchLocationChanging(handlers, context, () =>
		{
			EndLocationChangingCancellation(instance, cancellation);
			// 本次 dispatch 已被后续导航取代：token 已取消，提交它会把过期的目标写回 history。
			if (cancellation.Signal.Aborted)
				return;

			if (!LocationChangingContextModule.IsNavigationPrevented(context))
				CommitInternalNavigation(instance, target, replace, historyState);
		});
	}

	private static AbortController BeginLocationChangingCancellation(object instance)
	{
		if (LocationChangingCancellations.Has(instance))
			LocationChangingCancellations.Get(instance)!.Abort();

		var cancellation = new AbortController();
		LocationChangingCancellations.Set(instance, cancellation);
		return cancellation;
	}

	// 只有仍属于本次 dispatch 时才清除：被取代之后 WeakMap 里已经换成新的 controller。
	private static void EndLocationChangingCancellation(object instance, AbortController cancellation)
	{
		if (LocationChangingCancellations.Get(instance) == cancellation)
			LocationChangingCancellations.Delete(instance);
	}

	private static void CommitInternalNavigation(
		object instance,
		URL target,
		bool replace,
		object? historyState)
	{
		var route = target.Pathname + target.Search + target.Hash;
		var history = GetWindowHistory();
		if (replace)
			history.ReplaceState(historyState!, "", route);
		else
			history.PushState(historyState!, "", route);
		NotifyLocationChanged(instance, true);
	}

	// handler 全部先同步启动（副作用顺序与 CLR 一致），其异步部分全部结算之后才读取
	// PreventNavigation() 标记；NavigateTo 保持 void，与真实 Blazor 的 fire-and-forget 一致。
	private static void DispatchLocationChanging(Array<object> handlers, object context, Action commit)
	{
		IPromise settled = Promise.Resolve();
		for (var handlerIndex = 0; handlerIndex < handlers.Length; handlerIndex++)
		{
			var handler = handlers[handlerIndex];
			if (handler is null)
				continue;

			// handler 返回 ValueTask（擦除为 Promise）；同步完成的 handler 返回 undefined。
			IPromise awaited = Promise.Resolve(ECMAScript.Reflect.Apply(handler, null, [context]));
			settled = settled.Then(() => awaited);
		}

		settled.Then(commit);
	}

	private static void Refresh(object instance, bool forceReload)
	{
		if (forceReload)
		{
			var location = GetWindowLocation();
			var reload = ECMAScript.Reflect.Get(location, "reload")
				?? throw new Error("The browser window location cannot reload.");
			ECMAScript.Reflect.Apply(reload, location, []);
			return;
		}

		// RazorVue has no enhanced-navigation transport. A replacement full navigation is
		// the supported browser equivalent and retains the current history position.
		NavigateTo(instance, GetUri(), true, true);
	}

	private static bool GetBool(object instance, string name)
		=> IsBooleanTrue(ECMAScript.Reflect.Get(instance, name));

	private static bool IsBooleanTrue(object? value)
		=> ECMAScript.Global.TypeOf(value) == "boolean" && (bool)value!;

	private static string GetUri()
		=> ECMAScript.Reflect.Get(GetWindowLocation(), "href") as string ?? "";

	private static string? GetHistoryEntryState()
	{
		var state = ECMAScript.Reflect.Get(GetWindowHistory(), "state");
		return ECMAScript.Global.TypeOf(state) == "string" ? (string)state! : null;
	}

	private static string GetBaseUri()
	{
		var current = GetUri();
		var declared = ECMAScript.Global.Document.QuerySelector("base[href]")?.GetAttribute("href") ?? "/";
		var baseUri = URL.Parse(declared, current);
		if (baseUri is null)
			return "/";

		var pathname = baseUri.Pathname;
		if (!pathname.EndsWith("/", StringComparison.Ordinal))
			baseUri.Pathname = pathname + "/";
		baseUri.Search = "";
		baseUri.Hash = "";
		return baseUri.Href;
	}

	// System.Uri lowers to the browser URL constructor, so the resolved URL object is the
	// runtime value and stays usable through the System.Uri members mapped in UriModule.
	private static URL ToAbsoluteUri(object instance, string? relativeUri)
	{
		var resolved = URL.Parse(relativeUri ?? "", GetBaseUri());
		if (resolved is null)
			throw new Error($"Navigation URI '{relativeUri ?? ""}' is invalid.");
		return resolved;
	}

	private static string ToBaseRelativePath(object instance, string uri)
	{
		var baseUri = GetBaseUri();
		if (uri.StartsWith(baseUri, StringComparison.Ordinal))
			return uri[baseUri.Length..];

		var suffixIndex = IndexOfPathSuffix(uri);
		var pathOnly = suffixIndex < 0 ? uri : uri[..suffixIndex];
		if (baseUri.EndsWith("/", StringComparison.Ordinal) &&
			pathOnly == baseUri[..^1])
		{
			return uri[(baseUri.Length - 1)..];
		}

		throw new Error($"ArgumentException: The URI '{uri}' is not contained by the base URI '{baseUri}'.");
	}

	private static bool IsWithinBaseUriSpace(URL target, URL baseUri)
	{
		if (target.Origin != baseUri.Origin)
			return false;

		var basePath = baseUri.Pathname.EndsWith("/", StringComparison.Ordinal)
			? baseUri.Pathname
			: baseUri.Pathname + "/";
		return target.Pathname.StartsWith(basePath, StringComparison.Ordinal) ||
			target.Pathname == basePath[..^1];
	}

	private static int IndexOfPathSuffix(string uri)
	{
		var queryIndex = uri.IndexOf("?", StringComparison.Ordinal);
		var hashIndex = uri.IndexOf("#", StringComparison.Ordinal);
		if (queryIndex < 0)
			return hashIndex;
		if (hashIndex < 0)
			return queryIndex;
		return queryIndex < hashIndex ? queryIndex : hashIndex;
	}

	private static object GetWindowLocation()
		=> Window.Location
			?? throw new Error("The browser window does not expose a location object.");

	private static JazorHistory GetWindowHistory()
		=> Window.History
			?? throw new Error("The browser window does not expose a history object.");

	private static void AssignWindowLocation(string href, bool replace)
	{
		var location = GetWindowLocation();
		var method = ECMAScript.Reflect.Get(location, replace ? "replace" : "assign")
			?? throw new Error("The browser window location cannot navigate.");
		ECMAScript.Reflect.Apply(method, location, [href]);
	}
}
