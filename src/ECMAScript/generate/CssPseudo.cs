namespace ECMAScript;

/// <summary>
/// CSSPseudoElement
/// </summary>
[ECMAScript]
[Description("@#CSSPseudoElement")]
public class CSSPseudoElement : EventTarget
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// element
    /// </summary>
    [Description("@#element")]
    public extern Element Element { get; }

    /// <summary>
    /// parent
    /// </summary>
    [Description("@#parent")]
    public extern Either<Element, CSSPseudoElement> Parent { get; }

    /// <summary>
    /// pseudo
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#pseudo")]
    public extern CSSPseudoElement? Pseudo(string type);

    #region mixin GeometryUtils
    /// <summary>
    /// getBoxQuads
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getBoxQuads")]
    public extern DOMQuad[] GetBoxQuads(BoxQuadOptions? options = default);

    /// <summary>
    /// convertQuadFromNode
    /// </summary>
    /// <param name="quad">quad</param>
    /// <param name="from">from</param>
    /// <param name="options">options</param>
    [Description("@#convertQuadFromNode")]
    public extern DOMQuad ConvertQuadFromNode(DOMQuadInit quad, GeometryNode from, ConvertCoordinateOptions? options = default);

    /// <summary>
    /// convertRectFromNode
    /// </summary>
    /// <param name="rect">rect</param>
    /// <param name="from">from</param>
    /// <param name="options">options</param>
    [Description("@#convertRectFromNode")]
    public extern DOMQuad ConvertRectFromNode(DOMRectReadOnly rect, GeometryNode from, ConvertCoordinateOptions? options = default);

    /// <summary>
    /// convertPointFromNode
    /// </summary>
    /// <param name="point">point</param>
    /// <param name="from">from</param>
    /// <param name="options">options</param>
    [Description("@#convertPointFromNode")]
    public extern DOMPoint ConvertPointFromNode(DOMPointInit point, GeometryNode from, ConvertCoordinateOptions? options = default);
    #endregion
}