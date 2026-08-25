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
	private static readonly Number MaxListCapacity = 2147483591;
	private static readonly WeakMap<Array<T>, Number> Capacities = new();

	[ECMAScriptInline("null")]
	private extern static T? MissingValue();

	private static void EnsureInstance(Array<T> instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
	}

	private static Number GetCapacity(Array<T> instance)
	{
		EnsureInstance(instance);
		if (!Capacities.Has(instance))
			Capacities.Set(instance, instance.Length);
		return Capacities.Get(instance)!;
	}

	private static Array<T> CreateWithCapacity(Number capacity)
	{
		EnsureWholeNumber(capacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
		if (capacity < 0)
			throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");
		if (capacity > MaxListCapacity)
			throw new Error("OutOfMemoryException: requested list capacity is too large.");

		var instance = RuntimeModule.MarkAsMutableListCarrier(new Array<T>());
		Capacities.Set(instance, capacity);
		return instance;
	}

	private static Number ExpandCapacity(Number currentCapacity, Number requiredCapacity)
	{
		if (requiredCapacity > MaxListCapacity)
			throw new Error("OutOfMemoryException: requested list capacity is too large.");

		Number expanded = currentCapacity == 0 ? 4 : currentCapacity * 2;
		if (expanded > MaxListCapacity)
			expanded = MaxListCapacity;
		return expanded < requiredCapacity ? requiredCapacity : expanded;
	}

	private static Number EnsureCapacityCore(Array<T> instance, Number capacity)
	{
		EnsureWholeNumber(capacity, "ArgumentOutOfRangeException: capacity must be a whole number.");
		if (capacity < 0)
			throw new Error("ArgumentOutOfRangeException: capacity must be non-negative.");

		var current = GetCapacity(instance);
		if (capacity <= current)
			return current;

		var expanded = ExpandCapacity(current, capacity);
		Capacities.Set(instance, expanded);
		return expanded;
	}

	private static void AddCore(Array<T> instance, T item)
	{
		EnsureInstance(instance);
		EnsureCapacityCore(instance, instance.Length + 1);
		instance.Push(item);
	}

	private static Array<T> CreateFrom(IEnumerable<T> collection)
	{
		if (collection is null)
			throw new Error("ArgumentNullException: collection is null.");

		var capacity = collection is Array<T> array
			? array.Length
			: collection is Set<T> set
				? set.Size
				: 0;
		var result = CreateWithCapacity(capacity);
		foreach (var item in collection)
			AddCore(result, item);
		return result;
	}

	private static void EnsureWholeNumber(Number value, string message)
	{
		if (IsNaN(value) || Math.FloorFunc(value) != value)
			throw new Error(message);
	}

	private static void EnsureTargetArray(Array<T> array)
	{
		if (array is null)
			throw new Error("ArgumentNullException: array is null");
	}

	private static void EnsureTargetIndex(Array<T> array, Number arrayIndex)
	{
		EnsureWholeNumber(arrayIndex, "ArgumentOutOfRangeException: arrayIndex must be a whole number.");
		if (arrayIndex < 0 || arrayIndex > array.Length)
			throw new Error("ArgumentOutOfRangeException: arrayIndex is out of range.");
	}

	private static void EnsureCopyCapacity(Array<T> array, Number arrayIndex, Number copyCount)
	{
		if (arrayIndex + copyCount > array.Length)
			throw new Error("ArgumentException: Not enough space in destination array.");
	}

	private static void EnsureInsertIndex(Array<T> instance, Number index)
	{
		EnsureInstance(instance);
		EnsureWholeNumber(index, "ArgumentOutOfRangeException: index must be a whole number.");
		if (index < 0 || index > instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");
	}

	private static void EnsureExistingIndex(Array<T> instance, Number index)
	{
		EnsureInstance(instance);
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

	private static void EnsureMatch(Predicate<T> match)
	{
		if (match is null)
			throw new Error("ArgumentNullException: match is null");
	}

	private static void EnsureForwardSearchStartIndex(Array<T> instance, Number startIndex)
	{
		EnsureInsertIndex(instance, startIndex);
	}

	private static void EnsureForwardSearchRange(Array<T> instance, Number startIndex, Number count)
	{
		EnsureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");
		EnsureForwardSearchStartIndex(instance, startIndex);
		if (count < 0 || startIndex + count > instance.Length)
			throw new Error("ArgumentOutOfRangeException: count is out of range.");
	}

	private static void EnsureLastSearchStartIndex(Array<T> instance, Number startIndex, string parameterName)
	{
		EnsureInstance(instance);
		EnsureWholeNumber(startIndex, $"ArgumentOutOfRangeException: {parameterName} must be a whole number.");

		if (instance.Length == 0)
		{
			if (startIndex != -1)
				throw new Error($"ArgumentOutOfRangeException: {parameterName} is out of range.");
			return;
		}

		if (startIndex < 0 || startIndex >= instance.Length)
			throw new Error($"ArgumentOutOfRangeException: {parameterName} is out of range.");
	}

	private static void EnsureLastSearchRange(Array<T> instance, Number startIndex, Number count, string startIndexName)
	{
		EnsureLastSearchStartIndex(instance, startIndex, startIndexName);
		EnsureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");

		if (count < 0)
			throw new Error("ArgumentOutOfRangeException: count is out of range.");

		if (instance.Length == 0)
		{
			if (count != 0)
				throw new Error("ArgumentOutOfRangeException: count is out of range.");
			return;
		}

		if (count > startIndex + 1)
			throw new Error("ArgumentOutOfRangeException: count is out of range.");
	}

	private static Number CompareWith(IComparer<T>? comparer, T left, T right)
		=> comparer is null
			? ComparerT1Module<T>.CompareCore(left, right)
			: comparer.Compare(left, right);

	private static Number BinarySearchCore(
		Array<T> instance,
		Number index,
		Number count,
		T item,
		IComparer<T>? comparer)
	{
		EnsureRemoveRange(instance, index, count);

		var lower = index;
		var upper = index + count - 1;
		while (lower <= upper)
		{
			var midpoint = lower + Math.FloorFunc((upper - lower) / 2);
			var comparison = CompareWith(comparer, instance[midpoint], item);
			if (comparison == 0)
				return midpoint;
			if (comparison < 0)
				lower = midpoint + 1;
			else
				upper = midpoint - 1;
		}

		return ~lower;
	}

	// Avoid depending on EqualityComparer<T> CLR coverage inside runtime modules.
	// Keep list search/remove equality aligned to JS SameValueZero-like behavior.
	private static bool EqualsForListSearch(T left, T right)
	{
		if (Object.Is(left, right))
			return true;

		if (left is Number leftNumber && right is Number rightNumber)
			return leftNumber == rightNumber;

		return false;
	}

	// Keep list mutations in imports so index validation and iteration stay visible in Jazor code
	// instead of being hidden inside JS string templates.
	private static void AppendRange(Array<T> instance, IEnumerable<T> collection)
	{
		EnsureInstance(instance);
		if (collection is null)
			throw new Error("ArgumentNullException: collection is null");

		// .NET List.AddRange supports self-add. Snapshot first to avoid iterating a collection
		// that is being mutated during the same loop.
		if (collection is Array<T> source && Object.Is(instance, source))
		{
			var originalLength = source.Length;
			for (uint i = 0; i < originalLength; i++)
				AddCore(instance, source[i]);
			return;
		}

		foreach (var item in collection)
			AddCore(instance, item);
	}

	/// <summary>
	/// C#: new List&lt;T&gt;()
	/// JS: []
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.List()", "createDefault")]
	public static Array<T> CreateDefault()
		=> CreateWithCapacity(0);

	/// <summary>
	/// C#: new List&lt;T&gt;(capacity)
	/// JS: new Array(capacity) 或 []
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.List(int)", "createWithInitialCapacity")]
	public static Array<T> CreateWithInitialCapacity(Number capacity)
		=> CreateWithCapacity(capacity);

	/// <summary>
	/// C#: new List&lt;T&gt;(collection)
	/// JS: Array.from(collection)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.List(System.Collections.Generic.IEnumerable<T>)", "createFromCollection")]
	public static Array<T> CreateFromCollection(IEnumerable<T> collection)
		=> CreateFrom(collection);

	[Jazor(Op.Import, "System.Collections.Generic.List<T>.Capacity.get", "getCapacityMember")]
	public static Number GetCapacityMember(Array<T> instance)
		=> GetCapacity(instance);

	[Jazor(Op.Import, "System.Collections.Generic.List<T>.Capacity.set", "setCapacity")]
	public static void SetCapacity(Array<T> instance, Number value)
	{
		EnsureWholeNumber(value, "ArgumentOutOfRangeException: capacity must be a whole number.");
		if (value < instance.Length)
			throw new Error("ArgumentOutOfRangeException: capacity cannot be less than Count.");
		if (value > MaxListCapacity)
			throw new Error("OutOfMemoryException: requested list capacity is too large.");
		GetCapacity(instance);
		Capacities.Set(instance, value);
	}

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
		EnsureExistingIndex(instance, index);
		return instance[index];
	}

	/// <summary>
	/// C#: list[index] = value
	/// JS: array[index] = value (越界时抛出 ArgumentOutOfRangeException)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.this[int].set")]
	public static void _c16a7960302ea054(Array<T> instance, Number index, T value)
	{
		EnsureExistingIndex(instance, index);
		instance[index] = value;
	}

	/// <summary>
	/// C#: list.Add(item)
	/// JS: array.push(item)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.Add(T)", "add")]
	public static void Add(Array<T> instance, T item)
		=> AddCore(instance, item);

	/// <summary>
	/// C#: list.AddRange(collection)
	/// JS: array.push(...collection)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.AddRange(System.Collections.Generic.IEnumerable<T>)")]
	public static void _a2660853a4ebc1f6(Array<T> instance, IEnumerable<T> collection)
		=> AppendRange(instance, collection);

	/// <summary>
	/// Produces a live read-only view over the List carrier through the shared RuntimeModule
	/// Array Proxy protocol. Source mutations remain visible; view mutations fail.
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.AsReadOnly()")]
	public static System.Collections.ObjectModel.ReadOnlyCollection<T> _f7981b5a4cd02bdb(Array<T> instance)
	{
		EnsureInstance(instance);
		return (System.Collections.ObjectModel.ReadOnlyCollection<T>)(object)RuntimeModule.CreateReadOnlyArrayView(
			instance,
			"NullReferenceException: instance is null.");
	}

	[Jazor(Op.Import, "System.Collections.Generic.List<T>.BinarySearch(int, int, T, System.Collections.Generic.IComparer<T>)")]
	public static Number _95ada27dd960bae5(Array<T> instance, Number index, Number count, T item, IComparer<T>? comparer)
		=> BinarySearchCore(instance, index, count, item, comparer);

	[Jazor(Op.Import, "System.Collections.Generic.List<T>.BinarySearch(T)")]
	public static Number _3d21965eedc9916f(Array<T> instance, T item)
	{
		EnsureInstance(instance);
		return BinarySearchCore(instance, 0, instance.Length, item, comparer: null);
	}

	[Jazor(Op.Import, "System.Collections.Generic.List<T>.BinarySearch(T, System.Collections.Generic.IComparer<T>)")]
	public static Number _65e239056cc65177(Array<T> instance, T item, IComparer<T>? comparer)
	{
		EnsureInstance(instance);
		return BinarySearchCore(instance, 0, instance.Length, item, comparer);
	}

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

	[Jazor(Op.Import, "System.Collections.Generic.List<T>.ConvertAll<TOutput>(System.Converter<T, TOutput>)")]
	public static Array<TOutput> _098c2e027f3a5996<TOutput>(Array<T> instance, Func<T, TOutput> converter)
	{
		EnsureInstance(instance);
		if (converter is null)
			throw new Error("ArgumentNullException: converter is null.");

		// ConvertAll materializes exactly one output per source item. The generic output carrier
		// lazily observes its Length as capacity, which matches List<TOutput>'s construction size.
		return RuntimeModule.MarkAsMutableListCarrier(instance.Map(converter));
	}

	/// <summary>
	/// C#: list.CopyTo(array)
	/// JS: array.push(...list)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.CopyTo(T[])")]
	public static void _9a3a4817585dded1(Array<T> instance, Array<T> array)
	{
		EnsureInstance(instance);
		EnsureTargetArray(array);
		EnsureCopyCapacity(array, 0, instance.Length);

		for (uint i = 0; i < instance.Length; i++)
			array[i] = instance[i];
	}

	/// <summary>
	/// C#: list.CopyTo(index, array, arrayIndex, count)
	/// JS: for 循环复制
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.CopyTo(int, T[], int, int)")]
	public static void _0fdf1627d283f8ae(Array<T> instance, Number index, Array<T> array, Number arrayIndex, Number count)
	{
		EnsureInstance(instance);
		EnsureTargetArray(array);
		EnsureWholeNumber(index, "ArgumentOutOfRangeException: index must be a whole number.");
		EnsureWholeNumber(count, "ArgumentOutOfRangeException: count must be a whole number.");
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

	/// <summary>
	/// C#: list.CopyTo(array, arrayIndex)
	/// JS: for 循环复制
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.CopyTo(T[], int)")]
	public static void _3559b1ff2a643922(Array<T> instance, Array<T> array, Number arrayIndex)
	{
		EnsureInstance(instance);
		EnsureTargetArray(array);
		EnsureTargetIndex(array, arrayIndex);
		EnsureCopyCapacity(array, arrayIndex, instance.Length);

		for (uint i = 0; i < instance.Length; i++)
			array[(uint)arrayIndex + i] = instance[i];
	}

	[Jazor(Op.Import, "System.Collections.Generic.List<T>.EnsureCapacity(int)", "ensureCapacity")]
	public static Number EnsureCapacity(Array<T> instance, Number capacity)
		=> EnsureCapacityCore(instance, capacity);

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
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.FindAll(System.Predicate<T>)", "findAll")]
	public static Array<T> FindAll(Array<T> instance, Predicate<T> match)
	{
		EnsureInstance(instance);
		EnsureMatch(match);
		var result = CreateWithCapacity(0);
		for (uint index = 0; index < instance.Length; index++)
		{
			if (match(instance[index]))
				AddCore(result, instance[index]);
		}
		return result;
	}

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
		EnsureMatch(match);
		EnsureForwardSearchStartIndex(instance, startIndex);
		if (startIndex == instance.Length)
			return -1;

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
		EnsureMatch(match);
		EnsureForwardSearchRange(instance, startIndex, count);
		if (count == 0)
			return -1;

		int end = (int)startIndex + (int)count;
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
		EnsureInstance(instance);
		EnsureMatch(match);

		for (uint i = instance.Length; i > 0; i--)
		{
			if (match(instance[i - 1]))
				return instance[i - 1];
		}
		return MissingValue();
	}

	/// <summary>
	/// C#: list.FindLastIndex(match)
	/// JS: 从后向前搜索返回索引
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.FindLastIndex(System.Predicate<T>)")]
	public static Number _ae1a0b59c73f2b1a(Array<T> instance, Predicate<T> match)
	{
		EnsureInstance(instance);
		EnsureMatch(match);

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
		EnsureMatch(match);
		EnsureLastSearchStartIndex(instance, startIndex, "startIndex");
		if (instance.Length == 0)
			return -1;

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
		EnsureMatch(match);
		EnsureLastSearchRange(instance, startIndex, count, "startIndex");
		if (count == 0)
			return -1;

		int start = (int)startIndex - (int)count + 1;
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
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.GetRange(int, int)")]
	public static Array<T> _c35c9c99a23ff96a(Array<T> instance, Number index, Number count)
	{
		EnsureRemoveRange(instance, index, count);
		var result = CreateWithCapacity(count);
		for (var offset = 0; offset < count; offset++)
			AddCore(result, instance[index + offset]);
		return result;
	}

	/// <summary>
	/// C#: list.Slice(start, length)
	/// JS: array.slice(start, start + length)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.Slice(int, int)", "slice")]
	public static Array<T> Slice(Array<T> instance, Number start, Number length)
		=> _c35c9c99a23ff96a(instance, start, length);

	/// <summary>
	/// C#: list.IndexOf(item)
	/// JS: array.indexOf(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.IndexOf(T)", "indexOf")]
	public extern static Number _2bb4b70655cede73(Array<T> instance, T item);

	/// <summary>
	/// C#: list.IndexOf(item, index)
	/// JS: 从指定起点向后搜索
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.IndexOf(T, int)")]
	public static Number _71ee35e0e260eb27(Array<T> instance, T item, Number index)
	{
		EnsureForwardSearchStartIndex(instance, index);
		if (index == instance.Length)
			return -1;

		for (uint i = (uint)index; i < instance.Length; i++)
		{
			if (EqualsForListSearch(instance[i], item))
				return (int)i;
		}
		return -1;
	}

	/// <summary>
	/// C#: list.IndexOf(item, index, count)
	/// JS: 在范围内搜索
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.IndexOf(T, int, int)")]
	public static Number _5ee52e4e4fc54e6d(Array<T> instance, T item, Number index, Number count)
	{
		EnsureForwardSearchRange(instance, index, count);
		if (count == 0)
			return -1;

		uint end = (uint)((int)index + (int)count);
		for (uint i = (uint)index; i < end; i++)
		{
			if (EqualsForListSearch(instance[i], item))
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
		EnsureCapacityCore(instance, instance.Length + 1);
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
		if (collection is null)
			throw new Error("ArgumentNullException: collection is null");

		// Materialize first, including self-insert, so enumeration observes the original
		// source and the target can be shifted in one linear pass.
		var values = new Array<T>();
		foreach (var item in collection)
			values.Push(item);

		if (values.Length == 0)
			return;

		var originalLength = instance.Length;
		EnsureCapacityCore(instance, originalLength + values.Length);
		for (var read = originalLength; read > index; read--)
			instance[read + values.Length - 1] = instance[read - 1];

		for (var offset = 0; offset < values.Length; offset++)
			instance[index + offset] = values[offset];
	}

	/// <summary>
	/// C#: list.LastIndexOf(item)
	/// JS: array.lastIndexOf(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.LastIndexOf(T)", "lastIndexOf")]
	public extern static Number _121df07eb2f61749(Array<T> instance, T item);

	/// <summary>
	/// C#: list.LastIndexOf(item, index)
	/// JS: 从指定起点向前搜索
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.LastIndexOf(T, int)")]
	public static Number _279befda6399cda5(Array<T> instance, T item, Number index)
	{
		EnsureLastSearchStartIndex(instance, index, "index");
		if (instance.Length == 0)
			return -1;

		for (int i = (int)index; i >= 0; i--)
		{
			if (EqualsForListSearch(instance[(uint)i], item))
				return i;
		}
		return -1;
	}

	/// <summary>
	/// C#: list.LastIndexOf(item, index, count)
	/// JS: 在范围内反向搜索
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.LastIndexOf(T, int, int)")]
	public static Number _b2f1955b62962812(Array<T> instance, T item, Number index, Number count)
	{
		EnsureLastSearchRange(instance, index, count, "index");
		if (count == 0)
			return -1;

		int start = (int)index - (int)count + 1;
		for (int i = (int)index; i >= start; i--)
		{
			if (EqualsForListSearch(instance[(uint)i], item))
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
		EnsureInstance(instance);

		for (uint i = 0; i < instance.Length; i++)
		{
			if (EqualsForListSearch(instance[i], item))
			{
				instance.Splice(i, 1);
				return true;
			}
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
		EnsureInstance(instance);
		EnsureMatch(match);

		var write = 0;
		var count = 0;
		for (var read = 0; read < instance.Length; read++)
		{
			var item = instance[read];
			if (match(item))
			{
				count++;
				continue;
			}

			instance[write++] = item;
		}

		if (count > 0)
			instance.Splice(write, count);
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
		EnsureRemoveRange(instance, index, count);
		if (count <= 1)
			return;

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

	private static Number CompareDefault(T left, T right)
		=> ComparerT1Module<T>.CompareCore(left, right);

	/// <summary>
	/// C#: list.Sort()
	/// JS: array.sort(defaultComparer)
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.Sort()")]
	public static void _36a478f36b41a6d2(Array<T> instance)
	{
		EnsureInstance(instance);
		instance.Sort((left, right) => CompareDefault(left, right));
	}

	/// <summary>
	/// C#: list.Sort(comparer)
	/// JS: array.sort((a, b) => comparer.Compare(a, b))
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.Sort(System.Collections.Generic.IComparer<T>)")]
	public static void _5fa599e721e252ff(Array<T> instance, IComparer<T>? comparer)
	{
		EnsureInstance(instance);
		if (comparer is null)
			instance.Sort((left, right) => CompareDefault(left, right));
		else
			instance.Sort((left, right) => comparer.Compare(left, right));
	}

	/// <summary>
	/// C#: list.Sort(index, count, comparer)
	/// JS: 子数组排序
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.List<T>.Sort(int, int, System.Collections.Generic.IComparer<T>)")]
	public static void _19207851b52a5287(Array<T> instance, Number index, Number count, IComparer<T>? comparer)
	{
		EnsureInstance(instance);
		EnsureRemoveRange(instance, index, count);
		if (count <= 1)
			return;

		var subArray = instance.Slice((int)index, (int)index + (int)count);
		if (comparer != null)
			subArray.Sort((a, b) => comparer.Compare(a, b));
		else
			subArray.Sort((left, right) => CompareDefault(left, right));
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

	[Jazor(Op.Import, "System.Collections.Generic.List<T>.TrimExcess()")]
	public static void _27c95e83eced65e9(Array<T> instance)
	{
		var capacity = GetCapacity(instance);
		if (instance.Length < Math.FloorFunc(capacity * 0.9))
			Capacities.Set(instance, instance.Length);
	}

	/// <summary>
	/// C#: list.TrueForAll(match)
	/// JS: array.every(match)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.List<T>.TrueForAll(System.Predicate<T>)", "every")]
	public extern static bool _d12a4656f219490c(Array<T> instance, Predicate<T> match);
}
