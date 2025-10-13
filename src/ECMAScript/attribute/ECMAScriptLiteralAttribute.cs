namespace ECMAScript;

/// <summary>
/// 用于标记方法内联 ECMAScript 原生字面量
/// </summary>
/// <param name="code">目标方法映射的 ECMAScript 原生代码</param>
/// <param name="inject">是否注入参数</param>
/// <param name="imports">原生方法代码中的带入模块</param>
[AttributeUsage(AttributeTargets.Method|AttributeTargets.Property, Inherited = false)]
public sealed class ECMAScriptLiteralAttribute(string code, bool inject = true, string[]? imports = null) : Attribute
{
	/// <summary>
	/// 目标方法映射的 ECMAScript 原生代码
	/// </summary>
	public string Code { get; } = code;

	public bool Inject { get; } = inject;

	public string[]? Imports { get; } = imports;
}
