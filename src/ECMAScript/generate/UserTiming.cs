namespace ECMAScript;

/// <summary>
/// PerformanceMarkOptions
/// </summary>
[ECMAScript]
[Description("@#PerformanceMarkOptions")]
public record PerformanceMarkOptions(
    [property: Description("@#detail")]object? Detail = default,
	[property: Description("@#startTime")]double StartTime = default);

/// <summary>
/// PerformanceMeasureOptions
/// </summary>
[ECMAScript]
[Description("@#PerformanceMeasureOptions")]
public record PerformanceMeasureOptions(
    [property: Description("@#detail")]object? Detail = default,
	[property: Description("@#start")]Either<string, double>? Start = default,
	[property: Description("@#duration")]double Duration = default,
	[property: Description("@#end")]Either<string, double>? End = default);

/// <summary>
/// PerformanceMark
/// </summary>
[ECMAScript]
[Description("@#PerformanceMark")]
public class PerformanceMark : PerformanceEntry
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="markName">markName</param>
    /// <param name="markOptions">markOptions</param>
    public extern PerformanceMark(string markName, PerformanceMarkOptions markOptions);

    /// <summary>
    /// detail
    /// </summary>
    [Description("@#detail")]
    public extern object Detail { get; }
}

/// <summary>
/// PerformanceMeasure
/// </summary>
[ECMAScript]
[Description("@#PerformanceMeasure")]
public class PerformanceMeasure : PerformanceEntry
{
    /// <summary>
    /// detail
    /// </summary>
    [Description("@#detail")]
    public extern object Detail { get; }
}