namespace ECMAScript;

/// <summary>
/// RegistrationOptions
/// </summary>
[ECMAScript]
[Description("@#RegistrationOptions")]
public record RegistrationOptions(
    [property: Description("@#scope")]string? Scope = default,
	[property: Description("@#type")]WorkerType Type = WorkerType.Classic,
	[property: Description("@#updateViaCache")]ServiceWorkerUpdateViaCache UpdateViaCache = ServiceWorkerUpdateViaCache.Imports);

/// <summary>
/// NavigationPreloadState
/// </summary>
[ECMAScript]
[Description("@#NavigationPreloadState")]
public record NavigationPreloadState(
    [property: Description("@#enabled")]bool Enabled = false,
	[property: Description("@#headerValue")]byte[]? HeaderValue = default);

/// <summary>
/// ClientQueryOptions
/// </summary>
[ECMAScript]
[Description("@#ClientQueryOptions")]
public record ClientQueryOptions(
    [property: Description("@#includeUncontrolled")]bool IncludeUncontrolled = false,
	[property: Description("@#type")]ClientType Type = ClientType.Window);

/// <summary>
/// ExtendableEventInit
/// </summary>
[ECMAScript]
[Description("@#ExtendableEventInit")]
public abstract record ExtendableEventInit();

/// <summary>
/// RouterRule
/// </summary>
[ECMAScript]
[Description("@#RouterRule")]
public record RouterRule(
    [property: Description("@#condition")]RouterCondition? Condition = default,
	[property: Description("@#source")]RouterSource? Source = default);

/// <summary>
/// RouterCondition
/// </summary>
[ECMAScript]
[Description("@#RouterCondition")]
public record RouterCondition(
    [property: Description("@#urlPattern")]URLPatternCompatible? UrlPattern = default,
	[property: Description("@#requestMethod")]byte[]? RequestMethod = default,
	[property: Description("@#requestMode")]RequestMode? RequestMode = default,
	[property: Description("@#requestDestination")]RequestDestination? RequestDestination = default,
	[property: Description("@#runningStatus")]RunningStatus? RunningStatus = default,
	[property: Description("@#or")]RouterCondition[]? Or = default,
	[property: Description("@#not")]RouterCondition? Not = default);

/// <summary>
/// RouterSourceDict
/// </summary>
[ECMAScript]
[Description("@#RouterSourceDict")]
public record RouterSourceDict(
    [property: Description("@#cacheName")]string? CacheName = default);

/// <summary>
/// FetchEventInit
/// </summary>
[ECMAScript]
[Description("@#FetchEventInit")]
public record FetchEventInit(
    [property: Description("@#request")]Request? Request = default,
	[property: Description("@#preloadResponse")]PromiseResult<object>? PreloadResponse = default,
	[property: Description("@#clientId")]string? ClientId = default,
	[property: Description("@#resultingClientId")]string? ResultingClientId = default,
	[property: Description("@#replacesClientId")]string? ReplacesClientId = default,
	[property: Description("@#handled")]PromiseResult<object>? Handled = default): ExtendableEventInit;

/// <summary>
/// ExtendableMessageEventInit
/// </summary>
[ECMAScript]
[Description("@#ExtendableMessageEventInit")]
public record ExtendableMessageEventInit(
    [property: Description("@#data")]object? Data = default,
	[property: Description("@#origin")]string? Origin = default,
	[property: Description("@#lastEventId")]string? LastEventId = default,
	[property: Description("@#source")]Either<Client, ServiceWorker, MessagePort>? Source = default,
	[property: Description("@#ports")]MessagePort[]? Ports = default): ExtendableEventInit;

/// <summary>
/// CacheQueryOptions
/// </summary>
[ECMAScript]
[Description("@#CacheQueryOptions")]
public record CacheQueryOptions(
    [property: Description("@#ignoreSearch")]bool IgnoreSearch = false,
	[property: Description("@#ignoreMethod")]bool IgnoreMethod = false,
	[property: Description("@#ignoreVary")]bool IgnoreVary = false);

/// <summary>
/// MultiCacheQueryOptions
/// </summary>
[ECMAScript]
[Description("@#MultiCacheQueryOptions")]
public record MultiCacheQueryOptions(
    [property: Description("@#cacheName")]string? CacheName = default): CacheQueryOptions;

/// <summary>
/// ServiceWorker
/// </summary>
[ECMAScript]
[Description("@#ServiceWorker")]
public class ServiceWorker : EventTarget
{
    /// <summary>
    /// scriptURL
    /// </summary>
    [Description("@#scriptURL")]
    public extern string ScriptURL { get; }

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern ServiceWorkerState State { get; }

    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="transfer">transfer</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, object[] transfer);

    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="options">options</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, StructuredSerializeOptions? options = default);

    /// <summary>
    /// onstatechange
    /// </summary>
    [Description("@#onstatechange")]
    public extern EventHandler Onstatechange { get; set; }

    #region mixin AbstractWorker
    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }
    #endregion
}

