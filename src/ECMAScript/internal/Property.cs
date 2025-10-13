namespace ECMAScript;

[ECMAScript]
public abstract class PropertyDescriptor
{
	[DisplayName("configurable")]
	public bool? Configurable { get; set; } = null!;

	[DisplayName("enumerable")]
	public bool? Enumerable { get; set; } = null!;

	[DisplayName("value")]
	public object? Value { get; set; } = null!;

	[DisplayName("writable")]
	public bool? Writable { get; set; } = null!;

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
