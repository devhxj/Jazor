namespace ECMAScript;

[ECMAScript]
/// <summary>
/// JavaScript Array 的非泛型 authoring binding。
/// </summary>
/// <remarks>
/// Array 是可变、可迭代且按 Number length 管理的 JS 容器；接口声明只提供编译器可投影的成员，
/// 不承诺 CLR Array 的多维、固定长度或运行时元素类型语义。
/// </remarks>
public interface IArray : IEnumerable
{
	[Description("@#length")]
	Number Length { get; }

	[Description("@#isArray")]
	static extern bool IsArray(object? obj);
}

[ECMAScript]
/// <summary>带编译期元素类型标注的 JavaScript Array binding。</summary>
public interface IArray<T> : IArray
{
	/// <summary>
	/// Direct JavaScript index access surface.
	/// This stays non-nullable for compatibility with existing array-like mappings.
	/// Callers that need absence-aware reads should prefer APIs such as <c>At()</c> on concrete hosts.
	/// </summary>
	T this[Number index] { get; }
}

/// <summary>
/// JavaScript <c>Array</c> runtime host.
/// Hidden members near the end of this type exist only for CLR bridge scenarios such as collection initializers;
/// they are not intended to redefine the JavaScript runtime shape.
/// </summary>
/// <typeparam name="T"></typeparam>
[ECMAScript]
[Description("@#Array")]
public partial class Array<T> : object, IArray<T>
{
	public extern Array();

	public extern Array(Number size);

	public extern Array(T item);

	public extern Array(T item1, T item2);

	public extern Array(T item1, T item2, params T[] items);

	public extern static implicit operator T[](Array<T> x);

	public extern static implicit operator List<T>(Array<T> x);

	public extern static implicit operator Array<T>(T[] array);

	public extern static implicit operator Array<T>(List<T> array);

	public extern static implicit operator Array<T>(ReadOnlyCollection<T> array);

	public extern static implicit operator Array<T>(ReadOnlySet<T> array);

	public extern static implicit operator Array<T>(Array array);

	/// <summary>
	/// Direct JavaScript index access surface.
	/// This stays non-nullable to preserve compatibility with CLR collection-style mappings that project to JavaScript arrays.
	/// Use <see cref="At" /> when you need a nullable result for out-of-range access.
	/// </summary>
	public extern T this[Number index] { get; set; }

	/// <summary>
	/// Gets or sets the length of the array. This is a number one higher than the highest index in the array.
	/// </summary>
	[Description("@#length")]
	public extern Number Length { get; }

	/// <summary>
	/// Returns the JavaScript string form of the array.
	/// This is the direct projection of <c>Array.prototype.toString()</c>.
	/// </summary>
	[Description("@#toString")]
	public extern override string ToString();

	/// <summary>
	/// Returns a locale-sensitive string representation of the array.
	/// This is the direct projection of <c>Array.prototype.toLocaleString()</c>.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString();

	/// <summary>
	/// Returns a locale-sensitive string representation of the array.
	/// JavaScript forwards <paramref name="locales" /> and <paramref name="options" /> to each element's own <c>toLocaleString</c> method.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(string? locales, object? options = null);

	/// <summary>
	/// C# convenience overload for the JavaScript form that omits <c>locales</c> and only supplies options.
	/// This exists because C# cannot naturally skip the leading locale argument in method calls.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(object? options);

	/// <summary>
	/// Returns a locale-sensitive string representation of the array.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
	/// JavaScript forwards <paramref name="locales" /> and <paramref name="options" /> to each element's own <c>toLocaleString</c> method.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(IEnumerable<string>? locales, object? options = null);

	/// <summary>
	/// Removes the last element from an array and returns it.
	/// If the array is empty, JavaScript returns <c>undefined</c>; this C# projection surfaces that absence as <see langword="null" />
	/// and does not modify the array.
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
	public extern Number Push(params T[] items);

