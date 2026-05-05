namespace ECMAScript.CSS;

/// <summary>
/// BreakTokenOptions
/// </summary>
[ECMAScript]
[Description("@#BreakTokenOptions")]
public record BreakTokenOptions(
    [property: Description("@#childBreakTokens")]ChildBreakToken[]? ChildBreakTokens = default,
    [property: Description("@#data")]object? Data = default);

/// <summary>
/// CSSMatrixComponentOptions
/// </summary>
[ECMAScript]
[Description("@#CSSMatrixComponentOptions")]
public record CSSMatrixComponentOptions(
    [property: Description("@#is2D")]bool Is2D = default);

/// <summary>
/// CSSNumericType
/// </summary>
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
/// CSSParserOptions
/// </summary>
[ECMAScript]
[Description("@#CSSParserOptions")]
public record CSSParserOptions(
    [property: Description("@#atRules")]object? AtRules = default);

/// <summary>
/// CSSStyleSheetInit
/// </summary>
[ECMAScript]
[Description("@#CSSStyleSheetInit")]
public record CSSStyleSheetInit(
    [property: Description("@#baseURL")]string? BaseURL = default,
    [property: Description("@#media")]CSSStyleSheetInitMedia? Media = default,
    [property: Description("@#disabled")]bool Disabled = false);

/// <summary>
/// FragmentResultOptions
/// </summary>
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
/// IntrinsicSizesResultOptions
/// </summary>
[ECMAScript]
[Description("@#IntrinsicSizesResultOptions")]
public record IntrinsicSizesResultOptions(
    [property: Description("@#maxContentSize")]double MaxContentSize = default,
    [property: Description("@#minContentSize")]double MinContentSize = default);

/// <summary>
/// LayoutConstraintsOptions
/// </summary>
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
/// LayoutOptions
/// </summary>
[ECMAScript]
[Description("@#LayoutOptions")]
public record LayoutOptions(
    [property: Description("@#childDisplay")]ChildDisplayType ChildDisplay = ChildDisplayType.Block,
    [property: Description("@#sizing")]LayoutSizingMode Sizing = LayoutSizingMode.BlockLike);

/// <summary>
/// PaintRenderingContext2DSettings
/// </summary>
[ECMAScript]
[Description("@#PaintRenderingContext2DSettings")]
public record PaintRenderingContext2DSettings(
    [property: Description("@#alpha")]bool Alpha = false);

/// <summary>
/// PropertyDefinition
/// </summary>
[ECMAScript]
[Description("@#PropertyDefinition")]
public record PropertyDefinition(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#syntax")]string? Syntax = default,
    [property: Description("@#inherits")]bool Inherits = default,
    [property: Description("@#initialValue")]string? InitialValue = default);
