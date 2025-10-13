namespace ECMAScript;

/// <summary>
/// PerformanceObserverCallbackOptions
/// </summary>
[ECMAScript]
[Description("@#PerformanceObserverCallbackOptions")]
public record PerformanceObserverCallbackOptions(
    [property: Description("@#droppedEntriesCount")]ulong DroppedEntriesCount = default);

/// <summary>
/// PerformanceEntry
/// </summary>
[ECMAScript]
[Description("@#PerformanceEntry")]
public class PerformanceEntry
{
    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern ulong Id { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// entryType
    /// </summary>
    [Description("@#entryType")]
    public extern string EntryType { get; }

    /// <summary>
    /// startTime
    /// </summary>
    [Description("@#startTime")]
    public extern double StartTime { get; }

    /// <summary>
    /// duration
    /// </summary>
    [Description("@#duration")]
    public extern double Duration { get; }

    /// <summary>
    /// navigationId
    /// </summary>
    [Description("@#navigationId")]
    public extern ulong NavigationId { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}

/// <summary>
/// PerformanceObserver
/// </summary>
[ECMAScript]
[Description("@#PerformanceObserver")]
public class PerformanceObserver
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="callback">callback</param>
    public extern PerformanceObserver(PerformanceObserverCallback callback);

    /// <summary>
    /// observe
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#observe")]
    public extern void Observe(PerformanceObserverInit? options = default);

    /// <summary>
    /// disconnect
    /// </summary>
    [Description("@#disconnect")]
    public extern void Disconnect();

    /// <summary>
    /// takeRecords
    /// </summary>
    [Description("@#takeRecords")]
    public extern PerformanceEntryList TakeRecords();

    /// <summary>
    /// supportedEntryTypes
    /// </summary>
    [Description("@#supportedEntryTypes")]
    public extern static FrozenSet<string> SupportedEntryTypes { get; }
}

/// <summary>
/// PerformanceObserverEntryList
/// </summary>
[ECMAScript]
[Description("@#PerformanceObserverEntryList")]
public class PerformanceObserverEntryList
{
    /// <summary>
    /// getEntries
    /// </summary>
    [Description("@#getEntries")]
    public extern PerformanceEntryList GetEntries();

    /// <summary>
    /// getEntriesByType
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#getEntriesByType")]
    public extern PerformanceEntryList GetEntriesByType(string type);

    /// <summary>
    /// getEntriesByName
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="type">type</param>
    [Description("@#getEntriesByName")]
    public extern PerformanceEntryList GetEntriesByName(string name, string type);
}