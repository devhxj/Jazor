namespace ECMAScript.Contract;

/// <summary>
/// 为类型或成员声明 Jazor 白名单映射规则。
///
/// 这个特性是 producer 侧事实来源：
/// - Generator 扫描它生成白名单与 Compile 分发表
/// - Analyzer 用生成结果决定“成员是否允许进入编译域”
/// - Compiler 再按对应 Op 消费
///
/// 注意：
/// - 这里声明的是“该成员应该如何暴露给编译器”，不是“最终 JS 一定长什么样”
/// - generated 白名单只能通过 Generator 刷新，不应手改生成文件绕过这里
/// </summary>
/// <param name="member">使用 Jazor.Common 格式化后的完整成员签名，例如 <c>string.Length.get</c> 或 <c>static bool.Parse(string)</c></param>
/// <param name="op">成员处理方式，见 <see cref="Op"/></param>
/// <param name="value">附加值：Alias 时通常是 JS 名称，Inline 时通常是表达式模板，Import 时是显式 runtime export 名称</param>
/// <param name="modulePath">Import 的可选 ESM module specifier；未提供时沿用声明类型的模块路径。</param>
[AttributeUsage(
	AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method,
	AllowMultiple = false,
	Inherited = false)]
internal sealed class JazorAttribute : Attribute
{
	public Op Op { get; }

	public string Member { get; }

	public string? Value { get; }

	/// <summary>Import mapping's explicit ESM module specifier.</summary>
	public string? ModulePath { get; }

	/// <summary>
	/// 无参形式，等价于声明 <see cref="Op.Compile"/>。
	/// 只应在编译器内部明确拥有 Compile_* 消费逻辑时使用。
	/// 不要把它当成“以后再说”的占位写法。
	/// </summary>
	public JazorAttribute()
	{
		Op = Op.Compile;
		Member = string.Empty;
		Value = null;
		ModulePath = null;
	}

	/// <summary>
	/// 单字符串形式，等价于声明 <see cref="Op.Inline"/>。
	/// 主要给 ECMAScript 核心库使用；Jazor.CLR 一般应显式写出 member。
	/// 这里的字符串是表达式模板，不是任意 JS 语句块。
	/// </summary>
	public JazorAttribute(string value)
	{
		Op = Op.Inline;
		Member = string.Empty;
		Value = value;
		ModulePath = null;
	}

	/// <summary>
	/// 完整形式，显式指定 producer 侧 Op、成员签名和附加值。
	/// Jazor.CLR 中应优先使用这个构造器，让生成器输入保持明确和可审查。
	/// </summary>
	public JazorAttribute(Op op, string member, string? value = null, string? modulePath = null)
	{
		Op = op;
		Member = member;
		Value = value;
		ModulePath = modulePath;
	}
}
