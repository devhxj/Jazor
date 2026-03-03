namespace Jazor.CLR;

/// <summary>
/// System.Collections.Generic.HashSet<T> 类型模块映射规则
///
/// C# HashSet<T> 与 JavaScript Set 的对应关系：
/// - 都表示不重复元素的集合
/// - 大部分方法可以直接映射
///
/// Op 类型选择原则：
/// - Inline: 简单构造
/// - Alias: JS Set 原生方法（如 has、add、delete）
/// - Import: 需要完整实现的复杂逻辑
/// - Discard: 不支持或极少使用
/// </summary>
[ECMAScriptModule("System/Collections/Generic/HashSetModule.js")]
[Jazor(Op.Alias, "System.Collections.Generic.HashSet<T>","Set")]
public static class HashSetModule<T>
{
	/// <summary>
	/// C#: new HashSet<T>()
	/// JS: new Set()
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.HashSet()", "new Set()")]
	public extern static Set<T> _55c044d94c5b0ca8();

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the specified equality comparer for the set type.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEqualityComparer<T>)")]
	public extern static Set<T> _3a131c59650baae9(object comparer);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty, but has reserved space for <paramref name="capacity" /> items and uses the default equality comparer for the set type.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.HashSet(int)")]
	public extern static Set<T> _304904fb5a22f950(Number capacity);

	/// <summary>
	/// C#: new HashSet<T>(collection)
	/// JS: new Set(collection)
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>)", "new Set(@#{0})")]
	public extern static Set<T> _1bd2e054852d9d5f(Array<T> collection);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEqualityComparer<T>)")]
	public extern static Set<T> _fe5bb664d9f9c877(Array<T> collection, object comparer);

	///<summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, and has sufficient capacity to accommodate <paramref name="capacity" /> elements.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.HashSet(int, System.Collections.Generic.IEqualityComparer<T>)")]
	public extern static Set<T> _baf729bee477b2e7(Number capacity, object comparer);

	/// <summary>
	/// C#: set.Clear()
	/// JS: set.clear()
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.HashSet<T>.Clear()", "clear")]
	public extern static void _56d632bf48c92530(Set<T> instance);

	/// <summary>
	/// C#: set.Contains(item)
	/// JS: set.has(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.HashSet<T>.Contains(T)", "has")]
	public extern static bool _32b989c96ea23e8c(Set<T> instance, T item);

	/// <summary>
	/// C#: set.Remove(item)
	/// JS: set.delete(item)
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.HashSet<T>.Remove(T)", "delete")]
	public extern static bool _cfb963650cb3dabd(Set<T> instance, T item);

	/// <summary>
	/// C#: set.Count
	/// JS: set.size
	/// </summary>
	[Jazor(Op.Alias, "System.Collections.Generic.HashSet<T>.Count.get", "size")]
	public extern static Number _4bec0b4d27073edb(Set<T> instance);

	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.Capacity.get")]
	public extern static Number _97c019008a0c8260(Set<T> instance);

	///<summary>Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.HashSet`1" /> using a <typeparamref name="TAlternate" /> instead of a <typeparamref name="T" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.GetAlternateLookup<TAlternate>()")]
	public extern static System.Collections.Generic.HashSet<T>.AlternateLookup<TAlternate> _3ed41a9b4870a040<TAlternate>(Set<T> instance);

	///<summary>Gets an instance of a type that can be used to perform operations on the current <see cref="T:System.Collections.Generic.HashSet`1" /> using a <typeparamref name="TAlternate" /> instead of a <typeparamref name="T" />.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.TryGetAlternateLookup<TAlternate>(out System.Collections.Generic.HashSet<T>.AlternateLookup<TAlternate>)")]
	public extern static Array<object?> _859aac4462f2d063<TAlternate>(Set<T> instance, object lookup);

	///<summary>Returns an enumerator that iterates through a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.GetEnumerator()")]
	public extern static System.Collections.Generic.HashSet<T>.Enumerator _68a59c6ba9ebe57d(Set<T> instance);

	///<summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	[Jazor(Op.Discard ,"virtual System.Collections.Generic.HashSet<T>.GetObjectData(System.Runtime.Serialization.SerializationInfo, System.Runtime.Serialization.StreamingContext)")]
	public extern static void _8f2db3c5ff390af9(Set<T> instance, object info, object context);

	///<summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.</summary>
	[Jazor(Op.Discard ,"virtual System.Collections.Generic.HashSet<T>.OnDeserialization(object)")]
	public extern static void _26975bd136a2f896(Set<T> instance, object? sender);

	/// <summary>
	/// C#: set.Add(item)
	/// JS: set.add(item) - returns the Set, but C# returns bool
	/// </summary>
	[Jazor(Op.Import, "System.Collections.Generic.HashSet<T>.Add(T)")]
	public static bool _e1d2ba750a2788cb(Set<T> instance, T item)
	{
		var size = instance.Size;
		instance.Add(item);
		return instance.Size > size;  // Returns true if item was added (not already present)
	}

	///<summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.TryGetValue(T, out T)")]
	public extern static Array<object?> _20eb460b32c63404(Set<T> instance, object equalValue, object actualValue);

	/// <summary>
	/// C#: set.UnionWith(other)
	/// JS: 遍历 other，添加所有元素
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.UnionWith(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { for (var item of other) set.add(item); })(@#{0}, @#{1})")]
	public extern static void _b2bd5d22aadd44a8(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: set.IntersectWith(other)
	/// JS: 保留同时存在于两个集合中的元素
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.IntersectWith(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { for (var item of set) { var found = false; for (var o of other) { if (o === item) { found = true; break; } } if (!found) set.delete(item); } })(@#{0}, @#{1})")]
	public extern static void _3a6a072035334578(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: set.ExceptWith(other)
	/// JS: 从 instance 中删除 other 中的所有元素
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.ExceptWith(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { for (var item of other) set.delete(item); })(@#{0}, @#{1})")]
	public extern static void _373e2e9ed1fb3f5b(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: set.SymmetricExceptWith(other)
	/// JS: 保留只存在于一个集合中的元素
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.SymmetricExceptWith(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { for (var item of other) { if (set.has(item)) set.delete(item); else set.add(item); } })(@#{0}, @#{1})")]
	public extern static void _a22fe44dc0ae9ad2(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: set.IsSubsetOf(other)
	/// JS: 检查 instance 是否是 other 的子集
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.IsSubsetOf(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { for (var item of set) { var found = false; for (var o of other) { if (o === item) { found = true; break; } } if (!found) return false; } return true; })(@#{0}, @#{1})")]
	public extern static bool _23c8bcfc6b71d2b1(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: set.IsProperSubsetOf(other)
	/// JS: 检查 instance 是否是 other 的真子集
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.IsProperSubsetOf(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { var otherSize = 0; for (var _ of other) otherSize++; if (set.size >= otherSize) return false; for (var item of set) { var found = false; for (var o of other) { if (o === item) { found = true; break; } } if (!found) return false; } return true; })(@#{0}, @#{1})")]
	public extern static bool _fb8566ae66aa9591(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: set.IsSupersetOf(other)
	/// JS: 检查 instance 是否是 other 的超集
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.IsSupersetOf(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { for (var item of other) { if (!set.has(item)) return false; } return true; })(@#{0}, @#{1})")]
	public extern static bool _3be7fbb1d68799fb(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: set.IsProperSupersetOf(other)
	/// JS: 检查 instance 是否是 other 的真超集
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.IsProperSupersetOf(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { var otherSize = 0; for (var _ of other) otherSize++; if (set.size <= otherSize) return false; for (var item of other) { if (!set.has(item)) return false; } return true; })(@#{0}, @#{1})")]
	public extern static bool _cc0cc2d0f5be70db(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: set.Overlaps(other)
	/// JS: 检查是否有交集
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.Overlaps(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { for (var item of other) { if (set.has(item)) return true; } return false; })(@#{0}, @#{1})")]
	public extern static bool _84709aa8ff70a52a(Set<T> instance, Array<T> other);

	/// <summary>
	/// C#: set.SetEquals(other)
	/// JS: 检查两个集合是否相等
	/// </summary>
	[Jazor(Op.Inline, "System.Collections.Generic.HashSet<T>.SetEquals(System.Collections.Generic.IEnumerable<T>)", "((set, other) => { var otherSize = 0; for (var _ of other) otherSize++; if (set.size !== otherSize) return false; for (var item of other) { if (!set.has(item)) return false; } return true; })(@#{0}, @#{1})")]
	public extern static bool _55425d259e5f54ea(Set<T> instance, Array<T> other);

	///<summary>Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.CopyTo(T[])")]
	public extern static void _614185e6ff9ff9fd(Set<T> instance, Array<T> array);

	///<summary>Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.CopyTo(T[], int)")]
	public extern static void _9ac2dfb153a1d53c(Set<T> instance, Array<T> array, Number arrayIndex);

	///<summary>Copies the specified number of elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.CopyTo(T[], int, int)")]
	public extern static void _622a881b75871c97(Set<T> instance, Array<T> array, Number arrayIndex, Number count);

	///<summary>Removes all elements that match the conditions defined by the specified predicate from a <see cref="T:System.Collections.Generic.HashSet`1" /> collection.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.RemoveWhere(System.Predicate<T>)")]
	public extern static Number _112079825eb01119(Set<T> instance, Predicate<T> match);

	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.Comparer.get")]
	public extern static System.Collections.Generic.IEqualityComparer<T> _0c0d81e2205a9cb9(Set<T> instance);

	///<summary>Ensures that this hash set can hold the specified number of elements without any further expansion of its backing storage.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.EnsureCapacity(int)")]
	public extern static Number _b53dcd5d4f0c57d7(Set<T> instance, Number capacity);

	///<summary>Sets the capacity of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to the actual number of elements it contains, rounded up to a nearby, implementation-specific value.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.TrimExcess()")]
	public extern static void _09f9b6aba126decb(Set<T> instance);

	///<summary>Sets the capacity of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to the specified number of entries, rounded up to a nearby, implementation-specific value.</summary>
	[Jazor(Op.Discard ,"System.Collections.Generic.HashSet<T>.TrimExcess(int)")]
	public extern static void _e4dd8faf507013ad(Set<T> instance, Number capacity);

	///<summary>Returns an <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for equality testing of a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
	[Jazor(Op.Discard ,"static System.Collections.Generic.HashSet<T>.CreateSetComparer()")]
	public extern static System.Collections.Generic.IEqualityComparer<System.Collections.Generic.HashSet<T>> _2d028c1bc3e2f479();
}
