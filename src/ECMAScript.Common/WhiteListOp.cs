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
	/// 特殊处理，取空值
	/// </summary>
	Empty,

	/// <summary>
	/// 特殊处理，转字符串
	/// </summary>
	ToString,

	/// <summary>
	/// 特殊处理，判断相等
	/// </summary>
	Equals,

	/// <summary>
	/// 特殊处理，比较大小
	/// </summary>
	CompareTo,

	/// <summary>
	/// 特殊处理，typeof
	/// </summary>
	GetType,

	/// <summary>
	/// 特殊处理，强转
	/// </summary>
	Convert,

	/// <summary>
	/// 特殊处理，创建BigInt
	/// </summary>
	BigIntNew,
	BigIntZero,
	BigIntOne,
	BigIntMinusOne,
}
