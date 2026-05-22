using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue;

internal static class RazorVueSourceStableLocalInitializerHelper
{
    private readonly record struct SourceStableLocalInitializer(
        IOperation Initializer,
        SyntaxNode? AllowedAssignmentSyntax);

    public static bool IsSupportedLifecycleFirstRenderCarrierType(ITypeSymbol? type)
    {
        if (type is null)
            return false;

        if (type.SpecialType == SpecialType.System_Boolean)
            return true;

        return type is INamedTypeSymbol
        {
            OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
            TypeArguments.Length: 1
        } namedType &&
               namedType.TypeArguments[0].SpecialType == SpecialType.System_Boolean;
    }

    public static bool CanParticipateInLifecycleCompilerFallback(ITypeSymbol? type)
        => type is not null && type.TypeKind != TypeKind.Error;

    public static Dictionary<ILocalSymbol, IOperation> CollectSourceStableLocalInitializers(
        Compilation compilation,
        IReadOnlyList<IOperation> operations,
        Func<ITypeSymbol?, bool> isSupportedCarrierType)
    {
        var collected = CollectSourceStableLocalInitializerStates(operations, isSupportedCarrierType);
        var result = new Dictionary<ILocalSymbol, IOperation>(SymbolEqualityComparer.Default);
        foreach (var pair in collected)
        {
            if (TryGetSourceStableLocalInitializer(compilation, pair.Key, isSupportedCarrierType, out var initializer) &&
                initializer is not null)
            {
                result[pair.Key] = initializer;
            }
        }

        return result;
    }

    public static bool TryGetSourceStableLocalInitializer(
        Compilation compilation,
        ILocalSymbol local,
        Func<ITypeSymbol?, bool> isSupportedCarrierType,
        out IOperation? initializer)
    {
        initializer = null;
        if (!isSupportedCarrierType(local.Type))
            return false;

        foreach (var syntaxReference in local.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not VariableDeclaratorSyntax declarator)
                continue;

            var semanticModel = compilation.GetSemanticModel(declarator.SyntaxTree);
            if (!TryGetSourceStableLocalInitializer(
                    local,
                    declarator,
                    semanticModel,
                    isSupportedCarrierType,
                    out initializer))
            {
                continue;
            }

            return initializer is not null;
        }

