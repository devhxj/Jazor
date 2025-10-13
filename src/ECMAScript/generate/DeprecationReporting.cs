namespace ECMAScript;

/// <summary>
/// DeprecationReportBody
/// </summary>
[ECMAScript]
[Description("@#DeprecationReportBody")]
public record DeprecationReportBody(
    [property: Description("@#id")]string? Id = default,
	[property: Description("@#anticipatedRemoval")]object? AnticipatedRemoval = default,
	[property: Description("@#message")]string? Message = default,
	[property: Description("@#sourceFile")]string? SourceFile = default,
	[property: Description("@#lineNumber")]uint LineNumber = default,
	[property: Description("@#columnNumber")]uint ColumnNumber = default): ReportBody;