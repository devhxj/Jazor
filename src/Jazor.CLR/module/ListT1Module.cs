namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.List&lt;T&gt; 类型模块映射规则
///
/// C# List&lt;T&gt; 与 JavaScript Array 的对应关系：
/// - List&lt;T&gt; 映射为 JavaScript Array
/// - 大多数方法可以直接映射
///
/// Op 类型选择原则：
/// - Alias: JS Array 有同名方法
/// - Inline: 简单表达式
/// - Import: 需要额外逻辑的方法
/// - Discard: List 特有但 JS Array 不完全支持的功能
/// </summary>
[ECMAScriptModule("System/Collections/Generic/ListT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.List<T>","Array")]
public static class ListT1Module<T>
{
	private static void EnsureWholeNumber(Number value, string message)
	{
		if (IsNaN(value) || Math.Floor_(value) != value)
			throw new Error(message);
	}

	private static void EnsureInsertIndex(Array<T> instance, Number index)
	{
		EnsureWholeNumber(index, "ArgumentOutOfRangeException: index must be a whole number.");
		if (index < 0 || index > instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");
	}

	private static void EnsureExistingIndex(Array<T> instance, Number index)
	{
		EnsureWholeNumber(index, "ArgumentOutOfRangeException: index must be a whole number.");
		if (index < 0 || index >= instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");
	}

	private static void EnsureRemoveRange(Array<T> instance, Number index, Number count)
	{
		EnsureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");
		EnsureInsertIndex(instance, index);
		if (count < 0 || index + count > instance.Length)
			throw new Error("ArgumentException: offset and length were out of bounds for the list.");
	}

	// Keep list mutations in imports so index validation and iteration stay visible in Jazor code
	// instead of being hidden inside JS string templates.
	private static void AppendRange(Array<T> instance, IEnumerable<T> collection)
	{
		foreach (var item in collection)
			instance.Push(item);
	}

	/// <summary>
	/// C#: new List&lt;T&gt;()
	/// JS: []
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.List<T>.List()", "[]")]
	public extern static Array<T> _01dceb3b4d503bbf();

	/// <summary>
	/// C#: new List&lt;T&gt;(capacity)
	/// JS: new Array(capacity) 或 []
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.List<T>.List(int)", "[]")]
	public extern static Array<T> _feacfe24abeee54b(Number capacity);

	/// <summary>
	/// C#: new List&lt;T&gt;(collection)
	/// JS: Array.from(collection)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.List<T>.List(System.Collections.Generic.IEnumerable<T>)", "Array.from(__arg1)")]
	public extern static Array<T> _ea4c991aac8688c0(Array<T> collection);

	[Jazor(Op.Discard, "System.Collections.Generic.List<T>.Capacity.get")]
	public extern static Number _ffa580d06e0078ae(Array<T> instance);

	[Jazor(Op.Discard, "System.Collections.Generic.List<T>.Capacity.set")]
	public extern static void _db03a5f0f4bc11af(Array<T> instance, Number value);

	/// <summary>
	/// C#: list.Count
	/// JS: array.length
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.Count.get", "length")]
	public extern static Number _a2137cdeeb85f3d9(Array<T> instance);

	/// <summary>
	/// C#: list[index]
	/// JS: array[index] (越界时抛出 ArgumentOutOfRangeException)
	/// 当前仍保留 Import：语义关键点不是下标访问本身，而是越界即 throw。
	/// 在 Compile 只有表达式级 contract 的前提下，这类索引器先不要硬迁。
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.this[int].get")]
	public static T _d389c31d59037b42(Array<T> instance, Number index)
	{
		if (index < 0 || index >= instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");
		return instance[index];
	}

	/// <summary>
	/// C#: list[index] = value
	/// JS: array[index] = value
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.List<T>.this[int].set", "(__arg1[__arg2] = __arg3)")]
	public extern static void _c16a7960302ea054(Array<T> instance, Number index, T value);

	/// <summary>
	/// C#: list.Add(item)
	/// JS: array.push(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.Add(T)", "push")]
	public extern static void _342f4a7099c7ddf0(Array<T> instance, T item);

	/// <summary>
	/// C#: list.AddRange(collection)
	/// JS: array.push(...collection)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.AddRange(System.Collections.Generic.IEnumerable<T>)")]
	public static void _a2660853a4ebc1f6(Array<T> instance, IEnumerable<T> collection)
		=> AppendRange(instance, collection);

	[Jazor(Op.Discard, "System.Collections.Generic.List<T>.AsReadOnly()")]
	public extern static Array<T> _f7981b5a4cd02bdb(Array<T> instance);

	[Jazor(Op.Discard, "System.Collections.Generic.List<T>.BinarySearch(int, int, T, System.Collections.Generic.IComparer<T>)")]
	public extern static Number _95ada27dd960bae5(Array<T> instance, Number index, Number count, T item, IComparer<T>? comparer);

	[Jazor(Op.Discard, "System.Collections.Generic.List<T>.BinarySearch(T)")]
	public extern static Number _3d21965eedc9916f(Array<T> instance, T item);

	[Jazor(Op.Discard, "System.Collections.Generic.List<T>.BinarySearch(T, System.Collections.Generic.IComparer<T>)")]
	public extern static Number _65e239056cc65177(Array<T> instance, T item, IComparer<T>? comparer);

	/// <summary>
	/// C#: list.Clear()
	/// JS: array.length = 0
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.List<T>.Clear()", "(__arg1.length = 0)")]
	public extern static void _7de26e55010ee1a8(Array<T> instance);

	/// <summary>
	/// C#: list.Contains(item)
	/// JS: array.includes(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.Contains(T)", "includes")]
	public extern static bool _d9fab27c685b7de9(Array<T> instance, T item);

	[Jazor(Op.Discard, "System.Collections.Generic.List<T>.ConvertAll<TOutput>(System.Converter<T, TOutput>)")]
	public extern static Array<TOutput> _098c2e027f3a5996<TOutput>(Array<T> instance, object converter);

	/// <summary>
	/// C#: list.CopyTo(array)
	/// JS: array.push(...list)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.CopyTo(T[])")]
	public static void _9a3a4817585dded1(Array<T> instance, Array<T> array)
	{
		for (uint i = 0; i < instance.Length; i++)
			array.Push(instance[i]);
	}

	/// <summary>
	/// C#: list.CopyTo(index, array, arrayIndex, count)
	/// JS: for 循环复制
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.CopyTo(int, T[], int, int)")]
	public static void _0fdf1627d283f8ae(Array<T> instance, Number index, Array<T> array, Number arrayIndex, Number count)
	{
		for (uint i = 0; i < (uint)count; i++)
			array[(uint)arrayIndex + i] = instance[(uint)index + i];
	}

	/// <summary>
	/// C#: list.CopyTo(array, arrayIndex)
	/// JS: for 循环复制
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.CopyTo(T[], int)")]
	public static void _3559b1ff2a643922(Array<T> instance, Array<T> array, Number arrayIndex)
	{
		for (uint i = 0; i < instance.Length; i++)
			array[(uint)arrayIndex + i] = instance[i];
	}

	[Jazor(Op.Discard, "System.Collections.Generic.List<T>.EnsureCapacity(int)")]
	public extern static Number _6dffb0ed23f010e0(Array<T> instance, Number capacity);

	/// <summary>
	/// C#: list.Exists(match)
	/// JS: array.some(match)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.Exists(System.Predicate<T>)", "some")]
	public extern static bool _b23997dd4232ced6(Array<T> instance, Predicate<T> match);

	/// <summary>
	/// C#: list.Find(match)
	/// JS: array.find(match)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.Find(System.Predicate<T>)", "find")]
	public extern static T? _089a5c28e11eeeaf(Array<T> instance, Predicate<T> match);

	/// <summary>
	/// C#: list.FindAll(match)
	/// JS: array.filter(match)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.FindAll(System.Predicate<T>)", "filter")]
	public extern static Array<T> _d8e500da425f2be5(Array<T> instance, Predicate<T> match);

	/// <summary>
	/// C#: list.FindIndex(match)
	/// JS: array.findIndex(match)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.FindIndex(System.Predicate<T>)", "findIndex")]
	public extern static Number _4770bba04510e57b(Array<T> instance, Predicate<T> match);

	/// <summary>
	/// C#: list.FindIndex(startIndex, match)
	/// JS: array.findIndex((x, i) => i >= startIndex && match(x))
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.FindIndex(int, System.Predicate<T>)")]
	public static Number _db9b68fbc73e342b(Array<T> instance, Number startIndex, Predicate<T> match)
	{
		for (int i = (int)startIndex; i < (int)instance.Length; i++)
		{
			if (match(instance[(uint)i]))
				return i;
		}
		return -1;
	}

	/// <summary>
	/// C#: list.FindIndex(startIndex, count, match)
	/// JS: 搜索指定范围
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.FindIndex(int, int, System.Predicate<T>)")]
	public static Number _41b337b09c5daf75(Array<T> instance, Number startIndex, Number count, Predicate<T> match)
	{
		int end = Math.Min((int)startIndex + (int)count, (int)instance.Length);
		for (int i = (int)startIndex; i < end; i++)
		{
			if (match(instance[(uint)i]))
				return i;
		}
		return -1;
	}

	/// <summary>
	/// C#: list.FindLast(match)
	/// JS: 从后向前搜索
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.FindLast(System.Predicate<T>)")]
	public static T? _de0943e496e36f2d(Array<T> instance, Predicate<T> match)
	{
		for (uint i = instance.Length; i > 0; i--)
		{
			if (match(instance[i - 1]))
				return instance[i - 1];
		}
		return default;
	}

	/// <summary>
	/// C#: list.FindLastIndex(match)
	/// JS: 从后向前搜索返回索引
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.FindLastIndex(System.Predicate<T>)")]
	public static Number _ae1a0b59c73f2b1a(Array<T> instance, Predicate<T> match)
	{
		for (uint i = instance.Length; i > 0; i--)
		{
			if (match(instance[i - 1]))
				return (int)(i - 1);
		}
		return -1;
	}

	/// <summary>
	/// C#: list.FindLastIndex(startIndex, match)
	/// JS: 从指定位置向后搜索
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.FindLastIndex(int, System.Predicate<T>)")]
	public static Number _081aa9ae0b09d058(Array<T> instance, Number startIndex, Predicate<T> match)
	{
		for (int i = (int)startIndex; i >= 0; i--)
		{
			if (match(instance[(uint)i]))
				return i;
		}
		return -1;
	}

	/// <summary>
	/// C#: list.FindLastIndex(startIndex, count, match)
	/// JS: 搜索指定范围
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.FindLastIndex(int, int, System.Predicate<T>)")]
	public static Number _58cc54dc07e440c4(Array<T> instance, Number startIndex, Number count, Predicate<T> match)
	{
		int start = (int)startIndex - (int)count + 1;
		if (start < 0) start = 0;
		for (int i = (int)startIndex; i >= start; i--)
		{
			if (match(instance[(uint)i]))
				return i;
		}
		return -1;
	}

	/// <summary>
	/// C#: list.ForEach(action)
	/// JS: array.forEach(action)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.ForEach(System.Action<T>)", "forEach")]
	public extern static void _7395d2cfe6dce3fb(Array<T> instance, Action<T> action);

	[Jazor(Op.Discard, "System.Collections.Generic.List<T>.GetEnumerator()")]
	public extern static object _b9724d52a219e3b6(Array<T> instance);

	/// <summary>
	/// C#: list.GetRange(index, count)
	/// JS: array.slice(index, index + count)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.List<T>.GetRange(int, int)", "__arg1.slice(__arg2, __arg2 + __arg3)")]
	public extern static Array<T> _c35c9c99a23ff96a(Array<T> instance, Number index, Number count);

	/// <summary>
	/// C#: list.Slice(start, length)
	/// JS: array.slice(start, start + length)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.List<T>.Slice(int, int)", "__arg1.slice(__arg2, __arg2 + __arg3)")]
	public extern static Array<T> _adcf2df90da54ec8(Array<T> instance, Number start, Number length);

	/// <summary>
	/// C#: list.IndexOf(item)
	/// JS: array.indexOf(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.IndexOf(T)", "indexOf")]
	public extern static Number _2bb4b70655cede73(Array<T> instance, T item);

	/// <summary>
	/// C#: list.IndexOf(item, index)
	/// JS: array.indexOf(item, index)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.IndexOf(T, int)", "indexOf")]
	public extern static Number _71ee35e0e260eb27(Array<T> instance, T item, Number index);

	/// <summary>
	/// C#: list.IndexOf(item, index, count)
	/// JS: 在范围内搜索
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.IndexOf(T, int, int)")]
	public static Number _5ee52e4e4fc54e6d(Array<T> instance, T item, Number index, Number count)
	{
		uint end = (uint)Math.Min((int)index + (int)count, (int)instance.Length);
		for (uint i = (uint)index; i < end; i++)
		{
			if (EqualityComparer<T>.Default.Equals(instance[i], item))
				return (int)i;
		}
		return -1;
	}

	/// <summary>
	/// C#: list.Insert(index, item)
	/// JS: array.splice(index, 0, item)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.Insert(int, T)")]
	public static void _0dc538197c677986(Array<T> instance, Number index, T item)
	{
		EnsureInsertIndex(instance, index);
		instance.Splice(index, 0, item);
	}

	/// <summary>
	/// C#: list.InsertRange(index, collection)
	/// JS: array.splice(index, 0, ...collection)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.InsertRange(int, System.Collections.Generic.IEnumerable<T>)")]
	public static void _56ef9aefabac7c09(Array<T> instance, Number index, IEnumerable<T> collection)
	{
		EnsureInsertIndex(instance, index);

		var insertionIndex = index;
		foreach (var item in collection)
		{
			instance.Splice(insertionIndex, 0, item);
			insertionIndex++;
		}
	}

	/// <summary>
	/// C#: list.LastIndexOf(item)
	/// JS: array.lastIndexOf(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.LastIndexOf(T)", "lastIndexOf")]
	public extern static Number _121df07eb2f61749(Array<T> instance, T item);

	/// <summary>
	/// C#: list.LastIndexOf(item, index)
	/// JS: array.lastIndexOf(item, index)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.LastIndexOf(T, int)", "lastIndexOf")]
	public extern static Number _279befda6399cda5(Array<T> instance, T item, Number index);

	/// <summary>
	/// C#: list.LastIndexOf(item, index, count)
	/// JS: 在范围内反向搜索
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.LastIndexOf(T, int, int)")]
	public static Number _b2f1955b62962812(Array<T> instance, T item, Number index, Number count)
	{
		int start = (int)index - (int)count + 1;
		if (start < 0) start = 0;
		for (int i = (int)index; i >= start; i--)
		{
			if (EqualityComparer<T>.Default.Equals(instance[(uint)i], item))
				return i;
		}
		return -1;
	}

	/// <summary>
	/// C#: list.Remove(item)
	/// JS: 找到并删除
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.Remove(T)")]
	public static bool _562f832fd220e768(Array<T> instance, T item)
	{
		Number index = instance.IndexOf(item, null);
		if ((double)index >= 0)
		{
			instance.Splice(index, 1);
			return true;
		}
		return false;
	}

	/// <summary>
	/// C#: list.RemoveAll(match)
	/// JS: filter 并重新赋值
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.RemoveAll(System.Predicate<T>)")]
	public static Number _b864beda26f186e2(Array<T> instance, Predicate<T> match)
	{
		int count = 0;
		for (uint i = instance.Length; i > 0; i--)
		{
			if (match(instance[i - 1]))
			{
				instance.Splice(i - 1, 1);
				count++;
			}
		}
		return count;
	}

	/// <summary>
	/// C#: list.RemoveAt(index)
	/// JS: array.splice(index, 1)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.RemoveAt(int)")]
	public static void _a5e8c6b27df6470b(Array<T> instance, Number index)
	{
		EnsureExistingIndex(instance, index);
		instance.Splice(index, 1);
	}

	/// <summary>
	/// C#: list.RemoveRange(index, count)
	/// JS: array.splice(index, count)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.RemoveRange(int, int)")]
	public static void _8425758ef4e7b6f9(Array<T> instance, Number index, Number count)
	{
		EnsureRemoveRange(instance, index, count);
		if (count == 0)
			return;
		instance.Splice(index, count);
	}

	/// <summary>
	/// C#: list.Reverse()
	/// JS: array.reverse()
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.Reverse()", "reverse")]
	public extern static void _8a13946a926a97b2(Array<T> instance);

	/// <summary>
	/// C#: list.Reverse(index, count)
	/// JS: 子数组反转
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.Reverse(int, int)")]
	public static void _56dc1af8af32e484(Array<T> instance, Number index, Number count)
	{
		uint start = (uint)index;
		uint end = (uint)((int)index + (int)count - 1);
		while (start < end)
		{
			var temp = instance[start];
			instance[start] = instance[end];
			instance[end] = temp;
			start++;
			end--;
		}
	}

	/// <summary>
	/// C#: list.Sort()
	/// JS: array.sort()
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.Sort()", "sort")]
	public extern static void _36a478f36b41a6d2(Array<T> instance);

	/// <summary>
	/// C#: list.Sort(comparer)
	/// JS: array.sort((a, b) => comparer(a, b))
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.Sort(System.Collections.Generic.IComparer<T>)", "sort")]
	public extern static void _5fa599e721e252ff(Array<T> instance, IComparer<T>? comparer);

	/// <summary>
	/// C#: list.Sort(index, count, comparer)
	/// JS: 子数组排序
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.Sort(int, int, System.Collections.Generic.IComparer<T>)")]
	public static void _19207851b52a5287(Array<T> instance, Number index, Number count, IComparer<T>? comparer)
	{
		var subArray = instance.Slice((int)index, (int)index + (int)count);
		if (comparer != null)
			subArray.Sort((a, b) => comparer.Compare(a, b));
		else
			subArray.Sort();
		for (uint i = 0; i < (uint)count; i++)
			instance[(uint)index + i] = subArray[i];
	}

	/// <summary>
	/// C#: list.Sort(comparison)
	/// JS: array.sort(comparison)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.Sort(System.Comparison<T>)", "sort")]
	public extern static void _0d91dcbccdea7c8c(Array<T> instance, Comparison<T> comparison);

	/// <summary>
	/// C#: list.ToArray()
	/// JS: array.slice()
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.ToArray()", "slice")]
	public extern static Array<T> _eedb6fcf490f54cb(Array<T> instance);

	[Jazor(Op.Discard, "System.Collections.Generic.List<T>.TrimExcess()")]
	public extern static void _27c95e83eced65e9(Array<T> instance);

	/// <summary>
	/// C#: list.TrueForAll(match)
	/// JS: array.every(match)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.TrueForAll(System.Predicate<T>)", "every")]
	public extern static bool _d12a4656f219490c(Array<T> instance, Predicate<T> match);
}
