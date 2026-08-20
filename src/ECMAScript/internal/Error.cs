namespace ECMAScript;

[ECMAScript]
[Description("@#")]
/// <summary>
/// JavaScript <c>Error</c> constructor options object binding.
/// JavaScript <c>Error</c> 构造器可接受的 options 对象绑定。
/// </summary>
public sealed class ErrorOptions
{
	/// <summary>
	/// Gets or sets the optional value exposed on the created JavaScript error as <c>cause</c>.
	/// 获取或设置创建出的 JavaScript 错误以 <c>cause</c> 公开的可选值。
	/// </summary>
	[Description("@#cause")]
	public object? Cause { get; set; }
}

[ECMAScript]
[Description("@#Error")]
/// <summary>
/// JavaScript <c>Error</c> object and its static construction and inspection APIs host binding.
/// JavaScript <c>Error</c> 对象及其静态构造、判断 API 的宿主绑定。
/// </summary>
/// <remarks>
/// Error runtime branding comes from JavaScript, not CLR <see cref="System.Exception"/> inheritance;
/// exception lowering must preserve the compiler error protocol instead of treating every Error as an ordinary C# exception match.
/// Error 的运行时品牌来自 JavaScript，而非 CLR <see cref="System.Exception"/> 继承关系；异常 lowering
/// 必须遵守编译器错误协议，不能将所有 Error 都作为普通 C# Exception 匹配。
/// </remarks>
public class Error : System.Exception
{
	/// <summary>
	/// Gets the JavaScript <c>Error.prototype</c> object.
	/// This stays on the constructor host so the mapped surface remains recognizable from JavaScript.
	/// 获取 JavaScript <c>Error.prototype</c> 对象；保留在构造器宿主上使映射表面与 JavaScript 保持对应。
	/// </summary>
	[Description("@#prototype")]
	public extern static Error Prototype { get; }

	/// <summary>
	/// Returns whether the supplied value is a JavaScript error object.
	/// This mirrors JavaScript <c>Error.isError</c> and checks runtime error branding rather than CLR inheritance.
	/// 判断给定值是否为 JavaScript 错误对象；映射 <c>Error.isError</c>，检查运行时品牌而非 CLR 继承关系。
	/// </summary>
	[Description("@#isError")]
	public extern static bool IsError(object? arg);

	/// <summary>
	/// Gets the error message exposed by JavaScript <c>Error.prototype.message</c>.
	/// This stays as a mapped host member so reads observe the runtime error object rather than CLR exception text synthesis.
	/// 获取 JavaScript <c>Error.prototype.message</c> 公开的错误消息；读取的是运行时错误对象而非 CLR 合成的异常文本。
	/// </summary>
	[Description("@#message")]
	public new extern string? Message { get; }

	/// <summary>
	/// Gets JavaScript <c>Error.prototype.name</c>.
	/// This remains runtime-backed instead of hard-coded so custom error instances can still expose overridden names.
	/// 获取 JavaScript <c>Error.prototype.name</c>；保持运行时取值，令自定义错误实例仍可公开重写后的名称。
	/// </summary>
	[Description("@#name")]
	public extern virtual string Name { get; }

	/// <summary>
	/// Gets the optional value that caused this error.
	/// JavaScript permits any value, not only another <see cref="Error"/>.
	/// 获取导致此错误的可选值；JavaScript 允许任意值，而不限于另一个 <see cref="Error"/>。
	/// </summary>
	[Description("@#cause")]
	public extern object? Cause { get; }

	/// <summary>
	/// Gets JavaScript stack text when the runtime provides it.
	/// Stack formatting and availability are runtime-dependent rather than an ECMAScript language guarantee.
	/// 获取运行时提供的 JavaScript 堆栈文本；堆栈格式和可用性由运行时决定，并非 ECMAScript 语言保证。
	/// </summary>
	[Description("@#stack")]
	public extern string? Stack { get; }

