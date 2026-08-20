using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

namespace ECMAScript;

[ECMAScript]
/// <summary>
/// Options for constructing a resizable JavaScript <c>ArrayBuffer</c>.
/// 构造可调整大小的 JavaScript <c>ArrayBuffer</c> 时使用的选项。
/// </summary>
/// <param name="MaxByteLength">Maximum capacity allowed for a resizable buffer. 可调整缓冲区允许的最大字节容量。</param>
public record struct ArrayBufferOption(Number? MaxByteLength = null);

/// <summary>
/// Marker for JavaScript values accepted where a binary buffer source is required.
/// 标记可作为底层二进制缓冲区源传递的 JavaScript 值。
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
/// JavaScript <c>Atomics</c> 接受的 typed array 桥接接口。它被刻意隐藏，只用于精确约束 C# 宿主表面，
/// 不引入新的 JavaScript 运行时类型。
/// </summary>
[ECMAScript]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IAtomicArray<T> : IArrayBufferView
{
}

/// <summary>
/// Bridge interface for typed arrays that JavaScript <c>Atomics.wait</c> and <c>Atomics.notify</c> accept.
/// JavaScript restricts this further than general atomic operations, so the bridge keeps that distinction explicit.
/// JavaScript <c>Atomics.wait</c> 与 <c>Atomics.notify</c> 接受的 typed array 桥接接口；其约束比普通原子操作更窄，故在 C# 表面明确区分。
/// </summary>
[ECMAScript]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IWaitableAtomicArray<T> : IAtomicArray<T>
{
}

/// <summary>
/// Common JavaScript view over an <c>ArrayBuffer</c> or <c>SharedArrayBuffer</c>.
/// <c>ArrayBuffer</c> 或 <c>SharedArrayBuffer</c> 上 JavaScript 视图的通用契约。
/// </summary>
[ECMAScript]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IArrayBufferView : IAllowSharedBufferSource
{
	/// <summary>Gets the backing JavaScript buffer. 获取底层 JavaScript 缓冲区。</summary>
	[Description("@#buffer")]
	ArrayBuffer Buffer { get; }

	/// <summary>Gets the view length in bytes. 获取视图的字节长度。</summary>
	[Description("@#byteLength")]
	Number ByteLength { get; }

	/// <summary>Gets the byte offset into the backing buffer. 获取相对于底层缓冲区的字节偏移量。</summary>
	[Description("@#byteOffset")]
	Number ByteOffset { get; }
}

/// <summary>
/// Raw binary data buffer in the JavaScript runtime.
/// Contents are accessed through typed arrays or <see cref="DataView"/>, which apply the chosen numeric layout; the buffer itself does not expose indexed elements.
/// JavaScript <c>ArrayBuffer</c> represents raw binary bytes。不能直接通过索引读写其内容，
/// 应通过 typed array 或 <see cref="DataView"/> 按指定数值布局读写。
/// </summary>
[ECMAScript]
[Description("@#ArrayBuffer")]
public class ArrayBuffer : IAllowSharedBufferSource
{
	/// <summary>
	/// JavaScript <c>ArrayBuffer.prototype</c> object.
	/// This remains on the constructor host so the public surface matches the runtime host boundary.
	/// JavaScript <c>ArrayBuffer.prototype</c> 对象，保留在构造器宿主上以对齐运行时边界。
	/// </summary>
	[Description("@#prototype")]
	public extern static ArrayBuffer Prototype { get; }

	/// <summary>Creates a fixed-length JavaScript buffer with the requested byte length. 创建指定字节长度的固定大小 JavaScript 缓冲区。</summary>
	public extern ArrayBuffer(Number length);

	/// <summary>Creates a JavaScript buffer that may be resizable up to the configured maximum. 创建可在配置最大值内调整大小的 JavaScript 缓冲区。</summary>
	public extern ArrayBuffer(Number length, ArrayBufferOption? option = null);

	/// <summary>Checks whether a runtime value is an <c>ArrayBuffer</c> view. 检查运行时值是否为 <c>ArrayBuffer</c> 视图。</summary>
	[Description("@#isView")]
	public extern static bool IsView(object arg);

	/// <summary>Gets the current buffer length in bytes. 获取当前缓冲区字节长度。</summary>
	[Description("@#byteLength")]
	public extern virtual Number ByteLength { get; }

	/// <summary>
	/// Maximum length this buffer can grow to.
	/// This is only meaningful for resizable JavaScript array buffers.
	/// 获取缓冲区可增长到的最大字节长度；仅对可调整大小的 JavaScript ArrayBuffer 有意义。
	/// </summary>
	[Description("@#maxByteLength")]
	public extern virtual Number MaxByteLength { get; }

	/// <summary>
	/// Returns whether this buffer can be resized in place by JavaScript <c>ArrayBuffer.prototype.resize</c>.
	/// 指示缓冲区是否可通过 JavaScript <c>ArrayBuffer.prototype.resize</c> 原地调整大小。
	/// </summary>
	[Description("@#resizable")]
	public extern virtual bool Resizable { get; }

	/// <summary>
	/// Returns a copied section of this JavaScript buffer.
	/// 返回此 JavaScript 缓冲区的一段副本；不返回原缓冲区的视图。
	/// </summary>
	/// <param name="begin"></param>
	/// <param name="end"></param>
	/// <returns></returns>
	[Description("@#slice")]
	public extern virtual ArrayBuffer Slice(Number begin, Number? end = null);

	/// <summary>
	/// Resizes a resizable JavaScript array buffer in place.
	/// This maps to the runtime host member and is intentionally absent on fixed-length buffers at runtime.
	/// 原地调整可调整大小 JavaScript 缓冲区的长度；固定大小缓冲区在运行时不支持该成员。
	/// </summary>
	[Description("@#resize")]
	public extern virtual void Resize(Number newByteLength);

	/// <summary>
	/// Transfers this buffer into a new JavaScript <see cref="ArrayBuffer"/>, optionally changing the byte length.
	/// 将此缓冲区转移到新的 JavaScript <see cref="ArrayBuffer"/>，可选地改变字节长度；原缓冲区会按 JavaScript transfer 语义 detached。
	/// </summary>
	[Description("@#transfer")]
	public extern virtual ArrayBuffer Transfer(Number? newByteLength = null);

	/// <summary>
	/// Transfers this buffer into a fixed-length JavaScript <see cref="ArrayBuffer"/>.
	/// 转移到固定大小的 JavaScript <see cref="ArrayBuffer"/>；原缓冲区会按 JavaScript transfer 语义 detached。
	/// </summary>
	[Description("@#transferToFixedLength")]
	public extern virtual ArrayBuffer TransferToFixedLength(Number? newByteLength = null);
}

[ECMAScript]
[Description("@#SharedArrayBuffer")]
/// <summary>
/// JavaScript shared-memory binary buffer.
/// JavaScript 共享内存二进制缓冲区，可被多个 agent 访问，通常与 <see cref="Atomics"/> 配合使用。
/// </summary>
public class SharedArrayBuffer : ArrayBuffer, IAllowSharedBufferSource
{
	/// <summary>
	/// JavaScript <c>SharedArrayBuffer.prototype</c> object.
	/// This intentionally hides <see cref="ArrayBuffer.Prototype"/> because JavaScript exposes a distinct prototype object on the <c>SharedArrayBuffer</c> constructor.
	/// JavaScript <c>SharedArrayBuffer.prototype</c> 对象；它刻意隐藏 <see cref="ArrayBuffer.Prototype"/>，因为构造器有独立原型对象。
	/// </summary>
	[Description("@#prototype")]
	public new extern static SharedArrayBuffer Prototype { get; }

