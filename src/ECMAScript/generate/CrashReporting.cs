namespace ECMAScript;

/// <summary>
/// CrashReportBody
/// </summary>
[ECMAScript]
[Description("@#CrashReportBody")]
public record CrashReportBody(
    [property: Description("@#reason")]string? Reason = default,
	[property: Description("@#stack")]string? Stack = default,
	[property: Description("@#is_top_level")]bool IsTopLevel = default,
	[property: Description("@#page_visibility")]DocumentVisibilityState? PageVisibility = default): ReportBody;