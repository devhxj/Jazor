namespace ECMAScript;

/// <summary>
/// BackgroundFetchUIOptions
/// </summary>
[ECMAScript]
[Description("@#BackgroundFetchUIOptions")]
public record BackgroundFetchUIOptions(
    [property: Description("@#icons")]ImageResource[]? Icons = default,
	[property: Description("@#title")]string? Title = default);

/// <summary>
/// BackgroundFetchOptions
/// </summary>
[ECMAScript]
[Description("@#BackgroundFetchOptions")]
public record BackgroundFetchOptions(
    [property: Description("@#downloadTotal")]ulong DownloadTotal = 0): BackgroundFetchUIOptions;

/// <summary>
/// BackgroundFetchEventInit
/// </summary>
[ECMAScript]
[Description("@#BackgroundFetchEventInit")]
public record BackgroundFetchEventInit(
    [property: Description("@#registration")]BackgroundFetchRegistration? Registration = default): ExtendableEventInit;

/// <summary>
/// ServiceWorkerGlobalScope
/// </summary>
[ECMAScript]
[Description("@#ServiceWorkerGlobalScope")]
public partial class ServiceWorkerGlobalScope : WorkerGlobalScope
{
    /// <summary>
    /// onbackgroundfetchsuccess
    /// </summary>
    [Description("@#onbackgroundfetchsuccess")]
    public extern EventHandler Onbackgroundfetchsuccess { get; set; }

    /// <summary>
    /// onbackgroundfetchfail
    /// </summary>
    [Description("@#onbackgroundfetchfail")]
    public extern EventHandler Onbackgroundfetchfail { get; set; }

    /// <summary>
    /// onbackgroundfetchabort
    /// </summary>
    [Description("@#onbackgroundfetchabort")]
    public extern EventHandler Onbackgroundfetchabort { get; set; }

    /// <summary>
    /// onbackgroundfetchclick
    /// </summary>
    [Description("@#onbackgroundfetchclick")]
    public extern EventHandler Onbackgroundfetchclick { get; set; }

    /// <summary>
    /// onsync
    /// </summary>
    [Description("@#onsync")]
    public extern EventHandler Onsync { get; set; }

    /// <summary>
    /// oncontentdelete
    /// </summary>
    [Description("@#oncontentdelete")]
    public extern EventHandler Oncontentdelete { get; set; }

    /// <summary>
    /// cookieStore
    /// </summary>
    [Description("@#cookieStore")]
    public extern CookieStore CookieStore { get; }

    /// <summary>
    /// oncookiechange
    /// </summary>
    [Description("@#oncookiechange")]
    public extern EventHandler Oncookiechange { get; set; }

    /// <summary>
    /// onnotificationclick
    /// </summary>
    [Description("@#onnotificationclick")]
    public extern EventHandler Onnotificationclick { get; set; }

    /// <summary>
    /// onnotificationclose
    /// </summary>
    [Description("@#onnotificationclose")]
    public extern EventHandler Onnotificationclose { get; set; }

    /// <summary>
    /// oncanmakepayment
    /// </summary>
    [Description("@#oncanmakepayment")]
    public extern EventHandler Oncanmakepayment { get; set; }

    /// <summary>
    /// onpaymentrequest
    /// </summary>
    [Description("@#onpaymentrequest")]
    public extern EventHandler Onpaymentrequest { get; set; }

    /// <summary>
    /// onperiodicsync
    /// </summary>
    [Description("@#onperiodicsync")]
    public extern EventHandler Onperiodicsync { get; set; }

    /// <summary>
    /// onpush
    /// </summary>
    [Description("@#onpush")]
    public extern EventHandler Onpush { get; set; }

    /// <summary>
    /// onpushsubscriptionchange
    /// </summary>
    [Description("@#onpushsubscriptionchange")]
    public extern EventHandler Onpushsubscriptionchange { get; set; }

    /// <summary>
    /// clients
    /// </summary>
    [Description("@#clients")]
    public extern Clients Clients { get; }

