using PropertyKey = ECMAScript.PropertyKey;

namespace ECMAScript;

[ECMAScript]
[Description("@#")]
/// <summary>
/// 表示经过 compiler 擦除后仍保留一个值投影的泛型 host wrapper。
/// Represents a generic host wrapper whose value projection remains after compiler erasure.
/// </summary>
/// <remarks>This interface is an authoring-time type constraint and does not automatically emit a CLR-style wrapper object.
/// 该接口用于 authoring 类型约束，不会自动发射 CLR 风格包装对象。</remarks>
public interface IReadOnly<T>
{
	/// <summary>Gets the projected value. 获取投影后的值。</summary>
	T Value { get; }
}

[ECMAScript]
[Description("@#Object")]
/// <summary>Host binding contract for JavaScript <c>Object</c>. JavaScript <c>Object</c> 的宿主绑定契约。</summary>
/// <remarks>Member mapping depends on ECMAScript descriptor metadata and whitelist rules; it is not the <c>System.Object</c> API.
/// 成员映射依赖 ECMAScript 描述属性和白名单规则，不等同于 <c>System.Object</c> API。</remarks>
public interface IObject
{
	/// <summary>
	/// Represents a JavaScript object that can be indexed by property name.
	/// This interface is intentionally narrow: it models "object-like index access"
	/// rather than every value that can exist in JavaScript.
	/// 表示可按属性名索引的 JavaScript 对象。该接口刻意保持狭窄，只建模“对象式索引访问”，而不是 JavaScript 中的一切值。
	/// </summary>
	IObject? this[string key] { get; }

	/// <summary>
	/// Represents JavaScript index access on object-like values.
	/// 表示对象式值上的 JavaScript 数值索引访问。
	/// </summary>
	IObject? this[Number index] { get; }
}

public static partial class Global
{
	/// <summary>
	/// Projection of JavaScript's Object constructor and Object.prototype surface onto
	/// C#'s extension-member model. Static members in this block correspond to
	/// <c>Object.*</c>, while instance members correspond to <c>Object.prototype.*</c>.
	/// This keeps user code close to JavaScript runtime shape without introducing
	/// extra C# host types that would increase the sense of mismatch.
	/// Members that C# instance dispatch cannot project faithfully, such as
	/// <c>Object.prototype.toString()</c> on <see cref="object"/>, are intentionally omitted
	/// instead of being exposed under misleading CLR-only shapes.
	/// Likewise, the callable <c>Object(...)</c> coercion entry point is intentionally
	/// not exposed here because its boxed-wrapper result shape does not map cleanly to
	/// the public C# host model.
	/// 将 JavaScript <c>Object</c> 构造器和 <c>Object.prototype</c> 投影到 C# 扩展成员模型：静态成员对应 <c>Object.*</c>，
	/// 实例成员对应 <c>Object.prototype.*</c>。无法忠实投影的 C# 实例派发（如 <see cref="object"/> 上的 <c>toString()</c>）会省略，
	/// 而不是以误导性的 CLR 形式公开；同样不公开可调用的 <c>Object(...)</c> 强制转换入口，因为其装箱结果形状无法干净映射到公开 C# 宿主模型。
	/// </summary>
	extension(object obj)
	{
		/// <summary>Uses JavaScript equality lowering for arbitrary runtime values. 对任意运行时值使用 JavaScript 相等比较 lowering。</summary>
		public extern static bool operator ==(object? a, object? b);
		
		/// <summary>Uses JavaScript inequality lowering for arbitrary runtime values. 对任意运行时值使用 JavaScript 不等比较 lowering。</summary>
		public extern static bool operator !=(object? a, object? b);

		/// <summary>
		/// Calls the current object's base constructor; valid only inside a derived constructor.
		/// 仅能在派生构造函数中使用，用于调用当前对象的父类构造函数。
		/// </summary>
		/// <param name="values"></param>
		[Description("@#super")]
		public extern void Super(params Array<object?> values);

