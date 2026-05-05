using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

namespace ECMAScript;

[ECMAScript]
public record struct ArrayBufferOption(Number? MaxByteLength = null);

/// <summary>
/// 底层二进制数据缓冲区的数组视图
/// </summary>
[ECMAScript]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IBufferSource
{
}

[ECMAScript]
public interface IAllowSharedBufferSource : IBufferSource
{

}

/// <summary>
/// Bridge interface for typed arrays that JavaScript <c>Atomics</c> accepts.
/// This is intentionally hidden because it exists only to make the C# host surface precise without introducing a new JavaScript runtime type.
/// </summary>
[ECMAScript]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IAtomicArray<T> : IArrayBufferView
{
}

/// <summary>
/// Bridge interface for typed arrays that JavaScript <c>Atomics.wait</c> and <c>Atomics.notify</c> accept.
/// JavaScript restricts this further than general atomic operations, so the bridge keeps that distinction explicit.
/// </summary>
[ECMAScript]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IWaitableAtomicArray<T> : IAtomicArray<T>
{
}

/// <summary>
/// 底层二进制数据缓冲区的数组视图
/// </summary>
[ECMAScript]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IArrayBufferView : IAllowSharedBufferSource
{
	[Description("@#buffer")]
	ArrayBuffer Buffer { get; }

	[Description("@#byteLength")]
	Number ByteLength { get; }

	[Description("@#byteOffset")]
	Number ByteOffset { get; }
}

/// <summary>
/// 用来表示通用的原始二进制数据缓冲区。
/// 它是一个字节数组，通常在其他语言中称为“byte array”。你不能直接操作 ArrayBuffer 中的内容；而是要通过类型化数组对象或 DataView 对象来操作，它们会将缓冲区中的数据表示为特定的格式，并通过这些格式来读写缓冲区的内容。
/// ArrayBuffer() 构造函数创建一个以字节为单位的给定长度的新 ArrayBuffer。你也可以从现有的数据（例如，从 Base64 字符串或者从本地文件）获取数组缓冲区。
/// </summary>
[ECMAScript]
[Description("@#ArrayBuffer")]
public class ArrayBuffer : IAllowSharedBufferSource
{
	/// <summary>
	/// JavaScript <c>ArrayBuffer.prototype</c> object.
	/// This remains on the constructor host so the public surface matches the runtime host boundary.
	/// </summary>
	[Description("@#prototype")]
	public extern static ArrayBuffer Prototype { get; }

	public extern ArrayBuffer(Number length);

	public extern ArrayBuffer(Number length, ArrayBufferOption? option = null);

	[Description("@#isView")]
	public extern static bool IsView(object arg);

	[Description("@#byteLength")]
	public extern virtual Number ByteLength { get; }

	/// <summary>
	/// Maximum length this buffer can grow to.
	/// This is only meaningful for resizable JavaScript array buffers.
	/// </summary>
	[Description("@#maxByteLength")]
	public extern virtual Number MaxByteLength { get; }

	/// <summary>
	/// Returns whether this buffer can be resized in place by JavaScript <c>ArrayBuffer.prototype.resize</c>.
	/// </summary>
	[Description("@#resizable")]
	public extern virtual bool Resizable { get; }

	/// <summary>
	/// Returns a section of an TypedArrayBuffer.
	/// </summary>
	/// <param name="begin"></param>
	/// <param name="end"></param>
	/// <returns></returns>
	[Description("@#slice")]
	public extern virtual ArrayBuffer Slice(Number begin, Number? end = null);

	/// <summary>
	/// Resizes a resizable JavaScript array buffer in place.
	/// This maps to the runtime host member and is intentionally absent on fixed-length buffers at runtime.
	/// </summary>
	[Description("@#resize")]
	public extern virtual void Resize(Number newByteLength);

	/// <summary>
	/// Transfers this buffer into a new JavaScript <see cref="ArrayBuffer"/>, optionally changing the byte length.
	/// </summary>
	[Description("@#transfer")]
	public extern virtual ArrayBuffer Transfer(Number? newByteLength = null);

