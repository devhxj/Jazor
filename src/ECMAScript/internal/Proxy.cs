using System.ComponentModel;
using PropertyKey = ECMAScript.Either<string, ECMAScript.Number, ECMAScript.Symbol>;

namespace ECMAScript;

/// <summary>
/// JavaScript object shape returned by <c>Proxy.revocable()</c>.
/// This stays explicit because JavaScript returns a record-like object containing both the proxy and its paired revoke callback.
/// </summary>
/// <typeparam name="TTarget">Static CLR view of the proxied target object.</typeparam>
[ECMAScript]
[Description("@#")]
public sealed class RevocableProxy<TTarget> where TTarget : class
{
	/// <summary>
	/// The revocable JavaScript proxy instance.
	/// </summary>
	[Description("@#proxy")]
	public extern Proxy<TTarget> Proxy { get; }

	/// <summary>
	/// Revokes the proxy so future operations fail according to JavaScript proxy semantics.
	/// </summary>
	[Description("@#revoke")]
	public extern Action Revoke { get; }
}

/// <summary>
/// Static JavaScript <c>Proxy</c> host members.
/// This stays separate from <see cref="Proxy{TTarget}"/> so the runtime static API can be modeled without inventing a CLR-only wrapper type.
/// </summary>
[ECMAScript]
[Description("@#Proxy")]
public static class Proxy
{
	/// <summary>
	/// Creates a revocable JavaScript proxy together with its paired revoke callback.
	/// This models JavaScript <c>Proxy.revocable(target, handler)</c> directly on the <c>Proxy</c> host.
	/// </summary>
	[Description("@#revocable")]
	public extern static RevocableProxy<TTarget> Revocable<TTarget>(TTarget target, ProxyHandler<TTarget> handler) where TTarget : class;
}

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
	/// Trap for deleting an own property.
	/// </summary>
	[Description("@#deleteProperty")]
	public extern virtual bool DeleteProperty(TTarget target, PropertyKey property);

	/// <summary>
	/// Trap for defining or reconfiguring an own property.
	/// </summary>
	[Description("@#defineProperty")]
	public extern virtual bool DefineProperty(TTarget target, PropertyKey property, PropertyDescriptor attributes);

	/// <summary>
	/// Trap for reading an own property descriptor.
	/// </summary>
	[Description("@#getOwnPropertyDescriptor")]
	public extern virtual PropertyDescriptor? GetOwnPropertyDescriptor(TTarget target, PropertyKey property);

	/// <summary>
	/// Trap for enumerating own property keys, including symbols.
	/// </summary>
	[Description("@#ownKeys")]
	public extern virtual Array<PropertyKey> OwnKeys(TTarget target);

	/// <summary>
	/// Trap for reading the proxy target prototype.
	/// </summary>
	[Description("@#getPrototypeOf")]
	public extern virtual IObject? GetPrototypeOf(TTarget target);

	/// <summary>
	/// Trap for updating the proxy target prototype.
	/// </summary>
	[Description("@#setPrototypeOf")]
	public extern virtual bool SetPrototypeOf(TTarget target, object? prototype);

	/// <summary>
	/// Trap for checking whether the target remains extensible.
	/// </summary>
	[Description("@#isExtensible")]
	public extern virtual bool IsExtensible(TTarget target);

	/// <summary>
	/// Trap for preventing extensions on the target.
	/// </summary>
	[Description("@#preventExtensions")]
	public extern virtual bool PreventExtensions(TTarget target);

	/// <summary>
	/// Trap for the <c>in</c> operator.
	/// </summary>
	[Description("@#has")]
	public extern virtual bool Has(TTarget target, PropertyKey property);

	/// <summary>
	/// Trap for function invocation.
	/// The receiver and argument list stay nullable because JavaScript call sites may supply any runtime values there.
	/// </summary>
	[Description("@#apply")]
	public extern virtual object? Apply(TTarget target, object? thisArg, object?[] argumentsList);

	/// <summary>
	/// Trap for constructor invocation with <c>new</c>.
	/// The argument list stays nullable because JavaScript constructor calls may supply any runtime values there.
	/// </summary>
	[Description("@#construct")]
	public extern virtual object? Construct(TTarget target, object?[] argumentsList, object newTarget);
}
