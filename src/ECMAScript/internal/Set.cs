namespace ECMAScript;

[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ISetLike<T>
{
	/// <summary>
	/// JavaScript set-like size property.
	/// This bridge exists so new Set methods can accept the same kind of set-like operand that JavaScript specifies, without inventing a new runtime host.
	/// </summary>
	[Description("@#size")]
	Number Size { get; }

	/// <summary>
	/// JavaScript set-like membership check.
	/// </summary>
	[Description("@#has")]
	bool Has(T value);

	/// <summary>
	/// JavaScript set-like key iterator.
	/// For Set-like objects this yields the values themselves, matching the JavaScript protocol used by the new Set methods.
	/// </summary>
	[Description("@#keys")]
	IEnumerable<T> Keys();
}

[ECMAScript]
[Description("@#Set")]
/// <summary>
/// JavaScript Set 的泛型 C# authoring binding。
/// </summary>
/// <remarks>
/// Set 的唯一性和 NaN/零值比较遵循 JavaScript SameValueZero 规则；T 只是编译期标注，
/// 不代表运行时会进行 CLR 类型检查。
/// </remarks>
public sealed class Set<T> : IEnumerable, ISetLike<T> //where T : class
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
	/// Returns a new set containing values present in either this set or the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.union</c> and does not mutate the source set.
	/// </summary>
	[Description("@#union")]
	public extern Set<T> Union(ISetLike<T> other);

	/// <summary>
	/// Returns a new set containing values present in both this set and the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.intersection</c> and does not mutate the source set.
	/// </summary>
	[Description("@#intersection")]
	public extern Set<T> Intersection(ISetLike<T> other);

	/// <summary>
	/// Returns a new set containing values present in this set but not in the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.difference</c> and does not mutate the source set.
	/// </summary>
	[Description("@#difference")]
	public extern Set<T> Difference(ISetLike<T> other);

	/// <summary>
	/// Returns a new set containing values present in exactly one of the two set-like operands.
	/// This mirrors JavaScript <c>Set.prototype.symmetricDifference</c> and does not mutate the source set.
	/// </summary>
	[Description("@#symmetricDifference")]
	public extern Set<T> SymmetricDifference(ISetLike<T> other);

	/// <summary>
	/// Returns whether every value in this set is also present in the other set-like operand.
	/// </summary>
	[Description("@#isSubsetOf")]
	public extern bool IsSubsetOf(ISetLike<T> other);

	/// <summary>
	/// Returns whether every value in the other set-like operand is also present in this set.
	/// </summary>
	[Description("@#isSupersetOf")]
	public extern bool IsSupersetOf(ISetLike<T> other);

	/// <summary>
	/// Returns whether this set and the other set-like operand share no values.
	/// </summary>
	[Description("@#isDisjointFrom")]
	public extern bool IsDisjointFrom(ISetLike<T> other);

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.keys()</c>.
	/// In JavaScript, this yields the same values as <c>values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<T> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<T> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[value, value]</c>.
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<T>> Entries();

	/// <summary>
	/// Calls callbackfn once for each value in insertion order.
	/// In JavaScript Set.prototype.forEach, the second callback argument repeats the value rather than exposing an index.
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: T, key: T, set: Set&lt;T&gt;) => void</b></para>A function invoked for each value.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
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
[Description("@#Set")]
public sealed class Set : IEnumerable, ISetLike<object?>
{
	/// <summary>
	/// JavaScript <c>Set.prototype</c> object.
	/// The non-generic constructor host carries this member so the runtime shape stays recognizable in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static Set Prototype { get; }

	public extern Set();

	/// <summary>
	/// Creates a set from a JavaScript iterable of values.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// Nullable element types are used because JavaScript <c>Set</c> accepts <see langword="null" /> as an ordinary stored value.
	/// </summary>
	public extern Set(IEnumerable<object?> values);

	[Description("@#add")]
	public extern Set Add(object? value);

