using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue;

internal static class RazorVueImperativeRenderFragmentCarrierHelper
{
    public static bool IsRenderFragmentCarrierType(ITypeSymbol? typeSymbol)
        => RazorVueRenderFragmentTypeHelper.IsRenderFragmentType(typeSymbol);

    public static Dictionary<ILocalSymbol, IOperation> CollectSourceStableLocalRenderFragmentInitializers(
        Compilation compilation,
        IReadOnlyList<IOperation> operations)
        => RazorVueSourceStableLocalInitializerHelper.CollectSourceStableLocalInitializers(
            compilation,
            operations,
            IsRenderFragmentCarrierType);

    public static bool TryGetSourceStableLocalRenderFragmentInitializer(
        Compilation compilation,
        ILocalSymbol local,
        out IOperation? initializer)
        => RazorVueSourceStableLocalInitializerHelper.TryGetSourceStableLocalInitializer(
            compilation,
            local,
            IsRenderFragmentCarrierType,
            out initializer);

    public static bool IsSourceStableLocalRenderFragmentInitializerInvalidatedByLaterWrites(
        Compilation compilation,
        ILocalSymbol local)
        => RazorVueSourceStableLocalInitializerHelper.IsSourceStableLocalInitializerInvalidatedByLaterWrites(
            compilation,
            local,
            IsRenderFragmentCarrierType);

    public static bool TryGetAnonymousFunction(IOperation? operation, out IAnonymousFunctionOperation anonymousFunction)
    {
        anonymousFunction = default!;
        var current = UnwrapDelegateCarrier(operation);
        switch (current)
        {
            case IAnonymousFunctionOperation directAnonymousFunction:
                anonymousFunction = directAnonymousFunction;
                return true;
            case IDelegateCreationOperation delegateCreation when UnwrapDelegateCarrier(delegateCreation.Target) is IAnonymousFunctionOperation targetAnonymousFunction:
                anonymousFunction = targetAnonymousFunction;
                return true;
            default:
                return false;
        }
    }

    public static bool TryGetSingleBuilderParameter(
        IAnonymousFunctionOperation anonymousFunction,
        out IParameterSymbol builderParameter)
    {
        builderParameter = anonymousFunction.Symbol.Parameters.FirstOrDefault(
            static parameter => IsRenderTreeBuilderType(parameter.Type))!;
        return builderParameter is not null && anonymousFunction.Symbol.Parameters.Length == 1;
    }

    public static bool TryGetTypedBuilderTemplate(
        IOperation? operation,
        out IAnonymousFunctionOperation outerAnonymousFunction,
        out IAnonymousFunctionOperation builderAnonymousFunction)
    {
        outerAnonymousFunction = default!;
        builderAnonymousFunction = default!;
        if (!TryGetAnonymousFunction(operation, out outerAnonymousFunction) ||
            outerAnonymousFunction.Symbol.Parameters.Length != 1)
        {
            return false;
        }

        if (!TryGetReturnedAnonymousFunction(outerAnonymousFunction.Body, out builderAnonymousFunction))
            return false;

        return TryGetSingleBuilderParameter(builderAnonymousFunction, out _);
    }

    public static bool TryGetSingleReturnedValue(IBlockOperation block, out IOperation? returnedValue)
    {
        returnedValue = null;
        if (block.Operations.Length != 1 ||
            block.Operations[0] is not IReturnOperation returnOperation)
        {
            return false;
        }

        returnedValue = RazorVueOperationNormalizer.Unwrap(returnOperation.ReturnedValue);
        return returnedValue is not null;
    }

    public static bool TryGetCurrentComponentRenderFragmentMemberInitializer(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        ISymbol member,
        Func<IOperation?, IOperation?> unwrap,
        Func<Compilation, ISymbol, bool> isSourceStableMutableCarrierMember,
        out IOperation? initializer)
    {
        initializer = null;
        if (!RazorVueSymbolIdentity.IsCurrentComponentMember(componentSymbol, member, instance: null, unwrap))
            return false;

        if (!IsSupportedCurrentComponentRenderFragmentCarrierMember(compilation, member, isSourceStableMutableCarrierMember))
            return false;

        initializer = member switch
        {
            IPropertySymbol property => TryGetPropertyRenderFragmentInitializer(compilation, property),
            IFieldSymbol field => TryGetFieldRenderFragmentInitializer(compilation, field),
            _ => null
        };

        return initializer is not null;
    }

