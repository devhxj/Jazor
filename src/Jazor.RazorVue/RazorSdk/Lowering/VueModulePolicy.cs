using Jazor.Compiler;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Projects a Razor component hierarchy into one Vue artifact module without adding a
/// RazorVue-specific mode to the core converter.
/// 它定义模块成员枚举、命名及嵌套运行时类 flatten 策略，核心 AstConverter 不感知 Vue。
/// </summary>
internal sealed class VueModulePolicy : AstConverterModulePolicy
{
    public static VueModulePolicy Instance { get; } = new();

    private VueModulePolicy()
    {
    }

    public override IEnumerable<INamedTypeSymbol> EnumerateModuleTypes(INamedTypeSymbol moduleType)
    {
        var hierarchy = new Stack<INamedTypeSymbol>();
        for (var current = moduleType;
             current is { SpecialType: not SpecialType.System_Object };
             current = current.BaseType)
        {
            hierarchy.Push(current);
        }

        while (hierarchy.Count > 0)
            yield return hierarchy.Pop();
    }

    public override bool ShouldFlattenNestedRuntimeClass(
        INamedTypeSymbol moduleType,
        INamedTypeSymbol containingRuntimeClass,
        INamedTypeSymbol nestedRuntimeClass)
        => true;

    public override string? GetPreferredModuleDeclaredName(ISymbol symbol)
    {
        if (symbol is not IMethodSymbol { MethodKind: MethodKind.PropertyGet } getter)
            return null;

        // Roslyn binds every property getter with its associated property symbol.
        return Util.GetConfigOrSymbolName((IPropertySymbol)getter.AssociatedSymbol!);
    }

    public override bool ShouldExportModuleMember(
        INamedTypeSymbol moduleType,
        ISymbol member)
    {
        if (member.ContainingType is null ||
            SymbolEqualityComparer.Default.Equals(
                member.ContainingType.OriginalDefinition,
                moduleType.OriginalDefinition) ||
            !ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(moduleType, member.ContainingType))
        {
            return true;
        }

        var exportName = GetModuleExportName(member);
        // The preceding hierarchy predicate establishes that member.ContainingType occurs in
        // this chain, so the loop reaches it before BaseType can become null.
        for (var current = moduleType;
             !SymbolEqualityComparer.Default.Equals(
                 current.OriginalDefinition,
                 member.ContainingType.OriginalDefinition);
             current = current.BaseType!)
        {
            if (current.GetMembers().Any(candidate =>
                    IsPublicModuleSurfaceMember(candidate) &&
                    string.Equals(GetModuleExportName(candidate), exportName, StringComparison.Ordinal)))
            {
                // Keep the inherited declaration local so base.Method() can still bind to it,
                // while the most-derived declaration owns the single ES module export name.
                // 继承链中的同名成员共享 CLR 槽位时，只导出最派生版本，避免 ES export 冲突。
                return false;
            }
        }

        return true;
    }

    public override bool IsAdditionalTopLevelAccessibilityAllowed(Accessibility accessibility)
        => accessibility == Accessibility.Internal;

    private static bool IsPublicModuleSurfaceMember(ISymbol symbol)
        => symbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;

    private static string GetModuleExportName(ISymbol symbol)
        => symbol switch
        {
            IMethodSymbol { AssociatedSymbol: IPropertySymbol property } =>
                Util.GetConfigOrSymbolName(property),
            _ => Util.GetConfigOrSymbolName(symbol)
        };
}
