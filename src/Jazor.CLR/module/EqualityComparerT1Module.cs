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
	private static object? DefaultInstance = null;

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

	internal static bool EqualsCore(T? left, T? right)
	{
		if (Object.Is(left, right))
			return true;

		if (TypeOf(left) == "number" && TypeOf(right) == "number")
			return (Number)(object)left! == (Number)(object)right!;

		return false;
	}

	internal static Number GetHashCodeCore(T? value)
	{
		if (value is null)
			return 0;

		if (TypeOf(value) == "boolean")
			return (bool)(object)value ? 1 : 0;

		if (TypeOf(value) == "number")
		{
			var numberValue = (Number)(object)value;
			if (IsNaN(numberValue) || numberValue == 0)
				return 0;

			if (Math.FloorFn(numberValue) == numberValue &&
				numberValue >= -2147483648 &&
				numberValue <= 2147483647)
				return numberValue | 0;

			return HashStringCore(numberValue.ToString());
		}

		if (TypeOf(value) == "string")
			return HashStringCore((string)(object)value);

		if (TypeOf(value) == "bigint")
			return HashStringCore(((BigInt)(object)value).ToString());

		var text = value.ToString();
		return text is null ? 0 : HashStringCore(text);
	}

	internal static bool EqualsInstance(object instance, T x, T y)
	{
		EnsureComparerInstance(instance);
		var equals = Reflect.Get(instance, "equals");
		if (equals == null)
			throw new Error("MissingMethodException: comparer does not expose equals.");

		return (bool)Reflect.Apply(equals, instance, [x, y])!;
	}

	internal static Number GetHashCodeInstance(object instance, T value)
	{
		EnsureComparerInstance(instance);
		var getHashCode = Reflect.Get(instance, "getHashCode");
		if (getHashCode == null)
			throw new Error("MissingMethodException: comparer does not expose getHashCode.");

		return (Number)Reflect.Apply(getHashCode, instance, [value])!;
	}

	/// <summary>
	/// C#: EqualityComparer&lt;T&gt;.Default
	/// JS: 全局缓存单例比较器对象
	/// </summary>
	[Jazor(Op.Import, "static System.Collections.Generic.EqualityComparer<T>.Default.get", "getDefault")]
	public static object GetDefault()
	{
		if (DefaultInstance == null)
		{
			var instance = Object.Create(null);
			Reflect.Set(instance, "equals", (Func<T, T, bool>)EqualsCore);
			Reflect.Set(instance, "getHashCode", (Func<T, Number>)GetHashCodeCore);
			DefaultInstance = instance;
		}

		return DefaultInstance;
	}

	/// <summary>
	/// C#: comparer.Equals(x, y)
	/// JS: Object.is + Number 特殊值（NaN、+0/-0）一致性
	/// 说明：该逻辑超过 inline 模板的可读性阈值，保留为 Import 实现。
	/// receiver 空检查保留在模块内，保证实例调用路径与 C# 空接收者语义一致。
	/// </summary>
	[Jazor(Op.Import, "virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)")]
	public static bool _4614e5ce6b42a7ad(object instance, T x, T y)
		=> EqualsInstance(instance, x, y);

	/// <summary>
	/// C#: comparer.GetHashCode(obj)
	/// JS: 统一哈希入口（覆盖 null/primitive/number NaN）
	/// </summary>
	[Jazor(Op.Import, "virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)")]
	public static Number _2c3736bd7d205921(object instance, T obj)
		=> GetHashCodeInstance(instance, obj);
}
