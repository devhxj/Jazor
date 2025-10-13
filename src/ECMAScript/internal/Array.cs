using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ECMAScript;


/// <summary>
/// A function that accepts up to three arguments. The every method calls the predicate function for each element in the array until the predicate returns a value which is coercible to the Boolean value false, or until the end of the array.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="S"></typeparam>
/// <param name="value">value</param>
/// <param name="index">index</param>
/// <param name="array">array</param>
/// <returns></returns>
[ECMAScript]
public delegate S Predicate<T, S>(T value, uint index, Array<T> array);

/// <summary>
/// A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="U"></typeparam>
/// <param name="Value"></param>
/// <param name="index"></param>
/// <param name="array"></param>
/// <returns></returns>
[ECMAScript]
public delegate U CallbackFunc<T, U>(T Value, uint index, Array<T> array);

/// <summary>
/// A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="U"></typeparam>
/// <param name="Value"></param>
/// <param name="index"></param>
/// <param name="array"></param>
[ECMAScript]
public delegate void CallbackFunc1<T, U>(T Value, uint index, Array<T> array);

/// <summary>
/// A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="U"></typeparam>
/// <param name="Value"></param>
/// <param name="index"></param>
/// <returns></returns>
[ECMAScript]
public delegate U CallbackFunc2<T, U>(T Value, uint index);

/// <summary>
/// A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="U"></typeparam>
/// <param name="Value"></param>
/// <param name="index"></param>
[ECMAScript]
public delegate void CallbackFunc3<T, U>(T Value, uint index);

/// <summary>
/// A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="U"></typeparam>
/// <param name="Value"></param>
/// <param name="index"></param>
[ECMAScript]
public delegate void CallbackFunc4<T, U>(T Value);

/// <summary>
/// A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="U"></typeparam>
/// <param name="Value"></param>
/// <param name="index"></param>
[ECMAScript]
public delegate U CallbackFunc5<T, U>(T Value);

/// <summary>
/// A function that accepts up to four arguments. The reduce method calls the callbackfn function one time for each element in the array.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="U"></typeparam>
/// <param name="previousValue"></param>
/// <param name="currentValue"></param>
/// <param name="currentIndex"></param>
/// <param name="array"></param>
/// <returns></returns>
[ECMAScript]
public delegate U ReduceFunc<T, U>(U previousValue, T currentValue, uint currentIndex, Array<T> array);

[ECMAScript]
public interface IArray : IEnumerable
{
	uint Length { get; }

	static extern bool IsArray(object? obj);
}

[ECMAScript]
public interface IArray<T> : IArray
{
	T this[uint index] { get; }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
[ECMAScript]
[Description("@#Array")]
public partial class Array<T>: IArray<T>
{
	public extern Array();

	public extern Array(uint size);

	public extern Array(T item);

	public extern Array(T item1, T item2);

	public extern Array(T item1, T item2, params T[] items);

	public extern static implicit operator T[](Array<T> x);

	public extern static implicit operator List<T>[](Array<T> x);

	public extern static implicit operator Array<T>(T[] array);

	public extern static implicit operator Array<T>(List<T> array);

	public extern static implicit operator Array<T>(ReadOnlyCollection<T> array);

	public extern static implicit operator Array<T>(ReadOnlySet<T> array);

	public extern T this[uint index] { get; set; }

	/// <summary>
	/// Gets or sets the length of the array. This is a number one higher than the highest index in the array.
	/// </summary>
	[Description("@#length")]
	public extern uint Length { get; }

	/////<summary>
	/////Returns a string representation of an object.
	/////</summary>
	/////<returns>Returns "[object objectname]", where objectname is the name of the object type.</returns>
	//[Description("@#toString")]
	//public extern override string? ToString();

	///// <summary>
	///// Returns a string representation of an array. The elements are converted to string using their toLocaleString methods.
	///// </summary>
	///// <returns></returns>
	//[Description("@#toLocaleString")]
	//public extern string ToLocaleString();

	/// <summary>
	/// Removes the last element from an array and returns it.
	/// If the array is empty, undefined is returned and the array is not modified.
	/// </summary>
	/// <returns></returns>
	[Description("@#pop")]
	public extern T? Pop();

	/// <summary>
	/// Appends new elements to the end of an array, and returns the new length of the array.
	/// </summary>
	/// <param name="items">New elements to add to the array.</param>
	/// <returns></returns>
	[Description("@#push")]
	public extern double Push(params IEnumerable<T> items);

	/// <summary>
	/// Combines two or more arrays.
	/// This method returns a new array without modifying any existing arrays.
	/// </summary>
	/// <param name="items">Additional arrays and/or items to add to the end of the array.</param>
	/// <returns></returns>
	[Description("@#concat")]
	public extern IEnumerable<T> Concat(params IEnumerable<T>[] items);

