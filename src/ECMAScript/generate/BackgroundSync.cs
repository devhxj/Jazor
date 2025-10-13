namespace ECMAScript;

/// <summary>
/// SyncEventInit
/// </summary>
[ECMAScript]
[Description("@#SyncEventInit")]
public record SyncEventInit(
    [property: Description("@#tag")]string? Tag = default,
	[property: Description("@#lastChance")]bool LastChance = false): ExtendableEventInit;

/// <summary>
/// SyncManager
/// </summary>
[ECMAScript]
[Description("@#SyncManager")]
public class SyncManager
{
    /// <summary>
    /// register
    /// </summary>
    /// <param name="tag">tag</param>
    [Description("@#register")]
    public extern PromiseResult<object> Register(string tag);

    /// <summary>
    /// getTags
    /// </summary>
    [Description("@#getTags")]
    public extern PromiseResult<string[]> GetTags();
}

/// <summary>
/// SyncEvent
/// </summary>
[ECMAScript]
[Description("@#SyncEvent")]
public class SyncEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="init">init</param>
    public extern SyncEvent(string type, SyncEventInit init);

    /// <summary>
    /// tag
    /// </summary>
    [Description("@#tag")]
    public extern string Tag { get; }

    /// <summary>
    /// lastChance
    /// </summary>
    [Description("@#lastChance")]
    public extern bool LastChance { get; }
}