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

		if (x is Number leftNumber && y is Number rightNumber)
		{
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

		if (x is string leftString && y is string rightString)
		{
			if (leftString < rightString)
				return -1;
			if (leftString > rightString)
				return 1;
			return 0;
		}

		if (x is bool leftBool && y is bool rightBool)
			return leftBool == rightBool ? 0 : (leftBool ? 1 : -1);

		if (x is BigInt leftBigInt && y is BigInt rightBigInt)
		{
			if (leftBigInt < rightBigInt)
				return -1;
			if (leftBigInt > rightBigInt)
				return 1;
			return 0;
		}

		var leftText = x.ToString();
		var rightText = y.ToString();
		if (leftText is null)
			return rightText is null ? 0 : -1;
		if (rightText is null)
			return 1;
		if (leftText == rightText)
			return 0;
		return leftText < rightText ? -1 : 1;
	}

	internal static Number CompareCore(T x, T y)
		=> CompareObjectsCore(x, y);

	/// <summary>
	/// C#: Comparer&lt;T&gt;.Default
	/// JS: 全局缓存单例比较器对象
	/// </summary>
	[Jazor(Op.Inline, "static System.Collections.Generic.Comparer<T>.Default.get", "(globalThis.__jazorComparerDefault ??= {})")]
	public extern static object _6845b441a35aaf43();

	/// <summary>
	/// C#: comparer.Compare(x, y)
	/// </summary>
	[Jazor(Op.Import, "virtual System.Collections.Generic.Comparer<T>.Compare(T, T)")]
	public static Number _a4222c99b516b861(object instance, T x, T y)
	{
		// Keep receiver null-check: virtual instance call on null must surface NullReferenceException semantics.
		EnsureComparerInstance(instance);
		return CompareCore(x, y);
	}
}