	/// <summary>Creates a fixed-length shared JavaScript buffer. 创建固定长度的共享 JavaScript 缓冲区。</summary>
	public extern SharedArrayBuffer(Number length);

	/// <summary>Creates a shared buffer that can grow to the configured maximum. 创建可增长到配置最大值的共享缓冲区。</summary>
	public extern SharedArrayBuffer(Number length, ArrayBufferOption? option = null);

	/// <summary>
	/// Maximum length this shared buffer can grow to.
	/// 获取此共享缓冲区可增长到的最大字节长度。
	/// </summary>
	[Description("@#maxByteLength")]
	public extern override Number MaxByteLength { get; }

	/// <summary>
	/// Returns whether this shared buffer is growable.
	/// 指示该共享缓冲区是否可增长。
	/// </summary>
	[Description("@#growable")]
	public extern bool Growable { get; }

	/// <summary>
	/// Grows a JavaScript <c>SharedArrayBuffer</c> in place.
	/// 原地增长 JavaScript <c>SharedArrayBuffer</c>；不能缩小，失败条件遵循 JavaScript 运行时。
	/// </summary>
	[Description("@#grow")]
	public extern void Grow(Number newByteLength);

	/// <summary>
	/// Shared array buffer slicing stays on the <c>SharedArrayBuffer</c> host in JavaScript.
	/// The covariant return keeps the C# projection aligned with that runtime behavior.
	/// 共享缓冲区切片保留在 <c>SharedArrayBuffer</c> 宿主上，协变返回值使 C# 投影与 JavaScript 行为对齐。
	/// </summary>
	[Description("@#slice")]
	public extern override SharedArrayBuffer Slice(Number begin, Number? end = null);
}

/// <summary>
/// Low-level JavaScript view for reading and writing multiple numeric layouts in a binary buffer.
/// <see cref="DataView"/> exposes explicit endian control, so layout is independent of the host platform byte order.
/// 用于从二进制 ArrayBuffer 读写多种数值布局的底层 JavaScript 视图；可显式指定端序，
/// 因而不依赖宿主平台的字节序。
/// </summary>
[ECMAScript]
[Description("@#DataView")]
public class DataView : IArrayBufferView, IBufferSource
{
	/// <summary>
	/// JavaScript <c>DataView.prototype</c> object.
	/// Exposing this on the constructor host avoids inventing a separate CLR-side helper type.
	/// JavaScript <c>DataView.prototype</c> 对象；直接暴露在构造器宿主上，避免虚构 CLR 辅助类型。
	/// </summary>
	[Description("@#prototype")]
	public extern static DataView Prototype { get; }

	/// <summary>Creates a view over a buffer region; omitted offset and length follow JavaScript defaults. 在缓冲区区域上创建视图；省略偏移和长度时遵循 JavaScript 默认值。</summary>
	public extern DataView(ArrayBuffer buffer, Number? byteOffset = null, Number? byteLength = null);

	/// <summary>Gets the backing JavaScript buffer. 获取底层 JavaScript 缓冲区。</summary>
	[Description("@#buffer")]
	public virtual ArrayBuffer Buffer { get; }

	/// <summary>Gets the view length in bytes. 获取此视图的字节长度。</summary>
	[Description("@#byteLength")]
	public virtual Number ByteLength { get; }

	/// <summary>Gets the view start offset in bytes. 获取视图起始字节偏移量。</summary>
	[Description("@#byteOffset")]
	public virtual Number ByteOffset { get; }

	/// <summary>Reads a 32-bit floating-point value using JavaScript's default big-endian order. 以 JavaScript 默认大端序读取 32 位浮点值。</summary>
	[Description("@#getFloat32")]
	public virtual extern float GetFloat32(Number byteOffset);

	/// <summary>Reads a 32-bit floating-point value with explicit byte order. 按显式字节序读取 32 位浮点值。</summary>
	[Description("@#getFloat32")]
	public virtual extern float GetFloat32(Number byteOffset, bool littleEndian);

	/// <summary>
	/// Reads a JavaScript float16 value.
	/// The C# surface uses <see cref="float"/> to stay consistent with the existing <see cref="Float16Array"/> projection.
	/// 读取 JavaScript float16；C# 表面使用 <see cref="float"/>，以与现有 <see cref="Float16Array"/> 投影一致。
	/// </summary>
	[Description("@#getFloat16")]
	public virtual extern float GetFloat16(Number byteOffset);

	/// <summary>
	/// Reads a JavaScript float16 value with explicit endian control.
	/// 按显式端序读取 JavaScript float16 值。
	/// </summary>
	[Description("@#getFloat16")]
	public virtual extern float GetFloat16(Number byteOffset, bool littleEndian);

	/// <summary>Reads a 64-bit floating-point value using JavaScript's default big-endian order. 以 JavaScript 默认大端序读取 64 位浮点值。</summary>
	[Description("@#getFloat64")]
	public virtual extern double GetFloat64(Number byteOffset);

	/// <summary>Reads a 64-bit floating-point value with explicit byte order. 按显式字节序读取 64 位浮点值。</summary>
	[Description("@#getFloat64")]
	public virtual extern double GetFloat64(Number byteOffset, bool littleEndian);

	/// <summary>Reads a signed 8-bit integer. 读取有符号 8 位整数。</summary>
	[Description("@#getInt8")]
	public virtual extern sbyte GetInt8(Number byteOffset);

	/// <summary>Reads a signed 16-bit integer using JavaScript's default big-endian order. 以 JavaScript 默认大端序读取有符号 16 位整数。</summary>
	[Description("@#getInt16")]
	public virtual extern short GetInt16(Number byteOffset);

	/// <summary>Reads a signed 16-bit integer with explicit byte order. 按显式字节序读取有符号 16 位整数。</summary>
	[Description("@#getInt16")]
	public virtual extern short GetInt16(Number byteOffset, bool littleEndian);

	/// <summary>Reads a signed 32-bit integer using JavaScript's default big-endian order. 以 JavaScript 默认大端序读取有符号 32 位整数。</summary>
	[Description("@#getInt32")]
	public virtual extern int GetInt32(Number byteOffset);

	/// <summary>Reads a signed 32-bit integer with explicit byte order. 按显式字节序读取有符号 32 位整数。</summary>
	[Description("@#getInt32")]
	public virtual extern int GetInt32(Number byteOffset, bool littleEndian);

	/// <summary>Reads a signed 64-bit JavaScript bigint with explicit byte order. 按显式字节序读取有符号 64 位 JavaScript bigint。</summary>
	[Description("@#getBigInt64")]
	public virtual extern BigInt GetBigInt64(Number byteOffset, bool littleEndian);

	/// <summary>Reads an unsigned 8-bit integer. 读取无符号 8 位整数。</summary>
	[Description("@#getUint8")]
	public virtual extern byte GetUint8(Number byteOffset);

	/// <summary>Reads an unsigned 16-bit integer using JavaScript's default big-endian order. 以 JavaScript 默认大端序读取无符号 16 位整数。</summary>
	[Description("@#getUint16")]
	public virtual extern ushort GetUint16(Number byteOffset);

