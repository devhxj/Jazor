namespace ECMAScript.CSS;

/// <summary>
/// HighlightsFromPointOptions
/// </summary>
[ECMAScript]
[Description("@#HighlightsFromPointOptions")]
public record HighlightsFromPointOptions(
    [property: Description("@#shadowRoots")]ShadowRoot[]? ShadowRoots = default);

/// <summary>
/// Highlight
/// </summary>
[ECMAScript]
[Description("@#Highlight")]
public class Highlight : ISet<AbstractRange>
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="initialRanges">initialRanges</param>
    public extern Highlight(AbstractRange initialRanges);

    #region Set
    extern int ICollection<AbstractRange>.Count { get; }
    extern bool ICollection<AbstractRange>.IsReadOnly { get; }
    extern bool ISet<AbstractRange>.Add(AbstractRange item);
    extern void ICollection<AbstractRange>.Clear();
    extern bool ICollection<AbstractRange>.Contains(AbstractRange item);
    extern void ICollection<AbstractRange>.CopyTo(AbstractRange[] array, int arrayIndex);
    extern void ISet<AbstractRange>.ExceptWith(IEnumerable<AbstractRange> other);
    extern IEnumerator<AbstractRange> IEnumerable<AbstractRange>.GetEnumerator();
    extern void ISet<AbstractRange>.IntersectWith(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsProperSubsetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsProperSupersetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsSubsetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsSupersetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.Overlaps(IEnumerable<AbstractRange> other);
    extern bool ICollection<AbstractRange>.Remove(AbstractRange item);
    extern bool ISet<AbstractRange>.SetEquals(IEnumerable<AbstractRange> other);
    extern void ISet<AbstractRange>.SymmetricExceptWith(IEnumerable<AbstractRange> other);
    extern void ISet<AbstractRange>.UnionWith(IEnumerable<AbstractRange> other);
    extern void ICollection<AbstractRange>.Add(AbstractRange item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion

    /// <summary>
    /// priority
    /// </summary>
    [Description("@#priority")]
    public extern int Priority { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern HighlightType Type { get; set; }
}

/// <summary>
/// HighlightRegistry
/// </summary>
[ECMAScript]
[Description("@#HighlightRegistry")]
public partial class HighlightRegistry : IDictionary<string, Highlight>
{
    #region Dictionary
    extern Highlight IDictionary<string, Highlight>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, Highlight>.Keys { get; }
    extern ICollection<Highlight> IDictionary<string, Highlight>.Values { get; }
    extern int ICollection<KeyValuePair<string, Highlight>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, Highlight>>.IsReadOnly { get; }
    extern void IDictionary<string, Highlight>.Add(string key, Highlight value);
    extern void ICollection<KeyValuePair<string, Highlight>>.Add(KeyValuePair<string, Highlight> item);
    extern void ICollection<KeyValuePair<string, Highlight>>.Clear();
    extern bool ICollection<KeyValuePair<string, Highlight>>.Contains(KeyValuePair<string, Highlight> item);
    extern bool IDictionary<string, Highlight>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, Highlight>>.CopyTo(KeyValuePair<string, Highlight>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, Highlight>> IEnumerable<KeyValuePair<string, Highlight>>.GetEnumerator();
    extern bool IDictionary<string, Highlight>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, Highlight>>.Remove(KeyValuePair<string, Highlight> item);
    extern bool IDictionary<string, Highlight>.TryGetValue(string key, [MaybeNullWhen(false)] out Highlight value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion

    /// <summary>
    /// highlightsFromPoint
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="options">options</param>
    [Description("@#highlightsFromPoint")]
    public extern Highlight[] HighlightsFromPoint(float x, float y, HighlightsFromPointOptions? options = default);
}