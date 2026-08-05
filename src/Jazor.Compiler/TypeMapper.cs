// File: TypeMapper.cs
// Purpose: Classifies supported C# runtime carriers into JavaScript representation families.
// 映射只指导 usage-site lowering；它不是完整 CLR type system，也不自动授权外部成员访问。
namespace Jazor.Compiler;

/// <summary>
/// 表示 C# 类型在 JavaScript 运行时中的目标表示类别。
/// </summary>
/// <remarks>
/// 这是编译期映射结果，不是运行时类型检查器。Undefined、Null 等值类别描述表达式结果，
/// Class/Unknown 则描述需要继续使用宿主或普通 lowering 的类型形状，不能直接当成 JS 构造器名。
/// </remarks>
public enum TypeMapper
{
    Undefined,
    Null,
    Object,
    String,
    Boolean,
    Number,
    Date,
    BigInt,
    Array,
    Map,
    Set,
    Class,
    Unknown
}