	/// <summary>
	/// Combines two or more arrays.
	/// </summary>
	/// <param name="items">Additional arrays and/or items to add to the end of the array.</param>
	/// <returns>This method returns a new array without modifying any existing arrays.</returns>
	[Description("@#concat")]
	public extern IEnumerable<T> Concat(params IEnumerable<T> items);

	/// <summary>
	/// Adds all the elements of an array into a string, separated by the specified separator string.
	/// </summary>
	/// <param name="separator">A string used to separate one element of the array from the next in the resulting string. If omitted, the array elements are separated with a comma.</param>
	/// <returns></returns>
	[Description("@#join")]
	public extern string Join(string? separator = null);

	/// <summary>
	/// Reverses the elements in an array in place.
	/// </summary>
	/// <returns>This method mutates the array and returns a reference to the same array.</returns>
	[Description("@#reverse")]
	public extern Array<T> Reverse();

	/// <summary>
	/// Removes the first element from an array and returns it.
	/// </summary>
	/// <returns>If the array is empty, undefined is returned and the array is not modified.</returns>
	[Description("@#shift")]
	public extern T? Shift();

	/// <summary>
	/// Returns a copy of a section of an array.
	/// For both start and end, a negative index can be used to indicate an offset from the end of the array.
	/// For example, -2 refers to the second to last element of the array.
	/// </summary>
	/// <param name="start">The beginning index of the specified portion of the array.</param>
	/// <param name="end">The end index of the specified portion of the array. This is exclusive of the element at the index 'end'.</param>
	/// <returns>If start is undefined, then the slice begins at index 0.If end is undefined, then the slice extends to the end of the array.</returns>
	[Description("@#slice")]
	public extern Array<T> Slice(double? start = null, double? end = null);

	/// <summary>
	/// Sorts an array in place.
	/// This method mutates the array and returns a reference to the same array.
	/// </summary>
	/// <param name="compareFn"><para><b>(a: T, b: T) => number</b></para>Function used to determine the order of the elements.It is expected to return</param>
	/// <returns>a negative value if the first argument is less than the second argument, zero if they're equal, and a positive value otherwise.If omitted, the elements are sorted in ascending, UTF-16 code unit order.</returns>
	[Description("@#sort")]
	public extern Array<T> Sort(Func<T, T, double>? compareFn = null);

	/// <summary>
	/// Removes elements from an array and, if necessary, inserts new elements in their place, returning the deleted elements.
	/// </summary>
	/// <param name="start">The zero-based location in the array from which to start removing elements.</param>
	/// <param name="deleteCount">The number of elements to remove.</param>
	/// <returns>An array containing the elements that were deleted.</returns>
	[Description("@#splice")]
	public extern Array<T> Splice(double start, double? deleteCount = null);

	/// <summary>
	/// Removes elements from an array and, if necessary, inserts new elements in their place, returning the deleted elements.
	/// </summary>
	/// <param name="start">The zero-based location in the array from which to start removing elements.</param>
	/// <param name="deleteCount">The number of elements to remove.</param>
	/// <param name="items">Elements to insert into the array in place of the deleted elements.</param>
	/// <returns>An array containing the elements that were deleted.</returns>
	[Description("@#splice")]
	public extern Array<T> Splice(double start, double deleteCount, params IEnumerable<T> items);

	/// <summary>
	/// Inserts new elements at the start of an array, and returns the new length of the array.
	/// </summary>
	/// <param name="items">Elements to insert at the start of the array.</param>
	/// <returns></returns>
	[Description("@#unshift")]
	public extern double Unshift(params IEnumerable<T> items);

	/// <summary>
	/// Returns the index of the first occurrence of a value in an array, or -1 if it is not present.
	/// </summary>
	/// <param name="searchElement">The value to locate in the array.</param>
	/// <param name="fromIndex">The array index at which to begin the search.If fromIndex is omitted, the search starts at index 0.</param>
	/// <returns></returns>
	[Description("@#indexOf")]
	public extern double IndexOf(T searchElement, double? fromIndex);

	/// <summary>
	/// Returns the index of the last occurrence of a specified value in an array, or -1 if it is not present.
	/// </summary>
	/// <param name="searchElement">The value to locate in the array.</param>
	/// <param name="fromIndex">The array index at which to begin searching backward.If fromIndex is omitted, the search starts at the last index in the array.</param>
	/// <returns></returns>
	[Description("@#lastIndexOf")]
	public extern double LastIndexOf(T searchElement, double? fromIndex);

