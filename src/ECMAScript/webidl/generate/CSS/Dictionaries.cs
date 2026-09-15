namespace ECMAScript.CSS;

/// <summary>
/// WebIDL dictionary BreakTokenOptions。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dictdef-breaktokenoptions">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
/// </remarks>
/// <param name="ChildBreakTokens">BreakTokenOptions 字典中的 childBreakTokens 成员，WebIDL 类型为 sequence&lt;ChildBreakToken&gt;。可省略。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktokenoptions-childbreaktokens">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see></param>
/// <param name="Data">BreakTokenOptions 字典中的 data 成员，WebIDL 类型为 any。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;null&quot;&#10;                }。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktokenoptions-data">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see></param>
[ECMAScript]
[Description("@#BreakTokenOptions")]
public record BreakTokenOptions(
    [property: Description("@#childBreakTokens")]ChildBreakToken[]? ChildBreakTokens = default,
    [property: Description("@#data")]object? Data = default);

/// <summary>
/// WebIDL dictionary FragmentResultOptions。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dictdef-fragmentresultoptions">CSS Layout API Level 1: 6.2 Performing Layout</see>
/// </remarks>
/// <param name="InlineSize">FragmentResultOptions 字典中的 inlineSize 成员，WebIDL 类型为 double。可省略。WebIDL 默认值：0。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-inlinesize">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="BlockSize">FragmentResultOptions 字典中的 blockSize 成员，WebIDL 类型为 double。可省略。WebIDL 默认值：0。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-blocksize">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="AutoBlockSize">FragmentResultOptions 字典中的 autoBlockSize 成员，WebIDL 类型为 double。可省略。WebIDL 默认值：0。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-autoblocksize">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="ChildFragments">FragmentResultOptions 字典中的 childFragments 成员，WebIDL 类型为 sequence&lt;LayoutFragment&gt;。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;sequence&quot;,&#10;                  &quot;value&quot;: []&#10;                }。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-childfragments">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="Data">FragmentResultOptions 字典中的 data 成员，WebIDL 类型为 any。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;null&quot;&#10;                }。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-data">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="BreakToken">FragmentResultOptions 字典中的 breakToken 成员，WebIDL 类型为 BreakTokenOptions。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;null&quot;&#10;                }。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-breaktoken">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
[ECMAScript]
[Description("@#FragmentResultOptions")]
public record FragmentResultOptions(
    [property: Description("@#inlineSize")]double InlineSize = 0d,
    [property: Description("@#blockSize")]double BlockSize = 0d,
    [property: Description("@#autoBlockSize")]double AutoBlockSize = 0d,
    [property: Description("@#childFragments")]LayoutFragment[]? ChildFragments = default,
    [property: Description("@#data")]object? Data = default,
    [property: Description("@#breakToken")]BreakTokenOptions? BreakToken = default);

/// <summary>
/// WebIDL dictionary IntrinsicSizesResultOptions。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dictdef-intrinsicsizesresultoptions">CSS Layout API Level 1: 6.2 Performing Layout</see>
/// </remarks>
/// <param name="MaxContentSize">IntrinsicSizesResultOptions 字典中的 maxContentSize 成员，WebIDL 类型为 double。可省略。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-intrinsicsizesresultoptions-maxcontentsize">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="MinContentSize">IntrinsicSizesResultOptions 字典中的 minContentSize 成员，WebIDL 类型为 double。可省略。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-intrinsicsizesresultoptions-mincontentsize">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
[ECMAScript]
[Description("@#IntrinsicSizesResultOptions")]
public record IntrinsicSizesResultOptions(
    [property: Description("@#maxContentSize")]double MaxContentSize = default,
    [property: Description("@#minContentSize")]double MinContentSize = default);

