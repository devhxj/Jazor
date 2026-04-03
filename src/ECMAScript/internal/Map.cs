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

	/// <summary>
	/// Gets or sets the value associated with <paramref name="key" />.
	/// The getter is nullable because JavaScript returns <c>undefined</c> for a missing key,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// Use <see cref="Has" /> when you need exact presence semantics.
	/// </summary>
	public extern TValue? this[TKey key] { get; set; }

	[Description("@#set")]
	public extern Map<TKey, TValue> Set(TKey key, TValue value);

	/// <summary>
	/// Returns the value associated with <paramref name="key" />.
	/// JavaScript uses <c>undefined</c> when the key is missing,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// Callers that need exact presence semantics should pair this with <see cref="Has" />.
	/// </summary>
	[Description("@#get")]
	public extern TValue? Get(TKey key);

	[Description("@#has")]
	public extern bool Has(TKey key);

	[Description("@#delete")]
	public extern bool Delete(TKey key);

	[Description("@#clear")]
	public extern void Clear();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.keys()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<TKey> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<TValue> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[key, value]</c>.
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<object?>> Entries();

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

	/// <summary>
	/// Groups iterable values by arbitrary JavaScript keys and returns the grouped result as a map.
	/// The generic key type is preserved because JavaScript <c>Map.groupBy</c> does not coerce keys to property names.
	/// </summary>
	[Description("@#groupBy")]
	public extern static Map<TKey, Array<T>> GroupBy<T, TKey>(IEnumerable<T> items, Func<T, Number, TKey> callbackfn);

	/// <summary>
	/// Groups iterable values by arbitrary JavaScript keys and returns the grouped result as a map.
	/// This overload mirrors the JavaScript callback shape when the caller does not need the index argument.
	/// </summary>
	[Description("@#groupBy")]
	public extern static Map<TKey, Array<T>> GroupBy<T, TKey>(IEnumerable<T> items, Func<T, TKey> callbackfn);

	public extern object? this[object key] { get; set; }

	[Description("@#set")]
	public extern Map Set(object key, object? value);

	/// <summary>
	/// Returns the value associated with <paramref name="key" />.
	/// If the key is missing, JavaScript returns <c>undefined</c>; this non-generic C# projection surfaces that absence as <see langword="null" />.
	/// Use <see cref="Has" /> when you need to distinguish a missing key from a stored <see langword="null" /> value.
	/// </summary>
	[Description("@#get")]
	public extern object? Get(object key);

	[Description("@#has")]
	public extern bool Has(object key);

	[Description("@#delete")]
	public extern bool Delete(object key);

	[Description("@#clear")]
	public extern void Clear();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.keys()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<object> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<object?> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[key, value]</c>.
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<object?>> Entries();

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

	/// <summary>
	/// Returns the value associated with <paramref name="key" />.
	/// JavaScript uses <c>undefined</c> when the key is missing,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// Callers that need exact presence semantics should pair this with <see cref="Has" />.
	/// </summary>
	[Description("@#get")]
	public extern TValue? Get(TKey key);

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

	/// <summary>
	/// Returns the value associated with <paramref name="key" />.
	/// If the key is missing, JavaScript returns <c>undefined</c>; this non-generic C# projection surfaces that absence as <see langword="null" />.
	/// Use <see cref="Has" /> when you need to distinguish a missing key from a stored <see langword="null" /> value.
	/// </summary>
	[Description("@#get")]
	public extern object? Get(object key);

	[Description("@#has")]
	public extern bool Has(object key);

	[Description("@#delete")]
	public extern bool Delete(object key);
}

