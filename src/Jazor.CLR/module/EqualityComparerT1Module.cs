namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.EqualityComparer&lt;T&gt; 模块映射规则
///
/// 目标支持面：
/// - EqualityComparer&lt;T&gt;.Default
/// - EqualityComparer&lt;T&gt;.Equals(T, T)
/// </summary>
[ECMAScriptModule("System/Collections/Generic/EqualityComparerT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.EqualityComparer<T>", "Object")]
public static class EqualityComparerT1Module<T>
{
	internal static bool EqualsCore(T left, T right)
	{
		if (Object.Is(left, right))
			return true;

		if (left is Number leftNumber && right is Number rightNumber)
			return leftNumber == rightNumber;

		return false;
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
	/// 不在此重复注入 instance 空检查，交由调用侧编译语义处理。
	/// </summary>
	[Jazor(Op.Import, "virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)")]
	public static bool _4614e5ce6b42a7ad(object instance, T x, T y)
		=> EqualsCore(x, y);
}
