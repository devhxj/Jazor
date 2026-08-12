namespace ECMAScript;

[ECMAScript]
[Description("@#Map")]
/// <summary>
/// Generic C# authoring binding for JavaScript <c>Map</c>.
/// JavaScript <c>Map</c> 的泛型 C# 编写绑定。
/// </summary>
/// <remarks>
/// Key equality, missing values, and iteration order follow the JavaScript runtime; generic parameters are compile-time annotations only and do not create a CLR map.
/// Reading a missing key and calling <see cref="Has"/> must remain distinct, because a stored value can itself be <see langword="null"/>.
/// Map 的键相等、缺失值和迭代顺序遵循 JavaScript 运行时；泛型参数只提供编译期约束，
/// 不会生成 CLR Map 类型。缺失键读取与 <see cref="Has"/> 必须区分，因为已存储的值也可以为 <see langword="null"/>。
/// </remarks>
public sealed class Map<TKey, TValue> : IEnumerable
{
	/// <summary>Creates an empty JavaScript map. 创建空的 JavaScript Map。</summary>
	public extern Map();

	/// <summary>
	/// Creates a map from a JavaScript iterable of <c>[key, value]</c> entries.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript iterables.
	/// Each inner <see cref="Array{T}"/> represents one JavaScript two-element entry.
	/// 从 JavaScript <c>[key, value]</c> entry iterable 创建 Map；<see cref="IEnumerable{T}"/> 是 JavaScript iterable 的通用 C# 输入表面。
	/// </summary>
	public extern Map(IEnumerable<Array<object?>> entries);

	/// <summary>
	/// Creates a map from a JavaScript iterable of entry iterables.
	/// This overload keeps the public host closer to JavaScript, where each entry only needs to be iterable rather than specifically an Array object.
	/// 从 entry iterable 创建 Map；每个 entry 只需可迭代，不必是特定 <see cref="Array{T}"/> 对象，贴近 JavaScript 构造器输入。
	/// </summary>
	public extern Map(IEnumerable<IEnumerable<object?>> entries);

	/// <summary>
	/// Gets or sets the value associated with <paramref name="key" />.
	/// The getter is nullable because JavaScript returns <c>undefined</c> for a missing key,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// Use <see cref="Has" /> when you need exact presence semantics.
	/// 获取或设置 key 对应的值；缺失 key 的 JavaScript <c>undefined</c> 投影为 <see langword="null"/>，精确存在性请使用 <see cref="Has"/>。
	/// </summary>
	public extern TValue? this[TKey key] { get; set; }

	/// <summary>Stores a key-value pair and returns this map. 存储键值对并返回当前 Map。</summary>
	[Description("@#set")]
	public extern Map<TKey, TValue> Set(TKey key, TValue value);

	/// <summary>
	/// Adds an entry through the C# collection-initializer protocol.
	/// JavaScript exposes this operation as <c>Map.prototype.set</c>; the alternate C# name only
	/// makes <c>new Map&lt;TKey, TValue&gt; { { key, value } }</c> bind without changing runtime shape.
	/// 通过 C# 集合初始化器协议添加 entry；JavaScript 对应 <c>Map.prototype.set</c>，名称变化仅为让集合初始化器绑定，不改变运行时形状。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#set")]
	public extern void Add(TKey key, TValue value);

	/// <summary>
	/// Returns the value associated with <paramref name="key" />.
	/// JavaScript uses <c>undefined</c> when the key is missing,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// Callers that need exact presence semantics should pair this with <see cref="Has" />.
	/// 获取 key 对应的值；缺失时 JavaScript <c>undefined</c> 投影为 <see langword="null"/>，需区分缺失和已存储 null 时请配合 <see cref="Has"/>。
	/// </summary>
	[Description("@#get")]
	public extern TValue? Get(TKey key);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or stores and returns <paramref name="value" /> when the key is missing.
	/// This mirrors JavaScript <c>Map.prototype.getOrInsert</c>.
	/// 返回已有值，或在 key 缺失时存储并返回 <paramref name="value"/>；镜像 JavaScript <c>Map.prototype.getOrInsert</c>。
	/// </summary>
	[Description("@#getOrInsert")]
	public extern TValue GetOrInsert(TKey key, TValue value);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or computes, stores, and returns a new value when the key is missing.
	/// JavaScript invokes the callback with the key as its single argument.
	/// 返回已有值，或在 key 缺失时调用回调计算、存储并返回新值；JavaScript 以 key 作为回调唯一参数。
	/// </summary>
	[Description("@#getOrInsertComputed")]
	public extern TValue GetOrInsertComputed(TKey key, Func<TKey, TValue> callback);

