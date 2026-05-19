using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RenderTree;

internal static class RazorVueImperativeRenderSegmentationPlanner
{
    internal sealed record PlannedSegment(
        int StartIndex,
        int EndExclusive,
        RazorVueImperativeBlockKind Kind);

    public static bool TryPlanLocalSegments(
        IReadOnlyList<IOperation> operations,
        out ImmutableArray<PlannedSegment> segments)
    {
        segments = ImmutableArray<PlannedSegment>.Empty;
        if (operations.Count == 0)
            return false;

        var builder = ImmutableArray.CreateBuilder<PlannedSegment>();
        var pendingSupportStart = -1;

        for (var index = 0; index < operations.Count; index++)
        {
            var current = RazorVueOperationNormalizer.Unwrap(operations[index]);
            if (current is null)
                continue;

            if (IsBufferableSupportOperation(current))
            {
                if (pendingSupportStart < 0)
                    pendingSupportStart = index;

                continue;
            }

            if (IsImmediateTemplateScopedDeclarationAssignment(operations, index))
            {
                pendingSupportStart = -1;
                continue;
            }

            if (!TryClassifyCurrentLevelImperativeOperation(current, out var kind))
            {
                pendingSupportStart = -1;
                continue;
            }

            var startIndex = pendingSupportStart >= 0 ? pendingSupportStart : index;
            var endExclusive = index + 1;
            if (ShouldExtendSegmentToEnd(operations, startIndex, endExclusive))
                endExclusive = operations.Count;

            builder.Add(new PlannedSegment(startIndex, endExclusive, kind));
            pendingSupportStart = -1;
            index = endExclusive - 1;
        }

        if (builder.Count == 0)
            return false;

        var planned = builder.ToImmutable();
        if (!HasDeclarativeGaps(operations.Count, planned))
            return false;

        segments = planned;
        return true;
    }

    private static bool HasDeclarativeGaps(
        int operationCount,
        ImmutableArray<PlannedSegment> segments)
    {
        if (segments.IsDefaultOrEmpty)
            return false;

        if (segments[0].StartIndex > 0)
            return true;

        for (var index = 1; index < segments.Length; index++)
        {
            if (segments[index - 1].EndExclusive < segments[index].StartIndex)
                return true;
        }

        return segments[segments.Length - 1].EndExclusive < operationCount;
    }

    private static bool IsImmediateTemplateScopedDeclarationAssignment(
        IReadOnlyList<IOperation> operations,
        int currentIndex)
    {
        if (currentIndex <= 0)
            return false;

        var current = RazorVueOperationNormalizer.Unwrap(operations[currentIndex]);
        if (current is not IExpressionStatementOperation expressionStatement ||
            RazorVueOperationNormalizer.Unwrap(expressionStatement.Operation) is not ISimpleAssignmentOperation assignment ||
            assignment.Target is not ILocalReferenceOperation localReference)
        {
            return false;
        }

        for (var previousIndex = currentIndex - 1; previousIndex >= 0; previousIndex--)
        {
            var previous = RazorVueOperationNormalizer.Unwrap(operations[previousIndex]);
            if (previous is null or IEmptyOperation)
                continue;

            return ContainsPendingTemplateScopedDeclarator(previous, localReference.Local);
        }

        return false;
    }

    private static bool ContainsPendingTemplateScopedDeclarator(IOperation operation, ILocalSymbol localSymbol)
    {
        return RazorVueOperationNormalizer.Unwrap(operation) switch
        {
            IVariableDeclarationGroupOperation declarationGroup => declarationGroup.Declarations.Any(declaration =>
                declaration.Declarators.Any(declarator =>
                    SymbolEqualityComparer.Default.Equals(declarator.Symbol, localSymbol) &&
                    declarator.Initializer?.Value is null &&
                    !IsRenderTreeBuilderType(declarator.Symbol.Type) &&
                    !IsRenderFragmentType(declarator.Symbol.Type))),
            IVariableDeclarationOperation declarationOperation => declarationOperation.Declarators.Any(declarator =>
                SymbolEqualityComparer.Default.Equals(declarator.Symbol, localSymbol) &&
                declarator.Initializer?.Value is null &&
                !IsRenderTreeBuilderType(declarator.Symbol.Type) &&
                !IsRenderFragmentType(declarator.Symbol.Type)),
            _ => false
        };
    }

