namespace Jazor.CLR;

/// <summary>
/// System.Collections.ICollection 类型模块映射规则
///
/// ICollection 是非泛型集合接口。
/// 在当前运行时边界统一投影到 JavaScript Array。
///
/// Op 类型选择原则：
/// - Alias: 直接落到稳定 carrier 成员的方法
/// - Import: 需要显式承载 CLR 语义检查的方法
/// - Discard: 同步/锁相关语义在当前运行时边界不可可靠表达
/// </summary>
[ECMAScriptModule("System/Collections/ICollectionModule.js")]
[Jazor(Op.Alias, "System.Collections.ICollection", "Array")]
public static class ICollectionModule
{
	/// <summary>
	/// C#: collection.Count
	/// JS: array.length
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.ICollection.Count.get", "length")]
	public extern static Number _8b43b08a5a1889e8(Array<object?> instance);

	/// <summary>
	/// Copies the elements of the ICollection to an Array, starting at a particular Array index.
	/// 接口层仍保留 Import，因为 CopyTo 需要显式承载 CLR 的边界检查。
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ICollection.CopyTo(System.Array, int)")]
	public static void _5d3d00c3ee9d4076(Array<object?> instance, Array<object?> array, Number index)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
		if (array is null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0 || index > array.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");
		if (index + instance.Length > array.Length)
			throw new Error("ArgumentException: Not enough space in destination array.");

		for (uint i = 0; i < instance.Length; i++)
			array[(uint)index + i] = instance[i];
	}

	[Jazor(Op.Discard, "System.Collections.ICollection.SyncRoot.get")]
	public extern static object _594fb8edb0d7b6c1(object instance);

	[Jazor(Op.Discard, "System.Collections.ICollection.IsSynchronized.get")]
	public extern static bool _65695a034b0b0a95(object instance);
}
