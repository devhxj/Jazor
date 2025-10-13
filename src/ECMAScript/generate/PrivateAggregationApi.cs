namespace ECMAScript;

/// <summary>
/// PAHistogramContribution
/// </summary>
[ECMAScript]
[Description("@#PAHistogramContribution")]
public record PAHistogramContribution(
    [property: Description("@#bucket")]BigInteger? Bucket = default,
	[property: Description("@#value")]int Value = default,
	[property: Description("@#filteringId")]BigInteger? FilteringId = default);

/// <summary>
/// PADebugModeOptions
/// </summary>
[ECMAScript]
[Description("@#PADebugModeOptions")]
public record PADebugModeOptions(
    [property: Description("@#debugKey")]BigInteger? DebugKey = default);

/// <summary>
/// PrivateAggregation
/// </summary>
[ECMAScript]
[Description("@#PrivateAggregation")]
public class PrivateAggregation
{
    /// <summary>
    /// contributeToHistogram
    /// </summary>
    /// <param name="contribution">contribution</param>
    [Description("@#contributeToHistogram")]
    public extern void ContributeToHistogram(PAHistogramContribution contribution);

    /// <summary>
    /// contributeToHistogramOnEvent
    /// </summary>
    /// <param name="event">event</param>
    /// <param name="contribution">contribution</param>
    [Description("@#contributeToHistogramOnEvent")]
    public extern void ContributeToHistogramOnEvent(string @event, Dictionary<string, object> contribution);

    /// <summary>
    /// enableDebugMode
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#enableDebugMode")]
    public extern void EnableDebugMode(PADebugModeOptions? options = default);
}