		/// <summary>
		/// Copies enumerable own properties from one or more source objects onto the target object.
		/// The target instance itself is returned so the original static type is preserved in C#.
		/// Source values are nullable because JavaScript allows <c>null</c>-like values in the source list and handles them at runtime.
		/// 将一个或多个 source 的可枚举自有属性复制到 target，并返回原 target 以保持 C# 静态类型；空 source 的处理遵循 JavaScript 运行时。
		/// </summary>
		/// <param name="target">The target object to mutate.</param>
		/// <param name="source">One or more source objects whose properties will be copied.</param>
		/// <returns>The same <paramref name="target"/> instance after assignment.</returns>
		[Description("@#assign")]
		public extern static TTarget Assign<TTarget>(TTarget target, params object?[] source);

		/// <summary>
		/// Creates a new JavaScript object with the specified prototype.
		/// The return type stays as <see cref="IObject"/> because the created value is primarily
		/// consumed through dynamic property/index access rather than a CLR prototype contract.
		/// 使用指定 prototype 创建 JavaScript 对象；结果保持 <see cref="IObject"/>，因为通常通过动态属性/索引访问使用，而不是 CLR prototype 契约。
		/// </summary>
		/// <param name="proto">The object to use as the prototype. May be null.</param>
		/// <returns>A newly created JavaScript object.</returns>
		[Description("@#create")]
		public extern static IObject Create(object? proto);

		/// <summary>
		/// Creates a new JavaScript object with the specified prototype and property descriptors.
		/// 使用指定 prototype 和 property descriptor map 创建 JavaScript 对象。
		/// </summary>
		/// <param name="proto">The object to use as the prototype. May be null.</param>
		/// <param name="propertiesObject">A JavaScript object containing property descriptors.</param>
		/// <returns>A newly created JavaScript object.</returns>
		[Description("@#create")]
		public extern static IObject Create(object? proto, PropertyDescriptorMap propertiesObject);

		/// <summary>
		/// Compares two values using JavaScript SameValue semantics.
		/// Unlike loose equality it performs no coercion. Unlike strict equality, it treats <c>NaN</c> as equal to itself and distinguishes <c>+0</c> from <c>-0</c>.
		/// 使用 JavaScript SameValue 语义比较两个值。它不同于宽松相等，不进行类型强制转换；也不同于严格相等：<c>NaN</c> 与自身相同，<c>+0</c> 与 <c>-0</c> 不同。
		/// </summary>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		/// <returns></returns>
		[Description("@#is")]
		public extern static bool Is(object? value1, object? value2);	

		/// <summary>
		/// Adds a property to an object, or modifies attributes of an existing property.
		/// The original target type is returned so callers keep their static CLR shape.
		/// 在对象上新增属性或修改既有属性 descriptor；返回原目标类型以保留调用方的 C# 静态形状。
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
		/// 在对象上新增或修改多个属性 descriptor；返回原目标类型以保留调用方的 C# 静态形状。
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
		/// 阻止扩展与重新配置对象的自有属性；返回原目标类型以保留调用方的 C# 静态形状。
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
		/// 获取对象 prototype；返回 <see cref="IObject"/>，因为调用方通常将其作为 JavaScript 对象消费，而非强类型 CLR prototype 实例。
		/// </summary>
		/// <param name="o">The object that references the prototype.</param>
		/// <returns>The prototype object, or null when the JavaScript result is null.</returns>
		[Description("@#getPrototypeOf")]
		public extern static IObject? GetPrototypeOf(object o);

		/// <summary>
		/// Gets the own property descriptor of the specified object.
		/// An own property descriptor is one that is defined directly on the object and is not inherited from the object's prototype.
		/// 获取指定对象的自有属性 descriptor；自有属性直接定义在对象上，不从 prototype 继承。不存在时返回 <see langword="null"/>。
		/// </summary>
		/// <param name="o">Object that contains the property.</param>
		/// <param name="p">Name of the property.</param>
		/// <returns></returns>
		[Description("@#getOwnPropertyDescriptor")]
		public extern static PropertyDescriptor? GetOwnPropertyDescriptor(object o, PropertyKey p);

