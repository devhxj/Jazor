namespace ECMAScript;

[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
/// <summary>
/// JavaScript set-like protocol consumed by the new <c>Set.prototype</c> relation methods.
/// 新版 JavaScript <c>Set.prototype</c> 关系方法消费的 set-like 协议。
/// </summary>
public interface ISetLike<T>
{
	/// <summary>
	/// JavaScript set-like size property.
	/// This bridge exists so new Set methods can accept the same kind of set-like operand that JavaScript specifies, without inventing a new runtime host.
	/// JavaScript set-like 的 <c>size</c> 属性；该桥接使新 Set 方法能接受规范定义的 set-like 操作数，而不引入新运行时宿主。
	/// </summary>
	[Description("@#size")]
	Number Size { get; }

	/// <summary>
	/// JavaScript set-like membership check.
	/// JavaScript set-like 的成员检查。
	/// </summary>
	[Description("@#has")]
	bool Has(T value);

	/// <summary>
	/// JavaScript set-like key iterator.
	/// For Set-like objects this yields the values themselves, matching the JavaScript protocol used by the new Set methods.
	/// JavaScript set-like 的 key 迭代器；对 set-like 对象它产生值本身，符合新版 Set 方法使用的协议。
	/// </summary>
	[Description("@#keys")]
	IEnumerable<T> Keys();
}

[ECMAScript]
[Description("@#Set")]
/// <summary>
/// Generic C# authoring binding for JavaScript <c>Set</c>.
/// JavaScript <c>Set</c> 的泛型 C# 编写绑定。
/// </summary>
/// <remarks>
/// Uniqueness and NaN/zero comparison follow JavaScript SameValueZero semantics; <typeparamref name="T"/> is only a compile-time annotation and does not cause CLR runtime type checks.
/// Set 的唯一性和 NaN/零值比较遵循 JavaScript SameValueZero 规则；<typeparamref name="T"/> 只是编译期标注，
/// 不代表运行时会进行 CLR 类型检查。
/// </remarks>
public sealed class Set<T> : IEnumerable, ISetLike<T> //where T : class
{
	/// <summary>Creates an empty JavaScript set. 创建空的 JavaScript Set。</summary>
	public extern Set();

	/// <summary>
	/// Creates a set from a JavaScript iterable of values.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// 从 JavaScript iterable 创建 Set；<see cref="IEnumerable{T}"/> 是数组、列表等可迭代值的通用 C# 输入表面。
	/// </summary>
	public extern Set(IEnumerable<T> values);

	/// <summary>Adds a value and returns this set; duplicate values are ignored by SameValueZero equality. 添加值并返回当前 Set；按 SameValueZero 相等的重复值会被忽略。</summary>
	[Description("@#add")]
	public extern Set<T> Add(T value);

	/// <summary>Checks whether a value is present using JavaScript SameValueZero equality. 使用 JavaScript SameValueZero 相等规则检查值是否存在。</summary>
	[Description("@#has")]
	public extern bool Has(T value);

	/// <summary>Deletes a value and reports whether it was present. 删除值并报告其此前是否存在。</summary>
	[Description("@#delete")]
	public extern bool Delete(T value);

	/// <summary>Removes every value from this set. 删除此 Set 的全部值。</summary>
	[Description("@#clear")]
	public extern void Clear();

	/// <summary>
	/// Returns a new set containing values present in either this set or the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.union</c> and does not mutate the source set.
	/// 返回当前 Set 与另一个 set-like 操作数的并集，不修改源 Set；镜像 JavaScript <c>Set.prototype.union</c>。
	/// </summary>
	[Description("@#union")]
	public extern Set<T> Union(ISetLike<T> other);

	/// <summary>
	/// Returns a new set containing values present in both this set and the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.intersection</c> and does not mutate the source set.
	/// 返回交集，不修改源 Set；镜像 JavaScript <c>Set.prototype.intersection</c>。
	/// </summary>
	[Description("@#intersection")]
	public extern Set<T> Intersection(ISetLike<T> other);

	/// <summary>
	/// Returns a new set containing values present in this set but not in the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.difference</c> and does not mutate the source set.
	/// 返回当前 Set 中存在而另一个 set-like 操作数中不存在的值，不修改源 Set。
	/// </summary>
	[Description("@#difference")]
	public extern Set<T> Difference(ISetLike<T> other);

	/// <summary>
	/// Returns a new set containing values present in exactly one of the two set-like operands.
	/// This mirrors JavaScript <c>Set.prototype.symmetricDifference</c> and does not mutate the source set.
	/// 返回仅存在于两个 set-like 操作数之一的值，不修改源 Set。
	/// </summary>
	[Description("@#symmetricDifference")]
	public extern Set<T> SymmetricDifference(ISetLike<T> other);

	/// <summary>
	/// Returns whether every value in this set is also present in the other set-like operand.
	/// 检查当前 Set 是否为另一个 set-like 操作数的子集。
	/// </summary>
	[Description("@#isSubsetOf")]
	public extern bool IsSubsetOf(ISetLike<T> other);

	/// <summary>
	/// Returns whether every value in the other set-like operand is also present in this set.
	/// 检查当前 Set 是否为另一个 set-like 操作数的超集。
	/// </summary>
	[Description("@#isSupersetOf")]
	public extern bool IsSupersetOf(ISetLike<T> other);

	/// <summary>
	/// Returns whether this set and the other set-like operand share no values.
	/// 检查当前 Set 与另一个 set-like 操作数是否没有共同值。
	/// </summary>
	[Description("@#isDisjointFrom")]
	public extern bool IsDisjointFrom(ISetLike<T> other);

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.keys()</c>.
	/// In JavaScript, this yields the same values as <c>values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>Set.prototype.keys()</c> 的 JavaScript 迭代器；对 Set 来说与 <c>values()</c> 产生相同值。
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<T> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>Set.prototype.values()</c> 的 JavaScript 迭代器，按 insertion order 产生值。
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<T> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[value, value]</c>.
	/// 返回 <c>Set.prototype.entries()</c> 的 JavaScript 迭代器；每项为 <c>[value, value]</c>。
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<T>> Entries();

	/// <summary>
	/// Calls callbackfn once for each value in insertion order.
	/// In JavaScript Set.prototype.forEach, the second callback argument repeats the value rather than exposing an index.
	/// 按 insertion order 为每个值执行回调；JavaScript <c>Set.prototype.forEach</c> 的第二个回调参数重复该值，不是索引。
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: T, key: T, set: Set&lt;T&gt;) => void</b></para>A function invoked for each value.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<T, T, Set<T>> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(Action<T, T> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(Action<T> callbackfn, object? thisArg = null);

	/// <summary>Gets the number of values in this set. 获取此 Set 的值数量。</summary>
	[Description("@#size")]
	public extern Number Size { get; }

	extern IEnumerator IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#Set")]
/// <summary>Non-generic host binding for JavaScript <c>Set</c>. JavaScript <c>Set</c> 的非泛型宿主绑定。</summary>
public sealed class Set : IEnumerable, ISetLike<object?>
{
	/// <summary>
	/// JavaScript <c>Set.prototype</c> object.
	/// The non-generic constructor host carries this member so the runtime shape stays recognizable in C#.
	/// JavaScript <c>Set.prototype</c> 对象；非泛型构造器宿主直接携带它，使 C# 中仍可识别运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static Set Prototype { get; }

	/// <summary>Creates an empty JavaScript set. 创建空的 JavaScript Set。</summary>
	public extern Set();

	/// <summary>
	/// Creates a set from a JavaScript iterable of values.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// Nullable element types are used because JavaScript <c>Set</c> accepts <see langword="null" /> as an ordinary stored value.
	/// 从 JavaScript iterable 创建 Set；元素可空，因为 JavaScript <c>Set</c> 将 <see langword="null"/> 视为普通存储值。
	/// </summary>
	public extern Set(IEnumerable<object?> values);

	/// <summary>Adds a value and returns this set. 添加值并返回当前 Set。</summary>
	[Description("@#add")]
	public extern Set Add(object? value);

	/// <summary>Checks whether a value is present using JavaScript SameValueZero equality. 使用 JavaScript SameValueZero 相等规则检查值是否存在。</summary>
	[Description("@#has")]
	public extern bool Has(object? value);

	/// <summary>Deletes a value and reports whether it was present. 删除值并报告其此前是否存在。</summary>
	[Description("@#delete")]
	public extern bool Delete(object? value);

	/// <summary>Removes every value from this set. 删除此 Set 的全部值。</summary>
	[Description("@#clear")]
	public extern void Clear();

	/// <summary>
	/// Returns a new set containing values present in either this set or the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.union</c> and does not mutate the source set.
	/// 返回并集，不修改源 Set；镜像 JavaScript <c>Set.prototype.union</c>。
	/// </summary>
	[Description("@#union")]
	public extern Set Union(ISetLike<object?> other);

	/// <summary>
	/// Returns a new set containing values present in both this set and the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.intersection</c> and does not mutate the source set.
	/// 返回交集，不修改源 Set；镜像 JavaScript <c>Set.prototype.intersection</c>。
	/// </summary>
	[Description("@#intersection")]
	public extern Set Intersection(ISetLike<object?> other);

	/// <summary>
	/// Returns a new set containing values present in this set but not in the other set-like operand.
	/// This mirrors JavaScript <c>Set.prototype.difference</c> and does not mutate the source set.
	/// 返回差集，不修改源 Set；镜像 JavaScript <c>Set.prototype.difference</c>。
	/// </summary>
	[Description("@#difference")]
	public extern Set Difference(ISetLike<object?> other);

	/// <summary>
	/// Returns a new set containing values present in exactly one of the two set-like operands.
	/// This mirrors JavaScript <c>Set.prototype.symmetricDifference</c> and does not mutate the source set.
	/// 返回对称差集，不修改源 Set；镜像 JavaScript <c>Set.prototype.symmetricDifference</c>。
	/// </summary>
	[Description("@#symmetricDifference")]
	public extern Set SymmetricDifference(ISetLike<object?> other);

	/// <summary>
	/// Returns whether every value in this set is also present in the other set-like operand.
	/// 检查当前 Set 是否为另一个 set-like 操作数的子集。
	/// </summary>
	[Description("@#isSubsetOf")]
	public extern bool IsSubsetOf(ISetLike<object?> other);

	/// <summary>
	/// Returns whether every value in the other set-like operand is also present in this set.
	/// 检查当前 Set 是否为另一个 set-like 操作数的超集。
	/// </summary>
	[Description("@#isSupersetOf")]
	public extern bool IsSupersetOf(ISetLike<object?> other);

	/// <summary>
	/// Returns whether this set and the other set-like operand share no values.
	/// 检查当前 Set 与另一个 set-like 操作数是否没有共同值。
	/// </summary>
	[Description("@#isDisjointFrom")]
	public extern bool IsDisjointFrom(ISetLike<object?> other);

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.keys()</c>.
	/// In JavaScript, this yields the same values as <c>values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>Set.prototype.keys()</c> 的 JavaScript 迭代器；对 Set 与 <c>values()</c> 相同。
	/// </summary>
	[Description("@#keys")]
	public extern IEnumerable<object?> Keys();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.values()</c>.
	/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
	/// 返回 <c>Set.prototype.values()</c> 的 JavaScript 迭代器，按 insertion order 产生值。
	/// </summary>
	[Description("@#values")]
	public extern IEnumerable<object?> Values();

	/// <summary>
	/// Returns the JavaScript iterator produced by <c>Set.prototype.entries()</c>.
	/// Each yielded item is the JavaScript two-element pair <c>[value, value]</c>.
	/// 返回 <c>Set.prototype.entries()</c> 的 JavaScript 迭代器；每项为 <c>[value, value]</c>。
	/// </summary>
	[Description("@#entries")]
	public extern IEnumerable<Array<object?>> Entries();

	/// <summary>
	/// Calls callbackfn once for each value in insertion order.
	/// In JavaScript Set.prototype.forEach, the second callback argument repeats the value rather than exposing an index.
	/// 按 insertion order 执行回调；第二个参数重复 value，不是索引。
	/// </summary>
	/// <param name="callbackfn"><para><b>(value: unknown, key: unknown, set: Set) => void</b></para>A function invoked for each value.</param>
	/// <param name="thisArg">An arbitrary value passed as the JavaScript this argument to callbackfn. If omitted, JavaScript uses its default callback receiver; this projection does not expose <c>undefined</c> as a separate public value.</param>
	[Description("@#forEach")]
	public extern void ForEach(Action<object?, object?, Set> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(Action<object?, object?> callbackfn, object? thisArg = null);
	[Description("@#forEach")]
	public extern void ForEach(Action<object?> callbackfn, object? thisArg = null);

	/// <summary>Gets the number of values in this set. 获取此 Set 的值数量。</summary>
	[Description("@#size")]
	public extern Number Size { get; }

	extern IEnumerator IEnumerable.GetEnumerator();
}

[ECMAScript]
[Description("@#WeakSet")]
/// <summary>Generic host binding for JavaScript <c>WeakSet</c>. JavaScript <c>WeakSet</c> 的泛型宿主绑定。</summary>
/// <remarks>WeakSet is non-enumerable and weakly holds its values. The C# <c>class</c> constraint is only authoring guidance; JavaScript enforces the final <c>CanBeHeldWeakly</c> rule.
/// WeakSet 不可枚举并弱持有其值。C# <c>class</c> 约束仅提供编写指导，JavaScript 负责最终 <c>CanBeHeldWeakly</c> 校验。</remarks>
public sealed class WeakSet<T> where T : class
{
	/// <summary>Creates an empty JavaScript weak set. 创建空的 JavaScript WeakSet。</summary>
	public extern WeakSet();

	/// <summary>
	/// Creates a weak set from a JavaScript iterable of values.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// WeakSet itself is not enumerable in JavaScript, so this host intentionally does not implement <see cref="IEnumerable" />.
	/// The <c>class</c> constraint is only a C# approximation of JavaScript weakly held values;
	/// the runtime still enforces the actual <c>CanBeHeldWeakly</c> rule and may reject values such as strings.
	/// 从 JavaScript iterable 创建 WeakSet；它不可枚举，<c>class</c> 约束只是 C# 近似，运行时仍执行 <c>CanBeHeldWeakly</c> 校验。
	/// </summary>
	public extern WeakSet(IEnumerable<T> values);

	/// <summary>Adds a weakly held value and returns this set. 添加弱持有值并返回当前 WeakSet。</summary>
	[Description("@#add")]
	public extern WeakSet<T> Add(T value);

	/// <summary>Checks whether a weakly held value is present. 检查弱持有值是否存在。</summary>
	[Description("@#has")]
	public extern bool Has(T value);

	/// <summary>Deletes a weakly held value and reports whether it was present. 删除弱持有值并报告其此前是否存在。</summary>
	[Description("@#delete")]
	public extern bool Delete(T value);
}

[ECMAScript]
[Description("@#WeakSet")]
/// <summary>Non-generic host binding for JavaScript <c>WeakSet</c>. JavaScript <c>WeakSet</c> 的非泛型宿主绑定。</summary>
public sealed class WeakSet
{
	/// <summary>
	/// JavaScript <c>WeakSet.prototype</c> object.
	/// The non-generic constructor host carries this member so the runtime shape stays visible in C#.
	/// JavaScript <c>WeakSet.prototype</c> 对象；非泛型构造器宿主直接携带它，使 C# 中仍可见运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static WeakSet Prototype { get; }

	/// <summary>Creates an empty JavaScript weak set. 创建空的 JavaScript WeakSet。</summary>
	public extern WeakSet();

	/// <summary>
	/// Creates a weak set from a JavaScript iterable of values.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// WeakSet itself is not enumerable in JavaScript, so this host intentionally does not implement <see cref="IEnumerable" />.
	/// Values are intentionally not nullable on this host because JavaScript weak-set entries must be weakly held values.
	/// That includes objects and non-global symbols; the runtime performs the final validity check.
	/// 从 JavaScript iterable 创建 WeakSet；它不可枚举。值不可为空且必须可弱持有，最终有效性由 JavaScript 运行时判断。
	/// </summary>
	public extern WeakSet(IEnumerable<object> values);

	/// <summary>Adds a weakly held value and returns this set. 添加弱持有值并返回当前 WeakSet。</summary>
	[Description("@#add")]
	public extern WeakSet Add(object value);

	/// <summary>Checks whether a weakly held value is present. 检查弱持有值是否存在。</summary>
	[Description("@#has")]
	public extern bool Has(object value);

	/// <summary>Deletes a weakly held value and reports whether it was present. 删除弱持有值并报告其此前是否存在。</summary>
	[Description("@#delete")]
	public extern bool Delete(object value);
}
