using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

namespace ECMAScript;

[ECMAScript]
public record struct ArrayBufferOption(uint? MaxByteLength = null);

/// <summary>
/// 底层二进制数据缓冲区的数组视图
/// </summary>
[ECMAScript]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IBufferSource
{
}

[ECMAScript]
public interface IAllowSharedBufferSource
{

}

/// <summary>
/// 底层二进制数据缓冲区的数组视图
/// </summary>
[ECMAScript]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IArrayBufferView
{
	ArrayBuffer Buffer { get; }

	uint ByteLength { get; }

	uint ByteOffset { get; }
}

/// <summary>
/// 用来表示通用的原始二进制数据缓冲区。
/// 它是一个字节数组，通常在其他语言中称为“byte array”。你不能直接操作 ArrayBuffer 中的内容；而是要通过类型化数组对象或 DataView 对象来操作，它们会将缓冲区中的数据表示为特定的格式，并通过这些格式来读写缓冲区的内容。
/// ArrayBuffer() 构造函数创建一个以字节为单位的给定长度的新 ArrayBuffer。你也可以从现有的数据（例如，从 Base64 字符串或者从本地文件）获取数组缓冲区。
/// </summary>
[ECMAScript]
[Description("@#ArrayBuffer")]
public class ArrayBuffer : IBufferSource
{
	public extern ArrayBuffer(uint length);

	public extern ArrayBuffer(uint length, ArrayBufferOption? option = null);

	public extern static bool IsView(object arg);

	public extern virtual double ByteLength { get; }

	/// <summary>
	/// Returns a section of an TypedArrayBuffer.
	/// </summary>
	/// <param name="begin"></param>
	/// <param name="end"></param>
	/// <returns></returns>
	public extern virtual ArrayBuffer Slice(uint begin, uint? end = null);
}

[ECMAScript]
[Description("@#SharedArrayBuffer")]
public class SharedArrayBuffer : ArrayBuffer, IAllowSharedBufferSource
{
	public extern SharedArrayBuffer(uint length);
}

/// <summary>
/// 是一个可以从二进制 ArrayBuffer 对象中读写多种数值类型的底层接口，使用它时，不用考虑不同平台的字节序问题。
/// </summary>
[ECMAScript]
[Description("@#DataView")]
public class DataView : IArrayBufferView, IBufferSource
{
	public extern DataView(ArrayBuffer buffer, uint? byteOffset = null, uint? byteLength = null);

	public virtual ArrayBuffer Buffer { get; }

	public virtual uint ByteLength { get; }

	public virtual uint ByteOffset { get; }

	public virtual extern float GetFloat32(uint byteOffset);

	public virtual extern float GetFloat32(uint byteOffset, bool littleEndian);

	public virtual extern double GetFloat64(uint byteOffset);

	public virtual extern double GetFloat64(uint byteOffset, bool littleEndian);

	public virtual extern sbyte GetInt8(uint byteOffset);

	public virtual extern short GetInt16(uint byteOffset);

	public virtual extern short GetInt16(uint byteOffset, bool littleEndian);

	public virtual extern int GetInt32(uint byteOffset);

	public virtual extern int GetInt32(uint byteOffset, bool littleEndian);

	public virtual extern BigInt GetBigInt64(uint byteOffset, bool littleEndian);

	public virtual extern byte GetUint8(uint byteOffset);

	public virtual extern ushort GetUint16(uint byteOffset);

	public virtual extern ushort GetUint16(uint byteOffset, bool littleEndian);

	public virtual extern uint GetUint32(uint byteOffset);

	public virtual extern uint GetUint32(uint byteOffset, bool littleEndian);

	public virtual extern BigInt GetBigUint64(uint byteOffset, bool littleEndian);

	public virtual extern void SetFloat32(uint byteOffset, float value);

	public virtual extern void SetFloat32(uint byteOffset, float value, bool littleEndian);