	/// <summary>Checks whether the key exists independently of its stored value. 检查 key 是否存在，与其存储值无关。</summary>
	[Description("@#has")]
	public extern bool Has(TKey key);

	/// <summary>Deletes the key and reports whether it existed. 删除 key 并报告其此前是否存在。</summary>
	[Description("@#delete")]
	public extern bool Delete(TKey key);

	/// <summary>Removes every entry from this map. 删除此 Map 的全部 entry。</summary>
	[Description("@#clear")]
	public extern void Clear();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.keys()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>Map.prototype.keys()</c> 的 JavaScript 迭代器；<see cref="IEnumerable{T}"/> 仅为 C# 宿主表面。
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<TKey> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>Map.prototype.values()</c> 的 JavaScript 迭代器；按 insertion order 产生值。
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<TValue> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[key, value]</c>.
	/// 返回 <c>Map.prototype.entries()</c> 的 JavaScript 迭代器；每项是 <c>[key, value]</c>。
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<object?>> Entries();

	/// <summary>
	/// Calls callbackfn once for each key-value pair in insertion order.
	/// 按 insertion order 为每个键值对执行回调；Map 在迭代中变更的可见性遵循 JavaScript <c>forEach</c> 规则。
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: TValue, key: TKey, map: Map&lt;TKey, TValue&gt;) => void</b></para>A function invoked for each entry.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<TValue, TKey, Map<TKey, TValue>> callbackfn, object? thisArg = null);

	extern IEnumerator IEnumerable.GetEnumerator();

	/// <summary>Gets the number of entries in this map. 获取此 Map 的 entry 数量。</summary>
	[Description("@#size")]
	public extern Number Size { get; }
}

[ECMAScript]
[Description("@#Map")]
/// <summary>Non-generic host binding for JavaScript <c>Map</c>. JavaScript <c>Map</c> 的非泛型宿主绑定。</summary>
public sealed class Map : IEnumerable
{
	/// <summary>
	/// JavaScript <c>Map.prototype</c> object.
	/// The non-generic constructor host carries this member so the runtime shape stays visible in C#.
	/// JavaScript <c>Map.prototype</c> 对象；非泛型构造器宿主直接携带它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Map Prototype { get; }

	/// <summary>Creates an empty JavaScript map. 创建空的 JavaScript Map。</summary>
	public extern Map();

	/// <summary>
	/// Creates a map from a JavaScript iterable of <c>[key, value]</c> entries.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript iterables.
	/// Each inner <see cref="Array{T}"/> represents one JavaScript two-element entry.
	/// 从 JavaScript <c>[key, value]</c> entry iterable 创建 Map；<see cref="IEnumerable{T}"/> 是 JavaScript iterable 的通用 C# 输入表面。
	/// </summary>
	public extern Map(IEnumerable<Array<object?>> entries);

	/// <summary>
	/// Creates a map from a JavaScript iterable of entry iterables.
	/// This overload keeps the public host closer to JavaScript, where each entry only needs to be iterable rather than specifically an Array object.
	/// 从 entry iterable 创建 Map；每个 entry 只需可迭代，不必是具体 Array 对象。
	/// </summary>
	public extern Map(IEnumerable<IEnumerable<object?>> entries);

	/// <summary>
	/// Groups iterable values by arbitrary JavaScript keys and returns the grouped result as a map.
	/// The generic key type is preserved because JavaScript <c>Map.groupBy</c> does not coerce keys to property names.
	/// 按任意 JavaScript key 对 iterable 值分组；保留泛型 key，因为 <c>Map.groupBy</c> 不会将 key 强制转为属性名。
	/// </summary>
	[Description("@#groupBy")]
	public extern static Map<TKey, Array<T>> GroupBy<T, TKey>(IEnumerable<T> items, Func<T, Number, TKey> callbackfn);

