namespace Jazor.Style;

[ECMAScript]
[Description("@#")]
public partial record CssDeclarations
{
    [Description("@#$additional")]
    public CssDeclaration[]? Additional { get; init; }

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
public sealed record CssDeclaration(
    string Name,
    CssValue Value,
    bool Important = false);

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
