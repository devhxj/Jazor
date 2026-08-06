using System.Runtime.Versioning;

namespace ECMAScript;

/// <summary>
/// 用于标记类使用 ECMAScript 语法校验
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
[SupportedOSPlatform("browser")]
public sealed class ECMAScriptAttribute : Attribute
{
	/// <summary>
	/// 目标类依赖的 ECMAScript module 文件路径
	/// 浏览器库使用 manifest 声明的逻辑模块名，例如 vue 或 vue-router；
	/// 包版本与物理资源路径不属于该特性的职责。
	/// </summary>
	public string? Import { get; }

	/// <summary>
	/// 指示该类是原生 ECMAScript 支持
	/// </summary>
	public ECMAScriptAttribute()
	{
		Import = null;
	}

	/// <summary>
	/// 指示该类是基于导入的模块
	/// </summary>
	/// <param name="import">该类依赖的 ECMAScript module 文件路径</param>
	public ECMAScriptAttribute(string import)
	{
		Import = import;
	}
}
