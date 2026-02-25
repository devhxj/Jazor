namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Array","System/ArrayModule.js")]
public static class ArrayModule
{
	[Jazor(Op.Discard ,"System.Array.Length.get")]
	public extern static Number _fdebc1c5c62f33cc(System.Array instance);

	[Jazor(Op.Discard ,"System.Array.LongLength.get")]
	public extern static BigInt _82dc944f60373152(System.Array instance);

	[Jazor(Op.Discard ,"System.Array.Rank.get")]
	public extern static Number _6ab1259f55d0dd24(System.Array instance);

	///<summary>Initializes every element of the value-type <see cref="T:System.Array" /> by calling the parameterless constructor of the value type.</summary>
	[Jazor(Op.Discard ,"System.Array.Initialize()")]
	public extern static void _a93e4c6dc74a4cff(System.Array instance);

	///<summary>Returns a read-only wrapper for the specified array.</summary>
	[Jazor(Op.Discard ,"static System.Array.AsReadOnly<T>(T[])")]
	public extern static Array<T> _abd52ebcdb6fefcb<T>(Array<T> array);

	///<summary>Changes the number of elements of a one-dimensional array to the specified new size.</summary>
	[Jazor(Op.Discard ,"static System.Array.Resize<T>(ref T[], int)")]
	public extern static Array<object?> _127013d39cf5bff9<T>(ref Array<T>? array, Number newSize);

	///<summary>Creates a one-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and length, with zero-based indexing.</summary>
	[Jazor(Op.Discard ,"static System.Array.CreateInstance(System.Type, int)")]
	public extern static System.Array _7cf4f1d72cf2dca7(object elementType, Number length);

	///<summary>Creates a two-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing.</summary>
	[Jazor(Op.Discard ,"static System.Array.CreateInstance(System.Type, int, int)")]
	public extern static System.Array _3800bc5f99a65eb7(object elementType, Number length1, Number length2);

	///<summary>Creates a three-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing.</summary>
	[Jazor(Op.Discard ,"static System.Array.CreateInstance(System.Type, int, int, int)")]
	public extern static System.Array _946705c3abbbb67c(object elementType, Number length1, Number length2, Number length3);

	///<summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing. The dimension lengths are specified in an array of 32-bit integers.</summary>
	[Jazor(Op.Discard ,"static System.Array.CreateInstance(System.Type, params int[])")]
	public extern static System.Array _55c950cf5ea775e9(object elementType,  object lengths);

	///<summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with the specified lower bounds.</summary>
	[Jazor(Op.Discard ,"static System.Array.CreateInstance(System.Type, int[], int[])")]
	public extern static System.Array _81e3451a7be5290d(object elementType, object lengths, object lowerBounds);

	///<summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing. The dimension lengths are specified in an array of 64-bit integers.</summary>
	[Jazor(Op.Discard ,"static System.Array.CreateInstance(System.Type, params long[])")]
	public extern static System.Array _d1e6f82b64452f99(object elementType,  object lengths);

	///<summary>Creates a one-dimensional <see cref="T:System.Array" /> of the specified array type and length, with zero-based indexing.</summary>
	[Jazor(Op.Discard ,"static System.Array.CreateInstanceFromArrayType(System.Type, int)")]
	public extern static System.Array _8d8c533adf78f2c2(object arrayType, Number length);

	///<summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing.</summary>
	[Jazor(Op.Discard ,"static System.Array.CreateInstanceFromArrayType(System.Type, params int[])")]
	public extern static System.Array _11529b7770340ce8(object arrayType,  object lengths);

	///<summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with the specified lower bounds.</summary>
	[Jazor(Op.Discard ,"static System.Array.CreateInstanceFromArrayType(System.Type, int[], int[])")]
	public extern static System.Array _c78b33d4f8633a9b(object arrayType, object lengths, object lowerBounds);

	///<summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the first element and pastes them into another <see cref="T:System.Array" /> starting at the first element. The length is specified as a 64-bit integer.</summary>
	[Jazor(Op.Discard ,"static System.Array.Copy(System.Array, System.Array, long)")]
	public extern static void _7a3d7a78ff429283(object sourceArray, object destinationArray, BigInt length);