    private static bool IsBufferableSupportOperation(IOperation operation)
    {
        operation = RazorVueOperationNormalizer.Unwrap(operation)!;
        return operation switch
        {
            IVariableDeclarationGroupOperation declarationGroup => IsTemplateScopedDeclarationSupportOnly(declarationGroup),
            IExpressionStatementOperation expressionStatement => IsBuilderAliasAssignment(expressionStatement),
            _ => false
        };
    }

    private static bool IsTemplateScopedDeclarationSupportOnly(IVariableDeclarationGroupOperation declarationGroup)
    {
        foreach (var declaration in declarationGroup.Declarations)
        {
            foreach (var declarator in declaration.Declarators)
            {
                if (IsRenderTreeBuilderType(declarator.Symbol.Type))
                    return false;

                if (declarator.Initializer?.Value is not null)
                    continue;

                if (IsRenderFragmentType(declarator.Symbol.Type))
                    return false;
            }
        }

        return true;
    }

    private static bool IsBuilderAliasAssignment(IExpressionStatementOperation expressionStatement)
    {
        var current = RazorVueOperationNormalizer.Unwrap(expressionStatement.Operation);
        return current is ISimpleAssignmentOperation assignment &&
               assignment.Target is ILocalReferenceOperation localReference &&
               IsRenderTreeBuilderType(localReference.Local.Type) &&
               IsKnownBuilderSource(assignment.Value);
    }

    private static bool IsKnownBuilderSource(IOperation? operation)
        => RazorVueOperationNormalizer.Unwrap(operation) switch
        {
            IParameterReferenceOperation parameterReference => IsRenderTreeBuilderType(parameterReference.Parameter.Type),
            ILocalReferenceOperation localReference => IsRenderTreeBuilderType(localReference.Local.Type),
            _ => false
        };

    private static bool TryClassifyCurrentLevelImperativeOperation(
        IOperation operation,
        out RazorVueImperativeBlockKind kind)
    {
        operation = RazorVueOperationNormalizer.Unwrap(operation)!;
        switch (operation)
        {
            case IReturnOperation { IsImplicit: false }:
            case IThrowOperation:
                kind = RazorVueImperativeBlockKind.MethodBody;
                return true;
            case IWhileLoopOperation:
                kind = RazorVueImperativeBlockKind.LoopBlock;
                return true;
            case ISwitchOperation:
                kind = RazorVueImperativeBlockKind.SwitchBlock;
                return true;
            case ILockOperation:
                kind = RazorVueImperativeBlockKind.LockBlock;
                return true;
            case ITryOperation:
            case IUsingOperation:
            case IUsingDeclarationOperation:
                kind = RazorVueImperativeBlockKind.TryBlock;
                return true;
            case IBranchOperation { BranchKind: BranchKind.Break or BranchKind.Continue }:
                kind = RazorVueImperativeBlockKind.LoopBlock;
                return true;
            case IAssignmentOperation:
            case IIncrementOrDecrementOperation:
                kind = RazorVueImperativeBlockKind.LocalBlock;
                return true;
            case IExpressionStatementOperation expressionStatement:
            {
                var statement = RazorVueOperationNormalizer.Unwrap(expressionStatement.Operation);
                if (statement is ISimpleAssignmentOperation && IsBuilderAliasAssignment(expressionStatement))
                    break;

                if (statement is IAssignmentOperation or IIncrementOrDecrementOperation)
                {
                    kind = RazorVueImperativeBlockKind.LocalBlock;
                    return true;
                }

                break;
            }
        }

        kind = default;
        return false;
    }

