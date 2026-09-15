namespace ECMAScript.CSS;

/// <summary>
/// The CSS interface holds useful CSS-related methods. No objects with this interface are implemented: it contains only static methods and is therefore a utilitarian interface.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSS">MDN Web Docs: CSS</see>
/// </remarks>
[ECMAScript]
[Description("@#CSS")]
public static partial class CSS
{
    /// <summary>
    /// JavaScript 属性 CSS.animationWorklet：Worklet。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-animationworklet-1/#dom-css-animationworklet">CSS Animation Worklet API: 2 Animation Worklet</see>
    /// </remarks>
    [Description("@#animationWorklet")]
    public static extern Worklet AnimationWorklet { get; }

    /// <summary>
    /// The CSS.supports() static method returns a boolean value indicating if the browser supports a given CSS feature, or not.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSS/supports_static">MDN Web Docs: CSS.supports</see>
    /// </remarks>
    /// <param name="property"><see href="https://drafts.csswg.org/css-conditional-3/#dom-css-supports-property-value-property">CSS Conditional Rules Module Level 3: 7.5 The CSS namespace, and the supports() function</see></param>
    /// <param name="value">A string containing the value of the CSS property to check. <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSS/supports_static">MDN Web Docs: value</see></param>
    /// <returns>true if the browser supports the rule, otherwise false.</returns>
    [Description("@#supports")]
    public static extern bool Supports(string property, string value);

    /// <summary>
    /// The CSS.supports() static method returns a boolean value indicating if the browser supports a given CSS feature, or not.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSS/supports_static">MDN Web Docs: CSS.supports</see>
    /// </remarks>
    /// <param name="conditionText"><see href="https://drafts.csswg.org/css-conditional-3/#dom-css-supports-conditiontext-conditiontext">CSS Conditional Rules Module Level 3: 7.5 The CSS namespace, and the supports() function</see></param>
    /// <returns>true if the browser supports the rule, otherwise false.</returns>
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
    /// JavaScript 属性 CSS.elementSources：any。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.csswg.org/css-images-4/#dom-css-elementsources">CSS Images Module Level 4: 2.7.2 Using Out-Of-Document Sources: the ElementSources interface</see>
    /// </remarks>
    [Description("@#elementSources")]
    public static extern object ElementSources { get; }

    /// <summary>
    /// JavaScript 属性 CSS.layoutWorklet：Worklet。只读；由宿主 API 提供当前值。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-css-layoutworklet">CSS Layout API Level 1: 3 Layout Worklet</see>
    /// </remarks>
    [Description("@#layoutWorklet")]
    public static extern Worklet LayoutWorklet { get; }

    /// <summary>
    /// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The static, read-only paintWorklet property of the CSS interface provides access to the paint worklet, which programmatically generates an image where a CSS property expects a file.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSS/paintWorklet_static">MDN Web Docs: CSS.paintWorklet</see>
    /// </remarks>
    [Description("@#paintWorklet")]
    public static extern Worklet PaintWorklet { get; }

