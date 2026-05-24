using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RenderTree;

internal static class RazorVueImperativeRenderPromotionAnalyzer
{
    public static bool ShouldPromoteBody(IEnumerable<IOperation> operations)
    {
        return RequiresImperativePromotion(operations);
    }

    public static RazorVueImperativeBlockKind ClassifyBodyKind(IEnumerable<IOperation> operations)
    {
        return ClassifyBodyKindOrNull(operations) ?? RazorVueImperativeBlockKind.MethodBody;
    }

    private static bool RequiresImperativePromotion(IOperation? operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return false;

        if (TryClassify(current) is not null)
            return true;

        return current switch
        {
            IBlockOperation block => RequiresImperativePromotion(block.Operations),
            IConditionalOperation conditional => RequiresImperativePromotion(conditional.WhenTrue) ||
                                                 RequiresImperativePromotion(conditional.WhenFalse),
            IForEachLoopOperation { IsAsynchronous: true } => true,
            IForEachLoopOperation loop => RequiresImperativePromotion(loop.Body),
            IForLoopOperation loop => RequiresImperativePromotion(loop.Body),
            ILockOperation lockOperation => RequiresImperativePromotion(lockOperation.Body),
            IUsingOperation usingOperation => RequiresImperativePromotion(usingOperation.Body),
            IUsingDeclarationOperation => true,
            IThrowOperation => true,
            IAwaitOperation => true,
            IExpressionStatementOperation expressionStatement => RequiresImperativePromotionExpressionStatement(expressionStatement),
            IBranchOperation { BranchKind: BranchKind.Break or BranchKind.Continue } => true,
            _ => false
        };
    }

    private static bool RequiresImperativePromotion(IEnumerable<IOperation> operations)
    {
        var buffered = operations as IReadOnlyList<IOperation> ?? operations.ToArray();
        for (var index = 0; index < buffered.Count; index++)
        {
            var current = RazorVueOperationNormalizer.Unwrap(buffered[index]);
            if (current is null)
                continue;

            if (IsImmediateSupportedLocalDeclarationAssignment(buffered, index))
                continue;

            if (RequiresImperativePromotion(current))
                return true;
        }

        return false;
    }

    private static bool RequiresImperativePromotionExpressionStatement(IExpressionStatementOperation expressionStatement)
    {
        var current = RazorVueOperationNormalizer.Unwrap(expressionStatement.Operation);
        return current is IAssignmentOperation or IIncrementOrDecrementOperation or IAwaitOperation;
    }

    private static RazorVueImperativeBlockKind? TryClassify(IOperation operation)
        => operation switch
        {
            IReturnOperation { IsImplicit: false } => RazorVueImperativeBlockKind.MethodBody,
            IThrowOperation => RazorVueImperativeBlockKind.MethodBody,
            IAwaitOperation => RazorVueImperativeBlockKind.MethodBody,
            IWhileLoopOperation => RazorVueImperativeBlockKind.LoopBlock,
            IForEachLoopOperation { IsAsynchronous: true } => RazorVueImperativeBlockKind.LoopBlock,
            ISwitchOperation => RazorVueImperativeBlockKind.SwitchBlock,
            ILockOperation => RazorVueImperativeBlockKind.LockBlock,
            ITryOperation => RazorVueImperativeBlockKind.TryBlock,
            IUsingOperation => RazorVueImperativeBlockKind.TryBlock,
            IUsingDeclarationOperation => RazorVueImperativeBlockKind.TryBlock,
            IBranchOperation { BranchKind: BranchKind.Break or BranchKind.Continue } => RazorVueImperativeBlockKind.LoopBlock,
            IExpressionStatementOperation expressionStatement => TryClassifyExpressionStatement(expressionStatement),
            IAssignmentOperation => RazorVueImperativeBlockKind.LocalBlock,
            IIncrementOrDecrementOperation => RazorVueImperativeBlockKind.LocalBlock,
            _ => null
        };

    private static RazorVueImperativeBlockKind? TryClassifyExpressionStatement(IExpressionStatementOperation expressionStatement)
    {
        var current = RazorVueOperationNormalizer.Unwrap(expressionStatement.Operation);
        return current switch
        {
            IAssignmentOperation => RazorVueImperativeBlockKind.LocalBlock,
            IIncrementOrDecrementOperation => RazorVueImperativeBlockKind.LocalBlock,
            IAwaitOperation => RazorVueImperativeBlockKind.MethodBody,
            _ => null
        };
    }

    private static RazorVueImperativeBlockKind? ClassifyOperationKind(IOperation? operation)
    {
        var current = RazorVueOperationNormalizer.Unwrap(operation);
        if (current is null)
            return null;

        var directKind = TryClassify(current);
        if (directKind is not null)
            return directKind;

        return current switch
        {
            IBlockOperation block => ClassifyBodyKindOrNull(block.Operations),
            IConditionalOperation conditional => ClassifyOperationKind(conditional.WhenTrue) ??
                                                 ClassifyOperationKind(conditional.WhenFalse),
            IForEachLoopOperation { IsAsynchronous: true } => RazorVueImperativeBlockKind.LoopBlock,
            IForEachLoopOperation loop => ClassifyOperationKind(loop.Body),
            IForLoopOperation loop => ClassifyOperationKind(loop.Body),
            ILockOperation lockOperation => ClassifyOperationKind(lockOperation.Body),
            IUsingOperation usingOperation => ClassifyOperationKind(usingOperation.Body),
            _ => null
        };
    }

    private static RazorVueImperativeBlockKind? ClassifyBodyKindOrNull(IEnumerable<IOperation> operations)
    {
        var buffered = operations as IReadOnlyList<IOperation> ?? operations.ToArray();
        for (var index = 0; index < buffered.Count; index++)
        {
            if (IsImmediateSupportedLocalDeclarationAssignment(buffered, index))
                continue;

            var kind = ClassifyOperationKind(buffered[index]);
            if (kind is not null)
                return kind.Value;
        }

        return null;
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

    private static bool IsSupportedLocalDeclarationContinuation(IOperation operation)
        => RazorVueOperationNormalizer.Unwrap(operation) switch
        {
            IVariableDeclarationGroupOperation declarationGroup => declarationGroup.Declarations.All(static declaration =>
                declaration.Declarators.All(static declarator => !IsRenderTreeBuilderType(declarator.Symbol.Type))),
            IVariableDeclarationOperation declarationOperation => declarationOperation.Declarators.All(static declarator =>
                !IsRenderTreeBuilderType(declarator.Symbol.Type)),
            _ => false
        };

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

    private static bool IsRenderTreeBuilderType(ITypeSymbol? typeSymbol)
        => string.Equals(
            typeSymbol?.ToDisplayString(),
            "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder",
            StringComparison.Ordinal);

}
