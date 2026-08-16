// File: AstConverter.Options.cs
// Purpose: Defines immutable configuration for AstConverter module conversion.
// 集中表达 profile、成员筛选和 host policy，避免把产品特例写入核心转换流程。
using Jazor.Common;
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
    SemanticWalkerHost? Host = null,
    AstConverterModulePolicy? ModulePolicy = null,
    RuntimeClassPrivateStorage RuntimeClassPrivateStorage = RuntimeClassPrivateStorage.JavaScriptPrivateFields)
{
    public static AstConverterOptions Default { get; } = new(AstConverterProfile.Standard);
}

/// <summary>
/// Selects the JavaScript storage representation for non-public runtime-class fields.
/// <see cref="JavaScriptPrivateFields"/> preserves normal module semantics. Products that place
/// runtime-class instances inside an ES Proxy can opt into a mangled ordinary property because
/// proxy receivers cannot satisfy JavaScript private-field brand checks.
/// </summary>
public enum RuntimeClassPrivateStorage
{
    JavaScriptPrivateFields = 0,
    ProxySafeMangledProperties = 1
}

/// <summary>Shared names for proxy-safe storage. The prefix is outside C# identifier syntax.</summary>
internal static class RuntimeClassPrivateStorageNames
{
    private const string ProxySafePrefix = "$jazor$private$";

    internal static string GetFieldStorageName(
        RuntimeClassPrivateStorage storage,
        IFieldSymbol field,
        string fallbackName)
    {
        if (storage != RuntimeClassPrivateStorage.ProxySafeMangledProperties)
            return fallbackName;

        // Implicit auto-property backing fields have no source declaration. Their compiler
        // lowering already uses the property hash, so recreate that canonical name here rather
        // than depend on a module-name-plan implementation detail.
        var canonicalName = field.AssociatedSymbol is IPropertySymbol property && field.IsImplicitlyDeclared
            ? Format.HashName(property.OriginalDefinition.ToDisplayString(Format.NameFormat))
            : fallbackName;
        return ProxySafePrefix + canonicalName;
    }

    internal static string GetSyntheticStorageName(RuntimeClassPrivateStorage storage, string fallbackName)
        => storage == RuntimeClassPrivateStorage.ProxySafeMangledProperties
            ? ProxySafePrefix + fallbackName
            : fallbackName;
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
    ClrRuntime = 1
}
