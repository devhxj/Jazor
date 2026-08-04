namespace Jazor.CLR;

/// <summary>
/// 将常用 System.Linq.Enumerable 查询操作投影为 JavaScript Array 物化操作。
/// </summary>
/// <remarks>
/// 当前实现优先覆盖高频、可直接物化为 Array 的查询路径，不承诺完整 LINQ 延迟执行模型。
/// OrderBy 在调用 selector 一次后以 Comparer&lt;T&gt; 默认比较排序，并以源偏移作为稳定 tie-breaker；
/// ThenBy 仅衔接当前模块产出的 materialized order state，不能将未知 IOrderedEnumerable 误当成普通 Array 重排；
/// SelectMany 保留外层 source、collection selector 与内层 collection 的遍历顺序，并在当前调用中物化结果；
/// GroupBy、Join 与 GroupJoin 使用 EqualityComparer&lt;T&gt; 的哈希和相等语义，而不直接暴露 JavaScript Map 的键语义；
/// Distinct、Union、Except、Intersect 与 Contains 复用同一套默认相等性集合，不依赖 JavaScript Set 的键协议；
/// source/predicate 等参数的空值行为和遍历顺序仍需保持 C# 可观察语义。
/// </remarks>
[ECMAScriptModule("System/Linq/EnumerableModule.js")]
[Jazor(Op.Alias, "System.Linq.Enumerable", "Array")]
public static class EnumerableModule<TSource>
{
	// IOrderedEnumerable<T> is represented by the materialized Array<T> returned by this module.
	// Keep the carrier explicit so the CLR adapter type matches the JavaScript runtime value.
	private static readonly WeakMap<Array<TSource>, (Array<TSource> Items, Func<Number, Number, Number> Compare)> OrderedStates = new();

	// Lookup groups erase their key/element generic annotations at runtime. This private registry
	// is therefore intentionally object-shaped; it is never exposed through the CLR API surface.
	private static readonly WeakMap<object, object?> LookupComparers = new();

	private static Array<TSource> Materialize(IEnumerable<TSource> source)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var result = new Array<TSource>();
		foreach (var item in source)
			result.Push(item);

