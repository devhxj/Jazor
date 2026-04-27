namespace Jazor.CLR;

/// <summary>
/// System.Collections.IEnumerable 类型模块映射规则
///
/// IEnumerable 是非泛型集合基接口。
/// 在当前运行时边界统一投影到 JavaScript Array。
///
/// Op 类型选择原则：
/// - Discard: 显式枚举器对象在当前运行时边界没有稳定的 CLR 等价物
/// </summary>
[ECMAScriptModule("System/Collections/IEnumerableModule.js")]
[Jazor(Op.Alias, "System.Collections.IEnumerable", "Array")]
public static class IEnumerableModule
{
	[Jazor(Op.Discard, "System.Collections.IEnumerable.GetEnumerator()")]
	public extern static System.Collections.IEnumerator _0f43a10d580eb492(Array<object?> instance);
}
