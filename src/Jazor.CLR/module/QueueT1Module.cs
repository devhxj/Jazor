namespace Jazor.CLR;

/// <summary>
/// Queue&lt;T&gt; 走 CLR runtime wrapper，避免 raw JavaScript 宿主降级。
/// carrier 以 items + head 游标保留 FIFO 顺序；未实现的容量管理成员仍保持 unsupported。
/// </summary>
[ECMAScriptModule("System/Collections/Generic/QueueT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.Queue<T>", "Object")]
public static class QueueT1Module<T>
{
	private static void EnsureInstance(RuntimeModule.JQueue<T>? instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
	}

	private static Number CountCore(RuntimeModule.JQueue<T> instance)
		=> instance.Items.Length - instance.Head;

	private static T DequeueCore(RuntimeModule.JQueue<T> instance)
	{
		if (CountCore(instance) == 0)
			throw new Error("InvalidOperationException: Queue is empty.");

		var value = instance.Items[instance.Head];
		instance.Head++;
		return value;
	}

	private static T PeekCore(RuntimeModule.JQueue<T> instance)
	{
		if (CountCore(instance) == 0)
			throw new Error("InvalidOperationException: Queue is empty.");

		return instance.Items[instance.Head];
	}

	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.Count.get")]
	public static Number _874ffef6d586566e(RuntimeModule.JQueue<T> instance)
	{
		EnsureInstance(instance);
		return CountCore(instance);
	}

	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.Clear()")]
	public static void _c1380aa32ab3b19e(RuntimeModule.JQueue<T> instance)
	{
		EnsureInstance(instance);
		instance.Items.Splice(0, instance.Items.Length);
		instance.Head = 0;
	}

	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.Enqueue(T)")]
	public static void _8a87022169c02c22(RuntimeModule.JQueue<T> instance, T item)
	{
		EnsureInstance(instance);
		instance.Items.Push(item);
	}

	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.Dequeue()")]
	public static T _9828432fec9d535a(RuntimeModule.JQueue<T> instance)
	{
		EnsureInstance(instance);
		return DequeueCore(instance);
	}

	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.TryDequeue(out T)")]
	public static Array<object?> _96c6e0d13a99b6ff(RuntimeModule.JQueue<T> instance)
	{
		EnsureInstance(instance);
		return CountCore(instance) == 0 ? [false, null] : [true, DequeueCore(instance)];
	}

	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.Peek()")]
	public static T _e17f3e583930e78f(RuntimeModule.JQueue<T> instance)
	{
		EnsureInstance(instance);
		return PeekCore(instance);
	}

	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.TryPeek(out T)")]
	public static Array<object?> _35559a67cebb0fd9(RuntimeModule.JQueue<T> instance)
	{
		EnsureInstance(instance);
		return CountCore(instance) == 0 ? [false, null] : [true, PeekCore(instance)];
	}

	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.Contains(T)")]
	public static bool _45549ae297d2d16d(RuntimeModule.JQueue<T> instance, T item)
	{
		EnsureInstance(instance);
		for (var index = instance.Head; index < instance.Items.Length; index++)
		{
			if (EqualityComparerT1Module<T>.EqualsCore(instance.Items[index], item))
				return true;
		}

		return false;
	}

	[Jazor(Op.Import, "System.Collections.Generic.Queue<T>.ToArray()")]
	public static Array<T> _8cda2376e71ddbd2(RuntimeModule.JQueue<T> instance)
	{
		EnsureInstance(instance);
		var result = new Array<T>();
		for (var index = instance.Head; index < instance.Items.Length; index++)
			result.Push(instance.Items[index]);
		return result;
	}

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
