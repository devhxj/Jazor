namespace ECMAScript;

/// <summary>
/// CSSScopeRule
/// </summary>
[ECMAScript]
[Description("@#CSSScopeRule")]
public class CSSScopeRule : CSSGroupingRule
{
    /// <summary>
    /// start
    /// </summary>
    [Description("@#start")]
    public extern string? Start { get; }

    /// <summary>
    /// end
    /// </summary>
    [Description("@#end")]
    public extern string? End { get; }
}