using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Collections.Generic.List<T>")]
public static class ListModule
{
    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has the default initial capacity.      </summary>    [WhiteList("System.Collections.Generic.List<T>.List()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Array<T> ListNew<T>();

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has the specified initial capacity.      </summary>
    ///<param name="capacity">The number of elements that the new list can initially store.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="capacity" /> is less than 0.      </exception>    [WhiteList("System.Collections.Generic.List<T>.List(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Array<T> ListNew2<T>(Number capacity);

    ///<summary>        Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.      </summary>
    ///<param name="collection">The collection whose elements are copied to the new list.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>    [WhiteList("System.Collections.Generic.List<T>.List(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Array<T> ListNew3<T>(IEnumerable<T> collection);

    [WhiteList("System.Collections.Generic.List<T>.Capacity.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListGetCapacity<T>(Array<T> instance);

    [WhiteList("System.Collections.Generic.List<T>.Capacity.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListSetCapacity<T>(Array<T> instance, Number value);

    [WhiteList("System.Collections.Generic.List<T>.Count.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListGetCount<T>(Array<T> instance);

    [WhiteList("System.Collections.Generic.List<T>.this[int].get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static T ListThis<T>(Array<T> instance, Number index);

    [WhiteList("System.Collections.Generic.List<T>.this[int].set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListThis<T>(Array<T> instance, Number index, T value);

    ///<summary>        Adds an object to the end of the <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="item">        The object to be added to the end of the <see cref="T:System.Collections.Generic.List`1" />. The value can be <see langword="null" /> for reference types.      </param>    [WhiteList("System.Collections.Generic.List<T>.Add(T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListAdd<T>(Array<T> instance, T item);

    ///<summary>        Adds the elements of the specified collection to the end of the <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="collection">        The collection whose elements should be added to the end of the <see cref="T:System.Collections.Generic.List`1" />. The collection itself cannot be <see langword="null" />, but it can contain elements that are <see langword="null" />, if type <paramref name="T" /> is a reference type.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>    [WhiteList("System.Collections.Generic.List<T>.AddRange(System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListAddRange<T>(Array<T> instance, IEnumerable<T> collection);

    ///<summary>        Returns a read-only <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> wrapper for the current collection.      </summary>
    ///<returns>        An object that acts as a read-only wrapper around the current <see cref="T:System.Collections.Generic.List`1" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.AsReadOnly()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Array<T> ListAsReadOnly<T>(Array<T> instance);

    ///<summary>        Searches a range of elements in the sorted <see cref="T:System.Collections.Generic.List`1" /> for an element using the specified comparer and returns the zero-based index of the element.      </summary>
    ///<param name="index">The zero-based starting index of the range to search.</param>
    ///<param name="count">The length of the range to search.</param>
    ///<param name="item">        The object to locate. The value can be <see langword="null" /> for reference types.      </param>
    ///<param name="comparer">        The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements, or <see langword="null" /> to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" />.      </param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than 0.        -or-        <paramref name="count" /> is less than 0.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="count" /> do not denote a valid range in the <see cref="T:System.Collections.Generic.List`1" />.      </exception>
    ///<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find an implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.      </exception>
    ///<returns>        The zero-based index of <paramref name="item" /> in the sorted <see cref="T:System.Collections.Generic.List`1" />, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.List`1.Count" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.BinarySearch(int, int, T, System.Collections.Generic.IComparer<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListBinarySearch<T>(Array<T> instance, Number index, Number count, T item, IComparer<T>? comparer);

    ///<summary>        Searches the entire sorted <see cref="T:System.Collections.Generic.List`1" /> for an element using the default comparer and returns the zero-based index of the element.      </summary>
    ///<param name="item">        The object to locate. The value can be <see langword="null" /> for reference types.      </param>
    ///<exception cref="T:System.InvalidOperationException">        The default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find an implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.      </exception>
    ///<returns>        The zero-based index of <paramref name="item" /> in the sorted <see cref="T:System.Collections.Generic.List`1" />, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.List`1.Count" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.BinarySearch(T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListBinarySearch2<T>(Array<T> instance, T item);

    ///<summary>        Searches the entire sorted <see cref="T:System.Collections.Generic.List`1" /> for an element using the specified comparer and returns the zero-based index of the element.      </summary>
    ///<param name="item">        The object to locate. The value can be <see langword="null" /> for reference types.      </param>
    ///<param name="comparer">        The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.        -or-        <see langword="null" /> to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" />.      </param>
    ///<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find an implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.      </exception>
    ///<returns>        The zero-based index of <paramref name="item" /> in the sorted <see cref="T:System.Collections.Generic.List`1" />, if <paramref name="item" /> is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than <paramref name="item" /> or, if there is no larger element, the bitwise complement of <see cref="P:System.Collections.Generic.List`1.Count" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.BinarySearch(T, System.Collections.Generic.IComparer<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListBinarySearch3<T>(Array<T> instance, T item, IComparer<T>? comparer);

    ///<summary>        Removes all elements from the <see cref="T:System.Collections.Generic.List`1" />.      </summary>    [WhiteList("System.Collections.Generic.List<T>.Clear()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListClear<T>(Array<T> instance);

    ///<summary>        Determines whether an element is in the <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="item">        The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be <see langword="null" /> for reference types.      </param>
    ///<returns>  <see langword="true" /> if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.List`1" />; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.Contains(T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ListContains<T>(Array<T> instance, T item);

    ///<summary>        Converts the elements in the current <see cref="T:System.Collections.Generic.List`1" /> to another type, and returns a list containing the converted elements.      </summary>
    ///<param name="converter">        A <see cref="T:System.Converter`2" /> delegate that converts each element from one type to another type.      </param>
    ///<typeparam name="TOutput">The type of the elements of the target array.</typeparam>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="converter" /> is <see langword="null" />.      </exception>
    ///<returns>        A <see cref="T:System.Collections.Generic.List`1" /> of the target type containing the converted elements from the current <see cref="T:System.Collections.Generic.List`1" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.ConvertAll<TOutput>(System.Converter<T, TOutput>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.List<TOutput> ListConvertAll<T, TOutput>(Array<T> instance, System.Converter<T, TOutput> converter);

    ///<summary>        Copies the entire <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the beginning of the target array.      </summary>
    ///<param name="array">        The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.List`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentException">        The number of elements in the source <see cref="T:System.Collections.Generic.List`1" /> is greater than the number of elements that the destination <paramref name="array" /> can contain.      </exception>    [WhiteList("System.Collections.Generic.List<T>.CopyTo(T[])")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListCopyTo<T>(Array<T> instance, Array<T> array);

    ///<summary>        Copies a range of elements from the <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the specified index of the target array.      </summary>
    ///<param name="index">        The zero-based index in the source <see cref="T:System.Collections.Generic.List`1" /> at which copying begins.      </param>
    ///<param name="array">        The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.List`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.      </param>
    ///<param name="arrayIndex">        The zero-based index in <paramref name="array" /> at which copying begins.      </param>
    ///<param name="count">The number of elements to copy.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than 0.        -or-        <paramref name="arrayIndex" /> is less than 0.        -or-        <paramref name="count" /> is less than 0.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="index" /> is equal to or greater than the <see cref="P:System.Collections.Generic.List`1.Count" /> of the source <see cref="T:System.Collections.Generic.List`1" />.        -or-        The number of elements from <paramref name="index" /> to the end of the source <see cref="T:System.Collections.Generic.List`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.      </exception>    [WhiteList("System.Collections.Generic.List<T>.CopyTo(int, T[], int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListCopyTo2<T>(Array<T> instance, Number index, Array<T> array, Number arrayIndex, Number count);

    ///<summary>        Copies the entire <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the specified index of the target array.      </summary>
    ///<param name="array">        The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.List`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.      </param>
    ///<param name="arrayIndex">        The zero-based index in <paramref name="array" /> at which copying begins.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="array" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="arrayIndex" /> is less than 0.      </exception>
    ///<exception cref="T:System.ArgumentException">        The number of elements in the source <see cref="T:System.Collections.Generic.List`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.      </exception>    [WhiteList("System.Collections.Generic.List<T>.CopyTo(T[], int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListCopyTo3<T>(Array<T> instance, Array<T> array, Number arrayIndex);

    ///<summary>        Ensures that the capacity of this list is at least the specified <paramref name="capacity" />. If the current capacity is less than <paramref name="capacity" />, it is increased to at least the specified <paramref name="capacity" />.      </summary>
    ///<param name="capacity">The minimum capacity to ensure.</param>
    ///<returns>The new capacity of this list.</returns>    [WhiteList("System.Collections.Generic.List<T>.EnsureCapacity(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListEnsureCapacity<T>(Array<T> instance, Number capacity);

    ///<summary>        Determines whether the <see cref="T:System.Collections.Generic.List`1" /> contains elements that match the conditions defined by the specified predicate.      </summary>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to search for.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if the <see cref="T:System.Collections.Generic.List`1" /> contains one or more elements that match the conditions defined by the specified predicate; otherwise, <see langword="false" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.Exists(System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ListExists<T>(Array<T> instance, Predicate<T> match);

    ///<summary>        Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<returns>        The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.Find(System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static T? ListFind<T>(Array<T> instance, Predicate<T> match);

    ///<summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to search for.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<returns>        A <see cref="T:System.Collections.Generic.List`1" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="T:System.Collections.Generic.List`1" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.FindAll(System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.List<T> ListFindAll<T>(Array<T> instance, Predicate<T> match);

    ///<summary>        Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<returns>        The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.FindIndex(System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListFindIndex<T>(Array<T> instance, Predicate<T> match);

    ///<summary>        Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the specified index to the last element.      </summary>
    ///<param name="startIndex">The zero-based starting index of the search.</param>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.      </exception>
    ///<returns>        The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.FindIndex(int, System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListFindIndex2<T>(Array<T> instance, Number startIndex, Predicate<T> match);

    ///<summary>        Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that starts at the specified index and contains the specified number of elements.      </summary>
    ///<param name="startIndex">The zero-based starting index of the search.</param>
    ///<param name="count">The number of elements in the section to search.</param>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.        -or-        <paramref name="count" /> is less than 0.        -or-        <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in the <see cref="T:System.Collections.Generic.List`1" />.      </exception>
    ///<returns>        The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.FindIndex(int, int, System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListFindIndex3<T>(Array<T> instance, Number startIndex, Number count, Predicate<T> match);

    ///<summary>        Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<returns>        The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.FindLast(System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static T? ListFindLast<T>(Array<T> instance, Predicate<T> match);

    ///<summary>        Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<returns>        The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.FindLastIndex(System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListFindLastIndex<T>(Array<T> instance, Predicate<T> match);

    ///<summary>        Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the first element to the specified index.      </summary>
    ///<param name="startIndex">The zero-based starting index of the backward search.</param>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.      </exception>
    ///<returns>        The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.FindLastIndex(int, System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListFindLastIndex2<T>(Array<T> instance, Number startIndex, Predicate<T> match);

    ///<summary>        Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that contains the specified number of elements and ends at the specified index.      </summary>
    ///<param name="startIndex">The zero-based starting index of the backward search.</param>
    ///<param name="count">The number of elements in the section to search.</param>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the element to search for.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="startIndex" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.        -or-        <paramref name="count" /> is less than 0.        -or-        <paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in the <see cref="T:System.Collections.Generic.List`1" />.      </exception>
    ///<returns>        The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.FindLastIndex(int, int, System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListFindLastIndex3<T>(Array<T> instance, Number startIndex, Number count, Predicate<T> match);

    ///<summary>        Performs the specified action on each element of the <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="action">        The <see cref="T:System.Action`1" /> delegate to perform on each element of the <see cref="T:System.Collections.Generic.List`1" />.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="action" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.InvalidOperationException">An element in the collection has been modified.</exception>    [WhiteList("System.Collections.Generic.List<T>.ForEach(System.Action<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListForEach<T>(Array<T> instance, System.Action<T> action);

    ///<summary>        Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<returns>        A <see cref="T:System.Collections.Generic.List`1.Enumerator" /> for the <see cref="T:System.Collections.Generic.List`1" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.GetEnumerator()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.List<T>.Enumerator ListGetEnumerator<T>(Array<T> instance);

    ///<summary>        Creates a shallow copy of a range of elements in the source <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="index">        The zero-based <see cref="T:System.Collections.Generic.List`1" /> index at which the range starts.      </param>
    ///<param name="count">The number of elements in the range.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than 0.        -or-        <paramref name="count" /> is less than 0.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the <see cref="T:System.Collections.Generic.List`1" />.      </exception>
    ///<returns>        A shallow copy of a range of elements in the source <see cref="T:System.Collections.Generic.List`1" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.GetRange(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.List<T> ListGetRange<T>(Array<T> instance, Number index, Number count);

    ///<summary>        Creates a shallow copy of a range of elements in the source <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="start">        The zero-based <see cref="T:System.Collections.Generic.List`1" /> index at which the range starts.      </param>
    ///<param name="length">The length of the range.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="start" /> is less than 0.        -or-        <paramref name="length" /> is less than 0.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="start" /> and <paramref name="length" /> do not denote a valid range of elements in the <see cref="T:System.Collections.Generic.List`1" />.      </exception>
    ///<returns>        A shallow copy of a range of elements in the source <see cref="T:System.Collections.Generic.List`1" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.Slice(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Collections.Generic.List<T> ListSlice<T>(Array<T> instance, Number start, Number length);

    ///<summary>        Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="item">        The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be <see langword="null" /> for reference types.      </param>
    ///<returns>        The zero-based index of the first occurrence of <paramref name="item" /> within the entire <see cref="T:System.Collections.Generic.List`1" />, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.IndexOf(T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListIndexOf<T>(Array<T> instance, T item);

    ///<summary>        Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the specified index to the last element.      </summary>
    ///<param name="item">        The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be <see langword="null" /> for reference types.      </param>
    ///<param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.      </exception>
    ///<returns>        The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from <paramref name="index" /> to the last element, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.IndexOf(T, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListIndexOf2<T>(Array<T> instance, T item, Number index);

    ///<summary>        Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that starts at the specified index and contains the specified number of elements.      </summary>
    ///<param name="item">        The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be <see langword="null" /> for reference types.      </param>
    ///<param name="index">The zero-based starting index of the search. 0 (zero) is valid in an empty list.</param>
    ///<param name="count">The number of elements in the section to search.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.        -or-        <paramref name="count" /> is less than 0.        -or-        <paramref name="index" /> and <paramref name="count" /> do not specify a valid section in the <see cref="T:System.Collections.Generic.List`1" />.      </exception>
    ///<returns>        The zero-based index of the first occurrence of <paramref name="item" /> within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that starts at <paramref name="index" /> and contains <paramref name="count" /> number of elements, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.IndexOf(T, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListIndexOf3<T>(Array<T> instance, T item, Number index, Number count);

    ///<summary>        Inserts an element into the <see cref="T:System.Collections.Generic.List`1" /> at the specified index.      </summary>
    ///<param name="index">        The zero-based index at which <paramref name="item" /> should be inserted.      </param>
    ///<param name="item">        The object to insert. The value can be <see langword="null" /> for reference types.      </param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than 0.        -or-        <paramref name="index" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.      </exception>    [WhiteList("System.Collections.Generic.List<T>.Insert(int, T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListInsert<T>(Array<T> instance, Number index, T item);

    ///<summary>        Inserts the elements of a collection into the <see cref="T:System.Collections.Generic.List`1" /> at the specified index.      </summary>
    ///<param name="index">The zero-based index at which the new elements should be inserted.</param>
    ///<param name="collection">        The collection whose elements should be inserted into the <see cref="T:System.Collections.Generic.List`1" />. The collection itself cannot be <see langword="null" />, but it can contain elements that are <see langword="null" />, if type <paramref name="T" /> is a reference type.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="collection" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than 0.        -or-        <paramref name="index" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.      </exception>    [WhiteList("System.Collections.Generic.List<T>.InsertRange(int, System.Collections.Generic.IEnumerable<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListInsertRange<T>(Array<T> instance, Number index, IEnumerable<T> collection);

    ///<summary>        Searches for the specified object and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="item">        The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be <see langword="null" /> for reference types.      </param>
    ///<returns>        The zero-based index of the last occurrence of <paramref name="item" /> within the entire the <see cref="T:System.Collections.Generic.List`1" />, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.LastIndexOf(T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListLastIndexOf<T>(Array<T> instance, T item);

    ///<summary>        Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the first element to the specified index.      </summary>
    ///<param name="item">        The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be <see langword="null" /> for reference types.      </param>
    ///<param name="index">The zero-based starting index of the backward search.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.      </exception>
    ///<returns>        The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the first element to <paramref name="index" />, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.LastIndexOf(T, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListLastIndexOf2<T>(Array<T> instance, T item, Number index);

    ///<summary>        Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that contains the specified number of elements and ends at the specified index.      </summary>
    ///<param name="item">        The object to locate in the <see cref="T:System.Collections.Generic.List`1" />. The value can be <see langword="null" /> for reference types.      </param>
    ///<param name="index">The zero-based starting index of the backward search.</param>
    ///<param name="count">The number of elements in the section to search.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is outside the range of valid indexes for the <see cref="T:System.Collections.Generic.List`1" />.        -or-        <paramref name="count" /> is less than 0.        -or-        <paramref name="index" /> and <paramref name="count" /> do not specify a valid section in the <see cref="T:System.Collections.Generic.List`1" />.      </exception>
    ///<returns>        The zero-based index of the last occurrence of <paramref name="item" /> within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that contains <paramref name="count" /> number of elements and ends at <paramref name="index" />, if found; otherwise, -1.      </returns>    [WhiteList("System.Collections.Generic.List<T>.LastIndexOf(T, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListLastIndexOf3<T>(Array<T> instance, T item, Number index, Number count);

    ///<summary>        Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="item">        The object to remove from the <see cref="T:System.Collections.Generic.List`1" />. The value can be <see langword="null" /> for reference types.      </param>
    ///<returns>  <see langword="true" /> if <paramref name="item" /> is successfully removed; otherwise, <see langword="false" />.  This method also returns <see langword="false" /> if <paramref name="item" /> was not found in the <see cref="T:System.Collections.Generic.List`1" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.Remove(T)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ListRemove<T>(Array<T> instance, T item);

    ///<summary>Removes all the elements that match the conditions defined by the specified predicate.</summary>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to remove.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<returns>        The number of elements removed from the <see cref="T:System.Collections.Generic.List`1" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.RemoveAll(System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number ListRemoveAll<T>(Array<T> instance, Predicate<T> match);

    ///<summary>        Removes the element at the specified index of the <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="index">The zero-based index of the element to remove.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than 0.        -or-        <paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Generic.List`1.Count" />.      </exception>    [WhiteList("System.Collections.Generic.List<T>.RemoveAt(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListRemoveAt<T>(Array<T> instance, Number index);

    ///<summary>        Removes a range of elements from the <see cref="T:System.Collections.Generic.List`1" />.      </summary>
    ///<param name="index">The zero-based starting index of the range of elements to remove.</param>
    ///<param name="count">The number of elements to remove.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than 0.        -or-        <paramref name="count" /> is less than 0.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the <see cref="T:System.Collections.Generic.List`1" />.      </exception>    [WhiteList("System.Collections.Generic.List<T>.RemoveRange(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListRemoveRange<T>(Array<T> instance, Number index, Number count);

    ///<summary>        Reverses the order of the elements in the entire <see cref="T:System.Collections.Generic.List`1" />.      </summary>    [WhiteList("System.Collections.Generic.List<T>.Reverse()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListReverse<T>(Array<T> instance);

    ///<summary>Reverses the order of the elements in the specified range.</summary>
    ///<param name="index">The zero-based starting index of the range to reverse.</param>
    ///<param name="count">The number of elements in the range to reverse.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than 0.        -or-        <paramref name="count" /> is less than 0.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the <see cref="T:System.Collections.Generic.List`1" />.      </exception>    [WhiteList("System.Collections.Generic.List<T>.Reverse(int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListReverse2<T>(Array<T> instance, Number index, Number count);

    ///<summary>        Sorts the elements in the entire <see cref="T:System.Collections.Generic.List`1" /> using the default comparer.      </summary>
    ///<exception cref="T:System.InvalidOperationException">        The default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find an implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.      </exception>    [WhiteList("System.Collections.Generic.List<T>.Sort()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListSort<T>(Array<T> instance);

    ///<summary>        Sorts the elements in the entire <see cref="T:System.Collections.Generic.List`1" /> using the specified comparer.      </summary>
    ///<param name="comparer">        The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements, or <see langword="null" /> to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" />.      </param>
    ///<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.      </exception>
    ///<exception cref="T:System.ArgumentException">        The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.      </exception>    [WhiteList("System.Collections.Generic.List<T>.Sort(System.Collections.Generic.IComparer<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListSort2<T>(Array<T> instance, IComparer<T>? comparer);

    ///<summary>        Sorts the elements in a range of elements in <see cref="T:System.Collections.Generic.List`1" /> using the specified comparer.      </summary>
    ///<param name="index">The zero-based starting index of the range to sort.</param>
    ///<param name="count">The length of the range to sort.</param>
    ///<param name="comparer">        The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements, or <see langword="null" /> to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" />.      </param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than 0.        -or-        <paramref name="count" /> is less than 0.      </exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="index" /> and <paramref name="count" /> do not specify a valid range in the <see cref="T:System.Collections.Generic.List`1" />.        -or-        The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.      </exception>
    ///<exception cref="T:System.InvalidOperationException">  <paramref name="comparer" /> is <see langword="null" />, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default" /> cannot find implementation of the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface for type <paramref name="T" />.      </exception>    [WhiteList("System.Collections.Generic.List<T>.Sort(int, int, System.Collections.Generic.IComparer<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListSort3<T>(Array<T> instance, Number index, Number count, IComparer<T>? comparer);

    ///<summary>        Sorts the elements in the entire <see cref="T:System.Collections.Generic.List`1" /> using the specified <see cref="T:System.Comparison`1" />.      </summary>
    ///<param name="comparison">        The <see cref="T:System.Comparison`1" /> to use when comparing elements.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="comparison" /> is <see langword="null" />.      </exception>
    ///<exception cref="T:System.ArgumentException">        The implementation of <paramref name="comparison" /> caused an error during the sort. For example, <paramref name="comparison" /> might not return 0 when comparing an item with itself.      </exception>    [WhiteList("System.Collections.Generic.List<T>.Sort(System.Comparison<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListSort4<T>(Array<T> instance, Comparison<T> comparison);

    ///<summary>        Copies the elements of the <see cref="T:System.Collections.Generic.List`1" /> to a new array.      </summary>
    ///<returns>        An array containing copies of the elements of the <see cref="T:System.Collections.Generic.List`1" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.ToArray()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Array<T> ListToArray<T>(Array<T> instance);

    ///<summary>        Sets the capacity to the actual number of elements in the <see cref="T:System.Collections.Generic.List`1" />, if that number is less than a threshold value.      </summary>    [WhiteList("System.Collections.Generic.List<T>.TrimExcess()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void ListTrimExcess<T>(Array<T> instance);

    ///<summary>        Determines whether every element in the <see cref="T:System.Collections.Generic.List`1" /> matches the conditions defined by the specified predicate.      </summary>
    ///<param name="match">        The <see cref="T:System.Predicate`1" /> delegate that defines the conditions to check against the elements.      </param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="match" /> is <see langword="null" />.      </exception>
    ///<returns>  <see langword="true" /> if every element in the <see cref="T:System.Collections.Generic.List`1" /> matches the conditions defined by the specified predicate; otherwise, <see langword="false" />. If the list has no elements, the return value is <see langword="true" />.      </returns>    [WhiteList("System.Collections.Generic.List<T>.TrueForAll(System.Predicate<T>)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool ListTrueForAll<T>(Array<T> instance, Predicate<T> match);
}