	/// <summary>
	/// Transfers this buffer into a fixed-length JavaScript <see cref="ArrayBuffer"/>.
	/// </summary>
	[Description("@#transferToFixedLength")]
	public extern virtual ArrayBuffer TransferToFixedLength(Number? newByteLength = null);
}

[ECMAScript]
[Description("@#SharedArrayBuffer")]
public class SharedArrayBuffer : ArrayBuffer, IAllowSharedBufferSource
{
	/// <summary>
	/// JavaScript <c>SharedArrayBuffer.prototype</c> object.
	/// This intentionally hides <see cref="ArrayBuffer.Prototype"/> because JavaScript exposes a distinct prototype object on the <c>SharedArrayBuffer</c> constructor.
	/// </summary>
	[Description("@#prototype")]
	public new extern static SharedArrayBuffer Prototype { get; }

	public extern SharedArrayBuffer(Number length);

	public extern SharedArrayBuffer(Number length, ArrayBufferOption? option = null);

	/// <summary>
	/// Maximum length this shared buffer can grow to.
	/// </summary>
	[Description("@#maxByteLength")]
	public extern override Number MaxByteLength { get; }

	/// <summary>
	/// Returns whether this shared buffer is growable.
	/// </summary>
	[Description("@#growable")]
	public extern bool Growable { get; }

	/// <summary>
	/// Grows a JavaScript <c>SharedArrayBuffer</c> in place.
	/// </summary>
	[Description("@#grow")]
	public extern void Grow(Number newByteLength);

	/// <summary>
	/// Shared array buffer slicing stays on the <c>SharedArrayBuffer</c> host in JavaScript.
	/// The covariant return keeps the C# projection aligned with that runtime behavior.
	/// </summary>
	[Description("@#slice")]
	public extern override SharedArrayBuffer Slice(Number begin, Number? end = null);
}

/// <summary>
/// 是一个可以从二进制 ArrayBuffer 对象中读写多种数值类型的底层接口，使用它时，不用考虑不同平台的字节序问题。
/// </summary>
[ECMAScript]
[Description("@#DataView")]
public class DataView : IArrayBufferView, IBufferSource
{
	/// <summary>
	/// JavaScript <c>DataView.prototype</c> object.
	/// Exposing this on the constructor host avoids inventing a separate CLR-side helper type.
	/// </summary>
	[Description("@#prototype")]
	public extern static DataView Prototype { get; }

	public extern DataView(ArrayBuffer buffer, Number? byteOffset = null, Number? byteLength = null);

	[Description("@#buffer")]
	public virtual ArrayBuffer Buffer { get; }

	[Description("@#byteLength")]
	public virtual Number ByteLength { get; }

	[Description("@#byteOffset")]
	public virtual Number ByteOffset { get; }

	[Description("@#getFloat32")]
	public virtual extern float GetFloat32(Number byteOffset);

	[Description("@#getFloat32")]
	public virtual extern float GetFloat32(Number byteOffset, bool littleEndian);

	/// <summary>
	/// Reads a JavaScript float16 value.
	/// The C# surface uses <see cref="float"/> to stay consistent with the existing <see cref="Float16Array"/> projection.
	/// </summary>
	[Description("@#getFloat16")]
	public virtual extern float GetFloat16(Number byteOffset);

	/// <summary>
	/// Reads a JavaScript float16 value with explicit endian control.
	/// </summary>
	[Description("@#getFloat16")]
	public virtual extern float GetFloat16(Number byteOffset, bool littleEndian);

	[Description("@#getFloat64")]
	public virtual extern double GetFloat64(Number byteOffset);

	[Description("@#getFloat64")]
	public virtual extern double GetFloat64(Number byteOffset, bool littleEndian);

	[Description("@#getInt8")]
	public virtual extern sbyte GetInt8(Number byteOffset);

	[Description("@#getInt16")]
	public virtual extern short GetInt16(Number byteOffset);

	[Description("@#getInt16")]
	public virtual extern short GetInt16(Number byteOffset, bool littleEndian);