    private static bool ShouldExtendSegmentToEnd(
        IReadOnlyList<IOperation> operations,
        int startIndex,
        int endExclusive)
    {
        if (endExclusive >= operations.Count)
            return false;

        if (ContainsUsingDeclarationSemantics(operations, startIndex, endExclusive))
            return true;

        var declaredLocals = CollectDeclaredLocals(operations, startIndex, endExclusive);
        if (declaredLocals.Count == 0)
            return false;

        for (var index = endExclusive; index < operations.Count; index++)
        {
            if (ReadsAnyLocal(operations[index], declaredLocals))
                return true;
        }

        return false;
    }

    private static bool ContainsUsingDeclarationSemantics(
        IReadOnlyList<IOperation> operations,
        int startIndex,
        int endExclusive)
    {
        for (var index = startIndex; index < endExclusive; index++)
        {
            foreach (var current in EnumerateSelfAndDescendants(operations[index]))
            {
                if (current is IUsingDeclarationOperation)
                    return true;
            }
        }

        return false;
    }

    private static HashSet<ILocalSymbol> CollectDeclaredLocals(
        IReadOnlyList<IOperation> operations,
        int startIndex,
        int endExclusive)
    {
        var declaredLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        for (var index = startIndex; index < endExclusive; index++)
        {
            foreach (var current in EnumerateSelfAndDescendants(operations[index]))
            {
                switch (current)
                {
                    case IVariableDeclarationGroupOperation declarationGroup:
                        foreach (var declaration in declarationGroup.Declarations)
                        {
                            foreach (var declarator in declaration.Declarators)
                                declaredLocals.Add(declarator.Symbol);
                        }

                        break;
                    case IForEachLoopOperation foreachLoop:
                        foreach (var local in foreachLoop.Locals)
                            declaredLocals.Add(local);
                        break;
                    case IForLoopOperation forLoop:
                        foreach (var local in forLoop.Locals)
                            declaredLocals.Add(local);
                        break;
                    case IUsingDeclarationOperation usingDeclaration when usingDeclaration.DeclarationGroup is not null:
                        foreach (var declaration in usingDeclaration.DeclarationGroup.Declarations)
                        {
                            foreach (var declarator in declaration.Declarators)
                                declaredLocals.Add(declarator.Symbol);
                        }

                        break;
                }
            }
        }

        return declaredLocals;
    }

    private static bool ReadsAnyLocal(IOperation operation, HashSet<ILocalSymbol> locals)
    {
        foreach (var current in EnumerateSelfAndDescendants(operation))
        {
            if (current is ILocalReferenceOperation localReference &&
                locals.Contains(localReference.Local))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<IOperation> EnumerateSelfAndDescendants(IOperation operation)
    {
        operation = RazorVueOperationNormalizer.Unwrap(operation)!;
        yield return operation;
        foreach (var child in operation.ChildOperations)
        {
            if (child is null)
                continue;

            foreach (var nested in EnumerateSelfAndDescendants(child))
                yield return nested;
        }
    }

    private static bool IsRenderTreeBuilderType(ITypeSymbol? typeSymbol)
        => string.Equals(
            typeSymbol?.ToDisplayString(),
            "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder",
            StringComparison.Ordinal);

    private static bool IsRenderFragmentType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return false;

        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.IsGenericType &&
            namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
        {
            typeSymbol = namedType.TypeArguments[0];
        }

        var displayName = typeSymbol.ToDisplayString();
        return string.Equals(displayName, "Microsoft.AspNetCore.Components.RenderFragment", StringComparison.Ordinal) ||
               displayName.StartsWith("Microsoft.AspNetCore.Components.RenderFragment<", StringComparison.Ordinal);
    }
}
