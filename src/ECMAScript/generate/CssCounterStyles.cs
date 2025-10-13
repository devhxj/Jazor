namespace ECMAScript;

/// <summary>
/// CSSCounterStyleRule
/// </summary>
[ECMAScript]
[Description("@#CSSCounterStyleRule")]
public class CSSCounterStyleRule : CSSRule
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// system
    /// </summary>
    [Description("@#system")]
    public extern string System { get; set; }

    /// <summary>
    /// symbols
    /// </summary>
    [Description("@#symbols")]
    public extern string Symbols { get; set; }

    /// <summary>
    /// additiveSymbols
    /// </summary>
    [Description("@#additiveSymbols")]
    public extern string AdditiveSymbols { get; set; }

    /// <summary>
    /// negative
    /// </summary>
    [Description("@#negative")]
    public extern string Negative { get; set; }

    /// <summary>
    /// prefix
    /// </summary>
    [Description("@#prefix")]
    public extern string Prefix { get; set; }

    /// <summary>
    /// suffix
    /// </summary>
    [Description("@#suffix")]
    public extern string Suffix { get; set; }

    /// <summary>
    /// range
    /// </summary>
    [Description("@#range")]
    public extern string Range { get; set; }

    /// <summary>
    /// pad
    /// </summary>
    [Description("@#pad")]
    public extern string Pad { get; set; }

    /// <summary>
    /// speakAs
    /// </summary>
    [Description("@#speakAs")]
    public extern string SpeakAs { get; set; }

    /// <summary>
    /// fallback
    /// </summary>
    [Description("@#fallback")]
    public extern string Fallback { get; set; }
}