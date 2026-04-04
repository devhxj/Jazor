namespace ECMAScript;

/// <summary>
/// JavaScript <c>DisposableStack</c> runtime host.
/// This stays as its own host because JavaScript exposes structured disposal through a concrete runtime object rather than a CLR-only helper pattern.
/// </summary>
[ECMAScript]
[Description("@#DisposableStack")]
public sealed class DisposableStack
{
	/// <summary>
	/// JavaScript <c>DisposableStack.prototype</c> object.
	/// Keeping this on the constructor host preserves the recognizable JavaScript runtime shape.
	/// </summary>
	[Description("@#prototype")]
	public extern static DisposableStack Prototype { get; }

	public extern DisposableStack();

	/// <summary>
	/// Returns whether this stack has already been disposed or moved.
	/// </summary>
	[Description("@#disposed")]
	public extern bool Disposed { get; }

	/// <summary>
	/// Registers a disposable value with the stack and returns that same value.
	/// The generic return preserves the caller's CLR static type while still mapping to JavaScript <c>DisposableStack.prototype.use</c>.
	/// </summary>
	[Description("@#use")]
	public extern T Use<T>(T value);

	/// <summary>
	/// Registers a value together with a custom synchronous disposer and returns that same value.
	/// </summary>
	[Description("@#adopt")]
	public extern T Adopt<T>(T value, Action<T> onDispose);

	/// <summary>
	/// Registers a synchronous cleanup callback to run when the stack is disposed.
	/// </summary>
	[Description("@#defer")]
	public extern void Defer(Action onDispose);

	/// <summary>
	/// Transfers all registered resources into a new JavaScript <see cref="DisposableStack"/>.
	/// The current stack becomes unusable after the move, matching JavaScript runtime semantics.
	/// </summary>
	[Description("@#move")]
	public extern DisposableStack Move();

	/// <summary>
	/// Disposes the registered resources in LIFO order.
	/// This mirrors JavaScript <c>DisposableStack.prototype.dispose()</c>.
	/// </summary>
	[Description("@#dispose")]
	public extern void Dispose();
}

/// <summary>
/// JavaScript <c>AsyncDisposableStack</c> runtime host.
/// Callback overloads explicitly distinguish synchronous bridge callbacks from promise-producing callbacks so the C# surface stays close to JavaScript disposal semantics.
/// </summary>
[ECMAScript]
[Description("@#AsyncDisposableStack")]
public sealed class AsyncDisposableStack
{
	/// <summary>
	/// JavaScript <c>AsyncDisposableStack.prototype</c> object.
	/// Keeping this on the constructor host preserves the recognizable JavaScript runtime shape.
	/// </summary>
	[Description("@#prototype")]
	public extern static AsyncDisposableStack Prototype { get; }

	public extern AsyncDisposableStack();

	/// <summary>
	/// Returns whether this stack has already been disposed or moved.
	/// </summary>
	[Description("@#disposed")]
	public extern bool Disposed { get; }

	/// <summary>
	/// Registers a value whose JavaScript runtime object participates in async disposal and returns that same value.
	/// </summary>
	[Description("@#use")]
	public extern T Use<T>(T value);

	/// <summary>
	/// Registers a value together with a synchronous cleanup callback and returns that same value.
	/// JavaScript async disposal also accepts synchronous cleanup functions.
	/// </summary>
	[Description("@#adopt")]
	public extern T Adopt<T>(T value, Action<T> onDisposeAsync);

	/// <summary>
	/// Registers a value together with an asynchronous cleanup callback and returns that same value.
	/// <see cref="IPromise"/> is used as the common host surface for JavaScript promises.
	/// </summary>
	[Description("@#adopt")]
	public extern T Adopt<T>(T value, Func<T, IPromise> onDisposeAsync);

	/// <summary>
	/// Registers a value together with an asynchronous cleanup callback and returns that same value.
	/// <see cref="PromiseResult"/> is included for compiler-lowered async methods that do not surface an explicit promise object.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#adopt")]
	public extern T Adopt<T>(T value, Func<T, PromiseResult> onDisposeAsync);

	/// <summary>
	/// Registers a synchronous cleanup callback to run when the stack is asynchronously disposed.
	/// </summary>
	[Description("@#defer")]
	public extern void Defer(Action onDisposeAsync);

	/// <summary>
	/// Registers an asynchronous cleanup callback to run when the stack is asynchronously disposed.
	/// </summary>
	[Description("@#defer")]
	public extern void Defer(Func<IPromise> onDisposeAsync);

	/// <summary>
	/// Registers an asynchronous cleanup callback to run when the stack is asynchronously disposed.
	/// <see cref="PromiseResult"/> is included for compiler-lowered async methods that do not surface an explicit promise object.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#defer")]
	public extern void Defer(Func<PromiseResult> onDisposeAsync);

	/// <summary>
	/// Transfers all registered resources into a new JavaScript <see cref="AsyncDisposableStack"/>.
	/// The current stack becomes unusable after the move, matching JavaScript runtime semantics.
	/// </summary>
	[Description("@#move")]
	public extern AsyncDisposableStack Move();

	/// <summary>
	/// Asynchronously disposes the registered resources in LIFO order.
	/// <see cref="IPromise"/> is used because JavaScript resolves this operation through a promise.
	/// </summary>
	[Description("@#disposeAsync")]
	public extern IPromise DisposeAsync();
}
