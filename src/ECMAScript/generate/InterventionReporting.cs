namespace ECMAScript;

/// <summary>
/// InterventionReportBody
/// </summary>
[ECMAScript]
[Description("@#InterventionReportBody")]
public record InterventionReportBody(
    [property: Description("@#id")]string? Id = default,
	[property: Description("@#message")]string? Message = default,
	[property: Description("@#sourceFile")]string? SourceFile = default,
	[property: Description("@#lineNumber")]uint LineNumber = default,
	[property: Description("@#columnNumber")]uint ColumnNumber = default): ReportBody;