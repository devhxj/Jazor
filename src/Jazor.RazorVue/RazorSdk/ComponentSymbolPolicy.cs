using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Defines which source base types belong to a component's authored member surface.
/// 用于区分可内联进 Vue module 的源码基类成员与必须由 host 处理的外部框架成员。
/// </summary>
internal static class ComponentSymbolPolicy
{
    // Razor components commonly move reusable members to a source base class. Treat that
    // base chain as one component surface while callers retain their own source/protocol checks.
    public static bool IsDeclaredOnComponentHierarchy(
        INamedTypeSymbol componentType,
        INamedTypeSymbol? containingType)
    {
        // The component projection owns authored source members only. ComponentBase and its
        // CLR ancestors are in the inheritance chain too, but treating them as component
        // methods bypasses the normal whitelist route (for example object.ReferenceEquals).
        // 组件投影只接管源码成员；框架/BCL 基类仍必须走 compiler 的 CLR mapping。
        if (containingType is null ||
            !containingType.Locations.Any(static location => location.IsInSource))
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
