namespace ECMAScript;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <param name="Value"></param>
/// <param name="key"></param>
/// <param name="map"></param>
[ECMAScript]
public delegate void MapCallbackFunc<TKey, TValue>(TValue Value, TKey key, Map<TKey, TValue> map);

[ECMAScript]
public delegate void MapCallbackFunc(object? Value, object key, Map map);

[ECMAScript]
[Description("@#Map")]
public sealed class Map<TKey, TValue> : IEnumerable
{
	public extern TValue this[TKey key] { get; set; }

	public extern Map<TKey, TValue> Set(TKey key, TValue value);

	public extern TValue Get(TKey key);

	public extern bool Has(TKey key);

	public extern bool Delete(TKey key);

	public extern void Clear();

	public extern void ForEach(MapCallbackFunc<TKey, TValue> callbackfn);

	extern IEnumerator IEnumerable.GetEnumerator();

	public extern Number Size { get; }
}

[ECMAScript]
[Description("@#Map")]
public sealed class Map : IEnumerable
{
	public extern object? this[object key] { get; set; }

	public extern Map Set(object key, object? value);

	public extern object? Get(object key);

	public extern bool Has(object key);

	public extern bool Delete(object key);

	public extern void Clear();

	public extern void ForEach(MapCallbackFunc callbackfn);

	extern IEnumerator IEnumerable.GetEnumerator();

	public extern Number Size { get; }
}

[ECMAScript]
[Description("@#WeakMap")]
public sealed class WeakMap<TKey, TValue>  //where TKey : class
{
	public extern void Set(TKey key, TValue value);

	public extern TValue Get(TKey key);

	public extern bool Has(TKey key);

	public extern bool Delete(TKey key);
}

[ECMAScript]
[Description("@#WeakMap")]
public sealed class WeakMap //where TKey : class
{
	public extern void Set(object key, object? value);

	public extern object? Get(object key);

	public extern bool Has(object key);

	public extern bool Delete(object key);
}

