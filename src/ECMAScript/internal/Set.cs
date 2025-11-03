namespace ECMAScript;

[ECMAScript]
[DisplayName("Set")]
public sealed class Set<T> : IEnumerable //where T : class
{
	public extern Set<T> Add(T value);

	public extern bool Has(T value);

	public extern bool Delete(T value);

	public extern void Clear();

	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc<T, uint> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc1<T, uint> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc2<T, uint> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc3<T, uint> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc4<T, uint> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(CallbackFunc5<T, uint> callbackfn, object? thisArg = null);


	public extern Number Size { get; }

	extern IEnumerator IEnumerable.GetEnumerator();
}

[ECMAScript]
[DisplayName("WeakSet")]
public sealed class WeakSet<T> :  IEnumerable where T : class
{
	public extern Set<T> Add(T value);

	public extern bool Has(T value);

	public extern bool Delete(T value);

	extern IEnumerator IEnumerable.GetEnumerator();
}

