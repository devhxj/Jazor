namespace ECMAScript;

[ECMAScript]
[Description("@#Set")]
public sealed class Set<T> : IEnumerable //where T : class
{
	public extern Set();

	/// <summary>
	/// Creates a set from a JavaScript iterable of values.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// </summary>
	public extern Set(IEnumerable<T> values);

	[Description("@#add")]
	public extern Set<T> Add(T value);

	[Description("@#has")]
	public extern bool Has(T value);

	[Description("@#delete")]
	public extern bool Delete(T value);

	[Description("@#clear")]
	public extern void Clear();

	/// <summary>
	/// Calls callbackfn once for each value in insertion order.
	/// In JavaScript Set.prototype.forEach, the second callback argument repeats the value rather than exposing an index.
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: T, key: T, set: Set&lt;T&gt;) => void</b></para>A function invoked for each value.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, undefined is used.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<T, T, Set<T>> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(Action<T, T> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(Action<T> callbackfn, object? thisArg = null);

	[Description("@#size")]
	public extern Number Size { get; }

	extern IEnumerator IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#WeakSet")]
public sealed class WeakSet<T> where T : class
{
	public extern WeakSet();

	/// <summary>
	/// Creates a weak set from a JavaScript iterable of values.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// WeakSet itself is not enumerable in JavaScript, so this host intentionally does not implement <see cref="IEnumerable" />.
	/// </summary>
	public extern WeakSet(IEnumerable<T> values);

	[Description("@#add")]
	public extern WeakSet<T> Add(T value);

	[Description("@#has")]
	public extern bool Has(T value);

	[Description("@#delete")]
	public extern bool Delete(T value);
}