	public virtual extern void SetFloat64(uint byteOffset, double value);

	public virtual extern void SetFloat64(uint byteOffset, double value, bool littleEndian);

	public virtual extern void SetInt8(uint byteOffset, sbyte value);

	public virtual extern void SetInt16(uint byteOffset, short value);

	public virtual extern void SetInt16(uint byteOffset, short value, bool littleEndian);

	public virtual extern void SetInt32(uint byteOffset, int value);

	public virtual extern void SetInt32(uint byteOffset, int value, bool littleEndian);

	public virtual extern void SetBigInt64(uint byteOffset, BigInt value, bool littleEndian);

	public virtual extern void SetUint8(uint byteOffset, byte value);

	public virtual extern void SetUint16(uint byteOffset, ushort value);

	public virtual extern void SetUint16(uint byteOffset, ushort value, bool littleEndian);

	public virtual extern void SetUint32(uint byteOffset, uint value);

	public virtual extern void SetUint32(uint byteOffset, uint value, bool littleEndian);

	public virtual extern void SetBigUint64(uint byteOffset, BigInt value, bool littleEndian);
}

/// <summary>
/// 底层二进制数据缓冲区的类数组视图
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TArray"></typeparam>
[ECMAScript]
public abstract class TypedArray<T, TArray> : IArrayBufferView, IBufferSource, IEnumerable<T>
	where T : IMinMaxValue<T>
	where TArray : TypedArray<T, TArray>
{
	public extern TypedArray(uint length);

	public extern TypedArray(IEnumerable<T> array);

	public extern TypedArray(ArrayBuffer buffer, uint? byteOffset = null, uint? length = null);

	/// <summary>
	/// The size in bytes of each element in the array.
	/// </summary>
	[Description("@#BYTES_PER_ELEMENT")]
	public extern static uint BYTES_PER_ELEMENT { get; }

	/// <summary>
	/// Returns a new array from a set of elements.
	/// </summary>
	/// <param name="value">A set of elements to include in the new array object.</param>
	/// <returns></returns>
	[Description("@#of")]
	public extern static TArray Of(params IEnumerable<T> items);

	/// <summary>
	/// Creates an array from an array-like or iterable object.
	/// </summary>
	/// <param name="arrayLike">An array-like object to convert to an array.</param>
	/// <returns></returns>
	[Description("@#from")]
	public extern static TArray From(IEnumerable<T> arrayLike);

	/// <summary>
	/// Creates an array from an array-like or iterable object.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="arrayLike">An array-like object to convert to an array.</param>
	/// <param name="mapFn">A mapping function to call on every element of the array.</param>
	/// <param name="thisArg">Value of 'this' used to invoke the mapfn.</param>
	/// <returns></returns>
	[Description("@#from")]
	public extern static TArray From<U>(IEnumerable<U> arrayLike, Func<U, T, T> mapFn, IEnumerable? thisArg = null);

	/// <summary>
	/// The ArrayBuffer instance referenced by the array.
	/// </summary>
	[Description("@#buffer")]
	public extern ArrayBuffer Buffer { get; }

	/// <summary>
	/// The length in bytes of the array.
	/// </summary>
	[Description("@#byteLength")]
	public extern uint ByteLength { get; }

	/// <summary>
	/// The offset in bytes of the array.
	/// </summary>
	[Description("@#byteOffset")]
	public extern uint ByteOffset { get; }

	/// <summary>
	/// Returns the this object after copying a section of the array identified by start and end
	/// to the same array starting at position target
	/// </summary>
	/// <param name="target">If target is negative, it is treated as length+target where length is the length of the array.</param>
	/// <param name="start">If start is negative, it is treated as length+start.If end is negative, it is treated as length+end.</param>
	/// <param name="end">If not specified, length of the this object is used as its default value.</param>
	/// <returns></returns>
	[Description("@#copyWithin")]
	public extern TArray CopyWithin(uint target, uint start, uint? end = null);

	/// <summary>
	/// Determines whether all the members of an array satisfy the specified test.
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: ICollection<T>) => unknown</b></para>A function that accepts up to three arguments. The every method calls the predicate function for each element in the array until the predicate returns a value which is coercible to the Boolean value false, or until the end of the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function.If thisArg is omitted, undefined is used as the this value.</param>
	/// <returns></returns>
	[Description("@#every")]
	public extern bool Every(Func<T, uint, TArray, object?> predicate, TArray? thisArg = null);

	/// <summary>
	/// Changes all array elements from `start` to `end` index to a static `value` and returns the modified array
	/// </summary>
	/// <param name="value">value to fill array section with</param>
	/// <param name="start">index to start filling the array at.If start is negative, it is treated as length+start where length is the length of the array.</param>
	/// <param name="end">index to stop filling the array at. If end is negative, it is treated as length+end.</param>
	/// <returns></returns>
	[Description("@#fill")]
	public extern TArray Fill(T value, uint? start = null, uint? end = null);

	/// <summary>
	/// Returns the elements of an array that meet the condition specified in a callback function.
	/// </summary>
	/// <param name="predicate">(value: number, index: number, array: this) => any,A function that accepts up to three arguments.The filter method calls the predicate function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function.If thisArg is omitted, undefined is used as the this value.</param>
	/// <returns></returns>
	[Description("@#filter")]
	public extern TArray Filter(Func<T, uint, TArray, object> predicate, TArray? thisArg = null);

	/// <summary>
	/// Returns the value of the first element in the array where predicate is true, and undefined otherwise.
	/// </summary>
	/// <param name="predicate">find calls predicate once for each element of the array, in ascending order, until it finds one where predicate returns true. If such an element is found, find immediately returns that element value. Otherwise, find returns undefined.</param>
	/// <param name="thisArg">If provided, it will be used as the this value for each invocation of predicate.If it is not provided, undefined is used instead.</param>
	/// <returns></returns>
	[Description("@#find")]
	public extern T? Find(Func<T, uint, TArray, bool> predicate, TArray? thisArg = null);

	/// <summary>
	/// Returns the index of the first element in the array where predicate is true, and -1 otherwise.
	/// </summary>
	/// <param name="predicate">(value: number, index: number, obj: this) => boolean,find calls predicate once for each element of the array, in ascending order, until it finds one where predicate returns true. If such an element is found,findIndex immediately returns that element index.Otherwise, findIndex returns -1.</param>
	/// <param name="thisArg">If provided, it will be used as the this value for each invocation of predicate.If it is not provided, undefined is used instead.</param>
	/// <returns></returns>
	[Description("@#findIndex")]
	public extern uint FindIndex(Func<T, uint, TArray, bool> predicate, TArray? thisArg = null);

	/// <summary>
	/// Performs the specified action for each element in an array.
	/// </summary>
	/// <param name="callbackfn">(value: number, index: number, array: this) => void,A function that accepts up to three arguments.forEach calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the callbackfn function.If thisArg is omitted, undefined is used as the this value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<T, uint, TArray> callbackfn, TArray? thisArg = null);

	/// <summary>
	/// Returns the index of the first occurrence of a value in an array.
	/// </summary>
	/// <param name="searchElement">The value to locate in the array.</param>
	/// <param name="fromIndex">The array index at which to begin the search.If fromIndex is omitted, the search starts at index 0.</param>
	/// <returns></returns>
	[Description("@#indexOf")]
	public extern uint IndexOf(uint searchElement, uint? fromIndex = null);

	/// <summary>
	/// Adds all the elements of an array separated by the specified separator string.
	/// </summary>
	/// <param name="separator">A string used to separate one element of an array from the next in the resulting String.If omitted, the array elements are separated with a comma.</param>
	/// <returns></returns>
	[Description("@#join")]
	public extern string Join(string? separator = null);

	/// <summary>
	/// Returns the index of the last occurrence of a value in an array.
	/// </summary>
	/// <param name="searchElement">The value to locate in the array.</param>
	/// <param name="fromIndex">The array index at which to begin the search.If fromIndex is omitted, the search starts at index 0.</param>
	/// <returns></returns>
	[Description("@#lastIndexOf")]
	public extern uint LastIndexOf(uint searchElement, uint? fromIndex = null);

	/// <summary>
	/// The length of the array.
	/// </summary>
	[Description("@#length")]
	public extern uint Length { get; }

	/// <summary>
	/// Calls a defined callback function on each element of an array, and returns an array that contains the results.
	/// </summary>
	/// <param name="callbackfn">(value: number, index: number, array: this) => number,A function that accepts up to three arguments.The map method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the callbackfn function.If thisArg is omitted, undefined is used as the this value.</param>
	/// <returns></returns>
	[Description("@#map")]
	public extern TArray Map(Func<T, uint, TArray, double> callbackfn, TArray? thisArg = null);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <param name="callbackfn">(previousValue: number, currentValue: number, currentIndex: number, array: this) => number,A function that accepts up to four arguments.The reduce method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, uint, TArray, T> callbackfn, T? initialValue = default);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="callbackfn"><para><b>(previousValue: U, currentValue: number, currentIndex: number, array: this) => U</b></para>A function that accepts up to four arguments.The reduce method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduce")]
	public extern U Reduce<U>(Func<U, T, uint, TArray, U> callbackfn, U initialValue);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <param name="callbackfn"><para><b>(previousValue: number, currentValue: number, currentIndex: number, array: this) => number </b></para>A function that accepts up to four arguments.The reduceRight method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, uint, TArray, T> callbackfn, T initialValue);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="callbackfn">(previousValue: U, currentValue: number, currentIndex: number, array: this) => U,A function that accepts up to four arguments.The reduceRight method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduceRight")]
	public extern U ReduceRight<U>(Func<U, T, uint, TArray, U> callbackfn, U initialValue);

	/// <summary>
	/// Reverses the elements in an Array.
	/// </summary>
	/// <returns></returns>
	[Description("@#reverse")]
	public extern TArray Reverse();

	/// <summary>
	/// Sets a value or an array of values.
	/// </summary>
	/// <param name="array">A typed or untyped array of values to set.</param>
	/// <param name="offset">The index in the current array at which the values are to be written.</param>
	[Description("@#set")]
	public extern void Set(IEnumerable<T> array, uint? offset = null);

	/// <summary>
	/// Returns a section of an array.
	/// </summary>
	/// <param name="start">The beginning of the specified portion of the array.</param>
	/// <param name="end">The end of the specified portion of the array. This is exclusive of the element at the index 'end'.</param>
	/// <returns></returns>
	[Description("@#slice")]
	public extern TArray Slice(uint? start, uint? end);

	/// <summary>
	/// Determines whether the specified callback function returns true for any element of an array.
	/// </summary>
	/// <param name="predicate">(value: number, index: number, array: this) => unknown,A function that accepts up to three arguments.The some method calls the predicate function for each element in the array until the predicate returns a value which is coercible to the Boolean value true, or until the end of the array.</param>
	/// <param name="thisArg">An object to which the this keyword can refer in the predicate function.If thisArg is omitted, undefined is used as the this value.</param>
	/// <returns></returns>
	[Description("@#some")]
	public extern bool Some(Func<T, uint, TArray, object?> predicate, TArray? thisArg = null);

	/// <summary>
	/// Sorts an array.
	/// </summary>
	/// <param name="compareFn">(a: number, b: number) => number,Function used to determine the order of the elements.It is expected to return a negative value if first argument is less than second argument, zero if they're equal and a positive value otherwise.If omitted, the elements are sorted in ascending order.</param>
	/// <returns></returns>
	[Description("@#sort")]
	public extern TArray Sort(Func<T, T, uint> compareFn);

	/// <summary>
	/// Gets a new TArray view of the ArrayBuffer store for this array, referencing the elements at begin, inclusive, up to end, exclusive.
	/// </summary>
	/// <param name="begin">The index of the beginning of the array.</param>
	/// <param name="end">The index of the end of the array.</param>
	/// <returns></returns>
	[Description("@#subarray")]
	public extern TArray Subarray(uint? begin, uint? end);

	///// <summary>
	///// Converts a number to a string by using the current locale.
	///// </summary>
	///// <returns></returns>
	//[Description("@#toLocaleString")]
	//public extern string ToLocaleString();

	extern IEnumerator<T> IEnumerable<T>.GetEnumerator();

	extern IEnumerator IEnumerable.GetEnumerator();

	public extern T this[uint index] { get; set; }

	//[EditorBrowsable(EditorBrowsableState.Never)]
	//public extern override Object this[string name] { get; set; }
}

