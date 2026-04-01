using System.ComponentModel;
using PropertyKey = ECMAScript.Either<string, ECMAScript.Number, ECMAScript.Symbol>;

namespace ECMAScript;

/// <summary>
/// Projection of JavaScript's <c>Proxy</c> constructor.
/// </summary>
/// <typeparam name="TTarget">Static CLR view of the proxied target object.</typeparam>
[ECMAScript]
[Description("@#Proxy")]
public sealed class Proxy<TTarget> where TTarget : class
{
	/// <summary>
	/// Creates a JavaScript proxy for the supplied target and handler.
	/// </summary>
	public extern Proxy(TTarget target, ProxyHandler<TTarget> handler);
}

/// <summary>
/// Bridge type used to declare JavaScript proxy traps.
/// This type itself is not a JavaScript runtime host.
/// </summary>
/// <typeparam name="TTarget">Static CLR view of the proxied target object.</typeparam>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class ProxyHandler<TTarget> where TTarget : class
{
	/// <summary>
	/// Trap for property reads.
	/// </summary>
	[Description("@#get")]
	public extern virtual object? Get(TTarget target, PropertyKey property, object receiver);

	/// <summary>
	/// Trap for property writes.
	/// </summary>
	[Description("@#set")]
	public extern virtual bool Set(TTarget target, PropertyKey property, object? value, object receiver);

	/// <summary>
	/// Trap for the <c>in</c> operator.
	/// </summary>
	[Description("@#has")]
	public extern virtual bool Has(TTarget target, PropertyKey property);

	/// <summary>
	/// Trap for function invocation.
	/// </summary>
	[Description("@#apply")]
	public extern virtual object? Apply(TTarget target, object thisArg, object[] argumentsList);
}
