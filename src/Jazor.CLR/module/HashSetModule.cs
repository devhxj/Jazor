namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Collections.Generic.HashSet<T>","System/Collections/Generic/HashSetModule.js")]
public static class HashSetModule<T>
{
	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the default equality comparer for the set type.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.HashSet()")]
	public extern static Set<T> _55c044d94c5b0ca8();

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the specified equality comparer for the set type.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEqualityComparer<T>)")]
	public extern static Set<T> _3a131c59650baae9(object comparer);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty, but has reserved space for <paramref name="capacity" /> items and uses the default equality comparer for the set type.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.HashSet(int)")]
	public extern static Set<T> _304904fb5a22f950(Number capacity);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the default equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>)")]
	public extern static Set<T> _1bd2e054852d9d5f(Array<T> collection);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEqualityComparer<T>)")]
	public extern static Set<T> _fe5bb664d9f9c877(Array<T> collection, object comparer);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, and has sufficient capacity to accommodate <paramref name="capacity" /> elements.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.HashSet(int, System.Collections.Generic.IEqualityComparer<T>)")]
	public extern static Set<T> _baf729bee477b2e7(Number capacity, object comparer);

	///<summary>Removes all elements from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.Clear()")]
	public extern static void _56d632bf48c92530(Set<T> instance);

	///<summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object contains the specified element.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.Contains(T)")]
	public extern static bool _32b989c96ea23e8c(Set<T> instance, object item);

	///<summary>Removes the specified element from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.Remove(T)")]
	public extern static bool _cfb963650cb3dabd(Set<T> instance, object item);

	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.Count.get")]
	public extern static Number _4bec0b4d27073edb(Set<T> instance);

	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.Capacity.get")]
	public extern static Number _97c019008a0c8260(Set<T> instance);

	///<summary>Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.HashSet`1" /> using a <typeparamref name="TAlternate" /> instead of a <typeparamref name="T" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.GetAlternateLookup<TAlternate>()")]
	public extern static System.Collections.Generic.HashSet<T>.AlternateLookup<TAlternate> _3ed41a9b4870a040<TAlternate>(Set<T> instance);

	///<summary>Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.HashSet`1" /> using a <typeparamref name="TAlternate" /> instead of a <typeparamref name="T" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.TryGetAlternateLookup<TAlternate>(out System.Collections.Generic.HashSet<T>.AlternateLookup<TAlternate>)")]
	public extern static Array<object?> _859aac4462f2d063<TAlternate>(Set<T> instance, object lookup);

	///<summary>Returns an enumerator that iterates through a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.GetEnumerator()")]
	public extern static System.Collections.Generic.HashSet<T>.Enumerator _68a59c6ba9ebe57d(Set<T> instance);

	///<summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	[Jazor(Op.Discard ,"virtual System.Collections.Generic.HashSet<T>.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)")]
	public extern static void _8f2db3c5ff390af9(Set<T> instance, object info, object context);

	///<summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.</summary>
	[Jazor(Op.Discard ,"virtual System.Collections.Generic.HashSet<T>.OnDeserialization(object)")]
	public extern static void _26975bd136a2f896(Set<T> instance, object? sender);

	///<summary>Adds the specified element to a set.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.Add(T)")]
	public extern static bool _e1d2ba750a2788cb(Set<T> instance, object item);

	///<summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.TryGetValue(T, out T)")]
	public extern static Array<object?> _20eb460b32c63404(Set<T> instance, object equalValue, object actualValue);

	///<summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain all elements that are present in itself, the specified collection, or both.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.UnionWith(System.Collections.Generic.IEnumerable<T>)")]
	public extern static void _b2bd5d22aadd44a8(Set<T> instance, Array<T> other);

	///<summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present in that object and in the specified collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.IntersectWith(System.Collections.Generic.IEnumerable<T>)")]
	public extern static void _3a6a072035334578(Set<T> instance, Array<T> other);

	///<summary>Removes all elements in the specified collection from the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.ExceptWith(System.Collections.Generic.IEnumerable<T>)")]
	public extern static void _373e2e9ed1fb3f5b(Set<T> instance, Array<T> other);

	///<summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present either in that object or in the specified collection, but not both.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.SymmetricExceptWith(System.Collections.Generic.IEnumerable<T>)")]
	public extern static void _a22fe44dc0ae9ad2(Set<T> instance, Array<T> other);

	///<summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a subset of the specified collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _23c8bcfc6b71d2b1(Set<T> instance, Array<T> other);

	///<summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper subset of the specified collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _fb8566ae66aa9591(Set<T> instance, Array<T> other);

	///<summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a superset of the specified collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _3be7fbb1d68799fb(Set<T> instance, Array<T> other);

	///<summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper superset of the specified collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _cc0cc2d0f5be70db(Set<T> instance, Array<T> other);

	///<summary>Determines whether the current <see cref="T:System.Collections.Generic.HashSet`1" /> object and a specified collection share common elements.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _84709aa8ff70a52a(Set<T> instance, Array<T> other);

	///<summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object and the specified collection contain the same elements.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)")]
	public extern static bool _55425d259e5f54ea(Set<T> instance, Array<T> other);

	///<summary>Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.CopyTo(T[])")]
	public extern static void _614185e6ff9ff9fd(Set<T> instance, Array<T> array);

	///<summary>Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.CopyTo(T[], int)")]
	public extern static void _9ac2dfb153a1d53c(Set<T> instance, Array<T> array, Number arrayIndex);

	///<summary>Copies the specified number of elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.CopyTo(T[], int, int)")]
	public extern static void _622a881b75871c97(Set<T> instance, Array<T> array, Number arrayIndex, Number count);

	///<summary>Removes all elements that match the conditions defined by the specified predicate from a <see cref="T:System.Collections.Generic.HashSet`1" /> collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.RemoveWhere(System.Predicate<T>)")]
	public extern static Number _112079825eb01119(Set<T> instance, Predicate<T> match);

	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.Comparer.get")]
	public extern static System.Collections.Generic.IEqualityComparer<T> _0c0d81e2205a9cb9(Set<T> instance);

	///<summary>Ensures that this hash set can hold the specified number of elements without any further expansion of its backing storage.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.EnsureCapacity(int)")]
	public extern static Number _b53dcd5d4f0c57d7(Set<T> instance, Number capacity);

	///<summary>Sets the capacity of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to the actual number of elements it contains, rounded up to a nearby, implementation-specific value.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.TrimExcess()")]
	public extern static void _09f9b6aba126decb(Set<T> instance);

	///<summary>Sets the capacity of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to the specified number of entries, rounded up to a nearby, implementation-specific value.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.TrimExcess(int)")]
	public extern static void _e4dd8faf507013ad(Set<T> instance, Number capacity);

	///<summary>Returns an <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for equality testing of a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	[Jazor(Op.Discard ,"static System.Collections.Generic.HashSet<T>.CreateSetComparer()")]
	public extern static System.Collections.Generic.IEqualityComparer<System.Collections.Generic.HashSet<T>> _2d028c1bc3e2f479();
}
