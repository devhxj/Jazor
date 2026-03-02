namespace Jazor.CLR;

/// <summary>
/// System.Collections.ObjectModel.ReadOnlyCollection&lt;T&gt; 类型模块映射规则
///
/// C# ReadOnlyCollection&lt;T&gt; 与 JavaScript Array 的对应关系：
/// - ReadOnlyCollection&lt;T&gt; 映射为 JavaScript Array（只读视图）
/// - 底层数组不变，只是语义上只读
///
/// Op 类型选择原则：
/// - Replace: JS Array 有同名方法/属性
/// - Inline: 简单表达式
/// - Discard: ReadOnlyCollection 特有的但 JS Array 不完全支持的功能
/// </summary>
[ECMAScriptModule]
[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyCollection<T>","System/Collections/ObjectModel/ReadOnlyCollectionModule.js")]
public static class ReadOnlyCollectionModule<T>
{
	/// <summary>
	/// C#: new ReadOnlyCollection&lt;T&gt;(list)
	/// JS: list (直接使用原数组)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlyCollection<T>.ReadOnlyCollection(System.Collections.Generic.IList<T>)", "@#{0}")]
	public extern static Array<T> _d4e5f6a7b8c9d0e1(Array<T> list);

	/// <summary>
	/// C#: ReadOnlyCollection.Empty
	/// JS: []
	/// </summary>
	[Jazor(Op.Inline, "static System.Collections.ObjectModel.ReadOnlyCollection<T>.Empty.get", "[]")]
	public extern static Array<T> _e5f6a7b8c9d0e1f2();

	/// <summary>
	/// C#: collection.Count
	/// JS: array.length
	/// </summary>
	[Jazor(Op.Replace, "System.Collections.ObjectModel.ReadOnlyCollection<T>.Count.get", "length")]
	public extern static Number _f6a7b8c9d0e1f2a3(Array<T> instance);

	/// <summary>
	/// C#: collection.Contains(item)
	/// JS: array.includes(item)
	/// </summary>
	[Jazor(Op.Replace, "System.Collections.ObjectModel.ReadOnlyCollection<T>.Contains(T)", "includes")]
	public extern static bool _a7b8c9d0e1f2a3b4(Array<T> instance, T item);

	/// <summary>
	/// C#: collection[index]
	/// JS: array[index]
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.ObjectModel.ReadOnlyCollection<T>.this[int].get", "@#{0}[@#{1}]")]
	public extern static T _b8c9d0e1f2a3b4c5(Array<T> instance, Number index);

	/// <summary>
	/// C#: collection.IndexOf(item)
	/// JS: array.indexOf(item)
	/// </summary>
	[Jazor(Op.Replace, "System.Collections.ObjectModel.ReadOnlyCollection<T>.IndexOf(T)", "indexOf")]
	public extern static Number _c9d0e1f2a3b4c5d6(Array<T> instance, T item);

	/// <summary>
	/// C#: collection.CopyTo(array)
	/// JS: for循环复制
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[])")]
	public static void _d0e1f2a3b4c5d6e7(Array<T> instance, Array<T> array)
	{
		for (uint i = 0; i < instance.Length; i++)
			array.Push(instance[i]);
	}

	/// <summary>
	/// C#: collection.CopyTo(array, index)
	/// JS: for循环复制
	/// </summary>
	[Jazor(Op.Import, "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[], int)")]
	public static void _e1f2a3b4c5d6e7f8(Array<T> instance, Array<T> array, Number arrayIndex)
	{
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
		for (uint i = 0; i < (uint)count; i++)
			array[(uint)arrayIndex + i] = instance[(uint)index + i];
	}

	///<summary>Returns an enumerator that iterates through the collection.</summary>
	[Jazor(Op.Discard, "System.Collections.ObjectModel.ReadOnlyCollection<T>.GetEnumerator()")]
	public extern static object _a3b4c5d6e7f8a9b0(Array<T> instance);

	[Jazor(Op.Discard, "static System.Collections.ObjectModel.ReadOnlyCollection.CreateCollection<T>(params System.ReadOnlySpan<T>)")]
	public extern static Array<T> _a0cccd63a3a3eee1(object values);

	[Jazor(Op.Discard, "static System.Collections.ObjectModel.ReadOnlyCollection.CreateSet<T>(params System.ReadOnlySpan<T>)")]
	public extern static System.Collections.ObjectModel.ReadOnlySet<T> _b80678a096dde585(object values);
}
