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
        => symbol is IMethodSymbol
        {
            MethodKind: MethodKind.PropertyGet,
            AssociatedSymbol: IPropertySymbol property
        }
            ? Util.GetConfigOrSymbolName(property)
            : null;

    public override bool IsAdditionalTopLevelAccessibilityAllowed(Accessibility accessibility)
        => accessibility == Accessibility.Internal;
}
