namespace ECMAScript.Style;

[ECMAScript]
[Description("@#")]
public partial record CssDeclarations
{
    [Description("@#$additional")]
    public ICssDeclaration[]? Additional { get; init; }

    public extern CssValue? this[string propertyName] { get; set; }
}

[ECMAScript]
[Description("@#")]
public sealed record CssRule : CssDeclarations
{
    [Description("@#$children")]
    public CssChild[]? Children { get; init; }
}

[ECMAScript]
[Description("@#")]
/// <summary>
/// Represents a declaration appended after generated properties. This is the escape hatch for
/// intentional duplicate declarations, while normal properties remain strongly typed.
/// 表示追加在生成属性后的声明，用于有意保留重复声明；普通属性仍保持强类型。
/// </summary>
public interface ICssDeclaration
{
    [Description("@#name")]
    string Name { get; }

    [Description("@#value")]
    CssValue Value { get; }

    [Description("@#priority")]
    CssDeclarationPriority Priority { get; }
}

[String]
public enum CssDeclarationPriority
{
    [Description("@#normal")]
    Normal,

    [Description("@#important")]
    Important
}

public sealed record CssDeclaration(
    [property: Description("@#name")] string Name,
    [property: Description("@#value")] CssValue Value,
    [property: Description("@#priority")] CssDeclarationPriority Priority = CssDeclarationPriority.Normal) : ICssDeclaration;

/// <summary>
/// Describes one structural CSS shadow. Optional parts are omitted instead of
/// serialized as defaults, so the authored C# mirrors the CSS grammar.
/// 描述一个结构化 CSS 阴影。可选部分会被省略而非填充默认值，使 C# 写法与 CSS 语法一致。
/// </summary>
public sealed record CssShadow(
    [property: Description("@#offsetX")] CssShadowLength OffsetX,
    [property: Description("@#offsetY")] CssShadowLength OffsetY,
    [property: Description("@#blur")] CssShadowLength? Blur = null,
    [property: Description("@#spread")] CssShadowLength? Spread = null,
    [property: Description("@#color")] CssShadowColor? Color = null,
    [property: Description("@#inset")] bool Inset = false);

/// <summary>
/// Describes one gradient color stop. <see cref="From"/> and <see cref="To"/> model the optional
/// one- or two-position form without admitting arbitrary CSS text.
/// 描述一个渐变色标；From/To 精确表达可选的单位置或双位置形式。
/// </summary>
public sealed record CssGradientStop(
    [property: Description("@#color")] CssColorValue Color,
    [property: Description("@#from")] CssLengthPercentageValue? From = null,
    [property: Description("@#to")] CssLengthPercentageValue? To = null);

[String]
public enum CssChildKind
{
    [Description("@#selector")]
    Selector,

    [Description("@#media")]
    Media,

    [Description("@#supports")]
    Supports,

    [Description("@#container")]
    Container,

    [Description("@#layer")]
    Layer,

    [Description("@#scope")]
    Scope,

    [Description("@#starting-style")]
    StartingStyle
}

[ECMAScript]
[Description("@#")]
public sealed record CssChild(
    [property: Description("@#kind")] CssChildKind Kind,
    [property: Description("@#prelude")] string? Prelude,
    [property: Description("@#rule")] CssRule Rule);

[ECMAScript]
[Description("@#")]
public sealed record CssFrame(
    [property: Description("@#selector")] string Selector,
    [property: Description("@#declarations")] CssDeclarations Declarations);

[ECMAScript]
[Description("@#")]
public sealed record CssAtRule(
    [property: Description("@#name")] string Name,
    [property: Description("@#declarations")] CssDeclarations Declarations,
    [property: Description("@#prelude")] string? Prelude = null,
    [property: Description("@#children")] CssAtRule[]? Children = null);

[ECMAScript]
[Description("@#")]
public sealed record CssOptions
{
    [Description("@#styleId")]
    public string? StyleId { get; init; }

    [Description("@#nonce")]
    public string? Nonce { get; init; }

    [Description("@#target")]
    public DocumentFragment? Target { get; init; }

    [Description("@#detached")]
    public bool Detached { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record CssSnapshot(
    [property: Description("@#styleId")] string StyleId,
    [property: Description("@#nonce")] string? Nonce,
    [property: Description("@#cssText")] string CssText,
    [property: Description("@#hydrationText")] string HydrationText);

[ECMAScript]
[Description("@#")]
public sealed record CssContext
{
    internal CssContext(bool initialized)
    {
    }

    [Description("@#$namesByCanonical")]
    internal Map<string, string> NamesByCanonical { get; init; } = null!;

    [Description("@#$canonicalByName")]
    internal Map<string, string> CanonicalByName { get; init; } = null!;

    [Description("@#$bodyById")]
    internal Map<string, string> BodyById { get; init; } = null!;

    [Description("@#$entryIds")]
    internal Array<string> EntryIds { get; set; } = null!;

    [Description("@#$entryBodies")]
    internal Array<string> EntryBodies { get; set; } = null!;

    [Description("@#$styleId")]
    internal string StyleId { get; set; } = null!;

    [Description("@#$nonce")]
    internal string? Nonce { get; set; }

    [Description("@#$target")]
    internal DocumentFragment? Target { get; set; }

    [Description("@#$detached")]
    internal bool Detached { get; set; }

    [Description("@#$hasRegistered")]
    internal bool HasRegistered { get; set; }

    [Description("@#$domStyle")]
    internal HTMLStyleElement? DomStyle { get; set; }

    [Description("@#$domDocument")]
    internal Document? DomDocument { get; set; }

    [Description("@#$domHydrated")]
    internal bool DomHydrated { get; set; }
}
