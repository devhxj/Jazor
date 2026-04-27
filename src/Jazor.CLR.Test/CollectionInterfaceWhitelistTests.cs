using System.Reflection;
using Jazor.Common;

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
	public void IDictionaryInterfaceMappings_OnlyKeepCarrierStableQueryMembers()
	{
		AssertTypeAlias(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>", "Map");

		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].get", Op.Inline);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.Keys.get", Op.Inline);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.Values.get", Op.Inline);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.ContainsKey(TKey)", Op.Alias);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", Op.Import);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].set", Op.Inline);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.Add(TKey, TValue)", Op.Discard);
		AssertMemberOp(typeof(Jazor.CLR.IDictionaryT2Module<,>), "System.Collections.Generic.IDictionary<TKey, TValue>.Remove(TKey)", Op.Discard);
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
