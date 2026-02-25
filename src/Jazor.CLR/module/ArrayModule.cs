namespace Jazor.CLR;

/// <summary>
/// System.Array 类型模块映射规则
///
/// C# Array 与 JavaScript Array 的对应关系：
/// - C# Array 是静态数组，JavaScript Array 是动态数组
/// - 一维数组可以直接映射，多维数组不支持
///
/// Op 类型选择原则：
/// - Replace: JavaScript 有原生对应方法（如 length, indexOf, reverse, sort 等）
/// - Inline: JavaScript 有对应操作但名称/行为不同（如 Clone 用 slice()）
/// - Import: 需要 C# 实现转换逻辑（如 Empty<T>, ConvertAll<T> 等）
/// - Discard: JavaScript 无对应概念（如多维数组、类型系统相关方法）
///
/// 类型映射：
/// - System.Array → Array<T> 或 Array（泛型方法用 Array<T>，非泛型用 Array）
/// - int → Number
/// - long → BigInt
/// - T[] → Array<T>
/// - out 参数 → 返回数组（第一个元素是返回值，后续是 out 参数值）
/// </summary>
[ECMAScriptModule]
[Jazor(Op.Import, "System.Array", "System/ArrayModule.js")]
public static class ArrayModule
{
	/// <summary>
	/// C#: array.Length
	/// JS: array.length
	/// </summary>
	[Jazor(Op.Replace, "System.Array.Length.get", "length")]
	public extern static Number _fdebc1c5c62f33cc(object instance);

	/// <summary>
	/// JavaScript 数组没有 LongLength 概念，长度最大为 2^32-1
	/// </summary>
	[Jazor(Op.Discard, "System.Array.LongLength.get")]
	public extern static BigInt _82dc944f60373152(object instance);

	/// <summary>
	/// JavaScript 数组始终是一维的，不支持 Rank
	/// </summary>
	[Jazor(Op.Discard, "System.Array.Rank.get")]
	public extern static Number _6ab1259f55d0dd24(object instance);

	/// <summary>
	/// JavaScript 数组元素初始化为 undefined，无需 Initialize
	/// </summary>
	[Jazor(Op.Discard, "System.Array.Initialize()")]
	public extern static void _a93e4c6dc74a4cff(object instance);

	/// <summary>
	/// C#: Array.AsReadOnly<T>(T[])
	/// JS: 使用 Object.freeze() 或返回只读包装
	/// 需要 C# 实现
	/// </summary>
	[Jazor(Op.Discard, "static System.Array.AsReadOnly<T>(T[])")]
	public extern static Array<T> _abd52ebcdb6fefcb<T>(Array<T> array);

	/// <summary>
	/// C#: Array.Resize<T>(ref T[], int)
	/// JS: JavaScript 数组可动态调整大小
	/// out 参数处理：返回 [newArray]
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Resize<T>(ref T[], int)")]
	public static Array<object?> _127013d39cf5bff9<T>(Array<T>? array, Number newSize)
	{
		if (array == null)
			return new object?[newSize];
		var newArray = new object?[newSize];
		for (uint i = 0; i < array.Length && i < newSize; i++)
			newArray[i] = array[i];
		return newArray;
	}

	/// <summary>
	/// JavaScript 没有类型系统，无法动态创建指定类型的数组
	/// </summary>
	[Jazor(Op.Discard, "static System.Array.CreateInstance(System.Type, int)")]
	public extern static object _7cf4f1d72cf2dca7(object elementType, Number length);

	/// <summary>
	/// JavaScript 不支持多维数组
	/// </summary>
	[Jazor(Op.Discard, "static System.Array.CreateInstance(System.Type, int, int)")]
	public extern static object _3800bc5f99a65eb7(object elementType, Number length1, Number length2);

	/// <summary>
	/// JavaScript 不支持三维数组
	/// </summary>
	[Jazor(Op.Discard, "static System.Array.CreateInstance(System.Type, int, int, int)")]
	public extern static object _946705c3abbbb67c(object elementType, Number length1, Number length2, Number length3);

