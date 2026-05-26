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
	public void ICollectionInterfaceMappings_OnlyKeepSafeQueryMembers()
	{
		AssertTypeAlias(typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection", "Array");
		AssertTypeAlias(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>", "Array");

		AssertMemberOp(typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection.Count.get", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection.CopyTo(System.Array, int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection.SyncRoot.get", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionModule), "System.Collections.ICollection.IsSynchronized.get", Op.Discard);

		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.Count.get", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.Contains(T)", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.CopyTo(T[], int)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.IsReadOnly.get", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.Add(T)", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.Clear()", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.ICollectionT1Module<>), "System.Collections.Generic.ICollection<T>.Remove(T)", Op.Discard);
	}

	[TestMethod]
	public void IListInterfaceMappings_RejectAmbiguousMutationMembers()
	{
		AssertTypeAlias(typeof(Jazor.CLR.IListModule), "System.Collections.IList", "Array");
		AssertTypeAlias(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>", "Array");

		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.this[int].get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.Contains(object)", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.IndexOf(object)", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.this[int].set", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.Clear()", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.IsReadOnly.get", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.IsFixedSize.get", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.Insert(int, object)", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.Remove(object)", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.IListModule), "System.Collections.IList.RemoveAt(int)", Op.Discard);

		AssertMemberOp(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>.this[int].get", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>.IndexOf(T)", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>.this[int].set", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>.Insert(int, T)", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.IListT1Module<>), "System.Collections.Generic.IList<T>.RemoveAt(int)", Op.Discard);
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
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.Add(TKey, TValue)", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.Remove(TKey)", Op.Discard);
	}

	[TestMethod]
	public void DictionaryMappings_IndexerGetUsesImportForStableThrowSemantics()
	{
		AssertTypeAlias(typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>", "Map");
		AssertMemberOp(typeof(Jazor.CLR.DictionaryT2Module<,>), "System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get", Op.Import);
	}

	[TestMethod]
	public void EqualityComparerMappings_SupportDefaultAndEquals()
	{
		AssertTypeAlias(typeof(Jazor.CLR.EqualityComparerT1Module<>), "System.Collections.Generic.EqualityComparer<T>", "Object");
		AssertMemberOp(typeof(Jazor.CLR.EqualityComparerT1Module<>), "static System.Collections.Generic.EqualityComparer<T>.Default.get", Op.Inline);
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
		AssertMemberOp(typeof(Jazor.CLR.ComparerT1Module<>), "static System.Collections.Generic.Comparer<T>.Default.get", Op.Inline);
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
	public void ReadOnlySetMappings_UseImportForReadOnlyCarrierSemantics()
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
	public void ReadOnlyCollectionMappings_UseImportForReadOnlyCarrierAndCopySemantics()
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
	public void ListMappings_UseImportForRangeSensitiveOperations()
	{
		AssertTypeAlias(typeof(Jazor.CLR.ListT1Module<>), "System.Collections.Generic.List<T>", "Array");

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