	/// <summary>
	/// Creates a JavaScript error without an explicit message.
	/// This overload exists because JavaScript allows <c>new Error()</c>.
	/// 创建不带显式消息的 JavaScript 错误；此重载对应 JavaScript 的 <c>new Error()</c>。
	/// </summary>
	public extern Error();

	/// <summary>
	/// Creates a JavaScript error with options but without an explicit message.
	/// This keeps the C# host surface aligned with JavaScript's optional-message constructor shape.
	/// 创建带 options 而不带显式消息的 JavaScript 错误，使 C# 宿主表面与 JavaScript 的可选消息构造形式保持一致。
	/// </summary>
	public extern Error(ErrorOptions? options);

	/// <summary>Creates a JavaScript error with a message. 创建带消息的 JavaScript 错误。</summary>
	public extern Error(string message);

	/// <summary>Creates a JavaScript error with a message and options such as <c>cause</c>。创建带消息和 <c>cause</c> 等 options 的 JavaScript 错误。</summary>
	public extern Error(string message, ErrorOptions? options);

	/// <summary>Returns JavaScript's formatted error text. 返回 JavaScript 格式化后的错误文本。</summary>
	[Description("@#toString")]
	public extern override string ToString();
}

[ECMAScript]
[Description("@#EvalError")]
/// <summary>JavaScript <c>EvalError</c> host binding. JavaScript <c>EvalError</c> 宿主绑定。</summary>
public class EvalError : Error
{
	/// <summary>
	/// JavaScript <c>EvalError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static EvalError Prototype { get; }

	/// <summary>Gets the runtime error name. 获取运行时错误名称。</summary>
	[Description("@#name")]
	public extern override string Name { get; }

	/// <summary>
	/// Creates a JavaScript <c>EvalError</c> without an explicit message.
	/// </summary>
	public extern EvalError();

	/// <summary>
	/// Creates a JavaScript <c>EvalError</c> with options but without an explicit message.
	/// </summary>
	public extern EvalError(ErrorOptions? options);

	/// <summary>Creates an <c>EvalError</c> with a message. 创建带消息的 <c>EvalError</c>。</summary>
	public extern EvalError(string message);

	/// <summary>Creates an <c>EvalError</c> with a message and options. 创建带消息和 options 的 <c>EvalError</c>。</summary>
	public extern EvalError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#RangeError")]
/// <summary>JavaScript <c>RangeError</c> host binding. JavaScript <c>RangeError</c> 宿主绑定。</summary>
public class RangeError : Error
{
	/// <summary>
	/// JavaScript <c>RangeError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static RangeError Prototype { get; }

	/// <summary>Gets the runtime error name. 获取运行时错误名称。</summary>
	[Description("@#name")]
	public extern override string Name { get; }

	/// <summary>
	/// Creates a JavaScript <c>RangeError</c> without an explicit message.
	/// </summary>
	public extern RangeError();

	/// <summary>
	/// Creates a JavaScript <c>RangeError</c> with options but without an explicit message.
	/// </summary>
	public extern RangeError(ErrorOptions? options);

	/// <summary>Creates a <c>RangeError</c> with a message. 创建带消息的 <c>RangeError</c>。</summary>
	public extern RangeError(string message);

	/// <summary>Creates a <c>RangeError</c> with a message and options. 创建带消息和 options 的 <c>RangeError</c>。</summary>
	public extern RangeError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#ReferenceError")]
/// <summary>JavaScript <c>ReferenceError</c> host binding. JavaScript <c>ReferenceError</c> 宿主绑定。</summary>
public class ReferenceError : Error
{
	/// <summary>
	/// JavaScript <c>ReferenceError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static ReferenceError Prototype { get; }

	/// <summary>Gets the runtime error name. 获取运行时错误名称。</summary>
	[Description("@#name")]
	public extern override string Name { get; }

	/// <summary>
	/// Creates a JavaScript <c>ReferenceError</c> without an explicit message.
	/// </summary>
	public extern ReferenceError();

	/// <summary>
	/// Creates a JavaScript <c>ReferenceError</c> with options but without an explicit message.
	/// </summary>
	public extern ReferenceError(ErrorOptions? options);

