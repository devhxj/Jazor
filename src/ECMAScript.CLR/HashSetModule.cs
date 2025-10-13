using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Collections.Generic.HashSet<T>")]
public static class HashSetModule
{
    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the default equality comparer for the set type.      </summary>    [WhiteList("System.Collections.Generic.HashSet<T>.HashSet()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Set<T> HashSetNew<T>();

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the specified equality comparer for the set type.      </summary>
    ///<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> implementation for the set type.      </param>    [WhiteList("System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEqualityComparer<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Set<T> HashSetNew2<T>(System.Collections.Generic.IEqualityComparer<T>? comparer);

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty, but has reserved space for <paramref name="capacity" /> items and uses the default equality comparer for the set type.      </summary>
    ///<param name="capacity">        The initial size of the <see cref="T:System.Collections.Generic.HashSet`1" />.      </param>    [WhiteList("System.Collections.Generic.HashSet<T>.HashSet(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Set<T> HashSetNew3<T>(Number capacity);

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the default equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.      </summary>
    ///<param name="collection">The collection whose elements are copied to the new set.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>    [WhiteList("System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Set<T> HashSetNew4<T>(IEnumerable<T> collection);

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.      </summary>
    ///<param name="collection">The collection whose elements are copied to the new set.</param>
    ///<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> implementation for the set type.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>    [WhiteList("System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEqualityComparer<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Set<T> HashSetNew5<T>(IEnumerable<T> collection, System.Collections.Generic.IEqualityComparer<T>? comparer);

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, and has sufficient capacity to accommodate <paramref name="capacity" /> elements.      </summary>
    ///<param name="capacity">        The initial size of the <see cref="T:System.Collections.Generic.HashSet`1" />.      </param>
    ///<param name="comparer">        The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or null (Nothing in Visual Basic) to use the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation for the set type.      </param>    [WhiteList("System.Collections.Generic.HashSet<T>.HashSet(int, System.Collections.Generic.IEqualityComparer<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Set<T> HashSetNew6<T>(Number capacity, System.Collections.Generic.IEqualityComparer<T>? comparer);

    ///<summary>        Removes all elements from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>    [WhiteList("System.Collections.Generic.HashSet<T>.Clear()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetClear<T>(Set<T> instance);

    ///<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object contains the specified element.      </summary>
    ///<param name="item">        The element to locate in the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object contains the specified element; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.Contains(T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool HashSetContains<T>(Set<T> instance, T item);

    ///<summary>        Removes the specified element from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>
    ///<param name="item">The element to remove.</param>
    ///<returns>  <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.  This method returns <see langword="false" /> if <paramref name="item" /> is not found in the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.Remove(T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool HashSetRemove<T>(Set<T> instance, T item);

    [WhiteList("System.Collections.Generic.HashSet<T>.Count.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number HashSetGetCount<T>(Set<T> instance);

    [WhiteList("System.Collections.Generic.HashSet<T>.Capacity.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number HashSetGetCapacity<T>(Set<T> instance);

    ///<summary>        Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.HashSet`1" /> using a <typeparamref name="TAlternate" /> instead of a <typeparamref name="T" />.      </summary>
    ///<typeparam name="TAlternate">The alternate type of instance for performing lookups.</typeparam>
    ///<returns>The created lookup instance.</returns>    [WhiteList("System.Collections.Generic.HashSet<T>.GetAlternateLookup<TAlternate>()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.HashSet<T>.AlternateLookup<TAlternate> HashSetGetAlternateLookup<T, TAlternate>(Set<T> instance);

    ///<summary>        Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.HashSet`1" /> using a <typeparamref name="TAlternate" /> instead of a <typeparamref name="T" />.      </summary>
    ///<param name="lookup">The created lookup instance when the method returns true, or a default instance that should not be used if the method returns false.</param>
    ///<typeparam name="TAlternate">The alternate type of instance for performing lookups.</typeparam>
    ///<returns>  <see langword="true" /> if a lookup could be created; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.TryGetAlternateLookup<TAlternate>(out System.Collections.Generic.HashSet<T>.AlternateLookup<TAlternate>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool HashSetTryGetAlternateLookup<T, TAlternate>(Set<T> instance, OutValue<System.Collections.Generic.HashSet<T>.AlternateLookup<TAlternate>> lookup);

    ///<summary>        Returns an enumerator that iterates through a <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>
    ///<returns>        A <see cref="T:System.Collections.Generic.HashSet`1.Enumerator" /> object for the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.GetEnumerator()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.HashSet<T>.Enumerator HashSetGetEnumerator<T>(Set<T> instance);

    ///<summary>        Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize a <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>
    ///<param name="info">        A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<param name="context">        A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="info" /> is <see langword="null" />.      </exception>    [WhiteList("virtual System.Collections.Generic.HashSet<T>.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetGetObjectData<T>(Set<T> instance, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context);

    ///<summary>        Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.      </summary>
    ///<param name="sender">The source of the deserialization event.</param>
    ///<exception cref="T:System.Runtime.Serialization.SerializationException">        The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Generic.HashSet`1" /> object is invalid.      </exception>    [WhiteList("virtual System.Collections.Generic.HashSet<T>.OnDeserialization(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetOnDeserialization<T>(Set<T> instance, Object? sender);

    ///<summary>Adds the specified element to a set.</summary>
    ///<param name="item">The element to add to the set.</param>
    ///<returns>  <see langword="true" /> if the element is added to the <see cref="T:System.Collections.Generic.HashSet`1" /> object; <see langword="false" /> if the element is already present.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.Add(T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool HashSetAdd<T>(Set<T> instance, T item);

    ///<summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
    ///<param name="equalValue">The value to search for.</param>
    ///<param name="actualValue">The value from the set that the search found, or the default value of T when the search yielded no match.</param>
    ///<returns>A value indicating whether the search was successful.</returns>    [WhiteList("System.Collections.Generic.HashSet<T>.TryGetValue(T, out T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool HashSetTryGetValue<T>(Set<T> instance, T equalValue, OutValue<T> actualValue);

    ///<summary>        Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain all elements that are present in itself, the specified collection, or both.      </summary>
    ///<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>    [WhiteList("System.Collections.Generic.HashSet<T>.UnionWith(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetUnionWith<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>        Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present in that object and in the specified collection.      </summary>
    ///<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>    [WhiteList("System.Collections.Generic.HashSet<T>.IntersectWith(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetIntersectWith<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>        Removes all elements in the specified collection from the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>
    ///<param name="other">        The collection of items to remove from the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>    [WhiteList("System.Collections.Generic.HashSet<T>.ExceptWith(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetExceptWith<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>        Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present either in that object or in the specified collection, but not both.      </summary>
    ///<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>    [WhiteList("System.Collections.Generic.HashSet<T>.SymmetricExceptWith(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetSymmetricExceptWith<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a subset of the specified collection.      </summary>
    ///<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a subset of <paramref name="other" />; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool HashSetIsSubsetOf<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper subset of the specified collection.      </summary>
    ///<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper subset of <paramref name="other" />; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool HashSetIsProperSubsetOf<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a superset of the specified collection.      </summary>
    ///<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a superset of <paramref name="other" />; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool HashSetIsSupersetOf<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper superset of the specified collection.      </summary>
    ///<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper superset of <paramref name="other" />; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool HashSetIsProperSupersetOf<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>        Determines whether the current <see cref="T:System.Collections.Generic.HashSet`1" /> object and a specified collection share common elements.      </summary>
    ///<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object and <paramref name="other" /> share at least one common element; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool HashSetOverlaps<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>        Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object and the specified collection contain the same elements.      </summary>
    ///<param name="other">        The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="other" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is equal to <paramref name="other" />; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool HashSetSetEquals<T>(Set<T> instance, IEnumerable<T> other);

    ///<summary>        Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array.      </summary>
    ///<param name="array">        The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.      </exception>    [WhiteList("System.Collections.Generic.HashSet<T>.CopyTo(T[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetCopyTo<T>(Set<T> instance, Array<T> array);

    ///<summary>        Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.      </summary>
    ///<param name="array">        The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.      </param>
    ///<param name="arrayIndex">        The zero-based index in <paramref name="array" /> at which copying begins.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="arrayIndex" /> is less than 0.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="arrayIndex" /> is greater than the length of the destination <paramref name="array" />.      </exception>    [WhiteList("System.Collections.Generic.HashSet<T>.CopyTo(T[], int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetCopyTo2<T>(Set<T> instance, Array<T> array, Number arrayIndex);

    ///<summary>        Copies the specified number of elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.      </summary>
    ///<param name="array">        The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.      </param>
    ///<param name="arrayIndex">        The zero-based index in <paramref name="array" /> at which copying begins.      </param>
    ///<param name="count">        The number of elements to copy to <paramref name="array" />.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="arrayIndex" /> is less than 0.        -or-        <paramref name="count" /> is less than 0.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="arrayIndex" /> is greater than the length of the destination <paramref name="array" />.        -or-        <paramref name="count" /> is greater than the available space from the <paramref name="index" /> to the end of the destination <paramref name="array" />.      </exception>    [WhiteList("System.Collections.Generic.HashSet<T>.CopyTo(T[], int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetCopyTo3<T>(Set<T> instance, Array<T> array, Number arrayIndex, Number count);

    ///<summary>        Removes all elements that match the conditions defined by the specified predicate from a <see cref="T:System.Collections.Generic.HashSet`1" /> collection.      </summary>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to remove.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<returns>        The number of elements that were removed from the <see cref="T:System.Collections.Generic.HashSet`1" /> collection.      </returns>    [WhiteList("System.Collections.Generic.HashSet<T>.RemoveWhere(System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number HashSetRemoveWhere<T>(Set<T> instance, Predicate<T> match);

    [WhiteList("System.Collections.Generic.HashSet<T>.Comparer.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.IEqualityComparer<T> HashSetGetComparer<T>(Set<T> instance);

    ///<summary>Ensures that this hash set can hold the specified number of elements without any further expansion of its backing storage.</summary>
    ///<param name="capacity">The minimum capacity to ensure.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than zero.      </exception>
    ///<returns>The new capacity of this instance.</returns>    [WhiteList("System.Collections.Generic.HashSet<T>.EnsureCapacity(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number HashSetEnsureCapacity<T>(Set<T> instance, Number capacity);

    ///<summary>        Sets the capacity of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to the actual number of elements it contains, rounded up to a nearby, implementation-specific value.      </summary>    [WhiteList("System.Collections.Generic.HashSet<T>.TrimExcess()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetTrimExcess<T>(Set<T> instance);

    ///<summary>        Sets the capacity of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to the specified number of entries, rounded up to a nearby, implementation-specific value.      </summary>
    ///<param name="capacity">The new capacity.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The specified capacity is lower than the count of entries.</exception>    [WhiteList("System.Collections.Generic.HashSet<T>.TrimExcess(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void HashSetTrimExcess2<T>(Set<T> instance, Number capacity);

    ///<summary>        Returns an <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for equality testing of a <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </summary>
    ///<returns>        An <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for deep equality testing of the <see cref="T:System.Collections.Generic.HashSet`1" /> object.      </returns>    [WhiteList("static System.Collections.Generic.HashSet<T>.CreateSetComparer()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.IEqualityComparer<System.Collections.Generic.HashSet<T>> HashSetCreateSetComparer<T>();
}