/// <summary>
/// 64 位有符号整型（补码）数组视图，-2^63 到 2^63 - 1
/// </summary>
[ECMAScript]
[Description("@#BigInt64Array")]
public class BigInt64Array : TypedArray<long, BigInt64Array>
{
	public extern BigInt64Array(uint length);

	public extern BigInt64Array(BigInt64Array array);

	public extern BigInt64Array(IEnumerable<long> array);

	public extern BigInt64Array(IArrayBufferView array);

	public extern BigInt64Array(ArrayBuffer buffer, uint? byteOffset = null, uint? length = null);
}

/// <summary>
/// 64 位无符号整型数组视图，0 到 2^64 - 1
/// </summary>
[ECMAScript]
[Description("@#BigUint64Array")]
public class BigUint64Array : TypedArray<ulong, BigUint64Array>
{
	public extern BigUint64Array(uint length);

	public extern BigUint64Array(BigUint64Array array);

	public extern BigUint64Array(IEnumerable<ulong> array);

	public extern BigUint64Array(IArrayBufferView array);

	public extern BigUint64Array(ArrayBuffer buffer, uint? ulongOffset = null, uint? length = null);
}

[ECMAScript]
[Description("@#Float16Array")]
public class Float16Array : TypedArray<float, Float32Array>
{
	public extern Float16Array(uint length);

