namespace ECMAScript;

/// <summary>
/// CookieStoreGetOptions
/// </summary>
[ECMAScript]
[Description("@#CookieStoreGetOptions")]
public record CookieStoreGetOptions(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#url")]string? Url = default);

/// <summary>
/// CookieInit
/// </summary>
[ECMAScript]
[Description("@#CookieInit")]
public record CookieInit(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#value")]string? Value = default,
	[property: Description("@#expires")]double? Expires = null,
	[property: Description("@#domain")]string? Domain = null,
	[property: Description("@#path")]string? Path = default,
	[property: Description("@#sameSite")]CookieSameSite SameSite = CookieSameSite.Strict,
	[property: Description("@#partitioned")]bool Partitioned = false);

/// <summary>
/// CookieStoreDeleteOptions
/// </summary>
[ECMAScript]
[Description("@#CookieStoreDeleteOptions")]
public record CookieStoreDeleteOptions(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#domain")]string? Domain = null,
	[property: Description("@#path")]string? Path = default,
	[property: Description("@#partitioned")]bool Partitioned = false);

/// <summary>
/// CookieListItem
/// </summary>
[ECMAScript]
[Description("@#CookieListItem")]
public record CookieListItem(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#value")]string? Value = default,
	[property: Description("@#domain")]string? Domain = default,
	[property: Description("@#path")]string? Path = default,
	[property: Description("@#expires")]double Expires = default,
	[property: Description("@#secure")]bool Secure = default,
	[property: Description("@#sameSite")]CookieSameSite? SameSite = default,
	[property: Description("@#partitioned")]bool Partitioned = default);

/// <summary>
/// CookieChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#CookieChangeEventInit")]
public record CookieChangeEventInit(
    [property: Description("@#changed")]CookieList? Changed = default,
	[property: Description("@#deleted")]CookieList? Deleted = default): EventInit;

/// <summary>
/// ExtendableCookieChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#ExtendableCookieChangeEventInit")]
public record ExtendableCookieChangeEventInit(
    [property: Description("@#changed")]CookieList? Changed = default,
	[property: Description("@#deleted")]CookieList? Deleted = default): ExtendableEventInit;

/// <summary>
/// CookieStore
/// </summary>
[ECMAScript]
[Description("@#CookieStore")]
public class CookieStore : EventTarget
{
    /// <summary>
    /// get
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#get")]
    public extern PromiseResult<CookieListItem?> Get(string name);

    /// <summary>
    /// get
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#get")]
    public extern PromiseResult<CookieListItem?> Get(CookieStoreGetOptions? options = default);

    /// <summary>
    /// getAll
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getAll")]
    public extern PromiseResult<CookieList> GetAll(string name);

    /// <summary>
    /// getAll
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getAll")]
    public extern PromiseResult<CookieList> GetAll(CookieStoreGetOptions? options = default);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    [Description("@#set")]
    public extern PromiseResult<object> Set(string name, string value);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#set")]
    public extern PromiseResult<object> Set(CookieInit options);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#delete")]
    public extern PromiseResult<object> Delete(string name);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#delete")]
    public extern PromiseResult<object> Delete(CookieStoreDeleteOptions options);

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }
}

/// <summary>
/// CookieStoreManager
/// </summary>
[ECMAScript]
[Description("@#CookieStoreManager")]
public class CookieStoreManager
{
    /// <summary>
    /// subscribe
    /// </summary>
    /// <param name="subscriptions">subscriptions</param>
    [Description("@#subscribe")]
    public extern PromiseResult<object> Subscribe(CookieStoreGetOptions[] subscriptions);

    /// <summary>
    /// getSubscriptions
    /// </summary>
    [Description("@#getSubscriptions")]
    public extern PromiseResult<CookieStoreGetOptions[]> GetSubscriptions();

    /// <summary>
    /// unsubscribe
    /// </summary>
    /// <param name="subscriptions">subscriptions</param>
    [Description("@#unsubscribe")]
    public extern PromiseResult<object> Unsubscribe(CookieStoreGetOptions[] subscriptions);
}

/// <summary>
/// CookieChangeEvent
/// </summary>
[ECMAScript]
[Description("@#CookieChangeEvent")]
public class CookieChangeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern CookieChangeEvent(string type, CookieChangeEventInit eventInitDict);

    /// <summary>
    /// changed
    /// </summary>
    [Description("@#changed")]
    public extern FrozenSet<CookieListItem> Changed { get; }

    /// <summary>
    /// deleted
    /// </summary>
    [Description("@#deleted")]
    public extern FrozenSet<CookieListItem> Deleted { get; }
}

/// <summary>
/// ExtendableCookieChangeEvent
/// </summary>
[ECMAScript]
[Description("@#ExtendableCookieChangeEvent")]
public class ExtendableCookieChangeEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern ExtendableCookieChangeEvent(string type, ExtendableCookieChangeEventInit eventInitDict);

    /// <summary>
    /// changed
    /// </summary>
    [Description("@#changed")]
    public extern FrozenSet<CookieListItem> Changed { get; }

    /// <summary>
    /// deleted
    /// </summary>
    [Description("@#deleted")]
    public extern FrozenSet<CookieListItem> Deleted { get; }
}