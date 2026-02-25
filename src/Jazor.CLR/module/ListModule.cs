namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Collections.Generic.List<T>","System/Collections/Generic/ListModule.js")]
public static class ListModule<T>
{
	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has the default initial capacity.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.List()")]
	public extern static Array<T> _01dceb3b4d503bbf();

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has the specified initial capacity.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.List(int)")]
	public extern static Array<T> _feacfe24abeee54b(Number capacity);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.List(System.Collections.Generic.IEnumerable<T>)")]
	public extern static Array<T> _ea4c991aac8688c0(Array<T> collection);

	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Capacity.get")]
	public extern static Number _ffa580d06e0078ae(Array<T> instance);

	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Capacity.set")]
	public extern static void _db03a5f0f4bc11af(Array<T> instance, Number value);

	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Count.get")]
	public extern static Number _a2137cdeeb85f3d9(Array<T> instance);

	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.this[int].get")]
	public extern static T _d389c31d59037b42(Array<T> instance, Number index);

	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.this[int].set")]
	public extern static void _c16a7960302ea054(Array<T> instance, Number index, object value);

	///<summary>Adds an object to the end of the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Add(T)")]
	public extern static void _342f4a7099c7ddf0(Array<T> instance, object item);

	///<summary>Adds the elements of the specified collection to the end of the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.AddRange(System.Collections.Generic.IEnumerable<T>)")]
	public extern static void _a2660853a4ebc1f6(Array<T> instance, Array<T> collection);

	///<summary>Returns a read-only <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> wrapper for the current collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.AsReadOnly()")]
	public extern static Array<T> _f7981b5a4cd02bdb(Array<T> instance);

	///<summary>Searches a range of elements in the sorted <see cref="T:System.Collections.Generic.List`1" /> for an element using the specified comparer and returns the zero-based index of the element.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.BinarySearch(int, int, T, System.Collections.Generic.IComparer<T>)")]
	public extern static Number _95ada27dd960bae5(Array<T> instance, Number index, Number count, object item, IComparer<T>? comparer);

	///<summary>Searches the entire sorted <see cref="T:System.Collections.Generic.List`1" /> for an element using the default comparer and returns the zero-based index of the element.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.BinarySearch(T)")]
	public extern static Number _3d21965eedc9916f(Array<T> instance, object item);

	///<summary>Searches the entire sorted <see cref="T:System.Collections.Generic.List`1" /> for an element using the specified comparer and returns the zero-based index of the element.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.BinarySearch(T, System.Collections.Generic.IComparer<T>)")]
	public extern static Number _65e239056cc65177(Array<T> instance, object item, IComparer<T>? comparer);

	///<summary>Removes all elements from the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Clear()")]
	public extern static void _7de26e55010ee1a8(Array<T> instance);

	///<summary>Determines whether an element is in the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Contains(T)")]
	public extern static bool _d9fab27c685b7de9(Array<T> instance, object item);

	///<summary>Converts the elements in the current <see cref="T:System.Collections.Generic.List`1" /> to another type, and returns a list containing the converted elements.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.ConvertAll<TOutput>(System.Converter<T, TOutput>)")]
	public extern static System.Collections.Generic.List<TOutput> _098c2e027f3a5996<TOutput>(Array<T> instance, object converter);

	///<summary>Copies the entire <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the beginning of the target array.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.CopyTo(T[])")]
	public extern static void _9a3a4817585dded1(Array<T> instance, Array<T> array);

	///<summary>Copies a range of elements from the <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.CopyTo(int, T[], int, int)")]
	public extern static void _0fdf1627d283f8ae(Array<T> instance, Number index, Array<T> array, Number arrayIndex, Number count);

	///<summary>Copies the entire <see cref="T:System.Collections.Generic.List`1" /> to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.CopyTo(T[], int)")]
	public extern static void _3559b1ff2a643922(Array<T> instance, Array<T> array, Number arrayIndex);

	///<summary>Ensures that the capacity of this list is at least the specified <paramref name="capacity" />. If the current capacity is less than <paramref name="capacity" />, it is increased to at least the specified <paramref name="capacity" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.EnsureCapacity(int)")]
	public extern static Number _6dffb0ed23f010e0(Array<T> instance, Number capacity);

