namespace ECMAScript;

/// <summary>
/// JavaScript <c>DisposableStack</c> runtime host.
/// This stays as its own host because JavaScript exposes structured disposal through a concrete runtime object rather than a CLR-only helper pattern.
/// JavaScript <c>DisposableStack</c> 运行时宿主；JavaScript 通过具体运行时对象公开结构化释放，因此不应替换为仅 CLR 的辅助模式。
/// </summary>
[ECMAScript]
[Description("@#DisposableStack")]
/// <remarks>
/// <c>DisposableStack</c> is a host binding for JavaScript explicit resource management; it is not the compiler expansion of CLR <c>using</c>.
/// Disposal and exception propagation must follow this host protocol instead of being replaced with ordinary array operations.
/// <c>DisposableStack</c> 是 JavaScript 显式资源管理协议的宿主绑定，不等同于 CLR <c>using</c> 的编译器展开；
/// 释放和异常传播必须遵循此宿主协议，不能自行替换为普通数组操作。
/// </remarks>
public sealed class DisposableStack
{
	/// <summary>
/// Gets JavaScript <c>DisposableStack.prototype</c> object.
/// Keeping this on the constructor host preserves the recognizable JavaScript runtime shape.
/// 获取 JavaScript <c>DisposableStack.prototype</c> 对象；保留在构造器宿主上可维持可辨识的 JavaScript 运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static DisposableStack Prototype { get; }

	/// <summary>Creates an empty JavaScript disposal stack. 创建空的 JavaScript 释放栈。</summary>
	public extern DisposableStack();

	/// <summary>
/// Gets whether this stack has already been disposed or moved.
/// 获取此栈是否已被释放或移动。
	/// </summary>
	[Description("@#disposed")]
	public extern bool Disposed { get; }

	/// <summary>
/// Registers a disposable value with the stack and returns that same value.
/// The generic return preserves the caller's CLR static type while still mapping to JavaScript <c>DisposableStack.prototype.use</c>.
/// 向栈注册可释放值并返回该值；泛型返回保留调用方 CLR 静态类型，同时映射到 JavaScript <c>DisposableStack.prototype.use</c>。
	/// </summary>
	[Description("@#use")]
	public extern T Use<T>(T value);

	/// <summary>
/// Registers a value together with a custom synchronous disposer and returns that same value.
/// 向栈注册值及其自定义同步释放器，并返回该值。
	/// </summary>
	[Description("@#adopt")]
	public extern T Adopt<T>(T value, Action<T> onDispose);

	/// <summary>
/// Registers a synchronous cleanup callback to run when the stack is disposed.
/// 注册在栈释放时执行的同步清理回调。
	/// </summary>
	[Description("@#defer")]
	public extern void Defer(Action onDispose);

	/// <summary>
/// Transfers all registered resources into a new JavaScript <see cref="DisposableStack"/>.
/// The current stack becomes unusable after the move, matching JavaScript runtime semantics.
/// 将所有已注册资源转移到新的 JavaScript <see cref="DisposableStack"/>；移动后当前栈不可再用，符合 JavaScript 运行时语义。
	/// </summary>
	[Description("@#move")]
	public extern DisposableStack Move();

	/// <summary>
/// Disposes the registered resources in LIFO order.
/// This mirrors JavaScript <c>DisposableStack.prototype.dispose()</c>.
/// 以后进先出顺序释放已注册资源；映射 JavaScript <c>DisposableStack.prototype.dispose()</c>。
	/// </summary>
	[Description("@#dispose")]
	public extern void Dispose();
}

/// <summary>
/// JavaScript <c>AsyncDisposableStack</c> runtime host.
/// Callback overloads explicitly distinguish synchronous bridge callbacks from promise-producing callbacks so the C# surface stays close to JavaScript disposal semantics.
/// JavaScript <c>AsyncDisposableStack</c> 运行时宿主；回调重载明确区分同步桥接回调和产生 Promise 的回调，以保持 C# 表面贴近 JavaScript 释放语义。
/// </summary>
[ECMAScript]
[Description("@#AsyncDisposableStack")]
/// <remarks>
/// <c>AsyncDisposableStack</c> preserves JavaScript asynchronous disposal order and promise-propagation rules.
/// <c>AsyncDisposableStack</c> 保留 JavaScript 异步释放顺序和 Promise 传播规则。
/// </remarks>
public sealed class AsyncDisposableStack
{
	/// <summary>
	/// Gets JavaScript <c>AsyncDisposableStack.prototype</c> object.
	/// Keeping this on the constructor host preserves the recognizable JavaScript runtime shape.
	/// 获取 JavaScript <c>AsyncDisposableStack.prototype</c> 对象；保留在构造器宿主上可维持可辨识的 JavaScript 运行时形状。
	/// </summary>
	[Description("@#prototype")]
	public extern static AsyncDisposableStack Prototype { get; }

