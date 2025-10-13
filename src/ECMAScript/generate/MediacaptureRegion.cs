namespace ECMAScript;

/// <summary>
/// CropTarget
/// </summary>
[ECMAScript]
[Description("@#CropTarget")]
public class CropTarget
{
    /// <summary>
    /// fromElement
    /// </summary>
    /// <param name="element">element</param>
    [Description("@#fromElement")]
    public extern static PromiseResult<CropTarget> FromElement(Element element);
}