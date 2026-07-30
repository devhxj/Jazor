namespace ECMAScript;

/// <summary>
/// JavaScript <c>WeakRef</c> lets you keep a weak reference to a weakly held value without preventing garbage collection.
/// JavaScript allows objects and non-global symbols here; the runtime enforces that rule.
/// </summary>
[ECMAScript]
[Description("@#WeakRef")]
/// <remarks>
/// WeakRef 的回收时机不可由 C# binding 控制；Deref 返回 null 只表示 JavaScript 的 undefined
/// 在投影中的缺失值，不代表目标一定已经回收或可以安全做生命周期推断。
/// </remarks>
public sealed class WeakRef
{
	/// <summary>
	/// JavaScript <c>WeakRef.prototype</c> object.
	/// This stays on the constructor host to preserve the runtime host boundary in the public API.
	/// </summary>
	[Description("@#prototype")]
	public extern static WeakRef Prototype { get; }

	public extern WeakRef(object target);

	/// <summary>
	/// 返回 WeakRef 的目标值。
	/// 如果该对象已被垃圾收集，底层 JavaScript 会返回 <c>undefined</c>，
	/// 此 C# 投影将该缺失值表示为 <see langword="null" />。
	/// </summary>
	/// <returns></returns>
	[Description("@#deref")]
	public extern object? Deref();
}

/// <summary>
/// Generic projection of JavaScript <c>WeakRef</c>.
/// The <c>class</c> constraint is only a C# approximation of JavaScript weakly held values; the runtime still performs the final validity check.
/// </summary>
[ECMAScript]
[Description("@#WeakRef")]
public sealed class WeakRef<T> where T : class
{
	public extern WeakRef(T target);

	/// <summary>
	/// 返回 WeakRef 的目标值。
	/// 如果该对象已被垃圾收集，底层 JavaScript 会返回 <c>undefined</c>，
	/// 此 C# 投影将该缺失值表示为 <see langword="null" />。
	/// </summary>
	/// <returns></returns>
	[Description("@#deref")]
	public extern T? Deref();
}