	/// <summary>Reads an unsigned 16-bit integer with explicit byte order. 按显式字节序读取无符号 16 位整数。</summary>
	[Description("@#getUint16")]
	public virtual extern ushort GetUint16(Number byteOffset, bool littleEndian);

	/// <summary>Reads an unsigned 32-bit integer using JavaScript's default big-endian order. 以 JavaScript 默认大端序读取无符号 32 位整数。</summary>
	[Description("@#getUint32")]
	public virtual extern uint GetUint32(Number byteOffset);

	/// <summary>Reads an unsigned 32-bit integer with explicit byte order. 按显式字节序读取无符号 32 位整数。</summary>
	[Description("@#getUint32")]
	public virtual extern uint GetUint32(Number byteOffset, bool littleEndian);

	/// <summary>Reads an unsigned 64-bit JavaScript bigint with explicit byte order. 按显式字节序读取无符号 64 位 JavaScript bigint。</summary>
	[Description("@#getBigUint64")]
	public virtual extern BigInt GetBigUint64(Number byteOffset, bool littleEndian);

	/// <summary>Writes a 32-bit floating-point value using JavaScript's default big-endian order. 以 JavaScript 默认大端序写入 32 位浮点值。</summary>
	[Description("@#setFloat32")]
	public virtual extern void SetFloat32(Number byteOffset, float value);

	/// <summary>Writes a 32-bit floating-point value with explicit byte order. 按显式字节序写入 32 位浮点值。</summary>
	[Description("@#setFloat32")]
	public virtual extern void SetFloat32(Number byteOffset, float value, bool littleEndian);

	/// <summary>
	/// Writes a JavaScript float16 value.
	/// The C# surface uses <see cref="float"/> to stay consistent with the existing <see cref="Float16Array"/> projection.
	/// 写入 JavaScript float16；C# 表面使用 <see cref="float"/>，以与现有 <see cref="Float16Array"/> 投影一致。
	/// </summary>
	[Description("@#setFloat16")]
	public virtual extern void SetFloat16(Number byteOffset, float value);

	/// <summary>
	/// Writes a JavaScript float16 value with explicit endian control.
	/// 按显式端序写入 JavaScript float16 值。
	/// </summary>
	[Description("@#setFloat16")]
	public virtual extern void SetFloat16(Number byteOffset, float value, bool littleEndian);

	/// <summary>Writes a 64-bit floating-point value using JavaScript's default big-endian order. 以 JavaScript 默认大端序写入 64 位浮点值。</summary>
	[Description("@#setFloat64")]
	public virtual extern void SetFloat64(Number byteOffset, double value);

	/// <summary>Writes a 64-bit floating-point value with explicit byte order. 按显式字节序写入 64 位浮点值。</summary>
	[Description("@#setFloat64")]
	public virtual extern void SetFloat64(Number byteOffset, double value, bool littleEndian);

	/// <summary>Writes a signed 8-bit integer. 写入有符号 8 位整数。</summary>
	[Description("@#setInt8")]
	public virtual extern void SetInt8(Number byteOffset, sbyte value);

	/// <summary>Writes a signed 16-bit integer using JavaScript's default big-endian order. 以 JavaScript 默认大端序写入有符号 16 位整数。</summary>
	[Description("@#setInt16")]
	public virtual extern void SetInt16(Number byteOffset, short value);

	/// <summary>Writes a signed 16-bit integer with explicit byte order. 按显式字节序写入有符号 16 位整数。</summary>
	[Description("@#setInt16")]
	public virtual extern void SetInt16(Number byteOffset, short value, bool littleEndian);

	/// <summary>Writes a signed 32-bit integer using JavaScript's default big-endian order. 以 JavaScript 默认大端序写入有符号 32 位整数。</summary>
	[Description("@#setInt32")]
	public virtual extern void SetInt32(Number byteOffset, int value);

	/// <summary>Writes a signed 32-bit integer with explicit byte order. 按显式字节序写入有符号 32 位整数。</summary>
	[Description("@#setInt32")]
	public virtual extern void SetInt32(Number byteOffset, int value, bool littleEndian);

	/// <summary>Writes a signed 64-bit JavaScript bigint with explicit byte order. 按显式字节序写入有符号 64 位 JavaScript bigint。</summary>
	[Description("@#setBigInt64")]
	public virtual extern void SetBigInt64(Number byteOffset, BigInt value, bool littleEndian);

	/// <summary>Writes an unsigned 8-bit integer. 写入无符号 8 位整数。</summary>
	[Description("@#setUint8")]
	public virtual extern void SetUint8(Number byteOffset, byte value);

	/// <summary>Writes an unsigned 16-bit integer using JavaScript's default big-endian order. 以 JavaScript 默认大端序写入无符号 16 位整数。</summary>
	[Description("@#setUint16")]
	public virtual extern void SetUint16(Number byteOffset, ushort value);

	/// <summary>Writes an unsigned 16-bit integer with explicit byte order. 按显式字节序写入无符号 16 位整数。</summary>
	[Description("@#setUint16")]
	public virtual extern void SetUint16(Number byteOffset, ushort value, bool littleEndian);

	/// <summary>Writes an unsigned 32-bit integer using JavaScript's default big-endian order. 以 JavaScript 默认大端序写入无符号 32 位整数。</summary>
	[Description("@#setUint32")]
	public virtual extern void SetUint32(Number byteOffset, uint value);

	/// <summary>Writes an unsigned 32-bit integer with explicit byte order. 按显式字节序写入无符号 32 位整数。</summary>
	[Description("@#setUint32")]
	public virtual extern void SetUint32(Number byteOffset, uint value, bool littleEndian);

	/// <summary>Writes an unsigned 64-bit JavaScript bigint with explicit byte order. 按显式字节序写入无符号 64 位 JavaScript bigint。</summary>
	[Description("@#setBigUint64")]
	public virtual extern void SetBigUint64(Number byteOffset, BigInt value, bool littleEndian);
}