	///<summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the specified source index and pastes them to another <see cref="T:System.Array" /> starting at the specified destination index. The length and the indexes are specified as 64-bit integers.</summary>
	[Jazor(Op.Discard ,"static System.Array.Copy(System.Array, long, System.Array, long, long)")]
	public extern static void _e2bd26f0b897dcdc(object sourceArray, BigInt sourceIndex, object destinationArray, BigInt destinationIndex, BigInt length);

	///<summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the specified source index and pastes them to another <see cref="T:System.Array" /> starting at the specified destination index.  Guarantees that all changes are undone if the copy does not succeed completely.</summary>
	[Jazor(Op.Discard ,"static System.Array.ConstrainedCopy(System.Array, int, System.Array, int, int)")]
	public extern static void _e83857a6975e2bca(object sourceArray, Number sourceIndex, object destinationArray, Number destinationIndex, Number length);

	///<summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the first element and pastes them into another <see cref="T:System.Array" /> starting at the first element. The length is specified as a 32-bit integer.</summary>
	[Jazor(Op.Discard ,"static System.Array.Copy(System.Array, System.Array, int)")]
	public extern static void _236e3a8894f7381f(object sourceArray, object destinationArray, Number length);

	///<summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the specified source index and pastes them to another <see cref="T:System.Array" /> starting at the specified destination index. The length and the indexes are specified as 32-bit integers.</summary>
	[Jazor(Op.Discard ,"static System.Array.Copy(System.Array, int, System.Array, int, int)")]
	public extern static void _5afb5659a201668f(object sourceArray, Number sourceIndex, object destinationArray, Number destinationIndex, Number length);

	///<summary>Clears the contents of an array.</summary>
	[Jazor(Op.Discard ,"static System.Array.Clear(System.Array)")]
	public extern static void _96774f9ec153a919(object array);

	///<summary>Sets a range of elements in an array to the default value of each element type.</summary>
	[Jazor(Op.Discard ,"static System.Array.Clear(System.Array, int, int)")]
	public extern static void _e6e9140591777519(object array, Number index, Number length);

	///<summary>Gets a 32-bit integer that represents the number of elements in the specified dimension of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"System.Array.GetLength(int)")]
	public extern static Number _4a62a6d3092e758c(System.Array instance, Number dimension);

	///<summary>Gets the index of the last element of the specified dimension in the array.</summary>
	[Jazor(Op.Discard ,"System.Array.GetUpperBound(int)")]
	public extern static Number _240013ed6fb455ce(System.Array instance, Number dimension);

	///<summary>Gets the index of the first element of the specified dimension in the array.</summary>
	[Jazor(Op.Discard ,"System.Array.GetLowerBound(int)")]
	public extern static Number _de93a1deaab12d20(System.Array instance, Number dimension);

	///<summary>Gets the value at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 32-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.GetValue(params int[])")]
	public extern static object? _e938260256ca4a08(System.Array instance,  object indices);

	///<summary>Gets the value at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 32-bit integer.</summary>
	[Jazor(Op.Discard ,"System.Array.GetValue(int)")]
	public extern static object? _eba14f0435c17445(System.Array instance, Number index);

	///<summary>Gets the value at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.GetValue(int, int)")]
	public extern static object? _c479de104d41183c(System.Array instance, Number index1, Number index2);

	///<summary>Gets the value at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.GetValue(int, int, int)")]
	public extern static object? _a9dc664f06ce55a4(System.Array instance, Number index1, Number index2, Number index3);

	///<summary>Sets a value to the element at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 32-bit integer.</summary>
	[Jazor(Op.Discard ,"System.Array.SetValue(object, int)")]
	public extern static void _1f2a45eb847a2ec4(System.Array instance, object? value, Number index);

	///<summary>Sets a value to the element at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.SetValue(object, int, int)")]
	public extern static void _7ca03dfc64fd5640(System.Array instance, object? value, Number index1, Number index2);

	///<summary>Sets a value to the element at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.SetValue(object, int, int, int)")]
	public extern static void _a8dff91417f83303(System.Array instance, object? value, Number index1, Number index2, Number index3);

	///<summary>Sets a value to the element at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 32-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.SetValue(object, params int[])")]
	public extern static void _8752076a83fbb3f1(System.Array instance, object? value,  object indices);

	///<summary>Gets the value at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 64-bit integer.</summary>
	[Jazor(Op.Discard ,"System.Array.GetValue(long)")]
	public extern static object? _99c592f7140b4f20(System.Array instance, BigInt index);

	///<summary>Gets the value at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.GetValue(long, long)")]
	public extern static object? _2bad686c503b1e40(System.Array instance, BigInt index1, BigInt index2);