	[Description("@#has")]
	public extern bool Has(object? value);

	[Description("@#delete")]
	public extern bool Delete(object? value);

	[Description("@#clear")]
	public extern void Clear();

	/// <summary>
	/// Returns a new set containing values present in either this set or the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.union</c> and does not mutate the source set.
	/// </summary>
	[Description("@#union")]
	public extern Set Union(ISetLike<object?> other);

	/// <summary>
	/// Returns a new set containing values present in both this set and the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.intersection</c> and does not mutate the source set.
	/// </summary>
	[Description("@#intersection")]
	public extern Set Intersection(ISetLike<object?> other);

	/// <summary>
	/// Returns a new set containing values present in this set but not in the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.difference</c> and does not mutate the source set.
	/// </summary>
	[Description("@#difference")]
	public extern Set Difference(ISetLike<object?> other);

	/// <summary>
	/// Returns a new set containing values present in exactly one of the two set-like operands.
	/// This mirrors JavaScript <c>Set.prototype.symmetricDifference</c> and does not mutate the source set.
	/// </summary>
	[Description("@#symmetricDifference")]
	public extern Set SymmetricDifference(ISetLike<object?> other);

	/// <summary>
	/// Returns whether every value in this set is also present in the other set-like operand.
	/// </summary>
	[Description("@#isSubsetOf")]
	public extern bool IsSubsetOf(ISetLike<object?> other);

	/// <summary>
	/// Returns whether every value in the other set-like operand is also present in this set.
	/// </summary>
	[Description("@#isSupersetOf")]
	public extern bool IsSupersetOf(ISetLike<object?> other);

	/// <summary>
	/// Returns whether this set and the other set-like operand share no values.
	/// </summary>
	[Description("@#isDisjointFrom")]
	public extern bool IsDisjointFrom(ISetLike<object?> other);

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.keys()</c>.
	/// In JavaScript, this yields the same values as <c>values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<object?> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<object?> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[value, value]</c>.
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<object?>> Entries();

	/// <summary>
	/// Calls callbackfn once for each value in insertion order.
	/// In JavaScript Set.prototype.forEach, the second callback argument repeats the value rather than exposing an index.
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: unknown, key: unknown, set: Set) => void</b></para>A function invoked for each value.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<object?, object?, Set> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(Action<object?, object?> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(Action<object?> callbackfn, object? thisArg = null);

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
	/// The <c>class</c> constraint is only a C# approximation of JavaScript weakly held values;
	/// the runtime still enforces the actual <c>CanBeHeldWeakly</c> rule and may reject values such as strings.
	/// </summary>
	public extern WeakSet(IEnumerable<T> values);

	[Description("@#add")]
	public extern WeakSet<T> Add(T value);

	[Description("@#has")]
	public extern bool Has(T value);

	[Description("@#delete")]
	public extern bool Delete(T value);
}

[ECMAScript]
[Description("@#WeakSet")]
public sealed class WeakSet
{
	/// <summary>
	/// JavaScript <c>WeakSet.prototype</c> object.
	/// The non-generic constructor host carries this member so the runtime shape stays visible in C#.
	/// </summary>
	[Description("@#prototype")]
	public extern static WeakSet Prototype { get; }

	public extern WeakSet();

	/// <summary>
	/// Creates a weak set from a JavaScript iterable of values.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// WeakSet itself is not enumerable in JavaScript, so this host intentionally does not implement <see cref="IEnumerable" />.
	/// Values are intentionally not nullable on this host because JavaScript weak-set entries must be weakly held values.
	/// That includes objects and non-global symbols; the runtime performs the final validity check.
	/// </summary>
	public extern WeakSet(IEnumerable<object> values);

	[Description("@#add")]
	public extern WeakSet Add(object value);

	[Description("@#has")]
	public extern bool Has(object value);

	[Description("@#delete")]
	public extern bool Delete(object value);
}

