using System.Reflection;

using System.Collections.Generic;

namespace ECMAScript;

[ECMAScript]
[Description("@#")]
/// <summary>JavaScript Error 构造器可接受的 options 对象 binding。</summary>
public sealed class ErrorOptions
{
	/// <summary>
	/// Optional value exposed on the created JavaScript error as <c>cause</c>.
	/// </summary>
	[Description("@#cause")]
	public object? Cause { get; set; }
}

[ECMAScript]
[Description("@#Error")]
/// <summary>
/// JavaScript Error 对象及其静态构造/判断 API 的 host binding。
/// </summary>
/// <remarks>
/// Error 的 runtime branding 来自 JavaScript，而不是 CLR Exception 继承关系；异常 lowering
/// 需要遵守 compiler 的错误协议，不能把所有 Error 都当作普通 C# Exception 类型匹配。
/// </remarks>
public class Error : System.Exception
{
	/// <summary>
	/// JavaScript <c>Error.prototype</c> object.
	/// This stays on the constructor host so the mapped surface remains recognizable from JavaScript.
	/// </summary>
	[Description("@#prototype")]
	public extern static Error Prototype { get; }

	/// <summary>
	/// Returns whether the supplied value is a JavaScript error object.
	/// This mirrors JavaScript <c>Error.isError</c> and checks runtime error branding rather than CLR inheritance.
	/// </summary>
	[Description("@#isError")]
	public extern static bool IsError(object? arg);

	/// <summary>
	/// The error message as exposed by JavaScript <c>Error.prototype.message</c>.
	/// This stays as a mapped host member so reads observe the runtime error object rather than CLR exception text synthesis.
	/// </summary>
	[Description("@#message")]
	public new extern string? Message { get; }

	/// <summary>
	/// JavaScript <c>Error.prototype.name</c>.
	/// This remains runtime-backed instead of hard-coded so custom error instances can still expose overridden names.
	/// </summary>
	[Description("@#name")]
	public extern virtual string Name { get; }

	/// <summary>
	/// Optional value that caused this error.
	/// </summary>
	[Description("@#cause")]
	public extern object? Cause { get; }

	/// <summary>
	/// JavaScript stack text when the runtime provides it.
	/// </summary>
	[Description("@#stack")]
	public extern string? Stack { get; }

	/// <summary>
	/// Creates a JavaScript error without an explicit message.
	/// This overload exists because JavaScript allows <c>new Error()</c>.
	/// </summary>
	public extern Error();

	/// <summary>
	/// Creates a JavaScript error with options but without an explicit message.
	/// This keeps the C# host surface aligned with JavaScript's optional-message constructor shape.
	/// </summary>
	public extern Error(ErrorOptions? options);

	public extern Error(string message);

	public extern Error(string message, ErrorOptions? options);

	[Description("@#toString")]
	public extern override string ToString();
}

[ECMAScript]
[Description("@#EvalError")]
public class EvalError : Error
{
	/// <summary>
	/// JavaScript <c>EvalError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static EvalError Prototype { get; }

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

	public extern EvalError(string message);

	public extern EvalError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#RangeError")]
public class RangeError : Error
{
	/// <summary>
	/// JavaScript <c>RangeError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static RangeError Prototype { get; }

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

	public extern RangeError(string message);

	public extern RangeError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#ReferenceError")]
public class ReferenceError : Error
{
	/// <summary>
	/// JavaScript <c>ReferenceError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static ReferenceError Prototype { get; }

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

	public extern ReferenceError(string message);

	public extern ReferenceError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#SyntaxError")]
public class SyntaxError : Error
{
	/// <summary>
	/// JavaScript <c>SyntaxError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static SyntaxError Prototype { get; }

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

	public extern SyntaxError(string message);

	public extern SyntaxError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#TypeError")]
public class TypeError : Error
{
	/// <summary>
	/// JavaScript <c>TypeError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static TypeError Prototype { get; }

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

	public extern TypeError(string message);

	public extern TypeError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#URIError")]
public class URIError : Error
{
	/// <summary>
	/// JavaScript <c>URIError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static URIError Prototype { get; }

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

	public extern URIError(string message);

	public extern URIError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#AggregateError")]
public class AggregateError : Error
{
	/// <summary>
	/// JavaScript <c>AggregateError.prototype</c> object.
	/// This intentionally hides <see cref="Error.Prototype"/> because the runtime constructor has its own prototype object.
	/// </summary>
	[Description("@#prototype")]
	public new extern static AggregateError Prototype { get; }

	[Description("@#name")]
	public extern override string Name { get; }

	/// <summary>
	/// JavaScript <c>AggregateError.prototype.errors</c> array.
	/// </summary>
	[Description("@#errors")]
	public extern Array<object?> Errors { get; }

	/// <summary>
	/// Creates an <c>AggregateError</c> from a JavaScript iterable of error values.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// </summary>
	public extern AggregateError(IEnumerable<object?> errors, string? message = null);

	/// <summary>
	/// Creates an <c>AggregateError</c> from a JavaScript iterable of error values with options but without an explicit message.
	/// This overload exists because JavaScript treats the message as optional.
	/// </summary>
	public extern AggregateError(IEnumerable<object?> errors, ErrorOptions? options);

	/// <summary>
	/// Creates an <c>AggregateError</c> from a JavaScript iterable of error values.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// </summary>
	public extern AggregateError(IEnumerable<object?> errors, string? message, ErrorOptions? options);
}

/// <summary>
/// JavaScript <c>SuppressedError</c> runtime host used by explicit resource management.
/// This error preserves both the new error and the earlier suppressed error value so user code can inspect the full disposal failure chain.
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

	[Description("@#name")]
	public extern override string Name { get; }

	/// <summary>
	/// The later error value that replaced the previously active one during disposal.
	/// JavaScript allows any value here, not just <see cref="Error"/>.
	/// </summary>
	[Description("@#error")]
	public extern object? Error { get; }

	/// <summary>
	/// The earlier error value that became suppressed by <see cref="Error"/>.
	/// JavaScript allows any value here, not just <see cref="Error"/>.
	/// </summary>
	[Description("@#suppressed")]
	public extern object? Suppressed { get; }

	/// <summary>
	/// Creates a JavaScript <c>SuppressedError</c> without an explicit message.
	/// </summary>
	public extern SuppressedError(object? error, object? suppressed);

	/// <summary>
	/// Creates a JavaScript <c>SuppressedError</c> with an explicit message.
	/// </summary>
	public extern SuppressedError(object? error, object? suppressed, string? message);
}