	/// <summary>
	/// Combines two or more arrays.
	/// This method returns a new array without modifying any existing arrays.
	/// </summary>
	/// <param name="items">Additional arrays and/or items to add to the end of the array.</param>
	/// <returns></returns>
	[Description("@#concat")]
	public extern Array<T> Concat(params IEnumerable<T>[] items);

	/// <summary>
	/// Combines two or more arrays.
	/// </summary>
	/// <param name="items">Additional arrays and/or items to add to the end of the array.</param>
	/// <returns>This method returns a new array without modifying any existing arrays.</returns>
	[Description("@#concat")]
	public extern Array<T> Concat(params T[] items);

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
	/// <returns>If the array is empty, JavaScript returns <c>undefined</c>; this C# projection surfaces that absence as <see langword="null" /> and does not modify the array.</returns>
	[Description("@#shift")]
	public extern T? Shift();

	/// <summary>
	/// Returns a copy of a section of an array.
	/// For both start and end, a negative index can be used to indicate an offset from the end of the array.
	/// For example, -2 refers to the second to last element of the array.
	/// </summary>
	/// <param name="start">The beginning index of the specified portion of the array.</param>
	/// <param name="end">The end index of the specified portion of the array. This is exclusive of the element at the index 'end'.</param>
	/// <returns>If <paramref name="start" /> is omitted, the slice begins at index 0. If <paramref name="end" /> is omitted, the slice extends to the end of the array.</returns>
	[Description("@#slice")]
	public extern Array<T> Slice(Number? start = null, Number? end = null);

	/// <summary>
	/// Sorts an array in place.
	/// This method mutates the array and returns a reference to the same array.
	/// </summary>
	/// <param name="compareFn"><para><b>(a: T, b: T) => number</b></para>Function used to determine the order of the elements.It is expected to return</param>
	/// <returns>a negative value if the first argument is less than the second argument, zero if they're equal, and a positive value otherwise.If omitted, the elements are sorted in ascending, UTF-16 code unit order.</returns>
	[Description("@#sort")]
	public extern Array<T> Sort(Func<T, T, Number>? compareFn = null);

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#sort")]
	public extern Array<T> Sort(Comparison<T> compareFn);

	/// <summary>
	/// Removes elements from an array and, if necessary, inserts new elements in their place, returning the deleted elements.
	/// </summary>
	/// <param name="start">The zero-based location in the array from which to start removing elements.</param>
	/// <param name="deleteCount">The number of elements to remove.</param>
	/// <returns>An array containing the elements that were deleted.</returns>
	[Description("@#splice")]
	public extern Array<T> Splice(Number start, Number? deleteCount = null);

	/// <summary>
	/// Removes elements from an array and, if necessary, inserts new elements in their place, returning the deleted elements.
	/// </summary>
	/// <param name="start">The zero-based location in the array from which to start removing elements.</param>
	/// <param name="deleteCount">The number of elements to remove.</param>
	/// <param name="items">Elements to insert into the array in place of the deleted elements.</param>
	/// <returns>An array containing the elements that were deleted.</returns>
	[Description("@#splice")]
	public extern Array<T> Splice(Number start, Number deleteCount, params T[] items);

	/// <summary>
	/// Inserts new elements at the start of an array, and returns the new length of the array.
	/// </summary>
	/// <param name="items">Elements to insert at the start of the array.</param>
	/// <returns></returns>
	[Description("@#unshift")]
	public extern Number Unshift(params T[] items);

	/// <summary>
	/// Returns the index of the first occurrence of a value in an array, or -1 if it is not present.
	/// </summary>
	/// <param name="searchElement">The value to locate in the array.</param>
	/// <param name="fromIndex">The array index at which to begin the search.If fromIndex is omitted, the search starts at index 0.</param>
	/// <returns></returns>
	[Description("@#indexOf")]
	public extern Number IndexOf(T searchElement, Number? fromIndex = null);

	/// <summary>
	/// Projection of JavaScript <c>Array.prototype.includes</c>.
	/// This stays on the array host so user code can follow JavaScript runtime shape directly.
	/// </summary>
	[Description("@#includes")]
	public extern bool Includes(T searchElement, Number? fromIndex = null);

