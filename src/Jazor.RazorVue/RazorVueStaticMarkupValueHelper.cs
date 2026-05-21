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
    }

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

            default:
                return null;
        }
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
