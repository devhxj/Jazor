namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.IReadOnlyCollection&lt;T&gt; 类型模块映射规则。
///
/// IReadOnlyCollection&lt;T&gt; 在当前运行时边界作为 JavaScript Array 的只读集合视图。
/// </summary>
[ECMAScriptModule("System/Collections/Generic/IReadOnlyCollectionT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.IReadOnlyCollection<T>", "Array")]
public static class IReadOnlyCollectionT1Module<T>
{
	/// <summary>
	/// C#: collection.Count
	/// JS: array.length
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.IReadOnlyCollection<T>.Count.get", "length")]
	public extern static Number _b4cd9ccf8f89d4e2(Array<T> instance);
}
