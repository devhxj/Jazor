namespace ECMAScript;

/// <summary>
/// StorageAccessTypes
/// </summary>
[ECMAScript]
[Description("@#StorageAccessTypes")]
public record StorageAccessTypes(
    [property: Description("@#all")]bool All = false,
	[property: Description("@#cookies")]bool Cookies = false,
	[property: Description("@#sessionStorage")]bool SessionStorage = false,
	[property: Description("@#localStorage")]bool LocalStorage = false,
	[property: Description("@#indexedDB")]bool IndexedDB = false,
	[property: Description("@#locks")]bool Locks = false,
	[property: Description("@#caches")]bool Caches = false,
	[property: Description("@#getDirectory")]bool GetDirectory = false,
	[property: Description("@#estimate")]bool Estimate = false,
	[property: Description("@#createObjectURL")]bool CreateObjectURL = false,
	[property: Description("@#revokeObjectURL")]bool RevokeObjectURL = false,
	[property: Description("@#BroadcastChannel")]bool BroadcastChannel = false,
	[property: Description("@#SharedWorker")]bool SharedWorker = false);

/// <summary>
/// SharedWorkerOptions
/// </summary>
[ECMAScript]
[Description("@#SharedWorkerOptions")]
public record SharedWorkerOptions(
    [property: Description("@#sameSiteCookies")]SameSiteCookiesType? SameSiteCookies = default): WorkerOptions;

/// <summary>
/// StorageAccessHandle
/// </summary>
[ECMAScript]
[Description("@#StorageAccessHandle")]
public class StorageAccessHandle
{
    /// <summary>
    /// sessionStorage
    /// </summary>
    [Description("@#sessionStorage")]
    public extern Storage SessionStorage { get; }

    /// <summary>
    /// localStorage
    /// </summary>
    [Description("@#localStorage")]
    public extern Storage LocalStorage { get; }

    /// <summary>
    /// indexedDB
    /// </summary>
    [Description("@#indexedDB")]
    public extern IDBFactory IndexedDB { get; }

    /// <summary>
    /// locks
    /// </summary>
    [Description("@#locks")]
    public extern LockManager Locks { get; }

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

    /// <summary>
    /// estimate
    /// </summary>
    [Description("@#estimate")]
    public extern PromiseResult<StorageEstimate> Estimate();

    /// <summary>
    /// createObjectURL
    /// </summary>
    /// <param name="obj">obj</param>
    [Description("@#createObjectURL")]
    public extern string CreateObjectURL(Either<Blob, MediaSource> obj);

    /// <summary>
    /// createObjectURL
    /// </summary>
    /// <param name="obj">obj</param>
    [Description("@#createObjectURL")]
    public extern string CreateObjectURL(Blob obj);

    /// <summary>
    /// createObjectURL
    /// </summary>
    /// <param name="obj">obj</param>
    [Description("@#createObjectURL")]
    public extern string CreateObjectURL(MediaSource obj);

    /// <summary>
    /// revokeObjectURL
    /// </summary>
    /// <param name="url">url</param>
    [Description("@#revokeObjectURL")]
    public extern void RevokeObjectURL(string url);

    /// <summary>
    /// BroadcastChannel
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#BroadcastChannel")]
    public extern BroadcastChannel BroadcastChannel(string name);

    /// <summary>
    /// SharedWorker
    /// </summary>
    /// <param name="scriptUrl">scriptURL</param>
    /// <param name="options">options</param>
    [Description("@#SharedWorker")]
    public extern SharedWorker SharedWorker(string scriptUrl, Either<string, SharedWorkerOptions> options);

    /// <summary>
    /// SharedWorker
    /// </summary>
    /// <param name="scriptUrl">scriptURL para</param>
    /// <param name="options">options</param>
    [Description("@#SharedWorker")]
    public extern SharedWorker SharedWorker(string scriptUrl, string options);

    /// <summary>
    /// SharedWorker
    /// </summary>
    /// <param name="scriptUrl">scriptURL para</param>
    /// <param name="options">options</param>
    [Description("@#SharedWorker")]
    public extern SharedWorker SharedWorker(string scriptUrl, SharedWorkerOptions? options = default);
}