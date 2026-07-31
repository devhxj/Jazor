using Jazor.Compiler;
using Microsoft.CodeAnalysis;

namespace Jazor.ComplierTest;

internal sealed class InheritedModuleProjectionPolicy : AstConverterModulePolicy
{
    public static InheritedModuleProjectionPolicy Instance { get; } = new();

    private InheritedModuleProjectionPolicy()
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

    public override bool IsAdditionalTopLevelAccessibilityAllowed(Accessibility accessibility)
        => accessibility == Accessibility.Internal;
}

internal sealed class ConfiguredPropertyGetterModulePolicy : AstConverterModulePolicy
{
    public static ConfiguredPropertyGetterModulePolicy Instance { get; } = new();

    private ConfiguredPropertyGetterModulePolicy()
    {
    }

    public override string? GetPreferredModuleDeclaredName(ISymbol symbol)
        => symbol is IMethodSymbol
        {
            MethodKind: MethodKind.PropertyGet,
            AssociatedSymbol: IPropertySymbol property
        }
            ? Util.GetConfigOrSymbolName(property)
            : null;
}

internal sealed class FlattenNestedRuntimeClassModulePolicy : AstConverterModulePolicy
{
    public static FlattenNestedRuntimeClassModulePolicy Instance { get; } = new();

    private FlattenNestedRuntimeClassModulePolicy()
    {
    }

    public override bool ShouldFlattenNestedRuntimeClass(
        INamedTypeSymbol moduleType,
        INamedTypeSymbol containingRuntimeClass,
        INamedTypeSymbol nestedRuntimeClass)
        => true;
}