	/// <summary>Creates a <c>ReferenceError</c> with a message. 创建带消息的 <c>ReferenceError</c>。</summary>
	public extern ReferenceError(string message);

	/// <summary>Creates a <c>ReferenceError</c> with a message and options. 创建带消息和 options 的 <c>ReferenceError</c>。</summary>
	public extern ReferenceError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#SyntaxError")]
/// <summary>JavaScript <c>SyntaxError</c> host binding. JavaScript <c>SyntaxError</c> 宿主绑定。</summary>
public class SyntaxError : Error
{
	/// <summary>
	/// JavaScript <c>SyntaxError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static SyntaxError Prototype { get; }

	/// <summary>Gets the runtime error name. 获取运行时错误名称。</summary>
	[Description("@#name")]
	public extern override string Name { get; }

	/// <summary>
	/// Creates a JavaScript <c>SyntaxError</c> without an explicit message.
	/// </summary>
	public extern SyntaxError();

	/// <summary>
	/// Creates a JavaScript <c>SyntaxError</c> with options but without an explicit message.
	/// </summary>
	public extern SyntaxError(ErrorOptions? options);

	/// <summary>Creates a <c>SyntaxError</c> with a message. 创建带消息的 <c>SyntaxError</c>。</summary>
	public extern SyntaxError(string message);

	/// <summary>Creates a <c>SyntaxError</c> with a message and options. 创建带消息和 options 的 <c>SyntaxError</c>。</summary>
	public extern SyntaxError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#TypeError")]
/// <summary>JavaScript <c>TypeError</c> host binding. JavaScript <c>TypeError</c> 宿主绑定。</summary>
public class TypeError : Error
{
	/// <summary>
	/// JavaScript <c>TypeError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static TypeError Prototype { get; }

	/// <summary>Gets the runtime error name. 获取运行时错误名称。</summary>
	[Description("@#name")]
	public extern override string Name { get; }

	/// <summary>
	/// Creates a JavaScript <c>TypeError</c> without an explicit message.
	/// </summary>
	public extern TypeError();

	/// <summary>
	/// Creates a JavaScript <c>TypeError</c> with options but without an explicit message.
	/// </summary>
	public extern TypeError(ErrorOptions? options);

	/// <summary>Creates a <c>TypeError</c> with a message. 创建带消息的 <c>TypeError</c>。</summary>
	public extern TypeError(string message);

	/// <summary>Creates a <c>TypeError</c> with a message and options. 创建带消息和 options 的 <c>TypeError</c>。</summary>
	public extern TypeError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#URIError")]
/// <summary>JavaScript <c>URIError</c> host binding. JavaScript <c>URIError</c> 宿主绑定。</summary>
public class URIError : Error
{
	/// <summary>
	/// JavaScript <c>URIError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static URIError Prototype { get; }

	/// <summary>Gets the runtime error name. 获取运行时错误名称。</summary>
	[Description("@#name")]
	public extern override string Name { get; }

	/// <summary>
	/// Creates a JavaScript <c>URIError</c> without an explicit message.
	/// </summary>
	public extern URIError();

	/// <summary>
	/// Creates a JavaScript <c>URIError</c> with options but without an explicit message.
	/// </summary>
	public extern URIError(ErrorOptions? options);

	/// <summary>Creates a <c>URIError</c> with a message. 创建带消息的 <c>URIError</c>。</summary>
	public extern URIError(string message);

	/// <summary>Creates a <c>URIError</c> with a message and options. 创建带消息和 options 的 <c>URIError</c>。</summary>
	public extern URIError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#AggregateError")]
/// <summary>JavaScript <c>AggregateError</c> host binding. JavaScript <c>AggregateError</c> 宿主绑定。</summary>
public class AggregateError : Error
{
	/// <summary>
	/// JavaScript <c>AggregateError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static AggregateError Prototype { get; }

	/// <summary>Gets the runtime error name. 获取运行时错误名称。</summary>
	[Description("@#name")]
	public extern override string Name { get; }

