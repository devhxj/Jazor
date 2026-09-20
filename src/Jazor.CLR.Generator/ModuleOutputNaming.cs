namespace Jazor.CLR.Generator;

/// <summary>
/// Provides deterministic source-file and runtime-module identities for CLR scaffolds.
/// </summary>
/// <remarks>
/// CLR metadata names omit generic arity after the backtick. The output identity must retain
/// that arity because both <c>Foo</c> and <c>Foo&lt;T&gt;</c> can be selected in one generator run.
/// </remarks>
public static class ModuleOutputNaming
{
    public static string GetModuleName(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var metadataName = type.Name;
        var genericMarker = metadataName.IndexOf('`');
        var simpleName = genericMarker >= 0
            ? metadataName[..genericMarker]
            : metadataName;
        var genericArity = type.IsGenericType
            ? type.GetGenericArguments().Length
            : 0;

        return genericArity == 0
            ? $"{simpleName}Module"
            : $"{simpleName}T{genericArity}Module";
    }

    /// <summary>
    /// 返回 carrier 在项目源码树中的模块路径。
    ///
    /// 自有源码 carrier 写在 <c>clr/</c> 下（与 [ECMAScriptModule] 声明和 ClrRuntimeCatalog 一致），
    /// 因此这里带上同一前缀；逻辑路径（不含前缀）只用于产物图内部的稳定身份。
    /// </summary>
    public static string GetModulePath(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var namespacePath = type.Namespace?.Replace('.', '/') ?? string.Empty;
        var moduleName = GetModuleName(type);
        return string.IsNullOrEmpty(namespacePath)
            ? $"{CarrierRoot}{moduleName}.js"
            : $"{CarrierRoot}{namespacePath}/{moduleName}.js";
    }

    /// <summary>carrier 在项目源码树中的根目录。</summary>
    public const string CarrierRoot = "clr/";
}