	[Description("@#getInt32")]
	public virtual extern int GetInt32(Number byteOffset);

	[Description("@#getInt32")]
	public virtual extern int GetInt32(Number byteOffset, bool littleEndian);

	[Description("@#getBigInt64")]
	public virtual extern BigInt GetBigInt64(Number byteOffset, bool littleEndian);

	[Description("@#getUint8")]
	public virtual extern byte GetUint8(Number byteOffset);

	[Description("@#getUint16")]
	public virtual extern ushort GetUint16(Number byteOffset);

	[Description("@#getUint16")]
	public virtual extern ushort GetUint16(Number byteOffset, bool littleEndian);

	[Description("@#getUint32")]
	public virtual extern uint GetUint32(Number byteOffset);

	[Description("@#getUint32")]
	public virtual extern uint GetUint32(Number byteOffset, bool littleEndian);

	[Description("@#getBigUint64")]
	public virtual extern BigInt GetBigUint64(Number byteOffset, bool littleEndian);

	[Description("@#setFloat32")]
	public virtual extern void SetFloat32(Number byteOffset, float value);

	[Description("@#setFloat32")]
	public virtual extern void SetFloat32(Number byteOffset, float value, bool littleEndian);

	/// <summary>
	/// Writes a JavaScript float16 value.
	/// The C# surface uses <see cref="float"/> to stay consistent with the existing <see cref="Float16Array"/> projection.
	/// </summary>
	[Description("@#setFloat16")]
	public virtual extern void SetFloat16(Number byteOffset, float value);

	/// <summary>
	/// Writes a JavaScript float16 value with explicit endian control.
	/// </summary>
	[Description("@#setFloat16")]
	public virtual extern void SetFloat16(Number byteOffset, float value, bool littleEndian);

	[Description("@#setFloat64")]
	public virtual extern void SetFloat64(Number byteOffset, double value);

	[Description("@#setFloat64")]
	public virtual extern void SetFloat64(Number byteOffset, double value, bool littleEndian);

	[Description("@#setInt8")]
	public virtual extern void SetInt8(Number byteOffset, sbyte value);

	[Description("@#setInt16")]
	public virtual extern void SetInt16(Number byteOffset, short value);

	[Description("@#setInt16")]
	public virtual extern void SetInt16(Number byteOffset, short value, bool littleEndian);

	[Description("@#setInt32")]
	public virtual extern void SetInt32(Number byteOffset, int value);

	[Description("@#setInt32")]
	public virtual extern void SetInt32(Number byteOffset, int value, bool littleEndian);

	[Description("@#setBigInt64")]
	public virtual extern void SetBigInt64(Number byteOffset, BigInt value, bool littleEndian);

	[Description("@#setUint8")]
	public virtual extern void SetUint8(Number byteOffset, byte value);

	[Description("@#setUint16")]
	public virtual extern void SetUint16(Number byteOffset, ushort value);

	[Description("@#setUint16")]
	public virtual extern void SetUint16(Number byteOffset, ushort value, bool littleEndian);

	[Description("@#setUint32")]
	public virtual extern void SetUint32(Number byteOffset, uint value);

	[Description("@#setUint32")]
	public virtual extern void SetUint32(Number byteOffset, uint value, bool littleEndian);

	[Description("@#setBigUint64")]
	public virtual extern void SetBigUint64(Number byteOffset, BigInt value, bool littleEndian);
}

