namespace ECMAScript.CSS;

/// <summary>
/// BlockFragmentationType
/// </summary>
[Description("@#BlockFragmentationType")]
[ECMAScript]
[String]
public enum BlockFragmentationType
{
    [Description("@#none")]
    None = 0,

    [Description("@#page")]
    Page = 1,

    [Description("@#column")]
    Column = 2,

    [Description("@#region")]
    Region = 3
}

/// <summary>
/// BreakType
/// </summary>
[Description("@#BreakType")]
[ECMAScript]
[String]
public enum BreakType
{
    [Description("@#none")]
    None = 0,

    [Description("@#line")]
    Line = 1,

    [Description("@#column")]
    Column = 2,

    [Description("@#page")]
    Page = 3,

    [Description("@#region")]
    Region = 4
}

/// <summary>
/// CSSMathOperator
/// </summary>
[Description("@#CSSMathOperator")]
[ECMAScript]
[String]
public enum CSSMathOperator
{
    [Description("@#sum")]
    Sum = 0,

    [Description("@#product")]
    Product = 1,

    [Description("@#negate")]
    Negate = 2,

    [Description("@#invert")]
    Invert = 3,

    [Description("@#min")]
    Min = 4,

    [Description("@#max")]
    Max = 5,

    [Description("@#clamp")]
    Clamp = 6
}

/// <summary>
/// CSSNumericBaseType
/// </summary>
[Description("@#CSSNumericBaseType")]
[ECMAScript]
[String]
public enum CSSNumericBaseType
{
    [Description("@#length")]
    Length = 0,

    [Description("@#angle")]
    Angle = 1,

    [Description("@#time")]
    Time = 2,

    [Description("@#frequency")]
    Frequency = 3,

    [Description("@#resolution")]
    Resolution = 4,

    [Description("@#flex")]
    Flex = 5,

    [Description("@#percent")]
    Percent = 6
}

/// <summary>
/// ChildDisplayType
/// </summary>
[Description("@#ChildDisplayType")]
[ECMAScript]
[String]
public enum ChildDisplayType
{
    [Description("@#block")]
    Block = 0,

    [Description("@#normal")]
    Normal = 1
}

/// <summary>
/// HighlightType
/// </summary>
[Description("@#HighlightType")]
[ECMAScript]
[String]
public enum HighlightType
{
    [Description("@#highlight")]
    Highlight = 0,

    [Description("@#spelling-error")]
    SpellingError = 1,

    [Description("@#grammar-error")]
    GrammarError = 2
}

/// <summary>
/// LayoutSizingMode
/// </summary>
[Description("@#LayoutSizingMode")]
[ECMAScript]
[String]
public enum LayoutSizingMode
{
    [Description("@#block-like")]
    BlockLike = 0,

    [Description("@#manual")]
    Manual = 1
}
