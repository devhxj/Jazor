using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RenderTree;

internal static class RazorVueImperativeRenderPromotionAnalyzer
{
    public static bool ShouldPromoteBody(IEnumerable<IOperation> operations)
    {
        foreach (var operation in operations)
        {
            if (RequiresImperativePromotion(operation))
                return true;
        }

        return false;
    }

    public static RazorVueImperativeBlockKind ClassifyBodyKind(IEnumerable<IOperation> operations)
    {
        foreach (var operation in operations)
        {
            var kind = TryClassify(operation);
            if (kind is not null)
                return kind.Value;
        }

        return RazorVueImperativeBlockKind.MethodBody;
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
            IBlockOperation block => ShouldPromoteBody(block.Operations),
            IConditionalOperation conditional => RequiresImperativePromotion(conditional.WhenTrue) ||
                                                 RequiresImperativePromotion(conditional.WhenFalse),
            IForEachLoopOperation loop => RequiresImperativePromotion(loop.Body),
            IForLoopOperation loop => RequiresImperativePromotion(loop.Body),
            ILockOperation lockOperation => RequiresImperativePromotion(lockOperation.Body),
            IUsingOperation usingOperation => RequiresImperativePromotion(usingOperation.Body),
            IUsingDeclarationOperation => true,
            IExpressionStatementOperation expressionStatement => RequiresImperativePromotionExpressionStatement(expressionStatement),
            _ => false
        };
    }

    private static bool RequiresImperativePromotionExpressionStatement(IExpressionStatementOperation expressionStatement)
    {
        var current = RazorVueOperationNormalizer.Unwrap(expressionStatement.Operation);
        return current is IAssignmentOperation or IIncrementOrDecrementOperation;
    }

    private static RazorVueImperativeBlockKind? TryClassify(IOperation operation)
        => operation switch
        {
            IReturnOperation { IsImplicit: false } => RazorVueImperativeBlockKind.MethodBody,
            IWhileLoopOperation => RazorVueImperativeBlockKind.LoopBlock,
            ISwitchOperation => RazorVueImperativeBlockKind.SwitchBlock,
            ILockOperation => RazorVueImperativeBlockKind.LockBlock,
            ITryOperation => RazorVueImperativeBlockKind.TryBlock,
            IUsingOperation => RazorVueImperativeBlockKind.TryBlock,
            IUsingDeclarationOperation => RazorVueImperativeBlockKind.TryBlock,
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
            _ => null
        };
    }
}
