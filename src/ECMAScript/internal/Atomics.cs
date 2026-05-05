namespace ECMAScript;

/// <summary>
/// JavaScript object shape returned by <c>Atomics.waitAsync</c>.
/// The <c>value</c> field is either an immediate status string or a promise that resolves to that status, so the union is kept explicit.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class AtomicsWaitAsyncResult
{
	/// <summary>
	/// Indicates whether the operation completed asynchronously.
	/// </summary>
	[Description("@#async")]
	public extern bool Async { get; }

	/// <summary>
	/// Immediate status string or the promise that will resolve to that status.
	/// JavaScript uses strings such as <c>"ok"</c>, <c>"not-equal"</c>, or <c>"timed-out"</c>.
	/// </summary>
	[Description("@#value")]
	public extern AtomicsWaitAsyncValue Value { get; }
}

/// <summary>
/// Projection of JavaScript's <c>Atomics</c> host object.
/// The bridge interfaces on typed arrays keep the C# surface limited to JavaScript's atomic integer-array family,
/// while the shared-buffer backing requirement still remains a runtime constraint just like in JavaScript.
/// </summary>
[ECMAScript]
[Description("@#Atomics")]
public static class Atomics
{
	/// <summary>
	/// Returns whether atomic operations on values of the given byte size are lock-free on the current JavaScript runtime.
	/// </summary>
	[Description("@#isLockFree")]
	public extern static bool IsLockFree(Number size);

	/// <summary>
	/// Atomically adds a value and returns the previous element value.
	/// </summary>
	[Description("@#add")]
	public extern static T Add<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically subtracts a value and returns the previous element value.
	/// </summary>
	[Description("@#sub")]
	public extern static T Sub<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically applies bitwise and and returns the previous element value.
	/// </summary>
	[Description("@#and")]
	public extern static T And<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically applies bitwise or and returns the previous element value.
	/// </summary>
	[Description("@#or")]
	public extern static T Or<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically applies bitwise xor and returns the previous element value.
	/// </summary>
	[Description("@#xor")]
	public extern static T Xor<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically stores a value and returns the previous element value.
	/// </summary>
	[Description("@#exchange")]
	public extern static T Exchange<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Atomically stores a replacement when the current value matches the expected value, and returns the previous element value.
	/// </summary>
	[Description("@#compareExchange")]
	public extern static T CompareExchange<T>(IAtomicArray<T> typedArray, Number index, T expectedValue, T replacementValue);

	/// <summary>
	/// Atomically reads the current element value.
	/// </summary>
	[Description("@#load")]
	public extern static T Load<T>(IAtomicArray<T> typedArray, Number index);

	/// <summary>
	/// Atomically writes a value and returns that stored value.
	/// </summary>
	[Description("@#store")]
	public extern static T Store<T>(IAtomicArray<T> typedArray, Number index, T value);

	/// <summary>
	/// Blocks the current agent until the indexed value changes, the wait times out, or the wait is notified.
	/// JavaScript returns status strings such as <c>"ok"</c>, <c>"not-equal"</c>, or <c>"timed-out"</c>.
	/// </summary>
	[Description("@#wait")]
	public extern static string Wait<T>(IWaitableAtomicArray<T> typedArray, Number index, T value, Number? timeout = null);

	/// <summary>
	/// Starts an asynchronous wait on the indexed value.
	/// JavaScript exposes both a synchronous completion flag and a value field, so the result is modeled as its own object shape instead of collapsing it to a promise.
	/// </summary>
	[Description("@#waitAsync")]
	public extern static AtomicsWaitAsyncResult WaitAsync<T>(IWaitableAtomicArray<T> typedArray, Number index, T value, Number? timeout = null);

	/// <summary>
	/// Wakes agents waiting on the indexed element and returns the number of agents that were notified.
	/// </summary>
	[Description("@#notify")]
	public extern static Number Notify<T>(IWaitableAtomicArray<T> typedArray, Number index, Number? count = null);
}