/// <summary>
/// JavaScript typed array host.
/// <see cref="IEnumerable{T}"/> is used here as the common C# input/output surface for values
/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
/// It does not mean typed arrays follow .NET collection semantics.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TArray"></typeparam>
[ECMAScript]
public abstract class TypedArray<T, TArray> : IArrayBufferView, IBufferSource, IEnumerable<T>
	where TArray : TypedArray<T, TArray>
{
	public extern TypedArray(Number length);

	/// <summary>
	/// Creates a typed array from a JavaScript iterable.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// </summary>
	public extern TypedArray(IEnumerable<T> array);

	public extern TypedArray(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

	/// <summary>
	/// The size in bytes of each element in the array.
	/// </summary>
	[Description("@#BYTES_PER_ELEMENT")]
	public extern static Number BYTES_PER_ELEMENT { get; }

	/// <summary>
	/// Returns a new array from a set of elements.
	/// </summary>
	/// <param name="value">A set of elements to include in the new array object.</param>
	/// <returns></returns>
	[Description("@#of")]
	public extern static TArray Of(params T[] items);

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
	public extern static TArray From<U>(IEnumerable<U> arrayLike, Func<U, Number, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from an array-like or iterable object.
	/// This overload mirrors JavaScript <c>TypedArray.from</c> when the caller does not need the element index in the mapping callback.
	/// </summary>
	[Description("@#from")]
	public extern static TArray From<U>(IEnumerable<U> arrayLike, Func<U, T> mapFn, object? thisArg = null);

	/// <summary>
	/// The ArrayBuffer instance referenced by the array.
	/// </summary>
	[Description("@#buffer")]
	public extern ArrayBuffer Buffer { get; }

	/// <summary>
	/// The length in bytes of the array.
	/// </summary>
	[Description("@#byteLength")]
	public extern Number ByteLength { get; }

	/// <summary>
	/// The offset in bytes of the array.
	/// </summary>
	[Description("@#byteOffset")]
	public extern Number ByteOffset { get; }

	/// <summary>
	/// Returns the this object after copying a section of the array identified by start and end
	/// to the same array starting at position target
	/// </summary>
	/// <param name="target">If target is negative, it is treated as length+target where length is the length of the array.</param>
	/// <param name="start">If start is negative, it is treated as length+start.If end is negative, it is treated as length+end.</param>
	/// <param name="end">If not specified, length of the this object is used as its default value.</param>
	/// <returns></returns>
	[Description("@#copyWithin")]
	public extern TArray CopyWithin(Number target, Number start, Number? end = null);

	/// <summary>
	/// Determines whether all the elements of the typed array satisfy the specified test.
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: this) => unknown</b></para>A function that accepts up to three arguments. The every method calls the predicate function for each element in the typed array until the predicate returns a value which is coercible to the Boolean value false, or until the end of the array.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to predicate. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#every")]
	public extern bool Every(Func<T, Number, TArray, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Changes all array elements from `start` to `end` index to a static `value` and returns the modified array
	/// </summary>
	/// <param name="value">value to fill array section with</param>
	/// <param name="start">index to start filling the array at.If start is negative, it is treated as length+start where length is the length of the array.</param>
	/// <param name="end">index to stop filling the array at. If end is negative, it is treated as length+end.</param>
	/// <returns></returns>
	[Description("@#fill")]
	public extern TArray Fill(T value, Number? start = null, Number? end = null);

	/// <summary>
	/// Returns the elements of an array that meet the condition specified in a callback function.
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: this) => unknown</b></para>A function that accepts up to three arguments. The filter method calls the predicate function one time for each element in the array.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to predicate. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#filter")]
	public extern TArray Filter(Func<T, Number, TArray, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Returns the value of the first element in the typed array where the predicate result is truthy.
	/// If no matching element exists, JavaScript returns <c>undefined</c> and this C# projection surfaces that absence as <see langword="null" />.
	/// The callback result stays as <see cref="object"/> because JavaScript uses truthy/falsy coercion here rather than requiring a strict boolean.
	/// </summary>
	/// <param name="predicate">find calls predicate once for each element of the typed array, in ascending order, until it finds one where predicate returns a truthy value. If such an element is found, find immediately returns that element value. Otherwise, JavaScript returns <c>undefined</c>.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to predicate. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#find")]
	public extern T? Find(Func<T, Number, TArray, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Returns the last element whose value satisfies the provided testing function.
	/// Nullable is used because JavaScript returns <c>undefined</c> when no matching element exists,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// </summary>
	[Description("@#findLast")]
	public extern T? FindLast(Func<T, Number, TArray, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Returns the index of the first element in the typed array whose predicate result is truthy, and <c>-1</c> otherwise.
	/// The callback result stays as <see cref="object"/> because JavaScript uses truthy/falsy coercion here rather than requiring a strict boolean.
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: this) => unknown</b></para>findIndex calls predicate once for each element of the typed array, in ascending order, until it finds one where predicate returns a truthy value. If such an element is found, findIndex immediately returns that element index. Otherwise, findIndex returns -1.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to predicate. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#findIndex")]
	public extern Number FindIndex(Func<T, Number, TArray, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Returns the index of the last element whose value satisfies the provided testing function, or <c>-1</c> if no match is found.
	/// </summary>
	[Description("@#findLastIndex")]
	public extern Number FindLastIndex(Func<T, Number, TArray, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Performs the specified action for each element in an array.
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: T, index: number, array: this) => void</b></para>A function that accepts up to three arguments. forEach calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<T, Number, TArray> callbackfn, object? thisArg = null);

	/// <summary>
	/// Returns the index of the first occurrence of a value in an array.
	/// </summary>
	/// <param name="searchElement">The value to locate in the array.</param>
	/// <param name="fromIndex">The array index at which to begin the search. If negative, it is treated as length + fromIndex.</param>
	/// <returns></returns>
	[Description("@#indexOf")]
	public extern Number IndexOf(T searchElement, Number? fromIndex = null);

	/// <summary>
	/// Projection of JavaScript <c>TypedArray.prototype.includes</c>.
	/// This remains on the typed-array host instead of widening to a CLR collection helper.
	/// </summary>
	[Description("@#includes")]
	public extern bool Includes(T searchElement, Number? fromIndex = null);

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
	/// <param name="fromIndex">The array index at which to begin the search. If negative, it is treated as length + fromIndex.</param>
	/// <returns></returns>
	[Description("@#lastIndexOf")]
	public extern Number LastIndexOf(T searchElement, Number? fromIndex = null);

	/// <summary>
	/// The length of the array.
	/// </summary>
	[Description("@#length")]
	public extern Number Length { get; }

	/// <summary>
	/// C# host projection of JavaScript <c>TypedArray.prototype.at</c>.
	/// Nullable is used because JavaScript returns <c>undefined</c> for an out-of-range index,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// </summary>
	[Description("@#at")]
	public extern T? At(Number index);

	/// <summary>
	/// Calls a defined callback function on each element of an array, and returns an array that contains the results.
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: T, index: number, array: this) => T</b></para>A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#map")]
	public extern TArray Map(Func<T, Number, TArray, T> callbackfn, object? thisArg = null);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <param name="callbackfn"><para><b>(previousValue: T, currentValue: T, currentIndex: number, array: this) => T</b></para>A function that accepts up to four arguments. When no initial value is supplied, JavaScript uses the first typed-array element as the initial accumulator.</param>
	/// <returns></returns>
	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, Number, TArray, T> callbackfn);

	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, T> callbackfn);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <param name="callbackfn">(previousValue: number, currentValue: number, currentIndex: number, array: this) => number,A function that accepts up to four arguments.The reduce method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, Number, TArray, T> callbackfn, T initialValue);

	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, T> callbackfn, T initialValue);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="callbackfn"><para><b>(previousValue: U, currentValue: number, currentIndex: number, array: this) => U</b></para>A function that accepts up to four arguments.The reduce method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduce")]
	public extern U Reduce<U>(Func<U, T, Number, TArray, U> callbackfn, U initialValue);

	[Description("@#reduce")]
	public extern U Reduce<U>(Func<U, T, U> callbackfn, U initialValue);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <param name="callbackfn"><para><b>(previousValue: T, currentValue: T, currentIndex: number, array: this) => T</b></para>A function that accepts up to four arguments. When no initial value is supplied, JavaScript uses the last typed-array element as the initial accumulator.</param>
	/// <returns></returns>
	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, Number, TArray, T> callbackfn);

	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, T> callbackfn);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <param name="callbackfn"><para><b>(previousValue: number, currentValue: number, currentIndex: number, array: this) => number </b></para>A function that accepts up to four arguments.The reduceRight method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, Number, TArray, T> callbackfn, T initialValue);

	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, T> callbackfn, T initialValue);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <typeparam name="U"></typeparam>
	/// <param name="callbackfn">(previousValue: U, currentValue: number, currentIndex: number, array: this) => U,A function that accepts up to four arguments.The reduceRight method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	[Description("@#reduceRight")]
	public extern U ReduceRight<U>(Func<U, T, Number, TArray, U> callbackfn, U initialValue);

	[Description("@#reduceRight")]
	public extern U ReduceRight<U>(Func<U, T, U> callbackfn, U initialValue);

	/// <summary>
	/// Reverses the elements in an Array.
	/// </summary>
	/// <returns></returns>
	[Description("@#reverse")]
	public extern TArray Reverse();

	/// <summary>
	/// Returns a copied typed array with the elements in reverse order.
	/// Unlike <see cref="Reverse"/>, JavaScript <c>toReversed()</c> does not mutate the source typed array.
	/// </summary>
	[Description("@#toReversed")]
	public extern TArray ToReversed();

	/// <summary>
	/// Copies values from a JavaScript iterable into the typed array.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// </summary>
	/// <param name="array">A typed or untyped iterable of values to copy.</param>
	/// <param name="offset">The index in the current array at which the values are to be written.</param>
	[Description("@#set")]
	public extern void Set(IEnumerable<T> array, Number? offset = null);

	/// <summary>
	/// Returns a section of an array.
	/// </summary>
	/// <param name="start">The beginning of the specified portion of the array.</param>
	/// <param name="end">The end of the specified portion of the array. This is exclusive of the element at the index 'end'.</param>
	/// <returns></returns>
	[Description("@#slice")]
	public extern TArray Slice(Number? start = null, Number? end = null);

	/// <summary>
	/// Determines whether the specified callback function returns a truthy value for any element of the typed array.
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: this) => unknown</b></para>A function that accepts up to three arguments. The some method calls the predicate function for each element in the typed array until the predicate returns a value which is coercible to the Boolean value true, or until the end of the array.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to predicate. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#some")]
	public extern bool Some(Func<T, Number, TArray, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Sorts an array.
	/// </summary>
	/// <param name="compareFn"><para><b>(a: T, b: T) => number</b></para>Function used to determine the order of the elements. It is expected to return a negative value if first argument is less than second argument, zero if they're equal, and a positive value otherwise. If omitted, the elements are sorted in ascending order.</param>
	/// <returns></returns>
	[Description("@#sort")]
	public extern TArray Sort(Func<T, T, Number>? compareFn = null);

	/// <summary>
	/// Returns a copied typed array with its elements sorted.
	/// This stays on <typeparamref name="TArray"/> because JavaScript preserves the concrete typed-array host.
	/// </summary>
	[Description("@#toSorted")]
	public extern TArray ToSorted(Func<T, T, Number>? compareFn = null);

	/// <summary>
	/// Returns a copied typed array with the element at the specified index replaced.
	/// Negative indices follow JavaScript <c>TypedArray.prototype.with</c> semantics and count from the end.
	/// </summary>
	[Description("@#with")]
	public extern TArray With(Number index, T value);

	/// <summary>
	/// Gets a new TArray view of the ArrayBuffer store for this array, referencing the elements at begin, inclusive, up to end, exclusive.
	/// </summary>
	/// <param name="begin">The index of the beginning of the array.</param>
	/// <param name="end">The index of the end of the array.</param>
	/// <returns></returns>
	[Description("@#subarray")]
	public extern TArray Subarray(Number? begin = null, Number? end = null);

	/// <summary>
	/// Returns the JavaScript string form of the typed array.
	/// This is the direct projection of <c>TypedArray.prototype.toString()</c>.
	/// </summary>
	[Description("@#toString")]
	public extern override string ToString();

	/// <summary>
	/// Returns a locale-sensitive string representation of the typed array.
	/// This is the direct projection of <c>TypedArray.prototype.toLocaleString()</c>.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString();

	/// <summary>
	/// Returns a locale-sensitive string representation of the typed array.
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
	/// Returns a locale-sensitive string representation of the typed array.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
	/// JavaScript forwards <paramref name="locales" /> and <paramref name="options" /> to each element's own <c>toLocaleString</c> method.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(IEnumerable<string>? locales, object? options = null);

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>TypedArray.prototype.keys()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<Number> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>TypedArray.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<T> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>TypedArray.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[index, value]</c>.
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<object?>> Entries();

	extern IEnumerator<T> IEnumerable<T>.GetEnumerator();

	extern IEnumerator IEnumerable.GetEnumerator();

	/// <summary>
	/// Direct JavaScript index access surface.
	/// This stays non-nullable to preserve compatibility with array-like CLR projections.
	/// Use <see cref="At" /> when you need a nullable result for out-of-range access.
	/// </summary>
	public extern T this[Number index] { get; set; }

	//[EditorBrowsable(EditorBrowsableState.Never)]
	//public extern override Object this[string name] { get; set; }
}

