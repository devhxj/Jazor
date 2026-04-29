namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.EqualityComparer&lt;T&gt; 模块映射规则
///
/// 目标支持面：
/// - EqualityComparer&lt;T&gt;.Default
/// - EqualityComparer&lt;T&gt;.Equals(T, T)
/// - EqualityComparer&lt;T&gt;.GetHashCode(T)
/// </summary>
[ECMAScriptModule("System/Collections/Generic/EqualityComparerT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.EqualityComparer<T>", "Object")]
public static class EqualityComparerT1Module<T>
{
	internal static void EnsureComparerInstance(object instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
	}

	private static Number HashStringCore(string text)
	{
		var hash = 17;
		for (int i = 0; i < text.Length; i++)
			hash = ((hash * 31) + text[i]) | 0;
		return hash;
	}

	internal static bool EqualsCore(T left, T right)
	{
		if (Object.Is(left, right))
			return true;

		if (left is Number leftNumber && right is Number rightNumber)
			return leftNumber == rightNumber;

		return false;
	}

	internal static Number GetHashCodeCore(T value)
	{
		if (value is null)
			return 0;

		if (value is bool boolValue)
			return boolValue ? 1 : 0;

		if (value is Number numberValue)
		{
			if (IsNaN(numberValue) || numberValue == 0)
				return 0;

			if (Math.FloorFn(numberValue) == numberValue &&
				numberValue >= -2147483648 &&
				numberValue <= 2147483647)
				return numberValue | 0;

			return HashStringCore(numberValue.ToString());
		}

		if (value is string stringValue)
			return HashStringCore(stringValue);

		if (value is BigInt bigIntValue)
			return HashStringCore(bigIntValue.ToString());

		var text = value.ToString();
		return text is null ? 0 : HashStringCore(text);
	}

	/// <summary>
	/// C#: EqualityComparer&lt;T&gt;.Default
	/// JS: 全局缓存单例比较器对象
	/// </summary>
	[Jazor(Op.Inline, "static System.Collections.Generic.EqualityComparer<T>.Default.get", "(globalThis.__jazorEqualityComparerDefault ??= {})")]
	public extern static object _74d554fc30b2950f();

	/// <summary>
	/// C#: comparer.Equals(x, y)
	/// JS: Object.is + Number 特殊值（NaN、+0/-0）一致性
	/// 说明：该逻辑超过 inline 模板的可读性阈值，保留为 Import 实现。
	/// receiver 空检查保留在模块内，保证实例调用路径与 C# 空接收者语义一致。
	/// </summary>
	[Jazor(Op.Import, "virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)")]
	public static bool _4614e5ce6b42a7ad(object instance, T x, T y)
	{
		// Keep receiver null-check: virtual instance call on null must surface NullReferenceException semantics.
		EnsureComparerInstance(instance);
		return EqualsCore(x, y);
	}

	/// <summary>
	/// C#: comparer.GetHashCode(obj)
	/// JS: 统一哈希入口（覆盖 null/primitive/number NaN）
	/// </summary>
	[Jazor(Op.Import, "virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)")]
	public static Number _2c3736bd7d205921(object instance, T obj)
	{
		// Keep receiver null-check: virtual instance call on null must surface NullReferenceException semantics.
		EnsureComparerInstance(instance);
		return GetHashCodeCore(obj);
	}
}
