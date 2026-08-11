namespace ECMAScript.CSS;

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dictdef-breaktokenoptions">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see>
/// </summary>
/// <param name="ChildBreakTokens"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktokenoptions-childbreaktokens">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see></param>
/// <param name="Data"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-breaktokenoptions-data">CSS Layout API Level 1: 4.5 Breaking and Fragmentation</see></param>
[ECMAScript]
[Description("@#BreakTokenOptions")]
public record BreakTokenOptions(
    [property: Description("@#childBreakTokens")]ChildBreakToken[]? ChildBreakTokens = default,
    [property: Description("@#data")]object? Data = default);

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dictdef-fragmentresultoptions">CSS Layout API Level 1: 6.2 Performing Layout</see>
/// </summary>
/// <param name="InlineSize"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-inlinesize">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="BlockSize"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-blocksize">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="AutoBlockSize"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-autoblocksize">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="ChildFragments"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-childfragments">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="Data"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-data">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="BreakToken"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-fragmentresultoptions-breaktoken">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
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
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dictdef-intrinsicsizesresultoptions">CSS Layout API Level 1: 6.2 Performing Layout</see>
/// </summary>
/// <param name="MaxContentSize"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-intrinsicsizesresultoptions-maxcontentsize">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
/// <param name="MinContentSize"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-intrinsicsizesresultoptions-mincontentsize">CSS Layout API Level 1: 6.2 Performing Layout</see></param>
[ECMAScript]
[Description("@#IntrinsicSizesResultOptions")]
public record IntrinsicSizesResultOptions(
    [property: Description("@#maxContentSize")]double MaxContentSize = default,
    [property: Description("@#minContentSize")]double MinContentSize = default);

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dictdef-layoutconstraintsoptions">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see>
/// </summary>
/// <param name="AvailableInlineSize"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-availableinlinesize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="AvailableBlockSize"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-availableblocksize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="FixedInlineSize"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-fixedinlinesize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="FixedBlockSize"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-fixedblocksize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="PercentageInlineSize"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-percentageinlinesize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="PercentageBlockSize"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-percentageblocksize">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="BlockFragmentationOffset"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-blockfragmentationoffset">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="BlockFragmentationType"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-blockfragmentationtype">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
/// <param name="Data"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutconstraintsoptions-data">CSS Layout API Level 1: 4.4.1 Constraints for Layout Children</see></param>
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
/// <see href="https://drafts.css-houdini.org/css-layout-api-1/#dictdef-layoutoptions">CSS Layout API Level 1: 3.2 Registering A Layout</see>
/// </summary>
/// <param name="ChildDisplay"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutoptions-childdisplay">CSS Layout API Level 1: 3.2 Registering A Layout</see></param>
/// <param name="Sizing"><see href="https://drafts.css-houdini.org/css-layout-api-1/#dom-layoutoptions-sizing">CSS Layout API Level 1: 3.2 Registering A Layout</see></param>
[ECMAScript]
[Description("@#LayoutOptions")]
public record LayoutOptions(
    [property: Description("@#childDisplay")]ChildDisplayType ChildDisplay = ChildDisplayType.Block,
    [property: Description("@#sizing")]LayoutSizingMode Sizing = LayoutSizingMode.BlockLike);

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-paint-api-1/#dictdef-paintrenderingcontext2dsettings">CSS Painting API Level 1: 2 Paint Worklet</see>
/// </summary>
/// <param name="Alpha"><see href="https://drafts.css-houdini.org/css-paint-api-1/#dom-paintrenderingcontext2dsettings-alpha">CSS Painting API Level 1: 2 Paint Worklet</see></param>
[ECMAScript]
[Description("@#PaintRenderingContext2DSettings")]
public record PaintRenderingContext2DSettings(
    [property: Description("@#alpha")]bool Alpha = false);

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dictdef-propertydefinition">CSS Properties and Values API Level 1: 4.2 The PropertyDefinition Dictionary</see>
/// </summary>
/// <param name="Name"><see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-propertydefinition-name">CSS Properties and Values API Level 1: 4.2 The PropertyDefinition Dictionary</see></param>
/// <param name="Syntax"><see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-propertydefinition-syntax">CSS Properties and Values API Level 1: 4.2 The PropertyDefinition Dictionary</see></param>
/// <param name="Inherits"><see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-propertydefinition-inherits">CSS Properties and Values API Level 1: 4.2 The PropertyDefinition Dictionary</see></param>
/// <param name="InitialValue"><see href="https://drafts.css-houdini.org/css-properties-values-api-1/#dom-propertydefinition-initialvalue">CSS Properties and Values API Level 1: 4.2 The PropertyDefinition Dictionary</see></param>
[ECMAScript]
[Description("@#PropertyDefinition")]
public record PropertyDefinition(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#syntax")]string? Syntax = default,
    [property: Description("@#inherits")]bool Inherits = default,
    [property: Description("@#initialValue")]string? InitialValue = default);

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dictdef-cssmatrixcomponentoptions">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see>
/// </summary>
/// <param name="Is2D"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssmatrixcomponentoptions-is2d">CSS Typed OM Level 1: 4.4 CSSTransformValue objects</see></param>
[ECMAScript]
[Description("@#CSSMatrixComponentOptions")]
public record CSSMatrixComponentOptions(
    [property: Description("@#is2D")]bool Is2D = default);

