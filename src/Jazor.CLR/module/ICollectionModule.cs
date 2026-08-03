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
/// - Inline: CLR Array-backed ICollection 的同步属性具有稳定常量/identity 语义
/// </summary>
[ECMAScriptModule("System/Collections/ICollectionModule.js")]
[Jazor(Op.Alias, "System.Collections.ICollection", "Array")]
public static class ICollectionModule
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
		EnsureWholeNumber(index, nameof(index));
		if (index < 0 || index > array.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");
		if (index + instance.Length > array.Length)
			throw new Error("ArgumentException: Not enough space in destination array.");

		for (uint i = 0; i < instance.Length; i++)
			array[(uint)index + i] = instance[i];
	}

	[Jazor(Op.Inline, "System.Collections.ICollection.SyncRoot.get", "__arg1")]
	public extern static object _594fb8edb0d7b6c1(Array<object?> instance);

	[Jazor(Op.Inline, "System.Collections.ICollection.IsSynchronized.get", "false")]
	public extern static bool _65695a034b0b0a95(Array<object?> instance);
}
