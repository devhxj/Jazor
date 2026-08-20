namespace Jazor.CLR;

/// <summary>
/// System.Collections.ObjectModel.ReadOnlyCollection&lt;T&gt; 类型模块映射规则
///
/// C# ReadOnlyCollection&lt;T&gt; 与 JavaScript Array 的对应关系：
/// - ReadOnlyCollection&lt;T&gt; 映射为 JavaScript Array（只读视图）
/// - 构造函数要求实时跟踪原 IList；当前 Array carrier 尚无对应 view 协议
///
/// Op 类型选择原则：
/// - Alias: JS Array 有同名方法/属性
/// - Import: 需要只读语义或完整参数校验的成员
/// - Discard: ReadOnlyCollection 特有的但 JS Array 不完全支持的功能
/// </summary>
[ECMAScriptModule("System/Collections/ObjectModel/ReadOnlyCollectionT1Module.js")]
[Jazor(Op.Alias, "System.Collections.ObjectModel.ReadOnlyCollection<T>","Array")]
public static class ReadOnlyCollectionT1Module<T>
{
	private static void EnsureWholeNumber(Number value, string parameterName)
	{
		if (IsNaN(value) || Math.FloorFunc(value) != value)
			throw new Error($"ArgumentOutOfRangeException: {parameterName} must be a whole number.");
	}

	private static void EnsureSource(Array<T> instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
	}

	private static void EnsureTarget(Array<T> array)
	{
		if (array is null)
			throw new Error("ArgumentNullException: array is null");
	}

	private static void EnsureTargetIndex(Array<T> array, Number arrayIndex)
	{
		EnsureWholeNumber(arrayIndex, nameof(arrayIndex));
		if (arrayIndex < 0 || arrayIndex > array.Length)
			throw new Error("ArgumentOutOfRangeException: arrayIndex is out of range.");
	}

	private static void EnsureCopyCapacity(Array<T> array, Number arrayIndex, Number copyCount)
	{
		if (arrayIndex + copyCount > array.Length)
			throw new Error("ArgumentException: Not enough space in destination array.");
	}

	/// <summary>
	/// C# wrapper must observe source-list changes. The shared Array Proxy protocol provides
	/// liveness without freezing or copying the source carrier.
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyCollection<T>.ReadOnlyCollection(System.Collections.Generic.IList<T>)")]
	public static System.Collections.ObjectModel.ReadOnlyCollection<T> _d4e5f6a7b8c9d0e1(Array<T>? list)
		=> (System.Collections.ObjectModel.ReadOnlyCollection<T>)(object)RuntimeModule.CreateReadOnlyArrayView(
			list,
			"ArgumentNullException: list is null.");

	/// <summary>
	/// C#: ReadOnlyCollection.Empty
	/// JS: Object.freeze([])
	/// </summary>
	[Jazor(Op.Import, "static System.Collections.ObjectModel.ReadOnlyCollection<T>.Empty.get")]
	public static Array<T> _e5f6a7b8c9d0e1f2()
		=> Object.Freeze(new Array<T>());

	/// <summary>
	/// C#: collection.Count
	/// JS: array.length
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.ObjectModel.ReadOnlyCollection<T>.Count.get", "length")]
	public extern static Number _f6a7b8c9d0e1f2a3(Array<T> instance);

	/// <summary>
	/// C#: collection.Contains(item)
	/// JS: array.includes(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.ObjectModel.ReadOnlyCollection<T>.Contains(T)", "includes")]
	public extern static bool _a7b8c9d0e1f2a3b4(Array<T> instance, T item);

	/// <summary>
	/// C#: collection[index]
	/// JS: array[index] (越界时抛出 ArgumentOutOfRangeException)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyCollection<T>.this[int].get")]
	public static T _b8c9d0e1f2a3b4c5(Array<T> instance, Number index)
	{
		EnsureSource(instance);
		EnsureWholeNumber(index, nameof(index));
		if (index < 0 || index >= instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");
		return instance[index];
	}

	/// <summary>
	/// C#: collection.IndexOf(item)
	/// JS: array.indexOf(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.ObjectModel.ReadOnlyCollection<T>.IndexOf(T)", "indexOf")]
	public extern static Number _c9d0e1f2a3b4c5d6(Array<T> instance, T item);

	/// <summary>
	/// C#: collection.CopyTo(array)
	/// JS: for循环复制
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[])")]
	public static void _d0e1f2a3b4c5d6e7(Array<T> instance, Array<T> array)
	{
		EnsureSource(instance);
		EnsureTarget(array);
		EnsureCopyCapacity(array, 0, instance.Length);

		for (uint i = 0; i < instance.Length; i++)
			array[i] = instance[i];
	}

	/// <summary>
	/// C#: collection.CopyTo(array, index)
	/// JS: for循环复制
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[], int)")]
	public static void _e1f2a3b4c5d6e7f8(Array<T> instance, Array<T> array, Number arrayIndex)
	{
		EnsureSource(instance);
		EnsureTarget(array);
		EnsureTargetIndex(array, arrayIndex);
		EnsureCopyCapacity(array, arrayIndex, instance.Length);

		for (uint i = 0; i < instance.Length; i++)
			array[(uint)arrayIndex + i] = instance[i];
	}

	/// <summary>
	/// C#: collection.CopyTo(index, array, arrayIndex, count)
	/// JS: for循环复制
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(int, T[], int, int)")]
	public static void _f2a3b4c5d6e7f8a9(Array<T> instance, Number index, Array<T> array, Number arrayIndex, Number count)
	{
		EnsureSource(instance);
		EnsureTarget(array);
		EnsureWholeNumber(index, nameof(index));
		EnsureWholeNumber(count, nameof(count));
		EnsureTargetIndex(array, arrayIndex);

		if (index < 0 || index > instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");
		if (count < 0)
			throw new Error("ArgumentOutOfRangeException: count is out of range.");
		if (index + count > instance.Length)
			throw new Error("ArgumentException: source index and count are out of range.");
		EnsureCopyCapacity(array, arrayIndex, count);

		for (uint i = 0; i < (uint)count; i++)
			array[(uint)arrayIndex + i] = instance[(uint)index + i];
	}

	///<summary>Returns an enumerator that iterates through the collection.</summary>
	[Jazor(Op.Discard, "System.Collections.ObjectModel.ReadOnlyCollection<T>.GetEnumerator()")]
	public extern static object _a3b4c5d6e7f8a9b0(Array<T> instance);

	[Jazor(Op.Import, "static System.Collections.ObjectModel.ReadOnlyCollection.CreateCollection<T>(params System.ReadOnlySpan<T>)")]
	public static Array<T> _a0cccd63a3a3eee1(Array<T> values)
		=> Object.Freeze(values.Slice());

	[Jazor(Op.Import, "static System.Collections.ObjectModel.ReadOnlyCollection.CreateSet<T>(params System.ReadOnlySpan<T>)")]
	public static Set<T> _b80678a096dde585(Array<T> values)
	{
		var result = HashSetT1Module<T>.Create(null);
		for (uint index = 0; index < values.Length; index++)
			HashSetT1Module<T>.AddCore(result, values[index]);

		return RuntimeModule.MarkAsReadOnlySetCarrier(result);
	}
}
