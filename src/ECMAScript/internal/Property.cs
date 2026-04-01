namespace ECMAScript;

[ECMAScript]
public class PropertyDescriptor
{
	[Description("@#configurable")]
	public bool? Configurable { get; set; }

	[Description("@#enumerable")]
	public bool? Enumerable { get; set; }

	[Description("@#value")]
	public object? Value { get; set; }

	[Description("@#writable")]
	public bool? Writable { get; set; }

	[Description("@#get")]
	public extern object? Get();

	[Description("@#set")]
	public extern void Set(object v);
}

[ECMAScript]
public abstract class PropertyDescriptorMap
{
	public extern PropertyDescriptor this[string key] { get; set; }
}