/// <summary>
/// WebIDL dictionary LayoutConstraintsOptions。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dictdef-layoutconstraintsoptions">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see>
/// </remarks>
/// <param name="AvailableInlineSize">LayoutConstraintsOptions 字典中的 availableInlineSize 成员，WebIDL 类型为 double。可省略。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-availableinlinesize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="AvailableBlockSize">LayoutConstraintsOptions 字典中的 availableBlockSize 成员，WebIDL 类型为 double。可省略。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-availableblocksize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="FixedInlineSize">LayoutConstraintsOptions 字典中的 fixedInlineSize 成员，WebIDL 类型为 double。可省略。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-fixedinlinesize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="FixedBlockSize">LayoutConstraintsOptions 字典中的 fixedBlockSize 成员，WebIDL 类型为 double。可省略。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-fixedblocksize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="PercentageInlineSize">LayoutConstraintsOptions 字典中的 percentageInlineSize 成员，WebIDL 类型为 double。可省略。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-percentageinlinesize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="PercentageBlockSize">LayoutConstraintsOptions 字典中的 percentageBlockSize 成员，WebIDL 类型为 double。可省略。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-percentageblocksize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="BlockFragmentationOffset">LayoutConstraintsOptions 字典中的 blockFragmentationOffset 成员，WebIDL 类型为 double。可省略。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-blockfragmentationoffset">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="BlockFragmentationType">LayoutConstraintsOptions 字典中的 blockFragmentationType 成员，WebIDL 类型为 BlockFragmentationType。可省略。WebIDL 默认值：none。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-blockfragmentationtype">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="Data">LayoutConstraintsOptions 字典中的 data 成员，WebIDL 类型为 any。可省略。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-data">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
[ECMAScript]
[Description("@#LayoutConstraintsOptions")]
public record LayoutConstraintsOptions(
    [property: Description("@#availableInlineSize")]double AvailableInlineSize = default,
    [property: Description("@#availableBlockSize")]double AvailableBlockSize = default,
    [property: Description("@#fixedInlineSize")]double FixedInlineSize = default,
    [property: Description("@#fixedBlockSize")]double FixedBlockSize = default,
    [property: Description("@#percentageInlineSize")]double PercentageInlineSize = default,
    [property: Description("@#percentageBlockSize")]double PercentageBlockSize = default,
    [property: Description("@#blockFragmentationOffset")]double BlockFragmentationOffset = default,
    [property: Description("@#blockFragmentationType")]BlockFragmentationType BlockFragmentationType = BlockFragmentationType.None,
    [property: Description("@#data")]object? Data = default);

/// <summary>
/// WebIDL dictionary LayoutOptions。定义于 CSS Layout API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dictdef-layoutoptions">CSS Layout API Level 1: 3.2 Registering A Layout</see>
/// </remarks>
/// <param name="ChildDisplay">LayoutOptions 字典中的 childDisplay 成员，WebIDL 类型为 ChildDisplayType。可省略。WebIDL 默认值：block。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutoptions-childdisplay">CSS Layout API Level 1: 3.2 Registering A Layout</see></param>
/// <param name="Sizing">LayoutOptions 字典中的 sizing 成员，WebIDL 类型为 LayoutSizingMode。可省略。WebIDL 默认值：block-like。 <see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutoptions-sizing">CSS Layout API Level 1: 3.2 Registering A Layout</see></param>
[ECMAScript]
[Description("@#LayoutOptions")]
public record LayoutOptions(
    [property: Description("@#childDisplay")]ChildDisplayType ChildDisplay = ChildDisplayType.Block,
    [property: Description("@#sizing")]LayoutSizingMode Sizing = LayoutSizingMode.BlockLike);

