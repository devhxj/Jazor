namespace ECMAScript;

/// <summary>
/// PerformanceObserverInit
/// </summary>
[ECMAScript]
[Description("@#PerformanceObserverInit")]
public record PerformanceObserverInit(
    [property: Description("@#durationThreshold")]double DurationThreshold = default,
	[property: Description("@#entryTypes")]string[]? EntryTypes = default,
	[property: Description("@#type")]string? Type = default,
	[property: Description("@#buffered")]bool Buffered = default)
{
    [Category("optional")]
    public extern static PerformanceObserverInit OptionalDurationThreshold(
        [Description("@#durationThreshold")]double DurationThreshold = default);

    [Category("optional")]
    public extern static PerformanceObserverInit OptionalEntryTypesTypeBuffered(
        [Description("@#entryTypes")]string[]? EntryTypes = default,
        [Description("@#type")]string? Type = default,
        [Description("@#buffered")]bool Buffered = default);
}

/// <summary>
/// PerformanceEventTiming
/// </summary>
[ECMAScript]
[Description("@#PerformanceEventTiming")]
public class PerformanceEventTiming : PerformanceEntry
{
    /// <summary>
    /// processingStart
    /// </summary>
    [Description("@#processingStart")]
    public extern double ProcessingStart { get; }

    /// <summary>
    /// processingEnd
    /// </summary>
    [Description("@#processingEnd")]
    public extern double ProcessingEnd { get; }

    /// <summary>
    /// cancelable
    /// </summary>
    [Description("@#cancelable")]
    public extern bool Cancelable { get; }

    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern Node? Target { get; }

    /// <summary>
    /// interactionId
    /// </summary>
    [Description("@#interactionId")]
    public extern ulong InteractionId { get; }
}

/// <summary>
/// EventCounts
/// </summary>
[ECMAScript]
[Description("@#EventCounts")]
public class EventCounts : IDictionary<string, ulong>
{
    #region Dictionary
    extern ulong IDictionary<string, ulong>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, ulong>.Keys { get; }
    extern ICollection<ulong> IDictionary<string, ulong>.Values { get; }
    extern int ICollection<KeyValuePair<string, ulong>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, ulong>>.IsReadOnly { get; }
    extern void IDictionary<string, ulong>.Add(string key, ulong value);
    extern void ICollection<KeyValuePair<string, ulong>>.Add(KeyValuePair<string, ulong> item);
    extern void ICollection<KeyValuePair<string, ulong>>.Clear();
    extern bool ICollection<KeyValuePair<string, ulong>>.Contains(KeyValuePair<string, ulong> item);
    extern bool IDictionary<string, ulong>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, ulong>>.CopyTo(KeyValuePair<string, ulong>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, ulong>> IEnumerable<KeyValuePair<string, ulong>>.GetEnumerator();
    extern bool IDictionary<string, ulong>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, ulong>>.Remove(KeyValuePair<string, ulong> item);
    extern bool IDictionary<string, ulong>.TryGetValue(string key, [MaybeNullWhen(false)] out ulong value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// Performance
/// </summary>
[ECMAScript]
[Description("@#Performance")]
public partial class Performance : EventTarget
{
    /// <summary>
    /// eventCounts
    /// </summary>
    [Description("@#eventCounts")]
    public extern EventCounts EventCounts { get; }

    /// <summary>
    /// interactionCount
    /// </summary>
    [Description("@#interactionCount")]
    public extern ulong InteractionCount { get; }

    /// <summary>
    /// now
    /// </summary>
    [Description("@#now")]
    public extern double Now();

    /// <summary>
    /// timeOrigin
    /// </summary>
    [Description("@#timeOrigin")]
    public extern double TimeOrigin { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();

    /// <summary>
    /// timing
    /// </summary>
    [Description("@#timing")]
    public extern PerformanceTiming Timing { get; }

    /// <summary>
    /// navigation
    /// </summary>
    [Description("@#navigation")]
    public extern PerformanceNavigation Navigation { get; }

    /// <summary>
    /// measureUserAgentSpecificMemory
    /// </summary>
    [Description("@#measureUserAgentSpecificMemory")]
    public extern PromiseResult<MemoryMeasurement> MeasureUserAgentSpecificMemory();

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

    /// <summary>
    /// clearResourceTimings
    /// </summary>
    [Description("@#clearResourceTimings")]
    public extern void ClearResourceTimings();

    /// <summary>
    /// setResourceTimingBufferSize
    /// </summary>
    /// <param name="maxSize">maxSize</param>
    [Description("@#setResourceTimingBufferSize")]
    public extern void SetResourceTimingBufferSize(uint maxSize);

    /// <summary>
    /// onresourcetimingbufferfull
    /// </summary>
    [Description("@#onresourcetimingbufferfull")]
    public extern EventHandler Onresourcetimingbufferfull { get; set; }

    /// <summary>
    /// mark
    /// </summary>
    /// <param name="markName">markName</param>
    /// <param name="markOptions">markOptions</param>
    [Description("@#mark")]
    public extern PerformanceMark Mark(string markName, PerformanceMarkOptions? markOptions = default);

    /// <summary>
    /// clearMarks
    /// </summary>
    /// <param name="markName">markName</param>
    [Description("@#clearMarks")]
    public extern void ClearMarks(string markName);

    /// <summary>
    /// measure
    /// </summary>
    /// <param name="measureName">measureName</param>
    /// <param name="startOrMeasureOptions">startOrMeasureOptions</param>
    /// <param name="endMark">endMark</param>
    [Description("@#measure")]
    public extern PerformanceMeasure Measure(string measureName, Either<string, PerformanceMeasureOptions> startOrMeasureOptions, string endMark);

    /// <summary>
    /// clearMeasures
    /// </summary>
    /// <param name="measureName">measureName</param>
    [Description("@#clearMeasures")]
    public extern void ClearMeasures(string measureName);
}