	///<summary>Gets the value at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.GetValue(long, long, long)")]
	public extern static object? _8e8e4b0752cd3155(System.Array instance, BigInt index1, BigInt index2, BigInt index3);

	///<summary>Gets the value at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 64-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.GetValue(params long[])")]
	public extern static object? _6a12948779406121(System.Array instance,  object indices);

	///<summary>Sets a value to the element at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 64-bit integer.</summary>
	[Jazor(Op.Discard ,"System.Array.SetValue(object, long)")]
	public extern static void _d845170315112950(System.Array instance, object? value, BigInt index);

	///<summary>Sets a value to the element at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.SetValue(object, long, long)")]
	public extern static void _24864536d32c0b93(System.Array instance, object? value, BigInt index1, BigInt index2);

	///<summary>Sets a value to the element at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.SetValue(object, long, long, long)")]
	public extern static void _314db333058e554d(System.Array instance, object? value, BigInt index1, BigInt index2, BigInt index3);

	///<summary>Sets a value to the element at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 64-bit integers.</summary>
	[Jazor(Op.Discard ,"System.Array.SetValue(object, params long[])")]
	public extern static void _e3923681669a96b5(System.Array instance, object? value,  object indices);

	///<summary>Gets a 64-bit integer that represents the number of elements in the specified dimension of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"System.Array.GetLongLength(int)")]
	public extern static BigInt _b529d6e54112cf3e(System.Array instance, Number dimension);

	[Jazor(Op.Discard ,"System.Array.SyncRoot.get")]
	public extern static object _5df324fc2064bf14(System.Array instance);

	[Jazor(Op.Discard ,"System.Array.IsReadOnly.get")]
	public extern static bool _957efa892fba2b42(System.Array instance);

	[Jazor(Op.Discard ,"System.Array.IsFixedSize.get")]
	public extern static bool _af3654cc2dd2fa42(System.Array instance);

	[Jazor(Op.Discard ,"System.Array.IsSynchronized.get")]
	public extern static bool _818cd5ec440253da(System.Array instance);

	///<summary>Creates a shallow copy of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"System.Array.Clone()")]
	public extern static object _7b75e1326e081bb2(System.Array instance);

	///<summary>Searches an entire one-dimensional sorted array for a specific element, using the <see cref="T:System.IComparable" /> interface implemented by each element of the array and by the specified object.</summary>
	[Jazor(Op.Discard ,"static System.Array.BinarySearch(System.Array, object)")]
	public extern static Number _0c9e99640a975a5b(object array, object? value);

	///<summary>Searches a range of elements in a one-dimensional sorted array for a value, using the <see cref="T:System.IComparable" /> interface implemented by each element of the array and by the specified value.</summary>
	[Jazor(Op.Discard ,"static System.Array.BinarySearch(System.Array, int, int, object)")]
	public extern static Number _fa538add1f784012(object array, Number index, Number length, object? value);

	///<summary>Searches an entire one-dimensional sorted array for a value using the specified <see cref="T:System.Collections.IComparer" /> interface.</summary>
	[Jazor(Op.Discard ,"static System.Array.BinarySearch(System.Array, object, System.Collections.IComparer)")]
	public extern static Number _c453dd981ecbb5c5(object array, object? value, object comparer);

	///<summary>Searches a range of elements in a one-dimensional sorted array for a value, using the specified <see cref="T:System.Collections.IComparer" /> interface.</summary>
	[Jazor(Op.Discard ,"static System.Array.BinarySearch(System.Array, int, int, object, System.Collections.IComparer)")]
	public extern static Number _f1fb5c20cf9ffd4d(object array, Number index, Number length, object? value, object comparer);

	///<summary>Searches an entire one-dimensional sorted array for a specific element, using the <see cref="T:System.IComparable`1" /> generic interface implemented by each element of the <see cref="T:System.Array" /> and by the specified object.</summary>
	[Jazor(Op.Discard ,"static System.Array.BinarySearch<T>(T[], T)")]
	public extern static Number _75258b66e0bba01a<T>(Array<T> array, object value);

	///<summary>Searches an entire one-dimensional sorted array for a value using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Discard ,"static System.Array.BinarySearch<T>(T[], T, System.Collections.Generic.IComparer<T>)")]
	public extern static Number _87f2af26c36fed01<T>(Array<T> array, object value, IComparer<T>? comparer);

