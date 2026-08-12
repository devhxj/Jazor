namespace ECMAScript;

/// <summary>
/// Marks an extern method as an ECMAScript inline template.
/// During compilation, invocation sites are lowered with the provided JavaScript expression template,
/// using <c>__arg1</c>, <c>__arg2</c>, ... placeholders (same contract as <c>Op.Inline</c>).
/// 标记 extern 方法为 ECMAScript 内联模板。编译时调用点会使用提供的 JavaScript 表达式模板
/// （占位符为 <c>__arg1</c>、<c>__arg2</c> 等，与 <c>Op.Inline</c> 契约一致）进行 lowering。
/// </summary>
/// <param name="rawFuncCode">JavaScript expression template with <c>__arg1</c>, <c>__arg2</c>, ... placeholders.</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ECMAScriptInlineAttribute(string rawFuncCode) : Attribute
{
	/// <summary>Gets the JavaScript expression template used at each lowered call site. 获取每个 lowering 调用点使用的 JavaScript 表达式模板。</summary>
	public string RawFuncCode { get; } = rawFuncCode;
}
