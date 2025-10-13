namespace ECMAScript;

/// <summary>
/// CSPViolationReportBody
/// </summary>
[ECMAScript]
[Description("@#CSPViolationReportBody")]
public record CSPViolationReportBody(
    [property: Description("@#documentURL")]string? DocumentURL = default,
	[property: Description("@#referrer")]string? Referrer = default,
	[property: Description("@#blockedURL")]string? BlockedURL = default,
	[property: Description("@#effectiveDirective")]string? EffectiveDirective = default,
	[property: Description("@#originalPolicy")]string? OriginalPolicy = default,
	[property: Description("@#sourceFile")]string? SourceFile = default,
	[property: Description("@#sample")]string? Sample = default,
	[property: Description("@#disposition")]SecurityPolicyViolationEventDisposition? Disposition = default,
	[property: Description("@#statusCode")]ushort StatusCode = default,
	[property: Description("@#lineNumber")]uint LineNumber = default,
	[property: Description("@#columnNumber")]uint ColumnNumber = default): ReportBody;

/// <summary>
/// SecurityPolicyViolationEventInit
/// </summary>
[ECMAScript]
[Description("@#SecurityPolicyViolationEventInit")]
public record SecurityPolicyViolationEventInit(
    [property: Description("@#documentURI")]string? DocumentURI = default,
	[property: Description("@#referrer")]string? Referrer = default,
	[property: Description("@#blockedURI")]string? BlockedURI = default,
	[property: Description("@#violatedDirective")]string? ViolatedDirective = default,
	[property: Description("@#effectiveDirective")]string? EffectiveDirective = default,
	[property: Description("@#originalPolicy")]string? OriginalPolicy = default,
	[property: Description("@#sourceFile")]string? SourceFile = default,
	[property: Description("@#sample")]string? Sample = default,
	[property: Description("@#disposition")]SecurityPolicyViolationEventDisposition Disposition = SecurityPolicyViolationEventDisposition.Enforce,
	[property: Description("@#statusCode")]ushort StatusCode = 0,
	[property: Description("@#lineNumber")]uint LineNumber = 0,
	[property: Description("@#columnNumber")]uint ColumnNumber = 0): EventInit;

/// <summary>
/// SecurityPolicyViolationEvent
/// </summary>
[ECMAScript]
[Description("@#SecurityPolicyViolationEvent")]
public class SecurityPolicyViolationEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern SecurityPolicyViolationEvent(string type, SecurityPolicyViolationEventInit eventInitDict);

    /// <summary>
    /// documentURI
    /// </summary>
    [Description("@#documentURI")]
    public extern string DocumentURI { get; }

    /// <summary>
    /// referrer
    /// </summary>
    [Description("@#referrer")]
    public extern string Referrer { get; }

    /// <summary>
    /// blockedURI
    /// </summary>
    [Description("@#blockedURI")]
    public extern string BlockedURI { get; }

    /// <summary>
    /// effectiveDirective
    /// </summary>
    [Description("@#effectiveDirective")]
    public extern string EffectiveDirective { get; }

    /// <summary>
    /// violatedDirective
    /// </summary>
    [Description("@#violatedDirective")]
    public extern string ViolatedDirective { get; }

    /// <summary>
    /// originalPolicy
    /// </summary>
    [Description("@#originalPolicy")]
    public extern string OriginalPolicy { get; }

    /// <summary>
    /// sourceFile
    /// </summary>
    [Description("@#sourceFile")]
    public extern string SourceFile { get; }

    /// <summary>
    /// sample
    /// </summary>
    [Description("@#sample")]
    public extern string Sample { get; }

    /// <summary>
    /// disposition
    /// </summary>
    [Description("@#disposition")]
    public extern SecurityPolicyViolationEventDisposition Disposition { get; }

    /// <summary>
    /// statusCode
    /// </summary>
    [Description("@#statusCode")]
    public extern ushort StatusCode { get; }

    /// <summary>
    /// lineNumber
    /// </summary>
    [Description("@#lineNumber")]
    public extern uint LineNumber { get; }

    /// <summary>
    /// columnNumber
    /// </summary>
    [Description("@#columnNumber")]
    public extern uint ColumnNumber { get; }
}