		return result;
	}

	private static Array<Number> RangeCore(Number start, Number count)
	{
		if (count < 0 || count > 0 && start + count - 1 > 2147483647)
			throw new Error("ArgumentOutOfRangeException: count must produce Int32 values");

		var result = new Array<Number>();
		for (Number index = 0; index < count; index++)
			result.Push(start + index);

		return result;
	}

	private static Array<TValue> RepeatCore<TValue>(TValue element, Number count)
	{
		if (count < 0)
			throw new Error("ArgumentOutOfRangeException: count must be non-negative");

		var result = new Array<TValue>();
		for (Number index = 0; index < count; index++)
			result.Push(element);

		return result;
	}

	private static Array<TKey> CreateOrderKeys<TKey>(Array<TSource> items, Func<TSource, TKey> keySelector)
	{
		var keys = new Array<TKey>();
		for (Number index = 0; index < items.Length; index++)
			keys.Push(keySelector(items[index]));

		return keys;
	}

	private static Number CompareWith<TKey>(
		System.Collections.Generic.IComparer<TKey>? comparer,
		TKey left,
		TKey right)
		=> comparer == null
			? ComparerT1Module<TKey>.CompareCore(left, right)
			: comparer.Compare(left, right);

	private static bool EqualsWith<TValue>(
		System.Collections.Generic.IEqualityComparer<TValue>? comparer,
		TValue left,
		TValue right)
		=> comparer == null
			? EqualityComparerT1Module<TValue>.EqualsCore(left, right)
			: comparer.Equals(left, right);

	private static Number HashWith<TValue>(
		System.Collections.Generic.IEqualityComparer<TValue>? comparer,
		TValue value)
		=> comparer == null
			? EqualityComparerT1Module<TValue>.GetHashCodeCore(value)
			: comparer.GetHashCode(value!);

	private static Func<Number, Number, Number> CreateKeyComparison<TKey>(
		Array<TKey> keys,
		bool descending,
		System.Collections.Generic.IComparer<TKey>? comparer)
		=> (left, right) =>
		{
			var comparison = CompareWith(comparer, keys[left], keys[right]);
			return descending ? -comparison : comparison;
		};

	private static Array<TSource> MaterializeOrderedResult(
		Array<TSource> items,
		Func<Number, Number, Number> comparison)
	{
		var order = new Array<Number>();
		for (Number index = 0; index < items.Length; index++)
			order.Push(index);

		order.Sort((left, right) =>
		{
			var result = comparison(left, right);
			return result != 0 ? result : left - right;
		});

		var result = new Array<TSource>();
		for (Number index = 0; index < order.Length; index++)
			result.Push(items[order[index]]);

		OrderedStates.Set(result, (items, comparison));
		return result;
	}

	private static Array<TSource> OrderByCore<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		bool descending,
		System.Collections.Generic.IComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");

		// LINQ OrderBy invokes the selector once per source item before sorting. Keep keys and
		// original offsets separate so the JS Array sort stays stable even if a host changes its
		// native sort implementation or key comparisons return zero.
		var items = Materialize(source);
		return MaterializeOrderedResult(items, CreateKeyComparison(CreateOrderKeys(items, keySelector), descending, comparer));
	}

	private static Array<TSource> OrderCore(IEnumerable<TSource> source, bool descending)
		=> OrderByCore(source, item => item, descending, comparer: null);

	private static Array<TSource> ThenByCore<TKey>(
		Array<TSource> source,
		Func<TSource, TKey> keySelector,
		bool descending,
		System.Collections.Generic.IComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");
		if (!OrderedStates.Has(source))
			throw new Error("NotSupportedException: ThenBy requires an ordering produced by Jazor's Enumerable.OrderBy runtime.");

		var state = OrderedStates.Get(source)!;
		var secondary = CreateKeyComparison(CreateOrderKeys(state.Items, keySelector), descending, comparer);
		Func<Number, Number, Number> comparison = (left, right) =>
		{
			var primary = state.Compare(left, right);
			return primary != 0 ? primary : secondary(left, right);
		};

		return MaterializeOrderedResult(state.Items, comparison);
	}

	private static Array<TSource> SkipCore(IEnumerable<TSource> source, Number count)
	{
		var items = Materialize(source);
		return count <= 0 ? items : items.Slice(count);
	}

	private static Array<TSource> TakeCore(IEnumerable<TSource> source, Number count)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (count <= 0)
			return new Array<TSource>();

		return Materialize(source).Slice(0, count);
	}

	private static Array<TSource> TakeRangeCore(IEnumerable<TSource> source, RuntimeModule.JRange range)
	{
		var items = Materialize(source);
		var layout = range.GetOffsetAndLength(items.Length);
		return items.Slice(layout.Offset, layout.Offset + layout.Length);
	}

	private static Array<TSource> SkipWhileCore(
		IEnumerable<TSource> source,
		Func<TSource, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		var skipping = true;
		var result = new Array<TSource>();
		foreach (var item in source)
		{
			if (skipping)
			{
				if (predicate(item))
					continue;

				// Enumerable.SkipWhile stops probing after the first false; later source items are
				// copied without another predicate call.
				skipping = false;
			}

			result.Push(item);
		}

		return result;
	}

	private static Array<TSource> SkipWhileAtCore(
		IEnumerable<TSource> source,
		Func<TSource, Number, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		var skipping = true;
		Number index = 0;
		var result = new Array<TSource>();
		foreach (var item in source)
		{
			if (skipping)
			{
				var shouldSkip = predicate(item, index);
				index++;
				if (shouldSkip)
					continue;

				skipping = false;
			}

			result.Push(item);
		}

		return result;
	}

	private static Array<TSource> TakeWhileCore(
		IEnumerable<TSource> source,
		Func<TSource, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		var result = new Array<TSource>();
		foreach (var item in source)
		{
			if (!predicate(item))
				break;

			result.Push(item);
		}

		return result;
	}

	private static Array<TSource> TakeWhileAtCore(
		IEnumerable<TSource> source,
		Func<TSource, Number, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		Number index = 0;
		var result = new Array<TSource>();
		foreach (var item in source)
		{
			var shouldTake = predicate(item, index);
			index++;
			if (!shouldTake)
				break;

			result.Push(item);
		}

		return result;
	}

	private static Array<TSource> SkipLastCore(IEnumerable<TSource> source, Number count)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (count <= 0)
			return Materialize(source);

		var result = new Array<TSource>();
		var tail = new Array<TSource>();
		Number tailIndex = 0;
		foreach (var item in source)
		{
			if (tail.Length < count)
			{
				tail.Push(item);
				continue;
			}

			result.Push(tail[tailIndex]);
			tail[tailIndex] = item;
			tailIndex = (tailIndex + 1) % count;
		}

		return result;
	}

	private static Array<TSource> TakeLastCore(IEnumerable<TSource> source, Number count)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (count <= 0)
			return new Array<TSource>();

		var tail = new Array<TSource>();
		Number tailIndex = 0;
		foreach (var item in source)
		{
			if (tail.Length < count)
			{
				tail.Push(item);
				continue;
			}

			tail[tailIndex] = item;
			tailIndex = (tailIndex + 1) % count;
		}

		if (tail.Length < count)
			return tail;

		var result = new Array<TSource>();
		for (Number offset = 0; offset < tail.Length; offset++)
		{
			result.Push(tail[tailIndex]);
			tailIndex++;
			if (tailIndex == tail.Length)
				tailIndex = 0;
		}

		return result;
	}

	private static Array<TSource> DefaultIfEmptyCore(IEnumerable<TSource> source, TSource defaultValue)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		// The CLR overload selects its fallback only after observing that the source is empty.
		// Materializing once preserves source order and prevents probing an IEnumerable twice.
		var result = Materialize(source);
		if (result.Length == 0)
			result.Push(defaultValue);

		return result;
	}

	private static Array<Array<TSource>> ChunkCore(IEnumerable<TSource> source, Number size)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (size < 1)
			throw new Error("ArgumentOutOfRangeException: size must be greater than zero");

		var result = new Array<Array<TSource>>();
		var chunk = new Array<TSource>();
		foreach (var item in source)
		{
			chunk.Push(item);
			if (chunk.Length != size)
				continue;

			result.Push(chunk);
			chunk = new Array<TSource>();
		}

		if (chunk.Length > 0)
			result.Push(chunk);

		return result;
	}

	private static Array<TSource> ReverseCore(Array<TSource> items)
	{
		var result = new Array<TSource>();
		for (Number index = items.Length; index > 0; index--)
			result.Push(items[index - 1]);

		return result;
	}

	private static Array<TSource> ConcatCore(IEnumerable<TSource> first, IEnumerable<TSource> second)
	{
		if (first == null)
			throw new Error("ArgumentNullException: first is null");
		if (second == null)
			throw new Error("ArgumentNullException: second is null");

		// Do not use Array.concat here: IEnumerable inputs must remain observable in first-then-
		// second enumeration order, including when a materialized carrier is supplied by a host.
		var result = new Array<TSource>();
		foreach (var item in first)
			result.Push(item);
		foreach (var item in second)
			result.Push(item);

		return result;
	}

	private static Array<TSource> AppendCore(IEnumerable<TSource> source, TSource element)
	{
		var result = Materialize(source);
		result.Push(element);
		return result;
	}

	private static Array<TSource> PrependCore(IEnumerable<TSource> source, TSource element)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var result = new Array<TSource>();
		result.Push(element);
		foreach (var item in source)
			result.Push(item);

		return result;
	}

	private static TSource ElementAtCore(IEnumerable<TSource> source, Number index)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (index < 0)
			throw new Error("ArgumentOutOfRangeException: index is less than zero");

		var currentIndex = 0;
		foreach (var item in source)
		{
			if (currentIndex == index)
				return item;
			currentIndex++;
		}

		throw new Error("ArgumentOutOfRangeException: index is out of range.");
	}

	private static TSource ElementAtIndexCore(IEnumerable<TSource> source, RuntimeModule.JIndex index)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var indexValue = index.Value;
		if (!index.IsFromEnd)
		{
			var currentIndex = 0;
			foreach (var item in source)
			{
				if (currentIndex == indexValue)
					return item;
				currentIndex++;
			}

			throw new Error("ArgumentOutOfRangeException: index is out of range.");
		}

		if (indexValue == 0)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");

		var tail = new Array<TSource>();
		var tailIndex = 0;
		foreach (var item in source)
		{
			if (tail.Length < indexValue)
			{
				tail.Push(item);
				continue;
			}

			tail[tailIndex] = item;
			tailIndex = (tailIndex + 1) % indexValue;
		}

		if (tail.Length < indexValue)
			throw new Error("ArgumentOutOfRangeException: index is out of range.");

		return tail[tailIndex];
	}

	private static TSource FirstOrDefaultCore(IEnumerable<TSource> source, TSource defaultValue)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		foreach (var item in source)
			return item;

		return defaultValue;
	}

	private static TSource FirstOrDefaultCore(
		IEnumerable<TSource> source,
		Func<TSource, bool> predicate,
		TSource defaultValue)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		foreach (var item in source)
		{
			if (predicate(item))
				return item;
		}

		return defaultValue;
	}

	private static TSource FirstCore(IEnumerable<TSource> source)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		foreach (var item in source)
			return item;

		throw new Error("InvalidOperationException: Sequence contains no elements");
	}

	private static TSource FirstCore(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		foreach (var item in source)
		{
			if (predicate(item))
				return item;
		}

		throw new Error("InvalidOperationException: Sequence contains no matching element");
	}

	private static TSource LastCore(IEnumerable<TSource> source)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var result = new Array<TSource>();
		foreach (var item in source)
			result[0] = item;

		if (result.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return result[0];
	}

	private static TSource LastCore(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		var result = new Array<TSource>();
		foreach (var item in source)
		{
			if (predicate(item))
				result[0] = item;
		}

		if (result.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no matching element");

		return result[0];
	}

	private static TSource LastOrDefaultCore(IEnumerable<TSource> source, TSource defaultValue)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var result = defaultValue;
		var found = false;
		foreach (var item in source)
		{
			result = item;
			found = true;
		}

		return found ? result : defaultValue;
	}

	private static TSource LastOrDefaultCore(
		IEnumerable<TSource> source,
		Func<TSource, bool> predicate,
		TSource defaultValue)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		var result = defaultValue;
		var found = false;
		foreach (var item in source)
		{
			if (!predicate(item))
				continue;

			result = item;
			found = true;
		}

		return found ? result : defaultValue;
	}

	private static TSource SingleCore(IEnumerable<TSource> source)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var result = new Array<TSource>();
		foreach (var item in source)
		{
			if (result.Length != 0)
				throw new Error("InvalidOperationException: Sequence contains more than one element");

			result[0] = item;
		}

		if (result.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return result[0];
	}

	private static TSource SingleCore(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		var result = new Array<TSource>();
		foreach (var item in source)
		{
			if (!predicate(item))
				continue;
			if (result.Length != 0)
				throw new Error("InvalidOperationException: Sequence contains more than one matching element");

			result[0] = item;
		}

		if (result.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no matching element");

		return result[0];
	}

	private static TSource SingleOrDefaultCore(IEnumerable<TSource> source, TSource defaultValue)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var result = defaultValue;
		var found = false;
		foreach (var item in source)
		{
			if (found)
				throw new Error("InvalidOperationException: Sequence contains more than one element");

			result = item;
			found = true;
		}

		return found ? result : defaultValue;
	}

	private static TSource SingleOrDefaultCore(
		IEnumerable<TSource> source,
		Func<TSource, bool> predicate,
		TSource defaultValue)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		var result = defaultValue;
		var found = false;
		foreach (var item in source)
		{
			if (!predicate(item))
				continue;
			if (found)
				throw new Error("InvalidOperationException: Sequence contains more than one matching element");

			result = item;
			found = true;
		}

		return found ? result : defaultValue;
	}

	private static bool SequenceEqualCore(
		Array<TSource> first,
		Array<TSource> second,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
	{
		if (first == null)
			throw new Error("ArgumentNullException: first is null");
		if (second == null)
			throw new Error("ArgumentNullException: second is null");

		// IEnumerable<T> is an Array carrier at this runtime boundary. Compare the same offsets
		// in source order so a mismatch short-circuits without materializing or mutating either input.
		if (first.Length != second.Length)
			return false;

		for (Number index = 0; index < first.Length; index++)
		{
			if (!EqualsWith(comparer, first[index], second[index]))
				return false;
		}

		return true;
	}

	private static TSource AggregateCore(IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (func == null)
			throw new Error("ArgumentNullException: func is null");

		var result = new Array<TSource>();
		foreach (var item in source)
		{
			if (result.Length == 0)
			{
				result[0] = item;
				continue;
			}

			result[0] = func(result[0], item);
		}

		if (result.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return result[0];
	}

	private static TAccumulate AggregateCore<TAccumulate>(
		IEnumerable<TSource> source,
		TAccumulate seed,
		Func<TAccumulate, TSource, TAccumulate> func)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (func == null)
			throw new Error("ArgumentNullException: func is null");

		var result = seed;
		foreach (var item in source)
			result = func(result, item);

		return result;
	}

	private static TResult AggregateCore<TAccumulate, TResult>(
		IEnumerable<TSource> source,
		TAccumulate seed,
		Func<TAccumulate, TSource, TAccumulate> func,
		Func<TAccumulate, TResult> resultSelector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (func == null)
			throw new Error("ArgumentNullException: func is null");
		if (resultSelector == null)
			throw new Error("ArgumentNullException: resultSelector is null");

		var result = seed;
		foreach (var item in source)
			result = func(result, item);

		return resultSelector(result);
	}

	private static Array<TResult> SelectManyCore<TResult>(
		IEnumerable<TSource> source,
		Func<TSource, IEnumerable<TResult>> collectionSelector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (collectionSelector == null)
			throw new Error("ArgumentNullException: collectionSelector is null");

		var result = new Array<TResult>();
		foreach (var sourceItem in source)
		{
			var collection = collectionSelector(sourceItem);
			AppendSelectedItems(result, collection);
		}

		return result;
	}

	private static Array<TResult> SelectManyAtCore<TResult>(
		IEnumerable<TSource> source,
		Func<TSource, Number, IEnumerable<TResult>> collectionSelector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (collectionSelector == null)
			throw new Error("ArgumentNullException: collectionSelector is null");

		var result = new Array<TResult>();
		Number index = 0;
		foreach (var sourceItem in source)
		{
			var collection = collectionSelector(sourceItem, index);
			index++;
			AppendSelectedItems(result, collection);
		}

		return result;
	}

	private static Array<TResult> SelectManyCore<TCollection, TResult>(
		IEnumerable<TSource> source,
		Func<TSource, IEnumerable<TCollection>> collectionSelector,
		Func<TSource, TCollection, TResult> resultSelector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (collectionSelector == null)
			throw new Error("ArgumentNullException: collectionSelector is null");
		if (resultSelector == null)
			throw new Error("ArgumentNullException: resultSelector is null");

		var result = new Array<TResult>();
		foreach (var sourceItem in source)
		{
			var collection = collectionSelector(sourceItem);
			if (collection == null)
				throw new Error("NullReferenceException: collection selector returned null");

			foreach (var collectionItem in collection)
				result.Push(resultSelector(sourceItem, collectionItem));
		}

		return result;
	}

	private static void AppendSelectedItems<TResult>(Array<TResult> result, IEnumerable<TResult> collection)
	{
		if (collection == null)
			throw new Error("NullReferenceException: collection selector returned null");

		foreach (var collectionItem in collection)
			result.Push(collectionItem);
	}

	private static bool AnyCore(IEnumerable<TSource> source)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		foreach (var _ in source)
			return true;

		return false;
	}

	private static bool AnyCore(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		foreach (var item in source)
		{
			if (predicate(item))
				return true;
		}

		return false;
	}

	private static bool AllCore(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		foreach (var item in source)
		{
			if (!predicate(item))
				return false;
		}

		return true;
	}

	private static Number CountCore(IEnumerable<TSource> source)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		Number count = 0;
		foreach (var _ in source)
		{
			if (count == 2147483647)
				throw new Error("OverflowException: Count exceeds Int32.MaxValue.");
			count++;
		}

		return count;
	}

	private static Number CountCore(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		Number count = 0;
		foreach (var item in source)
		{
			if (predicate(item))
			{
				if (count == 2147483647)
					throw new Error("OverflowException: Count exceeds Int32.MaxValue.");
				count++;
			}
		}

		return count;
	}

	private static Array<System.Collections.Generic.KeyValuePair<TKey, Number>> CountByCore<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");

		var groups = new Array<Array<Number>>();
		var groupsByHash = new Map<Number, Array<Array<Number>>>();
		foreach (var item in source)
		{
			var key = keySelector(item);
			var accumulator = GetGrouping(groupsByHash, groups, key, comparer);
			if (accumulator.Length == 0)
			{
				accumulator.Push(1);
				continue;
			}

			var count = accumulator[0];
			if (count == 2147483647)
				throw new Error("OverflowException: CountBy count exceeds Int32.MaxValue.");
			accumulator[0] = count + 1;
		}

		return MaterializeAccumulations<TKey, Number>(groups);
	}

	private static Array<System.Collections.Generic.KeyValuePair<TKey, TAccumulate>> AggregateByCore<TKey, TAccumulate>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		TAccumulate seed,
		Func<TAccumulate, TSource, TAccumulate> func,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");
		if (func == null)
			throw new Error("ArgumentNullException: func is null");

		var groups = new Array<Array<TAccumulate>>();
		var groupsByHash = new Map<Number, Array<Array<TAccumulate>>>();
		foreach (var item in source)
		{
			var key = keySelector(item);
			var accumulator = GetGrouping(groupsByHash, groups, key, comparer);
			if (accumulator.Length == 0)
			{
				accumulator.Push(func(seed, item));
				continue;
			}

			accumulator[0] = func(accumulator[0], item);
		}

		return MaterializeAccumulations<TKey, TAccumulate>(groups);
	}

	private static Array<System.Collections.Generic.KeyValuePair<TKey, TAccumulate>> AggregateByCore<TKey, TAccumulate>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TKey, TAccumulate> seedSelector,
		Func<TAccumulate, TSource, TAccumulate> func,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");
		if (seedSelector == null)
			throw new Error("ArgumentNullException: seedSelector is null");
		if (func == null)
			throw new Error("ArgumentNullException: func is null");

		var groups = new Array<Array<TAccumulate>>();
		var groupsByHash = new Map<Number, Array<Array<TAccumulate>>>();
		foreach (var item in source)
		{
			var key = keySelector(item);
			var accumulator = GetGrouping(groupsByHash, groups, key, comparer);
			if (accumulator.Length == 0)
			{
				accumulator.Push(func(seedSelector(key), item));
				continue;
			}

			accumulator[0] = func(accumulator[0], item);
		}

		return MaterializeAccumulations<TKey, TAccumulate>(groups);
	}

	private static BigInt LongCountCore(IEnumerable<TSource> source)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var maximum = BigIntFn("9223372036854775807");
		var count = BigInt.Zero;
		foreach (var _ in source)
		{
			if (count == maximum)
				throw new Error("OverflowException: LongCount exceeds Int64.MaxValue.");
			count = count + BigInt.One;
		}

		return count;
	}

	private static BigInt LongCountCore(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		var maximum = BigIntFn("9223372036854775807");
		var count = BigInt.Zero;
		foreach (var item in source)
		{
			if (!predicate(item))
				continue;
			if (count == maximum)
				throw new Error("OverflowException: LongCount exceeds Int64.MaxValue.");
			count = count + BigInt.One;
		}

		return count;
	}

	private static Number SumIntCore<TValue>(IEnumerable<TValue> source, Func<TValue, Number> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		Number sum = 0;
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			EnsureInt32AdditionInRange(sum, item, "Sum");
			sum += item;
		}

		return sum;
	}

	private static void EnsureInt32AdditionInRange(Number total, Number value, string operation)
	{
		if ((value > 0 && total > 2147483647 - value) ||
			(value < 0 && total < -2147483648 - value))
		{
			throw new Error($"OverflowException: {operation} exceeds Int32 bounds.");
		}
	}

	private static BigInt SumInt64Core<TValue>(IEnumerable<TValue> source, Func<TValue, BigInt> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var sum = BigInt.Zero;
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			EnsureInt64AdditionInRange(sum, item, "Sum");
			sum += item;
		}

		return sum;
	}

	private static void EnsureInt64AdditionInRange(BigInt total, BigInt value, string operation)
	{
		var maximum = BigIntFn("9223372036854775807");
		var minimum = BigIntFn("-9223372036854775808");
		if ((value > BigInt.Zero && total > maximum - value) ||
			(value < BigInt.Zero && total < minimum - value))
		{
			throw new Error($"OverflowException: {operation} exceeds Int64 bounds.");
		}
	}

	private static Number SumNumberCore<TValue>(
		IEnumerable<TValue> source,
		Func<TValue, Number> selector,
		bool singlePrecision)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		Number sum = 0;
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			sum += item;
		}

		// Enumerable.Sum(float) accumulates at wider precision and converts once on return.
		return singlePrecision ? Math.FroundFn(sum) : sum;
	}

	private static decimal SumDecimalCore<TValue>(IEnumerable<TValue> source, Func<TValue, decimal> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		// decimal uses the string-backed carrier; seed through its mapped factory, never Number zero.
		decimal sum = decimal.Parse("0");
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			sum = decimal.Add(sum, item);
		}

		return sum;
	}

	private static Number AverageIntCore<TValue>(IEnumerable<TValue> source, Func<TValue, Number> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var sum = BigInt.Zero;
		var count = BigInt.Zero;
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			var value = BigIntFn(item);
			EnsureInt64AdditionInRange(sum, value, "Average");
			EnsureInt64AdditionInRange(count, BigInt.One, "Average count");
			sum += value;
			count += BigInt.One;
		}

		if (count == BigInt.Zero)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return NumberFn(sum) / NumberFn(count);
	}

	private static Number AverageInt64Core<TValue>(IEnumerable<TValue> source, Func<TValue, BigInt> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var sum = BigInt.Zero;
		var count = BigInt.Zero;
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			EnsureInt64AdditionInRange(sum, item, "Average");
			EnsureInt64AdditionInRange(count, BigInt.One, "Average count");
			sum += item;
			count += BigInt.One;
		}

		if (count == BigInt.Zero)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return NumberFn(sum) / NumberFn(count);
	}

	private static Number AverageNumberCore<TValue>(
		IEnumerable<TValue> source,
		Func<TValue, Number> selector,
		bool singlePrecision)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		Number sum = 0;
		var count = BigInt.Zero;
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			EnsureInt64AdditionInRange(count, BigInt.One, "Average count");
			sum += item;
			count += BigInt.One;
		}

		if (count == BigInt.Zero)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		var average = sum / NumberFn(count);
		return singlePrecision ? Math.FroundFn(average) : average;
	}

	private static decimal AverageDecimalCore<TValue>(IEnumerable<TValue> source, Func<TValue, decimal> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		decimal sum = decimal.Parse("0");
		var count = BigInt.Zero;
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			EnsureInt64AdditionInRange(count, BigInt.One, "Average count");
			sum = decimal.Add(sum, item);
			count += BigInt.One;
		}

		if (count == BigInt.Zero)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return decimal.Divide(sum, decimal.Parse(count.ToString()));
	}

	private static Number SumNullableIntCore<TValue>(IEnumerable<TValue> source, Func<TValue, Number?> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		Number sum = 0;
		foreach (var sourceItem in source)
		{
			var selected = selector(sourceItem);
			if (!selected.HasValue)
				continue;

			var item = selected.Value;
			EnsureInt32AdditionInRange(sum, item, "Sum");
			sum += item;
		}

		return sum;
	}

	private static BigInt SumNullableInt64Core<TValue>(IEnumerable<TValue> source, Func<TValue, BigInt?> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var sum = BigInt.Zero;
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			if (Object.ReferenceEquals(item, null))
				continue;

			EnsureInt64AdditionInRange(sum, item, "Sum");
			sum += item;
		}

		return sum;
	}

	private static Number SumNullableNumberCore<TValue>(IEnumerable<TValue> source, Func<TValue, Number?> selector, bool singlePrecision)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		Number sum = 0;
		foreach (var sourceItem in source)
		{
			var selected = selector(sourceItem);
			if (selected.HasValue)
				sum += selected.Value;
		}

		return singlePrecision ? Math.FroundFn(sum) : sum;
	}

	private static decimal SumNullableDecimalCore<TValue>(IEnumerable<TValue> source, Func<TValue, decimal?> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		decimal sum = decimal.Parse("0");
		foreach (var sourceItem in source)
		{
			var selected = selector(sourceItem);
			if (selected.HasValue)
				sum = decimal.Add(sum, selected.Value);
		}

		return sum;
	}

	private static Number? AverageNullableIntCore<TValue>(IEnumerable<TValue> source, Func<TValue, Number?> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var sum = BigInt.Zero;
		var count = BigInt.Zero;
		foreach (var sourceItem in source)
		{
			var selected = selector(sourceItem);
			if (!selected.HasValue)
				continue;

			var item = BigIntFn(selected.Value);
			EnsureInt64AdditionInRange(sum, item, "Average");
			EnsureInt64AdditionInRange(count, BigInt.One, "Average count");
			sum += item;
			count += BigInt.One;
		}

		return count == BigInt.Zero ? null : NumberFn(sum) / NumberFn(count);
	}

	private static Number? AverageNullableInt64Core<TValue>(IEnumerable<TValue> source, Func<TValue, BigInt?> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var sum = BigInt.Zero;
		var count = BigInt.Zero;
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			if (Object.ReferenceEquals(item, null))
				continue;

			EnsureInt64AdditionInRange(sum, item, "Average");
			EnsureInt64AdditionInRange(count, BigInt.One, "Average count");
			sum += item;
			count += BigInt.One;
		}

		return count == BigInt.Zero ? null : NumberFn(sum) / NumberFn(count);
	}

	private static Number? AverageNullableNumberCore<TValue>(IEnumerable<TValue> source, Func<TValue, Number?> selector, bool singlePrecision)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		Number sum = 0;
		var count = BigInt.Zero;
		foreach (var sourceItem in source)
		{
			var selected = selector(sourceItem);
			if (!selected.HasValue)
				continue;

			sum += selected.Value;
			EnsureInt64AdditionInRange(count, BigInt.One, "Average count");
			count += BigInt.One;
		}

		if (count == BigInt.Zero)
			return null;

		var average = sum / NumberFn(count);
		return singlePrecision ? Math.FroundFn(average) : average;
	}

	private static decimal? AverageNullableDecimalCore<TValue>(IEnumerable<TValue> source, Func<TValue, decimal?> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		decimal sum = decimal.Parse("0");
		var count = BigInt.Zero;
		foreach (var sourceItem in source)
		{
			var selected = selector(sourceItem);
			if (!selected.HasValue)
				continue;

			sum = decimal.Add(sum, selected.Value);
			EnsureInt64AdditionInRange(count, BigInt.One, "Average count");
			count += BigInt.One;
		}

		return count == BigInt.Zero
			? null
			: decimal.Divide(sum, decimal.Parse(count.ToString()));
	}

	private static Array<(Number Index, TSource Item)> IndexCore(IEnumerable<TSource> source)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var result = new Array<(Number Index, TSource Item)>();
		Number index = 0;
		foreach (var item in source)
		{
			if (index == 2147483647)
				throw new Error("OverflowException: Index exceeds Int32.MaxValue.");
			result.Push((index, item));
			index++;
		}

		return result;
	}

	private static Number MinNumberCore<TValue>(IEnumerable<TValue> source, Func<TValue, Number> selector, bool floating)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<Number>();
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			if (candidates.Length == 0)
			{
				candidates.Push(item);
				continue;
			}
			if (floating && IsNaN(item))
				return item;
			if (item < candidates[0])
				candidates[0] = item;
		}

		if (candidates.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return candidates[0];
	}

	private static Number MaxNumberCore<TValue>(IEnumerable<TValue> source, Func<TValue, Number> selector, bool floating)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<Number>();
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			if (candidates.Length == 0)
			{
				candidates.Push(item);
				continue;
			}
			if (floating && IsNaN(candidates[0]))
			{
				if (!IsNaN(item))
					candidates[0] = item;
				continue;
			}
			if (item > candidates[0])
				candidates[0] = item;
		}

		if (candidates.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return candidates[0];
	}

	private static BigInt MinBigIntCore<TValue>(IEnumerable<TValue> source, Func<TValue, BigInt> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<BigInt>();
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			if (candidates.Length == 0)
			{
				candidates.Push(item);
				continue;
			}
			if (item < candidates[0])
				candidates[0] = item;
		}

		if (candidates.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return candidates[0];
	}

	private static BigInt MaxBigIntCore<TValue>(IEnumerable<TValue> source, Func<TValue, BigInt> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<BigInt>();
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			if (candidates.Length == 0)
			{
				candidates.Push(item);
				continue;
			}
			if (item > candidates[0])
				candidates[0] = item;
		}

		if (candidates.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return candidates[0];
	}

	private static decimal MinDecimalCore<TValue>(IEnumerable<TValue> source, Func<TValue, decimal> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<decimal>();
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			if (candidates.Length == 0)
			{
				candidates.Push(item);
				continue;
			}
			if (decimal.Compare(item, candidates[0]) < 0)
				candidates[0] = item;
		}

		if (candidates.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return candidates[0];
	}

	private static decimal MaxDecimalCore<TValue>(IEnumerable<TValue> source, Func<TValue, decimal> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<decimal>();
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			if (candidates.Length == 0)
			{
				candidates.Push(item);
				continue;
			}
			if (decimal.Compare(item, candidates[0]) > 0)
				candidates[0] = item;
		}

		if (candidates.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return candidates[0];
	}

	private static Number? MinNullableNumberCore<TValue>(IEnumerable<TValue> source, Func<TValue, Number?> selector, bool floating)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<Number>();
		foreach (var sourceItem in source)
		{
			var selected = selector(sourceItem);
			if (!selected.HasValue)
				continue;

			var item = selected.Value;
			if (candidates.Length == 0)
			{
				candidates.Push(item);
				continue;
			}
			if (floating && IsNaN(item))
				return item;
			if (item < candidates[0])
				candidates[0] = item;
		}

		return candidates.Length == 0 ? null : candidates[0];
	}

	private static Number? MaxNullableNumberCore<TValue>(IEnumerable<TValue> source, Func<TValue, Number?> selector, bool floating)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<Number>();
		foreach (var sourceItem in source)
		{
			var selected = selector(sourceItem);
			if (!selected.HasValue)
				continue;

			var item = selected.Value;
			if (candidates.Length == 0)
			{
				candidates.Push(item);
				continue;
			}
			if (floating && IsNaN(candidates[0]))
			{
				if (!IsNaN(item))
					candidates[0] = item;
				continue;
			}
			if (item > candidates[0])
				candidates[0] = item;
		}

		return candidates.Length == 0 ? null : candidates[0];
	}

	private static BigInt? MinNullableBigIntCore<TValue>(IEnumerable<TValue> source, Func<TValue, BigInt?> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<BigInt>();
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			if (Object.ReferenceEquals(item, null))
				continue;

			if (candidates.Length == 0)
			{
				candidates.Push(item);
				continue;
			}
			if (item < candidates[0])
				candidates[0] = item;
		}

		return candidates.Length == 0 ? null : candidates[0];
	}

	private static BigInt? MaxNullableBigIntCore<TValue>(IEnumerable<TValue> source, Func<TValue, BigInt?> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<BigInt>();
		foreach (var sourceItem in source)
		{
			var item = selector(sourceItem);
			if (Object.ReferenceEquals(item, null))
				continue;

			if (candidates.Length == 0)
			{
				candidates.Push(item);
				continue;
			}
			if (item > candidates[0])
				candidates[0] = item;
		}

		return candidates.Length == 0 ? null : candidates[0];
	}

	private static decimal? MinNullableDecimalCore<TValue>(IEnumerable<TValue> source, Func<TValue, decimal?> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<decimal>();
		foreach (var sourceItem in source)
		{
			var selected = selector(sourceItem);
			if (!selected.HasValue)
				continue;

			var item = selected.Value;
			if (candidates.Length == 0 || decimal.Compare(item, candidates[0]) < 0)
				candidates[0] = item;
		}

		return candidates.Length == 0 ? null : candidates[0];
	}

	private static decimal? MaxNullableDecimalCore<TValue>(IEnumerable<TValue> source, Func<TValue, decimal?> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		var candidates = new Array<decimal>();
		foreach (var sourceItem in source)
		{
			var selected = selector(sourceItem);
			if (!selected.HasValue)
				continue;

			var item = selected.Value;
			if (candidates.Length == 0 || decimal.Compare(item, candidates[0]) > 0)
				candidates[0] = item;
		}

		return candidates.Length == 0 ? null : candidates[0];
	}

	private static bool ContainsByEquality<TValue>(
		Map<Number, Array<TValue>> valuesByHash,
		TValue value,
		System.Collections.Generic.IEqualityComparer<TValue>? comparer)
	{
		var hashCode = HashWith(comparer, value);
		if (!valuesByHash.Has(hashCode))
			return false;

		var bucket = valuesByHash.Get(hashCode);
		if (bucket == null)
			return false;

		// The hash only identifies a bucket. EqualityComparer is still authoritative so this
		// stays aligned with CLR equality for collisions, NaN and signed zero.
		for (Number index = 0; index < bucket.Length; index++)
		{
			if (EqualsWith(comparer, bucket[index], value))
				return true;
		}

		return false;
	}

	private static bool AddByEquality<TValue>(
		Map<Number, Array<TValue>> valuesByHash,
		TValue value,
		System.Collections.Generic.IEqualityComparer<TValue>? comparer)
	{
		if (ContainsByEquality(valuesByHash, value, comparer))
			return false;

		var hashCode = HashWith(comparer, value);
		var bucket = new Array<TValue>();
		if (valuesByHash.Has(hashCode))
		{
			var existingBucket = valuesByHash.Get(hashCode);
			if (existingBucket != null)
				bucket = existingBucket;
		}
		else
		{
			valuesByHash.Set(hashCode, bucket);
		}

		bucket.Push(value);
		return true;
	}

	private static bool RemoveByEquality<TValue>(
		Map<Number, Array<TValue>> valuesByHash,
		TValue value,
		System.Collections.Generic.IEqualityComparer<TValue>? comparer)
	{
		var hashCode = HashWith(comparer, value);
		if (!valuesByHash.Has(hashCode))
			return false;

		var bucket = valuesByHash.Get(hashCode);
		if (bucket == null)
			return false;

		for (Number index = 0; index < bucket.Length; index++)
		{
			if (!EqualsWith(comparer, bucket[index], value))
				continue;

			bucket.Splice(index, 1);
			return true;
		}

		return false;
	}

	private static Map<Number, Array<TValue>> CreateEqualitySet<TValue>(
		IEnumerable<TValue> source,
		System.Collections.Generic.IEqualityComparer<TValue>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var valuesByHash = new Map<Number, Array<TValue>>();
		foreach (var value in source)
			AddByEquality(valuesByHash, value, comparer);

		return valuesByHash;
	}

	private static Array<TSource> DistinctCore(
		IEnumerable<TSource> source,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var seen = new Map<Number, Array<TSource>>();
		var result = new Array<TSource>();
		foreach (var item in source)
		{
			if (AddByEquality(seen, item, comparer))
				result.Push(item);
		}

		return result;
	}

	private static Array<TSource> DistinctByCore<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");

		var seenKeys = new Map<Number, Array<TKey>>();
		var result = new Array<TSource>();
		foreach (var item in source)
		{
			// The selector is evaluated once for every observed source item, including duplicates.
			if (AddByEquality(seenKeys, keySelector(item), comparer))
				result.Push(item);
		}

		return result;
	}

	private static TSource ExtremumByCore<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		bool maximum,
		System.Collections.Generic.IComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");

		// Keep a typed candidate carrier rather than initializing TSource/TKey with default(T).
		// This preserves selector/enumeration order and keeps the empty-sequence failure explicit.
		var candidates = new Array<TSource>();
		var candidateKeys = new Array<TKey>();
		foreach (var item in source)
		{
			var key = keySelector(item);
			if (candidates.Length == 0)
			{
				candidates.Push(item);
				candidateKeys.Push(key);
				continue;
			}

			var comparison = CompareWith(comparer, key, candidateKeys[0]);
			if (maximum ? comparison > 0 : comparison < 0)
			{
				candidates[0] = item;
				candidateKeys[0] = key;
			}
		}

		if (candidates.Length == 0)
			throw new Error("InvalidOperationException: Sequence contains no elements");

		return candidates[0];
	}

	private static Array<TSource> UnionCore(
		IEnumerable<TSource> first,
		IEnumerable<TSource> second,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
	{
		if (first == null)
			throw new Error("ArgumentNullException: first is null");
		if (second == null)
			throw new Error("ArgumentNullException: second is null");

		var seen = new Map<Number, Array<TSource>>();
		var result = new Array<TSource>();
		foreach (var item in first)
		{
			if (AddByEquality(seen, item, comparer))
				result.Push(item);
		}
		foreach (var item in second)
		{
			if (AddByEquality(seen, item, comparer))
				result.Push(item);
		}

		return result;
	}

	private static Array<TSource> ExceptCore(
		IEnumerable<TSource> first,
		IEnumerable<TSource> second,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
	{
		if (first == null)
			throw new Error("ArgumentNullException: first is null");

		// Enumerable.Except builds the exclusion set before it starts yielding first, then adds
		// yielded elements to that same set to preserve first-sequence order without duplicates.
		var excluded = CreateEqualitySet(second, comparer);
		var result = new Array<TSource>();
		foreach (var item in first)
		{
			if (AddByEquality(excluded, item, comparer))
				result.Push(item);
		}

		return result;
	}

	private static Array<TSource> IntersectCore(
		IEnumerable<TSource> first,
		IEnumerable<TSource> second,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
	{
		if (first == null)
			throw new Error("ArgumentNullException: first is null");

		// Removing a matched second-set member makes each result distinct while retaining first's
		// traversal order. It also avoids any second selector/enumeration after first begins.
		var remaining = CreateEqualitySet(second, comparer);
		var result = new Array<TSource>();
		foreach (var item in first)
		{
			if (RemoveByEquality(remaining, item, comparer))
				result.Push(item);
		}

		return result;
	}

	private static Array<TSource> UnionByCore<TKey>(
		IEnumerable<TSource> first,
		IEnumerable<TSource> second,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (first == null)
			throw new Error("ArgumentNullException: first is null");
		if (second == null)
			throw new Error("ArgumentNullException: second is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");

		var seenKeys = new Map<Number, Array<TKey>>();
		var result = new Array<TSource>();
		foreach (var item in first)
		{
			if (AddByEquality(seenKeys, keySelector(item), comparer))
				result.Push(item);
		}
		foreach (var item in second)
		{
			if (AddByEquality(seenKeys, keySelector(item), comparer))
				result.Push(item);
		}

		return result;
	}

	private static Array<TSource> ExceptByCore<TKey>(
		IEnumerable<TSource> first,
		IEnumerable<TKey> second,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (first == null)
			throw new Error("ArgumentNullException: first is null");
		if (second == null)
			throw new Error("ArgumentNullException: second is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");

		// ExceptBy observes every second key before it starts first. Reusing that key set for
		// source members filters exclusions and makes the result distinct by key in one pass.
		var excludedKeys = CreateEqualitySet(second, comparer);
		var result = new Array<TSource>();
		foreach (var item in first)
		{
			if (AddByEquality(excludedKeys, keySelector(item), comparer))
				result.Push(item);
		}

		return result;
	}

	private static Array<TSource> IntersectByCore<TKey>(
		IEnumerable<TSource> first,
		IEnumerable<TKey> second,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (first == null)
			throw new Error("ArgumentNullException: first is null");
		if (second == null)
			throw new Error("ArgumentNullException: second is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");

		// Removing a matched key retains first traversal order while ensuring each selected key
		// contributes at most one source element, even when either input contains duplicates.
		var remainingKeys = CreateEqualitySet(second, comparer);
		var result = new Array<TSource>();
		foreach (var item in first)
		{
			if (RemoveByEquality(remainingKeys, keySelector(item), comparer))
				result.Push(item);
		}

		return result;
	}

	private static bool ContainsCore(
		IEnumerable<TSource> source,
		TSource value,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		foreach (var item in source)
		{
			if (EqualsWith(comparer, item, value))
				return true;
		}

		return false;
	}

	private static Array<TElement>? FindGrouping<TKey, TElement>(
		Map<Number, Array<Array<TElement>>> groupsByHash,
		TKey key,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		var hashCode = HashWith(comparer, key);
		if (!groupsByHash.Has(hashCode))
			return null;

		var bucket = groupsByHash.Get(hashCode);
		if (bucket == null)
			return null;

		// Hash codes only narrow the candidate set. EqualityComparer remains the source of truth,
		// so collisions, NaN and signed-zero follow the same CLR equality contract everywhere.
		for (Number index = 0; index < bucket.Length; index++)
		{
			var grouping = bucket[index];
			if (EqualsWith(comparer, GroupingT2Module<TKey, TElement>.GetKey(grouping)!, key))
				return grouping;
		}

		return null;
	}

	private static Array<TElement>? FindLookupGrouping<TKey, TElement>(
		Array<Array<TElement>> groups,
		TKey key,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		for (Number index = 0; index < groups.Length; index++)
		{
			var group = groups[index];
			if (EqualsWith(comparer, GroupingT2Module<TKey, TElement>.GetKey(group)!, key))
				return group;
		}

		return null;
	}

	private static Array<TElement> GetGrouping<TKey, TElement>(
		Map<Number, Array<Array<TElement>>> groupsByHash,
		Array<Array<TElement>> groups,
		TKey key,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		var existing = FindGrouping(groupsByHash, key, comparer);
		if (existing != null)
			return existing;

		var hashCode = HashWith(comparer, key);
		var bucket = new Array<Array<TElement>>();
		if (groupsByHash.Has(hashCode))
		{
			var existingBucket = groupsByHash.Get(hashCode);
			if (existingBucket != null)
				bucket = existingBucket;
		}
		else
		{
			groupsByHash.Set(hashCode, bucket);
		}

		var created = GroupingT2Module<TKey, TElement>.Create(key);
		bucket.Push(created);
		groups.Push(created);
		return created;
	}

	private static Array<System.Collections.Generic.KeyValuePair<TKey, TValue>> MaterializeAccumulations<TKey, TValue>(
		Array<Array<TValue>> groups)
	{
		var result = new Array<System.Collections.Generic.KeyValuePair<TKey, TValue>>();
		for (Number index = 0; index < groups.Length; index++)
		{
			var group = groups[index];
			result.Push(new System.Collections.Generic.KeyValuePair<TKey, TValue>(
				GroupingT2Module<TKey, TValue>.GetKey(group)!,
				group[0]));
		}

		return result;
	}

	private static Array<Array<TSource>> GroupByCore<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> GroupByCore(source, keySelector, item => item, comparer);

	private static Array<TResult> JoinCore<TInner, TKey, TResult>(
		IEnumerable<TSource> outer,
		IEnumerable<TInner> inner,
		Func<TSource, TKey> outerKeySelector,
		Func<TInner, TKey> innerKeySelector,
		Func<TSource, TInner, TResult> resultSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (outer == null)
			throw new Error("ArgumentNullException: outer is null");
		if (inner == null)
			throw new Error("ArgumentNullException: inner is null");
		if (outerKeySelector == null)
			throw new Error("ArgumentNullException: outerKeySelector is null");
		if (innerKeySelector == null)
			throw new Error("ArgumentNullException: innerKeySelector is null");
		if (resultSelector == null)
			throw new Error("ArgumentNullException: resultSelector is null");

		var groups = new Array<Array<TInner>>();
		var groupsByHash = new Map<Number, Array<Array<TInner>>>();
		foreach (var innerItem in inner)
		{
			var key = innerKeySelector(innerItem);
			GetGrouping(groupsByHash, groups, key, comparer).Push(innerItem);
		}

		var result = new Array<TResult>();
		foreach (var outerItem in outer)
		{
			var grouping = FindGrouping(groupsByHash, outerKeySelector(outerItem), comparer);
			if (grouping == null)
				continue;

			for (Number index = 0; index < grouping.Length; index++)
				result.Push(resultSelector(outerItem, grouping[index]));
		}

		return result;
	}

	private static Array<TResult> GroupJoinCore<TInner, TKey, TResult>(
		IEnumerable<TSource> outer,
		IEnumerable<TInner> inner,
		Func<TSource, TKey> outerKeySelector,
		Func<TInner, TKey> innerKeySelector,
		Func<TSource, Array<TInner>, TResult> resultSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (outer == null)
			throw new Error("ArgumentNullException: outer is null");
		if (inner == null)
			throw new Error("ArgumentNullException: inner is null");
		if (outerKeySelector == null)
			throw new Error("ArgumentNullException: outerKeySelector is null");
		if (innerKeySelector == null)
			throw new Error("ArgumentNullException: innerKeySelector is null");
		if (resultSelector == null)
			throw new Error("ArgumentNullException: resultSelector is null");

		var groups = new Array<Array<TInner>>();
		var groupsByHash = new Map<Number, Array<Array<TInner>>>();
		foreach (var innerItem in inner)
		{
			var key = innerKeySelector(innerItem);
			GetGrouping(groupsByHash, groups, key, comparer).Push(innerItem);
		}

		var result = new Array<TResult>();
		foreach (var outerItem in outer)
		{
			var grouping = FindGrouping(groupsByHash, outerKeySelector(outerItem), comparer);
			result.Push(resultSelector(outerItem, grouping ?? new Array<TInner>()));
		}

		return result;
	}

	private static Array<Array<TElement>> GroupByCore<TKey, TElement>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");
		if (elementSelector == null)
			throw new Error("ArgumentNullException: elementSelector is null");

		return GroupByCoreUnchecked(source, keySelector, elementSelector, comparer);
	}

	private static Array<TResult> GroupByResultCore<TKey, TElement, TResult>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector,
		Func<TKey, Array<TElement>, TResult> resultSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");
		if (elementSelector == null)
			throw new Error("ArgumentNullException: elementSelector is null");
		if (resultSelector == null)
			throw new Error("ArgumentNullException: resultSelector is null");

		var groups = GroupByCoreUnchecked(source, keySelector, elementSelector, comparer);
		var result = new Array<TResult>();
		// The result selector observes fully materialized groups, after all source selectors run.
		for (Number index = 0; index < groups.Length; index++)
		{
			var group = groups[index];
			result.Push(resultSelector(GroupingT2Module<TKey, TElement>.GetKey(group)!, group));
		}

		return result;
	}

	private static Array<Array<TElement>> GroupByCoreUnchecked<TKey, TElement>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		var groups = new Array<Array<TElement>>();
		var groupsByHash = new Map<Number, Array<Array<TElement>>>();
		foreach (var sourceItem in source)
		{
			var key = keySelector(sourceItem);
			var element = elementSelector(sourceItem);
			GetGrouping(groupsByHash, groups, key, comparer).Push(element);
		}

		return groups;
	}

	private static Array<Array<TElement>> ToLookupCore<TKey, TElement>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		var groups = GroupByCore(source, keySelector, elementSelector, comparer);
		LookupComparers.Set(groups, comparer);
		return groups;
	}

	private static Set<TSource> ToHashSetCore(
		IEnumerable<TSource> source,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
		=> HashSetT1Module<TSource>.CreateFrom(source, comparer);

	private static Map<TKey, TElement> ToDictionaryCore<TKey, TElement>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (keySelector == null)
			throw new Error("ArgumentNullException: keySelector is null");
		if (elementSelector == null)
			throw new Error("ArgumentNullException: elementSelector is null");

		var result = DictionaryT2Module<TKey, TElement>.Create(comparer);
		foreach (var item in source)
		{
			var key = keySelector(item);
			if (result.Has(key))
				throw new Error("ArgumentException: An item with the same key has already been added.");
			result.Set(key, elementSelector(item));
		}

		return result;
	}

	[Jazor(Op.Compile, "static System.Linq.Enumerable.Where<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", "EnumerableArrayLike")]
	public static Array<TSource> _a0d3305d7a8d4c01(IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		return Materialize(source).Filter(item => predicate(item));
	}

	[Jazor(Op.Compile, "static System.Linq.Enumerable.Where<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)", "EnumerableArrayLike")]
	public static Array<TSource> _0f6f6fe4a8e94447(IEnumerable<TSource> source, Func<TSource, Number, bool> predicate)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (predicate == null)
			throw new Error("ArgumentNullException: predicate is null");

		return Materialize(source).Filter((item, index) => predicate(item, index));
	}

	[Jazor(Op.Import, "static System.Linq.Enumerable.Empty<TResult>()", "empty")]
	public static Array<TSource> Empty()
		=> new Array<TSource>();

	[Jazor(Op.Import, "static System.Linq.Enumerable.Range(int, int)", "range")]
	public static Array<Number> Range(Number start, Number count)
		=> RangeCore(start, count);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Repeat<TResult>(TResult, int)", "repeat")]
	public static Array<TSource> Repeat(TSource element, Number count)
		=> RepeatCore(element, count);

	[Jazor(Op.Import, "static System.Linq.Enumerable.AsEnumerable<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "asEnumerable")]
	public static IEnumerable<TSource> AsEnumerable(IEnumerable<TSource> source)
		=> source;

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sequence<T>(T, T, T)", "sequence")]
	public static Array<TSource> Sequence(TSource first, TSource second, TSource third)
		=> [first, second, third];

	[Jazor(Op.Compile, "static System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TResult>)", "EnumerableArrayLike")]
	public static Array<TResult> _0d5df18d09084f3b<TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		return Materialize(source).Map(selector);
	}

	[Jazor(Op.Compile, "static System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, TResult>)", "EnumerableArrayLike")]
	public static Array<TResult> _aab4dc2444d44402<TResult>(IEnumerable<TSource> source, Func<TSource, Number, TResult> selector)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (selector == null)
			throw new Error("ArgumentNullException: selector is null");

		return Materialize(source).Map(selector);
	}

	[Jazor(Op.Import, "static System.Linq.Enumerable.SelectMany<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, System.Collections.Generic.IEnumerable<TResult>>)")]
	public static Array<TResult> _edce1ee9a9c5c4cc<TResult>(
		IEnumerable<TSource> source,
		Func<TSource, IEnumerable<TResult>> collectionSelector)
		=> SelectManyCore(source, collectionSelector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.SelectMany<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, System.Collections.Generic.IEnumerable<TResult>>)")]
	public static Array<TResult> _de31ec2f4619ef07<TResult>(
		IEnumerable<TSource> source,
		Func<TSource, Number, IEnumerable<TResult>> collectionSelector)
		=> SelectManyAtCore(source, collectionSelector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.SelectMany<TSource, TCollection, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, System.Collections.Generic.IEnumerable<TCollection>>, System.Func<TSource, TCollection, TResult>)")]
	public static Array<TResult> _aacc82f5a0d854d2<TCollection, TResult>(
		IEnumerable<TSource> source,
		Func<TSource, IEnumerable<TCollection>> collectionSelector,
		Func<TSource, TCollection, TResult> resultSelector)
		=> SelectManyCore(source, collectionSelector, resultSelector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Skip<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)")]
	public static Array<TSource> _7a0726e65cb5b3a2(IEnumerable<TSource> source, Number count)
		=> SkipCore(source, count);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Take<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)")]
	public static Array<TSource> _4abc4f56a4100834(IEnumerable<TSource> source, Number count)
		=> TakeCore(source, count);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Take<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Range)", "takeRange")]
	public static Array<TSource> TakeRange(IEnumerable<TSource> source, RuntimeModule.JRange range)
		=> TakeRangeCore(source, range);

	[Jazor(Op.Import, "static System.Linq.Enumerable.SkipWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", "skipWhile")]
	public static Array<TSource> SkipWhile(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> SkipWhileCore(source, predicate);

	[Jazor(Op.Import, "static System.Linq.Enumerable.SkipWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)", "skipWhileAt")]
	public static Array<TSource> SkipWhileAt(IEnumerable<TSource> source, Func<TSource, Number, bool> predicate)
		=> SkipWhileAtCore(source, predicate);

	[Jazor(Op.Import, "static System.Linq.Enumerable.TakeWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", "takeWhile")]
	public static Array<TSource> TakeWhile(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> TakeWhileCore(source, predicate);

	[Jazor(Op.Import, "static System.Linq.Enumerable.TakeWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)", "takeWhileAt")]
	public static Array<TSource> TakeWhileAt(IEnumerable<TSource> source, Func<TSource, Number, bool> predicate)
		=> TakeWhileAtCore(source, predicate);

	[Jazor(Op.Import, "static System.Linq.Enumerable.SkipLast<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", "skipLast")]
	public static Array<TSource> SkipLast(IEnumerable<TSource> source, Number count)
		=> SkipLastCore(source, count);

	[Jazor(Op.Import, "static System.Linq.Enumerable.TakeLast<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", "takeLast")]
	public static Array<TSource> TakeLast(IEnumerable<TSource> source, Number count)
		=> TakeLastCore(source, count);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Chunk<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", "chunk")]
	public static Array<Array<TSource>> Chunk(IEnumerable<TSource> source, Number size)
		=> ChunkCore(source, size);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Reverse<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "reverse")]
	public static Array<TSource> Reverse(IEnumerable<TSource> source)
		=> ReverseCore(Materialize(source));

	[Jazor(Op.Import, "static System.Linq.Enumerable.Reverse<TSource>(TSource[])", "reverseArray")]
	public static Array<TSource> ReverseArray(Array<TSource> source)
		=> ReverseCore(source);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Concat<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)", "concat")]
	public static Array<TSource> Concat(IEnumerable<TSource> first, IEnumerable<TSource> second)
		=> ConcatCore(first, second);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Append<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", "append")]
	public static Array<TSource> Append(IEnumerable<TSource> source, TSource element)
		=> AppendCore(source, element);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Prepend<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", "prepend")]
	public static Array<TSource> Prepend(IEnumerable<TSource> source, TSource element)
		=> PrependCore(source, element);

	/// <summary>
	/// The parameterless overload needs the caller's closed TSource default. The JavaScript
	/// runtime does not retain erased generic arguments, so SemanticWalker delegates to the
	/// explicit fallback overload after lowering default(TSource) at the bound invocation site.
	/// </summary>
	[Jazor(Op.Compile, "static System.Linq.Enumerable.DefaultIfEmpty<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "EnumerableDefaultIfEmpty")]
	public static Array<TSource> DefaultIfEmpty(IEnumerable<TSource> source)
		=> DefaultIfEmptyCore(source, default!);

	[Jazor(Op.Import, "static System.Linq.Enumerable.DefaultIfEmpty<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", "defaultIfEmpty")]
	public static Array<TSource> DefaultIfEmpty(IEnumerable<TSource> source, TSource defaultValue)
		=> DefaultIfEmptyCore(source, defaultValue);

	[Jazor(Op.Compile, "static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "EnumerableFirstOrDefault")]
	public static TSource FirstOrDefault(IEnumerable<TSource> source)
		=> FirstOrDefaultCore(source, default!);

	[Jazor(Op.Compile, "static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", "EnumerableFirstOrDefault")]
	public static TSource FirstOrDefault(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> FirstOrDefaultCore(source, predicate, default!);

	[Jazor(Op.Import, "static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", "firstOrDefault")]
	public static TSource FirstOrDefault(IEnumerable<TSource> source, TSource defaultValue)
		=> FirstOrDefaultCore(source, defaultValue);

	[Jazor(Op.Import, "static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)", "firstOrDefaultWhere")]
	public static TSource FirstOrDefaultWhere(IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue)
		=> FirstOrDefaultCore(source, predicate, defaultValue);

	[Jazor(Op.Compile, "static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "EnumerableLastOrDefault")]
	public static TSource LastOrDefault(IEnumerable<TSource> source)
		=> LastOrDefaultCore(source, default!);

	[Jazor(Op.Compile, "static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", "EnumerableLastOrDefault")]
	public static TSource LastOrDefault(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> LastOrDefaultCore(source, predicate, default!);

	[Jazor(Op.Import, "static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", "lastOrDefault")]
	public static TSource LastOrDefault(IEnumerable<TSource> source, TSource defaultValue)
		=> LastOrDefaultCore(source, defaultValue);

	[Jazor(Op.Import, "static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)", "lastOrDefaultWhere")]
	public static TSource LastOrDefaultWhere(IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue)
		=> LastOrDefaultCore(source, predicate, defaultValue);

	[Jazor(Op.Compile, "static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "EnumerableSingleOrDefault")]
	public static TSource SingleOrDefault(IEnumerable<TSource> source)
		=> SingleOrDefaultCore(source, default!);

	[Jazor(Op.Compile, "static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", "EnumerableSingleOrDefault")]
	public static TSource SingleOrDefault(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> SingleOrDefaultCore(source, predicate, default!);

	[Jazor(Op.Import, "static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", "singleOrDefault")]
	public static TSource SingleOrDefault(IEnumerable<TSource> source, TSource defaultValue)
		=> SingleOrDefaultCore(source, defaultValue);

	[Jazor(Op.Import, "static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)", "singleOrDefaultWhere")]
	public static TSource SingleOrDefaultWhere(IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue)
		=> SingleOrDefaultCore(source, predicate, defaultValue);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", "elementAt")]
	public static TSource ElementAt(IEnumerable<TSource> source, Number index)
		=> ElementAtCore(source, index);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Index)", "elementAtIndex")]
	public static TSource ElementAtIndex(IEnumerable<TSource> source, RuntimeModule.JIndex index)
		=> ElementAtIndexCore(source, index);

	/// <summary>
	/// Returns the item at the requested zero-based index, or the closed C# default value when
	/// the index is negative or outside the source. The runtime erases <c>TSource</c>, so the
	/// compiler owns closed <c>default(TSource)</c> lowering through the Compile contract.
	/// </summary>
	[Jazor(Op.Compile, "static System.Linq.Enumerable.ElementAtOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", "EnumerableElementAtOrDefault")]
	public static TSource ElementAtOrDefault(IEnumerable<TSource> source, Number index)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");
		if (index < 0)
			return default!;

		var currentIndex = 0;
		foreach (var item in source)
		{
			if (currentIndex == index)
				return item;
			currentIndex++;
		}

		return default!;
	}

	/// <summary>
	/// Returns the item at the requested from-start or from-end index, or the closed C# default
	/// value when the index is outside the source. The compiler owns both the closed default and
	/// the distinct traversal protocols through the shared Compile contract.
	/// </summary>
	[Jazor(Op.Compile, "static System.Linq.Enumerable.ElementAtOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Index)", "EnumerableElementAtOrDefault")]
	public static TSource ElementAtOrDefault(IEnumerable<TSource> source, RuntimeModule.JIndex index)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		var indexValue = index.Value;
		if (!index.IsFromEnd)
		{
			var currentIndex = 0;
			foreach (var item in source)
			{
				if (currentIndex == indexValue)
					return item;
				currentIndex++;
			}

			return default!;
		}

		if (indexValue == 0)
			return default!;

		var tail = new Array<TSource>();
		var tailIndex = 0;
		foreach (var item in source)
		{
			if (tail.Length < indexValue)
			{
				tail.Push(item);
				continue;
			}

			tail[tailIndex] = item;
			tailIndex = (tailIndex + 1) % indexValue;
		}

		return tail.Length < indexValue ? default! : tail[tailIndex];
	}

	[Jazor(Op.Import, "static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "first")]
	public static TSource First(IEnumerable<TSource> source)
		=> FirstCore(source);

	[Jazor(Op.Import, "static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", "firstWhere")]
	public static TSource FirstWhere(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> FirstCore(source, predicate);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "last")]
	public static TSource Last(IEnumerable<TSource> source)
		=> LastCore(source);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", "lastWhere")]
	public static TSource LastWhere(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> LastCore(source, predicate);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "single")]
	public static TSource Single(IEnumerable<TSource> source)
		=> SingleCore(source);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", "singleWhere")]
	public static TSource SingleWhere(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> SingleCore(source, predicate);

	[Jazor(Op.Import, "static System.Linq.Enumerable.SequenceEqual<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)", "sequenceEqual")]
	public static bool SequenceEqual(Array<TSource> first, Array<TSource> second)
		=> SequenceEqualCore(first, second, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.SequenceEqual<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)", "sequenceEqualWithComparer")]
	public static bool SequenceEqualWithComparer(
		Array<TSource> first,
		Array<TSource> second,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
		=> SequenceEqualCore(first, second, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Aggregate<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TSource, TSource>)", "aggregate")]
	public static TSource Aggregate(IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
		=> AggregateCore(source, func);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Aggregate<TSource, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>)", "aggregateWithSeed")]
	public static TAccumulate AggregateWithSeed<TAccumulate>(
		IEnumerable<TSource> source,
		TAccumulate seed,
		Func<TAccumulate, TSource, TAccumulate> func)
		=> AggregateCore(source, seed, func);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Aggregate<TSource, TAccumulate, TResult>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Func<TAccumulate, TResult>)", "aggregateWithResult")]
	public static TResult AggregateWithResult<TAccumulate, TResult>(
		IEnumerable<TSource> source,
		TAccumulate seed,
		Func<TAccumulate, TSource, TAccumulate> func,
		Func<TAccumulate, TResult> resultSelector)
		=> AggregateCore(source, seed, func, resultSelector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>)")]
	public static bool _9832a60d5939c887(IEnumerable<TSource> source)
		=> AnyCore(source);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)")]
	public static bool _8995eebc6c105f1d(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> AnyCore(source, predicate);

	[Jazor(Op.Import, "static System.Linq.Enumerable.All<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)")]
	public static bool _7e4a11c411867592(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> AllCore(source, predicate);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>)")]
	public static Number _1cb3ec9a7fb8aaab(IEnumerable<TSource> source)
		=> CountCore(source);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)")]
	public static Number _e19baea1a0d8c2c6(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> CountCore(source, predicate);

	[Jazor(Op.Import, "static System.Linq.Enumerable.CountBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)", "countBy")]
	public static Array<System.Collections.Generic.KeyValuePair<TKey, Number>> CountBy<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> CountByCore(source, keySelector, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)", "aggregateBy")]
	public static Array<System.Collections.Generic.KeyValuePair<TKey, TAccumulate>> AggregateBy<TKey, TAccumulate>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		TAccumulate seed,
		Func<TAccumulate, TSource, TAccumulate> func,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> AggregateByCore(source, keySelector, seed, func, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, TAccumulate>, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)", "aggregateByWithSeedSelector")]
	public static Array<System.Collections.Generic.KeyValuePair<TKey, TAccumulate>> AggregateByWithSeedSelector<TKey, TAccumulate>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TKey, TAccumulate> seedSelector,
		Func<TAccumulate, TSource, TAccumulate> func,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> AggregateByCore(source, keySelector, seedSelector, func, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.LongCount<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "longCount")]
	public static BigInt LongCount(IEnumerable<TSource> source)
		=> LongCountCore(source);

	[Jazor(Op.Import, "static System.Linq.Enumerable.LongCount<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", "longCountWhere")]
	public static BigInt LongCountWhere(IEnumerable<TSource> source, Func<TSource, bool> predicate)
		=> LongCountCore(source, predicate);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Index<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "index")]
	public static Array<(Number Index, TSource Item)> Index(IEnumerable<TSource> source)
		=> IndexCore(source);

	/// <summary>
	/// C#: Enumerable.TryGetNonEnumeratedCount(source, out count)
	/// JS: the Jazor IEnumerable&lt;T&gt; carrier is always Array, so length is observable without
	/// advancing the iterator. The return array is the compiler's existing ref/out packing ABI:
	/// [method result, out count].
	/// </summary>
	[Jazor(Op.Import, "static System.Linq.Enumerable.TryGetNonEnumeratedCount<TSource>(System.Collections.Generic.IEnumerable<TSource>, out int)", "tryGetNonEnumeratedCount")]
	public static Array<object?> TryGetNonEnumeratedCount(Array<TSource> source, Number count)
	{
		if (source == null)
			throw new Error("ArgumentNullException: source is null");

		return [true, source.Length];
	}

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int>)", "sumInt")]
	public static Number SumInt(IEnumerable<Number> source)
		=> SumIntCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int?>)", "sumNullableInt")]
	public static Number? SumNullableInt(IEnumerable<Number?> source)
		=> SumNullableIntCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", "sumNullableIntBy")]
	public static Number? SumNullableIntBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> SumNullableIntCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", "sumIntBy")]
	public static Number SumIntBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> SumIntCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long>)", "sumInt64")]
	public static BigInt SumInt64(IEnumerable<BigInt> source)
		=> SumInt64Core(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long?>)", "sumNullableInt64")]
	public static BigInt? SumNullableInt64(IEnumerable<BigInt?> source)
		=> SumNullableInt64Core(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", "sumNullableInt64By")]
	public static BigInt? SumNullableInt64By(IEnumerable<TSource> source, Func<TSource, BigInt?> selector)
		=> SumNullableInt64Core(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", "sumInt64By")]
	public static BigInt SumInt64By(IEnumerable<TSource> source, Func<TSource, BigInt> selector)
		=> SumInt64Core(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float>)", "sumSingle")]
	public static Number SumSingle(IEnumerable<Number> source)
		=> SumNumberCore(source, item => item, singlePrecision: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float?>)", "sumNullableSingle")]
	public static Number? SumNullableSingle(IEnumerable<Number?> source)
		=> SumNullableNumberCore(source, item => item, singlePrecision: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", "sumNullableSingleBy")]
	public static Number? SumNullableSingleBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> SumNullableNumberCore(source, selector, singlePrecision: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", "sumSingleBy")]
	public static Number SumSingleBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> SumNumberCore(source, selector, singlePrecision: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double>)", "sumDouble")]
	public static Number SumDouble(IEnumerable<Number> source)
		=> SumNumberCore(source, item => item, singlePrecision: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double?>)", "sumNullableDouble")]
	public static Number? SumNullableDouble(IEnumerable<Number?> source)
		=> SumNullableNumberCore(source, item => item, singlePrecision: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", "sumNullableDoubleBy")]
	public static Number? SumNullableDoubleBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> SumNullableNumberCore(source, selector, singlePrecision: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", "sumDoubleBy")]
	public static Number SumDoubleBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> SumNumberCore(source, selector, singlePrecision: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal>)", "sumDecimal")]
	public static decimal SumDecimal(IEnumerable<decimal> source)
		=> SumDecimalCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal?>)", "sumNullableDecimal")]
	public static decimal? SumNullableDecimal(IEnumerable<decimal?> source)
		=> SumNullableDecimalCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", "sumNullableDecimalBy")]
	public static decimal? SumNullableDecimalBy(IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		=> SumNullableDecimalCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", "sumDecimalBy")]
	public static decimal SumDecimalBy(IEnumerable<TSource> source, Func<TSource, decimal> selector)
		=> SumDecimalCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int>)", "averageInt")]
	public static Number AverageInt(IEnumerable<Number> source)
		=> AverageIntCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int?>)", "averageNullableInt")]
	public static Number? AverageNullableInt(IEnumerable<Number?> source)
		=> AverageNullableIntCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", "averageNullableIntBy")]
	public static Number? AverageNullableIntBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> AverageNullableIntCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", "averageIntBy")]
	public static Number AverageIntBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> AverageIntCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long>)", "averageInt64")]
	public static Number AverageInt64(IEnumerable<BigInt> source)
		=> AverageInt64Core(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long?>)", "averageNullableInt64")]
	public static Number? AverageNullableInt64(IEnumerable<BigInt?> source)
		=> AverageNullableInt64Core(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", "averageNullableInt64By")]
	public static Number? AverageNullableInt64By(IEnumerable<TSource> source, Func<TSource, BigInt?> selector)
		=> AverageNullableInt64Core(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", "averageInt64By")]
	public static Number AverageInt64By(IEnumerable<TSource> source, Func<TSource, BigInt> selector)
		=> AverageInt64Core(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float>)", "averageSingle")]
	public static Number AverageSingle(IEnumerable<Number> source)
		=> AverageNumberCore(source, item => item, singlePrecision: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float?>)", "averageNullableSingle")]
	public static Number? AverageNullableSingle(IEnumerable<Number?> source)
		=> AverageNullableNumberCore(source, item => item, singlePrecision: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", "averageNullableSingleBy")]
	public static Number? AverageNullableSingleBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> AverageNullableNumberCore(source, selector, singlePrecision: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", "averageSingleBy")]
	public static Number AverageSingleBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> AverageNumberCore(source, selector, singlePrecision: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double>)", "averageDouble")]
	public static Number AverageDouble(IEnumerable<Number> source)
		=> AverageNumberCore(source, item => item, singlePrecision: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double?>)", "averageNullableDouble")]
	public static Number? AverageNullableDouble(IEnumerable<Number?> source)
		=> AverageNullableNumberCore(source, item => item, singlePrecision: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", "averageNullableDoubleBy")]
	public static Number? AverageNullableDoubleBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> AverageNullableNumberCore(source, selector, singlePrecision: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", "averageDoubleBy")]
	public static Number AverageDoubleBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> AverageNumberCore(source, selector, singlePrecision: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal>)", "averageDecimal")]
	public static decimal AverageDecimal(IEnumerable<decimal> source)
		=> AverageDecimalCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal?>)", "averageNullableDecimal")]
	public static decimal? AverageNullableDecimal(IEnumerable<decimal?> source)
		=> AverageNullableDecimalCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", "averageNullableDecimalBy")]
	public static decimal? AverageNullableDecimalBy(IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		=> AverageNullableDecimalCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", "averageDecimalBy")]
	public static decimal AverageDecimalBy(IEnumerable<TSource> source, Func<TSource, decimal> selector)
		=> AverageDecimalCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int>)", "minInt")]
	public static Number MinInt(IEnumerable<Number> source)
		=> MinNumberCore(source, item => item, floating: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int?>)", "minNullableInt")]
	public static Number? MinNullableInt(IEnumerable<Number?> source)
		=> MinNullableNumberCore(source, item => item, floating: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", "minNullableIntBy")]
	public static Number? MinNullableIntBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> MinNullableNumberCore(source, selector, floating: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", "minIntBy")]
	public static Number MinIntBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> MinNumberCore(source, selector, floating: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<int>)", "maxInt")]
	public static Number MaxInt(IEnumerable<Number> source)
		=> MaxNumberCore(source, item => item, floating: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<int?>)", "maxNullableInt")]
	public static Number? MaxNullableInt(IEnumerable<Number?> source)
		=> MaxNullableNumberCore(source, item => item, floating: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", "maxNullableIntBy")]
	public static Number? MaxNullableIntBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> MaxNullableNumberCore(source, selector, floating: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", "maxIntBy")]
	public static Number MaxIntBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> MaxNumberCore(source, selector, floating: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<long>)", "minInt64")]
	public static BigInt MinInt64(IEnumerable<BigInt> source)
		=> MinBigIntCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<long?>)", "minNullableInt64")]
	public static BigInt? MinNullableInt64(IEnumerable<BigInt?> source)
		=> MinNullableBigIntCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", "minNullableInt64By")]
	public static BigInt? MinNullableInt64By(IEnumerable<TSource> source, Func<TSource, BigInt?> selector)
		=> MinNullableBigIntCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", "minInt64By")]
	public static BigInt MinInt64By(IEnumerable<TSource> source, Func<TSource, BigInt> selector)
		=> MinBigIntCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<long>)", "maxInt64")]
	public static BigInt MaxInt64(IEnumerable<BigInt> source)
		=> MaxBigIntCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<long?>)", "maxNullableInt64")]
	public static BigInt? MaxNullableInt64(IEnumerable<BigInt?> source)
		=> MaxNullableBigIntCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", "maxNullableInt64By")]
	public static BigInt? MaxNullableInt64By(IEnumerable<TSource> source, Func<TSource, BigInt?> selector)
		=> MaxNullableBigIntCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", "maxInt64By")]
	public static BigInt MaxInt64By(IEnumerable<TSource> source, Func<TSource, BigInt> selector)
		=> MaxBigIntCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<float>)", "minSingle")]
	public static Number MinSingle(IEnumerable<Number> source)
		=> MinNumberCore(source, item => item, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<float?>)", "minNullableSingle")]
	public static Number? MinNullableSingle(IEnumerable<Number?> source)
		=> MinNullableNumberCore(source, item => item, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", "minNullableSingleBy")]
	public static Number? MinNullableSingleBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> MinNullableNumberCore(source, selector, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", "minSingleBy")]
	public static Number MinSingleBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> MinNumberCore(source, selector, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<float>)", "maxSingle")]
	public static Number MaxSingle(IEnumerable<Number> source)
		=> MaxNumberCore(source, item => item, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<float?>)", "maxNullableSingle")]
	public static Number? MaxNullableSingle(IEnumerable<Number?> source)
		=> MaxNullableNumberCore(source, item => item, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", "maxNullableSingleBy")]
	public static Number? MaxNullableSingleBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> MaxNullableNumberCore(source, selector, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", "maxSingleBy")]
	public static Number MaxSingleBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> MaxNumberCore(source, selector, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<double>)", "minDouble")]
	public static Number MinDouble(IEnumerable<Number> source)
		=> MinNumberCore(source, item => item, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<double?>)", "minNullableDouble")]
	public static Number? MinNullableDouble(IEnumerable<Number?> source)
		=> MinNullableNumberCore(source, item => item, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", "minNullableDoubleBy")]
	public static Number? MinNullableDoubleBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> MinNullableNumberCore(source, selector, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", "minDoubleBy")]
	public static Number MinDoubleBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> MinNumberCore(source, selector, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<double>)", "maxDouble")]
	public static Number MaxDouble(IEnumerable<Number> source)
		=> MaxNumberCore(source, item => item, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<double?>)", "maxNullableDouble")]
	public static Number? MaxNullableDouble(IEnumerable<Number?> source)
		=> MaxNullableNumberCore(source, item => item, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", "maxNullableDoubleBy")]
	public static Number? MaxNullableDoubleBy(IEnumerable<TSource> source, Func<TSource, Number?> selector)
		=> MaxNullableNumberCore(source, selector, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", "maxDoubleBy")]
	public static Number MaxDoubleBy(IEnumerable<TSource> source, Func<TSource, Number> selector)
		=> MaxNumberCore(source, selector, floating: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<decimal>)", "minDecimal")]
	public static decimal MinDecimal(IEnumerable<decimal> source)
		=> MinDecimalCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<decimal?>)", "minNullableDecimal")]
	public static decimal? MinNullableDecimal(IEnumerable<decimal?> source)
		=> MinNullableDecimalCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", "minNullableDecimalBy")]
	public static decimal? MinNullableDecimalBy(IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		=> MinNullableDecimalCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", "minDecimalBy")]
	public static decimal MinDecimalBy(IEnumerable<TSource> source, Func<TSource, decimal> selector)
		=> MinDecimalCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal>)", "maxDecimal")]
	public static decimal MaxDecimal(IEnumerable<decimal> source)
		=> MaxDecimalCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal?>)", "maxNullableDecimal")]
	public static decimal? MaxNullableDecimal(IEnumerable<decimal?> source)
		=> MaxNullableDecimalCore(source, item => item);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", "maxNullableDecimalBy")]
	public static decimal? MaxNullableDecimalBy(IEnumerable<TSource> source, Func<TSource, decimal?> selector)
		=> MaxNullableDecimalCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", "maxDecimalBy")]
	public static decimal MaxDecimalBy(IEnumerable<TSource> source, Func<TSource, decimal> selector)
		=> MaxDecimalCore(source, selector);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Distinct<TSource>(System.Collections.Generic.IEnumerable<TSource>)")]
	public static Array<TSource> _a2bc38786226403e(IEnumerable<TSource> source)
		=> DistinctCore(source, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Distinct<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)", "distinctWithComparer")]
	public static Array<TSource> DistinctWithComparer(
		IEnumerable<TSource> source,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
		=> DistinctCore(source, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.DistinctBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", "distinctBy")]
	public static Array<TSource> DistinctBy<TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		=> DistinctByCore(source, keySelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.DistinctBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)", "distinctByWithComparer")]
	public static Array<TSource> DistinctByWithComparer<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> DistinctByCore(source, keySelector, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.MinBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", "minBy")]
	public static TSource MinBy<TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		=> ExtremumByCore(source, keySelector, maximum: false, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.MinBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)", "minByWithComparer")]
	public static TSource MinByWithComparer<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IComparer<TKey>? comparer)
		=> ExtremumByCore(source, keySelector, maximum: false, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.MaxBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", "maxBy")]
	public static TSource MaxBy<TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		=> ExtremumByCore(source, keySelector, maximum: true, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.MaxBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)", "maxByWithComparer")]
	public static TSource MaxByWithComparer<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IComparer<TKey>? comparer)
		=> ExtremumByCore(source, keySelector, maximum: true, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Union<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)")]
	public static Array<TSource> _b5fae0c231974056(IEnumerable<TSource> first, IEnumerable<TSource> second)
		=> UnionCore(first, second, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Union<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)", "unionWithComparer")]
	public static Array<TSource> UnionWithComparer(
		IEnumerable<TSource> first,
		IEnumerable<TSource> second,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
		=> UnionCore(first, second, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Except<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)")]
	public static Array<TSource> _c71d4ff9a863431d(IEnumerable<TSource> first, IEnumerable<TSource> second)
		=> ExceptCore(first, second, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Except<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)", "exceptWithComparer")]
	public static Array<TSource> ExceptWithComparer(
		IEnumerable<TSource> first,
		IEnumerable<TSource> second,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
		=> ExceptCore(first, second, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Intersect<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)")]
	public static Array<TSource> _d83c9e4a7bf747a8(IEnumerable<TSource> first, IEnumerable<TSource> second)
		=> IntersectCore(first, second, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Intersect<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)", "intersectWithComparer")]
	public static Array<TSource> IntersectWithComparer(
		IEnumerable<TSource> first,
		IEnumerable<TSource> second,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
		=> IntersectCore(first, second, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.UnionBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", "unionBy")]
	public static Array<TSource> UnionBy<TKey>(
		IEnumerable<TSource> first,
		IEnumerable<TSource> second,
		Func<TSource, TKey> keySelector)
		=> UnionByCore(first, second, keySelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.UnionBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)", "unionByWithComparer")]
	public static Array<TSource> UnionByWithComparer<TKey>(
		IEnumerable<TSource> first,
		IEnumerable<TSource> second,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> UnionByCore(first, second, keySelector, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ExceptBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)", "exceptBy")]
	public static Array<TSource> ExceptBy<TKey>(
		IEnumerable<TSource> first,
		IEnumerable<TKey> second,
		Func<TSource, TKey> keySelector)
		=> ExceptByCore(first, second, keySelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ExceptBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)", "exceptByWithComparer")]
	public static Array<TSource> ExceptByWithComparer<TKey>(
		IEnumerable<TSource> first,
		IEnumerable<TKey> second,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> ExceptByCore(first, second, keySelector, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.IntersectBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)", "intersectBy")]
	public static Array<TSource> IntersectBy<TKey>(
		IEnumerable<TSource> first,
		IEnumerable<TKey> second,
		Func<TSource, TKey> keySelector)
		=> IntersectByCore(first, second, keySelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.IntersectBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)", "intersectByWithComparer")]
	public static Array<TSource> IntersectByWithComparer<TKey>(
		IEnumerable<TSource> first,
		IEnumerable<TKey> second,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> IntersectByCore(first, second, keySelector, comparer);

	[Jazor(Op.Compile, "static System.Linq.Enumerable.Zip<TFirst, TSecond>(System.Collections.Generic.IEnumerable<TFirst>, System.Collections.Generic.IEnumerable<TSecond>)", "EnumerableZip")]
	public static IEnumerable<(TSource First, TSecond Second)> Zip<TSecond>(
		IEnumerable<TSource> first,
		IEnumerable<TSecond> second)
	{
		throw new Error("Enumerable.Zip is lowered by the compiler iterator protocol.");
	}

	[Jazor(Op.Compile, "static System.Linq.Enumerable.Zip<TFirst, TSecond, TResult>(System.Collections.Generic.IEnumerable<TFirst>, System.Collections.Generic.IEnumerable<TSecond>, System.Func<TFirst, TSecond, TResult>)", "EnumerableZip")]
	public static IEnumerable<TResult> Zip<TSecond, TResult>(
		IEnumerable<TSource> first,
		IEnumerable<TSecond> second,
		Func<TSource, TSecond, TResult> resultSelector)
	{
		throw new Error("Enumerable.Zip is lowered by the compiler iterator protocol.");
	}

	[Jazor(Op.Compile, "static System.Linq.Enumerable.Zip<TFirst, TSecond, TThird>(System.Collections.Generic.IEnumerable<TFirst>, System.Collections.Generic.IEnumerable<TSecond>, System.Collections.Generic.IEnumerable<TThird>)", "EnumerableZip")]
	public static IEnumerable<(TSource First, TSecond Second, TThird Third)> Zip<TSecond, TThird>(
		IEnumerable<TSource> first,
		IEnumerable<TSecond> second,
		IEnumerable<TThird> third)
	{
		throw new Error("Enumerable.Zip is lowered by the compiler iterator protocol.");
	}

	[Jazor(Op.Compile, "static System.Linq.Enumerable.Cast<TResult>(System.Collections.IEnumerable)", "EnumerableCast")]
	public static IEnumerable<TResult> Cast<TResult>(System.Collections.IEnumerable source)
	{
		throw new Error("Enumerable.Cast is lowered by the compiler type-filter protocol.");
	}

	[Jazor(Op.Compile, "static System.Linq.Enumerable.OfType<TResult>(System.Collections.IEnumerable)", "EnumerableOfType")]
	public static IEnumerable<TResult> OfType<TResult>(System.Collections.IEnumerable source)
	{
		throw new Error("Enumerable.OfType is lowered by the compiler type-filter protocol.");
	}

	[Jazor(Op.Import, "static System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)")]
	public static bool _e94a7db8306f4e71(IEnumerable<TSource> source, TSource value)
		=> ContainsCore(source, value, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource, System.Collections.Generic.IEqualityComparer<TSource>)", "containsWithComparer")]
	public static bool ContainsWithComparer(
		IEnumerable<TSource> source,
		TSource value,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
		=> ContainsCore(source, value, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.GroupBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)")]
	public static Array<Array<TSource>> _b7a70ff977974880<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector)
		=> GroupByCore(source, keySelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.GroupBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)", "groupByWithComparer")]
	public static Array<Array<TSource>> GroupByWithComparer<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> GroupByCore(source, keySelector, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)")]
	public static Array<Array<TElement>> _e62121525c074f74<TKey, TElement>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector)
		=> GroupByCore(source, keySelector, elementSelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Collections.Generic.IEqualityComparer<TKey>)", "groupByElementWithComparer")]
	public static Array<Array<TElement>> GroupByElementWithComparer<TKey, TElement>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> GroupByCore(source, keySelector, elementSelector, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.GroupBy<TSource, TKey, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, System.Collections.Generic.IEnumerable<TSource>, TResult>)", "groupByResult")]
	public static Array<TResult> GroupByResult<TKey, TResult>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TKey, Array<TSource>, TResult> resultSelector)
		=> GroupByResultCore(source, keySelector, item => item, resultSelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.GroupBy<TSource, TKey, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, System.Collections.Generic.IEnumerable<TSource>, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)", "groupByResultWithComparer")]
	public static Array<TResult> GroupByResultWithComparer<TKey, TResult>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TKey, Array<TSource>, TResult> resultSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> GroupByResultCore(source, keySelector, item => item, resultSelector, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Func<TKey, System.Collections.Generic.IEnumerable<TElement>, TResult>)", "groupByElementResult")]
	public static Array<TResult> GroupByElementResult<TKey, TElement, TResult>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector,
		Func<TKey, Array<TElement>, TResult> resultSelector)
		=> GroupByResultCore(source, keySelector, elementSelector, resultSelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Func<TKey, System.Collections.Generic.IEnumerable<TElement>, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)", "groupByElementResultWithComparer")]
	public static Array<TResult> GroupByElementResultWithComparer<TKey, TElement, TResult>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector,
		Func<TKey, Array<TElement>, TResult> resultSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> GroupByResultCore(source, keySelector, elementSelector, resultSelector, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToLookup<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", "toLookup")]
	public static Array<Array<TSource>> ToLookup<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector)
		=> ToLookupCore(source, keySelector, item => item, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToLookup<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)", "toLookupWithComparer")]
	public static Array<Array<TSource>> ToLookupWithComparer<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> ToLookupCore(source, keySelector, item => item, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToLookup<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)", "toLookupElement")]
	public static Array<Array<TElement>> ToLookupElement<TKey, TElement>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector)
		=> ToLookupCore(source, keySelector, elementSelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToLookup<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Collections.Generic.IEqualityComparer<TKey>)", "toLookupElementWithComparer")]
	public static Array<Array<TElement>> ToLookupElementWithComparer<TKey, TElement>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> ToLookupCore(source, keySelector, elementSelector, comparer);

	[Jazor(Op.Import, "System.Linq.ILookup<TKey, TElement>.Count.get", "lookupCount")]
	public static Number LookupCount<TKey, TElement>(Array<Array<TElement>> instance)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		return instance.Length;
	}

	[Jazor(Op.Import, "System.Linq.ILookup<TKey, TElement>.Contains(TKey)", "lookupContains")]
	public static bool LookupContains<TKey, TElement>(Array<Array<TElement>> instance, TKey key)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		var comparer = LookupComparers.Has(instance)
			? (System.Collections.Generic.IEqualityComparer<TKey>?)LookupComparers.Get(instance)
			: null;
		return FindLookupGrouping(instance, key, comparer) != null;
	}

	[Jazor(Op.Import, "System.Linq.ILookup<TKey, TElement>.this[TKey].get", "lookupGet")]
	public static Array<TElement> LookupGet<TKey, TElement>(Array<Array<TElement>> instance, TKey key)
	{
		if (instance == null)
			throw new Error("NullReferenceException: instance is null.");

		var comparer = LookupComparers.Has(instance)
			? (System.Collections.Generic.IEqualityComparer<TKey>?)LookupComparers.Get(instance)
			: null;
		return FindLookupGrouping(instance, key, comparer) ?? new Array<TElement>();
	}

	[Jazor(Op.Import, "static System.Linq.Enumerable.Join<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, TInner, TResult>)")]
	public static Array<TResult> _f10104b4c52b4f96<TInner, TKey, TResult>(
		IEnumerable<TSource> outer,
		IEnumerable<TInner> inner,
		Func<TSource, TKey> outerKeySelector,
		Func<TInner, TKey> innerKeySelector,
		Func<TSource, TInner, TResult> resultSelector)
		=> JoinCore(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Join<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, TInner, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)", "joinWithComparer")]
	public static Array<TResult> JoinWithComparer<TInner, TKey, TResult>(
		IEnumerable<TSource> outer,
		IEnumerable<TInner> inner,
		Func<TSource, TKey> outerKeySelector,
		Func<TInner, TKey> innerKeySelector,
		Func<TSource, TInner, TResult> resultSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> JoinCore(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.GroupJoin<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, System.Collections.Generic.IEnumerable<TInner>, TResult>)")]
	public static Array<TResult> _b61f41d1ac124b69<TInner, TKey, TResult>(
		IEnumerable<TSource> outer,
		IEnumerable<TInner> inner,
		Func<TSource, TKey> outerKeySelector,
		Func<TInner, TKey> innerKeySelector,
		Func<TSource, Array<TInner>, TResult> resultSelector)
		=> GroupJoinCore(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.GroupJoin<TOuter, TInner, TKey, TResult>(System.Collections.Generic.IEnumerable<TOuter>, System.Collections.Generic.IEnumerable<TInner>, System.Func<TOuter, TKey>, System.Func<TInner, TKey>, System.Func<TOuter, System.Collections.Generic.IEnumerable<TInner>, TResult>, System.Collections.Generic.IEqualityComparer<TKey>)", "groupJoinWithComparer")]
	public static Array<TResult> GroupJoinWithComparer<TInner, TKey, TResult>(
		IEnumerable<TSource> outer,
		IEnumerable<TInner> inner,
		Func<TSource, TKey> outerKeySelector,
		Func<TInner, TKey> innerKeySelector,
		Func<TSource, Array<TInner>, TResult> resultSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> GroupJoinCore(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.OrderBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)")]
	public static Array<TSource> _c8e0de6cfb4d0b1e<TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		=> OrderByCore(source, keySelector, descending: false, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.OrderBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)", "orderByWithComparer")]
	public static Array<TSource> OrderByWithComparer<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IComparer<TKey>? comparer)
		=> OrderByCore(source, keySelector, descending: false, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.OrderByDescending<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)")]
	public static Array<TSource> _c955435630a10962<TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		=> OrderByCore(source, keySelector, descending: true, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.OrderByDescending<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)", "orderByDescendingWithComparer")]
	public static Array<TSource> OrderByDescendingWithComparer<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IComparer<TKey>? comparer)
		=> OrderByCore(source, keySelector, descending: true, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Order<T>(System.Collections.Generic.IEnumerable<T>)", "order")]
	public static Array<TSource> Order(IEnumerable<TSource> source)
		=> OrderCore(source, descending: false);

	[Jazor(Op.Import, "static System.Linq.Enumerable.Order<T>(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IComparer<T>)", "orderWithComparer")]
	public static Array<TSource> OrderWithComparer(
		IEnumerable<TSource> source,
		System.Collections.Generic.IComparer<TSource>? comparer)
		=> OrderByCore(source, item => item, descending: false, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.OrderDescending<T>(System.Collections.Generic.IEnumerable<T>)", "orderDescending")]
	public static Array<TSource> OrderDescending(IEnumerable<TSource> source)
		=> OrderCore(source, descending: true);

	[Jazor(Op.Import, "static System.Linq.Enumerable.OrderDescending<T>(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IComparer<T>)", "orderDescendingWithComparer")]
	public static Array<TSource> OrderDescendingWithComparer(
		IEnumerable<TSource> source,
		System.Collections.Generic.IComparer<TSource>? comparer)
		=> OrderByCore(source, item => item, descending: true, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ThenBy<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>)")]
	public static Array<TSource> _b9eeb5472648105d<TKey>(Array<TSource> source, Func<TSource, TKey> keySelector)
		=> ThenByCore(source, keySelector, descending: false, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ThenBy<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)", "thenByWithComparer")]
	public static Array<TSource> ThenByWithComparer<TKey>(
		Array<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IComparer<TKey>? comparer)
		=> ThenByCore(source, keySelector, descending: false, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ThenByDescending<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>)")]
	public static Array<TSource> _c08a571c42e14ee7<TKey>(Array<TSource> source, Func<TSource, TKey> keySelector)
		=> ThenByCore(source, keySelector, descending: true, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ThenByDescending<TSource, TKey>(System.Linq.IOrderedEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IComparer<TKey>)", "thenByDescendingWithComparer")]
	public static Array<TSource> ThenByDescendingWithComparer<TKey>(
		Array<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IComparer<TKey>? comparer)
		=> ThenByCore(source, keySelector, descending: true, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToHashSet<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "toHashSet")]
	public static Set<TSource> ToHashSet(IEnumerable<TSource> source)
		=> ToHashSetCore(source, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToHashSet<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEqualityComparer<TSource>)", "toHashSetWithComparer")]
	public static Set<TSource> ToHashSetWithComparer(
		IEnumerable<TSource> source,
		System.Collections.Generic.IEqualityComparer<TSource>? comparer)
		=> ToHashSetCore(source, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToDictionary<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", "toDictionary")]
	public static Map<TKey, TSource> ToDictionary<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector)
		=> ToDictionaryCore(source, keySelector, item => item, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToDictionary<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)", "toDictionaryWithComparer")]
	public static Map<TKey, TSource> ToDictionaryWithComparer<TKey>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> ToDictionaryCore(source, keySelector, item => item, comparer);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToDictionary<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)", "toDictionaryElement")]
	public static Map<TKey, TElement> ToDictionaryElement<TKey, TElement>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector)
		=> ToDictionaryCore(source, keySelector, elementSelector, comparer: null);

	[Jazor(Op.Import, "static System.Linq.Enumerable.ToDictionary<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Collections.Generic.IEqualityComparer<TKey>)", "toDictionaryElementWithComparer")]
	public static Map<TKey, TElement> ToDictionaryElementWithComparer<TKey, TElement>(
		IEnumerable<TSource> source,
		Func<TSource, TKey> keySelector,
		Func<TSource, TElement> elementSelector,
		System.Collections.Generic.IEqualityComparer<TKey>? comparer)
		=> ToDictionaryCore(source, keySelector, elementSelector, comparer);

	[Jazor(Op.Compile, "static System.Linq.Enumerable.ToList<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "EnumerableArrayLike")]
	public static Array<TSource> _6293e95141f14a55(IEnumerable<TSource> source)
		=> RuntimeModule.MarkAsMutableListCarrier(Materialize(source));

	[Jazor(Op.Compile, "static System.Linq.Enumerable.ToArray<TSource>(System.Collections.Generic.IEnumerable<TSource>)", "EnumerableArrayLike")]
	public static Array<TSource> _ea56f0fe56c44ae7(IEnumerable<TSource> source)
		=> Materialize(source);
}