/// <summary>
/// JavaScript typed array host.
/// <see cref="IEnumerable{T}"/> is used here as the common C# input/output surface for values
/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
/// It does not mean typed arrays follow .NET collection semantics.
/// Typed arrays have a fixed element representation and are views over binary storage; assignments use the JavaScript typed-array conversion rules.
/// <see cref="IEnumerable{T}"/> 仅是数组、列表等 JavaScript iterable 的通用 C# 输入/输出表面，
/// 不代表 typed array 遵循 .NET 集合语义。typed array 具有固定元素表示并视图化二进制存储，赋值遵循 JavaScript 的元素转换规则。
/// </summary>
/// <typeparam name="T">Compile-time element type exposed by the concrete typed-array host. 具体 typed array 宿主公开的元素编译期类型。</typeparam>
/// <typeparam name="TArray">Concrete typed-array host used for fluent return types. 用于流式返回类型的具体 typed array 宿主。</typeparam>
[ECMAScript]
public abstract class TypedArray<T, TArray> : IArrayBufferView, IBufferSource, IEnumerable<T>
	where TArray : TypedArray<T, TArray>
{
	/// <summary>Creates a typed array with the requested element count. 创建指定元素个数的 typed array。</summary>
	public extern TypedArray(Number length);

	/// <summary>
	/// Creates a typed array from a JavaScript iterable.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// 从 JavaScript iterable 创建 typed array；每一项会按具体 typed array 的 JavaScript 转换规则写入。
	/// </summary>
	public extern TypedArray(IEnumerable<T> array);

	/// <summary>Creates a typed-array view over a buffer region. 在缓冲区区域上创建 typed array 视图。</summary>
	public extern TypedArray(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

	/// <summary>
	/// The size in bytes of each element in the array.
	/// 获取每个元素占用的字节数；这是具体 JavaScript typed array 构造器的静态 <c>BYTES_PER_ELEMENT</c>。
	/// </summary>
	[Description("@#BYTES_PER_ELEMENT")]
	public extern static Number BYTES_PER_ELEMENT { get; }

	/// <summary>
	/// Returns a new array from a set of elements.
	/// 从给定元素创建新的 typed array，不展开其中的 iterable。
	/// </summary>
	/// <param name="value">A set of elements to include in the new array object.</param>
	/// <returns></returns>
	[Description("@#of")]
	public extern static TArray Of(params T[] items);

	/// <summary>
	/// Creates an array from an array-like or iterable object.
	/// 从类数组或 iterable 创建 typed array，并按元素类型转换每一项。
	/// </summary>
	/// <param name="arrayLike">An array-like object to convert to an array.</param>
	/// <returns></returns>
	[Description("@#from")]
	public extern static TArray From(IEnumerable<T> arrayLike);

	/// <summary>
	/// Creates an array from an array-like or iterable object.
	/// </summary>
	/// <typeparam name="U">Source element type annotation. 源元素类型标注。</typeparam>
	/// <param name="arrayLike">An array-like object to convert to an array.</param>
	/// <param name="mapFn">A mapping function to call on every element of the array.</param>
	/// <param name="thisArg">Value of 'this' used to invoke the mapfn.</param>
	/// <returns></returns>
	[Description("@#from")]
	public extern static TArray From<U>(IEnumerable<U> arrayLike, Func<U, Number, T> mapFn, object? thisArg = null);

	/// <summary>
	/// Creates an array from an array-like or iterable object.
	/// This overload mirrors JavaScript <c>TypedArray.from</c> when the caller does not need the element index in the mapping callback.
	/// 对应无需元素索引的 JavaScript <c>TypedArray.from</c> 映射回调。
	/// </summary>
	[Description("@#from")]
	public extern static TArray From<U>(IEnumerable<U> arrayLike, Func<U, T> mapFn, object? thisArg = null);

	/// <summary>
	/// The ArrayBuffer instance referenced by the array.
	/// 获取此 typed array 引用的底层 JavaScript 缓冲区。
	/// </summary>
	[Description("@#buffer")]
	public extern ArrayBuffer Buffer { get; }

	/// <summary>
	/// The length in bytes of the array.
	/// 获取此 typed array 视图覆盖的字节长度。
	/// </summary>
	[Description("@#byteLength")]
	public extern Number ByteLength { get; }

	/// <summary>
	/// The offset in bytes of the array.
	/// 获取此 typed array 视图相对底层缓冲区的字节偏移量。
	/// </summary>
	[Description("@#byteOffset")]
	public extern Number ByteOffset { get; }

	/// <summary>
	/// Returns the this object after copying a section of the array identified by start and end
	/// to the same array starting at position target
	/// 将同一 typed array 的区间复制到目标位置并原地返回该数组；区间重叠处理遵循 JavaScript <c>copyWithin</c>。
	/// </summary>
	/// <param name="target">If target is negative, it is treated as length+target where length is the length of the array.</param>
	/// <param name="start">If start is negative, it is treated as length+start.If end is negative, it is treated as length+end.</param>
	/// <param name="end">If not specified, length of the this object is used as its default value.</param>
	/// <returns></returns>
	[Description("@#copyWithin")]
	public extern TArray CopyWithin(Number target, Number start, Number? end = null);

	/// <summary>
	/// Determines whether all the elements of the typed array satisfy the specified test.
	/// 判断所有元素是否满足回调；回调结果按 JavaScript truthy/falsy 规则解释，并在首个 falsy 结果时停止。
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: this) => unknown</b></para>A function that accepts up to three arguments. The every method calls the predicate function for each element in the typed array until the predicate returns a value which is coercible to the Boolean value false, or until the end of the array.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to predicate. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#every")]
	public extern bool Every(Func<T, Number, TArray, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Changes all array elements from `start` to `end` index to a static `value` and returns the modified array
	/// 使用固定值填充指定区间并返回原 typed array；这是原地操作。
	/// </summary>
	/// <param name="value">value to fill array section with</param>
	/// <param name="start">index to start filling the array at.If start is negative, it is treated as length+start where length is the length of the array.</param>
	/// <param name="end">index to stop filling the array at. If end is negative, it is treated as length+end.</param>
	/// <returns></returns>
	[Description("@#fill")]
	public extern TArray Fill(T value, Number? start = null, Number? end = null);

	/// <summary>
	/// Returns the elements of an array that meet the condition specified in a callback function.
	/// 返回谓词结果为 truthy 的元素组成的新同类 typed array；不修改源数组。
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
	/// 返回首个谓词为 truthy 的元素索引；无匹配时返回 <c>-1</c>。回调结果按 JavaScript truthy/falsy 解释。
	/// 返回第一个谓词为 truthy 的元素；无匹配时 <c>undefined</c> 投影为 <see langword="null"/>，回调结果按 JavaScript truthy/falsy 解释。
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
	/// 从后向前返回最后一个谓词为 truthy 的元素；无匹配时 <c>undefined</c> 投影为 <see langword="null"/>。
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
	/// 从后向前返回最后一个谓词为 truthy 的元素索引；无匹配时返回 <c>-1</c>。
	/// </summary>
	[Description("@#findLastIndex")]
	public extern Number FindLastIndex(Func<T, Number, TArray, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Performs the specified action for each element in an array.
	/// 为每个 typed array 元素执行回调；执行和可观察修改遵循 JavaScript <c>forEach</c> 语义。
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: T, index: number, array: this) => void</b></para>A function that accepts up to three arguments. forEach calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<T, Number, TArray> callbackfn, object? thisArg = null);

	/// <summary>
	/// Returns the index of the first occurrence of a value in an array.
	/// 返回首个严格相等的元素索引；未找到时返回 <c>-1</c>。
	/// </summary>
	/// <param name="searchElement">The value to locate in the array.</param>
	/// <param name="fromIndex">The array index at which to begin the search. If negative, it is treated as length + fromIndex.</param>
	/// <returns></returns>
	[Description("@#indexOf")]
	public extern Number IndexOf(T searchElement, Number? fromIndex = null);

	/// <summary>
	/// Projection of JavaScript <c>TypedArray.prototype.includes</c>.
	/// This remains on the typed-array host instead of widening to a CLR collection helper.
	/// 直接投影 JavaScript <c>TypedArray.prototype.includes</c>，保留在 typed-array 宿主而不是扩展为 CLR 集合帮助器。
	/// </summary>
	[Description("@#includes")]
	public extern bool Includes(T searchElement, Number? fromIndex = null);

	/// <summary>
	/// Adds all the elements of an array separated by the specified separator string.
	/// 使用分隔符连接元素为文本；未提供分隔符时遵循 JavaScript 默认逗号规则。
	/// </summary>
	/// <param name="separator">A string used to separate one element of an array from the next in the resulting String.If omitted, the array elements are separated with a comma.</param>
	/// <returns></returns>
	[Description("@#join")]
	public extern string Join(string? separator = null);

	/// <summary>
	/// Returns the index of the last occurrence of a value in an array.
	/// 返回从后向前找到的最后一个严格相等元素索引；未找到时返回 <c>-1</c>。
	/// </summary>
	/// <param name="searchElement">The value to locate in the array.</param>
	/// <param name="fromIndex">The array index at which to begin the search. If negative, it is treated as length + fromIndex.</param>
	/// <returns></returns>
	[Description("@#lastIndexOf")]
	public extern Number LastIndexOf(T searchElement, Number? fromIndex = null);

	/// <summary>
	/// The length of the array.
	/// 获取 typed array 的元素数量，不是字节长度。
	/// </summary>
	[Description("@#length")]
	public extern Number Length { get; }

	/// <summary>
	/// C# host projection of JavaScript <c>TypedArray.prototype.at</c>.
	/// Nullable is used because JavaScript returns <c>undefined</c> for an out-of-range index,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// 直接投影 JavaScript <c>TypedArray.prototype.at</c>，支持负索引；越界 <c>undefined</c> 投影为 <see langword="null"/>。
	/// </summary>
	[Description("@#at")]
	public extern T? At(Number index);

	/// <summary>
	/// Calls a defined callback function on each element of an array, and returns an array that contains the results.
	/// 映射每个元素并返回同类新 typed array；结果写入时遵循目标 typed array 的元素转换规则。
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: T, index: number, array: this) => T</b></para>A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#map")]
	public extern TArray Map(Func<T, Number, TArray, T> callbackfn, object? thisArg = null);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// 从左向右归约；未提供初始值时首个元素为累加器，空 typed array 会按 JavaScript <c>reduce</c> 语义抛出运行时错误。
	/// </summary>
	/// <param name="callbackfn"><para><b>(previousValue: T, currentValue: T, currentIndex: number, array: this) => T</b></para>A function that accepts up to four arguments. When no initial value is supplied, JavaScript uses the first typed-array element as the initial accumulator.</param>
	/// <returns></returns>
	/// <summary>Value-only overload of <c>reduce</c>. 仅接收累加值和当前元素的 <c>reduce</c> 重载。</summary>
	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, Number, TArray, T> callbackfn);

	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, T> callbackfn);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// 从左向右归约，并以 <paramref name="initialValue"/> 作为显式初始累加器。
	/// </summary>
	/// <param name="callbackfn">(previousValue: number, currentValue: number, currentIndex: number, array: this) => number,A function that accepts up to four arguments.The reduce method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	/// <summary>Value-only overload with an initial accumulator. 带初始累加器的仅值 <c>reduce</c> 重载。</summary>
	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, Number, TArray, T> callbackfn, T initialValue);

	[Description("@#reduce")]
	public extern T Reduce(Func<T, T, T> callbackfn, T initialValue);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <typeparam name="U">Compile-time accumulator type. 累加器的编译期类型。</typeparam>
	/// <param name="callbackfn"><para><b>(previousValue: U, currentValue: number, currentIndex: number, array: this) => U</b></para>A function that accepts up to four arguments.The reduce method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	/// <summary>Value-only generic reduction overload. 仅值的泛型归约重载。</summary>
	[Description("@#reduce")]
	public extern U Reduce<U>(Func<U, T, Number, TArray, U> callbackfn, U initialValue);

	[Description("@#reduce")]
	public extern U Reduce<U>(Func<U, T, U> callbackfn, U initialValue);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// 从右向左归约；未提供初始值时末个元素为累加器，空 typed array 会按 JavaScript <c>reduceRight</c> 语义抛出运行时错误。
	/// </summary>
	/// <param name="callbackfn"><para><b>(previousValue: T, currentValue: T, currentIndex: number, array: this) => T</b></para>A function that accepts up to four arguments. When no initial value is supplied, JavaScript uses the last typed-array element as the initial accumulator.</param>
	/// <returns></returns>
	/// <summary>Value-only overload of <c>reduceRight</c>. 仅接收累加值和当前元素的 <c>reduceRight</c> 重载。</summary>
	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, Number, TArray, T> callbackfn);

	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, T> callbackfn);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// 从右向左归约，并以 <paramref name="initialValue"/> 作为显式初始累加器。
	/// </summary>
	/// <param name="callbackfn"><para><b>(previousValue: number, currentValue: number, currentIndex: number, array: this) => number </b></para>A function that accepts up to four arguments.The reduceRight method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	/// <summary>Value-only overload with an initial accumulator. 带初始累加器的仅值 <c>reduceRight</c> 重载。</summary>
	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, Number, TArray, T> callbackfn, T initialValue);

	[Description("@#reduceRight")]
	public extern T ReduceRight(Func<T, T, T> callbackfn, T initialValue);

	/// <summary>
	/// Calls the specified callback function for all the elements in an array, in descending order.The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.
	/// </summary>
	/// <typeparam name="U">Compile-time accumulator type. 累加器的编译期类型。</typeparam>
	/// <param name="callbackfn">(previousValue: U, currentValue: number, currentIndex: number, array: this) => U,A function that accepts up to four arguments.The reduceRight method calls the callbackfn function one time for each element in the array.</param>
	/// <param name="initialValue">If initialValue is specified, it is used as the initial value to start the accumulation.The first call to the callbackfn function provides this value as an argument instead of an array value.</param>
	/// <returns></returns>
	/// <summary>Value-only generic reduction overload. 仅值的泛型归约重载。</summary>
	[Description("@#reduceRight")]
	public extern U ReduceRight<U>(Func<U, T, Number, TArray, U> callbackfn, U initialValue);

	[Description("@#reduceRight")]
	public extern U ReduceRight<U>(Func<U, T, U> callbackfn, U initialValue);

	/// <summary>
	/// Reverses the elements in an Array.
	/// 原地反转 typed array 元素并返回同一实例。
	/// </summary>
	/// <returns></returns>
	[Description("@#reverse")]
	public extern TArray Reverse();

	/// <summary>
	/// Returns a copied typed array with the elements in reverse order.
	/// Unlike <see cref="Reverse"/>, JavaScript <c>toReversed()</c> does not mutate the source typed array.
	/// 返回反转后的 typed array 副本；不同于 <see cref="Reverse"/>，JavaScript <c>toReversed()</c> 不修改源数组。
	/// </summary>
	[Description("@#toReversed")]
	public extern TArray ToReversed();

	/// <summary>
	/// Copies values from a JavaScript iterable into the typed array.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// 将 JavaScript iterable 的值复制到 typed array；<see cref="IEnumerable{T}"/> 是可迭代输入的通用 C# 宿主表面。
	/// </summary>
	/// <param name="array">A typed or untyped iterable of values to copy.</param>
	/// <param name="offset">The index in the current array at which the values are to be written.</param>
	[Description("@#set")]
	public extern void Set(IEnumerable<T> array, Number? offset = null);

	/// <summary>
	/// Returns a section of an array.
	/// 返回 typed array 区间的副本；不修改源数组。
	/// </summary>
	/// <param name="start">The beginning of the specified portion of the array.</param>
	/// <param name="end">The end of the specified portion of the array. This is exclusive of the element at the index 'end'.</param>
	/// <returns></returns>
	[Description("@#slice")]
	public extern TArray Slice(Number? start = null, Number? end = null);

	/// <summary>
	/// Determines whether the specified callback function returns a truthy value for any element of the typed array.
	/// 判断是否存在谓词结果为 truthy 的元素，并在第一个匹配项时停止。
	/// </summary>
	/// <param name="predicate"><para><b>(value: T, index: number, array: this) => unknown</b></para>A function that accepts up to three arguments. The some method calls the predicate function for each element in the typed array until the predicate returns a value which is coercible to the Boolean value true, or until the end of the array.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to predicate. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	/// <returns></returns>
	[Description("@#some")]
	public extern bool Some(Func<T, Number, TArray, object?> predicate, object? thisArg = null);

	/// <summary>
	/// Sorts an array.
	/// 原地排序 typed array 并返回同一实例；未提供比较器时使用 JavaScript typed-array 默认数值排序。
	/// </summary>
	/// <param name="compareFn"><para><b>(a: T, b: T) => number</b></para>Function used to determine the order of the elements. It is expected to return a negative value if first argument is less than second argument, zero if they're equal, and a positive value otherwise. If omitted, the elements are sorted in ascending order.</param>
	/// <returns></returns>
	[Description("@#sort")]
	public extern TArray Sort(Func<T, T, Number>? compareFn = null);

	/// <summary>
	/// Returns a copied typed array with its elements sorted.
	/// This stays on <typeparamref name="TArray"/> because JavaScript preserves the concrete typed-array host.
	/// 返回排序后的 typed array 副本；JavaScript 保留具体 typed-array 宿主，因此返回 <typeparamref name="TArray"/>。
	/// </summary>
	[Description("@#toSorted")]
	public extern TArray ToSorted(Func<T, T, Number>? compareFn = null);

	/// <summary>
	/// Returns a copied typed array with the element at the specified index replaced.
	/// Negative indices follow JavaScript <c>TypedArray.prototype.with</c> semantics and count from the end.
	/// 返回替换指定索引元素后的 typed array 副本；负索引按 JavaScript <c>TypedArray.prototype.with</c> 从末尾计算。
	/// </summary>
	[Description("@#with")]
	public extern TArray With(Number index, T value);

	/// <summary>
	/// Gets a new TArray view of the ArrayBuffer store for this array, referencing the elements at begin, inclusive, up to end, exclusive.
	/// 返回引用同一 ArrayBuffer 存储区的子视图，而不是副本；范围为 [begin, end)。
	/// </summary>
	/// <param name="begin">The index of the beginning of the array.</param>
	/// <param name="end">The index of the end of the array.</param>
	/// <returns></returns>
	[Description("@#subarray")]
	public extern TArray Subarray(Number? begin = null, Number? end = null);

	/// <summary>
	/// Returns the JavaScript string form of the typed array.
	/// This is the direct projection of <c>TypedArray.prototype.toString()</c>.
	/// 直接投影 <c>TypedArray.prototype.toString()</c>，不等同于 CLR 集合格式化。
	/// </summary>
	[Description("@#toString")]
	public extern override string ToString();

	/// <summary>
	/// Returns a locale-sensitive string representation of the typed array.
	/// This is the direct projection of <c>TypedArray.prototype.toLocaleString()</c>.
	/// 直接投影 <c>TypedArray.prototype.toLocaleString()</c>，元素格式化由 JavaScript 运行时决定。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString();

	/// <summary>
	/// Returns a locale-sensitive string representation of the typed array.
	/// JavaScript forwards <paramref name="locales" /> and <paramref name="options" /> to each element's own <c>toLocaleString</c> method.
	/// JavaScript 将 <paramref name="locales"/> 与 <paramref name="options"/> 转交给元素的本地化格式化。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(string? locales, object? options = null);

	/// <summary>
	/// C# convenience overload for the JavaScript form that omits <c>locales</c> and only supplies options.
	/// This exists because C# cannot naturally skip the leading locale argument in method calls.
	/// C# 便利重载，用于只传 options 并省略前置 locales 参数。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(object? options);

	/// <summary>
	/// Returns a locale-sensitive string representation of the typed array.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
	/// JavaScript forwards <paramref name="locales" /> and <paramref name="options" /> to each element's own <c>toLocaleString</c> method.
	/// 使用 <see cref="IEnumerable{T}"/> 表达 JavaScript locale 列表，参数会转交给元素的本地化格式化。
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(IEnumerable<string>? locales, object? options = null);

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>TypedArray.prototype.keys()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>TypedArray.prototype.keys()</c> 产生的 JavaScript 迭代器。
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<Number> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>TypedArray.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>TypedArray.prototype.values()</c> 产生的 JavaScript 迭代器。
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<T> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>TypedArray.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[index, value]</c>.
	/// 返回 <c>TypedArray.prototype.entries()</c> 产生的迭代器；每项是 <c>[index, value]</c> 二元数组。
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<object?>> Entries();

	extern IEnumerator<T> IEnumerable<T>.GetEnumerator();

	extern IEnumerator IEnumerable.GetEnumerator();

	/// <summary>
	/// Direct JavaScript index access surface.
	/// This stays non-nullable to preserve compatibility with array-like CLR projections.
	/// Use <see cref="At" /> when you need a nullable result for out-of-range access.
	/// 直接映射 JavaScript 索引访问；为兼容数组式 CLR 投影保持非空，越界可空结果应使用 <see cref="At"/>。
	/// </summary>
	public extern T this[Number index] { get; set; }

	//[EditorBrowsable(EditorBrowsableState.Never)]
	//public extern override Object this[string name] { get; set; }
}