	/// <summary>Creates an empty JavaScript asynchronous disposal stack. 创建空的 JavaScript 异步释放栈。</summary>
	public extern AsyncDisposableStack();

	/// <summary>
	/// Gets whether this stack has already been disposed or moved.
	/// 获取此栈是否已被释放或移动。
	/// </summary>
	[Description("@#disposed")]
	public extern bool Disposed { get; }

	/// <summary>
	/// Registers a value whose JavaScript runtime object participates in async disposal and returns that same value.
	/// 向栈注册参与异步释放的 JavaScript 运行时对象值，并返回该值。
	/// </summary>
	[Description("@#use")]
	public extern T Use<T>(T value);

	/// <summary>
	/// Registers a value together with a synchronous cleanup callback and returns that same value.
	/// JavaScript async disposal also accepts synchronous cleanup functions.
	/// 向栈注册值及同步清理回调并返回该值；JavaScript 异步释放同样接受同步清理函数。
	/// </summary>
	[Description("@#adopt")]
	public extern T Adopt<T>(T value, Action<T> onDisposeAsync);

	/// <summary>
	/// Registers a value together with an asynchronous cleanup callback and returns that same value.
	/// <see cref="IPromise"/> is used as the common host surface for JavaScript promises.
	/// 向栈注册值及异步清理回调并返回该值；<see cref="IPromise"/> 作为 JavaScript Promise 的通用宿主表面。
	/// </summary>
	[Description("@#adopt")]
	public extern T Adopt<T>(T value, Func<T, IPromise> onDisposeAsync);

	/// <summary>
	/// Registers a value together with an asynchronous cleanup callback and returns that same value.
	/// <see cref="PromiseResult"/> is included for compiler-lowered async methods that do not surface an explicit promise object.
	/// 向栈注册值及异步清理回调并返回该值；<see cref="PromiseResult"/> 用于编译器 lowering 后未公开显式 Promise 对象的异步方法。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#adopt")]
	public extern T Adopt<T>(T value, Func<T, PromiseResult> onDisposeAsync);

	/// <summary>
	/// Registers a synchronous cleanup callback to run when the stack is asynchronously disposed.
	/// 注册在栈异步释放时执行的同步清理回调。
	/// </summary>
	[Description("@#defer")]
	public extern void Defer(Action onDisposeAsync);

	/// <summary>
	/// Registers an asynchronous cleanup callback to run when the stack is asynchronously disposed.
	/// 注册在栈异步释放时执行的异步清理回调。
	/// </summary>
	[Description("@#defer")]
	public extern void Defer(Func<IPromise> onDisposeAsync);

	/// <summary>
	/// Registers an asynchronous cleanup callback to run when the stack is asynchronously disposed.
	/// <see cref="PromiseResult"/> is included for compiler-lowered async methods that do not surface an explicit promise object.
	/// 注册在栈异步释放时执行的异步清理回调；<see cref="PromiseResult"/> 用于未公开显式 Promise 对象的编译器 lowering 异步方法。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#defer")]
	public extern void Defer(Func<PromiseResult> onDisposeAsync);

	/// <summary>
	/// Transfers all registered resources into a new JavaScript <see cref="AsyncDisposableStack"/>.
	/// The current stack becomes unusable after the move, matching JavaScript runtime semantics.
	/// 将所有已注册资源转移到新的 JavaScript <see cref="AsyncDisposableStack"/>；移动后当前栈不可再用，符合 JavaScript 运行时语义。
	/// </summary>
	[Description("@#move")]
	public extern AsyncDisposableStack Move();

	/// <summary>
	/// Asynchronously disposes the registered resources in LIFO order.
	/// <see cref="IPromise"/> is used because JavaScript resolves this operation through a promise.
	/// 以后进先出顺序异步释放已注册资源；使用 <see cref="IPromise"/> 是因为 JavaScript 通过 Promise 解析此操作。
	/// </summary>
	[Description("@#disposeAsync")]
	public extern IPromise DisposeAsync();
}
