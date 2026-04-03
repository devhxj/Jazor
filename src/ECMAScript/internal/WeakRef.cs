namespace ECMAScript;

/// <summary>
/// 对象允许你保留对另一个对象的弱引用，但不会阻止垃圾回收（GC）清理被弱引用的对象。
/// </summary>
[ECMAScript]
[Description("@#WeakRef")]
public sealed class WeakRef
{
	public extern WeakRef(object target);

	/// <summary>
	/// 返回 WeakRef 的目标对象。
	/// 如果该对象已被垃圾收集，底层 JavaScript 会返回 <c>undefined</c>，
	/// 此 C# 投影将该缺失值表示为 <see langword="null" />。
	/// </summary>
	/// <returns></returns>
	[Description("@#deref")]
	public extern object? Deref();
}

/// <summary>
/// 对象允许你保留对另一个对象的弱引用，但不会阻止垃圾回收（GC）清理被弱引用的对象。
/// </summary>
[ECMAScript]
[Description("@#WeakRef")]
public sealed class WeakRef<T> where T : class
{
	public extern WeakRef(T target);

	/// <summary>
	/// 返回 WeakRef 的目标对象。
	/// 如果该对象已被垃圾收集，底层 JavaScript 会返回 <c>undefined</c>，
	/// 此 C# 投影将该缺失值表示为 <see langword="null" />。
	/// </summary>
	/// <returns></returns>
	[Description("@#deref")]
	public extern T? Deref();
}