	///<summary>Searches a range of elements in a one-dimensional sorted array for a value, using the <see cref="T:System.IComparable`1" /> generic interface implemented by each element of the <see cref="T:System.Array" /> and by the specified value.</summary>
	[Jazor(Op.Discard ,"static System.Array.BinarySearch<T>(T[], int, int, T)")]
	public extern static Number _60003ac825620c60<T>(Array<T> array, Number index, Number length, object value);

	///<summary>Searches a range of elements in a one-dimensional sorted array for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Discard ,"static System.Array.BinarySearch<T>(T[], int, int, T, System.Collections.Generic.IComparer<T>)")]
	public extern static Number _42b1da24db771714<T>(Array<T> array, Number index, Number length, object value, IComparer<T>? comparer);

	///<summary>Converts an array of one type to an array of another type.</summary>
	[Jazor(Op.Discard ,"static System.Array.ConvertAll<TInput, TOutput>(TInput[], System.Converter<TInput, TOutput>)")]
	public extern static TOutput[] _a73f4ff0bddcc6f6<TInput, TOutput>(object array, object converter);

	///<summary>Copies all the elements of the current one-dimensional array to the specified one-dimensional array starting at the specified destination array index. The index is specified as a 32-bit integer.</summary>
	[Jazor(Op.Discard ,"System.Array.CopyTo(System.Array, int)")]
	public extern static void _559d75b1e44b3eb0(System.Array instance, object array, Number index);

	///<summary>Copies all the elements of the current one-dimensional array to the specified one-dimensional array starting at the specified destination array index. The index is specified as a 64-bit integer.</summary>
	[Jazor(Op.Discard ,"System.Array.CopyTo(System.Array, long)")]
	public extern static void _02714528e8c676b0(System.Array instance, object array, BigInt index);

	///<summary>Returns an empty array.</summary>
	[Jazor(Op.Discard ,"static System.Array.Empty<T>()")]
	public extern static Array<T> _b36a1b49fd533b3e<T>();

	///<summary>Determines whether the specified array contains elements that match the conditions defined by the specified predicate.</summary>
	[Jazor(Op.Discard ,"static System.Array.Exists<T>(T[], System.Predicate<T>)")]
	public extern static bool _3795c9344e3fe39f<T>(Array<T> array, Predicate<T> match);

	///<summary>Assigns the given <paramref name="value" /> of type <typeparamref name="T" /> to each element of the specified <paramref name="array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Fill<T>(T[], T)")]
	public extern static void _65ab99eba8176bda<T>(Array<T> array, object value);

	///<summary>Assigns the given <paramref name="value" /> of type <typeparamref name="T" /> to the elements of the specified <paramref name="array" /> which are          within the range of <paramref name="startIndex" /> (inclusive) and the next <paramref name="count" /> number of indices.</summary>
	[Jazor(Op.Discard ,"static System.Array.Fill<T>(T[], T, int, int)")]
	public extern static void _8edf171ab37f3a05<T>(Array<T> array, object value, Number startIndex, Number count);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Find<T>(T[], System.Predicate<T>)")]
	public extern static T? _1dfc77048ccf0234<T>(Array<T> array, Predicate<T> match);

