namespace ECMAScript;

/// <summary>
/// StorageBucketOptions
/// </summary>
[ECMAScript]
[Description("@#StorageBucketOptions")]
public record StorageBucketOptions(
    [property: Description("@#persisted")]bool Persisted = false,
	[property: Description("@#quota")]ulong Quota = default,
	[property: Description("@#expires")]double Expires = default);

/// <summary>
/// StorageBucketManager
/// </summary>
[ECMAScript]
[Description("@#StorageBucketManager")]
public class StorageBucketManager
{
    /// <summary>
    /// open
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="options">options</param>
    [Description("@#open")]
    public extern PromiseResult<StorageBucket> Open(string name, StorageBucketOptions? options = default);

    /// <summary>
    /// keys
    /// </summary>
    [Description("@#keys")]
    public extern PromiseResult<string[]> Keys();

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#delete")]
    public extern PromiseResult<object> Delete(string name);
}

/// <summary>
/// StorageBucket
/// </summary>
[ECMAScript]
[Description("@#StorageBucket")]
public class StorageBucket
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// persist
    /// </summary>
    [Description("@#persist")]
    public extern PromiseResult<bool> Persist();

    /// <summary>
    /// persisted
    /// </summary>
    [Description("@#persisted")]
    public extern PromiseResult<bool> Persisted();

    /// <summary>
    /// estimate
    /// </summary>
    [Description("@#estimate")]
    public extern PromiseResult<StorageEstimate> Estimate();

    /// <summary>
    /// setExpires
    /// </summary>
    /// <param name="expires">expires</param>
    [Description("@#setExpires")]
    public extern PromiseResult<object> SetExpires(double expires);

    /// <summary>
    /// expires
    /// </summary>
    [Description("@#expires")]
    public extern PromiseResult<double?> Expires();

    /// <summary>
    /// indexedDB
    /// </summary>
    [Description("@#indexedDB")]
    public extern IDBFactory IndexedDB { get; }

    /// <summary>
    /// caches
    /// </summary>
    [Description("@#caches")]
    public extern CacheStorage Caches { get; }

    /// <summary>
    /// getDirectory
    /// </summary>
    [Description("@#getDirectory")]
    public extern PromiseResult<FileSystemDirectoryHandle> GetDirectory();
}