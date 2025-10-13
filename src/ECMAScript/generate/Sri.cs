namespace ECMAScript;

/// <summary>
/// IntegrityViolationReportBody
/// </summary>
[ECMAScript]
[Description("@#IntegrityViolationReportBody")]
public record IntegrityViolationReportBody(
    [property: Description("@#documentURL")]string? DocumentURL = default,
	[property: Description("@#blockedURL")]string? BlockedURL = default,
	[property: Description("@#destination")]string? Destination = default,
	[property: Description("@#reportOnly")]bool ReportOnly = default): ReportBody;