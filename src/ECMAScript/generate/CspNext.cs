namespace ECMAScript;

/// <summary>
/// ScriptingPolicyReportBody
/// </summary>
[ECMAScript]
[Description("@#ScriptingPolicyReportBody")]
public record ScriptingPolicyReportBody(
    [property: Description("@#violationType")]string? ViolationType = default,
	[property: Description("@#violationURL")]string? ViolationURL = default,
	[property: Description("@#violationSample")]string? ViolationSample = default,
	[property: Description("@#lineno")]uint Lineno = default,
	[property: Description("@#colno")]uint Colno = default): ReportBody;