	public extern Float16Array(Float32Array array);

	public extern Float16Array(IEnumerable<float> array);

	public extern Float16Array(IArrayBufferView array);

	public extern Float16Array(ArrayBuffer buffer, uint? byteOffset = null, uint? length = null);
}

/// <summary>
/// 32 位 IEEE 浮点数（7 位有效数字，例如 1.234567）数组视图，-3.4E38 到 3.4E38 并且 1.2E-38 是最小的正数
/// </summary>
[ECMAScript]
[Description("@#Float32Array")]
public class Float32Array : TypedArray<float, Float32Array>
{
	public extern Float32Array(uint length);

	public extern Float32Array(Float32Array array);

	public extern Float32Array(IEnumerable<float> array);

	public extern Float32Array(IArrayBufferView array);

	public extern Float32Array(ArrayBuffer buffer, uint? byteOffset = null, uint? length = null);
}

/// <summary>
/// 64 位 IEEE 浮点数（16 位有效数字，例如 1.23456789012345）数组视图，-1.8E308 到 1.8E308 并且 5E-324 是最小的正数
/// </summary>
[ECMAScript]
[Description("@#Float64Array")]
public class Float64Array : TypedArray<double, Float64Array>
{
	public extern Float64Array(uint length);

	public extern Float64Array(Float64Array array);