		/// <summary>
		/// Returns all own property descriptors of an object.
		/// 返回对象全部自有属性 descriptor 的 JavaScript descriptor map。
		/// </summary>
		/// <param name="o">Object that contains the own properties.</param>
		/// <returns>A JavaScript object whose values are property descriptors.</returns>
		[Description("@#getOwnPropertyDescriptors")]
		public extern static PropertyDescriptorMap GetOwnPropertyDescriptors(object o);

		/// <summary>
		/// Returns the names of the own properties of an object. The own properties of an object are those that are defined directly
		/// on that object, and are not inherited from the object's prototype. The properties of an object include both fields (objects) and functions.
		/// 返回对象全部自有属性名，包括不可枚举属性，不包括从 prototype 继承的属性。
		/// </summary>
		/// <param name="o">Object that contains the own properties.</param>
		/// <returns></returns>
		[Description("@#getOwnPropertyNames")]
		public extern static Array<string> GetOwnPropertyNames(object o);

		/// <summary>
		/// Returns the own symbol properties of an object.
		/// 返回对象直接定义的 symbol key，自有属性不包括 prototype 继承的 symbol。
		/// </summary>
		/// <param name="o">Object that contains the own symbol properties.</param>
		/// <returns>An array of symbol keys defined directly on <paramref name="o"/>.</returns>
		[Description("@#getOwnPropertySymbols")]
		public extern static Array<Symbol> GetOwnPropertySymbols(object o);

		/// <summary>
		/// Returns a locale-sensitive string representation of the object.
		/// 返回对象的本地化字符串表示；普通对象会按 JavaScript <c>Object.prototype.toLocaleString</c> 委托行为处理。
		/// </summary>
		/// <returns></returns>
		[Description("@#toLocaleString")]
		public extern string ToLocaleString();

		/// <summary>
		/// Returns a locale-sensitive string representation of the object.
		/// JavaScript keeps these parameters for historical compatibility and ECMA-402 forwarding, even though plain <c>Object.prototype.toLocaleString</c> delegates to <c>toString()</c>.
		/// 带保留参数的本地化字符串表示；JavaScript 为历史兼容和 ECMA-402 转发保留这些参数，普通 Object.prototype 实际委托到 <c>toString()</c>。
		/// </summary>
		[Description("@#toLocaleString")]
		public extern string ToLocaleString(object? reserved1, object? reserved2 = null);

		/// <summary>
		/// Returns the underlying JavaScript object value for this host projection.
		/// This is the direct projection of <c>Object.prototype.valueOf()</c>.
		/// 返回此宿主投影所承载的底层 JavaScript 对象值；直接映射 <c>Object.prototype.valueOf()</c>。
		/// </summary>
		[Description("@#valueOf")]
		public extern object ValueOf();

		/// <summary>
		/// Returns a callable that permanently uses the supplied receiver.
		/// </summary>
		/// <remarks>
		/// This is the exact <c>Function.prototype.bind</c> host shape. The receiver remains
		/// <see cref="object"/> because JavaScript functions can bind arbitrary values; callers
		/// must establish callability before invoking this member.
		/// 返回永久绑定指定 receiver 的 callable；这是 <c>Function.prototype.bind</c> 的准确宿主形状。receiver 使用 <see cref="object"/>，因为 JavaScript 函数可绑定任意值，调用方需自行保证当前对象可调用。
		/// </remarks>
		[Description("@#bind")]
		public extern object Bind(object? thisArg);

		/// <summary>
		/// Determines whether an object has a property with the specified name.
		/// 检查对象是否具有指定的自有属性，不遍历 prototype chain。
		/// </summary>
		/// <param name="v">A property name.</param>
		/// <returns></returns>
		[Description("@#hasOwnProperty")]
		public extern bool HasOwnProperty(PropertyKey v);		

		/// <summary>
		/// Determines whether an object exists in another object's prototype chain.
		/// 检查当前对象是否位于另一个对象的 prototype chain 中。
		/// </summary>
		/// <param name="v">Another object whose prototype chain is to be checked.</param>
		/// <returns></returns>
		[Description("@#isPrototypeOf")]
		public extern bool IsPrototypeOf(object? v);

