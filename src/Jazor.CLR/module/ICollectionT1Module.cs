namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.ICollection&lt;T&gt; 类型模块映射规则
///
/// ICollection&lt;T&gt; 是泛型集合基接口，映射到 JavaScript Array。
///
/// Op 类型选择原则：
/// - Alias: 可直接投影到 Array carrier 的稳定成员
/// - Import: 通过 List carrier marker 保留接口可变性和固定数组边界
/// - Discard: 仅保留尚无完整运行时协议的成员
/// </summary>
[ECMAScriptModule("System/Collections/Generic/ICollectionT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.ICollection<T>", "Array")]
public static class ICollectionT1Module<T>
{
	private static void EnsureWholeNumber(Number value, string parameterName)
	{
		if (IsNaN(value) || Math.FloorFn(value) != value)
			throw new Error($"ArgumentOutOfRangeException: {parameterName} must be a whole number.");
	}

	/// <summary>
	/// C#: collection.Count
	/// JS: array.length
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.ICollection<T>.Count.get", "length")]
	public extern static Number _c325d97a583f4b86(Array<T> instance);

	[Jazor(Op.Import, "System.Collections.Generic.ICollection<T>.IsReadOnly.get")]
	public static bool _1257c5832793c86d(Array<T> instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		return !RuntimeModule.IsMutableListCarrier(instance);
	}

	/// <summary>
	/// C#: collection.Add(item)
	/// JS: array.push(item)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.ICollection<T>.Add(T)")]
	public static void _c0023f4a7a67220a(Array<T> instance, T item)
	{
		RuntimeModule.RequireMutableListCarrier(instance);
		ListT1Module<T>.Add(instance, item);
	}

	/// <summary>
	/// C#: collection.Clear()
	/// JS: array.length = 0
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.ICollection<T>.Clear()")]
	public static void _d067c092ac624f6a(Array<T> instance)
	{
		RuntimeModule.RequireMutableListCarrier(instance);
		instance.Splice(0, instance.Length);
	}

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
		EnsureWholeNumber(arrayIndex, nameof(arrayIndex));
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
	[Jazor(Op.Import, "System.Collections.Generic.ICollection<T>.Remove(T)")]
	public static bool _0a859d3497130ea7(Array<T> instance, T item)
	{
		RuntimeModule.RequireMutableListCarrier(instance);
		return ListT1Module<T>._562f832fd220e768(instance, item);
	}
}
