namespace ECMAScript;

[ECMAScript]
[Description("@#")]
/// <summary>
/// JavaScript own-property descriptor object shape.
/// JavaScript 自身属性描述符对象形状。
/// </summary>
public class JazorPropertyDescriptor
{
	/// <summary>
	/// Gets or sets whether the descriptor can be changed and the property can be deleted.
	/// Gets or sets whether the descriptor can be changed and the property can be deleted.
	/// 获取或设置描述符是否可修改且属性是否可删除。
	/// </summary>
	[Description("@#configurable")]
	public bool? Configurable { get; set; }

	/// <summary>
	/// Gets or sets whether the property shows up during enumeration.
	/// 获取或设置属性是否在枚举中出现。
	/// </summary>
	[Description("@#enumerable")]
	public bool? Enumerable { get; set; }

	/// <summary>
	/// Gets or sets the value for a data descriptor.
	/// This must not be combined with <see cref="Get"/> or <see cref="Set"/> in one JavaScript descriptor.
	/// 获取或设置数据描述符的值；JavaScript 中不可与 <see cref="Get"/> 或 <see cref="Set"/> 同时出现在一个描述符内。
	/// </summary>
	[Description("@#value")]
	public object? Value { get; set; }

	/// <summary>
	/// Gets or sets whether the value of a data descriptor can be reassigned.
	/// 获取或设置数据描述符的值是否可重新赋值。
	/// </summary>
	[Description("@#writable")]
	public bool? Writable { get; set; }

	/// <summary>
	/// Gets or sets the getter function for an accessor descriptor.
	/// This is modeled as a property because JavaScript descriptors expose <c>get</c> as a function-valued field rather than an invocable descriptor method.
	/// 获取或设置访问器描述符的 getter 函数；建模为属性是因为 JavaScript 描述符将 <c>get</c> 作为函数值字段公开，而非描述符自身的可调用方法。
	/// </summary>
	[Description("@#get")]
	public Func<object?>? Get { get; set; }

	/// <summary>
	/// Gets or sets the setter function for an accessor descriptor.
	/// This is modeled as a property because JavaScript descriptors expose <c>set</c> as a function-valued field rather than an invocable descriptor method.
	/// 获取或设置访问器描述符的 setter 函数；建模为属性是因为 JavaScript 描述符将 <c>set</c> 作为函数值字段公开，而非描述符自身的可调用方法。
	/// </summary>
	[Description("@#set")]
	public Action<object?>? Set { get; set; }
}

[ECMAScript]
/// <summary>JavaScript property-keyed descriptor map bridge. JavaScript 属性键描述符映射桥接。</summary>
public abstract class PropertyDescriptorMap
{
	/// <summary>Gets or sets the descriptor for a string property key. 获取或设置字符串属性键的描述符。</summary>
	public extern JazorPropertyDescriptor this[string key] { get; set; }
}
