namespace ECMAScript.Common;

public enum WhiteListOp
{
	/// <summary>
	/// 不支持，丢弃
	/// </summary>
	Discard = 0,
	/// <summary>
	/// 支持，无其他操作
	/// </summary>
	Allowed = 1,
	/// <summary>
	/// 支持，使用变量
	/// </summary>
	Literal = 2,
	/// <summary>
	/// 支持，替换名称
	/// </summary>
	Replace = 3,
}
