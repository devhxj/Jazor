namespace ECMAScript;

/// <summary>
/// XRMesh
/// </summary>
[ECMAScript]
[Description("@#XRMesh")]
public class XRMesh
{
    /// <summary>
    /// meshSpace
    /// </summary>
    [Description("@#meshSpace")]
    public extern XRSpace MeshSpace { get; }

    /// <summary>
    /// vertices
    /// </summary>
    [Description("@#vertices")]
    public extern FrozenSet<Float32Array> Vertices { get; }

    /// <summary>
    /// indices
    /// </summary>
    [Description("@#indices")]
    public extern Uint32Array Indices { get; }

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
/// XRMeshSet
/// </summary>
[ECMAScript]
[Description("@#XRMeshSet")]
public class XRMeshSet : ISet<XRMesh>
{
    #region Set
    extern int ICollection<XRMesh>.Count { get; }
    extern bool ICollection<XRMesh>.IsReadOnly { get; }
    extern bool ISet<XRMesh>.Add(XRMesh item);
    extern void ICollection<XRMesh>.Clear();
    extern bool ICollection<XRMesh>.Contains(XRMesh item);
    extern void ICollection<XRMesh>.CopyTo(XRMesh[] array, int arrayIndex);
    extern void ISet<XRMesh>.ExceptWith(IEnumerable<XRMesh> other);
    extern IEnumerator<XRMesh> IEnumerable<XRMesh>.GetEnumerator();
    extern void ISet<XRMesh>.IntersectWith(IEnumerable<XRMesh> other);
    extern bool ISet<XRMesh>.IsProperSubsetOf(IEnumerable<XRMesh> other);
    extern bool ISet<XRMesh>.IsProperSupersetOf(IEnumerable<XRMesh> other);
    extern bool ISet<XRMesh>.IsSubsetOf(IEnumerable<XRMesh> other);
    extern bool ISet<XRMesh>.IsSupersetOf(IEnumerable<XRMesh> other);
    extern bool ISet<XRMesh>.Overlaps(IEnumerable<XRMesh> other);
    extern bool ICollection<XRMesh>.Remove(XRMesh item);
    extern bool ISet<XRMesh>.SetEquals(IEnumerable<XRMesh> other);
    extern void ISet<XRMesh>.SymmetricExceptWith(IEnumerable<XRMesh> other);
    extern void ISet<XRMesh>.UnionWith(IEnumerable<XRMesh> other);
    extern void ICollection<XRMesh>.Add(XRMesh item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}