/// <summary>
/// 64 位有符号整型（补码）数组视图，-2^63 到 2^63 - 1
/// </summary>
[ECMAScript]
[Description("@#BigInt64Array")]
public class BigInt64Array : TypedArray<BigInt, BigInt64Array>, IWaitableAtomicArray<BigInt>
{
	/// <summary>
	/// JavaScript <c>BigInt64Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static BigInt64Array Prototype { get; }

	public extern BigInt64Array(Number length);

	public extern BigInt64Array(BigInt64Array array);

	public extern BigInt64Array(IEnumerable<BigInt> array);

	public extern BigInt64Array(IArrayBufferView array);

	public extern BigInt64Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// 64 位无符号整型数组视图，0 到 2^64 - 1
/// </summary>
[ECMAScript]
[Description("@#BigUint64Array")]
public class BigUint64Array : TypedArray<BigInt, BigUint64Array>, IAtomicArray<BigInt>
{
	/// <summary>
	/// JavaScript <c>BigUint64Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static BigUint64Array Prototype { get; }

	public extern BigUint64Array(Number length);

	public extern BigUint64Array(BigUint64Array array);

	public extern BigUint64Array(IEnumerable<BigInt> array);

	public extern BigUint64Array(IArrayBufferView array);

	public extern BigUint64Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

[ECMAScript]
[Description("@#Float16Array")]
public class Float16Array : TypedArray<float, Float16Array>
{
	/// <summary>
	/// JavaScript <c>Float16Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Float16Array Prototype { get; }

