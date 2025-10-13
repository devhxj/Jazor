using System.Reflection;

namespace ECMAScript;

[ECMAScript]
[Description("@#Error")]
public class Error : Exception
{
	public virtual string Name => "Error";

	public string? Stack => StackTrace;

	public extern Error(string message, string? stack = null);

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
	public override string Name => "EvalError";

	public extern EvalError(string message, string? stack = null);
}

[ECMAScript]
[Description("@#RangeError")]
public class RangeError : Error
{
	public override string Name => "RangeError";

	public extern RangeError(string message, string? stack = null);
}

[ECMAScript]
[Description("@#ReferenceError")]
public class ReferenceError : Error
{
	public override string Name => "ReferenceError";

	public extern ReferenceError(string message, string? stack = null);
}

[ECMAScript]
[Description("@#SyntaxError")]
public class SyntaxError : Error
{
	public override string Name => "SyntaxError";

	public extern SyntaxError(string message, string? stack = null);
}

[ECMAScript]
[Description("@#TypeError")]
public class TypeError : Error
{
	public override string Name => "TypeError";

	public extern TypeError(string message, string? stack = null);
}

[ECMAScript]
[Description("@#URIError")]
public class URIError : Error
{
	public override string Name => "URIError";

	public extern URIError(string message, string? stack = null);
}

[ECMAScript]
[Description("@#AggregateError")]
public class AggregateError : Error
{
	public override string Name => "AggregateError";

	public IReadOnlyList<Exception> Errors { get; }

	public extern AggregateError(IReadOnlyList<Exception> errors, string? message = null, string? stack = null);
}
