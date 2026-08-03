namespace ECMAScript;

[ECMAScript]
[Description("@#Map")]
/// <summary>
/// JavaScript Map 的泛型 C# authoring binding。
/// </summary>
/// <remarks>
/// Map 的键相等、缺失值和迭代顺序遵循 JavaScript 运行时；泛型参数只提供编译期约束，
/// 不会生成 CLR Map 类型。缺失键读取与 Has 必须区分，不能用 null 代替存在性判断。
/// </remarks>
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
	/// Creates a map from a JavaScript iterable of entry iterables.
	/// This overload keeps the public host closer to JavaScript, where each entry only needs to be iterable rather than specifically an Array object.
	/// </summary>
	public extern Map(IEnumerable<IEnumerable<object?>> entries);

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
	/// Adds an entry through the C# collection-initializer protocol.
	/// JavaScript exposes this operation as <c>Map.prototype.set</c>; the alternate C# name only
	/// makes <c>new Map&lt;TKey, TValue&gt; { { key, value } }</c> bind without changing runtime shape.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#set")]
	public extern void Add(TKey key, TValue value);

	/// <summary>
	/// Returns the value associated with <paramref name="key" />.
	/// JavaScript uses <c>undefined</c> when the key is missing,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// Callers that need exact presence semantics should pair this with <see cref="Has" />.
	/// </summary>
	[Description("@#get")]
	public extern TValue? Get(TKey key);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or stores and returns <paramref name="value" /> when the key is missing.
	/// This mirrors JavaScript <c>Map.prototype.getOrInsert</c>.
	/// </summary>
	[Description("@#getOrInsert")]
	public extern TValue GetOrInsert(TKey key, TValue value);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or computes, stores, and returns a new value when the key is missing.
	/// JavaScript invokes the callback with the key as its single argument.
	/// </summary>
	[Description("@#getOrInsertComputed")]
	public extern TValue GetOrInsertComputed(TKey key, Func<TKey, TValue> callback);

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
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<TValue, TKey, Map<TKey, TValue>> callbackfn, object? thisArg = null);

	extern IEnumerator IEnumerable.GetEnumerator();

	[Description("@#size")]
	public extern Number Size { get; }
}

[ECMAScript]
[Description("@#Map")]
/// <summary>JavaScript Map 的非泛型 host binding。</summary>
public sealed class Map : IEnumerable
{
	/// <summary>
	/// JavaScript <c>Map.prototype</c> object.
	/// The non-generic constructor host carries this member so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Map Prototype { get; }

	public extern Map();

	/// <summary>
	/// Creates a map from a JavaScript iterable of <c>[key, value]</c> entries.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript iterables.
	/// Each inner <see cref="Array{T}"/> represents one JavaScript two-element entry.
	/// </summary>
	public extern Map(IEnumerable<Array<object?>> entries);

	/// <summary>
	/// Creates a map from a JavaScript iterable of entry iterables.
	/// This overload keeps the public host closer to JavaScript, where each entry only needs to be iterable rather than specifically an Array object.
	/// </summary>
	public extern Map(IEnumerable<IEnumerable<object?>> entries);

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

	/// <summary>
	/// Non-generic JavaScript map indexer.
	/// Nullable keys are allowed because JavaScript <c>Map</c> accepts <c>null</c> as an ordinary key value.
	/// </summary>
	public extern object? this[object? key] { get; set; }

	[Description("@#set")]
	public extern Map Set(object? key, object? value);

	/// <summary>
	/// Returns the value associated with <paramref name="key" />.
	/// If the key is missing, JavaScript returns <c>undefined</c>; this non-generic C# projection surfaces that absence as <see langword="null" />.
	/// Use <see cref="Has" /> when you need to distinguish a missing key from a stored <see langword="null" /> value.
	/// </summary>
	[Description("@#get")]
	public extern object? Get(object? key);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or stores and returns <paramref name="value" /> when the key is missing.
	/// This mirrors JavaScript <c>Map.prototype.getOrInsert</c>.
	/// </summary>
	[Description("@#getOrInsert")]
	public extern object? GetOrInsert(object? key, object? value);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or computes, stores, and returns a new value when the key is missing.
	/// JavaScript invokes the callback with the key as its single argument.
	/// </summary>
	[Description("@#getOrInsertComputed")]
	public extern object? GetOrInsertComputed(object? key, Func<object?, object?> callback);

	[Description("@#has")]
	public extern bool Has(object? key);

	[Description("@#delete")]
	public extern bool Delete(object? key);

	[Description("@#clear")]
	public extern void Clear();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.keys()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<object?> Keys();

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
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<object?, object?, Map> callbackfn, object? thisArg = null);

	extern IEnumerator IEnumerable.GetEnumerator();

	[Description("@#size")]
	public extern Number Size { get; }
}

