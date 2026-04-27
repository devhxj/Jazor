namespace Jazor.CLR;

[ECMAScriptModule("System/Collections/ObjectModel/ReadOnlySetT1Module.js")]
[Jazor(Op.Alias, "System.Collections.ObjectModel.ReadOnlySet<T>","Set")]
public static class ReadOnlySetT1Module<T>
{
	// ReadOnlySet shares the same JS backing type as HashSet, so subset/superset logic can
	// delegate to the HashSet helpers instead of duplicating set traversal code here.
	///<summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlySet`1" /> class that is a wrapper around the specified set.</summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlySet<T>.ReadOnlySet(System.Collections.Generic.ISet<T>)")]
	public static Set<T> _aede400efbd05842(IEnumerable<T> set)
	{
		if (set == null)
			throw new Error("ArgumentNullException: set is null");

		// Snapshot from IEnumerable so any iterable-backed ISet projection can flow through here.
		var snapshot = new Set<T>(set);
		return SetCarrierRuntime.MarkAsReadOnlyCarrier(snapshot);
	}

	/// <summary>
	/// C#: ReadOnlySet.Empty
	/// JS: new Set()
	/// </summary>
	[Jazor(Op.Import, "static System.Collections.ObjectModel.ReadOnlySet<T>.Empty.get")]
	public static System.Collections.ObjectModel.ReadOnlySet<T> _843cd8664672a9f8()
		=> (System.Collections.ObjectModel.ReadOnlySet<T>)(object)SetCarrierRuntime.MarkAsReadOnlyCarrier(new Set<T>());

	/// <summary>
	/// C#: instance.Count
	/// JS: instance.size
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlySet<T>.Count.get", "__arg1.size")]
	public extern static Number _ede23209845683c4(Set<T> instance);

	///<summary>Returns an enumerator that iterates through the collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.ObjectModel.ReadOnlySet<T>.GetEnumerator()")]
	public extern static Array<T> _1d4e088c99161116(Set<T> instance);

	/// <summary>
	/// C#: instance.Contains(item)
	/// JS: instance.has(item)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlySet<T>.Contains(T)", "__arg1.has(__arg2)")]
	public extern static bool _a9cd3343f82c8a7b(Set<T> instance, object item);

	/// <summary>
	/// C#: instance.IsProperSubsetOf(other)
	/// JS: 检查是否为真子集
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _8745918ab865b9f0(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.IsProperSubsetOfCore(instance, other);

	/// <summary>
	/// C#: instance.IsProperSupersetOf(other)
	/// JS: 检查是否为真超集
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlySet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _ab53c8c15a545026(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.IsProperSupersetOfCore(instance, other);

	/// <summary>
	/// C#: instance.IsSubsetOf(other)
	/// JS: 检查是否为子集
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlySet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _f72f25db872c4c11(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.IsSubsetOfCore(instance, other);

	/// <summary>
	/// C#: instance.IsSupersetOf(other)
	/// JS: 检查是否为超集
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlySet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _e7d6617cc0e3119e(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.IsSupersetOfCore(instance, other);

	/// <summary>
	/// C#: instance.Overlaps(other)
	/// JS: 检查是否有交集
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlySet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _520d7f31ddf30fea(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.OverlapsCore(instance, other);

	/// <summary>
	/// C#: instance.SetEquals(other)
	/// JS: 检查集合是否相等
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlySet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _eb16d835e6822ba0(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.SetEqualsCore(instance, other);
}
