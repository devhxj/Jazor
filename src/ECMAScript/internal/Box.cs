namespace ECMAScript;

[ECMAScript]
[Jazor]
public sealed class Box<T>(T? value = default)
{
	[Description("@#value")]
	public T? Value { get; set; } = value;
}
