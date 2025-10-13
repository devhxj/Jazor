namespace ECMAScript.CSS;

/// <summary>
/// LayoutOptions
/// </summary>
[ECMAScript]
[Description("@#LayoutOptions")]
public record LayoutOptions(
    [property: Description("@#childDisplay")]ChildDisplayType ChildDisplay = ChildDisplayType.Block,
	[property: Description("@#sizing")]LayoutSizingMode Sizing = LayoutSizingMode.BlockLike);

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
/// BreakTokenOptions
/// </summary>
[ECMAScript]
[Description("@#BreakTokenOptions")]
public record BreakTokenOptions(
    [property: Description("@#childBreakTokens")]ChildBreakToken[]? ChildBreakTokens = default,
	[property: Description("@#data")]object? Data = default);

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
/// LayoutWorkletGlobalScope
/// </summary>
[ECMAScript]
[Description("@#LayoutWorkletGlobalScope")]
public class LayoutWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// registerLayout
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="layoutCtor">layoutCtor</param>
    [Description("@#registerLayout")]
    public extern void RegisterLayout(string name, Action layoutCtor);
}

/// <summary>
/// LayoutChild
/// </summary>
[ECMAScript]
[Description("@#LayoutChild")]
public class LayoutChild
{
    /// <summary>
    /// styleMap
    /// </summary>
    [Description("@#styleMap")]
    public extern StylePropertyMapReadOnly StyleMap { get; }

    /// <summary>
    /// intrinsicSizes
    /// </summary>
    [Description("@#intrinsicSizes")]
    public extern PromiseResult<IntrinsicSizes> IntrinsicSizes();

    /// <summary>
    /// layoutNextFragment
    /// </summary>
    /// <param name="constraints">constraints</param>
    /// <param name="breakToken">breakToken</param>
    [Description("@#layoutNextFragment")]
    public extern PromiseResult<LayoutFragment> LayoutNextFragment(LayoutConstraintsOptions constraints, ChildBreakToken breakToken);
}

/// <summary>
/// LayoutFragment
/// </summary>
[ECMAScript]
[Description("@#LayoutFragment")]
public class LayoutFragment
{
    /// <summary>
    /// inlineSize
    /// </summary>
    [Description("@#inlineSize")]
    public extern double InlineSize { get; }

    /// <summary>
    /// blockSize
    /// </summary>
    [Description("@#blockSize")]
    public extern double BlockSize { get; }

    /// <summary>
    /// inlineOffset
    /// </summary>
    [Description("@#inlineOffset")]
    public extern double InlineOffset { get; set; }

    /// <summary>
    /// blockOffset
    /// </summary>
    [Description("@#blockOffset")]
    public extern double BlockOffset { get; set; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }

    /// <summary>
    /// breakToken
    /// </summary>
    [Description("@#breakToken")]
    public extern ChildBreakToken? BreakToken { get; }
}

/// <summary>
/// IntrinsicSizes
/// </summary>
[ECMAScript]
[Description("@#IntrinsicSizes")]
public class IntrinsicSizes
{
    /// <summary>
    /// minContentSize
    /// </summary>
    [Description("@#minContentSize")]
    public extern double MinContentSize { get; }

    /// <summary>
    /// maxContentSize
    /// </summary>
    [Description("@#maxContentSize")]
    public extern double MaxContentSize { get; }
}

/// <summary>
/// LayoutConstraints
/// </summary>
[ECMAScript]
[Description("@#LayoutConstraints")]
public class LayoutConstraints
{
    /// <summary>
    /// availableInlineSize
    /// </summary>
    [Description("@#availableInlineSize")]
    public extern double AvailableInlineSize { get; }

    /// <summary>
    /// availableBlockSize
    /// </summary>
    [Description("@#availableBlockSize")]
    public extern double AvailableBlockSize { get; }

    /// <summary>
    /// fixedInlineSize
    /// </summary>
    [Description("@#fixedInlineSize")]
    public extern double? FixedInlineSize { get; }

    /// <summary>
    /// fixedBlockSize
    /// </summary>
    [Description("@#fixedBlockSize")]
    public extern double? FixedBlockSize { get; }

    /// <summary>
    /// percentageInlineSize
    /// </summary>
    [Description("@#percentageInlineSize")]
    public extern double PercentageInlineSize { get; }

    /// <summary>
    /// percentageBlockSize
    /// </summary>
    [Description("@#percentageBlockSize")]
    public extern double PercentageBlockSize { get; }

    /// <summary>
    /// blockFragmentationOffset
    /// </summary>
    [Description("@#blockFragmentationOffset")]
    public extern double? BlockFragmentationOffset { get; }

    /// <summary>
    /// blockFragmentationType
    /// </summary>
    [Description("@#blockFragmentationType")]
    public extern BlockFragmentationType BlockFragmentationType { get; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }
}

/// <summary>
/// ChildBreakToken
/// </summary>
[ECMAScript]
[Description("@#ChildBreakToken")]
public class ChildBreakToken
{
    /// <summary>
    /// breakType
    /// </summary>
    [Description("@#breakType")]
    public extern BreakType BreakType { get; }

    /// <summary>
    /// child
    /// </summary>
    [Description("@#child")]
    public extern LayoutChild Child { get; }
}

/// <summary>
/// BreakToken
/// </summary>
[ECMAScript]
[Description("@#BreakToken")]
public class BreakToken
{
    /// <summary>
    /// childBreakTokens
    /// </summary>
    [Description("@#childBreakTokens")]
    public extern FrozenSet<ChildBreakToken> ChildBreakTokens { get; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }
}

/// <summary>
/// LayoutEdges
/// </summary>
[ECMAScript]
[Description("@#LayoutEdges")]
public class LayoutEdges
{
    /// <summary>
    /// inlineStart
    /// </summary>
    [Description("@#inlineStart")]
    public extern double InlineStart { get; }

    /// <summary>
    /// inlineEnd
    /// </summary>
    [Description("@#inlineEnd")]
    public extern double InlineEnd { get; }

    /// <summary>
    /// blockStart
    /// </summary>
    [Description("@#blockStart")]
    public extern double BlockStart { get; }

    /// <summary>
    /// blockEnd
    /// </summary>
    [Description("@#blockEnd")]
    public extern double BlockEnd { get; }

    /// <summary>
    /// inline
    /// </summary>
    [Description("@#inline")]
    public extern double Inline { get; }

    /// <summary>
    /// block
    /// </summary>
    [Description("@#block")]
    public extern double Block { get; }
}

/// <summary>
/// FragmentResult
/// </summary>
[ECMAScript]
[Description("@#FragmentResult")]
public class FragmentResult
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern FragmentResult(FragmentResultOptions options);

    /// <summary>
    /// inlineSize
    /// </summary>
    [Description("@#inlineSize")]
    public extern double InlineSize { get; }

    /// <summary>
    /// blockSize
    /// </summary>
    [Description("@#blockSize")]
    public extern double BlockSize { get; }
}