    /// <summary>
    /// JavaScript CSS.parseStylesheet(css, options) 的强类型绑定，WebIDL 返回类型为 Promise&lt;CSSParserRule&gt;。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsestylesheet">CSS Parser API: 2 Parsing API</see>
    /// </remarks>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsestylesheet-css-options-css">CSS Parser API: 2 Parsing API</see></param>
    /// <param name="options"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsestylesheet-css-options-options">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseStylesheet")]
    public static extern PromiseResult<CSSParserRule[]> ParseStylesheet(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// JavaScript CSS.parseRuleList(css, options) 的强类型绑定，WebIDL 返回类型为 Promise&lt;CSSParserRule&gt;。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parserulelist">CSS Parser API: 2 Parsing API</see>
    /// </remarks>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parserulelist-css-options-css">CSS Parser API: 2 Parsing API</see></param>
    /// <param name="options"><see href="https://wicg.github.io/css-parser-api/#dom-css-parserulelist-css-options-options">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseRuleList")]
    public static extern PromiseResult<CSSParserRule[]> ParseRuleList(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// JavaScript CSS.parseRule(css, options) 的强类型绑定，WebIDL 返回类型为 Promise&lt;CSSParserRule&gt;。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parserule">CSS Parser API: 2 Parsing API</see>
    /// </remarks>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parserule-css-options-css">CSS Parser API: 2 Parsing API</see></param>
    /// <param name="options"><see href="https://wicg.github.io/css-parser-api/#dom-css-parserule-css-options-options">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseRule")]
    public static extern PromiseResult<CSSParserRule> ParseRule(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// JavaScript CSS.parseDeclarationList(css, options) 的强类型绑定，WebIDL 返回类型为 Promise&lt;CSSParserRule&gt;。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclarationlist">CSS Parser API: 2 Parsing API</see>
    /// </remarks>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclarationlist-css-options-css">CSS Parser API: 2 Parsing API</see></param>
    /// <param name="options"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclarationlist-css-options-options">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseDeclarationList")]
    public static extern PromiseResult<CSSParserRule[]> ParseDeclarationList(CSSStringSource css, CSSParserOptions? options = default);

    /// <summary>
    /// JavaScript CSS.parseDeclaration(css, options) 的强类型绑定，WebIDL 返回类型为 CSSParserDeclaration。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclaration">CSS Parser API: 2 Parsing API</see>
    /// </remarks>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclaration-css-options-css">CSS Parser API: 2 Parsing API</see></param>
    /// <param name="options"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsedeclaration-css-options-options">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseDeclaration")]
    public static extern CSSParserDeclaration ParseDeclaration(string css, CSSParserOptions? options = default);

    /// <summary>
    /// JavaScript CSS.parseValue(css) 的强类型绑定，WebIDL 返回类型为 CSSToken。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsevalue">CSS Parser API: 2 Parsing API</see>
    /// </remarks>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsevalue-css-css">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseValue")]
    public static extern CSSToken ParseValue(string css);

    /// <summary>
    /// JavaScript CSS.parseValueList(css) 的强类型绑定，WebIDL 返回类型为 sequence&lt;CSSToken&gt;。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsevaluelist">CSS Parser API: 2 Parsing API</see>
    /// </remarks>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsevaluelist-css-css">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseValueList")]
    public static extern CSSToken[] ParseValueList(string css);

    /// <summary>
    /// JavaScript CSS.parseCommaValueList(css) 的强类型绑定，WebIDL 返回类型为 sequence&lt;CSSToken&gt;。
    /// </summary>
    /// <remarks>
    /// <see href="https://wicg.github.io/css-parser-api/#dom-css-parsecommavaluelist">CSS Parser API: 2 Parsing API</see>
    /// </remarks>
    /// <param name="css"><see href="https://wicg.github.io/css-parser-api/#dom-css-parsecommavaluelist-css-css">CSS Parser API: 2 Parsing API</see></param>
    [Description("@#parseCommaValueList")]
    public static extern CSSToken[][] ParseCommaValueList(string css);

    /// <summary>
    /// The CSS.registerProperty() static method registers custom properties, allowing for property type checking, default values, and properties that do or do not inherit their value. Registering a custom property allows you to tell the browser how the custom property should behave; what types are allowed, whether the custom property inherits its value, and what the default value of the custom property is.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/CSS/registerProperty_static">MDN Web Docs: CSS.registerProperty</see>
    /// </remarks>
    /// <param name="definition"><see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-css-registerproperty-definition-definition">CSS Properties and Values API Level 1: 4 Registering Custom Properties in JS</see></param>
    [Description("@#registerProperty")]
    public static extern void RegisterProperty(PropertyDefinition definition);

    /// <summary>
    /// JavaScript CSS.number(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-number">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-number-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#number")]
    public static extern CSSUnitValue Number(double value);

    /// <summary>
    /// JavaScript CSS.percent(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-percent">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-percent-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#percent")]
    public static extern CSSUnitValue Percent(double value);

    /// <summary>
    /// JavaScript CSS.cap(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cap">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cap-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cap")]
    public static extern CSSUnitValue Cap(double value);

    /// <summary>
    /// JavaScript CSS.ch(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ch">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ch-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#ch")]
    public static extern CSSUnitValue Ch(double value);

    /// <summary>
    /// JavaScript CSS.em(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-em">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-em-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#em")]
    public static extern CSSUnitValue Em(double value);

    /// <summary>
    /// JavaScript CSS.ex(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ex">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ex-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#ex")]
    public static extern CSSUnitValue Ex(double value);

    /// <summary>
    /// JavaScript CSS.ic(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ic">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ic-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#ic")]
    public static extern CSSUnitValue Ic(double value);

    /// <summary>
    /// JavaScript CSS.lh(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lh")]
    public static extern CSSUnitValue Lh(double value);

    /// <summary>
    /// JavaScript CSS.rcap(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rcap">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rcap-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rcap")]
    public static extern CSSUnitValue Rcap(double value);

    /// <summary>
    /// JavaScript CSS.rch(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rch">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rch-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rch")]
    public static extern CSSUnitValue Rch(double value);

    /// <summary>
    /// JavaScript CSS.rem(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rem">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rem-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rem")]
    public static extern CSSUnitValue Rem(double value);

    /// <summary>
    /// JavaScript CSS.rex(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rex">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rex-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rex")]
    public static extern CSSUnitValue Rex(double value);

    /// <summary>
    /// JavaScript CSS.ric(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ric">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ric-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#ric")]
    public static extern CSSUnitValue Ric(double value);

    /// <summary>
    /// JavaScript CSS.rlh(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rlh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rlh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rlh")]
    public static extern CSSUnitValue Rlh(double value);

    /// <summary>
    /// JavaScript CSS.vw(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vw">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vw-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vw")]
    public static extern CSSUnitValue Vw(double value);

    /// <summary>
    /// JavaScript CSS.vh(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vh")]
    public static extern CSSUnitValue Vh(double value);

    /// <summary>
    /// JavaScript CSS.vi(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vi")]
    public static extern CSSUnitValue Vi(double value);

    /// <summary>
    /// JavaScript CSS.vb(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vb">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vb-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vb")]
    public static extern CSSUnitValue Vb(double value);

    /// <summary>
    /// JavaScript CSS.vmin(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vmin">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vmin-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vmin")]
    public static extern CSSUnitValue Vmin(double value);

    /// <summary>
    /// JavaScript CSS.vmax(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vmax">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-vmax-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#vmax")]
    public static extern CSSUnitValue Vmax(double value);

    /// <summary>
    /// JavaScript CSS.svw(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svw">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svw-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svw")]
    public static extern CSSUnitValue Svw(double value);

    /// <summary>
    /// JavaScript CSS.svh(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svh")]
    public static extern CSSUnitValue Svh(double value);

    /// <summary>
    /// JavaScript CSS.svi(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svi")]
    public static extern CSSUnitValue Svi(double value);

    /// <summary>
    /// JavaScript CSS.svb(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svb">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svb-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svb")]
    public static extern CSSUnitValue Svb(double value);

    /// <summary>
    /// JavaScript CSS.svmin(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svmin">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svmin-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svmin")]
    public static extern CSSUnitValue Svmin(double value);

    /// <summary>
    /// JavaScript CSS.svmax(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svmax">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-svmax-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#svmax")]
    public static extern CSSUnitValue Svmax(double value);

    /// <summary>
    /// JavaScript CSS.lvw(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvw">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvw-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvw")]
    public static extern CSSUnitValue Lvw(double value);

    /// <summary>
    /// JavaScript CSS.lvh(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvh")]
    public static extern CSSUnitValue Lvh(double value);

    /// <summary>
    /// JavaScript CSS.lvi(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvi")]
    public static extern CSSUnitValue Lvi(double value);

    /// <summary>
    /// JavaScript CSS.lvb(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvb">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvb-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvb")]
    public static extern CSSUnitValue Lvb(double value);

    /// <summary>
    /// JavaScript CSS.lvmin(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvmin">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvmin-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvmin")]
    public static extern CSSUnitValue Lvmin(double value);

    /// <summary>
    /// JavaScript CSS.lvmax(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvmax">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-lvmax-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#lvmax")]
    public static extern CSSUnitValue Lvmax(double value);

    /// <summary>
    /// JavaScript CSS.dvw(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvw">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvw-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvw")]
    public static extern CSSUnitValue Dvw(double value);

    /// <summary>
    /// JavaScript CSS.dvh(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvh")]
    public static extern CSSUnitValue Dvh(double value);

    /// <summary>
    /// JavaScript CSS.dvi(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvi")]
    public static extern CSSUnitValue Dvi(double value);

    /// <summary>
    /// JavaScript CSS.dvb(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvb">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvb-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvb")]
    public static extern CSSUnitValue Dvb(double value);

    /// <summary>
    /// JavaScript CSS.dvmin(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvmin">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvmin-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvmin")]
    public static extern CSSUnitValue Dvmin(double value);

    /// <summary>
    /// JavaScript CSS.dvmax(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvmax">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dvmax-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dvmax")]
    public static extern CSSUnitValue Dvmax(double value);

    /// <summary>
    /// JavaScript CSS.cqw(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqw">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqw-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqw")]
    public static extern CSSUnitValue Cqw(double value);

    /// <summary>
    /// JavaScript CSS.cqh(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqh">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqh-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqh")]
    public static extern CSSUnitValue Cqh(double value);

    /// <summary>
    /// JavaScript CSS.cqi(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqi")]
    public static extern CSSUnitValue Cqi(double value);

    /// <summary>
    /// JavaScript CSS.cqb(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqb">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqb-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqb")]
    public static extern CSSUnitValue Cqb(double value);

    /// <summary>
    /// JavaScript CSS.cqmin(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqmin">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqmin-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqmin")]
    public static extern CSSUnitValue Cqmin(double value);

    /// <summary>
    /// JavaScript CSS.cqmax(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqmax">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cqmax-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cqmax")]
    public static extern CSSUnitValue Cqmax(double value);

    /// <summary>
    /// JavaScript CSS.cm(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cm">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-cm-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#cm")]
    public static extern CSSUnitValue Cm(double value);

    /// <summary>
    /// JavaScript CSS.mm(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-mm">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-mm-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#mm")]
    public static extern CSSUnitValue Mm(double value);

    /// <summary>
    /// JavaScript CSS.Q(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-q">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-q-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#Q")]
    public static extern CSSUnitValue Q(double value);

    /// <summary>
    /// JavaScript CSS.in(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-in">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-in-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#in")]
    public static extern CSSUnitValue In(double value);

    /// <summary>
    /// JavaScript CSS.pt(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-pt">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-pt-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#pt")]
    public static extern CSSUnitValue Pt(double value);

    /// <summary>
    /// JavaScript CSS.pc(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-pc">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-pc-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#pc")]
    public static extern CSSUnitValue Pc(double value);

    /// <summary>
    /// JavaScript CSS.px(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-px">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-px-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#px")]
    public static extern CSSUnitValue Px(double value);

    /// <summary>
    /// JavaScript CSS.deg(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-deg">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-deg-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#deg")]
    public static extern CSSUnitValue Deg(double value);

    /// <summary>
    /// JavaScript CSS.grad(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-grad">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-grad-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#grad")]
    public static extern CSSUnitValue Grad(double value);

    /// <summary>
    /// JavaScript CSS.rad(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rad">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-rad-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#rad")]
    public static extern CSSUnitValue Rad(double value);

    /// <summary>
    /// JavaScript CSS.turn(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-turn">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-turn-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#turn")]
    public static extern CSSUnitValue Turn(double value);

    /// <summary>
    /// JavaScript CSS.s(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-s">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-s-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#s")]
    public static extern CSSUnitValue S(double value);

    /// <summary>
    /// JavaScript CSS.ms(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ms">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-ms-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#ms")]
    public static extern CSSUnitValue Ms(double value);

    /// <summary>
    /// JavaScript CSS.Hz(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-hz">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-hz-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#Hz")]
    public static extern CSSUnitValue Hz(double value);

    /// <summary>
    /// JavaScript CSS.kHz(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-khz">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-khz-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#kHz")]
    public static extern CSSUnitValue KHz(double value);

    /// <summary>
    /// JavaScript CSS.dpi(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dpi">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dpi-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dpi")]
    public static extern CSSUnitValue Dpi(double value);

    /// <summary>
    /// JavaScript CSS.dpcm(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dpcm">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dpcm-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dpcm")]
    public static extern CSSUnitValue Dpcm(double value);

    /// <summary>
    /// JavaScript CSS.dppx(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dppx">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
    /// <param name="value"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-dppx-value-value">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see></param>
    [Description("@#dppx")]
    public static extern CSSUnitValue Dppx(double value);

    /// <summary>
    /// JavaScript CSS.fr(value) 的强类型绑定，WebIDL 返回类型为 CSSUnitValue。
    /// </summary>
    /// <remarks>
    /// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-css-fr">CSS Typed OM Level 1: 4.3.5 Numeric Factory Functions</see>
    /// </remarks>
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