	/// <summary>
	/// Returns the index of the last occurrence of a specified value in an array, or -1 if it is not present.
	/// </summary>
	/// <param name="searchElement">The value to locate in the array.</param>
	/// <param name="fromIndex">The array index at which to begin searching backward.If fromIndex is omitted, the search starts at the last index in the array.</param>
	/// <returns></returns>
	[Description("@#lastIndexOf")]
	public extern Number LastIndexOf(T searchElement, Number? fromIndex = null);

	/// <summary>
	/// Determines whether all the members of an array satisfy the specified test.
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: IEnumerable<T>) => unknown</b></para>A function that accepts up to three arguments. The every method calls the predicate function for each element in the array until the predicate returns a value which is coercible to the Boolean value false, or until the end of the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#every")]
	public extern bool Every(Func<T, Number, Array<T>, object?> predicate, object? thisArg = null);

	[Description("@#every")]
	public extern bool Every(Func<T, Number, object?> predicate, object? thisArg = null);

		[Description("@#every")]
		public extern bool Every(Func<T, object?> predicate, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#every")]
		public extern bool Every(Predicate<T> predicate, object? thisArg = null);

	/// <summary>
	/// Determines whether the specified callback function returns true for any element of an array.
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: IEnumerable<T>) => unknown</b></para>A function that accepts up to three arguments.The some method calls the predicate function for each element in the array until the predicate returns a value which is coercible to the Boolean value true, or until the end of the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#some")]
	public extern bool Some(Func<T, Number, Array<T>, object?> predicate, object? thisArg = null);

	[Description("@#some")]
	public extern bool Some(Func<T, Number, object?> predicate, object? thisArg = null);

		[Description("@#some")]
		public extern bool Some(Func<T, object?> predicate, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#some")]
		public extern bool Some(Predicate<T> predicate, object? thisArg = null);

	/// <summary>
	/// Performs the specified action for each element in an array.
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: T, index: number, array: IEnumerable<T>) => void</b></para>A function that accepts up to three arguments. forEach calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the callbackfn function. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<T, Number, Array<T>> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(Action<T, Number> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(Action<T> callbackfn, object? thisArg = null);

	/// <summary>
	/// Calls a defined callback function on each element of an array, and returns an array that contains the results.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="callbackfn"><para><b>(value: T, index: number, array: IEnumerable<T>) => U</b></para>A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the callbackfn function. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#map")]
	public extern Array<U> Map<U>(Func<T, Number, Array<T>, U> callbackfn, object? thisArg = null);

	[Description("@#map")]
	public extern Array<U> Map<U>(Func<T, Number, U> callbackfn, object? thisArg = null);

	[Description("@#map")]
	public extern Array<U> Map<U>(Func<T, U> callbackfn, object? thisArg = null);

	/// <summary>
	/// Returns the elements of an array that meet the condition specified in a callback function.
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: IEnumerable<T>) => unknown</b></para>A function that accepts up to three arguments.The filter method calls the predicate function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#filter")]
	public extern Array<T> Filter(Func<T, Number, Array<T>, object?> predicate, object? thisArg = null);

	[Description("@#filter")]
	public extern Array<T> Filter(Func<T, Number, object?> predicate, object? thisArg = null);

		[Description("@#filter")]
		public extern Array<T> Filter(Func<T, object?> predicate, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#filter")]
		public extern Array<T> Filter(Predicate<T> predicate, object? thisArg = null);

	/// <summary>
	/// Returns the first element whose value satisfies the provided testing function.
	/// Nullable is used because JavaScript returns <c>undefined</c> when no matching element exists,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// </summary>
	[Description("@#find")]
	public extern T? Find(Func<T, Number, Array<T>, object?> predicate, object? thisArg = null);

	[Description("@#find")]
	public extern T? Find(Func<T, Number, object?> predicate, object? thisArg = null);

		[Description("@#find")]
		public extern T? Find(Func<T, object?> predicate, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#find")]
		public extern T? Find(Predicate<T> predicate, object? thisArg = null);

	/// <summary>
	/// Returns a new array with nested array elements recursively concatenated up to the specified depth.
	/// The return type is widened to <see cref="Array{T}"/> of <see cref="object"/> because JavaScript flattening changes the element shape in ways C# generics cannot faithfully express here.
	/// </summary>
	[Description("@#flat")]
	public extern Array<object?> Flat(Number? depth = null);

	/// <summary>
	/// Maps each element to a value and then flattens the result by one level.
	/// This overload covers the JavaScript case where the callback returns scalar values.
	/// </summary>
	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, Number, Array<T>, U> callbackfn, object? thisArg = null);

	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, Number, U> callbackfn, object? thisArg = null);

	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, U> callbackfn, object? thisArg = null);

	/// <summary>
	/// Maps each element to an array and then flattens the mapped arrays by one level.
	/// This matches the most common JavaScript <c>flatMap</c> usage while keeping the C# generic result type explicit.
	/// </summary>
	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, Number, Array<T>, Array<U>> callbackfn, object? thisArg = null);

	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, Number, Array<U>> callbackfn, object? thisArg = null);

	[Description("@#flatMap")]
	public extern Array<U> FlatMap<U>(Func<T, Array<U>> callbackfn, object? thisArg = null);

	/// <summary>
	/// Returns the last element whose value satisfies the provided testing function.
	/// Nullable is used because JavaScript returns <c>undefined</c> when no matching element exists,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// </summary>
	[Description("@#findLast")]
	public extern T? FindLast(Func<T, Number, Array<T>, object?> predicate, object? thisArg = null);

	[Description("@#findLast")]
	public extern T? FindLast(Func<T, Number, object?> predicate, object? thisArg = null);

		[Description("@#findLast")]
		public extern T? FindLast(Func<T, object?> predicate, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#findLast")]
		public extern T? FindLast(Predicate<T> predicate, object? thisArg = null);

	/// <summary>
	/// C# host projection of JavaScript <c>Array.prototype.at</c>.
	/// Nullable is used because JavaScript returns <c>undefined</c> for an out-of-range index,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// </summary>
	[Description("@#at")]
	public extern T? At(Number index);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <param name="callbackfn"><para><b>(previousValue: T, currentValue: T, currentIndex: number, array: IEnumerable&lt;T&gt;) => T</b></para>A function that accepts up to four arguments. When no initial value is supplied, JavaScript uses the first array element as the initial accumulator.</param>
	/// <returns>The accumulated result.</returns>
	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, Number, Array<T>, T> callbackfn);

	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, T> callbackfn);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="callbackfn"><para><b>(previousValue: U, currentValue: T, currentIndex: number, array: IEnumerable<T>) => U</b></para>A function that accepts up to four arguments.The reduce method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduce")]
	public extern U Reduce<U>(Func<U, T, Number, Array<T>, U> callbackfn, U initialValue);

	[Description("@#reduce")]
	public extern U Reduce<U>(Func<U, T, U> callbackfn, U initialValue);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <param name="callbackfn"><para><b>(previousValue: T, currentValue: T, currentIndex: number, array: IEnumerable&lt;T&gt;) => T</b></para>A function that accepts up to four arguments. When no initial value is supplied, JavaScript uses the last array element as the initial accumulator.</param>
	/// <returns>The accumulated result.</returns>
	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, Number, Array<T>, T> callbackfn);

	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, T> callbackfn);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="callbackfn">A function that accepts up to four arguments.The reduceRight method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduceRight")]
	public extern U ReduceRight<U>(Func<U, T, Number, Array<T>, U> callbackfn, U initialValue);

	[Description("@#reduceRight")]
	public extern U ReduceRight<U>(Func<U, T, U> callbackfn, U initialValue);

	/// <summary>
	/// 用一个固定值填充一个数组中从起始索引（默认为 0）到终止索引（默认为 array.length）内的全部元素。它返回修改后的数组。
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="value">用来填充数组元素的值。注意所有数组中的元素都将是这个确定的值：如果 value 是个对象，那么数组的每一项都会引用这个元素。</param>
	/// <param name="start">
	/// 基于零的索引，从此开始填充，转换为整数。
	/// 负数索引从数组的末端开始计算，如果 start < 0，则使用 start + array.length。
	/// 如果 start < -array.length 或 start 被省略，则使用 0。
	/// 如果 start >= array.length，没有索引被填充。
	/// </param>
	/// <param name="end">
	/// 基于零的索引，在此结束填充，转换为整数。fill() 填充到但不包含 end 索引。
	/// 负数索引从数组的末端开始计算，如果 end < 0，则使用 end + array.length。
	/// 如果 end < -array.length，则使用 0。
	/// 如果 end >= array.length 或 end 被省略，则使用 array.length，导致所有索引都被填充。
	/// 如果经标准化后，end 的位置在 start 之前或之上，没有索引被填充。
	/// </param>
	/// <returns>经 value 填充修改后的数组。</returns>
	[Description("@#fill")]
	public extern Array<T> Fill(T value, Number? start = null, Number? end = null);

	[Description("@#findIndex")]
	public extern Number FindIndex(Func<T, Number, Array<T>, object?> callbackfn, object? thisArg = null);

	[Description("@#findIndex")]
	public extern Number FindIndex(Func<T, Number, object?> callbackfn, object? thisArg = null);

		[Description("@#findIndex")]
		public extern Number FindIndex(Func<T, object?> callbackfn, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#findIndex")]
		public extern Number FindIndex(Predicate<T> callbackfn, object? thisArg = null);

	/// <summary>
	/// Returns the index of the last element whose value satisfies the provided testing function, or <c>-1</c> if no match is found.
	/// </summary>
	[Description("@#findLastIndex")]
	public extern Number FindLastIndex(Func<T, Number, Array<T>, object?> callbackfn, object? thisArg = null);

	[Description("@#findLastIndex")]
	public extern Number FindLastIndex(Func<T, Number, object?> callbackfn, object? thisArg = null);

		[Description("@#findLastIndex")]
		public extern Number FindLastIndex(Func<T, object?> callbackfn, object? thisArg = null);

		/// <summary>
		/// CLR convenience overload.
		/// Hidden because JavaScript array predicates are truthy/falsy-based and the object-returning overloads above are the primary runtime-shaped surface.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#findLastIndex")]
		public extern Number FindLastIndex(Predicate<T> callbackfn, object? thisArg = null);

	/// <summary>
	/// Returns a copied array with the elements in reverse order.
	/// This stays distinct from <see cref="Reverse"/> because JavaScript exposes a non-mutating copy-producing variant.
	/// </summary>
	[Description("@#toReversed")]
	public extern Array<T> ToReversed();

	/// <summary>
	/// Returns a copied array with its elements sorted.
	/// This stays distinct from <see cref="Sort(Func{T, T, Number}?)"/> because JavaScript exposes a non-mutating copy-producing variant.
	/// </summary>
	[Description("@#toSorted")]
	public extern Array<T> ToSorted(Func<T, T, Number>? compareFn = null);

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#toSorted")]
	public extern Array<T> ToSorted(Comparison<T> compareFn);

	/// <summary>
	/// Returns a copied array with items removed and optionally inserted at the given index.
	/// This mirrors JavaScript <c>Array.prototype.toSpliced</c>, which does not mutate the source array.
	/// </summary>
	[Description("@#toSpliced")]
	public extern Array<T> ToSpliced(Number start, Number? deleteCount = null);

	/// <summary>
	/// Returns a copied array with items removed and optionally inserted at the given index.
	/// This mirrors JavaScript <c>Array.prototype.toSpliced</c>, which does not mutate the source array.
	/// </summary>
	[Description("@#toSpliced")]
	public extern Array<T> ToSpliced(Number start, Number deleteCount, params T[] items);

	/// <summary>
	/// Returns a copied array with the element at the specified index replaced.
	/// Negative indices follow JavaScript <c>Array.prototype.with</c> semantics and count from the end.
	/// </summary>
	[Description("@#with")]
	public extern Array<T> With(Number index, T value);

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Array.prototype.keys()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<Number> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Array.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<T> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Array.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[index, value]</c>.
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<object?>> Entries();

	/// <summary>
	/// Creates an array from a JavaScript iterable or array-like value.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// </summary>
	[Description("@#from")]
	public extern static Array<T> From(IEnumerable<T> arrayLike);

	/// <summary>
	/// Creates an array from a JavaScript iterable or array-like value.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// </summary>
	[Description("@#from")]
	public extern static Array<T> From<U>(IEnumerable<U> arrayLike, Func<U, Number, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from a JavaScript iterable or array-like value.
	/// This overload mirrors JavaScript <c>Array.from</c> when the caller does not need the element index in the mapping callback.
	/// </summary>
	[Description("@#from")]
	public extern static Array<T> From<U>(IEnumerable<U> arrayLike, Func<U, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from a JavaScript async iterable or iterable value.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync(IEnumerable<T> arrayLike);

	/// <summary>
	/// Creates an array from promise-like JavaScript items and awaits each element before storing it.
	/// <see cref="IPromise{T}"/> is used as the host surface for JavaScript promise-like values.
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync(IEnumerable<IPromise<T>> arrayLike);

	/// <summary>
	/// Creates an array from a JavaScript async iterable or iterable value.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, Number, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from a JavaScript iterable and awaits each mapper result before storing it.
	/// This matches the JavaScript case where <c>Array.fromAsync</c> receives an async mapping callback.
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, Number, IPromise<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Bridge-only overload for compiler-lowered async mapping callbacks.
	/// JavaScript still sees the usual async mapper behavior; the bridge type only exists on the C# side.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, Number, PromiseResult<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from promise-like JavaScript items and applies a synchronous mapping callback to the awaited source values.
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, Number, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from promise-like JavaScript items and applies an async mapping callback to the awaited source values.
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, Number, IPromise<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Bridge-only overload for compiler-lowered async mapping callbacks over promise-like source items.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, Number, PromiseResult<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from a JavaScript async iterable or iterable value.
	/// This overload mirrors JavaScript <c>Array.fromAsync</c> when the caller does not need the element index in the mapping callback.
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from a JavaScript iterable and awaits each mapper result before storing it.
	/// This overload mirrors JavaScript <c>Array.fromAsync</c> when the caller does not need the element index in the async mapping callback.
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, IPromise<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Bridge-only overload for compiler-lowered async mapping callbacks without the index parameter.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<U> arrayLike, Func<U, PromiseResult<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from promise-like JavaScript items and applies a synchronous mapping callback to the awaited source values.
	/// This overload mirrors JavaScript <c>Array.fromAsync</c> when the caller does not need the element index in the mapping callback.
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from promise-like JavaScript items and applies an async mapping callback to the awaited source values.
	/// This overload mirrors JavaScript <c>Array.fromAsync</c> when the caller does not need the element index in the mapping callback.
	/// </summary>
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, IPromise<T>> mapFn, object? thisArg = null);

	/// <summary>
	/// Bridge-only overload for compiler-lowered async mapping callbacks over promise-like source items without the index parameter.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#fromAsync")]
	public extern static IPromise<Array<T>> FromAsync<U>(IEnumerable<IPromise<U>> arrayLike, Func<U, PromiseResult<T>> mapFn, object? thisArg = null);

	[Description("@#isArray")]
	public extern static bool IsArray(object? value);

	[Description("@#of")]
	public extern static Array<T> Of(params T[] value);

	/// <summary>
	/// CLR bridge members kept for collection-initializer and collection-like interop.
	/// They do not correspond to distinct JavaScript <c>Array.prototype</c> members.
	/// </summary>
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
		/// <summary>
		/// Mirrors JavaScript's <c>Array.isArray</c> static check on the global <c>Array</c> constructor host.
		/// </summary>
		[Description("@#isArray")]
		public extern static bool IsArray(object? obj);
	}
}
