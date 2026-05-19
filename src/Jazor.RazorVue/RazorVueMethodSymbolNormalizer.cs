using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue;

internal static class RazorVueMethodSymbolNormalizer
{
    public static IMethodSymbol GetCanonicalMethod(IMethodSymbol method)
        => method.OriginalDefinition;

    public static IParameterSymbol NormalizeParameter(IMethodSymbol method, IParameterSymbol parameter)
    {
        var canonicalMethod = GetCanonicalMethod(method);
        var ordinal = parameter.Ordinal;
        return ordinal >= 0 && ordinal < canonicalMethod.Parameters.Length
            ? canonicalMethod.Parameters[ordinal]
            : parameter;
    }
}
