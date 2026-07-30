namespace ECMAScript;

/// <summary>
/// Projection of JavaScript's <c>FinalizationRegistry</c> constructor host.
/// The surface stays non-generic so the C# host shape remains close to the JavaScript runtime object.
/// </summary>
[ECMAScript]
[Description("@#FinalizationRegistry")]
/// <remarks>
/// FinalizationRegistry 的回调执行时机由 JavaScript garbage collector 决定，不能用于确定性的
/// 资源释放。需要 using/Dispose 语义时，应走 compiler 的显式释放协议。
/// </remarks>
public sealed class FinalizationRegistry
{
	/// <summary>
	/// JavaScript <c>FinalizationRegistry.prototype</c> object.
	/// Exposing this on the constructor host keeps the C# surface close to the runtime host shape.
	/// </summary>
	[Description("@#prototype")]
	public extern static FinalizationRegistry Prototype { get; }

	/// <summary>
	/// Creates a registry whose cleanup callback receives the held value supplied at registration time.
	/// </summary>
	/// <param name="cleanupCallback">JavaScript cleanup callback invoked with the held value after the target becomes collectible.</param>
	public extern FinalizationRegistry(Action<object?> cleanupCallback);

	/// <summary>
	/// Registers a target with a held value.
	/// </summary>
	/// <param name="target">Target value to observe weakly. JavaScript allows objects and non-global symbols here.</param>
	/// <param name="heldValue">Value delivered back to the cleanup callback.</param>
	[Description("@#register")]
	public extern void Register(object target, object? heldValue);

	/// <summary>
	/// Registers a target with a held value and an explicit unregister token.
	/// </summary>
	/// <param name="target">Target value to observe weakly. JavaScript allows objects and non-global symbols here.</param>
	/// <param name="heldValue">Value delivered back to the cleanup callback.</param>
	/// <param name="unregisterToken">Token that can later be passed to <see cref="Unregister"/>. JavaScript uses the same weakly held value rule here.</param>
	[Description("@#register")]
	public extern void Register(object target, object? heldValue, object unregisterToken);

	/// <summary>
	/// Removes registrations associated with the supplied unregister token.
	/// </summary>
	/// <param name="unregisterToken">Token previously supplied to <see cref="Register(object, object?, object)"/>.</param>
	/// <returns><see langword="true"/> when at least one registration was removed.</returns>
	[Description("@#unregister")]
	public extern bool Unregister(object unregisterToken);
}
