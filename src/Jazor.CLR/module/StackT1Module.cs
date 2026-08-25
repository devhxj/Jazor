namespace Jazor.CLR;

/// <summary>
/// Stack&lt;T&gt; 走 CLR runtime wrapper，避免 raw JavaScript 宿主降级。
/// carrier 直接以数组尾部表示栈顶；未实现的容量管理成员仍保持 unsupported。
/// </summary>
[ECMAScriptModule("System/Collections/Generic/StackT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.Stack<T>", "Object")]
public static class StackT1Module<T>
{
	private static void EnsureInstance(RuntimeModule.JStack<T>? instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");
	}

	private static Number CountCore(RuntimeModule.JStack<T> instance)
		=> instance.Items.Length;

	private static T PeekCore(RuntimeModule.JStack<T> instance)
	{
		if (CountCore(instance) == 0)
			throw new Error("InvalidOperationException: Stack is empty.");

		return instance.Items[instance.Items.Length - 1];
	}

	private static T PopCore(RuntimeModule.JStack<T> instance)
	{
		var value = PeekCore(instance);
		instance.Items.Splice(instance.Items.Length - 1, 1);
		return value;
	}

	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.Count.get")]
	public static Number _ec97cc120d8d804b(RuntimeModule.JStack<T> instance)
	{
		EnsureInstance(instance);
		return CountCore(instance);
	}

	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.Clear()")]
	public static void _431a6c983678bc4d(RuntimeModule.JStack<T> instance)
	{
		EnsureInstance(instance);
		instance.Items.Splice(0, instance.Items.Length);
	}

	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.Contains(T)")]
	public static bool _f8679c85a69f0514(RuntimeModule.JStack<T> instance, T item)
	{
		EnsureInstance(instance);
		for (var index = 0; index < instance.Items.Length; index++)
		{
			if (EqualityComparerT1Module<T>.EqualsCore(instance.Items[index], item))
				return true;
		}

		return false;
	}

	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.Peek()")]
	public static T _c406861f59a5ccaf(RuntimeModule.JStack<T> instance)
	{
		EnsureInstance(instance);
		return PeekCore(instance);
	}

	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.TryPeek(out T)")]
	public static Array<object?> _fa141b6d3bc0d25a(RuntimeModule.JStack<T> instance)
	{
		EnsureInstance(instance);
		return CountCore(instance) == 0 ? [false, null] : [true, PeekCore(instance)];
	}

	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.Pop()")]
	public static T _26474a0aeb01f889(RuntimeModule.JStack<T> instance)
	{
		EnsureInstance(instance);
		return PopCore(instance);
	}

	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.TryPop(out T)")]
	public static Array<object?> _247c56433f8b7216(RuntimeModule.JStack<T> instance)
	{
		EnsureInstance(instance);
		return CountCore(instance) == 0 ? [false, null] : [true, PopCore(instance)];
	}

	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.Push(T)")]
	public static void _c18157d266fca530(RuntimeModule.JStack<T> instance, T item)
	{
		EnsureInstance(instance);
		instance.Items.Push(item);
	}

	[Jazor(Op.Import, "System.Collections.Generic.Stack<T>.ToArray()")]
	public static Array<T> _e40d0cf595a7fe44(RuntimeModule.JStack<T> instance)
	{
		EnsureInstance(instance);
		var result = new Array<T>();
		for (var index = instance.Items.Length; index > 0; index--)
			result.Push(instance.Items[index - 1]);
		return result;
	}

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
