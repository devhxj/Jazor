namespace ECMAScript;

/// <summary>
/// AttributionAggregationService
/// </summary>
[ECMAScript]
[Description("@#AttributionAggregationService")]
public record AttributionAggregationService(
    [property: Description("@#protocol")]string? Protocol = default);

/// <summary>
/// AttributionImpressionOptions
/// </summary>
[ECMAScript]
[Description("@#AttributionImpressionOptions")]
public record AttributionImpressionOptions(
    [property: Description("@#histogramIndex")]uint HistogramIndex = default,
	[property: Description("@#matchValue")]uint MatchValue = 0,
	[property: Description("@#conversionSites")]string[]? ConversionSites = default,
	[property: Description("@#lifetimeDays")]uint LifetimeDays = 30);

/// <summary>
/// AttributionImpressionResult
/// </summary>
[ECMAScript]
[Description("@#AttributionImpressionResult")]
public abstract record AttributionImpressionResult();

/// <summary>
/// AttributionConversionOptions
/// </summary>
[ECMAScript]
[Description("@#AttributionConversionOptions")]
public record AttributionConversionOptions(
    [property: Description("@#aggregationService")]string? AggregationService = default,
	[property: Description("@#epsilon")]double Epsilon = 1.0d,
	[property: Description("@#histogramSize")]uint HistogramSize = default,
	[property: Description("@#lookbackDays")]uint LookbackDays = default,
	[property: Description("@#matchValue")]uint[]? MatchValue = default,
	[property: Description("@#impressionSites")]string[]? ImpressionSites = default,
	[property: Description("@#intermediarySites")]string[]? IntermediarySites = default,
	[property: Description("@#logic")]AttributionLogic Logic = AttributionLogic.LastTouch,
	[property: Description("@#value")]uint Value = 1,
	[property: Description("@#maxValue")]uint MaxValue = 1);

/// <summary>
/// AttributionConversionResult
/// </summary>
[ECMAScript]
[Description("@#AttributionConversionResult")]
public record AttributionConversionResult(
    [property: Description("@#report")]Uint8Array? Report = default);

/// <summary>
/// AttributionAggregationServices
/// </summary>
[ECMAScript]
[Description("@#AttributionAggregationServices")]
public class AttributionAggregationServices : IDictionary<string, AttributionAggregationService>
{
    #region Dictionary
    extern AttributionAggregationService IDictionary<string, AttributionAggregationService>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, AttributionAggregationService>.Keys { get; }
    extern ICollection<AttributionAggregationService> IDictionary<string, AttributionAggregationService>.Values { get; }
    extern int ICollection<KeyValuePair<string, AttributionAggregationService>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, AttributionAggregationService>>.IsReadOnly { get; }
    extern void IDictionary<string, AttributionAggregationService>.Add(string key, AttributionAggregationService value);
    extern void ICollection<KeyValuePair<string, AttributionAggregationService>>.Add(KeyValuePair<string, AttributionAggregationService> item);
    extern void ICollection<KeyValuePair<string, AttributionAggregationService>>.Clear();
    extern bool ICollection<KeyValuePair<string, AttributionAggregationService>>.Contains(KeyValuePair<string, AttributionAggregationService> item);
    extern bool IDictionary<string, AttributionAggregationService>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, AttributionAggregationService>>.CopyTo(KeyValuePair<string, AttributionAggregationService>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, AttributionAggregationService>> IEnumerable<KeyValuePair<string, AttributionAggregationService>>.GetEnumerator();
    extern bool IDictionary<string, AttributionAggregationService>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, AttributionAggregationService>>.Remove(KeyValuePair<string, AttributionAggregationService> item);
    extern bool IDictionary<string, AttributionAggregationService>.TryGetValue(string key, [MaybeNullWhen(false)] out AttributionAggregationService value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// Attribution
/// </summary>
[ECMAScript]
[Description("@#Attribution")]
public partial class Attribution
{
    /// <summary>
    /// aggregationServices
    /// </summary>
    [Description("@#aggregationServices")]
    public extern AttributionAggregationServices AggregationServices { get; }

    /// <summary>
    /// saveImpression
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#saveImpression")]
    public extern PromiseResult<AttributionImpressionResult> SaveImpression(AttributionImpressionOptions options);

    /// <summary>
    /// measureConversion
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#measureConversion")]
    public extern PromiseResult<AttributionConversionResult> MeasureConversion(AttributionConversionOptions options);
}