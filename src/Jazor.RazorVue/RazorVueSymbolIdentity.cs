using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue;

internal static class RazorVueSymbolIdentity
{
    private static readonly SymbolDisplayFormat ComponentTypeFormat =
        SymbolDisplayFormat.FullyQualifiedFormat;

    public static bool IsCurrentComponentMember(
        INamedTypeSymbol componentSymbol,
        ISymbol symbol,
        IOperation? instance,
        Func<IOperation?, IOperation?> unwrap)
    {
        if (componentSymbol is null)
            throw new ArgumentNullException(nameof(componentSymbol));
        if (symbol is null)
            throw new ArgumentNullException(nameof(symbol));
        if (unwrap is null)
            throw new ArgumentNullException(nameof(unwrap));

        for (var current = componentSymbol; current is not null; current = current.BaseType)
        {
            if (SameType(symbol.ContainingType, current))
            {
                if (IsStaticMember(symbol))
                    return symbol.ContainingType?.Locations.Any(static location => location.IsInSource) == true;

                return instance is null || unwrap(instance) is IInstanceReferenceOperation;
            }
        }

        return false;
    }

    private static bool IsStaticMember(ISymbol symbol)
        => symbol switch
        {
            IMethodSymbol method => method.IsStatic,
            IPropertySymbol property => property.IsStatic,
            IFieldSymbol field => field.IsStatic,
            IEventSymbol @event => @event.IsStatic,
            _ => false
        };

    public static bool SameMember(ISymbol left, ISymbol right)
    {
        if (left is null)
            throw new ArgumentNullException(nameof(left));
        if (right is null)
            throw new ArgumentNullException(nameof(right));

        if (SymbolEqualityComparer.Default.Equals(left, right))
            return true;

        if (!string.Equals(left.Name, right.Name, StringComparison.Ordinal))
            return false;

        if (!SameType(left.ContainingType, right.ContainingType))
            return false;

        return (left, right) switch
        {
            (IMethodSymbol leftMethod, IMethodSymbol rightMethod) => SameSignature(leftMethod, rightMethod),
            (IPropertySymbol leftProperty, IPropertySymbol rightProperty) => SameProperty(leftProperty, rightProperty),
            (IFieldSymbol, IFieldSymbol) => true,
            _ => false
        };
    }

    public static bool SameType(INamedTypeSymbol? left, INamedTypeSymbol? right)
    {
        if (left is null || right is null)
            return false;

        return SymbolEqualityComparer.Default.Equals(left, right) ||
               string.Equals(GetTypeKey(left), GetTypeKey(right), StringComparison.Ordinal);
    }

    private static bool SameSignature(IMethodSymbol left, IMethodSymbol right)
    {
        if (left.Parameters.Length != right.Parameters.Length)
            return false;

        for (var i = 0; i < left.Parameters.Length; i++)
        {
            if (!SameType(left.Parameters[i].Type as INamedTypeSymbol, right.Parameters[i].Type as INamedTypeSymbol) &&
                !string.Equals(GetTypeKey(left.Parameters[i].Type), GetTypeKey(right.Parameters[i].Type), StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SameProperty(IPropertySymbol left, IPropertySymbol right)
    {
        if (left.Parameters.Length != right.Parameters.Length)
            return false;

        for (var i = 0; i < left.Parameters.Length; i++)
        {
            if (!string.Equals(GetTypeKey(left.Parameters[i].Type), GetTypeKey(right.Parameters[i].Type), StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    private static string GetTypeKey(ITypeSymbol? symbol)
    {
        if (symbol is null)
            return string.Empty;

        var original = symbol.OriginalDefinition;
        return original.ToDisplayString(ComponentTypeFormat);
    }
}