	public extern Float16Array(Number length);

	public extern Float16Array(Float16Array array);

	public extern Float16Array(IEnumerable<float> array);

	public extern Float16Array(IArrayBufferView array);

	public extern Float16Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// 32 位 IEEE 浮点数（7 位有效数字，例如 1.234567）数组视图，-3.4E38 到 3.4E38 并且 1.2E-38 是最小的正数
/// </summary>
[ECMAScript]
[Description("@#Float32Array")]
public class Float32Array : TypedArray<float, Float32Array>
{
	/// <summary>
	/// JavaScript <c>Float32Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Float32Array Prototype { get; }

	public extern Float32Array(Number length);

	public extern Float32Array(Float32Array array);

	public extern Float32Array(IEnumerable<float> array);

	public extern Float32Array(IArrayBufferView array);

	public extern Float32Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// 64 位 IEEE 浮点数（16 位有效数字，例如 1.23456789012345）数组视图，-1.8E308 到 1.8E308 并且 5E-324 是最小的正数
/// </summary>
[ECMAScript]
[Description("@#Float64Array")]
public class Float64Array : TypedArray<double, Float64Array>
{
	/// <summary>
	/// JavaScript <c>Float64Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Float64Array Prototype { get; }

	public extern Float64Array(Number length);

	public extern Float64Array(Float64Array array);

