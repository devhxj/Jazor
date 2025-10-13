namespace ECMAScript;

/// <summary>
/// StartViewTransitionOptions
/// </summary>
[ECMAScript]
[Description("@#StartViewTransitionOptions")]
public record StartViewTransitionOptions(
    [property: Description("@#update")]ViewTransitionUpdateCallback? Update = null,
	[property: Description("@#types")]string[]? Types = null);

/// <summary>
/// CSSViewTransitionRule
/// </summary>
[ECMAScript]
[Description("@#CSSViewTransitionRule")]
public class CSSViewTransitionRule : CSSRule
{
    /// <summary>
    /// navigation
    /// </summary>
    [Description("@#navigation")]
    public extern string Navigation { get; }

    /// <summary>
    /// types
    /// </summary>
    [Description("@#types")]
    public extern FrozenSet<string> Types { get; }
}

/// <summary>
/// ViewTransitionTypeSet
/// </summary>
[ECMAScript]
[Description("@#ViewTransitionTypeSet")]
public class ViewTransitionTypeSet : ISet<string>
{
    #region Set
    extern int ICollection<string>.Count { get; }
    extern bool ICollection<string>.IsReadOnly { get; }
    extern bool ISet<string>.Add(string item);
    extern void ICollection<string>.Clear();
    extern bool ICollection<string>.Contains(string item);
    extern void ICollection<string>.CopyTo(string[] array, int arrayIndex);
    extern void ISet<string>.ExceptWith(IEnumerable<string> other);
    extern IEnumerator<string> IEnumerable<string>.GetEnumerator();
    extern void ISet<string>.IntersectWith(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.Overlaps(IEnumerable<string> other);
    extern bool ICollection<string>.Remove(string item);
    extern bool ISet<string>.SetEquals(IEnumerable<string> other);
    extern void ISet<string>.SymmetricExceptWith(IEnumerable<string> other);
    extern void ISet<string>.UnionWith(IEnumerable<string> other);
    extern void ICollection<string>.Add(string item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// ViewTransition
/// </summary>
[ECMAScript]
[Description("@#ViewTransition")]
public partial class ViewTransition
{
    /// <summary>
    /// types
    /// </summary>
    [Description("@#types")]
    public extern ViewTransitionTypeSet Types { get; set; }

    /// <summary>
    /// updateCallbackDone
    /// </summary>
    [Description("@#updateCallbackDone")]
    public extern PromiseResult<object> UpdateCallbackDone { get; }

    /// <summary>
    /// ready
    /// </summary>
    [Description("@#ready")]
    public extern PromiseResult<object> Ready { get; }

    /// <summary>
    /// finished
    /// </summary>
    [Description("@#finished")]
    public extern PromiseResult<object> Finished { get; }

    /// <summary>
    /// skipTransition
    /// </summary>
    [Description("@#skipTransition")]
    public extern void SkipTransition();
}