	/// <summary>
	/// Groups iterable values by arbitrary JavaScript keys and returns the grouped result as a map.
	/// This overload mirrors the JavaScript callback shape when the caller does not need the index argument.
	/// 不需要索引参数时的 JavaScript <c>Map.groupBy</c> 回调重载。
	/// </summary>
	[Description("@#groupBy")]
	public extern static Map<TKey, Array<T>> GroupBy<T, TKey>(IEnumerable<T> items, Func<T, TKey> callbackfn);

	/// <summary>
	/// Non-generic JavaScript map indexer.
	/// Nullable keys are allowed because JavaScript <c>Map</c> accepts <c>null</c> as an ordinary key value.
	/// 非泛型 JavaScript Map 索引器；可空 key 合法，因为 JavaScript <c>Map</c> 将 <c>null</c> 当作普通 key。
	/// </summary>
	public extern object? this[object? key] { get; set; }

	/// <summary>Stores a key-value pair and returns this map. 存储键值对并返回当前 Map。</summary>
	[Description("@#set")]
	public extern Map Set(object? key, object? value);

	/// <summary>
	/// Returns the value associated with <paramref name="key" />.
	/// If the key is missing, JavaScript returns <c>undefined</c>; this non-generic C# projection surfaces that absence as <see langword="null" />.
	/// Use <see cref="Has" /> when you need to distinguish a missing key from a stored <see langword="null" /> value.
	/// 获取 key 对应的值；缺失 key 的 <c>undefined</c> 投影为 <see langword="null"/>，需与已存储 null 区分时使用 <see cref="Has"/>。
	/// </summary>
	[Description("@#get")]
	public extern object? Get(object? key);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or stores and returns <paramref name="value" /> when the key is missing.
	/// This mirrors JavaScript <c>Map.prototype.getOrInsert</c>.
	/// 返回已有值，或在 key 缺失时存储并返回 <paramref name="value"/>；镜像 <c>Map.prototype.getOrInsert</c>。
	/// </summary>
	[Description("@#getOrInsert")]
	public extern object? GetOrInsert(object? key, object? value);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or computes, stores, and returns a new value when the key is missing.
	/// JavaScript invokes the callback with the key as its single argument.
	/// 返回已有值，或在 key 缺失时计算、存储并返回新值；JavaScript 以 key 作为回调唯一参数。
	/// </summary>
	[Description("@#getOrInsertComputed")]
	public extern object? GetOrInsertComputed(object? key, Func<object?, object?> callback);

	/// <summary>Checks whether the key exists independently of its stored value. 检查 key 是否存在，与存储值无关。</summary>
	[Description("@#has")]
	public extern bool Has(object? key);

	/// <summary>Deletes the key and reports whether it existed. 删除 key 并报告其此前是否存在。</summary>
	[Description("@#delete")]
	public extern bool Delete(object? key);

	/// <summary>Removes every entry from this map. 删除此 Map 的全部 entry。</summary>
	[Description("@#clear")]
	public extern void Clear();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.keys()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>Map.prototype.keys()</c> 的 JavaScript 迭代器。
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<object?> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>Map.prototype.values()</c> 的 JavaScript 迭代器，按 insertion order 产生值。
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<object?> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Map.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[key, value]</c>.
	/// 返回 <c>Map.prototype.entries()</c> 的 JavaScript 迭代器；每项为 <c>[key, value]</c>。
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<object?>> Entries();

	/// <summary>
	/// Calls callbackfn once for each key-value pair in insertion order.
	/// 按 insertion order 为每个键值对执行回调。
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: unknown, key: unknown, map: Map) => void</b></para>A function invoked for each entry.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<object?, object?, Map> callbackfn, object? thisArg = null);

	extern IEnumerator IEnumerable.GetEnumerator();

	/// <summary>Gets the number of entries in this map. 获取此 Map 的 entry 数量。</summary>
	[Description("@#size")]
	public extern Number Size { get; }
}

