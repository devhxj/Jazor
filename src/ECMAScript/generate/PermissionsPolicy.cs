namespace ECMAScript;

/// <summary>
/// PermissionsPolicyViolationReportBody
/// </summary>
[ECMAScript]
[Description("@#PermissionsPolicyViolationReportBody")]
public record PermissionsPolicyViolationReportBody(
    [property: Description("@#featureId")]string? FeatureId = default,
	[property: Description("@#sourceFile")]string? SourceFile = default,
	[property: Description("@#lineNumber")]int LineNumber = default,
	[property: Description("@#columnNumber")]int ColumnNumber = default,
	[property: Description("@#disposition")]string? Disposition = default,
	[property: Description("@#allowAttribute")]string? AllowAttribute = default,
	[property: Description("@#srcAttribute")]string? SrcAttribute = default): ReportBody;

/// <summary>
/// PermissionsPolicy
/// </summary>
[ECMAScript]
[Description("@#PermissionsPolicy")]
public class PermissionsPolicy
{
    /// <summary>
    /// allowsFeature
    /// </summary>
    /// <param name="feature">feature</param>
    /// <param name="origin">origin</param>
    [Description("@#allowsFeature")]
    public extern bool AllowsFeature(string feature, string origin);

    /// <summary>
    /// features
    /// </summary>
    [Description("@#features")]
    public extern string[] Features();

    /// <summary>
    /// allowedFeatures
    /// </summary>
    [Description("@#allowedFeatures")]
    public extern string[] AllowedFeatures();

    /// <summary>
    /// getAllowlistForFeature
    /// </summary>
    /// <param name="feature">feature</param>
    [Description("@#getAllowlistForFeature")]
    public extern string[] GetAllowlistForFeature(string feature);
}