using PropertyKey = ECMAScript.Either<string, ECMAScript.Number, ECMAScript.Symbol>;

namespace ECMAScript;

[ECMAScript]
[Description("@#")]
public interface IReadOnly<T>
{
	T Value { get; }
}

[ECMAScript]
[Description("@#Object")]
public interface IObject
{
	/// <summary>
	/// Represents a JavaScript object that can be indexed by property name.
	/// This interface is intentionally narrow: it models "object-like index access"
	/// rather than every value that can exist in JavaScript.
	/// </summary>
	IObject? this[string key] { get; }

	/// <summary>
	/// Represents JavaScript index access on object-like values.
	/// </summary>
	IObject? this[uint index] { get; }
}

public static partial class Global
{
	/// <summary>
	/// Projection of JavaScript's Object constructor and Object.prototype surface onto
	/// C#'s extension-member model. Static members in this block correspond to
	/// <c>Object.*</c>, while instance members correspond to <c>Object.prototype.*</c>.
	/// This keeps user code close to JavaScript runtime shape without introducing
	/// extra C# host types that would increase the sense of mismatch.
	/// </summary>
	extension(object obj)
	{
		public extern static bool operator ==(object? a, object? b);
		
		public extern static bool operator !=(object? a, object? b);

		/// <summary>
		/// 仅能在构造函数中使用，调用当前对象的父类构造函数
		/// </summary>
		/// <param name="values"></param>
		[Description("@#super")]
		public extern void Super(params Array<object?> values);

		/// <summary>
		/// Copies enumerable own properties from one or more source objects onto the target object.
		/// The target instance itself is returned so the original static type is preserved in C#.
		/// </summary>
		/// <param name="target">The target object to mutate.</param>
		/// <param name="source">One or more source objects whose properties will be copied.</param>
		/// <returns>The same <paramref name="target"/> instance after assignment.</returns>
		[Description("@#assign")]
		public extern static TTarget Assign<TTarget>(TTarget target, params object[] source);

		/// <summary>
		/// Creates a new JavaScript object with the specified prototype.
		/// The return type stays as <see cref="IObject"/> because the created value is primarily
		/// consumed through dynamic property/index access rather than a CLR prototype contract.
		/// </summary>
		/// <param name="proto">The object to use as the prototype. May be null.</param>
		/// <returns>A newly created object, or null when the JavaScript result is null.</returns>
		[Description("@#create")]
		public extern static IObject? Create(object? proto);

		/// <summary>
		/// Creates a new JavaScript object with the specified prototype and property descriptors.
		/// </summary>
		/// <param name="proto">The object to use as the prototype. May be null.</param>
		/// <param name="propertiesObject">A JavaScript object containing property descriptors.</param>
		/// <returns>A newly created object, or null when the JavaScript result is null.</returns>
		[Description("@#create")]
		public extern static IObject? Create(object? proto, PropertyDescriptorMap propertiesObject);

		/// <summary>
		///  确定两个值是否为相同值。如果以下其中一项成立，则两个值相同：<br/>
		///  --1、都是 undefined<br/>
		///  --2、都是 null<br/>
		///  --3、都是 true 或者都是 false<br/>
		///  --4、都是长度相同、字符相同、顺序相同的字符串<br/>
		///  --5、都是相同的对象（意味着两个值都引用了内存中的同一对象）<br/>
		///  --6、都是 BigInt 且具有相同的数值<br/>
		///  --7、都是 symbol 且引用相同的 symbol 值<br/>
		///  --8、都是数字且<br/>
		///  --9、都是 +0<br/>
		///  --10、都是 -0<br/>
		///  --11、都是 NaN<br/>
		///  --12、都有相同的值，非零且都不是 NaN<br/>
		///  Object.is() 与 == 运算符并不等价。== 运算符在测试相等性之前，会对两个操作数进行类型转换（如果它们不是相同的类型），这可能会导致一些非预期的行为，例如 "" == false 的结果是 true，但是 Object.is() 不会对其操作数进行类型转换。<br/>
		///  Object.is() 也不等价于 === 运算符。Object.is() 和 === 之间的唯一区别在于它们处理带符号的 0 和 NaN 值的时候。=== 运算符（和 == 运算符）将数值 -0 和 +0 视为相等，但是会将 NaN 视为彼此不相等。
		/// </summary>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		/// <returns></returns>
		[Description("@#is")]
		public extern static bool Is(object? value1, object? value2);	

		/// <summary>
		/// Adds a property to an object, or modifies attributes of an existing property.
		/// The original target type is returned so callers keep their static CLR shape.
		/// </summary>
		/// <typeparam name="TTarget">The static CLR type of the target object.</typeparam>
		/// <param name="o">Object on which to add or modify the property. This can be a native JavaScript object or a DOM object.</param>
		/// <param name="p">The property name.</param>
		/// <param name="attributes">Descriptor for the property. It can describe a data or accessor property.</param>
		/// <returns>The same <paramref name="o"/> instance.</returns>
		[Description("@#defineProperty")]
		public extern static TTarget DefineProperty<TTarget>(TTarget o, PropertyKey p, PropertyDescriptor attributes);

		/// <summary>
		/// Adds one or more properties to an object, and/or modifies attributes of existing properties.
		/// The original target type is returned so callers keep their static CLR shape.
		/// </summary>
		/// <typeparam name="TTarget">The static CLR type of the target object.</typeparam>
		/// <param name="o">Object on which to add or modify the properties. This can be a native JavaScript object or a DOM object.</param>
		/// <param name="properties">A JavaScript object containing one or more descriptor objects.</param>
		/// <returns>The same <paramref name="o"/> instance.</returns>
		[Description("@#defineProperties")]
		public extern static TTarget DefineProperties<TTarget>(TTarget o, PropertyDescriptorMap properties);

		/// <summary>
		/// Prevents the modification of attributes of existing properties and prevents the addition of new properties.
		/// The original target type is returned so callers keep their static CLR shape.
		/// </summary>
		/// <typeparam name="TTarget">The static CLR type of the target object.</typeparam>
		/// <param name="o">Object on which to lock the attributes.</param>
		/// <returns>The same <paramref name="o"/> instance.</returns>
		[Description("@#seal")]
		public extern static TTarget Seal<TTarget>(TTarget o);

		/// <summary>
		/// Returns the prototype of an object.
		/// The result is modeled as <see cref="IObject"/> because callers typically consume it
		/// as a JavaScript object rather than as a strongly typed CLR prototype instance.
		/// </summary>
		/// <param name="o">The object that references the prototype.</param>
		/// <returns>The prototype object, or null when the JavaScript result is null.</returns>
		[Description("@#getPrototypeOf")]
		public extern static IObject? GetPrototypeOf(object o);

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
		public extern static ref readonly TTarget Freeze<TTarget>(TTarget o);
	}
}
