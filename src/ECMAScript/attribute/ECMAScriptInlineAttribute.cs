namespace ECMAScript;

/// <summary>
/// 用于标记方法内联 ECMAScript 原生方法
/// </summary>
/// <param name="code">目标方法映射的 ECMAScript 原生方法代码</param>
/// <param name="imports">原生方法代码中的带入模块</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ECMAScriptInlineAttribute : Attribute
{
	/// <summary>
	/// 目标方法映射的 ECMAScript 原生方法代码
	/// </summary>
	public string Code { get; }

	/// <summary>
	/// 目标方法映射的 ECMAScript 原生方法代码
	/// </summary>
	public bool Inject { get; }

	/// <summary>
	/// 目标方法映射的 ECMAScript 原生方法代码
	/// </summary>
	public string[]? Imports { get; }

	public ECMAScriptInlineAttribute(string code)
	{
		Code = code;
	}

	public ECMAScriptInlineAttribute(string code,bool inject)
	{
		Code = code;
		Inject = inject;
	}

	public ECMAScriptInlineAttribute(string code, string[] imports)
	{
		Code = code;
		Imports = imports;
	}

	public ECMAScriptInlineAttribute(string code, bool inject, string[] imports)
	{
		Code = code;
		Inject = inject;
		Imports = imports;
	}
}
