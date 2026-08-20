namespace ECMAScript;

/// <summary>
/// JavaScript object shape returned by <c>Atomics.waitAsync</c>.
/// The <c>value</c> field is either an immediate status string or a promise that resolves to that status, so the union is kept explicit.
/// JavaScript <c>Atomics.waitAsync</c> 返回的对象形状；<c>value</c> 可为立即状态字符串或解析为状态的 Promise，因此明确保留联合类型。
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class AtomicsWaitAsyncResult
{
	/// <summary>
	/// Gets whether the operation completed asynchronously.
	/// 获取操作是否异步完成。
	/// </summary>
	[Description("@#async")]
	public extern bool Async { get; }

	/// <summary>
	/// Gets the immediate status string or the promise that will resolve to that status.
	/// JavaScript uses strings such as <c>"ok"</c>, <c>"not-equal"</c>, or <c>"timed-out"</c>.
	/// 获取立即状态字符串或将解析为该状态的 Promise；JavaScript 使用 <c>"ok"</c>、<c>"not-equal"</c>、<c>"timed-out"</c> 等字符串。
	/// </summary>
	[Description("@#value")]
	public extern AtomicsWaitAsyncValue Value { get; }
}

/// <summary>
/// Projection of JavaScript's <c>Atomics</c> host object.
/// The bridge interfaces on typed arrays keep the C# surface limited to JavaScript's atomic integer-array family,
/// while the shared-buffer backing requirement still remains a runtime constraint just like in JavaScript.
/// JavaScript <c>Atomics</c> 宿主对象投影；类型化数组桥接接口将 C# 表面限制在 JavaScript 原子整数数组族中，
/// 共享缓冲区后备要求仍是与 JavaScript 一致的运行时约束。
/// </summary>
[ECMAScript]
[Description("@#Atomics")]
public static class Atomics
{
	/// <summary>
	/// Returns whether atomic operations on values of the given byte size are lock-free on the current JavaScript runtime.
	/// 返回当前 JavaScript 运行时对给定字节大小的原子操作是否无锁。
	/// </summary>
	[Description("@#isLockFree")]
	public extern static bool IsLockFree(Number size);

	/// <summary>
	/// Atomically adds a value and returns the previous element value.
	/// 原子加上值并返回元素的旧值。
	/// </summary>
	[Description("@#add")]
	public extern static T Add<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically subtracts a value and returns the previous element value.
	/// 原子减去值并返回元素的旧值。
	/// </summary>
	[Description("@#sub")]
	public extern static T Sub<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically applies bitwise AND and returns the previous element value.
	/// 原子执行按位 AND 并返回元素的旧值。
	/// </summary>
	[Description("@#and")]
	public extern static T And<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically applies bitwise OR and returns the previous element value.
	/// 原子执行按位 OR 并返回元素的旧值。
	/// </summary>
	[Description("@#or")]
	public extern static T Or<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically applies bitwise XOR and returns the previous element value.
	/// 原子执行按位 XOR 并返回元素的旧值。
	/// </summary>
	[Description("@#xor")]
	public extern static T Xor<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically stores a value and returns the previous element value.
	/// 原子存储值并返回元素的旧值。
	/// </summary>
	[Description("@#exchange")]
	public extern static T Exchange<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically stores a replacement when the current value matches the expected value, and returns the previous element value.
	/// 当前值匹配预期值时原子存储替代值，并返回元素的旧值。
	/// </summary>
	[Description("@#compareExchange")]
	public extern static T CompareExchange<T>(IAtomicArray<T> typedArray, Number index, T expectedValue, T replacementValue);

	/// <summary>
	/// Atomically reads the current element value.
	/// 原子读取当前元素值。
	/// </summary>
	[Description("@#load")]
	public extern static T Load<T>(IAtomicArray<T> typedArray, Number index);

	/// <summary>
	/// Atomically writes a value and returns that stored value.
	/// 原子写入值并返回写入后的值。
	/// </summary>
	[Description("@#store")]
	public extern static T Store<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Blocks the current agent until the indexed value changes, the wait times out, or the wait is notified.
	/// JavaScript returns status strings such as <c>"ok"</c>, <c>"not-equal"</c>, or <c>"timed-out"</c>.
	/// 阻塞当前 agent，直到索引值改变、等待超时或被通知；JavaScript 返回 <c>"ok"</c>、<c>"not-equal"</c>、<c>"timed-out"</c> 等状态字符串。
	/// </summary>
	[Description("@#wait")]
	public extern static string Wait<T>(IWaitableAtomicArray<T> typedArray, Number index, T value, Number? timeout = null);

	/// <summary>
	/// Starts an asynchronous wait on the indexed value.
	/// JavaScript exposes both a synchronous completion flag and a value field, so the result is modeled as its own object shape instead of collapsing it to a promise.
	/// 对索引值开始异步等待；JavaScript 同时公开同步完成标志和 value 字段，因此结果建模为独立对象形状而不折叠为 Promise。
	/// </summary>
	[Description("@#waitAsync")]
	public extern static AtomicsWaitAsyncResult WaitAsync<T>(IWaitableAtomicArray<T> typedArray, Number index, T value, Number? timeout = null);

	/// <summary>
	/// Wakes agents waiting on the indexed element and returns the number of agents that were notified.
	/// 唤醒等待该索引元素的 agent，并返回已通知的 agent 数量。
	/// </summary>
	[Description("@#notify")]
	public extern static Number Notify<T>(IWaitableAtomicArray<T> typedArray, Number index, Number? count = null);
}
