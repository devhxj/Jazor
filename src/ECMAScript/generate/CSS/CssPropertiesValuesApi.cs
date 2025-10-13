namespace ECMAScript.CSS;

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

/// <summary>
/// CSSPropertyRule
/// </summary>
[ECMAScript]
[Description("@#CSSPropertyRule")]
public class CSSPropertyRule : CSSRule
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// syntax
    /// </summary>
    [Description("@#syntax")]
    public extern string Syntax { get; }

    /// <summary>
    /// inherits
    /// </summary>
    [Description("@#inherits")]
    public extern bool Inherits { get; }

    /// <summary>
    /// initialValue
    /// </summary>
    [Description("@#initialValue")]
    public extern string? InitialValue { get; }
}