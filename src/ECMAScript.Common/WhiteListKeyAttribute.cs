using System;

namespace ECMAScript.Common;

/// <summary>
/// 标记白名单成员
/// </summary>
/// <param name="key">白名单键</param>
/// <param name="member">白名单成员</param>
/// <param name="value">白名单值</param>
[AttributeUsage(
	AttributeTargets.Class |
	AttributeTargets.Constructor |
	//AttributeTargets.Field |
	//AttributeTargets.Property |
	AttributeTargets.Method,
	AllowMultiple = true,
	Inherited = false)]
public sealed class WhiteListAttribute(string key, string member, WhiteListOp op, string? value = null) : Attribute
{
	public string Key { get; } = key;

	public string Member { get; } = member;

	public WhiteListOp Op { get; } = op;

	public string? Value { get; } = value;
}
