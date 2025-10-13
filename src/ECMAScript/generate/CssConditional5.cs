namespace ECMAScript;

/// <summary>
/// CSSContainerRule
/// </summary>
[ECMAScript]
[Description("@#CSSContainerRule")]
public class CSSContainerRule : CSSConditionRule
{
    /// <summary>
    /// containerName
    /// </summary>
    [Description("@#containerName")]
    public extern string ContainerName { get; }

    /// <summary>
    /// containerQuery
    /// </summary>
    [Description("@#containerQuery")]
    public extern string ContainerQuery { get; }
}