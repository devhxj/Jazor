namespace ECMAScript;

[ECMAScriptModule]
public static class CLRModule
{
	public sealed class OutValue<T>()
	{
		public T? Value { get; set; }
	}

	public sealed class RefValue<T>(T value)
	{
		public T Value { get; set; } = value;
	}
}
