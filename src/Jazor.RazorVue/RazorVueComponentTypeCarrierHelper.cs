using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue;

internal static class RazorVueComponentTypeCarrierHelper
{
    private static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;

    public static bool IsSystemType(ITypeSymbol? typeSymbol)
        => string.Equals(
            typeSymbol?.ToDisplayString(),
            "System.Type",
            StringComparison.Ordinal);

    public static bool IsVueComponentType(Compilation compilation, INamedTypeSymbol typeSymbol)
    {
        var vueComponentType = compilation.GetTypeByMetadataName("ECMAScript.Vue3+IVueComponent");
        if (vueComponentType is null)
            return false;

        return typeSymbol.AllInterfaces.Any(candidate => Comparer.Equals(candidate.OriginalDefinition, vueComponentType));
    }

    public static bool TryResolveSourceStableVueComponentTypeLocal(
        Compilation compilation,
        ILocalSymbol local,
        out INamedTypeSymbol componentType)
        => TryResolveSourceStableVueComponentTypeLocal(
            compilation,
            componentSymbol: null,
            local,
            out componentType);

    public static bool TryResolveSourceStableVueComponentTypeLocal(
        Compilation compilation,
        INamedTypeSymbol? componentSymbol,
        ILocalSymbol local,
        out INamedTypeSymbol componentType)
    {
        componentType = default!;
        if (!IsSystemType(local.Type))
            return false;

        if (!RazorVueSourceStableLocalInitializerHelper.TryGetSourceStableLocalInitializer(
                compilation,
                local,
                IsSystemType,
                out var initializer))
        {
            return false;
        }

        if (!TryResolveComponentTypeCore(
                compilation,
                componentSymbol,
                initializer,
                new HashSet<ILocalSymbol>(Comparer),
                new HashSet<ISymbol>(Comparer),
                out var localComponentType,
                out _,
                out _,
                out _))
        {
            return false;
        }

        if (!IsVueComponentType(compilation, localComponentType))
            return false;

        componentType = localComponentType;
        return true;
    }

    public static bool TryResolveComponentType(
        Compilation compilation,
        IOperation? operation,
        out INamedTypeSymbol componentType,
        out ITypeOfOperation? typeOfOperation)
        => TryResolveComponentType(
            compilation,
            componentSymbol: null,
            operation,
            out componentType,
            out typeOfOperation,
            out _);

    public static bool TryResolveComponentType(
        Compilation compilation,
        INamedTypeSymbol? componentSymbol,
        IOperation? operation,
        out INamedTypeSymbol componentType,
        out ITypeOfOperation? typeOfOperation)
        => TryResolveComponentType(
            compilation,
            componentSymbol,
            operation,
            out componentType,
            out typeOfOperation,
            out _);

    public static bool TryResolveComponentType(
        Compilation compilation,
        INamedTypeSymbol? componentSymbol,
        IOperation? operation,
        out INamedTypeSymbol componentType,
        out ITypeOfOperation? typeOfOperation,
        out ISymbol? memberCarrier)
    {
        componentType = default!;
        typeOfOperation = null;
        memberCarrier = null;

        return TryResolveComponentTypeCore(
            compilation,
            componentSymbol,
            operation,
            new HashSet<ILocalSymbol>(Comparer),
            new HashSet<ISymbol>(Comparer),
            out componentType,
            out typeOfOperation,
            out memberCarrier,
            out _);
    }

    private static bool TryResolveComponentTypeCore(
        Compilation compilation,
        INamedTypeSymbol? componentSymbol,
        IOperation? operation,
        HashSet<ILocalSymbol> visitedLocals,
        HashSet<ISymbol> visitedMembers,
        out INamedTypeSymbol componentType,
        out ITypeOfOperation? typeOfOperation,
        out ISymbol? memberCarrier,
        out ISymbol? invalidatedMemberCarrier)
    {
        componentType = default!;
        typeOfOperation = null;
        memberCarrier = null;
        invalidatedMemberCarrier = null;

        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is ITypeOfOperation { TypeOperand: INamedTypeSymbol directComponentType } directTypeOf)
        {
            componentType = directComponentType;
            typeOfOperation = directTypeOf;
            return true;
        }

        if (current is ILocalReferenceOperation localReference &&
            IsSystemType(localReference.Local.Type))
        {
            if (!visitedLocals.Add(localReference.Local))
                return false;

            if (!RazorVueSourceStableLocalInitializerHelper.TryGetSourceStableLocalInitializer(
                    compilation,
                    localReference.Local,
                    IsSystemType,
                    out var initializer))
            {
                return false;
            }

            return TryResolveComponentTypeCore(
                compilation,
                componentSymbol,
                initializer,
                visitedLocals,
                visitedMembers,
                out componentType,
                out typeOfOperation,
                out memberCarrier,
                out invalidatedMemberCarrier);
        }