	public extern Float64Array(IEnumerable<double> array);

	public extern Float64Array(IArrayBufferView array);

	public extern Float64Array(ArrayBuffer buffer, uint? byteOffset = null, uint? length = null);
}

/// <summary>
/// 8 位有符号整型（补码）数组视图，-128 到 127
/// </summary>
[ECMAScript]
[Description("@#Int8Array")]
public class Int8Array : TypedArray<sbyte, Int8Array>
{
	public extern Int8Array(uint length);

	public extern Int8Array(Int8Array array);

	public extern Int8Array(IEnumerable<sbyte> array);

	public extern Int8Array(IArrayBufferView array);

	public extern Int8Array(ArrayBuffer buffer, uint? byteOffset = null, uint? length = null);

}

/// <summary>
/// 16 位有符号整型（补码）数组视图，-32768 到 32767
/// </summary>
[ECMAScript]
[Description("@#Int16Array")]
public class Int16Array : TypedArray<short, Int16Array>
{
	public extern Int16Array(uint length);

	public extern Int16Array(Int16Array array);

	public extern Int16Array(IEnumerable<short> array);

	public extern Int16Array(IArrayBufferView array);

	public extern Int16Array(ArrayBuffer buffer, uint? byteOffset = null, uint? length = null);
}

/// <summary>
/// 32 位有符号整型（补码）数组视图，-2147483648 到 2147483647
/// </summary>
[ECMAScript]
[Description("@#Int32Array")]
public class Int32Array : TypedArray<int, Int32Array>
{
	public extern Int32Array(uint length);

