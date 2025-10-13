namespace ECMAScript.CSS;

/// <summary>
/// CSSParserOptions
/// </summary>
[ECMAScript]
[Description("@#CSSParserOptions")]
public record CSSParserOptions(
    [property: Description("@#atRules")]object? AtRules = default);

/// <summary>
/// CSSParserRule
/// </summary>
[ECMAScript]
[Description("@#CSSParserRule")]
public abstract class CSSParserRule
{

}

/// <summary>
/// CSSParserAtRule
/// </summary>
[ECMAScript]
[Description("@#CSSParserAtRule")]
public class CSSParserAtRule : CSSParserRule
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="prelude">prelude</param>
    /// <param name="body">body</param>
    public extern CSSParserAtRule(string name, CSSToken[] prelude, CSSParserRule[]? body);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// prelude
    /// </summary>
    [Description("@#prelude")]
    public extern FrozenSet<CSSParserValue> Prelude { get; }

    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserRule>? Body { get; }
}

/// <summary>
/// CSSParserQualifiedRule
/// </summary>
[ECMAScript]
[Description("@#CSSParserQualifiedRule")]
public class CSSParserQualifiedRule : CSSParserRule
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="prelude">prelude</param>
    /// <param name="body">body</param>
    public extern CSSParserQualifiedRule(CSSToken[] prelude, CSSParserRule[]? body);

    /// <summary>
    /// prelude
    /// </summary>
    [Description("@#prelude")]
    public extern FrozenSet<CSSParserValue> Prelude { get; }

    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserRule> Body { get; }
}

/// <summary>
/// CSSParserDeclaration
/// </summary>
[ECMAScript]
[Description("@#CSSParserDeclaration")]
public class CSSParserDeclaration : CSSParserRule
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="body">body</param>
    public extern CSSParserDeclaration(string name, CSSParserRule[] body);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserValue> Body { get; }
}

/// <summary>
/// CSSParserValue
/// </summary>
[ECMAScript]
[Description("@#CSSParserValue")]
public abstract class CSSParserValue
{

}

/// <summary>
/// CSSParserBlock
/// </summary>
[ECMAScript]
[Description("@#CSSParserBlock")]
public class CSSParserBlock : CSSParserValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="body">body</param>
    public extern CSSParserBlock(string name, CSSParserValue[] body);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserValue> Body { get; }
}

/// <summary>
/// CSSParserFunction
/// </summary>
[ECMAScript]
[Description("@#CSSParserFunction")]
public class CSSParserFunction : CSSParserValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="args">args</param>
    public extern CSSParserFunction(string name, CSSParserValue[][] args);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// args
    /// </summary>
    [Description("@#args")]
    public extern FrozenSet<FrozenSet<CSSParserValue>> Args { get; }
}