	public extern Float64Array(IEnumerable<double> array);

	public extern Float64Array(IArrayBufferView array);

	public extern Float64Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// 8 位有符号整型（补码）数组视图，-128 到 127
/// </summary>
[ECMAScript]
[Description("@#Int8Array")]
public class Int8Array : TypedArray<sbyte, Int8Array>, IAtomicArray<sbyte>
{
	/// <summary>
	/// JavaScript <c>Int8Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Int8Array Prototype { get; }

	public extern Int8Array(Number length);

	public extern Int8Array(Int8Array array);

	public extern Int8Array(IEnumerable<sbyte> array);

	public extern Int8Array(IArrayBufferView array);

	public extern Int8Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// 16 位有符号整型（补码）数组视图，-32768 到 32767
/// </summary>
[ECMAScript]
[Description("@#Int16Array")]
public class Int16Array : TypedArray<short, Int16Array>, IAtomicArray<short>
{
	/// <summary>
	/// JavaScript <c>Int16Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Int16Array Prototype { get; }

	public extern Int16Array(Number length);

	public extern Int16Array(Int16Array array);

	public extern Int16Array(IEnumerable<short> array);

	public extern Int16Array(IArrayBufferView array);

	public extern Int16Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// 32 位有符号整型（补码）数组视图，-2147483648 到 2147483647
/// </summary>
[ECMAScript]
[Description("@#Int32Array")]
public class Int32Array : TypedArray<int, Int32Array>, IWaitableAtomicArray<int>
{
	/// <summary>
	/// JavaScript <c>Int32Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Int32Array Prototype { get; }

