namespace ECMAScript.CSS;

/// <summary>
/// Return a read-only, live CSSRuleList object representing the CSS rules.
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#namespacedef-css">CSS Object Model (CSSOM) Module Level 1: 8.1 The CSS.escape() Method</see>
/// </remarks>
[ECMAScript]
[Description("@#CSS")]
public static partial class CSS
{
    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-css-animationworklet">CSS Animation Worklet API: 2 Animation Worklet</see>
    /// </summary>
    [Description("@#animationWorklet")]
    public static extern Worklet AnimationWorklet { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-conditional-3/#dom-css-supports">CSS Conditional Rules Module Level 3: 7.5 The CSS namespace, and the supports() function</see>
    /// </summary>
    /// <param name="property"><see href="https://drafts.csswg.org/css-conditional-3/#dom-css-supports-property-value-property">CSS Conditional Rules Module Level 3: 7.5 The CSS namespace, and the supports() function</see></param>
    /// <param name="value"><see href="https://drafts.csswg.org/css-conditional-3/#dom-css-supports-property-value-value">CSS Conditional Rules Module Level 3: 7.5 The CSS namespace, and the supports() function</see></param>
    [Description("@#supports")]
    public static extern bool Supports(string property, string value);

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-conditional-3/#dom-css-supports-conditiontext">CSS Conditional Rules Module Level 3: 7.5 The CSS namespace, and the supports() function</see>
    /// </summary>
    /// <param name="conditionText"><see href="https://drafts.csswg.org/css-conditional-3/#dom-css-supports-conditiontext-conditiontext">CSS Conditional Rules Module Level 3: 7.5 The CSS namespace, and the supports() function</see></param>
    [Description("@#supports")]
    public static extern bool Supports(string conditionText);

    /// <summary>
    /// The highlight overlays of the custom highlights are below those of the built-in highlight pseudo-elements in the stacking order described in css-pseudo-4#highlight-painting.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-css-highlights">CSS Custom Highlight API Module Level 1: 3.2 Registering Custom Highlights</see>
    /// </remarks>
    [Description("@#highlights")]
    public static extern HighlightRegistry Highlights { get; }

    /// <summary>
    /// <see href="https://drafts.csswg.org/css-images-4/#dom-css-elementsources">CSS Images Module Level 4: 2.7.2 Using Out-Of-Document Sources: the ElementSources interface</see>
    /// </summary>
    [Description("@#elementSources")]
    public static extern object ElementSources { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-css-layoutworklet">CSS Layout API Level 1: 3 Layout Worklet</see>
    /// </summary>
    [Description("@#layoutWorklet")]
    public static extern Worklet LayoutWorklet { get; }

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-paint-api-1/#dom-css-paintworklet">CSS Painting API Level 1: 2 Paint Worklet</see>
    /// </summary>
    [Description("@#paintWorklet")]
    public static extern Worklet PaintWorklet { get; }

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsestylesheet">CSS Parser API: 2 Parsing API</see>
    /// </summary>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsestylesheet-css-options-css">CSS Parser API: 2 Parsing API</see></param>
    /// <param name="options"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsestylesheet-css-options-options">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseStylesheet")]
    public static extern PromiseResult<CSSParserRule[]> ParseStylesheet(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parserulelist">CSS Parser API: 2 Parsing API</see>
    /// </summary>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parserulelist-css-options-css">CSS Parser API: 2 Parsing API</see></param>
    /// <param name="options"><see href="https://wicg.github.io/css-parser-api/#dom-css-parserulelist-css-options-options">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseRuleList")]
    public static extern PromiseResult<CSSParserRule[]> ParseRuleList(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parserule">CSS Parser API: 2 Parsing API</see>
    /// </summary>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parserule-css-options-css">CSS Parser API: 2 Parsing API</see></param>
    /// <param name="options"><see href="https://wicg.github.io/css-parser-api/#dom-css-parserule-css-options-options">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseRule")]
    public static extern PromiseResult<CSSParserRule> ParseRule(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclarationlist">CSS Parser API: 2 Parsing API</see>
    /// </summary>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclarationlist-css-options-css">CSS Parser API: 2 Parsing API</see></param>
    /// <param name="options"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclarationlist-css-options-options">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseDeclarationList")]
    public static extern PromiseResult<CSSParserRule[]> ParseDeclarationList(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclaration">CSS Parser API: 2 Parsing API</see>
    /// </summary>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclaration-css-options-css">CSS Parser API: 2 Parsing API</see></param>
    /// <param name="options"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclaration-css-options-options">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseDeclaration")]
    public static extern CSSParserDeclaration ParseDeclaration(string css, CSSParserOptions? options = default);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsevalue">CSS Parser API: 2 Parsing API</see>
    /// </summary>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsevalue-css-css">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseValue")]
    public static extern CSSToken ParseValue(string css);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsevaluelist">CSS Parser API: 2 Parsing API</see>
    /// </summary>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsevaluelist-css-css">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseValueList")]
    public static extern CSSToken[] ParseValueList(string css);

    /// <summary>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsecommavaluelist">CSS Parser API: 2 Parsing API</see>
    /// </summary>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsecommavaluelist-css-css">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseCommaValueList")]
    public static extern CSSToken[][] ParseCommaValueList(string css);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-css-registerproperty">CSS Properties and Values API Level 1: 4.1 The registerProperty() Function</see>
    /// </summary>
    /// <param name="definition"><see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-css-registerproperty-definition-definition">CSS Properties and Values API Level 1: 4 Registering Custom Properties in JS</see></param>
    [Description("@#registerProperty")]
    public static extern void RegisterProperty(PropertyDefinition definition);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-number">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-number-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#number")]
    public static extern CSSUnitValue Number(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-percent">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-percent-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#percent")]
    public static extern CSSUnitValue Percent(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cap">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cap-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cap")]
    public static extern CSSUnitValue Cap(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ch">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ch-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#ch")]
    public static extern CSSUnitValue Ch(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-em">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-em-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#em")]
    public static extern CSSUnitValue Em(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ex">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ex-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#ex")]
    public static extern CSSUnitValue Ex(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ic">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ic-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#ic")]
    public static extern CSSUnitValue Ic(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lh")]
    public static extern CSSUnitValue Lh(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rcap">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rcap-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rcap")]
    public static extern CSSUnitValue Rcap(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rch">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rch-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rch")]
    public static extern CSSUnitValue Rch(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rem">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rem-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rem")]
    public static extern CSSUnitValue Rem(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rex">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rex-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rex")]
    public static extern CSSUnitValue Rex(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ric">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ric-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#ric")]
    public static extern CSSUnitValue Ric(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rlh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rlh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rlh")]
    public static extern CSSUnitValue Rlh(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vw">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vw-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vw")]
    public static extern CSSUnitValue Vw(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vh")]
    public static extern CSSUnitValue Vh(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vi")]
    public static extern CSSUnitValue Vi(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vb">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vb-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vb")]
    public static extern CSSUnitValue Vb(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vmin">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vmin-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vmin")]
    public static extern CSSUnitValue Vmin(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vmax">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vmax-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vmax")]
    public static extern CSSUnitValue Vmax(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svw">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svw-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svw")]
    public static extern CSSUnitValue Svw(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svh")]
    public static extern CSSUnitValue Svh(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svi")]
    public static extern CSSUnitValue Svi(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svb">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svb-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svb")]
    public static extern CSSUnitValue Svb(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svmin">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svmin-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svmin")]
    public static extern CSSUnitValue Svmin(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svmax">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svmax-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svmax")]
    public static extern CSSUnitValue Svmax(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvw">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvw-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvw")]
    public static extern CSSUnitValue Lvw(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvh")]
    public static extern CSSUnitValue Lvh(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvi")]
    public static extern CSSUnitValue Lvi(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvb">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvb-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvb")]
    public static extern CSSUnitValue Lvb(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvmin">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvmin-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvmin")]
    public static extern CSSUnitValue Lvmin(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvmax">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvmax-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvmax")]
    public static extern CSSUnitValue Lvmax(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvw">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvw-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvw")]
    public static extern CSSUnitValue Dvw(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvh")]
    public static extern CSSUnitValue Dvh(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvi")]
    public static extern CSSUnitValue Dvi(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvb">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvb-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvb")]
    public static extern CSSUnitValue Dvb(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvmin">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvmin-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvmin")]
    public static extern CSSUnitValue Dvmin(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvmax">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvmax-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvmax")]
    public static extern CSSUnitValue Dvmax(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqw">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqw-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqw")]
    public static extern CSSUnitValue Cqw(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqh")]
    public static extern CSSUnitValue Cqh(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqi")]
    public static extern CSSUnitValue Cqi(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqb">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqb-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqb")]
    public static extern CSSUnitValue Cqb(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqmin">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqmin-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqmin")]
    public static extern CSSUnitValue Cqmin(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqmax">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqmax-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqmax")]
    public static extern CSSUnitValue Cqmax(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cm">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cm-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cm")]
    public static extern CSSUnitValue Cm(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-mm">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-mm-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#mm")]
    public static extern CSSUnitValue Mm(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-q">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-q-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#Q")]
    public static extern CSSUnitValue Q(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-in">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-in-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#in")]
    public static extern CSSUnitValue In(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-pt">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-pt-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#pt")]
    public static extern CSSUnitValue Pt(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-pc">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-pc-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#pc")]
    public static extern CSSUnitValue Pc(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-px">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-px-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#px")]
    public static extern CSSUnitValue Px(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-deg">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-deg-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#deg")]
    public static extern CSSUnitValue Deg(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-grad">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-grad-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#grad")]
    public static extern CSSUnitValue Grad(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rad">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rad-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rad")]
    public static extern CSSUnitValue Rad(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-turn">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-turn-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#turn")]
    public static extern CSSUnitValue Turn(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-s">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-s-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#s")]
    public static extern CSSUnitValue S(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ms">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ms-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#ms")]
    public static extern CSSUnitValue Ms(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-hz">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-hz-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#Hz")]
    public static extern CSSUnitValue Hz(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-khz">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-khz-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#kHz")]
    public static extern CSSUnitValue KHz(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dpi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dpi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dpi")]
    public static extern CSSUnitValue Dpi(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dpcm">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dpcm-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dpcm")]
    public static extern CSSUnitValue Dpcm(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dppx">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dppx-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dppx")]
    public static extern CSSUnitValue Dppx(double value);

    /// <summary>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-fr">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </summary>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-fr-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#fr")]
    public static extern CSSUnitValue Fr(double value);

    /// <summary>
    /// The CSS.escape() static method is introduced.
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/cssom-1/#dom-css-escape">CSS Object Model (CSSOM) Module Level 1: 8.1 The CSS.escape() Method</see>
    /// </remarks>
    /// <param name="ident"><see href="https://drafts.csswg.org/cssom-1/#dom-css-escape-ident-ident">CSS Object Model (CSSOM) Module Level 1: 8.1 The CSS.escape() Method</see></param>
    [Description("@#escape")]
    public static extern string Escape(string ident);
}