		/// <summary>
		/// Determines whether a specified property is enumerable.
		/// 检查指定自有属性是否可枚举；不存在或继承属性返回 <see langword="false"/>。
		/// </summary>
		/// <param name="v">A property name.</param>
		/// <returns></returns>
		[Description("@#propertyIsEnumerable")]
		public extern bool PropertyIsEnumerable(PropertyKey v);

		/// <summary>
		/// Legacy JavaScript accessor for the current prototype.
		/// This member is exposed under its exact JavaScript name because C# can represent it directly,
		/// which keeps the public surface closer to <c>Object.prototype</c> without an extra host alias.
		/// 当前 prototype 的遗留 JavaScript 访问器；保留精确 JavaScript 名称，因为 C# 可以直接表达，避免额外宿主别名。
		/// </summary>
		[Description("@#__proto__")]
		public extern object? __proto__ { get; set; }

		/// <summary>
		/// Legacy JavaScript helper that installs a getter for the supplied property key on the current object.
		/// The getter delegate matches the accessor shape carried by JavaScript property descriptors.
		/// 遗留 JavaScript 帮助器，在当前对象上安装指定 key 的 getter；委托形状匹配 JavaScript property descriptor 的 accessor。
		/// </summary>
		[Description("@#__defineGetter__")]
		public extern void __defineGetter__(PropertyKey property, Func<object?> getter);

		/// <summary>
		/// Legacy JavaScript helper that installs a setter for the supplied property key on the current object.
		/// The setter delegate matches the accessor shape carried by JavaScript property descriptors.
		/// 遗留 JavaScript 帮助器，在当前对象上安装指定 key 的 setter；委托形状匹配 JavaScript property descriptor 的 accessor。
		/// </summary>
		[Description("@#__defineSetter__")]
		public extern void __defineSetter__(PropertyKey property, Action<object?> setter);

		/// <summary>
		/// Legacy JavaScript helper that looks up an inherited or own getter for the supplied property key.
		/// JavaScript returns <c>undefined</c> when no getter exists, and this projection surfaces that absence as <see langword="null" />.
		/// 查找当前对象或 prototype chain 上指定 key 的 getter；不存在时 JavaScript <c>undefined</c> 投影为 <see langword="null"/>。
		/// </summary>
		[Description("@#__lookupGetter__")]
		public extern Func<object?>? __lookupGetter__(PropertyKey property);

		/// <summary>
		/// Legacy JavaScript helper that looks up an inherited or own setter for the supplied property key.
		/// JavaScript returns <c>undefined</c> when no setter exists, and this projection surfaces that absence as <see langword="null" />.
		/// 查找当前对象或 prototype chain 上指定 key 的 setter；不存在时 JavaScript <c>undefined</c> 投影为 <see langword="null"/>。
		/// </summary>
		[Description("@#__lookupSetter__")]
		public extern Action<object?>? __lookupSetter__(PropertyKey property);

		/// <summary>
		/// Returns the names of the enumerable string properties and methods of an object.
		/// 返回对象可枚举的自有字符串 key；不包含 symbol 和继承属性。
		/// </summary>
		/// <param name="o">Object that contains the properties and methods.This can be an object that you created or an existing Document Object Model(DOM) object.</param>
		/// <returns></returns>
		[Description("@#keys")]
		public extern static Array<string> Keys(object o);

		/// <summary>
		/// Returns the values of the enumerable own properties of an object.
		/// 返回对象可枚举自有字符串 key 对应的值，顺序遵循 JavaScript <c>Object.values</c>。
		/// </summary>
		/// <param name="o">Object that contains the properties.</param>
		/// <returns>An array of property values.</returns>
		[Description("@#values")]
		public extern static Array<object?> Values(object o);

		/// <summary>
		/// Returns the enumerable own property key-value pairs of an object.
		/// 返回对象可枚举自有字符串 key 的 [key, value] 对，顺序遵循 JavaScript <c>Object.entries</c>。
		/// </summary>
		/// <param name="o">Object that contains the properties.</param>
		/// <returns>An array of two-element key-value pairs.</returns>
		[Description("@#entries")]
		public extern static Array<Array<object?>> Entries(object o);

