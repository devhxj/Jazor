namespace ECMAScript;

/// <summary>
/// SVGClipPathElement
/// </summary>
[ECMAScript]
[Description("@#SVGClipPathElement")]
public class SVGClipPathElement : SVGElement
{
    /// <summary>
    /// clipPathUnits
    /// </summary>
    [Description("@#clipPathUnits")]
    public extern SVGAnimatedEnumeration ClipPathUnits { get; }

    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern SVGAnimatedTransformList Transform { get; }
}

/// <summary>
/// SVGMaskElement
/// </summary>
[ECMAScript]
[Description("@#SVGMaskElement")]
public class SVGMaskElement : SVGElement
{
    /// <summary>
    /// maskUnits
    /// </summary>
    [Description("@#maskUnits")]
    public extern SVGAnimatedEnumeration MaskUnits { get; }

    /// <summary>
    /// maskContentUnits
    /// </summary>
    [Description("@#maskContentUnits")]
    public extern SVGAnimatedEnumeration MaskContentUnits { get; }

    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }
}