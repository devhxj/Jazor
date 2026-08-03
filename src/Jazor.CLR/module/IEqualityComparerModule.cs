namespace Jazor.CLR;

/// <summary>
/// System.Collections.IEqualityComparer 模块映射规则
///
/// 当前开放与 EqualityComparer&lt;T&gt;.Default 可直接联动的：
/// - Equals(object, object)
/// - GetHashCode(object)
/// </summary>
[ECMAScriptModule("System/Collections/IEqualityComparerModule.js")]
[Jazor(Op.Alias, "System.Collections.IEqualityComparer", "Object")]
public static class IEqualityComparerModule
{
	/// <summary>
	/// C#: comparer.Equals(x, y)
	/// JS: 复用 EqualityComparer 的核心等价逻辑
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IEqualityComparer.Equals(object, object)")]
	public static bool _eb0a1792ad8b44b7(object instance, object? x, object? y)
	{
		EqualityComparerT1Module<object?>.EnsureComparerInstance(instance);
		var equals = Reflect.Get(instance, "equals");
		if (equals == null)
			throw new Error("MissingMethodException: comparer does not expose equals.");

		return (bool)Reflect.Apply(equals, instance, [x, y])!;
	}

	/// <summary>
	/// C#: comparer.GetHashCode(obj)
	/// JS: 复用 EqualityComparer 的核心哈希逻辑
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IEqualityComparer.GetHashCode(object)")]
	public static Number _8f16da840d40722e(object instance, object? obj)
	{
		EqualityComparerT1Module<object?>.EnsureComparerInstance(instance);
		var getHashCode = Reflect.Get(instance, "getHashCode");
		if (getHashCode == null)
			throw new Error("MissingMethodException: comparer does not expose getHashCode.");

		return (Number)Reflect.Apply(getHashCode, instance, [obj])!;
	}
}
