namespace ECMAScript;

[ECMAScript]
public class PropertyDescriptor
{
	[DisplayName("configurable")]
	public bool? Configurable { get; set; }

	[DisplayName("enumerable")]
	public bool? Enumerable { get; set; }

	[DisplayName("value")]
	public object? Value { get; set; }

	[DisplayName("writable")]
	public bool? Writable { get; set; }

	[DisplayName("get")]
	public extern object? Get();

	[DisplayName("set")]
	public extern void Set(object v);
}

[ECMAScript]
public abstract class PropertyDescriptorMap
{
	public extern PropertyDescriptor this[string key] { get; set; }
}