    public static bool TryGetRenderFragmentFactoryReturnedValue(
        Compilation compilation,
        IInvocationOperation invocation,
        out IOperation returnedValue)
        => TryGetRenderFragmentFactoryReturnedValue(
            compilation,
            RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod),
            out returnedValue);

    public static bool TryGetRenderFragmentFactoryReturnedValue(
        Compilation compilation,
        IMethodSymbol method,
        out IOperation returnedValue)
    {
        returnedValue = default!;
        foreach (var syntaxReference in RazorVueMethodSymbolNormalizer.GetCanonicalMethod(method).DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax();
            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            switch (syntax)
            {
                case MethodDeclarationSyntax methodDeclaration:
                    if (methodDeclaration.ExpressionBody?.Expression is { } methodExpressionBody &&
                        RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                            semanticModel,
                            methodExpressionBody,
                            out var methodExpressionBodyOperation) &&
                        methodExpressionBodyOperation is not null)
                    {
                        returnedValue = methodExpressionBodyOperation;
                        return true;
                    }

                    if (methodDeclaration.Body is not null &&
                        semanticModel.GetOperation(methodDeclaration.Body) is IBlockOperation methodBlock &&
                        TryGetSingleReturnedValue(methodBlock, out var methodReturnValue) &&
                        methodReturnValue is not null)
                    {
                        returnedValue = methodReturnValue;
                        return true;
                    }

                    break;

                case LocalFunctionStatementSyntax localFunction:
                    if (localFunction.ExpressionBody?.Expression is { } localExpressionBody &&
                        RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                            semanticModel,
                            localExpressionBody,
                            out var localExpressionBodyOperation) &&
                        localExpressionBodyOperation is not null)
                    {
                        returnedValue = localExpressionBodyOperation;
                        return true;
                    }

                    if (localFunction.Body is not null &&
                        semanticModel.GetOperation(localFunction.Body) is IBlockOperation localBlock &&
                        TryGetSingleReturnedValue(localBlock, out var localReturnValue) &&
                        localReturnValue is not null)
                    {
                        returnedValue = localReturnValue;
                        return true;
                    }

                    break;
            }
        }

        return false;
    }

    public static bool TryEnumerateNestedImperativeRenderFragmentBodies(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IOperation? operation,
        Func<IOperation?, IOperation?> unwrap,
        Func<Compilation, ISymbol, bool> isSourceStableMutableCarrierMember,
        out ImmutableArray<IOperation> nestedBodies)
    {
        var builder = ImmutableArray.CreateBuilder<IOperation>();
        var visitedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        var visitedMembers = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var visitedMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        CollectNestedBodies(
            compilation,
            componentSymbol,
            unwrap(operation),
            unwrap,
            isSourceStableMutableCarrierMember,
            visitedLocals,
            visitedMembers,
            visitedMethods,
            builder);
        nestedBodies = builder.ToImmutable();
        return nestedBodies.Length > 0;
    }

    private static void CollectNestedBodies(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IOperation? operation,
        Func<IOperation?, IOperation?> unwrap,
        Func<Compilation, ISymbol, bool> isSourceStableMutableCarrierMember,
        HashSet<ILocalSymbol> visitedLocals,
        HashSet<ISymbol> visitedMembers,
        HashSet<IMethodSymbol> visitedMethods,
        ImmutableArray<IOperation>.Builder bodies)
    {
        var current = unwrap(operation);
        if (current is null)
            return;

        if (TryGetAnonymousFunction(current, out var anonymousFunction) &&
            TryGetSingleBuilderParameter(anonymousFunction, out _))
        {
            bodies.Add(anonymousFunction.Body);
            return;
        }

        if (TryGetTypedBuilderTemplate(current, out _, out var builderAnonymousFunction) &&
            TryGetSingleBuilderParameter(builderAnonymousFunction, out _))
        {
            bodies.Add(builderAnonymousFunction.Body);
            return;
        }

        switch (current)
        {
            case ILocalReferenceOperation localReference:
                if (!visitedLocals.Add(localReference.Local))
                    return;

                CollectNestedBodies(
                    compilation,
                    componentSymbol,
                    TryGetLocalRenderFragmentInitializer(compilation, localReference.Local),
                    unwrap,
                    isSourceStableMutableCarrierMember,
                    visitedLocals,
                    visitedMembers,
                    visitedMethods,
                    bodies);
                return;

            case IPropertyReferenceOperation propertyReference
                when visitedMembers.Add(propertyReference.Property):
                if (TryGetCurrentComponentRenderFragmentMemberInitializer(
                        compilation,
                        componentSymbol,
                        propertyReference.Property,
                        unwrap,
                        isSourceStableMutableCarrierMember,
                        out var propertyInitializer))
                {
                    CollectNestedBodies(
                        compilation,
                        componentSymbol,
                        propertyInitializer,
                        unwrap,
                        isSourceStableMutableCarrierMember,
                        visitedLocals,
                        visitedMembers,
                        visitedMethods,
                        bodies);
                }

                return;

            case IFieldReferenceOperation fieldReference
                when visitedMembers.Add(fieldReference.Field):
                if (TryGetCurrentComponentRenderFragmentMemberInitializer(
                        compilation,
                        componentSymbol,
                        fieldReference.Field,
                        unwrap,
                        isSourceStableMutableCarrierMember,
                        out var fieldInitializer))
                {
                    CollectNestedBodies(
                        compilation,
                        componentSymbol,
                        fieldInitializer,
                        unwrap,
                        isSourceStableMutableCarrierMember,
                        visitedLocals,
                        visitedMembers,
                        visitedMethods,
                        bodies);
                }

                return;

            case IInvocationOperation invocation
                when IsSupportedRenderFragmentFactoryInvocation(compilation, componentSymbol, invocation, unwrap) &&
                     IsRenderFragmentCarrierType(invocation.TargetMethod.ReturnType):
                var canonicalMethod = RazorVueMethodSymbolNormalizer.GetCanonicalMethod(invocation.TargetMethod);
                if (!visitedMethods.Add(canonicalMethod))
                    return;

                if (TryGetRenderFragmentFactoryReturnedValue(compilation, invocation, out var returnedValue))
                {
                    CollectNestedBodies(
                        compilation,
                        componentSymbol,
                        returnedValue,
                        unwrap,
                        isSourceStableMutableCarrierMember,
                        visitedLocals,
                        visitedMembers,
                        visitedMethods,
                        bodies);
                }

                return;
        }

        foreach (var child in current.ChildOperations)
        {
            if (child is null)
                continue;

            CollectNestedBodies(
                compilation,
                componentSymbol,
                child,
                unwrap,
                isSourceStableMutableCarrierMember,
                visitedLocals,
                visitedMembers,
                visitedMethods,
                bodies);
        }
    }

    private static bool IsSupportedRenderFragmentFactoryInvocation(
        Compilation compilation,
        INamedTypeSymbol componentSymbol,
        IInvocationOperation invocation,
        Func<IOperation?, IOperation?> unwrap)
    {
        _ = compilation;
        return RazorVueSymbolIdentity.IsCurrentComponentMember(componentSymbol, invocation.TargetMethod, invocation.Instance, unwrap) ||
               invocation is { Instance: null, TargetMethod.MethodKind: MethodKind.LocalFunction };
    }

    private static bool IsSupportedCurrentComponentRenderFragmentCarrierMember(
        Compilation compilation,
        ISymbol member,
        Func<Compilation, ISymbol, bool> isSourceStableMutableCarrierMember)
    {
        switch (member)
        {
            case IPropertySymbol propertySymbol:
                if (!IsRenderFragmentCarrierType(propertySymbol.Type))
                    return false;

                if (propertySymbol.SetMethod is null)
                    return true;

                return isSourceStableMutableCarrierMember(compilation, propertySymbol);

            case IFieldSymbol fieldSymbol:
                if (!IsRenderFragmentCarrierType(fieldSymbol.Type))
                    return false;

                if (fieldSymbol.IsReadOnly)
                    return true;

                return isSourceStableMutableCarrierMember(compilation, fieldSymbol);

            default:
                return false;
        }
    }

    private static IOperation? TryGetLocalRenderFragmentInitializer(Compilation compilation, ILocalSymbol local)
        => TryGetSourceStableLocalRenderFragmentInitializer(compilation, local, out var initializer)
            ? initializer
            : null;

    private static IOperation? TryGetPropertyRenderFragmentInitializer(Compilation compilation, IPropertySymbol property)
    {
        foreach (var syntaxReference in property.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax declaration)
                continue;

            var semanticModel = compilation.GetSemanticModel(declaration.SyntaxTree);
            if (RazorVuePropertyInitializerHelper.TryGetPropertyValueOperation(
                    semanticModel,
                    declaration,
                    out var propertyOperation))
            {
                return propertyOperation;
            }
        }

        return null;
    }

    private static IOperation? TryGetFieldRenderFragmentInitializer(Compilation compilation, IFieldSymbol field)
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
                    out var initializerOperation))
            {
                return initializerOperation;
            }
        }

        return null;
    }

    private static IEnumerable<IOperation> EnumerateOperations(IOperation root)
    {
        yield return root;

        foreach (var child in root.ChildOperations)
        {
            if (child is null)
                continue;

            foreach (var nested in EnumerateOperations(child))
                yield return nested;
        }
    }

    private static bool TryGetReturnedAnonymousFunction(
        IOperation? body,
        out IAnonymousFunctionOperation anonymousFunction)
    {
        anonymousFunction = default!;
        switch (RazorVueOperationNormalizer.Unwrap(body))
        {
            case IAnonymousFunctionOperation direct:
                anonymousFunction = direct;
                return true;
            case IDelegateCreationOperation delegateCreation:
                return TryGetAnonymousFunction(delegateCreation.Target, out anonymousFunction);
            case IBlockOperation block when TryGetSingleReturnedValue(block, out var returnValue):
                return TryGetAnonymousFunction(returnValue, out anonymousFunction);
            case IReturnOperation returnOperation when returnOperation.ReturnedValue is not null:
                return TryGetAnonymousFunction(returnOperation.ReturnedValue, out anonymousFunction);
            default:
                return false;
        }
    }

    private static IOperation? UnwrapDelegateCarrier(IOperation? operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        while (true)
        {
            switch (current)
            {
                case IConversionOperation conversion:
                    current = RazorVueOperationNormalizer.Unwrap(conversion.Operand);
                    continue;
                case IDelegateCreationOperation delegateCreation:
                    current = RazorVueOperationNormalizer.Unwrap(delegateCreation.Target);
                    continue;
                default:
                    return current;
            }
        }
    }

    private static bool IsRenderTreeBuilderType(ITypeSymbol? typeSymbol)
        => string.Equals(
            typeSymbol?.ToDisplayString(),
            "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder",
            StringComparison.Ordinal);
}
