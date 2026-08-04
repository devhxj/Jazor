namespace Jazor.CLR;

/// <summary>
/// Maps <see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}"/> to the two-slot
/// Array entry emitted by JavaScript <c>Map</c> iteration.
/// </summary>
/// <remarks>
/// Dictionary and the LINQ aggregations that return key/value pairs share this carrier. Keeping
/// the positional projection here lets ordinary property access and C# deconstruction agree
/// without introducing a wrapper object or changing Map enumeration.
/// </remarks>
[ECMAScriptModule("System/Collections/Generic/KeyValuePairT2Module.js")]
[Jazor(Op.Alias, "System.Collections.Generic.KeyValuePair<TKey, TValue>", "Array")]
public static class KeyValuePairT2Module<TKey, TValue>
{
	/// <summary>
	/// C#: new KeyValuePair&lt;TKey, TValue&gt;(key, value)
	/// JS: [key, value]
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.KeyValuePair<TKey, TValue>.KeyValuePair(TKey, TValue)", "[__arg1, __arg2]")]
	public extern static Array Create(TKey key, TValue value);

	[Jazor(Op.Inline, "System.Collections.Generic.KeyValuePair<TKey, TValue>.Key.get", "__arg1[0]")]
	public extern static TKey GetKey(Array<TKey> instance);

	[Jazor(Op.Inline, "System.Collections.Generic.KeyValuePair<TKey, TValue>.Value.get", "__arg1[1]")]
	public extern static TValue GetValue(Array<TValue> instance);

	// foreach key/value deconstruction already follows the compiler's structural Array binding.
	// A direct Deconstruct(out, out) mapping needs a dedicated ref/out adapter and remains unsupported.
	[Jazor(Op.Discard, "System.Collections.Generic.KeyValuePair<TKey, TValue>.Deconstruct(out TKey, out TValue)")]
	public extern static Array Deconstruct(Array<TKey> instance, TKey key, TValue value);
}
