namespace Jazor.CLR;

/// <summary>
/// System.Collections.IList 类型模块映射规则
///
/// IList 是非泛型列表接口，直接映射到 JavaScript Array。
///
/// Op 类型选择原则：
/// - Import: 纯读取/查找语义，且不依赖具体可变 carrier
/// - Discard: 写入、容量与可变性语义依赖具体实现，接口层不能静默假设
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
	[Jazor(Op.Discard, "System.Collections.IList.this[int].set")]
	public extern static void _d1d1f177e5b9f8db(Array<object?> instance, Number index, object? value);

	/// <summary>
	/// C#: list.Add(item)
	/// JS: array.push(item) 返回索引
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.IList.Add(object)")]
	public extern static Number _436bcdacebfc9159(Array<object?> instance, object? value);

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
	[Jazor(Op.Discard, "System.Collections.IList.Clear()")]
	public extern static void _00d8476a94b1a75c(Array<object?> instance);

	/// <summary>
	/// JavaScript 数组不是只读的
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.IList.IsReadOnly.get")]
	public extern static bool _2ce407a9d9be8186(Array<object?> instance);

	/// <summary>
	/// JavaScript 数组大小不固定
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.IList.IsFixedSize.get")]
	public extern static bool _b17a6c1583e0a5af(Array<object?> instance);

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
	[Jazor(Op.Discard, "System.Collections.IList.Insert(int, object)")]
	public extern static void _9e2711121aad1093(Array<object?> instance, Number index, object? value);

	/// <summary>
	/// C#: list.Remove(item)
	/// JS: 找到并删除第一个匹配项
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.IList.Remove(object)")]
	public extern static void _305c8313418aa043(Array<object?> instance, object? value);

	/// <summary>
	/// C#: list.RemoveAt(index)
	/// JS: array.splice(index, 1)
	/// </summary>
	[Jazor(Op.Discard, "System.Collections.IList.RemoveAt(int)")]
	public extern static void _72d07d6eb16afece(Array<object?> instance, Number index);
}
