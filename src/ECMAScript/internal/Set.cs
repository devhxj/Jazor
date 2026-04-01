namespace ECMAScript;

[ECMAScript]
[Description("@#Set")]
public sealed class Set<T> : IEnumerable //where T : class
{
	[Description("@#add")]
	public extern Set<T> Add(T value);

	[Description("@#has")]
	public extern bool Has(T value);

	[Description("@#delete")]
	public extern bool Delete(T value);

	[Description("@#clear")]
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

	[Description("@#size")]
	public extern Number Size { get; }

	extern IEnumerator IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#WeakSet")]
public sealed class WeakSet<T> :  IEnumerable where T : class
{
	[Description("@#add")]
	public extern WeakSet<T> Add(T value);

	[Description("@#has")]
	public extern bool Has(T value);

	[Description("@#delete")]
	public extern bool Delete(T value);

	extern IEnumerator IEnumerable.GetEnumerator();
}

