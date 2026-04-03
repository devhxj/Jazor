namespace ECMAScript;

[ECMAScript]
[Description("@#Map")]
public sealed class Map<TKey, TValue> : IEnumerable
{
	public extern Map();

	/// <summary>
	/// Creates a map from a JavaScript iterable of <c>[key, value]</c> entries.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript iterables.
	/// Each inner <see cref="Array{T}"/> represents one JavaScript two-element entry.
	/// </summary>
	public extern Map(IEnumerable<Array<object?>> entries);

	public extern TValue this[TKey key] { get; set; }

	[Description("@#set")]
	public extern Map<TKey, TValue> Set(TKey key, TValue value);

	[Description("@#get")]
	public extern TValue Get(TKey key);

	[Description("@#has")]
	public extern bool Has(TKey key);

	[Description("@#delete")]
	public extern bool Delete(TKey key);

	[Description("@#clear")]
	public extern void Clear();

	/// <summary>
	/// Calls callbackfn once for each key-value pair in insertion order.
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: TValue, key: TKey, map: Map&lt;TKey, TValue&gt;) => void</b></para>A function invoked for each entry.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, undefined is used.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<TValue, TKey, Map<TKey, TValue>> callbackfn, object? thisArg = null);

	extern IEnumerator IEnumerable.GetEnumerator();

	[Description("@#size")]
	public extern Number Size { get; }
}

[ECMAScript]
[Description("@#Map")]
public sealed class Map : IEnumerable
{
	public extern Map();

	/// <summary>
	/// Creates a map from a JavaScript iterable of <c>[key, value]</c> entries.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript iterables.
	/// Each inner <see cref="Array{T}"/> represents one JavaScript two-element entry.
	/// </summary>
	public extern Map(IEnumerable<Array<object?>> entries);

	public extern object? this[object key] { get; set; }

	[Description("@#set")]
	public extern Map Set(object key, object? value);

	[Description("@#get")]
	public extern object? Get(object key);

	[Description("@#has")]
	public extern bool Has(object key);

	[Description("@#delete")]
	public extern bool Delete(object key);

	[Description("@#clear")]
	public extern void Clear();

	/// <summary>
	/// Calls callbackfn once for each key-value pair in insertion order.
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: unknown, key: unknown, map: Map) => void</b></para>A function invoked for each entry.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, undefined is used.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<object?, object, Map> callbackfn, object? thisArg = null);

	extern IEnumerator IEnumerable.GetEnumerator();

	[Description("@#size")]
	public extern Number Size { get; }
}

[ECMAScript]
[Description("@#WeakMap")]
public sealed class WeakMap<TKey, TValue> where TKey : class
{
	public extern WeakMap();

	/// <summary>
	/// Creates a weak map from a JavaScript iterable of <c>[key, value]</c> entries.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript iterables.
	/// Each inner <see cref="Array{T}"/> represents one JavaScript two-element entry.
	/// </summary>
	public extern WeakMap(IEnumerable<Array<object?>> entries);

	[Description("@#set")]
	public extern WeakMap<TKey, TValue> Set(TKey key, TValue value);

	[Description("@#get")]
	public extern TValue Get(TKey key);

	[Description("@#has")]
	public extern bool Has(TKey key);

	[Description("@#delete")]
	public extern bool Delete(TKey key);
}

[ECMAScript]
[Description("@#WeakMap")]
public sealed class WeakMap
{
	public extern WeakMap();

	/// <summary>
	/// Creates a weak map from a JavaScript iterable of <c>[key, value]</c> entries.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript iterables.
	/// Each inner <see cref="Array{T}"/> represents one JavaScript two-element entry.
	/// </summary>
	public extern WeakMap(IEnumerable<Array<object?>> entries);

	[Description("@#set")]
	public extern WeakMap Set(object key, object? value);

	[Description("@#get")]
	public extern object? Get(object key);

	[Description("@#has")]
	public extern bool Has(object key);

	[Description("@#delete")]
	public extern bool Delete(object key);
}

