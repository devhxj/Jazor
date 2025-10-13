namespace ECMAScript;

/// <summary>
/// SVGPathDataSettings
/// </summary>
[ECMAScript]
[Description("@#SVGPathDataSettings")]
public record SVGPathDataSettings(
    [property: Description("@#normalize")]bool Normalize = false);

/// <summary>
/// SVGPathSegment
/// </summary>
[ECMAScript]
[Description("@#SVGPathSegment")]
public class SVGPathSegment
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// values
    /// </summary>
    [Description("@#values")]
    public extern FrozenSet<float> Values { get; set; }
}

/// <summary>
/// SVGPathElement
/// </summary>
[ECMAScript]
[Description("@#SVGPathElement")]
public class SVGPathElement : SVGGeometryElement
{
    /// <summary>
    /// getPathSegmentAtLength
    /// </summary>
    /// <param name="distance">distance</param>
    [Description("@#getPathSegmentAtLength")]
    public extern SVGPathSegment? GetPathSegmentAtLength(float distance);

    #region mixin SVGPathData
    /// <summary>
    /// getPathData
    /// </summary>
    /// <param name="settings">settings</param>
    [Description("@#getPathData")]
    public extern SVGPathSegment[] GetPathData(SVGPathDataSettings? settings = default);

    /// <summary>
    /// setPathData
    /// </summary>
    /// <param name="pathData">pathData</param>
    [Description("@#setPathData")]
    public extern void SetPathData(SVGPathSegment[] pathData);
    #endregion
}