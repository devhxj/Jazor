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

	public extern EvalError(string message);

	public extern EvalError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#RangeError")]
public class RangeError : Error
{
	[Description("@#name")]
	public override string Name => "RangeError";

	public extern RangeError(string message);

	public extern RangeError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#ReferenceError")]
public class ReferenceError : Error
{
	[Description("@#name")]
	public override string Name => "ReferenceError";

	public extern ReferenceError(string message);

	public extern ReferenceError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#SyntaxError")]
public class SyntaxError : Error
{
	[Description("@#name")]
	public override string Name => "SyntaxError";

	public extern SyntaxError(string message);

	public extern SyntaxError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#TypeError")]
public class TypeError : Error
{
	[Description("@#name")]
	public override string Name => "TypeError";

	public extern TypeError(string message);

	public extern TypeError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#URIError")]
public class URIError : Error
{
	[Description("@#name")]
	public override string Name => "URIError";

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
	/// Creates an <c>AggregateError</c> from a JavaScript iterable of error values.
	/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
	/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
	/// </summary>
	public extern AggregateError(IEnumerable<object?> errors, string? message, ErrorOptions? options);
}