[ECMAScript]
[Description("@#WeakMap")]
/// <summary>Generic host binding for JavaScript <c>WeakMap</c>. JavaScript <c>WeakMap</c> 的泛型宿主绑定。</summary>
/// <remarks>Weak-map keys are held weakly and the collection is intentionally non-enumerable. The C# <c>class</c> constraint is only authoring guidance; JavaScript performs the final <c>CanBeHeldWeakly</c> validation.
/// WeakMap 的 key 为弱持有，且集合刻意不可枚举。C# <c>class</c> 约束仅提供编写指导，JavaScript 负责最终的 <c>CanBeHeldWeakly</c> 校验。</remarks>
public sealed class WeakMap<TKey, TValue> where TKey : class
{
	/// <summary>Creates an empty JavaScript weak map. 创建空的 JavaScript WeakMap。</summary>
	public extern WeakMap();

	/// <summary>
	/// Creates a weak map from a JavaScript iterable of <c>[key, value]</c> entries.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript iterables.
	/// Each inner <see cref="Array{T}"/> represents one JavaScript two-element entry.
	/// The <c>class</c> constraint is only a C# approximation of JavaScript weakly held keys;
	/// the runtime still enforces the actual <c>CanBeHeldWeakly</c> rule and may reject values such as strings.
	/// 从 JavaScript <c>[key, value]</c> entry iterable 创建 WeakMap；<c>class</c> 约束只是弱持有 key 的 C# 近似，运行时仍执行 <c>CanBeHeldWeakly</c> 校验。
	/// </summary>
	public extern WeakMap(IEnumerable<Array<object?>> entries);

	/// <summary>
	/// Creates a weak map from a JavaScript iterable of entry iterables.
	/// This overload keeps the public host closer to JavaScript, where each entry only needs to be iterable rather than specifically an Array object.
	/// The runtime still enforces the actual weak-key rules for the first entry value.
	/// 从 entry iterable 创建 WeakMap；entry 只需可迭代，首个值的弱 key 合法性仍由 JavaScript 运行时校验。
	/// </summary>
	public extern WeakMap(IEnumerable<IEnumerable<object?>> entries);

	/// <summary>Stores a value under a weakly held key and returns this map. 在弱持有 key 下存储值并返回当前 WeakMap。</summary>
	[Description("@#set")]
	public extern WeakMap<TKey, TValue> Set(TKey key, TValue value);

	/// <summary>
	/// Returns the value associated with <paramref name="key" />.
	/// JavaScript uses <c>undefined</c> when the key is missing,
	/// and the C# projection maps that absence to <see langword="null" />.
	/// Callers that need exact presence semantics should pair this with <see cref="Has" />.
	/// 获取 key 对应的值；缺失 key 的 <c>undefined</c> 投影为 <see langword="null"/>，精确存在性请使用 <see cref="Has"/>。
	/// </summary>
	[Description("@#get")]
	public extern TValue? Get(TKey key);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or stores and returns <paramref name="value" /> when the key is missing.
	/// This mirrors JavaScript <c>WeakMap.prototype.getOrInsert</c>.
	/// The runtime still enforces JavaScript weak-reference rules for <paramref name="key" />.
	/// 返回已有值，或在 key 缺失时存储并返回值；运行时仍对 <paramref name="key"/> 执行 JavaScript 弱引用合法性校验。
	/// </summary>
	[Description("@#getOrInsert")]
	public extern TValue GetOrInsert(TKey key, TValue value);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or computes, stores, and returns a new value when the key is missing.
	/// JavaScript invokes the callback with the key as its single argument.
	/// The runtime still enforces JavaScript weak-reference rules for <paramref name="key" />.
	/// 返回已有值，或在 key 缺失时计算、存储并返回新值；运行时仍校验 key 的弱持有合法性。
	/// </summary>
	[Description("@#getOrInsertComputed")]
	public extern TValue GetOrInsertComputed(TKey key, Func<TKey, TValue> callback);

	/// <summary>Checks whether a weak key is present. 检查弱 key 是否存在。</summary>
	[Description("@#has")]
	public extern bool Has(TKey key);