	/// <summary>
	/// Gets JavaScript <c>AggregateError.prototype.errors</c> array.
	/// Values are preserved as arbitrary JavaScript values and are not coerced to <see cref="Error"/>.
	/// 获取 JavaScript <c>AggregateError.prototype.errors</c> 数组；值保留为任意 JavaScript 值，不会强制转换为 <see cref="Error"/>。
	/// </summary>
	[Description("@#errors")]
	public extern Array<object?> Errors { get; }

	/// <summary>
	/// Creates an <c>AggregateError</c> from a JavaScript iterable of error values.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for arrays, lists, and read-only list families that map to JavaScript iterables.
	/// 从 JavaScript 错误值 iterable 创建 <c>AggregateError</c>；<see cref="IEnumerable{T}"/> 用作数组、列表和只读列表等映射为 JavaScript iterable 的通用 C# 输入表面。
	/// </summary>
	public extern AggregateError(IEnumerable<object?> errors, string? message = null);

	/// <summary>
	/// Creates an <c>AggregateError</c> from a JavaScript iterable of error values with options but without an explicit message.
	/// This overload exists because JavaScript treats the message as optional.
	/// 从 JavaScript 错误值 iterable 创建带 options 而不带显式消息的 <c>AggregateError</c>；JavaScript 将消息视为可选项。
	/// </summary>
	public extern AggregateError(IEnumerable<object?> errors, ErrorOptions? options);

	/// <summary>
	/// Creates an <c>AggregateError</c> from a JavaScript iterable of error values with a message and options.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for arrays, lists, and read-only list families that map to JavaScript iterables.
	/// 从 JavaScript 错误值 iterable 创建带消息和 options 的 <c>AggregateError</c>；<see cref="IEnumerable{T}"/> 用作映射为 JavaScript iterable 的通用 C# 输入表面。
	/// </summary>
	public extern AggregateError(IEnumerable<object?> errors, string? message, ErrorOptions? options);
}

/// <summary>
/// JavaScript <c>SuppressedError</c> runtime host used by explicit resource management.
/// This error preserves both the new error and the earlier suppressed error value so user code can inspect the full disposal failure chain.
/// 显式资源管理使用的 JavaScript <c>SuppressedError</c> 运行时宿主；它保留新错误和先前被抑制的错误值，以便检查完整的释放失败链。
/// </summary>
[ECMAScript]
[Description("@#SuppressedError")]
public class SuppressedError : Error
{
	/// <summary>
	/// JavaScript <c>SuppressedError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static SuppressedError Prototype { get; }

	/// <summary>Gets the runtime error name. 获取运行时错误名称。</summary>
	[Description("@#name")]
	public extern override string Name { get; }

	/// <summary>
	/// Gets the later error value that replaced the previously active one during disposal.
	/// JavaScript allows any value here, not just <see cref="Error"/>.
	/// 获取释放期间替代先前活动错误的后续错误值；JavaScript 允许任意值，而不限于 <see cref="Error"/>。
	/// </summary>
	[Description("@#error")]
	public extern object? Error { get; }

	/// <summary>
	/// Gets the earlier error value that became suppressed by <see cref="Error"/>.
	/// JavaScript allows any value here, not just <see cref="Error"/>.
	/// 获取被后续 <see cref="Error"/> 抑制的较早错误值；JavaScript 允许任意值，而不限于 <see cref="Error"/>。
	/// </summary>
	[Description("@#suppressed")]
	public extern object? Suppressed { get; }

	/// <summary>
	/// Creates a JavaScript <c>SuppressedError</c> without an explicit message.
	/// 创建不带显式消息的 JavaScript <c>SuppressedError</c>。
	/// </summary>
	public extern SuppressedError(object? error, object? suppressed);

	/// <summary>
	/// Creates a JavaScript <c>SuppressedError</c> with an explicit message.
	/// 创建带显式消息的 JavaScript <c>SuppressedError</c>。
	/// </summary>
	public extern SuppressedError(object? error, object? suppressed, string? message);
}
