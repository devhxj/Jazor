using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class CollectionInterfaceWhitelistTests
{
	[TestMethod]
	public void IEnumerableInterfaceMappings_DiscardExplicitEnumerators()
	{
		AssertTypeAlias(typeof(Jazor.CLR.IEnumerableModule), "System.Collections.IEnumerable", "Array");
		AssertTypeAlias(typeof(Jazor.CLR.IEnumerableT1Module<>), "System.Collections.Generic.IEnumerable<T>", "Array");

		AssertMemberOp(typeof(Jazor.CLR.IEnumerableModule), "System.Collections.IEnumerable.GetEnumerator()", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.IEnumerableT1Module<>), "System.Collections.Generic.IEnumerable<T>.GetEnumerator()", Op.Discard);
	}

	[TestMethod]
	public void EnumerableMappings_SupportMaterializedQueryAndTerminalOperations()
	{
		AssertTypeAlias(typeof(Jazor.CLR.EnumerableModule<>), "System.Linq.Enumerable", "Array");
		AssertTypeAlias(typeof(Jazor.CLR.LookupT2Module<,>), "System.Linq.ILookup<TKey, TElement>", "Array");

		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Empty<TResult>()", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Range(int, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Repeat<TResult>(TResult, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.AsEnumerable<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sequence<T>(T, T, T)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.SelectMany<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, System.Collections.Generic.IEnumerable<TResult>>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.SelectMany<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, System.Collections.Generic.IEnumerable<TResult>>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.SelectMany<TSource, TCollection, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, System.Collections.Generic.IEnumerable<TCollection>>, System.Func<TSource, TCollection, TResult>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.CountBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Collections.Generic.IEqualityComparer<TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.AggregateBy<TSource, TKey, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, TAccumulate>, System.Func<TAccumulate, TSource, TAccumulate>, System.Collections.Generic.IEqualityComparer<TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.LongCount<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.LongCount<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Index<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.TryGetNonEnumeratedCount<TSource>(System.Collections.Generic.IEnumerable<TSource>, out int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<int?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<long?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<float?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<double?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum(System.Collections.Generic.IEnumerable<decimal?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Sum<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<int?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<long?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<float?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<double?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average(System.Collections.Generic.IEnumerable<decimal?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Average<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<int?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<long?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<long?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<float?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<float?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<double?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<double?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<decimal?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, long?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, float?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, double?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, decimal?>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<int>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<int>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<long>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<long>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<float>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<float>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<double>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<double>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Min(System.Collections.Generic.IEnumerable<decimal>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Max(System.Collections.Generic.IEnumerable<decimal>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Reverse<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Reverse<TSource>(TSource[])", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.First<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Last<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Single<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.FirstOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.LastOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.SingleOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>, TSource)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.SequenceEqual<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Aggregate<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TSource, TSource>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.MemoryExtensionsModule<>), "System.ReadOnlySpan<T>.SequenceEqual<T>(System.ReadOnlySpan<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.MemoryExtensionsModule<>), "System.ReadOnlySpan<char>.Trim()", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.MemoryExtensionsModule<>), "System.ReadOnlySpan<char>.Trim(char)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.MemoryExtensionsModule<>), "System.ReadOnlySpan<char>.Trim(System.ReadOnlySpan<char>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.MemoryExtensionsModule<>), "System.ReadOnlySpan<char>.TrimStart()", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.MemoryExtensionsModule<>), "System.ReadOnlySpan<char>.TrimStart(char)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.MemoryExtensionsModule<>), "System.ReadOnlySpan<char>.TrimStart(System.ReadOnlySpan<char>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.MemoryExtensionsModule<>), "System.ReadOnlySpan<char>.TrimEnd()", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.MemoryExtensionsModule<>), "System.ReadOnlySpan<char>.TrimEnd(char)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.MemoryExtensionsModule<>), "System.ReadOnlySpan<char>.TrimEnd(System.ReadOnlySpan<char>)", Op.Import);
		AssertTypeAlias(typeof(Jazor.CLR.ReadOnlySpanT1Module<>), "System.ReadOnlySpan<T>", "Array");
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Aggregate<TSource, TAccumulate>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Aggregate<TSource, TAccumulate, TResult>(System.Collections.Generic.IEnumerable<TSource>, TAccumulate, System.Func<TAccumulate, TSource, TAccumulate>, System.Func<TAccumulate, TResult>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Concat<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Append<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Prepend<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.SkipWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.SkipWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.TakeWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.TakeWhile<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, int, bool>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.SkipLast<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.TakeLast<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Take<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Range)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.DefaultIfEmpty<TSource>(System.Collections.Generic.IEnumerable<TSource>)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.DefaultIfEmpty<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.ElementAt<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Index)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.ElementAtOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.ElementAtOrDefault<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Index)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.DistinctBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.UnionBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.ExceptBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.IntersectBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Collections.Generic.IEnumerable<TKey>, System.Func<TSource, TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Zip<TFirst, TSecond>(System.Collections.Generic.IEnumerable<TFirst>, System.Collections.Generic.IEnumerable<TSecond>)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Zip<TFirst, TSecond, TResult>(System.Collections.Generic.IEnumerable<TFirst>, System.Collections.Generic.IEnumerable<TSecond>, System.Func<TFirst, TSecond, TResult>)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Zip<TFirst, TSecond, TThird>(System.Collections.Generic.IEnumerable<TFirst>, System.Collections.Generic.IEnumerable<TSecond>, System.Collections.Generic.IEnumerable<TThird>)", Op.Compile);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Order<T>(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.OrderDescending<T>(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.MinBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.MaxBy<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.Chunk<TSource>(System.Collections.Generic.IEnumerable<TSource>, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.GroupBy<TSource, TKey, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TKey, System.Collections.Generic.IEnumerable<TSource>, TResult>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.GroupBy<TSource, TKey, TElement, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>, System.Func<TKey, System.Collections.Generic.IEnumerable<TElement>, TResult>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.ToLookup<TSource, TKey>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "static System.Linq.Enumerable.ToLookup<TSource, TKey, TElement>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TKey>, System.Func<TSource, TElement>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "System.Linq.ILookup<TKey, TElement>.Count.get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "System.Linq.ILookup<TKey, TElement>.Contains(TKey)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EnumerableModule<>), "System.Linq.ILookup<TKey, TElement>.this[TKey].get", Op.Import);
	}

	[TestMethod]
	public void KeyValuePairMappings_UseTheSharedTwoSlotEntryCarrier()
	{
		AssertTypeAlias(typeof(Jazor.CLR.KeyValuePairT2Module<,>), "System.Collections.Generic.KeyValuePair<TKey, TValue>", "Array");
		AssertMemberOp(typeof(Jazor.CLR.KeyValuePairT2Module<,>), "System.Collections.Generic.KeyValuePair<TKey, TValue>.KeyValuePair(TKey, TValue)", Op.Inline);
		AssertMemberOp(typeof(Jazor.CLR.KeyValuePairT2Module<,>), "System.Collections.Generic.KeyValuePair<TKey, TValue>.Key.get", Op.Inline);
		AssertMemberOp(typeof(Jazor.CLR.KeyValuePairT2Module<,>), "System.Collections.Generic.KeyValuePair<TKey, TValue>.Value.get", Op.Inline);
		AssertMemberOp(typeof(Jazor.CLR.KeyValuePairT2Module<,>), "System.Collections.Generic.KeyValuePair<TKey, TValue>.Deconstruct(out TKey, out TValue)", Op.Discard);
	}

	[TestMethod]
	public void ICollectionInterfaceMappings_PreserveListMutationAndFixedArrayBoundaries()
	{
		AssertTypeAlias(typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection", "Array");
		AssertTypeAlias(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>", "Array");

		AssertMemberOp(typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection.Count.get", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection.CopyTo(System.Array, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection.SyncRoot.get", Op.Inline);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection.IsSynchronized.get", Op.Inline);

		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.Count.get", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.Contains(T)", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.CopyTo(T[], int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.IsReadOnly.get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.Add(T)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.Clear()", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.Remove(T)", Op.Import);
	}

	[TestMethod]
	public void IListInterfaceMappings_PreserveListMutationAndFixedArrayBoundaries()
	{
		AssertTypeAlias(typeof(Jazor.CLR.IListModule), "System.Collections.IList", "Array");
		AssertTypeAlias(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>", "Array");

		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.this[int].get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.Contains(object)", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.IndexOf(object)", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.this[int].set", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.Add(object)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.Clear()", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.IsReadOnly.get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.IsFixedSize.get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.Insert(int, object)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.Remove(object)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.RemoveAt(int)", Op.Import);

		AssertMemberOp(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>.this[int].get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>.IndexOf(T)", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>.this[int].set", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>.Insert(int, T)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>.RemoveAt(int)", Op.Import);
	}

	[TestMethod]
	public void IReadOnlyCollectionInterfaceMappings_ProjectReadOnlyArrayView()
	{
		AssertTypeAlias(typeof(Jazor.CLR.IReadOnlyCollectionT1Module<>), "System.Collections.Generic.IReadOnlyCollection<T>", "Array");
		AssertMemberOp(typeof(Jazor.CLR.IReadOnlyCollectionT1Module<>), "System.Collections.Generic.IReadOnlyCollection<T>.Count.get", Op.Alias);
	}

	[TestMethod]
	public void IReadOnlyListInterfaceMappings_ProjectReadOnlyIndexedArrayView()
	{
		AssertTypeAlias(typeof(Jazor.CLR.IReadOnlyListT1Module<>), "System.Collections.Generic.IReadOnlyList<T>", "Array");
		AssertMemberOp(typeof(Jazor.CLR.IReadOnlyListT1Module<>), "System.Collections.Generic.IReadOnlyList<T>.this[int].get", Op.Import);
	}

	[TestMethod]
	public void IDictionaryInterfaceMappings_OnlyKeepCarrierStableQueryMembers()
	{
		AssertTypeAlias(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>", "Map");

		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.Keys.get", Op.Inline);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.Values.get", Op.Inline);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.ContainsKey(TKey)", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].set", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.Add(TKey, TValue)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.Remove(TKey)", Op.Import);
	}

	[TestMethod]
	public void DictionaryMappings_IndexerGetUsesImportForStableThrowSemantics()
	{
		AssertTypeAlias(typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>", "Map");
		AssertMemberOp(typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEqualityComparer<TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int, System.Collections.Generic.IEqualityComparer<TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>, System.Collections.Generic.IEqualityComparer<TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.Collections.Generic.IEqualityComparer<TKey>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.Comparer.get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get", Op.Import);
	}

	[TestMethod]
	public void HashSetMappings_SupportComparerCapacityAndCollectionOperations()
	{
		AssertTypeAlias(typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>", "Set");
		AssertMemberOp(typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEqualityComparer<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.HashSet(int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEqualityComparer<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.HashSet(int, System.Collections.Generic.IEqualityComparer<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.TryGetValue(T, out T)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.CopyTo(T[])", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.CopyTo(T[], int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.CopyTo(T[], int, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.RemoveWhere(System.Predicate<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.HashSetT1Module<>), "System.Collections.Generic.HashSet<T>.Comparer.get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.HashSetT1Module<>), "static System.Collections.Generic.HashSet<T>.CreateSetComparer()", Op.Import);
	}

	[TestMethod]
	public void ConditionalWeakTableMappings_SupportFactoryBasedValueCreation()
	{
		AssertTypeAlias(typeof(Jazor.CLR.ConditionalWeakTableT2Module<,>), "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>", "WeakMap");
		AssertMemberOp(typeof(Jazor.CLR.ConditionalWeakTableT2Module<,>), "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, System.Func<TKey, TValue>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ConditionalWeakTableT2Module<,>), "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd<TArg>(TKey, System.Func<TKey, TArg, TValue>, TArg)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ConditionalWeakTableT2Module<,>), "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetValue(TKey, System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.CreateValueCallback)", Op.Import);
	}

	[TestMethod]
	public void EqualityComparerMappings_SupportDefaultAndEquals()
	{
		AssertTypeAlias(typeof(Jazor.CLR.EqualityComparerT1Module<>), "System.Collections.Generic.EqualityComparer<T>", "Object");
		AssertMemberOp(typeof(Jazor.CLR.EqualityComparerT1Module<>), "static System.Collections.Generic.EqualityComparer<T>.Default.get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EqualityComparerT1Module<>), "virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.EqualityComparerT1Module<>), "virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)", Op.Import);
	}

	[TestMethod]
	public void EqualityComparerInterfaceMappings_SupportEqualsDispatch()
	{
		AssertTypeAlias(typeof(Jazor.CLR.IEqualityComparerModule), "System.Collections.IEqualityComparer", "Object");
		AssertMemberOp(typeof(Jazor.CLR.IEqualityComparerModule), "System.Collections.IEqualityComparer.Equals(object, object)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IEqualityComparerModule), "System.Collections.IEqualityComparer.GetHashCode(object)", Op.Import);

		AssertTypeAlias(typeof(Jazor.CLR.IEqualityComparerT1Module<>), "System.Collections.Generic.IEqualityComparer<T>", "Object");
		AssertMemberOp(typeof(Jazor.CLR.IEqualityComparerT1Module<>), "System.Collections.Generic.IEqualityComparer<T>.Equals(T, T)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IEqualityComparerT1Module<>), "System.Collections.Generic.IEqualityComparer<T>.GetHashCode(T)", Op.Import);
	}

	[TestMethod]
	public void ComparerMappings_SupportDefaultAndCompare()
	{
		AssertTypeAlias(typeof(Jazor.CLR.ComparerT1Module<>), "System.Collections.Generic.Comparer<T>", "Object");
		AssertMemberOp(typeof(Jazor.CLR.ComparerT1Module<>), "static System.Collections.Generic.Comparer<T>.Default.get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ComparerT1Module<>), "virtual System.Collections.Generic.Comparer<T>.Compare(T, T)", Op.Import);
	}

	[TestMethod]
	public void ComparerInterfaceMappings_SupportCompareDispatch()
	{
		AssertTypeAlias(typeof(Jazor.CLR.IComparerModule), "System.Collections.IComparer", "Object");
		AssertMemberOp(typeof(Jazor.CLR.IComparerModule), "System.Collections.IComparer.Compare(object, object)", Op.Import);

		AssertTypeAlias(typeof(Jazor.CLR.IComparerT1Module<>), "System.Collections.Generic.IComparer<T>", "Object");
		AssertMemberOp(typeof(Jazor.CLR.IComparerT1Module<>), "System.Collections.Generic.IComparer<T>.Compare(T, T)", Op.Import);
	}

	[TestMethod]
	public void IDisposableInterfaceMappings_SupportDisposeDispatch()
	{
		AssertTypeAlias(typeof(Jazor.CLR.IDisposableModule), "System.IDisposable", "Object");
		AssertMemberOp(typeof(Jazor.CLR.IDisposableModule), "System.IDisposable.Dispose()", Op.Import);
	}

	[TestMethod]
	public void IAsyncDisposableInterfaceMappings_SupportDisposeAsyncDispatch()
	{
		AssertTypeAlias(typeof(Jazor.CLR.IAsyncDisposableModule), "System.IAsyncDisposable", "Object");
		AssertMemberOp(typeof(Jazor.CLR.IAsyncDisposableModule), "System.IAsyncDisposable.DisposeAsync()", Op.Import);
	}

	[TestMethod]
	public void StringCompareToMappings_SupportCompareDispatch()
	{
		AssertMemberOp(typeof(Jazor.CLR.StringModule), "string.CompareTo(object)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.StringModule), "string.CompareTo(string)", Op.Import);
	}

	[TestMethod]
	public void ISetInterfaceMappings_DelegateToHashSetSemantics()
	{
		var typeAttribute = typeof(Jazor.CLR.ISetT1Module<>).GetCustomAttribute<JazorAttribute>();
		Assert.IsNotNull(typeAttribute);
		Assert.AreEqual(Op.Alias, typeAttribute.Op);
		Assert.AreEqual("System.Collections.Generic.ISet<T>", typeAttribute.Member);
		Assert.AreEqual("Set", typeAttribute.Value);

		AssertMemberOp(typeof(Jazor.CLR.ISetT1Module<>), "System.Collections.Generic.ISet<T>.Add(T)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ISetT1Module<>), "System.Collections.Generic.ISet<T>.UnionWith(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ISetT1Module<>), "System.Collections.Generic.ISet<T>.IntersectWith(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ISetT1Module<>), "System.Collections.Generic.ISet<T>.ExceptWith(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ISetT1Module<>), "System.Collections.Generic.ISet<T>.SymmetricExceptWith(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ISetT1Module<>), "System.Collections.Generic.ISet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ISetT1Module<>), "System.Collections.Generic.ISet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ISetT1Module<>), "System.Collections.Generic.ISet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ISetT1Module<>), "System.Collections.Generic.ISet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ISetT1Module<>), "System.Collections.Generic.ISet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ISetT1Module<>), "System.Collections.Generic.ISet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)", Op.Import);
	}

	[TestMethod]
	public void ReadOnlySetMappings_UseLiveViewConstructor()
	{
		var typeAttribute = typeof(Jazor.CLR.ReadOnlySetT1Module<>).GetCustomAttribute<JazorAttribute>();
		Assert.IsNotNull(typeAttribute);
		Assert.AreEqual(Op.Alias, typeAttribute.Op);
		Assert.AreEqual("System.Collections.ObjectModel.ReadOnlySet<T>", typeAttribute.Member);
		Assert.AreEqual("Set", typeAttribute.Value);

		AssertMemberOp(typeof(Jazor.CLR.ReadOnlySetT1Module<>), "System.Collections.ObjectModel.ReadOnlySet<T>.ReadOnlySet(System.Collections.Generic.ISet<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlySetT1Module<>), "static System.Collections.ObjectModel.ReadOnlySet<T>.Empty.get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlySetT1Module<>), "System.Collections.ObjectModel.ReadOnlySet<T>.Contains(T)", Op.Inline);
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlySetT1Module<>), "System.Collections.ObjectModel.ReadOnlySet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlySetT1Module<>), "System.Collections.ObjectModel.ReadOnlySet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)", Op.Import);
	}

	[TestMethod]
	public void ReadOnlyCollectionMappings_KeepLiveWrapperConstructorDiscarded()
	{
		AssertTypeAlias(typeof(Jazor.CLR.ReadOnlyCollectionT1Module<>), "System.Collections.ObjectModel.ReadOnlyCollection<T>", "Array");

		AssertMemberOp(typeof(Jazor.CLR.ReadOnlyCollectionT1Module<>), "System.Collections.ObjectModel.ReadOnlyCollection<T>.ReadOnlyCollection(System.Collections.Generic.IList<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlyCollectionT1Module<>), "static System.Collections.ObjectModel.ReadOnlyCollection<T>.Empty.get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlyCollectionT1Module<>), "System.Collections.ObjectModel.ReadOnlyCollection<T>.this[int].get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlyCollectionT1Module<>), "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[])", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlyCollectionT1Module<>), "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(T[], int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlyCollectionT1Module<>), "System.Collections.ObjectModel.ReadOnlyCollection<T>.CopyTo(int, T[], int, int)", Op.Import);
	}

	[TestMethod]
	public void LiveReadOnlyWrapperFactories_UseSharedViewCarriers()
	{
		AssertMemberOp(typeof(Jazor.CLR.ArrayModule<>), "static System.Array.AsReadOnly<T>(T[])", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlyDictionaryT2Module<,>), "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)", Op.Import);
	}

	[TestMethod]
	public void ReadOnlyDictionaryMappings_ProjectEnumerableKeyAndValueSequences()
	{
		AssertTypeAlias(typeof(Jazor.CLR.ReadOnlyDictionaryT2Module<,>), "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>", "Map");
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlyDictionaryT2Module<,>), "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Keys.get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ReadOnlyDictionaryT2Module<,>), "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Values.get", Op.Import);
	}

	[TestMethod]
	public void ListMappings_UseImportForRangeSensitiveOperations()
	{
		AssertTypeAlias(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>", "Array");

		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.AsReadOnly()", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.BinarySearch(int, int, T, System.Collections.Generic.IComparer<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.BinarySearch(T)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.BinarySearch(T, System.Collections.Generic.IComparer<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.ConvertAll<TOutput>(System.Converter<T, TOutput>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.this[int].set", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.IndexOf(T, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.LastIndexOf(T, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.GetRange(int, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.InsertRange(int, System.Collections.Generic.IEnumerable<T>)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.Reverse(int, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>.Sort(int, int, System.Collections.Generic.IComparer<T>)", Op.Import);
	}

	private static void AssertMemberOp(Type type, string member, Op expectedOp)
	{
		var attribute = type
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Select(method => method.GetCustomAttribute<JazorAttribute>())
			.OfType<JazorAttribute>()
			.Single(attribute => attribute.Member == member);

		Assert.AreEqual(expectedOp, attribute.Op, member);
	}

	private static void AssertTypeAlias(Type type, string member, string expectedAlias)
	{
		var attribute = type.GetCustomAttribute<JazorAttribute>();
		Assert.IsNotNull(attribute);
		Assert.AreEqual(Op.Alias, attribute.Op);
		Assert.AreEqual(member, attribute.Member);
		Assert.AreEqual(expectedAlias, attribute.Value);
	}
}
