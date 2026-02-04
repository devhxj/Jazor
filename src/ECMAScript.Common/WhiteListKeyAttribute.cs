using System;

namespace ECMAScript.Common;

/// <summary>
/// 标记白名单成员
/// </summary>
/// <param name="member">使用 ECMAScript.Common.Util.NameFormat 格式化后的成员名称（类名或方法名）</param>
/// <param name="op">处理方式</param>
/// <param name="value">op是Replace时，指定替换值</param>
/// <param name="path">member是类名时，指定模块路径</param>
[AttributeUsage(
	AttributeTargets.Class |
	AttributeTargets.Constructor |
	AttributeTargets.Method,
	AllowMultiple = false,
	Inherited = false)]
public sealed class WhiteListAttribute(string member, WhiteListOp op, string? value = null, string? path = null) : Attribute
{
	public string Member { get; } = member;

	public WhiteListOp Op { get; } = op;

	public string? Value { get; } = value;

	public string? Path { get; } = path;
}