    /// <summary>
    /// registration
    /// </summary>
    [Description("@#registration")]
    public extern ServiceWorkerRegistration Registration { get; }

    /// <summary>
    /// serviceWorker
    /// </summary>
    [Description("@#serviceWorker")]
    public extern ServiceWorker ServiceWorker { get; }

    /// <summary>
    /// skipWaiting
    /// </summary>
    [Description("@#skipWaiting")]
    public extern PromiseResult<object> SkipWaiting();

    /// <summary>
    /// oninstall
    /// </summary>
    [Description("@#oninstall")]
    public extern EventHandler Oninstall { get; set; }

    /// <summary>
    /// onactivate
    /// </summary>
    [Description("@#onactivate")]
    public extern EventHandler Onactivate { get; set; }

    /// <summary>
    /// onfetch
    /// </summary>
    [Description("@#onfetch")]
    public extern EventHandler Onfetch { get; set; }

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
/// ServiceWorkerRegistration
/// </summary>
[ECMAScript]
[Description("@#ServiceWorkerRegistration")]
public partial class ServiceWorkerRegistration : EventTarget
{
    /// <summary>
    /// backgroundFetch
    /// </summary>
    [Description("@#backgroundFetch")]
    public extern BackgroundFetchManager BackgroundFetch { get; }

    /// <summary>
    /// sync
    /// </summary>
    [Description("@#sync")]
    public extern SyncManager Sync { get; }

    /// <summary>
    /// index
    /// </summary>
    [Description("@#index")]
    public extern ContentIndex Index { get; }

    /// <summary>
    /// cookies
    /// </summary>
    [Description("@#cookies")]
    public extern CookieStoreManager Cookies { get; }

    /// <summary>
    /// showNotification
    /// </summary>
    /// <param name="title">title</param>
    /// <param name="options">options</param>
    [Description("@#showNotification")]
    public extern PromiseResult<object> ShowNotification(string title, NotificationOptions? options = default);

    /// <summary>
    /// getNotifications
    /// </summary>
    /// <param name="filter">filter</param>
    [Description("@#getNotifications")]
    public extern PromiseResult<Notification[]> GetNotifications(GetNotificationOptions? filter = default);

    /// <summary>
    /// paymentManager
    /// </summary>
    [Description("@#paymentManager")]
    public extern PaymentManager PaymentManager { get; }

    /// <summary>
    /// periodicSync
    /// </summary>
    [Description("@#periodicSync")]
    public extern PeriodicSyncManager PeriodicSync { get; }

    /// <summary>
    /// pushManager
    /// </summary>
    [Description("@#pushManager")]
    public extern PushManager PushManager { get; }

    /// <summary>
    /// installing
    /// </summary>
    [Description("@#installing")]
    public extern ServiceWorker? Installing { get; }

    /// <summary>
    /// waiting
    /// </summary>
    [Description("@#waiting")]
    public extern ServiceWorker? Waiting { get; }

    /// <summary>
    /// active
    /// </summary>
    [Description("@#active")]
    public extern ServiceWorker? Active { get; }

    /// <summary>
    /// navigationPreload
    /// </summary>
    [Description("@#navigationPreload")]
    public extern NavigationPreloadManager NavigationPreload { get; }

    /// <summary>
    /// scope
    /// </summary>
    [Description("@#scope")]
    public extern string Scope { get; }

    /// <summary>
    /// updateViaCache
    /// </summary>
    [Description("@#updateViaCache")]
    public extern ServiceWorkerUpdateViaCache UpdateViaCache { get; }

    /// <summary>
    /// update
    /// </summary>
    [Description("@#update")]
    public extern PromiseResult<ServiceWorkerRegistration> Update();

    /// <summary>
    /// unregister
    /// </summary>
    [Description("@#unregister")]
    public extern PromiseResult<bool> Unregister();

