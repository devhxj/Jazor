namespace Jazor.CLR;

/// <summary>
/// System.Array 类型模块映射规则
///
/// C# Array 与 JavaScript Array 的对应关系：
/// - C# Array 是静态数组，JavaScript Array 是动态数组
/// - 一维数组可以直接映射，多维数组不支持
///
/// Op 类型选择原则：
/// - Alias: JavaScript 有原生对应方法（如 length, indexOf, reverse, sort 等）
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
[ECMAScriptModule("System/ArrayModule.js")]
[Jazor(Op.Alias, "System.Array", "Array")]
public static class ArrayModule<T>
{
	[ECMAScriptInline("null")]
	private extern static T? MissingValue();

	private static Number CompareDefault(T left, T right)
		=> ComparerT1Module<T>.CompareCore(left, right);

	private static Number CompareDefaultObject(object? left, object? right)
		=> ComparerT1Module<object?>.CompareObjectsCore(left, right);

	private static Number CompareDefaultKey<TKey>(TKey left, TKey right)
		=> ComparerT1Module<TKey>.CompareCore(left, right);

	#region 属性

	/// <summary>
	/// C#: array.Length
	/// JS: array.length
	/// </summary>
	[Jazor(Op.Alias, "System.Array.Length.get", "length")]
	public extern static Number _fdebc1c5c62f33cc(Array instance);

	/// <summary>
	/// JavaScript 数组没有 LongLength 概念，长度最大为 2^32-1
	/// 但可以转换为 BigInt 返回
	/// </summary>
	[Jazor(Op.Inline, "System.Array.LongLength.get", "BigInt((__arg1).length)")]
	public extern static BigInt _82dc944f60373152(Array instance);

	/// <summary>
	/// JavaScript 数组始终是一维的，不支持 Rank
	/// </summary>
	[Jazor(Op.Discard, "System.Array.Rank.get")]
	public extern static Number _6ab1259f55d0dd24(Array instance);

	/// <summary>
	/// JavaScript 数组元素初始化为 undefined，无需 Initialize
	/// </summary>
	[Jazor(Op.Discard, "System.Array.Initialize()")]
	public extern static void _a93e4c6dc74a4cff(Array instance);

	#endregion

	#region 静态工厂方法

