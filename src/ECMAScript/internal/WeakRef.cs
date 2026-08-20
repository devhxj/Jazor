namespace ECMAScript;

/// <summary>
/// JavaScript <c>WeakRef</c> lets you keep a weak reference to a weakly held value without preventing garbage collection.
/// JavaScript allows objects and non-global symbols here; the runtime enforces that rule.
/// JavaScript <c>WeakRef</c> 可弱引用一个值而不阻止垃圾回收；JavaScript 允许对象和非全局 Symbol，运行时负责强制该规则。
/// </summary>
[ECMAScript]
[Description("@#WeakRef")]
/// <remarks>
/// Collection timing cannot be controlled by this C# binding. A <see langword="null"/> <c>Deref</c> result only projects JavaScript <c>undefined</c> as an absent value;
/// it must not be used to infer a target's exact lifecycle.
/// 回收时机不能由此 C# binding 控制；<c>Deref</c> 返回 <see langword="null"/> 仅表示 JavaScript <c>undefined</c>
/// 在投影中的缺失值，不可据此推断目标的精确生命周期。
/// </remarks>
public sealed class WeakRef
{
	/// <summary>
	/// Gets JavaScript <c>WeakRef.prototype</c> object.
	/// This stays on the constructor host to preserve the runtime host boundary in the public API.
	/// 获取 JavaScript <c>WeakRef.prototype</c> 对象；保留在构造器宿主上以维持公开 API 的运行时宿主边界。
	/// </summary>
	[Description("@#prototype")]
	public extern static WeakRef Prototype { get; }

	/// <summary>Creates a weak reference to <paramref name="target"/>. 创建指向 <paramref name="target"/> 的弱引用。</summary>
	public extern WeakRef(object target);

	/// <summary>
	/// Returns the weak-reference target when it remains available.
	/// JavaScript returns <c>undefined</c> after collection; this C# projection represents that absent value as <see langword="null"/>.
	/// 当弱引用目标仍可用时返回它；JavaScript 在回收后返回 <c>undefined</c>，此 C# 投影将该缺失值表示为 <see langword="null"/>。
	/// </summary>
	/// <returns>The target, or <see langword="null"/> when unavailable. 目标；不可用时为 <see langword="null"/>。</returns>
	[Description("@#deref")]
	public extern object? Deref();
}

/// <summary>
/// Generic projection of JavaScript <c>WeakRef</c>.
/// The <c>class</c> constraint is only a C# approximation of JavaScript weakly held values; the runtime still performs the final validity check.
/// JavaScript <c>WeakRef</c> 的泛型投影；<c>class</c> 约束仅近似表示 JavaScript 可弱持有值，最终有效性仍由运行时检查。
/// </summary>
[ECMAScript]
[Description("@#WeakRef")]
public sealed class WeakRef<T> where T : class
{
	/// <summary>Creates a typed weak reference to <paramref name="target"/>. 创建指向 <paramref name="target"/> 的强类型弱引用。</summary>
	public extern WeakRef(T target);

	/// <summary>
	/// Returns the weak-reference target when it remains available.
	/// JavaScript returns <c>undefined</c> after collection; this C# projection represents that absent value as <see langword="null"/>.
	/// 当弱引用目标仍可用时返回它；JavaScript 在回收后返回 <c>undefined</c>，此 C# 投影将该缺失值表示为 <see langword="null"/>。
	/// </summary>
	/// <returns>The target, or <see langword="null"/> when unavailable. 目标；不可用时为 <see langword="null"/>。</returns>
	[Description("@#deref")]
	public extern T? Deref();
}
