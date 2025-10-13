using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Collections.ObjectModel.ReadOnlySet<T>")]
public static class ReadOnlySetModule
{
    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlySet`1" /> class that is a wrapper around the specified set.      </summary>
    ///<param name="set">The set to wrap.</param>    [WhiteList("System.Collections.ObjectModel.ReadOnlySet<T>.ReadOnlySet(System.Collections.Generic.ISet<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Set<T> ReadOnlySetNew<T>(ISet<T> set);

    [WhiteList("static System.Collections.ObjectModel.ReadOnlySet<T>.Empty.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.ObjectModel.ReadOnlySet<T> ReadOnlySetGetEmpty<T>(Set<T> instance);

    [WhiteList("System.Collections.ObjectModel.ReadOnlySet<T>.Count.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ReadOnlySetGetCount<T>(Set<T> instance);

    ///<summary>Returns an enumerator that iterates through the collection.</summary>
    ///<returns>An enumerator that can be used to iterate through the collection.</returns>    [WhiteList("System.Collections.ObjectModel.ReadOnlySet<T>.GetEnumerator()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static IEnumerator<T> ReadOnlySetGetEnumerator<T>(Set<T> instance);

    ///<summary>        Determines whether the <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.ICollection`1"></xref> contains a specific value.      </summary>
    ///<param name="item">        The object to locate in the <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.ICollection`1"></xref>.      </param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if <code data-dev-comment-type="paramref">item</code> is found in the <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.ICollection`1"></xref>; otherwise, <code data-dev-comment-type="langword">false</code>.      </returns>    [WhiteList("System.Collections.ObjectModel.ReadOnlySet<T>.Contains(T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ReadOnlySetContains<T>(Set<T> instance, T item);

    ///<summary>Determines whether the current set is a proper (strict) subset of a specified collection.</summary>
    ///<param name="other">The collection to compare to the current set.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the current set is a proper subset of other; otherwise <code data-dev-comment-type="langword">false</code>.      </returns>    [WhiteList("System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ReadOnlySetIsProperSubsetOf<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>Determines whether the current set is a proper (strict) superset of a specified collection.</summary>
    ///<param name="other">The collection to compare to the current set.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the collection is a proper superset of other; otherwise <code data-dev-comment-type="langword">false</code>.      </returns>    [WhiteList("System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ReadOnlySetIsProperSupersetOf<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>Determines whether the current set is a subset of a specified collection.</summary>
    ///<param name="other">The collection to compare to the current set.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the current set is a subset of other; otherwise <code data-dev-comment-type="langword">false</code>.      </returns>    [WhiteList("System.Collections.ObjectModel.ReadOnlySet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ReadOnlySetIsSubsetOf<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>Determines whether the current set is a super set of a specified collection.</summary>
    ///<param name="other">The collection to compare to the current set.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the current set is a subset of other; otherwise <code data-dev-comment-type="langword">false</code>.      </returns>    [WhiteList("System.Collections.ObjectModel.ReadOnlySet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ReadOnlySetIsSupersetOf<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>Determines whether the current set overlaps with the specified collection.</summary>
    ///<param name="other">The collection to compare to the current set.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code>if the current set and other share at least one common element; otherwise, <code data-dev-comment-type="langword">false</code>.      </returns>    [WhiteList("System.Collections.ObjectModel.ReadOnlySet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ReadOnlySetOverlaps<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>Determines whether the current set and the specified collection contain the same elements.</summary>
    ///<param name="other">The collection to compare to the current set.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the current set is equal to other; otherwise, <code data-dev-comment-type="langword">false</code>.      </returns>    [WhiteList("System.Collections.ObjectModel.ReadOnlySet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ReadOnlySetSetEquals<T>(Set<T> instance, IEnumerable<T> other);
}