/// <summary>
/// JavaScript signed 64-bit bigint typed-array view.
/// JavaScript 有符号 64 位 bigint typed array 视图，采用补码表示，范围为 -2^63 至 2^63 - 1。
/// </summary>
[ECMAScript]
[Description("@#BigInt64Array")]
public class BigInt64Array : TypedArray<BigInt, BigInt64Array>, IWaitableAtomicArray<BigInt>
{
	/// <summary>
	/// JavaScript <c>BigInt64Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>BigInt64Array.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static BigInt64Array Prototype { get; }

	/// <summary>Creates a <c>BigInt64Array</c> with the specified element count. 创建指定元素数量的 <c>BigInt64Array</c>。</summary>
	public extern BigInt64Array(Number length);

	/// <summary>Copies values from another <c>BigInt64Array</c>. 从另一个 <c>BigInt64Array</c> 复制值。</summary>
	public extern BigInt64Array(BigInt64Array array);

	/// <summary>Creates a <c>BigInt64Array</c> from bigint iterable values. 从 bigint iterable 值创建 <c>BigInt64Array</c>。</summary>
	public extern BigInt64Array(IEnumerable<BigInt> array);

	/// <summary>Creates a <c>BigInt64Array</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>BigInt64Array</c>。</summary>
	public extern BigInt64Array(IArrayBufferView array);

