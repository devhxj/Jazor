namespace Jazor.CLR;

/// <summary>
/// 将常用 System.Linq.Enumerable 查询操作投影为 JavaScript Array 方法。
/// </summary>
/// <remarks>
/// 当前实现优先覆盖高频、可直接物化为 Array 的查询路径，不承诺完整 LINQ 延迟执行模型。
/// source/predicate 等参数的空值行为和遍历顺序仍需保持 C# 可观察语义。
/// </remarks>
[ECMAScriptModule("System/Linq/EnumerableModule.js")]
[Jazor(Op.Alias, "System.Linq.Enumerable", "Array")]
public static class EnumerableModule<TSource>
{
	private static Array<TSource> Materialize(IEnumerable<TSource> source)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var result = new Array<TSource>();
		foreach (var item in source)
			result.Push(item);

		return result;
	}

	[Jazor(Op.Import, "static System.Linq.Enumerable.Where<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)")]
	public static Array<TSource> _a0d3305d7a8d4c01(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		return Materialize(source).Filter(item => predicate(item));
	}

	[Jazor(Op.Import, "static System.Linq.Enumerable.Where<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)")]
	public static Array<TSource> _0f6f6fe4a8e94447(IEnumerable<TSource> source, Func<TSource, Number, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		return Materialize(source).Filter((item, index) => predicate(item, index));
	}

	[Jazor(Op.Import, "static System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TResult>)")]
	public static Array<TResult> _0d5df18d09084f3b<TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		return Materialize(source).Map(selector);
	}

	[Jazor(Op.Import, "static System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, TResult>)")]
	public static Array<TResult> _aab4dc2444d44402<TResult>(IEnumerable<TSource> source, Func<TSource, Number, TResult> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		return Materialize(source).Map(selector);
	}

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToList<TSource>(System.Collections.Generic.IEnumerable<TSource>)")]
	public static Array<TSource> _6293e95141f14a55(IEnumerable<TSource> source)
		=> Materialize(source);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToArray<TSource>(System.Collections.Generic.IEnumerable<TSource>)")]
	public static Array<TSource> _ea56f0fe56c44ae7(IEnumerable<TSource> source)
		=> Materialize(source);
}
