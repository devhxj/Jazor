using System.Runtime.Versioning;

namespace ECMAScript;

/// <summary>
/// Marks an enum as a JavaScript string domain instead of a numeric enum domain.
/// 标记枚举应按 JavaScript 字符串值域绑定，而不是数值枚举值域。
/// </summary>
/// <remarks>
/// This attribute supplies compiler/generator metadata and does not create a runtime enum object.
/// 该属性提供编译期/生成器元数据，不创建 runtime enum object；枚举成员最终按 contract 规则映射为字符串值。
/// </remarks>
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
[SupportedOSPlatform("browser")]
public sealed class StringAttribute : Attribute
{
}