	/// <summary>
	/// C#: Array.AsReadOnly&lt;T&gt;(T[])
	/// JS: 使用 Object.freeze() 创建只读数组
	/// </summary>
	[Jazor(Op.Import, "static System.Array.AsReadOnly<T>(T[])")]
	public static Array<T> _abd52ebcdb6fefcb(Array<T> array)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		return Object.Freeze(array.Slice());
	}

	/// <summary>
	/// C#: Array.Resize&lt;T&gt;(ref T[], int)
	/// JS: JavaScript 数组可动态调整大小
	/// ref 参数处理：返回 [newArray]
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Resize<T>(ref T[], int)")]
	public static Array<object?> _127013d39cf5bff9(Array<T>? array, Number newSize)
	{
		if (newSize < 0)
			throw new Error("ArgumentOutOfRangeException: newSize is less than zero");

		var newArray = new Array<T>(newSize);
		if (array == null)
			return [newArray];

		Number copyLength = Math.Min(array.Length, newSize);
		for (Number i = 0; i < copyLength; i++)
			newArray[i] = array[i];

		return [newArray];
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

	#endregion

	#region Copy 方法

	/// <summary>
	/// C#: Array.Copy(sourceArray, destinationArray, length)
	/// JS: 使用 slice + 循环或 Array.prototype.set (TypedArray)
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Copy(System.Array, System.Array, long)")]
	public static void _7a3d7a78ff429283(Array<T> sourceArray, Array<T> destinationArray, Number length)
	{
		if (sourceArray == null)
			throw new Error("ArgumentNullException: sourceArray is null");
		if (destinationArray == null)
			throw new Error("ArgumentNullException: destinationArray is null");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (length > sourceArray.Length)
			throw new Error("ArgumentException: length is greater than sourceArray length");
		if (length > destinationArray.Length)
			throw new Error("ArgumentException: length is greater than destinationArray length");

		for (Number i = 0; i < length; i++)
			destinationArray[i] = sourceArray[i];
	}

	/// <summary>
	/// C#: Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length)
	/// JS: 需要手动实现
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Copy(System.Array, long, System.Array, long, long)")]
	public static void _e2bd26f0b897dcdc(Array<T> sourceArray, Number sourceIndex, Array<T> destinationArray, Number destinationIndex, Number length)
	{
		if (sourceArray == null)
			throw new Error("ArgumentNullException: sourceArray is null");
		if (destinationArray == null)
			throw new Error("ArgumentNullException: destinationArray is null");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (sourceIndex < 0 || sourceIndex + length > sourceArray.Length)
			throw new Error("ArgumentOutOfRangeException: sourceIndex is out of range");
		if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
			throw new Error("ArgumentOutOfRangeException: destinationIndex is out of range");

		for (var i = 0; i < length; i++)
			destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
	}

	/// <summary>
	/// C#: Array.ConstrainedCopy - 原子性复制，失败时回滚
	/// JS: JavaScript 没有原子性复制概念，使用普通 Copy
	/// </summary>
	[Jazor(Op.Import, "static System.Array.ConstrainedCopy(System.Array, int, System.Array, int, int)")]
	public static void _e83857a6975e2bca(Array<T> sourceArray, Number sourceIndex, Array<T> destinationArray, Number destinationIndex, Number length)
	{
		if (sourceArray == null || destinationArray == null)
			throw new Error("ArgumentNullException: array is null");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (sourceIndex < 0 || sourceIndex + length > sourceArray.Length)
			throw new Error("ArgumentOutOfRangeException: sourceIndex is out of range");
		if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
			throw new Error("ArgumentOutOfRangeException: destinationIndex is out of range");

		for (var i = 0; i < length; i++)
			destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
	}

	/// <summary>
	/// C#: Array.Copy(sourceArray, destinationArray, length)
	/// JS: 使用 slice 或手动复制
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Copy(System.Array, System.Array, int)")]
	public static void _236e3a8894f7381f(Array<T> sourceArray, Array<T> destinationArray, Number length)
	{
		if (sourceArray == null)
			throw new Error("ArgumentNullException: sourceArray is null");
		if (destinationArray == null)
			throw new Error("ArgumentNullException: destinationArray is null");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (length > sourceArray.Length)
			throw new Error("ArgumentException: length is greater than sourceArray length");
		if (length > destinationArray.Length)
			throw new Error("ArgumentException: length is greater than destinationArray length");

		for (Number i = 0; i < length; i++)
			destinationArray[i] = sourceArray[i];
	}

	/// <summary>
	/// C#: Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length)
	/// JS: 手动实现
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Copy(System.Array, int, System.Array, int, int)")]
	public static void _5afb5659a201668f(Array<T> sourceArray, Number sourceIndex, Array<T> destinationArray, Number destinationIndex, Number length)
	{
		if (sourceArray == null)
			throw new Error("ArgumentNullException: sourceArray is null");
		if (destinationArray == null)
			throw new Error("ArgumentNullException: destinationArray is null");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (sourceIndex < 0 || sourceIndex + length > sourceArray.Length)
			throw new Error("ArgumentOutOfRangeException: sourceIndex is out of range");
		if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
			throw new Error("ArgumentOutOfRangeException: destinationIndex is out of range");

		for (var i = 0; i < length; i++)
			destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
	}

	#endregion

	#region Clear 方法

	/// <summary>
	/// C#: Array.Clear(array)
	/// JS: array.length = 0 或 array.fill(undefined)
	/// </summary>
	[Jazor(Op.Inline, "static System.Array.Clear(System.Array)", "__arg1.length = 0")]
	public extern static void _96774f9ec153a919(Array<T> array);

	/// <summary>
	/// C#: Array.Clear(array, index, length)
	/// JS: array.fill(undefined, index, index + length)
	/// </summary>
	[Jazor(Op.Inline, "static System.Array.Clear(System.Array, int, int)", "__arg1.fill(undefined, __arg2, __arg2 + __arg3)")]
	public extern static void _e6e9140591777519(Array array, Number index, Number length);

	#endregion

	#region GetLength/GetBounds 方法

	/// <summary>
	/// JavaScript 数组是一维的，GetLength 等同于 length
	/// </summary>
	[Jazor(Op.Inline, "System.Array.GetLength(int)", "(__arg1).length")]
	public extern static Number _4a62a6d3092e758c(object instance, Number dimension);

	/// <summary>
	/// JavaScript 数组是一维的，GetUpperBound 返回 length - 1
	/// </summary>
	[Jazor(Op.Inline, "System.Array.GetUpperBound(int)", "(__arg1).length - 1")]
	public extern static Number _240013ed6fb455ce(object instance, Number dimension);

	/// <summary>
	/// JavaScript 数组下界始终为 0
	/// </summary>
	[Jazor(Op.Inline, "System.Array.GetLowerBound(int)", "0")]
	public extern static Number _de93a1deaab12d20(object instance, Number dimension);

	/// <summary>
	/// JavaScript 数组是一维的，GetLongLength 等同于 BigInt(length)
	/// </summary>
	[Jazor(Op.Inline, "System.Array.GetLongLength(int)", "BigInt((__arg1).length)")]
	public extern static BigInt _b529d6e54112cf3e(object instance, Number dimension);

	#endregion

	#region GetValue/SetValue 方法

	/// <summary>
	/// JavaScript 不支持多维数组的索引数组访问
	/// </summary>
	[Jazor(Op.Discard, "System.Array.GetValue(params int[])")]
	public extern static object? _e938260256ca4a08(object instance, object indices);

	/// <summary>
	/// C#: array.GetValue(index)
	/// JS: array[index]
	/// </summary>
	[Jazor(Op.Inline, "System.Array.GetValue(int)", "(__arg1)[__arg2]")]
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
	[Jazor(Op.Inline, "System.Array.SetValue(object, int)", "(__arg1)[__arg3] = __arg2")]
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
	[Jazor(Op.Inline, "System.Array.GetValue(long)", "(__arg1)[__arg2]")]
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
	[Jazor(Op.Inline, "System.Array.SetValue(object, long)", "(__arg1)[__arg3] = __arg2")]
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

	#endregion

	#region 接口实现属性

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

	#endregion

	#region Clone

	/// <summary>
	/// C#: array.Clone()
	/// JS: array.slice() 或 [...array]
	/// </summary>
	[Jazor(Op.Inline, "System.Array.Clone()", "(__arg1).slice()")]
	public extern static object _7b75e1326e081bb2(object instance);

	#endregion

	#region BinarySearch 方法

	/// <summary>
	/// C#: Array.BinarySearch(array, value)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch(System.Array, object)")]
	public static Number _0c9e99640a975a5b(Array<T> array, object? value)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (array.Length == 0)
			return -1;

		Number left = 0;
		Number right = array.Length - 1;
		while (left <= right)
		{
			var mid = left + Math.FloorFn((right - left) / 2);
			var cmp = CompareDefaultObject(array[mid], value);
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	/// <summary>
	/// C#: Array.BinarySearch(array, index, length, value)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch(System.Array, int, int, object)")]
	public static Number _fa538add1f784012(Array<T> array, Number index, Number length, object? value)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0)
			throw new Error("ArgumentOutOfRangeException: index is less than zero");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (index + length > array.Length)
			throw new Error("ArgumentException: index + length is greater than array length");

		if (length == 0)
			return ~index;

		Number left = index;
		Number right = index + length - 1;
		while (left <= right)
		{
			var mid = left + Math.FloorFn((right - left) / 2);
			var cmp = CompareDefaultObject(array[mid], value);
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
	public static Number _c453dd981ecbb5c5(Array<T> array, object? value, System.Collections.IComparer comparer)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");

		if (array.Length == 0)
			return -1;

		Number left = 0;
		Number right = array.Length - 1;
		while (left <= right)
		{
			var mid = left + Math.FloorFn((right - left) / 2);
			Number cmp = comparer != null
				? comparer.Compare(array[mid], value)
				: CompareDefaultObject(array[mid], value);
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
	public static Number _f1fb5c20cf9ffd4d(Array<T> array, Number index, Number length, object? value, System.Collections.IComparer comparer)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0)
			throw new Error("ArgumentOutOfRangeException: index is less than zero");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (index + length > array.Length)
			throw new Error("ArgumentException: index + length is greater than array length");

		if (length == 0)
			return ~index;

		Number left = index;
		Number right = index + length - 1;
		while (left <= right)
		{
			var mid = left + Math.FloorFn((right - left) / 2);
			Number cmp = comparer != null
				? comparer.Compare(array[mid], value)
				: CompareDefaultObject(array[mid], value);
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	/// <summary>
	/// C#: Array.BinarySearch&lt;T&gt;(array, value)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch<T>(T[], T)")]
	public static Number _75258b66e0bba01a(Array<T> array, T value)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");

		if (array.Length == 0)
			return -1;

		Number left = 0;
		Number right = array.Length - 1;
		while (left <= right)
		{
			var mid = left + Math.FloorFn((right - left) / 2);
			var cmp = CompareDefault(array[mid], value);
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	/// <summary>
	/// C#: Array.BinarySearch&lt;T&gt;(array, value, comparer)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch<T>(T[], T, System.Collections.Generic.IComparer<T>)")]
	public static Number _87f2af26c36fed01(Array<T> array, T value, IComparer<T>? comparer)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");

		if (array.Length == 0)
			return -1;

		Number left = 0;
		Number right = array.Length - 1;
		while (left <= right)
		{
			var mid = left + Math.FloorFn((right - left) / 2);
			Number cmp = comparer != null
				? comparer.Compare(array[mid], value)
				: CompareDefault(array[mid], value);
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	/// <summary>
	/// C#: Array.BinarySearch&lt;T&gt;(array, index, length, value)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch<T>(T[], int, int, T)")]
	public static Number _60003ac825620c60(Array<T> array, Number index, Number length, T value)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0)
			throw new Error("ArgumentOutOfRangeException: index is less than zero");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (index + length > array.Length)
			throw new Error("ArgumentException: index + length is greater than array length");

		if (length == 0)
			return ~index;

		Number left = index;
		Number right = index + length - 1;
		while (left <= right)
		{
			var mid = left + Math.FloorFn((right - left) / 2);
			var cmp = CompareDefault(array[mid], value);
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	/// <summary>
	/// C#: Array.BinarySearch&lt;T&gt;(array, index, length, value, comparer)
	/// JS: 需要实现二分查找
	/// </summary>
	[Jazor(Op.Import, "static System.Array.BinarySearch<T>(T[], int, int, T, System.Collections.Generic.IComparer<T>)")]
	public static Number _42b1da24db771714(Array<T> array, Number index, Number length, T value, IComparer<T>? comparer)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0)
			throw new Error("ArgumentOutOfRangeException: index is less than zero");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (index + length > array.Length)
			throw new Error("ArgumentException: index + length is greater than array length");

		if (length == 0)
			return ~index;

		Number left = index;
		Number right = index + length - 1;
		while (left <= right)
		{
			var mid = left + Math.FloorFn((right - left) / 2);
			Number cmp = comparer != null
				? comparer.Compare(array[mid], value)
				: CompareDefault(array[mid], value);
			if (cmp == 0) return mid;
			if (cmp < 0) left = mid + 1;
			else right = mid - 1;
		}
		return ~left;
	}

	#endregion

	#region ConvertAll

	/// <summary>
	/// C#: Array.ConvertAll&lt;TInput, TOutput&gt;(array, converter)
	/// JS: array.map(converter)
	/// </summary>
	[Jazor(Op.Import, "static System.Array.ConvertAll<TInput, TOutput>(TInput[], System.Converter<TInput, TOutput>)")]
	public static TOutput[] _a73f4ff0bddcc6f6<TInput, TOutput>(Array<TInput> array, Func<TInput, TOutput> converter)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (converter == null)
			throw new Error("ArgumentNullException: converter is null");
		return array.Map(converter);
	}

	#endregion

	#region CopyTo 方法

	/// <summary>
	/// C#: array.CopyTo(destArray, index)
	/// JS: 使用 slice + 循环
	/// </summary>
	[Jazor(Op.Import, "System.Array.CopyTo(System.Array, int)")]
	public static void _559d75b1e44b3eb0(Array<T> instance, Array<T> array, Number index)
	{
		if (instance == null)
			throw new Error("ArgumentNullException: instance is null");
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0)
			throw new Error("ArgumentOutOfRangeException: index is less than zero");
		if (index + instance.Length > array.Length)
			throw new Error("ArgumentException: not enough space in destination array");

		for (Number i = 0; i < instance.Length; i++)
			array[index + i] = instance[i];
	}

	/// <summary>
	/// C#: array.CopyTo(destArray, longIndex)
	/// JS: 使用 BigInt 作为索引
	/// </summary>
	[Jazor(Op.Import, "System.Array.CopyTo(System.Array, long)")]
	public static void _02714528e8c676b0(Array<T> instance, Array<T> array, BigInt index)
	{
		if (instance == null)
			throw new Error("ArgumentNullException: instance is null");
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < BigInt.Zero)
			throw new Error("ArgumentOutOfRangeException: index is less than zero");
		if (NumberFn(index) + instance.Length > array.Length)
			throw new Error("ArgumentException: not enough space in destination array");

		for (Number i = 0; i < instance.Length; i++)
			array[NumberFn(index) + i] = instance[i];
	}

	#endregion

	#region Empty

	/// <summary>
	/// C#: Array.Empty&lt;T&gt;()
	/// JS: 返回空数组 []
	/// </summary>
	[Jazor(Op.Inline, "static System.Array.Empty<T>()", "[]")]
	public extern static Array<T> _b36a1b49fd533b3e();

	#endregion

	#region Exists

	/// <summary>
	/// C#: Array.Exists&lt;T&gt;(array, match)
	/// JS: array.some(match)
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Exists<T>(T[], System.Predicate<T>)")]
	public static bool _3795c9344e3fe39f(Array<T> array, Predicate<T> match)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (match == null)
			throw new Error("ArgumentNullException: match is null");
		return array.Some(match);
	}

	#endregion

	#region Fill 方法

	/// <summary>
	/// C#: Array.Fill&lt;T&gt;(array, value)
	/// JS: array.fill(value)
	/// </summary>
	[Jazor(Op.Alias, "static System.Array.Fill<T>(T[], T)", "fill")]
	public extern static void _65ab99eba8176bda(Array<T> array, T value);

	/// <summary>
	/// C#: Array.Fill&lt;T&gt;(array, value, startIndex, count)
	/// JS: array.fill(value, startIndex, startIndex + count)
	/// </summary>
	[Jazor(Op.Import, "static System.Array.Fill<T>(T[], T, int, int)")]
	public static void _8edf171ab37f3a05(Array<T> array, T value, Number startIndex, Number count)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (startIndex < 0)
			throw new Error("ArgumentOutOfRangeException: startIndex is less than zero");
		if (count < 0)
			throw new Error("ArgumentOutOfRangeException: count is less than zero");
		if (startIndex + count > array.Length)
			throw new Error("ArgumentException: startIndex + count exceeds array length");

		array.Fill(value, startIndex, startIndex + count);
	}

	#endregion

	#region Find 方法系列

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Import, "static System.Array.Find<T>(T[], System.Predicate<T>)")]
	public static T? _1dfc77048ccf0234(Array<T> array, Predicate<T> match)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (match == null)
			throw new Error("ArgumentNullException: match is null");
		return array.Find(match);
	}

	///<summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
	[Jazor(Op.Import, "static System.Array.FindAll<T>(T[], System.Predicate<T>)")]
	public static Array<T> _b373eb093e6c7b63(Array<T> array, Predicate<T> match)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (match == null)
			throw new Error("ArgumentNullException: match is null");
		return array.Filter(match);
	}

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Import, "static System.Array.FindIndex<T>(T[], System.Predicate<T>)")]
	public static Number _64f5a7fd5c436edb(Array<T> array, Predicate<T> match)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (match == null)
			throw new Error("ArgumentNullException: match is null");
		return array.FindIndex(match);
	}

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the specified index to the last element.</summary>
	[Jazor(Op.Import, "static System.Array.FindIndex<T>(T[], int, System.Predicate<T>)")]
	public static Number _42e008ba24b77e94(Array<T> array, Number startIndex, Predicate<T> match)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (match == null)
			throw new Error("ArgumentNullException: match is null");
		if (startIndex < 0 || startIndex > array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");

		for (var i = startIndex; i < array.Length; i++)
		{
			if (match(array[i]))
				return i;
		}
		return -1;
	}

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that starts at the specified index and contains the specified number of elements.</summary>
	[Jazor(Op.Import, "static System.Array.FindIndex<T>(T[], int, int, System.Predicate<T>)")]
	public static Number _fdfc005bdc859fff(Array<T> array, Number startIndex, Number count, Predicate<T> match)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (match == null)
			throw new Error("ArgumentNullException: match is null");
		if (startIndex < 0 || startIndex > array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
		if (count < 0 || startIndex + count > array.Length)
			throw new Error("ArgumentOutOfRangeException: count is out of range");

		for (var i = startIndex; i < startIndex + count; i++)
		{
			if (match(array[i]))
				return i;
		}
		return -1;
	}

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Import, "static System.Array.FindLast<T>(T[], System.Predicate<T>)")]
	public static T? _2786abe2cff245fa(Array<T> array, Predicate<T> match)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (match == null)
			throw new Error("ArgumentNullException: match is null");

		for (var i = array.Length - 1; i >= 0; i--)
		{
			if (match(array[i]))
				return array[i];
		}
		return MissingValue();
	}

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Import, "static System.Array.FindLastIndex<T>(T[], System.Predicate<T>)")]
	public static Number _ea3118f38aa5f363(Array<T> array, Predicate<T> match)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (match == null)
			throw new Error("ArgumentNullException: match is null");

		for (var i = array.Length - 1; i >= 0; i--)
		{
			if (match(array[i]))
				return i;
		}
		return -1;
	}

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	[Jazor(Op.Import, "static System.Array.FindLastIndex<T>(T[], int, System.Predicate<T>)")]
	public static Number _56359f972a00ab73(Array<T> array, Number startIndex, Predicate<T> match)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (match == null)
			throw new Error("ArgumentNullException: match is null");
		if (startIndex < -1 || startIndex >= array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");

		for (var i = startIndex; i >= 0; i--)
		{
			if (match(array[i]))
				return i;
		}
		return -1;
	}

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	[Jazor(Op.Import, "static System.Array.FindLastIndex<T>(T[], int, int, System.Predicate<T>)")]
	public static Number _6b63489e941ef0f0(Array<T> array, Number startIndex, Number count, Predicate<T> match)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (match == null)
			throw new Error("ArgumentNullException: match is null");
		if (startIndex < -1 || startIndex >= array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
		if (count < 0 || count > startIndex + 1)
			throw new Error("ArgumentOutOfRangeException: count is out of range");

		var endIndex = startIndex - count + 1;
		for (var i = startIndex; i >= endIndex; i--)
		{
			if (match(array[i]))
				return i;
		}
		return -1;
	}

	#endregion

	#region ForEach

	///<summary>Performs the specified action on each element of the specified array.</summary>
	[Jazor(Op.Import, "static System.Array.ForEach<T>(T[], System.Action<T>)")]
	public static void _ad1c39ab55fe27b9(Array<T> array, Action<T> action)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (action == null)
			throw new Error("ArgumentNullException: action is null");
		array.ForEach(action);
	}

	#endregion

	#region IndexOf 方法系列

	///<summary>Searches for the specified object and returns the index of its first occurrence in a one-dimensional array.</summary>
	[Jazor(Op.Import, "static System.Array.IndexOf(System.Array, object)")]
	public static Number _cde8d7a78af8dc9a(Array<T> array, T? value)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");

		if (value is null)
			return -1;

		return array.IndexOf(value);
	}

	///<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of its first occurrence. The range extends from a specified index to the end of the array.</summary>
	[Jazor(Op.Import, "static System.Array.IndexOf(System.Array, object, int)")]
	public static Number _2151f4cd0a63b0a2(Array<T> array, object? value, Number startIndex)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (startIndex < 0 || startIndex > array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");

		for (var i = startIndex; i < array.Length; i++)
		{
			if (Equals(array[i], value))
				return i;
		}
		return -1;
	}

	///<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of ifs first occurrence. The range extends from a specified index for a specified number of elements.</summary>
	[Jazor(Op.Import, "static System.Array.IndexOf(System.Array, object, int, int)")]
	public static Number _c419efc216312a6a(Array<T> array, T? value, Number startIndex, Number count)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (startIndex < 0 || startIndex > array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
		if (count < 0 || startIndex + count > array.Length)
			throw new Error("ArgumentOutOfRangeException: count is out of range");

		for (Number i = startIndex; i < startIndex + count; i++)
		{
			if (Equals(array[i], value))
				return i;
		}
		return -1;
	}

	///<summary>Searches for the specified object and returns the index of its first occurrence in a one-dimensional array.</summary>
	[Jazor(Op.Import, "static System.Array.IndexOf<T>(T[], T)")]
	public static Number _34e8668cac3c06fa(Array<T> array, T value)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		return array.IndexOf(value);
	}

	///<summary>Searches for the specified object in a range of elements of a one dimensional array, and returns the index of its first occurrence. The range extends from a specified index to the end of the array.</summary>
	[Jazor(Op.Import, "static System.Array.IndexOf<T>(T[], T, int)")]
	public static Number _d7a4d17a98a17e7e(Array<T> array, T value, Number startIndex)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (startIndex < 0 || startIndex > array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");

		for (Number i = startIndex; i < array.Length; i++)
		{
			if (Equals(array[i], value))
				return i;
		}
		return -1;
	}

	///<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of its first occurrence. The range extends from a specified index for a specified number of elements.</summary>
	[Jazor(Op.Import, "static System.Array.IndexOf<T>(T[], T, int, int)")]
	public static Number _e3d80b27a67e8a0d(Array<T> array, T value, Number startIndex, Number count)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (startIndex < 0 || startIndex > array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
		if (count < 0 || startIndex + count > array.Length)
			throw new Error("ArgumentOutOfRangeException: count is out of range");

		for (var i = startIndex; i < startIndex + count; i++)
		{
			if (Equals(array[i], value))
				return i;
		}
		return -1;
	}

	#endregion

	#region LastIndexOf 方法系列

	///<summary>Searches for the specified object and returns the index of the last occurrence within the entire one-dimensional <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Import, "static System.Array.LastIndexOf(System.Array, object)")]
	public static Number _85801a2dbc247f17(Array<T> array, T? value)
	{
		if (array is null)
			throw new Error("ArgumentNullException: array is null");

		if (value is null)
			return -1;

		return array.LastIndexOf(value);
	}

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	[Jazor(Op.Import, "static System.Array.LastIndexOf(System.Array, object, int)")]
	public static Number _6b23455f7b2f95ff(Array<T> array, object? value, Number startIndex)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (startIndex < -1 || startIndex >= array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");

		for (var i = startIndex; i >= 0; i--)
		{
			if (Equals(array[i], value))
				return i;
		}
		return -1;
	}

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	[Jazor(Op.Import, "static System.Array.LastIndexOf(System.Array, object, int, int)")]
	public static Number _7f5af90fd2a084fe(Array<T> array, object? value, Number startIndex, Number count)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (startIndex < -1 || startIndex >= array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
		if (count < 0 || count > startIndex + 1)
			throw new Error("ArgumentOutOfRangeException: count is out of range");

		var endIndex = startIndex - count + 1;
		for (var i = startIndex; i >= endIndex; i--)
		{
			if (Equals(array[i], value))
				return i;
		}
		return -1;
	}

	///<summary>Searches for the specified object and returns the index of the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Import, "static System.Array.LastIndexOf<T>(T[], T)")]
	public static Number _198d0f4fcb1c0679(Array<T> array, T value)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		return array.LastIndexOf(value);
	}

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	[Jazor(Op.Import, "static System.Array.LastIndexOf<T>(T[], T, int)")]
	public static Number _5c2c6aa99d0e0549(Array<T> array, T value, Number startIndex)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (startIndex < -1 || startIndex >= array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");

		for (var i = startIndex; i >= 0; i--)
		{
			if (Equals(array[i], value))
				return i;
		}
		return -1;
	}

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	[Jazor(Op.Import, "static System.Array.LastIndexOf<T>(T[], T, int, int)")]
	public static Number _b5bf131d8947c855(Array<T> array, T value, Number startIndex, Number count)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (startIndex < -1 || startIndex >= array.Length)
			throw new Error("ArgumentOutOfRangeException: startIndex is out of range");
		if (count < 0 || count > startIndex + 1)
			throw new Error("ArgumentOutOfRangeException: count is out of range");

		var endIndex = startIndex - count + 1;
		for (var i = startIndex; i >= endIndex; i--)
		{
			if (Equals(array[i], value))
				return i;
		}
		return -1;
	}

	#endregion

	#region Reverse 方法系列

	///<summary>Reverses the sequence of the elements in the entire one-dimensional <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Inline, "static System.Array.Reverse(System.Array)", "__arg1.reverse()")]
	public extern static void _c02ce18f02385f3d(Array<T> array);

	///<summary>Reverses the sequence of a subset of the elements in the one-dimensional <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Import, "static System.Array.Reverse(System.Array, int, int)")]
	public static void _36c04f95b4ffdfd5(Array<T> array, Number index, Number length)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0)
			throw new Error("ArgumentOutOfRangeException: index is less than zero");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (index + length > array.Length)
			throw new Error("ArgumentException: index + length exceeds array length");

		// 部分反转
		var endIndex = index + length - 1;
		while (index < endIndex)
		{
			var temp = array[index];
			array[index] = array[endIndex];
			array[endIndex] = temp;
			index++;
			endIndex--;
		}
	}

	///<summary>Reverses the sequence of the elements in the one-dimensional generic array.</summary>
	[Jazor(Op.Import, "static System.Array.Reverse<T>(T[])")]
	public static void _e2b02681782c394b(Array<T> array)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		array.Reverse();
	}

	///<summary>Reverses the sequence of a subset of the elements in the one-dimensional generic array.</summary>
	[Jazor(Op.Import, "static System.Array.Reverse<T>(T[], int, int)")]
	public static void _5b0cbdf276c63339(Array<T> array, Number index, Number length)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0)
			throw new Error("ArgumentOutOfRangeException: index is less than zero");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (index + length > array.Length)
			throw new Error("ArgumentException: index + length exceeds array length");

		var endIndex = index + length - 1;
		while (index < endIndex)
		{
			var temp = array[index];
			array[index] = array[endIndex];
			array[endIndex] = temp;
			index++;
			endIndex--;
		}
	}

	#endregion

	#region Sort 方法系列

	///<summary>Sorts the elements in an entire one-dimensional <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Import, "static System.Array.Sort(System.Array)")]
	public static void _07ee8311aaf13b6b(Array<T> array)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");

		array.Sort((left, right) => CompareDefault(left, right));
	}

	///<summary>Sorts a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each key.</summary>
	[Jazor(Op.Import, "static System.Array.Sort(System.Array, System.Array)")]
	public static void _4df21ca760120c59(Array<T> keys, Array items)
	{
		if (keys == null)
			throw new Error("ArgumentNullException: keys is null");
		if (items != null && keys.Length != items.Length)
			throw new Error("ArgumentException: keys and items have different lengths");
		// JS 不支持直接按键排序两个数组，需要实现
		// 简化实现：只排序 keys，忽略 items
		keys.Sort((left, right) => CompareDefault(left, right));
	}

	///<summary>Sorts the elements in a range of elements in a one-dimensional <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Import, "static System.Array.Sort(System.Array, int, int)")]
	public static void _4e10132b81a43421(Array<T> array, Number index, Number length)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0)
			throw new Error("ArgumentOutOfRangeException: index is less than zero");
		if (length < 0)
			throw new Error("ArgumentOutOfRangeException: length is less than zero");
		if (index + length > array.Length)
			throw new Error("ArgumentException: index + length exceeds array length");

		// 提取子数组，排序后放回
		var subArray = array.Slice(index, index + length);
		subArray.Sort((left, right) => CompareDefault(left, right));
		for (Number i = 0; i < length; i++)
			array[index + i] = subArray[i];
	}

	///<summary>Sorts a range of elements in a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each key.</summary>
	[Jazor(Op.Import, "static System.Array.Sort(System.Array, System.Array, int, int)")]
	public static void _12789d2affa27035(Array<T> keys, Array items, Number index, Number length)
	{
		if (keys == null)
			throw new Error("ArgumentNullException: keys is null");
		if (index < 0 || length < 0 || index + length > keys.Length)
			throw new Error("ArgumentException: invalid index or length");

		var subArray = keys.Slice(index, index + length);
		subArray.Sort((left, right) => CompareDefault(left, right));
		for (Number i = 0; i < length; i++)
			keys[index + i] = subArray[i];
	}

	///<summary>Sorts the elements in a one-dimensional <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Import, "static System.Array.Sort(System.Array, System.Collections.IComparer)")]
	public static void _093c373956602c04(Array<T> array, System.Collections.IComparer comparer)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (comparer == null)
			array.Sort((left, right) => CompareDefault(left, right));
		else
			array.Sort((a, b) => comparer.Compare(a, b));
	}

	///<summary>Sorts a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Import, "static System.Array.Sort(System.Array, System.Array, System.Collections.IComparer)")]
	public static void _122404a1fc2867ba(Array<T> keys, Array<T> items, System.Collections.IComparer comparer)
	{
		if (keys == null)
			throw new Error("ArgumentNullException: keys is null");
		if (comparer == null)
			keys.Sort((left, right) => CompareDefault(left, right));
		else
			keys.Sort((a, b) => comparer.Compare(a, b));
	}

	///<summary>Sorts the elements in a range of elements in a one-dimensional <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Import, "static System.Array.Sort(System.Array, int, int, System.Collections.IComparer)")]
	public static void _b2141b8c013bc1b0(Array<T> array, Number index, Number length, System.Collections.IComparer comparer)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0 || length < 0 || index + length > array.Length)
			throw new Error("ArgumentException: invalid index or length");

		var subArray = array.Slice(index, index + length);
		if (comparer == null)
			subArray.Sort((left, right) => CompareDefault(left, right));
		else
			subArray.Sort((a, b) => comparer.Compare(a, b));
		for (Number i = 0; i < length; i++)
			array[index + i] = subArray[i];
	}

	///<summary>Sorts a range of elements in a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Import, "static System.Array.Sort(System.Array, System.Array, int, int, System.Collections.IComparer)")]
	public static void _a95c3f83e8cd4623(Array<T> keys, Array<T> items, Number index, Number length, System.Collections.IComparer comparer)
	{
		if (keys == null)
			throw new Error("ArgumentNullException: keys is null");
		if (index < 0 || length < 0 || index + length > keys.Length)
			throw new Error("ArgumentException: invalid index or length");

		var subArray = keys.Slice(index, index + length);
		if (comparer == null)
			subArray.Sort((left, right) => CompareDefault(left, right));
		else
			subArray.Sort((a, b) => comparer.Compare(a, b));
		for (Number i = 0; i < length; i++)
			keys[index + i] = subArray[i];
	}

	///<summary>Sorts the elements in an entire <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Import, "static System.Array.Sort<T>(T[])")]
	public static void _382add2bad872f67(Array<T> array)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		array.Sort((left, right) => CompareDefault(left, right));
	}

	///<summary>Sorts a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each key.</summary>
	[Jazor(Op.Import, "static System.Array.Sort<TKey, TValue>(TKey[], TValue[])")]
	public static void _1a3ebd994898c67c<TKey, TValue>(Array<TKey> keys, Array<TValue> items)
	{
		if (keys == null)
			throw new Error("ArgumentNullException: keys is null");
		keys.Sort((left, right) => CompareDefaultKey(left, right));
	}

	///<summary>Sorts the elements in a range of elements in an <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Import, "static System.Array.Sort<T>(T[], int, int)")]
	public static void _80e6f8922ae8703c(Array<T> array, Number index, Number length)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0 || length < 0 || index + length > array.Length)
			throw new Error("ArgumentException: invalid index or length");

		var subArray = array.Slice(index, index + length);
		subArray.Sort((left, right) => CompareDefault(left, right));
		for (Number i = 0; i < length; i++)
			array[index + i] = subArray[i];
	}

	///<summary>Sorts a range of elements in a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each key.</summary>
	[Jazor(Op.Import, "static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int)")]
	public static void _9b803c8e781cf3c0<TKey, TValue>(Array<TKey> keys, Array<TValue> items, Number index, Number length)
	{
		if (keys == null)
			throw new Error("ArgumentNullException: keys is null");
		if (index < 0 || length < 0 || index + length > keys.Length)
			throw new Error("ArgumentException: invalid index or length");

		var subArray = keys.Slice(index, index + length);
		subArray.Sort((left, right) => CompareDefaultKey(left, right));
		for (Number i = 0; i < length; i++)
			keys[index + i] = subArray[i];
	}

	///<summary>Sorts the elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Import, "static System.Array.Sort<T>(T[], System.Collections.Generic.IComparer<T>)")]
	public static void _92474aed4e4823f3(Array<T> array, IComparer<T>? comparer)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (comparer == null)
			array.Sort((left, right) => CompareDefault(left, right));
		else
			array.Sort(comparer.Compare);
	}

	///<summary>Sorts a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Import, "static System.Array.Sort<TKey, TValue>(TKey[], TValue[], System.Collections.Generic.IComparer<TKey>)")]
	public static void _dfd5fefaaa03a228<TKey, TValue>(Array<TKey> keys, Array<TValue> items, IComparer<TKey>? comparer)
	{
		if (keys == null)
			throw new Error("ArgumentNullException: keys is null");
		if (comparer == null)
			keys.Sort((left, right) => CompareDefaultKey(left, right));
		else
			keys.Sort(comparer.Compare);
	}

	///<summary>Sorts the elements in a range of elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Import, "static System.Array.Sort<T>(T[], int, int, System.Collections.Generic.IComparer<T>)")]
	public static void _55dbc52295bd7984(Array<T> array, Number index, Number length, IComparer<T>? comparer)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (index < 0 || length < 0 || index + length > array.Length)
			throw new Error("ArgumentException: invalid index or length");

		var subArray = array.Slice(index, index + length);
		if (comparer == null)
			subArray.Sort((left, right) => CompareDefault(left, right));
		else
			subArray.Sort(comparer.Compare);
		for (Number i = 0; i < length; i++)
			array[index + i] = subArray[i];
	}

	///<summary>Sorts a range of elements in a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Import, "static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int, System.Collections.Generic.IComparer<TKey>)")]
	public static void _f3e7263659ac2e30<TKey, TValue>(Array<TKey> keys, Array<TValue> items, Number index, Number length, IComparer<TKey>? comparer)
	{
		if (keys == null)
			throw new Error("ArgumentNullException: keys is null");
		if (index < 0 || length < 0 || index + length > keys.Length)
			throw new Error("ArgumentException: invalid index or length");

		var subArray = keys.Slice(index, index + length);
		if (comparer == null)
			subArray.Sort((left, right) => CompareDefaultKey(left, right));
		else
			subArray.Sort(comparer.Compare);
		for (Number i = 0; i < length; i++)
			keys[index + i] = subArray[i];
	}

	///<summary>Sorts the elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Comparison`1" />.</summary>
	[Jazor(Op.Import, "static System.Array.Sort<T>(T[], System.Comparison<T>)")]
	public static void _c8fcae59a3aca6f6(Array<T> array, Comparison<T> comparison)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (comparison == null)
			throw new Error("ArgumentNullException: comparison is null");
		array.Sort(comparison);
	}

	#endregion

	#region TrueForAll

	///<summary>Determines whether every element in the array matches the conditions defined by the specified predicate.</summary>
	[Jazor(Op.Import, "static System.Array.TrueForAll<T>(T[], System.Predicate<T>)")]
	public static bool _7deb21b3fbe579c9(Array<T> array, Predicate<T> match)
	{
		if (array == null)
			throw new Error("ArgumentNullException: array is null");
		if (match == null)
			throw new Error("ArgumentNullException: match is null");
		return array.Every(match);
	}

	#endregion

	#region MaxLength

	[Jazor(Op.Inline, "static System.Array.MaxLength.get", "4294967295")]
	public extern static Number _a7a42b1fbdbc7628();

	#endregion

	#region GetEnumerator

	///<summary>Returns an <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard, "System.Array.GetEnumerator()")]
	public extern static System.Collections.IEnumerator _1e9012cd200b3827(System.Array instance);

	#endregion
}
