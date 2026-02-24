namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlySet<T>","System/Collections/ObjectModel/ReadOnlySetModule.js")]
public static class ReadOnlySetModule<T>
{
	///<summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlySet`1" /> class that is a wrapper around the specified set.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.ReadOnlySet(System.Collections.Generic.ISet<T>)")]
	public extern static Set<T> _aede400efbd05842(ISet<T> set);

	[Jazor(Op.Discard ,"static System.Collections.ObjectModel.ReadOnlySet<T>.Empty.get")]
	public extern static System.Collections.ObjectModel.ReadOnlySet<T> _843cd8664672a9f8(Set<T> instance);

	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.Count.get")]
	public extern static Number _ede23209845683c4(Set<T> instance);

	///<summary>Returns an enumerator that iterates through the collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.GetEnumerator()")]
	public extern static IEnumerator<T> _1d4e088c99161116(Set<T> instance);

	///<summary>Determines whether the <xref data-throw-if-not-resolved="true" uid="System.Collections.Generic.ICollection`1"></xref> contains a specific value.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.Contains(T)")]
	public extern static bool _a9cd3343f82c8a7b(Set<T> instance, object item);

	///<summary>Determines whether the current set is a proper (strict) subset of a specified collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _8745918ab865b9f0(Set<T> instance, IEnumerable<T> other);

	///<summary>Determines whether the current set is a proper (strict) superset of a specified collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _ab53c8c15a545026(Set<T> instance, IEnumerable<T> other);

	///<summary>Determines whether the current set is a subset of a specified collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _f72f25db872c4c11(Set<T> instance, IEnumerable<T> other);

	///<summary>Determines whether the current set is a super set of a specified collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _e7d6617cc0e3119e(Set<T> instance, IEnumerable<T> other);

	///<summary>Determines whether the current set overlaps with the specified collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _520d7f31ddf30fea(Set<T> instance, IEnumerable<T> other);

	///<summary>Determines whether the current set and the specified collection contain the same elements.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _eb16d835e6822ba0(Set<T> instance, IEnumerable<T> other);
}
