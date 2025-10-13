namespace ECMAScript;

/// <summary>
/// CSSNestedDeclarations
/// </summary>
[ECMAScript]
[Description("@#CSSNestedDeclarations")]
public class CSSNestedDeclarations : CSSRule
{
    /// <summary>
    /// style
    /// </summary>
    [Description("@#style")]
    public extern CSSStyleProperties Style { get; }
}