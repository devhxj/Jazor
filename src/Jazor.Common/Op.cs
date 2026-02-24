namespace Jazor.Common;

/// <summary>
/// Jazor编译器处理方式
/// </summary>
internal enum Op
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
	/// 支持，作为模块导入，放置在类上表示模块引用，放在方法上，指定的方法必须有实现
	/// </summary>
	Import,

	/// <summary>
	/// 支持，放在方法上或属性上，指定的字符串表示内联调用的代码
	/// 字符串内支持一个占位符，占位符格式为：@#{0}
	/// </summary>
	Inline,

	/// <summary>
	/// 支持，编译器特殊处理，放置在属性和方法上面，放置在类上表示Allowed
	/// </summary>
	Compile,
}
