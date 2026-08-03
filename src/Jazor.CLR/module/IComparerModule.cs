namespace Jazor.CLR;

/// <summary>
/// System.Collections.IComparer 模块映射规则
///
/// 当前开放与 Comparer&lt;T&gt;.Default 可直接联动的：
/// - Compare(object, object)
/// </summary>
[ECMAScriptModule("System/Collections/IComparerModule.js")]
[Jazor(Op.Alias, "System.Collections.IComparer", "Object")]
public static class IComparerModule
{
	/// <summary>
	/// C#: comparer.Compare(x, y)
	/// JS: 复用 Comparer 的核心比较逻辑
	/// </summary>
	[Jazor(Op.Import, "System.Collections.IComparer.Compare(object, object)")]
	public static Number _7dffdd7244581cc5(object instance, object? x, object? y)
	{
		ComparerT1Module<object?>.EnsureComparerInstance(instance);
		var compare = Reflect.Get(instance, "compare");
		if (compare == null)
			throw new Error("MissingMethodException: comparer does not expose compare.");

		return (Number)Reflect.Apply(compare, instance, [x, y])!;
	}
}
