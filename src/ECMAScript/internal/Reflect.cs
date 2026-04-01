using PropertyKey = ECMAScript.Either<string, ECMAScript.Number, ECMAScript.Symbol>;

namespace ECMAScript;

/// <summary>
/// Projection of JavaScript's <c>Reflect</c> object.
/// The surface here keeps JavaScript property-key semantics instead of collapsing them to <see cref="string"/>.
/// </summary>
[ECMAScript]
[Description("@#Reflect")]
public static class Reflect
{
	/// <summary>
	/// Invokes a target function with the supplied <c>this</c> value and argument list.
	/// </summary>
	[Description("@#apply")]
	public extern static object? Apply(object target, object thisArg, object[] argumentsList);

	/// <summary>
	/// Invokes a target as a constructor.
	/// </summary>
	[Description("@#construct")]
	public extern static object Construct(object target, object[] argumentsList);

	/// <summary>
	/// Invokes a target as a constructor with an explicit <c>newTarget</c>.
	/// </summary>
	[Description("@#construct")]
	public extern static object Construct(object target, object[] argumentsList, object newTarget);

	/// <summary>
	/// Reads a property from the target.
	/// </summary>
	[Description("@#get")]
	public extern static object? Get(object target, PropertyKey propertyKey);

	/// <summary>
	/// Reads a property from the target using an explicit receiver.
	/// </summary>
	[Description("@#get")]
	public extern static object? Get(object target, PropertyKey propertyKey, object receiver);

	/// <summary>
	/// Sets a property on the target.
	/// </summary>
	[Description("@#set")]
	public extern static bool Set(object target, PropertyKey propertyKey, object? value);

	/// <summary>
	/// Sets a property on the target using an explicit receiver.
	/// </summary>
	[Description("@#set")]
	public extern static bool Set(object target, PropertyKey propertyKey, object? value, object receiver);

	/// <summary>
	/// Returns whether the property exists on the target or its prototype chain.
	/// </summary>
	[Description("@#has")]
	public extern static bool Has(object target, PropertyKey propertyKey);

	/// <summary>
	/// Deletes an own property from the target.
	/// </summary>
	[Description("@#deleteProperty")]
	public extern static bool DeleteProperty(object target, PropertyKey propertyKey);

	/// <summary>
	/// Returns all own property keys of the target, including symbols.
	/// </summary>
	[Description("@#ownKeys")]
	public extern static Array<PropertyKey> OwnKeys(object target);

	/// <summary>
	/// Returns the own property descriptor for the specified property key.
	/// </summary>
	[Description("@#getOwnPropertyDescriptor")]
	public extern static PropertyDescriptor? GetOwnPropertyDescriptor(object target, PropertyKey propertyKey);
}