/// <summary>
/// WebIDL dictionary PaintRenderingContext2DSettings。定义于 CSS Painting API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-paint-api-1/#dictdef-paintrenderingcontext2dsettings">CSS Painting API Level 1: 2 Paint Worklet</see>
/// </remarks>
/// <param name="Alpha">PaintRenderingContext2DSettings 字典中的 alpha 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: true&#10;                }。 <see href="https://drafts.css-houdini.org/css-paint-api-1/#dom-paintrenderingcontext2dsettings-alpha">CSS Painting API Level 1: 2 Paint Worklet</see></param>
[ECMAScript]
[Description("@#PaintRenderingContext2DSettings")]
public record PaintRenderingContext2DSettings(
    [property: Description("@#alpha")]bool Alpha = false);

/// <summary>
/// WebIDL dictionary PropertyDefinition。定义于 CSS Properties and Values API Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dictdef-propertydefinition">CSS Properties and Values API Level 1: 4.2 The PropertyDefinition Dictionary</see>
/// </remarks>
/// <param name="Name">PropertyDefinition 字典中的 name 成员，WebIDL 类型为 DOMString。必须提供该成员。 <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-propertydefinition-name">CSS Properties and Values API Level 1: 4.2 The PropertyDefinition Dictionary</see></param>
/// <param name="Syntax">PropertyDefinition 字典中的 syntax 成员，WebIDL 类型为 DOMString。可省略。WebIDL 默认值：*。 <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-propertydefinition-syntax">CSS Properties and Values API Level 1: 4.2 The PropertyDefinition Dictionary</see></param>
/// <param name="Inherits">PropertyDefinition 字典中的 inherits 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: true&#10;                }。 <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-propertydefinition-inherits">CSS Properties and Values API Level 1: 4.2 The PropertyDefinition Dictionary</see></param>
/// <param name="InitialValue">PropertyDefinition 字典中的 initialValue 成员，WebIDL 类型为 DOMString。可省略。 <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-propertydefinition-initialvalue">CSS Properties and Values API Level 1: 4.2 The PropertyDefinition Dictionary</see></param>
[ECMAScript]
[Description("@#PropertyDefinition")]
public record PropertyDefinition(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#syntax")]string? Syntax = default,
    [property: Description("@#inherits")]bool Inherits = false,
    [property: Description("@#initialValue")]string? InitialValue = default);

/// <summary>
/// WebIDL dictionary CSSMatrixComponentOptions。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dictdef-cssmatrixcomponentoptions">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </remarks>
/// <param name="Is2D">CSSMatrixComponentOptions 字典中的 is2D 成员，WebIDL 类型为 boolean。可省略。 <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmatrixcomponentoptions-is2d">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
[ECMAScript]
[Description("@#CSSMatrixComponentOptions")]
public record CSSMatrixComponentOptions(
    [property: Description("@#is2D")]bool Is2D = default);

/// <summary>
/// WebIDL dictionary CSSNumericType。定义于 CSS Typed OM Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dictdef-cssnumerictype">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
/// </remarks>
/// <param name="Length">CSSNumericType 字典中的 length 成员，WebIDL 类型为 long。可省略。 <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-length">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Angle">CSSNumericType 字典中的 angle 成员，WebIDL 类型为 long。可省略。 <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-angle">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Time">CSSNumericType 字典中的 time 成员，WebIDL 类型为 long。可省略。 <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-time">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Frequency">CSSNumericType 字典中的 frequency 成员，WebIDL 类型为 long。可省略。 <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-frequency">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Resolution">CSSNumericType 字典中的 resolution 成员，WebIDL 类型为 long。可省略。 <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-resolution">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Flex">CSSNumericType 字典中的 flex 成员，WebIDL 类型为 long。可省略。 <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-flex">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Percent">CSSNumericType 字典中的 percent 成员，WebIDL 类型为 long。可省略。 <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-percent">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="PercentHint">CSSNumericType 字典中的 percentHint 成员，WebIDL 类型为 CSSNumericBaseType。可省略。 <see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-percenthint">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
[ECMAScript]
[Description("@#CSSNumericType")]
public record CSSNumericType(
    [property: Description("@#length")]int Length = default,
    [property: Description("@#angle")]int Angle = default,
    [property: Description("@#time")]int Time = default,
    [property: Description("@#frequency")]int Frequency = default,
    [property: Description("@#resolution")]int Resolution = default,
    [property: Description("@#flex")]int Flex = default,
    [property: Description("@#percent")]int Percent = default,
    [property: Description("@#percentHint")]CSSNumericBaseType? PercentHint = default);