	/// <summary>
	/// JavaScript 不支持多维数组
	/// </summary>
	[Jazor(Op.Discard, "static System.Array.CreateInstance(System.Type, params int[])")]
	public extern static object _55c950cf5ea775e9(object elementType, object lengths);

	/// <summary>
	/// JavaScript 不支持多维数组和下界
	/// </summary>
	[Jazor(Op.Discard, "static System.Array.CreateInstance(System.Type, int[], int[])")]
	public extern static object _81e3451a7be5290d(object elementType, object lengths, object lowerBounds);

	/// <summary>
	/// JavaScript 不支持多维数组
	/// </summary>
	[Jazor(Op.Discard, "static System.Array.CreateInstance(System.Type, params long[])")]
	public extern static object _d1e6f82b64452f99(object elementType, object lengths);

	/// <summary>
	/// JavaScript 没有类型系统，无法动态创建指定类型的数组
	/// </summary>
	[Jazor(Op.Discard, "static System.Array.CreateInstanceFromArrayType(System.Type, int)")]
	public extern static object _8d8c533adf78f2c2(object arrayType, Number length);

	/// <summary>
	/// JavaScript 不支持多维数组
	/// </summary>
	[Jazor(Op.Discard, "static System.Array.CreateInstanceFromArrayType(System.Type, params int[])")]
	public extern static object _11529b7770340ce8(object arrayType, object lengths);

	/// <summary>
	/// JavaScript 不支持多维数组和下界
	/// </summary>
	[Jazor(Op.Discard, "static System.Array.CreateInstanceFromArrayType(System.Type, int[], int[])")]
	public extern static object _c78b33d4f8633a9b(object arrayType, object lengths, object lowerBounds);

	/// <summary>
	/// C#: Array.Copy(sourceArray, destinationArray, length)
	/// JS: 使用 slice + 循环或 Array.prototype.set (TypedArray)
	/// 需要 C# 实现
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Copy(System.Array, System.Array, long)")]
	public static void _7a3d7a78ff429283<T>(Array<T> sourceArray, Array<T> destinationArray, Number length)
	{
		// 实现：将 sourceArray 的前 length 个元素复制到 destinationArray
		for (Number i = 0; i < length; i++)
			destinationArray[i] = sourceArray[i];
	}

	/// <summary>
	/// C#: Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length)
	/// JS: 需要手动实现
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Copy(System.Array, long, System.Array, long, long)")]
	public static void _e2bd26f0b897dcdc<T>(Array<T> sourceArray, Number sourceIndex, Array<T> destinationArray, Number destinationIndex, Number length)
	{
		for (Number i = 0; i < length; i++)
			destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
	}

	/// <summary>
	/// C#: Array.ConstrainedCopy - 原子性复制，失败时回滚
	/// JS: JavaScript 没有原子性复制概念，使用普通 Copy
	/// </summary>
	[Jazor(Op.Import, "static System.Array.ConstrainedCopy(System.Array, int, System.Array, int, int)")]
	public static void _e83857a6975e2bca<T>(Array<T> sourceArray, Number sourceIndex, Array<T> destinationArray, Number destinationIndex, Number length)
	{
		for (Number i = 0; i < length; i++)
			destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
	}

	/// <summary>
	/// C#: Array.Copy(sourceArray, destinationArray, length)
	/// JS: 使用 slice 或手动复制
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Copy(System.Array, System.Array, int)")]
	public static void _236e3a8894f7381f<T>(Array<T> sourceArray, Array<T> destinationArray, Number length)
	{
		for (Number i = 0; i < length; i++)
			destinationArray[i] = sourceArray[i];
	}

	/// <summary>
	/// C#: Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length)
	/// JS: 手动实现
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Copy(System.Array, int, System.Array, int, int)")]
	public static void _5afb5659a201668f<T>(Array<T> sourceArray, Number sourceIndex, Array<T> destinationArray, Number destinationIndex, Number length)
	{
		for (Number i = 0; i < length; i++)
			destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
	}

