namespace ECMAScript;

/// <summary>
/// Viewport
/// </summary>
[ECMAScript]
[Description("@#Viewport")]
public class Viewport
{
    /// <summary>
    /// segments
    /// </summary>
    [Description("@#segments")]
    public extern FrozenSet<DOMRect>? Segments { get; }
}