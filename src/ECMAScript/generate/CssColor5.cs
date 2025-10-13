namespace ECMAScript;

/// <summary>
/// CSSColorProfileRule
/// </summary>
[ECMAScript]
[Description("@#CSSColorProfileRule")]
public class CSSColorProfileRule : CSSRule
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; }

    /// <summary>
    /// renderingIntent
    /// </summary>
    [Description("@#renderingIntent")]
    public extern string RenderingIntent { get; }

    /// <summary>
    /// components
    /// </summary>
    [Description("@#components")]
    public extern string Components { get; }
}