    /// <summary>
    /// onupdatefound
    /// </summary>
    [Description("@#onupdatefound")]
    public extern EventHandler Onupdatefound { get; set; }
}

/// <summary>
/// BackgroundFetchManager
/// </summary>
[ECMAScript]
[Description("@#BackgroundFetchManager")]
public class BackgroundFetchManager
{
    /// <summary>
    /// fetch
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="requests">requests</param>
    /// <param name="options">options</param>
    [Description("@#fetch")]
    public extern PromiseResult<BackgroundFetchRegistration> Fetch(string id, Either<RequestInfo, RequestInfo[]> requests, BackgroundFetchOptions? options = default);

    /// <summary>
    /// get
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#get")]
    public extern PromiseResult<BackgroundFetchRegistration?> Get(string id);

    /// <summary>
    /// getIds
    /// </summary>
    [Description("@#getIds")]
    public extern PromiseResult<string[]> GetIds();
}

/// <summary>
/// BackgroundFetchRegistration
/// </summary>
[ECMAScript]
[Description("@#BackgroundFetchRegistration")]
public class BackgroundFetchRegistration : EventTarget
{
    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// uploadTotal
    /// </summary>
    [Description("@#uploadTotal")]
    public extern ulong UploadTotal { get; }

    /// <summary>
    /// uploaded
    /// </summary>
    [Description("@#uploaded")]
    public extern ulong Uploaded { get; }

    /// <summary>
    /// downloadTotal
    /// </summary>
    [Description("@#downloadTotal")]
    public extern ulong DownloadTotal { get; }

    /// <summary>
    /// downloaded
    /// </summary>
    [Description("@#downloaded")]
    public extern ulong Downloaded { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern BackgroundFetchResult Result { get; }

    /// <summary>
    /// failureReason
    /// </summary>
    [Description("@#failureReason")]
    public extern BackgroundFetchFailureReason FailureReason { get; }

    /// <summary>
    /// recordsAvailable
    /// </summary>
    [Description("@#recordsAvailable")]
    public extern bool RecordsAvailable { get; }

    /// <summary>
    /// onprogress
    /// </summary>
    [Description("@#onprogress")]
    public extern EventHandler Onprogress { get; set; }

    /// <summary>
    /// abort
    /// </summary>
    [Description("@#abort")]
    public extern PromiseResult<bool> Abort();

    /// <summary>
    /// match
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="options">options</param>
    [Description("@#match")]
    public extern PromiseResult<BackgroundFetchRecord> Match(RequestInfo request, CacheQueryOptions? options = default);

    /// <summary>
    /// matchAll
    /// </summary>
    /// <param name="request">request</param>
    /// <param name="options">options</param>
    [Description("@#matchAll")]
    public extern PromiseResult<BackgroundFetchRecord[]> MatchAll(RequestInfo request, CacheQueryOptions? options = default);
}

/// <summary>
/// BackgroundFetchRecord
/// </summary>
[ECMAScript]
[Description("@#BackgroundFetchRecord")]
public class BackgroundFetchRecord
{
    /// <summary>
    /// request
    /// </summary>
    [Description("@#request")]
    public extern Request Request { get; }

    /// <summary>
    /// responseReady
    /// </summary>
    [Description("@#responseReady")]
    public extern PromiseResult<Response> ResponseReady { get; }
}

/// <summary>
/// BackgroundFetchEvent
/// </summary>
[ECMAScript]
[Description("@#BackgroundFetchEvent")]
public class BackgroundFetchEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="init">init</param>
    public extern BackgroundFetchEvent(string type, BackgroundFetchEventInit init);

    /// <summary>
    /// registration
    /// </summary>
    [Description("@#registration")]
    public extern BackgroundFetchRegistration Registration { get; }
}

/// <summary>
/// BackgroundFetchUpdateUIEvent
/// </summary>
[ECMAScript]
[Description("@#BackgroundFetchUpdateUIEvent")]
public class BackgroundFetchUpdateUIEvent : BackgroundFetchEvent
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="init">init</param>
    public extern BackgroundFetchUpdateUIEvent(string type, BackgroundFetchEventInit init);

    /// <summary>
    /// updateUI
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#updateUI")]
    public extern PromiseResult<object> UpdateUI(BackgroundFetchUIOptions? options = default);
}