        return false;
    }

    public static bool IsSourceStableLocalInitializerInvalidatedByLaterWrites(
        Compilation compilation,
        ILocalSymbol local,
        Func<ITypeSymbol?, bool> isSupportedCarrierType)
    {
        if (!isSupportedCarrierType(local.Type))
            return false;

        foreach (var syntaxReference in local.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not VariableDeclaratorSyntax declarator)
                continue;

            var semanticModel = compilation.GetSemanticModel(declarator.SyntaxTree);
            if (!TryIsSourceStableLocalInitializerInvalidatedByLaterWrites(
                    local,
                    declarator,
                    semanticModel,
                    isSupportedCarrierType,
                    out var invalidated))
            {
                continue;
            }

            return invalidated;
        }

        return false;
    }

    private static bool TryGetSourceStableLocalInitializer(
        ILocalSymbol local,
        VariableDeclaratorSyntax declarator,
        SemanticModel semanticModel,
        Func<ITypeSymbol?, bool> isSupportedCarrierType,
        out IOperation? initializer)
    {
        initializer = null;
        if (!isSupportedCarrierType(local.Type))
            return false;

        if (declarator.Initializer?.Value is not null)
        {
            if (HasMutableLocalWrites(local, declarator, semanticModel))
                return false;

            if (!RazorVuePropertyInitializerHelper.TryGetNormalizedOperation(
                    semanticModel,
                    declarator.Initializer.Value,
                    out var initializerOperation))
            {
                return false;
            }

            initializer = initializerOperation;
            return true;
        }

        if (declarator.Parent?.Parent?.Parent is not BlockSyntax rootBlock ||
            semanticModel.GetOperation(rootBlock) is not IBlockOperation rootOperation)
        {
            return false;
        }

        var collected = CollectSourceStableLocalInitializerStates(rootOperation.Operations, isSupportedCarrierType);
        if (!collected.TryGetValue(local, out var resolved))
            return false;

        if (HasMutableLocalWrites(local, declarator, semanticModel, resolved.AllowedAssignmentSyntax))
            return false;

        initializer = resolved.Initializer;
        return true;
    }

    private static bool TryIsSourceStableLocalInitializerInvalidatedByLaterWrites(
        ILocalSymbol local,
        VariableDeclaratorSyntax declarator,
        SemanticModel semanticModel,
        Func<ITypeSymbol?, bool> isSupportedCarrierType,
        out bool invalidated)
    {
        invalidated = false;
        if (!isSupportedCarrierType(local.Type))
            return false;

        if (declarator.Initializer?.Value is not null)
        {
            invalidated = HasMutableLocalWrites(local, declarator, semanticModel);
            return true;
        }

        if (declarator.Parent?.Parent?.Parent is not BlockSyntax rootBlock ||
            semanticModel.GetOperation(rootBlock) is not IBlockOperation rootOperation)
        {
            return false;
        }

        var collected = CollectSourceStableLocalInitializerStates(rootOperation.Operations, isSupportedCarrierType);
        if (!collected.TryGetValue(local, out var resolved))
            return false;

        invalidated = HasMutableLocalWrites(local, declarator, semanticModel, resolved.AllowedAssignmentSyntax);
        return true;
    }

    private static Dictionary<ILocalSymbol, SourceStableLocalInitializer> CollectSourceStableLocalInitializerStates(
        IReadOnlyList<IOperation> operations,
        Func<ITypeSymbol?, bool> isSupportedCarrierType)
    {
        var result = new Dictionary<ILocalSymbol, SourceStableLocalInitializer>(SymbolEqualityComparer.Default);
        var pendingAssignments = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);

        foreach (var operation in operations)
        {
            var current = RazorVueOperationNormalizer.Unwrap(operation);
            if (pendingAssignments.Count > 0)
            {
                if (TryExtractImmediateAssignedLocal(
                        current,
                        pendingAssignments,
                        out var local,
                        out var initializer,
                        out var assignmentSyntax))
                {
                    result[local] = new SourceStableLocalInitializer(
                        ResolveInitializerAlias(initializer, result),
                        assignmentSyntax);
                    continue;
                }

                pendingAssignments.Clear();
            }

            switch (current)
            {
                case IVariableDeclarationGroupOperation declarationGroup:
                    RegisterSourceStableDeclarators(
                        declarationGroup.Declarations,
                        result,
                        pendingAssignments,
                        isSupportedCarrierType);
                    break;
                case IVariableDeclarationOperation declarationOperation:
                    RegisterSourceStableDeclarators(
                        [declarationOperation],
                        result,
                        pendingAssignments,
                        isSupportedCarrierType);
                    break;
            }
        }

        return result;
    }

    private static void RegisterSourceStableDeclarators(
        IEnumerable<IVariableDeclarationOperation> declarations,
        Dictionary<ILocalSymbol, SourceStableLocalInitializer> result,
        HashSet<ILocalSymbol> pendingAssignments,
        Func<ITypeSymbol?, bool> isSupportedCarrierType)
    {
        foreach (var declaration in declarations)
        {
            foreach (var declarator in declaration.Declarators)
            {
                if (!isSupportedCarrierType(declarator.Symbol.Type))
                    continue;

                if (declarator.Initializer?.Value is not { } initializer)
                {
                    pendingAssignments.Add(declarator.Symbol);
                    continue;
                }

                result[declarator.Symbol] = new SourceStableLocalInitializer(
                    RazorVueOperationNormalizer.Unwrap(initializer) ?? initializer,
                    null);
            }
        }
    }

    private static bool TryExtractImmediateAssignedLocal(
        IOperation? operation,
        HashSet<ILocalSymbol> pendingAssignments,
        out ILocalSymbol localSymbol,
        out IOperation initializer,
        out SyntaxNode? assignmentSyntax)
    {
        localSymbol = default!;
        initializer = default!;
        assignmentSyntax = null;
        switch (operation)
        {
            case ISimpleAssignmentOperation assignment
                when assignment.Target is ILocalReferenceOperation localReference &&
                     pendingAssignments.Remove(localReference.Local):
                localSymbol = localReference.Local;
                initializer = RazorVueOperationNormalizer.Unwrap(assignment.Value) ?? assignment.Value;
                assignmentSyntax = assignment.Syntax;
                return true;
            case IExpressionStatementOperation expressionStatement:
                return TryExtractImmediateAssignedLocal(
                    expressionStatement.Operation,
                    pendingAssignments,
                    out localSymbol,
                    out initializer,
                    out assignmentSyntax);
            case IBlockOperation block when block.Operations.Length == 1:
                return TryExtractImmediateAssignedLocal(
                    block.Operations[0],
                    pendingAssignments,
                    out localSymbol,
                    out initializer,
                    out assignmentSyntax);
            default:
                return false;
        }
    }

    private static IOperation ResolveInitializerAlias(
        IOperation initializer,
        IReadOnlyDictionary<ILocalSymbol, SourceStableLocalInitializer> result)
    {
        var current = RazorVueOperationNormalizer.Unwrap(initializer) ?? initializer;
        var visitedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        while (current is ILocalReferenceOperation localReference &&
               visitedLocals.Add(localReference.Local) &&
               result.TryGetValue(localReference.Local, out var resolved))
        {
            current = resolved.Initializer;
        }

        return current;
    }

    private static bool HasMutableLocalWrites(
        ILocalSymbol local,
        VariableDeclaratorSyntax declarator,
        SemanticModel semanticModel,
        SyntaxNode? allowedImmediateAssignmentSyntax = null)
    {
        var rootBlock = declarator.Ancestors().OfType<BlockSyntax>().FirstOrDefault();
        if (rootBlock is null)
            return true;

        if (semanticModel.GetOperation(rootBlock) is not IOperation rootOperation)
            return true;

        foreach (var operation in EnumerateOperations(rootOperation))
        {
            switch (operation)
            {
                case IAssignmentOperation assignment
                    when IsAllowedInitializerAssignment(assignment, declarator, allowedImmediateAssignmentSyntax):
                    continue;
                case IAssignmentOperation assignment
                    when ReferencesLocalSymbol(assignment.Target, local):
                    return true;
                case IIncrementOrDecrementOperation incrementOrDecrement
                    when ReferencesLocalSymbol(incrementOrDecrement.Target, local):
                    return true;
                case IArgumentOperation argument
                    when argument.Parameter?.RefKind is RefKind.Ref or RefKind.Out &&
                         ReferencesLocalSymbol(argument.Value, local):
                    return true;
            }
        }

        return false;
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

    private static bool IsAllowedInitializerAssignment(
        IAssignmentOperation assignment,
        VariableDeclaratorSyntax declarator,
        SyntaxNode? allowedImmediateAssignmentSyntax)
        => IsDeclaratorInitializerAssignment(assignment, declarator) ||
           (allowedImmediateAssignmentSyntax is not null &&
            assignment.Syntax is not null &&
            ReferenceEquals(assignment.Syntax.SyntaxTree, allowedImmediateAssignmentSyntax.SyntaxTree) &&
            assignment.Syntax.Span.Equals(allowedImmediateAssignmentSyntax.Span));

    private static bool IsDeclaratorInitializerAssignment(
        IAssignmentOperation assignment,
        VariableDeclaratorSyntax declarator)
        => assignment.Syntax is not null &&
           assignment.Syntax.Span == declarator.Span;

    private static bool ReferencesLocalSymbol(IOperation? operation, ILocalSymbol local)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return false;

        if (current is ILocalReferenceOperation localReference &&
            SymbolEqualityComparer.Default.Equals(localReference.Local, local))
        {
            return true;
        }

        foreach (var child in current.ChildOperations)
        {
            if (child is not null && ReferencesLocalSymbol(child, local))
                return true;
        }

        return false;
    }
}