	///<summary>Determines whether the <see cref="T:System.Collections.Generic.List`1" /> contains elements that match the conditions defined by the specified predicate.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Exists(System.Predicate<T>)")]
	public extern static bool _b23997dd4232ced6(Array<T> instance, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Find(System.Predicate<T>)")]
	public extern static T? _089a5c28e11eeeaf(Array<T> instance, Predicate<T> match);

	///<summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.FindAll(System.Predicate<T>)")]
	public extern static Array<T> _d8e500da425f2be5(Array<T> instance, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.FindIndex(System.Predicate<T>)")]
	public extern static Number _4770bba04510e57b(Array<T> instance, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the specified index to the last element.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.FindIndex(int, System.Predicate<T>)")]
	public extern static Number _db9b68fbc73e342b(Array<T> instance, Number startIndex, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that starts at the specified index and contains the specified number of elements.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.FindIndex(int, int, System.Predicate<T>)")]
	public extern static Number _41b337b09c5daf75(Array<T> instance, Number startIndex, Number count, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.FindLast(System.Predicate<T>)")]
	public extern static T? _de0943e496e36f2d(Array<T> instance, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.FindLastIndex(System.Predicate<T>)")]
	public extern static Number _ae1a0b59c73f2b1a(Array<T> instance, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the first element to the specified index.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.FindLastIndex(int, System.Predicate<T>)")]
	public extern static Number _081aa9ae0b09d058(Array<T> instance, Number startIndex, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that contains the specified number of elements and ends at the specified index.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.FindLastIndex(int, int, System.Predicate<T>)")]
	public extern static Number _58cc54dc07e440c4(Array<T> instance, Number startIndex, Number count, Predicate<T> match);

	///<summary>Performs the specified action on each element of the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.ForEach(System.Action<T>)")]
	public extern static void _7395d2cfe6dce3fb(Array<T> instance, object action);

	///<summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.GetEnumerator()")]
	public extern static System.Collections.Generic.List<T>.Enumerator _b9724d52a219e3b6(Array<T> instance);

	///<summary>Creates a shallow copy of a range of elements in the source <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.GetRange(int, int)")]
	public extern static Array<T> _c35c9c99a23ff96a(Array<T> instance, Number index, Number count);

	///<summary>Creates a shallow copy of a range of elements in the source <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Slice(int, int)")]
	public extern static Array<T> _adcf2df90da54ec8(Array<T> instance, Number start, Number length);

	///<summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.IndexOf(T)")]
	public extern static Number _2bb4b70655cede73(Array<T> instance, object item);

	///<summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the specified index to the last element.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.IndexOf(T, int)")]
	public extern static Number _71ee35e0e260eb27(Array<T> instance, object item, Number index);

	///<summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that starts at the specified index and contains the specified number of elements.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.IndexOf(T, int, int)")]
	public extern static Number _5ee52e4e4fc54e6d(Array<T> instance, object item, Number index, Number count);

	///<summary>Inserts an element into the <see cref="T:System.Collections.Generic.List`1" /> at the specified index.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Insert(int, T)")]
	public extern static void _0dc538197c677986(Array<T> instance, Number index, object item);

	///<summary>Inserts the elements of a collection into the <see cref="T:System.Collections.Generic.List`1" /> at the specified index.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.InsertRange(int, System.Collections.Generic.IEnumerable<T>)")]
	public extern static void _56ef9aefabac7c09(Array<T> instance, Number index, Array<T> collection);

	///<summary>Searches for the specified object and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.LastIndexOf(T)")]
	public extern static Number _121df07eb2f61749(Array<T> instance, object item);

	///<summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that extends from the first element to the specified index.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.LastIndexOf(T, int)")]
	public extern static Number _279befda6399cda5(Array<T> instance, object item, Number index);

	///<summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Collections.Generic.List`1" /> that contains the specified number of elements and ends at the specified index.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.LastIndexOf(T, int, int)")]
	public extern static Number _b2f1955b62962812(Array<T> instance, object item, Number index, Number count);

	///<summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Remove(T)")]
	public extern static bool _562f832fd220e768(Array<T> instance, object item);

	///<summary>Removes all the elements that match the conditions defined by the specified predicate.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.RemoveAll(System.Predicate<T>)")]
	public extern static Number _b864beda26f186e2(Array<T> instance, Predicate<T> match);

	///<summary>Removes the element at the specified index of the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.RemoveAt(int)")]
	public extern static void _a5e8c6b27df6470b(Array<T> instance, Number index);

	///<summary>Removes a range of elements from the <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.RemoveRange(int, int)")]
	public extern static void _8425758ef4e7b6f9(Array<T> instance, Number index, Number count);

	///<summary>Reverses the order of the elements in the entire <see cref="T:System.Collections.Generic.List`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Reverse()")]
	public extern static void _8a13946a926a97b2(Array<T> instance);

	///<summary>Reverses the order of the elements in the specified range.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Reverse(int, int)")]
	public extern static void _56dc1af8af32e484(Array<T> instance, Number index, Number count);

	///<summary>Sorts the elements in the entire <see cref="T:System.Collections.Generic.List`1" /> using the default comparer.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Sort()")]
	public extern static void _36a478f36b41a6d2(Array<T> instance);

	///<summary>Sorts the elements in the entire <see cref="T:System.Collections.Generic.List`1" /> using the specified comparer.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Sort(System.Collections.Generic.IComparer<T>)")]
	public extern static void _5fa599e721e252ff(Array<T> instance, IComparer<T>? comparer);

	///<summary>Sorts the elements in a range of elements in <see cref="T:System.Collections.Generic.List`1" /> using the specified comparer.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Sort(int, int, System.Collections.Generic.IComparer<T>)")]
	public extern static void _19207851b52a5287(Array<T> instance, Number index, Number count, IComparer<T>? comparer);

	///<summary>Sorts the elements in the entire <see cref="T:System.Collections.Generic.List`1" /> using the specified <see cref="T:System.Comparison`1" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.Sort(System.Comparison<T>)")]
	public extern static void _0d91dcbccdea7c8c(Array<T> instance, Comparison<T> comparison);

	///<summary>Copies the elements of the <see cref="T:System.Collections.Generic.List`1" /> to a new array.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.ToArray()")]
	public extern static Array<T> _eedb6fcf490f54cb(Array<T> instance);

	///<summary>Sets the capacity to the actual number of elements in the <see cref="T:System.Collections.Generic.List`1" />, if that number is less than a threshold value.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.TrimExcess()")]
	public extern static void _27c95e83eced65e9(Array<T> instance);

	///<summary>Determines whether every element in the <see cref="T:System.Collections.Generic.List`1" /> matches the conditions defined by the specified predicate.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.List<T>.TrueForAll(System.Predicate<T>)")]
	public extern static bool _d12a4656f219490c(Array<T> instance, Predicate<T> match);
}