	///<summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
	[Jazor(Op.Discard ,"static System.Array.FindAll<T>(T[], System.Predicate<T>)")]
	public extern static Array<T> _b373eb093e6c7b63<T>(Array<T> array, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.FindIndex<T>(T[], System.Predicate<T>)")]
	public extern static Number _64f5a7fd5c436edb<T>(Array<T> array, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the specified index to the last element.</summary>
	[Jazor(Op.Discard ,"static System.Array.FindIndex<T>(T[], int, System.Predicate<T>)")]
	public extern static Number _42e008ba24b77e94<T>(Array<T> array, Number startIndex, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that starts at the specified index and contains the specified number of elements.</summary>
	[Jazor(Op.Discard ,"static System.Array.FindIndex<T>(T[], int, int, System.Predicate<T>)")]
	public extern static Number _fdfc005bdc859fff<T>(Array<T> array, Number startIndex, Number count, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.FindLast<T>(T[], System.Predicate<T>)")]
	public extern static T? _2786abe2cff245fa<T>(Array<T> array, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.FindLastIndex<T>(T[], System.Predicate<T>)")]
	public extern static Number _ea3118f38aa5f363<T>(Array<T> array, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	[Jazor(Op.Discard ,"static System.Array.FindLastIndex<T>(T[], int, System.Predicate<T>)")]
	public extern static Number _56359f972a00ab73<T>(Array<T> array, Number startIndex, Predicate<T> match);

	///<summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	[Jazor(Op.Discard ,"static System.Array.FindLastIndex<T>(T[], int, int, System.Predicate<T>)")]
	public extern static Number _6b63489e941ef0f0<T>(Array<T> array, Number startIndex, Number count, Predicate<T> match);

	///<summary>Performs the specified action on each element of the specified array.</summary>
	[Jazor(Op.Discard ,"static System.Array.ForEach<T>(T[], System.Action<T>)")]
	public extern static void _ad1c39ab55fe27b9<T>(Array<T> array, object action);

	///<summary>Searches for the specified object and returns the index of its first occurrence in a one-dimensional array.</summary>
	[Jazor(Op.Discard ,"static System.Array.IndexOf(System.Array, object)")]
	public extern static Number _cde8d7a78af8dc9a(object array, object? value);

	///<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of its first occurrence. The range extends from a specified index to the end of the array.</summary>
	[Jazor(Op.Discard ,"static System.Array.IndexOf(System.Array, object, int)")]
	public extern static Number _2151f4cd0a63b0a2(object array, object? value, Number startIndex);

	///<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of ifs first occurrence. The range extends from a specified index for a specified number of elements.</summary>
	[Jazor(Op.Discard ,"static System.Array.IndexOf(System.Array, object, int, int)")]
	public extern static Number _c419efc216312a6a(object array, object? value, Number startIndex, Number count);

	///<summary>Searches for the specified object and returns the index of its first occurrence in a one-dimensional array.</summary>
	[Jazor(Op.Discard ,"static System.Array.IndexOf<T>(T[], T)")]
	public extern static Number _34e8668cac3c06fa<T>(Array<T> array, object value);

	///<summary>Searches for the specified object in a range of elements of a one dimensional array, and returns the index of its first occurrence. The range extends from a specified index to the end of the array.</summary>
	[Jazor(Op.Discard ,"static System.Array.IndexOf<T>(T[], T, int)")]
	public extern static Number _d7a4d17a98a17e7e<T>(Array<T> array, object value, Number startIndex);

	///<summary>Searches for the specified object in a range of elements of a one-dimensional array, and returns the index of its first occurrence. The range extends from a specified index for a specified number of elements.</summary>
	[Jazor(Op.Discard ,"static System.Array.IndexOf<T>(T[], T, int, int)")]
	public extern static Number _e3d80b27a67e8a0d<T>(Array<T> array, object value, Number startIndex, Number count);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the entire one-dimensional <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.LastIndexOf(System.Array, object)")]
	public extern static Number _85801a2dbc247f17(object array, object? value);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	[Jazor(Op.Discard ,"static System.Array.LastIndexOf(System.Array, object, int)")]
	public extern static Number _6b23455f7b2f95ff(object array, object? value, Number startIndex);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	[Jazor(Op.Discard ,"static System.Array.LastIndexOf(System.Array, object, int, int)")]
	public extern static Number _7f5af90fd2a084fe(object array, object? value, Number startIndex, Number count);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.LastIndexOf<T>(T[], T)")]
	public extern static Number _198d0f4fcb1c0679<T>(Array<T> array, object value);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	[Jazor(Op.Discard ,"static System.Array.LastIndexOf<T>(T[], T, int)")]
	public extern static Number _5c2c6aa99d0e0549<T>(Array<T> array, object value, Number startIndex);

	///<summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	[Jazor(Op.Discard ,"static System.Array.LastIndexOf<T>(T[], T, int, int)")]
	public extern static Number _b5bf131d8947c855<T>(Array<T> array, object value, Number startIndex, Number count);

	///<summary>Reverses the sequence of the elements in the entire one-dimensional <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Reverse(System.Array)")]
	public extern static void _c02ce18f02385f3d(object array);

	///<summary>Reverses the sequence of a subset of the elements in the one-dimensional <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Reverse(System.Array, int, int)")]
	public extern static void _36c04f95b4ffdfd5(object array, Number index, Number length);

	///<summary>Reverses the sequence of the elements in the one-dimensional generic array.</summary>
	[Jazor(Op.Discard ,"static System.Array.Reverse<T>(T[])")]
	public extern static void _e2b02681782c394b<T>(Array<T> array);

	///<summary>Reverses the sequence of a subset of the elements in the one-dimensional generic array.</summary>
	[Jazor(Op.Discard ,"static System.Array.Reverse<T>(T[], int, int)")]
	public extern static void _5b0cbdf276c63339<T>(Array<T> array, Number index, Number length);

	///<summary>Sorts the elements in an entire one-dimensional <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort(System.Array)")]
	public extern static void _07ee8311aaf13b6b(object array);

	///<summary>Sorts a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each key.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort(System.Array, System.Array)")]
	public extern static void _4df21ca760120c59(object keys, object items);

	///<summary>Sorts the elements in a range of elements in a one-dimensional <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort(System.Array, int, int)")]
	public extern static void _4e10132b81a43421(object array, Number index, Number length);

	///<summary>Sorts a range of elements in a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each key.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort(System.Array, System.Array, int, int)")]
	public extern static void _12789d2affa27035(object keys, object items, Number index, Number length);

	///<summary>Sorts the elements in a one-dimensional <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort(System.Array, System.Collections.IComparer)")]
	public extern static void _093c373956602c04(object array, object comparer);

	///<summary>Sorts a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort(System.Array, System.Array, System.Collections.IComparer)")]
	public extern static void _122404a1fc2867ba(object keys, object items, object comparer);

	///<summary>Sorts the elements in a range of elements in a one-dimensional <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort(System.Array, int, int, System.Collections.IComparer)")]
	public extern static void _b2141b8c013bc1b0(object array, Number index, Number length, object comparer);

	///<summary>Sorts a range of elements in a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort(System.Array, System.Array, int, int, System.Collections.IComparer)")]
	public extern static void _a95c3f83e8cd4623(object keys, object items, Number index, Number length, object comparer);

	///<summary>Sorts the elements in an entire <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort<T>(T[])")]
	public extern static void _382add2bad872f67<T>(Array<T> array);

	///<summary>Sorts a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each key.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort<TKey, TValue>(TKey[], TValue[])")]
	public extern static void _1a3ebd994898c67c<TKey, TValue>(object keys, object items);

	///<summary>Sorts the elements in a range of elements in an <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each element of the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort<T>(T[], int, int)")]
	public extern static void _80e6f8922ae8703c<T>(Array<T> array, Number index, Number length);

	///<summary>Sorts a range of elements in a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each key.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int)")]
	public extern static void _9b803c8e781cf3c0<TKey, TValue>(object keys, object items, Number index, Number length);

	///<summary>Sorts the elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort<T>(T[], System.Collections.Generic.IComparer<T>)")]
	public extern static void _92474aed4e4823f3<T>(Array<T> array, IComparer<T>? comparer);

	///<summary>Sorts a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort<TKey, TValue>(TKey[], TValue[], System.Collections.Generic.IComparer<TKey>)")]
	public extern static void _dfd5fefaaa03a228<TKey, TValue>(object keys, object items, object comparer);

	///<summary>Sorts the elements in a range of elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort<T>(T[], int, int, System.Collections.Generic.IComparer<T>)")]
	public extern static void _55dbc52295bd7984<T>(Array<T> array, Number index, Number length, IComparer<T>? comparer);

	///<summary>Sorts a range of elements in a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort<TKey, TValue>(TKey[], TValue[], int, int, System.Collections.Generic.IComparer<TKey>)")]
	public extern static void _f3e7263659ac2e30<TKey, TValue>(object keys, object items, Number index, Number length, object comparer);

	///<summary>Sorts the elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Comparison`1" />.</summary>
	[Jazor(Op.Discard ,"static System.Array.Sort<T>(T[], System.Comparison<T>)")]
	public extern static void _c8fcae59a3aca6f6<T>(Array<T> array, Comparison<T> comparison);

	///<summary>Determines whether every element in the array matches the conditions defined by the specified predicate.</summary>
	[Jazor(Op.Discard ,"static System.Array.TrueForAll<T>(T[], System.Predicate<T>)")]
	public extern static bool _7deb21b3fbe579c9<T>(Array<T> array, Predicate<T> match);

	[Jazor(Op.Discard ,"static System.Array.MaxLength.get")]
	public extern static Number _a7a42b1fbdbc7628(System.Array instance);

	///<summary>Returns an <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Array" />.</summary>
	[Jazor(Op.Discard ,"System.Array.GetEnumerator()")]
	public extern static System.Collections.IEnumerator _1e9012cd200b3827(System.Array instance);
}
