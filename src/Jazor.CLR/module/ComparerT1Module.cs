namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.Comparer&lt;T&gt; 模块映射规则
///
/// 目标支持面：
/// - Comparer&lt;T&gt;.Default
/// - Comparer&lt;T&gt;.Compare(T, T)
/// </summary>
[ECMAScriptModule("System/Collections/Generic/ComparerT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.Comparer<T>", "Object")]
public static class ComparerT1Module<T>
{
	private static object? DefaultInstance = null;

	internal static void EnsureComparerInstance(object instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
	}

	internal static Number CompareObjectsCore(object? x, object? y)
	{
		if (Object.Is(x, y))
			return 0;

		if (x is null)
			return -1;
		if (y is null)
			return 1;

		if (TypeOf(x) == "number" && TypeOf(y) == "number")
		{
			var leftNumber = (Number)x;
			var rightNumber = (Number)y;
			if (IsNaN(leftNumber))
				return IsNaN(rightNumber) ? 0 : -1;
			if (IsNaN(rightNumber))
				return 1;
			if (leftNumber < rightNumber)
				return -1;
			if (leftNumber > rightNumber)
				return 1;
			return 0;
		}

		if (TypeOf(x) == "string" && TypeOf(y) == "string")
		{
			var leftString = (string)x;
			var rightString = (string)y;
			if (leftString < rightString)
				return -1;
			if (leftString > rightString)
				return 1;
			return 0;
		}

		if (TypeOf(x) == "boolean" && TypeOf(y) == "boolean")
		{
			var leftBool = (bool)x;
			var rightBool = (bool)y;
			return leftBool == rightBool ? 0 : (leftBool ? 1 : -1);
		}

		if (TypeOf(x) == "bigint" && TypeOf(y) == "bigint")
		{
			var leftBigInt = (BigInt)x;
			var rightBigInt = (BigInt)y;
			if (leftBigInt < rightBigInt)
				return -1;
			if (leftBigInt > rightBigInt)
				return 1;
			return 0;
		}

		throw new Error("ArgumentException: At least one object must implement IComparable.");
	}

	internal static Number CompareCore(T x, T y)
		=> CompareObjectsCore(x, y);

	internal static Number CompareInstance(object instance, T x, T y)
	{
		EnsureComparerInstance(instance);
		var compare = Reflect.Get(instance, "compare");
		if (compare == null)
			throw new Error("MissingMethodException: comparer does not expose compare.");

		return (Number)Reflect.Apply(compare, instance, [x, y])!;
	}

	/// <summary>
	/// C#: Comparer&lt;T&gt;.Default
	/// JS: 全局缓存单例比较器对象
	/// </summary>
	[Jazor(Op.Import, "static System.Collections.Generic.Comparer<T>.Default.get", "getDefault")]
	public static object GetDefault()
	{
		if (DefaultInstance == null)
		{
			var instance = Object.Create(null);
			Reflect.Set(instance, "compare", (Func<T, T, Number>)CompareCore);
			DefaultInstance = instance;
		}

		return DefaultInstance;
	}

	/// <summary>
	/// C#: comparer.Compare(x, y)
	/// </summary>
	[Jazor(Op.Import, "virtual System.Collections.Generic.Comparer<T>.Compare(T, T)")]
	public static Number _a4222c99b516b861(object instance, T x, T y)
		=> CompareInstance(instance, x, y);
}
