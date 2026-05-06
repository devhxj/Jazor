namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.ISet&lt;T&gt; 类型模块映射规则
///
/// ISet&lt;T&gt; 运行时统一投影到 JavaScript Set。
/// 这里仅开放与 Set carrier 可稳定对齐的成员。
/// </summary>
[ECMAScriptModule("System/Collections/Generic/ISetT1Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.ISet<T>", "Set")]
public static class ISetT1Module<T>
{
	private static void EnsureWritable(Set<T> instance)
	{
		if (instance is null)
			throw new Error("NullReferenceException: instance is null.");

		if (RuntimeModule.IsReadOnlySetCarrier(instance))
			throw new Error("NotSupportedException: Collection is read-only.");
	}

	[Jazor(Op.Import, "System.Collections.Generic.ISet<T>.Add(T)")]
	public static bool _fa512a510bd763de(Set<T> instance, T item)
	{
		EnsureWritable(instance);
		return HashSetT1Module<T>._e1d2ba750a2788cb(instance, item);
	}

	[Jazor(Op.Import, "System.Collections.Generic.ISet<T>.UnionWith(System.Collections.Generic.IEnumerable<T>)")]
	public static void _d9af20d6b8c5e775(Set<T> instance, IEnumerable<T> other)
	{
		EnsureWritable(instance);
		HashSetT1Module<T>.UnionWithCore(instance, other);
	}

	[Jazor(Op.Import, "System.Collections.Generic.ISet<T>.IntersectWith(System.Collections.Generic.IEnumerable<T>)")]
	public static void _202b815f92a32e5d(Set<T> instance, IEnumerable<T> other)
	{
		EnsureWritable(instance);
		HashSetT1Module<T>.IntersectWithCore(instance, other);
	}

	[Jazor(Op.Import, "System.Collections.Generic.ISet<T>.ExceptWith(System.Collections.Generic.IEnumerable<T>)")]
	public static void _ac98ad1e0ac9efb5(Set<T> instance, IEnumerable<T> other)
	{
		EnsureWritable(instance);
		HashSetT1Module<T>.ExceptWithCore(instance, other);
	}

	[Jazor(Op.Import, "System.Collections.Generic.ISet<T>.SymmetricExceptWith(System.Collections.Generic.IEnumerable<T>)")]
	public static void _07907f6b669e590a(Set<T> instance, IEnumerable<T> other)
	{
		EnsureWritable(instance);
		HashSetT1Module<T>.SymmetricExceptWithCore(instance, other);
	}

	[Jazor(Op.Import, "System.Collections.Generic.ISet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _bcd9e5c5cd4a65e1(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.IsSubsetOfCore(instance, other);

	[Jazor(Op.Import, "System.Collections.Generic.ISet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _a64ad5f437ed3887(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.IsSupersetOfCore(instance, other);

	[Jazor(Op.Import, "System.Collections.Generic.ISet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _f7d6687c6a479566(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.IsProperSupersetOfCore(instance, other);

	[Jazor(Op.Import, "System.Collections.Generic.ISet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _bf1a417a69fffcb2(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.IsProperSubsetOfCore(instance, other);

	[Jazor(Op.Import, "System.Collections.Generic.ISet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _45e2e920f151fad2(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.OverlapsCore(instance, other);

	[Jazor(Op.Import, "System.Collections.Generic.ISet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)")]
	public static bool _afabf76c0df51242(Set<T> instance, IEnumerable<T> other)
		=> HashSetT1Module<T>.SetEqualsCore(instance, other);
}
