namespace ECMAScript.Contract;

/// <summary>
/// 声明当前字符串数组成员在未显式提供时，应从第一个泛型类型实参的公共实例属性名自动推导。
///
/// contract 负责声明“这个成员可以被推导”，编译器只负责按该规则补写对象字面量，
/// 而不关心具体是不是 Vue、是不是 props。
/// </summary>
internal sealed class PropsAttribute : RecordLiteralContractAttribute
{
	public PropsAttribute()
		: base(RecordLiteralContractKind.Props)
	{
	}
}