	/// <summary>
	/// C#: Array.Clear(array)
	/// JS: array.length = 0 或 array.fill(undefined)
	/// </summary>
	[Jazor(Op.Inline, "static System.Array.Clear(System.Array)", "@#{0}.length = 0")]
	public extern static void _96774f9ec153a919<T>(Array<T> array);

	/// <summary>
	/// C#: Array.Clear(array, index, length)
	/// JS: array.fill(undefined, index, index + length)
	/// </summary>
	[Jazor(Op.Inline, "static System.Array.Clear(System.Array, int, int)", "@#{0}.fill(undefined, @#{1}, @#{1} + @#{2})")]
	public extern static void _e6e9140591777519(Array array, Number index, Number length);

	/// <summary>
	/// JavaScript 数组是一维的，GetLength 等同于 length
	/// </summary>
	[Jazor(Op.Inline, "System.Array.GetLength(int)", "(@#{0}).length")]
	public extern static Number _4a62a6d3092e758c(object instance, Number dimension);

	/// <summary>
	/// JavaScript 数组是一维的，GetUpperBound 返回 length - 1
	/// </summary>
	[Jazor(Op.Inline, "System.Array.GetUpperBound(int)", "(@#{0}).length - 1")]
	public extern static Number _240013ed6fb455ce(object instance, Number dimension);

	/// <summary>
	/// JavaScript 数组下界始终为 0
	/// </summary>
	[Jazor(Op.Inline, "System.Array.GetLowerBound(int)", "0")]
	public extern static Number _de93a1deaab12d20(object instance, Number dimension);

	/// <summary>
	/// JavaScript 不支持多维数组的索引数组访问
	/// </summary>
	[Jazor(Op.Discard, "System.Array.GetValue(params int[])")]
	public extern static object? _e938260256ca4a08(object instance, object indices);

	/// <summary>
	/// C#: array.GetValue(index)
	/// JS: array[index]
	/// </summary>
	[Jazor(Op.Inline, "System.Array.GetValue(int)", "(@#{0})[@#{1}]")]
	public extern static object? _eba14f0435c17445(object instance, Number index);

	/// <summary>
	/// JavaScript 不支持二维数组
	/// </summary>
	[Jazor(Op.Discard, "System.Array.GetValue(int, int)")]
	public extern static object? _c479de104d41183c(object instance, Number index1, Number index2);

	/// <summary>
	/// JavaScript 不支持三维数组
	/// </summary>
	[Jazor(Op.Discard, "System.Array.GetValue(int, int, int)")]
	public extern static object? _a9dc664f06ce55a4(object instance, Number index1, Number index2, Number index3);

	/// <summary>
	/// C#: array.SetValue(value, index)
	/// JS: array[index] = value
	/// </summary>
	[Jazor(Op.Inline, "System.Array.SetValue(object, int)", "(@#{0})[@#{2}] = @#{1}")]
	public extern static void _1f2a45eb847a2ec4(object instance, object? value, Number index);

	/// <summary>
	/// JavaScript 不支持二维数组
	/// </summary>
	[Jazor(Op.Discard, "System.Array.SetValue(object, int, int)")]
	public extern static void _7ca03dfc64fd5640(object instance, object? value, Number index1, Number index2);

	/// <summary>
	/// JavaScript 不支持三维数组
	/// </summary>
	[Jazor(Op.Discard, "System.Array.SetValue(object, int, int, int)")]
	public extern static void _a8dff91417f83303(object instance, object? value, Number index1, Number index2, Number index3);

	/// <summary>
	/// JavaScript 不支持多维数组的索引数组访问
	/// </summary>
	[Jazor(Op.Discard, "System.Array.SetValue(object, params int[])")]
	public extern static void _8752076a83fbb3f1(object instance, object? value, object indices);

