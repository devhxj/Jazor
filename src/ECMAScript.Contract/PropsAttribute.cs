namespace ECMAScript.Contract;

/// <summary>
/// 声明当前字符串数组成员在未显式提供时，应从第一个泛型类型实参的公共实例属性名自动推导。
///
/// 当前约定允许通过 <see cref="TypeArgumentIndex"/> 调整来源类型参数位置。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class PropsAttribute : Attribute
{
	internal const int DefaultTypeArgumentIndex = 0;

	public int TypeArgumentIndex { get; set; } = DefaultTypeArgumentIndex;
}
