namespace Jazor.CLR;

/// <summary>
/// System.Collections.IList 类型模块映射规则
///
/// IList 是非泛型列表接口，直接映射到 JavaScript Array。
///
/// Op 类型选择原则：
/// - Import: 使用 List carrier marker 保留可变列表、固定数组和只读视图之间的边界
/// - Discard: 仅保留尚无完整运行时协议的成员
/// </summary>
[ECMAScriptModule("System/Collections/IListModule.js")]
[Jazor(Op.Alias, "System.Collections.IList", "Array")]
public static class IListModule
{
	private static void EnsureWholeNumber(Number value, string parameterName)
	{
		if (IsNaN(value) || Math.FloorFn(value) != value)
			throw new Error($"ArgumentOutOfRangeException: {parameterName} must be a whole number.");
	}

	private static void EnsureExistingIndex(Array<object?> instance, Number index)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");
		EnsureWholeNumber(index, nameof(index));
		if (index < 0 || index >= instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");
	}

	/// <summary>
	/// C#: list[index]
	/// JS: array[index]
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IList.this[int].get")]
	public static object? _049fed3e1cad6543(Array<object?> instance, Number index)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
		EnsureWholeNumber(index, nameof(index));
		if (index < 0 || index >= instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");

		return instance[index];
	}

	/// <summary>
	/// C#: list[index] = value
	/// JS: array[index] = value
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IList.this[int].set")]
	public static void _d1d1f177e5b9f8db(Array<object?> instance, Number index, object? value)
	{
		EnsureExistingIndex(instance, index);
		instance[index] = value;
	}

	/// <summary>
	/// C#: list.Add(item)
	/// JS: array.push(item) 返回索引
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IList.Add(object)")]
	public static Number _436bcdacebfc9159(Array<object?> instance, object? value)
	{
		RuntimeModule.RequireMutableListCarrier(instance);
		var index = instance.Length;
		ListT1Module<object?>.Add(instance, value);
		return index;
	}

	/// <summary>
	/// C#: list.Contains(item)
	/// JS: array.includes(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.IList.Contains(object)", "includes")]
	public extern static bool _1162c32e927a9e4a(Array<object?> instance, object? value);

	/// <summary>
	/// C#: list.Clear()
	/// JS: array.length = 0
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IList.Clear()")]
	public static void _00d8476a94b1a75c(Array<object?> instance)
	{
		RuntimeModule.RequireMutableListCarrier(instance);
		instance.Splice(0, instance.Length);
	}

	/// <summary>
	/// 原生数组映射 CLR T[]，因此在 IList 表面是固定大小；只有显式 List carrier 可写。
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IList.IsReadOnly.get")]
	public static bool _2ce407a9d9be8186(Array<object?> instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		return !RuntimeModule.IsMutableListCarrier(instance);
	}

	/// <summary>
	/// 非 List carrier 在 CLR IList 表面保持固定大小。
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IList.IsFixedSize.get")]
	public static bool _b17a6c1583e0a5af(Array<object?> instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		return !RuntimeModule.IsMutableListCarrier(instance);
	}

	/// <summary>
	/// C#: list.IndexOf(item)
	/// JS: array.indexOf(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.IList.IndexOf(object)", "indexOf")]
	public extern static Number _3a9e7f97e5f886b1(Array<object?> instance, object? value);

	/// <summary>
	/// C#: list.Insert(index, item)
	/// JS: array.splice(index, 0, item)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IList.Insert(int, object)")]
	public static void _9e2711121aad1093(Array<object?> instance, Number index, object? value)
	{
		RuntimeModule.RequireMutableListCarrier(instance);
		ListT1Module<object?>._0dc538197c677986(instance, index, value);
	}

	/// <summary>
	/// C#: list.Remove(item)
	/// JS: 找到并删除第一个匹配项
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IList.Remove(object)")]
	public static void _305c8313418aa043(Array<object?> instance, object? value)
	{
		RuntimeModule.RequireMutableListCarrier(instance);
		ListT1Module<object?>._562f832fd220e768(instance, value);
	}

	/// <summary>
	/// C#: list.RemoveAt(index)
	/// JS: array.splice(index, 1)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IList.RemoveAt(int)")]
	public static void _72d07d6eb16afece(Array<object?> instance, Number index)
	{
		RuntimeModule.RequireMutableListCarrier(instance);
		ListT1Module<object?>._a5e8c6b27df6470b(instance, index);
	}
}
