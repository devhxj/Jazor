namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.IReadOnlyList&lt;T&gt; 类型模块映射规则。
///
/// IReadOnlyList&lt;T&gt; 在当前运行时边界作为 JavaScript Array 的只读索引视图。
/// </summary>
[ECMAScriptModule("System/Collections/Generic/IReadOnlyListT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.IReadOnlyList<T>", "Array")]
public static class IReadOnlyListT1Module<T>
{
	private static void EnsureWholeNumber(Number value, string parameterName)
	{
		if (IsNaN(value) || Math.FloorFunc(value) != value)
			throw new Error($"ArgumentOutOfRangeException: {parameterName} must be a whole number.");
	}

	/// <summary>
	/// C#: list[index]
	/// JS: array[index] with CLR-compatible range validation.
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.IReadOnlyList<T>.this[int].get")]
	public static T _b6ea5fe846ef1d65(Array<T> instance, Number index)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
		EnsureWholeNumber(index, nameof(index));
		if (index < 0 || index >= instance.Length)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");

		return instance[index];
	}
}