	/// <summary>Creates a <c>BigInt64Array</c> view over a buffer region. 在缓冲区区域上创建 <c>BigInt64Array</c> 视图。</summary>
	public extern BigInt64Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// JavaScript unsigned 64-bit bigint typed-array view.
/// JavaScript 无符号 64 位 bigint typed array 视图，范围为 0 至 2^64 - 1。
/// </summary>
[ECMAScript]
[Description("@#BigUint64Array")]
public class BigUint64Array : TypedArray<BigInt, BigUint64Array>, IAtomicArray<BigInt>
{
	/// <summary>
	/// JavaScript <c>BigUint64Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>BigUint64Array.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static BigUint64Array Prototype { get; }

	/// <summary>Creates a <c>BigUint64Array</c> with the specified element count. 创建指定元素数量的 <c>BigUint64Array</c>。</summary>
	public extern BigUint64Array(Number length);

	/// <summary>Copies values from another <c>BigUint64Array</c>. 从另一个 <c>BigUint64Array</c> 复制值。</summary>
	public extern BigUint64Array(BigUint64Array array);

	/// <summary>Creates a <c>BigUint64Array</c> from bigint iterable values. 从 bigint iterable 值创建 <c>BigUint64Array</c>。</summary>
	public extern BigUint64Array(IEnumerable<BigInt> array);

