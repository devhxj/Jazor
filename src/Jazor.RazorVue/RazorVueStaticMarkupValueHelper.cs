using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue;

internal static class RazorVueStaticMarkupValueHelper
{
    public static string? TryGetStaticMarkupValue(IOperation? operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return null;

        if (TryGetConstantString(current) is string directConstant)
            return directConstant;

        if (TryGetMarkupStringCtorLiteral(current) is string constructorLiteral)
            return constructorLiteral;

        if (TryGetMarkupStringExplicitCastLiteral(current) is string explicitCastLiteral)
            return explicitCastLiteral;

        return null;
    }

    public static string? TryGetStaticMarkupValue(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver)
    {
        return TryGetStaticMarkupValue(
            operation,
            compilation,
            localInitializerResolver,
            propertyInitializerResolver,
            fieldInitializerResolver,
            visitedLocals: new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default),
            visitedMembers: new HashSet<ISymbol>(SymbolEqualityComparer.Default));
    }

    public static bool IsMarkupStringType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return false;

        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.IsGenericType &&
            namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
        {
            typeSymbol = namedType.TypeArguments[0];
        }

        return string.Equals(
            typeSymbol.ToDisplayString(),
            "Microsoft.AspNetCore.Components.MarkupString",
            StringComparison.Ordinal);
    }

    private static string? TryGetMarkupStringCtorLiteral(IOperation operation)
    {
        if (operation is not IObjectCreationOperation objectCreation ||
            !IsMarkupStringType(objectCreation.Type) ||
            objectCreation.Arguments.Length != 1)
        {
            return null;
        }

        return TryGetConstantString(objectCreation.Arguments[0].Value);
    }

    private static string? TryGetMarkupStringExplicitCastLiteral(IOperation operation)
    {
        if (operation is not IConversionOperation conversion ||
            conversion.IsImplicit ||
            !IsMarkupStringType(conversion.Type))
        {
            return null;
        }

        return TryGetConstantString(conversion.Operand);
    }

    private static string? TryGetConstantString(IOperation? operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current?.ConstantValue.HasValue == true &&
            current.ConstantValue.Value is string text)
        {
            return text;
        }

        return null;
    }

    private static string? TryGetStaticMarkupValue(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver,
        HashSet<ILocalSymbol> visitedLocals,
        HashSet<ISymbol> visitedMembers)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return null;

        if (TryGetStaticMarkupValue(current) is string directMarkup)
            return directMarkup;

        switch (current)
        {
            case ILocalReferenceOperation localReference:
                if (localInitializerResolver is null || !visitedLocals.Add(localReference.Local))
                    return null;

                return TryGetStaticMarkupValue(
                    localInitializerResolver(localReference.Local),
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    visitedLocals,
                    visitedMembers);

            case IPropertyReferenceOperation propertyReference:
                if (propertyInitializerResolver is null ||
                    propertyReference.Property.SetMethod is not null ||
                    !visitedMembers.Add(propertyReference.Property))
                {
                    return null;
                }

                return TryGetStaticMarkupValue(
                    propertyInitializerResolver(propertyReference.Property),
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    visitedLocals,
                    visitedMembers);

            case IFieldReferenceOperation fieldReference:
                if (fieldInitializerResolver is null ||
                    !fieldReference.Field.IsReadOnly ||
                    !visitedMembers.Add(fieldReference.Field))
                {
                    return null;
                }

                return TryGetStaticMarkupValue(
                    fieldInitializerResolver(fieldReference.Field),
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    visitedLocals,
                    visitedMembers);

            default:
                return null;
        }
    }
}
