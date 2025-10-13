namespace ECMAScript;

/// <summary>
/// XRPlane
/// </summary>
[ECMAScript]
[Description("@#XRPlane")]
public class XRPlane
{
    /// <summary>
    /// planeSpace
    /// </summary>
    [Description("@#planeSpace")]
    public extern XRSpace PlaneSpace { get; }

    /// <summary>
    /// polygon
    /// </summary>
    [Description("@#polygon")]
    public extern FrozenSet<DOMPointReadOnly> Polygon { get; }

    /// <summary>
    /// orientation
    /// </summary>
    [Description("@#orientation")]
    public extern XRPlaneOrientation? Orientation { get; }

    /// <summary>
    /// lastChangedTime
    /// </summary>
    [Description("@#lastChangedTime")]
    public extern double LastChangedTime { get; }

    /// <summary>
    /// semanticLabel
    /// </summary>
    [Description("@#semanticLabel")]
    public extern string? SemanticLabel { get; }
}

/// <summary>
/// XRPlaneSet
/// </summary>
[ECMAScript]
[Description("@#XRPlaneSet")]
public class XRPlaneSet : ISet<XRPlane>
{
    #region Set
    extern int ICollection<XRPlane>.Count { get; }
    extern bool ICollection<XRPlane>.IsReadOnly { get; }
    extern bool ISet<XRPlane>.Add(XRPlane item);
    extern void ICollection<XRPlane>.Clear();
    extern bool ICollection<XRPlane>.Contains(XRPlane item);
    extern void ICollection<XRPlane>.CopyTo(XRPlane[] array, int arrayIndex);
    extern void ISet<XRPlane>.ExceptWith(IEnumerable<XRPlane> other);
    extern IEnumerator<XRPlane> IEnumerable<XRPlane>.GetEnumerator();
    extern void ISet<XRPlane>.IntersectWith(IEnumerable<XRPlane> other);
    extern bool ISet<XRPlane>.IsProperSubsetOf(IEnumerable<XRPlane> other);
    extern bool ISet<XRPlane>.IsProperSupersetOf(IEnumerable<XRPlane> other);
    extern bool ISet<XRPlane>.IsSubsetOf(IEnumerable<XRPlane> other);
    extern bool ISet<XRPlane>.IsSupersetOf(IEnumerable<XRPlane> other);
    extern bool ISet<XRPlane>.Overlaps(IEnumerable<XRPlane> other);
    extern bool ICollection<XRPlane>.Remove(XRPlane item);
    extern bool ISet<XRPlane>.SetEquals(IEnumerable<XRPlane> other);
    extern void ISet<XRPlane>.SymmetricExceptWith(IEnumerable<XRPlane> other);
    extern void ISet<XRPlane>.UnionWith(IEnumerable<XRPlane> other);
    extern void ICollection<XRPlane>.Add(XRPlane item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}