	/// <summary>Creates a <c>BigUint64Array</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>BigUint64Array</c>。</summary>
	public extern BigUint64Array(IArrayBufferView array);

	/// <summary>Creates a <c>BigUint64Array</c> view over a buffer region. 在缓冲区区域上创建 <c>BigUint64Array</c> 视图。</summary>
	public extern BigUint64Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// JavaScript half-precision floating-point typed-array view.
/// JavaScript 半精度浮点 typed array 视图；C# 使用 <see cref="float"/> 编写，存储时仍按 float16 舍入。
/// </summary>
[ECMAScript]
[Description("@#Float16Array")]
public class Float16Array : TypedArray<float, Float16Array>
{
	/// <summary>
	/// JavaScript <c>Float16Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>Float16Array.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Float16Array Prototype { get; }

	/// <summary>Creates a <c>Float16Array</c> with the specified element count. 创建指定元素数量的 <c>Float16Array</c>。</summary>
	public extern Float16Array(Number length);

	/// <summary>Copies values from another <c>Float16Array</c>. 从另一个 <c>Float16Array</c> 复制值。</summary>
	public extern Float16Array(Float16Array array);

	/// <summary>Creates a <c>Float16Array</c> from floating-point iterable values. 从浮点 iterable 值创建 <c>Float16Array</c>。</summary>
	public extern Float16Array(IEnumerable<float> array);

	/// <summary>Creates a <c>Float16Array</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>Float16Array</c>。</summary>
	public extern Float16Array(IArrayBufferView array);

	/// <summary>Creates a <c>Float16Array</c> view over a buffer region. 在缓冲区区域上创建 <c>Float16Array</c> 视图。</summary>
	public extern Float16Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// JavaScript IEEE-754 single-precision typed-array view.
/// JavaScript 32 位 IEEE-754 浮点 typed array 视图；写入值按 float32 精度存储，范围约为 -3.4E38 至 3.4E38。
/// </summary>
[ECMAScript]
[Description("@#Float32Array")]
public class Float32Array : TypedArray<float, Float32Array>
{
	/// <summary>
	/// JavaScript <c>Float32Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>Float32Array.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Float32Array Prototype { get; }

	/// <summary>Creates a <c>Float32Array</c> with the specified element count. 创建指定元素数量的 <c>Float32Array</c>。</summary>
	public extern Float32Array(Number length);

	/// <summary>Copies values from another <c>Float32Array</c>. 从另一个 <c>Float32Array</c> 复制值。</summary>
	public extern Float32Array(Float32Array array);

	/// <summary>Creates a <c>Float32Array</c> from floating-point iterable values. 从浮点 iterable 值创建 <c>Float32Array</c>。</summary>
	public extern Float32Array(IEnumerable<float> array);

	/// <summary>Creates a <c>Float32Array</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>Float32Array</c>。</summary>
	public extern Float32Array(IArrayBufferView array);

	/// <summary>Creates a <c>Float32Array</c> view over a buffer region. 在缓冲区区域上创建 <c>Float32Array</c> 视图。</summary>
	public extern Float32Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// JavaScript IEEE-754 double-precision typed-array view.
/// JavaScript 64 位 IEEE-754 双精度 typed array 视图，范围约为 -1.8E308 至 1.8E308。
/// </summary>
[ECMAScript]
[Description("@#Float64Array")]
public class Float64Array : TypedArray<double, Float64Array>
{
	/// <summary>
	/// JavaScript <c>Float64Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>Float64Array.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Float64Array Prototype { get; }

	/// <summary>Creates a <c>Float64Array</c> with the specified element count. 创建指定元素数量的 <c>Float64Array</c>。</summary>
	public extern Float64Array(Number length);

	/// <summary>Copies values from another <c>Float64Array</c>. 从另一个 <c>Float64Array</c> 复制值。</summary>
	public extern Float64Array(Float64Array array);

	/// <summary>Creates a <c>Float64Array</c> from floating-point iterable values. 从浮点 iterable 值创建 <c>Float64Array</c>。</summary>
	public extern Float64Array(IEnumerable<double> array);

	/// <summary>Creates a <c>Float64Array</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>Float64Array</c>。</summary>
	public extern Float64Array(IArrayBufferView array);

	/// <summary>Creates a <c>Float64Array</c> view over a buffer region. 在缓冲区区域上创建 <c>Float64Array</c> 视图。</summary>
	public extern Float64Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// JavaScript signed 8-bit integer typed-array view.
/// JavaScript 有符号 8 位补码 typed array 视图，范围为 -128 至 127。
/// </summary>
[ECMAScript]
[Description("@#Int8Array")]
public class Int8Array : TypedArray<sbyte, Int8Array>, IAtomicArray<sbyte>
{
	/// <summary>
	/// JavaScript <c>Int8Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>Int8Array.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Int8Array Prototype { get; }

	/// <summary>Creates an <c>Int8Array</c> with the specified element count. 创建指定元素数量的 <c>Int8Array</c>。</summary>
	public extern Int8Array(Number length);

	/// <summary>Copies values from another <c>Int8Array</c>. 从另一个 <c>Int8Array</c> 复制值。</summary>
	public extern Int8Array(Int8Array array);

	/// <summary>Creates an <c>Int8Array</c> from signed-byte iterable values. 从有符号字节 iterable 值创建 <c>Int8Array</c>。</summary>
	public extern Int8Array(IEnumerable<sbyte> array);

	/// <summary>Creates an <c>Int8Array</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>Int8Array</c>。</summary>
	public extern Int8Array(IArrayBufferView array);

	/// <summary>Creates an <c>Int8Array</c> view over a buffer region. 在缓冲区区域上创建 <c>Int8Array</c> 视图。</summary>
	public extern Int8Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// JavaScript signed 16-bit integer typed-array view.
/// JavaScript 有符号 16 位补码 typed array 视图，范围为 -32768 至 32767。
/// </summary>
[ECMAScript]
[Description("@#Int16Array")]
public class Int16Array : TypedArray<short, Int16Array>, IAtomicArray<short>
{
	/// <summary>
	/// JavaScript <c>Int16Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>Int16Array.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Int16Array Prototype { get; }

	/// <summary>Creates an <c>Int16Array</c> with the specified element count. 创建指定元素数量的 <c>Int16Array</c>。</summary>
	public extern Int16Array(Number length);

	/// <summary>Copies values from another <c>Int16Array</c>. 从另一个 <c>Int16Array</c> 复制值。</summary>
	public extern Int16Array(Int16Array array);

	/// <summary>Creates an <c>Int16Array</c> from signed 16-bit iterable values. 从有符号 16 位 iterable 值创建 <c>Int16Array</c>。</summary>
	public extern Int16Array(IEnumerable<short> array);

	/// <summary>Creates an <c>Int16Array</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>Int16Array</c>。</summary>
	public extern Int16Array(IArrayBufferView array);

