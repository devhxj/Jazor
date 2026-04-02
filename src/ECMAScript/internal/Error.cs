using System.Reflection;

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

	public extern Error(string message, string? stack = null);

	public extern Error(string message, ErrorOptions? options);

	[Description("@#toString")]
	public extern override string ToString();

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override IDictionary Data { get; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override Exception GetBaseException();

	//[EditorBrowsable(EditorBrowsableState.Never)]
	//[Obsolete]
	//public extern override void GetObjectData(SerializationInfo info, StreamingContext context);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override string? HelpLink { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override string? Source { get; set; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override string StackTrace { get; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern new Exception InnerException { get; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern new MethodBase TargetSite { get; }

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern new Type GetType();

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override int GetHashCode();

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override bool Equals(object? obj);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern static new bool Equals(object objA, object objB);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern static new bool ReferenceEquals(object objA, object objB);
}

[ECMAScript]
[Description("@#EvalError")]
public class EvalError : Error
{
	[Description("@#name")]
	public override string Name => "EvalError";

	public extern EvalError(string message);

	public extern EvalError(string message, string? stack = null);

	public extern EvalError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#RangeError")]
public class RangeError : Error
{
	[Description("@#name")]
	public override string Name => "RangeError";

	public extern RangeError(string message);

	public extern RangeError(string message, string? stack = null);

	public extern RangeError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#ReferenceError")]
public class ReferenceError : Error
{
	[Description("@#name")]
	public override string Name => "ReferenceError";

	public extern ReferenceError(string message);

	public extern ReferenceError(string message, string? stack = null);

	public extern ReferenceError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#SyntaxError")]
public class SyntaxError : Error
{
	[Description("@#name")]
	public override string Name => "SyntaxError";

	public extern SyntaxError(string message);

	public extern SyntaxError(string message, string? stack = null);

	public extern SyntaxError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#TypeError")]
public class TypeError : Error
{
	[Description("@#name")]
	public override string Name => "TypeError";

	public extern TypeError(string message);

	public extern TypeError(string message, string? stack = null);

	public extern TypeError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#URIError")]
public class URIError : Error
{
	[Description("@#name")]
	public override string Name => "URIError";

	public extern URIError(string message);

	public extern URIError(string message, string? stack = null);

	public extern URIError(string message, ErrorOptions? options);
}

[ECMAScript]
[Description("@#AggregateError")]
public class AggregateError : Error
{
	[Description("@#name")]
	public override string Name => "AggregateError";

	[Description("@#errors")]
	public IReadOnlyList<object?> Errors { get; }

	public extern AggregateError(IReadOnlyList<object?> errors, string? message = null, string? stack = null);

	public extern AggregateError(IReadOnlyList<object?> errors, string? message, ErrorOptions? options);
}
