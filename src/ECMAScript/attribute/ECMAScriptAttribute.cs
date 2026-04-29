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
	/// 首选方式是从 JSR 中导入特定版本的 Vue 包，如 jsr:@denovue/create-vue
	/// 如果你需要使用 npm 生态中特定的 Vue 相关库（如 vue-router），可以直接使用 npm: 前缀导入。
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
