using System;

namespace ECMAScript;

/// <summary>
/// 用于标记类被 Jazor编译器识别可用
/// 一般用于引用第三方dll中的DTO对象
/// </summary>
/// <param name="classFullNamePrefix">完整类名的前缀</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ECMAScriptIncludeAttribute(string classFullNamePrefix) : Attribute
{
	/// <summary>
	/// 未标记为Jazor类的完整类名前缀
	/// </summary>
	public string ClassFullNamePrefix { get; } = classFullNamePrefix;
}
