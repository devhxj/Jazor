namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.ICollection&lt;T&gt; 类型模块映射规则
///
/// ICollection&lt;T&gt; 是泛型集合基接口，映射到 JavaScript Array。
///
/// Op 类型选择原则：
/// - Alias: 可直接投影到 Array carrier 的稳定成员
/// - Import: 需要显式保留 CLR 语义的查询/复制成员
/// - Discard: 读写能力或可变性依赖具体 carrier，不能在接口层静默假设
/// </summary>
[ECMAScriptModule("System/Collections/Generic/ICollectionT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.ICollection<T>", "Array")]
public static class ICollectionT1Module<T>
{
	/// <summary>
	/// C#: collection.Count
	/// JS: array.length
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.ICollection<T>.Count.get", "length")]
	public extern static Number _c325d97a583f4b86(Array<T> instance);

	[Jazor(Op.Discard, "System.Collections.Generic.ICollection<T>.IsReadOnly.get")]
	public extern static bool _1257c5832793c86d(Array<T> instance);

	/// <summary>
	/// C#: collection.Add(item)
	/// JS: array.push(item)
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.Generic.ICollection<T>.Add(T)")]
	public extern static void _c0023f4a7a67220a(Array<T> instance, T item);

	/// <summary>
	/// C#: collection.Clear()
	/// JS: array.length = 0
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.Generic.ICollection<T>.Clear()")]
	public extern static void _d067c092ac624f6a(Array<T> instance);

	/// <summary>
	/// C#: collection.Contains(item)
	/// JS: array.includes(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.ICollection<T>.Contains(T)", "includes")]
	public extern static bool _f4e19820d0dc17ec(Array<T> instance, T item);

	/// <summary>
	/// C#: collection.CopyTo(array, index)
	/// JS: 循环复制
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.ICollection<T>.CopyTo(T[], int)")]
	public static void _03c4a0ae3554065f(Array<T> instance, Array<T> array, Number arrayIndex)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
		if (array is null)
			throw new Error("ArgumentNullException: array is null");
		if (arrayIndex < 0 || arrayIndex > array.Length)
			throw new Error("ArgumentOutOfRangeException: arrayIndex is out of range.");
		if (arrayIndex + instance.Length > array.Length)
			throw new Error("ArgumentException: Not enough space in destination array.");

		for (uint i = 0; i < instance.Length; i++)
			array[(uint)arrayIndex + i] = instance[i];
	}

	/// <summary>
	/// C#: collection.Remove(item)
	/// JS: 找到并删除第一个匹配项
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.Generic.ICollection<T>.Remove(T)")]
	public extern static bool _0a859d3497130ea7(Array<T> instance, T item);
}