/// <summary>
/// <see href="https://drafts.css-houdini.org/css-typed-om-1/#dictdef-cssnumerictype">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see>
/// </summary>
/// <param name="Length"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-length">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Angle"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-angle">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Time"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-time">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Frequency"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-frequency">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Resolution"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-resolution">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Flex"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-flex">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="Percent"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-percent">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
/// <param name="PercentHint"><see href="https://drafts.css-houdini.org/css-typed-om-1/#dom-cssnumerictype-percenthint">CSS Typed OM Level 1: 4.3.1 Common Numeric Operations, and the CSSNumericValue Superclass</see></param>
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
/// <see href="https://drafts.csswg.org/css-highlight-api-1/#dictdef-highlighthitresult">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see>
/// </summary>
/// <param name="Highlight"><see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlighthitresult-highlight">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see></param>
/// <param name="Ranges"><see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlighthitresult-ranges">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see></param>
[ECMAScript]
[Description("@#HighlightHitResult")]
public record HighlightHitResult(
    [property: Description("@#highlight")]Highlight? Highlight = default,
    [property: Description("@#ranges")]AbstractRange[]? Ranges = default);

/// <summary>
/// <see href="https://drafts.csswg.org/css-highlight-api-1/#dictdef-highlightsfrompointoptions">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see>
/// </summary>
/// <param name="ShadowRoots"><see href="https://drafts.csswg.org/css-highlight-api-1/#dom-highlightsfrompointoptions-shadowroots">CSS Custom Highlight API Module Level 1: 6 Interacting with Custom Highlights</see></param>
[ECMAScript]
[Description("@#HighlightsFromPointOptions")]
public record HighlightsFromPointOptions(
    [property: Description("@#shadowRoots")]ShadowRoot[]? ShadowRoots = default);

/// <summary>
/// <see href="https://drafts.csswg.org/cssom-1/#dictdef-cssstylesheetinit">CSS Object Model (CSSOM) Module Level 1: 6.1.2 The CSSStyleSheet Interface</see>
/// </summary>
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
/// <see href="https://wicg.github.io/css-parser-api/#dictdef-cssparseroptions">CSS Parser API: 2 Parsing API</see>
/// </summary>
/// <param name="AtRules"><see href="https://wicg.github.io/css-parser-api/#dom-cssparseroptions-atrules">CSS Parser API: 2 Parsing API</see></param>
[ECMAScript]
[Description("@#CSSParserOptions")]
public record CSSParserOptions(
    [property: Description("@#atRules")]object? AtRules = default);
