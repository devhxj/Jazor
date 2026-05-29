using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Jazor.RazorVue;

internal static class RazorVueStaticMarkupValueHelper
{
    internal readonly record struct StaticMarkupCapturedBinding(
        IParameterSymbol ParameterSymbol,
        IOperation Initializer);

    internal readonly record struct StaticMarkupResolution(
        string Markup,
        ImmutableArray<StaticMarkupCapturedBinding> CapturedBindings)
    {
        public static StaticMarkupResolution Create(string markup)
            => new(markup, ImmutableArray<StaticMarkupCapturedBinding>.Empty);

        public StaticMarkupResolution PrependCapturedBindings(
            ImmutableArray<StaticMarkupCapturedBinding> capturedBindings)
        {
            if (capturedBindings.IsDefaultOrEmpty)
                return this;

            if (CapturedBindings.IsDefaultOrEmpty)
                return new StaticMarkupResolution(Markup, capturedBindings);

            var builder = ImmutableArray.CreateBuilder<StaticMarkupCapturedBinding>(
                capturedBindings.Length + CapturedBindings.Length);
            builder.AddRange(capturedBindings);
            builder.AddRange(CapturedBindings);
            return new StaticMarkupResolution(Markup, builder.MoveToImmutable());
        }

        public StaticMarkupResolution Append(StaticMarkupResolution next)
        {
            if (next.CapturedBindings.IsDefaultOrEmpty)
                return new StaticMarkupResolution(Markup + next.Markup, CapturedBindings);

            if (CapturedBindings.IsDefaultOrEmpty)
                return new StaticMarkupResolution(Markup + next.Markup, next.CapturedBindings);

            var builder = ImmutableArray.CreateBuilder<StaticMarkupCapturedBinding>(
                CapturedBindings.Length + next.CapturedBindings.Length);
            builder.AddRange(CapturedBindings);
            builder.AddRange(next.CapturedBindings);
            return new StaticMarkupResolution(Markup + next.Markup, builder.MoveToImmutable());
        }
    }

    internal abstract record StaticMarkupRenderResolution;

    internal sealed record StaticMarkupLiteralRenderResolution(
        StaticMarkupResolution Resolution) : StaticMarkupRenderResolution;

    internal sealed record StaticMarkupConditionalRenderResolution(
        IOperation Condition,
        StaticMarkupRenderResolution WhenTrue,
        StaticMarkupRenderResolution WhenFalse) : StaticMarkupRenderResolution;

    public static bool IsStaticMarkupCarrierType(ITypeSymbol? typeSymbol)
        => IsStringType(typeSymbol) || IsMarkupStringType(typeSymbol);

    public static bool IsStringType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return false;

        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.IsGenericType &&
            namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
        {
            typeSymbol = namedType.TypeArguments[0];
        }