		/// <summary>
		/// Creates an object from key-value entries.
		/// <see cref="IEnumerable{T}"/> is used here as the common C# input surface for values
		/// such as arrays, lists, and read-only list families that map to JavaScript arrays or iterables.
		/// 从键值 entry iterable 创建对象；<see cref="IEnumerable{T}"/> 是数组、列表等可映射 JavaScript iterable 的通用 C# 输入表面。
		/// </summary>
		/// <param name="entries">Pairs of property keys and values.</param>
		/// <returns>A newly created JavaScript object.</returns>
		[Description("@#fromEntries")]
		public extern static IObject FromEntries(IEnumerable<Array<object?>> entries);

		/// <summary>
		/// Creates an object from key-value entries.
		/// This overload accepts any C# sequence-of-sequences that maps cleanly to JavaScript's iterable-of-entry input.
		/// Each inner sequence is consumed as one JavaScript entry, and the runtime uses its first two produced values as the property key and value.
		/// 从键值 entry iterable 创建对象；每个内部序列作为一个 JavaScript entry 消费，前两个产生值分别用作属性 key 与 value。
		/// </summary>
		[Description("@#fromEntries")]
		public extern static IObject FromEntries(IEnumerable<IEnumerable<object?>> entries);

		/// <summary>
		/// Groups iterable values by a JavaScript property key and returns the grouped result as an object.
		/// The return type stays as <see cref="IObject"/> because JavaScript produces an object-like result whose keys are consumed through dynamic property access.
		/// 按 JavaScript property key 对 iterable 元素分组；结果为对象式值，key 通常通过动态属性访问消费，因此返回 <see cref="IObject"/>。
		/// </summary>
		[Description("@#groupBy")]
		public extern static IObject GroupBy<T>(IEnumerable<T> items, Func<T, Number, PropertyKey> callbackfn);

		/// <summary>
		/// Groups iterable values by a JavaScript property key and returns the grouped result as an object.
		/// This overload mirrors the JavaScript callback shape when the caller does not need the index argument.
		/// 不需要索引参数时的 <c>Object.groupBy</c> 回调重载。
		/// </summary>
		[Description("@#groupBy")]
		public extern static IObject GroupBy<T>(IEnumerable<T> items, Func<T, PropertyKey> callbackfn);

		/// <summary>
		/// Returns whether the object has the specified own property.
		/// </summary>
		/// <param name="o">Object that contains the property.</param>
		/// <param name="p">Property key to test.</param>
		/// <returns><see langword="true"/> when the property exists directly on the object.</returns>
		[Description("@#hasOwn")]
		public extern static bool HasOwn(object o, PropertyKey p);

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
		/// Prevents new properties from being added to an object.
		/// The original target type is returned so callers keep their static CLR shape.
		/// </summary>
		/// <typeparam name="TTarget">The static CLR type of the target object.</typeparam>
		/// <param name="o">Object on which to prevent extensions.</param>
		/// <returns>The same <paramref name="o"/> instance.</returns>
		[Description("@#preventExtensions")]
		public extern static TTarget PreventExtensions<TTarget>(TTarget o);

		/// <summary>
		/// Changes the prototype of an object.
		/// The original target type is returned so callers keep their static CLR shape.
		/// </summary>
		/// <typeparam name="TTarget">The static CLR type of the target object.</typeparam>
		/// <param name="o">Object whose prototype will be updated.</param>
		/// <param name="proto">The new prototype object, or null.</param>
		/// <returns>The same <paramref name="o"/> instance.</returns>
		[Description("@#setPrototypeOf")]
		public extern static TTarget SetPrototypeOf<TTarget>(TTarget o, object? proto);

		/// <summary>
		/// Prevents the modification of existing property attributes and values, and prevents the addition of new properties.
		/// The original target type is returned so callers keep their static CLR shape.
		/// </summary>
		/// <param name="o">Object on which to lock the attributes.</param>
		/// <returns></returns>
		[Description("@#freeze")]
		public extern static TTarget Freeze<TTarget>(TTarget o);
	}
}
