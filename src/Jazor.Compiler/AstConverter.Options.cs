using Microsoft.CodeAnalysis;

namespace Jazor.Compiler;

/// <summary>
/// 配置一次模块或 runtime class AST 转换的行为。
/// </summary>
/// <remarks>
/// Profile 决定转换契约；MemberFilter、DeclaredNames 和 Host 分别控制成员选择、稳定导出名
/// 和宿主投影。配置应在转换开始前确定，转换过程中不要修改其引用对象，以保持输出确定性。
/// </remarks>
public sealed record AstConverterOptions(
    AstConverterProfile Profile,
    Func<ISymbol, bool>? MemberFilter = null,
    IReadOnlyDictionary<ISymbol, string>? DeclaredNames = null,
    SemanticWalkerHost? Host = null)
{
    public static AstConverterOptions Default { get; } = new(AstConverterProfile.Standard);
}

/// <summary>
/// 标识 AST 转换所处的产品语义 profile。
/// </summary>
/// <remarks>
/// profile 不是普通优化开关。不同 profile 可能拥有不同的成员边界和宿主协议，新增值时必须
/// 同时明确其输入契约、允许的 runtime 形状以及对应测试覆盖。
/// </remarks>
public enum AstConverterProfile
{
    Standard = 0,
    ClrRuntime = 1,
    RazorVueRuntime = 2
}
