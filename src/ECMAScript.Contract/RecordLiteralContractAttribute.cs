namespace ECMAScript.Contract;

/// <summary>
/// 统一声明 ECMAScript record-like 对象字面量成员的缺省推导契约。
///
/// 这是内部核心模型：
/// - contract 声明成员属于哪一类推导规则
/// - compiler 按 kind 分发具体推导逻辑
///
/// 对声明侧，优先使用更短的 façade 特性，例如 <see cref="PropsAttribute"/>、<see cref="EmitsAttribute"/>。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal class RecordLiteralContractAttribute : Attribute
{
	public RecordLiteralContractKind Kind { get; }

	public RecordLiteralContractAttribute(RecordLiteralContractKind kind)
	{
		Kind = kind;
	}
}

/// <summary>
/// ECMAScript record-like 成员缺省推导类别。
/// </summary>
internal enum RecordLiteralContractKind
{
	Props,
	Emits,
}
