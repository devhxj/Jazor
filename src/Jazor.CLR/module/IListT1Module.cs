namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.IList&lt;T&gt; 类型模块映射规则
///
/// IList&lt;T&gt; 是泛型列表接口，映射到 JavaScript Array。
///
/// Op 类型选择原则：
/// - Import: 通过 RuntimeModule 的 List carrier marker 分辨可变 List、固定数组和只读视图
/// - Discard: 仅保留尚无完整运行时协议的成员
/// </summary>
[ECMAScriptModule("System/Collections/Generic/IListT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.IList<T>", "Array")]
public static class IListT1Module<T>
{
	private static void EnsureWholeNumber(Number value, string parameterName)
	{
		if (IsNaN(value) || Math.FloorFunc(value) != value)
			throw new Error($"ArgumentOutOfRangeException: {parameterName} must be a whole number.");
	}

	/// <summary>
	/// C#: list[index]
	/// JS: array[index]
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.IList<T>.this[int].get")]
	public static T _8b52bea1dfb9f9ba(Array<T> instance, Number index)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");
		EnsureWholeNumber(index, nameof(index));
		if (index < 0 || index >= instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");

		return instance[index];
	}

	/// <summary>
	/// C#: list[index] = value
	/// JS: array[index] = value, with CLR-compatible receiver/range validation.
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.IList<T>.this[int].set")]
	public static void _72c3ada14c4b312e(Array<T> instance, Number index, T value)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");
		EnsureWholeNumber(index, nameof(index));
		if (index < 0 || index >= instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");

		// Array and List both permit indexed replacement. A ReadOnlyCollection remains a Proxy
		// carrier, so the assignment reaches its shared read-only trap instead of mutating source.
		instance[index] = value;
	}

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
	[Jazor(Op.Import, "System.Collections.Generic.IList<T>.Insert(int, T)")]
	public static void _ad668b5fd142c4f4(Array<T> instance, Number index, T item)
	{
		RuntimeModule.RequireMutableListCarrier(instance);
		ListT1Module<T>._0dc538197c677986(instance, index, item);
	}

	/// <summary>
	/// C#: list.RemoveAt(index)
	/// JS: array.splice(index, 1)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.IList<T>.RemoveAt(int)")]
	public static void _d5f628d4cac6dafb(Array<T> instance, Number index)
	{
		RuntimeModule.RequireMutableListCarrier(instance);
		ListT1Module<T>._a5e8c6b27df6470b(instance, index);
	}
}
