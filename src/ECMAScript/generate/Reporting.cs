namespace ECMAScript;

/// <summary>
/// ReportBody
/// </summary>
[ECMAScript]
[Description("@#ReportBody")]
public abstract record ReportBody();

/// <summary>
/// Report
/// </summary>
[ECMAScript]
[Description("@#Report")]
public record Report(
    [property: Description("@#type")]string? Type = default,
	[property: Description("@#url")]string? Url = default,
	[property: Description("@#body")]ReportBody? Body = default);

/// <summary>
/// ReportingObserverOptions
/// </summary>
[ECMAScript]
[Description("@#ReportingObserverOptions")]
public record ReportingObserverOptions(
    [property: Description("@#types")]string[]? Types = default,
	[property: Description("@#buffered")]bool Buffered = false);

/// <summary>
/// GenerateTestReportParameters
/// </summary>
[ECMAScript]
[Description("@#GenerateTestReportParameters")]
public record GenerateTestReportParameters(
    [property: Description("@#message")]string? Message = default,
	[property: Description("@#group")]string? Group = default);

/// <summary>
/// ReportingObserver
/// </summary>
[ECMAScript]
[Description("@#ReportingObserver")]
public class ReportingObserver
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="callback">callback</param>
    /// <param name="options">options</param>
    public extern ReportingObserver(ReportingObserverCallback callback, ReportingObserverOptions options);

    /// <summary>
    /// observe
    /// </summary>
    [Description("@#observe")]
    public extern void Observe();

    /// <summary>
    /// disconnect
    /// </summary>
    [Description("@#disconnect")]
    public extern void Disconnect();

    /// <summary>
    /// takeRecords
    /// </summary>
    [Description("@#takeRecords")]
    public extern ReportList TakeRecords();
}