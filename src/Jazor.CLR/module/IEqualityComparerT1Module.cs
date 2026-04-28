namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.IEqualityComparer&lt;T&gt; 模块映射规则
///
/// 当前开放与 EqualityComparer&lt;T&gt;.Default 直接联动的：
/// - Equals(T, T)
/// - GetHashCode(T)
/// </summary>
[ECMAScriptModule("System/Collections/Generic/IEqualityComparerT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.IEqualityComparer<T>", "Object")]
public static class IEqualityComparerT1Module<T>
{
	/// <summary>
	/// C#: comparer.Equals(x, y)
	/// JS: 复用 EqualityComparer&lt;T&gt; 的核心等价逻辑
	/// 说明：保持实现简洁，不在此重复注入 instance 空检查。
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.IEqualityComparer<T>.Equals(T, T)")]
	public static bool _dae184550b995be1(object instance, T x, T y)
	{
		// Keep receiver null-check: interface dispatch on null must surface NullReferenceException semantics.
		EqualityComparerT1Module<T>.EnsureComparerInstance(instance);
		return EqualityComparerT1Module<T>.EqualsCore(x, y);
	}

	/// <summary>
	/// C#: comparer.GetHashCode(obj)
	/// JS: 复用 EqualityComparer&lt;T&gt; 的核心哈希逻辑
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.IEqualityComparer<T>.GetHashCode(T)")]
	public static Number _f53ff8f6435182d7(object instance, T obj)
	{
		// Keep receiver null-check: interface dispatch on null must surface NullReferenceException semantics.
		EqualityComparerT1Module<T>.EnsureComparerInstance(instance);
		return EqualityComparerT1Module<T>.GetHashCodeCore(obj);
	}
}
