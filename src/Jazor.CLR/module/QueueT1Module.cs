namespace Jazor.CLR;

/// <summary>
/// Queue&lt;T&gt; 走 CLR runtime wrapper，避免 raw JavaScript 宿主降级。
/// 当前仅开放构造与类型识别；成员仍需显式白名单后才允许使用。
/// </summary>
[ECMAScriptModule("System/Collections/Generic/QueueT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.Queue<T>", "Object")]
public static class QueueT1Module<T>
{
	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.Queue()")]
	public static RuntimeModule.JQueue<T> _ea05a56d08fbd4f9()
		=> new();

	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.Queue(int)")]
	public static RuntimeModule.JQueue<T> _7fc2b76467c43db9(Number capacity)
		=> RuntimeModule.JQueue<T>.WithCapacity(capacity);

	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.Queue(System.Collections.Generic.IEnumerable<T>)")]
	public static RuntimeModule.JQueue<T> _5eae085d83bbe242(IEnumerable<T> collection)
		=> new(collection);
}
