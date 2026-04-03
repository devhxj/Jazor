using System.Reflection;

using System.Collections.Generic;

namespace ECMAScript;

[ECMAScript]
[Description("@#")]
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
public class Error : Exception
{
	/// <summary>
	/// The error message as exposed by JavaScript <c>Error.prototype.message</c>.
	/// </summary>
	[Description("@#message")]
	public new string? Message => base.Message;

	[Description("@#name")]
	public virtual string Name => "Error";

	/// <summary>
	/// Optional value that caused this error.
	/// </summary>
	[Description("@#cause")]
	public object? Cause { get; }

	[Description("@#stack")]
	public string? Stack => StackTrace;

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
	[Description("@#name")]
	public override string Name => "EvalError";

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
	[Description("@#name")]
	public override string Name => "RangeError";

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
	[Description("@#name")]
	public override string Name => "ReferenceError";

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
	[Description("@#name")]
	public override string Name => "SyntaxError";

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
	[Description("@#name")]
	public override string Name => "TypeError";

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
	[Description("@#name")]
	public override string Name => "URIError";

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
	[Description("@#name")]
	public override string Name => "AggregateError";

	/// <summary>
	/// JavaScript <c>AggregateError.prototype.errors</c> array.
	/// </summary>
	[Description("@#errors")]
	public Array<object?> Errors { get; }

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
	[Description("@#name")]
	public override string Name => "SuppressedError";

	/// <summary>
	/// The later error value that replaced the previously active one during disposal.
	/// JavaScript allows any value here, not just <see cref="Error"/>.
	/// </summary>
	[Description("@#error")]
	public object? Error { get; }

	/// <summary>
	/// The earlier error value that became suppressed by <see cref="Error"/>.
	/// JavaScript allows any value here, not just <see cref="Error"/>.
	/// </summary>
	[Description("@#suppressed")]
	public object? Suppressed { get; }

	/// <summary>
	/// Creates a JavaScript <c>SuppressedError</c> without an explicit message.
	/// </summary>
	public extern SuppressedError(object? error, object? suppressed);

	/// <summary>
	/// Creates a JavaScript <c>SuppressedError</c> with an explicit message.
	/// </summary>
	public extern SuppressedError(object? error, object? suppressed, string? message);
}
