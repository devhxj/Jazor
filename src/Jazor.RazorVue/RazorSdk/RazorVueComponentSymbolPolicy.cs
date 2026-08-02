using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorSdk;

internal static class RazorVueComponentSymbolPolicy
{
    // Razor components commonly move reusable members to a source base class. Treat that
    // base chain as one component surface while callers retain their own source/protocol checks.
    public static bool IsDeclaredOnComponentHierarchy(
        INamedTypeSymbol componentType,
        INamedTypeSymbol? containingType)
    {
        if (containingType is null)
            return false;

        for (var current = componentType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(
                    containingType.OriginalDefinition,
                    current.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }
}