	/// <summary>
	/// Determines whether all the members of an array satisfy the specified test.
	/// </summary>
	/// <typeparam name="S"></typeparam>
	/// <param name="predicate"><para><b>(value: T, index: number, array: IEnumerable&lt;T&gt;) =&gt; value is S</b></para>A function that accepts up to three arguments. The every method calls the predicate function for each element in the array until the predicate returns a value which is coercible to the Boolean value false, or until the end of the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function.If thisArg is omitted, undefined is used as the this value.</param>
	/// <returns></returns>
	[Description("@#every")]
	public extern Array<S> Every<S>(Predicate<T, S> predicate, object? thisArg = null) where S : T;

	/// <summary>
	/// Determines whether all the members of an array satisfy the specified test.
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: IEnumerable<T>) => unknown</b></para>A function that accepts up to three arguments. The every method calls the predicate function for each element in the array until the predicate returns a value which is coercible to the Boolean value false, or until the end of the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function.If thisArg is omitted, undefined is used as the this value.</param>
	/// <returns></returns>
	[Description("@#every")]
	public extern bool Every(Predicate<T, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Determines whether the specified callback function returns true for any element of an array.
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: IEnumerable<T>) => unknown</b></para>A function that accepts up to three arguments.The some method calls the predicate function for each element in the array until the predicate returns a value which is coercible to the Boolean value true, or until the end of the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function.If thisArg is omitted, undefined is used as the this value.</param>
	/// <returns></returns>
	[Description("@#some")]
	public extern bool Some(Predicate<T, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Performs the specified action for each element in an array.
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: T, index: number, array: IEnumerable<T>) => void</b></para>A function that accepts up to three arguments. forEach calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the callbackfn function.If thisArg is omitted, undefined is used as the this value.</param>
	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc<T, uint> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc1<T, uint> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc2<T, uint> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc3<T, uint> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc4<T, uint> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc5<T, uint> callbackfn, object? thisArg = null);

	/// <summary>
	/// Calls a defined callback function on each element of an array, and returns an array that contains the results.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="callbackfn"><para><b>(value: T, index: number, array: IEnumerable<T>) => U</b></para>A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the callbackfn function.If thisArg is omitted, undefined is used as the this value.</param>
	/// <returns></returns>
	[Description("@#map")]
	public extern Array<U> Map<U>(CallbackFunc<T, U> callbackfn, object? thisArg = null);

	/// <summary>
	/// Returns the elements of an array that meet the condition specified in a callback function.
	/// </summary>
	/// <typeparam name="S"></typeparam>
	/// <param name="predicate"><para><b>(value: T, index: number, array: IEnumerable<T>) => value is S</b></para>A function that accepts up to three arguments.The filter method calls the predicate function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function.If thisArg is omitted, undefined is used as the this value.</param>
	/// <returns></returns>
	[Description("@#filter")]
	public extern Array<S> Filter<S>(Predicate<T, S> predicate, object? thisArg = null) where S : T;

	/// <summary>
	/// Returns the elements of an array that meet the condition specified in a callback function.
	/// </summary>
	/// <typeparam name="S"></typeparam>
	/// <param name="predicate"><para><b>(value: T, index: number, array: IEnumerable<T>) => unknown</b></para>A function that accepts up to three arguments.The filter method calls the predicate function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function.If thisArg is omitted, undefined is used as the this value.</param>
	/// <returns></returns>
	[Description("@#filter")]
	public extern IEnumerable<T> Filter<S>(Predicate<T, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="callbackfn"><para><b>(previousValue: U, currentValue: T, currentIndex: number, array: IEnumerable<T>) => U</b></para>A function that accepts up to four arguments.The reduce method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduce")]
	public extern U Reduce<U>(ReduceFunc<T, U> callbackfn, U initialValue);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="callbackfn">A function that accepts up to four arguments.The reduceRight method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduceRight")]
	public extern U ReduceRight<U>(ReduceFunc<T, U> callbackfn, U initialValue);

	public extern static IEnumerable<T> From(IEnumerable<T> arrayLike);

	public extern static IEnumerable<T> From<U>(IEnumerable<U> arrayLike, Func<U, int, T> mapFn, object? thisArg = null);

	public extern static PromiseResult<IEnumerable<T>> FromAsync(IEnumerable<T> arrayLike);

	public extern static PromiseResult<IEnumerable<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, int, T> mapFn, object? thisArg = null);

	public extern static bool IsArray(object? value);

	public extern static Array<T> Of(params IEnumerable<T> value);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern new Type GetType();

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override int GetHashCode();

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override bool Equals(object? obj);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern static new bool Equals(object objA, object objB);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern static new bool ReferenceEquals(object objA, object objB);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void Add(T item);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern virtual void Clear();

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern bool Contains(T item);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern void CopyTo(T[] array, int arrayIndex);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern bool Remove(T item);

	extern IEnumerator IEnumerable.GetEnumerator();

	//extern IEnumerator<T> IEnumerable<T>.GetEnumerator();
}

public static partial class Global
{
	extension(Array array)
	{
		public static bool IsArray(object obj) => default;
	}
}
