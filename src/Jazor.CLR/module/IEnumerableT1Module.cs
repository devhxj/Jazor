namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.IEnumerable&lt;T&gt; 类型模块映射规则
///
/// IEnumerable&lt;T&gt; 是泛型集合基接口。
/// 在当前运行时边界统一投影到 JavaScript Array。
///
/// Op 类型选择原则：
/// - Discard: 显式枚举器对象在当前运行时边界没有稳定的 CLR 等价物
/// </summary>
[ECMAScriptModule("System/Collections/Generic/IEnumerableT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.IEnumerable<T>", "Array")]
public static class IEnumerableT1Module<T>
{
	[Jazor(Op.Discard, "System.Collections.Generic.IEnumerable<T>.GetEnumerator()")]
	public extern static IEnumerator<T> _aeaa41b7af01f17e(Array<T> instance);
}
