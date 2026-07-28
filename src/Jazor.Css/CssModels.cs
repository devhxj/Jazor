namespace Jazor.Css;

[ECMAScript]
[Description("@#")]
public partial record CssDeclarations
{
    [Description("@#$additional")]
    public CssDeclaration[]? Additional { get; init; }

    public extern string? this[string propertyName] { get; set; }
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
public sealed record CssDeclaration(
    string Name,
    string Value,
    bool Important = false);

[String]
public enum CssChildKind
{
    [Description("@#selector")]
    Selector,

    [Description("@#media")]
    Media,

    [Description("@#supports")]
    Supports
}

[ECMAScript]
[Description("@#")]
public sealed record CssChild(
    CssChildKind Kind,
    string Prelude,
    CssRule Rule);

[ECMAScript]
[Description("@#")]
public sealed record CssFrame(
    string Selector,
    CssDeclarations Declarations);

[ECMAScript]
[Description("@#")]
public sealed record CssOptions
{
    public string? StyleId { get; init; }

    public string? Nonce { get; init; }
}
