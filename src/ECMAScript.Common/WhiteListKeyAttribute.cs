using System;

namespace ECMAScript.Common;

/// <summary>
/// 标记白名单成员
/// </summary>
/// <param name="member">成员</param>
/// <param name="op">处理方式</param>
/// <param name="value">值</param>
[AttributeUsage(
	AttributeTargets.Class |
	AttributeTargets.Constructor |
	//AttributeTargets.Field |
	//AttributeTargets.Property |
	AttributeTargets.Method,
	AllowMultiple = true,
	Inherited = false)]
public sealed class WhiteListAttribute(string member, WhiteListOp op, string? value = null) : Attribute
{
	public string Member { get; } = member;


	public WhiteListOp Op { get; } = op;

	public string? Value { get; } = value;
}