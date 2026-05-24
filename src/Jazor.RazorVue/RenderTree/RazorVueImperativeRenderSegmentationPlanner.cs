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

            if (IsImmediateSupportedLocalDeclarationAssignment(operations, index))
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
            ExpandSegmentBounds(operations, ref startIndex, ref endExclusive);

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

    private static void ExpandSegmentBounds(
        IReadOnlyList<IOperation> operations,
        ref int startIndex,
        ref int endExclusive)
    {
        var changed = true;
        while (changed)
        {
            changed = false;

            if (TryExpandSegmentForReferencedLocalFunctions(
                    operations,
                    startIndex,
                    endExclusive,
                    out var localFunctionStartIndex,
                    out var localFunctionEndExclusive))
            {
                if (localFunctionStartIndex < startIndex)
                {
                    startIndex = localFunctionStartIndex;
                    changed = true;
                }

                if (localFunctionEndExclusive > endExclusive)
                {
                    endExclusive = localFunctionEndExclusive;
                    changed = true;
                }
            }

            if (!ShouldExtendSegmentToEnd(operations, startIndex, endExclusive))
                continue;

            if (endExclusive != operations.Count)
            {
                endExclusive = operations.Count;
                changed = true;
            }

            var extendedStart = ExtendSegmentStartForImmediateAssignedLocalReads(
                operations,
                startIndex,
                endExclusive);
            if (extendedStart < startIndex)
            {
                startIndex = extendedStart;
                changed = true;
            }
        }
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

    private static bool IsImmediateSupportedLocalDeclarationAssignment(
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

            if (ContainsPendingSupportedLocalDeclarator(previous, localReference.Local))
                return true;

            if (IsSupportedLocalDeclarationContinuation(previous))
                continue;

            return false;
        }

        return false;
    }

    private static int ExtendSegmentStartForImmediateAssignedLocalReads(
        IReadOnlyList<IOperation> operations,
        int startIndex,
        int endExclusive)
    {
        var extendedStart = startIndex;
        foreach (var local in CollectLocalReferences(operations, startIndex, endExclusive))
        {
            if (TryFindImmediateAssignedLocalPrefixStart(
                    operations,
                    local,
                    startIndex,
                    out var prefixStart))
            {
                extendedStart = Math.Min(extendedStart, prefixStart);
            }
        }

        return extendedStart;
    }

    private static IEnumerable<ILocalSymbol> CollectLocalReferences(
        IReadOnlyList<IOperation> operations,
        int startIndex,
        int endExclusive)
    {
        var seen = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        for (var index = startIndex; index < endExclusive; index++)
        {
            foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(
                         operations[index],
                         includeLocalFunctionBodies: true))
            {
                if (current is ILocalReferenceOperation localReference &&
                    seen.Add(localReference.Local))
                {
                    yield return localReference.Local;
                }
            }
        }
    }

    private static bool TryFindImmediateAssignedLocalPrefixStart(
        IReadOnlyList<IOperation> operations,
        ILocalSymbol local,
        int beforeIndex,
        out int prefixStart)
    {
        prefixStart = -1;
        for (var declarationIndex = beforeIndex - 1; declarationIndex >= 0; declarationIndex--)
        {
            var declarationCandidate = RazorVueOperationNormalizer.Unwrap(operations[declarationIndex]);
            if (declarationCandidate is null or IEmptyOperation)
                continue;

            if (!ContainsPendingSupportedLocalDeclarator(declarationCandidate, local))
            {
                if (IsSimpleAssignmentToLocal(declarationCandidate, local))
                    continue;

                if (IsSupportedLocalDeclarationContinuation(declarationCandidate))
                    continue;

                return false;
            }

            if (HasImmediateAssignmentBeforeSegment(operations, declarationIndex + 1, beforeIndex, local))
            {
                prefixStart = declarationIndex;
                return true;
            }

            return false;
        }

        return false;
    }

    private static bool HasImmediateAssignmentBeforeSegment(
        IReadOnlyList<IOperation> operations,
        int startIndex,
        int endExclusive,
        ILocalSymbol local)
    {
        for (var index = startIndex; index < endExclusive; index++)
        {
            var current = RazorVueOperationNormalizer.Unwrap(operations[index]);
            if (current is null or IEmptyOperation)
                continue;

            if (IsSimpleAssignmentToLocal(current, local))
                return true;

            if (IsSupportedLocalDeclarationContinuation(current))
                continue;

            return false;
        }

        return false;
    }

    private static bool IsSimpleAssignmentToLocal(
        IOperation operation,
        ILocalSymbol local)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is IExpressionStatementOperation expressionStatement)
            current = RazorVueOperationNormalizer.Unwrap(expressionStatement.Operation);

        return current is ISimpleAssignmentOperation assignment &&
               assignment.Target is ILocalReferenceOperation localReference &&
               SymbolEqualityComparer.Default.Equals(localReference.Local, local);
    }

    private static bool TryExpandSegmentForReferencedLocalFunctions(
        IReadOnlyList<IOperation> operations,
        int startIndex,
        int endExclusive,
        out int expandedStartIndex,
        out int expandedEndExclusive)
    {
        expandedStartIndex = startIndex;
        expandedEndExclusive = endExclusive;

        foreach (var localFunction in CollectReferencedLocalFunctions(operations, startIndex, endExclusive))
        {
            if (!TryFindLocalFunctionDeclarationIndex(operations, localFunction, out var declarationIndex))
                continue;

            expandedStartIndex = Math.Min(expandedStartIndex, declarationIndex);
            expandedEndExclusive = Math.Max(expandedEndExclusive, declarationIndex + 1);
        }

        return expandedStartIndex != startIndex ||
               expandedEndExclusive != endExclusive;
    }

    private static IEnumerable<IMethodSymbol> CollectReferencedLocalFunctions(
        IReadOnlyList<IOperation> operations,
        int startIndex,
        int endExclusive)
    {
        var seen = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        for (var index = startIndex; index < endExclusive; index++)
        {
            foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(
                         operations[index],
                         includeLocalFunctionBodies: true))
            {
                if (current is IInvocationOperation { TargetMethod.MethodKind: MethodKind.LocalFunction } invocation)
                {
                    var method = invocation.TargetMethod.OriginalDefinition;
                    if (seen.Add(method))
                        yield return method;
                }
            }
        }
    }

    private static bool TryFindLocalFunctionDeclarationIndex(
        IReadOnlyList<IOperation> operations,
        IMethodSymbol localFunction,
        out int declarationIndex)
    {
        for (var index = 0; index < operations.Count; index++)
        {
            if (RazorVueOperationNormalizer.Unwrap(operations[index]) is ILocalFunctionOperation declaration &&
                SymbolEqualityComparer.Default.Equals(declaration.Symbol.OriginalDefinition, localFunction))
            {
                declarationIndex = index;
                return true;
            }
        }

        declarationIndex = -1;
        return false;
    }

    private static bool ContainsPendingSupportedLocalDeclarator(IOperation operation, ILocalSymbol localSymbol)
    {
        return RazorVueOperationNormalizer.Unwrap(operation) switch
        {
            IVariableDeclarationGroupOperation declarationGroup => declarationGroup.Declarations.Any(declaration =>
                declaration.Declarators.Any(declarator =>
                    SymbolEqualityComparer.Default.Equals(declarator.Symbol, localSymbol) &&
                    declarator.Initializer?.Value is null &&
                    !IsRenderTreeBuilderType(declarator.Symbol.Type))),
            IVariableDeclarationOperation declarationOperation => declarationOperation.Declarators.Any(declarator =>
                SymbolEqualityComparer.Default.Equals(declarator.Symbol, localSymbol) &&
                declarator.Initializer?.Value is null &&
                !IsRenderTreeBuilderType(declarator.Symbol.Type)),
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

            }
        }

        return true;
    }

    private static bool IsSupportedLocalDeclarationContinuation(IOperation operation)
        => RazorVueOperationNormalizer.Unwrap(operation) switch
        {
            IVariableDeclarationGroupOperation declarationGroup => IsTemplateScopedDeclarationSupportOnly(declarationGroup),
            IVariableDeclarationOperation declarationOperation => declarationOperation.Declarators.All(static declarator =>
                !IsRenderTreeBuilderType(declarator.Symbol.Type)),
            _ => false
        };

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
            case IAwaitOperation:
                kind = RazorVueImperativeBlockKind.MethodBody;
                return true;
            case IWhileLoopOperation:
                kind = RazorVueImperativeBlockKind.LoopBlock;
                return true;
            case IForEachLoopOperation { IsAsynchronous: true }:
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
            case ILabeledOperation:
                kind = RazorVueImperativeBlockKind.MethodBody;
                return true;
            case IBranchOperation { BranchKind: BranchKind.GoTo }:
                kind = RazorVueImperativeBlockKind.MethodBody;
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

                if (statement is IAwaitOperation)
                {
                    kind = RazorVueImperativeBlockKind.MethodBody;
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

        var declaredLocals = RazorVueOperationLocalCollector.CollectDeclaredLocals(operations, startIndex, endExclusive);
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
            foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operations[index]))
            {
                if (current is IUsingDeclarationOperation)
                    return true;
            }
        }

        return false;
    }

    private static bool ReadsAnyLocal(IOperation operation, HashSet<ILocalSymbol> locals)
    {
        foreach (var current in RazorVueOperationLocalCollector.EnumerateSelfAndDescendants(operation))
        {
            if (current is ILocalReferenceOperation localReference &&
                locals.Contains(localReference.Local))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRenderTreeBuilderType(ITypeSymbol? typeSymbol)
        => string.Equals(
            typeSymbol?.ToDisplayString(),
            "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder",
            StringComparison.Ordinal);

}
