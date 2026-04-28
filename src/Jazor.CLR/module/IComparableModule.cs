namespace Jazor.CLR;

/// <summary>
/// System.IComparable 模块映射规则
///
/// 当前仅开放 CompareTo(object) 的接口分发调用。
/// </summary>
[ECMAScriptModule("System/IComparableModule.js")]
[Jazor(Op.Alias, "System.IComparable", "Object")]
public static class IComparableModule
{
	/// <summary>
	/// C#: comparable.CompareTo(value)
	/// JS: 复用统一比较核心逻辑
	/// </summary>
	[Jazor(Op.Import, "System.IComparable.CompareTo(object)")]
	public static Number _7d491b9d00d63609(object instance, object? value)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");

		return ComparerT1Module<object>.CompareObjectsCore(instance, value);
	}
}
