namespace ECMAScript;

[ECMAScript]
[Description("@#")]
public class PropertyDescriptor
{
	/// <summary>
	/// Whether the property descriptor can be changed and the property can be deleted.
	/// </summary>
	[Description("@#configurable")]
	public bool? Configurable { get; set; }

	/// <summary>
	/// Whether the property shows up during enumeration.
	/// </summary>
	[Description("@#enumerable")]
	public bool? Enumerable { get; set; }

	/// <summary>
	/// The value for a data descriptor.
	/// </summary>
	[Description("@#value")]
	public object? Value { get; set; }

	/// <summary>
	/// Whether the value of a data descriptor can be reassigned.
	/// </summary>
	[Description("@#writable")]
	public bool? Writable { get; set; }

	/// <summary>
	/// Getter function for an accessor descriptor.
	/// This is modeled as a property because JavaScript descriptors expose <c>get</c>
	/// as a function-valued field rather than as an invocable method on the descriptor itself.
	/// </summary>
	[Description("@#get")]
	public Func<object?>? Get { get; set; }

	/// <summary>
	/// Setter function for an accessor descriptor.
	/// This is modeled as a property because JavaScript descriptors expose <c>set</c>
	/// as a function-valued field rather than as an invocable method on the descriptor itself.
	/// </summary>
	[Description("@#set")]
	public Action<object?>? Set { get; set; }
}

[ECMAScript]
public abstract class PropertyDescriptorMap
{
	public extern PropertyDescriptor this[string key] { get; set; }
}
