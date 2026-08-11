namespace ECMAScript.CSS;

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#enumdef-blockfragmentationtype">CSS Layout API Level 1: 4.4 Layout Constraints</see>
/// </summary>
[Description("@#BlockFragmentationType")]
[ECMAScript]
[String]
public enum BlockFragmentationType
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-blockfragmentationtype-none">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-blockfragmentationtype-page">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#page")]
    Page = 1,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-blockfragmentationtype-column">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#column")]
    Column = 2,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-blockfragmentationtype-region">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </summary>
    [Description("@#region")]
    Region = 3
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#enumdef-breaktype">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
/// </summary>
[Description("@#BreakType")]
[ECMAScript]
[String]
public enum BreakType
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktype-none">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </summary>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktype-line">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </summary>
    [Description("@#line")]
    Line = 1,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktype-column">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </summary>
    [Description("@#column")]
    Column = 2,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktype-page">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </summary>
    [Description("@#page")]
    Page = 3,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktype-region">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </summary>
    [Description("@#region")]
    Region = 4
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#enumdef-childdisplaytype">CSS Layout API Level 1: 3.2 Registering A Layout</see>
/// </summary>
[Description("@#ChildDisplayType")]
[ECMAScript]
[String]
public enum ChildDisplayType
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-childdisplaytype-block">CSS Layout API Level 1: 3.2 Registering A Layout</see>
    /// </summary>
    [Description("@#block")]
    Block = 0,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-childdisplaytype-normal">CSS Layout API Level 1: 3.2 Registering A Layout</see>
    /// </summary>
    [Description("@#normal")]
    Normal = 1
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#enumdef-layoutsizingmode">CSS Layout API Level 1: 3.2 Registering A Layout</see>
/// </summary>
[Description("@#LayoutSizingMode")]
[ECMAScript]
[String]
public enum LayoutSizingMode
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutsizingmode-block-like">CSS Layout API Level 1: 3.2 Registering A Layout</see>
    /// </summary>
    [Description("@#block-like")]
    BlockLike = 0,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutsizingmode-manual">CSS Layout API Level 1: 3.2 Registering A Layout</see>
    /// </summary>
    [Description("@#manual")]
    Manual = 1
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#enumdef-cssmathoperator">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
/// </summary>
[Description("@#CSSMathOperator")]
[ECMAScript]
[String]
public enum CSSMathOperator
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-sum">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#sum")]
    Sum = 0,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-product">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#product")]
    Product = 1,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-negate">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#negate")]
    Negate = 2,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-invert">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#invert")]
    Invert = 3,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-min">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#min")]
    Min = 4,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-max">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#max")]
    Max = 5,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-clamp">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </summary>
    [Description("@#clamp")]
    Clamp = 6
}

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#enumdef-cssnumericbasetype">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
/// </summary>
[Description("@#CSSNumericBaseType")]
[ECMAScript]
[String]
public enum CSSNumericBaseType
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-length">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    [Description("@#length")]
    Length = 0,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-angle">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    [Description("@#angle")]
    Angle = 1,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-time">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    [Description("@#time")]
    Time = 2,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-frequency">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    [Description("@#frequency")]
    Frequency = 3,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-resolution">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    [Description("@#resolution")]
    Resolution = 4,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-flex">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    [Description("@#flex")]
    Flex = 5,

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-percent">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </summary>
    [Description("@#percent")]
    Percent = 6
}

/// <summary>
/// <see href="https://drafts.csswg.org/css-highlight-api-1/#enumdef-highlighttype">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see>
/// </summary>
[Description("@#HighlightType")]
[ECMAScript]
[String]
public enum HighlightType
{
    /// <summary>
    /// Set highlight&apos;s type to highlight.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlighttype-highlight">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see>
    /// </remarks>
    [Description("@#highlight")]
    Highlight = 0,

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlighttype-spelling-error">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see>
    /// </summary>
    [Description("@#spelling-error")]
    SpellingError = 1,

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlighttype-grammar-error">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see>
    /// </summary>
    [Description("@#grammar-error")]
    GrammarError = 2
}
