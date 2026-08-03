namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.IComparer&lt;T&gt; 模块映射规则
///
/// 当前仅开放与 Comparer&lt;T&gt;.Default 直接联动的 Compare 调用。
/// </summary>
[ECMAScriptModule("System/Collections/Generic/IComparerT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.IComparer<T>", "Object")]
public static class IComparerT1Module<T>
{
	/// <summary>
	/// C#: comparer.Compare(x, y)
	/// JS: 复用 Comparer&lt;T&gt; 的核心比较逻辑
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.IComparer<T>.Compare(T, T)")]
	public static Number _0289dcf579b8a65e(object instance, T x, T y)
	{
		ComparerT1Module<T>.EnsureComparerInstance(instance);
		var compare = Reflect.Get(instance, "compare");
		if (compare == null)
			throw new Error("MissingMethodException: comparer does not expose compare.");

		return (Number)Reflect.Apply(compare, instance, [x, y])!;
	}
}
