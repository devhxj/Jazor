namespace ECMAScript;

/// <summary>
/// Projection of JavaScript's <c>FinalizationRegistry</c> constructor host.
/// The surface stays non-generic so the C# host shape remains close to the JavaScript runtime object.
/// JavaScript <c>FinalizationRegistry</c> 构造器宿主投影；表面保持非泛型，使 C# 宿主形状接近 JavaScript 运行时对象。
/// </summary>
[ECMAScript]
[Description("@#FinalizationRegistry")]
/// <remarks>
/// Finalization callback timing is decided by the JavaScript garbage collector and cannot provide deterministic resource release.
/// For <c>using</c>/<c>Dispose</c> semantics, use the compiler explicit-disposal protocol instead.
/// Finalization 回调执行时机由 JavaScript 垃圾回收器决定，不能用于确定性资源释放；
/// 需要 <c>using</c>/<c>Dispose</c> 语义时，应使用编译器显式释放协议。
/// </remarks>
public sealed class FinalizationRegistry
{
	/// <summary>
	/// Gets JavaScript <c>FinalizationRegistry.prototype</c> object.
	/// Exposing this on the constructor host keeps the C# surface close to the runtime host shape.
	/// 获取 JavaScript <c>FinalizationRegistry.prototype</c> 对象；将其公开在构造器宿主上使 C# 表面接近运行时宿主形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static FinalizationRegistry Prototype { get; }

	/// <summary>
	/// Creates a registry whose cleanup callback receives the held value supplied at registration time.
	/// The callback is best-effort and must not be used for required cleanup.
	/// 创建注册表，其清理回调接收注册时提供的 held value；该回调尽力执行，不能用于必须完成的清理。
	/// </summary>
	/// <param name="cleanupCallback">Cleanup callback invoked with the held value after the target becomes collectible. 目标可回收后以 held value 调用的清理回调。</param>
	public extern FinalizationRegistry(Action<object?> cleanupCallback);

	/// <summary>
	/// Registers a target with a held value.
	/// Registering does not keep the target alive.
	/// 使用 held value 注册目标；注册不会保持目标存活。
	/// </summary>
	/// <param name="target">Target value to observe weakly; JavaScript allows objects and non-global symbols. 要弱观察的目标值；JavaScript 允许对象和非全局 Symbol。</param>
	/// <param name="heldValue">Value delivered back to the cleanup callback. 回传给清理回调的值。</param>
	[Description("@#register")]
	public extern void Register(object target, object? heldValue);

	/// <summary>
	/// Registers a target with a held value and an explicit unregister token.
	/// 使用 held value 和显式注销令牌注册目标。
	/// </summary>
	/// <param name="target">Target value to observe weakly; JavaScript allows objects and non-global symbols. 要弱观察的目标值；JavaScript 允许对象和非全局 Symbol。</param>
	/// <param name="heldValue">Value delivered back to the cleanup callback. 回传给清理回调的值。</param>
	/// <param name="unregisterToken">Token later passed to <see cref="Unregister"/>; JavaScript applies the same weakly held value rule. 之后传给 <see cref="Unregister"/> 的令牌；JavaScript 对它采用相同的弱持有规则。</param>
	[Description("@#register")]
	public extern void Register(object target, object? heldValue, object unregisterToken);

	/// <summary>
	/// Removes registrations associated with the supplied unregister token.
	/// 移除与给定注销令牌关联的注册。
	/// </summary>
	/// <param name="unregisterToken">Token previously supplied to <see cref="Register(object, object?, object)"/>. 先前提供给 <see cref="Register(object, object?, object)"/> 的令牌。</param>
	/// <returns><see langword="true"/> when at least one registration was removed. 至少移除一个注册时为 <see langword="true"/>。</returns>
	[Description("@#unregister")]
	public extern bool Unregister(object unregisterToken);
}
