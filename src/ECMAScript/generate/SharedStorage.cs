namespace ECMAScript;

/// <summary>
/// SharedStorageUrlWithMetadata
/// </summary>
[ECMAScript]
[Description("@#SharedStorageUrlWithMetadata")]
public record SharedStorageUrlWithMetadata(
    [property: Description("@#url")]string? Url = default,
	[property: Description("@#reportingMetadata")]object? ReportingMetadata = default);

/// <summary>
/// SharedStorageModifierMethodOptions
/// </summary>
[ECMAScript]
[Description("@#SharedStorageModifierMethodOptions")]
public record SharedStorageModifierMethodOptions(
    [property: Description("@#withLock")]string? WithLock = default);

/// <summary>
/// SharedStorageSetMethodOptions
/// </summary>
[ECMAScript]
[Description("@#SharedStorageSetMethodOptions")]
public record SharedStorageSetMethodOptions(
    [property: Description("@#ignoreIfPresent")]bool IgnoreIfPresent = default): SharedStorageModifierMethodOptions;

/// <summary>
/// SharedStoragePrivateAggregationConfig
/// </summary>
[ECMAScript]
[Description("@#SharedStoragePrivateAggregationConfig")]
public record SharedStoragePrivateAggregationConfig(
    [property: Description("@#aggregationCoordinatorOrigin")]string? AggregationCoordinatorOrigin = default,
	[property: Description("@#contextId")]string? ContextId = default,
	[property: Description("@#filteringIdMaxBytes")]ulong FilteringIdMaxBytes = default,
	[property: Description("@#maxContributions")]ulong MaxContributions = default);

/// <summary>
/// SharedStorageRunOperationMethodOptions
/// </summary>
[ECMAScript]
[Description("@#SharedStorageRunOperationMethodOptions")]
public record SharedStorageRunOperationMethodOptions(
    [property: Description("@#data")]object? Data = default,
	[property: Description("@#resolveToConfig")]bool ResolveToConfig = false,
	[property: Description("@#keepAlive")]bool KeepAlive = false,
	[property: Description("@#privateAggregationConfig")]SharedStoragePrivateAggregationConfig? PrivateAggregationConfig = default,
	[property: Description("@#savedQuery")]string? SavedQuery = default);

/// <summary>
/// SharedStorageWorkletOptions
/// </summary>
[ECMAScript]
[Description("@#SharedStorageWorkletOptions")]
public record SharedStorageWorkletOptions(
    [property: Description("@#dataOrigin")]string? DataOrigin = default): WorkletOptions;

/// <summary>
/// SharedStorageWorklet
/// </summary>
[ECMAScript]
[Description("@#SharedStorageWorklet")]
public class SharedStorageWorklet : Worklet
{
    /// <summary>
    /// selectURL
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="urls">urls</param>
    /// <param name="options">options</param>
    [Description("@#selectURL")]
    public extern PromiseResult<SharedStorageResponse> SelectURL(string name, SharedStorageUrlWithMetadata[] urls, SharedStorageRunOperationMethodOptions? options = default);

    /// <summary>
    /// run
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="options">options</param>
    [Description("@#run")]
    public extern PromiseResult<object> Run(string name, SharedStorageRunOperationMethodOptions? options = default);
}

/// <summary>
/// SharedStorageWorkletGlobalScope
/// </summary>
[ECMAScript]
[Description("@#SharedStorageWorkletGlobalScope")]
public class SharedStorageWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// register
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="operationCtor">operationCtor</param>
    [Description("@#register")]
    public extern void Register(string name, Delegate operationCtor);

    /// <summary>
    /// sharedStorage
    /// </summary>
    [Description("@#sharedStorage")]
    public extern SharedStorage SharedStorage { get; }

    /// <summary>
    /// privateAggregation
    /// </summary>
    [Description("@#privateAggregation")]
    public extern PrivateAggregation PrivateAggregation { get; }

    /// <summary>
    /// interestGroups
    /// </summary>
    [Description("@#interestGroups")]
    public extern PromiseResult<StorageInterestGroup[]> InterestGroups();

    /// <summary>
    /// navigator
    /// </summary>
    [Description("@#navigator")]
    public extern SharedStorageWorkletNavigator Navigator { get; }
}

/// <summary>
/// SharedStorageModifierMethod
/// </summary>
[ECMAScript]
[Description("@#SharedStorageModifierMethod")]
public abstract class SharedStorageModifierMethod
{

}

