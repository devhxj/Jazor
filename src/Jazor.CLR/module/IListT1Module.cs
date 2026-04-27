namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.IList&lt;T&gt; 类型模块映射规则
///
/// IList&lt;T&gt; 是泛型列表接口，映射到 JavaScript Array。
///
/// Op 类型选择原则：
/// - Import: 纯读取/查找语义，且不依赖具体可变 carrier
/// - Discard: 写入或结构变更依赖真实 list mutability，接口层不能静默假设
/// </summary>
[ECMAScriptModule("System/Collections/Generic/IListT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.IList<T>", "Array")]
public static class IListT1Module<T>
{
	/// <summary>
	/// C#: list[index]
	/// JS: array[index]
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.IList<T>.this[int].get")]
	public static T _8b52bea1dfb9f9ba(Array<T> instance, Number index)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
		if (index < 0 || index >= instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");

		return instance[index];
	}

	/// <summary>
	/// C#: list[index] = value
	/// JS: array[index] = value
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.Generic.IList<T>.this[int].set")]
	public extern static void _72c3ada14c4b312e(Array<T> instance, Number index, T value);

	/// <summary>
	/// C#: list.IndexOf(item)
	/// JS: array.indexOf(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.IList<T>.IndexOf(T)", "indexOf")]
	public extern static Number _30b27f602151f145(Array<T> instance, T item);

	/// <summary>
	/// C#: list.Insert(index, item)
	/// JS: array.splice(index, 0, item)
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.Generic.IList<T>.Insert(int, T)")]
	public extern static void _ad668b5fd142c4f4(Array<T> instance, Number index, T item);

	/// <summary>
	/// C#: list.RemoveAt(index)
	/// JS: array.splice(index, 1)
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.Generic.IList<T>.RemoveAt(int)")]
	public extern static void _d5f628d4cac6dafb(Array<T> instance, Number index);
}