	public extern Int32Array(Number length);

	public extern Int32Array(Int32Array array);

	public extern Int32Array(IEnumerable<int> array);

	public extern Int32Array(IArrayBufferView array);

	public extern Int32Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// 8 位无符号整型数组视图，0 到 255
/// </summary>
[ECMAScript]
[Description("@#Uint8Array")]
public class Uint8Array : TypedArray<byte, Uint8Array>, IAtomicArray<byte>
{
	/// <summary>
	/// JavaScript <c>Uint8Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Uint8Array Prototype { get; }

	public extern Uint8Array(Number length);

	public extern Uint8Array(Uint8Array array);

	public extern Uint8Array(IEnumerable<byte> array);

	public extern Uint8Array(IArrayBufferView array);

	public extern Uint8Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// 8 位无符号整型（一定在 0 到 255 之间）数组视图，0 到 255
/// </summary>
[ECMAScript]
[Description("@#Uint8ClampedArray")]
public class Uint8ClampedArray : TypedArray<byte, Uint8ClampedArray>
{
	/// <summary>
	/// JavaScript <c>Uint8ClampedArray.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Uint8ClampedArray Prototype { get; }

	public extern Uint8ClampedArray(Number length);

	public extern Uint8ClampedArray(Uint8ClampedArray array);

	public extern Uint8ClampedArray(IEnumerable<byte> array);

	public extern Uint8ClampedArray(IArrayBufferView array);

	public extern Uint8ClampedArray(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// 16 位无符号整型数组视图，0 到 65535
/// </summary>
[ECMAScript]
[Description("@#Uint16Array")]
public class Uint16Array : TypedArray<ushort, Uint16Array>, IAtomicArray<ushort>
{
	/// <summary>
	/// JavaScript <c>Uint16Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Uint16Array Prototype { get; }

	public extern Uint16Array(Number length);

	public extern Uint16Array(Uint16Array array);

	public extern Uint16Array(IEnumerable<ushort> array);

	public extern Uint16Array(IArrayBufferView array);

	public extern Uint16Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// 32 位无符号整型数组视图，0 到 4294967295
/// </summary>
[ECMAScript]
[Description("@#Uint32Array")]
public class Uint32Array : TypedArray<uint, Uint32Array>, IAtomicArray<uint>
{
	/// <summary>
	/// JavaScript <c>Uint32Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Uint32Array Prototype { get; }

	public extern Uint32Array(Number length);

	public extern Uint32Array(Uint32Array array);

	public extern Uint32Array(IEnumerable<uint> array);

	public extern Uint32Array(IArrayBufferView array);

	public extern Uint32Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}