        return typeSymbol.SpecialType == SpecialType.System_String;
    }

    public static string? TryGetStaticMarkupValue(IOperation? operation)
    {
        return TryResolveStaticMarkup(operation)?.Markup;
    }

    public static StaticMarkupResolution? TryResolveStaticMarkup(IOperation? operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return null;

        if (TryGetConstantString(current) is string directConstant)
            return StaticMarkupResolution.Create(directConstant);

        if (TryGetMarkupStringCtorLiteral(current) is string constructorLiteral)
            return StaticMarkupResolution.Create(constructorLiteral);

        if (TryGetMarkupStringExplicitCastLiteral(current) is string explicitCastLiteral)
            return StaticMarkupResolution.Create(explicitCastLiteral);

        return null;
    }

    public static string? TryGetStaticMarkupValue(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver)
    {
        return TryResolveStaticMarkup(
            operation,
            compilation,
            localInitializerResolver,
            propertyInitializerResolver,
            fieldInitializerResolver,
            methodReturnedValueResolver: null,
            isSupportedMethodInvocation: null,
            visitedLocals: new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default),
            visitedMembers: new HashSet<ISymbol>(SymbolEqualityComparer.Default),
            visitedMethods: new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default))
            ?.Markup;
    }

    public static string? TryGetStaticMarkupValue(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver,
        Func<IInvocationOperation, IOperation?>? methodReturnedValueResolver,
        Func<IInvocationOperation, bool>? isSupportedMethodInvocation)
    {
        return TryResolveStaticMarkup(
            operation,
            compilation,
            localInitializerResolver,
            propertyInitializerResolver,
            fieldInitializerResolver,
            methodReturnedValueResolver,
            isSupportedMethodInvocation,
            visitedLocals: new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default),
            visitedMembers: new HashSet<ISymbol>(SymbolEqualityComparer.Default),
            visitedMethods: new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default))
            ?.Markup;
    }

    public static StaticMarkupResolution? TryResolveStaticMarkup(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver)
    {
        return TryResolveStaticMarkup(
            operation,
            compilation,
            localInitializerResolver,
            propertyInitializerResolver,
            fieldInitializerResolver,
            methodReturnedValueResolver: null,
            isSupportedMethodInvocation: null,
            visitedLocals: new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default),
            visitedMembers: new HashSet<ISymbol>(SymbolEqualityComparer.Default),
            visitedMethods: new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));
    }

    public static StaticMarkupResolution? TryResolveStaticMarkup(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver,
        Func<IInvocationOperation, IOperation?>? methodReturnedValueResolver,
        Func<IInvocationOperation, bool>? isSupportedMethodInvocation)
    {
        return TryResolveStaticMarkup(
            operation,
            compilation,
            localInitializerResolver,
            propertyInitializerResolver,
            fieldInitializerResolver,
            methodReturnedValueResolver,
            isSupportedMethodInvocation,
            visitedLocals: new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default),
            visitedMembers: new HashSet<ISymbol>(SymbolEqualityComparer.Default),
            visitedMethods: new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));
    }

    public static StaticMarkupRenderResolution? TryResolveStaticMarkupRender(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver,
        Func<IInvocationOperation, IOperation?>? methodReturnedValueResolver,
        Func<IInvocationOperation, bool>? isSupportedMethodInvocation)
    {
        return TryResolveStaticMarkupRender(
            operation,
            compilation,
            localInitializerResolver,
            propertyInitializerResolver,
            fieldInitializerResolver,
            methodReturnedValueResolver,
            isSupportedMethodInvocation,
            visitedLocals: new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default),
            visitedMembers: new HashSet<ISymbol>(SymbolEqualityComparer.Default),
            visitedMethods: new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));
    }

    public static bool TryGetInvalidatedSourceStableStaticMarkupMember(
        IOperation? operation,
        Compilation compilation,
        out ISymbol member)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));

        member = default!;
        return TryGetInvalidatedSourceStableStaticMarkupMemberCore(
            operation,
            compilation,
            out member,
            new HashSet<ISymbol>(SymbolEqualityComparer.Default));
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

    private static StaticMarkupResolution? TryResolveStaticMarkup(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver,
        HashSet<ILocalSymbol> visitedLocals,
        HashSet<ISymbol> visitedMembers)
        => TryResolveStaticMarkup(
            operation,
            compilation,
            localInitializerResolver,
            propertyInitializerResolver,
            fieldInitializerResolver,
            methodReturnedValueResolver: null,
            isSupportedMethodInvocation: null,
            visitedLocals,
            visitedMembers,
            visitedMethods: new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    private static StaticMarkupResolution? TryResolveStaticMarkup(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver,
        Func<IInvocationOperation, IOperation?>? methodReturnedValueResolver,
        Func<IInvocationOperation, bool>? isSupportedMethodInvocation,
        HashSet<ILocalSymbol> visitedLocals,
        HashSet<ISymbol> visitedMembers,
        HashSet<IMethodSymbol> visitedMethods)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return null;

        if (TryResolveStaticMarkup(current) is { } directResolution)
            return directResolution;

        switch (current)
        {
            case ILocalReferenceOperation localReference:
                if (localInitializerResolver is null || !visitedLocals.Add(localReference.Local))
                    return null;

                return TryResolveStaticMarkup(
                    localInitializerResolver(localReference.Local),
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);

            case IPropertyReferenceOperation propertyReference:
                if (propertyInitializerResolver is null ||
                    !IsSupportedStaticMarkupCarrierMember(compilation, propertyReference.Property) ||
                    !visitedMembers.Add(propertyReference.Property))
                {
                    return null;
                }

                return TryResolveStaticMarkup(
                    propertyInitializerResolver(propertyReference.Property),
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);

            case IFieldReferenceOperation fieldReference:
                if (fieldInitializerResolver is null ||
                    !IsSupportedStaticMarkupCarrierMember(compilation, fieldReference.Field) ||
                    !visitedMembers.Add(fieldReference.Field))
                {
                    return null;
                }

                return TryResolveStaticMarkup(
                    fieldInitializerResolver(fieldReference.Field),
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);

            case IBinaryOperation binaryOperation
                when binaryOperation.OperatorKind == BinaryOperatorKind.Add:
                var leftResolution = TryResolveStaticMarkupBranch(
                    binaryOperation.LeftOperand,
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);
                if (leftResolution is null)
                    return null;

                var rightResolution = TryResolveStaticMarkupBranch(
                    binaryOperation.RightOperand,
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);
                if (rightResolution is null)
                    return null;

                return leftResolution.Value.Append(rightResolution.Value);

            case IInvocationOperation invocation:
                if (methodReturnedValueResolver is null ||
                    isSupportedMethodInvocation is null ||
                    !isSupportedMethodInvocation(invocation) ||
                    !TryGetInvocationCapturedBindings(invocation, compilation, out var capturedBindings))
                {
                    return null;
                }

                var canonicalMethod = RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod);
                if (!visitedMethods.Add(canonicalMethod))
                    return null;

                var returnedResolution = TryResolveStaticMarkup(
                    methodReturnedValueResolver(invocation),
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);
                if (returnedResolution is null)
                    return null;

                return returnedResolution.Value.PrependCapturedBindings(capturedBindings);

            case IConversionOperation conversion
                when !conversion.IsImplicit &&
                     IsMarkupStringType(conversion.Type):
                return TryResolveStaticMarkup(
                    conversion.Operand,
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);

            case IObjectCreationOperation objectCreation
                when IsMarkupStringType(objectCreation.Type) &&
                     objectCreation.Arguments.Length == 1:
                return TryResolveStaticMarkup(
                    objectCreation.Arguments[0].Value,
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);

            default:
                return null;
        }
    }

    private static StaticMarkupResolution? TryResolveStaticMarkupBranch(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver,
        Func<IInvocationOperation, IOperation?>? methodReturnedValueResolver,
        Func<IInvocationOperation, bool>? isSupportedMethodInvocation,
        HashSet<ILocalSymbol> visitedLocals,
        HashSet<ISymbol> visitedMembers,
        HashSet<IMethodSymbol> visitedMethods)
    {
        return TryResolveStaticMarkup(
            operation,
            compilation,
            localInitializerResolver,
            propertyInitializerResolver,
            fieldInitializerResolver,
            methodReturnedValueResolver,
            isSupportedMethodInvocation,
            new HashSet<ILocalSymbol>(visitedLocals, SymbolEqualityComparer.Default),
            new HashSet<ISymbol>(visitedMembers, SymbolEqualityComparer.Default),
            new HashSet<IMethodSymbol>(visitedMethods, SymbolEqualityComparer.Default));
    }

    private static StaticMarkupRenderResolution? TryResolveStaticMarkupRender(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver,
        Func<IInvocationOperation, IOperation?>? methodReturnedValueResolver,
        Func<IInvocationOperation, bool>? isSupportedMethodInvocation,
        HashSet<ILocalSymbol> visitedLocals,
        HashSet<ISymbol> visitedMembers,
        HashSet<IMethodSymbol> visitedMethods)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return null;

        if (TryResolveStaticMarkup(
                current,
                compilation,
                localInitializerResolver,
                propertyInitializerResolver,
                fieldInitializerResolver,
                methodReturnedValueResolver,
                isSupportedMethodInvocation,
                visitedLocals,
                visitedMembers,
                visitedMethods) is { } literalResolution)
        {
            return new StaticMarkupLiteralRenderResolution(literalResolution);
        }

        switch (current)
        {
            case IConditionalOperation conditional:
                var whenTrue = TryResolveStaticMarkupRenderBranch(
                    conditional.WhenTrue,
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);
                if (whenTrue is null)
                    return null;

                var whenFalse = TryResolveStaticMarkupRenderBranch(
                    conditional.WhenFalse,
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);
                if (whenFalse is null)
                    return null;

                var condition = RazorVueOperationNormalizer.Unwrap(conditional.Condition) ?? conditional.Condition;
                return new StaticMarkupConditionalRenderResolution(condition, whenTrue, whenFalse);

            case IConversionOperation conversion
                when !conversion.IsImplicit &&
                     IsMarkupStringType(conversion.Type):
                return TryResolveStaticMarkupRender(
                    conversion.Operand,
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);

            case IObjectCreationOperation objectCreation
                when IsMarkupStringType(objectCreation.Type) &&
                     objectCreation.Arguments.Length == 1:
                return TryResolveStaticMarkupRender(
                    objectCreation.Arguments[0].Value,
                    compilation,
                    localInitializerResolver,
                    propertyInitializerResolver,
                    fieldInitializerResolver,
                    methodReturnedValueResolver,
                    isSupportedMethodInvocation,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods);

            default:
                return null;
        }
    }

    private static StaticMarkupRenderResolution? TryResolveStaticMarkupRenderBranch(
        IOperation? operation,
        Compilation compilation,
        Func<ILocalSymbol, IOperation?>? localInitializerResolver,
        Func<IPropertySymbol, IOperation?>? propertyInitializerResolver,
        Func<IFieldSymbol, IOperation?>? fieldInitializerResolver,
        Func<IInvocationOperation, IOperation?>? methodReturnedValueResolver,
        Func<IInvocationOperation, bool>? isSupportedMethodInvocation,
        HashSet<ILocalSymbol> visitedLocals,
        HashSet<ISymbol> visitedMembers,
        HashSet<IMethodSymbol> visitedMethods)
    {
        return TryResolveStaticMarkupRender(
            operation,
            compilation,
            localInitializerResolver,
            propertyInitializerResolver,
            fieldInitializerResolver,
            methodReturnedValueResolver,
            isSupportedMethodInvocation,
            new HashSet<ILocalSymbol>(visitedLocals, SymbolEqualityComparer.Default),
            new HashSet<ISymbol>(visitedMembers, SymbolEqualityComparer.Default),
            new HashSet<IMethodSymbol>(visitedMethods, SymbolEqualityComparer.Default));
    }

    private static bool TryGetInvalidatedSourceStableStaticMarkupMemberCore(
        IOperation? operation,
        Compilation compilation,
        out ISymbol member,
        HashSet<ISymbol> visitedMembers)
    {
        member = default!;
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return false;

        switch (current)
        {
            case IPropertyReferenceOperation propertyReference
                when IsStaticMarkupCarrierType(propertyReference.Property.Type):
                if (TryGetInvalidatedSourceStableStaticMarkupMember(
                        propertyReference.Property,
                        compilation,
                        out member))
                {
                    return true;
                }

                return TryGetStaticMarkupMemberInitializer(
                           propertyReference.Property,
                           compilation,
                           visitedMembers,
                           out var staticMarkupPropertyInitializer) &&
                       TryGetInvalidatedSourceStableStaticMarkupMemberCore(
                           staticMarkupPropertyInitializer,
                           compilation,
                           out member,
                           visitedMembers);

            case IFieldReferenceOperation fieldReference
                when IsStaticMarkupCarrierType(fieldReference.Field.Type):
                if (TryGetInvalidatedSourceStableStaticMarkupMember(
                        fieldReference.Field,
                        compilation,
                        out member))
                {
                    return true;
                }

                return TryGetStaticMarkupMemberInitializer(
                           fieldReference.Field,
                           compilation,
                           visitedMembers,
                           out var staticMarkupFieldInitializer) &&
                       TryGetInvalidatedSourceStableStaticMarkupMemberCore(
                           staticMarkupFieldInitializer,
                           compilation,
                           out member,
                           visitedMembers);

            case IConversionOperation conversion
                when !conversion.IsImplicit &&
                     IsMarkupStringType(conversion.Type):
                return TryGetInvalidatedSourceStableStaticMarkupMemberCore(
                    conversion.Operand,
                    compilation,
                    out member,
                    visitedMembers);

            case IObjectCreationOperation objectCreation
                when IsMarkupStringType(objectCreation.Type) &&
                     objectCreation.Arguments.Length == 1:
                return TryGetInvalidatedSourceStableStaticMarkupMemberCore(
                    objectCreation.Arguments[0].Value,
                    compilation,
                    out member,
                    visitedMembers);

            case IConditionalOperation conditional:
                return TryGetInvalidatedSourceStableStaticMarkupMemberCore(
                           conditional.WhenTrue,
                           compilation,
                           out member,
                           visitedMembers) ||
                       TryGetInvalidatedSourceStableStaticMarkupMemberCore(
                           conditional.WhenFalse,
                           compilation,
                           out member,
                           visitedMembers);

            case IBinaryOperation binaryOperation
                when binaryOperation.OperatorKind == BinaryOperatorKind.Add:
                return TryGetInvalidatedSourceStableStaticMarkupMemberCore(
                           binaryOperation.LeftOperand,
                           compilation,
                           out member,
                           visitedMembers) ||
                       TryGetInvalidatedSourceStableStaticMarkupMemberCore(
                           binaryOperation.RightOperand,
                           compilation,
                           out member,
                           visitedMembers);

            default:
                return false;
        }
    }

    private static bool TryGetInvalidatedSourceStableStaticMarkupMember(
        ISymbol candidate,
        Compilation compilation,
        out ISymbol member)
    {
        member = default!;
        if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(candidate) ||
            !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(compilation, candidate))
        {
            return false;
        }

        member = candidate;
        return true;
    }

    private static bool TryGetStaticMarkupMemberInitializer(
        ISymbol member,
        Compilation compilation,
        HashSet<ISymbol> visitedMembers,
        out IOperation initializer)
    {
        initializer = default!;
        if (!IsSupportedStaticMarkupCarrierMember(compilation, member) ||
            !visitedMembers.Add(member))
        {
            return false;
        }

        var resolvedInitializer = member switch
        {
            IPropertySymbol property => TryGetPropertyInitializer(property, compilation),
            IFieldSymbol field => TryGetFieldInitializer(field, compilation),
            _ => null
        };
        if (resolvedInitializer is null)
            return false;

        initializer = resolvedInitializer;
        return true;
    }

    private static IOperation? TryGetPropertyInitializer(
        IPropertySymbol property,
        Compilation compilation)
    {
        foreach (var reference in property.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;

            var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(semanticModel, declaration, out var propertyOperation))
                return propertyOperation;
        }

        return null;
    }

    private static IOperation? TryGetFieldInitializer(
        IFieldSymbol field,
        Compilation compilation)
    {
        foreach (var reference in field.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                declarator.Initializer?.Value is null)
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(declarator.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declarator.Initializer.Value,
                    out var initializerOperation))
            {
                return initializerOperation;
            }
        }

        return null;
    }

    private static bool TryGetInvocationCapturedBindings(
        IInvocationOperation invocation,
        Compilation compilation,
        out ImmutableArray<StaticMarkupCapturedBinding> capturedBindings)
    {
        capturedBindings = ImmutableArray<StaticMarkupCapturedBinding>.Empty;

        foreach (var parameter in invocation.TargetMethod.Parameters)
        {
            if (parameter.RefKind != RefKind.None)
                return false;
        }

        var boundParameters = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
        var builder = ImmutableArray.CreateBuilder<StaticMarkupCapturedBinding>(invocation.TargetMethod.Parameters.Length);
        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter is not { } rawParameter)
                return false;

            var parameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(invocation.TargetMethod, rawParameter);
            if (!boundParameters.Add(parameter))
                return false;

            var initializer = RazorVueOperationNormalizer.Unwrap(argument.Value);
            if (initializer is null)
                return false;

            builder.Add(new StaticMarkupCapturedBinding(parameter, initializer));
        }

        foreach (var rawParameter in invocation.TargetMethod.Parameters)
        {
            var parameter = RazorVueMethodSymbolNormalizer.NormalizeParameter(invocation.TargetMethod, rawParameter);
            if (boundParameters.Contains(parameter))
                continue;

            if (parameter.IsParams ||
                !parameter.HasExplicitDefaultValue)
            {
                return false;
            }

            var initializer = TryGetParameterDefaultValueOperation(parameter, compilation);
            if (initializer is null || !boundParameters.Add(parameter))
                return false;

            builder.Add(new StaticMarkupCapturedBinding(parameter, initializer));
        }

        if (boundParameters.Count != invocation.TargetMethod.Parameters.Length)
            return false;

        capturedBindings = builder.MoveToImmutable();
        return true;
    }

    private static IOperation? TryGetParameterDefaultValueOperation(
        IParameterSymbol parameter,
        Compilation compilation)
    {
        foreach (var syntaxReference in parameter.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not ParameterSyntax parameterSyntax ||
                parameterSyntax.Default?.Value is null)
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(parameterSyntax.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    parameterSyntax.Default.Value,
                    out var defaultValueOperation) &&
                RazorVueOperationNormalizer.Unwrap(defaultValueOperation) is { } initializer)
            {
                return initializer;
            }
        }

        return null;
    }

    private static bool IsSupportedStaticMarkupCarrierMember(Compilation compilation, ISymbol member)
    {
        switch (member)
        {
            case IPropertySymbol propertySymbol:
                if (propertySymbol.SetMethod is null)
                    return true;

                if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(propertySymbol))
                    return false;

                return !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(compilation, propertySymbol);

            case IFieldSymbol fieldSymbol:
                if (fieldSymbol.IsReadOnly)
                    return true;

                if (!RazorVueMemberWriteAnalysis.CanUseSourceStableMutableCarrierMember(fieldSymbol))
                    return false;

                return !RazorVueMemberWriteAnalysis.HasObservableWritesOutsideDeclarationInitializer(compilation, fieldSymbol);

            default:
                return false;
        }
    }
}
