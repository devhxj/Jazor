namespace ECMAScript;

[ECMAScript]
[SpecialCompile]
public sealed class Box<T>(T? value = default)
{
	[Description("@#value")]
	public T? Value { get; set; } = value;
}
