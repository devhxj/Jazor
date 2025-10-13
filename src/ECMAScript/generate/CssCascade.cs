namespace ECMAScript;

/// <summary>
/// CSSLayerBlockRule
/// </summary>
[ECMAScript]
[Description("@#CSSLayerBlockRule")]
public class CSSLayerBlockRule : CSSGroupingRule
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }
}

/// <summary>
/// CSSLayerStatementRule
/// </summary>
[ECMAScript]
[Description("@#CSSLayerStatementRule")]
public class CSSLayerStatementRule : CSSRule
{
    /// <summary>
    /// nameList
    /// </summary>
    [Description("@#nameList")]
    public extern FrozenSet<string> NameList { get; }
}