/// <summary>
/// ServiceWorkerContainer
/// </summary>
[ECMAScript]
[Description("@#ServiceWorkerContainer")]
public class ServiceWorkerContainer : EventTarget
{
    /// <summary>
    /// controller
    /// </summary>
    [Description("@#controller")]
    public extern ServiceWorker? Controller { get; }

    /// <summary>
    /// ready
    /// </summary>
    [Description("@#ready")]
    public extern PromiseResult<ServiceWorkerRegistration> Ready { get; }

    /// <summary>
    /// register
    /// </summary>
    /// <param name="scriptUrl">scriptURL</param>
    /// <param name="options">options</param>
    [Description("@#register")]
    public extern PromiseResult<ServiceWorkerRegistration> Register(Either<TrustedScriptURL, string> scriptUrl, RegistrationOptions? options = default);

    /// <summary>
    /// getRegistration
    /// </summary>
    /// <param name="clientUrl">clientURL</param>
    [Description("@#getRegistration")]
    public extern PromiseResult<Either<ServiceWorkerRegistration, object>> GetRegistration(string? clientUrl = default);

    /// <summary>
    /// getRegistrations
    /// </summary>
    [Description("@#getRegistrations")]
    public extern PromiseResult<FrozenSet<ServiceWorkerRegistration>> GetRegistrations();

    /// <summary>
    /// startMessages
    /// </summary>
    [Description("@#startMessages")]
    public extern void StartMessages();

    /// <summary>
    /// oncontrollerchange
    /// </summary>
    [Description("@#oncontrollerchange")]
    public extern EventHandler Oncontrollerchange { get; set; }

    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// onmessageerror
    /// </summary>
    [Description("@#onmessageerror")]
    public extern EventHandler Onmessageerror { get; set; }
}

/// <summary>
/// NavigationPreloadManager
/// </summary>
[ECMAScript]
[Description("@#NavigationPreloadManager")]
public class NavigationPreloadManager
{
    /// <summary>
    /// enable
    /// </summary>
    [Description("@#enable")]
    public extern PromiseResult<object> Enable();

    /// <summary>
    /// disable
    /// </summary>
    [Description("@#disable")]
    public extern PromiseResult<object> Disable();

    /// <summary>
    /// setHeaderValue
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#setHeaderValue")]
    public extern PromiseResult<object> SetHeaderValue(byte[] value);

    /// <summary>
    /// getState
    /// </summary>
    [Description("@#getState")]
    public extern PromiseResult<NavigationPreloadState> GetState();
}

/// <summary>
/// WindowClient
/// </summary>
[ECMAScript]
[Description("@#WindowClient")]
public class WindowClient : Client
{
    /// <summary>
    /// visibilityState
    /// </summary>
    [Description("@#visibilityState")]
    public extern DocumentVisibilityState VisibilityState { get; }

    /// <summary>
    /// focused
    /// </summary>
    [Description("@#focused")]
    public extern bool Focused { get; }

    /// <summary>
    /// ancestorOrigins
    /// </summary>
    [Description("@#ancestorOrigins")]
    public extern FrozenSet<string> AncestorOrigins { get; }

    /// <summary>
    /// focus
    /// </summary>
    [Description("@#focus")]
    public extern PromiseResult<WindowClient> Focus();

    /// <summary>
    /// navigate
    /// </summary>
    /// <param name="url">url</param>
    [Description("@#navigate")]
    public extern PromiseResult<WindowClient?> Navigate(string url);
}

/// <summary>
/// Clients
/// </summary>
[ECMAScript]
[Description("@#Clients")]
public class Clients
{
    /// <summary>
    /// get
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#get")]
    public extern PromiseResult<Either<Client, object>> Get(string id);

    /// <summary>
    /// matchAll
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#matchAll")]
    public extern PromiseResult<FrozenSet<Client>> MatchAll(ClientQueryOptions? options = default);

    /// <summary>
    /// openWindow
    /// </summary>
    /// <param name="url">url</param>
    [Description("@#openWindow")]
    public extern PromiseResult<WindowClient?> OpenWindow(string url);

    /// <summary>
    /// claim
    /// </summary>
    [Description("@#claim")]
    public extern PromiseResult<object> Claim();
}

/// <summary>
/// ExtendableEvent
/// </summary>
[ECMAScript]
[Description("@#ExtendableEvent")]
public class ExtendableEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern ExtendableEvent(string type, ExtendableEventInit eventInitDict);

    /// <summary>
    /// waitUntil
    /// </summary>
    /// <param name="f">f</param>
    [Description("@#waitUntil")]
    public extern void WaitUntil(PromiseResult<object> f);
}

