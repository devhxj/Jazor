using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Jazor.RazorVue.Descriptor;

namespace Jazor.RazorVue;

internal static class RazorVueCurrentComponentValueMemberHelper
{
    public static bool TryGetSupportedPropertyLoweringKind(
        Compilation compilation,
        IPropertySymbol property,
        out VueLogicPropertyLoweringKind loweringKind)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));
        if (property is null)
            throw new ArgumentNullException(nameof(property));

        if (TryGetValueMemberInitializer(compilation, property, out _) ||
            CanUseMutableSetupCarrierMember(property))
        {
            loweringKind = VueLogicPropertyLoweringKind.ValueBinding;
            return true;
        }

        if (property.SetMethod is not null)
        {
            loweringKind = VueLogicPropertyLoweringKind.Unsupported;
            return false;
        }

        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;

            if (declaration.Initializer?.Value is not null)
                continue;

            if (declaration.ExpressionBody?.Expression is not null)
            {
                loweringKind = VueLogicPropertyLoweringKind.GetterFunction;
                return true;
            }

            var getter = declaration.AccessorList?.Accessors
                .FirstOrDefault(static accessor => accessor.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration));
            if (getter?.ExpressionBody?.Expression is not null)
            {
                loweringKind = VueLogicPropertyLoweringKind.GetterFunction;
                return true;
            }

            if (getter?.Body?.Statements.Count == 1 &&
                getter.Body.Statements[0] is ReturnStatementSyntax { Expression: not null })
            {
                loweringKind = VueLogicPropertyLoweringKind.GetterFunction;
                return true;
            }
        }

        loweringKind = VueLogicPropertyLoweringKind.Unsupported;
        return false;
    }

    public static bool TryGetUnsupportedValueMemberReason(
        Compilation compilation,
        ISymbol member,
        out string reason)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));
        if (member is null)
            throw new ArgumentNullException(nameof(member));

        reason = string.Empty;
        switch (member)
        {
            case IPropertySymbol property when HasDeclarationInitializer(property):
                if (property.SetMethod is not null)
                {
                    if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(property))
                    {
                        reason = "only readonly properties or private mutable properties without later writes are supported for declaration-initialized setup value members";
                        return true;
                    }

                    if (RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(compilation, property))
                    {
                        reason = "declaration-initialized setup value properties cannot be observed through later writes";
                        return true;
                    }
                }

                return false;

            case IFieldSymbol field when HasDeclarationInitializer(field):
                if (!field.IsReadOnly)
                {
                    if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(field))
                    {
                        reason = "only readonly fields or private mutable fields without later writes are supported for declaration-initialized setup value members";
                        return true;
                    }

                    if (RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(compilation, field))
                    {
                        reason = "declaration-initialized setup value fields cannot be observed through later writes";
                        return true;
                    }
                }

                return false;

            default:
                return false;
        }
    }

    public static bool IsSupportedSourceStableValueMember(
        Compilation compilation,
        ISymbol member)
        => TryGetValueMemberInitializer(compilation, member, out _);

    public static bool CanUseMutableSetupCarrierMember(ISymbol member)
        => member switch
        {
            IPropertySymbol property => !property.IsStatic &&
                                        !property.IsIndexer &&
                                        !property.IsImplicitlyDeclared &&
                                        property.SetMethod?.DeclaredAccessibility == Accessibility.Private &&
                                        HasAutoPropertyAccessors(property),
            IFieldSymbol field => !field.IsStatic &&
                                  !field.IsReadOnly &&
                                  !field.IsImplicitlyDeclared &&
                                  field.AssociatedSymbol is null &&
                                  field.DeclaredAccessibility == Accessibility.Private,
            _ => false
        };

    public static bool TryGetUnsupportedMutableSetupCarrierMemberReason(
        ISymbol member,
        out string reason)
    {
        if (member is null)
            throw new ArgumentNullException(nameof(member));

        reason = string.Empty;
        if (CanUseMutableSetupCarrierMember(member))
            return false;

        reason = member switch
        {
            IPropertySymbol property when property.SetMethod?.DeclaredAccessibility != Accessibility.Private =>
                "mutable setup-carrier properties must have a private setter",
            IPropertySymbol => "mutable setup-carrier properties must be auto-properties with a private setter",
            IFieldSymbol => "mutable setup-carrier fields must be private",
            _ => "mutable setup carriers must be private component fields or properties"
        };
        return true;
    }

    private static bool HasAutoPropertyAccessors(IPropertySymbol property)
    {
        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration ||
                declaration.AccessorList is null ||
                declaration.ExpressionBody is not null)
            {
                continue;
            }

            var hasGetter = false;
            var hasSetter = false;
            var allAccessorsAreAuto = true;
            foreach (var accessor in declaration.AccessorList.Accessors)
            {
                if (accessor.Body is not null || accessor.ExpressionBody is not null)
                {
                    allAccessorsAreAuto = false;
                    break;
                }

                if (accessor.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration))
                    hasGetter = true;
                else if (accessor.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SetAccessorDeclaration))
                    hasSetter = true;
            }

            if (hasGetter && hasSetter && allAccessorsAreAuto)
                return true;
        }

        return false;
    }

    public static bool TryGetMutableSetupCarrierInitializer(
        Compilation compilation,
        IPropertySymbol property,
        out IOperation? initializer,
        out bool hasDeclarationInitializer)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));
        if (property is null)
            throw new ArgumentNullException(nameof(property));

        initializer = null;
        hasDeclarationInitializer = false;
        if (!CanUseMutableSetupCarrierMember(property))
            return false;

        initializer = TryGetPropertyDeclarationInitializer(compilation, property, out hasDeclarationInitializer);
        return true;
    }

    public static bool TryGetMutableSetupCarrierInitializer(
        Compilation compilation,
        IFieldSymbol field,
        out IOperation? initializer,
        out bool hasDeclarationInitializer)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));
        if (field is null)
            throw new ArgumentNullException(nameof(field));

        initializer = null;
        hasDeclarationInitializer = false;
        if (!CanUseMutableSetupCarrierMember(field))
            return false;

        initializer = TryGetFieldDeclarationInitializer(compilation, field, out hasDeclarationInitializer);
        return true;
    }

    public static bool TryGetValueMemberInitializer(
        Compilation compilation,
        ISymbol member,
        out IOperation? initializer)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));
        if (member is null)
            throw new ArgumentNullException(nameof(member));

        initializer = member switch
        {
            IPropertySymbol property => TryGetPropertyInitializer(compilation, property),
            IFieldSymbol field => TryGetFieldInitializer(compilation, field),
            _ => null
        };

        return initializer is not null;
    }

    private static IOperation? TryGetPropertyInitializer(
        Compilation compilation,
        IPropertySymbol property)
    {
        if (!IsSupportedValueProperty(compilation, property))
            return null;

        return TryGetPropertyDeclarationInitializer(compilation, property);
    }

    private static IOperation? TryGetPropertyDeclarationInitializer(
        Compilation compilation,
        IPropertySymbol property)
        => TryGetPropertyDeclarationInitializer(compilation, property, out _);

    private static IOperation? TryGetPropertyDeclarationInitializer(
        Compilation compilation,
        IPropertySymbol property,
        out bool hasDeclarationInitializer)
    {
        hasDeclarationInitializer = false;
        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration ||
                declaration.Initializer?.Value is null)
            {
                continue;
            }

            hasDeclarationInitializer = true;
            var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declaration.Initializer.Value,
                    out var operation) &&
                RazorVueOperationNormalizer.Unwrap(operation) is { } initializer)
            {
                return initializer;
            }
        }

        return null;
    }

    private static IOperation? TryGetFieldInitializer(
        Compilation compilation,
        IFieldSymbol field)
    {
        if (!IsSupportedValueField(compilation, field))
            return null;

        return TryGetFieldDeclarationInitializer(compilation, field);
    }

    private static IOperation? TryGetFieldDeclarationInitializer(
        Compilation compilation,
        IFieldSymbol field)
        => TryGetFieldDeclarationInitializer(compilation, field, out _);

    private static IOperation? TryGetFieldDeclarationInitializer(
        Compilation compilation,
        IFieldSymbol field,
        out bool hasDeclarationInitializer)
    {
        hasDeclarationInitializer = false;
        foreach (var syntaxReference in field.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                declarator.Initializer?.Value is null)
            {
                continue;
            }

            hasDeclarationInitializer = true;
            var semanticModel = compilation.GetSemanticModel(declarator.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declarator.Initializer.Value,
                    out var operation) &&
                RazorVueOperationNormalizer.Unwrap(operation) is { } initializer)
            {
                return initializer;
            }
        }

        return null;
    }

    private static bool IsSupportedValueProperty(
        Compilation compilation,
        IPropertySymbol property)
    {
        if (property.IsStatic || property.IsIndexer || property.IsImplicitlyDeclared)
            return false;

        if (property.SetMethod is null)
            return true;

        return CanUseMutableSetupCarrierMember(property) &&
               !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(compilation, property);
    }

    private static bool IsSupportedValueField(
        Compilation compilation,
        IFieldSymbol field)
    {
        if (field.IsStatic || field.IsImplicitlyDeclared || field.AssociatedSymbol is not null)
            return false;

        if (field.IsReadOnly)
            return true;

        return CanUseMutableSetupCarrierMember(field) &&
               !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(compilation, field);
    }

    private static bool HasDeclarationInitializer(IPropertySymbol property)
        => property.DeclaringSyntaxReferences
            .Select(static reference => reference.GetSyntax())
            .OfType<PropertyDeclarationSyntax>()
            .Any(static declaration => declaration.Initializer?.Value is not null);

    private static bool HasDeclarationInitializer(IFieldSymbol field)
        => field.DeclaringSyntaxReferences
            .Select(static reference => reference.GetSyntax())
            .OfType<VariableDeclaratorSyntax>()
            .Any(static declaration => declaration.Initializer?.Value is not null);
}
