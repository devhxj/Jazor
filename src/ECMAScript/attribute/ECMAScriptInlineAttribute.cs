namespace ECMAScript;

/// <summary>
/// Marks a method as an ECMAScript inline method. The method will be replaced with the provided code during compilation.
/// </summary>
/// <param name="rawFuncCode">必须是方法代码如 function myMethod() { ... }，参数占位符是@#{0}..@#{n}</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ECMAScriptInlineAttribute(string rawFuncCode) : Attribute
{
	public string RawFuncCode { get; } = rawFuncCode;
}