	/// <summary>
	/// C#: array.GetValue(longIndex)
	/// JS: array[Number(longIndex)]
	/// BigInt 可以作为索引使用
	/// </summary>
	[Jazor(Op.Inline, "System.Array.GetValue(long)", "(@#{0})[@#{1}]")]
	public extern static object? _99c592f7140b4f20(object instance, BigInt index);

	/// <summary>
	/// JavaScript 不支持二维数组
	/// </summary>
	[Jazor(Op.Discard, "System.Array.GetValue(long, long)")]
	public extern static object? _2bad686c503b1e40(object instance, BigInt index1, BigInt index2);

	/// <summary>
	/// JavaScript 不支持三维数组
	/// </summary>
	[Jazor(Op.Discard, "System.Array.GetValue(long, long, long)")]
	public extern static object? _8e8e4b0752cd3155(object instance, BigInt index1, BigInt index2, BigInt index3);

	/// <summary>
	/// JavaScript 不支持多维数组的索引数组访问
	/// </summary>
	[Jazor(Op.Discard, "System.Array.GetValue(params long[])")]
	public extern static object? _6a12948779406121(object instance, object indices);

	/// <summary>
	/// C#: array.SetValue(value, longIndex)
	/// JS: array[Number(longIndex)] = value
	/// </summary>
	[Jazor(Op.Inline, "System.Array.SetValue(object, long)", "(@#{0})[@#{2}] = @#{1}")]
	public extern static void _d845170315112950(object instance, object? value, BigInt index);

	/// <summary>
	/// JavaScript 不支持二维数组
	/// </summary>
	[Jazor(Op.Discard, "System.Array.SetValue(object, long, long)")]
	public extern static void _24864536d32c0b93(object instance, object? value, BigInt index1, BigInt index2);

	/// <summary>
	/// JavaScript 不支持三维数组
	/// </summary>
	[Jazor(Op.Discard, "System.Array.SetValue(object, long, long, long)")]
	public extern static void _314db333058e554d(object instance, object? value, BigInt index1, BigInt index2, BigInt index3);

	/// <summary>
	/// JavaScript 不支持多维数组的索引数组访问
	/// </summary>
	[Jazor(Op.Discard, "System.Array.SetValue(object, params long[])")]
	public extern static void _e3923681669a96b5(object instance, object? value, object indices);

	/// <summary>
	/// JavaScript 数组是一维的，GetLongLength 等同于 BigInt(length)
	/// </summary>
	[Jazor(Op.Inline, "System.Array.GetLongLength(int)", "BigInt((@#{0}).length)")]
	public extern static BigInt _b529d6e54112cf3e(object instance, Number dimension);

	/// <summary>
	/// JavaScript 没有同步根概念
	/// </summary>
	[Jazor(Op.Discard, "System.Array.SyncRoot.get")]
	public extern static object _5df324fc2064bf14(object instance);

	/// <summary>
	/// JavaScript 数组不是只读的
	/// </summary>
	[Jazor(Op.Inline, "System.Array.IsReadOnly.get", "false")]
	public extern static bool _957efa892fba2b42(object instance);

	/// <summary>
	/// JavaScript 数组大小可变
	/// </summary>
	[Jazor(Op.Inline, "System.Array.IsFixedSize.get", "false")]
	public extern static bool _af3654cc2dd2fa42(object instance);

	/// <summary>
	/// JavaScript 数组不是线程同步的
	/// </summary>
	[Jazor(Op.Inline, "System.Array.IsSynchronized.get", "false")]
	public extern static bool _818cd5ec440253da(object instance);

	/// <summary>
	/// C#: array.Clone()
	/// JS: array.slice() 或 [...array]
	/// </summary>
	[Jazor(Op.Inline, "System.Array.Clone()", "(@#{0}).slice()")]
	public extern static object _7b75e1326e081bb2(object instance);

	/// <summary>
	/// C#: Array.BinarySearch(array, value)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Discard, "static System.Array.BinarySearch(System.Array, object)")]
	public extern static Number _0c9e99640a975a5b(Array array, object? value);

