namespace ECMAScript.CSS;

/// <summary>
/// HighlightType
/// </summary>
[Description("@#HighlightType")]
[ECMAScript]
public enum HighlightType
{
    [Description("@#Highlight")]
    Highlight = 0,

    [Description("@#SpellingError")]
    SpellingError = 1,

    [Description("@#GrammarError")]
    GrammarError = 2
}

/// <summary>
/// ChildDisplayType
/// </summary>
[Description("@#ChildDisplayType")]
[ECMAScript]
public enum ChildDisplayType
{
    [Description("@#Block")]
    Block = 0,

    [Description("@#Normal")]
    Normal = 1
}

/// <summary>
/// LayoutSizingMode
/// </summary>
[Description("@#LayoutSizingMode")]
[ECMAScript]
public enum LayoutSizingMode
{
    [Description("@#BlockLike")]
    BlockLike = 0,

    [Description("@#Manual")]
    Manual = 1
}

/// <summary>
/// BlockFragmentationType
/// </summary>
[Description("@#BlockFragmentationType")]
[ECMAScript]
public enum BlockFragmentationType
{
    [Description("@#None")]
    None = 0,

    [Description("@#Page")]
    Page = 1,

    [Description("@#Column")]
    Column = 2,

    [Description("@#Region")]
    Region = 3
}

/// <summary>
/// BreakType
/// </summary>
[Description("@#BreakType")]
[ECMAScript]
public enum BreakType
{
    [Description("@#None")]
    None = 0,

    [Description("@#Line")]
    Line = 1,

    [Description("@#Column")]
    Column = 2,

    [Description("@#Page")]
    Page = 3,

    [Description("@#Region")]
    Region = 4
}

/// <summary>
/// CSSNumericBaseType
/// </summary>
[Description("@#CSSNumericBaseType")]
[ECMAScript]
public enum CSSNumericBaseType
{
    [Description("@#Length")]
    Length = 0,

    [Description("@#Angle")]
    Angle = 1,

    [Description("@#Time")]
    Time = 2,

    [Description("@#Frequency")]
    Frequency = 3,

    [Description("@#Resolution")]
    Resolution = 4,

    [Description("@#Flex")]
    Flex = 5,

    [Description("@#Percent")]
    Percent = 6
}

/// <summary>
/// CSSMathOperator
/// </summary>
[Description("@#CSSMathOperator")]
[ECMAScript]
public enum CSSMathOperator
{
    [Description("@#Sum")]
    Sum = 0,

    [Description("@#Product")]
    Product = 1,

    [Description("@#Negate")]
    Negate = 2,

    [Description("@#Invert")]
    Invert = 3,

    [Description("@#Min")]
    Min = 4,

    [Description("@#Max")]
    Max = 5,

    [Description("@#Clamp")]
    Clamp = 6
}