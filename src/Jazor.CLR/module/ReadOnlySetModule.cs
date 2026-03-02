namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlySet<T>","System/Collections/ObjectModel/ReadOnlySetModule.js")]
public static class ReadOnlySetModule<T>
{
	///<summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlySet`1" /> class that is a wrapper around the specified set.</summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlySet<T>.ReadOnlySet(System.Collections.Generic.ISet<T>)", "@#{0}")]
	public extern static Set<T> _aede400efbd05842(Set<T> set);

	/// <summary>
	/// C#: ReadOnlySet.Empty
	/// JS: new Set()
	/// </summary>
	[Jazor(Op.Inline, "static System.Collections.ObjectModel.ReadOnlySet<T>.Empty.get", "new Set()")]
	public extern static System.Collections.ObjectModel.ReadOnlySet<T> _843cd8664672a9f8();

	/// <summary>
	/// C#: instance.Count
	/// JS: instance.size
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlySet<T>.Count.get", "@#{0}.size")]
	public extern static Number _ede23209845683c4(Set<T> instance);

	///<summary>Returns an enumerator that iterates through the collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.GetEnumerator()")]
	public extern static Array<T> _1d4e088c99161116(Set<T> instance);

	/// <summary>
	/// C#: instance.Contains(item)
	/// JS: instance.has(item)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlySet<T>.Contains(T)", "@#{0}.has(@#{1})")]
	public extern static bool _a9cd3343f82c8a7b(Set<T> instance, object item);

	/// <summary>
	/// C#: instance.IsProperSubsetOf(other)
	/// JS: 检查是否为真子集
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { var otherSize = 0; for (var _ of other) otherSize++; if (set.size >= otherSize) return false; for (var item of set) { var found = false; for (var o of other) { if (o === item) { found = true; break; } } if (!found) return false; } return true; })(@#{0}, @#{1})")]
	public extern static bool _8745918ab865b9f0(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: instance.IsProperSupersetOf(other)
	/// JS: 检查是否为真超集
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { var otherSize = 0; for (var _ of other) otherSize++; if (set.size <= otherSize) return false; for (var item of other) { if (!set.has(item)) return false; } return true; })(@#{0}, @#{1})")]
	public extern static bool _ab53c8c15a545026(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: instance.IsSubsetOf(other)
	/// JS: 检查是否为子集
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlySet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { for (var item of set) { var found = false; for (var o of other) { if (o === item) { found = true; break; } } if (!found) return false; } return true; })(@#{0}, @#{1})")]
	public extern static bool _f72f25db872c4c11(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: instance.IsSupersetOf(other)
	/// JS: 检查是否为超集
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlySet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { for (var item of other) { if (!set.has(item)) return false; } return true; })(@#{0}, @#{1})")]
	public extern static bool _e7d6617cc0e3119e(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: instance.Overlaps(other)
	/// JS: 检查是否有交集
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlySet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { for (var item of other) { if (set.has(item)) return true; } return false; })(@#{0}, @#{1})")]
	public extern static bool _520d7f31ddf30fea(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: instance.SetEquals(other)
	/// JS: 检查集合是否相等
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlySet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { var otherSize = 0; for (var _ of other) otherSize++; if (set.size !== otherSize) return false; for (var item of other) { if (!set.has(item)) return false; } return true; })(@#{0}, @#{1})")]
	public extern static bool _eb16d835e6822ba0(Set<T> instance, Array<T> other);
}