	/// <summary>
	/// C#: Array.BinarySearch(array, index, length, value)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch(System.Array, int, int, object)")]
	public static Number _fa538add1f784012<T>(Array<T> array, Number index, Number length, object? value)
	{
		// 实现二分查找
		var left = index;
		var right = index + length - 1;
		while (left <= right)
		{
			var mid = left + (right - left) / 2;
			var cmp = string.Compare(array[mid]?.ToString(), value?.ToString());
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	/// <summary>
	/// C#: Array.BinarySearch(array, value, comparer)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch(System.Array, object, System.Collections.IComparer)")]
	public static Number _c453dd981ecbb5c5<T>(Array<T> array, object? value, System.Collections.IComparer comparer)
	{
		// 使用自定义比较器实现二分查找
		Number left = 0;
		var right = array.Length - 1;
		while (left <= right)
		{
			var mid = left + (right - left) / 2;
			var cmp = comparer.Compare(array[mid], value);
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	/// <summary>
	/// C#: Array.BinarySearch(array, index, length, value, comparer)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch(System.Array, int, int, object, System.Collections.IComparer)")]
	public static Number _f1fb5c20cf9ffd4d<T>(Array<T> array, Number index, Number length, object? value, System.Collections.IComparer comparer)
	{
		Number left = index;
		var right = index + length - 1;
		while (left <= right)
		{
			var mid = left + (right - left) / 2;
			var cmp = comparer.Compare(array[mid], value);
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	/// <summary>
	/// C#: Array.BinarySearch<T>(array, value)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch<T>(T[], T)")]
	public static Number _75258b66e0bba01a(Array array, object? value)
	{
		var index = Array.IndexOf(array, value);
		return index >= 0 ? index : ~index;
	}

	/// <summary>
	/// C#: Array.BinarySearch<T>(array, value, comparer)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch<T>(T[], T, System.Collections.Generic.IComparer<T>)")]
	public static Number _87f2af26c36fed01<T>(Array<T> array, T value, IComparer<T>? comparer)
	{
		Number left = 0;
		var right = array.Length - 1;
		while (left <= right)
		{
			var mid = left + (right - left) / 2;
			var cmp = comparer != null 
				? comparer.Compare(array[mid], value) 
				: string.Compare(array[mid]?.ToString(), value?.ToString());
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	/// <summary>
	/// C#: Array.BinarySearch<T>(array, index, length, value)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch<T>(T[], int, int, T)")]
	public static Number _60003ac825620c60<T>(Array<T> array, Number index, Number length, T value)
	{
		var left = index;
		var right = index + length - 1;
		while (left <= right)
		{
			var mid = left + (right - left) / 2;
			var cmp = string.Compare(array[mid]?.ToString(), value?.ToString());
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	/// <summary>
	/// C#: Array.BinarySearch<T>(array, index, length, value, comparer)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch<T>(T[], int, int, T, System.Collections.Generic.IComparer<T>)")]
	public static Number _42b1da24db771714<T>(Array<T> array, Number index, Number length, T value, IComparer<T>? comparer)
	{
		var left = index;
		var right = index + length - 1;
		while (left <= right)
		{
			var mid = left + (right - left) / 2;
			var cmp = comparer != null
				? comparer.Compare(array[mid], value)
				: string.Compare(array[mid]?.ToString(), value?.ToString());
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	/// <summary>
	/// C#: Array.ConvertAll<TInput, TOutput>(array, converter)
	/// JS: array.map(converter)
	/// </summary>
	[Jazor(Op.Import, "static System.Array.ConvertAll<TInput, TOutput>(TInput[], System.Converter<TInput, TOutput>)")]
	public static TOutput[] _a73f4ff0bddcc6f6<TInput, TOutput>(Array<TInput> array, CallbackFunc<TInput, TOutput> converter)
	{
		return array.Map(converter);
	}

	/// <summary>
	/// C#: array.CopyTo(destArray, index)
	/// JS: 使用 slice + 循环
	/// </summary>
	[Jazor(Op.Import, "System.Array.CopyTo(System.Array, int)")]
	public static void _559d75b1e44b3eb0<T>(Array<T> instance, Array<T> array, Number index)
	{
		for (Number i = 0; i < instance.Length; i++)
			array[index + i] = instance[i];
	}

	/// <summary>
	/// C#: array.CopyTo(destArray, longIndex)
	/// JS: 使用 BigInt 作为索引
	/// </summary>
	[Jazor(Op.Import, "System.Array.CopyTo(System.Array, long)")]
	public static void _02714528e8c676b0<T>(Array<T> instance, Array<T> array, Number index)
	{
		for (Number i = 0; i < instance.Length; i++)
			array[index + i] = instance[i];
	}

	/// <summary>
	/// C#: Array.Empty<T>()
	/// JS: 返回空数组 []
	/// </summary>
	[Jazor(Op.Inline, "static System.Array.Empty<T>()", "[]")]
	public extern static Array<T> _b36a1b49fd533b3e<T>();

	/// <summary>
	/// C#: Array.Exists<T>(array, match)
	/// JS: array.some(match)
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Exists<T>(T[], System.Predicate<T>)")]
	public static bool _3795c9344e3fe39f<T>(Array<T> array, Predicate<T> match)
	{
		return array.Some(match);
	}

	/// <summary>
	/// C#: Array.Fill<T>(array, value)
	/// JS: array.fill(value)
	/// </summary>
	[Jazor(Op.Replace, "static System.Array.Fill<T>(T[], T)", "fill")]
	public extern static void _65ab99eba8176bda<T>(Array<T> array, T value);

	/// <summary>
	/// C#: Array.Fill<T>(array, value, startIndex, count)
	/// JS: array.fill(value, startIndex, startIndex + count)
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Fill<T>(T[], T, int, int)")]
	public static void _8edf171ab37f3a05<T>(T[] array, T value, Number startIndex, Number count)
	{
		Array.Fill(array, value, startIndex, startIndex + count);
	}

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Find<T>(T[], System.Predicate<T>)")]
	public extern static T? _1dfc77048ccf0234<T>(Array<T> array, Predicate<T> match);

	///<summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
	[Jazor(Op.Discard, "static System.Array.FindAll<T>(T[], System.Predicate<T>)")]
	public extern static Array<T> _b373eb093e6c7b63<T>(Array<T> array, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.FindIndex<T>(T[], System.Predicate<T>)")]
	public extern static Number _64f5a7fd5c436edb<T>(Array<T> array, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the specified index to the last element.</summary>
	[Jazor(Op.Discard, "static System.Array.FindIndex<T>(T[], int, System.Predicate<T>)")]
	public extern static Number _42e008ba24b77e94<T>(Array<T> array, Number startIndex, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that starts at the specified index and contains the specified number of elements.</summary>
	[Jazor(Op.Discard, "static System.Array.FindIndex<T>(T[], int, int, System.Predicate<T>)")]
	public extern static Number _fdfc005bdc859fff<T>(Array<T> array, Number startIndex, Number count, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.FindLast<T>(T[], System.Predicate<T>)")]
	public extern static T? _2786abe2cff245fa<T>(Array<T> array, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.FindLastIndex<T>(T[], System.Predicate<T>)")]
	public extern static Number _ea3118f38aa5f363<T>(Array<T> array, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	[Jazor(Op.Discard, "static System.Array.FindLastIndex<T>(T[], int, System.Predicate<T>)")]
	public extern static Number _56359f972a00ab73<T>(Array<T> array, Number startIndex, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	[Jazor(Op.Discard, "static System.Array.FindLastIndex<T>(T[], int, int, System.Predicate<T>)")]
	public extern static Number _6b63489e941ef0f0<T>(Array<T> array, Number startIndex, Number count, Predicate<T> match);

	///<summary>Performs the specified action on each element of the specified array.</summary>
	[Jazor(Op.Discard, "static System.Array.ForEach<T>(T[], System.Action<T>)")]
	public extern static void _ad1c39ab55fe27b9<T>(Array<T> array, object action);

	///<summary>Searches for the specified object and returns the index of its first occurrence in a one-dimensional array.</summary>
	[Jazor(Op.Discard, "static System.Array.IndexOf(System.Array, object)")]
	public extern static Number _cde8d7a78af8dc9a(object array, object? value);

	///<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of its first occurrence. The range extends from a specified index to the end of the array.</summary>
	[Jazor(Op.Discard, "static System.Array.IndexOf(System.Array, object, int)")]
	public extern static Number _2151f4cd0a63b0a2(object array, object? value, Number startIndex);

	///<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of ifs first occurrence. The range extends from a specified index for a specified number of elements.</summary>
	[Jazor(Op.Discard, "static System.Array.IndexOf(System.Array, object, int, int)")]
	public extern static Number _c419efc216312a6a(object array, object? value, Number startIndex, Number count);

	///<summary>Searches for the specified object and returns the index of its first occurrence in a one-dimensional array.</summary>
	[Jazor(Op.Discard, "static System.Array.IndexOf<T>(T[], T)")]
	public extern static Number _34e8668cac3c06fa<T>(Array<T> array, object value);

	///<summary>Searches for the specified object in a range of elements of a one dimensional array, and returns the index of its first occurrence. The range extends from a specified index to the end of the array.</summary>
	[Jazor(Op.Discard, "static System.Array.IndexOf<T>(T[], T, int)")]
	public extern static Number _d7a4d17a98a17e7e<T>(Array<T> array, object value, Number startIndex);

	///<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of its first occurrence. The range extends from a specified index for a specified number of elements.</summary>
	[Jazor(Op.Discard, "static System.Array.IndexOf<T>(T[], T, int, int)")]
	public extern static Number _e3d80b27a67e8a0d<T>(Array<T> array, object value, Number startIndex, Number count);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the entire one-dimensional <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.LastIndexOf(System.Array, object)")]
	public extern static Number _85801a2dbc247f17(object array, object? value);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	[Jazor(Op.Discard, "static System.Array.LastIndexOf(System.Array, object, int)")]
	public extern static Number _6b23455f7b2f95ff(object array, object? value, Number startIndex);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	[Jazor(Op.Discard, "static System.Array.LastIndexOf(System.Array, object, int, int)")]
	public extern static Number _7f5af90fd2a084fe(object array, object? value, Number startIndex, Number count);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.LastIndexOf<T>(T[], T)")]
	public extern static Number _198d0f4fcb1c0679<T>(Array<T> array, object value);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	[Jazor(Op.Discard, "static System.Array.LastIndexOf<T>(T[], T, int)")]
	public extern static Number _5c2c6aa99d0e0549<T>(Array<T> array, object value, Number startIndex);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	[Jazor(Op.Discard, "static System.Array.LastIndexOf<T>(T[], T, int, int)")]
	public extern static Number _b5bf131d8947c855<T>(Array<T> array, object value, Number startIndex, Number count);

	///<summary>Reverses the sequence of the elements in the entire one-dimensional <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Reverse(System.Array)")]
	public extern static void _c02ce18f02385f3d(object array);

	///<summary>Reverses the sequence of a subset of the elements in the one-dimensional <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Reverse(System.Array, int, int)")]
	public extern static void _36c04f95b4ffdfd5(object array, Number index, Number length);

	///<summary>Reverses the sequence of the elements in the one-dimensional generic array.</summary>
	[Jazor(Op.Discard, "static System.Array.Reverse<T>(T[])")]
	public extern static void _e2b02681782c394b<T>(Array<T> array);

	///<summary>Reverses the sequence of a subset of the elements in the one-dimensional generic array.</summary>
	[Jazor(Op.Discard, "static System.Array.Reverse<T>(T[], int, int)")]
	public extern static void _5b0cbdf276c63339<T>(Array<T> array, Number index, Number length);

	///<summary>Sorts the elements in an entire one-dimensional <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort(System.Array)")]
	public extern static void _07ee8311aaf13b6b(object array);

	///<summary>Sorts a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each key.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort(System.Array, System.Array)")]
	public extern static void _4df21ca760120c59(object keys, object items);

	///<summary>Sorts the elements in a range of elements in a one-dimensional <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort(System.Array, int, int)")]
	public extern static void _4e10132b81a43421(object array, Number index, Number length);

	///<summary>Sorts a range of elements in a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each key.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort(System.Array, System.Array, int, int)")]
	public extern static void _12789d2affa27035(object keys, object items, Number index, Number length);

	///<summary>Sorts the elements in a one-dimensional <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort(System.Array, System.Collections.IComparer)")]
	public extern static void _093c373956602c04(object array, object comparer);

	///<summary>Sorts a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort(System.Array, System.Array, System.Collections.IComparer)")]
	public extern static void _122404a1fc2867ba(object keys, object items, object comparer);

	///<summary>Sorts the elements in a range of elements in a one-dimensional <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort(System.Array, int, int, System.Collections.IComparer)")]
	public extern static void _b2141b8c013bc1b0(object array, Number index, Number length, object comparer);

	///<summary>Sorts a range of elements in a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort(System.Array, System.Array, int, int, System.Collections.IComparer)")]
	public extern static void _a95c3f83e8cd4623(object keys, object items, Number index, Number length, object comparer);

	///<summary>Sorts the elements in an entire <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort<T>(T[])")]
	public extern static void _382add2bad872f67<T>(Array<T> array);

	///<summary>Sorts a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each key.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort<TKey, TValue>(TKey[], TValue[])")]
	public extern static void _1a3ebd994898c67c<TKey, TValue>(object keys, object items);

	///<summary>Sorts the elements in a range of elements in an <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort<T>(T[], int, int)")]
	public extern static void _80e6f8922ae8703c<T>(Array<T> array, Number index, Number length);

	///<summary>Sorts a range of elements in a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each key.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int)")]
	public extern static void _9b803c8e781cf3c0<TKey, TValue>(object keys, object items, Number index, Number length);

	///<summary>Sorts the elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort<T>(T[], System.Collections.Generic.IComparer<T>)")]
	public extern static void _92474aed4e4823f3<T>(Array<T> array, IComparer<T>? comparer);

	///<summary>Sorts a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort<TKey, TValue>(TKey[], TValue[], System.Collections.Generic.IComparer<TKey>)")]
	public extern static void _dfd5fefaaa03a228<TKey, TValue>(object keys, object items, object comparer);

	///<summary>Sorts the elements in a range of elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort<T>(T[], int, int, System.Collections.Generic.IComparer<T>)")]
	public extern static void _55dbc52295bd7984<T>(Array<T> array, Number index, Number length, IComparer<T>? comparer);

	///<summary>Sorts a range of elements in a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int, System.Collections.Generic.IComparer<TKey>)")]
	public extern static void _f3e7263659ac2e30<TKey, TValue>(object keys, object items, Number index, Number length, object comparer);

	///<summary>Sorts the elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Comparison`1" />.</summary>
	[Jazor(Op.Discard, "static System.Array.Sort<T>(T[], System.Comparison<T>)")]
	public extern static void _c8fcae59a3aca6f6<T>(Array<T> array, Comparison<T> comparison);

	///<summary>Determines whether every element in the array matches the conditions defined by the specified predicate.</summary>
	[Jazor(Op.Discard, "static System.Array.TrueForAll<T>(T[], System.Predicate<T>)")]
	public extern static bool _7deb21b3fbe579c9<T>(Array<T> array, Predicate<T> match);

	[Jazor(Op.Discard, "static System.Array.MaxLength.get")]
	public extern static Number _a7a42b1fbdbc7628(System.Array instance);

	///<summary>Returns an <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "System.Array.GetEnumerator()")]
	public extern static System.Collections.IEnumerator _1e9012cd200b3827(System.Array instance);
}