/// <summary>
/// WebIDL dictionary HighlightHitResult。定义于 CSS Custom Highlight API Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/css-highlight-api-1/#dictdef-highlighthitresult">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see>
/// </remarks>
/// <param name="Highlight">HighlightHitResult 字典中的 highlight 成员，WebIDL 类型为 Highlight。可省略。 <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlighthitresult-highlight">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see></param>
/// <param name="Ranges">HighlightHitResult 字典中的 ranges 成员，WebIDL 类型为 sequence&lt;AbstractRange&gt;。可省略。 <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlighthitresult-ranges">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see></param>
[ECMAScript]
[Description("@#HighlightHitResult")]
public record HighlightHitResult(
    [property: Description("@#highlight")]Highlight? Highlight = default,
    [property: Description("@#ranges")]AbstractRange[]? Ranges = default);

/// <summary>
/// WebIDL dictionary HighlightsFromPointOptions。定义于 CSS Custom Highlight API Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/css-highlight-api-1/#dictdef-highlightsfrompointoptions">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see>
/// </remarks>
/// <param name="ShadowRoots">HighlightsFromPointOptions 字典中的 shadowRoots 成员，WebIDL 类型为 sequence&lt;ShadowRoot&gt;。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;sequence&quot;,&#10;                  &quot;value&quot;: []&#10;                }。 <see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlightsfrompointoptions-shadowroots">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see></param>
[ECMAScript]
[Description("@#HighlightsFromPointOptions")]
public record HighlightsFromPointOptions(
    [property: Description("@#shadowRoots")]ShadowRoot[]? ShadowRoots = default);

/// <summary>
/// WebIDL dictionary CSSStyleSheetInit。定义于 CSS Object Model (CSSOM) Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/cssom-1/#dictdef-cssstylesheetinit">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see>
/// </remarks>
/// <param name="BaseURL">Set sheet&apos;s stylesheet base URL to the baseURL attribute value from options. <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheetinit-baseurl">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see></param>
/// <param name="Media">If the media attribute of options is a string, create a MediaList object from the string and assign it as sheet&apos;s CSSStyleSheet/media. Otherwise, serialize a media query list from the attribute and then create a MediaList object from the resulting string and set it as sheet&apos;s CSSStyleSheet/media. <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheetinit-media">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see></param>
/// <param name="Disabled">If the disabled attribute of options is true, set sheet&apos;s CSSStyleSheet/disabled flag. <see href="https://drafts.csswg.org/cssom-1/#dom-cssstylesheetinit-disabled">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see></param>
[ECMAScript]
[Description("@#CSSStyleSheetInit")]
public record CSSStyleSheetInit(
    [property: Description("@#baseURL")]string? BaseURL = null,
    [property: Description("@#media")]CSSStyleSheetInitMedia? Media = default,
    [property: Description("@#disabled")]bool Disabled = false);

/// <summary>
/// WebIDL dictionary CSSParserOptions。定义于 CSS Parser API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/css-parser-api/#dictdef-cssparseroptions">CSS Parser API: 2 Parsing API</see>
/// </remarks>
/// <param name="AtRules">CSSParserOptions 字典中的 atRules 成员，WebIDL 类型为 object。可省略。 <see href="https://wicg.github.io/css-parser-api/#dom-cssparseroptions-atrules">CSS Parser API: 2 Parsing API</see></param>
[ECMAScript]
[Description("@#CSSParserOptions")]
public record CSSParserOptions(
    [property: Description("@#atRules")]object? AtRules = default);