using System.Runtime.Versioning;

namespace ECMAScript;

/// <summary>
/// Marks a declaration as an ECMAScript host contract that Jazor can validate and lower.
/// 标记声明为可由 Jazor 校验和 lowering 的 ECMAScript 宿主契约。
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
[SupportedOSPlatform("browser")]
public sealed class ECMAScriptAttribute : Attribute
{
	/// <summary>
/// Gets the ECMAScript module specifier required by the marked host contract.
/// 获取被标记宿主契约所依赖的 ECMAScript 模块 specifier。
	/// 浏览器库使用 manifest 声明的逻辑模块名，例如 vue 或 vue-router；
	/// 包版本与物理资源路径不属于该特性的职责。
	/// </summary>
	public string? Import { get; }

	/// <summary>
/// Marks a declaration backed by the ambient JavaScript runtime instead of an imported module.
/// 标记由环境 JavaScript 运行时提供、而不是由导入模块提供的声明。
	/// </summary>
	public ECMAScriptAttribute()
	{
		Import = null;
	}

	/// <summary>
/// Marks a declaration backed by an imported ECMAScript module.
/// 标记由导入 ECMAScript 模块提供的声明。
	/// </summary>
/// <param name="import">Module specifier preserved in generated JavaScript imports. 将保留到生成 JavaScript import 中的模块 specifier。</param>
	public ECMAScriptAttribute(string import)
	{
		Import = import;
	}
}
