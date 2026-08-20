namespace ECMAScript;

/// <summary>
/// Projection of JavaScript's <c>Reflect</c> object.
/// The surface keeps JavaScript property-key semantics instead of collapsing them to <see cref="string"/>.
/// JavaScript <c>Reflect</c> 对象投影；表面保留 JavaScript 属性键语义，而不折叠为 <see cref="string"/>。
/// </summary>
[ECMAScript]
[Description("@#Reflect")]
/// <remarks>
/// <c>Reflect</c> retains JavaScript property-key, receiver, and return-value rules, so some parameters must retain
/// <see cref="object"/> or <see cref="JPropertyKey"/> shapes. Do not reduce these to string-only or catch-all APIs for superficial C# convenience.
/// <c>Reflect</c> 保留 JavaScript 的属性键、receiver 和返回值规则，因此部分参数必须保留
/// <see cref="object"/> 或 <see cref="JPropertyKey"/> 形状；不要为表面简洁而改成仅 string 或 catch-all API。
/// </remarks>
public static class Reflect
{
	/// <summary>
	/// Invokes a target function with the supplied <c>this</c> value and argument list.
	/// The argument list stays nullable because JavaScript call arguments can carry <see langword="null"/> values directly.
	/// 使用给定 <c>this</c> 值和参数列表调用目标函数；参数列表元素保持可空，因为 JavaScript 调用参数可直接携带 <see langword="null"/>。
	/// </summary>
	[Description("@#apply")]
	public extern static object? Apply(object target, object? thisArg, object?[] argumentsList);

	/// <summary>
	/// Invokes a target function with the supplied <c>this</c> value and argument list.
	/// This overload accepts any C# sequence family that maps cleanly to the JavaScript argument-list surface.
	/// 使用给定 <c>this</c> 值和参数列表调用目标函数；此重载接受可清晰映射到 JavaScript 参数列表表面的任意 C# 序列族。
	/// </summary>
	[Description("@#apply")]
	public extern static object? Apply(object target, object? thisArg, IEnumerable<object?> argumentsList);

	/// <summary>
	/// Invokes a target as a constructor.
	/// The argument list stays nullable because JavaScript constructor arguments can carry <see langword="null"/> values directly.
	/// 将目标作为构造器调用；参数列表元素保持可空，因为 JavaScript 构造器参数可直接携带 <see langword="null"/>。
	/// </summary>
	[Description("@#construct")]
	public extern static object Construct(object target, object?[] argumentsList);

	/// <summary>
	/// Invokes a target as a constructor.
	/// This overload accepts any C# sequence family that maps cleanly to the JavaScript argument-list surface.
	/// 将目标作为构造器调用；此重载接受可清晰映射到 JavaScript 参数列表表面的任意 C# 序列族。
	/// </summary>
	[Description("@#construct")]
	public extern static object Construct(object target, IEnumerable<object?> argumentsList);

	/// <summary>
	/// Invokes a target as a constructor with an explicit <c>newTarget</c>.
	/// 使用显式 <c>newTarget</c> 将目标作为构造器调用。
	/// </summary>
	[Description("@#construct")]
	public extern static object Construct(object target, object?[] argumentsList, object newTarget);

	/// <summary>
	/// Invokes a target as a constructor with an explicit <c>newTarget</c>.
	/// This overload accepts any C# sequence family that maps cleanly to the JavaScript argument-list surface.
	/// 使用显式 <c>newTarget</c> 将目标作为构造器调用；此重载接受可清晰映射到 JavaScript 参数列表表面的任意 C# 序列族。
	/// </summary>
	[Description("@#construct")]
	public extern static object Construct(object target, IEnumerable<object?> argumentsList, object newTarget);

	/// <summary>
	/// Defines or reconfigures a property and reports the JavaScript boolean result directly.
	/// This stays distinct from <c>Object.defineProperty</c>, which returns the target object instead.
	/// 定义或重新配置属性并直接返回 JavaScript 布尔结果；区别于返回目标对象的 <c>Object.defineProperty</c>。
	/// </summary>
	[Description("@#defineProperty")]
	public extern static bool DefineProperty(object target, JPropertyKey propertyKey, JSPropertyDescriptor attributes);

	/// <summary>
	/// Reads a property from the target.
	/// 读取目标上的属性。
	/// </summary>
	[Description("@#get")]
	public extern static object? Get(object target, JPropertyKey propertyKey);

	/// <summary>
	/// Reads a property from the target using an explicit receiver.
	/// 使用显式 receiver 读取目标上的属性。
	/// </summary>
	[Description("@#get")]
	public extern static object? Get(object target, JPropertyKey propertyKey, object receiver);

	/// <summary>
	/// Sets a property on the target.
	/// 在目标上设置属性。
	/// </summary>
	[Description("@#set")]
	public extern static bool Set(object target, JPropertyKey propertyKey, object? value);

	/// <summary>
	/// Sets a property on the target using an explicit receiver.
	/// 使用显式 receiver 在目标上设置属性。
	/// </summary>
	[Description("@#set")]
	public extern static bool Set(object target, JPropertyKey propertyKey, object? value, object receiver);

	/// <summary>
	/// Returns whether the property exists on the target or its prototype chain.
	/// 返回属性是否存在于目标或其原型链上。
	/// </summary>
	[Description("@#has")]
	public extern static bool Has(object target, JPropertyKey propertyKey);

	/// <summary>
	/// Deletes an own property from the target.
	/// 从目标删除自身属性。
	/// </summary>
	[Description("@#deleteProperty")]
	public extern static bool DeleteProperty(object target, JPropertyKey propertyKey);

	/// <summary>
	/// Reads the prototype of the target.
	/// The return type stays as <see cref="IObject"/> because the JavaScript result is consumed as an object-like runtime value.
	/// 读取目标的原型；返回类型保持为 <see cref="IObject"/>，因为 JavaScript 结果作为对象类运行时值使用。
	/// </summary>
	[Description("@#getPrototypeOf")]
	public extern static IObject? GetPrototypeOf(object target);

	/// <summary>
	/// Returns all own property keys of the target, including symbols.
	/// 返回目标的所有自身属性键，包括 Symbol。
	/// </summary>
	[Description("@#ownKeys")]
	public extern static Array<JPropertyKey> OwnKeys(object target);

	/// <summary>
	/// Returns the own property descriptor for the specified property key.
	/// 返回给定属性键的自身属性描述符。
	/// </summary>
	[Description("@#getOwnPropertyDescriptor")]
	public extern static JSPropertyDescriptor? GetOwnPropertyDescriptor(object target, JPropertyKey propertyKey);

	/// <summary>
	/// Updates the target prototype and returns the JavaScript boolean success result.
	/// This stays aligned with <c>Reflect.setPrototypeOf</c>, which differs from <c>Object.setPrototypeOf</c>.
	/// 更新目标原型并返回 JavaScript 布尔成功结果；保持与 <c>Reflect.setPrototypeOf</c> 一致，区别于 <c>Object.setPrototypeOf</c>。
	/// </summary>
	[Description("@#setPrototypeOf")]
	public extern static bool SetPrototypeOf(object target, object? proto);

	/// <summary>
	/// Returns whether new properties can still be added to the target.
	/// 返回是否仍可向目标添加新属性。
	/// </summary>
	[Description("@#isExtensible")]
	public extern static bool IsExtensible(object target);

	/// <summary>
	/// Prevents extensions on the target and returns the JavaScript boolean success result.
	/// 阻止目标扩展并返回 JavaScript 布尔成功结果。
	/// </summary>
	[Description("@#preventExtensions")]
	public extern static bool PreventExtensions(object target);
}
