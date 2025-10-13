using PropertyKey = ECMAScript.Either<string, ECMAScript.Number, ECMAScript.Symbol>;

namespace ECMAScript;

[ECMAScript]
public interface IReadOnly<T>
{
	T Value { get; }
}

[ECMAScript]
[Description("@#Object")]
public interface IObject
{
	IObject? this[string key] { get; }

	IObject? this[uint index] { get; }
}

[ECMAScript]
[Description("@#Object")]
public interface IObject<TPrototype> : IObject
{
	/// <summary>
	/// Returns the primitive value of the specified object.
	/// </summary>
	/// <returns></returns>
	[Description("@#valueOf")]
	public extern TPrototype ValueOf();

	/// <summary>
	/// A reference to the prototype for a class of objects.
	/// </summary>
	[Description("@#prototype")]
	public extern static TPrototype Prototype { get; }
}

[ECMAScript]
[Description("@#Object")]
public interface IObject<TPrototype,TValue> : IObject<TPrototype>
{
	public new extern TValue? this[string key] { get; }

	public new extern TValue? this[uint index] { get; }
}

public static partial class Global
{
	extension<T>(T obj)
	{
		/// <summary>
		/// Returns undefined value.This is a special property designed to simulate the undefined implementation of javascript using C # syntax
		/// </summary>
		[ECMAScriptLiteral("undefined")]
		public extern static T? Undefined { get; }
	}

	extension(object obj)
	{
		[Description("@#is")]
		public extern static bool Is(object? value1, object value2);

		/// <summary>
		/// Adds a property to an object, or modifies attributes of an existing property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="o">Object on which to add or modify the property.This can be a native JavaScript object (that is, a user-defined object or a built in object) or a DOM object.</param>
		/// <param name="p">The property name.</param>
		/// <param name="attributes">Descriptor for the property.It can be for a data property or an accessor property.</param>
		/// <returns></returns>
		[Description("@#defineProperty")]
		public extern static IObject<TTargetT> DefineProperty<TTargetT>(TTargetT o, PropertyKey p, PropertyDescriptor attributes);

		/// <summary>
		/// Adds one or more properties to an object, and/or modifies attributes of existing properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="o">Object on which to add or modify the properties.This can be a native JavaScript object or a DOM object.</param>
		/// <param name="properties">JavaScript object that contains one or more descriptor objects. Each descriptor object describes a data property or an accessor property.</param>
		/// <returns></returns>
		[Description("@#defineProperties")]
		public extern static IObject<TTarget> DefineProperties<TTarget>(TTarget o, PropertyDescriptorMap properties);

		/// <summary>
		/// Prevents the modification of attributes of existing properties, and prevents the addition of new properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="o">Object on which to lock the attributes.</param>
		/// <returns></returns>
		[Description("@#seal")]
		public extern static IObject<TTarget> Seal<TTarget>(TTarget o);

		/// <summary>
		/// Returns the prototype of an object.
		/// </summary>
		/// <param name="o">The object that references the prototype.</param>
		/// <returns></returns>
		[Description("@#getPrototypeOf")]
		public extern static IObject<TPrototype> GetPrototypeOf<TPrototype>(TPrototype o);

		/// <summary>
		/// Gets the own property descriptor of the specified object.
		/// An own property descriptor is one that is defined directly on the object and is not inherited from the object's prototype.
		/// </summary>
		/// <param name="o">Object that contains the property.</param>
		/// <param name="p">Name of the property.</param>
		/// <returns></returns>
		[Description("@#getOwnPropertyDescriptor")]
		public extern static PropertyDescriptor? GetOwnPropertyDescriptor(object o, PropertyKey p);

		/// <summary>
		/// Returns the names of the own properties of an object. The own properties of an object are those that are defined directly
		/// on that object, and are not inherited from the object's prototype. The properties of an object include both fields (objects) and functions.
		/// </summary>
		/// <param name="o">Object that contains the own properties.</param>
		/// <returns></returns>
		[Description("@#getOwnPropertyNames")]
		public extern static Array<string> GetOwnPropertyNames(object o);

		/// <summary>
		/// Creates an object that has the specified prototype or that has null prototype.
		/// </summary>
		/// <param name="o">Object to use as a prototype.May be null.</param>
		/// <returns></returns>
		[Description("@#create")]
		public extern static IObject Create(object? o);

		/// <summary>
		/// Creates an object that has the specified prototype, and that optionally contains specified properties.
		/// </summary>
		/// <param name="o">Object to use as a prototype.May be null</param>
		/// <param name="properties">JavaScript object that contains one or more property descriptors.</param>
		/// <returns></returns>
		[Description("@#create")]
		public extern static IObject Create(object? o, PropertyDescriptorMap properties);

		/// <summary>
		/// Returns a date converted to a string using the current locale.
		/// </summary>
		/// <returns></returns>
		[Description("@#toLocaleString")]
		public extern string ToLocaleString();

		/// <summary>
		/// Returns a date converted to a string using the current locale.
		/// </summary>
		/// <returns></returns>
		[Description("@#toString")]
		public extern string ToString(uint r);

		/// <summary>
		/// Determines whether an object has a property with the specified name.
		/// </summary>
		/// <param name="v">A property name.</param>
		/// <returns></returns>
		[Description("@#hasOwnProperty")]
		public extern bool HasOwnProperty(PropertyKey v);

		/// <summary>
		/// Determines whether an object exists in another object's prototype chain.
		/// </summary>
		/// <param name="v">Another object whose prototype chain is to be checked.</param>
		/// <returns></returns>
		[Description("@#isPrototypeOf")]
		public extern bool IsPrototypeOf(object? v);

		/// <summary>
		/// Determines whether a specified property is enumerable.
		/// </summary>
		/// <param name="v">A property name.</param>
		/// <returns></returns>
		[Description("@#propertyIsEnumerable")]
		public extern bool PropertyIsEnumerable(PropertyKey v);

		/// <summary>
		/// Returns the names of the enumerable string properties and methods of an object.
		/// </summary>
		/// <param name="o">Object that contains the properties and methods.This can be an object that you created or an existing Document Object Model(DOM) object.</param>
		/// <returns></returns>
		[Description("@#keys")]
		public extern static Array<string> Keys(object o);

		/// <summary>
		/// Returns true if existing property attributes cannot be modified in an object and new properties cannot be added to the object.
		/// </summary>
		/// <param name="o">Object to test.</param>
		/// <returns></returns>
		[Description("@#isSealed")]
		public extern static bool IsSealed(object o);

		/// <summary>
		/// Returns true if existing property attributes and values cannot be modified in an object, and new properties cannot be added to the object.
		/// </summary>
		/// <param name="o">Object to test.</param>
		/// <returns></returns>
		[Description("@#isFrozen")]
		public extern static bool IsFrozen(object o);

		/// <summary>
		/// Returns a value that indicates whether new properties can be added to an object.
		/// </summary>
		/// <param name="o">Object to test.</param>
		/// <returns></returns>
		[Description("@#isExtensible")]
		public extern static bool IsExtensible(object o);

		/// <summary>
		/// Prevents the modification of existing property attributes and values, and prevents the addition of new properties.
		/// </summary>
		/// <param name="o">Object on which to lock the attributes.</param>
		/// <returns></returns>
		[Description("@#freeze")]
		public extern static IReadOnly<TTarget> Freeze<TTarget>(TTarget o);
	}
}