namespace Jazor.Common;

/// <summary>
/// 标记编译器特殊处理成员
/// </summary>
/// <param name="member">使用 ECMAScript.Common.Util.NameFormat 格式化后的成员名称（类名或方法名）</param>
/// <param name="op">处理方式</param>
/// <param name="value">op是Alias时，指定替换值</param>
[AttributeUsage(
	AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method,
	AllowMultiple = false,
	Inherited = false)]
internal sealed class JazorAttribute : Attribute
{
	public Op Op { get; }

	public string Member { get; }

	public string? Value { get; }

	/// <summary>
	/// 无参构造函数：指定需要Jazor编译器进行特殊处理
	/// </summary>
	public JazorAttribute()
	{
		Op = Op.Compile;
		Member = string.Empty;
		Value = null;
	}

	/// <summary>
	/// 1个字符串参数构造函数：指定为内联代码调用
	/// </summary>
	public JazorAttribute(string value)
	{
		Op = Op.Inline;
		Member = string.Empty;
		Value = value;
	}

	/// <summary>
	/// 2或3个参数构造函数：详细指定
	/// </summary>
	public JazorAttribute(Op op, string member, string? value = null)
	{
		Op = op;
		Member = member;
		Value = value;
	}
}