/// <summary>
/// InstallEvent
/// </summary>
[ECMAScript]
[Description("@#InstallEvent")]
public class InstallEvent : ExtendableEvent
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern InstallEvent(string type, ExtendableEventInit eventInitDict);

    /// <summary>
    /// addRoutes
    /// </summary>
    /// <param name="rules">rules</param>
    [Description("@#addRoutes")]
    public extern PromiseResult<object> AddRoutes(Either<RouterRule, RouterRule[]> rules);

    /// <summary>
    /// addRoutes
    /// </summary>
    /// <param name="rules">rules</param>
    [Description("@#addRoutes")]
    public extern PromiseResult<object> AddRoutes(RouterRule? rules = default);

    /// <summary>
    /// addRoutes
    /// </summary>
    /// <param name="rules">rules</param>
    [Description("@#addRoutes")]
    public extern PromiseResult<object> AddRoutes(RouterRule[] rules);
}

/// <summary>
/// FetchEvent
/// </summary>
[ECMAScript]
[Description("@#FetchEvent")]
public class FetchEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern FetchEvent(string type, FetchEventInit eventInitDict);

    /// <summary>
    /// request
    /// </summary>
    [Description("@#request")]
    public extern Request Request { get; }

    /// <summary>
    /// preloadResponse
    /// </summary>
    [Description("@#preloadResponse")]
    public extern PromiseResult<object> PreloadResponse { get; }

    /// <summary>
    /// clientId
    /// </summary>
    [Description("@#clientId")]
    public extern string ClientId { get; }

    /// <summary>
    /// resultingClientId
    /// </summary>
    [Description("@#resultingClientId")]
    public extern string ResultingClientId { get; }

    /// <summary>
    /// replacesClientId
    /// </summary>
    [Description("@#replacesClientId")]
    public extern string ReplacesClientId { get; }

    /// <summary>
    /// handled
    /// </summary>
    [Description("@#handled")]
    public extern PromiseResult<object> Handled { get; }

    /// <summary>
    /// respondWith
    /// </summary>
    /// <param name="r">r</param>
    [Description("@#respondWith")]
    public extern void RespondWith(PromiseResult<Response> r);
}

/// <summary>
/// ExtendableMessageEvent
/// </summary>
[ECMAScript]
[Description("@#ExtendableMessageEvent")]
public class ExtendableMessageEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern ExtendableMessageEvent(string type, ExtendableMessageEventInit eventInitDict);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }

    /// <summary>
    /// origin
    /// </summary>
    [Description("@#origin")]
    public extern string Origin { get; }

    /// <summary>
    /// lastEventId
    /// </summary>
    [Description("@#lastEventId")]
    public extern string LastEventId { get; }

    /// <summary>
    /// source
    /// </summary>
    [Description("@#source")]
    public extern Either<Client, ServiceWorker, MessagePort>? Source { get; }

    /// <summary>
    /// ports
    /// </summary>
    [Description("@#ports")]
    public extern FrozenSet<MessagePort> Ports { get; }
}

/// <summary>
/// Cache
/// </summary>
[ECMAScript]
[Description("@#Cache")]
public class Cache
{
    /// <summary>
    /// match
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="options">options</param>
    [Description("@#match")]
    public extern PromiseResult<Either<Response, object>> Match(RequestInfo request, CacheQueryOptions? options = default);

    /// <summary>
    /// matchAll
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="options">options</param>
    [Description("@#matchAll")]
    public extern PromiseResult<FrozenSet<Response>> MatchAll(RequestInfo request, CacheQueryOptions? options = default);

    /// <summary>
    /// add
    /// </summary>
    /// <param name="request">request</param>
    [Description("@#add")]
    public extern PromiseResult<object> Add(RequestInfo request);

    /// <summary>
    /// addAll
    /// </summary>
    /// <param name="requests">requests</param>
    [Description("@#addAll")]
    public extern PromiseResult<object> AddAll(RequestInfo[] requests);

    /// <summary>
    /// put
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="response">response</param>
    [Description("@#put")]
    public extern PromiseResult<object> Put(RequestInfo request, Response response);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="options">options</param>
    [Description("@#delete")]
    public extern PromiseResult<bool> Delete(RequestInfo request, CacheQueryOptions? options = default);

    /// <summary>
    /// keys
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="options">options</param>
    [Description("@#keys")]
    public extern PromiseResult<FrozenSet<Request>> Keys(RequestInfo request, CacheQueryOptions? options = default);
}

/// <summary>
/// CacheStorage
/// </summary>
[ECMAScript]
[Description("@#CacheStorage")]
public class CacheStorage
{
    /// <summary>
    /// match
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="options">options</param>
    [Description("@#match")]
    public extern PromiseResult<Either<Response, object>> Match(RequestInfo request, MultiCacheQueryOptions? options = default);

    /// <summary>
    /// has
    /// </summary>
    /// <param name="cacheName">cacheName</param>
    [Description("@#has")]
    public extern PromiseResult<bool> Has(string cacheName);

    /// <summary>
    /// open
    /// </summary>
    /// <param name="cacheName">cacheName</param>
    [Description("@#open")]
    public extern PromiseResult<Cache> Open(string cacheName);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="cacheName">cacheName</param>
    [Description("@#delete")]
    public extern PromiseResult<bool> Delete(string cacheName);

    /// <summary>
    /// keys
    /// </summary>
    [Description("@#keys")]
    public extern PromiseResult<string[]> Keys();
}