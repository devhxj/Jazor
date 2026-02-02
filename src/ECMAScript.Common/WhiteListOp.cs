namespace ECMAScript.Common;

public enum WhiteListOp
{
	/// <summary>
	/// 不支持，丢弃
	/// </summary>
	Discard,

	/// <summary>
	/// 支持，无其他操作
	/// </summary>
	Allowed,

	/// <summary>
	/// 支持，替换名称
	/// </summary>
	Replace,

	/// <summary>
	/// 支持，作为模块导入
	/// </summary>
	Import,

	/// <summary>
	/// 特殊处理，判断相等
	/// </summary>
	Equals,

	/// <summary>
	/// 特殊处理，比较大小
	/// </summary>
	CompareTo,
}
