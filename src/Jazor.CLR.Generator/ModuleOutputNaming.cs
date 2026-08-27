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

    public static string GetModulePath(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var namespacePath = type.Namespace?.Replace('.', '/') ?? string.Empty;
        var moduleName = GetModuleName(type);
        return string.IsNullOrEmpty(namespacePath)
            ? $"{moduleName}.js"
            : $"{namespacePath}/{moduleName}.js";
    }
}
