namespace Jazor.CLR;

/// <summary>
/// System.IComparable&lt;T&gt; 模块映射规则
///
/// 当前仅开放 CompareTo(T) 的接口分发调用。
/// </summary>
[ECMAScriptModule("System/IComparableT1Module.js")]
[Jazor(Op.Alias, "System.IComparable<T>", "Object")]
public static class IComparableT1Module<T>
{
	/// <summary>
	/// C#: comparable.CompareTo(other)
	/// JS: 复用统一比较核心逻辑
	/// </summary>
	[Jazor(Op.Import, "System.IComparable<T>.CompareTo(T)")]
	public static Number _797b5246c9b12c8d(object instance, T other)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");

		return ComparerT1Module<T>.CompareObjectsCore(instance, other);
	}
}
