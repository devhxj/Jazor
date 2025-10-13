namespace ECMAScript;

/// <summary>
/// BackgroundSyncOptions
/// </summary>
[ECMAScript]
[Description("@#BackgroundSyncOptions")]
public record BackgroundSyncOptions(
    [property: Description("@#minInterval")]ulong MinInterval = 0);

/// <summary>
/// PeriodicSyncEventInit
/// </summary>
[ECMAScript]
[Description("@#PeriodicSyncEventInit")]
public record PeriodicSyncEventInit(
    [property: Description("@#tag")]string? Tag = default): ExtendableEventInit;

/// <summary>
/// PeriodicSyncManager
/// </summary>
[ECMAScript]
[Description("@#PeriodicSyncManager")]
public class PeriodicSyncManager
{
    /// <summary>
    /// register
    /// </summary>
    /// <param name="tag">tag</param>
    /// <param name="options">options</param>
    [Description("@#register")]
    public extern PromiseResult<object> Register(string tag, BackgroundSyncOptions? options = default);

    /// <summary>
    /// getTags
    /// </summary>
    [Description("@#getTags")]
    public extern PromiseResult<string[]> GetTags();

    /// <summary>
    /// unregister
    /// </summary>
    /// <param name="tag">tag</param>
    [Description("@#unregister")]
    public extern PromiseResult<object> Unregister(string tag);
}

/// <summary>
/// PeriodicSyncEvent
/// </summary>
[ECMAScript]
[Description("@#PeriodicSyncEvent")]
public class PeriodicSyncEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="init">init</param>
    public extern PeriodicSyncEvent(string type, PeriodicSyncEventInit init);

    /// <summary>
    /// tag
    /// </summary>
    [Description("@#tag")]
    public extern string Tag { get; }
}