namespace Jazor.CLR;

/// <summary>
/// Stack&lt;T&gt; 走 CLR runtime wrapper，避免 raw JavaScript 宿主降级。
/// 当前仅开放构造与类型识别；成员仍需显式白名单后才允许使用。
/// </summary>
[ECMAScriptModule("System/Collections/Generic/StackT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.Stack<T>", "Object")]
public static class StackT1Module<T>
{
	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.Stack()")]
	public static RuntimeModule.JStack<T> _7d15fcc03d17599b()
		=> new();

	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.Stack(int)")]
	public static RuntimeModule.JStack<T> _f4ca5eb8de25d4a3(Number capacity)
		=> RuntimeModule.JStack<T>.WithCapacity(capacity);

	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.Stack(System.Collections.Generic.IEnumerable<T>)")]
	public static RuntimeModule.JStack<T> _60d564060ac5fb0f(IEnumerable<T> collection)
		=> new(collection);
}