	public extern Int32Array(Int32Array array);

	public extern Int32Array(IEnumerable<int> array);

	public extern Int32Array(IArrayBufferView array);

	public extern Int32Array(ArrayBuffer buffer, uint? byteOffset = null, uint? length = null);
}

/// <summary>
/// 8 位无符号整型数组视图，0 到 255
/// </summary>
[ECMAScript]
[Description("@#Uint8Array")]
public class Uint8Array : TypedArray<byte, Uint8Array>
{
	public extern Uint8Array(uint length);

	public extern Uint8Array(Uint8Array array);

	public extern Uint8Array(IEnumerable<byte> array);

	public extern Uint8Array(IArrayBufferView array);

	public extern Uint8Array(ArrayBuffer buffer, uint? byteOffset = null, uint? length = null);
}

/// <summary>
/// 8 位无符号整型（一定在 0 到 255 之间）数组视图，0 到 255
/// </summary>
[ECMAScript]
[Description("@#Uint8ClampedArray")]
public class Uint8ClampedArray : TypedArray<byte, Uint8ClampedArray>
{
	public extern Uint8ClampedArray(uint length);

	public extern Uint8ClampedArray(Uint8ClampedArray array);

	public extern Uint8ClampedArray(IEnumerable<byte> array);

	public extern Uint8ClampedArray(IArrayBufferView array);

	public extern Uint8ClampedArray(ArrayBuffer buffer, uint? byteOffset = null, uint? length = null);

}

/// <summary>
/// 16 位无符号整型数组视图，0 到 65535
/// </summary>
[ECMAScript]
[Description("@#Uint16Array")]
public class Uint16Array : TypedArray<ushort, Uint16Array>
{
	public extern Uint16Array(uint length);

	public extern Uint16Array(Uint16Array array);

	public extern Uint16Array(IEnumerable<ushort> array);

	public extern Uint16Array(IArrayBufferView array);

	public extern Uint16Array(ArrayBuffer buffer, uint? ushortOffset = null, uint? length = null);

}

/// <summary>
/// 32 位无符号整型数组视图，0 到 4294967295
/// </summary>
[ECMAScript]
[Description("@#Uint32Array")]
public class Uint32Array : TypedArray<uint, Uint32Array>
{
	public extern Uint32Array(uint length);

	public extern Uint32Array(Uint32Array array);

	public extern Uint32Array(IEnumerable<uint> array);

	public extern Uint32Array(IArrayBufferView array);

	public extern Uint32Array(ArrayBuffer buffer, uint? uintOffset = null, uint? length = null);

}
