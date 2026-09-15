namespace ECMAScript.CSS;

/// <summary>
/// WebIDL enum BlockFragmentationType。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#enumdef-blockfragmentationtype">CSS Layout API Level 1: 4.4 Layout Constraints</see>
/// </remarks>
[Description("@#BlockFragmentationType")]
[ECMAScript]
[String]
public enum BlockFragmentationType
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 BlockFragmentationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-blockfragmentationtype-none">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “page”；属于 BlockFragmentationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-blockfragmentationtype-page">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#page")]
    Page = 1,

    /// <summary>
    /// JavaScript 字符串取值 “column”；属于 BlockFragmentationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-blockfragmentationtype-column">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#column")]
    Column = 2,

    /// <summary>
    /// JavaScript 字符串取值 “region”；属于 BlockFragmentationType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-blockfragmentationtype-region">CSS Layout API Level 1: 4.4 Layout Constraints</see>
    /// </remarks>
    [Description("@#region")]
    Region = 3
}

/// <summary>
/// WebIDL enum BreakType。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#enumdef-breaktype">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
/// </remarks>
[Description("@#BreakType")]
[ECMAScript]
[String]
public enum BreakType
{
    /// <summary>
    /// JavaScript 字符串取值 “none”；属于 BreakType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktype-none">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </remarks>
    [Description("@#none")]
    None = 0,

    /// <summary>
    /// JavaScript 字符串取值 “line”；属于 BreakType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktype-line">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </remarks>
    [Description("@#line")]
    Line = 1,

    /// <summary>
    /// JavaScript 字符串取值 “column”；属于 BreakType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktype-column">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </remarks>
    [Description("@#column")]
    Column = 2,

    /// <summary>
    /// JavaScript 字符串取值 “page”；属于 BreakType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktype-page">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </remarks>
    [Description("@#page")]
    Page = 3,

    /// <summary>
    /// JavaScript 字符串取值 “region”；属于 BreakType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktype-region">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
    /// </remarks>
    [Description("@#region")]
    Region = 4
}

/// <summary>
/// WebIDL enum ChildDisplayType。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#enumdef-childdisplaytype">CSS Layout API Level 1: 3.2 Registering A Layout</see>
/// </remarks>
[Description("@#ChildDisplayType")]
[ECMAScript]
[String]
public enum ChildDisplayType
{
    /// <summary>
    /// JavaScript 字符串取值 “block”；属于 ChildDisplayType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-childdisplaytype-block">CSS Layout API Level 1: 3.2 Registering A Layout</see>
    /// </remarks>
    [Description("@#block")]
    Block = 0,

    /// <summary>
    /// JavaScript 字符串取值 “normal”；属于 ChildDisplayType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-childdisplaytype-normal">CSS Layout API Level 1: 3.2 Registering A Layout</see>
    /// </remarks>
    [Description("@#normal")]
    Normal = 1
}

/// <summary>
/// WebIDL enum LayoutSizingMode。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#enumdef-layoutsizingmode">CSS Layout API Level 1: 3.2 Registering A Layout</see>
/// </remarks>
[Description("@#LayoutSizingMode")]
[ECMAScript]
[String]
public enum LayoutSizingMode
{
    /// <summary>
    /// JavaScript 字符串取值 “block-like”；属于 LayoutSizingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutsizingmode-block-like">CSS Layout API Level 1: 3.2 Registering A Layout</see>
    /// </remarks>
    [Description("@#block-like")]
    BlockLike = 0,

    /// <summary>
    /// JavaScript 字符串取值 “manual”；属于 LayoutSizingMode 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutsizingmode-manual">CSS Layout API Level 1: 3.2 Registering A Layout</see>
    /// </remarks>
    [Description("@#manual")]
    Manual = 1
}

/// <summary>
/// WebIDL enum CSSMathOperator。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#enumdef-cssmathoperator">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
/// </remarks>
[Description("@#CSSMathOperator")]
[ECMAScript]
[String]
public enum CSSMathOperator
{
    /// <summary>
    /// JavaScript 字符串取值 “sum”；属于 CSSMathOperator 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-sum">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </remarks>
    [Description("@#sum")]
    Sum = 0,

    /// <summary>
    /// JavaScript 字符串取值 “product”；属于 CSSMathOperator 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-product">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </remarks>
    [Description("@#product")]
    Product = 1,

    /// <summary>
    /// JavaScript 字符串取值 “negate”；属于 CSSMathOperator 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-negate">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </remarks>
    [Description("@#negate")]
    Negate = 2,

    /// <summary>
    /// JavaScript 字符串取值 “invert”；属于 CSSMathOperator 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-invert">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </remarks>
    [Description("@#invert")]
    Invert = 3,

    /// <summary>
    /// JavaScript 字符串取值 “min”；属于 CSSMathOperator 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-min">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </remarks>
    [Description("@#min")]
    Min = 4,

    /// <summary>
    /// JavaScript 字符串取值 “max”；属于 CSSMathOperator 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-max">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </remarks>
    [Description("@#max")]
    Max = 5,

    /// <summary>
    /// JavaScript 字符串取值 “clamp”；属于 CSSMathOperator 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmathoperator-clamp">CSS Typed OM Level 1: 4.3.4 Complex Numeric Values: CSSMathValue objects</see>
    /// </remarks>
    [Description("@#clamp")]
    Clamp = 6
}

/// <summary>
/// WebIDL enum CSSNumericBaseType。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#enumdef-cssnumericbasetype">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
/// </remarks>
[Description("@#CSSNumericBaseType")]
[ECMAScript]
[String]
public enum CSSNumericBaseType
{
    /// <summary>
    /// JavaScript 字符串取值 “length”；属于 CSSNumericBaseType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-length">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </remarks>
    [Description("@#length")]
    Length = 0,

    /// <summary>
    /// JavaScript 字符串取值 “angle”；属于 CSSNumericBaseType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-angle">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </remarks>
    [Description("@#angle")]
    Angle = 1,

    /// <summary>
    /// JavaScript 字符串取值 “time”；属于 CSSNumericBaseType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-time">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </remarks>
    [Description("@#time")]
    Time = 2,

    /// <summary>
    /// JavaScript 字符串取值 “frequency”；属于 CSSNumericBaseType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-frequency">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </remarks>
    [Description("@#frequency")]
    Frequency = 3,

    /// <summary>
    /// JavaScript 字符串取值 “resolution”；属于 CSSNumericBaseType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-resolution">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </remarks>
    [Description("@#resolution")]
    Resolution = 4,

    /// <summary>
    /// JavaScript 字符串取值 “flex”；属于 CSSNumericBaseType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-flex">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </remarks>
    [Description("@#flex")]
    Flex = 5,

    /// <summary>
    /// JavaScript 字符串取值 “percent”；属于 CSSNumericBaseType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumericbasetype-percent">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
    /// </remarks>
    [Description("@#percent")]
    Percent = 6
}

/// <summary>
/// WebIDL enum HighlightType。定义于 CSS Custom Highlight API Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/css-highlight-api-1/#enumdef-highlighttype">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see>
/// </remarks>
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
    /// JavaScript 字符串取值 “spelling-error”；属于 HighlightType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlighttype-spelling-error">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see>
    /// </remarks>
    [Description("@#spelling-error")]
    SpellingError = 1,

    /// <summary>
    /// JavaScript 字符串取值 “grammar-error”；属于 HighlightType 的规范取值域。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlighttype-grammar-error">CSS Custom Highlight API Module Level 1: 3.1 Creating Custom Highlights</see>
    /// </remarks>
    [Description("@#grammar-error")]
    GrammarError = 2
}