/// <summary>
/// SharedStorageSetMethod
/// </summary>
[ECMAScript]
[Description("@#SharedStorageSetMethod")]
public class SharedStorageSetMethod : SharedStorageModifierMethod
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <param name="options">options</param>
    public extern SharedStorageSetMethod(string key, string value, SharedStorageSetMethodOptions options);
}

/// <summary>
/// SharedStorageAppendMethod
/// </summary>
[ECMAScript]
[Description("@#SharedStorageAppendMethod")]
public class SharedStorageAppendMethod : SharedStorageModifierMethod
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <param name="options">options</param>
    public extern SharedStorageAppendMethod(string key, string value, SharedStorageModifierMethodOptions options);
}

/// <summary>
/// SharedStorageDeleteMethod
/// </summary>
[ECMAScript]
[Description("@#SharedStorageDeleteMethod")]
public class SharedStorageDeleteMethod : SharedStorageModifierMethod
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="options">options</param>
    public extern SharedStorageDeleteMethod(string key, SharedStorageModifierMethodOptions options);
}

/// <summary>
/// SharedStorageClearMethod
/// </summary>
[ECMAScript]
[Description("@#SharedStorageClearMethod")]
public class SharedStorageClearMethod : SharedStorageModifierMethod
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern SharedStorageClearMethod(SharedStorageModifierMethodOptions options);
}

/// <summary>
/// SharedStorage
/// </summary>
[ECMAScript]
[Description("@#SharedStorage")]
public class SharedStorage : IEnumerable<(string, string)>
{
    /// <summary>
    /// get
    /// </summary>
    /// <param name="key">key</param>
    [Description("@#get")]
    public extern PromiseResult<string> Get(string key);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <param name="options">options</param>
    [Description("@#set")]
    public extern PromiseResult<object> Set(string key, string value, SharedStorageSetMethodOptions? options = default);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="value">value</param>
    /// <param name="options">options</param>
    [Description("@#append")]
    public extern PromiseResult<object> Append(string key, string value, SharedStorageModifierMethodOptions? options = default);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="options">options</param>
    [Description("@#delete")]
    public extern PromiseResult<object> Delete(string key, SharedStorageModifierMethodOptions? options = default);

    /// <summary>
    /// clear
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#clear")]
    public extern PromiseResult<object> Clear(SharedStorageModifierMethodOptions? options = default);

    /// <summary>
    /// batchUpdate
    /// </summary>
    /// <param name="methods">methods</param>
    /// <param name="options">options</param>
    [Description("@#batchUpdate")]
    public extern PromiseResult<object> BatchUpdate(SharedStorageModifierMethod[] methods, SharedStorageModifierMethodOptions? options = default);

    /// <summary>
    /// selectURL
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="urls">urls</param>
    /// <param name="options">options</param>
    [Description("@#selectURL")]
    public extern PromiseResult<SharedStorageResponse> SelectURL(string name, SharedStorageUrlWithMetadata[] urls, SharedStorageRunOperationMethodOptions? options = default);

    /// <summary>
    /// run
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="options">options</param>
    [Description("@#run")]
    public extern PromiseResult<object> Run(string name, SharedStorageRunOperationMethodOptions? options = default);

    /// <summary>
    /// createWorklet
    /// </summary>
    /// <param name="moduleUrl">moduleURL</param>
    /// <param name="options">options</param>
    [Description("@#createWorklet")]
    public extern PromiseResult<SharedStorageWorklet> CreateWorklet(string moduleUrl, SharedStorageWorkletOptions? options = default);

    /// <summary>
    /// worklet
    /// </summary>
    [Description("@#worklet")]
    public extern SharedStorageWorklet Worklet { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern PromiseResult<uint> Length();

    /// <summary>
    /// remainingBudget
    /// </summary>
    [Description("@#remainingBudget")]
    public extern PromiseResult<double> RemainingBudget();

    extern IEnumerator<(string, string)> IEnumerable<(string, string)>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();
}

/// <summary>
/// SharedStorageWorkletNavigator
/// </summary>
[ECMAScript]
[Description("@#SharedStorageWorkletNavigator")]
public class SharedStorageWorkletNavigator
{


    #region mixin NavigatorLocks
    /// <summary>
    /// locks
    /// </summary>
    [Description("@#locks")]
    public extern LockManager Locks { get; }
    #endregion
}