	/// <summary>Creates an <c>Int16Array</c> view over a buffer region. 在缓冲区区域上创建 <c>Int16Array</c> 视图。</summary>
	public extern Int16Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// JavaScript signed 32-bit integer typed-array view.
/// JavaScript 有符号 32 位补码 typed array 视图，范围为 -2147483648 至 2147483647。
/// </summary>
[ECMAScript]
[Description("@#Int32Array")]
public class Int32Array : TypedArray<int, Int32Array>, IWaitableAtomicArray<int>
{
	/// <summary>
	/// JavaScript <c>Int32Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>Int32Array.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Int32Array Prototype { get; }

	/// <summary>Creates an <c>Int32Array</c> with the specified element count. 创建指定元素数量的 <c>Int32Array</c>。</summary>
	public extern Int32Array(Number length);

	/// <summary>Copies values from another <c>Int32Array</c>. 从另一个 <c>Int32Array</c> 复制值。</summary>
	public extern Int32Array(Int32Array array);

	/// <summary>Creates an <c>Int32Array</c> from signed 32-bit iterable values. 从有符号 32 位 iterable 值创建 <c>Int32Array</c>。</summary>
	public extern Int32Array(IEnumerable<int> array);

	/// <summary>Creates an <c>Int32Array</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>Int32Array</c>。</summary>
	public extern Int32Array(IArrayBufferView array);

	/// <summary>Creates an <c>Int32Array</c> view over a buffer region. 在缓冲区区域上创建 <c>Int32Array</c> 视图。</summary>
	public extern Int32Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// JavaScript unsigned 8-bit integer typed-array view.
/// JavaScript 无符号 8 位 typed array 视图，范围为 0 至 255。
/// </summary>
[ECMAScript]
[Description("@#Uint8Array")]
public class Uint8Array : TypedArray<byte, Uint8Array>, IAtomicArray<byte>
{
	/// <summary>
	/// JavaScript <c>Uint8Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>Uint8Array.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Uint8Array Prototype { get; }

	/// <summary>Creates a <c>Uint8Array</c> with the specified element count. 创建指定元素数量的 <c>Uint8Array</c>。</summary>
	public extern Uint8Array(Number length);

	/// <summary>Copies values from another <c>Uint8Array</c>. 从另一个 <c>Uint8Array</c> 复制值。</summary>
	public extern Uint8Array(Uint8Array array);

	/// <summary>Creates a <c>Uint8Array</c> from byte iterable values. 从字节 iterable 值创建 <c>Uint8Array</c>。</summary>
	public extern Uint8Array(IEnumerable<byte> array);

	/// <summary>Creates a <c>Uint8Array</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>Uint8Array</c>。</summary>
	public extern Uint8Array(IArrayBufferView array);

	/// <summary>Creates a <c>Uint8Array</c> view over a buffer region. 在缓冲区区域上创建 <c>Uint8Array</c> 视图。</summary>
	public extern Uint8Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// JavaScript clamped unsigned 8-bit typed-array view.
/// JavaScript 钳位无符号 8 位 typed array 视图；写入值会按 <c>Uint8ClampedArray</c> 规则限制到 0 至 255。
/// </summary>
[ECMAScript]
[Description("@#Uint8ClampedArray")]
public class Uint8ClampedArray : TypedArray<byte, Uint8ClampedArray>
{
	/// <summary>
	/// JavaScript <c>Uint8ClampedArray.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>Uint8ClampedArray.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Uint8ClampedArray Prototype { get; }

	/// <summary>Creates a <c>Uint8ClampedArray</c> with the specified element count. 创建指定元素数量的 <c>Uint8ClampedArray</c>。</summary>
	public extern Uint8ClampedArray(Number length);

	/// <summary>Copies values from another <c>Uint8ClampedArray</c>. 从另一个 <c>Uint8ClampedArray</c> 复制值。</summary>
	public extern Uint8ClampedArray(Uint8ClampedArray array);

	/// <summary>Creates a <c>Uint8ClampedArray</c> from byte iterable values. 从字节 iterable 值创建 <c>Uint8ClampedArray</c>。</summary>
	public extern Uint8ClampedArray(IEnumerable<byte> array);

	/// <summary>Creates a <c>Uint8ClampedArray</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>Uint8ClampedArray</c>。</summary>
	public extern Uint8ClampedArray(IArrayBufferView array);

	/// <summary>Creates a <c>Uint8ClampedArray</c> view over a buffer region. 在缓冲区区域上创建 <c>Uint8ClampedArray</c> 视图。</summary>
	public extern Uint8ClampedArray(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// JavaScript unsigned 16-bit integer typed-array view.
/// JavaScript 无符号 16 位 typed array 视图，范围为 0 至 65535。
/// </summary>
[ECMAScript]
[Description("@#Uint16Array")]
public class Uint16Array : TypedArray<ushort, Uint16Array>, IAtomicArray<ushort>
{
	/// <summary>
	/// JavaScript <c>Uint16Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>Uint16Array.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Uint16Array Prototype { get; }

	/// <summary>Creates a <c>Uint16Array</c> with the specified element count. 创建指定元素数量的 <c>Uint16Array</c>。</summary>
	public extern Uint16Array(Number length);

	/// <summary>Copies values from another <c>Uint16Array</c>. 从另一个 <c>Uint16Array</c> 复制值。</summary>
	public extern Uint16Array(Uint16Array array);

	/// <summary>Creates a <c>Uint16Array</c> from unsigned 16-bit iterable values. 从无符号 16 位 iterable 值创建 <c>Uint16Array</c>。</summary>
	public extern Uint16Array(IEnumerable<ushort> array);

	/// <summary>Creates a <c>Uint16Array</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>Uint16Array</c>。</summary>
	public extern Uint16Array(IArrayBufferView array);

	/// <summary>Creates a <c>Uint16Array</c> view over a buffer region. 在缓冲区区域上创建 <c>Uint16Array</c> 视图。</summary>
	public extern Uint16Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}

/// <summary>
/// JavaScript unsigned 32-bit integer typed-array view.
/// JavaScript 无符号 32 位 typed array 视图，范围为 0 至 4294967295。
/// </summary>
[ECMAScript]
[Description("@#Uint32Array")]
public class Uint32Array : TypedArray<uint, Uint32Array>, IAtomicArray<uint>
{
	/// <summary>
	/// JavaScript <c>Uint32Array.prototype</c> object.
	/// The concrete typed-array constructor host exposes this directly so the runtime shape stays visible in C#.
	/// JavaScript <c>Uint32Array.prototype</c> 对象；具体构造器宿主直接暴露它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Uint32Array Prototype { get; }

	/// <summary>Creates a <c>Uint32Array</c> with the specified element count. 创建指定元素数量的 <c>Uint32Array</c>。</summary>
	public extern Uint32Array(Number length);

	/// <summary>Copies values from another <c>Uint32Array</c>. 从另一个 <c>Uint32Array</c> 复制值。</summary>
	public extern Uint32Array(Uint32Array array);

	/// <summary>Creates a <c>Uint32Array</c> from unsigned 32-bit iterable values. 从无符号 32 位 iterable 值创建 <c>Uint32Array</c>。</summary>
	public extern Uint32Array(IEnumerable<uint> array);

	/// <summary>Creates a <c>Uint32Array</c> by copying another buffer view. 通过复制其他缓冲区视图创建 <c>Uint32Array</c>。</summary>
	public extern Uint32Array(IArrayBufferView array);

	/// <summary>Creates a <c>Uint32Array</c> view over a buffer region. 在缓冲区区域上创建 <c>Uint32Array</c> 视图。</summary>
	public extern Uint32Array(ArrayBuffer buffer, Number? byteOffset = null, Number? length = null);

}