	/// <summary>Deletes a weak key and reports whether it was present. 删除弱 key 并报告其此前是否存在。</summary>
	[Description("@#delete")]
	public extern bool Delete(TKey key);
}

[ECMAScript]
[Description("@#WeakMap")]
/// <summary>Non-generic host binding for JavaScript <c>WeakMap</c>. JavaScript <c>WeakMap</c> 的非泛型宿主绑定。</summary>
public sealed class WeakMap
{
	/// <summary>
	/// JavaScript <c>WeakMap.prototype</c> object.
	/// The non-generic constructor host carries this member so the runtime shape stays visible in C#.
	/// JavaScript <c>WeakMap.prototype</c> 对象；非泛型构造器宿主直接携带它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static WeakMap Prototype { get; }

	/// <summary>Creates an empty JavaScript weak map. 创建空的 JavaScript WeakMap。</summary>
	public extern WeakMap();

	/// <summary>
	/// Creates a weak map from a JavaScript iterable of <c>[key, value]</c> entries.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript iterables.
	/// Each inner <see cref="Array{T}"/> represents one JavaScript two-element entry.
	/// Keys are intentionally not nullable on this host because JavaScript weak-map keys must be weakly held values.
	/// That includes objects and non-global symbols; the runtime performs the final validity check.
	/// 从 JavaScript <c>[key, value]</c> entry iterable 创建 WeakMap；key 不可为空，因为 JavaScript weak-map key 必须可弱持有，最终有效性由运行时判断。
	/// </summary>
	public extern WeakMap(IEnumerable<Array<object?>> entries);

	/// <summary>
	/// Creates a weak map from a JavaScript iterable of entry iterables.
	/// This overload keeps the public host closer to JavaScript, where each entry only needs to be iterable rather than specifically an Array object.
	/// The runtime still performs the final weak-key validation for the first entry value.
	/// 从 entry iterable 创建 WeakMap；首个 entry 值的弱 key 合法性仍由 JavaScript 运行时校验。
	/// </summary>
	public extern WeakMap(IEnumerable<IEnumerable<object?>> entries);

	/// <summary>Stores a value under a weakly held key and returns this map. 在弱持有 key 下存储值并返回当前 WeakMap。</summary>
	[Description("@#set")]
	public extern WeakMap Set(object key, object? value);

	/// <summary>
	/// Returns the value associated with <paramref name="key" />.
	/// If the key is missing, JavaScript returns <c>undefined</c>; this non-generic C# projection surfaces that absence as <see langword="null" />.
	/// Use <see cref="Has" /> when you need to distinguish a missing key from a stored <see langword="null" /> value.
	/// 获取 key 对应的值；缺失 key 的 <c>undefined</c> 投影为 <see langword="null"/>，需区分已存储 null 时请使用 <see cref="Has"/>。
	/// </summary>
	[Description("@#get")]
	public extern object? Get(object key);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or stores and returns <paramref name="value" /> when the key is missing.
	/// This mirrors JavaScript <c>WeakMap.prototype.getOrInsert</c>.
	/// The runtime still enforces JavaScript weak-reference rules for <paramref name="key" />.
	/// 返回已有值，或在 key 缺失时存储并返回值；运行时仍校验 key 是否可弱持有。
	/// </summary>
	[Description("@#getOrInsert")]
	public extern object? GetOrInsert(object key, object? value);

	/// <summary>
	/// Returns the existing value for <paramref name="key" />, or computes, stores, and returns a new value when the key is missing.
	/// JavaScript invokes the callback with the key as its single argument.
	/// The runtime still enforces JavaScript weak-reference rules for <paramref name="key" />.
	/// 返回已有值，或在 key 缺失时计算、存储并返回新值；运行时仍校验 key 是否可弱持有。
	/// </summary>
	[Description("@#getOrInsertComputed")]
	public extern object? GetOrInsertComputed(object key, Func<object, object?> callback);

	[Description("@#has")]
	public extern bool Has(object key);

	[Description("@#delete")]
	public extern bool Delete(object key);
}
