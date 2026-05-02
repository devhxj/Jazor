namespace ECMAScript;

/// <summary>
/// Marks an extern method as an ECMAScript inline template.
/// During compilation, invocation sites are lowered with the provided JavaScript expression template,
/// using <c>__arg1</c>, <c>__arg2</c>, ... placeholders (same contract as <c>Op.Inline</c>).
/// </summary>
/// <param name="rawFuncCode">JavaScript expression template with <c>__arg1</c>, <c>__arg2</c>, ... placeholders.</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ECMAScriptInlineAttribute(string rawFuncCode) : Attribute
{
	public string RawFuncCode { get; } = rawFuncCode;
}
