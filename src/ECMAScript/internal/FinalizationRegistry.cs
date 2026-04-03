namespace ECMAScript;

/// <summary>
/// Projection of JavaScript's <c>FinalizationRegistry</c> constructor host.
/// The surface stays non-generic so the C# host shape remains close to the JavaScript runtime object.
/// </summary>
[ECMAScript]
[Description("@#FinalizationRegistry")]
public sealed class FinalizationRegistry
{
	/// <summary>
	/// Creates a registry whose cleanup callback receives the held value supplied at registration time.
	/// </summary>
	/// <param name="cleanupCallback">JavaScript cleanup callback invoked with the held value after the target becomes collectible.</param>
	public extern FinalizationRegistry(Action<object?> cleanupCallback);

	/// <summary>
	/// Registers a target with a held value.
	/// </summary>
	/// <param name="target">Target object to observe weakly.</param>
	/// <param name="heldValue">Value delivered back to the cleanup callback.</param>
	[Description("@#register")]
	public extern void Register(object target, object? heldValue);

	/// <summary>
	/// Registers a target with a held value and an explicit unregister token.
	/// </summary>
	/// <param name="target">Target object to observe weakly.</param>
	/// <param name="heldValue">Value delivered back to the cleanup callback.</param>
	/// <param name="unregisterToken">Token that can later be passed to <see cref="Unregister"/>.</param>
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
