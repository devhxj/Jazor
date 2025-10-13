namespace ECMAScript;

[ECMAScript]
[DisplayName("Set")]
public sealed class Set<T> : IEnumerable //where T : class
{
	public extern Set<T> Add(T value);

	public extern bool Has(T value);

	public extern bool Delete(T value);

	public extern void Clear();

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

