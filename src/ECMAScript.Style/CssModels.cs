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
    string Name { get; }

    CssValue Value { get; }

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
    string Name,
    CssValue Value,
    CssDeclarationPriority Priority = CssDeclarationPriority.Normal) : ICssDeclaration;

/// <summary>
/// Describes one structural CSS shadow. Optional parts are omitted instead of
/// serialized as defaults, so the authored C# mirrors the CSS grammar.
/// 描述一个结构化 CSS 阴影。可选部分会被省略而非填充默认值，使 C# 写法与 CSS 语法一致。
/// </summary>
public sealed record CssShadow(
    CssShadowLength OffsetX,
    CssShadowLength OffsetY,
    CssShadowLength? Blur = null,
    CssShadowLength? Spread = null,
    CssShadowColor? Color = null,
    bool Inset = false);

/// <summary>
/// Describes one gradient color stop. <see cref="From"/> and <see cref="To"/> model the optional
/// one- or two-position form without admitting arbitrary CSS text.
/// 描述一个渐变色标；From/To 精确表达可选的单位置或双位置形式。
/// </summary>
public sealed record CssGradientStop(
    CssColorValue Color,
    CssLengthPercentageValue? From = null,
    CssLengthPercentageValue? To = null);

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
    CssChildKind Kind,
    string? Prelude,
    CssRule Rule);

[ECMAScript]
[Description("@#")]
public sealed record CssFrame(
    string Selector,
    CssDeclarations Declarations);

[ECMAScript]
[Description("@#")]
public sealed record CssAtRule(
    string Name,
    CssDeclarations Declarations,
    string? Prelude = null,
    CssAtRule[]? Children = null);

[ECMAScript]
[Description("@#")]
public sealed record CssOptions
{
    public string? StyleId { get; init; }

    public string? Nonce { get; init; }

    public DocumentFragment? Target { get; init; }

    public bool Detached { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record CssSnapshot(
    string StyleId,
    string? Nonce,
    string CssText,
    string HydrationText);

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
