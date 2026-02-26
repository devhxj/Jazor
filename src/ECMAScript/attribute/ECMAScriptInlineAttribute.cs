namespace ECMAScript;

/// <summary>
/// Marks a method as an ECMAScript inline method. The method will be replaced with the provided code during compilation.
/// </summary>
/// <param name="code"></param>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ECMAScriptInlineAttribute(string code) : Attribute
{
	public string Code { get; } = code;
}
