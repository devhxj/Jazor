namespace ECMAScript;

/// <summary>
/// 用于标记方法内联 ECMAScript 原生字面量
/// </summary>
/// <param name="code">目标方法映射的 ECMAScript 原生代码</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
public sealed class ECMAScriptLiteralAttribute(string code) : Attribute
{
	/// <summary>
	/// 目标方法映射的 ECMAScript 原生代码
	/// </summary>
	public string Code { get; } = code;
}