        if (TryGetCurrentComponentSystemTypeMemberReference(
                componentSymbol,
                current,
                out var member,
                out var memberType) &&
            IsSystemType(memberType))
        {
            if (!visitedMembers.Add(member))
                return false;

            if (!IsPotentialSourceStableSystemTypeMember(member))
                return false;

            if (CanUseSourceStableMember(member) &&
                RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(compilation, member))
            {
                invalidatedMemberCarrier = member;
                return false;
            }

            if (TryGetCurrentComponentTypeMemberInitializer(compilation, member) is not { } memberInitializer)
                return false;

            if (!TryResolveComponentTypeCore(
                    compilation,
                    componentSymbol,
                    memberInitializer,
                    visitedLocals,
                    visitedMembers,
                    out componentType,
                    out typeOfOperation,
                    out _,
                    out invalidatedMemberCarrier))
            {
                return false;
            }

            memberCarrier = member;
            return true;
        }

        return false;
    }

    public static bool TryResolveSourceStableVueComponentTypeMember(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IOperation? operation,
        out ISymbol memberCarrier,
        out INamedTypeSymbol componentType)
    {
        memberCarrier = default!;
        componentType = default!;
        if (!TryResolveComponentType(
                compilation,
                componentSymbol,
                operation,
                out var resolvedComponentType,
                out _,
                out var resolvedMemberCarrier) ||
            resolvedMemberCarrier is null ||
            !IsVueComponentType(compilation, resolvedComponentType))
        {
            return false;
        }

        memberCarrier = resolvedMemberCarrier;
        componentType = resolvedComponentType;
        return true;
    }

    public static bool TryGetInvalidatedSourceStableComponentTypeMember(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IOperation? operation,
        out ISymbol memberCarrier)
    {
        memberCarrier = default!;
        if (TryResolveComponentTypeCore(
                compilation,
                componentSymbol,
                operation,
                new HashSet<ILocalSymbol>(Comparer),
                new HashSet<ISymbol>(Comparer),
                out _,
                out _,
                out _,
                out var invalidatedMember) ||
            invalidatedMember is null)
        {
            return false;
        }

        memberCarrier = invalidatedMember;
        return true;
    }

    private static bool TryGetCurrentComponentSystemTypeMemberReference(
        INamedTypeSymbol? componentSymbol,
        IOperation? operation,
        out ISymbol member,
        out ITypeSymbol? memberType)
    {
        member = default!;
        memberType = null;
        if (componentSymbol is null)
            return false;

        switch (RazorVueOperationNormalizer.Unwrap(operation))
        {
            case IPropertyReferenceOperation propertyReference
                when RazorVueSymbolIdentity.IsCurrentComponentMember(
                    componentSymbol,
                    propertyReference.Property,
                    propertyReference.Instance,
                    RazorVueOperationNormalizer.Unwrap):
                member = propertyReference.Property;
                memberType = propertyReference.Property.Type;
                return true;
            case IFieldReferenceOperation fieldReference
                when RazorVueSymbolIdentity.IsCurrentComponentMember(
                    componentSymbol,
                    fieldReference.Field,
                    fieldReference.Instance,
                    RazorVueOperationNormalizer.Unwrap):
                member = fieldReference.Field;
                memberType = fieldReference.Field.Type;
                return true;
            default:
                return false;
        }
    }

    private static IOperation? TryGetCurrentComponentTypeMemberInitializer(
        Compilation compilation,
        ISymbol member)
        => member switch
        {
            IPropertySymbol property => TryGetPropertyTypeInitializer(compilation, property),
            IFieldSymbol field => TryGetFieldTypeInitializer(compilation, field),
            _ => null
        };

    private static bool IsPotentialSourceStableSystemTypeMember(ISymbol member)
    {
        switch (member)
        {
            case IPropertySymbol property:
                if (property.IsStatic || property.IsIndexer || property.IsImplicitlyDeclared || !IsSystemType(property.Type))
                    return false;

                if (property.SetMethod is null)
                    return true;

                return CanUseSourceStableMember(property);

            case IFieldSymbol field:
                if (field.IsStatic || field.IsImplicitlyDeclared || field.AssociatedSymbol is not null || !IsSystemType(field.Type))
                    return false;

                if (field.IsReadOnly)
                    return true;

                return CanUseSourceStableMember(field);

            default:
                return false;
        }
    }

    private static bool CanUseSourceStableMember(ISymbol member)
        => member switch
        {
            IPropertySymbol property => property.SetMethod is not null &&
                                        RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(property),
            IFieldSymbol field => !field.IsReadOnly &&
                                  RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(field),
            _ => false
        };

    private static IOperation? TryGetPropertyTypeInitializer(Compilation compilation, IPropertySymbol property)
    {
        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;

            var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(
                    semanticModel,
                    declaration,
                    out var operation))
            {
                return operation;
            }
        }

        return null;
    }

    private static IOperation? TryGetFieldTypeInitializer(Compilation compilation, IFieldSymbol field)
    {
        foreach (var syntaxReference in field.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                declarator.Initializer?.Value is null)
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(declarator.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declarator.Initializer.Value,
                    out var operation))
            {
                return operation;
            }
        }

        return null;
    }
}
