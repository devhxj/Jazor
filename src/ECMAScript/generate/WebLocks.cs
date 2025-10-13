namespace ECMAScript;

/// <summary>
/// LockOptions
/// </summary>
[ECMAScript]
[Description("@#LockOptions")]
public record LockOptions(
    [property: Description("@#mode")]LockMode Mode = LockMode.Exclusive,
	[property: Description("@#ifAvailable")]bool IfAvailable = false,
	[property: Description("@#steal")]bool Steal = false,
	[property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// LockManagerSnapshot
/// </summary>
[ECMAScript]
[Description("@#LockManagerSnapshot")]
public record LockManagerSnapshot(
    [property: Description("@#held")]LockInfo[]? Held = default,
	[property: Description("@#pending")]LockInfo[]? Pending = default);

/// <summary>
/// LockInfo
/// </summary>
[ECMAScript]
[Description("@#LockInfo")]
public record LockInfo(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#mode")]LockMode? Mode = default,
	[property: Description("@#clientId")]string? ClientId = default);

/// <summary>
/// LockManager
/// </summary>
[ECMAScript]
[Description("@#LockManager")]
public class LockManager
{
    /// <summary>
    /// request
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="callback">callback</param>
    [Description("@#request")]
    public extern PromiseResult<object> Request(string name, LockGrantedCallback callback);

    /// <summary>
    /// request
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="options">options</param>
    /// <param name="callback">callback</param>
    [Description("@#request")]
    public extern PromiseResult<object> Request(string name, LockOptions options, LockGrantedCallback callback);

    /// <summary>
    /// query
    /// </summary>
    [Description("@#query")]
    public extern PromiseResult<LockManagerSnapshot> Query();
}

/// <summary>
/// Lock
/// </summary>
[ECMAScript]
[Description("@#Lock")]
public class Lock
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// mode
    /// </summary>
    [Description("@#mode")]
    public extern LockMode Mode { get; }
}