[ECMAScript]
[Description("@#WeakMap")]
/// <summary>JavaScript WeakMap 的泛型 host binding。</summary>
public sealed class WeakMap<TKey, TValue> where TKey : class
{
	public extern WeakMap();

	/// <summary>
	/// Creates a weak map from a JavaScript iterable of <c>[key, value]</c> entries.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript iterables.
	/// Each inner <see cref="Array{T}"/> represents one JavaScript two-element entry.
	/// The <c>class</c> constraint is only a C# approximation of JavaScript weakly held keys;
	/// the runtime still enforces the actual <c>CanBeHeldWeakly</c> rule and may reject values such as strings.
	/// </summary>
	public extern WeakMap(IEnumerable<Array<object?>> entries);

	/// <summary>
	/// Creates a weak map from a JavaScript iterable of entry iterables.
	/// This overload keeps the public host closer to JavaScript, where each entry only needs to be iterable rather than specifically an Array object.
	/// The runtime still enforces the actual weak-key rules for the first entry value.
	/// </summary>
	public extern WeakMap(IEnumerable<IEnumerable<object?>> entries);

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

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or stores and returns <paramref name="value" /> when the key is missing.
	/// This mirrors JavaScript <c>WeakMap.prototype.getOrInsert</c>.
	/// The runtime still enforces JavaScript weak-reference rules for <paramref name="key" />.
	/// </summary>
	[Description("@#getOrInsert")]
	public extern TValue GetOrInsert(TKey key, TValue value);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or computes, stores, and returns a new value when the key is missing.
	/// JavaScript invokes the callback with the key as its single argument.
	/// The runtime still enforces JavaScript weak-reference rules for <paramref name="key" />.
	/// </summary>
	[Description("@#getOrInsertComputed")]
	public extern TValue GetOrInsertComputed(TKey key, Func<TKey, TValue> callback);

	[Description("@#has")]
	public extern bool Has(TKey key);

	[Description("@#delete")]
	public extern bool Delete(TKey key);
}

[ECMAScript]
[Description("@#WeakMap")]
/// <summary>JavaScript WeakMap 的非泛型 host binding。</summary>
public sealed class WeakMap
{
	/// <summary>
	/// JavaScript <c>WeakMap.prototype</c> object.
	/// The non-generic constructor host carries this member so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static WeakMap Prototype { get; }

	public extern WeakMap();

	/// <summary>
	/// Creates a weak map from a JavaScript iterable of <c>[key, value]</c> entries.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript iterables.
	/// Each inner <see cref="Array{T}"/> represents one JavaScript two-element entry.
	/// Keys are intentionally not nullable on this host because JavaScript weak-map keys must be weakly held values.
	/// That includes objects and non-global symbols; the runtime performs the final validity check.
	/// </summary>
	public extern WeakMap(IEnumerable<Array<object?>> entries);

	/// <summary>
	/// Creates a weak map from a JavaScript iterable of entry iterables.
	/// This overload keeps the public host closer to JavaScript, where each entry only needs to be iterable rather than specifically an Array object.
	/// The runtime still performs the final weak-key validation for the first entry value.
	/// </summary>
	public extern WeakMap(IEnumerable<IEnumerable<object?>> entries);

	[Description("@#set")]
	public extern WeakMap Set(object key, object? value);

	/// <summary>
	/// Returns the value associated with <paramref name="key" />.
	/// If the key is missing, JavaScript returns <c>undefined</c>; this non-generic C# projection surfaces that absence as <see langword="null" />.
	/// Use <see cref="Has" /> when you need to distinguish a missing key from a stored <see langword="null" /> value.
	/// </summary>
	[Description("@#get")]
	public extern object? Get(object key);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or stores and returns <paramref name="value" /> when the key is missing.
	/// This mirrors JavaScript <c>WeakMap.prototype.getOrInsert</c>.
	/// The runtime still enforces JavaScript weak-reference rules for <paramref name="key" />.
	/// </summary>
	[Description("@#getOrInsert")]
	public extern object? GetOrInsert(object key, object? value);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or computes, stores, and returns a new value when the key is missing.
	/// JavaScript invokes the callback with the key as its single argument.
	/// The runtime still enforces JavaScript weak-reference rules for <paramref name="key" />.
	/// </summary>
	[Description("@#getOrInsertComputed")]
	public extern object? GetOrInsertComputed(object key, Func<object, object?> callback);

	[Description("@#has")]
	public extern bool Has(object key);

	[Description("@#delete")]
	public extern bool Delete(object key);
}
